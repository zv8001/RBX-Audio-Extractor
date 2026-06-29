Imports System.Diagnostics
Imports System.IO
Imports Microsoft.Win32

Namespace Views
    Public Class MaintenanceView
        Private busy As Boolean

        Public Sub New()
            InitializeComponent()
            ApplicationDataPathText.Text = AssetNameStore.DataDirectory
            UpdateDatabaseDisplay()
            AddHandler Loaded, AddressOf MaintenanceView_Loaded
        End Sub

        Private Sub UpdateDatabaseDisplay()
            CachePathText.Text = AppServices.DatabasePath
            DatabasePathText.Text = "SQL Database: " & AppServices.DatabasePath
            ResetDatabaseButton.IsEnabled = Not busy AndAlso AppServices.IsUsingCustomDatabase
        End Sub

        Private Async Sub ChangeDatabaseButton_Click(sender As Object, e As RoutedEventArgs)
            If busy Then Return
            Dim dialog As New OpenFileDialog With {
                .Title = "Select an rbx-storage.db database",
                .Filter = "Roblox cache database (*.db)|*.db|All files (*.*)|*.*",
                .CheckFileExists = True}
            If File.Exists(AppServices.DatabasePath) Then dialog.InitialDirectory = Path.GetDirectoryName(AppServices.DatabasePath)
            If dialog.ShowDialog() <> True Then Return
            AppServices.SetDatabasePathOverride(dialog.FileName)
            UpdateDatabaseDisplay()
            AppServices.Report($"SQL database set to {dialog.FileName}. Re-scan each workspace to load it.", 100)
            Await RefreshStatsAsync()
        End Sub

        Private Async Sub ResetDatabaseButton_Click(sender As Object, e As RoutedEventArgs)
            If busy Then Return
            AppServices.ResetDatabasePathOverride()
            UpdateDatabaseDisplay()
            AppServices.Report("SQL database reset to the default Roblox cache.", 100)
            Await RefreshStatsAsync()
        End Sub

        Private Async Sub ExtractAllButton_Click(sender As Object, e As RoutedEventArgs)
            If busy Then Return
            If Not File.Exists(AppServices.DatabasePath) Then
                AppDialog.Show("The Roblox cache database was not found. Open Roblox once, or choose a database above, and try again.", "No cache database", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            Dim dialog As New OpenFolderDialog With {.Title = "Choose a folder for the full export"}
            If dialog.ShowDialog() <> True Then Return
            Dim folder = dialog.FolderName

            SetBusy(True)
            AppServices.Report("Starting full cache export...", 0, True)
            Try
                Dim progressAction As Action(Of String, Double) =
                    Sub(stage, percent) AppServices.Report(stage, percent)
                Dim summary = Await Task.Run(Function() BulkAssetExporter.ExportEverything(folder, progressAction))
                AppServices.Report($"Full export complete: {summary.Exported:N0} exported, {summary.Reused:N0} reused, {summary.Failed:N0} failed.", 100)

                Dim message As String
                If summary.Exported = 0 AndAlso summary.Reused = 0 Then
                    message = "No exportable assets were found in the cache."
                Else
                    message = String.Join(Environment.NewLine, summary.Lines)
                    message &= Environment.NewLine & Environment.NewLine & $"Total: {summary.Exported:N0} exported, {summary.Reused:N0} reused, {summary.Failed:N0} failed."
                    If summary.Skipped > 0 Then message &= $"{Environment.NewLine}{summary.Skipped:N0} unsupported meshes were skipped."
                End If
                Dim openNow = AppDialog.Show(message & Environment.NewLine & Environment.NewLine & "Open the export folder now?", "Extract all complete", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes)
                If openNow = MessageBoxResult.Yes AndAlso Directory.Exists(folder) Then AppServices.OpenPath(folder)
            Catch ex As Exception
                AppServices.Report($"Full export failed: {ex.Message}")
                AppDialog.Show(ex.Message, "Extract all failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Sub

        Private Sub SetBusy(value As Boolean)
            busy = value
            ExtractAllButton.IsEnabled = Not value
            ChangeDatabaseButton.IsEnabled = Not value
            ResetDatabaseButton.IsEnabled = Not value AndAlso AppServices.IsUsingCustomDatabase
        End Sub

        Private Async Sub MaintenanceView_Loaded(sender As Object, e As RoutedEventArgs)
            Await RefreshStatsAsync()
        End Sub

        Private Async Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs)
            Await RefreshStatsAsync()
        End Sub

        Private Async Function RefreshStatsAsync() As Task
            CacheSizeText.Text = "Calculating cache size..."
            Dim result = Await Task.Run(
                Function()
                    Dim size As Long = 0
                    Dim files As Long = 0
                    If File.Exists(AppServices.DatabasePath) Then
                        size += New FileInfo(AppServices.DatabasePath).Length
                        files += 1
                    End If
                    If Directory.Exists(AppServices.PayloadPath) Then
                        For Each path In Directory.EnumerateFiles(AppServices.PayloadPath, "*", SearchOption.AllDirectories)
                            Try
                                size += New FileInfo(path).Length
                                files += 1
                            Catch
                            End Try
                        Next
                    End If
                    Return (Size:=size, Files:=files)
                End Function)
            CacheSizeText.Text = $"{AppServices.FormatBytes(result.Size)} across {result.Files:N0} files"
            Dim applicationData = Await Task.Run(Function() AssetNameStore.GetDataSize())
            ApplicationDataSizeText.Text = $"{AppServices.FormatBytes(applicationData.Size)} across {applicationData.Files:N0} files"
        End Function

        Private Sub OpenFolderButton_Click(sender As Object, e As RoutedEventArgs)
            If Directory.Exists(AppServices.RobloxLocalPath) Then AppServices.OpenPath(AppServices.RobloxLocalPath)
        End Sub

        Private Async Sub ClearCacheButton_Click(sender As Object, e As RoutedEventArgs)
            Dim first = AppDialog.Show("This will delete Roblox's local asset cache, including rbx-storage.db. Continue?", "Clear Roblox asset cache?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
            If first <> MessageBoxResult.Yes Then Return

            Dim forceClose = AppDialog.Show("Force-close all running Roblox processes first?", "Force-close Roblox?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel)
            If forceClose = MessageBoxResult.Cancel Then Return
            If forceClose = MessageBoxResult.Yes Then
                Dim final = AppDialog.Show("WARNING: This immediately closes Roblox Player, Roblox Studio, and every process whose name begins with 'Roblox'. Unsaved Studio work may be lost.", "Roblox Studio will be closed", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No)
                If final <> MessageBoxResult.Yes Then Return
                Dim result = Await Task.Run(AddressOf ForceCloseRoblox)
                AppServices.AddLog($"Roblox process close complete: {result.Killed} closed, {result.Failed} failed.")
            End If

            AppServices.Report("Clearing Roblox asset cache...", 0, True)
            Try
                Await Task.Run(AddressOf DeleteCache)
                AppServices.Report("Roblox asset cache cleared. Scan again to refresh each workspace.", 100)
                AppServices.NotifyCacheCleared()
                Await RefreshStatsAsync()
            Catch ex As Exception
                AppServices.Report($"Cache clear failed: {ex.Message}")
                AppDialog.Show("The cache could not be completely cleared. Roblox may still be using one of its files." & Environment.NewLine & Environment.NewLine & ex.Message, "Cache clear failed", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Async Sub ClearApplicationDataButton_Click(sender As Object, e As RoutedEventArgs)
            Dim first = AppDialog.Show("This permanently removes every saved asset name and all RBX Asset Extractor crash logs on this computer. Roblox's cache will not be touched. Continue?", "Clear all application data?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
            If first <> MessageBoxResult.Yes Then Return
            Dim final = AppDialog.Show("This cannot be undone. Delete all RBX Asset Extractor application data now?", "Final confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
            If final <> MessageBoxResult.Yes Then Return
            Try
                Await Task.Run(AddressOf AssetNameStore.ClearAllApplicationData)
                ApplicationDataSizeText.Text = "0 bytes across 0 files"
                AppServices.NotifyApplicationDataCleared()
                AppServices.Report("All RBX Asset Extractor application data and saved names were cleared.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Application data could not be cleared", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Shared Function ForceCloseRoblox() As (Killed As Integer, Failed As Integer)
            Dim killed = 0
            Dim failed = 0
            For Each proc As Process In Process.GetProcesses()
                Try
                    If Not proc.ProcessName.StartsWith("Roblox", StringComparison.OrdinalIgnoreCase) Then Continue For
                    proc.Kill(entireProcessTree:=True)
                    proc.WaitForExit(5000)
                    killed += 1
                Catch
                    failed += 1
                Finally
                    proc.Dispose()
                End Try
            Next
            Return (killed, failed)
        End Function

        Private Shared Sub DeleteCache()
            Dim failures As New List(Of Exception)()
            If File.Exists(AppServices.DatabasePath) Then
                Try
                    File.Delete(AppServices.DatabasePath)
                Catch ex As Exception
                    failures.Add(ex)
                End Try
            End If
            Dim directories = {AppServices.PayloadPath, Path.Combine(Path.GetTempPath(), "Roblox", "http")}
            For Each directoryPath In directories
                If Not System.IO.Directory.Exists(directoryPath) Then Continue For
                Try
                    System.IO.Directory.Delete(directoryPath, recursive:=True)
                Catch ex As Exception
                    failures.Add(ex)
                End Try
            Next
            If failures.Count > 0 Then Throw New AggregateException(failures)
        End Sub
    End Class
End Namespace
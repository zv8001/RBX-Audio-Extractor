Imports System.IO
Imports Microsoft.Win32

Namespace Views
    Public Class CacheFilesView
        Private entries As New List(Of RobloxCacheAssetEntry)()
        Private busy As Boolean

        Public Async Sub StartScan()
            If busy Then Return
            SetBusy(True)
            AppServices.Report("Scanning RBXM and KTX cache files...", 0, True)
            Try
                Dim progressAction As Action(Of CacheAssetProgress) =
                    Sub(value)
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim percent = If(value.Total = 0, 0, value.Current * 100.0 / value.Total)
                                                   AppServices.Report($"Scanning cache files · {value.Found:N0} found", percent)
                                               End Sub)
                    End Sub
                entries = Await Task.Run(Function() RobloxCacheAssetExtractor.Scan(progressAction, RobloxCacheFileType.RbxmBinary, RobloxCacheFileType.RbxmXml, RobloxCacheFileType.Ktx1, RobloxCacheFileType.Ktx2))
                ApplyFilter()
                AppServices.SetCount("Cache", entries.Count)
                AppServices.Report($"Found {entries.Count:N0} RBXM and KTX files.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Cache-file scan failed", MessageBoxButton.OK, MessageBoxImage.Error)
                AppServices.Report("Cache-file scan failed.")
            Finally
                SetBusy(False)
            End Try
        End Sub

        Private Sub ScanButton_Click(sender As Object, e As RoutedEventArgs)
            StartScan()
        End Sub

        Private Sub Filter_Changed(sender As Object, e As EventArgs)
            ApplyFilter()
        End Sub

        Private Sub ApplyFilter()
            If AssetList Is Nothing OrElse TypeFilter Is Nothing Then Return
            Dim query = If(SearchBox?.Text, String.Empty).Trim()
            Dim mode = TypeFilter.SelectedIndex
            AssetList.ItemsSource = entries.Where(
                Function(item)
                    If query.Length > 0 AndAlso Not item.Hash.Contains(query, StringComparison.OrdinalIgnoreCase) AndAlso Not item.TypeLabel.Contains(query, StringComparison.OrdinalIgnoreCase) Then Return False
                    If mode = 1 Then Return item.FileType = RobloxCacheFileType.RbxmBinary OrElse item.FileType = RobloxCacheFileType.RbxmXml
                    If mode = 2 Then Return item.FileType = RobloxCacheFileType.Ktx1 OrElse item.FileType = RobloxCacheFileType.Ktx2
                    Return True
                End Function).ToList()
        End Sub

        Private Sub AssetList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ExportButton.IsEnabled = Not busy AndAlso AssetList.SelectedItem IsNot Nothing
        End Sub

        Private Async Sub ExportButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(AssetList.SelectedItem, RobloxCacheAssetEntry)
            If entry Is Nothing Then Return
            Dim dialog As New SaveFileDialog With {.Title = "Export cached asset", .FileName = entry.Hash & entry.Extension, .DefaultExt = entry.Extension.TrimStart("."c), .Filter = $"{entry.TypeLabel} (*{entry.Extension})|*{entry.Extension}|All files|*.*"}
            If dialog.ShowDialog() <> True Then Return
            SetBusy(True)
            Try
                Await Task.Run(Sub() RobloxCacheAssetExtractor.Export(entry, dialog.FileName))
                AppServices.Report($"Exported {Path.GetFileName(dialog.FileName)}.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Sub

        Private Async Sub ExportAllButton_Click(sender As Object, e As RoutedEventArgs)
            If entries.Count = 0 Then Return
            Dim dialog As New OpenFolderDialog With {.Title = "Choose a folder for cached RBXM and KTX files"}
            If dialog.ShowDialog() <> True Then Return
            Dim folder = dialog.FolderName
            SetBusy(True)
            Try
                Dim summary = Await Task.Run(Function() RobloxCacheAssetExtractor.ExportMany(entries, folder, Nothing))
                AppServices.Report($"Exported {summary.Exported:N0}; reused {summary.Reused:N0}; failed {summary.Failed:N0}.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Bulk export failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Sub

        Public Sub ResetData()
            entries.Clear()
            ApplyFilter()
            AppServices.SetCount("Cache", 0)
        End Sub
        Private Sub SetBusy(value As Boolean)
            busy = value
            ScanButton.IsEnabled = Not value
            AssetList.IsHitTestVisible = Not value
            ExportButton.IsEnabled = Not value AndAlso AssetList.SelectedItem IsNot Nothing
            ExportAllButton.IsEnabled = Not value AndAlso entries.Count > 0
        End Sub
    End Class
End Namespace
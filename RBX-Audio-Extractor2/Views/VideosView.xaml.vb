Imports System.IO
Imports Microsoft.Win32

Namespace Views
    Public Class VideosView
        Private entries As New List(Of RobloxVideoEntry)()
        Private busy As Boolean

        Private Sub VideoList_SizeChanged(sender As Object, e As SizeChangedEventArgs)
            AppServices.FitGridViewColumns(VideoList, (NameColumn, 0.34), (ResolutionColumn, 0.12), (DurationColumn, 0.14), (SegmentsColumn, 0.14), (BytesColumn, 0.14), (StatusColumn, 0.12))
        End Sub

        Public Async Function StartScanAsync() As Task
            If busy Then Return
            SetBusy(True)
            AppServices.Report("Scanning cached videos...", 0, True)
            Try
                Dim progress As IProgress(Of VideoScanProgress) = New Progress(Of VideoScanProgress)(
                    Sub(value)
                        Dim percent = If(value.Total = 0, 0, value.Current * 100.0 / value.Total)
                        AppServices.Report($"Scanning videos · {value.Found:N0} found", percent)
                    End Sub)
                entries = Await Task.Run(Function() RobloxVideoExtractor.Scan(progress))
                VideoList.ItemsSource = entries
                AppServices.SetCount("Videos", entries.Count)
                SessionStateStore.SaveVideos(entries)
                Dim ready = entries.Where(Function(item) item.IsComplete).Count()
                AppServices.Report($"Found {entries.Count:N0} cached videos; {ready:N0} are complete and previewable.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Video scan failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, Window.GetWindow(Me))
                AppServices.Report("Video scan failed.")
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub ScanButton_Click(sender As Object, e As RoutedEventArgs)
            Await StartScanAsync()
        End Sub

        Private Sub VideoList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            SetBusy(busy)
        End Sub

        Private Async Sub VideoList_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
            Await PreviewSelectedAsync()
        End Sub

        Private Async Sub RenameButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(VideoList.SelectedItem, RobloxVideoEntry)
            If entry Is Nothing Then Return
            If Await AssetNameStore.PromptAndSaveAsync(Window.GetWindow(Me), entry, Function() RobloxVideoExtractor.ReadFingerprintPayload(entry)) Then VideoList.Items.Refresh()
        End Sub

        Private Async Sub PreviewButton_Click(sender As Object, e As RoutedEventArgs)
            Await PreviewSelectedAsync()
        End Sub

        Private Async Function PreviewSelectedAsync() As Task
            Dim entry = TryCast(VideoList.SelectedItem, RobloxVideoEntry)
            If entry Is Nothing OrElse Not entry.IsComplete Then Return
            SetBusy(True)
            AppServices.Report($"Preparing video {AppServices.SafePrefix(entry.PlaylistHash)}...", 0, True)
            Dim package As VideoPreviewPackage = Nothing
            Try
                package = Await Task.Run(Function() RobloxVideoExtractor.CreatePreviewPackage(entry))
                Dim preview As New VideoPreviewWindow(entry, package) With {.Owner = Window.GetWindow(Me)}
                preview.Show()
                package = Nothing
                AppServices.Report($"Previewing {entry.Resolution} video ({entry.DurationLabel}).", 100)
            Catch ex As Exception
                If package IsNot Nothing Then CleanupPackage(package)
                AppDialog.Show(ex.Message, "Video preview failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, Window.GetWindow(Me))
                AppServices.Report("Video preview failed.")
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub ExportButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(VideoList.SelectedItem, RobloxVideoEntry)
            If entry Is Nothing OrElse Not entry.IsComplete Then Return
            Dim dialog As New OpenFolderDialog With {.Title = "Choose a folder for the exported video package"}
            If dialog.ShowDialog() <> True Then Return
            SetBusy(True)
            AppServices.Report($"Exporting video {AppServices.SafePrefix(entry.PlaylistHash)}...", 0, True)
            Try
                Dim result = Await Task.Run(Function() RobloxVideoExtractor.Export(entry, dialog.FolderName))
                AppServices.Report($"Exported {result.FilesWritten:N0} playlist/segment files to {result.DirectoryPath}.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Video export failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, Window.GetWindow(Me))
            Finally
                SetBusy(False)
            End Try
        End Sub

        Private Async Sub ExportSingleFileButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(VideoList.SelectedItem, RobloxVideoEntry)
            If entry Is Nothing OrElse Not entry.IsComplete Then Return
            Dim dialog As New SaveFileDialog With {
                .Title = "Export video as one WebM file",
                .FileName = entry.ExportBaseName & ".webm",
                .DefaultExt = "webm",
                .Filter = "WebM video (*.webm)|*.webm|All files|*.*"}
            If dialog.ShowDialog() <> True Then Return
            SetBusy(True)
            AppServices.Report($"Merging video {AppServices.SafePrefix(entry.PlaylistHash)}...", 0, True)
            Try
                Dim bytes = Await Task.Run(Function() RobloxVideoExtractor.ExportSingleFile(entry, dialog.FileName))
                AppServices.Report($"Exported {Path.GetFileName(dialog.FileName)} ({bytes:N0} bytes).", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Single-file video export failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, Window.GetWindow(Me))
                AppServices.Report("Single-file video export failed.")
            Finally
                SetBusy(False)
            End Try
        End Sub
        Private Async Sub ExportAllButton_Click(sender As Object, e As RoutedEventArgs)
            Dim exportable = entries.Where(Function(item) item.IsComplete).ToList()
            If exportable.Count = 0 Then Return
            Dim oneFileEach = ExportAllSingleFileCheckBox.IsChecked = True
            Dim dialog As New OpenFolderDialog With {.Title = If(oneFileEach, "Choose a folder for merged WebM videos", "Choose a folder for exported video packages")}
            If dialog.ShowDialog() <> True Then Return
            SetBusy(True)
            Try
                Dim exported As Integer
                Dim failed As Integer
                Dim usedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                Await Task.Run(
                    Sub()
                        For index = 0 To exportable.Count - 1
                            Try
                                Dim entry = exportable(index)
                                Dim baseName = AssetNameStore.GetBatchBaseName(entry, usedNames)
                                If oneFileEach Then
                                    RobloxVideoExtractor.ExportSingleFile(entry, Path.Combine(dialog.FolderName, baseName & ".webm"))
                                Else
                                    RobloxVideoExtractor.Export(entry, dialog.FolderName, baseName)
                                End If
                                exported += 1
                            Catch
                                failed += 1
                            End Try
                            Dim current = index + 1
                            Dispatcher.BeginInvoke(Sub() AppServices.Report($"Exporting video {current:N0} of {exportable.Count:N0}...", current * 100.0 / exportable.Count))
                        Next
                    End Sub)
                Dim formatLabel = If(oneFileEach, "single-file videos", "video packages")
                AppServices.Report($"Exported {exported:N0} {formatLabel}; {failed:N0} failed.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Bulk video export failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, Window.GetWindow(Me))
            Finally
                SetBusy(False)
            End Try
        End Sub

        Public Sub ClearSavedNames()
            AssetNameStore.ClearLoadedNames(entries)
            VideoList.Items.Refresh()
        End Sub

        Public Sub ResetData()
            entries.Clear()
            VideoList.ItemsSource = Nothing
            AppServices.SetCount("Videos", 0)
            SetBusy(False)
        End Sub

        Public Sub RestoreState()
            Dim restored = SessionStateStore.LoadVideos()
            If restored.Count = 0 Then Return
            entries = restored
            VideoList.ItemsSource = entries
            AppServices.SetCount("Videos", entries.Count)
            SetBusy(False)
        End Sub

        Private Sub SetBusy(value As Boolean)
            busy = value
            Dim selected = TryCast(VideoList.SelectedItem, RobloxVideoEntry)
            ScanButton.IsEnabled = Not value
            VideoList.IsHitTestVisible = Not value
            RenameButton.IsEnabled = Not value AndAlso selected IsNot Nothing
            PreviewButton.IsEnabled = Not value AndAlso selected IsNot Nothing AndAlso selected.IsComplete
            ExportButton.IsEnabled = PreviewButton.IsEnabled
            ExportSingleFileButton.IsEnabled = PreviewButton.IsEnabled
            ExportAllButton.IsEnabled = Not value AndAlso entries.Any(Function(item) item.IsComplete)
            ExportAllSingleFileCheckBox.IsEnabled = ExportAllButton.IsEnabled
        End Sub

        Private Shared Sub CleanupPackage(package As VideoPreviewPackage)
            Try
                If package IsNot Nothing AndAlso Directory.Exists(package.DirectoryPath) Then Directory.Delete(package.DirectoryPath, recursive:=True)
            Catch
            End Try
        End Sub
    End Class
End Namespace
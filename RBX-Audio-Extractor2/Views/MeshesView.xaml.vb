Imports System.IO
Imports Microsoft.Win32

Namespace Views
    Public Class MeshesView
        Private entries As New List(Of MeshCacheEntry)()
        Private busy As Boolean
        Private Sub AssetList_SizeChanged(sender As Object, e As SizeChangedEventArgs)
            AppServices.FitGridViewColumns(AssetList, (NameColumn, 0.22), (CacheKeyColumn, 0.28), (VersionColumn, 0.13), (BytesColumn, 0.15), (InlineColumn, 0.1), (ReadyColumn, 0.12))
        End Sub

        Public Async Function StartScanAsync() As Task
            If busy Then Return
            SetBusy(True)
            AppServices.Report("Scanning cached meshes...", 0, True)
            Try
                Dim progress As IProgress(Of MeshScanProgress) = New Progress(Of MeshScanProgress)(
                    Sub(value)
                        Dim percent = If(value.Total = 0, 0, value.Current * 100.0 / value.Total)
                        AppServices.Report($"Scanning meshes · {value.Found:N0} found", percent)
                    End Sub)
                entries = Await Task.Run(Function()
                                             Dim scanned = RobloxMeshExtractor.Scan(progress)
                                             AssetNameStore.ApplySavedNames(scanned)
                                             Return scanned
                                         End Function)
                AssetList.ItemsSource = entries
                AppServices.SetCount("Meshes", entries.Count)
                SessionStateStore.SaveMeshes(entries)
                Dim ready = entries.Where(Function(item) item.CanExport).Count()
                AppServices.Report($"Found {entries.Count:N0} meshes; {ready:N0} can be exported.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Mesh scan failed", MessageBoxButton.OK, MessageBoxImage.Error)
                AppServices.Report("Mesh scan failed.")
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub ScanButton_Click(sender As Object, e As RoutedEventArgs)
            Await StartScanAsync()
        End Sub

        Private Sub AssetList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim entry = TryCast(AssetList.SelectedItem, MeshCacheEntry)
            RenameButton.IsEnabled = Not busy AndAlso entry IsNot Nothing
            PreviewButton.IsEnabled = Not busy AndAlso entry IsNot Nothing AndAlso entry.CanExport
            ExportButton.IsEnabled = PreviewButton.IsEnabled
        End Sub

        Private Async Sub AssetList_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
            Await PreviewSelectedAsync()
        End Sub

        Private Async Sub RenameButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(AssetList.SelectedItem, MeshCacheEntry)
            If entry Is Nothing Then Return
            If Await AssetNameStore.PromptAndSaveAsync(Window.GetWindow(Me), entry, Function() RobloxMeshExtractor.ReadPayload(entry)) Then AssetList.Items.Refresh()
        End Sub

        Private Async Sub PreviewButton_Click(sender As Object, e As RoutedEventArgs)
            Await PreviewSelectedAsync()
        End Sub

        Private Async Function PreviewSelectedAsync() As Task
            Dim entry = TryCast(AssetList.SelectedItem, MeshCacheEntry)
            If entry Is Nothing OrElse Not entry.CanExport Then Return
            SetBusy(True)
            AppServices.Report($"Decoding mesh {AppServices.SafePrefix(entry.Hash)}...", 0, True)
            Try
                Dim data = Await Task.Run(Function() RobloxMeshExtractor.LoadPreview(entry))
                Dim preview As New MeshPreviewWindow(entry, data) With {.Owner = Window.GetWindow(Me)}
                preview.Show()
                AppServices.Report($"Previewing {data.Positions.Length:N0} vertices and {data.Indices.Length \ 3:N0} triangles.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Mesh preview failed", MessageBoxButton.OK, MessageBoxImage.Error)
                AppServices.Report("Mesh preview failed.")
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub ExportButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(AssetList.SelectedItem, MeshCacheEntry)
            If entry Is Nothing OrElse Not entry.CanExport Then Return
            Dim dialog As New SaveFileDialog With {.Title = "Export Roblox mesh as OBJ", .Filter = "Wavefront OBJ (*.obj)|*.obj", .FileName = entry.ExportBaseName & ".obj", .DefaultExt = "obj"}
            If dialog.ShowDialog() <> True Then Return
            SetBusy(True)
            AppServices.Report($"Exporting mesh {AppServices.SafePrefix(entry.Hash)}...", 0, True)
            Try
                Dim result = Await Task.Run(Function() RobloxMeshExtractor.Export(entry, dialog.FileName))
                AppServices.Report($"Exported {result.VertexCount:N0} vertices and {result.FaceCount:N0} faces.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Mesh export failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Sub

        Private Async Sub ExportAllButton_Click(sender As Object, e As RoutedEventArgs)
            Dim exportable = entries.Where(Function(item) item.CanExport).ToList()
            If exportable.Count = 0 Then Return
            Dim dialog As New OpenFolderDialog With {.Title = "Choose a folder for exported OBJ files"}
            If dialog.ShowDialog() <> True Then Return
            Dim folder = dialog.FolderName
            SetBusy(True)
            Try
                Dim exported = 0
                Dim failed = 0
                Dim usedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                Await Task.Run(Sub()
                                   For index = 0 To exportable.Count - 1
                                       Try
                                           RobloxMeshExtractor.Export(exportable(index), Path.Combine(folder, AssetNameStore.GetBatchBaseName(exportable(index), usedNames) & ".obj"))
                                           exported += 1
                                       Catch
                                           failed += 1
                                       End Try
                                       Dim current = index + 1
                                       Dispatcher.BeginInvoke(Sub() AppServices.Report($"Exporting mesh {current:N0} of {exportable.Count:N0}...", current * 100.0 / exportable.Count))
                                   Next
                               End Sub)
                AppServices.Report($"Exported {exported:N0} meshes; {failed:N0} failed.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Bulk mesh export failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Sub

        Public Sub ClearSavedNames()
            AssetNameStore.ClearLoadedNames(entries)
            AssetList.Items.Refresh()
        End Sub

        Public Sub ResetData()
            entries.Clear()
            AssetList.ItemsSource = Nothing
            AppServices.SetCount("Meshes", 0)
        End Sub

        Public Sub RestoreState()
            Dim restored = SessionStateStore.LoadMeshes()
            If restored.Count = 0 Then Return
            entries = restored
            AssetList.ItemsSource = entries
            AppServices.SetCount("Meshes", entries.Count)
            SetBusy(False)
        End Sub
        Private Sub SetBusy(value As Boolean)
            busy = value
            ScanButton.IsEnabled = Not value
            RenameButton.IsEnabled = Not value AndAlso AssetList.SelectedItem IsNot Nothing
            AssetList.IsHitTestVisible = Not value
            Dim selected = TryCast(AssetList.SelectedItem, MeshCacheEntry)
            PreviewButton.IsEnabled = Not value AndAlso selected IsNot Nothing AndAlso selected.CanExport
            ExportButton.IsEnabled = PreviewButton.IsEnabled
            ExportAllButton.IsEnabled = Not value AndAlso entries.Any(Function(item) item.CanExport)
        End Sub
    End Class
End Namespace
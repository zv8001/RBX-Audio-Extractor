Imports System.IO
Imports ImageMagick
Imports Microsoft.Win32

Namespace Views
    Public Class ImagesView
        Private entries As New List(Of RobloxCacheAssetEntry)()
        Private busy As Boolean
        Private previewVersion As Integer
        Private Sub AssetList_SizeChanged(sender As Object, e As SizeChangedEventArgs)
            AppServices.FitGridViewColumns(AssetList, (NameColumn, 0.26), (CacheKeyColumn, 0.36), (TypeColumn, 0.18), (BytesColumn, 0.2))
        End Sub

        Public Async Function StartScanAsync() As Task
            If busy Then Return
            SetBusy(True)
            AppServices.Report("Scanning cached images...", 0, True)
            Try
                Dim progressAction As Action(Of CacheAssetProgress) =
                    Sub(value)
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim percent = If(value.Total = 0, 0, value.Current * 100.0 / value.Total)
                                                   AppServices.Report($"Scanning images · {value.Found:N0} found", percent)
                                               End Sub)
                    End Sub
                entries = Await Task.Run(Function()
                                             Dim scanned = RobloxCacheAssetExtractor.Scan(progressAction, RobloxCacheFileType.Png, RobloxCacheFileType.Jpeg, RobloxCacheFileType.Bmp, RobloxCacheFileType.WebP)
                                             AssetNameStore.ApplySavedNames(scanned)
                                             Return scanned
                                         End Function)
                ApplyFilter()
                AppServices.SetCount("Images", entries.Count)
                SessionStateStore.SaveCacheAssets("images", entries)
                AppServices.Report($"Found {entries.Count:N0} cached images.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Image scan failed", MessageBoxButton.OK, MessageBoxImage.Error)
                AppServices.Report("Image scan failed.")
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub ScanButton_Click(sender As Object, e As RoutedEventArgs)
            Await StartScanAsync()
        End Sub

        Private Sub SearchBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            ApplyFilter()
        End Sub

        Private Sub ApplyFilter()
            If AssetList Is Nothing Then Return
            Dim query = If(SearchBox?.Text, String.Empty).Trim()
            AssetList.ItemsSource = If(query.Length = 0, entries, entries.Where(Function(item) item.Hash.Contains(query, StringComparison.OrdinalIgnoreCase) OrElse item.FriendlyName.Contains(query, StringComparison.OrdinalIgnoreCase) OrElse item.TypeLabel.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList())
        End Sub

        Private Async Sub AssetList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim entry = TryCast(AssetList.SelectedItem, RobloxCacheAssetEntry)
            RenameButton.IsEnabled = Not busy AndAlso entry IsNot Nothing
            ExportButton.IsEnabled = Not busy AndAlso entry IsNot Nothing
            previewVersion += 1
            Dim version = previewVersion
            If entry Is Nothing Then
                PreviewImage.Source = Nothing
                EmptyPreview.Visibility = Visibility.Visible
                PreviewDetails.Text = String.Empty
                Return
            End If
            AppServices.Report($"Loading {entry.TypeLabel.ToLowerInvariant()} preview...", 0, True)
            Try
                Dim bitmap = Await Task.Run(Function() DecodeImage(RobloxCacheAssetExtractor.ReadPayload(entry)))
                If version <> previewVersion Then Return
                PreviewImage.Source = bitmap
                EmptyPreview.Visibility = Visibility.Collapsed
                PreviewDetails.Text = $"{bitmap.PixelWidth:N0} × {bitmap.PixelHeight:N0} · {entry.TypeLabel} · {AppServices.FormatBytes(entry.Size)}"
                AppServices.Report($"Previewing {AppServices.SafePrefix(entry.Hash)}.", 100)
            Catch ex As Exception
                If version <> previewVersion Then Return
                PreviewImage.Source = Nothing
                EmptyPreview.Visibility = Visibility.Visible
                PreviewDetails.Text = "Preview unavailable"
                AppServices.Report($"Image preview failed: {ex.Message}")
            End Try
        End Sub

        Private Shared Function DecodeImage(payload As Byte()) As BitmapImage
            Dim png As Byte()
            Using image As New MagickImage(payload)
                image.Format = MagickFormat.Png
                png = image.ToByteArray()
            End Using
            Using stream As New MemoryStream(png, writable:=False)
                Dim bitmap As New BitmapImage()
                bitmap.BeginInit()
                bitmap.CacheOption = BitmapCacheOption.OnLoad
                bitmap.StreamSource = stream
                bitmap.EndInit()
                bitmap.Freeze()
                Return bitmap
            End Using
        End Function

        Private Async Sub RenameButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(AssetList.SelectedItem, RobloxCacheAssetEntry)
            If entry Is Nothing Then Return
            If Await AssetNameStore.PromptAndSaveAsync(Window.GetWindow(Me), entry, Function() RobloxCacheAssetExtractor.ReadPayload(entry)) Then
                AssetList.Items.Refresh()
                ApplyFilter()
            End If
        End Sub

        Private Async Sub ExportButton_Click(sender As Object, e As RoutedEventArgs)
            Dim entry = TryCast(AssetList.SelectedItem, RobloxCacheAssetEntry)
            If entry Is Nothing Then Return
            Dim dialog As New SaveFileDialog With {.Title = "Export cached image", .FileName = entry.ExportBaseName & entry.Extension, .DefaultExt = entry.Extension.TrimStart("."c), .Filter = $"{entry.TypeLabel} (*{entry.Extension})|*{entry.Extension}|All files|*.*"}
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
            Dim dialog As New OpenFolderDialog With {.Title = "Choose a folder for cached images"}
            If dialog.ShowDialog() <> True Then Return
            Dim folder = dialog.FolderName
            SetBusy(True)
            AppServices.Report($"Exporting {entries.Count:N0} images...", 0, True)
            Try
                Dim summary = Await Task.Run(Function() RobloxCacheAssetExtractor.ExportMany(entries, folder, Nothing))
                AppServices.Report($"Exported {summary.Exported:N0}; reused {summary.Reused:N0}; failed {summary.Failed:N0}.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Bulk export failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Sub

        Public Sub ClearSavedNames()
            AssetNameStore.ClearLoadedNames(entries)
            ApplyFilter()
        End Sub

        Public Sub ResetData()
            previewVersion += 1
            entries.Clear()
            ApplyFilter()
            PreviewImage.Source = Nothing
            EmptyPreview.Visibility = Visibility.Visible
            PreviewDetails.Text = String.Empty
            AppServices.SetCount("Images", 0)
        End Sub

        Public Sub RestoreState()
            Dim restored = SessionStateStore.LoadCacheAssets("images")
            If restored.Count = 0 Then Return
            entries = restored
            ApplyFilter()
            AppServices.SetCount("Images", entries.Count)
            SetBusy(False)
        End Sub
        Private Sub SetBusy(value As Boolean)
            busy = value
            ScanButton.IsEnabled = Not value
            RenameButton.IsEnabled = Not value AndAlso AssetList.SelectedItem IsNot Nothing
            AssetList.IsHitTestVisible = Not value
            ExportButton.IsEnabled = Not value AndAlso AssetList.SelectedItem IsNot Nothing
            ExportAllButton.IsEnabled = Not value AndAlso entries.Count > 0
        End Sub
    End Class
End Namespace
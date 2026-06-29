Imports System.IO
Imports System.Text
Imports ImageMagick
Imports Microsoft.Win32

Namespace Views
    Public Class ExtraAssetsView
        Private thumbnails As New List(Of SupplementalCacheEntry)()
        Private fonts As New List(Of SupplementalCacheEntry)()
        Private metadata As New List(Of SupplementalCacheEntry)()
        Private busy As Boolean
        Private previewVersion As Integer
        Private metadataPreviewVersion As Integer

        Private Async Function ScanAsync(label As String, scanner As Func(Of Action(Of SupplementalScanProgress), List(Of SupplementalCacheEntry))) As Task(Of List(Of SupplementalCacheEntry))
            If busy Then Return New List(Of SupplementalCacheEntry)()
            SetBusy(True)
            AppServices.Report($"Scanning cached {label}...", 0, True)
            Try
                Dim progressAction As Action(Of SupplementalScanProgress) =
                    Sub(value)
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim percent = If(value.Total = 0, 0, value.Current * 100.0 / value.Total)
                                                   AppServices.Report($"Scanning {label} · {value.Found:N0} found", percent)
                                               End Sub)
                    End Sub
                Dim result = Await Task.Run(Function()
                                                Dim scanned = scanner(progressAction)
                                                AssetNameStore.ApplySavedNames(scanned)
                                                Return scanned
                                            End Function)
                AppServices.Report($"Found {result.Count:N0} cached {label}.", 100)
                Return result
            Catch ex As Exception
                AppDialog.Show(ex.Message, $"{label} scan failed", MessageBoxButton.OK, MessageBoxImage.Error)
                AppServices.Report($"{label} scan failed.")
                Return New List(Of SupplementalCacheEntry)()
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub ScanThumbnailsButton_Click(sender As Object, e As RoutedEventArgs)
            thumbnails = Await ScanAsync("thumbnails", AddressOf RobloxSupplementalCacheExtractor.ScanThumbnails)
            ThumbnailList.ItemsSource = thumbnails
            ExportAllThumbnailsButton.IsEnabled = thumbnails.Count > 0
        End Sub

        Private Async Sub ScanFontsButton_Click(sender As Object, e As RoutedEventArgs)
            fonts = Await ScanAsync("fonts", AddressOf RobloxSupplementalCacheExtractor.ScanFonts)
            FontList.ItemsSource = fonts
            ExportAllFontsButton.IsEnabled = fonts.Count > 0
        End Sub

        Private Async Sub ScanMetadataButton_Click(sender As Object, e As RoutedEventArgs)
            metadata = Await ScanAsync("metadata files", AddressOf RobloxSupplementalCacheExtractor.ScanMetadata)
            MetadataList.ItemsSource = metadata
            ExportAllMetadataButton.IsEnabled = metadata.Count > 0
        End Sub

        Private Async Sub ThumbnailList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim entry = TryCast(ThumbnailList.SelectedItem, SupplementalCacheEntry)
            RenameThumbnailButton.IsEnabled = Not busy AndAlso entry IsNot Nothing
            ExportThumbnailButton.IsEnabled = Not busy AndAlso entry IsNot Nothing
            previewVersion += 1
            Dim version = previewVersion
            If entry Is Nothing Then
                ThumbnailPreview.Source = Nothing
                Return
            End If
            Try
                Dim bitmap = Await Task.Run(Function() DecodeImage(RobloxSupplementalCacheExtractor.ReadPayload(entry)))
                If version = previewVersion Then ThumbnailPreview.Source = bitmap
            Catch ex As Exception
                If version = previewVersion Then ThumbnailPreview.Source = Nothing
                AppServices.Report($"Thumbnail preview failed: {ex.Message}")
            End Try
        End Sub

        Private Sub FontList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            PreviewFontButton.IsEnabled = Not busy AndAlso FontList.SelectedItem IsNot Nothing
            RenameFontButton.IsEnabled = Not busy AndAlso FontList.SelectedItem IsNot Nothing
            ExportFontButton.IsEnabled = Not busy AndAlso FontList.SelectedItem IsNot Nothing
        End Sub

        Private Async Sub FontList_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
            Await PreviewSelectedFontAsync()
        End Sub

        Private Async Sub PreviewFontButton_Click(sender As Object, e As RoutedEventArgs)
            Await PreviewSelectedFontAsync()
        End Sub

        Private Async Function PreviewSelectedFontAsync() As Task
            Dim entry = TryCast(FontList.SelectedItem, SupplementalCacheEntry)
            If entry Is Nothing Then Return
            SetBusy(True)
            AppServices.Report("Loading font preview...", 0, True)
            Try
                Dim payload = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ReadPayload(entry))
                Dim preview As New FontPreviewWindow(entry, payload) With {.Owner = Window.GetWindow(Me)}
                preview.Show()
                AppServices.Report($"Previewing {entry.FriendlyName}.", 100)
            Catch ex As Exception
                AppServices.Report($"Font preview failed: {ex.Message}")
                AppDialog.Show(ex.Message, "Font preview failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub MetadataList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim entry = TryCast(MetadataList.SelectedItem, SupplementalCacheEntry)
            RenameMetadataButton.IsEnabled = Not busy AndAlso entry IsNot Nothing
            ExportMetadataButton.IsEnabled = Not busy AndAlso entry IsNot Nothing
            metadataPreviewVersion += 1
            Dim version = metadataPreviewVersion
            If entry Is Nothing Then
                MetadataPreview.Clear()
                Return
            End If
            MetadataPreview.Text = "Loading preview..."
            Try
                Dim payload = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ReadPayload(entry))
                If version = metadataPreviewVersion Then MetadataPreview.Text = Encoding.UTF8.GetString(payload)
            Catch ex As Exception
                If version = metadataPreviewVersion Then MetadataPreview.Text = $"Preview failed: {ex.Message}"
                AppServices.Report($"Metadata preview failed: {ex.Message}")
            End Try
        End Sub

        Private Async Sub RenameThumbnailButton_Click(sender As Object, e As RoutedEventArgs)
            Await RenameSupplementalAsync(ThumbnailList, TryCast(ThumbnailList.SelectedItem, SupplementalCacheEntry))
        End Sub

        Private Async Sub RenameFontButton_Click(sender As Object, e As RoutedEventArgs)
            Await RenameSupplementalAsync(FontList, TryCast(FontList.SelectedItem, SupplementalCacheEntry))
        End Sub

        Private Async Sub RenameMetadataButton_Click(sender As Object, e As RoutedEventArgs)
            Await RenameSupplementalAsync(MetadataList, TryCast(MetadataList.SelectedItem, SupplementalCacheEntry))
        End Sub

        Private Async Function RenameSupplementalAsync(list As ListView, entry As SupplementalCacheEntry) As Task
            If entry Is Nothing Then Return
            If Await AssetNameStore.PromptAndSaveAsync(Window.GetWindow(Me), entry, Function() RobloxSupplementalCacheExtractor.ReadPayload(entry)) Then list.Items.Refresh()
        End Function

        Private Async Sub ExportThumbnailButton_Click(sender As Object, e As RoutedEventArgs)
            Await ExportSelectedAsync(TryCast(ThumbnailList.SelectedItem, SupplementalCacheEntry))
        End Sub

        Private Async Sub ExportFontButton_Click(sender As Object, e As RoutedEventArgs)
            Await ExportSelectedAsync(TryCast(FontList.SelectedItem, SupplementalCacheEntry))
        End Sub

        Private Async Sub ExportMetadataButton_Click(sender As Object, e As RoutedEventArgs)
            Await ExportSelectedAsync(TryCast(MetadataList.SelectedItem, SupplementalCacheEntry))
        End Sub

        Private Async Function ExportSelectedAsync(entry As SupplementalCacheEntry) As Task
            If entry Is Nothing Then Return
            Dim dialog As New SaveFileDialog With {.Title = $"Export {entry.TypeLabel}", .FileName = entry.ExportBaseName & entry.Extension, .DefaultExt = entry.Extension.TrimStart("."c), .Filter = $"{entry.TypeLabel} (*{entry.Extension})|*{entry.Extension}|All files|*.*"}
            If dialog.ShowDialog() <> True Then Return
            SetBusy(True)
            Try
                Await Task.Run(Sub() RobloxSupplementalCacheExtractor.Export(entry, dialog.FileName))
                AppServices.Report($"Exported {Path.GetFileName(dialog.FileName)}.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Async Sub ExportAllThumbnailsButton_Click(sender As Object, e As RoutedEventArgs)
            Await ExportAllAsync(thumbnails, "cached thumbnails")
        End Sub

        Private Async Sub ExportAllFontsButton_Click(sender As Object, e As RoutedEventArgs)
            Await ExportAllAsync(fonts, "cached fonts")
        End Sub

        Private Async Sub ExportAllMetadataButton_Click(sender As Object, e As RoutedEventArgs)
            Await ExportAllAsync(metadata, "cached metadata")
        End Sub

        Private Async Function ExportAllAsync(items As List(Of SupplementalCacheEntry), label As String) As Task
            If items.Count = 0 Then Return
            Dim dialog As New OpenFolderDialog With {.Title = $"Choose a folder for {label}"}
            If dialog.ShowDialog() <> True Then Return
            Dim folder = dialog.FolderName
            SetBusy(True)
            Try
                Dim progressAction As Action(Of Integer, Integer) =
                    Sub(current, total)
                        If current Mod 25 = 0 OrElse current = total Then Dispatcher.BeginInvoke(Sub() AppServices.Report($"Exporting {current:N0} of {total:N0}...", current * 100.0 / total))
                    End Sub
                Dim summary = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ExportMany(items, folder, progressAction))
                AppServices.Report($"Exported {summary.Exported:N0}; reused {summary.Reused:N0}; failed {summary.Failed:N0}.", 100)
            Catch ex As Exception
                AppDialog.Show(ex.Message, "Bulk export failed", MessageBoxButton.OK, MessageBoxImage.Error)
            Finally
                SetBusy(False)
            End Try
        End Function

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

        Public Sub ClearSavedNames()
            AssetNameStore.ClearLoadedNames(thumbnails)
            AssetNameStore.ClearLoadedNames(fonts)
            AssetNameStore.ClearLoadedNames(metadata)
            ThumbnailList.Items.Refresh()
            FontList.Items.Refresh()
            MetadataList.Items.Refresh()
        End Sub

        Public Sub ResetData()
            previewVersion += 1
            thumbnails.Clear()
            fonts.Clear()
            metadata.Clear()
            ThumbnailList.ItemsSource = Nothing
            FontList.ItemsSource = Nothing
            MetadataList.ItemsSource = Nothing
            ThumbnailPreview.Source = Nothing
            MetadataPreview.Clear()
            SetBusy(False)
        End Sub
        Private Sub SetBusy(value As Boolean)
            busy = value
            ScanThumbnailsButton.IsEnabled = Not value
            ScanFontsButton.IsEnabled = Not value
            PreviewFontButton.IsEnabled = Not value AndAlso FontList.SelectedItem IsNot Nothing
            ScanMetadataButton.IsEnabled = Not value
            RenameThumbnailButton.IsEnabled = Not value AndAlso ThumbnailList.SelectedItem IsNot Nothing
            RenameFontButton.IsEnabled = Not value AndAlso FontList.SelectedItem IsNot Nothing
            RenameMetadataButton.IsEnabled = Not value AndAlso MetadataList.SelectedItem IsNot Nothing
            ThumbnailList.IsHitTestVisible = Not value
            FontList.IsHitTestVisible = Not value
            MetadataList.IsHitTestVisible = Not value
            ExportThumbnailButton.IsEnabled = Not value AndAlso ThumbnailList.SelectedItem IsNot Nothing
            ExportFontButton.IsEnabled = Not value AndAlso FontList.SelectedItem IsNot Nothing
            ExportMetadataButton.IsEnabled = Not value AndAlso MetadataList.SelectedItem IsNot Nothing
            ExportAllThumbnailsButton.IsEnabled = Not value AndAlso thumbnails.Count > 0
            ExportAllFontsButton.IsEnabled = Not value AndAlso fonts.Count > 0
            ExportAllMetadataButton.IsEnabled = Not value AndAlso metadata.Count > 0
        End Sub
    End Class
End Namespace
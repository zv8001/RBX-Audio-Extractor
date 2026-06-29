Imports System.IO
Imports System.Windows.Threading
Imports Microsoft.Win32
Imports NAudio.Vorbis
Imports NAudio.Wave

Namespace Views
    Public Class AudioView
        Private entries As New List(Of RobloxCacheAssetEntry)()
        Private output As WaveOutEvent
        Private reader As WaveStream
        Private payloadStream As MemoryStream
        Private ReadOnly timer As DispatcherTimer
        Private updatingSlider As Boolean
        Private userSeeking As Boolean
        Private busy As Boolean
        Private Sub AssetList_SizeChanged(sender As Object, e As SizeChangedEventArgs)
            AppServices.FitGridViewColumns(AssetList, (NameColumn, 0.2), (CacheKeyColumn, 0.27), (FormatColumn, 0.15), (DurationColumn, 0.12), (BytesColumn, 0.16), (InlineColumn, 0.1))
        End Sub

        Public Sub New()
            InitializeComponent()
            timer = New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(250)}
            AddHandler timer.Tick, AddressOf PlaybackTimer_Tick
        End Sub

        Public Async Function StartScanAsync() As Task
            If busy Then Return
            SetBusy(True)
            AppServices.Report("Scanning cached audio...", 0, True)
            Try
                Dim progressAction As Action(Of CacheAssetProgress) =
                    Sub(value)
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim percent = If(value.Total = 0, 0, value.Current * 100.0 / value.Total)
                                                   AppServices.Report($"Scanning audio · {value.Found:N0} found", percent)
                                               End Sub)
                    End Sub
                Dim durationProgress As Action(Of Integer, Integer) =
                    Sub(current, total)
                        Dispatcher.BeginInvoke(Sub()
                                                   Dim percent = If(total = 0, 0, current * 100.0 / total)
                                                   AppServices.Report($"Reading audio durations · {current:N0} of {total:N0}", percent)
                                               End Sub)
                    End Sub
                entries = Await Task.Run(Function()
                                             Dim scanned = RobloxCacheAssetExtractor.Scan(progressAction, RobloxCacheFileType.Ogg, RobloxCacheFileType.Mp3)
                                             RobloxCacheAssetExtractor.PopulateAudioDurations(scanned, durationProgress)
                                             AssetNameStore.ApplySavedNames(scanned)
                                             Return scanned.OrderByDescending(Function(item) item.DurationSeconds.GetValueOrDefault()).ThenBy(Function(item) item.Hash).ToList()
                                         End Function)
                ApplyFilter()
                AppServices.SetCount("Audio", entries.Count)
                SessionStateStore.SaveCacheAssets("audio", entries)
                AppServices.Report($"Found {entries.Count:N0} cached audio files.", 100)
            Catch ex As Exception
                AppServices.Report("Audio scan failed.")
                AppDialog.Show(ex.Message, "Audio scan failed", MessageBoxButton.OK, MessageBoxImage.Error)
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

        Private Sub AssetList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            RenameButton.IsEnabled = Not busy AndAlso AssetList.SelectedItem IsNot Nothing
            ExportButton.IsEnabled = Not busy AndAlso AssetList.SelectedItem IsNot Nothing
            Dim entry = TryCast(AssetList.SelectedItem, RobloxCacheAssetEntry)
            If entry IsNot Nothing Then NowPlayingText.Text = $"{AppServices.SafePrefix(entry.Hash, 20)} · {entry.TypeLabel} · {AppServices.FormatBytes(entry.Size)}"
        End Sub

        Private Async Sub AssetList_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
            Await PlaySelectedAsync()
        End Sub

        Private Async Sub PlayPauseButton_Click(sender As Object, e As RoutedEventArgs)
            If output IsNot Nothing AndAlso output.PlaybackState = PlaybackState.Playing Then
                output.Pause()
                PlayPauseButton.Content = "Resume"
                Return
            End If
            If output IsNot Nothing AndAlso output.PlaybackState = PlaybackState.Paused Then
                output.Play()
                PlayPauseButton.Content = "Pause"
                Return
            End If
            Await PlaySelectedAsync()
        End Sub

        Private Async Function PlaySelectedAsync() As Task
            Dim entry = TryCast(AssetList.SelectedItem, RobloxCacheAssetEntry)
            If entry Is Nothing Then Return
            SetBusy(True, keepListEnabled:=True)
            AppServices.Report($"Loading {entry.TypeLabel.ToLowerInvariant()}...", 0, True)
            Try
                Dim payload = Await Task.Run(Function() RobloxCacheAssetExtractor.ReadPayload(entry))
                StopPlayback()
                payloadStream = New MemoryStream(payload, writable:=False)
                If entry.FileType = RobloxCacheFileType.Ogg Then
                    reader = New VorbisWaveReader(payloadStream)
                Else
                    reader = New Mp3FileReader(payloadStream)
                End If
                output = New WaveOutEvent With {.Volume = CSng(VolumeSlider.Value)}
                AddHandler output.PlaybackStopped, AddressOf Output_PlaybackStopped
                output.Init(reader)
                output.Play()
                timer.Start()
                PlayPauseButton.Content = "Pause"
                PositionSlider.Maximum = Math.Max(0, reader.TotalTime.TotalSeconds)
                NowPlayingText.Text = $"{AppServices.SafePrefix(entry.Hash, 24)} · {entry.TypeLabel}"
                AppServices.Report($"Playing {AppServices.SafePrefix(entry.Hash)}.", 100)
            Catch ex As Exception
                StopPlayback()
                AppDialog.Show(ex.Message, "Playback failed", MessageBoxButton.OK, MessageBoxImage.Error)
                AppServices.Report("Audio playback failed.")
            Finally
                SetBusy(False)
            End Try
        End Function

        Private Sub Output_PlaybackStopped(sender As Object, e As StoppedEventArgs)
            Dispatcher.BeginInvoke(Sub()
                                       If reader IsNot Nothing AndAlso reader.Position >= reader.Length Then
                                           PlayPauseButton.Content = "Play"
                                           timer.Stop()
                                       End If
                                   End Sub)
        End Sub

        Private Sub StopButton_Click(sender As Object, e As RoutedEventArgs)
            StopPlayback()
        End Sub

        Public Sub StopPlayback()
            timer.Stop()
            If output IsNot Nothing Then
                RemoveHandler output.PlaybackStopped, AddressOf Output_PlaybackStopped
                output.Stop()
                output.Dispose()
                output = Nothing
            End If
            If reader IsNot Nothing Then reader.Dispose()
            reader = Nothing
            If payloadStream IsNot Nothing Then payloadStream.Dispose()
            payloadStream = Nothing
            PositionSlider.Value = 0
            TimeText.Text = "00:00 / 00:00"
            PlayPauseButton.Content = "Play"
        End Sub

        Private Sub PlaybackTimer_Tick(sender As Object, e As EventArgs)
            Dim activeReader = reader
            If activeReader Is Nothing OrElse userSeeking Then Return
            updatingSlider = True
            Try
                Dim currentSeconds = Math.Max(PositionSlider.Minimum, Math.Min(PositionSlider.Maximum, activeReader.CurrentTime.TotalSeconds))
                PositionSlider.Value = currentSeconds
                TimeText.Text = $"{FormatTime(activeReader.CurrentTime)} / {FormatTime(activeReader.TotalTime)}"
            Finally
                updatingSlider = False
            End Try
        End Sub

        Private Sub PositionSlider_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            userSeeking = True
            If IsThumbInteraction(TryCast(e.OriginalSource, DependencyObject)) Then Return

            Dim slider = TryCast(sender, Slider)
            If slider Is Nothing Then Return
            slider.CaptureMouse()
            UpdateSliderFromPointer(slider, e)
            SeekToSliderValue()
            e.Handled = True
        End Sub

        Private Sub PositionSlider_PreviewMouseMove(sender As Object, e As MouseEventArgs)
            Dim slider = TryCast(sender, Slider)
            If Not userSeeking OrElse slider Is Nothing OrElse Not slider.IsMouseCaptured OrElse e.LeftButton <> MouseButtonState.Pressed Then Return
            UpdateSliderFromPointer(slider, e)
            SeekToSliderValue()
            e.Handled = True
        End Sub

        Private Sub PositionSlider_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            Dim slider = TryCast(sender, Slider)
            Dim trackDrag = slider IsNot Nothing AndAlso slider.IsMouseCaptured
            Try
                If trackDrag Then UpdateSliderFromPointer(slider, e)
                SeekToSliderValue()
            Finally
                If trackDrag Then slider.ReleaseMouseCapture()
                userSeeking = False
            End Try
            e.Handled = trackDrag
        End Sub

        Private Shared Sub UpdateSliderFromPointer(slider As Slider, e As MouseEventArgs)
            If slider.ActualWidth <= 0 OrElse slider.Maximum <= slider.Minimum Then Return
            Dim ratio = Math.Max(0, Math.Min(1, e.GetPosition(slider).X / slider.ActualWidth))
            slider.Value = slider.Minimum + ratio * (slider.Maximum - slider.Minimum)
        End Sub

        Private Sub SeekToSliderValue()
            Dim activeReader = reader
            If activeReader Is Nothing OrElse activeReader IsNot reader OrElse output Is Nothing OrElse updatingSlider Then Return
            Try
                If Not activeReader.CanSeek Then Return
                Dim totalSeconds = activeReader.TotalTime.TotalSeconds
                If totalSeconds <= 0 OrElse Double.IsNaN(totalSeconds) OrElse Double.IsInfinity(totalSeconds) Then Return
                Dim waveFormat = activeReader.WaveFormat
                If waveFormat Is Nothing Then Return
                Dim blockSeconds = waveFormat.BlockAlign / CDbl(Math.Max(1, waveFormat.AverageBytesPerSecond))
                Dim maximumSeek = Math.Max(0, totalSeconds - Math.Max(0.001, blockSeconds))
                Dim requested = PositionSlider.Value
                If Double.IsNaN(requested) OrElse Double.IsInfinity(requested) Then Return
                If activeReader IsNot reader OrElse output Is Nothing Then Return
                activeReader.CurrentTime = TimeSpan.FromSeconds(Math.Max(0, Math.Min(maximumSeek, requested)))
            Catch ex As Exception When TypeOf ex Is ArgumentOutOfRangeException OrElse
                                            TypeOf ex Is ObjectDisposedException OrElse
                                            TypeOf ex Is NullReferenceException
                AppServices.AddLog($"Seek ignored because the decoder was no longer in a valid seek state: {ex.GetType().Name}.")
            End Try
        End Sub

        Private Shared Function IsThumbInteraction(source As DependencyObject) As Boolean
            Dim current = source
            While current IsNot Nothing
                If TypeOf current Is System.Windows.Controls.Primitives.Thumb Then Return True
                current = VisualTreeHelper.GetParent(current)
            End While
            Return False
        End Function

        Private Sub VolumeSlider_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If IsThumbInteraction(TryCast(e.OriginalSource, DependencyObject)) Then Return
            Dim slider = TryCast(sender, Slider)
            If slider Is Nothing Then Return
            slider.CaptureMouse()
            UpdateSliderFromPointer(slider, e)
            e.Handled = True
        End Sub

        Private Sub VolumeSlider_PreviewMouseMove(sender As Object, e As MouseEventArgs)
            Dim slider = TryCast(sender, Slider)
            If slider Is Nothing OrElse Not slider.IsMouseCaptured OrElse e.LeftButton <> MouseButtonState.Pressed Then Return
            UpdateSliderFromPointer(slider, e)
            e.Handled = True
        End Sub

        Private Sub VolumeSlider_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            Dim slider = TryCast(sender, Slider)
            Dim trackDrag = slider IsNot Nothing AndAlso slider.IsMouseCaptured
            If Not trackDrag Then Return
            Try
                UpdateSliderFromPointer(slider, e)
            Finally
                slider.ReleaseMouseCapture()
            End Try
            e.Handled = True
        End Sub

        Private Sub VolumeSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
            If output IsNot Nothing Then output.Volume = CSng(e.NewValue)
        End Sub

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
            Dim dialog As New SaveFileDialog With {.Title = "Export cached audio", .FileName = entry.ExportBaseName & entry.Extension, .DefaultExt = entry.Extension.TrimStart("."c), .Filter = $"{entry.TypeLabel} (*{entry.Extension})|*{entry.Extension}|All files|*.*"}
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
            Dim dialog As New OpenFolderDialog With {.Title = "Choose a folder for cached audio"}
            If dialog.ShowDialog() <> True Then Return
            Dim folder = dialog.FolderName
            SetBusy(True)
            AppServices.Report($"Exporting {entries.Count:N0} audio files...", 0, True)
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
            StopPlayback()
            entries.Clear()
            ApplyFilter()
            AppServices.SetCount("Audio", 0)
            AppServices.Report("Audio results cleared. Scan the cache again.")
        End Sub

        Public Sub RestoreState()
            Dim restored = SessionStateStore.LoadCacheAssets("audio")
            If restored.Count = 0 Then Return
            entries = restored
            ApplyFilter()
            AppServices.SetCount("Audio", entries.Count)
            SetBusy(False)
        End Sub
        Private Sub SetBusy(value As Boolean, Optional keepListEnabled As Boolean = False)
            busy = value
            ScanButton.IsEnabled = Not value
            RenameButton.IsEnabled = Not value AndAlso AssetList.SelectedItem IsNot Nothing
            AssetList.IsHitTestVisible = Not value OrElse keepListEnabled
            ExportButton.IsEnabled = Not value AndAlso AssetList.SelectedItem IsNot Nothing
            ExportAllButton.IsEnabled = Not value AndAlso entries.Count > 0
        End Sub

        Private Shared Function FormatTime(value As TimeSpan) As String
            Return If(value.TotalHours >= 1, value.ToString("h\:mm\:ss"), value.ToString("mm\:ss"))
        End Function
    End Class
End Namespace
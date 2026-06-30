Imports System.IO
Imports System.Text.Json
Imports Microsoft.Web.WebView2.Core

Public Class VideoPreviewWindow
    Private Const PreviewHost As String = "rbx-video.local"
    Private ReadOnly entry As RobloxVideoEntry
    Private ReadOnly package As VideoPreviewPackage
    Private segmentIndex As Integer
    Private currentSegmentSeconds As Double
    Private isPlaying As Boolean
    Private draggingPosition As Boolean
    Private closingStarted As Boolean
    Private webReady As Boolean

    Public Sub New(video As RobloxVideoEntry, previewPackage As VideoPreviewPackage)
        InitializeComponent()
        entry = video
        package = previewPackage
        PositionSlider.Maximum = Math.Max(0.001, entry.DurationSeconds)
        WindowTitleText.Text = $"Video preview · {entry.FriendlyName}"
        VideoNameText.Text = entry.FriendlyName
        VideoDetailsText.Text = $"{entry.Resolution} · {entry.DurationLabel} · {entry.Segments.Count:N0} WebM segments · {entry.Size:N0} bytes"
        PlayPauseButton.Content = "Play"
        UpdateTime(0)
    End Sub

    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Try
            Dim profileDirectory = Path.Combine(AssetNameStore.DataDirectory, "WebView2")
            Directory.CreateDirectory(profileDirectory)
            Dim options As New CoreWebView2EnvironmentOptions With {.AdditionalBrowserArguments = "--autoplay-policy=no-user-gesture-required"}
            Dim environment = Await CoreWebView2Environment.CreateAsync(Nothing, profileDirectory, options)
            Await Player.EnsureCoreWebView2Async(environment)

            With Player.CoreWebView2.Settings
                .AreBrowserAcceleratorKeysEnabled = False
                .AreDefaultContextMenusEnabled = False
                .AreDevToolsEnabled = False
                .IsBuiltInErrorPageEnabled = False
                .IsGeneralAutofillEnabled = False
                .IsPasswordAutosaveEnabled = False
                .IsStatusBarEnabled = False
                .IsZoomControlEnabled = False
            End With
            AddHandler Player.CoreWebView2.WebMessageReceived, AddressOf Player_WebMessageReceived
            AddHandler Player.CoreWebView2.NavigationStarting, AddressOf Player_NavigationStarting
            AddHandler Player.CoreWebView2.NavigationCompleted, AddressOf Player_NavigationCompleted
            AddHandler Player.CoreWebView2.NewWindowRequested, AddressOf Player_NewWindowRequested
            Player.CoreWebView2.SetVirtualHostNameToFolderMapping(PreviewHost, package.DirectoryPath, CoreWebView2HostResourceAccessKind.DenyCors)
            Player.CoreWebView2.Navigate($"https://{PreviewHost}/player.html")
        Catch ex As Exception
            ShowPlaybackError("Microsoft Edge WebView2 could not start. Install or repair the WebView2 Runtime, then reopen the preview." & vbCrLf & vbCrLf & ex.Message)
        End Try
    End Sub

    Private Sub Player_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs)
        Dim destination As Uri = Nothing
        If Not Uri.TryCreate(e.Uri, UriKind.Absolute, destination) OrElse Not destination.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) OrElse Not destination.Host.Equals(PreviewHost, StringComparison.OrdinalIgnoreCase) Then
            e.Cancel = True
        End If
    End Sub

    Private Sub Player_NewWindowRequested(sender As Object, e As CoreWebView2NewWindowRequestedEventArgs)
        e.Handled = True
    End Sub

    Private Sub Player_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)
        If Not e.IsSuccess Then
            ShowPlaybackError($"The local video player could not load ({e.WebErrorStatus}).")
            Return
        End If
        webReady = True
        OpenSegment(0, 0, shouldPlay:=False)
        SendCommand(New Dictionary(Of String, Object) From {{"action", "volume"}, {"value", VolumeSlider.Value}})
    End Sub

    Private Sub Player_WebMessageReceived(sender As Object, e As CoreWebView2WebMessageReceivedEventArgs)
        Try
            Using document = JsonDocument.Parse(e.WebMessageAsJson)
                Dim root = document.RootElement
                Dim messageType = root.GetProperty("type").GetString()
                Dim sourceIndex As JsonElement
                If root.TryGetProperty("index", sourceIndex) AndAlso sourceIndex.GetInt32() <> segmentIndex Then Return
                Select Case messageType
                    Case "opened"
                        UpdateProgress()
                        PlaybackErrorPanel.Visibility = Visibility.Collapsed
                        Player.Visibility = Visibility.Visible
                        SegmentText.Text = $"Segment {segmentIndex + 1:N0} of {package.Segments.Count:N0}"
                    Case "time"
                        currentSegmentSeconds = Math.Max(0, root.GetProperty("seconds").GetDouble())
                        UpdateProgress()
                    Case "ended"
                        If segmentIndex + 1 < package.Segments.Count Then
                            OpenSegment(segmentIndex + 1, 0, shouldPlay:=True)
                        Else
                            isPlaying = False
                            currentSegmentSeconds = SegmentDuration(segmentIndex)
                            PlayPauseButton.Content = "Play"
                            If Not draggingPosition Then PositionSlider.Value = entry.DurationSeconds
                            UpdateTime(entry.DurationSeconds)
                        End If
                    Case "error"
                        ShowPlaybackError("The Edge media engine could not decode this VP9/Opus WebM segment." & vbCrLf & vbCrLf & root.GetProperty("message").GetString())
                End Select
            End Using
        Catch ex As Exception
            AppServices.AddLog($"Video preview message failed: {ex.Message}")
        End Try
    End Sub

    Private Sub OpenSegment(index As Integer, positionSeconds As Double, shouldPlay As Boolean)
        If Not webReady OrElse index < 0 OrElse index >= package.Segments.Count Then Return
        segmentIndex = index
        currentSegmentSeconds = Math.Max(0, positionSeconds)
        isPlaying = shouldPlay
        PlayPauseButton.Content = If(shouldPlay, "Pause", "Play")
        SegmentText.Text = $"Segment {index + 1:N0} of {package.Segments.Count:N0}"
        SendCommand(New Dictionary(Of String, Object) From {
            {"action", "open"}, {"index", index}, {"position", currentSegmentSeconds}, {"duration", SegmentDuration(index)}, {"play", shouldPlay}})
        UpdateProgress()
    End Sub

    Private Sub PlayPauseButton_Click(sender As Object, e As RoutedEventArgs)
        If Not webReady Then Return
        If isPlaying Then
            SendCommand(New Dictionary(Of String, Object) From {{"action", "pause"}})
            isPlaying = False
            PlayPauseButton.Content = "Play"
        Else
            If PositionSlider.Value >= entry.DurationSeconds - 0.05 Then
                SeekTo(0, shouldPlay:=True)
                Return
            End If
            SendCommand(New Dictionary(Of String, Object) From {{"action", "play"}})
            isPlaying = True
            PlayPauseButton.Content = "Pause"
        End If
    End Sub

    Private Sub StopButton_Click(sender As Object, e As RoutedEventArgs)
        SeekTo(0, shouldPlay:=False)
    End Sub

    Private Sub PositionSlider_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        draggingPosition = True
        PositionSlider.CaptureMouse()
        UpdatePositionSlider(e.GetPosition(PositionSlider).X)
        e.Handled = True
    End Sub

    Private Sub PositionSlider_PreviewMouseMove(sender As Object, e As MouseEventArgs)
        If Not draggingPosition OrElse e.LeftButton <> MouseButtonState.Pressed Then Return
        UpdatePositionSlider(e.GetPosition(PositionSlider).X)
        e.Handled = True
    End Sub

    Private Sub PositionSlider_PreviewMouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If Not draggingPosition Then Return
        UpdatePositionSlider(e.GetPosition(PositionSlider).X)
        draggingPosition = False
        PositionSlider.ReleaseMouseCapture()
        SeekTo(PositionSlider.Value, isPlaying)
        e.Handled = True
    End Sub

    Private Sub UpdatePositionSlider(x As Double)
        If PositionSlider.ActualWidth <= 0 Then Return
        PositionSlider.Value = Math.Max(0, Math.Min(PositionSlider.Maximum, x / PositionSlider.ActualWidth * PositionSlider.Maximum))
        UpdateTime(PositionSlider.Value)
    End Sub

    Private Sub SeekTo(totalSeconds As Double, shouldPlay As Boolean)
        If Not webReady Then Return
        Dim target = Math.Max(0, Math.Min(entry.DurationSeconds, totalSeconds))
        Dim elapsed As Double
        For index = 0 To package.Segments.Count - 1
            Dim duration = SegmentDuration(index)
            If target <= elapsed + duration OrElse index = package.Segments.Count - 1 Then
                Dim within = Math.Max(0, target - elapsed)
                If index = segmentIndex Then
                    currentSegmentSeconds = within
                    SendCommand(New Dictionary(Of String, Object) From {{"action", "seek"}, {"position", within}})
                    SendCommand(New Dictionary(Of String, Object) From {{"action", If(shouldPlay, "play", "pause")}})
                    isPlaying = shouldPlay
                    PlayPauseButton.Content = If(shouldPlay, "Pause", "Play")
                    UpdateProgress()
                Else
                    OpenSegment(index, within, shouldPlay)
                End If
                Return
            End If
            elapsed += duration
        Next
    End Sub

    Private Sub UpdateProgress()
        Dim current = Math.Max(0, Math.Min(entry.DurationSeconds, ElapsedBefore(segmentIndex) + currentSegmentSeconds))
        If Not draggingPosition Then PositionSlider.Value = current
        UpdateTime(current)
    End Sub

    Private Function ElapsedBefore(index As Integer) As Double
        Dim total As Double
        For current = 0 To Math.Min(index, package.Segments.Count) - 1
            total += SegmentDuration(current)
        Next
        Return total
    End Function

    Private Function SegmentDuration(index As Integer) As Double
        If index < 0 OrElse index >= package.Segments.Count Then Return 0
        Return Math.Max(0.001, package.Segments(index).DurationSeconds)
    End Function

    Private Sub UpdateTime(seconds As Double)
        TimeText.Text = $"{FormatTime(seconds)} / {FormatTime(entry.DurationSeconds)}"
    End Sub

    Private Shared Function FormatTime(seconds As Double) As String
        Dim value = TimeSpan.FromSeconds(Math.Max(0, seconds))
        Return If(value.TotalHours >= 1, value.ToString("h\:mm\:ss"), value.ToString("mm\:ss"))
    End Function

    Private Sub VolumeSlider_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If webReady Then SendCommand(New Dictionary(Of String, Object) From {{"action", "volume"}, {"value", e.NewValue}})
    End Sub

    Private Sub SendCommand(message As IDictionary(Of String, Object))
        If Not webReady OrElse Player.CoreWebView2 Is Nothing Then Return
        Player.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message))
    End Sub

    Private Sub ShowPlaybackError(message As String)
        isPlaying = False
        PlayPauseButton.Content = "Play"
        Player.Visibility = Visibility.Collapsed
        PlaybackErrorText.Text = message
        PlaybackErrorPanel.Visibility = Visibility.Visible
        AppServices.AddLog($"Video preview failed: {message.Replace(vbCrLf, " ")}")
    End Sub

    Private Sub TitleBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton <> MouseButton.Left Then Return
        If e.ClickCount = 2 Then
            WindowState = If(WindowState = WindowState.Maximized, WindowState.Normal, WindowState.Maximized)
        Else
            DragMove()
        End If
    End Sub

    Private Sub MinimizeButton_Click(sender As Object, e As RoutedEventArgs)
        WindowState = WindowState.Minimized
    End Sub

    Private Sub MaximizeButton_Click(sender As Object, e As RoutedEventArgs)
        WindowState = If(WindowState = WindowState.Maximized, WindowState.Normal, WindowState.Maximized)
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Escape Then Close()
        If e.Key = Key.Space Then
            PlayPauseButton_Click(PlayPauseButton, New RoutedEventArgs())
            e.Handled = True
        End If
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        closingStarted = True
        Try
            If Player.CoreWebView2 IsNot Nothing Then Player.CoreWebView2.ClearVirtualHostNameToFolderMapping(PreviewHost)
            Player.Dispose()
        Catch
        End Try
        Dim previewDirectory = package.DirectoryPath
        Task.Run(Sub() DeletePreviewDirectory(previewDirectory))
    End Sub

    Private Shared Sub DeletePreviewDirectory(previewDirectory As String)
        For attempt = 1 To 12
            Try
                If Directory.Exists(previewDirectory) Then Directory.Delete(previewDirectory, recursive:=True)
                Return
            Catch
                Threading.Thread.Sleep(200)
            End Try
        Next
    End Sub
End Class
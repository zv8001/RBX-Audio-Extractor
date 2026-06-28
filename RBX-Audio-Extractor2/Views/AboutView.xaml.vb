Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Text.RegularExpressions

Namespace Views
    Public Class AboutView

        Private Const BaseUrl As String = "https://rbxextr.vexthatprotogen.com/"
        Private ReadOnly client As New HttpClient With {.Timeout = TimeSpan.FromSeconds(20)}

        Public Sub New()
            InitializeComponent()
            VersionText.Text = $"Version {AppServices.CurrentVersion}"
            ActivityList.DataContext = AppServices.Activity
        End Sub

        Private Async Sub CheckUpdatesButton_Click(sender As Object, e As RoutedEventArgs)
            AppServices.Report("Checking for updates...", 0, True)
            Try
                Dim latestText = (Await client.GetStringAsync(BaseUrl & "v.txt")).Trim()
                Dim current = ParseVersion(AppServices.CurrentVersion)
                Dim latest = ParseVersion(latestText)
                If latest > current Then
                    DownloadUpdateButton.Visibility = Visibility.Visible
                    AppServices.Report($"Update available: {latestText} (installed {AppServices.CurrentVersion}).", 100)
                    AppDialog.Show($"Version {latestText} is available.", "Update available", MessageBoxButton.OK, MessageBoxImage.Information)
                ElseIf latest < current Then
                    AppServices.Report($"This development build ({AppServices.CurrentVersion}) is newer than the published version ({latestText}).", 100)
                Else
                    AppServices.Report($"You are using the latest version ({AppServices.CurrentVersion}).", 100)
                End If
            Catch ex As Exception
                AppServices.Report($"Update check failed: {ex.Message}")
                AppDialog.Show(ex.Message, "Update check failed", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Public Sub SetCreatorMessage(message As String)
            CreatorMessageText.Text = If(String.IsNullOrWhiteSpace(message), "No message from the creator right now.", message)
        End Sub
        Private Async Sub RefreshMessageButton_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim message = Await RemoteContentService.GetCreatorMessageAsync()
                SetCreatorMessage(message)
                AppServices.Report("Creator message refreshed.", 100)
            Catch ex As Exception
                CreatorMessageText.Text = "The creator message could not be loaded."
                AppServices.Report($"Creator message failed: {ex.Message}")
            End Try
        End Sub

        Private Async Sub DownloadUpdateButton_Click(sender As Object, e As RoutedEventArgs)
            If AppDialog.Show("Download the published executable and replace this application after it closes?", "Install update", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) <> MessageBoxResult.Yes Then Return
            AppServices.Report("Downloading update...", 0, True)
            Try
                Dim payload = Await client.GetByteArrayAsync(BaseUrl & "RBXAssetExtractor.exe")

                Dim currentPath = Environment.ProcessPath
                If String.IsNullOrWhiteSpace(currentPath) Then Throw New InvalidOperationException("The current executable path is unavailable.")
                Dim tempPath = Path.Combine(Path.GetTempPath(), "RBXAssetExtractor.update.exe")
                Await File.WriteAllBytesAsync(tempPath, payload)
                Dim batchPath = Path.Combine(Path.GetTempPath(), "RBXAssetExtractor.update.cmd")
                Dim script = $"@echo off{vbCrLf}timeout /t 2 /nobreak >nul{vbCrLf}move /y ""{tempPath}"" ""{currentPath}"" >nul{vbCrLf}start """" ""{currentPath}""{vbCrLf}del ""%~f0"""
                Await File.WriteAllTextAsync(batchPath, script)
                Process.Start(New ProcessStartInfo(batchPath) With {.UseShellExecute = True, .WindowStyle = ProcessWindowStyle.Hidden})
                Application.Current.Shutdown()
            Catch ex As Exception
                AppServices.Report($"Update failed: {ex.Message}")
                AppDialog.Show(ex.Message, "Update failed", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub OpenGithubButton_Click(sender As Object, e As RoutedEventArgs)
            AppServices.OpenPath("https://github.com/zv8001/RBX-Audio-Extractor")
        End Sub

        Private Sub OpenWebsiteButton_Click(sender As Object, e As RoutedEventArgs)
            AppServices.OpenPath("http://vexthatprotogen.com/")
        End Sub

        Private Sub ClearLogButton_Click(sender As Object, e As RoutedEventArgs)
            AppServices.Activity.Clear()
        End Sub

        Private Shared Function ParseVersion(value As String) As Version
            Dim match = Regex.Match(value, "\d+(?:\.\d+){1,3}")
            If Not match.Success Then Throw New FormatException($"'{value}' does not contain a valid version number.")
            Return Version.Parse(match.Value)
        End Function
    End Class
End Namespace
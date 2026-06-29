Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Security
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.RegularExpressions

Namespace Views
    Public Class AboutView

        Private Const ProductionBaseUrl As String = "https://rbxextr.vexthatprotogen.com/"

#If DEBUG Then
        ' Debug builds only: allow pointing the updater at a local/staging host for testing, e.g.
        '   set RBX_UPDATE_BASEURL=http://localhost:8080/
        ' Release builds ignore this entirely and always use the production host.
        Private ReadOnly Property BaseUrl As String
            Get
                Dim overrideUrl = Environment.GetEnvironmentVariable("RBX_UPDATE_BASEURL")
                Return If(String.IsNullOrWhiteSpace(overrideUrl), ProductionBaseUrl, overrideUrl.Trim())
            End Get
        End Property
#Else
        Private Const BaseUrl As String = ProductionBaseUrl
#End If

        Private ReadOnly client As New HttpClient With {.Timeout = TimeSpan.FromSeconds(20)}
        Private ReadOnly manifestJsonOptions As New JsonSerializerOptions With {.PropertyNameCaseInsensitive = True}

        ' Downloads and authenticates the signed release manifest. The signature is checked against the
        ' embedded public key, so the version and executable hash it carries can be trusted.
        Private Async Function FetchVerifiedManifestAsync() As Task(Of UpdateManifest)
            Dim manifestBytes = Await client.GetByteArrayAsync(BaseUrl & "update.json")
            Dim manifestSignature = (Await client.GetStringAsync(BaseUrl & "update.json.sig")).Trim()
            If Not UpdateVerifier.Verify(manifestBytes, manifestSignature) Then
                Throw New SecurityException("The update manifest failed signature verification.")
            End If
            Dim manifest = JsonSerializer.Deserialize(Of UpdateManifest)(Encoding.UTF8.GetString(manifestBytes), manifestJsonOptions)
            If manifest Is Nothing OrElse String.IsNullOrWhiteSpace(manifest.Version) OrElse String.IsNullOrWhiteSpace(manifest.Sha256) Then
                Throw New InvalidDataException("The update manifest is malformed.")
            End If
            Return manifest
        End Function

        Public Sub New()
            InitializeComponent()
            VersionText.Text = AppServices.CurrentVersion
            ActivityList.DataContext = AppServices.Activity
        End Sub

        Private Async Sub CheckUpdatesButton_Click(sender As Object, e As RoutedEventArgs)
            Await CheckForUpdatesAsync(promptToInstall:=False, owner:=Window.GetWindow(Me))
        End Sub

        Public Async Function CheckForUpdatesAsync(promptToInstall As Boolean, Optional owner As Window = Nothing) As Task(Of Boolean)
            CheckUpdatesButton.IsEnabled = False
            DownloadUpdateButton.Visibility = Visibility.Collapsed
            LatestVersionText.Text = "Checking..."
            UpdateStatusText.Text = "Contacting the update service..."
            AppServices.Report("Checking for updates...", 0, True)
            Try
                Dim manifest = Await FetchVerifiedManifestAsync()
                Dim latestText = manifest.Version
                Dim current = ParseVersion(AppServices.CurrentVersion)
                Dim latest = ParseVersion(latestText)
                LatestVersionText.Text = latestText
                If latest > current Then
                    DownloadUpdateButton.Visibility = Visibility.Visible
                    UpdateStatusText.Text = "A newer version is available and ready to download."
                    AppServices.Report($"Update available: {latestText} (installed {AppServices.CurrentVersion}).", 100)
                    If promptToInstall Then
                        Dim result = AppDialog.Show($"Version {latestText} is available. You are using {AppServices.CurrentVersion}.{vbCrLf}{vbCrLf}Download and install it now?",
                                                    "Update available", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes, owner)
                        If result = MessageBoxResult.Yes Then Return Await DownloadUpdateAsync(confirmFirst:=False, owner)
                    Else
                        AppDialog.Show($"Version {latestText} is available.", "Update available", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, owner)
                    End If
                ElseIf latest < current Then
                    UpdateStatusText.Text = "This development build is newer than the published release."
                    AppServices.Report($"This development build ({AppServices.CurrentVersion}) is newer than the published version ({latestText}).", 100)
                Else
                    UpdateStatusText.Text = "You are using the latest published version."
                    AppServices.Report($"You are using the latest version ({AppServices.CurrentVersion}).", 100)
                End If
            Catch ex As Exception
                LatestVersionText.Text = "Unavailable"
                UpdateStatusText.Text = "The update service could not be reached."
                AppServices.Report($"Update check failed: {ex.Message}")
                If Not promptToInstall Then AppDialog.Show(ex.Message, "Update check failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, owner)
            Finally
                CheckUpdatesButton.IsEnabled = True
            End Try
            Return False
        End Function
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
            Await DownloadUpdateAsync(confirmFirst:=True, owner:=Window.GetWindow(Me))
        End Sub

        Private Async Function DownloadUpdateAsync(confirmFirst As Boolean, Optional owner As Window = Nothing) As Task(Of Boolean)
            If confirmFirst AndAlso AppDialog.Show("Download the published executable and replace this application after it closes?", "Install update", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, owner) <> MessageBoxResult.Yes Then Return False
            UpdateStatusText.Text = "Downloading and preparing the update..."
            AppServices.Report("Downloading update...", 0, True)
            Try
                ' Authenticate the version + hash before downloading anything large.
                Dim manifest = Await FetchVerifiedManifestAsync()
                Dim current = ParseVersion(AppServices.CurrentVersion)
                Dim latest = ParseVersion(manifest.Version)

                ' Anti-rollback: refuse anything not strictly newer than what is installed. A signed
                ' manifest proves authenticity, not freshness, so this is what stops replay of an
                ' older legitimately signed build.
                If latest <= current Then
                    Throw New SecurityException($"Refusing update: offered version {manifest.Version} is not newer than the installed version {AppServices.CurrentVersion}. This can indicate a rollback attempt.")
                End If

                Dim payload = Await client.GetByteArrayAsync(BaseUrl & "RBXAssetExtractor.exe")

                ' Bind the download to the signed manifest: the bytes must hash to the signed value.
                Dim actualHash = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant()
                If Not String.Equals(actualHash, manifest.Sha256.Trim(), StringComparison.OrdinalIgnoreCase) Then
                    Throw New SecurityException("The downloaded executable does not match the signed manifest hash and was not installed.")
                End If

                Dim currentPath = Environment.ProcessPath
                If String.IsNullOrWhiteSpace(currentPath) Then Throw New InvalidOperationException("The current executable path is unavailable.")

                ' Stage into a freshly created, unpredictable directory so another local process can't
                ' pre-place or swap the staged executable before it is applied (TOCTOU).
                Dim token = Guid.NewGuid().ToString("N")
                Dim stagingDir = Path.Combine(Path.GetTempPath(), "RBXAssetExtractor-update-" & token)
                Directory.CreateDirectory(stagingDir)
                Dim tempPath = Path.Combine(stagingDir, "RBXAssetExtractor.exe")
                Await File.WriteAllBytesAsync(tempPath, payload)

                Dim batchPath = Path.Combine(Path.GetTempPath(), "RBXAssetExtractor-update-" & token & ".cmd")
                ' Replace the running executable once this process releases it. A single fixed delay
                ' races the old process's exit and fails silently if it loses, so retry the move until
                ' it succeeds (or a bounded number of attempts elapses) instead of guessing.
                Dim script = String.Join(vbCrLf, {
                    "@echo off",
                    "setlocal enabledelayedexpansion",
                    "set /a tries=0",
                    ":waitloop",
                    $"move /y ""{tempPath}"" ""{currentPath}"" >nul 2>&1",
                    $"if not exist ""{tempPath}"" goto done",
                    ">nul timeout /t 1 /nobreak",
                    "set /a tries+=1",
                    "if !tries! lss 60 goto waitloop",
                    ":done",
                    $"start """" ""{currentPath}""",
                    $"rmdir /s /q ""{stagingDir}"" >nul 2>&1",
                    "del ""%~f0"""})
                Await File.WriteAllTextAsync(batchPath, script)
                Process.Start(New ProcessStartInfo(batchPath) With {.UseShellExecute = True, .WindowStyle = ProcessWindowStyle.Hidden})
                Application.Current.Shutdown()
                Return True
            Catch ex As Exception
                UpdateStatusText.Text = "The update could not be installed."
                AppServices.Report($"Update failed: {ex.Message}")
                AppDialog.Show(ex.Message, "Update failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, owner)
                Return False
            End Try
        End Function

        Private Sub OpenProjectWebsiteButton_Click(sender As Object, e As RoutedEventArgs)
            AppServices.OpenPath(AppServices.ProjectWebsiteUrl)
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
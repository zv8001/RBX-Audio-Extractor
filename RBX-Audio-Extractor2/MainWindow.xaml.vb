Imports System.ComponentModel
Imports System.IO
Imports RBXAssetExtractor.Views

Class MainWindow
    Private overview As OverviewView
    Private audio As AudioView
    Private images As ImagesView
    Private meshes As MeshesView
    Private cacheFiles As CacheFilesView
    Private extras As ExtraAssetsView
    Private maintenance As MaintenanceView
    Private about As AboutView
    Private ready As Boolean
    Private creatorMessageShown As Boolean

    Public Sub New()
        InitializeComponent()
        VersionBadgeText.Text = AppServices.CurrentVersion
        overview = New OverviewView()
        audio = New AudioView()
        images = New ImagesView()
        meshes = New MeshesView()
        cacheFiles = New CacheFilesView()
        extras = New ExtraAssetsView()
        maintenance = New MaintenanceView()
        about = New AboutView()
        AddHandler overview.NavigationRequested, AddressOf NavigateTo
        AddHandler AppServices.StatusChanged, AddressOf StatusChanged
        AddHandler AppServices.CacheCleared, AddressOf CacheCleared
        SidebarCachePath.Text = AppServices.DatabasePath
        SidebarCacheState.Text = If(File.Exists(AppServices.DatabasePath), "Cache detected", "Cache not found")
        PageHost.Content = overview
        ready = True
        AppServices.AddLog("WPF workspace initialized.")
    End Sub

    Private Sub Navigation_Checked(sender As Object, e As RoutedEventArgs)
        If Not ready Then Return
        Dim selected = TryCast(sender, RadioButton)
        If selected Is Nothing Then Return
        Select Case selected.Name
            Case NameOf(OverviewNav) : ShowPage(overview, "Overview", "Explore and export assets directly from Roblox's local cache.")
            Case NameOf(AudioNav) : ShowPage(audio, "Audio", "Play and export OGG and MP3 assets without temporary cache dumps.")
            Case NameOf(ImagesNav) : ShowPage(images, "Images", "Preview and export cached PNG, JPEG, BMP, and WebP images.")
            Case NameOf(MeshesNav) : ShowPage(meshes, "Meshes", "Inspect Roblox mesh versions, preview geometry, and export OBJ files.")
            Case NameOf(CacheNav) : ShowPage(cacheFiles, "Cache files", "Extract RBXM models and KTX textures directly from the cache.")
            Case NameOf(ExtrasNav) : ShowPage(extras, "More assets", "Browse thumbnails, fonts, JSON, XML, and playlist metadata.")
            Case NameOf(MaintenanceNav) : ShowPage(maintenance, "Maintenance", "Inspect or deliberately clear Roblox's local asset cache.")
            Case NameOf(AboutNav) : ShowPage(about, "About & logs", "Updates, creator messages, project details, and activity history.")
        End Select
    End Sub

    Private Sub NavigateTo(target As String)
        Select Case target
            Case "Audio" : AudioNav.IsChecked = True
            Case "Images" : ImagesNav.IsChecked = True
            Case "Meshes" : MeshesNav.IsChecked = True
            Case "Cache" : CacheNav.IsChecked = True
            Case "Extras" : ExtrasNav.IsChecked = True
            Case "Maintenance" : MaintenanceNav.IsChecked = True
            Case "About" : AboutNav.IsChecked = True
            Case Else : OverviewNav.IsChecked = True
        End Select
    End Sub

    Private Sub ShowPage(view As Object, title As String, subtitle As String)
        PageHost.Content = view
        PageTitleText.Text = title
        PageSubtitleText.Text = subtitle
    End Sub

    Private Sub StatusChanged(message As String, progress As Double, indeterminate As Boolean)
        StatusText.Text = message
        WorkProgress.IsIndeterminate = indeterminate
        If Not indeterminate Then WorkProgress.Value = progress
        SidebarCacheState.Text = If(File.Exists(AppServices.DatabasePath), "Cache detected", "Cache not found")
    End Sub

    Private Sub CacheCleared()
        SidebarCacheState.Text = "Cache cleared"
    End Sub

    Private Sub TitleBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ChangedButton <> MouseButton.Left Then Return
        If e.ClickCount = 2 Then
            ToggleMaximize()
        Else
            DragMove()
        End If
    End Sub

    Private Sub MinimizeButton_Click(sender As Object, e As RoutedEventArgs)
        WindowState = WindowState.Minimized
    End Sub

    Private Sub MaximizeButton_Click(sender As Object, e As RoutedEventArgs)
        ToggleMaximize()
    End Sub

    Private Sub ToggleMaximize()
        WindowState = If(WindowState = WindowState.Maximized, WindowState.Normal, WindowState.Maximized)
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Private Sub AlwaysOnTopCheckBox_Changed(sender As Object, e As RoutedEventArgs)
        Topmost = AlwaysOnTopCheckBox.IsChecked.GetValueOrDefault()
    End Sub

    Private Async Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        If creatorMessageShown Then Return
        creatorMessageShown = True
        If Await about.CheckForUpdatesAsync(promptToInstall:=True, owner:=Me) Then Return
        Try
            Dim message = Await RemoteContentService.GetCreatorMessageAsync()
            about.SetCreatorMessage(message)
            If Not String.IsNullOrWhiteSpace(message) Then
                AppServices.AddLog("Creator message received.")
                AppDialog.Show(message, "Message from the Creator", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, Me)
            End If
        Catch ex As Exception
            about.SetCreatorMessage(String.Empty)
            AppServices.AddLog($"Creator message failed: {ex.Message}")
        End Try
    End Sub
    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs)
        If audio IsNot Nothing Then audio.StopPlayback()
        RemoveHandler AppServices.StatusChanged, AddressOf StatusChanged
        RemoveHandler AppServices.CacheCleared, AddressOf CacheCleared
    End Sub
End Class
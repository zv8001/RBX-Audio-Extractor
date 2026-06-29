Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Interop
Imports System.Windows.Media.Animation
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

    Private Const WmGetMinMaxInfo As Integer = &H24
    Private Const MonitorDefaultToNearest As Integer = 2

    Protected Overrides Sub OnSourceInitialized(e As EventArgs)
        MyBase.OnSourceInitialized(e)
        Dim source = HwndSource.FromHwnd(New WindowInteropHelper(Me).Handle)
        If source IsNot Nothing Then source.AddHook(AddressOf WindowProcedure)
    End Sub

    Private Function WindowProcedure(hwnd As IntPtr, message As Integer, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr
        If message <> WmGetMinMaxInfo Then Return IntPtr.Zero
        Dim monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest)
        If monitor = IntPtr.Zero Then Return IntPtr.Zero

        Dim monitorInfo As New MonitorInfo With {.Size = Marshal.SizeOf(Of MonitorInfo)()}
        If Not GetMonitorInfo(monitor, monitorInfo) Then Return IntPtr.Zero

        Dim minMaxInfo = Marshal.PtrToStructure(Of MinMaxInfo)(lParam)
        minMaxInfo.MaxPosition.X = Math.Abs(monitorInfo.WorkArea.Left - monitorInfo.MonitorArea.Left)
        minMaxInfo.MaxPosition.Y = Math.Abs(monitorInfo.WorkArea.Top - monitorInfo.MonitorArea.Top)
        minMaxInfo.MaxSize.X = monitorInfo.WorkArea.Right - monitorInfo.WorkArea.Left
        minMaxInfo.MaxSize.Y = monitorInfo.WorkArea.Bottom - monitorInfo.WorkArea.Top
        minMaxInfo.MaxTrackSize = minMaxInfo.MaxSize

        Dim dpi = VisualTreeHelper.GetDpi(Me)
        Dim requestedMinWidth = CInt(Math.Ceiling(MinWidth * dpi.DpiScaleX))
        Dim requestedMinHeight = CInt(Math.Ceiling(MinHeight * dpi.DpiScaleY))
        minMaxInfo.MinTrackSize.X = Math.Min(requestedMinWidth, minMaxInfo.MaxTrackSize.X)
        minMaxInfo.MinTrackSize.Y = Math.Min(requestedMinHeight, minMaxInfo.MaxTrackSize.Y)

        Marshal.StructureToPtr(minMaxInfo, lParam, fDeleteOld:=True)
        handled = True
        Return IntPtr.Zero
    End Function

    <DllImport("user32.dll")>
    Private Shared Function MonitorFromWindow(hwnd As IntPtr, flags As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function GetMonitorInfo(monitor As IntPtr, ByRef info As MonitorInfo) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativePoint
        Public X As Integer
        Public Y As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure MinMaxInfo
        Public Reserved As NativePoint
        Public MaxSize As NativePoint
        Public MaxPosition As NativePoint
        Public MinTrackSize As NativePoint
        Public MaxTrackSize As NativePoint
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativeRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
    Private Structure MonitorInfo
        Public Size As Integer
        Public MonitorArea As NativeRect
        Public WorkArea As NativeRect
        Public Flags As Integer
    End Structure
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
        AddHandler AppServices.ApplicationDataCleared, AddressOf ApplicationDataCleared
        SidebarCachePath.Text = AppServices.DatabasePath
        SidebarCacheState.Text = If(File.Exists(AppServices.DatabasePath), "Cache detected", "Cache not found")
        PageHost.Content = overview
        ready = True
        AppServices.AddLog("WPF workspace initialized.")
        RestoreSession()
    End Sub

    Private Sub RestoreSession()
        Try
            audio.RestoreState()
            images.RestoreState()
            meshes.RestoreState()
            cacheFiles.RestoreState()
            extras.RestoreState()
        Catch ex As Exception
            AppServices.AddLog($"Could not restore previous session: {ex.Message}")
        End Try
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
        If ReferenceEquals(PageHost.Content, view) Then Return
        PageHost.Content = view
        PageTitleText.Text = title
        PageSubtitleText.Text = subtitle
        AnimatePageTransition()
    End Sub

    Private Sub AnimatePageTransition()
        Dim translate = TryCast(PageSurface.RenderTransform, TranslateTransform)
        If translate Is Nothing Then
            translate = New TranslateTransform()
            PageSurface.RenderTransform = translate
        End If

        PageSurface.BeginAnimation(UIElement.OpacityProperty, Nothing)
        translate.BeginAnimation(TranslateTransform.YProperty, Nothing)
        PageSurface.Opacity = 0.3
        translate.Y = 9

        Dim duration = New Duration(TimeSpan.FromMilliseconds(220))
        Dim easing As New CubicEase With {.EasingMode = EasingMode.EaseOut}
        Dim fade As New DoubleAnimation(1.0, duration) With {.EasingFunction = easing}
        Dim slide As New DoubleAnimation(0.0, duration) With {.EasingFunction = easing}

        PageSurface.BeginAnimation(UIElement.OpacityProperty, fade, HandoffBehavior.SnapshotAndReplace)
        translate.BeginAnimation(TranslateTransform.YProperty, slide, HandoffBehavior.SnapshotAndReplace)
    End Sub

    Private Sub StatusChanged(message As String, progress As Double, indeterminate As Boolean)
        StatusText.Text = message
        WorkProgress.IsIndeterminate = indeterminate
        If Not indeterminate Then WorkProgress.Value = progress
        SidebarCachePath.Text = AppServices.DatabasePath
        SidebarCacheState.Text = If(File.Exists(AppServices.DatabasePath), "Cache detected", "Cache not found")
    End Sub

    Private Sub ApplicationDataCleared()
        audio.ClearSavedNames()
        images.ClearSavedNames()
        meshes.ClearSavedNames()
        cacheFiles.ClearSavedNames()
        extras.ClearSavedNames()
    End Sub

    Private Sub CacheCleared()
        SidebarCacheState.Text = "Cache cleared"
        ' The persisted scan results now point at a deleted cache; drop them so a restart starts clean.
        SessionStateStore.ClearAll()
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

    Private scanningAll As Boolean

    Private Async Sub ScanAllButton_Click(sender As Object, e As RoutedEventArgs)
        If scanningAll Then Return
        scanningAll = True
        ScanAllButton.IsEnabled = False
        Dim originalContent = ScanAllButton.Content
        ScanAllButton.Content = "Scanning..."
        Try
            AppServices.Report("Scanning every workspace...", 0, True)
            Await audio.StartScanAsync()
            Await images.StartScanAsync()
            Await meshes.StartScanAsync()
            Await cacheFiles.StartScanAsync()
            Await extras.ScanAllAsync()
            AppServices.Report("Scan all complete. Every workspace is populated.", 100)
        Catch ex As Exception
            AppServices.Report($"Scan all failed: {ex.Message}")
        Finally
            ScanAllButton.Content = originalContent
            ScanAllButton.IsEnabled = True
            scanningAll = False
        End Try
    End Sub

    Private Async Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs)
        FitToWorkingArea()
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
    Private Sub FitToWorkingArea()
        Dim workingArea = SystemParameters.WorkArea
        Const edgeAllowance As Double = 32
        Width = Math.Max(MinWidth, Math.Min(Width, workingArea.Width - edgeAllowance))
        Height = Math.Max(MinHeight, Math.Min(Height, workingArea.Height - edgeAllowance))
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs)
        If audio IsNot Nothing Then audio.StopPlayback()
        RemoveHandler AppServices.StatusChanged, AddressOf StatusChanged
        RemoveHandler AppServices.CacheCleared, AddressOf CacheCleared
    End Sub
End Class
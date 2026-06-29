Namespace Views
    Public Class OverviewView
        Public Event NavigationRequested(target As String)

        Public Sub New()
            InitializeComponent()
            AddHandler AppServices.AssetCountChanged, AddressOf AssetCountChanged
        End Sub

        Private Sub AssetCountChanged(category As String, count As Integer)
            Select Case category
                Case "Audio" : AudioCountText.Text = count.ToString("N0")
                Case "Images" : ImageCountText.Text = count.ToString("N0")
                Case "Meshes" : MeshCountText.Text = count.ToString("N0")
                Case "Cache" : CacheCountText.Text = count.ToString("N0")
            End Select
        End Sub

        Private Sub BrowseAudio_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent NavigationRequested("Audio")
        End Sub

        Private Sub OpenProjectWebsite_Click(sender As Object, e As RoutedEventArgs)
            AppServices.OpenPath(AppServices.ProjectWebsiteUrl)
        End Sub

        Private Sub Navigate_Click(sender As Object, e As RoutedEventArgs)
            Dim button = TryCast(sender, Button)
            If button IsNot Nothing Then RaiseEvent NavigationRequested(CStr(button.Tag))
        End Sub
    End Class
End Namespace
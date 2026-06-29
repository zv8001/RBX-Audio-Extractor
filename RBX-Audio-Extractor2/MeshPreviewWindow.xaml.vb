Imports System.Numerics
Imports System.Windows.Media.Media3D

Public Class MeshPreviewWindow
    Private ReadOnly yawRotation As New AxisAngleRotation3D(New Vector3D(0, 1, 0), -32)
    Private ReadOnly pitchRotation As New AxisAngleRotation3D(New Vector3D(1, 0, 0), 20)
    Private dragging As Boolean
    Private previousMouse As Point
    Private cameraDistance As Double = 3.4

    Public Sub New(entry As MeshCacheEntry, data As MeshPreviewData)
        InitializeComponent()
        Dim separator = ChrW(&HB7)
        Dim displayName = entry.FriendlyName
        Title = $"Mesh Preview {separator} {AppServices.SafePrefix(displayName)}"
        WindowTitleText.Text = $"Mesh preview {separator} {AppServices.SafePrefix(displayName, 24)}"
        TitleText.Text = $"{displayName}  {separator}  mesh v{entry.Version}"
        DetailsText.Text = $"{data.Positions.Length:N0} vertices  {ChrW(&H2022)}  {data.Indices.Length \ 3:N0} triangles"
        BuildScene(data)
    End Sub

    Private Sub BuildScene(data As MeshPreviewData)
        Dim geometry As New MeshGeometry3D()
        Dim normalized = Normalize(data.Positions)
        For Each vertex In normalized
            geometry.Positions.Add(New Point3D(vertex.X, vertex.Y, vertex.Z))
        Next
        If data.Normals IsNot Nothing AndAlso data.Normals.Length = normalized.Length Then
            For Each sourceNormal In data.Normals
                Dim normal = sourceNormal
                If normal.LengthSquared() > 0.000001F Then normal = Vector3.Normalize(normal)
                geometry.Normals.Add(New Vector3D(normal.X, normal.Y, normal.Z))
            Next
        End If
        For Each index In data.Indices
            If index >= 0 AndAlso index < normalized.Length Then geometry.TriangleIndices.Add(index)
        Next
        geometry.Freeze()

        Dim material As New MaterialGroup()
        material.Children.Add(New DiffuseMaterial(New SolidColorBrush(Color.FromRgb(&H72, &H8B, &HFF))))
        material.Children.Add(New SpecularMaterial(New SolidColorBrush(Color.FromArgb(&H66, &HE5, &HE7, &HFF)), 28))
        material.Freeze()

        Dim model As New GeometryModel3D(geometry, material) With {.BackMaterial = material}
        Dim transforms As New Transform3DGroup()
        transforms.Children.Add(New RotateTransform3D(yawRotation))
        transforms.Children.Add(New RotateTransform3D(pitchRotation))
        model.Transform = transforms

        Dim group As New Model3DGroup()
        group.Children.Add(New AmbientLight(Color.FromRgb(&H3D, &H41, &H52)))
        group.Children.Add(New DirectionalLight(Colors.White, New Vector3D(-0.35, -0.5, -1)))
        group.Children.Add(New DirectionalLight(Color.FromRgb(&H79, &H62, &HFF), New Vector3D(0.8, 0.2, 1)))
        group.Children.Add(model)
        Viewport.Children.Add(New ModelVisual3D With {.Content = group})
    End Sub

    Private Shared Function Normalize(source As Vector3()) As Vector3()
        If source Is Nothing OrElse source.Length = 0 Then Return Array.Empty(Of Vector3)()
        Dim minimum = source(0)
        Dim maximum = source(0)
        For index = 1 To source.Length - 1
            minimum = Vector3.Min(minimum, source(index))
            maximum = Vector3.Max(maximum, source(index))
        Next
        Dim center = (minimum + maximum) * 0.5F
        Dim size = maximum - minimum
        Dim extent = Math.Max(size.X, Math.Max(size.Y, size.Z)) * 0.5F
        If extent <= 0.000001F OrElse Single.IsNaN(extent) OrElse Single.IsInfinity(extent) Then extent = 1
        Return source.Select(Function(vertex) (vertex - center) / extent).ToArray()
    End Function

    Private Sub Viewport_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ClickCount = 2 Then
            ResetView()
            e.Handled = True
            Return
        End If
        dragging = True
        previousMouse = e.GetPosition(Viewport)
        Viewport.CaptureMouse()
    End Sub

    Private Sub Viewport_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        dragging = False
        Viewport.ReleaseMouseCapture()
    End Sub

    Private Sub Viewport_MouseMove(sender As Object, e As MouseEventArgs)
        If Not dragging Then Return
        Dim current = e.GetPosition(Viewport)
        yawRotation.Angle += (current.X - previousMouse.X) * 0.45
        pitchRotation.Angle += (current.Y - previousMouse.Y) * 0.45
        previousMouse = current
    End Sub

    Private Sub Viewport_MouseWheel(sender As Object, e As MouseWheelEventArgs)
        cameraDistance *= If(e.Delta > 0, 0.88, 1.14)
        cameraDistance = Math.Max(1.6, Math.Min(15, cameraDistance))
        UpdateCamera()
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

    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Escape Then Close()
    End Sub

    Private Sub ResetButton_Click(sender As Object, e As RoutedEventArgs)
        ResetView()
    End Sub

    Private Sub ResetView()
        yawRotation.Angle = -32
        pitchRotation.Angle = 20
        cameraDistance = 3.4
        UpdateCamera()
    End Sub

    Private Sub UpdateCamera()
        Camera.Position = New Point3D(0, 0, cameraDistance)
        Camera.LookDirection = New Vector3D(0, 0, -cameraDistance)
    End Sub
End Class
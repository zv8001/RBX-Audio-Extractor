Imports System.Drawing.Drawing2D
Imports System.Numerics

Public NotInheritable Class MeshPreviewForm
    Inherits Form

    Private ReadOnly viewport As MeshViewport

    Public Sub New(entry As MeshCacheEntry, data As MeshPreviewData)
        Text = $"Mesh Preview - {entry.Hash.Substring(0, 12)}"
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(900, 650)
        MinimumSize = New Size(560, 420)
        BackColor = Color.FromArgb(20, 20, 20)
        ForeColor = Color.White
        ShowIcon = False

        Dim header As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 54,
            .BackColor = Color.FromArgb(27, 30, 36)
        }
        Dim title As New Label With {
            .AutoSize = False,
            .Location = New Point(14, 8),
            .Size = New Size(700, 20),
            .Font = New Font("Segoe UI Semibold", 11.0F),
            .ForeColor = Color.White,
            .Text = $"{entry.Hash}  |  mesh v{entry.Version}"
        }
        Dim details As New Label With {
            .AutoSize = False,
            .Location = New Point(14, 29),
            .Size = New Size(700, 18),
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.Silver,
            .Text = $"{data.Positions.Length:N0} vertices   •   {data.Indices.Length \ 3:N0} triangles"
        }
        Dim reset As New Button With {
            .Text = "Reset View",
            .Dock = DockStyle.Right,
            .Width = 112,
            .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.FromArgb(37, 45, 60),
            .ForeColor = Color.White
        }
        header.Controls.Add(title)
        header.Controls.Add(details)
        header.Controls.Add(reset)

        Dim footer As New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 30,
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.FromArgb(27, 30, 36),
            .ForeColor = Color.Silver,
            .Text = "Drag to rotate  •  Mouse wheel to zoom  •  Double-click the preview to reset"
        }

        viewport = New MeshViewport(data) With {.Dock = DockStyle.Fill}
        AddHandler reset.Click, Sub() viewport.ResetView()
        Controls.Add(viewport)
        Controls.Add(footer)
        Controls.Add(header)
    End Sub
End Class

Friend NotInheritable Class MeshViewport
    Inherits Control

    Private Structure RenderTriangle
        Public A As PointF
        Public B As PointF
        Public C As PointF
        Public Depth As Single
        Public Shade As Single
    End Structure

    Private ReadOnly vertices As Vector3()
    Private ReadOnly indices As Integer()
    Private dragging As Boolean
    Private previousMouse As Point
    Private yaw As Single = -0.55F
    Private pitch As Single = 0.35F
    Private zoom As Single = 1.0F

    Public Sub New(data As MeshPreviewData)
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        BackColor = Color.FromArgb(14, 16, 20)
        TabStop = True
        indices = If(data.Indices, Array.Empty(Of Integer)())
        vertices = NormalizeVertices(If(data.Positions, Array.Empty(Of Vector3)()))
    End Sub

    Public Sub ResetView()
        yaw = -0.55F
        pitch = 0.35F
        zoom = 1.0F
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        e.Graphics.Clear(BackColor)
        If vertices.Length = 0 OrElse indices.Length < 3 Then
            Using font As New Font("Segoe UI", 12.0F)
                TextRenderer.DrawText(e.Graphics, "This mesh has no previewable triangles.", font, ClientRectangle, Color.Silver, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
            End Using
            Return
        End If

        Dim transformed(vertices.Length - 1) As Vector3
        Dim projected(vertices.Length - 1) As PointF
        Dim cosY = CSng(Math.Cos(yaw))
        Dim sinY = CSng(Math.Sin(yaw))
        Dim cosX = CSng(Math.Cos(pitch))
        Dim sinX = CSng(Math.Sin(pitch))
        Dim centerX = ClientSize.Width / 2.0F
        Dim centerY = ClientSize.Height / 2.0F
        Dim screenScale = Math.Min(ClientSize.Width, ClientSize.Height) * 1.25F * zoom

        For i = 0 To vertices.Length - 1
            Dim source = vertices(i)
            Dim x = source.X * cosY + source.Z * sinY
            Dim z = -source.X * sinY + source.Z * cosY
            Dim y = source.Y * cosX - z * sinX
            z = source.Y * sinX + z * cosX
            transformed(i) = New Vector3(x, y, z)
            Dim distance = Math.Max(1.25F, 4.0F - z)
            Dim factor = screenScale / distance
            projected(i) = New PointF(centerX + x * factor, centerY - y * factor)
        Next

        Dim faceCount = indices.Length \ 3
        Dim stride = Math.Max(1, CInt(Math.Ceiling(faceCount / 40000.0R)))
        Dim triangles As New List(Of RenderTriangle)(Math.Min(faceCount, 40000))
        Dim light = Vector3.Normalize(New Vector3(-0.35F, 0.45F, 1.0F))
        For face = 0 To faceCount - 1 Step stride
            Dim offset = face * 3
            Dim ia = indices(offset)
            Dim ib = indices(offset + 1)
            Dim ic = indices(offset + 2)
            If ia < 0 OrElse ib < 0 OrElse ic < 0 OrElse ia >= vertices.Length OrElse ib >= vertices.Length OrElse ic >= vertices.Length Then Continue For
            Dim normal = Vector3.Cross(transformed(ib) - transformed(ia), transformed(ic) - transformed(ia))
            If normal.LengthSquared() < 0.0000001F Then Continue For
            normal = Vector3.Normalize(normal)
            Dim shade = 0.28F + 0.72F * Math.Abs(Vector3.Dot(normal, light))
            triangles.Add(New RenderTriangle With {
                .A = projected(ia),
                .B = projected(ib),
                .C = projected(ic),
                .Depth = (transformed(ia).Z + transformed(ib).Z + transformed(ic).Z) / 3.0F,
                .Shade = shade
            })
        Next
        triangles.Sort(Function(left, right) left.Depth.CompareTo(right.Depth))

        Dim drawEdges = triangles.Count <= 15000
        Dim shadeBrushes(31) As SolidBrush
        For i = 0 To shadeBrushes.Length - 1
            Dim shade = 0.28F + 0.72F * (i / CSng(shadeBrushes.Length - 1))
            shadeBrushes(i) = New SolidBrush(Color.FromArgb(CInt(60 * shade), CInt(145 * shade), CInt(225 * shade)))
        Next
        Try
            Using edgePen As New Pen(Color.FromArgb(70, 8, 15, 24), 1.0F)
                For Each triangle In triangles
                    Dim brushIndex = Math.Max(0, Math.Min(shadeBrushes.Length - 1, CInt((triangle.Shade - 0.28F) / 0.72F * (shadeBrushes.Length - 1))))
                    Dim points = {triangle.A, triangle.B, triangle.C}
                    e.Graphics.FillPolygon(shadeBrushes(brushIndex), points)
                    If drawEdges Then e.Graphics.DrawPolygon(edgePen, points)
                Next
            End Using
        Finally
            For Each brush In shadeBrushes
                brush.Dispose()
            Next
        End Try

        If stride > 1 Then
            Using font As New Font("Segoe UI", 8.5F)
                e.Graphics.DrawString($"Large mesh preview: showing every {stride}th triangle", font, Brushes.Silver, 10.0F, 10.0F)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            dragging = True
            previousMouse = e.Location
            Capture = True
            Focus()
        End If
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If Not dragging Then Return
        yaw += (e.X - previousMouse.X) * 0.01F
        pitch += (e.Y - previousMouse.Y) * 0.01F
        pitch = Math.Max(-1.5F, Math.Min(1.5F, pitch))
        previousMouse = e.Location
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            dragging = False
            Capture = False
        End If
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        zoom *= If(e.Delta > 0, 1.12F, 0.89F)
        zoom = Math.Max(0.25F, Math.Min(5.0F, zoom))
        Invalidate()
    End Sub

    Protected Overrides Sub OnDoubleClick(e As EventArgs)
        MyBase.OnDoubleClick(e)
        ResetView()
    End Sub

    Private Shared Function NormalizeVertices(source As Vector3()) As Vector3()
        If source.Length = 0 Then Return Array.Empty(Of Vector3)()
        Dim minimum = source(0)
        Dim maximum = source(0)
        For i = 1 To source.Length - 1
            minimum = Vector3.Min(minimum, source(i))
            maximum = Vector3.Max(maximum, source(i))
        Next
        Dim center = (minimum + maximum) * 0.5F
        Dim size = maximum - minimum
        Dim extent = Math.Max(size.X, Math.Max(size.Y, size.Z)) * 0.5F
        If extent <= 0.000001F OrElse Single.IsNaN(extent) OrElse Single.IsInfinity(extent) Then extent = 1.0F
        Dim result(source.Length - 1) As Vector3
        For i = 0 To source.Length - 1
            result(i) = (source(i) - center) / extent
        Next
        Return result
    End Function
End Class
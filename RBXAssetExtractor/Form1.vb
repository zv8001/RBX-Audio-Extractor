
Imports System.ComponentModel
Imports System.Data.SQLite
Imports System.Diagnostics.CodeAnalysis
Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Numerics
Imports System.Reflection
Imports System.Reflection.Metadata
Imports System.Security.Policy
Imports System.Security.Principal
Imports System.Text
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports ATL
Imports ATL.AudioData
Imports Microsoft.Win32
Imports NAudio.Vorbis
Imports NAudio.Wave
Imports ImageMagick
'MIT License

'Copyright (c) 2025 zv8001

'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:

'The above copyright notice and this permission notice shall be included in all
'copies or substantial portions of the Software.

'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
''FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
'SOFTWARE.

'https://github.com/zv8001/RBX-Audio-Extractor?tab=MIT-1-ov-file#readme

Imports NVorbis
Imports TagLib


Public Class MainForm

    Dim DisableFade2 As Boolean = False
    Dim DisableFade As Boolean = False
    Dim V = "2.0.0 OVERHAUL"
    Private WithEvents backgroundWorker As New BackgroundWorker()
    Dim Stage As Integer = 0
    Dim tempDirectory = Path.GetTempPath
    Dim Playing As Boolean = False
    Dim SelFile As String
    Dim Outdated As Boolean = False
    Private isDragging As Boolean = False
    Private startPoint As Point
    Dim HTTPDONE = True

    Private meshEntries As New List(Of MeshCacheEntry)()

    Private cacheAssetEntries As New List(Of RobloxCacheAssetEntry)()

    Private fullAudioEntries As New List(Of RobloxCacheAssetEntry)()
    Private fullImageEntries As New List(Of RobloxCacheAssetEntry)()
    Private selectedFullAudioEntry As RobloxCacheAssetEntry
    Private selectedFullImageEntry As RobloxCacheAssetEntry
    Private imagePreviewLoadVersion As Integer
    Private thumbnailEntries As New List(Of SupplementalCacheEntry)()
    Private fontEntries As New List(Of SupplementalCacheEntry)()
    Private metadataEntries As New List(Of SupplementalCacheEntry)()
    Private thumbnailPreviewLoadVersion As Integer

    Private Async Sub ThumbnailScanButton_Click(sender As Object, e As EventArgs) Handles thumbnailScanButton.Click
        SetSupplementalBusy(thumbnailScanButton, thumbnailExportButton, thumbnailExportAllButton, thumbnailList, True)
        thumbnailEntries.Clear()
        thumbnailList.Items.Clear()
        thumbnailStatus.Text = "Scanning the embedded Roblox thumbnail cache..."
        thumbnailProgress.Value = 0
        Try
            thumbnailEntries = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ScanThumbnails(
                Sub(value)
                    Me.BeginInvoke(New Action(Sub()
                                                  If value.Total > 0 Then thumbnailProgress.Value = Math.Min(100, CInt(CLng(value.Current) * 100L \ value.Total))
                                                  thumbnailStatus.Text = $"Scanning thumbnails... {value.Found:N0} found"
                                              End Sub))
                End Sub))
            thumbnailList.Items.AddRange(thumbnailEntries.Cast(Of Object)().ToArray())
            thumbnailStatus.Text = $"Found {thumbnailEntries.Count:N0} cached avatar, headshot, and thumbnail images."
        Catch ex As Exception
            thumbnailStatus.Text = "Thumbnail scan failed."
            CallError(ex.Message)
        Finally
            SetSupplementalBusy(thumbnailScanButton, thumbnailExportButton, thumbnailExportAllButton, thumbnailList, False, thumbnailEntries.Count)
            thumbnailProgress.Value = 0
        End Try
    End Sub

    Private Async Sub FontScanButton_Click(sender As Object, e As EventArgs) Handles fontScanButton.Click
        SetSupplementalBusy(fontScanButton, fontExportButton, fontExportAllButton, fontList, True)
        fontEntries.Clear()
        fontList.Items.Clear()
        fontStatus.Text = "Scanning the Roblox cache for fonts..."
        fontProgress.Value = 0
        Try
            fontEntries = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ScanFonts(
                Sub(value)
                    Me.BeginInvoke(New Action(Sub()
                                                  If value.Total > 0 Then fontProgress.Value = Math.Min(100, CInt(CLng(value.Current) * 100L \ value.Total))
                                                  fontStatus.Text = $"Scanning fonts... {value.Found:N0} found"
                                              End Sub))
                End Sub))
            fontList.Items.AddRange(fontEntries.Cast(Of Object)().ToArray())
            fontStatus.Text = $"Found {fontEntries.Count:N0} cached TTF and OTF files."
        Catch ex As Exception
            fontStatus.Text = "Font scan failed."
            CallError(ex.Message)
        Finally
            SetSupplementalBusy(fontScanButton, fontExportButton, fontExportAllButton, fontList, False, fontEntries.Count)
            fontProgress.Value = 0
        End Try
    End Sub

    Private Async Sub MetadataScanButton_Click(sender As Object, e As EventArgs) Handles metadataScanButton.Click
        SetSupplementalBusy(metadataScanButton, metadataExportButton, metadataExportAllButton, metadataList, True)
        metadataEntries.Clear()
        metadataList.Items.Clear()
        metadataPreview.Clear()
        metadataStatus.Text = "Scanning the Roblox cache for JSON, XML, and playlists..."
        metadataProgress.Value = 0
        Try
            metadataEntries = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ScanMetadata(
                Sub(value)
                    Me.BeginInvoke(New Action(Sub()
                                                  If value.Total > 0 Then metadataProgress.Value = Math.Min(100, CInt(CLng(value.Current) * 100L \ value.Total))
                                                  metadataStatus.Text = $"Scanning metadata... {value.Found:N0} found"
                                              End Sub))
                End Sub))
            metadataList.Items.AddRange(metadataEntries.Cast(Of Object)().ToArray())
            metadataStatus.Text = $"Found {metadataEntries.Count:N0} JSON, XML, and HLS metadata files."
        Catch ex As Exception
            metadataStatus.Text = "Metadata scan failed."
            CallError(ex.Message)
        Finally
            SetSupplementalBusy(metadataScanButton, metadataExportButton, metadataExportAllButton, metadataList, False, metadataEntries.Count)
            metadataProgress.Value = 0
        End Try
    End Sub

    Private Sub SetSupplementalBusy(scanButton As System.Windows.Forms.Button, exportButton As System.Windows.Forms.Button, exportAllButton As System.Windows.Forms.Button, list As ListBox, busy As Boolean, Optional itemCount As Integer = 0)
        scanButton.Enabled = Not busy
        list.Enabled = Not busy
        exportButton.Enabled = Not busy AndAlso list.SelectedItem IsNot Nothing
        exportAllButton.Enabled = Not busy AndAlso itemCount > 0
    End Sub

    Private Async Sub ThumbnailList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles thumbnailList.SelectedIndexChanged
        Dim entry = TryCast(thumbnailList.SelectedItem, SupplementalCacheEntry)
        thumbnailExportButton.Enabled = entry IsNot Nothing AndAlso thumbnailScanButton.Enabled
        If entry Is Nothing Then Return
        thumbnailPreviewLoadVersion += 1
        Dim version = thumbnailPreviewLoadVersion
        thumbnailStatus.Text = $"Loading {entry.TypeLabel}..."
        Try
            Dim image = Await Task.Run(Function() LoadImageFromPayload(RobloxSupplementalCacheExtractor.ReadPayload(entry)))
            If version <> thumbnailPreviewLoadVersion OrElse Not Object.ReferenceEquals(thumbnailList.SelectedItem, entry) Then
                image.Dispose()
                Return
            End If
            Dim oldImage = thumbnailPreview.Image
            thumbnailPreview.Image = image
            If oldImage IsNot Nothing Then oldImage.Dispose()
            thumbnailStatus.Text = $"Previewing {entry.DisplayName} ({image.Width} x {image.Height})."
        Catch ex As Exception
            If version = thumbnailPreviewLoadVersion Then
                thumbnailStatus.Text = "Thumbnail preview failed."
                CallError(ex.Message)
            End If
        End Try
    End Sub

    Private Sub FontList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles fontList.SelectedIndexChanged
        fontExportButton.Enabled = fontList.SelectedItem IsNot Nothing AndAlso fontScanButton.Enabled
    End Sub

    Private Async Sub MetadataList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles metadataList.SelectedIndexChanged
        Dim entry = TryCast(metadataList.SelectedItem, SupplementalCacheEntry)
        metadataExportButton.Enabled = entry IsNot Nothing AndAlso metadataScanButton.Enabled
        If entry Is Nothing Then Return
        Try
            Dim payload = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ReadPayload(entry))
            metadataPreview.Text = Encoding.UTF8.GetString(payload)
            metadataStatus.Text = $"Previewing {entry.TypeLabel} directly from the Roblox cache."
        Catch ex As Exception
            metadataPreview.Text = String.Empty
            metadataStatus.Text = "Metadata preview failed."
            CallError(ex.Message)
        End Try
    End Sub

    Private Async Function ExportSupplementalSelected(entry As SupplementalCacheEntry, status As Label) As Task
        If entry Is Nothing Then Return
        Using dialog As New SaveFileDialog With {
            .Filter = $"{entry.TypeLabel} (*{entry.Extension})|*{entry.Extension}|All files (*.*)|*.*",
            .DefaultExt = entry.Extension.TrimStart("."c),
            .AddExtension = True,
            .FileName = entry.ExportBaseName & entry.Extension
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            status.Text = $"Exporting {entry.TypeLabel}..."
            Try
                Await Task.Run(Sub() RobloxSupplementalCacheExtractor.Export(entry, dialog.FileName))
                status.Text = $"Exported {Path.GetFileName(dialog.FileName)}"
            Catch ex As Exception
                status.Text = "Export failed."
                CallError(ex.Message)
            End Try
        End Using
    End Function

    Private Async Function ExportSupplementalAll(entries As List(Of SupplementalCacheEntry), status As Label, description As String) As Task
        If entries.Count = 0 Then Return
        Using dialog As New FolderBrowserDialog With {.Description = description, .ShowNewFolderButton = True}
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            status.Text = $"Exporting {entries.Count:N0} files..."
            Try
                Dim summary = Await Task.Run(Function() RobloxSupplementalCacheExtractor.ExportMany(entries, dialog.SelectedPath,
                    Sub(current, total)
                        If current Mod 25 = 0 OrElse current = total Then Me.BeginInvoke(New Action(Sub() status.Text = $"Exporting {current:N0} of {total:N0}..."))
                    End Sub))
                status.Text = $"Exported {summary.Exported:N0}; reused {summary.Reused:N0}; failed {summary.Failed:N0}."
            Catch ex As Exception
                status.Text = "Bulk export failed."
                CallError(ex.Message)
            End Try
        End Using
    End Function

    Private Async Sub ThumbnailExportButton_Click(sender As Object, e As EventArgs) Handles thumbnailExportButton.Click
        Await ExportSupplementalSelected(TryCast(thumbnailList.SelectedItem, SupplementalCacheEntry), thumbnailStatus)
    End Sub

    Private Async Sub ThumbnailExportAllButton_Click(sender As Object, e As EventArgs) Handles thumbnailExportAllButton.Click
        Await ExportSupplementalAll(thumbnailEntries, thumbnailStatus, "Choose a folder for cached Roblox thumbnails")
    End Sub

    Private Async Sub FontExportButton_Click(sender As Object, e As EventArgs) Handles fontExportButton.Click
        Await ExportSupplementalSelected(TryCast(fontList.SelectedItem, SupplementalCacheEntry), fontStatus)
    End Sub

    Private Async Sub FontExportAllButton_Click(sender As Object, e As EventArgs) Handles fontExportAllButton.Click
        Await ExportSupplementalAll(fontEntries, fontStatus, "Choose a folder for cached Roblox fonts")
    End Sub

    Private Async Sub MetadataExportButton_Click(sender As Object, e As EventArgs) Handles metadataExportButton.Click
        Await ExportSupplementalSelected(TryCast(metadataList.SelectedItem, SupplementalCacheEntry), metadataStatus)
    End Sub

    Private Async Sub MetadataExportAllButton_Click(sender As Object, e As EventArgs) Handles metadataExportAllButton.Click
        Await ExportSupplementalAll(metadataEntries, metadataStatus, "Choose a folder for cached Roblox metadata")
    End Sub
    Private Function CheckForRoblox()
        If My.Computer.FileSystem.DirectoryExists($"{tempDirectory}\Roblox") Then
            Return True
        Else
            CallError("Roblox tempDirectory not found")
            Return False
        End If

    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            If Not My.Computer.FileSystem.FileExists($"{Application.StartupPath}\sni.dll") Then
                My.Computer.Network.DownloadFile("https://rbxaudioextractor-update-server.netlify.app/SQL%20dependencies/sni.dll", $"{Application.StartupPath}\sni.dll")

            End If

            If Not My.Computer.FileSystem.FileExists($"{Application.StartupPath}\SQLite.Interop.dll") Then
                My.Computer.Network.DownloadFile("https://rbxaudioextractor-update-server.netlify.app/SQL%20dependencies/SQLite.Interop.dll", $"{Application.StartupPath}\SQLite.Interop.dll")
            End If

            If Not My.Computer.FileSystem.FileExists($"{Application.StartupPath}\Magick.Native-Q8-x64.dll") Then
                My.Computer.Network.DownloadFile("https://rbxaudioextractor-update-server.netlify.app/SQL%20dependencies/Magick.Native-Q8-x64.dll", $"{Application.StartupPath}\Magick.Native-Q8-x64.dll")
            End If
        Catch ex As Exception
            FailedError.ShowDialog()
            End
        End Try


        TaskLBR.Visible = False
        LoadHTTP0.WorkerReportsProgress = True
        VText_LBR.Text = V
        CheckForUpdates()
        Me.Opacity = 0
        fadeInTimer.Interval = 50
        fadeInTimer.Start()

        MSGPopup.RunWorkerAsync()

    End Sub


    Private Sub CacheAssetFilterChanged(sender As Object, e As EventArgs) Handles cacheAssetFilter.SelectedIndexChanged
        RefreshCacheAssetList()
    End Sub

    Private Sub CacheAssetList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cacheAssetList.SelectedIndexChanged
        cacheAssetExportButton.Enabled = cacheAssetList.SelectedItem IsNot Nothing AndAlso cacheAssetScanButton.Enabled
    End Sub
    Private Sub RefreshCacheAssetList()
        If cacheAssetList Is Nothing Then Return
        Dim selectedFilter = If(cacheAssetFilter Is Nothing, 0, cacheAssetFilter.SelectedIndex)
        Dim visible = cacheAssetEntries.Where(
            Function(entry)
                If selectedFilter = 1 Then Return entry.FileType = RobloxCacheFileType.RbxmBinary OrElse entry.FileType = RobloxCacheFileType.RbxmXml
                If selectedFilter = 2 Then Return entry.FileType = RobloxCacheFileType.Ktx1 OrElse entry.FileType = RobloxCacheFileType.Ktx2
                Return True
            End Function).Cast(Of Object)().ToArray()
        cacheAssetList.BeginUpdate()
        Try
            cacheAssetList.Items.Clear()
            cacheAssetList.Items.AddRange(visible)
        Finally
            cacheAssetList.EndUpdate()
        End Try
        cacheAssetExportButton.Enabled = False
    End Sub

    Private Sub SetCacheAssetBusy(busy As Boolean)
        cacheAssetScanButton.Enabled = Not busy
        cacheAssetFilter.Enabled = Not busy
        cacheAssetList.Enabled = Not busy
        cacheAssetExportButton.Enabled = Not busy AndAlso cacheAssetList.SelectedItem IsNot Nothing
        cacheAssetExportAllButton.Enabled = Not busy AndAlso cacheAssetEntries.Count > 0
        If Not busy Then cacheAssetProgress.Value = 0
    End Sub

    Private Async Sub CacheAssetScanButton_Click(sender As Object, e As EventArgs) Handles cacheAssetScanButton.Click
        SetCacheAssetBusy(True)
        cacheAssetEntries.Clear()
        cacheAssetList.Items.Clear()
        cacheAssetStatus.Text = "Scanning models and textures..."
        Try
            cacheAssetEntries = Await Task.Run(
                Function()
                    Return RobloxCacheAssetExtractor.Scan(
                        Sub(value)
                            Me.BeginInvoke(New Action(Sub()
                                                          If value.Total > 0 Then cacheAssetProgress.Value = Math.Min(100, CInt(CLng(value.Current) * 100L \ value.Total))
                                                          cacheAssetStatus.Text = $"Scanning... {value.Found:N0} files found"
                                                      End Sub))
                        End Sub,
                        RobloxCacheFileType.RbxmBinary, RobloxCacheFileType.RbxmXml,
                        RobloxCacheFileType.Ktx1, RobloxCacheFileType.Ktx2)
                End Function)
            RefreshCacheAssetList()
            Dim rbxmCount = cacheAssetEntries.Where(Function(entry) entry.FileType = RobloxCacheFileType.RbxmBinary OrElse entry.FileType = RobloxCacheFileType.RbxmXml).Count()
            Dim ktxCount = cacheAssetEntries.Count - rbxmCount
            cacheAssetStatus.Text = $"Found {rbxmCount:N0} RBXM and {ktxCount:N0} KTX files."
        Catch ex As Exception
            cacheAssetStatus.Text = "Cache scan failed."
            CallError($"RBXM/KTX scan failed: {ex.Message}")
        Finally
            SetCacheAssetBusy(False)
        End Try
    End Sub

    Private Async Sub CacheAssetExportButton_Click(sender As Object, e As EventArgs) Handles cacheAssetExportButton.Click
        Dim entry = TryCast(cacheAssetList.SelectedItem, RobloxCacheAssetEntry)
        If entry Is Nothing Then Return
        Using dialog As New SaveFileDialog With {
            .Title = $"Export {entry.TypeLabel}",
            .Filter = $"{entry.TypeLabel} (*{entry.Extension})|*{entry.Extension}|All files (*.*)|*.*",
            .FileName = entry.Hash & entry.Extension,
            .DefaultExt = entry.Extension.TrimStart("."c),
            .AddExtension = True
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            SetCacheAssetBusy(True)
            cacheAssetProgress.Style = ProgressBarStyle.Marquee
            cacheAssetStatus.Text = $"Exporting {entry.TypeLabel}..."
            Try
                Await Task.Run(Sub() RobloxCacheAssetExtractor.Export(entry, dialog.FileName))
                cacheAssetStatus.Text = $"Exported {Path.GetFileName(dialog.FileName)}"
            Catch ex As Exception
                cacheAssetStatus.Text = "Export failed."
                CallError($"Asset export failed: {ex.Message}")
            Finally
                cacheAssetProgress.Style = ProgressBarStyle.Blocks
                SetCacheAssetBusy(False)
            End Try
        End Using
    End Sub

    Private Async Sub CacheAssetExportAllButton_Click(sender As Object, e As EventArgs) Handles cacheAssetExportAllButton.Click
        If cacheAssetEntries.Count = 0 Then Return
        Using dialog As New FolderBrowserDialog With {.Description = "Choose a folder for RBXM and KTX files", .UseDescriptionForTitle = True}
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            SetCacheAssetBusy(True)
            cacheAssetStatus.Text = $"Exporting {cacheAssetEntries.Count:N0} files..."
            Try
                Dim summary = Await Task.Run(
                    Function()
                        Return RobloxCacheAssetExtractor.ExportMany(
                            cacheAssetEntries, dialog.SelectedPath,
                            Sub(current, total)
                                If current Mod 25 = 0 OrElse current = total Then
                                    Me.BeginInvoke(New Action(Sub()
                                                                  If total > 0 Then cacheAssetProgress.Value = Math.Min(100, CInt(CLng(current) * 100L \ total))
                                                                  cacheAssetStatus.Text = $"Exporting {current:N0} of {total:N0}..."
                                                              End Sub))
                                End If
                            End Sub, True)
                    End Function)
                cacheAssetStatus.Text = $"Exported {summary.Exported:N0}; reused {summary.Reused:N0}; failed {summary.Failed:N0}."
            Catch ex As Exception
                cacheAssetStatus.Text = "Bulk export failed."
                CallError($"RBXM/KTX export failed: {ex.Message}")
            Finally
                SetCacheAssetBusy(False)
            End Try
        End Using
    End Sub


    Private Sub MeshList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles meshList.SelectedIndexChanged
        meshExportButton.Enabled = meshList.SelectedItem IsNot Nothing AndAlso meshScanButton.Enabled
    End Sub
    Private Sub SetMeshBusy(busy As Boolean)
        meshScanButton.Enabled = Not busy
        meshExportButton.Enabled = Not busy AndAlso meshList.SelectedItem IsNot Nothing
        meshExportAllButton.Enabled = Not busy AndAlso meshEntries.Any(Function(item) item.CanExport)
        meshList.Enabled = Not busy
        If Not busy Then meshProgress.Value = 0
    End Sub

    Private Async Sub MeshScanButton_Click(sender As Object, e As EventArgs) Handles meshScanButton.Click
        SetMeshBusy(True)
        meshList.Items.Clear()
        meshEntries.Clear()
        meshStatus.Text = "Scanning the Roblox cache..."
        Try
            Dim progress = New Progress(Of MeshScanProgress)(
                Sub(value)
                    If value.Total > 0 Then meshProgress.Value = Math.Min(100, CInt(value.Current * 100L \ value.Total))
                    meshStatus.Text = $"Scanning... {value.Current:N0} of {value.Total:N0} entries ({value.Found:N0} meshes found)"
                End Sub)
            meshEntries = Await Task.Run(Function() RobloxMeshExtractor.Scan(progress))
            meshList.BeginUpdate()
            For Each entry In meshEntries
                meshList.Items.Add(entry)
            Next
            meshList.EndUpdate()
            Dim ready = meshEntries.Where(Function(item) item.CanExport).Count()
            meshStatus.Text = $"Found {meshEntries.Count:N0} meshes; {ready:N0} can be exported to OBJ."
        Catch ex As Exception
            meshStatus.Text = "Mesh scan failed."
            CallError($"Mesh scan failed: {ex.Message}")
        Finally
            SetMeshBusy(False)
        End Try
    End Sub

    Private Async Sub MeshList_DoubleClick(sender As Object, e As EventArgs) Handles meshList.DoubleClick
        Dim entry = TryCast(meshList.SelectedItem, MeshCacheEntry)
        If entry Is Nothing Then Return
        If Not entry.CanExport Then
            CallError($"Mesh version {entry.Version} cannot be previewed yet.")
            Return
        End If

        SetMeshBusy(True)
        meshProgress.Style = ProgressBarStyle.Marquee
        meshStatus.Text = $"Loading preview for {entry.Hash.Substring(0, 12)}..."
        Try
            Dim data = Await Task.Run(Function() RobloxMeshExtractor.LoadPreview(entry))
            Dim preview As New MeshPreviewForm(entry, data)
            preview.Show(Me)
            meshStatus.Text = $"Previewing {data.Positions.Length:N0} vertices and {data.Indices.Length \ 3:N0} triangles."
        Catch ex As Exception
            meshStatus.Text = "Mesh preview failed."
            CallError($"Mesh preview failed: {ex.Message}")
        Finally
            meshProgress.Style = ProgressBarStyle.Blocks
            SetMeshBusy(False)
        End Try
    End Sub
    Private Async Sub MeshExportButton_Click(sender As Object, e As EventArgs) Handles meshExportButton.Click
        Dim entry = TryCast(meshList.SelectedItem, MeshCacheEntry)
        If entry Is Nothing Then Return
        If Not entry.CanExport Then
            CallError($"Mesh version {entry.Version} is not supported yet.")
            Return
        End If
        Using dialog As New SaveFileDialog With {
            .Title = "Export Roblox mesh as OBJ",
            .Filter = "Wavefront OBJ (*.obj)|*.obj",
            .FileName = $"{entry.Hash}.obj",
            .AddExtension = True,
            .DefaultExt = "obj"
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            SetMeshBusy(True)
            meshProgress.Style = ProgressBarStyle.Marquee
            meshStatus.Text = $"Exporting {entry.Hash.Substring(0, 12)}..."
            Try
                Dim result = Await Task.Run(Function() RobloxMeshExtractor.Export(entry, dialog.FileName))
                meshStatus.Text = $"Exported {result.VertexCount:N0} vertices and {result.FaceCount:N0} faces."
            Catch ex As Exception
                meshStatus.Text = "Mesh export failed."
                CallError($"Mesh export failed: {ex.Message}")
            Finally
                meshProgress.Style = ProgressBarStyle.Blocks
                SetMeshBusy(False)
            End Try
        End Using
    End Sub

    Private Async Sub MeshExportAllButton_Click(sender As Object, e As EventArgs) Handles meshExportAllButton.Click
        Dim exportable = meshEntries.Where(Function(item) item.CanExport).ToList()
        If exportable.Count = 0 Then Return
        Using dialog As New FolderBrowserDialog With {.Description = "Choose a folder for the exported OBJ files", .UseDescriptionForTitle = True}
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            SetMeshBusy(True)
            meshStatus.Text = $"Exporting {exportable.Count:N0} meshes..."
            Try
                Dim progress As IProgress(Of Integer) = New Progress(Of Integer)(
                    Sub(current)
                        meshProgress.Value = Math.Min(100, CInt(current * 100L \ exportable.Count))
                        meshStatus.Text = $"Exporting mesh {current:N0} of {exportable.Count:N0}..."
                    End Sub)
                Dim summary = Await Task.Run(
                    Function()
                        Dim exported = 0
                        Dim failed = 0
                        For i = 0 To exportable.Count - 1
                            Dim entry = exportable(i)
                            Try
                                RobloxMeshExtractor.Export(entry, Path.Combine(dialog.SelectedPath, entry.Hash & ".obj"))
                                exported += 1
                            Catch
                                failed += 1
                            End Try
                            progress.Report(i + 1)
                        Next
                        Return (Exported:=exported, Failed:=failed)
                    End Function)
                meshStatus.Text = $"Exported {summary.Exported:N0} meshes; {summary.Failed:N0} failed."
            Catch ex As Exception
                meshStatus.Text = "Bulk mesh export failed."
                CallError($"Bulk mesh export failed: {ex.Message}")
            Finally
                SetMeshBusy(False)
            End Try
        End Using
    End Sub
    Public Sub DownloadUpdate()

        Dim updateUrl As String = "https://rbxaudioextractor-update-server.netlify.app/RBXAssetExtractor/bin/Release/net8.0-windows/publish/win-x64/RBXAssetExtractor.exe"
        Dim tempPath As String = Path.Combine(Path.GetTempPath(), "new_version.exe")

        Try

            My.Computer.Network.DownloadFile(updateUrl, tempPath) 'Replaced becuase Webclient is obsolete 2/21/2025 1:42 PM CST

            Dim batchFile As String = Path.Combine(Path.GetTempPath(), "update.bat")
            System.IO.File.WriteAllText(batchFile, $"
@echo off
timeout /t 2 /nobreak >nul
del ""{Application.ExecutablePath}""
move ""{tempPath}"" ""{Application.ExecutablePath}""
del ""{tempPath}""
start """" ""{Application.ExecutablePath}""
del %0
            ")


            Process.Start(New ProcessStartInfo With {
                .FileName = batchFile,
                .WindowStyle = ProcessWindowStyle.Hidden
            })
            Application.Exit()

        Catch ex As Exception
            CallError($"Update failed: {ex.Message}")
        End Try

    End Sub

    Private Function CompareVersions(v1 As String, v2 As String) As Integer
        Dim cleanV1 = CleanVersionString(v1)
        Dim cleanV2 = CleanVersionString(v2)

        Dim parts1 = cleanV1.Split("."c)
        Dim parts2 = cleanV2.Split("."c)

        Dim length = Math.Max(parts1.Length, parts2.Length)

        For i = 0 To length - 1
            Dim num1 = If(i < parts1.Length, Integer.Parse(parts1(i)), 0)
            Dim num2 = If(i < parts2.Length, Integer.Parse(parts2(i)), 0)

            If num1 < num2 Then Return -1
            If num1 > num2 Then Return 1
        Next

        Return 0
    End Function

    Private Function CleanVersionString(version As String) As String
        Dim v = version.ToLower()
        If v.StartsWith("v") Then
            v = v.Substring(1)
        End If

        Dim dashIndex = v.IndexOf("-"c)
        If dashIndex >= 0 Then
            v = v.Substring(0, dashIndex)
        End If

        Return v
    End Function

    Public Function CheckForUpdates() As Boolean
        Dim content As String
        Dim url As String = "https://animated-platypus-6ba0a9.netlify.app/v.txt"

        output_log.Items.Add($"Checking for updates from server: {url}")

        Try
            Using client As New HttpClient()
                content = client.GetStringAsync(url).GetAwaiter().GetResult().Trim()
            End Using


            Dim compareResult As Integer = CompareVersions(V, content)

            If compareResult < 0 Then
                Outdated = True
                Dim text As String = $"The program is out of date. You have version: {V} and the latest available version is: {content}. Would you like to automatically download the update?"
                output_log.Items.Add(text)

                Dim Update As DialogResult = MessageBox.Show(text, "BRUJ", MessageBoxButtons.YesNo)

                If Update = DialogResult.Yes Then
                    DownloadUpdate()
                End If
            ElseIf compareResult > 0 Then
                output_log.Items.Add($"You are running a newer version ({V}) than the latest release ({content}). This may be a dev build.")
                MsgBox($"You are running a newer version ({V}) than the latest release ({content}). This may be a dev build. If you are seeing this by mistake, please report it to the developers, as this build may be unstable.", 48 + 0, "Warning")
            Else
                output_log.Items.Add($"You are using the latest version: {V}")
            End If

            Return True
        Catch ex As Exception
            output_log.Items.Add($"Error checking for updates: {ex.Message}")
            Return False
        End Try
    End Function



    Dim DisableTimer019 = False

    Private Sub LoadFullGame(Optional audioOnly As Boolean = True)
        If CheckForRoblox() Then
            If ProgressBar1.Value = 0 Then
                Dim assetLabel = If(audioOnly, "audio", "image")
                Dim result = MessageBox.Show($"Cached {assetLabel} files from previous sessions may still be present. Continue scanning the Roblox cache?", "Scan Roblox cache?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

                If result = DialogResult.Yes Then

                    Try
                        LoadHTTP0.RunWorkerAsync(audioOnly)
                    Catch ex As Exception
                        CallError("Two processes cannot run at the same time please wait for the original process to stop.")
                    End Try

                End If
            Else
                CallError("Cannot start two processes at the same time.")
            End If
        End If




    End Sub



    Private Sub LoadHttpBtn_Click(sender As Object, e As EventArgs) Handles LoadHttpBtn.Click
        LoadFullGame()
    End Sub

    Private Sub LoadOptimizedFullGameAudio()
        Me.Invoke(New Action(Sub()
                                 HTTPLISTBOX.Items.Clear()
                                 selectedFullAudioEntry = Nothing
                                 TaskLBR.Text = "Scanning Roblox audio cache"
                                 fullAudioStatus.Text = "Scanning the Roblox cache for audio..."
                                 TaskLBR.Visible = True
                                 ProgressBar1.Value = 0
                             End Sub))
        Try
            fullAudioEntries = RobloxCacheAssetExtractor.Scan(
                Sub(value)
                    Me.BeginInvoke(New Action(Sub()
                                                  If value.Total > 0 Then ProgressBar1.Value = Math.Min(99, CInt(CLng(value.Current) * 100L \ value.Total))
                                                  fullAudioStatus.Text = $"Scanning audio... {value.Found:N0} found"
                                              End Sub))
                End Sub, RobloxCacheFileType.Ogg, RobloxCacheFileType.Mp3)
            Dim listItems = fullAudioEntries.Cast(Of Object)().ToArray()
            Me.Invoke(New Action(Sub()
                                     HTTPLISTBOX.BeginUpdate()
                                     Try
                                         HTTPLISTBOX.Items.Clear()
                                         HTTPLISTBOX.Items.AddRange(listItems)
                                     Finally
                                         HTTPLISTBOX.EndUpdate()
                                     End Try
                                     fullAudioStatus.Text = $"Found {fullAudioEntries.Count:N0} audio files. Select one to stream it."
                                     output_log.Items.Add($"Audio scan complete: {fullAudioEntries.Count:N0} cache references found; no temporary files created.")
                                     ProgressBar1.Value = 0
                                     StatusLBR.Text = "0%"
                                     TaskLBR.Visible = False
                                 End Sub))
        Catch ex As Exception
            Me.Invoke(New Action(Sub()
                                     fullAudioStatus.Text = "Audio scan failed."
                                     ProgressBar1.Value = 0
                                     StatusLBR.Text = "0%"
                                     TaskLBR.Visible = False
                                     CallError($"Error reading Roblox audio cache: {ex.Message}")
                                 End Sub))
        End Try
    End Sub
    Private Sub LoadOptimizedFullGameImages()
        Me.Invoke(New Action(Sub()
                                 LoadImgListBox.Items.Clear()
                                 selectedFullImageEntry = Nothing
                                 TaskLBR.Text = "Scanning Roblox image cache"
                                 fullImageStatus.Text = "Scanning the Roblox cache for images..."
                                 TaskLBR.Visible = True
                                 ProgressBar1.Value = 0
                             End Sub))
        Try
            fullImageEntries = RobloxCacheAssetExtractor.Scan(
                Sub(value)
                    Me.BeginInvoke(New Action(Sub()
                                                  If value.Total > 0 Then ProgressBar1.Value = Math.Min(99, CInt(CLng(value.Current) * 100L \ value.Total))
                                                  fullImageStatus.Text = $"Scanning images... {value.Found:N0} found"
                                              End Sub))
                End Sub,
                RobloxCacheFileType.Png, RobloxCacheFileType.Jpeg, RobloxCacheFileType.Bmp, RobloxCacheFileType.WebP)
            Dim listItems = fullImageEntries.Cast(Of Object)().ToArray()
            Me.Invoke(New Action(Sub()
                                     LoadImgListBox.BeginUpdate()
                                     Try
                                         LoadImgListBox.Items.Clear()
                                         LoadImgListBox.Items.AddRange(listItems)
                                     Finally
                                         LoadImgListBox.EndUpdate()
                                     End Try
                                     fullImageStatus.Text = $"Found {fullImageEntries.Count:N0} images. Select one to preview it."
                                     output_log.Items.Add($"Image scan complete: {fullImageEntries.Count:N0} cache references found; no temporary files created.")
                                     ProgressBar1.Value = 0
                                     StatusLBR.Text = "0%"
                                     TaskLBR.Visible = False
                                 End Sub))
        Catch ex As Exception
            Me.Invoke(New Action(Sub()
                                     fullImageStatus.Text = "Image scan failed."
                                     ProgressBar1.Value = 0
                                     StatusLBR.Text = "0%"
                                     TaskLBR.Visible = False
                                     CallError($"Error reading Roblox image cache: {ex.Message}")
                                 End Sub))
        End Try
    End Sub
    Private Sub LoadHTTP_DoWork_1(sender As Object, e As DoWorkEventArgs) Handles LoadHTTP0.DoWork
        If Not TypeOf e.Argument Is Boolean Then Return
        If CBool(e.Argument) Then
            LoadOptimizedFullGameAudio()
        Else
            LoadOptimizedFullGameImages()
        End If
    End Sub
    Private Sub LoadHTTP0_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles LoadHTTP0.ProgressChanged
        ProgressBar1.Value = e.ProgressPercentage
    End Sub





    Sub EnableButtons(Enabled)
        ClearHTTPTEMP_BTN.Enabled = Enabled
        ClearTMP_BTN.Enabled = Enabled
        LoadParButton.Enabled = Enabled
        LoadHttpBtn.Enabled = Enabled
        ImgClearTmp.Enabled = Enabled
        LoadImgBtn.Enabled = Enabled
    End Sub


    Private Sub LoadPartialBackgoundWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles LoadPartialBackgoundWorker.DoWork
        Dim tempDirectory = Path.GetTempPath
        If Not My.Computer.FileSystem.FileExists(tempDirectory & "\RBX_SOUND_RIPPER") Then
            My.Computer.FileSystem.CreateDirectory(tempDirectory & "\RBX_SOUND_RIPPER")
        End If

        RemoveAllFileInDir1(tempDirectory & "\RBX_SOUND_RIPPER")


        CloneDirectory(tempDirectory & "\Roblox\sounds", tempDirectory & "\RBX_SOUND_RIPPER")
        RenameFileExtensions(tempDirectory & "\RBX_SOUND_RIPPER", ".ogg")

        Me.Invoke(Sub()
                      Sounds_Listbox.Items.Clear()
                  End Sub)



        Dim musicFiles = Directory.GetFiles(tempDirectory & "\RBX_SOUND_RIPPER", "*.ogg")

        Dim sortedMusicFiles = musicFiles.Select(Function(file)
                                                     Dim track As New Track(file)
                                                     Return New With {
                                                 .FilePath = file,
                                                 .Track = track,
                                                 .Duration = track.Duration
                                             }
                                                 End Function).
                                         OrderByDescending(Function(x) x.Duration).ToList()

        For Each fileData In sortedMusicFiles
            Dim InfoLoaded = True

            Dim title = fileData.Track.Title
            Dim Album = fileData.Track.Album
            Dim Artist = fileData.Track.Artist
            Dim Duration = TimeSpan.FromSeconds(fileData.Duration).ToString("mm\:ss")

            If String.IsNullOrEmpty(Album) Then
                InfoLoaded = False
            End If

            Try
                Me.Invoke(Sub()
                              If InfoLoaded Then
                                  Sounds_Listbox.Items.Insert(0, New With {
                              .DisplayText = $"{Duration} - {title} | Artists: {Artist} | Album: {Album} | Duration: {Duration}",
                              .FilePath = fileData.FilePath
                          })
                              Else
                                  Sounds_Listbox.Items.Add(New With {
                              .DisplayText = $"{Duration} - {Path.GetFileName(fileData.FilePath)}",
                              .FilePath = fileData.FilePath
                          })
                              End If
                          End Sub)
            Catch ex As Exception
                Me.Invoke(Sub()
                              CallError($"Error reading file: {fileData.FilePath} - {ex.Message}")
                          End Sub)
            End Try
        Next

        Me.Invoke(Sub()
                      Sounds_Listbox.DisplayMember = "DisplayText"
                      Sounds_Listbox.ValueMember = "FilePath"
                  End Sub)

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles LoadParButton.Click
        If CheckForRoblox() Then
            Try
                LoadPartialBackgoundWorker.RunWorkerAsync()
            Catch ex As Exception
                CallError("Two processes cannot run at the same time please wait for the original process to stop.")
            End Try
        End If


    End Sub


    Private Sub UpdateLog(message As String)
        If output_log.InvokeRequired Then
            output_log.Invoke(New Action(Of String)(AddressOf UpdateLog), message)
        Else
            output_log.Items.Add(message)
        End If
    End Sub



    Sub KillProcessByName(processName As String)
        Try
            Dim processes As Process() = Process.GetProcessesByName(processName)
            If processes.Length > 0 Then
                For Each proc As Process In processes
                    proc.Kill()
                    Console.WriteLine($"Killed {processName} with PID: {proc.Id}")
                Next
            Else
                Console.WriteLine($"{processName} not running.")
            End If
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
        End Try
    End Sub

    Private Sub StartRobloxCacheClear()
        Dim clearResult = MessageBox.Show(
            "This will delete Roblox's local asset cache, including rbx-storage.db. Continue?",
            "Clear Roblox asset cache?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If clearResult <> DialogResult.Yes Then Return

        Dim forceClose = MessageBox.Show(
            "Do you want RBX Asset Extractor to force-close all running Roblox processes first?",
            "Force-close Roblox?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If forceClose = DialogResult.Yes Then
            Dim finalConfirmation = MessageBox.Show(
                "WARNING: This will immediately close Roblox Player, Roblox Studio, and every other process whose name begins with 'Roblox'. Unsaved work in Studio may be lost." & vbCrLf & vbCrLf &
                "Force-close them and continue clearing the cache?",
                "Roblox Studio will be closed", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button2)
            If finalConfirmation <> DialogResult.Yes Then Return

            Dim result = ForceCloseRobloxProcesses()
            output_log.Items.Add($"Roblox process close complete: {result.Killed} closed, {result.Failed} failed.")
            If result.Failed > 0 Then
                MessageBox.Show($"{result.Failed} Roblox process(es) could not be closed. Cache clearing will still be attempted.",
                                "Some processes could not be closed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End If

        If ClearCache.IsBusy Then
            CallError("A cache-clear operation is already running.")
            Return
        End If
        fullAudioStatus.Text = "Clearing the Roblox asset cache..."
        fullImageStatus.Text = "Clearing the Roblox asset cache..."
        ClearCache.RunWorkerAsync()
    End Sub

    Private Function ForceCloseRobloxProcesses() As (Killed As Integer, Failed As Integer)
        Dim killed = 0
        Dim failed = 0
        For Each proc As System.Diagnostics.Process In System.Diagnostics.Process.GetProcesses()
            Try
                Dim processName = proc.ProcessName
                If Not processName.StartsWith("Roblox", StringComparison.OrdinalIgnoreCase) Then Continue For
                Dim processId = proc.Id
                Try
                    proc.Kill(True)
                    proc.WaitForExit(5000)
                    killed += 1
                    UpdateLog($"Closed Roblox process: {processName} (PID {processId})")
                Catch ex As Exception
                    failed += 1
                    UpdateLog($"Failed to close Roblox process {processName} (PID {processId}): {ex.Message}")
                End Try
            Catch
                ' A process may exit while its name is being inspected.
            Finally
                proc.Dispose()
            End Try
        Next
        Return (killed, failed)
    End Function
    Public Sub CallError(Err1)

        Try
            output_log.Items.Add($"ERROR {Err1}")
            MsgBox($"ERROR {Err1}", 0 + 16, "ERROR")
        Catch ex As Exception
            MsgBox($"Full error details: {ex}", 0 + 16, "The error handler encountered an error how did you even manage to do that??!")
        End Try

    End Sub




    Sub RenameFileExtensions(directoryPath As String, newExtension As String)
        Dim files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
        If Not Directory.Exists(directoryPath) Then

            Me.Invoke(Sub()
                          UpdateLog($"ERROR: Directory does not exist: {directoryPath}")
                      End Sub)


        End If


        For i As Integer = 0 To files.Length - 1
            Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(files(i))
            Dim newFileName As String = Path.Combine(directoryPath, fileNameWithoutExt & newExtension)
            Me.Invoke(Sub()
                          UpdateLog($"Renaming file extension: {files(i)} To {newFileName} ")
                      End Sub)

            Try
                System.IO.File.Move(files(i), newFileName, True)
            Catch ex As Exception

            End Try

            Dim progress As Integer = CInt(((i + 1) / files.Length) * 100)
            Me.Invoke(Sub()
                          ProgressBar1.Value = progress
                      End Sub)
        Next
    End Sub




    Sub RemoveAllFileInDir1(directoryPath)

        Dim files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)


        If Not Directory.Exists(directoryPath) Then

            UpdateLog($"ERROR: Directory does not exist: {directoryPath}")
        Else

            For i As Integer = 0 To files.Length - 1

                Try
                    UpdateLog($"Deleting file: {files(i)}")
                    System.IO.File.Delete(files(i))
                Catch ex As Exception
                    UpdateLog($"Failed to Delete file with error: {ex.Message}")
                End Try

                Dim progress As Integer = CInt(((i + 1) / files.Length) * 100)
                Me.Invoke(Sub()
                              ProgressBar1.Value = progress
                          End Sub)
            Next

            Me.Invoke(Sub()
                          ProgressBar1.Value = 100
                      End Sub)

        End If
    End Sub



    Sub CloneDirectory(sourceDir As String, targetDir As String)

        If Not Directory.Exists(sourceDir) Then
            Me.Invoke(Sub()
                          output_log.Items.Add($"Source directory does not exist: {sourceDir}")
                      End Sub)

        End If


        If Not Directory.Exists(targetDir) Then
            Directory.CreateDirectory(targetDir)
        End If


        For Each file As String In Directory.GetFiles(sourceDir)
            Try
                Dim fileName As String = Path.GetFileName(file)
                Dim destFile As String = Path.Combine(targetDir, fileName)
                Me.Invoke(Sub()
                              output_log.Items.Add($"Cloning: {fileName} Into: {destFile}")
                          End Sub)



                System.IO.File.Copy(file, destFile, True)
            Catch ex As Exception
                Me.Invoke(Sub()
                              output_log.Items.Add(ex.Message)
                          End Sub)


            End Try

        Next


        For Each subDir As String In Directory.GetDirectories(sourceDir)
            Try
                Dim subDirName As String = Path.GetFileName(subDir)
                Dim destSubDir As String = Path.Combine(targetDir, subDirName)
                Me.Invoke(Sub()
                              output_log.Items.Add($"Cloning (subdirectory): {subDirName} Into: {destSubDir}")
                          End Sub)

                CloneDirectory(subDir, destSubDir)
            Catch ex As Exception
                Me.Invoke(Sub()
                              output_log.Items.Add(ex.Message)
                          End Sub)
            End Try
        Next
    End Sub

    Public Async Sub SaveFile()
        Dim isCachedAudio = selectedFullAudioEntry IsNot Nothing
        Using saveFileDialog As New SaveFileDialog With {
            .Filter = If(isCachedAudio, $"{selectedFullAudioEntry.TypeLabel} (*{selectedFullAudioEntry.Extension})|*{selectedFullAudioEntry.Extension}", "OGG Files (*.ogg)|*.ogg"),
            .DefaultExt = If(isCachedAudio, selectedFullAudioEntry.Extension.TrimStart("."c), "ogg"),
            .AddExtension = True,
            .FileName = If(isCachedAudio, selectedFullAudioEntry.Hash & selectedFullAudioEntry.Extension, String.Empty)
        }
            If saveFileDialog.ShowDialog() <> DialogResult.OK Then Return
            Try
                If isCachedAudio Then
                    Dim entry = selectedFullAudioEntry
                    Await Task.Run(Sub() RobloxCacheAssetExtractor.Export(entry, saveFileDialog.FileName))
                    fullAudioStatus.Text = $"Exported {Path.GetFileName(saveFileDialog.FileName)}"
                ElseIf Not String.IsNullOrEmpty(SelFile) AndAlso System.IO.File.Exists(SelFile) Then
                    My.Computer.FileSystem.CopyFile(SelFile, saveFileDialog.FileName, overwrite:=True)
                Else
                    CallError("No audio file is selected.")
                End If
            Catch ex As Exception
                CallError(ex.Message)
            End Try
        End Using
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Download_BTN.Click
        SaveFile()
    End Sub





    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles ClearTMP_BTN.Click
        If CheckForRoblox() Then
            Dim result = MessageBox.Show("Are you sure you want to clear the temp directory? You will you will lose access to all of the audios your Roblox client has saved", "You sure?", MessageBoxButtons.YesNo)
            Dim tempDirectory = Path.GetTempPath
            If result = DialogResult.Yes Then
                RemoveAllFileInDir1(tempDirectory & "\Roblox\sounds")
            End If
        End If

    End Sub


    Public Sub SaveAll()
        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select a folder"
            folderDialog.ShowNewFolderButton = True

            If folderDialog.ShowDialog() = DialogResult.OK Then

            End If
        End Using
    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles DownloadALL_BTN.Click
        Using folderDialog As New FolderBrowserDialog
            folderDialog.Description = "Select a folder"
            folderDialog.ShowNewFolderButton = True

            If folderDialog.ShowDialog = DialogResult.OK Then
                CloneDirectory(tempDirectory & "\RBX_SOUND_RIPPER", folderDialog.SelectedPath)
            End If
        End Using
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub ListBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Sounds_Listbox.SelectedIndexChanged
        If Sounds_Listbox.SelectedItem IsNot Nothing Then

            Try
                selectedFullAudioEntry = Nothing
                Dim selectedFile = Sounds_Listbox.SelectedItem.FilePath
                SelFile = selectedFile
                PlayOggFile(SelFile)
            Catch ex As Exception
                CallError(ex.Message)
            End Try

        End If
    End Sub

    Private Sub CheckFOrButtons_Tick(sender As Object, e As EventArgs) Handles CheckFOrButtons.Tick
        If Sounds_Listbox.Items.Count > 0 Then
            DownloadALL_BTN.Enabled = True
            Download_BTN.Enabled = True
        Else
            DownloadALL_BTN.Enabled = False
            Download_BTN.Enabled = False
        End If

        If HTTPLISTBOX.Items.Count > 0 Then
            DownloadAllHTTP_BTN.Enabled = True
            DOWNLOADHTTP_BTN.Enabled = True
        Else
            DownloadAllHTTP_BTN.Enabled = False
            DOWNLOADHTTP_BTN.Enabled = False
        End If
    End Sub

    Public Sub OpenWebAdder(Adder)
        Try
            Process.Start(New ProcessStartInfo(Adder) With {.UseShellExecute = True})
        Catch ex1 As Exception
            Try
                Dim edgePath As String = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
                If Not System.IO.File.Exists(edgePath) Then
                    edgePath = "C:\Program Files\Microsoft\Edge\Application\msedge.exe"
                End If

                Process.Start(edgePath, "http://zv800.com/")
            Catch ex2 As Exception
                CallError("Failed to open browser: " & ex2.Message)
            End Try
        End Try
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        OpenWebAdder("http://zv800.com/")
    End Sub





    Private Async Sub ListBox1_SelectedIndexChanged_1(sender As Object, e As EventArgs) Handles HTTPLISTBOX.SelectedIndexChanged
        Dim entry = TryCast(HTTPLISTBOX.SelectedItem, RobloxCacheAssetEntry)
        If entry Is Nothing Then Return
        selectedFullAudioEntry = entry
        SelFile = Nothing
        Await PlayCachedOggAsync(entry)
    End Sub

    Private Sub DOWNLOADHTTP_BTN_Click(sender As Object, e As EventArgs) Handles DOWNLOADHTTP_BTN.Click
        SaveFile()
    End Sub




    Private Sub ClearHTTPTEMP_BTN_Click(sender As Object, e As EventArgs) Handles ClearHTTPTEMP_BTN.Click
        StartRobloxCacheClear()
    End Sub

    Private Sub ListboxAutoScrool_Tick(sender As Object, e As EventArgs) Handles ListboxAutoScrool.Tick
        If AutoScrollCHK.Checked Then
            Try
                output_log.TopIndex = output_log.Items.Count - 1
            Catch ex As Exception
            End Try

        End If


        StatusLBR.Text = $"{ProgressBar1.Value}%"
    End Sub

    Private Async Sub DownloadAllHTTP_BTN_Click(sender As Object, e As EventArgs) Handles DownloadAllHTTP_BTN.Click
        If fullAudioEntries.Count = 0 Then Return
        Using folderDialog As New FolderBrowserDialog With {.Description = "Select a folder for the cached audio files", .ShowNewFolderButton = True}
            If folderDialog.ShowDialog() <> DialogResult.OK Then Return
            DownloadAllHTTP_BTN.Enabled = False
            fullAudioStatus.Text = $"Exporting {fullAudioEntries.Count:N0} audio files..."
            Try
                Dim summary = Await Task.Run(
                    Function()
                        Return RobloxCacheAssetExtractor.ExportMany(
                            fullAudioEntries, folderDialog.SelectedPath,
                            Sub(current, total)
                                If current Mod 25 = 0 OrElse current = total Then
                                    Me.BeginInvoke(New Action(Sub() fullAudioStatus.Text = $"Exporting audio {current:N0} of {total:N0}..."))
                                End If
                            End Sub)
                    End Function)
                fullAudioStatus.Text = $"Exported {summary.Exported:N0}; reused {summary.Reused:N0}; failed {summary.Failed:N0}."
            Catch ex As Exception
                fullAudioStatus.Text = "Audio export failed."
                CallError(ex.Message)
            Finally
                DownloadAllHTTP_BTN.Enabled = True
            End Try
        End Using
    End Sub

    Private Sub Panel2_MouseDown(sender As Object, e As MouseEventArgs) Handles Panel2.MouseDown
        If e.Button = MouseButtons.Left Then
            isDragging = True
            startPoint = New Point(e.X, e.Y)
        End If
    End Sub

    Private Sub Panel2_MouseMove(sender As Object, e As MouseEventArgs) Handles Panel2.MouseMove
        If isDragging Then
            Dim endPoint As Point = PointToScreen(e.Location)
            Me.Location = New Point(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y)
        End If
    End Sub

    Private Sub Panel2_MouseUp(sender As Object, e As MouseEventArgs) Handles Panel2.MouseUp
        isDragging = False
    End Sub

    Private Sub CloseBTN_Click(sender As Object, e As EventArgs) Handles CloseBTN.Click
        Me.Close()
    End Sub

    Private Sub Button2_Click_2(sender As Object, e As EventArgs) Handles Button2.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub Form1_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Try
            If Not DisableFade And Not DisableFade2 Then
                For i As Double = Me.Opacity To 1.0 Step 0.05
                    Me.Opacity = i
                    Threading.Thread.Sleep(10)
                Next
            End If

        Catch ex As Exception

        End Try

    End Sub
    Private Sub fadeInTimer_Tick(sender As Object, e As EventArgs) Handles fadeInTimer.Tick
        If Me.Opacity < 1 Then
            Me.Opacity += 0.05
        Else
            fadeInTimer.Stop()
        End If
    End Sub
    Private Sub Form1_Deactivate(sender As Object, e As EventArgs) Handles Me.Deactivate
        For i As Double = Me.Opacity To 0.4 Step -0.05
            Try
                If Not DisableFade And Not DisableFade2 Then
                    Me.Opacity = i
                    Threading.Thread.Sleep(10)
                End If

            Catch ex As Exception

            End Try

        Next
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        DisableFade = True
        e.Cancel = True
        fadeOutTimer.Interval = 20
        fadeOutTimer.Start()
    End Sub

    Private Sub fadeOutTimer_Tick(sender As Object, e As EventArgs) Handles fadeOutTimer.Tick
        If Me.Opacity > 0 Then
            Me.Opacity -= 0.05
        Else

            fadeOutTimer.Stop()
            KillProcessByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath))
        End If
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        OpenWebAdder("https://github.com/zv8001/RBX-Audio-Extractor")

    End Sub

    Private Sub SaveLogBtn_Click(sender As Object, e As EventArgs) Handles SaveLogBtn.Click
        Dim SFD As New SaveFileDialog

        SFD.Filter = "LOG Files (*.log)|*.log"
        If SFD.ShowDialog = DialogResult.OK Then

            Try
                Dim listBoxItems As New List(Of String)

                For Each item As String In output_log.Items
                    listBoxItems.Add(item)
                Next

                If My.Computer.FileSystem.FileExists(SFD.FileName) Then
                    My.Computer.FileSystem.DeleteFile(SFD.FileName)
                End If

                System.IO.File.WriteAllLines(SFD.FileName, listBoxItems)
            Catch ex As Exception
                CallError($"an error code while trying to save the log file full error: {ex}")
            End Try


        End If
    End Sub

    Private Sub RemoveFilesInDir_ProgressChanged(sender As Object, e As ProgressChangedEventArgs)
        ProgressBar1.Value = e.ProgressPercentage
    End Sub
    Private Async Sub DownloadImgBtn_Click(sender As Object, e As EventArgs) Handles DownloadImgBtn.Click
        Dim entry = selectedFullImageEntry
        If entry Is Nothing Then Return
        Using saveDialog As New SaveFileDialog()
            If SaveAlAsPngCheck.Checked Then
                saveDialog.Filter = "PNG Files (*.png)|*.png"
                saveDialog.DefaultExt = "png"
                saveDialog.FileName = entry.Hash & ".png"
            Else
                saveDialog.Filter = $"{entry.TypeLabel} (*{entry.Extension})|*{entry.Extension}"
                saveDialog.DefaultExt = entry.Extension.TrimStart("."c)
                saveDialog.FileName = entry.Hash & entry.Extension
            End If
            saveDialog.AddExtension = True
            If saveDialog.ShowDialog() <> DialogResult.OK Then Return
            Try
                If SaveAlAsPngCheck.Checked Then
                    Await Task.Run(
                        Sub()
                            Dim payload = RobloxCacheAssetExtractor.ReadPayload(entry)
                            Using image As New MagickImage(payload)
                                image.Format = MagickFormat.Png
                                image.Write(saveDialog.FileName)
                            End Using
                        End Sub)
                Else
                    Await Task.Run(Sub() RobloxCacheAssetExtractor.Export(entry, saveDialog.FileName))
                End If
                fullImageStatus.Text = $"Exported {Path.GetFileName(saveDialog.FileName)}"
            Catch ex As Exception
                CallError(ex.Message)
            End Try
        End Using
    End Sub

    Private Function FormatBytes(bytes As Long) As String
        If bytes < 1024 Then
            Return bytes.ToString() & " bytes"
        ElseIf bytes < 1048576 Then
            Return (bytes / 1024).ToString("0.##") & " KB"
        ElseIf bytes < 1073741824 Then
            Return (bytes / 1048576).ToString("0.##") & " MB"
        Else
            Return (bytes / 1073741824).ToString("0.##") & " GB"
        End If
    End Function

    Private Function LoadImageFromPayload(payload As Byte()) As Image
        Using magickImage As New MagickImage(payload)
            Using stream As New MemoryStream()
                magickImage.Format = MagickFormat.Bmp
                magickImage.Write(stream)
                stream.Position = 0
                Using decoded = System.Drawing.Image.FromStream(stream)
                    Return New Bitmap(decoded)
                End Using
            End Using
        End Using
    End Function

    Private Async Sub LoadImgListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LoadImgListBox.SelectedIndexChanged
        Dim entry = TryCast(LoadImgListBox.SelectedItem, RobloxCacheAssetEntry)
        If entry Is Nothing Then Return
        selectedFullImageEntry = entry
        imagePreviewLoadVersion += 1
        Dim requestedVersion = imagePreviewLoadVersion
        fullImageStatus.Text = $"Loading {entry.Hash.Substring(0, 12)}..."
        Try
            Dim image = Await Task.Run(
                Function()
                    Dim payload = RobloxCacheAssetExtractor.ReadPayload(entry)
                    Return LoadImageFromPayload(payload)
                End Function)
            If requestedVersion <> imagePreviewLoadVersion OrElse Not Object.ReferenceEquals(selectedFullImageEntry, entry) Then
                image.Dispose()
                Return
            End If
            Dim oldImage = PreVeiwImgBox.Image
            PreVeiwImgBox.Image = image
            If oldImage IsNot Nothing Then oldImage.Dispose()
            ImgStats.Text = $"Dimensions: {image.Width} x {image.Height}" & vbCrLf & $"File Size: {FormatBytes(entry.Size)}"
            fullImageStatus.Text = $"Previewing {entry.Hash.Substring(0, 12)}... directly from the Roblox cache"
        Catch ex As Exception
            If requestedVersion = imagePreviewLoadVersion Then
                fullImageStatus.Text = "Image preview failed."
                CallError(ex.Message)
            End If
        End Try
    End Sub

    Private Sub LoadImgBtn_Click(sender As Object, e As EventArgs) Handles LoadImgBtn.Click
        LoadFullGame(False)
    End Sub

    Private Async Sub SaveAllBtn_Click(sender As Object, e As EventArgs) Handles SaveAllBtn.Click
        If fullImageEntries.Count = 0 Then Return
        Using folderDialog As New FolderBrowserDialog With {.Description = "Select a folder for the images", .ShowNewFolderButton = True}
            If folderDialog.ShowDialog() <> DialogResult.OK Then Return
            SaveAllBtn.Enabled = False
            fullImageStatus.Text = $"Exporting {fullImageEntries.Count:N0} images..."
            Try
                Dim summary = Await Task.Run(
                    Function()
                        Return RobloxCacheAssetExtractor.ExportMany(
                            fullImageEntries, folderDialog.SelectedPath,
                            Sub(current, total)
                                If current Mod 25 = 0 OrElse current = total Then
                                    Me.BeginInvoke(New Action(Sub() fullImageStatus.Text = $"Exporting images {current:N0} of {total:N0}..."))
                                End If
                            End Sub)
                    End Function)
                fullImageStatus.Text = $"Exported {summary.Exported:N0}; reused {summary.Reused:N0}; failed {summary.Failed:N0}."
            Catch ex As Exception
                fullImageStatus.Text = "Image export failed."
                CallError(ex.Message)
            Finally
                SaveAllBtn.Enabled = True
            End Try
        End Using
    End Sub

    Private Sub CheckBox1_CheckedChanged_1(sender As Object, e As EventArgs) Handles AlwaysOnTopCHK.CheckedChanged
        If AlwaysOnTopCHK.Checked Then
            Me.TopMost = True
            DisableFade2 = True
        Else
            DisableFade2 = False
            Me.TopMost = False
        End If
    End Sub


    Private Sub PreVeiwImgBox_Click(sender As Object, e As EventArgs) Handles PreVeiwImgBox.Click

    End Sub

    Private Sub Button1_Click_2(sender As Object, e As EventArgs) Handles ImgClearTmp.Click
        StartRobloxCacheClear()
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub KeepButtonsOff_Tick(sender As Object, e As EventArgs) Handles KeepButtonsOff.Tick
        'EnableButtons(False)
    End Sub

    Private Sub MainLoop_Tick(sender As Object, e As EventArgs) Handles MainLoop.Tick
        If LoadImgListBox.Items.Count > 0 Then
            DownloadImgBtn.Enabled = True
            SaveAllBtn.Enabled = True
        Else

            DownloadImgBtn.Enabled = False
            SaveAllBtn.Enabled = False
        End If

        If ProgressBar1.Value = 100 Then
            ProgressBar1.Value = 0
        End If
        If ProgressBar1.Value = 0 Then
            EnableButtons(True)
            LoadingGif.Visible = False
            ProgressBar1.Visible = False
        Else
            EnableButtons(False)
            LoadingGif.Visible = True
            ProgressBar1.Visible = True
        End If
    End Sub

    Private Sub ClearCache_DoWork(sender As Object, e As DoWorkEventArgs) Handles ClearCache.DoWork
        Dim directories As String() = {
        $"{tempDirectory}\Roblox\http",
        $"C:\Users\{Environment.UserName}\AppData\Local\Roblox\rbx-storage"
    }
        Try



            My.Computer.FileSystem.DeleteFile($"C:\Users\{Environment.UserName}\AppData\Local\Roblox\rbx-storage.db")

            Dim allFiles As New List(Of String)


            For Each dirPath In directories
                If Directory.Exists(dirPath) Then
                    Try
                        allFiles.AddRange(Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories))
                    Catch ex As Exception
                        UpdateLog($"ERROR: Failed to access directory {dirPath} with error: {ex.Message}")
                    End Try
                Else
                    UpdateLog($"ERROR: Directory does not exist: {dirPath}")
                End If
            Next


            For i As Integer = 0 To allFiles.Count - 1
                Try
                    UpdateLog($"Deleting file: {allFiles(i)}")
                    System.IO.File.Delete(allFiles(i))
                Catch ex As Exception
                    UpdateLog($"Failed to delete file with error: {ex.Message}")
                End Try

                Dim progress As Integer = CInt(((i + 1) / allFiles.Count) * 100)
                Me.Invoke(Sub()

                              ProgressBar1.Value = progress
                          End Sub)
            Next

            Me.Invoke(Sub()
                          ProgressBar1.Value = 100
                      End Sub)

            Me.Invoke(Sub()
                          StopPlayback()
                          HTTPLISTBOX.Items.Clear()
                          fullAudioEntries.Clear()
                          fullImageEntries.Clear()
                          selectedFullAudioEntry = Nothing
                          selectedFullImageEntry = Nothing
                          imagePreviewLoadVersion += 1
                          Dim oldPreview = PreVeiwImgBox.Image
                          PreVeiwImgBox.Image = Nothing
                          If oldPreview IsNot Nothing Then oldPreview.Dispose()
                          LoadImgListBox.Items.Clear()
                          fullAudioStatus.Text = "Roblox asset cache cleared. Load again to rescan."
                          fullImageStatus.Text = "Roblox asset cache cleared. Load again to rescan."
                      End Sub)

        Catch ex As Exception
            Me.Invoke(Sub()
                          fullAudioStatus.Text = "Cache clear failed."
                          fullImageStatus.Text = "Cache clear failed."
                          CallError("ERROR Roblox is currently using the database file please close Roblox before continuing | If Roblox is closed and you are receiving this error it is most likely because you have already cleared the cache")
                      End Sub)

        End Try



    End Sub


    Private Sub MSGPopup_DoWork(sender As Object, e As DoWorkEventArgs) Handles MSGPopup.DoWork

        Me.Invoke(Sub()
                      Try
                          Dim url = "https://animated-platypus-6ba0a9.netlify.app/message.txt"

                          Dim content As String

                          Using client As New HttpClient()
                              content = client.GetStringAsync(url).GetAwaiter().GetResult().Trim()
                          End Using

                          If Not content = "" Then


                              output_log.Items.Add($"received message: {content}")


                              MsgBox($"Message from the Creator: {content}", 0 + 64, "Message from the Creator")

                          End If

                      Catch ex As Exception

                          output_log.Items.Add($"ERROR FAILED TO RECEIVE MESSAGE WITH ERROR CODE: {ex}")

                      End Try
                  End Sub)

    End Sub


    'All of the code for the custom OGG player added in v1.1.3
    Private audioReader As WaveStream
    Private outputDevice As IWavePlayer
    Private isDragging0 As Boolean = False
    Private isStopping As Boolean = False


    Private Async Function PlayCachedOggAsync(entry As RobloxCacheAssetEntry) As Task
        Try
            fullAudioStatus.Text = $"Loading {entry.Hash.Substring(0, 12)}..."
            Dim payload = Await Task.Run(Function() RobloxCacheAssetExtractor.ReadPayload(entry))
            If Not Object.ReferenceEquals(selectedFullAudioEntry, entry) Then Return
            PlayCachedAudioPayload(payload, entry.FileType)
            fullAudioStatus.Text = $"Streaming {entry.Hash.Substring(0, 12)}... from the Roblox cache"
        Catch ex As Exception
            fullAudioStatus.Text = "Audio playback failed."
            CallError(ex.Message)
        End Try
    End Function

    Private Sub PlayCachedAudioPayload(payload As Byte(), fileType As RobloxCacheFileType)
        If fileType = RobloxCacheFileType.Mp3 Then
            StopPlayback()
            Try
                Dim stream As New MemoryStream(payload, False)
                outputDevice = New WaveOutEvent()
                audioReader = New Mp3FileReader(stream)
                outputDevice.Init(audioReader)
                outputDevice.Volume = CSng(VolumeControl1.Volume) / 100.0F
                outputDevice.Play()
                AddHandler outputDevice.PlaybackStopped, AddressOf OnPlaybackStopped
                trackBarTimeline.Maximum = Math.Max(1, CInt(audioReader.TotalTime.TotalSeconds))
                lblTotalTime.Text = FormatTime(audioReader.TotalTime)
                playbackTimer.Start()
                ChangeVol.Start()
                KeepPlayback0.Stop()
                Playing = True
                SoundPlayerPlayBtn.BackgroundImage = My.Resources.RedPlayButton
            Catch ex As Exception
                output_log.Items.Add("Error playing cached MP3: " & ex.Message)
            End Try
        Else
            PlayOggPayload(payload)
        End If
    End Sub
    Private Sub PlayOggPayload(payload As Byte())
        StopPlayback()
        Try
            Dim stream As New MemoryStream(payload, False)
            outputDevice = New WaveOutEvent()
            audioReader = New VorbisWaveReader(stream, True)
            outputDevice.Init(audioReader)
            outputDevice.Volume = CSng(VolumeControl1.Volume) / 100.0F
            outputDevice.Play()
            AddHandler outputDevice.PlaybackStopped, AddressOf OnPlaybackStopped
            trackBarTimeline.Maximum = Math.Max(1, CInt(audioReader.TotalTime.TotalSeconds))
            lblTotalTime.Text = FormatTime(audioReader.TotalTime)
            playbackTimer.Start()
            ChangeVol.Start()
            KeepPlayback0.Stop()
            Playing = True
            SoundPlayerPlayBtn.BackgroundImage = My.Resources.RedPlayButton
        Catch ex As Exception
            output_log.Items.Add("Error playing cached audio: " & ex.Message)
        End Try
    End Sub
    Private Sub PlayOggFile(filePath As String)
        StopPlayback()

        Try
            If HasMetadata(filePath) Then
                outputDevice = New WaveOutEvent()
                audioReader = New MediaFoundationReader(filePath)
            Else
                outputDevice = New WaveOutEvent()
                audioReader = New VorbisWaveReader(filePath)
            End If


            outputDevice.Init(audioReader)
            outputDevice.Volume = CSng(VolumeControl1.Volume) / 100.0F
            outputDevice.Play()

            AddHandler outputDevice.PlaybackStopped, AddressOf OnPlaybackStopped
            trackBarTimeline.Maximum = CInt(audioReader.TotalTime.TotalSeconds)
            lblTotalTime.Text = FormatTime(audioReader.TotalTime)
            playbackTimer.Start()
            ChangeVol.Start()
            KeepPlayback0.Stop()
            Playing = True
            SoundPlayerPlayBtn.BackgroundImage = My.Resources.RedPlayButton
        Catch ex As Exception
            output_log.Items.Add("Error playing file: " & ex.Message)
        End Try
    End Sub

    Private Function HasMetadata(filePath As String) As Boolean
        Try
            Dim track As New Track(filePath)
            Return Not String.IsNullOrEmpty(track.Artist) OrElse Not String.IsNullOrEmpty(track.Album)
        Catch ex As Exception
            Return False
        End Try
    End Function
    Private Sub OnPlaybackStopped(sender As Object, e As StoppedEventArgs)
        If Not isStopping AndAlso audioReader IsNot Nothing AndAlso audioReader.CurrentTime >= audioReader.TotalTime Then
            Invoke(Sub() StopPlayback())
        End If
    End Sub

    Private Sub StopPlayback()
        KeepPlayback0.Start()
        isStopping = True

        If outputDevice IsNot Nothing Then
            RemoveHandler outputDevice.PlaybackStopped, AddressOf OnPlaybackStopped
            outputDevice.Stop()
            outputDevice.Dispose()
            outputDevice = Nothing
        End If

        If audioReader IsNot Nothing Then
            audioReader.Dispose()
            audioReader = Nothing
        End If
        ChangeVol.Stop()
        playbackTimer.Stop()
        trackBarTimeline.Value = 0
        lblElapsedTime.Text = "00:00"
        lblTotalTime.Text = "00:00"
        Playing = False
        SoundPlayerPlayBtn.BackgroundImage = My.Resources.GreenPlayButton
        isStopping = False
    End Sub


    Private Sub playbackTimer_Tick(sender As Object, e As EventArgs) Handles playbackTimer.Tick
        If audioReader IsNot Nothing AndAlso Not isDragging0 Then
            trackBarTimeline.Value = CInt(audioReader.CurrentTime.TotalSeconds)
            lblElapsedTime.Text = FormatTime(audioReader.CurrentTime)
        End If
    End Sub


    Private Function FormatTime(time As TimeSpan) As String
        Return time.Minutes.ToString("D2") & ":" & time.Seconds.ToString("D2")
    End Function


    Private Sub trackBarTimeline_Scroll(sender As Object, e As EventArgs) Handles trackBarTimeline.Scroll
        If audioReader IsNot Nothing AndAlso Not isStopping Then
            Try
                isDragging0 = True
                Dim newTime As TimeSpan = TimeSpan.FromSeconds(trackBarTimeline.Value)
                lblElapsedTime.Text = FormatTime(newTime)
                If audioReader IsNot Nothing Then

                    audioReader.CurrentTime = newTime
                End If

                isDragging0 = False
            Catch ex As Exception
                output_log.Items.Add("Seek error: " & ex.Message)
            End Try
        End If
    End Sub

    Private Async Sub SoundPlayerPlayBtn_Click(sender As Object, e As EventArgs) Handles SoundPlayerPlayBtn.Click
        If Playing Then
            StopPlayback()
        ElseIf selectedFullAudioEntry IsNot Nothing Then
            Await PlayCachedOggAsync(selectedFullAudioEntry)
        ElseIf Not String.IsNullOrEmpty(SelFile) Then
            PlayOggFile(SelFile)
        End If
    End Sub

    Private Sub KeepPlayback0_Tick(sender As Object, e As EventArgs) Handles KeepPlayback0.Tick
        trackBarTimeline.Value = 0

    End Sub

    Private Sub ChangeVol_Tick(sender As Object, e As EventArgs) Handles ChangeVol.Tick
        If outputDevice IsNot Nothing Then
            outputDevice.Volume = CSng(VolumeControl1.Volume) / 100.0F
        End If
    End Sub


End Class

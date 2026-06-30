Imports System.IO

Public NotInheritable Class BulkExportSummary
    Public Property OutputDirectory As String
    Public Property Exported As Integer
    Public Property Reused As Integer
    Public Property Failed As Integer
    Public Property Skipped As Integer
    Public Property Lines As New List(Of String)()
End Class

''' <summary>
''' Scans the entire Roblox cache and exports every recognised asset into a single destination
''' folder, sorted into one subfolder per asset family.
''' </summary>
Public NotInheritable Class BulkAssetExporter
    Private Sub New()
    End Sub

    Public Shared Function ExportEverything(rootDirectory As String, progress As Action(Of String, Double), Optional combineVideosToSingleFile As Boolean = False) As BulkExportSummary
        If String.IsNullOrWhiteSpace(rootDirectory) Then Throw New ArgumentException("A destination folder is required.", NameOf(rootDirectory))
        Directory.CreateDirectory(rootDirectory)

        Dim summary As New BulkExportSummary With {.OutputDirectory = rootDirectory}

        ' --- Scan phase (0% - 30%) ---------------------------------------------------------------
        Report(progress, "Scanning Roblox cache...", 1)
        Dim cacheAssets = RobloxCacheAssetExtractor.Scan(
            Sub(p) Report(progress, "Scanning cache...", Scale(p.Current, p.Total, 1, 15)))
        AssetNameStore.ApplySavedNames(cacheAssets)

        Report(progress, "Scanning cached videos...", 15)
        Dim videos = RobloxVideoExtractor.Scan(New Progress(Of VideoScanProgress)())
        AssetNameStore.ApplySavedNames(videos)

        Report(progress, "Scanning meshes...", 18)
        Dim meshes = RobloxMeshExtractor.Scan(New Progress(Of MeshScanProgress)())
        AssetNameStore.ApplySavedNames(meshes)

        Report(progress, "Scanning thumbnails, fonts, and metadata...", 24)
        Dim thumbnails = RobloxSupplementalCacheExtractor.ScanThumbnails(Nothing)
        Dim fonts = RobloxSupplementalCacheExtractor.ScanFonts(Nothing)
        Dim metadata = RobloxSupplementalCacheExtractor.ScanMetadata(Nothing)
        AssetNameStore.ApplySavedNames(thumbnails)
        AssetNameStore.ApplySavedNames(fonts)
        AssetNameStore.ApplySavedNames(metadata)

        Dim audio = cacheAssets.Where(AddressOf IsAudio).ToList()
        Dim images = cacheAssets.Where(AddressOf IsImage).ToList()
        Dim cacheFiles = cacheAssets.Where(AddressOf IsCacheFile).ToList()
        Dim exportableMeshes = meshes.Where(Function(m) m.CanExport).ToList()
        summary.Skipped = meshes.Count - exportableMeshes.Count

        Dim grandTotal = audio.Count + videos.Count + images.Count + cacheFiles.Count + exportableMeshes.Count +
                         thumbnails.Count + fonts.Count + metadata.Count
        If grandTotal = 0 Then
            Report(progress, "No exportable assets were found in the cache.", 100)
            Return summary
        End If

        ' --- Export phase (30% - 100%) -----------------------------------------------------------
        Dim done As Integer = 0
        Dim mapExport = Function(stage As String) As Action(Of Integer, Integer)
                            Dim baseDone = done
                            Return Sub(current, total)
                                       Report(progress, stage, Scale(baseDone + current, grandTotal, 30, 100))
                                   End Sub
                        End Function

        ExportCacheGroup(summary, audio, Path.Combine(rootDirectory, "Audio"), "Audio", mapExport, done)
        ExportVideoGroup(summary, videos, Path.Combine(rootDirectory, "Videos"), progress, grandTotal, done, combineVideosToSingleFile)
        ExportCacheGroup(summary, images, Path.Combine(rootDirectory, "Images"), "Images", mapExport, done)
        ExportCacheGroup(summary, cacheFiles, Path.Combine(rootDirectory, "Cache Files"), "Cache Files", mapExport, done, separateTypeFolders:=True)
        ExportMeshGroup(summary, exportableMeshes, Path.Combine(rootDirectory, "Meshes"), progress, grandTotal, done)
        ExportSupplementalGroup(summary, thumbnails, Path.Combine(rootDirectory, "Thumbnails"), "Thumbnails", mapExport, done)
        ExportSupplementalGroup(summary, fonts, Path.Combine(rootDirectory, "Fonts"), "Fonts", mapExport, done)
        ExportSupplementalGroup(summary, metadata, Path.Combine(rootDirectory, "Metadata"), "Metadata", mapExport, done)

        Report(progress, "Bulk export complete.", 100)
        Return summary
    End Function

    Private Shared Sub ExportCacheGroup(summary As BulkExportSummary, entries As List(Of RobloxCacheAssetEntry), folder As String, label As String,
                                        mapExport As Func(Of String, Action(Of Integer, Integer)), ByRef done As Integer, Optional separateTypeFolders As Boolean = False)
        If entries.Count = 0 Then Return
        Dim result = RobloxCacheAssetExtractor.ExportMany(entries, folder, mapExport($"Exporting {label.ToLowerInvariant()}..."), separateTypeFolders)
        Accumulate(summary, label, result.Exported, result.Reused, result.Failed)
        done += entries.Count
    End Sub

    Private Shared Sub ExportVideoGroup(summary As BulkExportSummary, videos As List(Of RobloxVideoEntry), folder As String,
                                       progress As Action(Of String, Double), grandTotal As Integer, ByRef done As Integer, singleFile As Boolean)
        If videos.Count = 0 Then Return
        Directory.CreateDirectory(folder)
        Dim usedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim exported As Integer
        Dim reused As Integer
        Dim failed As Integer
        Dim baseDone = done
        For index = 0 To videos.Count - 1
            Dim entry = videos(index)
            Try
                If Not entry.IsComplete Then
                    failed += 1
                ElseIf singleFile Then
                    Dim outputPath = Path.Combine(folder, AssetNameStore.GetBatchBaseName(entry, usedNames) & ".webm")
                    If File.Exists(outputPath) AndAlso New FileInfo(outputPath).Length > 0 Then
                        reused += 1
                    Else
                        RobloxVideoExtractor.ExportSingleFile(entry, outputPath)
                        exported += 1
                    End If
                Else
                    Dim result = RobloxVideoExtractor.Export(entry, folder, AssetNameStore.GetBatchBaseName(entry, usedNames))
                    If result.WasReused Then reused += 1 Else exported += 1
                End If
            Catch
                failed += 1
            Finally
                Report(progress, "Exporting videos...", Scale(baseDone + index + 1, grandTotal, 30, 100))
            End Try
        Next
        Accumulate(summary, "Videos", exported, reused, failed)
        done += videos.Count
    End Sub
    Private Shared Sub ExportSupplementalGroup(summary As BulkExportSummary, entries As List(Of SupplementalCacheEntry), folder As String, label As String,
                                               mapExport As Func(Of String, Action(Of Integer, Integer)), ByRef done As Integer)
        If entries.Count = 0 Then Return
        Dim result = RobloxSupplementalCacheExtractor.ExportMany(entries, folder, mapExport($"Exporting {label.ToLowerInvariant()}..."))
        Accumulate(summary, label, result.Exported, result.Reused, result.Failed)
        done += entries.Count
    End Sub

    Private Shared Sub ExportMeshGroup(summary As BulkExportSummary, meshes As List(Of MeshCacheEntry), folder As String,
                                       progress As Action(Of String, Double), grandTotal As Integer, ByRef done As Integer)
        If meshes.Count = 0 Then Return
        Directory.CreateDirectory(folder)
        Dim usedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim exported = 0
        Dim reused = 0
        Dim failed = 0
        Dim baseDone = done
        For i = 0 To meshes.Count - 1
            Dim entry = meshes(i)
            Try
                Dim outputPath = Path.Combine(folder, AssetNameStore.GetBatchBaseName(entry, usedNames) & ".obj")
                If File.Exists(outputPath) AndAlso New FileInfo(outputPath).Length > 0 Then
                    reused += 1
                Else
                    RobloxMeshExtractor.Export(entry, outputPath)
                    exported += 1
                End If
            Catch
                failed += 1
            Finally
                Report(progress, "Exporting meshes...", Scale(baseDone + i + 1, grandTotal, 30, 100))
            End Try
        Next
        Accumulate(summary, "Meshes", exported, reused, failed)
        done += meshes.Count
    End Sub

    Private Shared Sub Accumulate(summary As BulkExportSummary, label As String, exported As Integer, reused As Integer, failed As Integer)
        summary.Exported += exported
        summary.Reused += reused
        summary.Failed += failed
        Dim line = $"{label}: {exported:N0} exported"
        If reused > 0 Then line &= $", {reused:N0} reused"
        If failed > 0 Then line &= $", {failed:N0} failed"
        summary.Lines.Add(line)
    End Sub

    Private Shared Sub Report(progress As Action(Of String, Double), stage As String, percent As Double)
        If progress IsNot Nothing Then progress(stage, Math.Max(0, Math.Min(100, percent)))
    End Sub

    Private Shared Function Scale(current As Integer, total As Integer, lower As Double, upper As Double) As Double
        If total <= 0 Then Return upper
        Return lower + (upper - lower) * (current / total)
    End Function

    Private Shared Function IsAudio(entry As RobloxCacheAssetEntry) As Boolean
        Return entry.FileType = RobloxCacheFileType.Ogg OrElse entry.FileType = RobloxCacheFileType.Mp3
    End Function

    Private Shared Function IsImage(entry As RobloxCacheAssetEntry) As Boolean
        Select Case entry.FileType
            Case RobloxCacheFileType.Png, RobloxCacheFileType.Jpeg, RobloxCacheFileType.Bmp, RobloxCacheFileType.WebP
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function IsCacheFile(entry As RobloxCacheAssetEntry) As Boolean
        Select Case entry.FileType
            Case RobloxCacheFileType.RbxmBinary, RobloxCacheFileType.RbxmXml, RobloxCacheFileType.Ktx1, RobloxCacheFileType.Ktx2
                Return True
            Case Else
                Return False
        End Select
    End Function
End Class

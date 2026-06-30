Imports System.Data.SQLite
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public NotInheritable Class VideoSegmentEntry
    Public Property Hash As String
    Public Property Size As Long
    Public Property IsInline As Boolean
    Public Property PayloadOffset As Integer
    Public Property PayloadLength As Integer
    Public Property FileName As String
    Public Property DurationSeconds As Double
    Public Property SourceKey As String
End Class

Public NotInheritable Class RobloxVideoEntry
    Implements IRenamableAsset

    Public Property PlaylistHash As String
    Public Property SourcePath As String
    Public Property Resolution As String
    Public Property DurationSeconds As Double
    Public Property Size As Long
    Public Property ExpectedSegments As Integer
    Public Property Segments As New List(Of VideoSegmentEntry)()
    Public Property CustomName As String Implements IRenamableAsset.CustomName
    Public Property ContentSha256 As String Implements IRenamableAsset.ContentSha256

    Public ReadOnly Property CacheKey As String Implements IRenamableAsset.CacheKey
        Get
            Return PlaylistHash
        End Get
    End Property

    Public ReadOnly Property FriendlyName As String Implements IRenamableAsset.FriendlyName
        Get
            If Not String.IsNullOrWhiteSpace(CustomName) Then Return CustomName
            Return $"{Resolution} video - {AppServices.SafePrefix(PlaylistHash)}"
        End Get
    End Property

    Public ReadOnly Property ExportBaseName As String Implements IRenamableAsset.ExportBaseName
        Get
            If Not String.IsNullOrWhiteSpace(CustomName) Then Return AssetNameStore.MakeSafeFileName(CustomName, PlaylistHash)
            Return AssetNameStore.MakeSafeFileName($"video_{Resolution}_{AppServices.SafePrefix(PlaylistHash)}", PlaylistHash)
        End Get
    End Property

    Public ReadOnly Property IsComplete As Boolean
        Get
            Return ExpectedSegments > 0 AndAlso Segments.Count = ExpectedSegments
        End Get
    End Property

    Public ReadOnly Property SegmentLabel As String
        Get
            Return $"{Segments.Count:N0} / {ExpectedSegments:N0}"
        End Get
    End Property

    Public ReadOnly Property DurationLabel As String
        Get
            Dim value = TimeSpan.FromSeconds(Math.Max(0, DurationSeconds))
            Return If(value.TotalHours >= 1, value.ToString("h\:mm\:ss"), value.ToString("m\:ss"))
        End Get
    End Property

    Public ReadOnly Property StatusLabel As String
        Get
            Return If(IsComplete, "Ready", "Incomplete")
        End Get
    End Property
End Class

Public NotInheritable Class VideoScanProgress
    Public Property Current As Integer
    Public Property Total As Integer
    Public Property Found As Integer
End Class

Public NotInheritable Class VideoPreviewSegment
    Public Property Path As String
    Public Property DurationSeconds As Double
End Class

Public NotInheritable Class VideoPreviewPackage
    Public Property DirectoryPath As String
    Public Property Segments As New List(Of VideoPreviewSegment)()
End Class

Public NotInheritable Class VideoExportResult
    Public Property DirectoryPath As String
    Public Property FilesWritten As Integer
    Public Property WasReused As Boolean
End Class

Public NotInheritable Class RobloxVideoExtractor
    Private Const EnvelopeSize As Integer = 37
    Private Const PrefixSize As Integer = 65536

    Private NotInheritable Class StoredRecord
        Public Property Hash As String
        Public Property Size As Long
        Public Property IsInline As Boolean
        Public Property Key As String
        Public Property ContentType As String
        Public Property PayloadOffset As Integer
        Public Property PayloadLength As Integer
    End Class

    Private NotInheritable Class PlaylistSegmentReference
        Public Property Key As String
        Public Property FileName As String
        Public Property DurationSeconds As Double
    End Class

    Private NotInheritable Class EbmlElement
        Public Property Id As ULong
        Public Property Offset As Integer
        Public Property HeaderLength As Integer
        Public Property DataOffset As Integer
        Public Property DataLength As Integer
        Public ReadOnly Property EndOffset As Integer
            Get
                Return DataOffset + DataLength
            End Get
        End Property
        Public ReadOnly Property TotalLength As Integer
            Get
                Return HeaderLength + DataLength
            End Get
        End Property
    End Class
    Private Sub New()
    End Sub

    Public Shared Function Scan(progress As IProgress(Of VideoScanProgress)) As List(Of RobloxVideoEntry)
        If Not File.Exists(AppServices.DatabasePath) Then Throw New FileNotFoundException("Roblox cache database was not found.", AppServices.DatabasePath)

        Dim playlists As New List(Of StoredRecord)()
        Dim webmRecords As New Dictionary(Of String, StoredRecord)(StringComparer.OrdinalIgnoreCase)
        Dim found As Integer

        Using connection As New SQLiteConnection($"Data Source={AppServices.DatabasePath};Read Only=True;")
            connection.Open()
            Dim total As Integer
            Using countCommand As New SQLiteCommand("SELECT COUNT(*) FROM files", connection)
                total = Convert.ToInt32(countCommand.ExecuteScalar())
            End Using

            Using command As New SQLiteCommand("SELECT id, size, content FROM files ORDER BY id", connection)
                Using reader = command.ExecuteReader()
                    Dim current As Integer
                    While reader.Read()
                        current += 1
                        Dim hash = Convert.ToHexString(DirectCast(reader("id"), Byte())).ToLowerInvariant()
                        Dim size = reader.GetInt64(1)
                        Dim isInline = Not reader.IsDBNull(2)
                        Dim prefix As Byte()
                        Dim recordLength As Long
                        If isInline Then
                            Dim wrapped = DirectCast(reader("content"), Byte())
                            prefix = wrapped
                            recordLength = wrapped.LongLength
                        Else
                            Dim path = ExternalPath(hash)
                            If path Is Nothing Then
                                Report(progress, current, total, found)
                                Continue While
                            End If
                            Using stream = OpenShared(path)
                                recordLength = stream.Length
                                Dim count = CInt(Math.Min(PrefixSize, stream.Length))
                                prefix = New Byte(Math.Max(0, count - 1)) {}
                                If count > 0 Then ReadExactly(stream, prefix, 0, count)
                            End Using
                        End If

                        Dim parsed As StoredRecord = Nothing
                        If TryParseHttpRecord(prefix, recordLength, hash, size, isInline, parsed) Then
                            Dim normalized = NormalizeKey(parsed.Key)
                            If IsPlaylist(parsed) Then
                                playlists.Add(parsed)
                            ElseIf IsWebM(parsed) Then
                                webmRecords(normalized) = parsed
                            End If
                        End If
                        Report(progress, current, total, found)
                    End While
                End Using
            End Using
        End Using

        Dim result As New List(Of RobloxVideoEntry)()
        For Each playlist In playlists
            Try
                Dim playlistBytes = ReadPayload(playlist)
                Dim references = ParsePlaylist(playlist.Key, playlistBytes)
                If references.Count = 0 Then Continue For

                Dim entry As New RobloxVideoEntry With {
                    .PlaylistHash = playlist.Hash,
                    .SourcePath = NormalizeKey(playlist.Key),
                    .Resolution = ResolutionFromKey(playlist.Key),
                    .ExpectedSegments = references.Count}

                For Each reference In references
                    Dim segmentRecord As StoredRecord = Nothing
                    If Not webmRecords.TryGetValue(reference.Key, segmentRecord) Then Continue For
                    entry.Segments.Add(New VideoSegmentEntry With {
                        .Hash = segmentRecord.Hash,
                        .Size = segmentRecord.Size,
                        .IsInline = segmentRecord.IsInline,
                        .PayloadOffset = segmentRecord.PayloadOffset,
                        .PayloadLength = segmentRecord.PayloadLength,
                        .FileName = reference.FileName,
                        .DurationSeconds = reference.DurationSeconds,
                        .SourceKey = reference.Key})
                    entry.DurationSeconds += reference.DurationSeconds
                    entry.Size += segmentRecord.PayloadLength
                Next
                result.Add(entry)
                found += 1
            Catch
                ' A malformed or concurrently evicted playlist is skipped without aborting the scan.
            End Try
        Next

        result = result.OrderByDescending(Function(item) item.DurationSeconds).ThenBy(Function(item) item.PlaylistHash).ToList()
        AssetNameStore.ApplySavedNames(result)
        Return result
    End Function

    Public Shared Function ReadFingerprintPayload(entry As RobloxVideoEntry) As Byte()
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        Dim identity = entry.PlaylistHash & "|" & String.Join("|", entry.Segments.Select(Function(segment) segment.Hash & ":" & segment.PayloadLength.ToString(CultureInfo.InvariantCulture)))
        Return Encoding.UTF8.GetBytes(identity)
    End Function

    Public Shared Function ReadSegmentPayload(segment As VideoSegmentEntry) As Byte()
        If segment Is Nothing Then Throw New ArgumentNullException(NameOf(segment))
        Dim record As New StoredRecord With {
            .Hash = segment.Hash,
            .Size = segment.Size,
            .IsInline = segment.IsInline,
            .PayloadOffset = segment.PayloadOffset,
            .PayloadLength = segment.PayloadLength}
        Return ReadPayload(record)
    End Function

    Public Shared Function CreatePreviewPackage(entry As RobloxVideoEntry) As VideoPreviewPackage
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        If Not entry.IsComplete Then Throw New InvalidDataException("The cached video is incomplete; one or more WebM segments are missing.")
        Dim stagingDirectory = Path.Combine(Path.GetTempPath(), "RBXAssetExtractor", "VideoPreviews", Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(stagingDirectory)
        Dim package As New VideoPreviewPackage With {.DirectoryPath = stagingDirectory}
        Try
            For index = 0 To entry.Segments.Count - 1
                Dim segment = entry.Segments(index)
                Dim fileName = $"{index:D4}.webm"
                Dim segmentPath = Path.Combine(stagingDirectory, fileName)
                File.WriteAllBytes(segmentPath, ReadSegmentPayload(segment))
                package.Segments.Add(New VideoPreviewSegment With {.Path = segmentPath, .DurationSeconds = segment.DurationSeconds})
            Next
            WritePreviewClient(stagingDirectory)
            Return package
        Catch
            Try
                Directory.Delete(stagingDirectory, recursive:=True)
            Catch
            End Try
            Throw
        End Try
    End Function

    Public Shared Function Export(entry As RobloxVideoEntry, rootDirectory As String, Optional baseName As String = Nothing) As VideoExportResult
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        If Not entry.IsComplete Then Throw New InvalidDataException("The cached video is incomplete; one or more WebM segments are missing.")
        If String.IsNullOrWhiteSpace(rootDirectory) Then Throw New ArgumentException("An export folder is required.", NameOf(rootDirectory))

        Dim targetName = AssetNameStore.MakeSafeFileName(If(String.IsNullOrWhiteSpace(baseName), entry.ExportBaseName, baseName), entry.PlaylistHash)
        Dim targetDirectory = Path.Combine(rootDirectory, targetName)
        Dim playlistPath = Path.Combine(targetDirectory, "playlist.m3u8")
        Dim reused = File.Exists(playlistPath) AndAlso New FileInfo(playlistPath).Length > 0
        Directory.CreateDirectory(targetDirectory)

        Dim targetDuration = Math.Max(1, CInt(Math.Ceiling(entry.Segments.Max(Function(segment) segment.DurationSeconds))))
        Dim playlist As New StringBuilder()
        playlist.AppendLine("#EXTM3U")
        playlist.AppendLine("#EXT-X-VERSION:11")
        playlist.AppendLine("#EXT-X-MEDIA-SEQUENCE:0")
        playlist.AppendLine($"#EXT-X-TARGETDURATION:{targetDuration}")

        Dim written As Integer
        For index = 0 To entry.Segments.Count - 1
            Dim segment = entry.Segments(index)
            Dim fileName = $"{index:D4}.webm"
            Dim outputPath = Path.Combine(targetDirectory, fileName)
            If Not File.Exists(outputPath) OrElse New FileInfo(outputPath).Length <> segment.PayloadLength Then
                File.WriteAllBytes(outputPath, ReadSegmentPayload(segment))
                written += 1
            End If
            playlist.AppendLine($"#EXTINF:{segment.DurationSeconds.ToString("0.000000", CultureInfo.InvariantCulture)},")
            playlist.AppendLine(fileName)
        Next
        playlist.AppendLine("#EXT-X-ENDLIST")
        File.WriteAllText(playlistPath, playlist.ToString(), New UTF8Encoding(False))
        written += 1
        Return New VideoExportResult With {.DirectoryPath = targetDirectory, .FilesWritten = written, .WasReused = reused}
    End Function

    Public Shared Function ExportSingleFile(entry As RobloxVideoEntry, outputPath As String) As Long
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        If Not entry.IsComplete Then Throw New InvalidDataException("The cached video is incomplete; one or more WebM segments are missing.")
        If String.IsNullOrWhiteSpace(outputPath) Then Throw New ArgumentException("An output file is required.", NameOf(outputPath))

        Dim fullOutputPath = Path.GetFullPath(outputPath)
        Dim outputDirectory = Path.GetDirectoryName(fullOutputPath)
        If String.IsNullOrWhiteSpace(outputDirectory) Then Throw New ArgumentException("The output file must have a parent directory.", NameOf(outputPath))
        Directory.CreateDirectory(outputDirectory)

        Dim firstPayload = ReadSegmentPayload(entry.Segments(0))
        Dim firstEbml As EbmlElement = Nothing
        Dim firstChildren As List(Of EbmlElement) = Nothing
        ParseWebM(firstPayload, firstEbml, firstChildren)
        Dim info = RequireElement(firstChildren, &H1549A966UL, "WebM Info")
        Dim tracks = RequireElement(firstChildren, &H1654AE6BUL, "WebM Tracks")
        Dim tags = firstChildren.FirstOrDefault(Function(item) item.Id = &H1254C367UL)

        Dim fullDurationMilliseconds = ReadDurationMilliseconds(firstPayload, info)
        For index = 1 To entry.Segments.Count - 1
            Dim payload = ReadSegmentPayload(entry.Segments(index))
            Dim ignoredEbml As EbmlElement = Nothing
            Dim children As List(Of EbmlElement) = Nothing
            ParseWebM(payload, ignoredEbml, children)
            fullDurationMilliseconds = Math.Max(fullDurationMilliseconds, ReadDurationMilliseconds(payload, RequireElement(children, &H1549A966UL, "WebM Info")))
        Next
        If fullDurationMilliseconds <= 0 Then fullDurationMilliseconds = entry.DurationSeconds * 1000.0

        Dim patchedInfo = ElementBytes(firstPayload, info)
        PatchDurationMilliseconds(patchedInfo, fullDurationMilliseconds)
        Dim temporaryPath = fullOutputPath & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Try
            Using output As New FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                output.Write(firstPayload, firstEbml.Offset, firstEbml.TotalLength)
                output.Write(New Byte() {&H18, &H53, &H80, &H67, &H1, &HFF, &HFF, &HFF, &HFF, &HFF, &HFF, &HFF})
                output.Write(patchedInfo)
                output.Write(firstPayload, tracks.Offset, tracks.TotalLength)
                If tags IsNot Nothing Then output.Write(firstPayload, tags.Offset, tags.TotalLength)

                Dim lastClusterTimecode As Long = -1
                Dim clustersWritten As Integer
                For index = 0 To entry.Segments.Count - 1
                    Dim payload = If(index = 0, firstPayload, ReadSegmentPayload(entry.Segments(index)))
                    Dim ignoredEbml As EbmlElement = Nothing
                    Dim children As List(Of EbmlElement) = Nothing
                    ParseWebM(payload, ignoredEbml, children)
                    For Each cluster In children.Where(Function(item) item.Id = &H1F43B675UL)
                        Dim timecode = ReadClusterTimecode(payload, cluster)
                        If timecode < lastClusterTimecode Then Throw New InvalidDataException("These WebM segments restart their internal timestamps and cannot be losslessly merged.")
                        lastClusterTimecode = timecode
                        output.Write(payload, cluster.Offset, cluster.TotalLength)
                        clustersWritten += 1
                    Next
                Next
                If clustersWritten = 0 Then Throw New InvalidDataException("The cached video does not contain any WebM clusters.")
            End Using
            File.Move(temporaryPath, fullOutputPath, overwrite:=True)
            Return New FileInfo(fullOutputPath).Length
        Catch
            Try
                If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
            Catch
            End Try
            Throw
        End Try
    End Function

    Private Shared Sub ParseWebM(data As Byte(), ByRef ebml As EbmlElement, ByRef segmentChildren As List(Of EbmlElement))
        If data Is Nothing OrElse data.Length < 16 Then Throw New InvalidDataException("The cached WebM segment is truncated.")
        ebml = ReadEbmlElement(data, 0, data.Length)
        If ebml.Id <> &H1A45DFA3UL Then Throw New InvalidDataException("The cached video segment does not have a WebM EBML header.")
        Dim segment = ReadEbmlElement(data, ebml.EndOffset, data.Length)
        If segment.Id <> &H18538067UL Then Throw New InvalidDataException("The cached video segment does not contain a WebM Segment element.")
        segmentChildren = ReadEbmlChildren(data, segment)
    End Sub

    Private Shared Function ReadEbmlChildren(data As Byte(), parent As EbmlElement) As List(Of EbmlElement)
        Dim result As New List(Of EbmlElement)()
        Dim position = parent.DataOffset
        While position < parent.EndOffset
            Dim child = ReadEbmlElement(data, position, parent.EndOffset)
            If child.EndOffset <= position Then Throw New InvalidDataException("The WebM element table is invalid.")
            result.Add(child)
            position = child.EndOffset
        End While
        Return result
    End Function

    Private Shared Function ReadEbmlElement(data As Byte(), offset As Integer, limit As Integer) As EbmlElement
        Dim position = offset
        Dim idLength As Integer
        Dim ignoredUnknown As Boolean
        Dim id = ReadEbmlVint(data, position, limit, identifier:=True, idLength, ignoredUnknown)
        Dim sizeLength As Integer
        Dim unknownSize As Boolean
        Dim encodedSize = ReadEbmlVint(data, position, limit, identifier:=False, sizeLength, unknownSize)
        Dim dataLength As Long = If(unknownSize, CLng(limit - position), CheckedLong(encodedSize))
        If dataLength < 0 OrElse position + dataLength > limit Then Throw New InvalidDataException("A WebM element extends past the cached segment boundary.")
        Return New EbmlElement With {
            .Id = id,
            .Offset = offset,
            .HeaderLength = idLength + sizeLength,
            .DataOffset = position,
            .DataLength = CInt(dataLength)}
    End Function

    Private Shared Function ReadEbmlVint(data As Byte(), ByRef position As Integer, limit As Integer, identifier As Boolean, ByRef encodedLength As Integer, ByRef unknown As Boolean) As ULong
        If position < 0 OrElse position >= limit Then Throw New InvalidDataException("A WebM variable-length integer is truncated.")
        Dim first = CInt(data(position))
        Dim mask = &H80
        encodedLength = 1
        While encodedLength <= 8 AndAlso (first And mask) = 0
            mask >>= 1
            encodedLength += 1
        End While
        If encodedLength > 8 OrElse position + encodedLength > limit Then Throw New InvalidDataException("A WebM variable-length integer is invalid.")

        Dim value As ULong
        If identifier Then
            For index = 0 To encodedLength - 1
                value = (value << 8) Or data(position + index)
            Next
            unknown = False
        Else
            value = CULng(first And (mask - 1))
            For index = 1 To encodedLength - 1
                value = (value << 8) Or data(position + index)
            Next
            unknown = value = ((1UL << (7 * encodedLength)) - 1UL)
        End If
        position += encodedLength
        Return value
    End Function

    Private Shared Function CheckedLong(value As ULong) As Long
        If value > CULng(Long.MaxValue) Then Throw New InvalidDataException("A WebM element is too large.")
        Return CLng(value)
    End Function

    Private Shared Function RequireElement(elements As IEnumerable(Of EbmlElement), id As ULong, label As String) As EbmlElement
        Dim result = elements.FirstOrDefault(Function(item) item.Id = id)
        If result Is Nothing Then Throw New InvalidDataException($"The cached segment is missing its {label} element.")
        Return result
    End Function

    Private Shared Function ElementBytes(data As Byte(), element As EbmlElement) As Byte()
        Dim result(element.TotalLength - 1) As Byte
        Buffer.BlockCopy(data, element.Offset, result, 0, result.Length)
        Return result
    End Function

    Private Shared Function ReadDurationMilliseconds(data As Byte(), info As EbmlElement) As Double
        Dim duration = ReadEbmlChildren(data, info).FirstOrDefault(Function(item) item.Id = &H4489UL)
        If duration Is Nothing Then Return 0
        Return ReadBigEndianFloat(data, duration.DataOffset, duration.DataLength)
    End Function

    Private Shared Sub PatchDurationMilliseconds(infoBytes As Byte(), value As Double)
        Dim info = ReadEbmlElement(infoBytes, 0, infoBytes.Length)
        Dim duration = ReadEbmlChildren(infoBytes, info).FirstOrDefault(Function(item) item.Id = &H4489UL)
        If duration Is Nothing Then Return
        WriteBigEndianFloat(infoBytes, duration.DataOffset, duration.DataLength, value)
    End Sub

    Private Shared Function ReadClusterTimecode(data As Byte(), cluster As EbmlElement) As Long
        Dim timecode = ReadEbmlChildren(data, cluster).FirstOrDefault(Function(item) item.Id = &HE7UL)
        If timecode Is Nothing OrElse timecode.DataLength < 1 OrElse timecode.DataLength > 8 Then Throw New InvalidDataException("A WebM cluster is missing a valid timecode.")
        Dim value As ULong
        For index = 0 To timecode.DataLength - 1
            value = (value << 8) Or data(timecode.DataOffset + index)
        Next
        Return CheckedLong(value)
    End Function

    Private Shared Function ReadBigEndianFloat(data As Byte(), offset As Integer, length As Integer) As Double
        If length <> 4 AndAlso length <> 8 Then Throw New InvalidDataException("The WebM duration has an unsupported floating-point size.")
        Dim value(length - 1) As Byte
        Buffer.BlockCopy(data, offset, value, 0, length)
        If BitConverter.IsLittleEndian Then Array.Reverse(value)
        Return If(length = 8, BitConverter.ToDouble(value, 0), CDbl(BitConverter.ToSingle(value, 0)))
    End Function

    Private Shared Sub WriteBigEndianFloat(data As Byte(), offset As Integer, length As Integer, value As Double)
        Dim encoded = If(length = 8, BitConverter.GetBytes(value), BitConverter.GetBytes(CSng(value)))
        If BitConverter.IsLittleEndian Then Array.Reverse(encoded)
        Buffer.BlockCopy(encoded, 0, data, offset, length)
    End Sub
    Private Shared Sub WritePreviewClient(directoryPath As String)
        Dim html = String.Join(vbLf, {
            "<!doctype html>",
            "<html lang=""en""><head><meta charset=""utf-8"">",
            "<meta http-equiv=""Content-Security-Policy"" content=""default-src 'none'; script-src 'self'; style-src 'self'; media-src 'self'; img-src 'none'; connect-src 'none'; object-src 'none'; base-uri 'none'; form-action 'none'"">",
            "<meta name=""viewport"" content=""width=device-width,initial-scale=1"">",
            "<link rel=""stylesheet"" href=""player.css""></head>",
            "<body><video id=""video"" playsinline></video><script src=""player.js""></script></body></html>"})
        Dim css = "html,body{width:100%;height:100%;margin:0;overflow:hidden;background:#030508}body{display:grid;place-items:center}video{width:100%;height:100%;object-fit:contain;background:#030508}"
        Dim script = String.Join(vbLf, {
            "'use strict';",
            "const video=document.getElementById('video');",
            "let index=0,pending=0,expected=0,origin=0,shouldPlay=false,lastSent=0;",
            "const send=(value)=>window.chrome.webview.postMessage(value);",
            "const playableStart=()=>{let rangeStart=0;try{if(video.seekable.length)rangeStart=video.seekable.start(0);else if(video.buffered.length)rangeStart=video.buffered.start(0);}catch{}const duration=Number.isFinite(video.duration)?video.duration:0;const inferred=Math.max(0,duration-expected);return Math.max(rangeStart,inferred>Math.max(.25,expected*.5)?inferred:0);};",
            "video.addEventListener('loadedmetadata',()=>{origin=playableStart();const playable=Math.max(0,(video.duration||origin+expected)-origin);video.currentTime=origin+Math.max(0,Math.min(playable,pending));send({type:'opened',index,duration:playable,width:video.videoWidth,height:video.videoHeight});if(shouldPlay)video.play().catch(error=>send({type:'error',index,message:error.message}));});",
            "video.addEventListener('timeupdate',()=>{const now=performance.now();if(now-lastSent>100){lastSent=now;send({type:'time',index,seconds:Math.max(0,(video.currentTime||0)-origin)});}});",
            "video.addEventListener('ended',()=>send({type:'ended',index}));",
            "video.addEventListener('error',()=>send({type:'error',index,message:video.error?video.error.message:'WebM playback failed'}));",
            "window.chrome.webview.addEventListener('message',event=>{const m=event.data||{};switch(m.action){case 'open':index=m.index||0;pending=m.position||0;expected=Math.max(0,m.duration||0);origin=0;shouldPlay=!!m.play;video.src=String(index).padStart(4,'0')+'.webm';video.load();break;case 'play':shouldPlay=true;video.play().catch(error=>send({type:'error',index,message:error.message}));break;case 'pause':shouldPlay=false;video.pause();break;case 'seek':{const playable=Math.max(0,(video.duration||origin+m.position)-origin);video.currentTime=origin+Math.max(0,Math.min(playable,m.position||0));break;}case 'volume':video.volume=Math.max(0,Math.min(1,m.value));break;}});"})
        File.WriteAllText(Path.Combine(directoryPath, "player.html"), html, New UTF8Encoding(False))
        File.WriteAllText(Path.Combine(directoryPath, "player.css"), css, New UTF8Encoding(False))
        File.WriteAllText(Path.Combine(directoryPath, "player.js"), script, New UTF8Encoding(False))
    End Sub
    Private Shared Function TryParseHttpRecord(bytes As Byte(), recordLength As Long, hash As String, size As Long, isInline As Boolean, ByRef record As StoredRecord) As Boolean
        record = Nothing
        If bytes Is Nothing OrElse bytes.Length < EnvelopeSize + 25 Then Return False
        Dim start = If(StartsAscii(bytes, 0, "RBXH"), EnvelopeSize, 0)
        Dim nullIndex = Array.IndexOf(bytes, CByte(0), start)
        If nullIndex < start OrElse nullIndex + 25 > bytes.Length Then Return False
        Dim metadata = nullIndex + 1
        Dim headerLength = BitConverter.ToUInt32(bytes, metadata + 4)
        Dim declaredLength = BitConverter.ToUInt32(bytes, metadata + 12)
        Dim calculated = CLng(metadata) + 24L + headerLength
        If calculated > bytes.Length OrElse declaredLength = 0 OrElse calculated + declaredLength > recordLength Then Return False

        Dim headers = Encoding.Latin1.GetString(bytes, metadata + 24, CInt(headerLength))
        Dim match = Regex.Match(headers, "content-type\s*:\s*([^\r\n\0]+)", RegexOptions.IgnoreCase)
        record = New StoredRecord With {
            .Hash = hash,
            .Size = size,
            .IsInline = isInline,
            .Key = Encoding.UTF8.GetString(bytes, start, nullIndex - start),
            .ContentType = If(match.Success, match.Groups(1).Value.Trim(), String.Empty),
            .PayloadOffset = CInt(calculated),
            .PayloadLength = CInt(declaredLength)}
        Return True
    End Function

    Private Shared Function ParsePlaylist(playlistKey As String, payload As Byte()) As List(Of PlaylistSegmentReference)
        Dim text = Encoding.UTF8.GetString(payload)
        If Not text.TrimStart().StartsWith("#EXTM3U", StringComparison.Ordinal) Then Return New List(Of PlaylistSegmentReference)()
        Dim lines = text.Replace(vbCr, String.Empty).Split(ControlChars.Lf)
        Dim result As New List(Of PlaylistSegmentReference)()
        Dim duration As Double?
        For Each rawLine In lines
            Dim line = rawLine.Trim()
            If line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase) Then
                Dim value = line.Substring(8)
                Dim comma = value.IndexOf(","c)
                If comma >= 0 Then value = value.Substring(0, comma)
                Dim parsed As Double
                duration = If(Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, parsed), parsed, 0)
            ElseIf duration.HasValue AndAlso line.Length > 0 AndAlso Not line.StartsWith("#", StringComparison.Ordinal) Then
                Dim cleanLine = StripQuery(line)
                result.Add(New PlaylistSegmentReference With {
                    .Key = ResolveRelativeKey(playlistKey, cleanLine),
                    .FileName = Path.GetFileName(cleanLine.Replace("/"c, Path.DirectorySeparatorChar)),
                    .DurationSeconds = Math.Max(0, duration.Value)})
                duration = Nothing
            End If
        Next
        Return result
    End Function

    Private Shared Function IsPlaylist(record As StoredRecord) As Boolean
        Return record.ContentType.IndexOf("mpegurl", StringComparison.OrdinalIgnoreCase) >= 0 OrElse StripQuery(record.Key).EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function IsWebM(record As StoredRecord) As Boolean
        Return record.ContentType.Equals("video/webm", StringComparison.OrdinalIgnoreCase) OrElse StripQuery(record.Key).EndsWith(".webm", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function NormalizeKey(value As String) As String
        Dim clean = StripQuery(If(value, String.Empty).Trim()).Replace("\"c, "/"c)
        Dim absolute As Uri = Nothing
        If Uri.TryCreate(clean, UriKind.Absolute, absolute) Then clean = absolute.AbsolutePath
        Return clean.TrimStart("/"c)
    End Function

    Private Shared Function ResolveRelativeKey(playlistKey As String, segmentKey As String) As String
        Dim baseKey = NormalizeKey(playlistKey)
        Dim slash = baseKey.LastIndexOf("/"c)
        Dim directory = If(slash >= 0, baseKey.Substring(0, slash + 1), String.Empty)
        Return NormalizeKey(directory & segmentKey)
    End Function

    Private Shared Function StripQuery(value As String) As String
        Dim query = value.IndexOf("?"c)
        Return If(query >= 0, value.Substring(0, query), value)
    End Function

    Private Shared Function ResolutionFromKey(value As String) As String
        Dim match = Regex.Match(NormalizeKey(value), "(?:^|/)(\d{3,4})(?:/|$)")
        Return If(match.Success, match.Groups(1).Value & "p", "WebM")
    End Function

    Private Shared Function ReadPayload(record As StoredRecord) As Byte()
        Dim wrapped As Byte()
        If record.IsInline Then
            Using connection As New SQLiteConnection($"Data Source={AppServices.DatabasePath};Read Only=True;")
                connection.Open()
                Using command As New SQLiteCommand("SELECT content FROM files WHERE id = @id", connection)
                    command.Parameters.Add("@id", System.Data.DbType.Binary).Value = Convert.FromHexString(record.Hash)
                    Dim value = command.ExecuteScalar()
                    If value Is Nothing OrElse value Is DBNull.Value Then Throw New FileNotFoundException("The cached video record no longer exists.")
                    wrapped = DirectCast(value, Byte())
                End Using
            End Using
        Else
            Dim path = ExternalPath(record.Hash)
            If path Is Nothing Then Throw New FileNotFoundException("The cached video segment no longer exists.")
            Using stream = OpenShared(path)
                If record.PayloadOffset < 0 OrElse record.PayloadLength < 0 OrElse CLng(record.PayloadOffset) + record.PayloadLength > stream.Length Then Throw New InvalidDataException("Cached video payload bounds are invalid.")
                stream.Position = record.PayloadOffset
                Dim payload(record.PayloadLength - 1) As Byte
                ReadExactly(stream, payload, 0, payload.Length)
                Return payload
            End Using
        End If

        If record.PayloadOffset < 0 OrElse record.PayloadLength < 0 OrElse CLng(record.PayloadOffset) + record.PayloadLength > wrapped.LongLength Then Throw New InvalidDataException("Cached video payload bounds are invalid.")
        Dim result(record.PayloadLength - 1) As Byte
        Buffer.BlockCopy(wrapped, record.PayloadOffset, result, 0, result.Length)
        Return result
    End Function

    Private Shared Function ExternalPath(hash As String) As String
        Dim primary = Path.Combine(AppServices.PayloadPath, hash.Substring(0, 2), hash)
        If File.Exists(primary) Then Return primary
        Dim fallback = Path.Combine(Path.GetTempPath(), "Roblox", "http", hash)
        If File.Exists(fallback) Then Return fallback
        Return Nothing
    End Function

    Private Shared Function OpenShared(path As String) As FileStream
        Return New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
    End Function

    Private Shared Sub ReadExactly(stream As Stream, buffer As Byte(), offset As Integer, count As Integer)
        Dim total As Integer
        While total < count
            Dim read = stream.Read(buffer, offset + total, count - total)
            If read <= 0 Then Throw New EndOfStreamException()
            total += read
        End While
    End Sub

    Private Shared Function StartsAscii(bytes As Byte(), offset As Integer, value As String) As Boolean
        If bytes Is Nothing OrElse offset < 0 OrElse bytes.Length < offset + value.Length Then Return False
        For index = 0 To value.Length - 1
            If bytes(offset + index) <> AscW(value(index)) Then Return False
        Next
        Return True
    End Function

    Private Shared Sub Report(progress As IProgress(Of VideoScanProgress), current As Integer, total As Integer, found As Integer)
        If progress IsNot Nothing AndAlso (current Mod 25 = 0 OrElse current = total) Then progress.Report(New VideoScanProgress With {.Current = current, .Total = total, .Found = found})
    End Sub
End Class
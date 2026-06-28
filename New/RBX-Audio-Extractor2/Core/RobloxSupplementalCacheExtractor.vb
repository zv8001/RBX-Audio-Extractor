Imports System.Data
Imports System.Data.SQLite
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Enum SupplementalCacheType
    ThumbnailPng
    ThumbnailWebP
    TrueTypeFont
    OpenTypeFont
    Json
    Xml
    Playlist
End Enum

Public NotInheritable Class SupplementalCacheEntry
    Public Property Hash As String
    Public Property FileType As SupplementalCacheType
    Public Property Size As Long
    Public Property IsInline As Boolean
    Public Property Category As Integer
    Public Property PayloadOffset As Integer
    Public Property PayloadLength As Integer = -1
    Public Property DisplayName As String

    Public ReadOnly Property Extension As String
        Get
            Select Case FileType
                Case SupplementalCacheType.ThumbnailPng : Return ".png"
                Case SupplementalCacheType.ThumbnailWebP : Return ".webp"
                Case SupplementalCacheType.TrueTypeFont : Return ".ttf"
                Case SupplementalCacheType.OpenTypeFont : Return ".otf"
                Case SupplementalCacheType.Json : Return ".json"
                Case SupplementalCacheType.Xml : Return ".xml"
                Case SupplementalCacheType.Playlist : Return ".m3u8"
                Case Else : Return ".bin"
            End Select
        End Get
    End Property

    Public ReadOnly Property TypeLabel As String
        Get
            Select Case FileType
                Case SupplementalCacheType.ThumbnailPng : Return "PNG thumbnail"
                Case SupplementalCacheType.ThumbnailWebP : Return "WebP thumbnail"
                Case SupplementalCacheType.TrueTypeFont : Return "TrueType font"
                Case SupplementalCacheType.OpenTypeFont : Return "OpenType font"
                Case SupplementalCacheType.Json : Return "JSON"
                Case SupplementalCacheType.Xml : Return "XML"
                Case SupplementalCacheType.Playlist : Return "HLS playlist"
                Case Else : Return FileType.ToString()
            End Select
        End Get
    End Property

    Public ReadOnly Property ExportBaseName As String
        Get
            If String.IsNullOrWhiteSpace(DisplayName) Then Return Hash
            Dim invalid = Path.GetInvalidFileNameChars()
            Dim builder As New StringBuilder()
            For Each character In DisplayName
                If Array.IndexOf(invalid, character) >= 0 OrElse Char.IsControl(character) Then
                    builder.Append("_"c)
                Else
                    builder.Append(character)
                End If
                If builder.Length >= 90 Then Exit For
            Next
            Dim value = builder.ToString().Trim(" "c, "."c, "_"c)
            If String.IsNullOrWhiteSpace(value) Then value = TypeLabel.Replace(" "c, "_"c)
            Return value & "_" & Hash.Substring(0, 10)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim label = If(String.IsNullOrWhiteSpace(DisplayName), Hash.Substring(0, 12) & "...", DisplayName)
        label = label.Replace(vbCr, " ").Replace(vbLf, " ")
        If label.Length > 62 Then label = label.Substring(0, 59) & "..."
        Return $"[{TypeLabel}]  {label}   {Size:N0} bytes"
    End Function
End Class

Public NotInheritable Class SupplementalScanProgress
    Public Property Current As Integer
    Public Property Total As Integer
    Public Property Found As Integer
End Class

Public NotInheritable Class RobloxSupplementalCacheExtractor
    Private Const EnvelopeSize As Integer = 37

    Private Sub New()
    End Sub

    Public Shared Function ScanThumbnails(progress As Action(Of SupplementalScanProgress)) As List(Of SupplementalCacheEntry)
        Return ScanCategory11(progress)
    End Function

    Public Shared Function ScanFonts(progress As Action(Of SupplementalScanProgress)) As List(Of SupplementalCacheEntry)
        Return ScanCategory10(progress, True)
    End Function

    Public Shared Function ScanMetadata(progress As Action(Of SupplementalScanProgress)) As List(Of SupplementalCacheEntry)
        Return ScanCategory10(progress, False)
    End Function

    Public Shared Function ReadPayload(entry As SupplementalCacheEntry) As Byte()
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        Dim wrapped = ReadStoredRecord(entry)
        Dim length = If(entry.PayloadLength >= 0, entry.PayloadLength, wrapped.Length - entry.PayloadOffset)
        If entry.PayloadOffset < 0 OrElse length < 0 OrElse entry.PayloadOffset + length > wrapped.Length Then Throw New InvalidDataException("Cached payload bounds are invalid.")
        If length = 0 Then Return Array.Empty(Of Byte)()
        Dim payload(length - 1) As Byte
        Buffer.BlockCopy(wrapped, entry.PayloadOffset, payload, 0, length)
        Return payload
    End Function

    Public Shared Sub Export(entry As SupplementalCacheEntry, path As String)
        Dim directory = System.IO.Path.GetDirectoryName(path)
        If Not String.IsNullOrEmpty(directory) Then System.IO.Directory.CreateDirectory(directory)
        File.WriteAllBytes(path, ReadPayload(entry))
    End Sub

    Public Shared Function ExportMany(entries As IEnumerable(Of SupplementalCacheEntry), directory As String, progress As Action(Of Integer, Integer)) As CacheExportSummary
        Dim items = entries.ToList()
        Dim result As New CacheExportSummary()
        System.IO.Directory.CreateDirectory(directory)
        For index = 0 To items.Count - 1
            Dim entry = items(index)
            Try
                Dim outputPath = Path.Combine(directory, entry.ExportBaseName & entry.Extension)
                If File.Exists(outputPath) AndAlso New FileInfo(outputPath).Length > 0 Then
                    result.Reused += 1
                Else
                    Export(entry, outputPath)
                    result.Exported += 1
                End If
                result.Paths.Add(outputPath)
            Catch
                result.Failed += 1
            Finally
                If progress IsNot Nothing Then progress(index + 1, items.Count)
            End Try
        Next
        Return result
    End Function

    Private Shared Function ScanCategory10(progress As Action(Of SupplementalScanProgress), fontsOnly As Boolean) As List(Of SupplementalCacheEntry)
        Dim results As New List(Of SupplementalCacheEntry)()
        Using connection As New SQLiteConnection($"Data Source={DatabasePath()};Read Only=True;")
            connection.Open()
            Dim total = Convert.ToInt32(New SQLiteCommand("SELECT COUNT(*) FROM files WHERE category = 10", connection).ExecuteScalar())
            Using command As New SQLiteCommand("SELECT id, substr(content, 1, 512), size FROM files WHERE category = 10", connection)
                Using reader = command.ExecuteReader()
                    Dim current = 0
                    While reader.Read()
                        current += 1
                        Dim hash = Convert.ToHexString(DirectCast(reader.GetValue(0), Byte())).ToLowerInvariant()
                        Dim inline = Not reader.IsDBNull(1)
                        Dim prefix = If(inline, DirectCast(reader.GetValue(1), Byte()), ReadPrefix(ExternalPath(hash), 512))
                        Dim offset = PayloadStart(prefix)
                        Dim detected = DetectSupplemental(prefix, offset)
                        If detected.HasValue AndAlso (IsFont(detected.Value) = fontsOnly) Then
                            results.Add(New SupplementalCacheEntry With {
                                .Hash = hash,
                                .FileType = detected.Value,
                                .Size = If(reader.IsDBNull(2), 0L, reader.GetInt64(2)),
                                .IsInline = inline,
                                .Category = 10,
                                .PayloadOffset = offset,
                                .DisplayName = SupplementalDisplayName(prefix, offset, hash, detected.Value)
                            })
                        End If
                        Report(progress, current, total, results.Count)
                    End While
                End Using
            End Using
        End Using
        Return results.OrderBy(Function(item) item.FileType).ThenBy(Function(item) item.Hash).ToList()
    End Function

    Private Shared Function ScanCategory11(progress As Action(Of SupplementalScanProgress)) As List(Of SupplementalCacheEntry)
        Dim results As New List(Of SupplementalCacheEntry)()
        Using connection As New SQLiteConnection($"Data Source={DatabasePath()};Read Only=True;")
            connection.Open()
            Dim total = Convert.ToInt32(New SQLiteCommand("SELECT COUNT(*) FROM files WHERE category = 11", connection).ExecuteScalar())
            Using command As New SQLiteCommand("SELECT id, content FROM files WHERE category = 11", connection)
                Using reader = command.ExecuteReader()
                    Dim current = 0
                    While reader.Read()
                        current += 1
                        Dim hash = Convert.ToHexString(DirectCast(reader.GetValue(0), Byte())).ToLowerInvariant()
                        Dim inline = Not reader.IsDBNull(1)
                        Dim bytes = If(inline, DirectCast(reader.GetValue(1), Byte()), ReadAllShared(ExternalPath(hash)))
                        Dim key As String = Nothing
                        Dim offset As Integer
                        Dim length As Integer
                        If TryParseHttpRecord(bytes, key, offset, length) Then
                            Dim type As SupplementalCacheType?
                            If IsPng(bytes, offset) Then type = SupplementalCacheType.ThumbnailPng
                            If IsWebP(bytes, offset) Then type = SupplementalCacheType.ThumbnailWebP
                            If type.HasValue Then
                                results.Add(New SupplementalCacheEntry With {
                                    .Hash = hash,
                                    .FileType = type.Value,
                                    .Size = length,
                                    .IsInline = inline,
                                    .Category = 11,
                                    .PayloadOffset = offset,
                                    .PayloadLength = length,
                                    .DisplayName = FriendlyKey(key)
                                })
                            End If
                        End If
                        Report(progress, current, total, results.Count)
                    End While
                End Using
            End Using
        End Using
        Return results.OrderBy(Function(item) item.DisplayName).ThenBy(Function(item) item.Hash).ToList()
    End Function

    Private Shared Function DetectSupplemental(bytes As Byte(), offset As Integer) As SupplementalCacheType?
        If bytes Is Nothing OrElse offset < 0 OrElse offset >= bytes.Length Then Return Nothing
        If bytes.Length >= offset + 4 AndAlso bytes(offset) = 0 AndAlso bytes(offset + 1) = 1 AndAlso bytes(offset + 2) = 0 AndAlso bytes(offset + 3) = 0 Then Return SupplementalCacheType.TrueTypeFont
        If StartsAscii(bytes, offset, "true") Then Return SupplementalCacheType.TrueTypeFont
        If StartsAscii(bytes, offset, "OTTO") Then Return SupplementalCacheType.OpenTypeFont
        offset = SkipWhitespace(bytes, offset)
        If StartsAscii(bytes, offset, "#EXTM3U") Then Return SupplementalCacheType.Playlist
        If bytes(offset) = AscW("{"c) OrElse bytes(offset) = AscW("["c) Then Return SupplementalCacheType.Json
        If StartsAscii(bytes, offset, "<?xml") Then
            Dim xmlSample = Encoding.UTF8.GetString(bytes, offset, Math.Min(512, bytes.Length - offset))
            If xmlSample.IndexOf("texturepack_version", StringComparison.OrdinalIgnoreCase) >= 0 Then Return SupplementalCacheType.Xml
            Return Nothing
        End If
        If StartsAscii(bytes, offset, "<roblox") Then
            Dim sample = Encoding.UTF8.GetString(bytes, offset, Math.Min(300, bytes.Length - offset))
            If sample.IndexOf("texturepack_version", StringComparison.OrdinalIgnoreCase) >= 0 Then Return SupplementalCacheType.Xml
        End If
        Return Nothing
    End Function

    Private Shared Function SupplementalDisplayName(bytes As Byte(), offset As Integer, hash As String, fileType As SupplementalCacheType) As String
        Dim label As String
        Select Case fileType
            Case SupplementalCacheType.Json : label = "JSON"
            Case SupplementalCacheType.Xml : label = "XML"
            Case SupplementalCacheType.Playlist : label = "HLS playlist"
            Case SupplementalCacheType.TrueTypeFont : label = "TrueType font"
            Case SupplementalCacheType.OpenTypeFont : label = "OpenType font"
            Case Else : label = fileType.ToString()
        End Select

        Dim candidate As String = Nothing
        If bytes IsNot Nothing AndAlso offset >= 0 AndAlso offset < bytes.Length Then
            Dim start = SkipWhitespace(bytes, offset)
            Dim sample = Encoding.UTF8.GetString(bytes, start, Math.Min(512, bytes.Length - start))
            If fileType = SupplementalCacheType.Json Then
                Dim match = Regex.Match(sample, """(?:name|title)""\s*:\s*""([^""]{1,100})""", RegexOptions.IgnoreCase)
                If match.Success Then candidate = match.Groups(1).Value
            ElseIf fileType = SupplementalCacheType.Xml Then
                Dim match = Regex.Match(sample, "<(?:name|title)[^>]*>\s*([^<]{1,100})\s*</(?:name|title)>", RegexOptions.IgnoreCase)
                If match.Success Then candidate = match.Groups(1).Value
            End If
        End If

        If String.IsNullOrWhiteSpace(candidate) Then candidate = label
        candidate = candidate.Replace(vbCr, " ").Replace(vbLf, " ").Trim()
        Return $"{candidate} - {hash.Substring(0, Math.Min(10, hash.Length))}"
    End Function

    Private Shared Function TryParseHttpRecord(bytes As Byte(), ByRef key As String, ByRef bodyOffset As Integer, ByRef bodyLength As Integer) As Boolean
        If bytes Is Nothing OrElse bytes.Length < EnvelopeSize + 25 Then Return False
        Dim start = PayloadStart(bytes)
        Dim nullIndex = Array.IndexOf(bytes, CByte(0), start)
        If nullIndex < start OrElse nullIndex + 25 > bytes.Length Then Return False
        Dim metadata = nullIndex + 1
        Dim headerLength = BitConverter.ToUInt32(bytes, metadata + 4)
        Dim declaredLength = BitConverter.ToUInt32(bytes, metadata + 12)
        Dim calculated = CLng(metadata) + 24L + headerLength
        If calculated > bytes.Length OrElse declaredLength = 0 OrElse declaredLength > bytes.Length - calculated Then Return False
        key = Encoding.UTF8.GetString(bytes, start, nullIndex - start)
        bodyOffset = CInt(calculated)
        bodyLength = CInt(declaredLength)
        Return True
    End Function

    Private Shared Function ReadStoredRecord(entry As SupplementalCacheEntry) As Byte()
        If entry.IsInline Then
            Using connection As New SQLiteConnection($"Data Source={DatabasePath()};Read Only=True;")
                connection.Open()
                Using command As New SQLiteCommand("SELECT content FROM files WHERE id = @id", connection)
                    command.Parameters.Add("@id", DbType.Binary).Value = Convert.FromHexString(entry.Hash)
                    Dim value = command.ExecuteScalar()
                    If value Is Nothing OrElse value Is DBNull.Value Then Throw New FileNotFoundException("The cached item no longer exists.")
                    Return DirectCast(value, Byte())
                End Using
            End Using
        End If
        Dim bytes = ReadAllShared(ExternalPath(entry.Hash))
        If bytes Is Nothing Then Throw New FileNotFoundException("The external cached item no longer exists.")
        Return bytes
    End Function

    Private Shared Function FriendlyKey(key As String) As String
        If String.IsNullOrWhiteSpace(key) Then Return "Roblox thumbnail"
        Dim value = key.Trim()
        Dim query = value.IndexOf("?"c)
        If query >= 0 Then value = value.Substring(0, query)
        If value.Length > 120 Then value = value.Substring(value.Length - 120)
        Return value.Trim("/"c)
    End Function

    Private Shared Function DatabasePath() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "rbx-storage.db")
    End Function

    Private Shared Function ExternalPath(hash As String) As String
        Dim root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "rbx-storage")
        Dim primary = Path.Combine(root, hash.Substring(0, 2), hash)
        If File.Exists(primary) Then Return primary
        Dim fallback = Path.Combine(Path.GetTempPath(), "Roblox", "http", hash)
        If File.Exists(fallback) Then Return fallback
        Return Nothing
    End Function

    Private Shared Function PayloadStart(bytes As Byte()) As Integer
        If bytes IsNot Nothing AndAlso bytes.Length >= EnvelopeSize AndAlso Encoding.ASCII.GetString(bytes, 0, 4) = "RBXH" Then Return EnvelopeSize
        Return 0
    End Function

    Private Shared Function IsPng(bytes As Byte(), offset As Integer) As Boolean
        Dim magic As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}
        Return Starts(bytes, offset, magic)
    End Function

    Private Shared Function IsWebP(bytes As Byte(), offset As Integer) As Boolean
        Return bytes IsNot Nothing AndAlso bytes.Length >= offset + 12 AndAlso StartsAscii(bytes, offset, "RIFF") AndAlso StartsAscii(bytes, offset + 8, "WEBP")
    End Function

    Private Shared Function IsFont(value As SupplementalCacheType) As Boolean
        Return value = SupplementalCacheType.TrueTypeFont OrElse value = SupplementalCacheType.OpenTypeFont
    End Function

    Private Shared Function Starts(bytes As Byte(), offset As Integer, signature As Byte()) As Boolean
        If bytes Is Nothing OrElse offset < 0 OrElse bytes.Length < offset + signature.Length Then Return False
        For index = 0 To signature.Length - 1
            If bytes(offset + index) <> signature(index) Then Return False
        Next
        Return True
    End Function

    Private Shared Function StartsAscii(bytes As Byte(), offset As Integer, value As String) As Boolean
        Return Starts(bytes, offset, Encoding.ASCII.GetBytes(value))
    End Function

    Private Shared Function SkipWhitespace(bytes As Byte(), offset As Integer) As Integer
        While offset < bytes.Length AndAlso (bytes(offset) = 9 OrElse bytes(offset) = 10 OrElse bytes(offset) = 13 OrElse bytes(offset) = 32)
            offset += 1
        End While
        Return offset
    End Function

    Private Shared Sub Report(progress As Action(Of SupplementalScanProgress), current As Integer, total As Integer, found As Integer)
        If progress IsNot Nothing AndAlso (current Mod 250 = 0 OrElse current = total) Then progress(New SupplementalScanProgress With {.Current = current, .Total = total, .Found = found})
    End Sub

    Private Shared Function ReadPrefix(path As String, count As Integer) As Byte()
        If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return Nothing
        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Dim bytes(CInt(Math.Min(count, stream.Length)) - 1) As Byte
            If bytes.Length > 0 Then stream.ReadExactly(bytes)
            Return bytes
        End Using
    End Function

    Private Shared Function ReadAllShared(path As String) As Byte()
        If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return Nothing
        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            If stream.Length > Integer.MaxValue Then Throw New IOException("Cached record is too large.")
            Dim bytes(CInt(stream.Length) - 1) As Byte
            If bytes.Length > 0 Then stream.ReadExactly(bytes)
            Return bytes
        End Using
    End Function
End Class
Imports System.Data.SQLite
Imports System.IO
Imports System.Text
Imports NAudio.Vorbis
Imports NAudio.Wave

Public Enum RobloxCacheFileType
    RbxmBinary
    RbxmXml
    Ktx1
    Ktx2
    Png
    Jpeg
    Bmp
    WebP
    Ogg
    Mp3
End Enum

Public NotInheritable Class RobloxCacheAssetEntry
    Implements IRenamableAsset
    Public Property Hash As String
    Public Property FileType As RobloxCacheFileType
    Public Property Size As Long
    Public Property IsInline As Boolean
    Public Property DurationSeconds As Double?
    Public Property CustomName As String Implements IRenamableAsset.CustomName
    Public Property ContentSha256 As String Implements IRenamableAsset.ContentSha256

    Public ReadOnly Property CacheKey As String Implements IRenamableAsset.CacheKey
        Get
            Return Hash
        End Get
    End Property

    Public ReadOnly Property FriendlyName As String Implements IRenamableAsset.FriendlyName
        Get
            Return If(String.IsNullOrWhiteSpace(CustomName), Hash, CustomName)
        End Get
    End Property

    Public ReadOnly Property ExportBaseName As String Implements IRenamableAsset.ExportBaseName
        Get
            Return AssetNameStore.MakeSafeFileName(CustomName, Hash)
        End Get
    End Property

    Public ReadOnly Property DurationText As String
        Get
            If Not DurationSeconds.HasValue Then Return "--:--"
            Dim duration = TimeSpan.FromSeconds(Math.Max(0, DurationSeconds.Value))
            Return If(duration.TotalHours >= 1, duration.ToString("h\:mm\:ss"), duration.ToString("mm\:ss"))
        End Get
    End Property

    Public ReadOnly Property Extension As String
        Get
            Select Case FileType
                Case RobloxCacheFileType.RbxmBinary, RobloxCacheFileType.RbxmXml : Return ".rbxm"
                Case RobloxCacheFileType.Ktx1 : Return ".ktx"
                Case RobloxCacheFileType.Ktx2 : Return ".ktx2"
                Case RobloxCacheFileType.Png : Return ".png"
                Case RobloxCacheFileType.Jpeg : Return ".jpg"
                Case RobloxCacheFileType.Bmp : Return ".bmp"
                Case RobloxCacheFileType.WebP : Return ".webp"
                Case RobloxCacheFileType.Ogg : Return ".ogg"
                Case RobloxCacheFileType.Mp3 : Return ".mp3"
                Case Else : Return ".bin"
            End Select
        End Get
    End Property

    Public ReadOnly Property TypeLabel As String
        Get
            Select Case FileType
                Case RobloxCacheFileType.RbxmBinary : Return "RBXM binary"
                Case RobloxCacheFileType.RbxmXml : Return "RBXM XML"
                Case RobloxCacheFileType.Ktx1 : Return "KTX"
                Case RobloxCacheFileType.Ktx2 : Return "KTX2"
                Case RobloxCacheFileType.Ogg : Return "OGG audio"
                Case RobloxCacheFileType.Mp3 : Return "MP3 audio"
                Case Else : Return FileType.ToString().ToUpperInvariant()
            End Select
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{TypeLabel}]  {Hash.Substring(0, 12)}...   {Size:N0} bytes"
    End Function
End Class

Public NotInheritable Class CacheAssetProgress
    Public Property Current As Integer
    Public Property Total As Integer
    Public Property Found As Integer
End Class

Public NotInheritable Class CacheExportSummary
    Public Property Exported As Integer
    Public Property Reused As Integer
    Public Property Failed As Integer
    Public Property Paths As New List(Of String)()
End Class

Public NotInheritable Class RobloxCacheAssetExtractor
    Private Const EnvelopeSize As Integer = 37
    Private Shared ReadOnly Ktx1Magic As Byte() = {&HAB, &H4B, &H54, &H58, &H20, &H31, &H31, &HBB, &HD, &HA, &H1A, &HA}
    Private Shared ReadOnly Ktx2Magic As Byte() = {&HAB, &H4B, &H54, &H58, &H20, &H32, &H30, &HBB, &HD, &HA, &H1A, &HA}
    Private Shared ReadOnly PngMagic As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}

    Private Sub New()
    End Sub

    Public Shared Function Scan(progress As Action(Of CacheAssetProgress), ParamArray requestedTypes As RobloxCacheFileType()) As List(Of RobloxCacheAssetEntry)
        Dim allowed = New HashSet(Of RobloxCacheFileType)(requestedTypes)
        Dim includeAll = allowed.Count = 0
        Dim results As New List(Of RobloxCacheAssetEntry)()
        Dim dbPath = GetDatabasePath()
        If Not File.Exists(dbPath) Then Throw New FileNotFoundException("Roblox cache database was not found.", dbPath)

        Using connection As New SQLiteConnection($"Data Source={dbPath};Read Only=True;")
            connection.Open()
            Dim total As Integer
            Using countCommand As New SQLiteCommand("SELECT COUNT(*) FROM files WHERE category = 10", connection)
                total = Convert.ToInt32(countCommand.ExecuteScalar())
            End Using
            Using command As New SQLiteCommand("SELECT id, substr(content, 1, 64), size FROM files WHERE category = 10", connection)
                Using reader = command.ExecuteReader()
                    Dim current = 0
                    While reader.Read()
                        current += 1
                        Dim id = DirectCast(reader.GetValue(0), Byte())
                        Dim hash = Convert.ToHexString(id).ToLowerInvariant()
                        Dim inline = Not reader.IsDBNull(1)
                        Dim prefix = If(inline, DirectCast(reader.GetValue(1), Byte()), ReadPrefix(FindExternalFile(hash), 64))
                        Dim detected = Detect(prefix)
                        If detected.HasValue AndAlso (includeAll OrElse allowed.Contains(detected.Value)) Then
                            results.Add(New RobloxCacheAssetEntry With {
                                .Hash = hash,
                                .FileType = detected.Value,
                                .Size = If(reader.IsDBNull(2), 0L, reader.GetInt64(2)),
                                .IsInline = inline
                            })
                        End If
                        If progress IsNot Nothing AndAlso (current Mod 250 = 0 OrElse current = total) Then
                            progress(New CacheAssetProgress With {.Current = current, .Total = total, .Found = results.Count})
                        End If
                    End While
                End Using
            End Using
        End Using
        Return results.OrderBy(Function(item) item.FileType).ThenBy(Function(item) item.Hash).ToList()
    End Function

    Public Shared Sub PopulateAudioDurations(entries As IEnumerable(Of RobloxCacheAssetEntry), progress As Action(Of Integer, Integer))
        Dim items = entries.Where(Function(item) item.FileType = RobloxCacheFileType.Ogg OrElse item.FileType = RobloxCacheFileType.Mp3).ToList()
        If items.Count = 0 Then Return
        Using connection As New SQLiteConnection($"Data Source={GetDatabasePath()};Read Only=True;")
            connection.Open()
            Using command As New SQLiteCommand("SELECT content FROM files WHERE id = @id", connection)
                command.Parameters.Add("@id", DbType.Binary)
                For index = 0 To items.Count - 1
                    Dim entry = items(index)
                    Try
                        Dim wrapped As Byte()
                        If entry.IsInline Then
                            command.Parameters("@id").Value = Convert.FromHexString(entry.Hash)
                            Dim value = command.ExecuteScalar()
                            If value Is Nothing OrElse value Is DBNull.Value Then Throw New FileNotFoundException("The cached audio asset is no longer present.")
                            wrapped = DirectCast(value, Byte())
                        Else
                            Dim sourcePath = FindExternalFile(entry.Hash)
                            If String.IsNullOrEmpty(sourcePath) Then Throw New FileNotFoundException("The external cached audio asset is no longer present.")
                            wrapped = File.ReadAllBytes(sourcePath)
                        End If

                        Dim offset = PayloadOffset(wrapped)
                        Using payload As New MemoryStream(wrapped, offset, wrapped.Length - offset, writable:=False)
                            Dim audioReader As WaveStream = Nothing
                            Try
                                audioReader = If(entry.FileType = RobloxCacheFileType.Ogg,
                                                 DirectCast(New VorbisWaveReader(payload), WaveStream),
                                                 New Mp3FileReader(payload))
                                Dim seconds = audioReader.TotalTime.TotalSeconds
                                If seconds >= 0 AndAlso Not Double.IsNaN(seconds) AndAlso Not Double.IsInfinity(seconds) Then entry.DurationSeconds = seconds
                            Finally
                                If audioReader IsNot Nothing Then audioReader.Dispose()
                            End Try
                        End Using
                    Catch
                        entry.DurationSeconds = Nothing
                    Finally
                        If progress IsNot Nothing AndAlso (index Mod 25 = 0 OrElse index + 1 = items.Count) Then progress(index + 1, items.Count)
                    End Try
                Next
            End Using
        End Using
    End Sub

    Public Shared Function ReadPayload(entry As RobloxCacheAssetEntry) As Byte()
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        Dim wrapped As Byte()
        If entry.IsInline Then
            Using connection As New SQLiteConnection($"Data Source={GetDatabasePath()};Read Only=True;")
                connection.Open()
                Using command As New SQLiteCommand("SELECT content FROM files WHERE id = @id", connection)
                    command.Parameters.Add("@id", DbType.Binary).Value = Convert.FromHexString(entry.Hash)
                    Dim value = command.ExecuteScalar()
                    If value Is Nothing OrElse value Is DBNull.Value Then Throw New FileNotFoundException("The cached asset is no longer present.")
                    wrapped = DirectCast(value, Byte())
                End Using
            End Using
        Else
            Dim sourcePath = FindExternalFile(entry.Hash)
            If String.IsNullOrEmpty(sourcePath) Then Throw New FileNotFoundException("The external cached asset is no longer present.")
            wrapped = File.ReadAllBytes(sourcePath)
        End If
        Dim offset = PayloadOffset(wrapped)
        Dim payload(wrapped.Length - offset - 1) As Byte
        Buffer.BlockCopy(wrapped, offset, payload, 0, payload.Length)
        Return payload
    End Function
    Public Shared Function Export(entry As RobloxCacheAssetEntry, outputPath As String) As String
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        Dim directory = Path.GetDirectoryName(outputPath)
        If Not String.IsNullOrEmpty(directory) Then System.IO.Directory.CreateDirectory(directory)
        Using connection As New SQLiteConnection($"Data Source={GetDatabasePath()};Read Only=True;")
            connection.Open()
            WriteEntry(entry, outputPath, connection)
        End Using
        Return outputPath
    End Function

    Public Shared Function ExportMany(entries As IEnumerable(Of RobloxCacheAssetEntry), outputDirectory As String,
                                      progress As Action(Of Integer, Integer), Optional separateTypeFolders As Boolean = False) As CacheExportSummary
        Dim items = entries.ToList()
        Dim summary As New CacheExportSummary()
        Dim usedNames As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Directory.CreateDirectory(outputDirectory)
        Using connection As New SQLiteConnection($"Data Source={GetDatabasePath()};Read Only=True;")
            connection.Open()
            For i = 0 To items.Count - 1
                Dim entry = items(i)
                Try
                    Dim folder = outputDirectory
                    If separateTypeFolders Then
                        folder = Path.Combine(outputDirectory, If(entry.FileType = RobloxCacheFileType.Ktx1 OrElse entry.FileType = RobloxCacheFileType.Ktx2, "KTX", "RBXM"))
                        Directory.CreateDirectory(folder)
                    End If
                    Dim outputPath = Path.Combine(folder, AssetNameStore.GetBatchBaseName(entry, usedNames) & entry.Extension)
                    If File.Exists(outputPath) AndAlso New FileInfo(outputPath).Length > 0 Then
                        summary.Reused += 1
                    Else
                        WriteEntry(entry, outputPath, connection)
                        summary.Exported += 1
                    End If
                    summary.Paths.Add(outputPath)
                Catch
                    summary.Failed += 1
                Finally
                    If progress IsNot Nothing Then progress(i + 1, items.Count)
                End Try
            Next
        End Using
        Return summary
    End Function

    Private Shared Sub WriteEntry(entry As RobloxCacheAssetEntry, outputPath As String, connection As SQLiteConnection)
        If entry.IsInline Then
            Using command As New SQLiteCommand("SELECT content FROM files WHERE id = @id", connection)
                command.Parameters.Add("@id", DbType.Binary).Value = Convert.FromHexString(entry.Hash)
                Dim value = command.ExecuteScalar()
                If value Is Nothing OrElse value Is DBNull.Value Then Throw New FileNotFoundException("The cached asset is no longer present.")
                Dim wrapped = DirectCast(value, Byte())
                Dim offset = PayloadOffset(wrapped)
                Using destination As New FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 131072, FileOptions.SequentialScan)
                    destination.Write(wrapped, offset, wrapped.Length - offset)
                End Using
            End Using
        Else
            Dim sourcePath = FindExternalFile(entry.Hash)
            If String.IsNullOrEmpty(sourcePath) Then Throw New FileNotFoundException("The external cached asset is no longer present.")
            Using source As New FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete, 131072, FileOptions.SequentialScan)
                source.Position = PayloadOffset(source)
                Using destination As New FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 131072, FileOptions.SequentialScan)
                    source.CopyTo(destination, 131072)
                End Using
            End Using
        End If
    End Sub

    Private Shared Function Detect(wrapped As Byte()) As RobloxCacheFileType?
        If wrapped Is Nothing OrElse wrapped.Length < 2 Then Return Nothing
        Dim offset = PayloadOffset(wrapped)
        If StartsAt(wrapped, offset, Ktx1Magic) Then Return RobloxCacheFileType.Ktx1
        If StartsAt(wrapped, offset, Ktx2Magic) Then Return RobloxCacheFileType.Ktx2
        If StartsAtAscii(wrapped, offset, "<roblox!") Then Return RobloxCacheFileType.RbxmBinary
        If StartsAtAscii(wrapped, offset, "<roblox") Then Return RobloxCacheFileType.RbxmXml
        If StartsAtAscii(wrapped, offset, "OggS") Then Return RobloxCacheFileType.Ogg
        If StartsAtAscii(wrapped, offset, "ID3") OrElse (wrapped.Length >= offset + 2 AndAlso wrapped(offset) = &HFF AndAlso (wrapped(offset + 1) And &HE0) = &HE0) Then Return RobloxCacheFileType.Mp3
        If StartsAt(wrapped, offset, PngMagic) Then Return RobloxCacheFileType.Png
        If wrapped.Length >= offset + 2 AndAlso wrapped(offset) = &HFF AndAlso wrapped(offset + 1) = &HD8 Then Return RobloxCacheFileType.Jpeg
        If StartsAtAscii(wrapped, offset, "BM") Then Return RobloxCacheFileType.Bmp
        If wrapped.Length >= offset + 12 AndAlso StartsAtAscii(wrapped, offset, "RIFF") AndAlso StartsAtAscii(wrapped, offset + 8, "WEBP") Then Return RobloxCacheFileType.WebP
        Return Nothing
    End Function

    Private Shared Function PayloadOffset(bytes As Byte()) As Integer
        Return If(bytes.Length >= EnvelopeSize AndAlso Encoding.ASCII.GetString(bytes, 0, 4) = "RBXH", EnvelopeSize, 0)
    End Function

    Private Shared Function PayloadOffset(stream As Stream) As Integer
        If stream.Length < 4 Then Return 0
        Dim original = stream.Position
        stream.Position = 0
        Dim header(3) As Byte
        Dim read = stream.Read(header, 0, header.Length)
        stream.Position = original
        Return If(read = 4 AndAlso Encoding.ASCII.GetString(header) = "RBXH" AndAlso stream.Length >= EnvelopeSize, EnvelopeSize, 0)
    End Function

    Private Shared Function StartsAt(bytes As Byte(), offset As Integer, signature As Byte()) As Boolean
        If offset < 0 OrElse bytes.Length < offset + signature.Length Then Return False
        For i = 0 To signature.Length - 1
            If bytes(offset + i) <> signature(i) Then Return False
        Next
        Return True
    End Function

    Private Shared Function StartsAtAscii(bytes As Byte(), offset As Integer, signature As String) As Boolean
        Return StartsAt(bytes, offset, Encoding.ASCII.GetBytes(signature))
    End Function

    Private Shared Function GetDatabasePath() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "rbx-storage.db")
    End Function

    Private Shared Function FindExternalFile(hash As String) As String
        Dim root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "rbx-storage")
        Dim primary = Path.Combine(root, hash.Substring(0, 2), hash)
        If File.Exists(primary) Then Return primary
        Dim fallback = Path.Combine(Path.GetTempPath(), "Roblox", "http", hash)
        If File.Exists(fallback) Then Return fallback
        Return Nothing
    End Function

    Private Shared Function ReadPrefix(path As String, count As Integer) As Byte()
        If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return Nothing
        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Dim length = CInt(Math.Min(count, stream.Length))
            If length = 0 Then Return Array.Empty(Of Byte)()
            Dim bytes(length - 1) As Byte
            Dim read = stream.Read(bytes, 0, bytes.Length)
            If read <> bytes.Length Then Array.Resize(bytes, read)
            Return bytes
        End Using
    End Function
End Class
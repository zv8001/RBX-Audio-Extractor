Imports System.Data.SQLite
Imports System.Globalization
Imports System.IO
Imports System.Numerics
Imports System.Text
Imports System.Text.RegularExpressions
Imports Openize.Drako

Public NotInheritable Class MeshCacheEntry
    Public Property Hash As String
    Public Property Version As String
    Public Property Size As Long
    Public Property IsInline As Boolean

    Public ReadOnly Property CanExport As Boolean
        Get
            Dim major As Integer
            Return Integer.TryParse(Version.Split("."c)(0), major) AndAlso ((major >= 1 AndAlso major <= 5) OrElse major = 7)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim support = If(CanExport, "ready", "unsupported")
        Return $"{Hash.Substring(0, 12)}...   mesh v{Version}   {Size:N0} bytes   [{support}]"
    End Function
End Class

Public NotInheritable Class MeshScanProgress
    Public Property Current As Integer
    Public Property Total As Integer
    Public Property Found As Integer
End Class

Public NotInheritable Class MeshPreviewData
    Public Property Positions As Vector3()
    Public Property Indices As Integer()
End Class
Public NotInheritable Class MeshExportResult
    Public Property VertexCount As Integer
    Public Property FaceCount As Integer
    Public Property OutputPath As String
End Class

Friend NotInheritable Class MeshGeometry
    Public ReadOnly Positions As New List(Of Vector3)()
    Public ReadOnly Normals As New List(Of Vector3)()
    Public ReadOnly UVs As New List(Of Vector2)()
    Public ReadOnly Indices As New List(Of Integer)()
End Class

Public NotInheritable Class RobloxMeshExtractor
    Private Const CacheEnvelopeSize As Integer = 37

    Private Sub New()
    End Sub

    Public Shared Function Scan(progress As IProgress(Of MeshScanProgress)) As List(Of MeshCacheEntry)
        Dim dbPath = GetDatabasePath()
        If Not File.Exists(dbPath) Then Throw New FileNotFoundException("Roblox cache database was not found.", dbPath)

        Dim results As New List(Of MeshCacheEntry)()
        Using connection As New SQLiteConnection($"Data Source={dbPath};Read Only=True;")
            connection.Open()
            Dim total As Integer
            Using countCommand As New SQLiteCommand("SELECT COUNT(*) FROM files", connection)
                total = Convert.ToInt32(countCommand.ExecuteScalar(), CultureInfo.InvariantCulture)
            End Using

            Using command As New SQLiteCommand("SELECT id, substr(content, 1, 128), size FROM files", connection)
                Using reader = command.ExecuteReader()
                    Dim current = 0
                    While reader.Read()
                        current += 1
                        Dim id = DirectCast(reader.GetValue(0), Byte())
                        Dim hash = Convert.ToHexString(id).ToLowerInvariant()
                        Dim prefix As Byte() = Nothing
                        Dim inline = Not reader.IsDBNull(1)
                        If inline Then
                            prefix = DirectCast(reader.GetValue(1), Byte())
                        Else
                            prefix = ReadPrefix(FindExternalFile(hash), 128)
                        End If

                        Dim version = GetMeshVersion(prefix)
                        If version IsNot Nothing Then
                            results.Add(New MeshCacheEntry With {
                                .Hash = hash,
                                .Version = version,
                                .Size = If(reader.IsDBNull(2), 0L, reader.GetInt64(2)),
                                .IsInline = inline
                            })
                        End If

                        If progress IsNot Nothing AndAlso (current Mod 250 = 0 OrElse current = total) Then
                            progress.Report(New MeshScanProgress With {.Current = current, .Total = total, .Found = results.Count})
                        End If
                    End While
                End Using
            End Using
        End Using
        Return results.OrderByDescending(Function(item) ParseMajor(item.Version)).ThenBy(Function(item) item.Hash).ToList()
    End Function

    Public Shared Function LoadPreview(entry As MeshCacheEntry) As MeshPreviewData
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        If Not entry.CanExport Then Throw New NotSupportedException($"Mesh version {entry.Version} is not supported yet.")
        Dim geometry = Decode(Unwrap(LoadContent(entry)), entry.Version)
        Return New MeshPreviewData With {
            .Positions = geometry.Positions.ToArray(),
            .Indices = geometry.Indices.ToArray()
        }
    End Function
    Public Shared Function Export(entry As MeshCacheEntry, outputPath As String) As MeshExportResult
        If entry Is Nothing Then Throw New ArgumentNullException(NameOf(entry))
        If Not entry.CanExport Then Throw New NotSupportedException($"Mesh version {entry.Version} is not supported yet.")
        Dim wrapped = LoadContent(entry)
        Dim payload = Unwrap(wrapped)
        Dim geometry = Decode(payload, entry.Version)
        WriteObj(outputPath, entry, geometry)
        Return New MeshExportResult With {
            .VertexCount = geometry.Positions.Count,
            .FaceCount = geometry.Indices.Count \ 3,
            .OutputPath = outputPath
        }
    End Function

    Private Shared Function GetDatabasePath() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "rbx-storage.db")
    End Function

    Private Shared Function GetCacheRoot() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "rbx-storage")
    End Function

    Private Shared Function FindExternalFile(hash As String) As String
        Dim primary = Path.Combine(GetCacheRoot(), hash.Substring(0, 2), hash)
        If File.Exists(primary) Then Return primary
        Dim fallback = Path.Combine(Path.GetTempPath(), "Roblox", "http", hash)
        If File.Exists(fallback) Then Return fallback
        Return Nothing
    End Function

    Private Shared Function ReadPrefix(path As String, count As Integer) As Byte()
        If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return Nothing
        Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Dim bytes(CInt(Math.Min(count, stream.Length)) - 1) As Byte
            If bytes.Length = 0 Then Return Array.Empty(Of Byte)()
            Dim read = stream.Read(bytes, 0, bytes.Length)
            If read <> bytes.Length Then Array.Resize(bytes, read)
            Return bytes
        End Using
    End Function

    Private Shared Function LoadContent(entry As MeshCacheEntry) As Byte()
        If Not entry.IsInline Then
            Dim path = FindExternalFile(entry.Hash)
            If String.IsNullOrEmpty(path) Then Throw New FileNotFoundException("The cached mesh file is no longer present.")
            Return File.ReadAllBytes(path)
        End If

        Using connection As New SQLiteConnection($"Data Source={GetDatabasePath()};Read Only=True;")
            connection.Open()
            Using command As New SQLiteCommand("SELECT content FROM files WHERE id = @id", connection)
                command.Parameters.Add("@id", DbType.Binary).Value = Convert.FromHexString(entry.Hash)
                Dim value = command.ExecuteScalar()
                If value Is Nothing OrElse value Is DBNull.Value Then Throw New FileNotFoundException("The cached mesh entry is no longer present.")
                Return DirectCast(value, Byte())
            End Using
        End Using
    End Function

    Private Shared Function Unwrap(bytes As Byte()) As Byte()
        If bytes Is Nothing OrElse bytes.Length = 0 Then Throw New InvalidDataException("The cached mesh is empty.")
        Dim offset = If(bytes.Length >= 4 AndAlso Encoding.ASCII.GetString(bytes, 0, 4) = "RBXH", CacheEnvelopeSize, 0)
        If bytes.Length <= offset Then Throw New InvalidDataException("The Roblox cache envelope is incomplete.")
        Dim payload(bytes.Length - offset - 1) As Byte
        Buffer.BlockCopy(bytes, offset, payload, 0, payload.Length)
        Return payload
    End Function

    Private Shared Function GetMeshVersion(prefix As Byte()) As String
        If prefix Is Nothing OrElse prefix.Length < 12 Then Return Nothing
        Dim offset = If(prefix.Length >= 4 AndAlso Encoding.ASCII.GetString(prefix, 0, 4) = "RBXH", CacheEnvelopeSize, 0)
        If prefix.Length < offset + 9 OrElse Encoding.ASCII.GetString(prefix, offset, 8) <> "version " Then Return Nothing
        Dim finish = offset + 8
        While finish < prefix.Length AndAlso prefix(finish) <> 10 AndAlso prefix(finish) <> 13 AndAlso finish - offset < 20
            finish += 1
        End While
        Dim version = Encoding.ASCII.GetString(prefix, offset + 8, finish - offset - 8).Trim()
        If Not Regex.IsMatch(version, "^\d+(\.\d+)?$") Then Return Nothing
        Return version
    End Function

    Private Shared Function ParseMajor(version As String) As Integer
        Dim value As Integer
        Integer.TryParse(version.Split("."c)(0), value)
        Return value
    End Function

    Private Shared Function Decode(payload As Byte(), version As String) As MeshGeometry
        Select Case ParseMajor(version)
            Case 1
                Return DecodeTextMesh(payload, version)
            Case 2 To 5
                Return DecodeBinaryMesh(payload, ParseMajor(version))
            Case 7
                Return DecodeDracoMesh(payload)
            Case Else
                Throw New NotSupportedException($"Mesh version {version} is not supported yet.")
        End Select
    End Function

    Private Shared Function DecodeTextMesh(payload As Byte(), version As String) As MeshGeometry
        Dim text = Encoding.ASCII.GetString(payload)
        Dim lines = text.Split({ControlChars.Cr, ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
        If lines.Length < 3 Then Throw New InvalidDataException("The text mesh is incomplete.")
        Dim faceCount As Integer
        If Not Integer.TryParse(lines(1).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, faceCount) Then Throw New InvalidDataException("The text mesh face count is invalid.")
        Dim matches = Regex.Matches(String.Join(" ", lines.Skip(2)), "\[\s*([^,]+),\s*([^,]+),\s*([^\]]+)\]")
        If matches.Count < faceCount * 9 Then Throw New InvalidDataException("The text mesh vertex data is incomplete.")

        Dim result As New MeshGeometry()
        Dim m = 0
        Dim scale = If(version.StartsWith("1.00", StringComparison.Ordinal), 0.5F, 1.0F)
        For vertex = 0 To faceCount * 3 - 1
            Dim p = ReadMatchVector3(matches(m)) * scale : m += 1
            Dim n = ReadMatchVector3(matches(m)) : m += 1
            Dim uv3 = ReadMatchVector3(matches(m)) : m += 1
            result.Positions.Add(p)
            result.Normals.Add(n)
            result.UVs.Add(New Vector2(uv3.X, uv3.Y))
            result.Indices.Add(vertex)
        Next
        Return result
    End Function

    Private Shared Function ReadMatchVector3(match As Match) As Vector3
        Return New Vector3(ParseSingle(match.Groups(1).Value), ParseSingle(match.Groups(2).Value), ParseSingle(match.Groups(3).Value))
    End Function

    Private Shared Function ParseSingle(value As String) As Single
        Return Single.Parse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function DecodeBinaryMesh(payload As Byte(), major As Integer) As MeshGeometry
        Using stream As New MemoryStream(payload, False)
            Using reader As New BinaryReader(stream, Encoding.ASCII, True)
                If Encoding.ASCII.GetString(reader.ReadBytes(8)) <> "version " Then Throw New InvalidDataException("Invalid mesh header.")
                stream.Position = 13
                reader.ReadUInt16()

                Dim vertexSize As Integer
                Dim vertexCount As UInteger
                Dim faceCount As UInteger
                Dim lodCount As UShort = 0
                Dim boneCount As UShort = 0
                If major >= 4 Then
                    reader.ReadUInt16()
                    vertexCount = reader.ReadUInt32()
                    faceCount = reader.ReadUInt32()
                    lodCount = reader.ReadUInt16()
                    boneCount = reader.ReadUInt16()
                    reader.ReadUInt32()
                    reader.ReadUInt16()
                    reader.ReadUInt16()
                    If major >= 5 Then reader.ReadUInt32() : reader.ReadUInt32()
                    vertexSize = 40
                Else
                    vertexSize = reader.ReadByte()
                    reader.ReadByte()
                    If major >= 3 Then reader.ReadUInt16() : lodCount = reader.ReadUInt16()
                    vertexCount = reader.ReadUInt32()
                    faceCount = reader.ReadUInt32()
                End If

                If vertexCount > 100000000UI OrElse faceCount > 100000000UI OrElse vertexSize < 36 Then Throw New InvalidDataException("Unreasonable mesh dimensions.")
                Dim result As New MeshGeometry()
                For i As UInteger = 0UI To vertexCount - 1UI
                    Dim start = stream.Position
                    result.Positions.Add(New Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()))
                    result.Normals.Add(New Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()))
                    result.UVs.Add(New Vector2(reader.ReadSingle(), reader.ReadSingle()))
                    reader.ReadUInt32()
                    stream.Position = start + vertexSize
                Next

                If boneCount > 0 Then stream.Position += CLng(vertexCount) * 8L
                For i As UInteger = 0UI To faceCount * 3UI - 1UI
                    Dim index = reader.ReadUInt32()
                    If index >= vertexCount Then Throw New InvalidDataException("A mesh face references a missing vertex.")
                    result.Indices.Add(CInt(index))
                Next

                If lodCount > 1 Then
                    Dim offsets As New List(Of UInteger)()
                    For i = 0 To lodCount - 1
                        offsets.Add(reader.ReadUInt32())
                    Next
                    If offsets.Count >= 2 AndAlso offsets(1) > offsets(0) AndAlso offsets(1) <= faceCount Then
                        Dim first = CInt(offsets(0)) * 3
                        Dim count = CInt(offsets(1) - offsets(0)) * 3
                        Dim highLod = result.Indices.GetRange(first, count)
                        result.Indices.Clear()
                        result.Indices.AddRange(highLod)
                    End If
                End If
                Return result
            End Using
        End Using
    End Function

    Private Shared Function DecodeDracoMesh(payload As Byte()) As MeshGeometry
        Dim marker = FindAscii(payload, "DRACO")
        If marker < 0 Then Throw New InvalidDataException("This version-7 mesh does not contain a Draco stream.")
        Dim compressed(payload.Length - marker - 1) As Byte
        Buffer.BlockCopy(payload, marker, compressed, 0, compressed.Length)
        Dim decoded = Draco.Decode(compressed)
        Dim mesh = TryCast(decoded, DracoMesh)
        If mesh Is Nothing Then Throw New InvalidDataException("The Draco stream could not be decoded.")

        Dim position = mesh.GetNamedAttribute(AttributeType.Position)
        If position Is Nothing AndAlso mesh.NumAttributes > 0 Then position = mesh.Attribute(0)
        If position Is Nothing OrElse position.ComponentsCount < 3 Then Throw New InvalidDataException("The mesh has no position attribute.")
        Dim normal = mesh.GetNamedAttribute(AttributeType.Normal)
        If normal Is Nothing AndAlso mesh.NumAttributes > 1 AndAlso mesh.Attribute(1).ComponentsCount >= 3 Then normal = mesh.Attribute(1)
        Dim uv = mesh.GetNamedAttribute(AttributeType.TexCoord)
        If uv Is Nothing AndAlso mesh.NumAttributes > 2 AndAlso mesh.Attribute(2).ComponentsCount >= 2 Then uv = mesh.Attribute(2)

        Dim result As New MeshGeometry()
        For point = 0 To mesh.NumPoints - 1
            result.Positions.Add(ReadVector3(position, point))
            If normal IsNot Nothing Then result.Normals.Add(ReadVector3(normal, point))
            If uv IsNot Nothing Then result.UVs.Add(ReadVector2(uv, point))
        Next
        For i = 0 To mesh.Indices.Count - 1
            result.Indices.Add(mesh.Indices(i))
        Next
        Return result
    End Function

    Private Shared Function ReadVector3(attribute As PointAttribute, point As Integer) As Vector3
        Dim values(2) As Single
        attribute.GetValue(attribute.MappedIndex(point), values)
        Return New Vector3(values(0), values(1), values(2))
    End Function

    Private Shared Function ReadVector2(attribute As PointAttribute, point As Integer) As Vector2
        Dim values(1) As Single
        attribute.GetValue(attribute.MappedIndex(point), values)
        Return New Vector2(values(0), values(1))
    End Function

    Private Shared Function FindAscii(bytes As Byte(), value As String) As Integer
        Dim needle = Encoding.ASCII.GetBytes(value)
        For i = 0 To bytes.Length - needle.Length
            Dim match = True
            For n = 0 To needle.Length - 1
                If bytes(i + n) <> needle(n) Then match = False : Exit For
            Next
            If match Then Return i
        Next
        Return -1
    End Function

    Private Shared Sub WriteObj(path As String, entry As MeshCacheEntry, mesh As MeshGeometry)
        Dim directory = System.IO.Path.GetDirectoryName(path)
        If Not String.IsNullOrEmpty(directory) Then System.IO.Directory.CreateDirectory(directory)
        Using writer As New StreamWriter(path, False, New UTF8Encoding(False))
            writer.WriteLine("# Exported by RBX Asset Extractor")
            writer.WriteLine($"# Roblox cache key: {entry.Hash}")
            writer.WriteLine($"# Roblox mesh version: {entry.Version}")
            For Each value In mesh.Positions
                writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "v {0:R} {1:R} {2:R}", value.X, value.Y, value.Z))
            Next
            For Each value In mesh.UVs
                writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "vt {0:R} {1:R}", value.X, 1.0F - value.Y))
            Next
            For Each value In mesh.Normals
                writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "vn {0:R} {1:R} {2:R}", value.X, value.Y, value.Z))
            Next
            Dim hasUv = mesh.UVs.Count = mesh.Positions.Count
            Dim hasNormal = mesh.Normals.Count = mesh.Positions.Count
            For i = 0 To mesh.Indices.Count - 1 Step 3
                Dim a = mesh.Indices(i) + 1
                Dim b = mesh.Indices(i + 1) + 1
                Dim c = mesh.Indices(i + 2) + 1
                If hasUv AndAlso hasNormal Then
                    writer.WriteLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}")
                ElseIf hasUv Then
                    writer.WriteLine($"f {a}/{a} {b}/{b} {c}/{c}")
                ElseIf hasNormal Then
                    writer.WriteLine($"f {a}//{a} {b}//{b} {c}//{c}")
                Else
                    writer.WriteLine($"f {a} {b} {c}")
                End If
            Next
        End Using
    End Sub
End Class
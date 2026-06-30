Imports System.Data
Imports System.Data.SQLite

''' <summary>
''' Persists the lightweight results of each workspace scan (asset metadata only, never payload
''' bytes) into RBXAssetExtractor.db so the tabs repopulate on the next launch without re-scanning.
''' Saved custom names are re-applied from the names table on load.
''' </summary>
Public NotInheritable Class SessionStateStore
    Private Sub New()
    End Sub

    ' --- Cache assets: audio, images, cache files ------------------------------------------------
    Public Shared Sub SaveCacheAssets(workspace As String, entries As IEnumerable(Of RobloxCacheAssetEntry))
        Dim list = entries.ToList()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using transaction = connection.BeginTransaction()
                        DeleteWorkspace(connection, transaction, "scan_cache_assets", workspace)
                        Using command As New SQLiteCommand("INSERT INTO scan_cache_assets (workspace, ord, hash, file_type, size, is_inline, duration) VALUES (@workspace, @ord, @hash, @type, @size, @inline, @duration)", connection, transaction)
                            command.Parameters.Add("@workspace", DbType.String)
                            command.Parameters.Add("@ord", DbType.Int32)
                            command.Parameters.Add("@hash", DbType.String)
                            command.Parameters.Add("@type", DbType.Int32)
                            command.Parameters.Add("@size", DbType.Int64)
                            command.Parameters.Add("@inline", DbType.Int32)
                            command.Parameters.Add("@duration", DbType.Double)
                            For ord = 0 To list.Count - 1
                                Dim entry = list(ord)
                                command.Parameters("@workspace").Value = workspace
                                command.Parameters("@ord").Value = ord
                                command.Parameters("@hash").Value = entry.Hash
                                command.Parameters("@type").Value = CInt(entry.FileType)
                                command.Parameters("@size").Value = entry.Size
                                command.Parameters("@inline").Value = If(entry.IsInline, 1, 0)
                                command.Parameters("@duration").Value = If(entry.DurationSeconds.HasValue, CObj(entry.DurationSeconds.Value), DBNull.Value)
                                command.ExecuteNonQuery()
                            Next
                        End Using
                        transaction.Commit()
                    End Using
                End Using
            End SyncLock
        Catch
            ' Persisted state is best-effort and must never interrupt a scan.
        End Try
    End Sub

    Public Shared Function LoadCacheAssets(workspace As String) As List(Of RobloxCacheAssetEntry)
        Dim result As New List(Of RobloxCacheAssetEntry)()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using command As New SQLiteCommand("SELECT hash, file_type, size, is_inline, duration FROM scan_cache_assets WHERE workspace = @workspace ORDER BY ord", connection)
                        command.Parameters.AddWithValue("@workspace", workspace)
                        Using reader = command.ExecuteReader()
                            While reader.Read()
                                result.Add(New RobloxCacheAssetEntry With {
                                    .Hash = reader.GetString(0),
                                    .FileType = CType(reader.GetInt32(1), RobloxCacheFileType),
                                    .Size = reader.GetInt64(2),
                                    .IsInline = reader.GetInt32(3) <> 0,
                                    .DurationSeconds = If(reader.IsDBNull(4), CType(Nothing, Double?), reader.GetDouble(4))})
                            End While
                        End Using
                    End Using
                End Using
            End SyncLock
        Catch
            Return New List(Of RobloxCacheAssetEntry)()
        End Try
        AssetNameStore.ApplySavedNames(result)
        Return result
    End Function

    ' --- Meshes ----------------------------------------------------------------------------------
    Public Shared Sub SaveMeshes(entries As IEnumerable(Of MeshCacheEntry))
        Dim list = entries.ToList()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using transaction = connection.BeginTransaction()
                        Using delete As New SQLiteCommand("DELETE FROM scan_meshes", connection, transaction)
                            delete.ExecuteNonQuery()
                        End Using
                        Using command As New SQLiteCommand("INSERT INTO scan_meshes (ord, hash, version, size, is_inline) VALUES (@ord, @hash, @version, @size, @inline)", connection, transaction)
                            command.Parameters.Add("@ord", DbType.Int32)
                            command.Parameters.Add("@hash", DbType.String)
                            command.Parameters.Add("@version", DbType.String)
                            command.Parameters.Add("@size", DbType.Int64)
                            command.Parameters.Add("@inline", DbType.Int32)
                            For ord = 0 To list.Count - 1
                                Dim entry = list(ord)
                                command.Parameters("@ord").Value = ord
                                command.Parameters("@hash").Value = entry.Hash
                                command.Parameters("@version").Value = If(CObj(entry.Version), DBNull.Value)
                                command.Parameters("@size").Value = entry.Size
                                command.Parameters("@inline").Value = If(entry.IsInline, 1, 0)
                                command.ExecuteNonQuery()
                            Next
                        End Using
                        transaction.Commit()
                    End Using
                End Using
            End SyncLock
        Catch
        End Try
    End Sub

    Public Shared Function LoadMeshes() As List(Of MeshCacheEntry)
        Dim result As New List(Of MeshCacheEntry)()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using command As New SQLiteCommand("SELECT hash, version, size, is_inline FROM scan_meshes ORDER BY ord", connection)
                        Using reader = command.ExecuteReader()
                            While reader.Read()
                                result.Add(New MeshCacheEntry With {
                                    .Hash = reader.GetString(0),
                                    .Version = If(reader.IsDBNull(1), Nothing, reader.GetString(1)),
                                    .Size = reader.GetInt64(2),
                                    .IsInline = reader.GetInt32(3) <> 0})
                            End While
                        End Using
                    End Using
                End Using
            End SyncLock
        Catch
            Return New List(Of MeshCacheEntry)()
        End Try
        AssetNameStore.ApplySavedNames(result)
        Return result
    End Function

    ' --- Videos ---------------------------------------------------------------------------------
    Public Shared Sub SaveVideos(entries As IEnumerable(Of RobloxVideoEntry))
        Dim list = entries.ToList()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using transaction = connection.BeginTransaction()
                        Using deleteSegments As New SQLiteCommand("DELETE FROM scan_video_segments", connection, transaction)
                            deleteSegments.ExecuteNonQuery()
                        End Using
                        Using deleteVideos As New SQLiteCommand("DELETE FROM scan_videos", connection, transaction)
                            deleteVideos.ExecuteNonQuery()
                        End Using
                        Using videoCommand As New SQLiteCommand("INSERT INTO scan_videos (ord, playlist_hash, source_path, resolution, duration, size, expected_segments) VALUES (@ord, @hash, @source, @resolution, @duration, @size, @expected)", connection, transaction),
                              segmentCommand As New SQLiteCommand("INSERT INTO scan_video_segments (video_ord, ord, hash, size, is_inline, payload_offset, payload_length, file_name, duration, source_key) VALUES (@video_ord, @ord, @hash, @size, @inline, @offset, @length, @file, @duration, @source)", connection, transaction)
                            For videoOrd = 0 To list.Count - 1
                                Dim entry = list(videoOrd)
                                videoCommand.Parameters.Clear()
                                videoCommand.Parameters.AddWithValue("@ord", videoOrd)
                                videoCommand.Parameters.AddWithValue("@hash", entry.PlaylistHash)
                                videoCommand.Parameters.AddWithValue("@source", entry.SourcePath)
                                videoCommand.Parameters.AddWithValue("@resolution", entry.Resolution)
                                videoCommand.Parameters.AddWithValue("@duration", entry.DurationSeconds)
                                videoCommand.Parameters.AddWithValue("@size", entry.Size)
                                videoCommand.Parameters.AddWithValue("@expected", entry.ExpectedSegments)
                                videoCommand.ExecuteNonQuery()

                                For segmentOrd = 0 To entry.Segments.Count - 1
                                    Dim segment = entry.Segments(segmentOrd)
                                    segmentCommand.Parameters.Clear()
                                    segmentCommand.Parameters.AddWithValue("@video_ord", videoOrd)
                                    segmentCommand.Parameters.AddWithValue("@ord", segmentOrd)
                                    segmentCommand.Parameters.AddWithValue("@hash", segment.Hash)
                                    segmentCommand.Parameters.AddWithValue("@size", segment.Size)
                                    segmentCommand.Parameters.AddWithValue("@inline", If(segment.IsInline, 1, 0))
                                    segmentCommand.Parameters.AddWithValue("@offset", segment.PayloadOffset)
                                    segmentCommand.Parameters.AddWithValue("@length", segment.PayloadLength)
                                    segmentCommand.Parameters.AddWithValue("@file", segment.FileName)
                                    segmentCommand.Parameters.AddWithValue("@duration", segment.DurationSeconds)
                                    segmentCommand.Parameters.AddWithValue("@source", segment.SourceKey)
                                    segmentCommand.ExecuteNonQuery()
                                Next
                            Next
                        End Using
                        transaction.Commit()
                    End Using
                End Using
            End SyncLock
        Catch
        End Try
    End Sub

    Public Shared Function LoadVideos() As List(Of RobloxVideoEntry)
        Dim result As New List(Of RobloxVideoEntry)()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Dim byOrder As New Dictionary(Of Integer, RobloxVideoEntry)()
                    Using command As New SQLiteCommand("SELECT ord, playlist_hash, source_path, resolution, duration, size, expected_segments FROM scan_videos ORDER BY ord", connection)
                        Using reader = command.ExecuteReader()
                            While reader.Read()
                                Dim order = reader.GetInt32(0)
                                Dim entry As New RobloxVideoEntry With {
                                    .PlaylistHash = reader.GetString(1),
                                    .SourcePath = reader.GetString(2),
                                    .Resolution = reader.GetString(3),
                                    .DurationSeconds = reader.GetDouble(4),
                                    .Size = reader.GetInt64(5),
                                    .ExpectedSegments = reader.GetInt32(6)}
                                byOrder(order) = entry
                                result.Add(entry)
                            End While
                        End Using
                    End Using
                    Using command As New SQLiteCommand("SELECT video_ord, hash, size, is_inline, payload_offset, payload_length, file_name, duration, source_key FROM scan_video_segments ORDER BY video_ord, ord", connection)
                        Using reader = command.ExecuteReader()
                            While reader.Read()
                                Dim entry As RobloxVideoEntry = Nothing
                                If Not byOrder.TryGetValue(reader.GetInt32(0), entry) Then Continue While
                                entry.Segments.Add(New VideoSegmentEntry With {
                                    .Hash = reader.GetString(1),
                                    .Size = reader.GetInt64(2),
                                    .IsInline = reader.GetInt32(3) <> 0,
                                    .PayloadOffset = reader.GetInt32(4),
                                    .PayloadLength = reader.GetInt32(5),
                                    .FileName = reader.GetString(6),
                                    .DurationSeconds = reader.GetDouble(7),
                                    .SourceKey = reader.GetString(8)})
                            End While
                        End Using
                    End Using
                End Using
            End SyncLock
        Catch
            Return New List(Of RobloxVideoEntry)()
        End Try
        AssetNameStore.ApplySavedNames(result)
        Return result
    End Function
    ' --- Supplemental: thumbnails, fonts, metadata -----------------------------------------------
    Public Shared Sub SaveSupplemental(workspace As String, entries As IEnumerable(Of SupplementalCacheEntry))
        Dim list = entries.ToList()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using transaction = connection.BeginTransaction()
                        DeleteWorkspace(connection, transaction, "scan_supplemental", workspace)
                        Using command As New SQLiteCommand("INSERT INTO scan_supplemental (workspace, ord, hash, file_type, size, is_inline, sub_category, payload_offset, payload_length, display_name) VALUES (@workspace, @ord, @hash, @type, @size, @inline, @category, @offset, @length, @display)", connection, transaction)
                            command.Parameters.Add("@workspace", DbType.String)
                            command.Parameters.Add("@ord", DbType.Int32)
                            command.Parameters.Add("@hash", DbType.String)
                            command.Parameters.Add("@type", DbType.Int32)
                            command.Parameters.Add("@size", DbType.Int64)
                            command.Parameters.Add("@inline", DbType.Int32)
                            command.Parameters.Add("@category", DbType.Int32)
                            command.Parameters.Add("@offset", DbType.Int32)
                            command.Parameters.Add("@length", DbType.Int32)
                            command.Parameters.Add("@display", DbType.String)
                            For ord = 0 To list.Count - 1
                                Dim entry = list(ord)
                                command.Parameters("@workspace").Value = workspace
                                command.Parameters("@ord").Value = ord
                                command.Parameters("@hash").Value = entry.Hash
                                command.Parameters("@type").Value = CInt(entry.FileType)
                                command.Parameters("@size").Value = entry.Size
                                command.Parameters("@inline").Value = If(entry.IsInline, 1, 0)
                                command.Parameters("@category").Value = entry.Category
                                command.Parameters("@offset").Value = entry.PayloadOffset
                                command.Parameters("@length").Value = entry.PayloadLength
                                command.Parameters("@display").Value = If(CObj(entry.DisplayName), DBNull.Value)
                                command.ExecuteNonQuery()
                            Next
                        End Using
                        transaction.Commit()
                    End Using
                End Using
            End SyncLock
        Catch
        End Try
    End Sub

    Public Shared Function LoadSupplemental(workspace As String) As List(Of SupplementalCacheEntry)
        Dim result As New List(Of SupplementalCacheEntry)()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using command As New SQLiteCommand("SELECT hash, file_type, size, is_inline, sub_category, payload_offset, payload_length, display_name FROM scan_supplemental WHERE workspace = @workspace ORDER BY ord", connection)
                        command.Parameters.AddWithValue("@workspace", workspace)
                        Using reader = command.ExecuteReader()
                            While reader.Read()
                                result.Add(New SupplementalCacheEntry With {
                                    .Hash = reader.GetString(0),
                                    .FileType = CType(reader.GetInt32(1), SupplementalCacheType),
                                    .Size = reader.GetInt64(2),
                                    .IsInline = reader.GetInt32(3) <> 0,
                                    .Category = reader.GetInt32(4),
                                    .PayloadOffset = reader.GetInt32(5),
                                    .PayloadLength = reader.GetInt32(6),
                                    .DisplayName = If(reader.IsDBNull(7), Nothing, reader.GetString(7))})
                            End While
                        End Using
                    End Using
                End Using
            End SyncLock
        Catch
            Return New List(Of SupplementalCacheEntry)()
        End Try
        AssetNameStore.ApplySavedNames(result)
        Return result
    End Function

    ''' <summary>Remove all persisted scan state (used when the Roblox cache is cleared).</summary>
    Public Shared Sub ClearAll()
        Try
            SyncLock AppDatabase.SyncRoot
                Using connection = AppDatabase.OpenConnection()
                    Using command As New SQLiteCommand("DELETE FROM scan_cache_assets; DELETE FROM scan_meshes; DELETE FROM scan_videos; DELETE FROM scan_video_segments; DELETE FROM scan_supplemental;", connection)
                        command.ExecuteNonQuery()
                    End Using
                End Using
            End SyncLock
        Catch
        End Try
    End Sub

    Private Shared Sub DeleteWorkspace(connection As SQLiteConnection, transaction As SQLiteTransaction, table As String, workspace As String)
        Using command As New SQLiteCommand($"DELETE FROM {table} WHERE workspace = @workspace", connection, transaction)
            command.Parameters.AddWithValue("@workspace", workspace)
            command.ExecuteNonQuery()
        End Using
    End Sub
End Class

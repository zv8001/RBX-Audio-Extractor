Imports System.Data.SQLite
Imports System.IO
Imports System.Security.Cryptography

Public Interface IRenamableAsset
    ReadOnly Property CacheKey As String
    Property CustomName As String
    Property ContentSha256 As String
    ReadOnly Property FriendlyName As String
    ReadOnly Property ExportBaseName As String
End Interface

Public NotInheritable Class AssetNameStore
    Private Shared ReadOnly SyncRoot As New Object()

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property DataDirectory As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RBXAssetExtractor")
        End Get
    End Property

    Public Shared ReadOnly Property DatabasePath As String
        Get
            Return Path.Combine(DataDirectory, "RBXAssetExtractor.db")
        End Get
    End Property

    Public Shared Sub ApplySavedNames(Of T As IRenamableAsset)(items As IEnumerable(Of T))
        If items Is Nothing OrElse Not File.Exists(DatabasePath) Then Return
        Dim saved As New Dictionary(Of String, (Name As String, Sha256 As String))(StringComparer.OrdinalIgnoreCase)
        SyncLock SyncRoot
            Using connection = OpenConnection()
                Using command As New SQLiteCommand("SELECT cache_key, custom_name, sha256 FROM asset_names ORDER BY updated_utc", connection)
                    Using reader = command.ExecuteReader()
                        While reader.Read()
                            saved(reader.GetString(0)) = (reader.GetString(1), reader.GetString(2))
                        End While
                    End Using
                End Using
            End Using
        End SyncLock
        For Each item In items
            Dim value As (Name As String, Sha256 As String) = Nothing
            If saved.TryGetValue(item.CacheKey, value) Then
                item.CustomName = value.Name
                item.ContentSha256 = value.Sha256
            End If
        Next
    End Sub

    Public Shared Async Function PromptAndSaveAsync(owner As Window, asset As IRenamableAsset, payloadLoader As Func(Of Byte())) As Task(Of Boolean)
        If asset Is Nothing Then Return False
        Dim dialog As New AssetRenameDialog(asset.FriendlyName, asset.CustomName) With {.Owner = owner}
        If dialog.ShowDialog() <> True Then Return False

        Dim requestedName = NormalizeName(dialog.AssetName)
        AppServices.Report(If(requestedName.Length = 0, "Removing saved asset name...", "Saving asset name..."), 0, True)
        Try
            Await Task.Run(
                Sub()
                    If requestedName.Length = 0 Then
                        DeleteName(asset)
                    Else
                        If payloadLoader Is Nothing Then Throw New InvalidOperationException("This asset cannot be fingerprinted.")
                        SaveName(asset, requestedName, payloadLoader())
                    End If
                End Sub)
            AppServices.Report(If(requestedName.Length = 0, "Saved name removed.", $"Saved name: {requestedName}"), 100)
            Return True
        Catch ex As Exception
            AppServices.Report($"Could not save asset name: {ex.Message}")
            AppDialog.Show(ex.Message, "Rename failed", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, owner)
            Return False
        End Try
    End Function

    Public Shared Function MakeSafeFileName(value As String, fallback As String) As String
        Dim source = If(String.IsNullOrWhiteSpace(value), fallback, value).Trim()
        Dim invalid = Path.GetInvalidFileNameChars()
        Dim result = New String(source.Select(Function(character) If(Array.IndexOf(invalid, character) >= 0 OrElse Char.IsControl(character), "_"c, character)).ToArray())
        result = result.Trim(" "c, "."c, "_"c)
        If result.Length > 120 Then result = result.Substring(0, 120).TrimEnd(" "c, "."c)
        Return If(String.IsNullOrWhiteSpace(result), fallback, result)
    End Function

    Public Shared Function GetBatchBaseName(asset As IRenamableAsset, usedNames As ISet(Of String)) As String
        Dim baseName = asset.ExportBaseName
        If usedNames Is Nothing OrElse usedNames.Add(baseName) Then Return baseName
        Dim suffix = If(asset.CacheKey.Length > 10, asset.CacheKey.Substring(0, 10), asset.CacheKey)
        Dim candidate = MakeSafeFileName(baseName & "_" & suffix, suffix)
        Dim counter = 2
        While Not usedNames.Add(candidate)
            candidate = MakeSafeFileName(baseName & "_" & suffix & "_" & counter.ToString(), suffix)
            counter += 1
        End While
        Return candidate
    End Function

    Public Shared Sub ClearLoadedNames(Of T As IRenamableAsset)(items As IEnumerable(Of T))
        If items Is Nothing Then Return
        For Each item In items
            item.CustomName = String.Empty
            item.ContentSha256 = String.Empty
        Next
    End Sub

    Public Shared Function GetDataSize() As (Size As Long, Files As Integer)
        If Not Directory.Exists(DataDirectory) Then Return (0, 0)
        Dim size As Long
        Dim count As Integer
        For Each path In Directory.EnumerateFiles(DataDirectory, "*", SearchOption.AllDirectories)
            Try
                size += New FileInfo(path).Length
                count += 1
            Catch
            End Try
        Next
        Return (size, count)
    End Function

    Public Shared Sub ClearAllApplicationData()
        SyncLock SyncRoot
            SQLiteConnection.ClearAllPools()
            If Directory.Exists(DataDirectory) Then Directory.Delete(DataDirectory, recursive:=True)
        End SyncLock
    End Sub

    Private Shared Sub SaveName(asset As IRenamableAsset, customName As String, payload As Byte())
        If payload Is Nothing Then Throw New InvalidDataException("The cached asset payload is unavailable.")
        Dim fingerprint = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant()
        SyncLock SyncRoot
            Using connection = OpenConnection()
                Using transaction = connection.BeginTransaction()
                    Using deleteCommand As New SQLiteCommand("DELETE FROM asset_names WHERE sha256 = @sha256 OR cache_key = @cache_key", connection, transaction)
                        deleteCommand.Parameters.AddWithValue("@sha256", fingerprint)
                        deleteCommand.Parameters.AddWithValue("@cache_key", asset.CacheKey)
                        deleteCommand.ExecuteNonQuery()
                    End Using
                    Using insertCommand As New SQLiteCommand("INSERT INTO asset_names (sha256, cache_key, custom_name, updated_utc) VALUES (@sha256, @cache_key, @custom_name, @updated_utc)", connection, transaction)
                        insertCommand.Parameters.AddWithValue("@sha256", fingerprint)
                        insertCommand.Parameters.AddWithValue("@cache_key", asset.CacheKey)
                        insertCommand.Parameters.AddWithValue("@custom_name", customName)
                        insertCommand.Parameters.AddWithValue("@updated_utc", DateTimeOffset.UtcNow.ToString("O"))
                        insertCommand.ExecuteNonQuery()
                    End Using
                    transaction.Commit()
                End Using
            End Using
        End SyncLock
        asset.ContentSha256 = fingerprint
        asset.CustomName = customName
    End Sub

    Private Shared Sub DeleteName(asset As IRenamableAsset)
        SyncLock SyncRoot
            If File.Exists(DatabasePath) Then
                Using connection = OpenConnection()
                    Using command As New SQLiteCommand("DELETE FROM asset_names WHERE cache_key = @cache_key OR sha256 = @sha256", connection)
                        command.Parameters.AddWithValue("@cache_key", asset.CacheKey)
                        command.Parameters.AddWithValue("@sha256", If(asset.ContentSha256, String.Empty))
                        command.ExecuteNonQuery()
                    End Using
                End Using
            End If
        End SyncLock
        asset.CustomName = String.Empty
        asset.ContentSha256 = String.Empty
    End Sub

    Private Shared Function OpenConnection() As SQLiteConnection
        Directory.CreateDirectory(DataDirectory)
        Dim connection As New SQLiteConnection($"Data Source={DatabasePath};Version=3;")
        connection.Open()
        Using command As New SQLiteCommand("CREATE TABLE IF NOT EXISTS asset_names (sha256 TEXT PRIMARY KEY NOT NULL, cache_key TEXT NOT NULL, custom_name TEXT NOT NULL, updated_utc TEXT NOT NULL); CREATE INDEX IF NOT EXISTS idx_asset_names_cache_key ON asset_names(cache_key);", connection)
            command.ExecuteNonQuery()
        End Using
        Return connection
    End Function

    Private Shared Function NormalizeName(value As String) As String
        Dim result = If(value, String.Empty).Trim()
        If result.Length > 120 Then result = result.Substring(0, 120).Trim()
        Return result
    End Function
End Class
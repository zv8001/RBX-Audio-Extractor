Imports System.Data.SQLite
Imports System.IO

''' <summary>
''' Single owner of the RBXAssetExtractor.db SQLite database: connection, schema, and the shared
''' lock used by every store that writes to it (saved names, settings, persisted scan state).
''' </summary>
Public NotInheritable Class AppDatabase
    Friend Shared ReadOnly SyncRoot As New Object()

    Private Sub New()
    End Sub

    Private Const SchemaSql As String =
        "CREATE TABLE IF NOT EXISTS asset_names (sha256 TEXT PRIMARY KEY NOT NULL, cache_key TEXT NOT NULL, custom_name TEXT NOT NULL, updated_utc TEXT NOT NULL);" &
        "CREATE INDEX IF NOT EXISTS idx_asset_names_cache_key ON asset_names(cache_key);" &
        "CREATE TABLE IF NOT EXISTS settings (key TEXT PRIMARY KEY NOT NULL, value TEXT);" &
        "CREATE TABLE IF NOT EXISTS scan_cache_assets (workspace TEXT NOT NULL, ord INTEGER NOT NULL, hash TEXT NOT NULL, file_type INTEGER NOT NULL, size INTEGER NOT NULL, is_inline INTEGER NOT NULL, duration REAL);" &
        "CREATE INDEX IF NOT EXISTS idx_scan_cache_assets_workspace ON scan_cache_assets(workspace);" &
        "CREATE TABLE IF NOT EXISTS scan_meshes (ord INTEGER NOT NULL, hash TEXT NOT NULL, version TEXT, size INTEGER NOT NULL, is_inline INTEGER NOT NULL);" &
        "CREATE TABLE IF NOT EXISTS scan_supplemental (workspace TEXT NOT NULL, ord INTEGER NOT NULL, hash TEXT NOT NULL, file_type INTEGER NOT NULL, size INTEGER NOT NULL, is_inline INTEGER NOT NULL, sub_category INTEGER NOT NULL, payload_offset INTEGER NOT NULL, payload_length INTEGER NOT NULL, display_name TEXT);" &
        "CREATE INDEX IF NOT EXISTS idx_scan_supplemental_workspace ON scan_supplemental(workspace);"

    ''' <summary>Opens the application database, creating the file and schema if needed.</summary>
    Public Shared Function OpenConnection() As SQLiteConnection
        Directory.CreateDirectory(AssetNameStore.DataDirectory)
        Dim connection As New SQLiteConnection($"Data Source={AssetNameStore.DatabasePath};Version=3;BusyTimeout=5000;")
        connection.Open()
        Using command As New SQLiteCommand(SchemaSql, connection)
            command.ExecuteNonQuery()
        End Using
        Return connection
    End Function

    Public Shared Function GetSetting(key As String) As String
        SyncLock SyncRoot
            Using connection = OpenConnection()
                Using command As New SQLiteCommand("SELECT value FROM settings WHERE key = @key", connection)
                    command.Parameters.AddWithValue("@key", key)
                    Dim value = command.ExecuteScalar()
                    If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
                    Return Convert.ToString(value)
                End Using
            End Using
        End SyncLock
    End Function

    Public Shared Sub SetSetting(key As String, value As String)
        SyncLock SyncRoot
            Using connection = OpenConnection()
                If value Is Nothing Then
                    Using command As New SQLiteCommand("DELETE FROM settings WHERE key = @key", connection)
                        command.Parameters.AddWithValue("@key", key)
                        command.ExecuteNonQuery()
                    End Using
                Else
                    Using command As New SQLiteCommand("INSERT OR REPLACE INTO settings (key, value) VALUES (@key, @value)", connection)
                        command.Parameters.AddWithValue("@key", key)
                        command.Parameters.AddWithValue("@value", value)
                        command.ExecuteNonQuery()
                    End Using
                End If
            End Using
        End SyncLock
    End Sub
End Class

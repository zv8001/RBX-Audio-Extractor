Imports TagLib
Imports System.IO
Imports System.Security.Principal
Imports System.Numerics
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Net
Imports NVorbis
Imports ATL.AudioData
Imports ATL
Imports Microsoft.Win32
Imports System.Net.Http.Headers
Imports System.Security.Policy
Imports System.Text
Imports System.ComponentModel
Imports System.Diagnostics.CodeAnalysis
Public Class Form1

    Dim DisableFade2 As Boolean = False
    Dim DisableFade As Boolean = False
    Dim V = "v1.0.56"
    Private WithEvents backgroundWorker As New BackgroundWorker()
    Private codecInstallerUrl As String = "https://files2.codecguide.com/K-Lite_Codec_Pack_1880_Standard.exe"


    Private codecInstallerPath As String = "C:\Temp\klcp.exe"
    Dim tempDirectory = Path.GetTempPath

    Dim SelFile As String
    Dim Outdated As Boolean = False
    Private isDragging As Boolean = False
    Private startPoint As Point

    Function IsKLiteInstalled() As Boolean
        If My.Computer.FileSystem.DirectoryExists("C:\Program Files (x86)\K-Lite Codec Pack") Then
            Return True
        End If

        If My.Computer.FileSystem.DirectoryExists("C:\Program Files\K-Lite Codec Pack") Then
            Return True
        End If
        Return False
    End Function

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

    Function IsOggFile(filePath As String) As Boolean
        Try

            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                Dim header(3) As Byte
                fs.Read(header, 0, 4)



                Dim headerStr As String = System.Text.Encoding.ASCII.GetString(header)
                output_log.Items.Add(headerStr)
                If headerStr = "OggS" Then
                    Return True
                End If
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error reading file: {filePath} - {ex.Message}")
        End Try

        Return False
    End Function

    Private Sub LoadHTTP_DoWork_1(sender As Object, e As DoWorkEventArgs) Handles LoadHTTP0.DoWork
        Dim directories As String() = CType(e.Argument, String())
        Dim sourceDir As String = directories(0)
        Dim targetDir As String = directories(1)


        Dim files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)

        For i As Integer = 0 To files.Length - 1

            If LoadHTTP0.CancellationPending Then
                e.Cancel = True
                Exit For
            End If

            ProcessFile(files(i), targetDir)


            LoadHTTP0.ReportProgress(CInt((i + 1) / files.Length * 100))
        Next
    End Sub
    Private Sub UpdateLog(message As String)
        If output_log.InvokeRequired Then
            output_log.Invoke(New Action(Of String)(AddressOf UpdateLog), message)
        Else
            ' Add the provided message to the ListBox
            output_log.Items.Add(message)
        End If



    End Sub

    Private Sub ProcessFile(file As String, targetDir As String)
        UpdateLog($"Begin processing on : {file}")

        Try

            Dim fileBytes = System.IO.File.ReadAllBytes(file)
            Dim oggHeader = Encoding.ASCII.GetBytes("OggS")
            Dim oggIndex As Integer = -1


            For i As Integer = 0 To fileBytes.Length - oggHeader.Length
                If fileBytes.Skip(i).Take(oggHeader.Length).SequenceEqual(oggHeader) Then
                    oggIndex = i
                    Exit For
                End If
            Next


            If oggIndex >= 0 Then
                UpdateLog($"copying file: {file} To: {targetDir}")
                Dim fileName = Path.GetFileName(file)
                Dim targetPath = Path.Combine(targetDir, fileName)
                System.IO.File.Copy(file, targetPath, True)


                Dim modifiedBytes = fileBytes.Skip(oggIndex).ToArray()

                UpdateLog($"Removing default Roblox parameters")
                System.IO.File.WriteAllBytes(targetPath, modifiedBytes)
                UpdateLog($"done processing file: {file}")
            Else
                UpdateLog($"Skipping : {file} is not a valid OGG file.")

            End If
        Catch ex As Exception
            UpdateLog($"Error processing file: {file} - {ex.Message}")
            ' Console.WriteLine($"Error processing file: {file} - {ex.Message}")
        End Try
    End Sub


    Private Sub LoadHTTP0_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles LoadHTTP0.ProgressChanged

        ProgressBar1.Value = e.ProgressPercentage
    End Sub

    Private Sub CheckForUpdates()

        Dim content As String
        Dim url As String = "https://animated-platypus-6ba0a9.netlify.app/v.txt"
        Dim tempFile As String = System.IO.Path.GetTempFileName() ' Temporary file path

        output_log.Items.Add($"Downloading update check from server: {url}")

        Try
            My.Computer.Network.DownloadFile(url, tempFile, "", "", False, 10000, True)

            content = System.IO.File.ReadAllText(tempFile).Trim()
            System.IO.File.Delete(tempFile)

            If Not content = V Then
                Outdated = True
                Dim text As String = "The program is out of date. You have version: " & V & " and the latest available version is: " & content & ". Please download the latest version now at: https://github.com/zv8001/RBX-Audio-Extractor"
                output_log.Items.Add(text)
                MsgBox(text, 0 + 48, "Update detected!")
            Else
                output_log.Items.Add("You are using the latest version: " & V)
            End If
        Catch ex As Exception
            CallError(ex.Message)
        End Try

        If Not Outdated Then
            output_log.Items.Add($"The program is up to date currently running version: {V}")
        End If
    End Sub

    Public Sub CallError(Err1)
        output_log.Items.Add($"ERROR {Err1}")
        MsgBox($"ERROR {Err1}", 0 + 16, "ERROR")
    End Sub




    Private Sub InstallCodec()
        Dim isInstalled As Boolean = IsKLiteInstalled()

        If Not isInstalled Then
            Dim result As DialogResult = MessageBox.Show("This program requires a custom Codec please click yes to install", "bruj", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

            If result = DialogResult.No Then


            Else
                If isInstalled Then

                Else
                    Try
                        My.Computer.Network.DownloadFile(codecInstallerUrl, tempDirectory & "\K-Lite_Codec_Pack_1880_Standard.exe")
                    Catch ex As Exception

                    End Try

                    Try

                        Dim command As String = "cmd.exe /c " & tempDirectory & "\K-Lite_Codec_Pack_1880_Standard.exe /silent"

                        Dim process As New Process()


                        process.StartInfo.FileName = "cmd.exe"
                        process.StartInfo.Arguments = "/c " & command
                        process.StartInfo.RedirectStandardOutput = True
                        process.StartInfo.UseShellExecute = False
                        process.StartInfo.CreateNoWindow = False


                        process.Start()


                        Dim output As String = process.StandardOutput.ReadToEnd()
                        Console.WriteLine(output)


                        process.WaitForExit()
                    Catch ex As Exception
                        CallError("Automatic download failed with the error:  " & ex.Message & "")

                        Try
                            Process.Start("C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "https://files2.codecguide.com/K-Lite_Codec_Pack_1880_Standard.exe")
                        Catch ex2 As Exception
                            CallError(ex.Message)
                        End Try
                    End Try
                End If


            End If

        End If


    End Sub

    Private Function IsRunningAsAdministrator() As Boolean

        Dim user As WindowsIdentity = WindowsIdentity.GetCurrent()


        Dim principal As New WindowsPrincipal(user)
        Return principal.IsInRole(WindowsBuiltInRole.Administrator)
    End Function

    Sub CloneDirectory(sourceDir As String, targetDir As String)

        If Not Directory.Exists(sourceDir) Then
            output_log.Items.Add($"Source directory does not exist: {sourceDir}")
        End If


        If Not Directory.Exists(targetDir) Then
            Directory.CreateDirectory(targetDir)
        End If


        For Each file As String In Directory.GetFiles(sourceDir)
            Try
                Dim fileName As String = Path.GetFileName(file)
                Dim destFile As String = Path.Combine(targetDir, fileName)

                output_log.Items.Add($"Cloning: {fileName} Into: {destFile}")


                System.IO.File.Copy(file, destFile, True)
            Catch ex As Exception
                CallError(ex.ToString)
            End Try

        Next


        For Each subDir As String In Directory.GetDirectories(sourceDir)
            Try
                Dim subDirName As String = Path.GetFileName(subDir)
                Dim destSubDir As String = Path.Combine(targetDir, subDirName)
                output_log.Items.Add($"Cloning (subdirectory): {subDirName} Into: {destSubDir}")
                CloneDirectory(subDir, destSubDir)
            Catch ex As Exception
                CallError(ex.ToString)
            End Try
        Next
    End Sub

    Sub RenameFileExtensions(directoryPath As String, newExtension As String)

        If Not Directory.Exists(directoryPath) Then
            '  output_log.Items.Add($"ERROR: Directory does not exist: {directoryPath}")
            CallError($"ERROR: Directory does not exist: {directoryPath}")
            'Throw New DirectoryNotFoundException($"Directory does not exist: {directoryPath}")
        End If


        For Each file As String In Directory.GetFiles(directoryPath)

            Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(file)




            Dim newFileName As String = Path.Combine(directoryPath, fileNameWithoutExt & newExtension)


            output_log.Items.Add($"Renaming file extension: {file} To {newFileName} ")

            Try
                System.IO.File.Move(file, newFileName, True)
            Catch ex As Exception

            End Try

        Next
    End Sub




    Sub DeleteAllFiles(directoryPath As String)

        If Not Directory.Exists(directoryPath) Then
            CallError($"ERROR: Directory does not exist: {directoryPath}")
        End If


        For Each file As String In Directory.GetFiles(directoryPath)

            Try
                output_log.Items.Add($"Deleting file: {file}")
                System.IO.File.Delete(file)
            Catch ex As Exception
                output_log.Items.Add($"Failed to Delete file with error: {ex}")
            End Try

        Next
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles LoadHTTP_BTN.Click
        Dim tempDirectory = Path.GetTempPath
        If Not My.Computer.FileSystem.FileExists(tempDirectory & "\RBX_SOUND_RIPPER") Then
            My.Computer.FileSystem.CreateDirectory(tempDirectory & "\RBX_SOUND_RIPPER")
        End If

        DeleteAllFiles(tempDirectory & "\RBX_SOUND_RIPPER")
        ' CopyOggFiles("C:\Users\denve\AppData\Local\Temp\Roblox\http", tempDirectory & "\RBX_SOUND_RIPPER")

        CloneDirectory(tempDirectory & "\Roblox\sounds", tempDirectory & "\RBX_SOUND_RIPPER")
        RenameFileExtensions(tempDirectory & "\RBX_SOUND_RIPPER", ".ogg")

        Sounds_Listbox.Items.Clear()


        Dim musicFiles = Directory.GetFiles(tempDirectory & "\RBX_SOUND_RIPPER", "*.ogg")

        For Each file In musicFiles
            Dim InfoLoaded = True

            Dim track As New Track(file)

            Dim title = track.Title
            Dim Album = track.Album
            Dim Artist = track.Artist

            If String.IsNullOrEmpty(Album) Then
                InfoLoaded = False
            End If

            Try

                If InfoLoaded Then
                    Sounds_Listbox.Items.Insert(0, New With {.DisplayText = $"" & title & " | Artists: " & Artist & " | Album: " & Album & "", .FilePath = file})
                Else
                    Sounds_Listbox.Items.Add(New With {.DisplayText = $"" & Path.GetFileName(file) & "", .FilePath = file})
                End If
            Catch ex As Exception
                CallError($"Error reading file: {file} - {ex.Message}")
                ' output_log.Items.Add($"Error reading file: {file} - {ex.Message}")
            End Try
        Next


        Sounds_Listbox.DisplayMember = "DisplayText"
        Sounds_Listbox.ValueMember = "FilePath"

    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles output_log.SelectedIndexChanged

    End Sub

    Public Sub SaveFile()
        Dim saveFileDialog As New SaveFileDialog


        saveFileDialog.Filter = "OGG Files (*.ogg)|*.ogg"

        If saveFileDialog.ShowDialog = DialogResult.OK Then

            Dim filePath = saveFileDialog.FileName

            Try
                My.Computer.FileSystem.CopyFile(SelFile, filePath)
            Catch ex As Exception
                CallError(ex.Message)
                'MsgBox(ex.Message, 0 + 16, "Woops")
            End Try

        End If
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Download_BTN.Click
        SaveFile()
    End Sub





    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles ClearTMP_BTN.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to clear the temp directory? You will you will lose access to all of the audios your Roblox client has saved", "You sure?", MessageBoxButtons.YesNo)
        Dim tempDirectory = Path.GetTempPath
        If result = DialogResult.Yes Then
            DeleteAllFiles(tempDirectory & "\Roblox\sounds")
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Panel3.BringToFront()
        ' TabControl1.Appearance = TabAppearance.FlatButtons
        LoadHTTP0.WorkerReportsProgress = True

        '  TabControl1.SizeMode = TabSizeMode.Fixed





        VText_LBR.Text = "Currently running version: " & V

        InstallCodec()
        Dim isInstalled As Boolean = IsKLiteInstalled()

        If Not isInstalled Then
            MsgBox("This program requires K-Lite_Codec_Pack please install it and try again.", 0 + 16, "balls")
            Me.Close()
            KillProcessByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath))
        End If
        CheckForUpdates()

        Me.Opacity = 0


        fadeInTimer.Interval = 50
        fadeInTimer.Start()


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
        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select a folder"
            folderDialog.ShowNewFolderButton = True

            If folderDialog.ShowDialog() = DialogResult.OK Then
                CloneDirectory(tempDirectory & "\RBX_SOUND_RIPPER", folderDialog.SelectedPath)
            End If
        End Using
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles AlwaysOnTopCHK.CheckedChanged
        If AlwaysOnTopCHK.Checked Then
            Me.TopMost = True
            DisableFade2 = True
        Else
            DisableFade2 = False
            Me.TopMost = False
        End If
    End Sub

    Private Sub ListBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Sounds_Listbox.SelectedIndexChanged
        If Sounds_Listbox.SelectedItem IsNot Nothing Then

            Try
                Dim selectedFile = DirectCast(Sounds_Listbox.SelectedItem, Object).FilePath
                SelFile = selectedFile
                AxWindowsMediaPlayer1.URL = SelFile
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

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Try
            Process.Start("C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "http://zv800.com/")
        Catch ex As Exception
            CallError(ex.Message)
        End Try
    End Sub

    Private Sub LoadHTTP0_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles LoadHTTP0.RunWorkerCompleted

        'CopyAndModifyOggFiles("C:\Users\denve\AppData\Local\Temp\Roblox\http", tempDirectory & "\RBX_SOUND_RIPPER_HTTP")

        RenameFileExtensions(tempDirectory & "\RBX_SOUND_RIPPER_HTTP", ".ogg")

        HTTPLISTBOX.Items.Clear()

        Dim musicFiles = Directory.GetFiles(tempDirectory & "\RBX_SOUND_RIPPER_HTTP", "*.ogg")

        For Each file In musicFiles
            Dim InfoLoaded = True

            Dim track As New Track(file)

            Dim title = track.Title
            Dim Album = track.Album
            Dim Artist = track.Artist

            If String.IsNullOrEmpty(Album) Then
                InfoLoaded = False
            End If

            Try

                If InfoLoaded Then
                    HTTPLISTBOX.Items.Insert(0, New With {.DisplayText = $"" & title & " | Artists: " & Artist & " | Album: " & Album & "", .FilePath = file})
                Else
                    HTTPLISTBOX.Items.Add(New With {.DisplayText = $"" & Path.GetFileName(file) & "", .FilePath = file})
                End If
            Catch ex As Exception
                CallError($"Error reading file: {file} - {ex.Message}")
            End Try
        Next


        HTTPLISTBOX.DisplayMember = "DisplayText"
        HTTPLISTBOX.ValueMember = "FilePath"
        LoadHttpBtn.Enabled = True
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles LoadHttpBtn.Click
        Dim result As DialogResult = MessageBox.Show("It is highly recommended you clear the temp folder before each game considering audio files from previous sessions will remain and it may take a long time to process. Do you still want to continue?", "Do you still want to continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

        If result = DialogResult.Yes Then
            Dim tempDirectory = Path.GetTempPath
            If Not My.Computer.FileSystem.FileExists(tempDirectory & "\RBX_SOUND_RIPPER_HTTP") Then
                My.Computer.FileSystem.CreateDirectory(tempDirectory & "\RBX_SOUND_RIPPER_HTTP")
            End If

            DeleteAllFiles(tempDirectory & "\RBX_SOUND_RIPPER_HTTP")
            Try
                LoadHttpBtn.Enabled = False
                LoadHTTP0.RunWorkerAsync(New String() {tempDirectory & "\Roblox\http", tempDirectory & "\RBX_SOUND_RIPPER_HTTP"})
            Catch ex As Exception
                LoadHttpBtn.Enabled = True
                CallError(ex)
            End Try

        End If


    End Sub

    Private Sub ListBox1_SelectedIndexChanged_1(sender As Object, e As EventArgs) Handles HTTPLISTBOX.SelectedIndexChanged
        If HTTPLISTBOX.SelectedItem IsNot Nothing Then

            Try
                Dim selectedFile = DirectCast(HTTPLISTBOX.SelectedItem, Object).FilePath
                SelFile = selectedFile
                AxWindowsMediaPlayer1.URL = SelFile
            Catch ex As Exception
                CallError(ex.Message)
            End Try

        End If
    End Sub

    Private Sub DOWNLOADHTTP_BTN_Click(sender As Object, e As EventArgs) Handles DOWNLOADHTTP_BTN.Click
        SaveFile()
    End Sub

    Private Sub ClearHTTPTEMP_BTN_Click(sender As Object, e As EventArgs) Handles ClearHTTPTEMP_BTN.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to clear the temp directory? You will you will lose access to all of the audios your Roblox client has saved", "You sure?", MessageBoxButtons.YesNo)
        Dim tempDirectory = Path.GetTempPath
        If result = DialogResult.Yes Then
            MsgBox("The program may freeze temporarily this is normal do not close", 0 + 64)
            DeleteAllFiles(tempDirectory & "\Roblox\http")
        End If
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

    Private Sub DownloadAllHTTP_BTN_Click(sender As Object, e As EventArgs) Handles DownloadAllHTTP_BTN.Click
        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select a folder"
            folderDialog.ShowNewFolderButton = True

            If folderDialog.ShowDialog() = DialogResult.OK Then
                CloneDirectory(tempDirectory & "\RBX_SOUND_RIPPER_HTTP", folderDialog.SelectedPath)
            End If
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
        Me.WindowState = WindowState.Minimized
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
        Try
            Process.Start("C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "https://github.com/zv8001/RBX-Audio-Extractor")
        Catch ex As Exception
            CallError(ex.Message)
        End Try
    End Sub

    Private Sub SaveLogBtn_Click(sender As Object, e As EventArgs) Handles SaveLogBtn.Click
        Dim SFD As New SaveFileDialog

        SFD.Filter = "LOG Files (*.log)|*.log"
        If SFD.ShowDialog() = DialogResult.OK Then

            Try
                Dim listBoxItems As New List(Of String)()

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
End Class

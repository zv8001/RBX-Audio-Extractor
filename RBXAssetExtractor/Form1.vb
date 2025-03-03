﻿
Imports System.IO
Imports System.Security.Principal
Imports System.Numerics
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Net
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
Imports ATL.AudioData
Imports ATL
Imports Microsoft.Win32
Imports System.Net.Http.Headers
Imports System.Security.Policy
Imports System.Text
Imports System.ComponentModel
Imports System.Diagnostics.CodeAnalysis
Imports System.Net.Http
Imports System.Reflection
Imports System.Reflection.Metadata
Imports System.Drawing
Imports TagLib
Imports NAudio.Wave
Imports NAudio.Vorbis
Public Class MainForm

    Dim DisableFade2 As Boolean = False
    Dim DisableFade As Boolean = False
    Dim V = "v1.1.50"
    Private WithEvents backgroundWorker As New BackgroundWorker()
    Dim Stage As Integer = 0
    Dim tempDirectory = Path.GetTempPath
    Dim Playing As Boolean = False
    Dim SelFile As String
    Dim Outdated As Boolean = False
    Private isDragging As Boolean = False
    Private startPoint As Point
    Dim HTTPDONE = True

    Private Function CheckForRoblox()
        If My.Computer.FileSystem.DirectoryExists($"{tempDirectory}\Roblox") Then
            Return True
        Else
            CallError("Roblox tempDirectory not found")
            Return False
        End If

    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        TaskLBR.Visible = False
        LoadHTTP0.WorkerReportsProgress = True
        VText_LBR.Text = V
        CheckForUpdates()
        Me.Opacity = 0
        fadeInTimer.Interval = 50
        fadeInTimer.Start()

        MSGPopup.RunWorkerAsync()

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

    Public Function CheckForUpdates()
        Dim content As String
        Dim url As String = "https://animated-platypus-6ba0a9.netlify.app/v.txt"

        output_log.Items.Add($"Checking for updates from server: {url}")

        Try
            Using client As New HttpClient()
                content = client.GetStringAsync(url).GetAwaiter().GetResult().Trim()
            End Using


            If Not String.Equals(content, V, StringComparison.OrdinalIgnoreCase) Then
                Outdated = True
                Dim text As String = $"The program is out of date. You have version: {V} and the latest available version is: {content}. would you like to automatically download the update?"
                output_log.Items.Add(text)
                ' MsgBox(text, MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation, "Update detected!")

                Dim Update As DialogResult = MessageBox.Show(text, "BRUJ", MessageBoxButtons.YesNo)

                If Update = DialogResult.Yes Then
                    DownloadUpdate()
                End If


            Else
                output_log.Items.Add($"You are using the latest version: {V}")
            End If
            Return True
        Catch ex As Exception
            Return False
            output_log.Items.Add($"Error checking for updates: {ex.Message}")
        End Try

        If Not Outdated Then
            output_log.Items.Add($"The program is up to date, currently running version: {V}")
        End If
    End Function



    Dim DisableTimer019 = False

    Private Sub LoadFullGame()
        If CheckForRoblox() Then
            If ProgressBar1.Value = 0 Then
                Dim result = MessageBox.Show("It is highly recommended you clear cache before each game considering audio files from previous sessions will remain and it may take a long time to process. Do you still want to continue?", "Do you still want to continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

                If result = DialogResult.Yes Then

                    Try
                        LoadHTTP0.RunWorkerAsync()
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

    Private Sub UpdateTaskLBR(Text)
        Me.Invoke(Sub()
                      TaskLBR.Text = Text
                  End Sub)
    End Sub

    Private Sub LoadHTTP_DoWork_1(sender As Object, e As DoWorkEventArgs) Handles LoadHTTP0.DoWork

        'The old code was a complete disaster it utilized like a bunch of background workers and then progress bars to check to see if the background workers are done
        'just created a buggy mess of code that was bound to failure
        'so I basically just moved all of the bullshit all into one background worker
        'that was the only proper fix to this disaster I don't know why my dumbass coded it like that but it's fixed now in v1.1.2

        Me.Invoke(Sub()
                      HTTPLISTBOX.DisplayMember = "DisplayText"
                      HTTPLISTBOX.ValueMember = "FilePath"

                      LoadImgListBox.DisplayMember = "DisplayText"
                      LoadImgListBox.ValueMember = "FilePath"
                  End Sub)


        Dim tempDirectory = Path.GetTempPath

        If Not My.Computer.FileSystem.FileExists(tempDirectory & "\RBXAudioExtractorIMG") Then
            My.Computer.FileSystem.CreateDirectory(tempDirectory & "\RBXAudioExtractorIMG")
        End If

        If Not My.Computer.FileSystem.FileExists(tempDirectory & "\RBX_SOUND_RIPPER_HTTP") Then
            My.Computer.FileSystem.CreateDirectory(tempDirectory & "\RBX_SOUND_RIPPER_HTTP")
        End If
        Me.Invoke(Sub()
                      TaskLBR.Visible = True
                  End Sub)

        Stage = 1
        UpdateTaskLBR("Task 1/6")

        RemoveAllFileInDir1(tempDirectory & "\RBX_SOUND_RIPPER_HTTP")
        UpdateTaskLBR("Task 2/6")
        RemoveAllFileInDir1(tempDirectory & "\RBXAudioExtractorIMG")


        Dim directories As String() = CType(e.Argument, String())
        Dim sourceDir As String = $"{tempDirectory}\Roblox\http"
        Dim targetDir As String = $"{tempDirectory}\RBX_SOUND_RIPPER_HTTP"


        Dim files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
        UpdateTaskLBR("Task 3/6")
        For i As Integer = 0 To files.Length - 1

            If LoadHTTP0.CancellationPending Then
                e.Cancel = True
                Exit For
            End If

            ProcessFile(files(i), targetDir)


            LoadHTTP0.ReportProgress(CInt((i + 1) / files.Length * 100))
        Next


        Stage = 3
        UpdateTaskLBR("Task 4/6")
        RenameFileExtensions(tempDirectory & "\RBX_SOUND_RIPPER_HTTP", ".ogg")
        UpdateTaskLBR("Task 5/6")
        RenameFileExtensions(tempDirectory & "\RBXAudioExtractorIMG", ".png")

        Me.Invoke(Sub()
                      ProgressBar1.Value = 99
                  End Sub)

        '   ProgressBar1.Value = 0

        Me.Invoke(Sub()
                      HTTPLISTBOX.Items.Clear()
                      LoadImgListBox.Items.Clear()
                  End Sub)


        Dim musicFiles = Directory.GetFiles(tempDirectory & "\RBX_SOUND_RIPPER_HTTP", "*.ogg")
        UpdateTaskLBR("Task 6/6")


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
                                  HTTPLISTBOX.Items.Insert(0, New With {
                              .DisplayText = $"{title} | Artists: {Artist} | Album: {Album} | Duration: {Duration}",
                              .FilePath = fileData.FilePath
                          })
                              Else
                                  HTTPLISTBOX.Items.Add(New With {
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


        Dim PngFiles = Directory.GetFiles(tempDirectory & "\RBXAudioExtractorIMG", "*.png")

        For Each file In PngFiles
            Me.Invoke(Sub()
                          LoadImgListBox.Items.Add(New With {.DisplayText = $"" & Path.GetFileName(file) & "", .FilePath = file})
                      End Sub)

        Next

        Stage = 0
        Me.Invoke(Sub()
                      ProgressBar1.Value = 0
                      TaskLBR.Visible = False
                  End Sub)


    End Sub

    Private Sub LoadHTTP0_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles LoadHTTP0.ProgressChanged
        ProgressBar1.Value = e.ProgressPercentage
    End Sub



    Private Sub ProcessFile(file As String, targetDir As String)
        UpdateLog($"Begin processing on : {file}")

        Try
            Dim fileBytes = System.IO.File.ReadAllBytes(file)
            Dim oggHeader As Byte() = Encoding.ASCII.GetBytes("OggS")
            Dim PNGHeader As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}

            Dim oggIndex As Integer = -1
            Dim PNGIndex As Integer = -1

            UpdateLog($"First 16 bytes of {file}: " & BitConverter.ToString(fileBytes.Take(16).ToArray()))


            For i As Integer = 0 To fileBytes.Length - 1
                If oggIndex = -1 AndAlso i <= fileBytes.Length - oggHeader.Length Then
                    If fileBytes.AsSpan(i, oggHeader.Length).SequenceEqual(oggHeader) Then
                        oggIndex = i
                        UpdateLog($"Found OGG header at index {oggIndex}")
                    End If
                End If

                If PNGIndex = -1 AndAlso i <= fileBytes.Length - PNGHeader.Length Then
                    If fileBytes.AsSpan(i, PNGHeader.Length).SequenceEqual(PNGHeader) Then
                        PNGIndex = i
                        UpdateLog($"Found PNG header at index {PNGIndex}")
                    End If
                End If

                If oggIndex >= 0 AndAlso PNGIndex >= 0 Then Exit For
            Next


            If oggIndex >= 0 Then
                UpdateLog($"Processing OGG file at index {oggIndex}")
                Dim fileName = Path.GetFileName(file)
                Dim targetPath = Path.Combine(targetDir, fileName)
                System.IO.File.Copy(file, targetPath, True)

                Dim modifiedBytes = fileBytes.Skip(oggIndex).ToArray()

                UpdateLog($"Removing default Roblox parameters")
                System.IO.File.WriteAllBytes(targetPath, modifiedBytes)
                UpdateLog($"Done processing file: {file}")
            Else
                UpdateLog($"Skipping: {file} is not a valid OGG file.")
            End If


            If PNGIndex >= 0 Then
                Dim imageDir = Path.Combine(tempDirectory, "RBXAudioExtractorIMG")
                Directory.CreateDirectory(imageDir)
                UpdateLog($"Processing PNG file at index {PNGIndex}")

                Dim fileName = Path.GetFileName(file)
                Dim targetPath = Path.Combine(imageDir, fileName)
                System.IO.File.Copy(file, targetPath, True)

                Dim modifiedBytes = fileBytes.Skip(PNGIndex).ToArray()

                UpdateLog($"Removing default Roblox parameters")
                System.IO.File.WriteAllBytes(targetPath, modifiedBytes)
                UpdateLog($"Done processing file: {file}")
            Else
                UpdateLog($"Skipping: {file} is not a valid PNG file.")
            End If
        Catch ex As Exception
            UpdateLog($"Error processing file: {file} - {ex.Message}")

        End Try
    End Sub

    Private Sub LoadHTTP0_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles LoadHTTP0.RunWorkerCompleted

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

    Public Sub SaveFile()
        Dim saveFileDialog As New SaveFileDialog

        saveFileDialog.Filter = "OGG Files (*.ogg)|*.ogg"

        If saveFileDialog.ShowDialog = DialogResult.OK Then

            Dim filePath = saveFileDialog.FileName

            Try
                My.Computer.FileSystem.CopyFile(SelFile, filePath)
            Catch ex As Exception
                CallError(ex.Message)
            End Try

        End If
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

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Try
            Process.Start("C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "http://zv800.com/")
        Catch ex As Exception
            CallError(ex.Message)
        End Try
    End Sub





    Private Sub ListBox1_SelectedIndexChanged_1(sender As Object, e As EventArgs) Handles HTTPLISTBOX.SelectedIndexChanged
        If HTTPLISTBOX.SelectedItem IsNot Nothing Then

            Try
                Dim selectedFile = HTTPLISTBOX.SelectedItem.FilePath
                SelFile = selectedFile

                If My.Computer.FileSystem.FileExists(SelFile) Then
                    PlayOggFile(SelFile)
                Else
                    CallError("File Not Found")
                End If

            Catch ex As Exception
                CallError(ex.Message)
            End Try

        End If
    End Sub

    Private Sub DOWNLOADHTTP_BTN_Click(sender As Object, e As EventArgs) Handles DOWNLOADHTTP_BTN.Click
        SaveFile()
    End Sub




    Private Sub ClearHTTPTEMP_BTN_Click(sender As Object, e As EventArgs) Handles ClearHTTPTEMP_BTN.Click

        If CheckForRoblox() Then
            Dim result = MessageBox.Show("Are you sure you want to clear the temp directory? You will you will lose access to all of the audios / images your Roblox client has saved", "You sure?", MessageBoxButtons.YesNo)
            Dim tempDirectory = Path.GetTempPath
            If result = DialogResult.Yes Then
                'EnableButtons(False)
                Try
                    ClearCache.RunWorkerAsync()
                Catch ex As Exception
                    CallError("cannot run two processes at the same time")
                End Try
            End If
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

    Private Sub DownloadAllHTTP_BTN_Click(sender As Object, e As EventArgs) Handles DownloadAllHTTP_BTN.Click, DownloadALL_BTN.Click
        Using folderDialog As New FolderBrowserDialog
            folderDialog.Description = "Select a folder"
            folderDialog.ShowNewFolderButton = True

            If folderDialog.ShowDialog = DialogResult.OK Then
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
        Try
            Process.Start("C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "https://github.com/zv8001/RBX-Audio-Extractor")
        Catch ex As Exception
            CallError(ex.Message)
        End Try
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
    Private Sub DownloadImgBtn_Click(sender As Object, e As EventArgs) Handles DownloadImgBtn.Click
        Dim Open As New SaveFileDialog
        Open.Filter = "PNG Files (*.png)|*.png"
        If Open.ShowDialog = DialogResult.OK Then
            Try
                My.Computer.FileSystem.CopyFile(SelFile, Open.FileName)
            Catch ex As Exception
                CallError(ex)
            End Try

        End If
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
    Private Sub LoadImgListBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles LoadImgListBox.SelectedIndexChanged
        If LoadImgListBox.SelectedItem IsNot Nothing Then

            Try

                Dim selectedFile = LoadImgListBox.SelectedItem.FilePath
                PreVeiwImgBox.Image = Image.FromFile(selectedFile)
                SelFile = selectedFile
                Dim image0 As Image = Image.FromFile(SelFile)
                Dim width As Integer = image0.Width
                Dim height As Integer = image0.Height
                Dim fileInfo As New FileInfo(SelFile)
                Dim fileSize As Long = fileInfo.Length
                ImgStats.Text = $"Dimensions: {width} x {height}" & vbCrLf &
                                     $"File Size: {FormatBytes(fileSize)}"

            Catch ex As Exception
                CallError("File Not Found")
            End Try

        End If
    End Sub

    Private Sub LoadImgBtn_Click(sender As Object, e As EventArgs) Handles LoadImgBtn.Click
        LoadFullGame()
    End Sub

    Private Sub SaveAllBtn_Click(sender As Object, e As EventArgs) Handles SaveAllBtn.Click
        Using folderDialog As New FolderBrowserDialog
            folderDialog.Description = "Select a folder"
            folderDialog.ShowNewFolderButton = True

            If folderDialog.ShowDialog = DialogResult.OK Then
                CloneDirectory(tempDirectory & "\RBXAudioExtractorIMG", folderDialog.SelectedPath)
            End If
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

        If CheckForRoblox() Then
            Dim result = MessageBox.Show("Are you sure you want to clear the temp directory? You will you will lose access to all of the audios / images your Roblox client has saved", "You sure?", MessageBoxButtons.YesNo)
            Dim tempDirectory = Path.GetTempPath
            If result = DialogResult.Yes Then
                'EnableButtons(False)
                Try
                    ClearCache.RunWorkerAsync()
                Catch ex As Exception
                    CallError("cannot run two processes at the same time")
                End Try
            End If
        End If
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click

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
        Dim files = Directory.GetFiles($"{tempDirectory}\Roblox\http", "*.*", SearchOption.AllDirectories)
        If Not Directory.Exists($"{tempDirectory}\Roblox\http") Then
            UpdateLog($"ERROR: Directory does not exist: {tempDirectory}\Roblox\http")
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

    Private Sub SoundPlayerPlayBtn_Click(sender As Object, e As EventArgs) Handles SoundPlayerPlayBtn.Click
        If Playing Then
            StopPlayback()
        Else

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

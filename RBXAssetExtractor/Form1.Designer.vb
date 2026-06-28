<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        LoadHTTP0 = New ComponentModel.BackgroundWorker()
        output_log = New ListBox()
        SaveFileDialog1 = New SaveFileDialog()
        LoadParButton = New Button()
        Download_BTN = New Button()
        ClearTMP_BTN = New Button()
        DownloadALL_BTN = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        TabControl1 = New TabControl()
        TabPage3 = New TabPage()
        Label4 = New Label()
        Label3 = New Label()
        Label9 = New Label()
        PictureBox2 = New PictureBox()
        Label5 = New Label()
        Label1 = New Label()
        LinkLabel2 = New LinkLabel()
        LinkLabel1 = New LinkLabel()
        Label2 = New Label()
        TabPage2 = New TabPage()
        Panel1 = New Panel()
        Sounds_Listbox = New ListBox()
        TabPage4 = New TabPage()
        fullAudioStatus = New Label()
        LoadHttpBtn = New Button()
        ClearHTTPTEMP_BTN = New Button()
        DownloadAllHTTP_BTN = New Button()
        DOWNLOADHTTP_BTN = New Button()
        HTTPLISTBOX = New ListBox()
        TabPage5 = New TabPage()
        fullImageStatus = New Label()
        SaveAlAsPngCheck = New CheckBox()
        ImgClearTmp = New Button()
        ImgStats = New Label()
        SaveAllBtn = New Button()
        DownloadImgBtn = New Button()
        LoadImgBtn = New Button()
        LoadImgListBox = New ListBox()
        PreVeiwImgBox = New PictureBox()
        meshTab = New TabPage()
        meshStatus = New Label()
        meshList = New ListBox()
        meshProgress = New ProgressBar()
        meshScanButton = New Button()
        meshExportButton = New Button()
        meshExportAllButton = New Button()
        cacheAssetTab = New TabPage()
        cacheAssetStatus = New Label()
        cacheAssetFilter = New ComboBox()
        cacheAssetList = New ListBox()
        cacheAssetProgress = New ProgressBar()
        cacheAssetScanButton = New Button()
        cacheAssetExportButton = New Button()
        cacheAssetExportAllButton = New Button()
        thumbnailTab = New TabPage()
        thumbnailStatus = New Label()
        thumbnailList = New ListBox()
        thumbnailPreview = New PictureBox()
        thumbnailProgress = New ProgressBar()
        thumbnailScanButton = New Button()
        thumbnailExportButton = New Button()
        thumbnailExportAllButton = New Button()
        fontTab = New TabPage()
        fontStatus = New Label()
        fontList = New ListBox()
        fontProgress = New ProgressBar()
        fontScanButton = New Button()
        fontExportButton = New Button()
        fontExportAllButton = New Button()
        metadataTab = New TabPage()
        metadataStatus = New Label()
        metadataList = New ListBox()
        metadataPreview = New TextBox()
        metadataProgress = New ProgressBar()
        metadataScanButton = New Button()
        metadataExportButton = New Button()
        metadataExportAllButton = New Button()
        TabPage1 = New TabPage()
        SaveLogBtn = New Button()
        AutoScrollCHK = New CheckBox()
        VText_LBR = New Label()
        CheckFOrButtons = New Timer(components)
        ClearTmp = New ComponentModel.BackgroundWorker()
        ListboxAutoScrool = New Timer(components)
        StatusLBR = New Label()
        Panel2 = New Panel()
        LoadingGif = New PictureBox()
        PictureBox1 = New PictureBox()
        Label6 = New Label()
        Button2 = New Button()
        CloseBTN = New Button()
        fadeInTimer = New Timer(components)
        fadeOutTimer = New Timer(components)
        AlwaysOnTopCHK = New CheckBox()
        KeepButtonsOff = New Timer(components)
        MainLoop = New Timer(components)
        LoadPartialBackgoundWorker = New ComponentModel.BackgroundWorker()
        ClearCache = New ComponentModel.BackgroundWorker()
        MSGPopup = New ComponentModel.BackgroundWorker()
        ProgressBar1 = New MetroFramework.Controls.MetroProgressBar()
        TaskLBR = New Label()
        trackBarTimeline = New MetroFramework.Controls.MetroTrackBar()
        lblElapsedTime = New Label()
        lblTotalTime = New Label()
        playbackTimer = New Timer(components)
        SoundPlayerPlayBtn = New Button()
        Panel3 = New Panel()
        VolumeControl1 = New cmdwtf.Toolkit.WinForms.Controls.VolumeControl()
        KeepPlayback0 = New Timer(components)
        ChangeVol = New Timer(components)
        TabControl1.SuspendLayout()
        TabPage3.SuspendLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        TabPage2.SuspendLayout()
        TabPage4.SuspendLayout()
        TabPage5.SuspendLayout()
        CType(PreVeiwImgBox, ComponentModel.ISupportInitialize).BeginInit()
        meshTab.SuspendLayout()
        cacheAssetTab.SuspendLayout()
        thumbnailTab.SuspendLayout()
        CType(thumbnailPreview, ComponentModel.ISupportInitialize).BeginInit()
        fontTab.SuspendLayout()
        metadataTab.SuspendLayout()
        TabPage1.SuspendLayout()
        Panel2.SuspendLayout()
        CType(LoadingGif, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        Panel3.SuspendLayout()
        SuspendLayout()
        ' 
        ' LoadHTTP0
        ' 
        ' 
        ' output_log
        ' 
        output_log.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        output_log.BackColor = Color.Black
        output_log.Font = New Font("Segoe UI", 7.25F)
        output_log.ForeColor = Color.Lime
        output_log.FormattingEnabled = True
        output_log.HorizontalScrollbar = True
        output_log.ItemHeight = 15
        output_log.Location = New Point(0, 0)
        output_log.Margin = New Padding(3, 4, 3, 4)
        output_log.Name = "output_log"
        output_log.Size = New Size(486, 424)
        output_log.TabIndex = 1
        ' 
        ' LoadParButton
        ' 
        LoadParButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        LoadParButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        LoadParButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        LoadParButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        LoadParButton.FlatStyle = FlatStyle.Flat
        LoadParButton.Font = New Font("Segoe UI", 12F)
        LoadParButton.ForeColor = Color.White
        LoadParButton.Location = New Point(7, 415)
        LoadParButton.Margin = New Padding(3, 4, 3, 4)
        LoadParButton.Name = "LoadParButton"
        LoadParButton.Size = New Size(86, 47)
        LoadParButton.TabIndex = 2
        LoadParButton.Text = "Load"
        LoadParButton.UseVisualStyleBackColor = False
        ' 
        ' Download_BTN
        ' 
        Download_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        Download_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        Download_BTN.Enabled = False
        Download_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        Download_BTN.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        Download_BTN.FlatStyle = FlatStyle.Flat
        Download_BTN.Font = New Font("Segoe UI", 12F)
        Download_BTN.ForeColor = Color.White
        Download_BTN.Location = New Point(99, 415)
        Download_BTN.Margin = New Padding(3, 4, 3, 4)
        Download_BTN.Name = "Download_BTN"
        Download_BTN.Size = New Size(120, 47)
        Download_BTN.TabIndex = 3
        Download_BTN.Text = "Download"
        Download_BTN.UseVisualStyleBackColor = False
        ' 
        ' ClearTMP_BTN
        ' 
        ClearTMP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ClearTMP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        ClearTMP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        ClearTMP_BTN.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        ClearTMP_BTN.FlatStyle = FlatStyle.Flat
        ClearTMP_BTN.Font = New Font("Segoe UI", 12F)
        ClearTMP_BTN.ForeColor = Color.White
        ClearTMP_BTN.Location = New Point(363, 415)
        ClearTMP_BTN.Margin = New Padding(3, 4, 3, 4)
        ClearTMP_BTN.Name = "ClearTMP_BTN"
        ClearTMP_BTN.Size = New Size(122, 47)
        ClearTMP_BTN.TabIndex = 5
        ClearTMP_BTN.Text = "Clear Cache"
        ClearTMP_BTN.UseVisualStyleBackColor = False
        ' 
        ' DownloadALL_BTN
        ' 
        DownloadALL_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DownloadALL_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DownloadALL_BTN.Enabled = False
        DownloadALL_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DownloadALL_BTN.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        DownloadALL_BTN.FlatStyle = FlatStyle.Flat
        DownloadALL_BTN.Font = New Font("Segoe UI", 12F)
        DownloadALL_BTN.ForeColor = Color.White
        DownloadALL_BTN.Location = New Point(226, 415)
        DownloadALL_BTN.Margin = New Padding(3, 4, 3, 4)
        DownloadALL_BTN.Name = "DownloadALL_BTN"
        DownloadALL_BTN.Size = New Size(130, 47)
        DownloadALL_BTN.TabIndex = 7
        DownloadALL_BTN.Text = "Download All"
        DownloadALL_BTN.UseVisualStyleBackColor = False
        ' 
        ' TabControl1
        ' 
        TabControl1.Controls.Add(TabPage3)
        TabControl1.Controls.Add(TabPage2)
        TabControl1.Controls.Add(TabPage4)
        TabControl1.Controls.Add(TabPage5)
        TabControl1.Controls.Add(meshTab)
        TabControl1.Controls.Add(cacheAssetTab)
        TabControl1.Controls.Add(thumbnailTab)
        TabControl1.Controls.Add(fontTab)
        TabControl1.Controls.Add(metadataTab)
        TabControl1.Controls.Add(TabPage1)
        TabControl1.Location = New Point(14, 60)
        TabControl1.Margin = New Padding(3, 4, 3, 4)
        TabControl1.Name = "TabControl1"
        TabControl1.SelectedIndex = 0
        TabControl1.ShowToolTips = True
        TabControl1.Size = New Size(502, 508)
        TabControl1.TabIndex = 9
        ' 
        ' TabPage3
        ' 
        TabPage3.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        TabPage3.Controls.Add(Label4)
        TabPage3.Controls.Add(Label3)
        TabPage3.Controls.Add(Label9)
        TabPage3.Controls.Add(PictureBox2)
        TabPage3.Controls.Add(Label5)
        TabPage3.Controls.Add(Label1)
        TabPage3.Controls.Add(LinkLabel2)
        TabPage3.Controls.Add(LinkLabel1)
        TabPage3.Controls.Add(Label2)
        TabPage3.Location = New Point(4, 29)
        TabPage3.Margin = New Padding(3, 4, 3, 4)
        TabPage3.Name = "TabPage3"
        TabPage3.Size = New Size(494, 475)
        TabPage3.TabIndex = 2
        TabPage3.Text = "About"
        ' 
        ' Label4
        ' 
        Label4.Font = New Font("MS UI Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Color.IndianRed
        Label4.Location = New Point(27, 190)
        Label4.Name = "Label4"
        Label4.Size = New Size(441, 122)
        Label4.TabIndex = 14
        Label4.Text = resources.GetString("Label4.Text")
        Label4.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label3
        ' 
        Label3.Font = New Font("MS UI Gothic", 19.8000011F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Color.IndianRed
        Label3.Location = New Point(27, 129)
        Label3.Name = "Label3"
        Label3.Size = New Size(441, 77)
        Label3.TabIndex = 13
        Label3.Text = "V 2.0.0 OVERHAUL IS HERE"
        Label3.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Font = New Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label9.ForeColor = Color.Gray
        Label9.Location = New Point(7, 110)
        Label9.Name = "Label9"
        Label9.Size = New Size(191, 19)
        Label9.TabIndex = 12
        Label9.Text = "pfp credit: @MIGHTBEMOD | "
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackgroundImage = My.Resources.Resources.Untitled_2_01
        PictureBox2.BackgroundImageLayout = ImageLayout.Stretch
        PictureBox2.Location = New Point(7, 4)
        PictureBox2.Margin = New Padding(3, 4, 3, 4)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(87, 93)
        PictureBox2.TabIndex = 11
        PictureBox2.TabStop = False
        ' 
        ' Label5
        ' 
        Label5.Font = New Font("MS UI Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Color.White
        Label5.Location = New Point(27, 328)
        Label5.Name = "Label5"
        Label5.Size = New Size(441, 77)
        Label5.TabIndex = 4
        Label5.Text = resources.GetString("Label5.Text")
        Label5.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("MS UI Gothic", 22.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(94, 4)
        Label1.Name = "Label1"
        Label1.Size = New Size(363, 37)
        Label1.TabIndex = 0
        Label1.Text = "| RBX Asset Extractor"
        Label1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LinkLabel2
        ' 
        LinkLabel2.AutoSize = True
        LinkLabel2.LinkColor = Color.White
        LinkLabel2.Location = New Point(6, 436)
        LinkLabel2.Name = "LinkLabel2"
        LinkLabel2.Size = New Size(328, 20)
        LinkLabel2.TabIndex = 8
        LinkLabel2.TabStop = True
        LinkLabel2.Text = "https://github.com/zv8001/RBX-Audio-Extractor"
        ' 
        ' LinkLabel1
        ' 
        LinkLabel1.AutoSize = True
        LinkLabel1.LinkColor = Color.White
        LinkLabel1.Location = New Point(6, 415)
        LinkLabel1.Name = "LinkLabel1"
        LinkLabel1.Size = New Size(127, 20)
        LinkLabel1.TabIndex = 6
        LinkLabel1.TabStop = True
        LinkLabel1.Text = "http://zv800.com/"
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Top
        Label2.AutoSize = True
        Label2.Font = New Font("MS UI Gothic", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.White
        Label2.Location = New Point(94, 48)
        Label2.Name = "Label2"
        Label2.Size = New Size(268, 38)
        Label2.TabIndex = 1
        Label2.Text = "Made by VexTheProtogen " & vbCrLf & "That one gay furry programer :3"
        Label2.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TabPage2
        ' 
        TabPage2.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        TabPage2.Controls.Add(Panel1)
        TabPage2.Controls.Add(Sounds_Listbox)
        TabPage2.Controls.Add(Download_BTN)
        TabPage2.Controls.Add(DownloadALL_BTN)
        TabPage2.Controls.Add(LoadParButton)
        TabPage2.Controls.Add(ClearTMP_BTN)
        TabPage2.ForeColor = Color.Black
        TabPage2.Location = New Point(4, 29)
        TabPage2.Margin = New Padding(3, 4, 3, 4)
        TabPage2.Name = "TabPage2"
        TabPage2.Padding = New Padding(3, 4, 3, 4)
        TabPage2.Size = New Size(494, 475)
        TabPage2.TabIndex = 1
        TabPage2.Text = "(Music) Partial Game Audio"
        ' 
        ' Panel1
        ' 
        Panel1.Location = New Point(170, 477)
        Panel1.Margin = New Padding(3, 4, 3, 4)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(229, 133)
        Panel1.TabIndex = 10
        ' 
        ' Sounds_Listbox
        ' 
        Sounds_Listbox.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        Sounds_Listbox.BorderStyle = BorderStyle.FixedSingle
        Sounds_Listbox.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Sounds_Listbox.ForeColor = Color.White
        Sounds_Listbox.FormattingEnabled = True
        Sounds_Listbox.HorizontalScrollbar = True
        Sounds_Listbox.ItemHeight = 25
        Sounds_Listbox.Location = New Point(7, 8)
        Sounds_Listbox.Margin = New Padding(3, 4, 3, 4)
        Sounds_Listbox.Name = "Sounds_Listbox"
        Sounds_Listbox.Size = New Size(479, 402)
        Sounds_Listbox.TabIndex = 0
        ' 
        ' TabPage4
        ' 
        TabPage4.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        TabPage4.Controls.Add(fullAudioStatus)
        TabPage4.Controls.Add(LoadHttpBtn)
        TabPage4.Controls.Add(ClearHTTPTEMP_BTN)
        TabPage4.Controls.Add(DownloadAllHTTP_BTN)
        TabPage4.Controls.Add(DOWNLOADHTTP_BTN)
        TabPage4.Controls.Add(HTTPLISTBOX)
        TabPage4.Location = New Point(4, 29)
        TabPage4.Margin = New Padding(3, 4, 3, 4)
        TabPage4.Name = "TabPage4"
        TabPage4.Size = New Size(494, 475)
        TabPage4.TabIndex = 3
        TabPage4.Text = "Full Game Audio"
        ' 
        ' fullAudioStatus
        ' 
        fullAudioStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        fullAudioStatus.ForeColor = Color.White
        fullAudioStatus.Location = New Point(7, 7)
        fullAudioStatus.Name = "fullAudioStatus"
        fullAudioStatus.Size = New Size(479, 23)
        fullAudioStatus.TabIndex = 8
        fullAudioStatus.Text = "Load the Roblox cache to find full-game audio."
        ' 
        ' LoadHttpBtn
        ' 
        LoadHttpBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        LoadHttpBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        LoadHttpBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        LoadHttpBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        LoadHttpBtn.FlatStyle = FlatStyle.Flat
        LoadHttpBtn.Font = New Font("Segoe UI", 12F)
        LoadHttpBtn.ForeColor = Color.White
        LoadHttpBtn.Location = New Point(7, 415)
        LoadHttpBtn.Margin = New Padding(3, 4, 3, 4)
        LoadHttpBtn.Name = "LoadHttpBtn"
        LoadHttpBtn.Size = New Size(86, 47)
        LoadHttpBtn.TabIndex = 7
        LoadHttpBtn.Text = "Load"
        LoadHttpBtn.UseVisualStyleBackColor = False
        ' 
        ' ClearHTTPTEMP_BTN
        ' 
        ClearHTTPTEMP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ClearHTTPTEMP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        ClearHTTPTEMP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        ClearHTTPTEMP_BTN.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        ClearHTTPTEMP_BTN.FlatStyle = FlatStyle.Flat
        ClearHTTPTEMP_BTN.Font = New Font("Segoe UI", 12F)
        ClearHTTPTEMP_BTN.ForeColor = Color.White
        ClearHTTPTEMP_BTN.Location = New Point(363, 415)
        ClearHTTPTEMP_BTN.Margin = New Padding(3, 4, 3, 4)
        ClearHTTPTEMP_BTN.MaximumSize = New Size(122, 47)
        ClearHTTPTEMP_BTN.MinimumSize = New Size(122, 47)
        ClearHTTPTEMP_BTN.Name = "ClearHTTPTEMP_BTN"
        ClearHTTPTEMP_BTN.Size = New Size(122, 47)
        ClearHTTPTEMP_BTN.TabIndex = 6
        ClearHTTPTEMP_BTN.Text = "Clear Cache"
        ClearHTTPTEMP_BTN.UseVisualStyleBackColor = False
        ' 
        ' DownloadAllHTTP_BTN
        ' 
        DownloadAllHTTP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DownloadAllHTTP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DownloadAllHTTP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DownloadAllHTTP_BTN.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        DownloadAllHTTP_BTN.FlatStyle = FlatStyle.Flat
        DownloadAllHTTP_BTN.Font = New Font("Segoe UI", 12F)
        DownloadAllHTTP_BTN.ForeColor = Color.White
        DownloadAllHTTP_BTN.Location = New Point(226, 415)
        DownloadAllHTTP_BTN.Margin = New Padding(3, 4, 3, 4)
        DownloadAllHTTP_BTN.Name = "DownloadAllHTTP_BTN"
        DownloadAllHTTP_BTN.Size = New Size(130, 47)
        DownloadAllHTTP_BTN.TabIndex = 5
        DownloadAllHTTP_BTN.Text = "Download All"
        DownloadAllHTTP_BTN.UseVisualStyleBackColor = False
        ' 
        ' DOWNLOADHTTP_BTN
        ' 
        DOWNLOADHTTP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DOWNLOADHTTP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DOWNLOADHTTP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DOWNLOADHTTP_BTN.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        DOWNLOADHTTP_BTN.FlatStyle = FlatStyle.Flat
        DOWNLOADHTTP_BTN.Font = New Font("Segoe UI", 12F)
        DOWNLOADHTTP_BTN.ForeColor = Color.White
        DOWNLOADHTTP_BTN.Location = New Point(99, 415)
        DOWNLOADHTTP_BTN.Margin = New Padding(3, 4, 3, 4)
        DOWNLOADHTTP_BTN.Name = "DOWNLOADHTTP_BTN"
        DOWNLOADHTTP_BTN.Size = New Size(120, 47)
        DOWNLOADHTTP_BTN.TabIndex = 4
        DOWNLOADHTTP_BTN.Text = "Download"
        DOWNLOADHTTP_BTN.UseVisualStyleBackColor = False
        ' 
        ' HTTPLISTBOX
        ' 
        HTTPLISTBOX.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        HTTPLISTBOX.BorderStyle = BorderStyle.FixedSingle
        HTTPLISTBOX.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        HTTPLISTBOX.ForeColor = Color.White
        HTTPLISTBOX.FormattingEnabled = True
        HTTPLISTBOX.ItemHeight = 25
        HTTPLISTBOX.Location = New Point(7, 34)
        HTTPLISTBOX.Margin = New Padding(3, 4, 3, 4)
        HTTPLISTBOX.Name = "HTTPLISTBOX"
        HTTPLISTBOX.Size = New Size(479, 352)
        HTTPLISTBOX.TabIndex = 0
        ' 
        ' TabPage5
        ' 
        TabPage5.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        TabPage5.Controls.Add(fullImageStatus)
        TabPage5.Controls.Add(SaveAlAsPngCheck)
        TabPage5.Controls.Add(ImgClearTmp)
        TabPage5.Controls.Add(ImgStats)
        TabPage5.Controls.Add(SaveAllBtn)
        TabPage5.Controls.Add(DownloadImgBtn)
        TabPage5.Controls.Add(LoadImgBtn)
        TabPage5.Controls.Add(LoadImgListBox)
        TabPage5.Controls.Add(PreVeiwImgBox)
        TabPage5.Location = New Point(4, 29)
        TabPage5.Margin = New Padding(3, 4, 3, 4)
        TabPage5.Name = "TabPage5"
        TabPage5.Size = New Size(494, 475)
        TabPage5.TabIndex = 4
        TabPage5.Text = "Full Game Images"
        ' 
        ' fullImageStatus
        ' 
        fullImageStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        fullImageStatus.ForeColor = Color.White
        fullImageStatus.Location = New Point(3, 4)
        fullImageStatus.Name = "fullImageStatus"
        fullImageStatus.Size = New Size(486, 23)
        fullImageStatus.TabIndex = 14
        fullImageStatus.Text = "Load the Roblox cache to find full-game images."
        ' 
        ' SaveAlAsPngCheck
        ' 
        SaveAlAsPngCheck.AutoSize = True
        SaveAlAsPngCheck.Font = New Font("Segoe UI", 6.25F)
        SaveAlAsPngCheck.ForeColor = Color.White
        SaveAlAsPngCheck.Location = New Point(376, 263)
        SaveAlAsPngCheck.Margin = New Padding(3, 4, 3, 4)
        SaveAlAsPngCheck.Name = "SaveAlAsPngCheck"
        SaveAlAsPngCheck.Size = New Size(121, 17)
        SaveAlAsPngCheck.TabIndex = 13
        SaveAlAsPngCheck.Text = "SaveAlwaysAsPNG"
        SaveAlAsPngCheck.UseVisualStyleBackColor = True
        ' 
        ' ImgClearTmp
        ' 
        ImgClearTmp.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ImgClearTmp.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        ImgClearTmp.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        ImgClearTmp.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        ImgClearTmp.FlatStyle = FlatStyle.Flat
        ImgClearTmp.Font = New Font("Segoe UI", 12F)
        ImgClearTmp.ForeColor = Color.White
        ImgClearTmp.Location = New Point(222, 292)
        ImgClearTmp.Margin = New Padding(3, 4, 3, 4)
        ImgClearTmp.Name = "ImgClearTmp"
        ImgClearTmp.Size = New Size(267, 48)
        ImgClearTmp.TabIndex = 12
        ImgClearTmp.Text = "Clear Cache"
        ImgClearTmp.UseVisualStyleBackColor = False
        ' 
        ' ImgStats
        ' 
        ImgStats.AutoSize = True
        ImgStats.ForeColor = Color.White
        ImgStats.Location = New Point(222, 245)
        ImgStats.Name = "ImgStats"
        ImgStats.Size = New Size(130, 20)
        ImgStats.TabIndex = 11
        ImgStats.Text = "Image size: 0x0 px"
        ' 
        ' SaveAllBtn
        ' 
        SaveAllBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        SaveAllBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        SaveAllBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        SaveAllBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        SaveAllBtn.FlatStyle = FlatStyle.Flat
        SaveAllBtn.Font = New Font("Segoe UI", 12F)
        SaveAllBtn.ForeColor = Color.White
        SaveAllBtn.Location = New Point(222, 403)
        SaveAllBtn.Margin = New Padding(3, 4, 3, 4)
        SaveAllBtn.Name = "SaveAllBtn"
        SaveAllBtn.Size = New Size(267, 47)
        SaveAllBtn.TabIndex = 10
        SaveAllBtn.Text = "Save All"
        SaveAllBtn.UseVisualStyleBackColor = False
        ' 
        ' DownloadImgBtn
        ' 
        DownloadImgBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DownloadImgBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DownloadImgBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DownloadImgBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        DownloadImgBtn.FlatStyle = FlatStyle.Flat
        DownloadImgBtn.Font = New Font("Segoe UI", 12F)
        DownloadImgBtn.ForeColor = Color.White
        DownloadImgBtn.Location = New Point(314, 348)
        DownloadImgBtn.Margin = New Padding(3, 4, 3, 4)
        DownloadImgBtn.Name = "DownloadImgBtn"
        DownloadImgBtn.Size = New Size(175, 47)
        DownloadImgBtn.TabIndex = 9
        DownloadImgBtn.Text = "Download"
        DownloadImgBtn.UseVisualStyleBackColor = False
        ' 
        ' LoadImgBtn
        ' 
        LoadImgBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        LoadImgBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        LoadImgBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        LoadImgBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        LoadImgBtn.FlatStyle = FlatStyle.Flat
        LoadImgBtn.Font = New Font("Segoe UI", 12F)
        LoadImgBtn.ForeColor = Color.White
        LoadImgBtn.Location = New Point(222, 348)
        LoadImgBtn.Margin = New Padding(3, 4, 3, 4)
        LoadImgBtn.Name = "LoadImgBtn"
        LoadImgBtn.Size = New Size(86, 47)
        LoadImgBtn.TabIndex = 8
        LoadImgBtn.Text = "Load"
        LoadImgBtn.UseVisualStyleBackColor = False
        ' 
        ' LoadImgListBox
        ' 
        LoadImgListBox.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        LoadImgListBox.ForeColor = Color.White
        LoadImgListBox.FormattingEnabled = True
        LoadImgListBox.HorizontalScrollbar = True
        LoadImgListBox.Location = New Point(3, 30)
        LoadImgListBox.Margin = New Padding(3, 4, 3, 4)
        LoadImgListBox.Name = "LoadImgListBox"
        LoadImgListBox.Size = New Size(211, 404)
        LoadImgListBox.TabIndex = 1
        ' 
        ' PreVeiwImgBox
        ' 
        PreVeiwImgBox.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        PreVeiwImgBox.Location = New Point(222, 30)
        PreVeiwImgBox.Margin = New Padding(3, 4, 3, 4)
        PreVeiwImgBox.Name = "PreVeiwImgBox"
        PreVeiwImgBox.Size = New Size(267, 211)
        PreVeiwImgBox.SizeMode = PictureBoxSizeMode.Zoom
        PreVeiwImgBox.TabIndex = 0
        PreVeiwImgBox.TabStop = False
        ' 
        ' meshTab
        ' 
        meshTab.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        meshTab.Controls.Add(meshStatus)
        meshTab.Controls.Add(meshList)
        meshTab.Controls.Add(meshProgress)
        meshTab.Controls.Add(meshScanButton)
        meshTab.Controls.Add(meshExportButton)
        meshTab.Controls.Add(meshExportAllButton)
        meshTab.Location = New Point(4, 29)
        meshTab.Name = "meshTab"
        meshTab.Padding = New Padding(7, 8, 7, 8)
        meshTab.Size = New Size(494, 475)
        meshTab.TabIndex = 5
        meshTab.Text = "Meshes"
        ' 
        ' meshStatus
        ' 
        meshStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        meshStatus.ForeColor = Color.White
        meshStatus.Location = New Point(7, 8)
        meshStatus.Name = "meshStatus"
        meshStatus.Size = New Size(479, 23)
        meshStatus.TabIndex = 0
        meshStatus.Text = "Scan the Roblox cache to find meshes."
        ' 
        ' meshList
        ' 
        meshList.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        meshList.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        meshList.BorderStyle = BorderStyle.FixedSingle
        meshList.Font = New Font("Segoe UI", 9F)
        meshList.ForeColor = Color.White
        meshList.FormattingEnabled = True
        meshList.HorizontalScrollbar = True
        meshList.Location = New Point(7, 35)
        meshList.Name = "meshList"
        meshList.Size = New Size(479, 322)
        meshList.TabIndex = 1
        ' 
        ' meshProgress
        ' 
        meshProgress.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        meshProgress.Location = New Point(7, 364)
        meshProgress.Name = "meshProgress"
        meshProgress.Size = New Size(479, 27)
        meshProgress.TabIndex = 2
        ' 
        ' meshScanButton
        ' 
        meshScanButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        meshScanButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        meshScanButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        meshScanButton.FlatStyle = FlatStyle.Flat
        meshScanButton.Font = New Font("Segoe UI", 10F)
        meshScanButton.ForeColor = Color.White
        meshScanButton.Location = New Point(7, 399)
        meshScanButton.Name = "meshScanButton"
        meshScanButton.Size = New Size(120, 63)
        meshScanButton.TabIndex = 3
        meshScanButton.Text = "Scan Cache"
        meshScanButton.UseVisualStyleBackColor = False
        ' 
        ' meshExportButton
        ' 
        meshExportButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        meshExportButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        meshExportButton.Enabled = False
        meshExportButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        meshExportButton.FlatStyle = FlatStyle.Flat
        meshExportButton.Font = New Font("Segoe UI", 10F)
        meshExportButton.ForeColor = Color.White
        meshExportButton.Location = New Point(134, 399)
        meshExportButton.Name = "meshExportButton"
        meshExportButton.Size = New Size(166, 63)
        meshExportButton.TabIndex = 4
        meshExportButton.Text = "Export Selected"
        meshExportButton.UseVisualStyleBackColor = False
        ' 
        ' meshExportAllButton
        ' 
        meshExportAllButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        meshExportAllButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        meshExportAllButton.Enabled = False
        meshExportAllButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        meshExportAllButton.FlatStyle = FlatStyle.Flat
        meshExportAllButton.Font = New Font("Segoe UI", 10F)
        meshExportAllButton.ForeColor = Color.White
        meshExportAllButton.Location = New Point(307, 399)
        meshExportAllButton.Name = "meshExportAllButton"
        meshExportAllButton.Size = New Size(179, 63)
        meshExportAllButton.TabIndex = 5
        meshExportAllButton.Text = "Export All"
        meshExportAllButton.UseVisualStyleBackColor = False
        ' 
        ' cacheAssetTab
        ' 
        cacheAssetTab.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        cacheAssetTab.Controls.Add(cacheAssetStatus)
        cacheAssetTab.Controls.Add(cacheAssetFilter)
        cacheAssetTab.Controls.Add(cacheAssetList)
        cacheAssetTab.Controls.Add(cacheAssetProgress)
        cacheAssetTab.Controls.Add(cacheAssetScanButton)
        cacheAssetTab.Controls.Add(cacheAssetExportButton)
        cacheAssetTab.Controls.Add(cacheAssetExportAllButton)
        cacheAssetTab.Location = New Point(4, 29)
        cacheAssetTab.Name = "cacheAssetTab"
        cacheAssetTab.Padding = New Padding(7, 8, 7, 8)
        cacheAssetTab.Size = New Size(494, 475)
        cacheAssetTab.TabIndex = 6
        cacheAssetTab.Text = "RBXM & KTX"
        ' 
        ' cacheAssetStatus
        ' 
        cacheAssetStatus.ForeColor = Color.White
        cacheAssetStatus.Location = New Point(7, 8)
        cacheAssetStatus.Name = "cacheAssetStatus"
        cacheAssetStatus.Size = New Size(335, 23)
        cacheAssetStatus.TabIndex = 0
        cacheAssetStatus.Text = "Scan the Roblox cache for models and textures."
        ' 
        ' cacheAssetFilter
        ' 
        cacheAssetFilter.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cacheAssetFilter.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        cacheAssetFilter.DropDownStyle = ComboBoxStyle.DropDownList
        cacheAssetFilter.ForeColor = Color.White
        cacheAssetFilter.FormattingEnabled = True
        cacheAssetFilter.Items.AddRange(New Object() {"All files", "RBXM only", "KTX only"})
        cacheAssetFilter.Location = New Point(348, 4)
        cacheAssetFilter.Name = "cacheAssetFilter"
        cacheAssetFilter.Size = New Size(138, 28)
        cacheAssetFilter.TabIndex = 1
        ' 
        ' cacheAssetList
        ' 
        cacheAssetList.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        cacheAssetList.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        cacheAssetList.BorderStyle = BorderStyle.FixedSingle
        cacheAssetList.Font = New Font("Segoe UI", 9F)
        cacheAssetList.ForeColor = Color.White
        cacheAssetList.FormattingEnabled = True
        cacheAssetList.HorizontalScrollbar = True
        cacheAssetList.Location = New Point(7, 35)
        cacheAssetList.Name = "cacheAssetList"
        cacheAssetList.Size = New Size(479, 322)
        cacheAssetList.TabIndex = 2
        ' 
        ' cacheAssetProgress
        ' 
        cacheAssetProgress.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        cacheAssetProgress.Location = New Point(7, 364)
        cacheAssetProgress.Name = "cacheAssetProgress"
        cacheAssetProgress.Size = New Size(479, 27)
        cacheAssetProgress.TabIndex = 3
        ' 
        ' cacheAssetScanButton
        ' 
        cacheAssetScanButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        cacheAssetScanButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        cacheAssetScanButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        cacheAssetScanButton.FlatStyle = FlatStyle.Flat
        cacheAssetScanButton.Font = New Font("Segoe UI", 10F)
        cacheAssetScanButton.ForeColor = Color.White
        cacheAssetScanButton.Location = New Point(7, 399)
        cacheAssetScanButton.Name = "cacheAssetScanButton"
        cacheAssetScanButton.Size = New Size(120, 63)
        cacheAssetScanButton.TabIndex = 4
        cacheAssetScanButton.Text = "Scan Cache"
        cacheAssetScanButton.UseVisualStyleBackColor = False
        ' 
        ' cacheAssetExportButton
        ' 
        cacheAssetExportButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        cacheAssetExportButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        cacheAssetExportButton.Enabled = False
        cacheAssetExportButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        cacheAssetExportButton.FlatStyle = FlatStyle.Flat
        cacheAssetExportButton.Font = New Font("Segoe UI", 10F)
        cacheAssetExportButton.ForeColor = Color.White
        cacheAssetExportButton.Location = New Point(134, 399)
        cacheAssetExportButton.Name = "cacheAssetExportButton"
        cacheAssetExportButton.Size = New Size(166, 63)
        cacheAssetExportButton.TabIndex = 5
        cacheAssetExportButton.Text = "Export Selected"
        cacheAssetExportButton.UseVisualStyleBackColor = False
        ' 
        ' cacheAssetExportAllButton
        ' 
        cacheAssetExportAllButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        cacheAssetExportAllButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        cacheAssetExportAllButton.Enabled = False
        cacheAssetExportAllButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        cacheAssetExportAllButton.FlatStyle = FlatStyle.Flat
        cacheAssetExportAllButton.Font = New Font("Segoe UI", 10F)
        cacheAssetExportAllButton.ForeColor = Color.White
        cacheAssetExportAllButton.Location = New Point(307, 399)
        cacheAssetExportAllButton.Name = "cacheAssetExportAllButton"
        cacheAssetExportAllButton.Size = New Size(179, 63)
        cacheAssetExportAllButton.TabIndex = 6
        cacheAssetExportAllButton.Text = "Export All"
        cacheAssetExportAllButton.UseVisualStyleBackColor = False
        ' 
        ' thumbnailTab
        ' 
        thumbnailTab.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        thumbnailTab.Controls.Add(thumbnailStatus)
        thumbnailTab.Controls.Add(thumbnailList)
        thumbnailTab.Controls.Add(thumbnailPreview)
        thumbnailTab.Controls.Add(thumbnailProgress)
        thumbnailTab.Controls.Add(thumbnailScanButton)
        thumbnailTab.Controls.Add(thumbnailExportButton)
        thumbnailTab.Controls.Add(thumbnailExportAllButton)
        thumbnailTab.Location = New Point(4, 29)
        thumbnailTab.Name = "thumbnailTab"
        thumbnailTab.Padding = New Padding(7, 8, 7, 8)
        thumbnailTab.Size = New Size(494, 475)
        thumbnailTab.TabIndex = 7
        thumbnailTab.Text = "Thumbnails"
        ' 
        ' thumbnailStatus
        ' 
        thumbnailStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        thumbnailStatus.ForeColor = Color.White
        thumbnailStatus.Location = New Point(7, 8)
        thumbnailStatus.Name = "thumbnailStatus"
        thumbnailStatus.Size = New Size(479, 23)
        thumbnailStatus.TabIndex = 0
        thumbnailStatus.Text = "Scan for cached avatar, headshot, and thumbnail images."
        ' 
        ' thumbnailList
        ' 
        thumbnailList.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        thumbnailList.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        thumbnailList.BorderStyle = BorderStyle.FixedSingle
        thumbnailList.Font = New Font("Segoe UI", 8.5F)
        thumbnailList.ForeColor = Color.White
        thumbnailList.FormattingEnabled = True
        thumbnailList.HorizontalScrollbar = True
        thumbnailList.ItemHeight = 19
        thumbnailList.Location = New Point(7, 35)
        thumbnailList.Name = "thumbnailList"
        thumbnailList.Size = New Size(230, 306)
        thumbnailList.TabIndex = 1
        ' 
        ' thumbnailPreview
        ' 
        thumbnailPreview.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        thumbnailPreview.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        thumbnailPreview.BorderStyle = BorderStyle.FixedSingle
        thumbnailPreview.Location = New Point(244, 35)
        thumbnailPreview.Name = "thumbnailPreview"
        thumbnailPreview.Size = New Size(242, 322)
        thumbnailPreview.SizeMode = PictureBoxSizeMode.Zoom
        thumbnailPreview.TabIndex = 2
        thumbnailPreview.TabStop = False
        ' 
        ' thumbnailProgress
        ' 
        thumbnailProgress.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        thumbnailProgress.Location = New Point(7, 364)
        thumbnailProgress.Name = "thumbnailProgress"
        thumbnailProgress.Size = New Size(479, 27)
        thumbnailProgress.TabIndex = 3
        ' 
        ' thumbnailScanButton
        ' 
        thumbnailScanButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        thumbnailScanButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        thumbnailScanButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        thumbnailScanButton.FlatStyle = FlatStyle.Flat
        thumbnailScanButton.Font = New Font("Segoe UI", 10F)
        thumbnailScanButton.ForeColor = Color.White
        thumbnailScanButton.Location = New Point(7, 399)
        thumbnailScanButton.Name = "thumbnailScanButton"
        thumbnailScanButton.Size = New Size(120, 63)
        thumbnailScanButton.TabIndex = 4
        thumbnailScanButton.Text = "Scan Cache"
        thumbnailScanButton.UseVisualStyleBackColor = False
        ' 
        ' thumbnailExportButton
        ' 
        thumbnailExportButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        thumbnailExportButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        thumbnailExportButton.Enabled = False
        thumbnailExportButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        thumbnailExportButton.FlatStyle = FlatStyle.Flat
        thumbnailExportButton.Font = New Font("Segoe UI", 10F)
        thumbnailExportButton.ForeColor = Color.White
        thumbnailExportButton.Location = New Point(134, 399)
        thumbnailExportButton.Name = "thumbnailExportButton"
        thumbnailExportButton.Size = New Size(166, 63)
        thumbnailExportButton.TabIndex = 5
        thumbnailExportButton.Text = "Export Selected"
        thumbnailExportButton.UseVisualStyleBackColor = False
        ' 
        ' thumbnailExportAllButton
        ' 
        thumbnailExportAllButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        thumbnailExportAllButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        thumbnailExportAllButton.Enabled = False
        thumbnailExportAllButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        thumbnailExportAllButton.FlatStyle = FlatStyle.Flat
        thumbnailExportAllButton.Font = New Font("Segoe UI", 10F)
        thumbnailExportAllButton.ForeColor = Color.White
        thumbnailExportAllButton.Location = New Point(307, 399)
        thumbnailExportAllButton.Name = "thumbnailExportAllButton"
        thumbnailExportAllButton.Size = New Size(179, 63)
        thumbnailExportAllButton.TabIndex = 6
        thumbnailExportAllButton.Text = "Export All"
        thumbnailExportAllButton.UseVisualStyleBackColor = False
        ' 
        ' fontTab
        ' 
        fontTab.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        fontTab.Controls.Add(fontStatus)
        fontTab.Controls.Add(fontList)
        fontTab.Controls.Add(fontProgress)
        fontTab.Controls.Add(fontScanButton)
        fontTab.Controls.Add(fontExportButton)
        fontTab.Controls.Add(fontExportAllButton)
        fontTab.Location = New Point(4, 29)
        fontTab.Name = "fontTab"
        fontTab.Padding = New Padding(7, 8, 7, 8)
        fontTab.Size = New Size(494, 475)
        fontTab.TabIndex = 8
        fontTab.Text = "Fonts"
        ' 
        ' fontStatus
        ' 
        fontStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        fontStatus.ForeColor = Color.White
        fontStatus.Location = New Point(7, 8)
        fontStatus.Name = "fontStatus"
        fontStatus.Size = New Size(479, 23)
        fontStatus.TabIndex = 0
        fontStatus.Text = "Scan the Roblox cache for TTF and OTF fonts."
        ' 
        ' fontList
        ' 
        fontList.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        fontList.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        fontList.BorderStyle = BorderStyle.FixedSingle
        fontList.Font = New Font("Segoe UI", 9F)
        fontList.ForeColor = Color.White
        fontList.FormattingEnabled = True
        fontList.HorizontalScrollbar = True
        fontList.Location = New Point(7, 35)
        fontList.Name = "fontList"
        fontList.Size = New Size(479, 322)
        fontList.TabIndex = 1
        ' 
        ' fontProgress
        ' 
        fontProgress.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        fontProgress.Location = New Point(7, 364)
        fontProgress.Name = "fontProgress"
        fontProgress.Size = New Size(479, 27)
        fontProgress.TabIndex = 2
        ' 
        ' fontScanButton
        ' 
        fontScanButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        fontScanButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        fontScanButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        fontScanButton.FlatStyle = FlatStyle.Flat
        fontScanButton.Font = New Font("Segoe UI", 10F)
        fontScanButton.ForeColor = Color.White
        fontScanButton.Location = New Point(7, 399)
        fontScanButton.Name = "fontScanButton"
        fontScanButton.Size = New Size(120, 63)
        fontScanButton.TabIndex = 3
        fontScanButton.Text = "Scan Cache"
        fontScanButton.UseVisualStyleBackColor = False
        ' 
        ' fontExportButton
        ' 
        fontExportButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        fontExportButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        fontExportButton.Enabled = False
        fontExportButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        fontExportButton.FlatStyle = FlatStyle.Flat
        fontExportButton.Font = New Font("Segoe UI", 10F)
        fontExportButton.ForeColor = Color.White
        fontExportButton.Location = New Point(134, 399)
        fontExportButton.Name = "fontExportButton"
        fontExportButton.Size = New Size(166, 63)
        fontExportButton.TabIndex = 4
        fontExportButton.Text = "Export Selected"
        fontExportButton.UseVisualStyleBackColor = False
        ' 
        ' fontExportAllButton
        ' 
        fontExportAllButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        fontExportAllButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        fontExportAllButton.Enabled = False
        fontExportAllButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        fontExportAllButton.FlatStyle = FlatStyle.Flat
        fontExportAllButton.Font = New Font("Segoe UI", 10F)
        fontExportAllButton.ForeColor = Color.White
        fontExportAllButton.Location = New Point(307, 399)
        fontExportAllButton.Name = "fontExportAllButton"
        fontExportAllButton.Size = New Size(179, 63)
        fontExportAllButton.TabIndex = 5
        fontExportAllButton.Text = "Export All"
        fontExportAllButton.UseVisualStyleBackColor = False
        ' 
        ' metadataTab
        ' 
        metadataTab.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        metadataTab.Controls.Add(metadataStatus)
        metadataTab.Controls.Add(metadataList)
        metadataTab.Controls.Add(metadataPreview)
        metadataTab.Controls.Add(metadataProgress)
        metadataTab.Controls.Add(metadataScanButton)
        metadataTab.Controls.Add(metadataExportButton)
        metadataTab.Controls.Add(metadataExportAllButton)
        metadataTab.Location = New Point(4, 29)
        metadataTab.Name = "metadataTab"
        metadataTab.Padding = New Padding(7, 8, 7, 8)
        metadataTab.Size = New Size(494, 475)
        metadataTab.TabIndex = 9
        metadataTab.Text = "Metadata"
        ' 
        ' metadataStatus
        ' 
        metadataStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        metadataStatus.ForeColor = Color.White
        metadataStatus.Location = New Point(7, 8)
        metadataStatus.Name = "metadataStatus"
        metadataStatus.Size = New Size(479, 23)
        metadataStatus.TabIndex = 0
        metadataStatus.Text = "Scan for cached JSON, XML, and HLS playlists."
        ' 
        ' metadataList
        ' 
        metadataList.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        metadataList.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        metadataList.BorderStyle = BorderStyle.FixedSingle
        metadataList.Font = New Font("Segoe UI", 8.5F)
        metadataList.ForeColor = Color.White
        metadataList.FormattingEnabled = True
        metadataList.HorizontalScrollbar = True
        metadataList.ItemHeight = 19
        metadataList.Location = New Point(7, 35)
        metadataList.Name = "metadataList"
        metadataList.Size = New Size(220, 306)
        metadataList.TabIndex = 1
        ' 
        ' metadataPreview
        ' 
        metadataPreview.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        metadataPreview.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        metadataPreview.BorderStyle = BorderStyle.FixedSingle
        metadataPreview.Font = New Font("Consolas", 8F)
        metadataPreview.ForeColor = Color.White
        metadataPreview.Location = New Point(234, 35)
        metadataPreview.Multiline = True
        metadataPreview.Name = "metadataPreview"
        metadataPreview.ReadOnly = True
        metadataPreview.ScrollBars = ScrollBars.Both
        metadataPreview.Size = New Size(252, 322)
        metadataPreview.TabIndex = 2
        metadataPreview.WordWrap = False
        ' 
        ' metadataProgress
        ' 
        metadataProgress.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        metadataProgress.Location = New Point(7, 364)
        metadataProgress.Name = "metadataProgress"
        metadataProgress.Size = New Size(479, 27)
        metadataProgress.TabIndex = 3
        ' 
        ' metadataScanButton
        ' 
        metadataScanButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        metadataScanButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        metadataScanButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        metadataScanButton.FlatStyle = FlatStyle.Flat
        metadataScanButton.Font = New Font("Segoe UI", 10F)
        metadataScanButton.ForeColor = Color.White
        metadataScanButton.Location = New Point(7, 399)
        metadataScanButton.Name = "metadataScanButton"
        metadataScanButton.Size = New Size(120, 63)
        metadataScanButton.TabIndex = 4
        metadataScanButton.Text = "Scan Cache"
        metadataScanButton.UseVisualStyleBackColor = False
        ' 
        ' metadataExportButton
        ' 
        metadataExportButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        metadataExportButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        metadataExportButton.Enabled = False
        metadataExportButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        metadataExportButton.FlatStyle = FlatStyle.Flat
        metadataExportButton.Font = New Font("Segoe UI", 10F)
        metadataExportButton.ForeColor = Color.White
        metadataExportButton.Location = New Point(134, 399)
        metadataExportButton.Name = "metadataExportButton"
        metadataExportButton.Size = New Size(166, 63)
        metadataExportButton.TabIndex = 5
        metadataExportButton.Text = "Export Selected"
        metadataExportButton.UseVisualStyleBackColor = False
        ' 
        ' metadataExportAllButton
        ' 
        metadataExportAllButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        metadataExportAllButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        metadataExportAllButton.Enabled = False
        metadataExportAllButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        metadataExportAllButton.FlatStyle = FlatStyle.Flat
        metadataExportAllButton.Font = New Font("Segoe UI", 10F)
        metadataExportAllButton.ForeColor = Color.White
        metadataExportAllButton.Location = New Point(307, 399)
        metadataExportAllButton.Name = "metadataExportAllButton"
        metadataExportAllButton.Size = New Size(179, 63)
        metadataExportAllButton.TabIndex = 6
        metadataExportAllButton.Text = "Export All"
        metadataExportAllButton.UseVisualStyleBackColor = False
        ' 
        ' TabPage1
        ' 
        TabPage1.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        TabPage1.BackgroundImageLayout = ImageLayout.Center
        TabPage1.Controls.Add(SaveLogBtn)
        TabPage1.Controls.Add(AutoScrollCHK)
        TabPage1.Controls.Add(output_log)
        TabPage1.Location = New Point(4, 29)
        TabPage1.Margin = New Padding(3, 4, 3, 4)
        TabPage1.Name = "TabPage1"
        TabPage1.Padding = New Padding(3, 4, 3, 4)
        TabPage1.Size = New Size(494, 475)
        TabPage1.TabIndex = 0
        TabPage1.Text = "Log"
        ' 
        ' SaveLogBtn
        ' 
        SaveLogBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        SaveLogBtn.FlatAppearance.BorderColor = Color.Silver
        SaveLogBtn.FlatAppearance.BorderSize = 2
        SaveLogBtn.FlatStyle = FlatStyle.Popup
        SaveLogBtn.Font = New Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        SaveLogBtn.ForeColor = Color.White
        SaveLogBtn.Location = New Point(400, 429)
        SaveLogBtn.Margin = New Padding(3, 4, 3, 4)
        SaveLogBtn.Name = "SaveLogBtn"
        SaveLogBtn.Size = New Size(86, 32)
        SaveLogBtn.TabIndex = 3
        SaveLogBtn.Text = "Save Log"
        SaveLogBtn.UseVisualStyleBackColor = False
        ' 
        ' AutoScrollCHK
        ' 
        AutoScrollCHK.AutoSize = True
        AutoScrollCHK.Checked = True
        AutoScrollCHK.CheckState = CheckState.Checked
        AutoScrollCHK.Font = New Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        AutoScrollCHK.ForeColor = Color.White
        AutoScrollCHK.Location = New Point(7, 431)
        AutoScrollCHK.Margin = New Padding(3, 4, 3, 4)
        AutoScrollCHK.Name = "AutoScrollCHK"
        AutoScrollCHK.Size = New Size(111, 27)
        AutoScrollCHK.TabIndex = 2
        AutoScrollCHK.Text = "auto scroll"
        AutoScrollCHK.UseVisualStyleBackColor = True
        ' 
        ' VText_LBR
        ' 
        VText_LBR.BackColor = Color.Transparent
        VText_LBR.Font = New Font("MS UI Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        VText_LBR.ForeColor = Color.White
        VText_LBR.Location = New Point(50, 27)
        VText_LBR.Name = "VText_LBR"
        VText_LBR.Size = New Size(275, 19)
        VText_LBR.TabIndex = 5
        VText_LBR.Text = "Currently running version:" & vbCrLf
        ' 
        ' CheckFOrButtons
        ' 
        CheckFOrButtons.Enabled = True
        ' 
        ' ListboxAutoScrool
        ' 
        ListboxAutoScrool.Enabled = True
        ListboxAutoScrool.Interval = 1
        ' 
        ' StatusLBR
        ' 
        StatusLBR.AutoSize = True
        StatusLBR.BackColor = Color.Transparent
        StatusLBR.Font = New Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        StatusLBR.ForeColor = Color.White
        StatusLBR.Location = New Point(14, 693)
        StatusLBR.Name = "StatusLBR"
        StatusLBR.Size = New Size(41, 28)
        StatusLBR.TabIndex = 10
        StatusLBR.Text = "0%"
        StatusLBR.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Panel2
        ' 
        Panel2.BackColor = Color.FromArgb(CByte(20), CByte(23), CByte(29))
        Panel2.Controls.Add(LoadingGif)
        Panel2.Controls.Add(PictureBox1)
        Panel2.Controls.Add(Label6)
        Panel2.Controls.Add(Button2)
        Panel2.Controls.Add(CloseBTN)
        Panel2.Controls.Add(VText_LBR)
        Panel2.Location = New Point(-3, -1)
        Panel2.Margin = New Padding(3, 4, 3, 4)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(544, 53)
        Panel2.TabIndex = 11
        ' 
        ' LoadingGif
        ' 
        LoadingGif.Image = My.Resources.Resources.Rolling_1x_1_0s_200px_200px
        LoadingGif.Location = New Point(408, 8)
        LoadingGif.Margin = New Padding(3, 4, 3, 4)
        LoadingGif.Name = "LoadingGif"
        LoadingGif.Size = New Size(32, 37)
        LoadingGif.SizeMode = PictureBoxSizeMode.Zoom
        LoadingGif.TabIndex = 6
        LoadingGif.TabStop = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Image = My.Resources.Resources.RobloxRippper
        PictureBox1.Location = New Point(3, 4)
        PictureBox1.Margin = New Padding(3, 4, 3, 4)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(38, 41)
        PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
        PictureBox1.TabIndex = 3
        PictureBox1.TabStop = False
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.ForeColor = Color.White
        Label6.Location = New Point(48, 4)
        Label6.Name = "Label6"
        Label6.Size = New Size(308, 23)
        Label6.TabIndex = 2
        Label6.Text = "RBX Asset Extractor (made by Vex :3)"
        Label6.TextAlign = ContentAlignment.TopCenter
        ' 
        ' Button2
        ' 
        Button2.BackgroundImageLayout = ImageLayout.Center
        Button2.FlatAppearance.BorderSize = 0
        Button2.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        Button2.FlatStyle = FlatStyle.Flat
        Button2.Font = New Font("Franklin Gothic Medium", 18F, FontStyle.Bold)
        Button2.ForeColor = Color.White
        Button2.Location = New Point(447, 0)
        Button2.Margin = New Padding(3, 4, 3, 4)
        Button2.Name = "Button2"
        Button2.Size = New Size(42, 53)
        Button2.TabIndex = 1
        Button2.Text = "-"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' CloseBTN
        ' 
        CloseBTN.BackgroundImageLayout = ImageLayout.Center
        CloseBTN.FlatAppearance.BorderSize = 0
        CloseBTN.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(208), CByte(1), CByte(27))
        CloseBTN.FlatStyle = FlatStyle.Flat
        CloseBTN.Font = New Font("Franklin Gothic Medium", 18F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        CloseBTN.ForeColor = Color.White
        CloseBTN.Location = New Point(490, 0)
        CloseBTN.Margin = New Padding(3, 4, 3, 4)
        CloseBTN.Name = "CloseBTN"
        CloseBTN.Size = New Size(42, 53)
        CloseBTN.TabIndex = 0
        CloseBTN.Text = "X"
        CloseBTN.TextAlign = ContentAlignment.MiddleRight
        CloseBTN.UseVisualStyleBackColor = True
        ' 
        ' fadeInTimer
        ' 
        ' 
        ' fadeOutTimer
        ' 
        ' 
        ' AlwaysOnTopCHK
        ' 
        AlwaysOnTopCHK.AutoSize = True
        AlwaysOnTopCHK.ForeColor = Color.White
        AlwaysOnTopCHK.Location = New Point(14, 576)
        AlwaysOnTopCHK.Margin = New Padding(3, 4, 3, 4)
        AlwaysOnTopCHK.Name = "AlwaysOnTopCHK"
        AlwaysOnTopCHK.Size = New Size(129, 24)
        AlwaysOnTopCHK.TabIndex = 12
        AlwaysOnTopCHK.Text = "Always On Top"
        AlwaysOnTopCHK.UseVisualStyleBackColor = True
        ' 
        ' KeepButtonsOff
        ' 
        ' 
        ' MainLoop
        ' 
        MainLoop.Enabled = True
        MainLoop.Interval = 1000
        ' 
        ' LoadPartialBackgoundWorker
        ' 
        ' 
        ' ClearCache
        ' 
        ' 
        ' MSGPopup
        ' 
        ' 
        ' ProgressBar1
        ' 
        ProgressBar1.FontSize = MetroFramework.MetroProgressBarSize.Medium
        ProgressBar1.FontWeight = MetroFramework.MetroProgressBarWeight.Light
        ProgressBar1.ForeColor = Color.Red
        ProgressBar1.HideProgressText = True
        ProgressBar1.Location = New Point(59, 696)
        ProgressBar1.Margin = New Padding(3, 4, 3, 4)
        ProgressBar1.Name = "ProgressBar1"
        ProgressBar1.ProgressBarStyle = ProgressBarStyle.Continuous
        ProgressBar1.Size = New Size(456, 25)
        ProgressBar1.Style = MetroFramework.MetroColorStyle.Blue
        ProgressBar1.StyleManager = Nothing
        ProgressBar1.TabIndex = 13
        ProgressBar1.TextAlign = ContentAlignment.MiddleRight
        ProgressBar1.Theme = MetroFramework.MetroThemeStyle.Dark
        ' 
        ' TaskLBR
        ' 
        TaskLBR.AutoSize = True
        TaskLBR.BackColor = Color.Transparent
        TaskLBR.Font = New Font("MS UI Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        TaskLBR.ForeColor = Color.White
        TaskLBR.Location = New Point(161, 580)
        TaskLBR.Name = "TaskLBR"
        TaskLBR.Size = New Size(76, 15)
        TaskLBR.TabIndex = 7
        TaskLBR.Text = "Task 0 / 4"
        ' 
        ' trackBarTimeline
        ' 
        trackBarTimeline.BackColor = Color.White
        trackBarTimeline.CustomBackground = False
        trackBarTimeline.LargeChange = 5UI
        trackBarTimeline.Location = New Point(3, 4)
        trackBarTimeline.Margin = New Padding(3, 4, 3, 4)
        trackBarTimeline.Maximum = 100
        trackBarTimeline.Minimum = 0
        trackBarTimeline.MouseWheelBarPartitions = 10
        trackBarTimeline.Name = "trackBarTimeline"
        trackBarTimeline.Size = New Size(494, 31)
        trackBarTimeline.SmallChange = 1UI
        trackBarTimeline.Style = MetroFramework.MetroColorStyle.Blue
        trackBarTimeline.StyleManager = Nothing
        trackBarTimeline.TabIndex = 14
        trackBarTimeline.Text = "MetroTrackBar1"
        trackBarTimeline.Theme = MetroFramework.MetroThemeStyle.Dark
        trackBarTimeline.Value = 50
        ' 
        ' lblElapsedTime
        ' 
        lblElapsedTime.AutoSize = True
        lblElapsedTime.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblElapsedTime.ForeColor = Color.White
        lblElapsedTime.Location = New Point(11, 35)
        lblElapsedTime.Name = "lblElapsedTime"
        lblElapsedTime.Size = New Size(56, 25)
        lblElapsedTime.TabIndex = 15
        lblElapsedTime.Text = "00:00"
        ' 
        ' lblTotalTime
        ' 
        lblTotalTime.AutoSize = True
        lblTotalTime.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblTotalTime.ForeColor = Color.White
        lblTotalTime.Location = New Point(406, 35)
        lblTotalTime.Name = "lblTotalTime"
        lblTotalTime.Size = New Size(56, 25)
        lblTotalTime.TabIndex = 16
        lblTotalTime.Text = "00:00"
        ' 
        ' playbackTimer
        ' 
        ' 
        ' SoundPlayerPlayBtn
        ' 
        SoundPlayerPlayBtn.BackgroundImage = My.Resources.Resources.RedPlayButton
        SoundPlayerPlayBtn.BackgroundImageLayout = ImageLayout.Zoom
        SoundPlayerPlayBtn.FlatAppearance.BorderSize = 0
        SoundPlayerPlayBtn.FlatStyle = FlatStyle.Flat
        SoundPlayerPlayBtn.ForeColor = Color.Transparent
        SoundPlayerPlayBtn.Location = New Point(455, 35)
        SoundPlayerPlayBtn.Margin = New Padding(3, 4, 3, 4)
        SoundPlayerPlayBtn.Name = "SoundPlayerPlayBtn"
        SoundPlayerPlayBtn.Size = New Size(42, 31)
        SoundPlayerPlayBtn.TabIndex = 17
        SoundPlayerPlayBtn.UseVisualStyleBackColor = True
        ' 
        ' Panel3
        ' 
        Panel3.BackColor = Color.FromArgb(CByte(15), CByte(15), CByte(15))
        Panel3.Controls.Add(VolumeControl1)
        Panel3.Controls.Add(trackBarTimeline)
        Panel3.Controls.Add(SoundPlayerPlayBtn)
        Panel3.Controls.Add(lblElapsedTime)
        Panel3.Controls.Add(lblTotalTime)
        Panel3.Location = New Point(14, 609)
        Panel3.Margin = New Padding(3, 4, 3, 4)
        Panel3.Name = "Panel3"
        Panel3.Size = New Size(502, 79)
        Panel3.TabIndex = 18
        ' 
        ' VolumeControl1
        ' 
        VolumeControl1.Location = New Point(247, 32)
        VolumeControl1.Margin = New Padding(5, 4, 5, 4)
        VolumeControl1.MinimumSize = New Size(73, 29)
        VolumeControl1.Mute = False
        VolumeControl1.Name = "VolumeControl1"
        VolumeControl1.Size = New Size(151, 29)
        VolumeControl1.TabIndex = 19
        VolumeControl1.Volume = 100
        ' 
        ' KeepPlayback0
        ' 
        KeepPlayback0.Enabled = True
        KeepPlayback0.Interval = 1
        ' 
        ' ChangeVol
        ' 
        ' 
        ' MainForm
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        BackgroundImageLayout = ImageLayout.Stretch
        ClientSize = New Size(529, 732)
        Controls.Add(Panel3)
        Controls.Add(TaskLBR)
        Controls.Add(ProgressBar1)
        Controls.Add(AlwaysOnTopCHK)
        Controls.Add(Panel2)
        Controls.Add(StatusLBR)
        Controls.Add(TabControl1)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(3, 4, 3, 4)
        MaximizeBox = False
        Name = "MainForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "RBX Asset Extractor (made by ZV800)"
        TabControl1.ResumeLayout(False)
        TabPage3.ResumeLayout(False)
        TabPage3.PerformLayout()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        TabPage2.ResumeLayout(False)
        TabPage4.ResumeLayout(False)
        TabPage5.ResumeLayout(False)
        TabPage5.PerformLayout()
        CType(PreVeiwImgBox, ComponentModel.ISupportInitialize).EndInit()
        meshTab.ResumeLayout(False)
        cacheAssetTab.ResumeLayout(False)
        thumbnailTab.ResumeLayout(False)
        CType(thumbnailPreview, ComponentModel.ISupportInitialize).EndInit()
        fontTab.ResumeLayout(False)
        metadataTab.ResumeLayout(False)
        metadataTab.PerformLayout()
        TabPage1.ResumeLayout(False)
        TabPage1.PerformLayout()
        Panel2.ResumeLayout(False)
        Panel2.PerformLayout()
        CType(LoadingGif, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        Panel3.ResumeLayout(False)
        Panel3.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents output_log As ListBox
    Friend WithEvents SaveFileDialog1 As SaveFileDialog
    Friend WithEvents LoadParButton As Button
    Friend WithEvents Download_BTN As Button
    Friend WithEvents ClearTMP_BTN As Button
    Friend WithEvents DownloadALL_BTN As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents Sounds_Listbox As ListBox
    Friend WithEvents TabPage3 As TabPage
    Friend WithEvents CheckFOrButtons As Timer
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents VText_LBR As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents LinkLabel1 As LinkLabel
    Friend WithEvents ClearTmp As System.ComponentModel.BackgroundWorker
    Friend WithEvents TabPage4 As TabPage
    Friend WithEvents HTTPLISTBOX As ListBox
    Friend WithEvents DownloadAllHTTP_BTN As Button
    Friend WithEvents DOWNLOADHTTP_BTN As Button
    Friend WithEvents ClearHTTPTEMP_BTN As Button
    Friend WithEvents LoadHttpBtn As Button
    Friend WithEvents LoadHTTP0 As System.ComponentModel.BackgroundWorker
    Friend WithEvents Panel1 As Panel
    Friend WithEvents ListboxAutoScrool As Timer
    Friend WithEvents StatusLBR As Label
    Friend WithEvents AutoScrollCHK As CheckBox
    Friend WithEvents Panel2 As Panel
    Friend WithEvents CloseBTN As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label6 As Label
    Friend WithEvents fadeInTimer As Timer
    Friend WithEvents fadeOutTimer As Timer
    Friend WithEvents LinkLabel2 As LinkLabel
    Friend WithEvents SaveLogBtn As Button
    Friend WithEvents TabPage5 As TabPage
    Friend WithEvents LoadImgListBox As ListBox
    Friend WithEvents PreVeiwImgBox As PictureBox
    Friend WithEvents SaveAllBtn As Button
    Friend WithEvents DownloadImgBtn As Button
    Friend WithEvents LoadImgBtn As Button
    Friend WithEvents AlwaysOnTopCHK As CheckBox
    Friend WithEvents Label8 As Label
    Friend WithEvents ImgStats As Label
    Friend WithEvents ImgClearTmp As Button
    Friend WithEvents KeepButtonsOff As Timer
    Friend WithEvents MainLoop As Timer
    Friend WithEvents LoadPartialBackgoundWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents ClearCache As System.ComponentModel.BackgroundWorker
    Friend WithEvents MSGPopup As System.ComponentModel.BackgroundWorker
    Friend WithEvents ProgressBar1 As MetroFramework.Controls.MetroProgressBar
    Friend WithEvents LoadingGif As PictureBox
    Friend WithEvents TaskLBR As Label
    Friend WithEvents trackBarTimeline As MetroFramework.Controls.MetroTrackBar
    Friend WithEvents lblElapsedTime As Label
    Friend WithEvents lblTotalTime As Label
    Friend WithEvents playbackTimer As Timer
    Friend WithEvents SoundPlayerPlayBtn As Button
    Friend WithEvents Panel3 As Panel
    Friend WithEvents KeepPlayback0 As Timer
    Friend WithEvents VolumeControl1 As cmdwtf.Toolkit.WinForms.Controls.VolumeControl
    Friend WithEvents ChangeVol As Timer
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents Label9 As Label
    Friend WithEvents SaveAlAsPngCheck As CheckBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents fullAudioStatus As Label
    Friend WithEvents fullImageStatus As Label
    Friend WithEvents meshTab As TabPage
    Friend WithEvents meshStatus As Label
    Friend WithEvents meshList As ListBox
    Friend WithEvents meshProgress As ProgressBar
    Friend WithEvents meshScanButton As Button
    Friend WithEvents meshExportButton As Button
    Friend WithEvents meshExportAllButton As Button
    Friend WithEvents cacheAssetTab As TabPage
    Friend WithEvents cacheAssetStatus As Label
    Friend WithEvents cacheAssetFilter As ComboBox
    Friend WithEvents cacheAssetList As ListBox
    Friend WithEvents cacheAssetProgress As ProgressBar
    Friend WithEvents cacheAssetScanButton As Button
    Friend WithEvents cacheAssetExportButton As Button
    Friend WithEvents cacheAssetExportAllButton As Button
    Friend WithEvents thumbnailTab As TabPage
    Friend WithEvents thumbnailStatus As Label
    Friend WithEvents thumbnailList As ListBox
    Friend WithEvents thumbnailPreview As PictureBox
    Friend WithEvents thumbnailProgress As ProgressBar
    Friend WithEvents thumbnailScanButton As Button
    Friend WithEvents thumbnailExportButton As Button
    Friend WithEvents thumbnailExportAllButton As Button
    Friend WithEvents fontTab As TabPage
    Friend WithEvents fontStatus As Label
    Friend WithEvents fontList As ListBox
    Friend WithEvents fontProgress As ProgressBar
    Friend WithEvents fontScanButton As Button
    Friend WithEvents fontExportButton As Button
    Friend WithEvents fontExportAllButton As Button
    Friend WithEvents metadataTab As TabPage
    Friend WithEvents metadataStatus As Label
    Friend WithEvents metadataList As ListBox
    Friend WithEvents metadataPreview As TextBox
    Friend WithEvents metadataProgress As ProgressBar
    Friend WithEvents metadataScanButton As Button
    Friend WithEvents metadataExportButton As Button
    Friend WithEvents metadataExportAllButton As Button

End Class

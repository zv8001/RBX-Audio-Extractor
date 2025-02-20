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
        AxWindowsMediaPlayer1 = New AxWMPLib.AxWindowsMediaPlayer()
        DownloadALL_BTN = New Button()
        FolderBrowserDialog1 = New FolderBrowserDialog()
        TabControl1 = New TabControl()
        TabPage3 = New TabPage()
        Label7 = New Label()
        Label4 = New Label()
        LinkLabel2 = New LinkLabel()
        LinkLabel1 = New LinkLabel()
        Label5 = New Label()
        Label3 = New Label()
        Label2 = New Label()
        Label1 = New Label()
        TabPage2 = New TabPage()
        Panel1 = New Panel()
        Sounds_Listbox = New ListBox()
        TabPage4 = New TabPage()
        LoadHttpBtn = New Button()
        ClearHTTPTEMP_BTN = New Button()
        DownloadAllHTTP_BTN = New Button()
        DOWNLOADHTTP_BTN = New Button()
        HTTPLISTBOX = New ListBox()
        TabPage5 = New TabPage()
        ImgClearTmp = New Button()
        ImgStats = New Label()
        SaveAllBtn = New Button()
        DownloadImgBtn = New Button()
        LoadImgBtn = New Button()
        LoadImgListBox = New ListBox()
        PreVeiwImgBox = New PictureBox()
        TabPage1 = New TabPage()
        SaveLogBtn = New Button()
        AutoScrollCHK = New CheckBox()
        VText_LBR = New Label()
        ProgressBar1 = New ProgressBar()
        CheckFOrButtons = New Timer(components)
        ClearTmp = New ComponentModel.BackgroundWorker()
        ListboxAutoScrool = New Timer(components)
        StatusLBR = New Label()
        Panel2 = New Panel()
        PictureBox1 = New PictureBox()
        Label6 = New Label()
        Button2 = New Button()
        CloseBTN = New Button()
        fadeInTimer = New Timer(components)
        fadeOutTimer = New Timer(components)
        Timer3 = New Timer(components)
        AlwaysOnTopCHK = New CheckBox()
        KeepButtonsOff = New Timer(components)
        MainLoop = New Timer(components)
        LoadPartialBackgoundWorker = New ComponentModel.BackgroundWorker()
        ClearCache = New ComponentModel.BackgroundWorker()
        CType(AxWindowsMediaPlayer1, ComponentModel.ISupportInitialize).BeginInit()
        TabControl1.SuspendLayout()
        TabPage3.SuspendLayout()
        TabPage2.SuspendLayout()
        TabPage4.SuspendLayout()
        TabPage5.SuspendLayout()
        CType(PreVeiwImgBox, ComponentModel.ISupportInitialize).BeginInit()
        TabPage1.SuspendLayout()
        Panel2.SuspendLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
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
        output_log.ItemHeight = 12
        output_log.Location = New Point(0, 0)
        output_log.Name = "output_log"
        output_log.Size = New Size(431, 316)
        output_log.TabIndex = 1
        ' 
        ' LoadParButton
        ' 
        LoadParButton.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        LoadParButton.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        LoadParButton.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        LoadParButton.FlatStyle = FlatStyle.Flat
        LoadParButton.Font = New Font("Segoe UI", 12.0F)
        LoadParButton.ForeColor = Color.White
        LoadParButton.Location = New Point(6, 311)
        LoadParButton.Name = "LoadParButton"
        LoadParButton.Size = New Size(75, 35)
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
        Download_BTN.FlatStyle = FlatStyle.Flat
        Download_BTN.Font = New Font("Segoe UI", 12.0F)
        Download_BTN.ForeColor = Color.White
        Download_BTN.Location = New Point(87, 311)
        Download_BTN.Name = "Download_BTN"
        Download_BTN.Size = New Size(105, 35)
        Download_BTN.TabIndex = 3
        Download_BTN.Text = "Download"
        Download_BTN.UseVisualStyleBackColor = False
        ' 
        ' ClearTMP_BTN
        ' 
        ClearTMP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ClearTMP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        ClearTMP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        ClearTMP_BTN.FlatStyle = FlatStyle.Flat
        ClearTMP_BTN.Font = New Font("Segoe UI", 12.0F)
        ClearTMP_BTN.ForeColor = Color.White
        ClearTMP_BTN.Location = New Point(318, 311)
        ClearTMP_BTN.Name = "ClearTMP_BTN"
        ClearTMP_BTN.Size = New Size(107, 35)
        ClearTMP_BTN.TabIndex = 5
        ClearTMP_BTN.Text = "Clear Cache"
        ClearTMP_BTN.UseVisualStyleBackColor = False
        ' 
        ' AxWindowsMediaPlayer1
        ' 
        AxWindowsMediaPlayer1.Enabled = True
        AxWindowsMediaPlayer1.Location = New Point(12, 456)
        AxWindowsMediaPlayer1.Name = "AxWindowsMediaPlayer1"
        AxWindowsMediaPlayer1.OcxState = CType(resources.GetObject("AxWindowsMediaPlayer1.OcxState"), AxHost.State)
        AxWindowsMediaPlayer1.Size = New Size(439, 45)
        AxWindowsMediaPlayer1.TabIndex = 6
        ' 
        ' DownloadALL_BTN
        ' 
        DownloadALL_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DownloadALL_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DownloadALL_BTN.Enabled = False
        DownloadALL_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DownloadALL_BTN.FlatStyle = FlatStyle.Flat
        DownloadALL_BTN.Font = New Font("Segoe UI", 12.0F)
        DownloadALL_BTN.ForeColor = Color.White
        DownloadALL_BTN.Location = New Point(198, 311)
        DownloadALL_BTN.Name = "DownloadALL_BTN"
        DownloadALL_BTN.Size = New Size(114, 35)
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
        TabControl1.Controls.Add(TabPage1)
        TabControl1.Location = New Point(12, 45)
        TabControl1.Name = "TabControl1"
        TabControl1.SelectedIndex = 0
        TabControl1.ShowToolTips = True
        TabControl1.Size = New Size(439, 380)
        TabControl1.TabIndex = 9
        ' 
        ' TabPage3
        ' 
        TabPage3.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        TabPage3.Controls.Add(Label7)
        TabPage3.Controls.Add(Label4)
        TabPage3.Controls.Add(LinkLabel2)
        TabPage3.Controls.Add(LinkLabel1)
        TabPage3.Controls.Add(Label5)
        TabPage3.Controls.Add(Label3)
        TabPage3.Controls.Add(Label2)
        TabPage3.Controls.Add(Label1)
        TabPage3.Location = New Point(4, 24)
        TabPage3.Name = "TabPage3"
        TabPage3.Size = New Size(431, 352)
        TabPage3.TabIndex = 2
        TabPage3.Text = "About"
        ' 
        ' Label7
        ' 
        Label7.Font = New Font("MS UI Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label7.ForeColor = Color.White
        Label7.Location = New Point(23, 237)
        Label7.Name = "Label7"
        Label7.Size = New Size(386, 23)
        Label7.TabIndex = 10
        Label7.Text = "Credits to fearedfusionx for a bit of help on the UI and the icon"
        Label7.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label4
        ' 
        Label4.Font = New Font("MS UI Gothic", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label4.ForeColor = Color.White
        Label4.Location = New Point(23, 260)
        Label4.Name = "Label4"
        Label4.Size = New Size(386, 37)
        Label4.TabIndex = 9
        Label4.Text = "THIS PROGRAM IS FOR EDUCATIONAL PURPOSES"
        Label4.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' LinkLabel2
        ' 
        LinkLabel2.AutoSize = True
        LinkLabel2.LinkColor = Color.White
        LinkLabel2.Location = New Point(3, 327)
        LinkLabel2.Name = "LinkLabel2"
        LinkLabel2.Size = New Size(264, 15)
        LinkLabel2.TabIndex = 8
        LinkLabel2.TabStop = True
        LinkLabel2.Text = "https://github.com/zv8001/RBX-Audio-Extractor"
        ' 
        ' LinkLabel1
        ' 
        LinkLabel1.AutoSize = True
        LinkLabel1.LinkColor = Color.White
        LinkLabel1.Location = New Point(3, 311)
        LinkLabel1.Name = "LinkLabel1"
        LinkLabel1.Size = New Size(103, 15)
        LinkLabel1.TabIndex = 6
        LinkLabel1.TabStop = True
        LinkLabel1.Text = "http://zv800.com/"
        ' 
        ' Label5
        ' 
        Label5.Font = New Font("MS UI Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label5.ForeColor = Color.White
        Label5.Location = New Point(23, 164)
        Label5.Name = "Label5"
        Label5.Size = New Size(386, 58)
        Label5.TabIndex = 4
        Label5.Text = resources.GetString("Label5.Text")
        Label5.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label3
        ' 
        Label3.Font = New Font("MS UI Gothic", 15.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Color.White
        Label3.Location = New Point(23, 73)
        Label3.Name = "Label3"
        Label3.Size = New Size(386, 91)
        Label3.TabIndex = 2
        Label3.Text = "This program was created by zv800. The original idea was suggested by fusion after he told me a way you could extract audios from the Roblox client."
        Label3.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label2
        ' 
        Label2.Anchor = AnchorStyles.Top
        Label2.AutoSize = True
        Label2.Font = New Font("MS UI Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.White
        Label2.Location = New Point(172, 50)
        Label2.Name = "Label2"
        Label2.Size = New Size(89, 13)
        Label2.TabIndex = 1
        Label2.Text = "Made by zv800"
        Label2.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("MS UI Gothic", 26.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(53, 15)
        Label1.Name = "Label1"
        Label1.Size = New Size(323, 35)
        Label1.TabIndex = 0
        Label1.Text = "RBX Asset Extractor"
        Label1.TextAlign = ContentAlignment.MiddleCenter
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
        TabPage2.Location = New Point(4, 24)
        TabPage2.Name = "TabPage2"
        TabPage2.Padding = New Padding(3)
        TabPage2.Size = New Size(431, 352)
        TabPage2.TabIndex = 1
        TabPage2.Text = "Partial Game Audio"
        ' 
        ' Panel1
        ' 
        Panel1.Location = New Point(149, 358)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(200, 100)
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
        Sounds_Listbox.ItemHeight = 20
        Sounds_Listbox.Location = New Point(6, 6)
        Sounds_Listbox.Name = "Sounds_Listbox"
        Sounds_Listbox.Size = New Size(419, 302)
        Sounds_Listbox.TabIndex = 0
        ' 
        ' TabPage4
        ' 
        TabPage4.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        TabPage4.Controls.Add(LoadHttpBtn)
        TabPage4.Controls.Add(ClearHTTPTEMP_BTN)
        TabPage4.Controls.Add(DownloadAllHTTP_BTN)
        TabPage4.Controls.Add(DOWNLOADHTTP_BTN)
        TabPage4.Controls.Add(HTTPLISTBOX)
        TabPage4.Location = New Point(4, 24)
        TabPage4.Name = "TabPage4"
        TabPage4.Size = New Size(431, 352)
        TabPage4.TabIndex = 3
        TabPage4.Text = "Full Game Audio"
        ' 
        ' LoadHttpBtn
        ' 
        LoadHttpBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        LoadHttpBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        LoadHttpBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        LoadHttpBtn.FlatStyle = FlatStyle.Flat
        LoadHttpBtn.Font = New Font("Segoe UI", 12.0F)
        LoadHttpBtn.ForeColor = Color.White
        LoadHttpBtn.Location = New Point(6, 311)
        LoadHttpBtn.Name = "LoadHttpBtn"
        LoadHttpBtn.Size = New Size(75, 35)
        LoadHttpBtn.TabIndex = 7
        LoadHttpBtn.Text = "Load"
        LoadHttpBtn.UseVisualStyleBackColor = False
        ' 
        ' ClearHTTPTEMP_BTN
        ' 
        ClearHTTPTEMP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ClearHTTPTEMP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        ClearHTTPTEMP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        ClearHTTPTEMP_BTN.FlatStyle = FlatStyle.Flat
        ClearHTTPTEMP_BTN.Font = New Font("Segoe UI", 12.0F)
        ClearHTTPTEMP_BTN.ForeColor = Color.White
        ClearHTTPTEMP_BTN.Location = New Point(318, 311)
        ClearHTTPTEMP_BTN.MaximumSize = New Size(107, 35)
        ClearHTTPTEMP_BTN.MinimumSize = New Size(107, 35)
        ClearHTTPTEMP_BTN.Name = "ClearHTTPTEMP_BTN"
        ClearHTTPTEMP_BTN.Size = New Size(107, 35)
        ClearHTTPTEMP_BTN.TabIndex = 6
        ClearHTTPTEMP_BTN.Text = "Clear Cache"
        ClearHTTPTEMP_BTN.UseVisualStyleBackColor = False
        ' 
        ' DownloadAllHTTP_BTN
        ' 
        DownloadAllHTTP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DownloadAllHTTP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DownloadAllHTTP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DownloadAllHTTP_BTN.FlatStyle = FlatStyle.Flat
        DownloadAllHTTP_BTN.Font = New Font("Segoe UI", 12.0F)
        DownloadAllHTTP_BTN.ForeColor = Color.White
        DownloadAllHTTP_BTN.Location = New Point(198, 311)
        DownloadAllHTTP_BTN.Name = "DownloadAllHTTP_BTN"
        DownloadAllHTTP_BTN.Size = New Size(114, 35)
        DownloadAllHTTP_BTN.TabIndex = 5
        DownloadAllHTTP_BTN.Text = "Download All"
        DownloadAllHTTP_BTN.UseVisualStyleBackColor = False
        ' 
        ' DOWNLOADHTTP_BTN
        ' 
        DOWNLOADHTTP_BTN.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DOWNLOADHTTP_BTN.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DOWNLOADHTTP_BTN.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DOWNLOADHTTP_BTN.FlatStyle = FlatStyle.Flat
        DOWNLOADHTTP_BTN.Font = New Font("Segoe UI", 12.0F)
        DOWNLOADHTTP_BTN.ForeColor = Color.White
        DOWNLOADHTTP_BTN.Location = New Point(87, 311)
        DOWNLOADHTTP_BTN.Name = "DOWNLOADHTTP_BTN"
        DOWNLOADHTTP_BTN.Size = New Size(105, 35)
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
        HTTPLISTBOX.ItemHeight = 20
        HTTPLISTBOX.Location = New Point(6, 6)
        HTTPLISTBOX.Name = "HTTPLISTBOX"
        HTTPLISTBOX.Size = New Size(419, 302)
        HTTPLISTBOX.TabIndex = 0
        ' 
        ' TabPage5
        ' 
        TabPage5.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        TabPage5.Controls.Add(ImgClearTmp)
        TabPage5.Controls.Add(ImgStats)
        TabPage5.Controls.Add(SaveAllBtn)
        TabPage5.Controls.Add(DownloadImgBtn)
        TabPage5.Controls.Add(LoadImgBtn)
        TabPage5.Controls.Add(LoadImgListBox)
        TabPage5.Controls.Add(PreVeiwImgBox)
        TabPage5.Location = New Point(4, 24)
        TabPage5.Name = "TabPage5"
        TabPage5.Size = New Size(431, 352)
        TabPage5.TabIndex = 4
        TabPage5.Text = "Full Game Images"
        ' 
        ' ImgClearTmp
        ' 
        ImgClearTmp.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        ImgClearTmp.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        ImgClearTmp.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        ImgClearTmp.FlatStyle = FlatStyle.Flat
        ImgClearTmp.Font = New Font("Segoe UI", 12.0F)
        ImgClearTmp.ForeColor = Color.White
        ImgClearTmp.Location = New Point(194, 219)
        ImgClearTmp.Name = "ImgClearTmp"
        ImgClearTmp.Size = New Size(234, 36)
        ImgClearTmp.TabIndex = 12
        ImgClearTmp.Text = "Clear Cache"
        ImgClearTmp.UseVisualStyleBackColor = False
        ' 
        ' ImgStats
        ' 
        ImgStats.AutoSize = True
        ImgStats.ForeColor = Color.White
        ImgStats.Location = New Point(194, 184)
        ImgStats.Name = "ImgStats"
        ImgStats.Size = New Size(102, 15)
        ImgStats.TabIndex = 11
        ImgStats.Text = "Image size: 0x0 px"
        ' 
        ' SaveAllBtn
        ' 
        SaveAllBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        SaveAllBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        SaveAllBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        SaveAllBtn.FlatStyle = FlatStyle.Flat
        SaveAllBtn.Font = New Font("Segoe UI", 12.0F)
        SaveAllBtn.ForeColor = Color.White
        SaveAllBtn.Location = New Point(194, 302)
        SaveAllBtn.Name = "SaveAllBtn"
        SaveAllBtn.Size = New Size(234, 35)
        SaveAllBtn.TabIndex = 10
        SaveAllBtn.Text = "SaveAll"
        SaveAllBtn.UseVisualStyleBackColor = False
        ' 
        ' DownloadImgBtn
        ' 
        DownloadImgBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        DownloadImgBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        DownloadImgBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        DownloadImgBtn.FlatStyle = FlatStyle.Flat
        DownloadImgBtn.Font = New Font("Segoe UI", 12.0F)
        DownloadImgBtn.ForeColor = Color.White
        DownloadImgBtn.Location = New Point(275, 261)
        DownloadImgBtn.Name = "DownloadImgBtn"
        DownloadImgBtn.Size = New Size(153, 35)
        DownloadImgBtn.TabIndex = 9
        DownloadImgBtn.Text = "Download"
        DownloadImgBtn.UseVisualStyleBackColor = False
        ' 
        ' LoadImgBtn
        ' 
        LoadImgBtn.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        LoadImgBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        LoadImgBtn.FlatAppearance.BorderColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        LoadImgBtn.FlatStyle = FlatStyle.Flat
        LoadImgBtn.Font = New Font("Segoe UI", 12.0F)
        LoadImgBtn.ForeColor = Color.White
        LoadImgBtn.Location = New Point(194, 261)
        LoadImgBtn.Name = "LoadImgBtn"
        LoadImgBtn.Size = New Size(75, 35)
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
        LoadImgListBox.ItemHeight = 15
        LoadImgListBox.Location = New Point(3, 3)
        LoadImgListBox.Name = "LoadImgListBox"
        LoadImgListBox.Size = New Size(185, 334)
        LoadImgListBox.TabIndex = 1
        ' 
        ' PreVeiwImgBox
        ' 
        PreVeiwImgBox.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        PreVeiwImgBox.Location = New Point(194, 3)
        PreVeiwImgBox.Name = "PreVeiwImgBox"
        PreVeiwImgBox.Size = New Size(234, 178)
        PreVeiwImgBox.SizeMode = PictureBoxSizeMode.StretchImage
        PreVeiwImgBox.TabIndex = 0
        PreVeiwImgBox.TabStop = False
        ' 
        ' TabPage1
        ' 
        TabPage1.BackColor = Color.FromArgb(CByte(20), CByte(20), CByte(20))
        TabPage1.BackgroundImageLayout = ImageLayout.Center
        TabPage1.Controls.Add(SaveLogBtn)
        TabPage1.Controls.Add(AutoScrollCHK)
        TabPage1.Controls.Add(output_log)
        TabPage1.Location = New Point(4, 24)
        TabPage1.Name = "TabPage1"
        TabPage1.Padding = New Padding(3)
        TabPage1.Size = New Size(431, 352)
        TabPage1.TabIndex = 0
        TabPage1.Text = "Log"
        ' 
        ' SaveLogBtn
        ' 
        SaveLogBtn.BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        SaveLogBtn.FlatAppearance.BorderColor = Color.Silver
        SaveLogBtn.FlatAppearance.BorderSize = 2
        SaveLogBtn.FlatStyle = FlatStyle.Popup
        SaveLogBtn.Font = New Font("Segoe UI", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        SaveLogBtn.ForeColor = Color.White
        SaveLogBtn.Location = New Point(350, 322)
        SaveLogBtn.Name = "SaveLogBtn"
        SaveLogBtn.Size = New Size(75, 24)
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
        AutoScrollCHK.Location = New Point(6, 323)
        AutoScrollCHK.Name = "AutoScrollCHK"
        AutoScrollCHK.Size = New Size(88, 21)
        AutoScrollCHK.TabIndex = 2
        AutoScrollCHK.Text = "auto scroll"
        AutoScrollCHK.UseVisualStyleBackColor = True
        ' 
        ' VText_LBR
        ' 
        VText_LBR.BackColor = Color.Transparent
        VText_LBR.Font = New Font("MS UI Gothic", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        VText_LBR.ForeColor = Color.White
        VText_LBR.Location = New Point(210, 432)
        VText_LBR.Name = "VText_LBR"
        VText_LBR.Size = New Size(241, 18)
        VText_LBR.TabIndex = 5
        VText_LBR.Text = "Currently running version:" & vbCrLf
        ' 
        ' ProgressBar1
        ' 
        ProgressBar1.Location = New Point(51, 509)
        ProgressBar1.Name = "ProgressBar1"
        ProgressBar1.Size = New Size(400, 17)
        ProgressBar1.TabIndex = 3
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
        StatusLBR.Anchor = AnchorStyles.Right
        StatusLBR.AutoSize = True
        StatusLBR.BackColor = Color.Transparent
        StatusLBR.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        StatusLBR.ForeColor = Color.White
        StatusLBR.Location = New Point(12, 505)
        StatusLBR.Name = "StatusLBR"
        StatusLBR.Size = New Size(33, 21)
        StatusLBR.TabIndex = 10
        StatusLBR.Text = "0%"
        StatusLBR.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Panel2
        ' 
        Panel2.BackColor = Color.FromArgb(CByte(20), CByte(23), CByte(29))
        Panel2.Controls.Add(PictureBox1)
        Panel2.Controls.Add(Label6)
        Panel2.Controls.Add(Button2)
        Panel2.Controls.Add(CloseBTN)
        Panel2.Location = New Point(-3, -1)
        Panel2.Name = "Panel2"
        Panel2.Size = New Size(476, 40)
        Panel2.TabIndex = 11
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Image = My.Resources.Resources.RobloxRippper
        PictureBox1.Location = New Point(3, 3)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(33, 31)
        PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
        PictureBox1.TabIndex = 3
        PictureBox1.TabStop = False
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label6.ForeColor = Color.White
        Label6.Location = New Point(42, 9)
        Label6.Name = "Label6"
        Label6.Size = New Size(234, 17)
        Label6.TabIndex = 2
        Label6.Text = "RBX Asset Extractor (made by zv800)"
        Label6.TextAlign = ContentAlignment.TopCenter
        ' 
        ' Button2
        ' 
        Button2.BackgroundImageLayout = ImageLayout.Center
        Button2.FlatAppearance.BorderSize = 0
        Button2.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(37), CByte(45), CByte(86))
        Button2.FlatStyle = FlatStyle.Flat
        Button2.Font = New Font("Franklin Gothic Medium", 18.0F, FontStyle.Bold)
        Button2.ForeColor = Color.White
        Button2.Location = New Point(391, 0)
        Button2.Name = "Button2"
        Button2.Size = New Size(37, 40)
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
        CloseBTN.Font = New Font("Franklin Gothic Medium", 18.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        CloseBTN.ForeColor = Color.White
        CloseBTN.Location = New Point(429, 0)
        CloseBTN.Name = "CloseBTN"
        CloseBTN.Size = New Size(37, 40)
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
        AlwaysOnTopCHK.Location = New Point(12, 432)
        AlwaysOnTopCHK.Name = "AlwaysOnTopCHK"
        AlwaysOnTopCHK.Size = New Size(104, 19)
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
        ' MainForm
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(27), CByte(30), CByte(36))
        BackgroundImageLayout = ImageLayout.Stretch
        ClientSize = New Size(463, 535)
        Controls.Add(AlwaysOnTopCHK)
        Controls.Add(Panel2)
        Controls.Add(StatusLBR)
        Controls.Add(AxWindowsMediaPlayer1)
        Controls.Add(VText_LBR)
        Controls.Add(TabControl1)
        Controls.Add(ProgressBar1)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MaximumSize = New Size(472, 550)
        Name = "MainForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "RBX Asset Extractor (made by ZV800)"
        CType(AxWindowsMediaPlayer1, ComponentModel.ISupportInitialize).EndInit()
        TabControl1.ResumeLayout(False)
        TabPage3.ResumeLayout(False)
        TabPage3.PerformLayout()
        TabPage2.ResumeLayout(False)
        TabPage4.ResumeLayout(False)
        TabPage5.ResumeLayout(False)
        TabPage5.PerformLayout()
        CType(PreVeiwImgBox, ComponentModel.ISupportInitialize).EndInit()
        TabPage1.ResumeLayout(False)
        TabPage1.PerformLayout()
        Panel2.ResumeLayout(False)
        Panel2.PerformLayout()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents output_log As ListBox
    Friend WithEvents SaveFileDialog1 As SaveFileDialog
    Friend WithEvents LoadParButton As Button
    Friend WithEvents Download_BTN As Button
    Friend WithEvents ClearTMP_BTN As Button
    Friend WithEvents AxWindowsMediaPlayer1 As AxWMPLib.AxWindowsMediaPlayer
    Friend WithEvents DownloadALL_BTN As Button
    Friend WithEvents FolderBrowserDialog1 As FolderBrowserDialog
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents Sounds_Listbox As ListBox
    Friend WithEvents TabPage3 As TabPage
    Friend WithEvents CheckFOrButtons As Timer
    Friend WithEvents Label1 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents VText_LBR As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents LinkLabel1 As LinkLabel
    Friend WithEvents ClearTmp As System.ComponentModel.BackgroundWorker
    Friend WithEvents TabPage4 As TabPage
    Friend WithEvents HTTPLISTBOX As ListBox
    Friend WithEvents DownloadAllHTTP_BTN As Button
    Friend WithEvents DOWNLOADHTTP_BTN As Button
    Friend WithEvents ProgressBar1 As ProgressBar
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
    Friend WithEvents Label4 As Label
    Friend WithEvents TabPage5 As TabPage
    Friend WithEvents LoadImgListBox As ListBox
    Friend WithEvents PreVeiwImgBox As PictureBox
    Friend WithEvents SaveAllBtn As Button
    Friend WithEvents DownloadImgBtn As Button
    Friend WithEvents LoadImgBtn As Button
    Friend WithEvents Timer3 As Timer
    Friend WithEvents AlwaysOnTopCHK As CheckBox
    Friend WithEvents Label8 As Label
    Friend WithEvents ImgStats As Label
    Friend WithEvents ImgClearTmp As Button
    Friend WithEvents KeepButtonsOff As Timer
    Friend WithEvents Label7 As Label
    Friend WithEvents MainLoop As Timer
    Friend WithEvents LoadPartialBackgoundWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents ClearCache As System.ComponentModel.BackgroundWorker

End Class

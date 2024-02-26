namespace COG.UI.Forms
{
    partial class SetUpForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetUpForm));
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.CB_BMP = new System.Windows.Forms.CheckBox();
            this.CB_OPTION_01 = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DGV_IMAGE_SAVE = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.DGV_SAVEOPTION_DATA = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn8 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.label32 = new System.Windows.Forms.Label();
            this.TB_LOG_CHECK_SPACE = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.TB_LOG_CHECK_PERIOD = new System.Windows.Forms.TextBox();
            this.BTN_SAVE = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.RBTN_LAN02 = new System.Windows.Forms.RadioButton();
            this.RBTN_LAN01 = new System.Windows.Forms.RadioButton();
            this.RBTN_LAN00 = new System.Windows.Forms.RadioButton();
            this.LB_LANGUAGESHOW = new System.Windows.Forms.Label();
            this.LB_LANGUAGE = new System.Windows.Forms.Label();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.PN_USE_RETRY = new System.Windows.Forms.Panel();
            this.CKD_USE_RETRY = new System.Windows.Forms.CheckBox();
            this.label47 = new System.Windows.Forms.Label();
            this.LB_RETRY_COUNT = new System.Windows.Forms.Label();
            this.PN_SETUP_1ST_ALIGN_NG = new System.Windows.Forms.Panel();
            this.label42 = new System.Windows.Forms.Label();
            this.CB_USE_1ST_ALIGN_ANGLE_NG = new System.Windows.Forms.CheckBox();
            this.label40 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.LB_SETUP_1ST_ALIGN_CORNER_NG = new System.Windows.Forms.Label();
            this.CB_USE_INSP_LIMIT = new System.Windows.Forms.CheckBox();
            this.CB_USE_LOADING_LIMIT = new System.Windows.Forms.CheckBox();
            this.label36 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.LB_SETUP_INSP_HIGHER_LIMIT = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.LB_SETUP_INSP_LOWER_LIMIT = new System.Windows.Forms.Label();
            this.label41 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.LB_SETUP_LOADING_Y_LIMIT = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.LB_SETUP_LOADING_X_LIMIT = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.groupBox5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_IMAGE_SAVE)).BeginInit();
            this.groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_SAVEOPTION_DATA)).BeginInit();
            this.groupBox14.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.PN_USE_RETRY.SuspendLayout();
            this.PN_SETUP_1ST_ALIGN_NG.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.CB_BMP);
            this.groupBox5.Controls.Add(this.CB_OPTION_01);
            this.groupBox5.Controls.Add(this.label16);
            this.groupBox5.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox5.ForeColor = System.Drawing.Color.White;
            this.groupBox5.Location = new System.Drawing.Point(2, 583);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(259, 243);
            this.groupBox5.TabIndex = 29;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "IMAGE SAVE Format";
            // 
            // CB_BMP
            // 
            this.CB_BMP.BackColor = System.Drawing.Color.White;
            this.CB_BMP.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BMP.ForeColor = System.Drawing.Color.Black;
            this.CB_BMP.Location = new System.Drawing.Point(141, 107);
            this.CB_BMP.Name = "CB_BMP";
            this.CB_BMP.Size = new System.Drawing.Size(63, 56);
            this.CB_BMP.TabIndex = 32;
            this.CB_BMP.Text = "BMP";
            this.CB_BMP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CB_BMP.UseVisualStyleBackColor = false;
            this.CB_BMP.Click += new System.EventHandler(this.BTN_OPTION_Click);
            // 
            // CB_OPTION_01
            // 
            this.CB_OPTION_01.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_OPTION_01.BackColor = System.Drawing.Color.White;
            this.CB_OPTION_01.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_OPTION_01.ForeColor = System.Drawing.Color.Black;
            this.CB_OPTION_01.Location = new System.Drawing.Point(21, 28);
            this.CB_OPTION_01.Name = "CB_OPTION_01";
            this.CB_OPTION_01.Size = new System.Drawing.Size(183, 56);
            this.CB_OPTION_01.TabIndex = 31;
            this.CB_OPTION_01.Text = "OVERLAY IMG SAVE";
            this.CB_OPTION_01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CB_OPTION_01.UseVisualStyleBackColor = false;
            this.CB_OPTION_01.Click += new System.EventHandler(this.BTN_OPTION_Click);
            // 
            // label16
            // 
            this.label16.BackColor = System.Drawing.Color.DarkGray;
            this.label16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label16.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label16.Location = new System.Drawing.Point(21, 107);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(114, 56);
            this.label16.TabIndex = 17;
            this.label16.Text = "Image Format (Default JPG)";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DGV_IMAGE_SAVE);
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(1505, 11);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(167, 530);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "IMAGE SAVE USE";
            // 
            // DGV_IMAGE_SAVE
            // 
            this.DGV_IMAGE_SAVE.AllowUserToAddRows = false;
            this.DGV_IMAGE_SAVE.AllowUserToDeleteRows = false;
            this.DGV_IMAGE_SAVE.AllowUserToResizeColumns = false;
            this.DGV_IMAGE_SAVE.AllowUserToResizeRows = false;
            this.DGV_IMAGE_SAVE.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_IMAGE_SAVE.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1,
            this.dataGridViewCheckBoxColumn2});
            this.DGV_IMAGE_SAVE.Location = new System.Drawing.Point(1, 24);
            this.DGV_IMAGE_SAVE.Name = "DGV_IMAGE_SAVE";
            this.DGV_IMAGE_SAVE.RowHeadersVisible = false;
            this.DGV_IMAGE_SAVE.RowTemplate.Height = 23;
            this.DGV_IMAGE_SAVE.Size = new System.Drawing.Size(164, 500);
            this.DGV_IMAGE_SAVE.TabIndex = 26;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewCheckBoxColumn1.HeaderText = "GD_IMAGE";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn1.Width = 79;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewCheckBoxColumn2.HeaderText = "NG_IMAGE";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            this.dataGridViewCheckBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn2.Width = 80;
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.DGV_SAVEOPTION_DATA);
            this.groupBox11.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox11.ForeColor = System.Drawing.Color.White;
            this.groupBox11.Location = new System.Drawing.Point(459, 547);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(320, 380);
            this.groupBox11.TabIndex = 41;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "SAVE DATA";
            // 
            // DGV_SAVEOPTION_DATA
            // 
            this.DGV_SAVEOPTION_DATA.AllowUserToAddRows = false;
            this.DGV_SAVEOPTION_DATA.AllowUserToDeleteRows = false;
            this.DGV_SAVEOPTION_DATA.AllowUserToResizeColumns = false;
            this.DGV_SAVEOPTION_DATA.AllowUserToResizeRows = false;
            this.DGV_SAVEOPTION_DATA.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_SAVEOPTION_DATA.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewCheckBoxColumn8});
            this.DGV_SAVEOPTION_DATA.Location = new System.Drawing.Point(6, 24);
            this.DGV_SAVEOPTION_DATA.Name = "DGV_SAVEOPTION_DATA";
            this.DGV_SAVEOPTION_DATA.RowHeadersVisible = false;
            this.DGV_SAVEOPTION_DATA.RowTemplate.Height = 23;
            this.DGV_SAVEOPTION_DATA.Size = new System.Drawing.Size(309, 350);
            this.DGV_SAVEOPTION_DATA.TabIndex = 27;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.NullValue = "0";
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTextBoxColumn1.HeaderText = "OPTION";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Width = 200;
            // 
            // dataGridViewCheckBoxColumn8
            // 
            this.dataGridViewCheckBoxColumn8.HeaderText = "USE CHECK";
            this.dataGridViewCheckBoxColumn8.Name = "dataGridViewCheckBoxColumn8";
            this.dataGridViewCheckBoxColumn8.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.label32);
            this.groupBox14.Controls.Add(this.TB_LOG_CHECK_SPACE);
            this.groupBox14.Controls.Add(this.label30);
            this.groupBox14.Controls.Add(this.label28);
            this.groupBox14.Controls.Add(this.label27);
            this.groupBox14.Controls.Add(this.TB_LOG_CHECK_PERIOD);
            this.groupBox14.ForeColor = System.Drawing.Color.White;
            this.groupBox14.Location = new System.Drawing.Point(780, 648);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(277, 98);
            this.groupBox14.TabIndex = 28;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "OLD LOG DATA CHECK";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("굴림", 10F);
            this.label32.ForeColor = System.Drawing.Color.Black;
            this.label32.Location = new System.Drawing.Point(200, 60);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(68, 14);
            this.label32.TabIndex = 245;
            this.label32.Text = "(GBytes)";
            // 
            // TB_LOG_CHECK_SPACE
            // 
            this.TB_LOG_CHECK_SPACE.Location = new System.Drawing.Point(106, 54);
            this.TB_LOG_CHECK_SPACE.Name = "TB_LOG_CHECK_SPACE";
            this.TB_LOG_CHECK_SPACE.Size = new System.Drawing.Size(90, 21);
            this.TB_LOG_CHECK_SPACE.TabIndex = 244;
            this.TB_LOG_CHECK_SPACE.Click += new System.EventHandler(this.TB_LOG_CHECK_SPACE_Click);
            // 
            // label30
            // 
            this.label30.BackColor = System.Drawing.Color.DarkGray;
            this.label30.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label30.Location = new System.Drawing.Point(6, 46);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(97, 30);
            this.label30.TabIndex = 243;
            this.label30.Text = "MAX SPACE";
            this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label28
            // 
            this.label28.BackColor = System.Drawing.Color.DarkGray;
            this.label28.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label28.Location = new System.Drawing.Point(21, 20);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(68, 30);
            this.label28.TabIndex = 242;
            this.label28.Text = "PERIOD";
            this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("굴림", 10F);
            this.label27.ForeColor = System.Drawing.Color.Black;
            this.label27.Location = new System.Drawing.Point(200, 30);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(53, 14);
            this.label27.TabIndex = 237;
            this.label27.Text = "(Days)";
            // 
            // TB_LOG_CHECK_PERIOD
            // 
            this.TB_LOG_CHECK_PERIOD.Location = new System.Drawing.Point(106, 26);
            this.TB_LOG_CHECK_PERIOD.Name = "TB_LOG_CHECK_PERIOD";
            this.TB_LOG_CHECK_PERIOD.Size = new System.Drawing.Size(90, 21);
            this.TB_LOG_CHECK_PERIOD.TabIndex = 236;
            this.TB_LOG_CHECK_PERIOD.Click += new System.EventHandler(this.TB_LOG_CHECK_PERIOD_Click);
            // 
            // BTN_SAVE
            // 
            this.BTN_SAVE.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_SAVE.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_SAVE.BackgroundImage")));
            this.BTN_SAVE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_SAVE.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SAVE.Location = new System.Drawing.Point(1686, 901);
            this.BTN_SAVE.Name = "BTN_SAVE";
            this.BTN_SAVE.Size = new System.Drawing.Size(100, 100);
            this.BTN_SAVE.TabIndex = 15;
            this.BTN_SAVE.Text = "SAVE";
            this.BTN_SAVE.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_SAVE.UseVisualStyleBackColor = false;
            this.BTN_SAVE.Click += new System.EventHandler(this.BTN_SAVE_Click);
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(1792, 901);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 1;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.RBTN_LAN02);
            this.groupBox12.Controls.Add(this.RBTN_LAN01);
            this.groupBox12.Controls.Add(this.RBTN_LAN00);
            this.groupBox12.Location = new System.Drawing.Point(1653, 997);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(239, 38);
            this.groupBox12.TabIndex = 222;
            this.groupBox12.TabStop = false;
            // 
            // RBTN_LAN02
            // 
            this.RBTN_LAN02.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_LAN02.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RBTN_LAN02.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_LAN02.Location = new System.Drawing.Point(159, 9);
            this.RBTN_LAN02.Name = "RBTN_LAN02";
            this.RBTN_LAN02.Size = new System.Drawing.Size(78, 27);
            this.RBTN_LAN02.TabIndex = 131;
            this.RBTN_LAN02.Text = "ENGLISH";
            this.RBTN_LAN02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_LAN02.UseVisualStyleBackColor = true;
            this.RBTN_LAN02.CheckedChanged += new System.EventHandler(this.RBTN_Button_Color_CheckedChanged);
            this.RBTN_LAN02.Click += new System.EventHandler(this.RBTN_LAN_Click);
            // 
            // RBTN_LAN01
            // 
            this.RBTN_LAN01.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_LAN01.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RBTN_LAN01.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_LAN01.Location = new System.Drawing.Point(79, 9);
            this.RBTN_LAN01.Name = "RBTN_LAN01";
            this.RBTN_LAN01.Size = new System.Drawing.Size(81, 27);
            this.RBTN_LAN01.TabIndex = 130;
            this.RBTN_LAN01.Text = "CHINESE";
            this.RBTN_LAN01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_LAN01.UseVisualStyleBackColor = true;
            this.RBTN_LAN01.CheckedChanged += new System.EventHandler(this.RBTN_Button_Color_CheckedChanged);
            this.RBTN_LAN01.Click += new System.EventHandler(this.RBTN_LAN_Click);
            // 
            // RBTN_LAN00
            // 
            this.RBTN_LAN00.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_LAN00.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RBTN_LAN00.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_LAN00.Location = new System.Drawing.Point(2, 9);
            this.RBTN_LAN00.Name = "RBTN_LAN00";
            this.RBTN_LAN00.Size = new System.Drawing.Size(78, 27);
            this.RBTN_LAN00.TabIndex = 129;
            this.RBTN_LAN00.TabStop = true;
            this.RBTN_LAN00.Text = "KOREAN";
            this.RBTN_LAN00.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_LAN00.UseVisualStyleBackColor = true;
            this.RBTN_LAN00.CheckedChanged += new System.EventHandler(this.RBTN_Button_Color_CheckedChanged);
            this.RBTN_LAN00.Click += new System.EventHandler(this.RBTN_LAN_Click);
            // 
            // LB_LANGUAGESHOW
            // 
            this.LB_LANGUAGESHOW.BackColor = System.Drawing.Color.DarkGray;
            this.LB_LANGUAGESHOW.Location = new System.Drawing.Point(1546, 914);
            this.LB_LANGUAGESHOW.Name = "LB_LANGUAGESHOW";
            this.LB_LANGUAGESHOW.Size = new System.Drawing.Size(103, 131);
            this.LB_LANGUAGESHOW.TabIndex = 223;
            this.LB_LANGUAGESHOW.Click += new System.EventHandler(this.LB_LANGUAGESHOW_Click);
            // 
            // LB_LANGUAGE
            // 
            this.LB_LANGUAGE.BackColor = System.Drawing.Color.DarkGray;
            this.LB_LANGUAGE.Location = new System.Drawing.Point(1646, 1002);
            this.LB_LANGUAGE.Name = "LB_LANGUAGE";
            this.LB_LANGUAGE.Size = new System.Drawing.Size(249, 38);
            this.LB_LANGUAGE.TabIndex = 224;
            // 
            // groupBox15
            // 
            this.groupBox15.Controls.Add(this.PN_USE_RETRY);
            this.groupBox15.Controls.Add(this.PN_SETUP_1ST_ALIGN_NG);
            this.groupBox15.Controls.Add(this.CB_USE_INSP_LIMIT);
            this.groupBox15.Controls.Add(this.CB_USE_LOADING_LIMIT);
            this.groupBox15.Controls.Add(this.label36);
            this.groupBox15.Controls.Add(this.label37);
            this.groupBox15.Controls.Add(this.LB_SETUP_INSP_HIGHER_LIMIT);
            this.groupBox15.Controls.Add(this.label39);
            this.groupBox15.Controls.Add(this.LB_SETUP_INSP_LOWER_LIMIT);
            this.groupBox15.Controls.Add(this.label41);
            this.groupBox15.Controls.Add(this.label34);
            this.groupBox15.Controls.Add(this.label33);
            this.groupBox15.Controls.Add(this.LB_SETUP_LOADING_Y_LIMIT);
            this.groupBox15.Controls.Add(this.label31);
            this.groupBox15.Controls.Add(this.LB_SETUP_LOADING_X_LIMIT);
            this.groupBox15.Controls.Add(this.label29);
            this.groupBox15.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox15.ForeColor = System.Drawing.Color.White;
            this.groupBox15.Location = new System.Drawing.Point(780, 547);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(277, 89);
            this.groupBox15.TabIndex = 228;
            this.groupBox15.TabStop = false;
            this.groupBox15.Text = "Retry Setting";
            // 
            // PN_USE_RETRY
            // 
            this.PN_USE_RETRY.Controls.Add(this.CKD_USE_RETRY);
            this.PN_USE_RETRY.Controls.Add(this.label47);
            this.PN_USE_RETRY.Controls.Add(this.LB_RETRY_COUNT);
            this.PN_USE_RETRY.Location = new System.Drawing.Point(6, 21);
            this.PN_USE_RETRY.Name = "PN_USE_RETRY";
            this.PN_USE_RETRY.Size = new System.Drawing.Size(213, 65);
            this.PN_USE_RETRY.TabIndex = 256;
            // 
            // CKD_USE_RETRY
            // 
            this.CKD_USE_RETRY.AutoSize = true;
            this.CKD_USE_RETRY.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CKD_USE_RETRY.ForeColor = System.Drawing.Color.Black;
            this.CKD_USE_RETRY.Location = new System.Drawing.Point(11, 7);
            this.CKD_USE_RETRY.Name = "CKD_USE_RETRY";
            this.CKD_USE_RETRY.Size = new System.Drawing.Size(102, 24);
            this.CKD_USE_RETRY.TabIndex = 249;
            this.CKD_USE_RETRY.Text = "USE RETRY";
            this.CKD_USE_RETRY.UseVisualStyleBackColor = true;
            this.CKD_USE_RETRY.CheckedChanged += new System.EventHandler(this.CKD_USE_RETRY_CheckedChanged);
            // 
            // label47
            // 
            this.label47.BackColor = System.Drawing.Color.DarkGray;
            this.label47.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label47.Location = new System.Drawing.Point(17, 28);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(80, 30);
            this.label47.TabIndex = 250;
            this.label47.Text = "Retry Count";
            this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_RETRY_COUNT
            // 
            this.LB_RETRY_COUNT.BackColor = System.Drawing.Color.White;
            this.LB_RETRY_COUNT.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_RETRY_COUNT.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_RETRY_COUNT.Location = new System.Drawing.Point(101, 34);
            this.LB_RETRY_COUNT.Name = "LB_RETRY_COUNT";
            this.LB_RETRY_COUNT.Size = new System.Drawing.Size(99, 20);
            this.LB_RETRY_COUNT.TabIndex = 252;
            this.LB_RETRY_COUNT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_RETRY_COUNT.Visible = false;
            this.LB_RETRY_COUNT.Click += new System.EventHandler(this.LB_RETRY_COUNT_Click);
            // 
            // PN_SETUP_1ST_ALIGN_NG
            // 
            this.PN_SETUP_1ST_ALIGN_NG.Controls.Add(this.label42);
            this.PN_SETUP_1ST_ALIGN_NG.Controls.Add(this.CB_USE_1ST_ALIGN_ANGLE_NG);
            this.PN_SETUP_1ST_ALIGN_NG.Controls.Add(this.label40);
            this.PN_SETUP_1ST_ALIGN_NG.Controls.Add(this.label35);
            this.PN_SETUP_1ST_ALIGN_NG.Controls.Add(this.LB_SETUP_1ST_ALIGN_VERTICAL_NG);
            this.PN_SETUP_1ST_ALIGN_NG.Controls.Add(this.label38);
            this.PN_SETUP_1ST_ALIGN_NG.Controls.Add(this.LB_SETUP_1ST_ALIGN_CORNER_NG);
            this.PN_SETUP_1ST_ALIGN_NG.Location = new System.Drawing.Point(17, 246);
            this.PN_SETUP_1ST_ALIGN_NG.Name = "PN_SETUP_1ST_ALIGN_NG";
            this.PN_SETUP_1ST_ALIGN_NG.Size = new System.Drawing.Size(250, 139);
            this.PN_SETUP_1ST_ALIGN_NG.TabIndex = 28;
            this.PN_SETUP_1ST_ALIGN_NG.Visible = false;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label42.ForeColor = System.Drawing.Color.White;
            this.label42.Location = new System.Drawing.Point(201, 105);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(39, 21);
            this.label42.TabIndex = 255;
            this.label42.Text = "( º )";
            // 
            // CB_USE_1ST_ALIGN_ANGLE_NG
            // 
            this.CB_USE_1ST_ALIGN_ANGLE_NG.AutoSize = true;
            this.CB_USE_1ST_ALIGN_ANGLE_NG.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_USE_1ST_ALIGN_ANGLE_NG.ForeColor = System.Drawing.Color.Black;
            this.CB_USE_1ST_ALIGN_ANGLE_NG.Location = new System.Drawing.Point(10, 10);
            this.CB_USE_1ST_ALIGN_ANGLE_NG.Name = "CB_USE_1ST_ALIGN_ANGLE_NG";
            this.CB_USE_1ST_ALIGN_ANGLE_NG.Size = new System.Drawing.Size(226, 24);
            this.CB_USE_1ST_ALIGN_ANGLE_NG.TabIndex = 249;
            this.CB_USE_1ST_ALIGN_ANGLE_NG.Text = "USE 1ST ALIGN ANGLE LIMIT";
            this.CB_USE_1ST_ALIGN_ANGLE_NG.UseVisualStyleBackColor = true;
            this.CB_USE_1ST_ALIGN_ANGLE_NG.Visible = false;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label40.ForeColor = System.Drawing.Color.White;
            this.label40.Location = new System.Drawing.Point(201, 55);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(39, 21);
            this.label40.TabIndex = 254;
            this.label40.Text = "( º )";
            // 
            // label35
            // 
            this.label35.BackColor = System.Drawing.Color.DarkGray;
            this.label35.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label35.Location = new System.Drawing.Point(10, 45);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(100, 30);
            this.label35.TabIndex = 250;
            this.label35.Text = "CORNER  ±";
            this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_SETUP_1ST_ALIGN_VERTICAL_NG
            // 
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.BackColor = System.Drawing.Color.White;
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.Location = new System.Drawing.Point(120, 90);
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.Name = "LB_SETUP_1ST_ALIGN_VERTICAL_NG";
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.Size = new System.Drawing.Size(75, 40);
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.TabIndex = 253;
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_SETUP_1ST_ALIGN_VERTICAL_NG.Click += new System.EventHandler(this.LB_SETUP_1ST_ALIGN_VERTICAL_NG_Click);
            // 
            // label38
            // 
            this.label38.BackColor = System.Drawing.Color.DarkGray;
            this.label38.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label38.Location = new System.Drawing.Point(10, 95);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(100, 30);
            this.label38.TabIndex = 251;
            this.label38.Text = "VERTICAL ±";
            this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_SETUP_1ST_ALIGN_CORNER_NG
            // 
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.BackColor = System.Drawing.Color.White;
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.Location = new System.Drawing.Point(120, 40);
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.Name = "LB_SETUP_1ST_ALIGN_CORNER_NG";
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.Size = new System.Drawing.Size(75, 40);
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.TabIndex = 252;
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_SETUP_1ST_ALIGN_CORNER_NG.Click += new System.EventHandler(this.LB_SETUP_1ST_ALIGN_CORNER_NG_Click);
            // 
            // CB_USE_INSP_LIMIT
            // 
            this.CB_USE_INSP_LIMIT.AutoSize = true;
            this.CB_USE_INSP_LIMIT.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_USE_INSP_LIMIT.ForeColor = System.Drawing.Color.Black;
            this.CB_USE_INSP_LIMIT.Location = new System.Drawing.Point(16, 151);
            this.CB_USE_INSP_LIMIT.Name = "CB_USE_INSP_LIMIT";
            this.CB_USE_INSP_LIMIT.Size = new System.Drawing.Size(187, 24);
            this.CB_USE_INSP_LIMIT.TabIndex = 248;
            this.CB_USE_INSP_LIMIT.Text = "USE INSPECTION LIMIT";
            this.CB_USE_INSP_LIMIT.UseVisualStyleBackColor = true;
            this.CB_USE_INSP_LIMIT.Visible = false;
            // 
            // CB_USE_LOADING_LIMIT
            // 
            this.CB_USE_LOADING_LIMIT.AutoSize = true;
            this.CB_USE_LOADING_LIMIT.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_USE_LOADING_LIMIT.ForeColor = System.Drawing.Color.Black;
            this.CB_USE_LOADING_LIMIT.Location = new System.Drawing.Point(16, 21);
            this.CB_USE_LOADING_LIMIT.Name = "CB_USE_LOADING_LIMIT";
            this.CB_USE_LOADING_LIMIT.Size = new System.Drawing.Size(168, 24);
            this.CB_USE_LOADING_LIMIT.TabIndex = 247;
            this.CB_USE_LOADING_LIMIT.Text = "USE LOADING LIMIT";
            this.CB_USE_LOADING_LIMIT.UseVisualStyleBackColor = true;
            this.CB_USE_LOADING_LIMIT.Visible = false;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label36.ForeColor = System.Drawing.Color.White;
            this.label36.Location = new System.Drawing.Point(21, 222);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(43, 21);
            this.label36.TabIndex = 246;
            this.label36.Text = "(um)";
            this.label36.Visible = false;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label37.ForeColor = System.Drawing.Color.White;
            this.label37.Location = new System.Drawing.Point(392, 224);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(43, 21);
            this.label37.TabIndex = 245;
            this.label37.Text = "(um)";
            this.label37.Visible = false;
            // 
            // LB_SETUP_INSP_HIGHER_LIMIT
            // 
            this.LB_SETUP_INSP_HIGHER_LIMIT.BackColor = System.Drawing.Color.White;
            this.LB_SETUP_INSP_HIGHER_LIMIT.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_SETUP_INSP_HIGHER_LIMIT.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_SETUP_INSP_HIGHER_LIMIT.Location = new System.Drawing.Point(101, 231);
            this.LB_SETUP_INSP_HIGHER_LIMIT.Name = "LB_SETUP_INSP_HIGHER_LIMIT";
            this.LB_SETUP_INSP_HIGHER_LIMIT.Size = new System.Drawing.Size(100, 40);
            this.LB_SETUP_INSP_HIGHER_LIMIT.TabIndex = 244;
            this.LB_SETUP_INSP_HIGHER_LIMIT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_SETUP_INSP_HIGHER_LIMIT.Visible = false;
            // 
            // label39
            // 
            this.label39.BackColor = System.Drawing.Color.DarkGray;
            this.label39.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label39.Location = new System.Drawing.Point(15, 240);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(70, 30);
            this.label39.TabIndex = 243;
            this.label39.Text = "HIGHER";
            this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label39.Visible = false;
            // 
            // LB_SETUP_INSP_LOWER_LIMIT
            // 
            this.LB_SETUP_INSP_LOWER_LIMIT.BackColor = System.Drawing.Color.White;
            this.LB_SETUP_INSP_LOWER_LIMIT.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_SETUP_INSP_LOWER_LIMIT.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_SETUP_INSP_LOWER_LIMIT.Location = new System.Drawing.Point(101, 181);
            this.LB_SETUP_INSP_LOWER_LIMIT.Name = "LB_SETUP_INSP_LOWER_LIMIT";
            this.LB_SETUP_INSP_LOWER_LIMIT.Size = new System.Drawing.Size(100, 40);
            this.LB_SETUP_INSP_LOWER_LIMIT.TabIndex = 242;
            this.LB_SETUP_INSP_LOWER_LIMIT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_SETUP_INSP_LOWER_LIMIT.Visible = false;
            // 
            // label41
            // 
            this.label41.BackColor = System.Drawing.Color.DarkGray;
            this.label41.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label41.Location = new System.Drawing.Point(16, 186);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(70, 30);
            this.label41.TabIndex = 241;
            this.label41.Text = "LOWER";
            this.label41.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label41.Visible = false;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label34.ForeColor = System.Drawing.Color.White;
            this.label34.Location = new System.Drawing.Point(392, 144);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(43, 21);
            this.label34.TabIndex = 239;
            this.label34.Text = "(um)";
            this.label34.Visible = false;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label33.ForeColor = System.Drawing.Color.White;
            this.label33.Location = new System.Drawing.Point(21, 42);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(43, 21);
            this.label33.TabIndex = 238;
            this.label33.Text = "(um)";
            // 
            // LB_SETUP_LOADING_Y_LIMIT
            // 
            this.LB_SETUP_LOADING_Y_LIMIT.BackColor = System.Drawing.Color.White;
            this.LB_SETUP_LOADING_Y_LIMIT.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_SETUP_LOADING_Y_LIMIT.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_SETUP_LOADING_Y_LIMIT.Location = new System.Drawing.Point(101, 101);
            this.LB_SETUP_LOADING_Y_LIMIT.Name = "LB_SETUP_LOADING_Y_LIMIT";
            this.LB_SETUP_LOADING_Y_LIMIT.Size = new System.Drawing.Size(100, 40);
            this.LB_SETUP_LOADING_Y_LIMIT.TabIndex = 34;
            this.LB_SETUP_LOADING_Y_LIMIT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_SETUP_LOADING_Y_LIMIT.Visible = false;
            this.LB_SETUP_LOADING_Y_LIMIT.Click += new System.EventHandler(this.LB_SETUP_LOADING_Y_LIMIT_Click);
            // 
            // label31
            // 
            this.label31.BackColor = System.Drawing.Color.DarkGray;
            this.label31.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label31.Location = new System.Drawing.Point(21, 106);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(40, 30);
            this.label31.TabIndex = 33;
            this.label31.Text = "Y ±";
            this.label31.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label31.Visible = false;
            // 
            // LB_SETUP_LOADING_X_LIMIT
            // 
            this.LB_SETUP_LOADING_X_LIMIT.BackColor = System.Drawing.Color.White;
            this.LB_SETUP_LOADING_X_LIMIT.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_SETUP_LOADING_X_LIMIT.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_SETUP_LOADING_X_LIMIT.Location = new System.Drawing.Point(101, 51);
            this.LB_SETUP_LOADING_X_LIMIT.Name = "LB_SETUP_LOADING_X_LIMIT";
            this.LB_SETUP_LOADING_X_LIMIT.Size = new System.Drawing.Size(100, 40);
            this.LB_SETUP_LOADING_X_LIMIT.TabIndex = 32;
            this.LB_SETUP_LOADING_X_LIMIT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_SETUP_LOADING_X_LIMIT.Visible = false;
            this.LB_SETUP_LOADING_X_LIMIT.Click += new System.EventHandler(this.LB_SETUP_LOADING_X_LIMIT_Click);
            // 
            // label29
            // 
            this.label29.BackColor = System.Drawing.Color.DarkGray;
            this.label29.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label29.Location = new System.Drawing.Point(21, 56);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(40, 30);
            this.label29.TabIndex = 31;
            this.label29.Text = "X ±";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label29.Visible = false;
            // 
            // SetUpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1904, 1050);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox14);
            this.Controls.Add(this.groupBox15);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_SAVE);
            this.Controls.Add(this.LB_LANGUAGE);
            this.Controls.Add(this.LB_LANGUAGESHOW);
            this.Controls.Add(this.groupBox12);
            this.Controls.Add(this.groupBox11);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "SetUpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_SetUp";
            this.Load += new System.EventHandler(this.Form_SetUp_Load);
            this.groupBox5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DGV_IMAGE_SAVE)).EndInit();
            this.groupBox11.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DGV_SAVEOPTION_DATA)).EndInit();
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.PN_USE_RETRY.ResumeLayout(false);
            this.PN_USE_RETRY.PerformLayout();
            this.PN_SETUP_1ST_ALIGN_NG.ResumeLayout(false);
            this.PN_SETUP_1ST_ALIGN_NG.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Button BTN_SAVE;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox CB_OPTION_01;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox CB_BMP;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView DGV_IMAGE_SAVE;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.DataGridView DGV_SAVEOPTION_DATA;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn8;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.RadioButton RBTN_LAN02;
        private System.Windows.Forms.RadioButton RBTN_LAN01;
        private System.Windows.Forms.RadioButton RBTN_LAN00;
        private System.Windows.Forms.Label LB_LANGUAGE;
        private System.Windows.Forms.Label LB_LANGUAGESHOW;
        private System.Windows.Forms.GroupBox groupBox14;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox TB_LOG_CHECK_PERIOD;
        private System.Windows.Forms.GroupBox groupBox15;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label LB_SETUP_INSP_HIGHER_LIMIT;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.Label LB_SETUP_INSP_LOWER_LIMIT;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label LB_SETUP_LOADING_Y_LIMIT;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label LB_SETUP_LOADING_X_LIMIT;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.CheckBox CB_USE_LOADING_LIMIT;
        private System.Windows.Forms.CheckBox CB_USE_INSP_LIMIT;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.TextBox TB_LOG_CHECK_SPACE;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label LB_SETUP_1ST_ALIGN_VERTICAL_NG;
        private System.Windows.Forms.Label LB_SETUP_1ST_ALIGN_CORNER_NG;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.CheckBox CB_USE_1ST_ALIGN_ANGLE_NG;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.Panel PN_SETUP_1ST_ALIGN_NG;
        private System.Windows.Forms.Panel PN_USE_RETRY;
        private System.Windows.Forms.CheckBox CKD_USE_RETRY;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label LB_RETRY_COUNT;
    }
}
namespace COG
{
    partial class Form_CalDisplay
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_CalDisplay));
            this.BTN_UNIT_01 = new System.Windows.Forms.Button();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BTN_COPY = new System.Windows.Forms.Button();
            this.GB_TRAY_CAL = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.BTN_COPY_DATA = new System.Windows.Forms.Button();
            this.LB_Resolution = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.LB_DX = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.LB_CENTER_B_LY = new System.Windows.Forms.Label();
            this.LB_CENTER_B_RX = new System.Windows.Forms.Label();
            this.LB_CENTER_B_RY = new System.Windows.Forms.Label();
            this.LB_CENTER_B_LX = new System.Windows.Forms.Label();
            this.GB_MOTCENTER = new System.Windows.Forms.GroupBox();
            this.LB_UNIT_NAME = new System.Windows.Forms.Label();
            this.LB_CENTER_T_RY = new System.Windows.Forms.Label();
            this.LB_CENTER_T_LY = new System.Windows.Forms.Label();
            this.LB_CENTER_T_LX = new System.Windows.Forms.Label();
            this.LB_CENTER_T_RX = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.GB_Reel_Caldata = new System.Windows.Forms.GroupBox();
            this.BTN_CALCULATION = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.LB_RESULT = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.LB_DISTANCE = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.LB_PIXEL_END_01 = new System.Windows.Forms.Label();
            this.LB_PIXEL_START_00 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.BTN_REEL2_CAL_01 = new System.Windows.Forms.Button();
            this.BTN_REEL1_CAL_00 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.GB_TRAY_CAL.SuspendLayout();
            this.GB_MOTCENTER.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.GB_Reel_Caldata.SuspendLayout();
            this.SuspendLayout();
            // 
            // BTN_UNIT_01
            // 
            this.BTN_UNIT_01.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_UNIT_01.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_UNIT_01.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_UNIT_01.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BTN_UNIT_01.Location = new System.Drawing.Point(1690, 962);
            this.BTN_UNIT_01.Name = "BTN_UNIT_01";
            this.BTN_UNIT_01.Size = new System.Drawing.Size(88, 73);
            this.BTN_UNIT_01.TabIndex = 26;
            this.BTN_UNIT_01.Text = "Matrix";
            this.BTN_UNIT_01.UseVisualStyleBackColor = false;
            this.BTN_UNIT_01.Click += new System.EventHandler(this.BTN_UNIT_01_Click);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("굴림", 9F);
            this.dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4});
            this.dataGridView.Location = new System.Drawing.Point(1, 2);
            this.dataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersVisible = false;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("굴림", 9F);
            this.dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridView.Size = new System.Drawing.Size(610, 1026);
            this.dataGridView.TabIndex = 121;
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellClick);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "UNIT NAME";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column1.Width = 110;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column2.Width = 130;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.FillWeight = 50F;
            this.dataGridViewTextBoxColumn1.HeaderText = "OBJECT_LEFT";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn1.Width = 87;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "OBJECT_RIGHT";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn2.Width = 95;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "TARGET_LEFT";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn3.Width = 90;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "TARGET_RIGHT";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dataGridViewTextBoxColumn4.Width = 98;
            // 
            // BTN_COPY
            // 
            this.BTN_COPY.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_COPY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_COPY.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_COPY.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BTN_COPY.Location = new System.Drawing.Point(1422, 963);
            this.BTN_COPY.Name = "BTN_COPY";
            this.BTN_COPY.Size = new System.Drawing.Size(240, 73);
            this.BTN_COPY.TabIndex = 122;
            this.BTN_COPY.Text = "CAL DATA COPY (PBD -> PBD_HEAD)";
            this.BTN_COPY.UseVisualStyleBackColor = false;
            this.BTN_COPY.Visible = false;
            this.BTN_COPY.Click += new System.EventHandler(this.BTN_COPY_Click);
            // 
            // GB_TRAY_CAL
            // 
            this.GB_TRAY_CAL.Controls.Add(this.label3);
            this.GB_TRAY_CAL.Controls.Add(this.label2);
            this.GB_TRAY_CAL.Controls.Add(this.label1);
            this.GB_TRAY_CAL.Controls.Add(this.BTN_COPY_DATA);
            this.GB_TRAY_CAL.Controls.Add(this.LB_Resolution);
            this.GB_TRAY_CAL.Controls.Add(this.label12);
            this.GB_TRAY_CAL.Controls.Add(this.LB_DX);
            this.GB_TRAY_CAL.Controls.Add(this.label14);
            this.GB_TRAY_CAL.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.GB_TRAY_CAL.ForeColor = System.Drawing.Color.White;
            this.GB_TRAY_CAL.Location = new System.Drawing.Point(1798, 804);
            this.GB_TRAY_CAL.Name = "GB_TRAY_CAL";
            this.GB_TRAY_CAL.Size = new System.Drawing.Size(79, 16);
            this.GB_TRAY_CAL.TabIndex = 123;
            this.GB_TRAY_CAL.TabStop = false;
            this.GB_TRAY_CAL.Text = "TRAY CALIBRATION";
            this.GB_TRAY_CAL.Visible = false;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.DarkGray;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(319, 460);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 24);
            this.label3.TabIndex = 140;
            this.label3.Text = "2. ";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.DarkGray;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.Blue;
            this.label2.Location = new System.Drawing.Point(213, 460);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 24);
            this.label2.TabIndex = 139;
            this.label2.Text = "Click 1.  ->";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Image = global::COG.Properties.Resources.TRAY_CAL;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(488, 412);
            this.label1.TabIndex = 138;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BTN_COPY_DATA
            // 
            this.BTN_COPY_DATA.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_COPY_DATA.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_COPY_DATA.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_COPY_DATA.Location = new System.Drawing.Point(318, 484);
            this.BTN_COPY_DATA.Name = "BTN_COPY_DATA";
            this.BTN_COPY_DATA.Size = new System.Drawing.Size(73, 84);
            this.BTN_COPY_DATA.TabIndex = 137;
            this.BTN_COPY_DATA.Text = "INPUT CAL DATA";
            this.BTN_COPY_DATA.UseVisualStyleBackColor = false;
            this.BTN_COPY_DATA.Click += new System.EventHandler(this.BTN_COPY_DATA_Click);
            // 
            // LB_Resolution
            // 
            this.LB_Resolution.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_Resolution.Location = new System.Drawing.Point(177, 538);
            this.LB_Resolution.Name = "LB_Resolution";
            this.LB_Resolution.Size = new System.Drawing.Size(121, 30);
            this.LB_Resolution.TabIndex = 31;
            this.LB_Resolution.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label12.Location = new System.Drawing.Point(6, 549);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(303, 38);
            this.label12.TabIndex = 33;
            this.label12.Text = "Resolution (㎛)                                                                   " +
    "          ";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_DX
            // 
            this.LB_DX.BackColor = System.Drawing.Color.White;
            this.LB_DX.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_DX.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_DX.Location = new System.Drawing.Point(176, 484);
            this.LB_DX.Name = "LB_DX";
            this.LB_DX.Size = new System.Drawing.Size(122, 49);
            this.LB_DX.TabIndex = 30;
            this.LB_DX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_DX.Click += new System.EventHandler(this.LB_DX_Click);
            // 
            // label14
            // 
            this.label14.BackColor = System.Drawing.Color.White;
            this.label14.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label14.ForeColor = System.Drawing.Color.Red;
            this.label14.Location = new System.Drawing.Point(7, 484);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(163, 49);
            this.label14.TabIndex = 29;
            this.label14.Text = "INPUT X Length (mm)";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_CENTER_B_LY
            // 
            this.LB_CENTER_B_LY.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_B_LY.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_B_LY.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_B_LY.Location = new System.Drawing.Point(78, 227);
            this.LB_CENTER_B_LY.Name = "LB_CENTER_B_LY";
            this.LB_CENTER_B_LY.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_B_LY.TabIndex = 125;
            this.LB_CENTER_B_LY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CENTER_B_RX
            // 
            this.LB_CENTER_B_RX.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_B_RX.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_B_RX.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_B_RX.Location = new System.Drawing.Point(431, 191);
            this.LB_CENTER_B_RX.Name = "LB_CENTER_B_RX";
            this.LB_CENTER_B_RX.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_B_RX.TabIndex = 127;
            this.LB_CENTER_B_RX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CENTER_B_RY
            // 
            this.LB_CENTER_B_RY.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_B_RY.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_B_RY.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_B_RY.Location = new System.Drawing.Point(372, 226);
            this.LB_CENTER_B_RY.Name = "LB_CENTER_B_RY";
            this.LB_CENTER_B_RY.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_B_RY.TabIndex = 131;
            this.LB_CENTER_B_RY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CENTER_B_LX
            // 
            this.LB_CENTER_B_LX.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_B_LX.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_B_LX.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_B_LX.Location = new System.Drawing.Point(20, 191);
            this.LB_CENTER_B_LX.Name = "LB_CENTER_B_LX";
            this.LB_CENTER_B_LX.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_B_LX.TabIndex = 130;
            this.LB_CENTER_B_LX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GB_MOTCENTER
            // 
            this.GB_MOTCENTER.Controls.Add(this.LB_UNIT_NAME);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_T_RY);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_T_LY);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_T_LX);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_T_RX);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_B_RY);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_B_LY);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_B_LX);
            this.GB_MOTCENTER.Controls.Add(this.LB_CENTER_B_RX);
            this.GB_MOTCENTER.Controls.Add(this.pictureBox1);
            this.GB_MOTCENTER.Location = new System.Drawing.Point(881, 759);
            this.GB_MOTCENTER.Name = "GB_MOTCENTER";
            this.GB_MOTCENTER.Size = new System.Drawing.Size(513, 276);
            this.GB_MOTCENTER.TabIndex = 132;
            this.GB_MOTCENTER.TabStop = false;
            this.GB_MOTCENTER.Text = "CENTER DSITANCE( mm )";
            this.GB_MOTCENTER.Visible = false;
            // 
            // LB_UNIT_NAME
            // 
            this.LB_UNIT_NAME.BackColor = System.Drawing.Color.LightGray;
            this.LB_UNIT_NAME.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_UNIT_NAME.ForeColor = System.Drawing.Color.Black;
            this.LB_UNIT_NAME.Location = new System.Drawing.Point(198, 34);
            this.LB_UNIT_NAME.Name = "LB_UNIT_NAME";
            this.LB_UNIT_NAME.Size = new System.Drawing.Size(109, 24);
            this.LB_UNIT_NAME.TabIndex = 136;
            this.LB_UNIT_NAME.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CENTER_T_RY
            // 
            this.LB_CENTER_T_RY.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_T_RY.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_T_RY.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_T_RY.Location = new System.Drawing.Point(372, 34);
            this.LB_CENTER_T_RY.Name = "LB_CENTER_T_RY";
            this.LB_CENTER_T_RY.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_T_RY.TabIndex = 135;
            this.LB_CENTER_T_RY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CENTER_T_LY
            // 
            this.LB_CENTER_T_LY.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_T_LY.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_T_LY.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_T_LY.Location = new System.Drawing.Point(78, 34);
            this.LB_CENTER_T_LY.Name = "LB_CENTER_T_LY";
            this.LB_CENTER_T_LY.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_T_LY.TabIndex = 132;
            this.LB_CENTER_T_LY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CENTER_T_LX
            // 
            this.LB_CENTER_T_LX.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_T_LX.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_T_LX.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_T_LX.Location = new System.Drawing.Point(20, 70);
            this.LB_CENTER_T_LX.Name = "LB_CENTER_T_LX";
            this.LB_CENTER_T_LX.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_T_LX.TabIndex = 134;
            this.LB_CENTER_T_LX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CENTER_T_RX
            // 
            this.LB_CENTER_T_RX.BackColor = System.Drawing.Color.LightGray;
            this.LB_CENTER_T_RX.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CENTER_T_RX.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_CENTER_T_RX.Location = new System.Drawing.Point(431, 70);
            this.LB_CENTER_T_RX.Name = "LB_CENTER_T_RX";
            this.LB_CENTER_T_RX.Size = new System.Drawing.Size(52, 29);
            this.LB_CENTER_T_RX.TabIndex = 133;
            this.LB_CENTER_T_RX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::COG.Properties.Resources.Cal_Image1;
            this.pictureBox1.Location = new System.Drawing.Point(6, 19);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(496, 250);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 124;
            this.pictureBox1.TabStop = false;
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(1791, 944);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 92);
            this.BTN_EXIT.TabIndex = 2;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // GB_Reel_Caldata
            // 
            this.GB_Reel_Caldata.Controls.Add(this.BTN_CALCULATION);
            this.GB_Reel_Caldata.Controls.Add(this.label8);
            this.GB_Reel_Caldata.Controls.Add(this.LB_RESULT);
            this.GB_Reel_Caldata.Controls.Add(this.label6);
            this.GB_Reel_Caldata.Controls.Add(this.LB_DISTANCE);
            this.GB_Reel_Caldata.Controls.Add(this.label5);
            this.GB_Reel_Caldata.Controls.Add(this.LB_PIXEL_END_01);
            this.GB_Reel_Caldata.Controls.Add(this.LB_PIXEL_START_00);
            this.GB_Reel_Caldata.Controls.Add(this.label24);
            this.GB_Reel_Caldata.Controls.Add(this.BTN_REEL2_CAL_01);
            this.GB_Reel_Caldata.Controls.Add(this.BTN_REEL1_CAL_00);
            this.GB_Reel_Caldata.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.GB_Reel_Caldata.ForeColor = System.Drawing.Color.White;
            this.GB_Reel_Caldata.Location = new System.Drawing.Point(1422, 754);
            this.GB_Reel_Caldata.Name = "GB_Reel_Caldata";
            this.GB_Reel_Caldata.Size = new System.Drawing.Size(356, 199);
            this.GB_Reel_Caldata.TabIndex = 135;
            this.GB_Reel_Caldata.TabStop = false;
            this.GB_Reel_Caldata.Text = "Input Reel Cal Data";
            this.GB_Reel_Caldata.Visible = false;
            // 
            // BTN_CALCULATION
            // 
            this.BTN_CALCULATION.ForeColor = System.Drawing.Color.Black;
            this.BTN_CALCULATION.Location = new System.Drawing.Point(242, 154);
            this.BTN_CALCULATION.Name = "BTN_CALCULATION";
            this.BTN_CALCULATION.Size = new System.Drawing.Size(106, 37);
            this.BTN_CALCULATION.TabIndex = 49;
            this.BTN_CALCULATION.Text = "CALCULATION";
            this.BTN_CALCULATION.UseVisualStyleBackColor = true;
            this.BTN_CALCULATION.Click += new System.EventHandler(this.BTN_CALCULATION_Click);
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.DarkGray;
            this.label8.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label8.Location = new System.Drawing.Point(8, 154);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(113, 37);
            this.label8.TabIndex = 48;
            this.label8.Text = "RESULT( um )";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_RESULT
            // 
            this.LB_RESULT.BackColor = System.Drawing.Color.White;
            this.LB_RESULT.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_RESULT.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_RESULT.Location = new System.Drawing.Point(127, 154);
            this.LB_RESULT.Name = "LB_RESULT";
            this.LB_RESULT.Size = new System.Drawing.Size(106, 37);
            this.LB_RESULT.TabIndex = 47;
            this.LB_RESULT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.DarkGray;
            this.label6.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.Location = new System.Drawing.Point(8, 110);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 37);
            this.label6.TabIndex = 46;
            this.label6.Text = "DISTANCE( um )";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_DISTANCE
            // 
            this.LB_DISTANCE.BackColor = System.Drawing.Color.White;
            this.LB_DISTANCE.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_DISTANCE.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_DISTANCE.Location = new System.Drawing.Point(127, 110);
            this.LB_DISTANCE.Name = "LB_DISTANCE";
            this.LB_DISTANCE.Size = new System.Drawing.Size(106, 37);
            this.LB_DISTANCE.TabIndex = 45;
            this.LB_DISTANCE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_DISTANCE.Click += new System.EventHandler(this.LB_DISTANCE_Click);
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.DarkGray;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.Location = new System.Drawing.Point(8, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 37);
            this.label5.TabIndex = 44;
            this.label5.Text = "PIXEL_END";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_PIXEL_END_01
            // 
            this.LB_PIXEL_END_01.BackColor = System.Drawing.Color.White;
            this.LB_PIXEL_END_01.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_PIXEL_END_01.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_PIXEL_END_01.Location = new System.Drawing.Point(127, 66);
            this.LB_PIXEL_END_01.Name = "LB_PIXEL_END_01";
            this.LB_PIXEL_END_01.Size = new System.Drawing.Size(106, 37);
            this.LB_PIXEL_END_01.TabIndex = 43;
            this.LB_PIXEL_END_01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_PIXEL_END_01.Click += new System.EventHandler(this.LB_PIXEL_Click);
            // 
            // LB_PIXEL_START_00
            // 
            this.LB_PIXEL_START_00.BackColor = System.Drawing.Color.White;
            this.LB_PIXEL_START_00.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_PIXEL_START_00.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_PIXEL_START_00.Location = new System.Drawing.Point(127, 22);
            this.LB_PIXEL_START_00.Name = "LB_PIXEL_START_00";
            this.LB_PIXEL_START_00.Size = new System.Drawing.Size(106, 37);
            this.LB_PIXEL_START_00.TabIndex = 42;
            this.LB_PIXEL_START_00.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_PIXEL_START_00.Click += new System.EventHandler(this.LB_PIXEL_Click);
            // 
            // label24
            // 
            this.label24.BackColor = System.Drawing.Color.DarkGray;
            this.label24.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label24.Location = new System.Drawing.Point(8, 22);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(113, 37);
            this.label24.TabIndex = 41;
            this.label24.Text = "PIXEL_START";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BTN_REEL2_CAL_01
            // 
            this.BTN_REEL2_CAL_01.ForeColor = System.Drawing.Color.Black;
            this.BTN_REEL2_CAL_01.Location = new System.Drawing.Point(242, 65);
            this.BTN_REEL2_CAL_01.Name = "BTN_REEL2_CAL_01";
            this.BTN_REEL2_CAL_01.Size = new System.Drawing.Size(106, 38);
            this.BTN_REEL2_CAL_01.TabIndex = 1;
            this.BTN_REEL2_CAL_01.Text = "Reel 2";
            this.BTN_REEL2_CAL_01.UseVisualStyleBackColor = true;
            this.BTN_REEL2_CAL_01.Click += new System.EventHandler(this.BTN_REEL_CAL_Click);
            // 
            // BTN_REEL1_CAL_00
            // 
            this.BTN_REEL1_CAL_00.ForeColor = System.Drawing.Color.Black;
            this.BTN_REEL1_CAL_00.Location = new System.Drawing.Point(242, 21);
            this.BTN_REEL1_CAL_00.Name = "BTN_REEL1_CAL_00";
            this.BTN_REEL1_CAL_00.Size = new System.Drawing.Size(106, 38);
            this.BTN_REEL1_CAL_00.TabIndex = 0;
            this.BTN_REEL1_CAL_00.Text = "Reel 1";
            this.BTN_REEL1_CAL_00.UseVisualStyleBackColor = true;
            this.BTN_REEL1_CAL_00.Click += new System.EventHandler(this.BTN_REEL_CAL_Click);
            // 
            // Form_CalDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 11F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1900, 1038);
            this.ControlBox = false;
            this.Controls.Add(this.GB_Reel_Caldata);
            this.Controls.Add(this.GB_TRAY_CAL);
            this.Controls.Add(this.BTN_COPY);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.BTN_UNIT_01);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.GB_MOTCENTER);
            this.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form_CalDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_CalDisplay";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.GB_TRAY_CAL.ResumeLayout(false);
            this.GB_MOTCENTER.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.GB_Reel_Caldata.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BTN_UNIT_01;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.Button BTN_COPY;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.GroupBox GB_TRAY_CAL;
        private System.Windows.Forms.Label LB_Resolution;
        private System.Windows.Forms.Label LB_DX;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button BTN_COPY_DATA;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label LB_CENTER_B_LY;
        private System.Windows.Forms.Label LB_CENTER_B_RX;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label LB_CENTER_B_RY;
        private System.Windows.Forms.Label LB_CENTER_B_LX;
        private System.Windows.Forms.GroupBox GB_MOTCENTER;
        private System.Windows.Forms.Label LB_CENTER_T_RY;
        private System.Windows.Forms.Label LB_CENTER_T_LY;
        private System.Windows.Forms.Label LB_CENTER_T_LX;
        private System.Windows.Forms.Label LB_CENTER_T_RX;
        private System.Windows.Forms.Label LB_UNIT_NAME;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.GroupBox GB_Reel_Caldata;
        private System.Windows.Forms.Button BTN_REEL2_CAL_01;
        private System.Windows.Forms.Button BTN_REEL1_CAL_00;
        private System.Windows.Forms.Button BTN_CALCULATION;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label LB_RESULT;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label LB_DISTANCE;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label LB_PIXEL_END_01;
        private System.Windows.Forms.Label LB_PIXEL_START_00;
        private System.Windows.Forms.Label label24;
    }
}
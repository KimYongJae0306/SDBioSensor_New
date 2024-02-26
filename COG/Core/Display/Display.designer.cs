namespace JAS.Controls.Display
{
    partial class Display
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Display));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.LB_MOUSE_POS = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.CogDisplay00 = new Cognex.VisionPro.CogRecordDisplay();
            this.RB_BTN_02 = new System.Windows.Forms.RadioButton();
            this.RB_BTN_01 = new System.Windows.Forms.RadioButton();
            this.PN_GROUP = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CB_BTN_CROSS_CUSTOM = new System.Windows.Forms.CheckBox();
            this.CB_BTN_ARC = new System.Windows.Forms.CheckBox();
            this.BTN_UNDO = new System.Windows.Forms.CheckBox();
            this.CB_BTN_SQUARE = new System.Windows.Forms.CheckBox();
            this.CB_BTN_CROSS_LINE = new System.Windows.Forms.CheckBox();
            this.CB_BTN_POINT2LINE = new System.Windows.Forms.CheckBox();
            this.CB_BTN_POINT2POINT = new System.Windows.Forms.CheckBox();
            this.BTN_DELALL = new System.Windows.Forms.CheckBox();
            this.BTN_IMG_LOAD = new System.Windows.Forms.Button();
            this.BTN_FIT = new System.Windows.Forms.Button();
            this.CB_BTN_MOUSE_MODE = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.NUD_RESOLUTION = new System.Windows.Forms.NumericUpDown();
            this.cogDisplayStatusBar00 = new Cognex.VisionPro.CogDisplayStatusBarV2();
            this.TIP_00 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.CogDisplay00)).BeginInit();
            this.PN_GROUP.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUD_RESOLUTION)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // LB_MOUSE_POS
            // 
            this.LB_MOUSE_POS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_MOUSE_POS.Location = new System.Drawing.Point(168, 8);
            this.LB_MOUSE_POS.Name = "LB_MOUSE_POS";
            this.LB_MOUSE_POS.Size = new System.Drawing.Size(99, 32);
            this.LB_MOUSE_POS.TabIndex = 128;
            this.LB_MOUSE_POS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_MOUSE_POS.Visible = false;
            // 
            // CogDisplay00
            // 
            this.CogDisplay00.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CogDisplay00.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.CogDisplay00.ColorMapLowerRoiLimit = 0D;
            this.CogDisplay00.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.CogDisplay00.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.CogDisplay00.ColorMapUpperRoiLimit = 1D;
            this.CogDisplay00.DoubleTapZoomCycleLength = 2;
            this.CogDisplay00.DoubleTapZoomSensitivity = 2.5D;
            this.CogDisplay00.Location = new System.Drawing.Point(0, 0);
            this.CogDisplay00.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.CogDisplay00.MouseWheelSensitivity = 1D;
            this.CogDisplay00.Name = "CogDisplay00";
            this.CogDisplay00.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("CogDisplay00.OcxState")));
            this.CogDisplay00.Size = new System.Drawing.Size(518, 448);
            this.CogDisplay00.TabIndex = 119;
            this.CogDisplay00.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CogDisplay00_MouseDown);
            this.CogDisplay00.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CogDisplay00_MouseMove);
            // 
            // RB_BTN_02
            // 
            this.RB_BTN_02.Appearance = System.Windows.Forms.Appearance.Button;
            this.RB_BTN_02.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RB_BTN_02.Location = new System.Drawing.Point(99, 3);
            this.RB_BTN_02.Name = "RB_BTN_02";
            this.RB_BTN_02.Size = new System.Drawing.Size(63, 32);
            this.RB_BTN_02.TabIndex = 132;
            this.RB_BTN_02.TabStop = true;
            this.RB_BTN_02.Text = "Pause";
            this.RB_BTN_02.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.RB_BTN_02.UseVisualStyleBackColor = true;
            this.RB_BTN_02.Visible = false;
            // 
            // RB_BTN_01
            // 
            this.RB_BTN_01.Appearance = System.Windows.Forms.Appearance.Button;
            this.RB_BTN_01.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RB_BTN_01.Location = new System.Drawing.Point(17, 3);
            this.RB_BTN_01.Name = "RB_BTN_01";
            this.RB_BTN_01.Size = new System.Drawing.Size(63, 32);
            this.RB_BTN_01.TabIndex = 131;
            this.RB_BTN_01.TabStop = true;
            this.RB_BTN_01.Text = "Live";
            this.RB_BTN_01.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.RB_BTN_01.UseVisualStyleBackColor = true;
            this.RB_BTN_01.Visible = false;
            // 
            // PN_GROUP
            // 
            this.PN_GROUP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PN_GROUP.Controls.Add(this.panel1);
            this.PN_GROUP.Controls.Add(this.panel3);
            this.PN_GROUP.Location = new System.Drawing.Point(0, 441);
            this.PN_GROUP.Name = "PN_GROUP";
            this.PN_GROUP.Size = new System.Drawing.Size(518, 68);
            this.PN_GROUP.TabIndex = 163;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.CB_BTN_CROSS_CUSTOM);
            this.panel1.Controls.Add(this.CB_BTN_ARC);
            this.panel1.Controls.Add(this.BTN_UNDO);
            this.panel1.Controls.Add(this.CB_BTN_SQUARE);
            this.panel1.Controls.Add(this.CB_BTN_CROSS_LINE);
            this.panel1.Controls.Add(this.CB_BTN_POINT2LINE);
            this.panel1.Controls.Add(this.CB_BTN_POINT2POINT);
            this.panel1.Controls.Add(this.BTN_DELALL);
            this.panel1.Controls.Add(this.BTN_IMG_LOAD);
            this.panel1.Controls.Add(this.BTN_FIT);
            this.panel1.Controls.Add(this.CB_BTN_MOUSE_MODE);
            this.panel1.Location = new System.Drawing.Point(0, 31);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(518, 37);
            this.panel1.TabIndex = 167;
            // 
            // CB_BTN_CROSS_CUSTOM
            // 
            this.CB_BTN_CROSS_CUSTOM.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_BTN_CROSS_CUSTOM.BackColor = System.Drawing.Color.DarkGray;
            this.CB_BTN_CROSS_CUSTOM.FlatAppearance.BorderSize = 0;
            this.CB_BTN_CROSS_CUSTOM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CB_BTN_CROSS_CUSTOM.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BTN_CROSS_CUSTOM.Image = global::COG.Properties.Resources.Cross_Custom;
            this.CB_BTN_CROSS_CUSTOM.Location = new System.Drawing.Point(152, 3);
            this.CB_BTN_CROSS_CUSTOM.Name = "CB_BTN_CROSS_CUSTOM";
            this.CB_BTN_CROSS_CUSTOM.Size = new System.Drawing.Size(30, 32);
            this.CB_BTN_CROSS_CUSTOM.TabIndex = 167;
            this.CB_BTN_CROSS_CUSTOM.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_CROSS_CUSTOM.UseVisualStyleBackColor = false;
            this.CB_BTN_CROSS_CUSTOM.CheckedChanged += new System.EventHandler(this.CB_BTN_CROSS_CUSTOM_CheckedChanged);
            this.CB_BTN_CROSS_CUSTOM.Click += new System.EventHandler(this.CB_BTN_CROSS_CUSTOM_Click);
            // 
            // CB_BTN_ARC
            // 
            this.CB_BTN_ARC.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_BTN_ARC.BackColor = System.Drawing.Color.DarkGray;
            this.CB_BTN_ARC.FlatAppearance.BorderSize = 0;
            this.CB_BTN_ARC.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CB_BTN_ARC.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BTN_ARC.Image = global::COG.Properties.Resources.Arc_24;
            this.CB_BTN_ARC.Location = new System.Drawing.Point(306, 2);
            this.CB_BTN_ARC.Name = "CB_BTN_ARC";
            this.CB_BTN_ARC.Size = new System.Drawing.Size(30, 32);
            this.CB_BTN_ARC.TabIndex = 165;
            this.CB_BTN_ARC.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_ARC.UseVisualStyleBackColor = false;
            this.CB_BTN_ARC.CheckedChanged += new System.EventHandler(this.CBBOX_CheckedChanged);
            // 
            // BTN_UNDO
            // 
            this.BTN_UNDO.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_UNDO.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_UNDO.FlatAppearance.BorderSize = 0;
            this.BTN_UNDO.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_UNDO.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_UNDO.Image = global::COG.Properties.Resources.undo3_16;
            this.BTN_UNDO.Location = new System.Drawing.Point(450, 2);
            this.BTN_UNDO.Name = "BTN_UNDO";
            this.BTN_UNDO.Size = new System.Drawing.Size(30, 32);
            this.BTN_UNDO.TabIndex = 166;
            this.BTN_UNDO.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_UNDO.UseVisualStyleBackColor = false;
            this.BTN_UNDO.Click += new System.EventHandler(this.BTN_UNDO_Click);
            // 
            // CB_BTN_SQUARE
            // 
            this.CB_BTN_SQUARE.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_BTN_SQUARE.BackColor = System.Drawing.Color.DarkGray;
            this.CB_BTN_SQUARE.FlatAppearance.BorderSize = 0;
            this.CB_BTN_SQUARE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CB_BTN_SQUARE.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BTN_SQUARE.Image = global::COG.Properties.Resources.square1_24;
            this.CB_BTN_SQUARE.Location = new System.Drawing.Point(265, 2);
            this.CB_BTN_SQUARE.Name = "CB_BTN_SQUARE";
            this.CB_BTN_SQUARE.Size = new System.Drawing.Size(30, 32);
            this.CB_BTN_SQUARE.TabIndex = 164;
            this.CB_BTN_SQUARE.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_SQUARE.UseVisualStyleBackColor = false;
            this.CB_BTN_SQUARE.CheckedChanged += new System.EventHandler(this.CBBOX_CheckedChanged);
            // 
            // CB_BTN_CROSS_LINE
            // 
            this.CB_BTN_CROSS_LINE.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_BTN_CROSS_LINE.BackColor = System.Drawing.Color.DarkGray;
            this.CB_BTN_CROSS_LINE.FlatAppearance.BorderSize = 0;
            this.CB_BTN_CROSS_LINE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CB_BTN_CROSS_LINE.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BTN_CROSS_LINE.Image = global::COG.Properties.Resources.Corss22_20;
            this.CB_BTN_CROSS_LINE.Location = new System.Drawing.Point(117, 2);
            this.CB_BTN_CROSS_LINE.Name = "CB_BTN_CROSS_LINE";
            this.CB_BTN_CROSS_LINE.Size = new System.Drawing.Size(30, 32);
            this.CB_BTN_CROSS_LINE.TabIndex = 163;
            this.CB_BTN_CROSS_LINE.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_CROSS_LINE.UseVisualStyleBackColor = false;
            this.CB_BTN_CROSS_LINE.CheckedChanged += new System.EventHandler(this.CBBOX_CheckedChanged);
            this.CB_BTN_CROSS_LINE.Click += new System.EventHandler(this.CB_BTN_CROSS_LINE_Click);
            // 
            // CB_BTN_POINT2LINE
            // 
            this.CB_BTN_POINT2LINE.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_BTN_POINT2LINE.BackColor = System.Drawing.Color.DarkGray;
            this.CB_BTN_POINT2LINE.FlatAppearance.BorderSize = 0;
            this.CB_BTN_POINT2LINE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CB_BTN_POINT2LINE.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BTN_POINT2LINE.Image = global::COG.Properties.Resources.arrows_24;
            this.CB_BTN_POINT2LINE.Location = new System.Drawing.Point(224, 2);
            this.CB_BTN_POINT2LINE.Name = "CB_BTN_POINT2LINE";
            this.CB_BTN_POINT2LINE.Size = new System.Drawing.Size(30, 32);
            this.CB_BTN_POINT2LINE.TabIndex = 162;
            this.CB_BTN_POINT2LINE.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_POINT2LINE.UseVisualStyleBackColor = false;
            this.CB_BTN_POINT2LINE.CheckedChanged += new System.EventHandler(this.CBBOX_CheckedChanged);
            // 
            // CB_BTN_POINT2POINT
            // 
            this.CB_BTN_POINT2POINT.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_BTN_POINT2POINT.BackColor = System.Drawing.Color.DarkGray;
            this.CB_BTN_POINT2POINT.FlatAppearance.BorderSize = 0;
            this.CB_BTN_POINT2POINT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CB_BTN_POINT2POINT.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BTN_POINT2POINT.Image = global::COG.Properties.Resources.Line_20;
            this.CB_BTN_POINT2POINT.Location = new System.Drawing.Point(183, 2);
            this.CB_BTN_POINT2POINT.Name = "CB_BTN_POINT2POINT";
            this.CB_BTN_POINT2POINT.Size = new System.Drawing.Size(30, 32);
            this.CB_BTN_POINT2POINT.TabIndex = 161;
            this.CB_BTN_POINT2POINT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_POINT2POINT.UseVisualStyleBackColor = false;
            this.CB_BTN_POINT2POINT.CheckedChanged += new System.EventHandler(this.CBBOX_CheckedChanged);
            // 
            // BTN_DELALL
            // 
            this.BTN_DELALL.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_DELALL.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_DELALL.FlatAppearance.BorderSize = 0;
            this.BTN_DELALL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_DELALL.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_DELALL.Image = global::COG.Properties.Resources.delete_16;
            this.BTN_DELALL.Location = new System.Drawing.Point(486, 2);
            this.BTN_DELALL.Name = "BTN_DELALL";
            this.BTN_DELALL.Size = new System.Drawing.Size(30, 32);
            this.BTN_DELALL.TabIndex = 158;
            this.BTN_DELALL.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_DELALL.UseVisualStyleBackColor = false;
            this.BTN_DELALL.Click += new System.EventHandler(this.BTN_DELALL_Click);
            // 
            // BTN_IMG_LOAD
            // 
            this.BTN_IMG_LOAD.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_IMG_LOAD.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_IMG_LOAD.FlatAppearance.BorderSize = 0;
            this.BTN_IMG_LOAD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_IMG_LOAD.Font = new System.Drawing.Font("맑은 고딕", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_IMG_LOAD.Image = global::COG.Properties.Resources.open3_16;
            this.BTN_IMG_LOAD.Location = new System.Drawing.Point(3, 2);
            this.BTN_IMG_LOAD.Name = "BTN_IMG_LOAD";
            this.BTN_IMG_LOAD.Size = new System.Drawing.Size(30, 32);
            this.BTN_IMG_LOAD.TabIndex = 160;
            this.BTN_IMG_LOAD.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_IMG_LOAD.UseVisualStyleBackColor = false;
            this.BTN_IMG_LOAD.Click += new System.EventHandler(this.BTN_IMG_LOAD_Click);
            // 
            // BTN_FIT
            // 
            this.BTN_FIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_FIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_FIT.FlatAppearance.BorderSize = 0;
            this.BTN_FIT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_FIT.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_FIT.Image = global::COG.Properties.Resources.Fit2_16;
            this.BTN_FIT.Location = new System.Drawing.Point(47, 2);
            this.BTN_FIT.Name = "BTN_FIT";
            this.BTN_FIT.Size = new System.Drawing.Size(30, 32);
            this.BTN_FIT.TabIndex = 159;
            this.BTN_FIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_FIT.UseVisualStyleBackColor = false;
            this.BTN_FIT.Click += new System.EventHandler(this.BTN_FIT_Click);
            // 
            // CB_BTN_MOUSE_MODE
            // 
            this.CB_BTN_MOUSE_MODE.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_BTN_MOUSE_MODE.BackColor = System.Drawing.Color.DarkGray;
            this.CB_BTN_MOUSE_MODE.FlatAppearance.BorderSize = 0;
            this.CB_BTN_MOUSE_MODE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CB_BTN_MOUSE_MODE.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_BTN_MOUSE_MODE.Image = global::COG.Properties.Resources.touch_16;
            this.CB_BTN_MOUSE_MODE.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_MOUSE_MODE.Location = new System.Drawing.Point(82, 2);
            this.CB_BTN_MOUSE_MODE.Name = "CB_BTN_MOUSE_MODE";
            this.CB_BTN_MOUSE_MODE.Size = new System.Drawing.Size(30, 32);
            this.CB_BTN_MOUSE_MODE.TabIndex = 158;
            this.CB_BTN_MOUSE_MODE.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_BTN_MOUSE_MODE.UseVisualStyleBackColor = false;
            this.CB_BTN_MOUSE_MODE.CheckedChanged += new System.EventHandler(this.CBBOX_CheckedChanged);
            this.CB_BTN_MOUSE_MODE.Click += new System.EventHandler(this.CB_BTN_MOUSE_MODE_Click);
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.NUD_RESOLUTION);
            this.panel3.Controls.Add(this.cogDisplayStatusBar00);
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(518, 32);
            this.panel3.TabIndex = 166;
            // 
            // NUD_RESOLUTION
            // 
            this.NUD_RESOLUTION.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NUD_RESOLUTION.DecimalPlaces = 2;
            this.NUD_RESOLUTION.Enabled = false;
            this.NUD_RESOLUTION.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.NUD_RESOLUTION.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.NUD_RESOLUTION.Location = new System.Drawing.Point(416, 8);
            this.NUD_RESOLUTION.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.NUD_RESOLUTION.Name = "NUD_RESOLUTION";
            this.NUD_RESOLUTION.Size = new System.Drawing.Size(99, 22);
            this.NUD_RESOLUTION.TabIndex = 131;
            this.NUD_RESOLUTION.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cogDisplayStatusBar00
            // 
            this.cogDisplayStatusBar00.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cogDisplayStatusBar00.CoordinateSpaceName = "*\\#";
            this.cogDisplayStatusBar00.CoordinateSpaceName3D = "*\\#";
            this.cogDisplayStatusBar00.Location = new System.Drawing.Point(3, 1);
            this.cogDisplayStatusBar00.Name = "cogDisplayStatusBar00";
            this.cogDisplayStatusBar00.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cogDisplayStatusBar00.Size = new System.Drawing.Size(416, 29);
            this.cogDisplayStatusBar00.TabIndex = 132;
            this.cogDisplayStatusBar00.Use3DCoordinateSpaceTree = false;
            // 
            // Display
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.Controls.Add(this.CogDisplay00);
            this.Controls.Add(this.PN_GROUP);
            this.Controls.Add(this.LB_MOUSE_POS);
            this.Controls.Add(this.RB_BTN_02);
            this.Controls.Add(this.RB_BTN_01);
            this.Name = "Display";
            this.Size = new System.Drawing.Size(519, 511);
            this.Load += new System.EventHandler(this.UC_DISPLAY_Load);
            ((System.ComponentModel.ISupportInitialize)(this.CogDisplay00)).EndInit();
            this.PN_GROUP.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NUD_RESOLUTION)).EndInit();
            this.ResumeLayout(false);

        }


        private Cognex.VisionPro.CogDisplayStatusBarV2 cogDisplayStatusBar00;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.NumericUpDown NUD_RESOLUTION;
        private System.Windows.Forms.CheckBox BTN_UNDO;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox CB_BTN_SQUARE;
        private System.Windows.Forms.CheckBox CB_BTN_CROSS_LINE;
        private System.Windows.Forms.CheckBox CB_BTN_POINT2LINE;
        private System.Windows.Forms.CheckBox CB_BTN_POINT2POINT;
        private System.Windows.Forms.CheckBox BTN_DELALL;
        private System.Windows.Forms.Button BTN_IMG_LOAD;
        private System.Windows.Forms.Button BTN_FIT;
        private System.Windows.Forms.CheckBox CB_BTN_MOUSE_MODE;
        public Cognex.VisionPro.CogRecordDisplay CogDisplay00;
        private System.Windows.Forms.RadioButton RB_BTN_02;
        private System.Windows.Forms.RadioButton RB_BTN_01;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Panel PN_GROUP;
        private System.Windows.Forms.Label LB_MOUSE_POS;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolTip TIP_00;
        private System.Windows.Forms.CheckBox CB_BTN_ARC;
        private System.Windows.Forms.CheckBox CB_BTN_CROSS_CUSTOM;
    }
}

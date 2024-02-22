namespace COG
{
    partial class Form_RCS
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_RCS));
            this.LB_ALIGNUNIT_NAME = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.LB_MARKPOS = new System.Windows.Forms.Label();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_MOVE_UP = new System.Windows.Forms.Button();
            this.BTN_MOVE_DOWN = new System.Windows.Forms.Button();
            this.BTN_MOVE_LEFT = new System.Windows.Forms.Button();
            this.BTN_MOVE_RIGHT = new System.Windows.Forms.Button();
            this.BTN_RCS_OK = new System.Windows.Forms.Button();
            this.BTN_RCS_NG = new System.Windows.Forms.Button();
            this.MM_DISPLAY00 = new JAS.Controls.Display.Display();
            this.RBTN_RCS_POINT01 = new System.Windows.Forms.RadioButton();
            this.RBTN_RCS_POINT02 = new System.Windows.Forms.RadioButton();
            this.RBTN_RCS_POINT03 = new System.Windows.Forms.RadioButton();
            this.RBTN_RCS_POINT04 = new System.Windows.Forms.RadioButton();
            this.RBTN_RCS_POINT05 = new System.Windows.Forms.RadioButton();
            this.RBTN_RCS_POINT06 = new System.Windows.Forms.RadioButton();
            this.RBTN_RCS_POINT07 = new System.Windows.Forms.RadioButton();
            this.RBTN_RCS_POINT08 = new System.Windows.Forms.RadioButton();
            this.CB_DIRECTION = new System.Windows.Forms.ComboBox();
            this.BTN_RCS_SCALE_BAR = new System.Windows.Forms.Button();
            this.BTN_RCS_POINT_OK = new System.Windows.Forms.Button();
            this.LB_SCALE = new System.Windows.Forms.Label();
            this.BTN_RCS_ROTATE_SCALE_BAR = new System.Windows.Forms.Button();
            this.BTN_RCS_COLOR_SCALE_BAR = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LB_ALIGNUNIT_NAME
            // 
            this.LB_ALIGNUNIT_NAME.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_ALIGNUNIT_NAME.Font = new System.Drawing.Font("굴림", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_ALIGNUNIT_NAME.Location = new System.Drawing.Point(840, 20);
            this.LB_ALIGNUNIT_NAME.Name = "LB_ALIGNUNIT_NAME";
            this.LB_ALIGNUNIT_NAME.Size = new System.Drawing.Size(274, 61);
            this.LB_ALIGNUNIT_NAME.TabIndex = 47;
            this.LB_ALIGNUNIT_NAME.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // LB_MARKPOS
            // 
            this.LB_MARKPOS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_MARKPOS.Font = new System.Drawing.Font("굴림", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_MARKPOS.Location = new System.Drawing.Point(840, 84);
            this.LB_MARKPOS.Name = "LB_MARKPOS";
            this.LB_MARKPOS.Size = new System.Drawing.Size(274, 61);
            this.LB_MARKPOS.TabIndex = 48;
            this.LB_MARKPOS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(1014, 707);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 44;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_MOVE_UP
            // 
            this.BTN_MOVE_UP.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_MOVE_UP.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_MOVE_UP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_MOVE_UP.Image = ((System.Drawing.Image)(resources.GetObject("BTN_MOVE_UP.Image")));
            this.BTN_MOVE_UP.Location = new System.Drawing.Point(933, 185);
            this.BTN_MOVE_UP.Name = "BTN_MOVE_UP";
            this.BTN_MOVE_UP.Size = new System.Drawing.Size(89, 77);
            this.BTN_MOVE_UP.TabIndex = 40;
            this.BTN_MOVE_UP.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_MOVE_UP.UseVisualStyleBackColor = false;
            this.BTN_MOVE_UP.Click += new System.EventHandler(this.BTN_MOVE_UP_Click);
            // 
            // BTN_MOVE_DOWN
            // 
            this.BTN_MOVE_DOWN.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_MOVE_DOWN.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_MOVE_DOWN.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_MOVE_DOWN.Image = ((System.Drawing.Image)(resources.GetObject("BTN_MOVE_DOWN.Image")));
            this.BTN_MOVE_DOWN.Location = new System.Drawing.Point(933, 350);
            this.BTN_MOVE_DOWN.Name = "BTN_MOVE_DOWN";
            this.BTN_MOVE_DOWN.Size = new System.Drawing.Size(89, 77);
            this.BTN_MOVE_DOWN.TabIndex = 41;
            this.BTN_MOVE_DOWN.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.BTN_MOVE_DOWN.UseVisualStyleBackColor = false;
            this.BTN_MOVE_DOWN.Click += new System.EventHandler(this.BTN_MOVE_DOWN_Click);
            // 
            // BTN_MOVE_LEFT
            // 
            this.BTN_MOVE_LEFT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_MOVE_LEFT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_MOVE_LEFT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_MOVE_LEFT.Image = ((System.Drawing.Image)(resources.GetObject("BTN_MOVE_LEFT.Image")));
            this.BTN_MOVE_LEFT.Location = new System.Drawing.Point(840, 265);
            this.BTN_MOVE_LEFT.Name = "BTN_MOVE_LEFT";
            this.BTN_MOVE_LEFT.Size = new System.Drawing.Size(89, 77);
            this.BTN_MOVE_LEFT.TabIndex = 42;
            this.BTN_MOVE_LEFT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_MOVE_LEFT.UseVisualStyleBackColor = false;
            this.BTN_MOVE_LEFT.Click += new System.EventHandler(this.BTN_MOVE_LEFT_Click);
            // 
            // BTN_MOVE_RIGHT
            // 
            this.BTN_MOVE_RIGHT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_MOVE_RIGHT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_MOVE_RIGHT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_MOVE_RIGHT.Image = ((System.Drawing.Image)(resources.GetObject("BTN_MOVE_RIGHT.Image")));
            this.BTN_MOVE_RIGHT.Location = new System.Drawing.Point(1025, 265);
            this.BTN_MOVE_RIGHT.Name = "BTN_MOVE_RIGHT";
            this.BTN_MOVE_RIGHT.Size = new System.Drawing.Size(89, 77);
            this.BTN_MOVE_RIGHT.TabIndex = 43;
            this.BTN_MOVE_RIGHT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_MOVE_RIGHT.UseVisualStyleBackColor = false;
            this.BTN_MOVE_RIGHT.Click += new System.EventHandler(this.BTN_MOVE_RIGHT_Click);
            // 
            // BTN_RCS_OK
            // 
            this.BTN_RCS_OK.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RCS_OK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RCS_OK.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RCS_OK.Location = new System.Drawing.Point(840, 460);
            this.BTN_RCS_OK.Name = "BTN_RCS_OK";
            this.BTN_RCS_OK.Size = new System.Drawing.Size(274, 68);
            this.BTN_RCS_OK.TabIndex = 257;
            this.BTN_RCS_OK.Text = "RCS OK";
            this.BTN_RCS_OK.UseVisualStyleBackColor = false;
            this.BTN_RCS_OK.Click += new System.EventHandler(this.BTN_RCS_OK_Click);
            // 
            // BTN_RCS_NG
            // 
            this.BTN_RCS_NG.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RCS_NG.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RCS_NG.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RCS_NG.Location = new System.Drawing.Point(840, 540);
            this.BTN_RCS_NG.Name = "BTN_RCS_NG";
            this.BTN_RCS_NG.Size = new System.Drawing.Size(274, 68);
            this.BTN_RCS_NG.TabIndex = 258;
            this.BTN_RCS_NG.Text = "RCS NG";
            this.BTN_RCS_NG.UseVisualStyleBackColor = false;
            this.BTN_RCS_NG.Click += new System.EventHandler(this.BTN_RCS_NG_Click);
            // 
            // MM_DISPLAY00
            // 
            this.MM_DISPLAY00.BackColor = System.Drawing.Color.DarkGray;
            this.MM_DISPLAY00.CustomCross = ((System.Drawing.PointF)(resources.GetObject("MM_DISPLAY00.CustomCross")));
            this.MM_DISPLAY00.DisplayManuConstants = ((JAS.Controls.Display.DisplayEnableConstants)(((((((((((JAS.Controls.Display.DisplayEnableConstants.LoadImage | JAS.Controls.Display.DisplayEnableConstants.DisplayFit) 
            | JAS.Controls.Display.DisplayEnableConstants.Undo) 
            | JAS.Controls.Display.DisplayEnableConstants.Delete) 
            | JAS.Controls.Display.DisplayEnableConstants.TouchMove0) 
            | JAS.Controls.Display.DisplayEnableConstants.PointToPoint1) 
            | JAS.Controls.Display.DisplayEnableConstants.LineToLine2) 
            | JAS.Controls.Display.DisplayEnableConstants.Arc3Points) 
            | JAS.Controls.Display.DisplayEnableConstants.Square5) 
            | JAS.Controls.Display.DisplayEnableConstants.CrossLine6) 
            | JAS.Controls.Display.DisplayEnableConstants.CrossCustom)));
            this.MM_DISPLAY00.Image = null;
            this.MM_DISPLAY00.Location = new System.Drawing.Point(14, 84);
            this.MM_DISPLAY00.Name = "MM_DISPLAY00";
            this.MM_DISPLAY00.Resuloution = 1D;
            this.MM_DISPLAY00.Size = new System.Drawing.Size(810, 680);
            this.MM_DISPLAY00.TabIndex = 259;
            this.MM_DISPLAY00.UseCustomCross = false;
            // 
            // RBTN_RCS_POINT01
            // 
            this.RBTN_RCS_POINT01.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT01.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT01.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT01.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT01.Location = new System.Drawing.Point(12, 40);
            this.RBTN_RCS_POINT01.Name = "RBTN_RCS_POINT01";
            this.RBTN_RCS_POINT01.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT01.TabIndex = 260;
            this.RBTN_RCS_POINT01.Text = "POINT1";
            this.RBTN_RCS_POINT01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT01.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT01.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT01.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // RBTN_RCS_POINT02
            // 
            this.RBTN_RCS_POINT02.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT02.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT02.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT02.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT02.Location = new System.Drawing.Point(114, 40);
            this.RBTN_RCS_POINT02.Name = "RBTN_RCS_POINT02";
            this.RBTN_RCS_POINT02.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT02.TabIndex = 261;
            this.RBTN_RCS_POINT02.Text = "POINT2";
            this.RBTN_RCS_POINT02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT02.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT02.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT02.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // RBTN_RCS_POINT03
            // 
            this.RBTN_RCS_POINT03.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT03.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT03.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT03.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT03.Location = new System.Drawing.Point(216, 40);
            this.RBTN_RCS_POINT03.Name = "RBTN_RCS_POINT03";
            this.RBTN_RCS_POINT03.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT03.TabIndex = 262;
            this.RBTN_RCS_POINT03.Text = "POINT3";
            this.RBTN_RCS_POINT03.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT03.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT03.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT03.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // RBTN_RCS_POINT04
            // 
            this.RBTN_RCS_POINT04.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT04.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT04.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT04.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT04.Location = new System.Drawing.Point(318, 40);
            this.RBTN_RCS_POINT04.Name = "RBTN_RCS_POINT04";
            this.RBTN_RCS_POINT04.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT04.TabIndex = 263;
            this.RBTN_RCS_POINT04.Text = "POINT4";
            this.RBTN_RCS_POINT04.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT04.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT04.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT04.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // RBTN_RCS_POINT05
            // 
            this.RBTN_RCS_POINT05.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT05.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT05.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT05.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT05.Location = new System.Drawing.Point(420, 40);
            this.RBTN_RCS_POINT05.Name = "RBTN_RCS_POINT05";
            this.RBTN_RCS_POINT05.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT05.TabIndex = 264;
            this.RBTN_RCS_POINT05.Text = "POINT5";
            this.RBTN_RCS_POINT05.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT05.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT05.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT05.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // RBTN_RCS_POINT06
            // 
            this.RBTN_RCS_POINT06.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT06.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT06.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT06.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT06.Location = new System.Drawing.Point(522, 40);
            this.RBTN_RCS_POINT06.Name = "RBTN_RCS_POINT06";
            this.RBTN_RCS_POINT06.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT06.TabIndex = 265;
            this.RBTN_RCS_POINT06.Text = "POINT6";
            this.RBTN_RCS_POINT06.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT06.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT06.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT06.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // RBTN_RCS_POINT07
            // 
            this.RBTN_RCS_POINT07.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT07.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT07.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT07.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT07.Location = new System.Drawing.Point(624, 40);
            this.RBTN_RCS_POINT07.Name = "RBTN_RCS_POINT07";
            this.RBTN_RCS_POINT07.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT07.TabIndex = 266;
            this.RBTN_RCS_POINT07.Text = "POINT7";
            this.RBTN_RCS_POINT07.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT07.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT07.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT07.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // RBTN_RCS_POINT08
            // 
            this.RBTN_RCS_POINT08.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_RCS_POINT08.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_RCS_POINT08.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_RCS_POINT08.ForeColor = System.Drawing.Color.Black;
            this.RBTN_RCS_POINT08.Location = new System.Drawing.Point(726, 40);
            this.RBTN_RCS_POINT08.Name = "RBTN_RCS_POINT08";
            this.RBTN_RCS_POINT08.Size = new System.Drawing.Size(100, 40);
            this.RBTN_RCS_POINT08.TabIndex = 267;
            this.RBTN_RCS_POINT08.Text = "POINT8";
            this.RBTN_RCS_POINT08.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_RCS_POINT08.UseVisualStyleBackColor = false;
            this.RBTN_RCS_POINT08.CheckedChanged += new System.EventHandler(this.RBTN_RCS_POINT_CheckedChanged);
            this.RBTN_RCS_POINT08.Click += new System.EventHandler(this.RBTN_RCS_POINT_Click);
            // 
            // CB_DIRECTION
            // 
            this.CB_DIRECTION.DisplayMember = "1";
            this.CB_DIRECTION.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_DIRECTION.FormattingEnabled = true;
            this.CB_DIRECTION.Items.AddRange(new object[] {
            "PANEL_ID_0",
            "PANEL_ID_1"});
            this.CB_DIRECTION.Location = new System.Drawing.Point(15, 3);
            this.CB_DIRECTION.Name = "CB_DIRECTION";
            this.CB_DIRECTION.Size = new System.Drawing.Size(269, 33);
            this.CB_DIRECTION.TabIndex = 268;
            // 
            // BTN_RCS_SCALE_BAR
            // 
            this.BTN_RCS_SCALE_BAR.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RCS_SCALE_BAR.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RCS_SCALE_BAR.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RCS_SCALE_BAR.Location = new System.Drawing.Point(14, 770);
            this.BTN_RCS_SCALE_BAR.Name = "BTN_RCS_SCALE_BAR";
            this.BTN_RCS_SCALE_BAR.Size = new System.Drawing.Size(180, 36);
            this.BTN_RCS_SCALE_BAR.TabIndex = 269;
            this.BTN_RCS_SCALE_BAR.Text = "판정 스케일 바";
            this.BTN_RCS_SCALE_BAR.UseVisualStyleBackColor = false;
            this.BTN_RCS_SCALE_BAR.Click += new System.EventHandler(this.BTN_RCS_SCALE_BAR_Click);
            // 
            // BTN_RCS_POINT_OK
            // 
            this.BTN_RCS_POINT_OK.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RCS_POINT_OK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RCS_POINT_OK.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RCS_POINT_OK.Location = new System.Drawing.Point(644, 770);
            this.BTN_RCS_POINT_OK.Name = "BTN_RCS_POINT_OK";
            this.BTN_RCS_POINT_OK.Size = new System.Drawing.Size(180, 36);
            this.BTN_RCS_POINT_OK.TabIndex = 270;
            this.BTN_RCS_POINT_OK.Text = "POINT OK";
            this.BTN_RCS_POINT_OK.UseVisualStyleBackColor = false;
            this.BTN_RCS_POINT_OK.Click += new System.EventHandler(this.BTN_RCS_POINT_OK_Click);
            // 
            // LB_SCALE
            // 
            this.LB_SCALE.BackColor = System.Drawing.Color.White;
            this.LB_SCALE.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_SCALE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LB_SCALE.Location = new System.Drawing.Point(200, 771);
            this.LB_SCALE.Name = "LB_SCALE";
            this.LB_SCALE.Size = new System.Drawing.Size(100, 34);
            this.LB_SCALE.TabIndex = 271;
            this.LB_SCALE.Text = "0";
            this.LB_SCALE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_SCALE.Click += new System.EventHandler(this.LB_SCALE_Click);
            // 
            // BTN_RCS_ROTATE_SCALE_BAR
            // 
            this.BTN_RCS_ROTATE_SCALE_BAR.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RCS_ROTATE_SCALE_BAR.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RCS_ROTATE_SCALE_BAR.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RCS_ROTATE_SCALE_BAR.Location = new System.Drawing.Point(310, 772);
            this.BTN_RCS_ROTATE_SCALE_BAR.Name = "BTN_RCS_ROTATE_SCALE_BAR";
            this.BTN_RCS_ROTATE_SCALE_BAR.Size = new System.Drawing.Size(48, 34);
            this.BTN_RCS_ROTATE_SCALE_BAR.TabIndex = 272;
            this.BTN_RCS_ROTATE_SCALE_BAR.Text = "각";
            this.BTN_RCS_ROTATE_SCALE_BAR.UseVisualStyleBackColor = false;
            this.BTN_RCS_ROTATE_SCALE_BAR.Click += new System.EventHandler(this.BTN_RCS_ROTATE_SCALE_BAR_Click);
            // 
            // BTN_RCS_COLOR_SCALE_BAR
            // 
            this.BTN_RCS_COLOR_SCALE_BAR.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RCS_COLOR_SCALE_BAR.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RCS_COLOR_SCALE_BAR.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RCS_COLOR_SCALE_BAR.Location = new System.Drawing.Point(364, 772);
            this.BTN_RCS_COLOR_SCALE_BAR.Name = "BTN_RCS_COLOR_SCALE_BAR";
            this.BTN_RCS_COLOR_SCALE_BAR.Size = new System.Drawing.Size(48, 34);
            this.BTN_RCS_COLOR_SCALE_BAR.TabIndex = 273;
            this.BTN_RCS_COLOR_SCALE_BAR.Text = "색";
            this.BTN_RCS_COLOR_SCALE_BAR.UseVisualStyleBackColor = false;
            this.BTN_RCS_COLOR_SCALE_BAR.Click += new System.EventHandler(this.BTN_RCS_COLOR_SCALE_BAR_Click);
            // 
            // Form_RCS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1132, 819);
            this.ControlBox = false;
            this.Controls.Add(this.BTN_RCS_COLOR_SCALE_BAR);
            this.Controls.Add(this.BTN_RCS_ROTATE_SCALE_BAR);
            this.Controls.Add(this.LB_SCALE);
            this.Controls.Add(this.BTN_RCS_POINT_OK);
            this.Controls.Add(this.BTN_RCS_SCALE_BAR);
            this.Controls.Add(this.CB_DIRECTION);
            this.Controls.Add(this.RBTN_RCS_POINT08);
            this.Controls.Add(this.RBTN_RCS_POINT07);
            this.Controls.Add(this.RBTN_RCS_POINT06);
            this.Controls.Add(this.RBTN_RCS_POINT05);
            this.Controls.Add(this.RBTN_RCS_POINT04);
            this.Controls.Add(this.RBTN_RCS_POINT03);
            this.Controls.Add(this.RBTN_RCS_POINT02);
            this.Controls.Add(this.RBTN_RCS_POINT01);
            this.Controls.Add(this.MM_DISPLAY00);
            this.Controls.Add(this.BTN_RCS_NG);
            this.Controls.Add(this.BTN_RCS_OK);
            this.Controls.Add(this.LB_MARKPOS);
            this.Controls.Add(this.LB_ALIGNUNIT_NAME);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_MOVE_UP);
            this.Controls.Add(this.BTN_MOVE_DOWN);
            this.Controls.Add(this.BTN_MOVE_LEFT);
            this.Controls.Add(this.BTN_MOVE_RIGHT);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_RCS";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_RCS";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_RCS_FormClosed);
            this.Load += new System.EventHandler(this.Form_RCS_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button BTN_MOVE_UP;
        private System.Windows.Forms.Button BTN_MOVE_DOWN;
        private System.Windows.Forms.Button BTN_MOVE_LEFT;
        private System.Windows.Forms.Button BTN_MOVE_RIGHT;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Label LB_ALIGNUNIT_NAME;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label LB_MARKPOS;
        private System.Windows.Forms.Button BTN_RCS_OK;
        private System.Windows.Forms.Button BTN_RCS_NG;
        private JAS.Controls.Display.Display MM_DISPLAY00;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT01;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT02;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT03;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT04;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT05;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT06;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT07;
        private System.Windows.Forms.RadioButton RBTN_RCS_POINT08;
        private System.Windows.Forms.ComboBox CB_DIRECTION;
        private System.Windows.Forms.Button BTN_RCS_SCALE_BAR;
        private System.Windows.Forms.Button BTN_RCS_POINT_OK;
        private System.Windows.Forms.Label LB_SCALE;
        private System.Windows.Forms.Button BTN_RCS_ROTATE_SCALE_BAR;
        private System.Windows.Forms.Button BTN_RCS_COLOR_SCALE_BAR;
    }
}
namespace COG
{
    partial class Form_ManualSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ManualSet));
            this.MM_DisplayStatusBar01 = new Cognex.VisionPro.CogDisplayStatusBarV2();
            this.MM_DisplayToolbar01 = new Cognex.VisionPro.CogDisplayToolbarV2();
            this.LB_ALIGNUNIT_NAME = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.LB_MARKPOS = new System.Windows.Forms.Label();
            this.MM_DISPLAY01 = new Cognex.VisionPro.Display.CogDisplay();
            this.BTN_SAVE = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_MOVE_UP = new System.Windows.Forms.Button();
            this.BTN_MOVE_DOWN = new System.Windows.Forms.Button();
            this.BTN_MOVE_LEFT = new System.Windows.Forms.Button();
            this.BTN_MOVE_RIGHT = new System.Windows.Forms.Button();
            this.BTN_CORNER_RETRY = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY01)).BeginInit();
            this.SuspendLayout();
            // 
            // MM_DisplayStatusBar01
            // 
            this.MM_DisplayStatusBar01.CoordinateSpaceName = "*\\#";
            this.MM_DisplayStatusBar01.CoordinateSpaceName3D = "*\\#";
            this.MM_DisplayStatusBar01.Location = new System.Drawing.Point(4, 739);
            this.MM_DisplayStatusBar01.Name = "MM_DisplayStatusBar01";
            this.MM_DisplayStatusBar01.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MM_DisplayStatusBar01.Size = new System.Drawing.Size(894, 22);
            this.MM_DisplayStatusBar01.TabIndex = 39;
            this.MM_DisplayStatusBar01.Use3DCoordinateSpaceTree = false;
            // 
            // MM_DisplayToolbar01
            // 
            this.MM_DisplayToolbar01.Location = new System.Drawing.Point(4, 6);
            this.MM_DisplayToolbar01.Name = "MM_DisplayToolbar01";
            this.MM_DisplayToolbar01.Size = new System.Drawing.Size(894, 26);
            this.MM_DisplayToolbar01.TabIndex = 38;
            // 
            // LB_ALIGNUNIT_NAME
            // 
            this.LB_ALIGNUNIT_NAME.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_ALIGNUNIT_NAME.Font = new System.Drawing.Font("굴림", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_ALIGNUNIT_NAME.Location = new System.Drawing.Point(920, 27);
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
            this.LB_MARKPOS.Location = new System.Drawing.Point(920, 100);
            this.LB_MARKPOS.Name = "LB_MARKPOS";
            this.LB_MARKPOS.Size = new System.Drawing.Size(274, 61);
            this.LB_MARKPOS.TabIndex = 48;
            this.LB_MARKPOS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MM_DISPLAY01
            // 
            this.MM_DISPLAY01.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY01.ColorMapLowerRoiLimit = 0D;
            this.MM_DISPLAY01.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.MM_DISPLAY01.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY01.ColorMapUpperRoiLimit = 1D;
            this.MM_DISPLAY01.DoubleTapZoomCycleLength = 2;
            this.MM_DISPLAY01.DoubleTapZoomSensitivity = 2.5D;
            this.MM_DISPLAY01.Location = new System.Drawing.Point(4, 38);
            this.MM_DISPLAY01.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.MM_DISPLAY01.MouseWheelSensitivity = 1D;
            this.MM_DISPLAY01.Name = "MM_DISPLAY01";
            this.MM_DISPLAY01.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MM_DISPLAY01.OcxState")));
            this.MM_DISPLAY01.Size = new System.Drawing.Size(894, 695);
            this.MM_DISPLAY01.TabIndex = 49;
            this.MM_DISPLAY01.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MM_DISPLAY01_MouseUp);
            // 
            // BTN_SAVE
            // 
            this.BTN_SAVE.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_SAVE.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_SAVE.BackgroundImage")));
            this.BTN_SAVE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_SAVE.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SAVE.Location = new System.Drawing.Point(965, 652);
            this.BTN_SAVE.Name = "BTN_SAVE";
            this.BTN_SAVE.Size = new System.Drawing.Size(100, 100);
            this.BTN_SAVE.TabIndex = 45;
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
            this.BTN_EXIT.Location = new System.Drawing.Point(1084, 652);
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
            this.BTN_MOVE_UP.Location = new System.Drawing.Point(1023, 187);
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
            this.BTN_MOVE_DOWN.Location = new System.Drawing.Point(1023, 344);
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
            this.BTN_MOVE_LEFT.Location = new System.Drawing.Point(928, 261);
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
            this.BTN_MOVE_RIGHT.Location = new System.Drawing.Point(1115, 261);
            this.BTN_MOVE_RIGHT.Name = "BTN_MOVE_RIGHT";
            this.BTN_MOVE_RIGHT.Size = new System.Drawing.Size(89, 77);
            this.BTN_MOVE_RIGHT.TabIndex = 43;
            this.BTN_MOVE_RIGHT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_MOVE_RIGHT.UseVisualStyleBackColor = false;
            this.BTN_MOVE_RIGHT.Click += new System.EventHandler(this.BTN_MOVE_RIGHT_Click);
            // 
            // BTN_CORNER_RETRY
            // 
            this.BTN_CORNER_RETRY.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_CORNER_RETRY.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_CORNER_RETRY.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_CORNER_RETRY.Image = global::COG.Properties.Resources.Find;
            this.BTN_CORNER_RETRY.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.BTN_CORNER_RETRY.Location = new System.Drawing.Point(965, 555);
            this.BTN_CORNER_RETRY.Name = "BTN_CORNER_RETRY";
            this.BTN_CORNER_RETRY.Size = new System.Drawing.Size(219, 80);
            this.BTN_CORNER_RETRY.TabIndex = 50;
            this.BTN_CORNER_RETRY.Text = "          CORNER RETRY";
            this.BTN_CORNER_RETRY.UseVisualStyleBackColor = false;
            this.BTN_CORNER_RETRY.Visible = false;
            this.BTN_CORNER_RETRY.Click += new System.EventHandler(this.BTN_CORNER_RETRY_Click);
            // 
            // Form_ManualSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1261, 819);
            this.ControlBox = false;
            this.Controls.Add(this.BTN_CORNER_RETRY);
            this.Controls.Add(this.MM_DISPLAY01);
            this.Controls.Add(this.LB_MARKPOS);
            this.Controls.Add(this.LB_ALIGNUNIT_NAME);
            this.Controls.Add(this.BTN_SAVE);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_MOVE_UP);
            this.Controls.Add(this.BTN_MOVE_DOWN);
            this.Controls.Add(this.BTN_MOVE_LEFT);
            this.Controls.Add(this.BTN_MOVE_RIGHT);
            this.Controls.Add(this.MM_DisplayStatusBar01);
            this.Controls.Add(this.MM_DisplayToolbar01);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_ManualSet";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_ManualSet";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_ManualSet_FormClosed);
            this.Load += new System.EventHandler(this.Form_ManualSet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY01)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Cognex.VisionPro.CogDisplayStatusBarV2 MM_DisplayStatusBar01;
        private Cognex.VisionPro.CogDisplayToolbarV2 MM_DisplayToolbar01;
        private System.Windows.Forms.Button BTN_MOVE_UP;
        private System.Windows.Forms.Button BTN_MOVE_DOWN;
        private System.Windows.Forms.Button BTN_MOVE_LEFT;
        private System.Windows.Forms.Button BTN_MOVE_RIGHT;
        private System.Windows.Forms.Button BTN_SAVE;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Label LB_ALIGNUNIT_NAME;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label LB_MARKPOS;
        private Cognex.VisionPro.Display.CogDisplay MM_DISPLAY01;
        private System.Windows.Forms.Button BTN_CORNER_RETRY;
    }
}
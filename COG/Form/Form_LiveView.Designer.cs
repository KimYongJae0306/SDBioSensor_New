namespace COG
{
    partial class Form_LiveView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_LiveView));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_DISNAME_00 = new System.Windows.Forms.Button();
            this.LB_DIST_Y = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.LB_DIST_X = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LB_ANGLE00 = new System.Windows.Forms.Label();
            this.LB_DISTANCE00 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.MM_DisplayToolbar00 = new Cognex.VisionPro.CogDisplayToolbarV2();
            this.MM_DisplayStatusBar00 = new Cognex.VisionPro.CogDisplayStatusBarV2();
            this.MM_DISPLAY00 = new JAS.Controls.Display.Display();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(1101, 820);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 52;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_DISNAME_00
            // 
            this.BTN_DISNAME_00.BackColor = System.Drawing.Color.SkyBlue;
            this.BTN_DISNAME_00.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_DISNAME_00.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_DISNAME_00.Location = new System.Drawing.Point(-1, -1);
            this.BTN_DISNAME_00.Name = "BTN_DISNAME_00";
            this.BTN_DISNAME_00.Size = new System.Drawing.Size(1100, 35);
            this.BTN_DISNAME_00.TabIndex = 53;
            this.BTN_DISNAME_00.Text = "LIVE MODE";
            this.BTN_DISNAME_00.UseVisualStyleBackColor = false;
            // 
            // LB_DIST_Y
            // 
            this.LB_DIST_Y.BackColor = System.Drawing.Color.White;
            this.LB_DIST_Y.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_DIST_Y.ForeColor = System.Drawing.Color.Black;
            this.LB_DIST_Y.Location = new System.Drawing.Point(783, 167);
            this.LB_DIST_Y.Name = "LB_DIST_Y";
            this.LB_DIST_Y.Size = new System.Drawing.Size(141, 30);
            this.LB_DIST_Y.TabIndex = 111;
            this.LB_DIST_Y.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.Location = new System.Drawing.Point(750, 167);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(237, 30);
            this.label5.TabIndex = 110;
            this.label5.Text = "Y                        (um)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LB_DIST_X
            // 
            this.LB_DIST_X.BackColor = System.Drawing.Color.White;
            this.LB_DIST_X.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_DIST_X.ForeColor = System.Drawing.Color.Black;
            this.LB_DIST_X.Location = new System.Drawing.Point(783, 130);
            this.LB_DIST_X.Name = "LB_DIST_X";
            this.LB_DIST_X.Size = new System.Drawing.Size(141, 30);
            this.LB_DIST_X.TabIndex = 109;
            this.LB_DIST_X.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(750, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(237, 30);
            this.label3.TabIndex = 108;
            this.label3.Text = "X                        (um)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(965, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 30);
            this.label1.TabIndex = 107;
            this.label1.Text = "(Deg)";
            // 
            // LB_ANGLE00
            // 
            this.LB_ANGLE00.BackColor = System.Drawing.Color.White;
            this.LB_ANGLE00.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_ANGLE00.ForeColor = System.Drawing.Color.Black;
            this.LB_ANGLE00.Location = new System.Drawing.Point(824, 51);
            this.LB_ANGLE00.Name = "LB_ANGLE00";
            this.LB_ANGLE00.Size = new System.Drawing.Size(141, 30);
            this.LB_ANGLE00.TabIndex = 106;
            this.LB_ANGLE00.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_DISTANCE00
            // 
            this.LB_DISTANCE00.BackColor = System.Drawing.Color.White;
            this.LB_DISTANCE00.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_DISTANCE00.ForeColor = System.Drawing.Color.Black;
            this.LB_DISTANCE00.Location = new System.Drawing.Point(596, 51);
            this.LB_DISTANCE00.Name = "LB_DISTANCE00";
            this.LB_DISTANCE00.Size = new System.Drawing.Size(141, 30);
            this.LB_DISTANCE00.TabIndex = 105;
            this.LB_DISTANCE00.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label9.Location = new System.Drawing.Point(738, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 30);
            this.label9.TabIndex = 104;
            this.label9.Text = "(um)";
            // 
            // MM_DisplayToolbar00
            // 
            this.MM_DisplayToolbar00.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.MM_DisplayToolbar00.Location = new System.Drawing.Point(4, 83);
            this.MM_DisplayToolbar00.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MM_DisplayToolbar00.Name = "MM_DisplayToolbar00";
            this.MM_DisplayToolbar00.Size = new System.Drawing.Size(1040, 26);
            this.MM_DisplayToolbar00.TabIndex = 103;
            // 
            // MM_DisplayStatusBar00
            // 
            this.MM_DisplayStatusBar00.CoordinateSpaceName = "*\\#";
            this.MM_DisplayStatusBar00.CoordinateSpaceName3D = "*\\#";
            this.MM_DisplayStatusBar00.Location = new System.Drawing.Point(12, 653);
            this.MM_DisplayStatusBar00.Name = "MM_DisplayStatusBar00";
            this.MM_DisplayStatusBar00.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MM_DisplayStatusBar00.Size = new System.Drawing.Size(894, 22);
            this.MM_DisplayStatusBar00.TabIndex = 112;
            this.MM_DisplayStatusBar00.Use3DCoordinateSpaceTree = false;
            // 
            // MM_DISPLAY00
            // 
            this.MM_DISPLAY00.BackColor = System.Drawing.Color.DarkGray;
            this.MM_DISPLAY00.DisplayManuConstants = ((JAS.Controls.Display.DisplayEnableConstants)((((((((((JAS.Controls.Display.DisplayEnableConstants.DisplayFit | JAS.Controls.Display.DisplayEnableConstants.Undo) 
            | JAS.Controls.Display.DisplayEnableConstants.Delete) 
            | JAS.Controls.Display.DisplayEnableConstants.TouchMove0) 
            | JAS.Controls.Display.DisplayEnableConstants.PointToPoint1) 
            | JAS.Controls.Display.DisplayEnableConstants.LineToLine2) 
            | JAS.Controls.Display.DisplayEnableConstants.Arc3Points) 
            | JAS.Controls.Display.DisplayEnableConstants.Square5) 
            | JAS.Controls.Display.DisplayEnableConstants.CrossLine6)
            | JAS.Controls.Display.DisplayEnableConstants.CrossCustom)));
            this.MM_DISPLAY00.Image = null;
            this.MM_DISPLAY00.Location = new System.Drawing.Point(0, 34);
            this.MM_DISPLAY00.Name = "MM_DISPLAY00";
            this.MM_DISPLAY00.Resuloution = 1D;
            this.MM_DISPLAY00.Size = new System.Drawing.Size(1100, 925);
            this.MM_DISPLAY00.TabIndex = 113;
            // 
            // Form_LiveView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1204, 957);
            this.ControlBox = false;
            this.Controls.Add(this.MM_DISPLAY00);
            this.Controls.Add(this.MM_DisplayStatusBar00);
            this.Controls.Add(this.LB_DIST_Y);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.LB_DIST_X);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LB_ANGLE00);
            this.Controls.Add(this.LB_DISTANCE00);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.MM_DisplayToolbar00);
            this.Controls.Add(this.BTN_DISNAME_00);
            this.Controls.Add(this.BTN_EXIT);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form_LiveView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_LiveView";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_LiveView_FormClosed);
            this.Load += new System.EventHandler(this.Form_LiveView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button BTN_DISNAME_00;
        private System.Windows.Forms.Label LB_DIST_Y;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label LB_DIST_X;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label LB_ANGLE00;
        private System.Windows.Forms.Label LB_DISTANCE00;
        private System.Windows.Forms.Label label9;
        private Cognex.VisionPro.CogDisplayToolbarV2 MM_DisplayToolbar00;
        private Cognex.VisionPro.CogDisplayStatusBarV2 MM_DisplayStatusBar00;
        private JAS.Controls.Display.Display MM_DISPLAY00;
    }
}
namespace COG
{
    partial class Form_NGMonitor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_NGMonitor));
            this.LB_ALIGNUNIT_NAME = new System.Windows.Forms.Label();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.Panel_1Cam = new System.Windows.Forms.Panel();
            this.MM_StatusBar_1Cam = new Cognex.VisionPro.CogDisplayStatusBarV2();
            this.MM_ToolBar_1Cam = new Cognex.VisionPro.CogDisplayToolbarV2();
            this.MM_DISPLAY_1CAM1 = new Cognex.VisionPro.CogRecordDisplay();
            this.Panel_2Cam = new System.Windows.Forms.Panel();
            this.MM_ToolBar_2Cam2 = new Cognex.VisionPro.CogDisplayToolbarV2();
            this.MM_StatusBar_2Cam2 = new Cognex.VisionPro.CogDisplayStatusBarV2();
            this.MM_StatusBar_2Cam1 = new Cognex.VisionPro.CogDisplayStatusBarV2();
            this.MM_DISPLAY_2CAM2 = new Cognex.VisionPro.CogRecordDisplay();
            this.cogDisplayStatusBarV22 = new Cognex.VisionPro.CogDisplayStatusBarV2();
            this.MM_ToolBar_2Cam1 = new Cognex.VisionPro.CogDisplayToolbarV2();
            this.MM_DISPLAY_2CAM1 = new Cognex.VisionPro.CogRecordDisplay();
            this.Panel_1Cam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY_1CAM1)).BeginInit();
            this.Panel_2Cam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY_2CAM2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY_2CAM1)).BeginInit();
            this.SuspendLayout();
            // 
            // LB_ALIGNUNIT_NAME
            // 
            this.LB_ALIGNUNIT_NAME.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_ALIGNUNIT_NAME.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_ALIGNUNIT_NAME.Location = new System.Drawing.Point(1054, 15);
            this.LB_ALIGNUNIT_NAME.Name = "LB_ALIGNUNIT_NAME";
            this.LB_ALIGNUNIT_NAME.Size = new System.Drawing.Size(199, 61);
            this.LB_ALIGNUNIT_NAME.TabIndex = 3;
            this.LB_ALIGNUNIT_NAME.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(1091, 591);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 45;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // Panel_1Cam
            // 
            this.Panel_1Cam.Controls.Add(this.MM_StatusBar_1Cam);
            this.Panel_1Cam.Controls.Add(this.MM_ToolBar_1Cam);
            this.Panel_1Cam.Controls.Add(this.MM_DISPLAY_1CAM1);
            this.Panel_1Cam.Location = new System.Drawing.Point(9, 12);
            this.Panel_1Cam.Name = "Panel_1Cam";
            this.Panel_1Cam.Size = new System.Drawing.Size(1038, 685);
            this.Panel_1Cam.TabIndex = 46;
            // 
            // MM_StatusBar_1Cam
            // 
            this.MM_StatusBar_1Cam.CoordinateSpaceName = "*\\#";
            this.MM_StatusBar_1Cam.Location = new System.Drawing.Point(3, 660);
            this.MM_StatusBar_1Cam.Name = "MM_StatusBar_1Cam";
            this.MM_StatusBar_1Cam.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MM_StatusBar_1Cam.Size = new System.Drawing.Size(1036, 19);
            this.MM_StatusBar_1Cam.TabIndex = 4;
            // 
            // MM_ToolBar_1Cam
            // 
            this.MM_ToolBar_1Cam.Location = new System.Drawing.Point(0, 3);
            this.MM_ToolBar_1Cam.Name = "MM_ToolBar_1Cam";
            this.MM_ToolBar_1Cam.Size = new System.Drawing.Size(1039, 26);
            this.MM_ToolBar_1Cam.TabIndex = 3;
            // 
            // MM_DISPLAY_1CAM1
            // 
            this.MM_DISPLAY_1CAM1.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY_1CAM1.ColorMapLowerRoiLimit = 0D;
            this.MM_DISPLAY_1CAM1.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.MM_DISPLAY_1CAM1.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY_1CAM1.ColorMapUpperRoiLimit = 1D;
            this.MM_DISPLAY_1CAM1.Location = new System.Drawing.Point(3, 27);
            this.MM_DISPLAY_1CAM1.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.MM_DISPLAY_1CAM1.MouseWheelSensitivity = 1D;
            this.MM_DISPLAY_1CAM1.Name = "MM_DISPLAY_1CAM1";
            this.MM_DISPLAY_1CAM1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MM_DISPLAY_1CAM1.OcxState")));
            this.MM_DISPLAY_1CAM1.Size = new System.Drawing.Size(1036, 627);
            this.MM_DISPLAY_1CAM1.TabIndex = 1;
            // 
            // Panel_2Cam
            // 
            this.Panel_2Cam.Controls.Add(this.MM_ToolBar_2Cam2);
            this.Panel_2Cam.Controls.Add(this.MM_StatusBar_2Cam2);
            this.Panel_2Cam.Controls.Add(this.MM_StatusBar_2Cam1);
            this.Panel_2Cam.Controls.Add(this.MM_DISPLAY_2CAM2);
            this.Panel_2Cam.Controls.Add(this.cogDisplayStatusBarV22);
            this.Panel_2Cam.Controls.Add(this.MM_ToolBar_2Cam1);
            this.Panel_2Cam.Controls.Add(this.MM_DISPLAY_2CAM1);
            this.Panel_2Cam.Location = new System.Drawing.Point(9, 12);
            this.Panel_2Cam.Name = "Panel_2Cam";
            this.Panel_2Cam.Size = new System.Drawing.Size(1038, 524);
            this.Panel_2Cam.TabIndex = 47;
            // 
            // MM_ToolBar_2Cam2
            // 
            this.MM_ToolBar_2Cam2.Location = new System.Drawing.Point(519, 0);
            this.MM_ToolBar_2Cam2.Name = "MM_ToolBar_2Cam2";
            this.MM_ToolBar_2Cam2.Size = new System.Drawing.Size(516, 26);
            this.MM_ToolBar_2Cam2.TabIndex = 8;
            // 
            // MM_StatusBar_2Cam2
            // 
            this.MM_StatusBar_2Cam2.CoordinateSpaceName = "*\\#";
            this.MM_StatusBar_2Cam2.Location = new System.Drawing.Point(522, 499);
            this.MM_StatusBar_2Cam2.Name = "MM_StatusBar_2Cam2";
            this.MM_StatusBar_2Cam2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MM_StatusBar_2Cam2.Size = new System.Drawing.Size(513, 22);
            this.MM_StatusBar_2Cam2.TabIndex = 7;
            // 
            // MM_StatusBar_2Cam1
            // 
            this.MM_StatusBar_2Cam1.CoordinateSpaceName = "*\\#";
            this.MM_StatusBar_2Cam1.Location = new System.Drawing.Point(3, 499);
            this.MM_StatusBar_2Cam1.Name = "MM_StatusBar_2Cam1";
            this.MM_StatusBar_2Cam1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.MM_StatusBar_2Cam1.Size = new System.Drawing.Size(513, 22);
            this.MM_StatusBar_2Cam1.TabIndex = 6;
            // 
            // MM_DISPLAY_2CAM2
            // 
            this.MM_DISPLAY_2CAM2.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY_2CAM2.ColorMapLowerRoiLimit = 0D;
            this.MM_DISPLAY_2CAM2.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.MM_DISPLAY_2CAM2.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY_2CAM2.ColorMapUpperRoiLimit = 1D;
            this.MM_DISPLAY_2CAM2.Location = new System.Drawing.Point(519, 25);
            this.MM_DISPLAY_2CAM2.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.MM_DISPLAY_2CAM2.MouseWheelSensitivity = 1D;
            this.MM_DISPLAY_2CAM2.Name = "MM_DISPLAY_2CAM2";
            this.MM_DISPLAY_2CAM2.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MM_DISPLAY_2CAM2.OcxState")));
            this.MM_DISPLAY_2CAM2.Size = new System.Drawing.Size(513, 470);
            this.MM_DISPLAY_2CAM2.TabIndex = 5;
            // 
            // cogDisplayStatusBarV22
            // 
            this.cogDisplayStatusBarV22.CoordinateSpaceName = "*\\#";
            this.cogDisplayStatusBarV22.Location = new System.Drawing.Point(3, 688);
            this.cogDisplayStatusBarV22.Name = "cogDisplayStatusBarV22";
            this.cogDisplayStatusBarV22.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cogDisplayStatusBarV22.Size = new System.Drawing.Size(951, 22);
            this.cogDisplayStatusBarV22.TabIndex = 4;
            // 
            // MM_ToolBar_2Cam1
            // 
            this.MM_ToolBar_2Cam1.Location = new System.Drawing.Point(0, 0);
            this.MM_ToolBar_2Cam1.Name = "MM_ToolBar_2Cam1";
            this.MM_ToolBar_2Cam1.Size = new System.Drawing.Size(516, 26);
            this.MM_ToolBar_2Cam1.TabIndex = 3;
            // 
            // MM_DISPLAY_2CAM1
            // 
            this.MM_DISPLAY_2CAM1.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY_2CAM1.ColorMapLowerRoiLimit = 0D;
            this.MM_DISPLAY_2CAM1.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.MM_DISPLAY_2CAM1.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.MM_DISPLAY_2CAM1.ColorMapUpperRoiLimit = 1D;
            this.MM_DISPLAY_2CAM1.Location = new System.Drawing.Point(3, 25);
            this.MM_DISPLAY_2CAM1.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.MM_DISPLAY_2CAM1.MouseWheelSensitivity = 1D;
            this.MM_DISPLAY_2CAM1.Name = "MM_DISPLAY_2CAM1";
            this.MM_DISPLAY_2CAM1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MM_DISPLAY_2CAM1.OcxState")));
            this.MM_DISPLAY_2CAM1.Size = new System.Drawing.Size(513, 470);
            this.MM_DISPLAY_2CAM1.TabIndex = 1;
            // 
            // Form_NGMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1265, 823);
            this.ControlBox = false;
            this.Controls.Add(this.Panel_2Cam);
            this.Controls.Add(this.Panel_1Cam);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.LB_ALIGNUNIT_NAME);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_NGMonitor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_NGMonitor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_NGMonitor_FormClosed);
            this.Load += new System.EventHandler(this.Form_NGMonitor_Load);
            this.Panel_1Cam.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY_1CAM1)).EndInit();
            this.Panel_2Cam.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY_2CAM2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MM_DISPLAY_2CAM1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LB_ALIGNUNIT_NAME;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Panel Panel_1Cam;
        private Cognex.VisionPro.CogDisplayStatusBarV2 MM_StatusBar_1Cam;
        private Cognex.VisionPro.CogDisplayToolbarV2 MM_ToolBar_1Cam;
        private Cognex.VisionPro.CogRecordDisplay MM_DISPLAY_1CAM1;
        private System.Windows.Forms.Panel Panel_2Cam;
        private Cognex.VisionPro.CogDisplayToolbarV2 MM_ToolBar_2Cam2;
        private Cognex.VisionPro.CogDisplayStatusBarV2 MM_StatusBar_2Cam2;
        private Cognex.VisionPro.CogDisplayStatusBarV2 MM_StatusBar_2Cam1;
        private Cognex.VisionPro.CogRecordDisplay MM_DISPLAY_2CAM2;
        private Cognex.VisionPro.CogDisplayStatusBarV2 cogDisplayStatusBarV22;
        private Cognex.VisionPro.CogDisplayToolbarV2 MM_ToolBar_2Cam1;
        private Cognex.VisionPro.CogRecordDisplay MM_DISPLAY_2CAM1;
    }
}
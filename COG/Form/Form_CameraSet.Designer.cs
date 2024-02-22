namespace COG
{
    partial class Form_CameraSet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_CameraSet));
            this.CS_ToolBlockEdit = new Cognex.VisionPro.ToolBlock.CogToolBlockEditV2();
            this.BTN_SAVE = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_RUN = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.CS_ToolBlockEdit)).BeginInit();
            this.SuspendLayout();
            // 
            // CS_ToolBlockEdit
            // 
            this.CS_ToolBlockEdit.AllowDrop = true;
            this.CS_ToolBlockEdit.ContextMenuCustomizer = null;
            this.CS_ToolBlockEdit.Location = new System.Drawing.Point(0, 1);
            this.CS_ToolBlockEdit.MinimumSize = new System.Drawing.Size(489, 0);
            this.CS_ToolBlockEdit.Name = "CS_ToolBlockEdit";
            this.CS_ToolBlockEdit.ShowNodeToolTips = true;
            this.CS_ToolBlockEdit.Size = new System.Drawing.Size(1224, 651);
            this.CS_ToolBlockEdit.SuspendElectricRuns = false;
            this.CS_ToolBlockEdit.TabIndex = 39;
            // 
            // BTN_SAVE
            // 
            this.BTN_SAVE.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_SAVE.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_SAVE.BackgroundImage")));
            this.BTN_SAVE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_SAVE.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SAVE.Location = new System.Drawing.Point(1019, 760);
            this.BTN_SAVE.Name = "BTN_SAVE";
            this.BTN_SAVE.Size = new System.Drawing.Size(100, 100);
            this.BTN_SAVE.TabIndex = 41;
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
            this.BTN_EXIT.Location = new System.Drawing.Point(1121, 760);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 40;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_RUN
            // 
            this.BTN_RUN.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RUN.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RUN.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RUN.Image = ((System.Drawing.Image)(resources.GetObject("BTN_RUN.Image")));
            this.BTN_RUN.Location = new System.Drawing.Point(825, 760);
            this.BTN_RUN.Name = "BTN_RUN";
            this.BTN_RUN.Size = new System.Drawing.Size(100, 100);
            this.BTN_RUN.TabIndex = 42;
            this.BTN_RUN.Text = "RUN";
            this.BTN_RUN.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_RUN.UseVisualStyleBackColor = false;
            this.BTN_RUN.Click += new System.EventHandler(this.BTN_RUN_Click);
            // 
            // Form_CameraSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1264, 862);
            this.ControlBox = false;
            this.Controls.Add(this.BTN_RUN);
            this.Controls.Add(this.BTN_SAVE);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.CS_ToolBlockEdit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form_CameraSet";
            this.Text = "Form_CameraSet";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_CameraSet_FormClosed);
            this.Load += new System.EventHandler(this.Form_CameraSet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.CS_ToolBlockEdit)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Cognex.VisionPro.ToolBlock.CogToolBlockEditV2 CS_ToolBlockEdit;
        private System.Windows.Forms.Button BTN_SAVE;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Button BTN_RUN;
    }
}
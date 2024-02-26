namespace COG.UI.Forms
{
    partial class PatternMaskForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PatternMaskForm));
            this.MaskEdit = new Cognex.VisionPro.CogImageMaskEditV2();
            this.BTN_SAVE = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_MASK_CLEAR = new System.Windows.Forms.Button();
            this.LB_TOOLBLOCKEDIT_HIDE = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MaskEdit
            // 
            this.MaskEdit.Location = new System.Drawing.Point(12, 12);
            this.MaskEdit.Name = "MaskEdit";
            this.MaskEdit.Size = new System.Drawing.Size(768, 673);
            this.MaskEdit.TabIndex = 62;
            // 
            // BTN_SAVE
            // 
            this.BTN_SAVE.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_SAVE.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_SAVE.BackgroundImage")));
            this.BTN_SAVE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_SAVE.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SAVE.Location = new System.Drawing.Point(797, 693);
            this.BTN_SAVE.Name = "BTN_SAVE";
            this.BTN_SAVE.Size = new System.Drawing.Size(100, 100);
            this.BTN_SAVE.TabIndex = 64;
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
            this.BTN_EXIT.Location = new System.Drawing.Point(915, 693);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 63;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_MASK_CLEAR
            // 
            this.BTN_MASK_CLEAR.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_MASK_CLEAR.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_MASK_CLEAR.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_MASK_CLEAR.Location = new System.Drawing.Point(12, 705);
            this.BTN_MASK_CLEAR.Name = "BTN_MASK_CLEAR";
            this.BTN_MASK_CLEAR.Size = new System.Drawing.Size(96, 88);
            this.BTN_MASK_CLEAR.TabIndex = 65;
            this.BTN_MASK_CLEAR.Text = "CLEAR";
            this.BTN_MASK_CLEAR.UseVisualStyleBackColor = false;
            this.BTN_MASK_CLEAR.Click += new System.EventHandler(this.BTN_MASK_CLEAR_Click);
            // 
            // LB_TOOLBLOCKEDIT_HIDE
            // 
            this.LB_TOOLBLOCKEDIT_HIDE.BackColor = System.Drawing.SystemColors.Control;
            this.LB_TOOLBLOCKEDIT_HIDE.Location = new System.Drawing.Point(12, 9);
            this.LB_TOOLBLOCKEDIT_HIDE.Name = "LB_TOOLBLOCKEDIT_HIDE";
            this.LB_TOOLBLOCKEDIT_HIDE.Size = new System.Drawing.Size(154, 24);
            this.LB_TOOLBLOCKEDIT_HIDE.TabIndex = 66;
            // 
            // Form_PatternMask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 805);
            this.ControlBox = false;
            this.Controls.Add(this.LB_TOOLBLOCKEDIT_HIDE);
            this.Controls.Add(this.BTN_MASK_CLEAR);
            this.Controls.Add(this.BTN_SAVE);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.MaskEdit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form_PatternMask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_PatternMask";
            this.Load += new System.EventHandler(this.Form_PatternMask_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Cognex.VisionPro.CogImageMaskEditV2 MaskEdit;
        private System.Windows.Forms.Button BTN_SAVE;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Button BTN_MASK_CLEAR;
        private System.Windows.Forms.Label LB_TOOLBLOCKEDIT_HIDE;
    }
}
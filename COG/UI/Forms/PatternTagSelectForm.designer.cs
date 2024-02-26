namespace COG.UI.Forms
{
    partial class PatternTagSelectForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PatternTagSelectForm));
            this.BTN_PATTERN_TAG_00 = new System.Windows.Forms.Button();
            this.BTN_PATTERN_TAG_01 = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BTN_PATTERN_TAG_00
            // 
            this.BTN_PATTERN_TAG_00.BackColor = System.Drawing.Color.Silver;
            this.BTN_PATTERN_TAG_00.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_PATTERN_TAG_00.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_PATTERN_TAG_00.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BTN_PATTERN_TAG_00.Location = new System.Drawing.Point(712, 80);
            this.BTN_PATTERN_TAG_00.Name = "BTN_PATTERN_TAG_00";
            this.BTN_PATTERN_TAG_00.Size = new System.Drawing.Size(374, 102);
            this.BTN_PATTERN_TAG_00.TabIndex = 55;
            this.BTN_PATTERN_TAG_00.Text = "STAGE 0";
            this.BTN_PATTERN_TAG_00.UseVisualStyleBackColor = false;
            this.BTN_PATTERN_TAG_00.Visible = false;
            this.BTN_PATTERN_TAG_00.Click += new System.EventHandler(this.BTN_PATTERN_TAG_00_Click);
            // 
            // BTN_PATTERN_TAG_01
            // 
            this.BTN_PATTERN_TAG_01.BackColor = System.Drawing.Color.Silver;
            this.BTN_PATTERN_TAG_01.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_PATTERN_TAG_01.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_PATTERN_TAG_01.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.BTN_PATTERN_TAG_01.Location = new System.Drawing.Point(712, 222);
            this.BTN_PATTERN_TAG_01.Name = "BTN_PATTERN_TAG_01";
            this.BTN_PATTERN_TAG_01.Size = new System.Drawing.Size(374, 102);
            this.BTN_PATTERN_TAG_01.TabIndex = 56;
            this.BTN_PATTERN_TAG_01.Text = "STAGE 1";
            this.BTN_PATTERN_TAG_01.UseVisualStyleBackColor = false;
            this.BTN_PATTERN_TAG_01.Visible = false;
            this.BTN_PATTERN_TAG_01.Click += new System.EventHandler(this.BTN_PATTERN_TAG_01_Click);
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(1177, 667);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(91, 99);
            this.BTN_EXIT.TabIndex = 59;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // PatternTagSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(1904, 1042);
            this.ControlBox = false;
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_PATTERN_TAG_01);
            this.Controls.Add(this.BTN_PATTERN_TAG_00);
            this.Name = "PatternTagSelectForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_PatternTagSelect";
            this.Load += new System.EventHandler(this.Form_PatternTagSelect_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BTN_PATTERN_TAG_00;
        private System.Windows.Forms.Button BTN_PATTERN_TAG_01;
        private System.Windows.Forms.Button BTN_EXIT;
    }
}
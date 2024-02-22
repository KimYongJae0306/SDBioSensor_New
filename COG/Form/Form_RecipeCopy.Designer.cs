namespace COG
{
    partial class Form_RecipeCopy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_RecipeCopy));
            this.BTN_Select_Stage1 = new System.Windows.Forms.RadioButton();
            this.BTN_Select_Stage2 = new System.Windows.Forms.RadioButton();
            this.BTN_Select_inspection = new System.Windows.Forms.RadioButton();
            this.BTN_Select_AlignInsp = new System.Windows.Forms.RadioButton();
            this.BTN_SAVE = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BTN_Select_Stage1
            // 
            this.BTN_Select_Stage1.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Select_Stage1.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_Select_Stage1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_Select_Stage1.ForeColor = System.Drawing.Color.Black;
            this.BTN_Select_Stage1.Location = new System.Drawing.Point(23, 31);
            this.BTN_Select_Stage1.Name = "BTN_Select_Stage1";
            this.BTN_Select_Stage1.Size = new System.Drawing.Size(268, 71);
            this.BTN_Select_Stage1.TabIndex = 2;
            this.BTN_Select_Stage1.Tag = "0";
            this.BTN_Select_Stage1.Text = "STAGE 1";
            this.BTN_Select_Stage1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BTN_Select_Stage1.UseVisualStyleBackColor = false;
            this.BTN_Select_Stage1.Click += new System.EventHandler(this.SelectStage);
            // 
            // BTN_Select_Stage2
            // 
            this.BTN_Select_Stage2.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Select_Stage2.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_Select_Stage2.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_Select_Stage2.ForeColor = System.Drawing.Color.Black;
            this.BTN_Select_Stage2.Location = new System.Drawing.Point(336, 31);
            this.BTN_Select_Stage2.Name = "BTN_Select_Stage2";
            this.BTN_Select_Stage2.Size = new System.Drawing.Size(268, 71);
            this.BTN_Select_Stage2.TabIndex = 3;
            this.BTN_Select_Stage2.Tag = "1";
            this.BTN_Select_Stage2.Text = "STAGE 2";
            this.BTN_Select_Stage2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BTN_Select_Stage2.UseVisualStyleBackColor = false;
            this.BTN_Select_Stage2.Click += new System.EventHandler(this.SelectStage);
            // 
            // BTN_Select_inspection
            // 
            this.BTN_Select_inspection.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Select_inspection.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_Select_inspection.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_Select_inspection.ForeColor = System.Drawing.Color.Black;
            this.BTN_Select_inspection.Location = new System.Drawing.Point(23, 128);
            this.BTN_Select_inspection.Name = "BTN_Select_inspection";
            this.BTN_Select_inspection.Size = new System.Drawing.Size(268, 71);
            this.BTN_Select_inspection.TabIndex = 4;
            this.BTN_Select_inspection.Tag = "0";
            this.BTN_Select_inspection.Text = "INSPECTION";
            this.BTN_Select_inspection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BTN_Select_inspection.UseVisualStyleBackColor = false;
            this.BTN_Select_inspection.Click += new System.EventHandler(this.SelectInsp);
            // 
            // BTN_Select_AlignInsp
            // 
            this.BTN_Select_AlignInsp.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Select_AlignInsp.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_Select_AlignInsp.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_Select_AlignInsp.ForeColor = System.Drawing.Color.Black;
            this.BTN_Select_AlignInsp.Location = new System.Drawing.Point(336, 128);
            this.BTN_Select_AlignInsp.Name = "BTN_Select_AlignInsp";
            this.BTN_Select_AlignInsp.Size = new System.Drawing.Size(268, 71);
            this.BTN_Select_AlignInsp.TabIndex = 5;
            this.BTN_Select_AlignInsp.Tag = "1";
            this.BTN_Select_AlignInsp.Text = "ALIGN INSPECITON";
            this.BTN_Select_AlignInsp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BTN_Select_AlignInsp.UseVisualStyleBackColor = false;
            this.BTN_Select_AlignInsp.Click += new System.EventHandler(this.SelectInsp);
            // 
            // BTN_SAVE
            // 
            this.BTN_SAVE.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_SAVE.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_SAVE.BackgroundImage")));
            this.BTN_SAVE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_SAVE.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SAVE.Location = new System.Drawing.Point(386, 231);
            this.BTN_SAVE.Name = "BTN_SAVE";
            this.BTN_SAVE.Size = new System.Drawing.Size(100, 100);
            this.BTN_SAVE.TabIndex = 16;
            this.BTN_SAVE.Text = "COPY";
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
            this.BTN_EXIT.Location = new System.Drawing.Point(502, 229);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 15;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // Form_RecipeCopy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(625, 343);
            this.Controls.Add(this.BTN_SAVE);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_Select_AlignInsp);
            this.Controls.Add(this.BTN_Select_inspection);
            this.Controls.Add(this.BTN_Select_Stage2);
            this.Controls.Add(this.BTN_Select_Stage1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form_RecipeCopy";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton BTN_Select_Stage1;
        private System.Windows.Forms.RadioButton BTN_Select_Stage2;
        private System.Windows.Forms.RadioButton BTN_Select_inspection;
        private System.Windows.Forms.RadioButton BTN_Select_AlignInsp;
        private System.Windows.Forms.Button BTN_SAVE;
        private System.Windows.Forms.Button BTN_EXIT;
    }
}
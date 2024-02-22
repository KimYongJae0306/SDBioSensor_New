namespace COG
{
    partial class Form_Password
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Password));
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.LB_CURRENT_PW = new System.Windows.Forms.Label();
            this.LB_NEW_PW = new System.Windows.Forms.Label();
            this.LB_CONFIRM_PW = new System.Windows.Forms.Label();
            this.LB_MESSAGE = new System.Windows.Forms.Label();
            this.LB_MTMODE = new System.Windows.Forms.Label();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_SAVE = new System.Windows.Forms.Button();
            this.RBTN_Mode_0 = new System.Windows.Forms.RadioButton();
            this.RBTN_Mode_1 = new System.Windows.Forms.RadioButton();
            this.RBTN_Mode_2 = new System.Windows.Forms.RadioButton();
            this.CB_PASSWORD = new System.Windows.Forms.CheckBox();
            this.BTN_RUN = new System.Windows.Forms.Button();
            this.LB_PWFORMAT = new System.Windows.Forms.Label();
            this.CB_PW_VIEW = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.DarkGray;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 343);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(178, 45);
            this.label2.TabIndex = 50;
            this.label2.Text = "Confirm Password";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label2.Visible = false;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.DarkGray;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 293);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 45);
            this.label1.TabIndex = 48;
            this.label1.Text = "New Password";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.Visible = false;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.DarkGray;
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(12, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(178, 45);
            this.label5.TabIndex = 46;
            this.label5.Text = "Current Passwod";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_CURRENT_PW
            // 
            this.LB_CURRENT_PW.BackColor = System.Drawing.Color.White;
            this.LB_CURRENT_PW.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_CURRENT_PW.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CURRENT_PW.ForeColor = System.Drawing.Color.Black;
            this.LB_CURRENT_PW.Location = new System.Drawing.Point(196, 123);
            this.LB_CURRENT_PW.Name = "LB_CURRENT_PW";
            this.LB_CURRENT_PW.Size = new System.Drawing.Size(364, 45);
            this.LB_CURRENT_PW.TabIndex = 55;
            this.LB_CURRENT_PW.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_CURRENT_PW.Click += new System.EventHandler(this.LB_INPUT_Click);
            // 
            // LB_NEW_PW
            // 
            this.LB_NEW_PW.BackColor = System.Drawing.Color.White;
            this.LB_NEW_PW.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_NEW_PW.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_NEW_PW.ForeColor = System.Drawing.Color.Black;
            this.LB_NEW_PW.Location = new System.Drawing.Point(196, 293);
            this.LB_NEW_PW.Name = "LB_NEW_PW";
            this.LB_NEW_PW.Size = new System.Drawing.Size(364, 45);
            this.LB_NEW_PW.TabIndex = 56;
            this.LB_NEW_PW.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_NEW_PW.Visible = false;
            this.LB_NEW_PW.Click += new System.EventHandler(this.LB_INPUT_Click);
            // 
            // LB_CONFIRM_PW
            // 
            this.LB_CONFIRM_PW.BackColor = System.Drawing.Color.White;
            this.LB_CONFIRM_PW.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_CONFIRM_PW.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_CONFIRM_PW.ForeColor = System.Drawing.Color.Black;
            this.LB_CONFIRM_PW.Location = new System.Drawing.Point(196, 343);
            this.LB_CONFIRM_PW.Name = "LB_CONFIRM_PW";
            this.LB_CONFIRM_PW.Size = new System.Drawing.Size(364, 45);
            this.LB_CONFIRM_PW.TabIndex = 57;
            this.LB_CONFIRM_PW.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_CONFIRM_PW.Visible = false;
            this.LB_CONFIRM_PW.Click += new System.EventHandler(this.LB_INPUT_Click);
            // 
            // LB_MESSAGE
            // 
            this.LB_MESSAGE.BackColor = System.Drawing.Color.DarkGray;
            this.LB_MESSAGE.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_MESSAGE.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_MESSAGE.ForeColor = System.Drawing.Color.BlueViolet;
            this.LB_MESSAGE.Location = new System.Drawing.Point(196, 12);
            this.LB_MESSAGE.Name = "LB_MESSAGE";
            this.LB_MESSAGE.Size = new System.Drawing.Size(364, 60);
            this.LB_MESSAGE.TabIndex = 58;
            this.LB_MESSAGE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LB_MTMODE
            // 
            this.LB_MTMODE.BackColor = System.Drawing.Color.Lime;
            this.LB_MTMODE.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LB_MTMODE.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_MTMODE.ForeColor = System.Drawing.Color.Black;
            this.LB_MTMODE.Location = new System.Drawing.Point(12, 12);
            this.LB_MTMODE.Name = "LB_MTMODE";
            this.LB_MTMODE.Size = new System.Drawing.Size(178, 60);
            this.LB_MTMODE.TabIndex = 59;
            this.LB_MTMODE.Text = "MT MODE";
            this.LB_MTMODE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_MTMODE.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LB_MTMODE_MouseDoubleClick);
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_EXIT.Location = new System.Drawing.Point(460, 200);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(103, 90);
            this.BTN_EXIT.TabIndex = 54;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_SAVE
            // 
            this.BTN_SAVE.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_SAVE.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_SAVE.BackgroundImage")));
            this.BTN_SAVE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_SAVE.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_SAVE.Location = new System.Drawing.Point(12, 200);
            this.BTN_SAVE.Name = "BTN_SAVE";
            this.BTN_SAVE.Size = new System.Drawing.Size(103, 90);
            this.BTN_SAVE.TabIndex = 52;
            this.BTN_SAVE.Text = "CHANGE PW";
            this.BTN_SAVE.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_SAVE.UseVisualStyleBackColor = false;
            this.BTN_SAVE.Visible = false;
            this.BTN_SAVE.Click += new System.EventHandler(this.BTN_SAVE_Click);
            // 
            // RBTN_Mode_0
            // 
            this.RBTN_Mode_0.BackColor = System.Drawing.Color.LawnGreen;
            this.RBTN_Mode_0.Checked = true;
            this.RBTN_Mode_0.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_Mode_0.ForeColor = System.Drawing.Color.Black;
            this.RBTN_Mode_0.Location = new System.Drawing.Point(12, 75);
            this.RBTN_Mode_0.Name = "RBTN_Mode_0";
            this.RBTN_Mode_0.Size = new System.Drawing.Size(126, 43);
            this.RBTN_Mode_0.TabIndex = 61;
            this.RBTN_Mode_0.TabStop = true;
            this.RBTN_Mode_0.Text = "MT";
            this.RBTN_Mode_0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_Mode_0.UseVisualStyleBackColor = false;
            this.RBTN_Mode_0.CheckedChanged += new System.EventHandler(this.BTN_MODE_CHANGE_Click);
            // 
            // RBTN_Mode_1
            // 
            this.RBTN_Mode_1.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_Mode_1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_Mode_1.ForeColor = System.Drawing.Color.Black;
            this.RBTN_Mode_1.Location = new System.Drawing.Point(144, 75);
            this.RBTN_Mode_1.Name = "RBTN_Mode_1";
            this.RBTN_Mode_1.Size = new System.Drawing.Size(126, 43);
            this.RBTN_Mode_1.TabIndex = 62;
            this.RBTN_Mode_1.Text = "MAKER";
            this.RBTN_Mode_1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_Mode_1.UseVisualStyleBackColor = false;
            this.RBTN_Mode_1.Visible = false;
            this.RBTN_Mode_1.CheckedChanged += new System.EventHandler(this.BTN_MODE_CHANGE_Click);
            // 
            // RBTN_Mode_2
            // 
            this.RBTN_Mode_2.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_Mode_2.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_Mode_2.ForeColor = System.Drawing.Color.Black;
            this.RBTN_Mode_2.Location = new System.Drawing.Point(276, 75);
            this.RBTN_Mode_2.Name = "RBTN_Mode_2";
            this.RBTN_Mode_2.Size = new System.Drawing.Size(126, 43);
            this.RBTN_Mode_2.TabIndex = 63;
            this.RBTN_Mode_2.Text = "JINUK";
            this.RBTN_Mode_2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_Mode_2.UseVisualStyleBackColor = false;
            this.RBTN_Mode_2.Visible = false;
            this.RBTN_Mode_2.CheckedChanged += new System.EventHandler(this.BTN_MODE_CHANGE_Click);
            // 
            // CB_PASSWORD
            // 
            this.CB_PASSWORD.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_PASSWORD.BackColor = System.Drawing.Color.DarkGray;
            this.CB_PASSWORD.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_PASSWORD.Location = new System.Drawing.Point(12, 171);
            this.CB_PASSWORD.Name = "CB_PASSWORD";
            this.CB_PASSWORD.Size = new System.Drawing.Size(178, 26);
            this.CB_PASSWORD.TabIndex = 64;
            this.CB_PASSWORD.Text = "PW CHANGE FORM";
            this.CB_PASSWORD.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CB_PASSWORD.UseVisualStyleBackColor = false;
            this.CB_PASSWORD.CheckedChanged += new System.EventHandler(this.CB_PASSWORD_CheckedChanged);
            // 
            // BTN_RUN
            // 
            this.BTN_RUN.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_RUN.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_RUN.Font = new System.Drawing.Font("맑은 고딕", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RUN.Image = ((System.Drawing.Image)(resources.GetObject("BTN_RUN.Image")));
            this.BTN_RUN.Location = new System.Drawing.Point(354, 200);
            this.BTN_RUN.Name = "BTN_RUN";
            this.BTN_RUN.Size = new System.Drawing.Size(103, 90);
            this.BTN_RUN.TabIndex = 65;
            this.BTN_RUN.Text = "ENTER";
            this.BTN_RUN.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_RUN.UseVisualStyleBackColor = false;
            this.BTN_RUN.Click += new System.EventHandler(this.BTN_RUN_Click);
            // 
            // LB_PWFORMAT
            // 
            this.LB_PWFORMAT.BackColor = System.Drawing.Color.DarkGray;
            this.LB_PWFORMAT.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_PWFORMAT.ForeColor = System.Drawing.Color.White;
            this.LB_PWFORMAT.Location = new System.Drawing.Point(566, 1);
            this.LB_PWFORMAT.Name = "LB_PWFORMAT";
            this.LB_PWFORMAT.Size = new System.Drawing.Size(21, 40);
            this.LB_PWFORMAT.TabIndex = 66;
            this.LB_PWFORMAT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LB_PWFORMAT.DoubleClick += new System.EventHandler(this.LB_PWFORMAT_DoubleClick);
            this.LB_PWFORMAT.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LB_PWFORMAT_MouseClick);
            // 
            // CB_PW_VIEW
            // 
            this.CB_PW_VIEW.Appearance = System.Windows.Forms.Appearance.Button;
            this.CB_PW_VIEW.BackColor = System.Drawing.Color.DarkGray;
            this.CB_PW_VIEW.Enabled = false;
            this.CB_PW_VIEW.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_PW_VIEW.Location = new System.Drawing.Point(196, 171);
            this.CB_PW_VIEW.Name = "CB_PW_VIEW";
            this.CB_PW_VIEW.Size = new System.Drawing.Size(74, 26);
            this.CB_PW_VIEW.TabIndex = 67;
            this.CB_PW_VIEW.Text = "PW VIEW";
            this.CB_PW_VIEW.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CB_PW_VIEW.UseVisualStyleBackColor = false;
            this.CB_PW_VIEW.Visible = false;
            this.CB_PW_VIEW.CheckedChanged += new System.EventHandler(this.CB_PW_VIEW_CheckedChanged);
            // 
            // Form_Password
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(589, 292);
            this.ControlBox = false;
            this.Controls.Add(this.CB_PW_VIEW);
            this.Controls.Add(this.LB_PWFORMAT);
            this.Controls.Add(this.BTN_RUN);
            this.Controls.Add(this.CB_PASSWORD);
            this.Controls.Add(this.RBTN_Mode_2);
            this.Controls.Add(this.RBTN_Mode_1);
            this.Controls.Add(this.RBTN_Mode_0);
            this.Controls.Add(this.LB_MTMODE);
            this.Controls.Add(this.LB_MESSAGE);
            this.Controls.Add(this.LB_CONFIRM_PW);
            this.Controls.Add(this.LB_NEW_PW);
            this.Controls.Add(this.LB_CURRENT_PW);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_SAVE);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Name = "Form_Password";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_Password";
            this.Load += new System.EventHandler(this.Form_Password_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Button BTN_SAVE;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label LB_CURRENT_PW;
        private System.Windows.Forms.Label LB_NEW_PW;
        private System.Windows.Forms.Label LB_CONFIRM_PW;
        private System.Windows.Forms.Label LB_MESSAGE;
        private System.Windows.Forms.Label LB_MTMODE;
        private System.Windows.Forms.RadioButton RBTN_Mode_0;
        private System.Windows.Forms.RadioButton RBTN_Mode_1;
        private System.Windows.Forms.RadioButton RBTN_Mode_2;
        private System.Windows.Forms.CheckBox CB_PASSWORD;
        private System.Windows.Forms.Button BTN_RUN;
        private System.Windows.Forms.Label LB_PWFORMAT;
        private System.Windows.Forms.CheckBox CB_PW_VIEW;
    }
}
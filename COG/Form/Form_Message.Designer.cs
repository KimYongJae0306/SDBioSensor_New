namespace COG
{
    partial class Form_Message
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
            this.LB_MESSAGE = new System.Windows.Forms.Label();
            this.BTN_CLOSE = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // LB_MESSAGE
            // 
            this.LB_MESSAGE.BackColor = System.Drawing.Color.DarkGray;
            this.LB_MESSAGE.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_MESSAGE.Location = new System.Drawing.Point(12, 9);
            this.LB_MESSAGE.Name = "LB_MESSAGE";
            this.LB_MESSAGE.Size = new System.Drawing.Size(507, 108);
            this.LB_MESSAGE.TabIndex = 0;
            this.LB_MESSAGE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BTN_CLOSE
            // 
            this.BTN_CLOSE.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_CLOSE.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_CLOSE.Location = new System.Drawing.Point(145, 127);
            this.BTN_CLOSE.Name = "BTN_CLOSE";
            this.BTN_CLOSE.Size = new System.Drawing.Size(225, 67);
            this.BTN_CLOSE.TabIndex = 1;
            this.BTN_CLOSE.Text = "CLOSE";
            this.BTN_CLOSE.UseVisualStyleBackColor = true;
            this.BTN_CLOSE.Click += new System.EventHandler(this.BTN_CLOSE_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form_Message
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 206);
            this.ControlBox = false;
            this.Controls.Add(this.BTN_CLOSE);
            this.Controls.Add(this.LB_MESSAGE);
            this.Name = "Form_Message";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Message";
            this.Shown += new System.EventHandler(this.Form_Message_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label LB_MESSAGE;
        private System.Windows.Forms.Button BTN_CLOSE;
        private System.Windows.Forms.Timer timer1;
    }
}
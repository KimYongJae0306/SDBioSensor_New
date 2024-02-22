namespace COG
{
    partial class Form_Chart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Chart));
            this.MSChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.BTN_UPDATE = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.RBTN_TAG_01 = new System.Windows.Forms.RadioButton();
            this.RBTN_TAG_00 = new System.Windows.Forms.RadioButton();
            this.RBTN_TAG_02 = new System.Windows.Forms.RadioButton();
            this.RBTN_TAG_03 = new System.Windows.Forms.RadioButton();
            this.CB_OBJ_DATA = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.MSChart)).BeginInit();
            this.SuspendLayout();
            // 
            // MSChart
            // 
            this.MSChart.BackColor = System.Drawing.Color.Ivory;
            this.MSChart.Location = new System.Drawing.Point(12, 12);
            this.MSChart.Name = "MSChart";
            this.MSChart.Size = new System.Drawing.Size(1734, 964);
            this.MSChart.TabIndex = 6;
            this.MSChart.Text = "chart1";
            this.MSChart.Click += new System.EventHandler(this.MSChart_Click);
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(1765, 930);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 41;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_UPDATE
            // 
            this.BTN_UPDATE.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_UPDATE.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_UPDATE.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this.BTN_UPDATE.Image = global::COG.Properties.Resources.Finde48;
            this.BTN_UPDATE.Location = new System.Drawing.Point(1765, 694);
            this.BTN_UPDATE.Name = "BTN_UPDATE";
            this.BTN_UPDATE.Size = new System.Drawing.Size(100, 102);
            this.BTN_UPDATE.TabIndex = 82;
            this.BTN_UPDATE.Text = "UPDATE";
            this.BTN_UPDATE.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_UPDATE.UseVisualStyleBackColor = false;
            this.BTN_UPDATE.Visible = false;
            this.BTN_UPDATE.Click += new System.EventHandler(this.BTN_UPDATE_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.DarkGray;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button1.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this.button1.Image = global::COG.Properties.Resources.Finde48;
            this.button1.Location = new System.Drawing.Point(1765, 822);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 102);
            this.button1.TabIndex = 83;
            this.button1.Text = "CLEAR";
            this.button1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // timer1
            // 
            this.timer1.Interval = 3000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // RBTN_TAG_01
            // 
            this.RBTN_TAG_01.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_TAG_01.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_TAG_01.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_TAG_01.ForeColor = System.Drawing.Color.Black;
            this.RBTN_TAG_01.Location = new System.Drawing.Point(1765, 453);
            this.RBTN_TAG_01.Name = "RBTN_TAG_01";
            this.RBTN_TAG_01.Size = new System.Drawing.Size(116, 43);
            this.RBTN_TAG_01.TabIndex = 85;
            this.RBTN_TAG_01.Text = "STAGE 2";
            this.RBTN_TAG_01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_TAG_01.UseVisualStyleBackColor = false;
            this.RBTN_TAG_01.Visible = false;
            this.RBTN_TAG_01.CheckedChanged += new System.EventHandler(this.BTN_PAT_CHANGE_Click);
            this.RBTN_TAG_01.Click += new System.EventHandler(this.RBTN_PAT_Click);
            // 
            // RBTN_TAG_00
            // 
            this.RBTN_TAG_00.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_TAG_00.BackColor = System.Drawing.Color.LawnGreen;
            this.RBTN_TAG_00.Checked = true;
            this.RBTN_TAG_00.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_TAG_00.ForeColor = System.Drawing.Color.Black;
            this.RBTN_TAG_00.Location = new System.Drawing.Point(1765, 410);
            this.RBTN_TAG_00.Name = "RBTN_TAG_00";
            this.RBTN_TAG_00.Size = new System.Drawing.Size(116, 37);
            this.RBTN_TAG_00.TabIndex = 84;
            this.RBTN_TAG_00.TabStop = true;
            this.RBTN_TAG_00.Text = "STAGE 1";
            this.RBTN_TAG_00.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_TAG_00.UseVisualStyleBackColor = false;
            this.RBTN_TAG_00.Visible = false;
            this.RBTN_TAG_00.CheckedChanged += new System.EventHandler(this.BTN_PAT_CHANGE_Click);
            this.RBTN_TAG_00.Click += new System.EventHandler(this.RBTN_PAT_Click);
            // 
            // RBTN_TAG_02
            // 
            this.RBTN_TAG_02.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_TAG_02.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_TAG_02.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_TAG_02.ForeColor = System.Drawing.Color.Black;
            this.RBTN_TAG_02.Location = new System.Drawing.Point(1765, 502);
            this.RBTN_TAG_02.Name = "RBTN_TAG_02";
            this.RBTN_TAG_02.Size = new System.Drawing.Size(116, 43);
            this.RBTN_TAG_02.TabIndex = 86;
            this.RBTN_TAG_02.Text = "STAGE 3";
            this.RBTN_TAG_02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_TAG_02.UseVisualStyleBackColor = false;
            this.RBTN_TAG_02.Visible = false;
            this.RBTN_TAG_02.CheckedChanged += new System.EventHandler(this.BTN_PAT_CHANGE_Click);
            this.RBTN_TAG_02.Click += new System.EventHandler(this.RBTN_PAT_Click);
            // 
            // RBTN_TAG_03
            // 
            this.RBTN_TAG_03.Appearance = System.Windows.Forms.Appearance.Button;
            this.RBTN_TAG_03.BackColor = System.Drawing.Color.DarkGray;
            this.RBTN_TAG_03.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RBTN_TAG_03.ForeColor = System.Drawing.Color.Black;
            this.RBTN_TAG_03.Location = new System.Drawing.Point(1765, 551);
            this.RBTN_TAG_03.Name = "RBTN_TAG_03";
            this.RBTN_TAG_03.Size = new System.Drawing.Size(116, 43);
            this.RBTN_TAG_03.TabIndex = 87;
            this.RBTN_TAG_03.Text = "STAGE 4";
            this.RBTN_TAG_03.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RBTN_TAG_03.UseVisualStyleBackColor = false;
            this.RBTN_TAG_03.Visible = false;
            this.RBTN_TAG_03.CheckedChanged += new System.EventHandler(this.BTN_PAT_CHANGE_Click);
            this.RBTN_TAG_03.Click += new System.EventHandler(this.RBTN_PAT_Click);
            // 
            // CB_OBJ_DATA
            // 
            this.CB_OBJ_DATA.BackColor = System.Drawing.Color.DarkGray;
            this.CB_OBJ_DATA.CheckAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.CB_OBJ_DATA.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CB_OBJ_DATA.Location = new System.Drawing.Point(1869, 3);
            this.CB_OBJ_DATA.Name = "CB_OBJ_DATA";
            this.CB_OBJ_DATA.Size = new System.Drawing.Size(34, 20);
            this.CB_OBJ_DATA.TabIndex = 88;
            this.CB_OBJ_DATA.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.CB_OBJ_DATA.UseVisualStyleBackColor = false;
            this.CB_OBJ_DATA.Visible = false;
            this.CB_OBJ_DATA.CheckedChanged += new System.EventHandler(this.CB_OBJ_DATA_CheckedChanged);
            // 
            // Form_Chart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(1904, 1042);
            this.ControlBox = false;
            this.Controls.Add(this.CB_OBJ_DATA);
            this.Controls.Add(this.RBTN_TAG_03);
            this.Controls.Add(this.RBTN_TAG_02);
            this.Controls.Add(this.RBTN_TAG_01);
            this.Controls.Add(this.RBTN_TAG_00);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.BTN_UPDATE);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.MSChart);
            this.Name = "Form_Chart";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_Chart";
            ((System.ComponentModel.ISupportInitialize)(this.MSChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart MSChart;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button BTN_UPDATE;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RadioButton RBTN_TAG_01;
        private System.Windows.Forms.RadioButton RBTN_TAG_00;
        private System.Windows.Forms.RadioButton RBTN_TAG_02;
        private System.Windows.Forms.RadioButton RBTN_TAG_03;
        private System.Windows.Forms.CheckBox CB_OBJ_DATA;
    }
}
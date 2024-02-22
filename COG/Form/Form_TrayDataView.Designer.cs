namespace COG
{
    partial class Form_TrayDataView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_TrayDataView));
            this.DGV_TRAY_DATA = new System.Windows.Forms.DataGridView();
            this.DGV_ITEM_INDEX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_ITEM_CAL_X = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_ITEM_CAL_Y = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_ITEM_CAL_T1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_ITEM_RESULT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_Tray2 = new System.Windows.Forms.Button();
            this.BTN_Tray1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_TRAY_DATA)).BeginInit();
            this.SuspendLayout();
            // 
            // DGV_TRAY_DATA
            // 
            this.DGV_TRAY_DATA.AllowUserToAddRows = false;
            this.DGV_TRAY_DATA.AllowUserToDeleteRows = false;
            this.DGV_TRAY_DATA.AllowUserToResizeColumns = false;
            this.DGV_TRAY_DATA.AllowUserToResizeRows = false;
            this.DGV_TRAY_DATA.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_TRAY_DATA.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DGV_ITEM_INDEX,
            this.DGV_ITEM_CAL_X,
            this.DGV_ITEM_CAL_Y,
            this.DGV_ITEM_CAL_T1,
            this.DGV_ITEM_RESULT});
            this.DGV_TRAY_DATA.Location = new System.Drawing.Point(2, 2);
            this.DGV_TRAY_DATA.Name = "DGV_TRAY_DATA";
            this.DGV_TRAY_DATA.RowHeadersVisible = false;
            this.DGV_TRAY_DATA.RowTemplate.Height = 23;
            this.DGV_TRAY_DATA.Size = new System.Drawing.Size(469, 813);
            this.DGV_TRAY_DATA.TabIndex = 27;
            // 
            // DGV_ITEM_INDEX
            // 
            this.DGV_ITEM_INDEX.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.DGV_ITEM_INDEX.HeaderText = "INDEX";
            this.DGV_ITEM_INDEX.Name = "DGV_ITEM_INDEX";
            this.DGV_ITEM_INDEX.ReadOnly = true;
            this.DGV_ITEM_INDEX.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_ITEM_INDEX.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.DGV_ITEM_INDEX.Width = 47;
            // 
            // DGV_ITEM_CAL_X
            // 
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.NullValue = "0";
            this.DGV_ITEM_CAL_X.DefaultCellStyle = dataGridViewCellStyle1;
            this.DGV_ITEM_CAL_X.HeaderText = "X (um)";
            this.DGV_ITEM_CAL_X.Name = "DGV_ITEM_CAL_X";
            this.DGV_ITEM_CAL_X.ReadOnly = true;
            this.DGV_ITEM_CAL_X.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_ITEM_CAL_X.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV_ITEM_CAL_Y
            // 
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.NullValue = "0";
            this.DGV_ITEM_CAL_Y.DefaultCellStyle = dataGridViewCellStyle2;
            this.DGV_ITEM_CAL_Y.HeaderText = "Y (um)";
            this.DGV_ITEM_CAL_Y.Name = "DGV_ITEM_CAL_Y";
            this.DGV_ITEM_CAL_Y.ReadOnly = true;
            this.DGV_ITEM_CAL_Y.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_ITEM_CAL_Y.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV_ITEM_CAL_T1
            // 
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.NullValue = "0";
            this.DGV_ITEM_CAL_T1.DefaultCellStyle = dataGridViewCellStyle3;
            this.DGV_ITEM_CAL_T1.HeaderText = "T1 (um)";
            this.DGV_ITEM_CAL_T1.Name = "DGV_ITEM_CAL_T1";
            this.DGV_ITEM_CAL_T1.ReadOnly = true;
            this.DGV_ITEM_CAL_T1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_ITEM_CAL_T1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.DGV_ITEM_CAL_T1.Visible = false;
            // 
            // DGV_ITEM_RESULT
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.NullValue = "0";
            this.DGV_ITEM_RESULT.DefaultCellStyle = dataGridViewCellStyle4;
            this.DGV_ITEM_RESULT.HeaderText = "Result";
            this.DGV_ITEM_RESULT.Name = "DGV_ITEM_RESULT";
            this.DGV_ITEM_RESULT.ReadOnly = true;
            this.DGV_ITEM_RESULT.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV_ITEM_RESULT.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(490, 2);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 28;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_Tray2
            // 
            this.BTN_Tray2.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_Tray2.Location = new System.Drawing.Point(490, 207);
            this.BTN_Tray2.Name = "BTN_Tray2";
            this.BTN_Tray2.Size = new System.Drawing.Size(100, 64);
            this.BTN_Tray2.TabIndex = 29;
            this.BTN_Tray2.Text = "Tray 2";
            this.BTN_Tray2.UseVisualStyleBackColor = true;
            this.BTN_Tray2.Visible = false;
            this.BTN_Tray2.Click += new System.EventHandler(this.BTN_Tray2_Click);
            // 
            // BTN_Tray1
            // 
            this.BTN_Tray1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_Tray1.Location = new System.Drawing.Point(490, 113);
            this.BTN_Tray1.Name = "BTN_Tray1";
            this.BTN_Tray1.Size = new System.Drawing.Size(100, 64);
            this.BTN_Tray1.TabIndex = 30;
            this.BTN_Tray1.Text = "Tray 1";
            this.BTN_Tray1.UseVisualStyleBackColor = true;
            this.BTN_Tray1.Visible = false;
            this.BTN_Tray1.Click += new System.EventHandler(this.BTN_Tray1_Click);
            // 
            // Form_TrayDataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(606, 819);
            this.ControlBox = false;
            this.Controls.Add(this.BTN_Tray1);
            this.Controls.Add(this.BTN_Tray2);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.DGV_TRAY_DATA);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_TrayDataView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_TrayDataView";
            this.Load += new System.EventHandler(this.Form_TrayDataView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DGV_TRAY_DATA)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView DGV_TRAY_DATA;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_ITEM_INDEX;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_ITEM_CAL_X;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_ITEM_CAL_Y;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_ITEM_CAL_T1;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_ITEM_RESULT;
        private System.Windows.Forms.Button BTN_Tray2;
        private System.Windows.Forms.Button BTN_Tray1;
    }
}
namespace COG
{
    partial class Form_Project
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Project));
            this.listModel = new System.Windows.Forms.ListBox();
            this.BTN_DELETE = new System.Windows.Forms.Button();
            this.BTN_SAVE = new System.Windows.Forms.Button();
            this.GB_SELECT = new System.Windows.Forms.GroupBox();
            this.LB_DISPLAY_SELECTE = new System.Windows.Forms.Label();
            this.GB_CURRENT = new System.Windows.Forms.GroupBox();
            this.LB_DISPLAY_CURRENT = new System.Windows.Forms.Label();
            this.BTN_LOAD = new System.Windows.Forms.Button();
            this.BTN_RENAME = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_DOWN = new System.Windows.Forms.Button();
            this.BTN_UP = new System.Windows.Forms.Button();
            this.GB_SELECT.SuspendLayout();
            this.GB_CURRENT.SuspendLayout();
            this.SuspendLayout();
            // 
            // listModel
            // 
            this.listModel.Font = new System.Drawing.Font("굴림", 15F);
            this.listModel.FormattingEnabled = true;
            this.listModel.ItemHeight = 20;
            this.listModel.Location = new System.Drawing.Point(12, 87);
            this.listModel.Name = "listModel";
            this.listModel.Size = new System.Drawing.Size(896, 364);
            this.listModel.TabIndex = 19;
            this.listModel.SelectedIndexChanged += new System.EventHandler(this.listModel_SelectedIndexChanged);
            // 
            // BTN_DELETE
            // 
            this.BTN_DELETE.BackColor = System.Drawing.Color.Silver;
            this.BTN_DELETE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_DELETE.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_DELETE.Location = new System.Drawing.Point(711, 457);
            this.BTN_DELETE.Name = "BTN_DELETE";
            this.BTN_DELETE.Size = new System.Drawing.Size(274, 86);
            this.BTN_DELETE.TabIndex = 24;
            this.BTN_DELETE.Text = "DELETE";
            this.BTN_DELETE.UseVisualStyleBackColor = false;
            this.BTN_DELETE.Click += new System.EventHandler(this.BTN_DELETE_Click);
            // 
            // BTN_SAVE
            // 
            this.BTN_SAVE.BackColor = System.Drawing.Color.Silver;
            this.BTN_SAVE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_SAVE.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_SAVE.Location = new System.Drawing.Point(261, 457);
            this.BTN_SAVE.Name = "BTN_SAVE";
            this.BTN_SAVE.Size = new System.Drawing.Size(193, 86);
            this.BTN_SAVE.TabIndex = 23;
            this.BTN_SAVE.Text = "SAVE";
            this.BTN_SAVE.UseVisualStyleBackColor = false;
            this.BTN_SAVE.Click += new System.EventHandler(this.BTN_SAVE_Click);
            // 
            // GB_SELECT
            // 
            this.GB_SELECT.Controls.Add(this.LB_DISPLAY_SELECTE);
            this.GB_SELECT.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GB_SELECT.ForeColor = System.Drawing.Color.White;
            this.GB_SELECT.Location = new System.Drawing.Point(505, 12);
            this.GB_SELECT.Name = "GB_SELECT";
            this.GB_SELECT.Size = new System.Drawing.Size(480, 69);
            this.GB_SELECT.TabIndex = 26;
            this.GB_SELECT.TabStop = false;
            this.GB_SELECT.Text = "Selected Model";
            // 
            // LB_DISPLAY_SELECTE
            // 
            this.LB_DISPLAY_SELECTE.AutoSize = true;
            this.LB_DISPLAY_SELECTE.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LB_DISPLAY_SELECTE.ForeColor = System.Drawing.Color.RoyalBlue;
            this.LB_DISPLAY_SELECTE.Location = new System.Drawing.Point(6, 23);
            this.LB_DISPLAY_SELECTE.Name = "LB_DISPLAY_SELECTE";
            this.LB_DISPLAY_SELECTE.Size = new System.Drawing.Size(0, 29);
            this.LB_DISPLAY_SELECTE.TabIndex = 0;
            // 
            // GB_CURRENT
            // 
            this.GB_CURRENT.Controls.Add(this.LB_DISPLAY_CURRENT);
            this.GB_CURRENT.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GB_CURRENT.ForeColor = System.Drawing.Color.White;
            this.GB_CURRENT.Location = new System.Drawing.Point(12, 12);
            this.GB_CURRENT.Name = "GB_CURRENT";
            this.GB_CURRENT.Size = new System.Drawing.Size(480, 69);
            this.GB_CURRENT.TabIndex = 25;
            this.GB_CURRENT.TabStop = false;
            this.GB_CURRENT.Text = "Current Model";
            // 
            // LB_DISPLAY_CURRENT
            // 
            this.LB_DISPLAY_CURRENT.AutoSize = true;
            this.LB_DISPLAY_CURRENT.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LB_DISPLAY_CURRENT.ForeColor = System.Drawing.Color.Black;
            this.LB_DISPLAY_CURRENT.Location = new System.Drawing.Point(6, 27);
            this.LB_DISPLAY_CURRENT.Name = "LB_DISPLAY_CURRENT";
            this.LB_DISPLAY_CURRENT.Size = new System.Drawing.Size(0, 29);
            this.LB_DISPLAY_CURRENT.TabIndex = 0;
            // 
            // BTN_LOAD
            // 
            this.BTN_LOAD.BackColor = System.Drawing.Color.Silver;
            this.BTN_LOAD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_LOAD.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_LOAD.Location = new System.Drawing.Point(12, 457);
            this.BTN_LOAD.Name = "BTN_LOAD";
            this.BTN_LOAD.Size = new System.Drawing.Size(243, 86);
            this.BTN_LOAD.TabIndex = 22;
            this.BTN_LOAD.Text = "LOAD";
            this.BTN_LOAD.UseVisualStyleBackColor = false;
            this.BTN_LOAD.Click += new System.EventHandler(this.BTN_LOAD_Click);
            // 
            // BTN_RENAME
            // 
            this.BTN_RENAME.BackColor = System.Drawing.Color.Silver;
            this.BTN_RENAME.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_RENAME.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_RENAME.Location = new System.Drawing.Point(460, 457);
            this.BTN_RENAME.Name = "BTN_RENAME";
            this.BTN_RENAME.Size = new System.Drawing.Size(245, 86);
            this.BTN_RENAME.TabIndex = 28;
            this.BTN_RENAME.Text = "RENAME";
            this.BTN_RENAME.UseVisualStyleBackColor = false;
            this.BTN_RENAME.Click += new System.EventHandler(this.BTN_RENAME_Click);
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.BTN_EXIT.Location = new System.Drawing.Point(885, 649);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(100, 100);
            this.BTN_EXIT.TabIndex = 27;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // BTN_DOWN
            // 
            this.BTN_DOWN.BackColor = System.Drawing.Color.Silver;
            this.BTN_DOWN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_DOWN.Font = new System.Drawing.Font("굴림", 20F, System.Drawing.FontStyle.Bold);
            this.BTN_DOWN.Image = global::COG.Properties.Resources.DOWN1;
            this.BTN_DOWN.Location = new System.Drawing.Point(914, 265);
            this.BTN_DOWN.Name = "BTN_DOWN";
            this.BTN_DOWN.Size = new System.Drawing.Size(71, 186);
            this.BTN_DOWN.TabIndex = 21;
            this.BTN_DOWN.UseVisualStyleBackColor = false;
            this.BTN_DOWN.Click += new System.EventHandler(this.BTN_DOWN_Click);
            // 
            // BTN_UP
            // 
            this.BTN_UP.BackColor = System.Drawing.Color.Silver;
            this.BTN_UP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_UP.Font = new System.Drawing.Font("굴림", 20F, System.Drawing.FontStyle.Bold);
            this.BTN_UP.Image = global::COG.Properties.Resources.UP1;
            this.BTN_UP.Location = new System.Drawing.Point(914, 87);
            this.BTN_UP.Name = "BTN_UP";
            this.BTN_UP.Size = new System.Drawing.Size(71, 172);
            this.BTN_UP.TabIndex = 20;
            this.BTN_UP.UseVisualStyleBackColor = false;
            this.BTN_UP.Click += new System.EventHandler(this.BTN_UP_Click);
            // 
            // Form_Project
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.ClientSize = new System.Drawing.Size(997, 761);
            this.ControlBox = false;
            this.Controls.Add(this.GB_SELECT);
            this.Controls.Add(this.BTN_RENAME);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_DELETE);
            this.Controls.Add(this.BTN_SAVE);
            this.Controls.Add(this.BTN_LOAD);
            this.Controls.Add(this.BTN_DOWN);
            this.Controls.Add(this.BTN_UP);
            this.Controls.Add(this.listModel);
            this.Controls.Add(this.GB_CURRENT);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "Form_Project";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MODEL LOAD/SAVE/DELETE";
            this.Load += new System.EventHandler(this.Form_Project_Load);
            this.GB_SELECT.ResumeLayout(false);
            this.GB_SELECT.PerformLayout();
            this.GB_CURRENT.ResumeLayout(false);
            this.GB_CURRENT.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listModel;
        private System.Windows.Forms.Button BTN_DELETE;
        private System.Windows.Forms.Button BTN_SAVE;
        private System.Windows.Forms.Button BTN_DOWN;
        private System.Windows.Forms.Button BTN_UP;
        private System.Windows.Forms.GroupBox GB_SELECT;
        private System.Windows.Forms.Label LB_DISPLAY_SELECTE;
        private System.Windows.Forms.GroupBox GB_CURRENT;
        private System.Windows.Forms.Label LB_DISPLAY_CURRENT;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Button BTN_LOAD;
        private System.Windows.Forms.Button BTN_RENAME;

    }
}
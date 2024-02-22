namespace COG
{
    partial class Form_Permission
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Permission));
            this.pnlPermission = new System.Windows.Forms.Panel();
            this.tlpPermission = new System.Windows.Forms.TableLayoutPanel();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.tlpSelectPermission = new System.Windows.Forms.TableLayoutPanel();
            this.btnMaker = new System.Windows.Forms.Button();
            this.btnEngineer = new System.Windows.Forms.Button();
            this.btnOperator = new System.Windows.Forms.Button();
            this.pnlPermission.SuspendLayout();
            this.tlpPermission.SuspendLayout();
            this.tlpSelectPermission.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlPermission
            // 
            this.pnlPermission.Controls.Add(this.tlpPermission);
            this.pnlPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPermission.Location = new System.Drawing.Point(0, 0);
            this.pnlPermission.Name = "pnlPermission";
            this.pnlPermission.Size = new System.Drawing.Size(781, 380);
            this.pnlPermission.TabIndex = 0;
            // 
            // tlpPermission
            // 
            this.tlpPermission.ColumnCount = 1;
            this.tlpPermission.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPermission.Controls.Add(this.BTN_EXIT, 0, 1);
            this.tlpPermission.Controls.Add(this.tlpSelectPermission, 0, 0);
            this.tlpPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpPermission.Location = new System.Drawing.Point(0, 0);
            this.tlpPermission.Name = "tlpPermission";
            this.tlpPermission.RowCount = 2;
            this.tlpPermission.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.tlpPermission.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpPermission.Size = new System.Drawing.Size(781, 380);
            this.tlpPermission.TabIndex = 0;
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.DarkGray;
            this.BTN_EXIT.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BTN_EXIT.BackgroundImage")));
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BTN_EXIT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BTN_EXIT.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_EXIT.Location = new System.Drawing.Point(3, 250);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(775, 127);
            this.BTN_EXIT.TabIndex = 28;
            this.BTN_EXIT.Text = "EXIT";
            this.BTN_EXIT.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            // 
            // tlpSelectPermission
            // 
            this.tlpSelectPermission.ColumnCount = 3;
            this.tlpSelectPermission.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSelectPermission.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSelectPermission.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpSelectPermission.Controls.Add(this.btnMaker, 0, 0);
            this.tlpSelectPermission.Controls.Add(this.btnEngineer, 0, 0);
            this.tlpSelectPermission.Controls.Add(this.btnOperator, 0, 0);
            this.tlpSelectPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSelectPermission.Location = new System.Drawing.Point(3, 3);
            this.tlpSelectPermission.Name = "tlpSelectPermission";
            this.tlpSelectPermission.RowCount = 1;
            this.tlpSelectPermission.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSelectPermission.Size = new System.Drawing.Size(775, 241);
            this.tlpSelectPermission.TabIndex = 0;
            // 
            // btnMaker
            // 
            this.btnMaker.BackColor = System.Drawing.Color.DarkGray;
            this.btnMaker.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMaker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMaker.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnMaker.Image = ((System.Drawing.Image)(resources.GetObject("btnMaker.Image")));
            this.btnMaker.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMaker.Location = new System.Drawing.Point(521, 40);
            this.btnMaker.Margin = new System.Windows.Forms.Padding(5, 40, 5, 40);
            this.btnMaker.Name = "btnMaker";
            this.btnMaker.Size = new System.Drawing.Size(249, 161);
            this.btnMaker.TabIndex = 44;
            this.btnMaker.Text = "MAKER";
            this.btnMaker.UseVisualStyleBackColor = false;
            this.btnMaker.Click += new System.EventHandler(this.btnSelectPermission_Click);
            // 
            // btnEngineer
            // 
            this.btnEngineer.BackColor = System.Drawing.Color.DarkGray;
            this.btnEngineer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnEngineer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnEngineer.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnEngineer.Image = ((System.Drawing.Image)(resources.GetObject("btnEngineer.Image")));
            this.btnEngineer.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEngineer.Location = new System.Drawing.Point(263, 40);
            this.btnEngineer.Margin = new System.Windows.Forms.Padding(5, 40, 5, 40);
            this.btnEngineer.Name = "btnEngineer";
            this.btnEngineer.Size = new System.Drawing.Size(248, 161);
            this.btnEngineer.TabIndex = 43;
            this.btnEngineer.Text = "ENGINEER";
            this.btnEngineer.UseVisualStyleBackColor = false;
            this.btnEngineer.Click += new System.EventHandler(this.btnSelectPermission_Click);
            // 
            // btnOperator
            // 
            this.btnOperator.BackColor = System.Drawing.Color.DarkGray;
            this.btnOperator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnOperator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOperator.Font = new System.Drawing.Font("맑은 고딕", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnOperator.Image = ((System.Drawing.Image)(resources.GetObject("btnOperator.Image")));
            this.btnOperator.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOperator.Location = new System.Drawing.Point(5, 40);
            this.btnOperator.Margin = new System.Windows.Forms.Padding(5, 40, 5, 40);
            this.btnOperator.Name = "btnOperator";
            this.btnOperator.Size = new System.Drawing.Size(248, 161);
            this.btnOperator.TabIndex = 42;
            this.btnOperator.Text = "OPERATOR";
            this.btnOperator.UseVisualStyleBackColor = false;
            this.btnOperator.Click += new System.EventHandler(this.btnSelectPermission_Click);
            // 
            // Form_Permission
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(781, 380);
            this.Controls.Add(this.pnlPermission);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Permission";
            this.Text = "Select Permission";
            this.pnlPermission.ResumeLayout(false);
            this.tlpPermission.ResumeLayout(false);
            this.tlpSelectPermission.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlPermission;
        private System.Windows.Forms.TableLayoutPanel tlpPermission;
        private System.Windows.Forms.TableLayoutPanel tlpSelectPermission;
        private System.Windows.Forms.Button btnOperator;
        private System.Windows.Forms.Button btnMaker;
        private System.Windows.Forms.Button btnEngineer;
        private System.Windows.Forms.Button BTN_EXIT;
    }
}
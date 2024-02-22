namespace COG
{
    partial class UserControl_Info
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel5 = new System.Windows.Forms.Panel();
            this.lblHDDD = new System.Windows.Forms.Label();
            this.lblHDDC = new System.Windows.Forms.Label();
            this.lblRAM = new System.Windows.Forms.Label();
            this.lblCPU = new System.Windows.Forms.Label();
            this.progressBar_HDDD = new System.Windows.Forms.ProgressBar();
            this.progressBar_HDDC = new System.Windows.Forms.ProgressBar();
            this.progressBar_RAM = new System.Windows.Forms.ProgressBar();
            this.progressBar_CPU = new System.Windows.Forms.ProgressBar();
            this.HDD_D_Label = new System.Windows.Forms.Label();
            this.HDD_C_Label = new System.Windows.Forms.Label();
            this.RamLabel = new System.Windows.Forms.Label();
            this.CpuLabel = new System.Windows.Forms.Label();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.DarkGray;
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.lblHDDD);
            this.panel5.Controls.Add(this.lblHDDC);
            this.panel5.Controls.Add(this.lblRAM);
            this.panel5.Controls.Add(this.lblCPU);
            this.panel5.Controls.Add(this.progressBar_HDDD);
            this.panel5.Controls.Add(this.progressBar_HDDC);
            this.panel5.Controls.Add(this.progressBar_RAM);
            this.panel5.Controls.Add(this.progressBar_CPU);
            this.panel5.Controls.Add(this.HDD_D_Label);
            this.panel5.Controls.Add(this.HDD_C_Label);
            this.panel5.Controls.Add(this.RamLabel);
            this.panel5.Controls.Add(this.CpuLabel);
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(240, 120);
            this.panel5.TabIndex = 6;
            // 
            // lblHDDD
            // 
            this.lblHDDD.AutoSize = true;
            this.lblHDDD.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblHDDD.ForeColor = System.Drawing.Color.Yellow;
            this.lblHDDD.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblHDDD.Location = new System.Drawing.Point(210, 100);
            this.lblHDDD.Name = "lblHDDD";
            this.lblHDDD.Size = new System.Drawing.Size(20, 12);
            this.lblHDDD.TabIndex = 18;
            this.lblHDDD.Text = " 0%";
            // 
            // lblHDDC
            // 
            this.lblHDDC.AutoSize = true;
            this.lblHDDC.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblHDDC.ForeColor = System.Drawing.Color.Yellow;
            this.lblHDDC.Location = new System.Drawing.Point(210, 70);
            this.lblHDDC.Name = "lblHDDC";
            this.lblHDDC.Size = new System.Drawing.Size(20, 12);
            this.lblHDDC.TabIndex = 17;
            this.lblHDDC.Text = " 0%";
            this.lblHDDC.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRAM
            // 
            this.lblRAM.AutoSize = true;
            this.lblRAM.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblRAM.ForeColor = System.Drawing.Color.Yellow;
            this.lblRAM.Location = new System.Drawing.Point(210, 40);
            this.lblRAM.Name = "lblRAM";
            this.lblRAM.Size = new System.Drawing.Size(20, 12);
            this.lblRAM.TabIndex = 16;
            this.lblRAM.Text = " 0%";
            this.lblRAM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCPU
            // 
            this.lblCPU.AutoSize = true;
            this.lblCPU.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCPU.ForeColor = System.Drawing.Color.Yellow;
            this.lblCPU.Location = new System.Drawing.Point(210, 10);
            this.lblCPU.Name = "lblCPU";
            this.lblCPU.Size = new System.Drawing.Size(20, 12);
            this.lblCPU.TabIndex = 15;
            this.lblCPU.Text = " 0%";
            this.lblCPU.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // progressBar_HDDD
            // 
            this.progressBar_HDDD.Location = new System.Drawing.Point(40, 92);
            this.progressBar_HDDD.Name = "progressBar_HDDD";
            this.progressBar_HDDD.Size = new System.Drawing.Size(160, 24);
            this.progressBar_HDDD.TabIndex = 14;
            // 
            // progressBar_HDDC
            // 
            this.progressBar_HDDC.Location = new System.Drawing.Point(40, 62);
            this.progressBar_HDDC.Name = "progressBar_HDDC";
            this.progressBar_HDDC.Size = new System.Drawing.Size(160, 24);
            this.progressBar_HDDC.TabIndex = 13;
            // 
            // progressBar_RAM
            // 
            this.progressBar_RAM.Location = new System.Drawing.Point(40, 32);
            this.progressBar_RAM.Name = "progressBar_RAM";
            this.progressBar_RAM.Size = new System.Drawing.Size(160, 24);
            this.progressBar_RAM.TabIndex = 12;
            // 
            // progressBar_CPU
            // 
            this.progressBar_CPU.Location = new System.Drawing.Point(40, 2);
            this.progressBar_CPU.Name = "progressBar_CPU";
            this.progressBar_CPU.Size = new System.Drawing.Size(160, 24);
            this.progressBar_CPU.TabIndex = 11;
            // 
            // HDD_D_Label
            // 
            this.HDD_D_Label.AutoSize = true;
            this.HDD_D_Label.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.HDD_D_Label.Location = new System.Drawing.Point(2, 95);
            this.HDD_D_Label.Name = "HDD_D_Label";
            this.HDD_D_Label.Size = new System.Drawing.Size(40, 12);
            this.HDD_D_Label.TabIndex = 10;
            this.HDD_D_Label.Text = "Ddrv :";
            // 
            // HDD_C_Label
            // 
            this.HDD_C_Label.AutoSize = true;
            this.HDD_C_Label.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.HDD_C_Label.Location = new System.Drawing.Point(2, 65);
            this.HDD_C_Label.Name = "HDD_C_Label";
            this.HDD_C_Label.Size = new System.Drawing.Size(40, 12);
            this.HDD_C_Label.TabIndex = 9;
            this.HDD_C_Label.Text = "Cdrv :";
            // 
            // RamLabel
            // 
            this.RamLabel.AutoSize = true;
            this.RamLabel.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RamLabel.Location = new System.Drawing.Point(2, 35);
            this.RamLabel.Name = "RamLabel";
            this.RamLabel.Size = new System.Drawing.Size(40, 12);
            this.RamLabel.TabIndex = 8;
            this.RamLabel.Text = "RAM :";
            // 
            // CPULabel
            // 
            this.CpuLabel.AutoSize = true;
            this.CpuLabel.Font = new System.Drawing.Font("굴림", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CpuLabel.Location = new System.Drawing.Point(2, 5);
            this.CpuLabel.Name = "CpuLabel";
            this.CpuLabel.Size = new System.Drawing.Size(40, 12);
            this.CpuLabel.TabIndex = 7;
            this.CpuLabel.Text = "CPU :";
            // 
            // UserControl_Info
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel5);
            this.Name = "UserControl_Info";
            this.Size = new System.Drawing.Size(240, 120);
            this.Load += new System.EventHandler(this.UcComputer_Load);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label lblHDDD;
        private System.Windows.Forms.Label lblHDDC;
        private System.Windows.Forms.Label lblRAM;
        private System.Windows.Forms.Label lblCPU;
        private System.Windows.Forms.ProgressBar progressBar_HDDD;
        private System.Windows.Forms.ProgressBar progressBar_HDDC;
        private System.Windows.Forms.ProgressBar progressBar_RAM;
        private System.Windows.Forms.ProgressBar progressBar_CPU;
        private System.Windows.Forms.Label HDD_D_Label;
        private System.Windows.Forms.Label HDD_C_Label;
        private System.Windows.Forms.Label RamLabel;
        private System.Windows.Forms.Label CpuLabel;

    }
}

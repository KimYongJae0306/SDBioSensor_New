namespace Cog.Framework.UI.Controls
{
    partial class DisplayControl
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
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayControl));
            this.btnDisplayName = new System.Windows.Forms.Button();
            this.CogDisplay = new Cognex.VisionPro.CogRecordDisplay();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.CogDisplay)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDisplayName
            // 
            this.btnDisplayName.BackColor = System.Drawing.Color.SkyBlue;
            this.btnDisplayName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDisplayName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDisplayName.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnDisplayName.Location = new System.Drawing.Point(1, 1);
            this.btnDisplayName.Margin = new System.Windows.Forms.Padding(1);
            this.btnDisplayName.Name = "btnDisplayName";
            this.btnDisplayName.Size = new System.Drawing.Size(626, 30);
            this.btnDisplayName.TabIndex = 104;
            this.btnDisplayName.Text = "DISPLAY_01";
            this.btnDisplayName.UseVisualStyleBackColor = false;
            this.btnDisplayName.Visible = false;
            // 
            // CogDisplay
            // 
            this.CogDisplay.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.CogDisplay.ColorMapLowerRoiLimit = 0D;
            this.CogDisplay.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.CogDisplay.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.CogDisplay.ColorMapUpperRoiLimit = 1D;
            this.CogDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CogDisplay.DoubleTapZoomCycleLength = 2;
            this.CogDisplay.DoubleTapZoomSensitivity = 2.5D;
            this.CogDisplay.Location = new System.Drawing.Point(1, 33);
            this.CogDisplay.Margin = new System.Windows.Forms.Padding(1);
            this.CogDisplay.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.CogDisplay.MouseWheelSensitivity = 1D;
            this.CogDisplay.Name = "CogDisplay";
            this.CogDisplay.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("CogDisplay.OcxState")));
            this.CogDisplay.Size = new System.Drawing.Size(626, 432);
            this.CogDisplay.TabIndex = 105;
            this.CogDisplay.Visible = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.btnDisplayName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CogDisplay, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(628, 466);
            this.tableLayoutPanel1.TabIndex = 106;
            // 
            // DisplayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DisplayControl";
            this.Size = new System.Drawing.Size(628, 466);
            this.Load += new System.EventHandler(this.DisplayControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.CogDisplay)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDisplayName;
        private Cognex.VisionPro.CogRecordDisplay CogDisplay;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

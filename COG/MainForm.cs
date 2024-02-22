using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace COG
{
    public partial class MainForm : Form
    {
        #region 필드

        #endregion

        #region 속성
        #endregion

        #region 이벤트
        #endregion

        #region 델리게이트
        #endregion

        #region 생성자
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region 메서드
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
          
            AddControls();

            LicenseCheck();

        }

        private void AddControls()
        {
            for (int i = 0; i < 4; i++)
            {
                BTN_DISNAME_01.Visible = true;
                MA_Display01.Visible = true;
            }
            //DisplayContainer.Appearance = TabAppearance.Buttons;
            //DisplayContainer.SizeMode = TabSizeMode.Fixed;
            //DisplayContainer.ItemSize = new Size(0, 1); // TAB 우측 버튼부분 숨기려고 만듬
        }

        private void LicenseCheck()
        {
            if(Cognex.VisionPro.CogLicense.GetLicensedFeatures(false, false).Count == 0)
            {
                MessageBox.Show("Cognex USB Lincense not detected");
            }
        }
    }
}

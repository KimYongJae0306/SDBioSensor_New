using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro.Display;
using Cognex.VisionPro;
namespace COG
{
    public partial class Form_NGMonitor : Form
    {
        public Main.AlignUnitTag AlignUnit;
        public int m_PatTagNo;
        public bool IsFormLoad = false;
        public Form_NGMonitor(Main.AlignUnitTag AlignUnit)
        {
            InitializeComponent();
            this.AlignUnit = AlignUnit;
            if (AlignUnit.m_AlignType[m_PatTagNo] == Main.DEFINE.M_1CAM1SHOT)
            {
                Panel_1Cam.Visible = true;
                Panel_2Cam.Visible = false;

                MM_ToolBar_1Cam.Display = MM_DISPLAY_1CAM1;
                MM_StatusBar_1Cam.Display = MM_DISPLAY_1CAM1;
                MM_StatusBar_1Cam.CoordinateSpaceName = "*\\#\\@";
            }
            else     // Display 2EA
            {
                Panel_1Cam.Visible = false;
                Panel_2Cam.Visible = true;

                MM_ToolBar_2Cam1.Display = MM_DISPLAY_2CAM1;
                MM_StatusBar_2Cam1.Display = MM_DISPLAY_2CAM1;
                MM_StatusBar_2Cam1.CoordinateSpaceName = "*\\#\\@";

                MM_ToolBar_2Cam2.Display = MM_DISPLAY_2CAM2;
                MM_StatusBar_2Cam2.Display = MM_DISPLAY_2CAM2;
                MM_StatusBar_2Cam2.CoordinateSpaceName = "*\\#\\@";
            }
            LB_ALIGNUNIT_NAME.Text = AlignUnit.m_AlignName;
        }
        private void Form_NGMonitor_Load(object sender, EventArgs e)
        {
            IsFormLoad = true;
            Form ThisForm = (Form)sender;
        }
        public void Form_ImageChange()
        {
            if (AlignUnit.m_AlignType[m_PatTagNo] == Main.DEFINE.M_1CAM1SHOT)
            {
                OverlayBlobImage(MM_DISPLAY_1CAM1, AlignUnit.PAT[m_PatTagNo, 0].TempImage, AlignUnit.PAT[m_PatTagNo, 0].BlobResults);
            }
            else
            {
                OverlayBlobImage(MM_DISPLAY_2CAM1, AlignUnit.PAT[m_PatTagNo, 0].TempImage, AlignUnit.PAT[m_PatTagNo, 0].BlobResults);
                OverlayBlobImage(MM_DISPLAY_2CAM2, AlignUnit.PAT[m_PatTagNo, 1].TempImage, AlignUnit.PAT[m_PatTagNo, 1].BlobResults);
            }
        }
        public void OverlayBlobImage(CogRecordDisplay MM_RecordDISPLAY, CogImage8Grey BlobImage, Main.BlobResult[] BlobResult_)
        {
            Main.DisplayClear(MM_RecordDISPLAY);
            MM_RecordDISPLAY.Image = BlobImage;
            Main.DisplayFit(MM_RecordDISPLAY);
            Main.DrawOverlayBlobTool(MM_RecordDISPLAY, BlobResult_);
        }
        private void Form_NGMonitor_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
            IsFormLoad = false;
            Main.DisplayClear(MM_DISPLAY_1CAM1);
            Main.DisplayClear(MM_DISPLAY_2CAM1);
            Main.DisplayClear(MM_DISPLAY_2CAM2);
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            Form_NGMonitor_FormClosed(null, null);
        }
    }
}                    
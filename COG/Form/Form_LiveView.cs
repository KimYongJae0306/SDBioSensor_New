using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Dimensioning;

using JAS.Controls.Display;


namespace COG
{
    public partial class Form_LiveView : Form
    {
        CogPointMarker[,] MarkPoint = new CogPointMarker[1, 2];
        CogDistancePointPointTool nDistance = new CogDistancePointPointTool();
        bool[] nDistanceShow = new bool[4];

       
        int nCrossSize = 200;
        int nDisplayCount = 1;
        private int[] m_CamNo = new int[1];
        int nAlignNo = 0, nPatTagNo = 0 , nPatNo = 0;
        private List<Label> LB_DISTANCE = new List<Label>();
        private List<Label> LB_ANGLE = new List<Label>();


        private List<CogDisplay> cogDisplay = new List<CogDisplay>();
        private List<CogDisplayToolbarV2> cogDiToolBar = new List<CogDisplayToolbarV2>();
        private List<CogDisplayStatusBarV2> cogDisStatuBar = new List<CogDisplayStatusBarV2>();
        private CogLine mCogLine1 = new CogLine();
        private CogLine mCogLine2 = new CogLine();

        private double ZoomBackup = new double();
        public Form_LiveView(int nCamNo1)
        {
            m_CamNo[0] = nCamNo1;

            InitializeComponent();
        }
        private void Form_LiveView_Load(object sender, EventArgs e)
        {       
            Allocate_Array();
        }
        private void Allocate_Array()
        {
            cogDisplay.Add(MM_DISPLAY00.CogDisplay00);
            cogDiToolBar.Add(MM_DisplayToolbar00);
            cogDisStatuBar.Add(MM_DisplayStatusBar00);
            LB_DISTANCE.Add(LB_DISTANCE00);
            LB_ANGLE.Add(LB_ANGLE00);
            
            for (int i = 0; i < nDisplayCount; i++)
            {
                cogDiToolBar[i].Display = cogDisplay[i];
                cogDisStatuBar[i].Display = cogDisplay[i];
            }

        }
        public void FormLoad()
        {
            timer1.Enabled = true;
            bool breakFlag = false;

            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                for (int jj = 0; jj < Main.AlignUnit[i].m_AlignPatTagMax; jj++)
                {
                    for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[jj]; j++)
                    {
                        if (Main.AlignUnit[i].PAT[jj, j].m_CamNo == m_CamNo[0])
                        {
                            nAlignNo = i;
                            nPatNo = j;

                             nPatTagNo = jj;

                            breakFlag = true;
                            break;
                        }
                    }
                }
                if (breakFlag) break;
            }
            MM_DISPLAY00.Resuloution = Main.AlignUnit[nAlignNo].PAT[nPatTagNo, nPatNo].m_CalX[0];
            BTN_DISNAME_00.Text = Main.AlignUnit[nAlignNo].m_AlignName;

            LB_DISTANCE00.Text = "";
            LB_ANGLE00.Text = "";

            MM_DISPLAY00.DisplayClear();
            BTN_DISNAME_00.BackColor = Color.SkyBlue;
            MM_DISPLAY00.CrossLine();
            if (Main.vision.USE_CUSTOM_CROSS[m_CamNo[0]] == true)
            {
                MM_DISPLAY00.CustomCross = new PointF(Main.vision.CUSTOM_CROSS_X[m_CamNo[0]], Main.vision.CUSTOM_CROSS_Y[m_CamNo[0]]);
                //MM_DISPLAY00.CustomCrossLine();
            }
            Main.DisplayFit(cogDisplay[0]);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < nDisplayCount; i++)
            {
                if (Main.vision.GrabRefresh_LiveView[m_CamNo[i]] || Main.DEFINE.OPEN_F)     //200113 Stop 일대 0번의 값을 한번 뿌리면 나머지 표시안됨. i -> m_CamNo[i] 바꿈.
                {
                    cogDisplay[i].Image = Main.vision.CogCamBuf[m_CamNo[i]];
                    Main.vision.GrabRefresh_LiveView[m_CamNo[i]] = false;       //200113 Stop 일대 0번의 값을 한번 뿌리면 나머지 표시안됨. i -> m_CamNo[i] 바꿈.
                }
            }
        }    
        public void BTN_EXIT_Click(object sender, EventArgs e)
        {
          //  this.Close();
            MM_DISPLAY00.DisplayClear();
            timer1.Enabled = false;
            //Main.vision.USE_CUSTOM_CROSS[m_CamNo[0]] = MM_DISPLAY00.UseCustomCross;
            //Main.vision.CUSTOM_CROSS_X[m_CamNo[0]] = (int)MM_DISPLAY00.CustomCross.X;
            //Main.vision.CUSTOM_CROSS_Y[m_CamNo[0]] = (int)MM_DISPLAY00.CustomCross.Y;

            this.Hide();

        }
        private void Form_LiveView_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;
            timer1.Dispose();

            MM_DisplayStatusBar00.Dispose();
            MM_DisplayToolbar00.Dispose();
        }
    }
}

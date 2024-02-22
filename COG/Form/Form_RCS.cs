using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

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

namespace COG
{
    public partial class Form_RCS : Form
    {
        public int m_AlignNo;
        private int m_PatTagNo;
        private int m_PatNo;
        private int m_CamNo;

        private int m_iSelPoint;
        private int m_iScaleBarSize;
        private int m_iScaleBarPixel;
        private bool m_bShowScaleBar;
        private static MTickTimer m_Timer = new MTickTimer();
     //   public bool nResult = false;

//         double MarkPos_X;
//         double MarkPos_Y;
        int nCrossSize = 300;
        double nScale = 1.00000000000000000000000;

        CogPointMarker MarkPoint = new CogPointMarker();
        //     CogCoordinateAxes MarkPoint = new CogCoordinateAxes();

        CogLineSegment CogLineScaleBar = new CogLineSegment();

        private List<RadioButton> RBTN_POINTS = new List<RadioButton>();

        private struct MTickTimer
        {
            public DateTime timeStart;
            public DateTime timeEnd;

            public void StartTimer()
            {
                timeStart = DateTime.Now;
            }

            public long GetEllapseTime()
            {
                timeEnd = DateTime.Now;
                return (timeEnd.Ticks - timeStart.Ticks) / 10000; //리턴값 1ms 
            }
        }
        public Form_RCS()
        {
            InitializeComponent();
//             m_AlignNo = nAlignNum;
//             m_PatNo = nPatNum;

        }
        private void Initial_Form()
        {
            RBTN_POINTS.Add(RBTN_RCS_POINT01);
            RBTN_POINTS.Add(RBTN_RCS_POINT02);
            RBTN_POINTS.Add(RBTN_RCS_POINT03);
            RBTN_POINTS.Add(RBTN_RCS_POINT04);
            RBTN_POINTS.Add(RBTN_RCS_POINT05);
            RBTN_POINTS.Add(RBTN_RCS_POINT06);
            RBTN_POINTS.Add(RBTN_RCS_POINT07);
            RBTN_POINTS.Add(RBTN_RCS_POINT08);

            RBTN_RCS_POINT01.Checked = true;

            LB_SCALE.Text = m_iScaleBarSize.ToString() + " um";

            m_bShowScaleBar = false;

            //            MarkPoint.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
            //            MarkPoint.SizeInScreenPixels = nCrossSize; //화면에 표시 되는 + 모양 크기 . 
            //            MarkPoint.LineStyle = CogGraphicLineStyleConstants.Dot;
            //            MarkPoint.SelectedLineWidthInScreenPixels = 3;
            ////             MarkPoint.GraphicDOFEnable = CogCoordinateAxesDOFConstants.All;
            ////             MarkPoint.DisplayedXAxisLength = nCrossSize; //화면에 표시 되는 + 모양 크기 . 


            //       //     MarkPoint.DragLineWidthInScreenPixels = nCrossSize; //화면에 표시 되는 + 모양 크기 . 
            m_PatTagNo = 0;
            m_PatNo = 1;    // INSPECTION IMAGE
            m_CamNo = 0;

            //            MarkPoint.Interactive = true;
            //MM_DisplayToolbar01.Display   = MM_DISPLAY00;
            //MM_DisplayStatusBar01.Display = MM_DISPLAY00;
            //MM_DisplayStatusBar01.CoordinateSpaceName = "*\\#\\@";
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempImage.Allocated)
                MM_DISPLAY00.Image = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempImage;
            else
                MM_DISPLAY00.Image = new CogImage8Grey(Main.vision.CogCamBuf[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] as CogImage8Grey);

            MM_DISPLAY00.DisplayFit();
        }

        private static void CmdCheck(int nAdress)
        {
            if (Main.DEFINE.OPEN_F || Main.DEFINE.OPEN_CAM) return; 
            int seq = 0;
            bool LoopFlag = true;

            while (LoopFlag)
            {
                switch (seq)
                {
                    case 0:
                        m_Timer.StartTimer();
                        seq++;
                        break;

                    case 1:
                        if (m_Timer.GetEllapseTime() > Main.DEFINE.MOVE_TIMEOUT)
                        {
                            seq = SEQ.COMPLET_SEQ;
                            break;
                        }
                        if (PLCDataTag.RData[nAdress] != 5)
                            break;
                        else
                            seq = SEQ.COMPLET_SEQ;
                        break;

                    case SEQ.COMPLET_SEQ:
                        LoopFlag = false;
                        break;

                }

            }

        }
        private void Update_Display()
        {
            //             MarkPoint.X = MarkPos_X;
            //             MarkPoint.Y = MarkPos_Y;          
            //             MarkPoint.Transform.TranslationX = MarkPos_X;
            //             MarkPoint.Transform.TranslationY = MarkPos_Y;     
            //     MM_DISPLAY01.InteractiveGraphics.Add(MarkPoint as ICogGraphicInteractive, "Find MarkerPos", false);
            switch (m_iSelPoint)
            {
                case 1:
                    m_AlignNo = 0;
                    break;
                case 2:
                case 3:
                case 7:
                    m_AlignNo = 2;
                    break;
                case 4:
                case 5:
                case 8:
                    m_AlignNo = 3;
                    break;
                case 6:
                    m_AlignNo = 1;
                    break;
                default:
                    m_AlignNo = 0;
                    break;
            }

            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempImage.Allocated)
                MM_DISPLAY00.Image = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempImage;
            else
                MM_DISPLAY00.Image = new CogImage8Grey(Main.vision.CogCamBuf[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] as CogImage8Grey);
        }


        private void BTN_MOVE_UP_Click(object sender, EventArgs e)
        {
            MarkPoint.Y = MarkPoint.Y - 1;// (nScale / MM_DISPLAY00.);
        //    UPdate_Display();
        }

        private void BTN_MOVE_LEFT_Click(object sender, EventArgs e)
        {
            MarkPoint.X = MarkPoint.X - 1;// (nScale / MM_DISPLAY01.Zoom); ;
      //      UPdate_Display();
        }

        private void BTN_MOVE_RIGHT_Click(object sender, EventArgs e)
        {
            MarkPoint.X = MarkPoint.X + 1;//(nScale / MM_DISPLAY01.Zoom);
        //    UPdate_Display();
        }

        private void BTN_MOVE_DOWN_Click(object sender, EventArgs e)
        {
            MarkPoint.Y = MarkPoint.Y + 1;// (nScale / MM_DISPLAY01.Zoom);
         //   UPdate_Display();
        }

        private void MM_DISPLAY01_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LB_MARKPOS.Text = "X:" + MarkPoint.X.ToString("0.0000") + " ," + "Y:" + MarkPoint.Y.ToString("0.0000");
            MM_DISPLAY00.Image = Main.vision.CogCamBuf[m_CamNo];


             if (!Main.DEFINE.OPEN_F)
             {
                 if (PLCDataTag.RData[Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH] == 0)
                        BTN_EXIT_Click(null, null);
             }
        } 

        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            double ntempX = 0,ntempY = 0;

            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(MarkPoint.X, MarkPoint.Y, ref ntempX, ref ntempY);
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_RobotPosX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosX = ntempX;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_RobotPosY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosY = ntempY;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_Pixel[Main.DEFINE.X] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.X] = MarkPoint.X;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_Pixel[Main.DEFINE.Y] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.Y] = MarkPoint.Y;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.ManuMathchFlag = true;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.SearchResult = true;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.Score = 1.01;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SearchResult = true;
            Main.AlignUnit[m_AlignNo].m_ManualMatchResult = true;
            BTN_EXIT_Click(null,null);
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH, 0);
//             while(true)
//             {
            //                 if (PLCDataTag.RData[Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH] == 0)
//                     break; // 0을 적고 나갔는데 다음 못찾는 패턴때문에 5를쓸때
//             }
            timer1.Enabled = false;
            //Main.AlignUnit[m_AlignNo].m_ManualMatchFlag = false;
            MM_DISPLAY00.DisplayClear();
            this.Hide();
        }

        private void Form_RCS_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;
            timer1.Dispose();
        }

        private void Form_RCS_Load(object sender, EventArgs e)
        {
            MM_DISPLAY00.Resuloution = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CalX[0];
            m_iScaleBarSize = 500;
            m_iScaleBarPixel = (int)((double)m_iScaleBarSize / MM_DISPLAY00.Resuloution);

            Initial_Form();
        }

        private void RBTN_RCS_POINT_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }

        private void RBTN_RCS_POINT_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            int m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));

            if (TempBTN.Checked)
            {
                m_iSelPoint = m_Number;
                Update_Display();
            }
        }

        private void LB_SCALE_Click(object sender, EventArgs e)
        {
            using (Form_KeyPad form_keypad = new Form_KeyPad(0, 10000, m_iScaleBarSize, "SCALE BAR SIZE", 1))
            {
                form_keypad.ShowDialog();
                m_iScaleBarSize = (int)form_keypad.m_data;
                m_iScaleBarPixel = (int)((double)m_iScaleBarSize / MM_DISPLAY00.Resuloution);
                
                LB_SCALE.Text = m_iScaleBarSize.ToString() + " um";
            }
            if (m_bShowScaleBar == true)
            {
                MM_DISPLAY00.CogDisplay00.InteractiveGraphics.Remove("SB");
                CogLineScaleBar.SetStartLengthRotation(CogLineScaleBar.StartX, CogLineScaleBar.StartY, m_iScaleBarPixel, CogLineScaleBar.Rotation);
                MM_DISPLAY00.CogDisplay00.InteractiveGraphics.Add(CogLineScaleBar as ICogGraphicInteractive, "SB", true);
            }
        }

        private void BTN_RCS_SCALE_BAR_Click(object sender, EventArgs e)
        {
            CogLineScaleBar.SetStartEnd(100,100,100+m_iScaleBarPixel,100);
            CogLineScaleBar.GraphicDOFEnable = CogLineSegmentDOFConstants.Position; //| CogRectangleAffineDOFConstants.Rotation        
            CogLineScaleBar.Interactive = true;
            if (m_bShowScaleBar == false)
            {
                MM_DISPLAY00.CogDisplay00.InteractiveGraphics.Add(CogLineScaleBar as ICogGraphicInteractive, "SB", true);
                m_bShowScaleBar = true;
            }
            else
            {
                m_bShowScaleBar = false;
                MM_DISPLAY00.CogDisplay00.InteractiveGraphics.Remove("SB");
            }
        }

        private void BTN_RCS_POINT_OK_Click(object sender, EventArgs e)
        {

        }

        private void BTN_RCS_OK_Click(object sender, EventArgs e)
        {
            Main.Status.MC_MODE = Main.DEFINE.MC_MAINFORM;
            BTN_EXIT_Click(null, null);
        }

        private void BTN_RCS_NG_Click(object sender, EventArgs e)
        {
            BTN_EXIT_Click(null, null);
        }

        private void BTN_RCS_ROTATE_SCALE_BAR_Click(object sender, EventArgs e)
        {
            if (CogLineScaleBar.Rotation == 0)
                CogLineScaleBar.SetStartLengthRotation(CogLineScaleBar.StartX, CogLineScaleBar.StartY, CogLineScaleBar.Length, 45 * Main.DEFINE.radian);
            else if (CogLineScaleBar.Rotation > 0 && CogLineScaleBar.Rotation < 1)
                CogLineScaleBar.SetStartLengthRotation(CogLineScaleBar.StartX, CogLineScaleBar.StartY, CogLineScaleBar.Length, 90 * Main.DEFINE.radian);
            else
                CogLineScaleBar.SetStartLengthRotation(CogLineScaleBar.StartX, CogLineScaleBar.StartY, CogLineScaleBar.Length, 0);
        }

        private void BTN_RCS_COLOR_SCALE_BAR_Click(object sender, EventArgs e)
        {
            if (CogLineScaleBar.Color == CogColorConstants.Red)
                CogLineScaleBar.Color = CogColorConstants.Green;
            else if (CogLineScaleBar.Color == CogColorConstants.Green)
                CogLineScaleBar.Color = CogColorConstants.Blue;
            else
                CogLineScaleBar.Color = CogColorConstants.Red;

        }
    }
}

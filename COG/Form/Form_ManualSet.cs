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
    public partial class Form_ManualSet : Form
    {
        const int UI_FOCUS_POINT_MARKER = 0;
        const int UI_FOCUS_HORIZONTAL_LINE = 1;
        const int UI_FOCUS_VERTICAL_LINE = 2;

        public int m_AlignNo;
        private int m_PatTagNo;
        private int m_PatNo;
        private int m_CamNo;
        private static MTickTimer m_Timer = new MTickTimer();
     //   public bool nResult = false;

//         double MarkPos_X;
//         double MarkPos_Y;
        int nCrossSize = 500;
        double nScale = 1.00000000000000000000000;
        int nLastUIFocus = UI_FOCUS_POINT_MARKER;

        CogPointMarker MarkPoint = new CogPointMarker();
        CogLine mCogLineH = new CogLine();
        CogLine mCogLineV = new CogLine();
        CogIntersectLineLineTool[] mCrossLineTool = new CogIntersectLineLineTool[3];

        //     CogCoordinateAxes MarkPoint = new CogCoordinateAxes();

        /*        CogLine mCogLine = new CogLine();*/
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
        public Form_ManualSet()
        {
            InitializeComponent();
//             m_AlignNo = nAlignNum;
//             m_PatNo = nPatNum;

        }
        private void Initial_Form()
        {
            MarkPoint.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
            MarkPoint.SizeInScreenPixels = nCrossSize; //화면에 표시 되는 + 모양 크기 . 
            MarkPoint.LineStyle = CogGraphicLineStyleConstants.Dot;
            MarkPoint.SelectedLineWidthInScreenPixels = 3;
            //             MarkPoint.GraphicDOFEnable = CogCoordinateAxesDOFConstants.All;
            //             MarkPoint.DisplayedXAxisLength = nCrossSize; //화면에 표시 되는 + 모양 크기 . 


            //     MarkPoint.DragLineWidthInScreenPixels = nCrossSize; //화면에 표시 되는 + 모양 크기 . 

            
            mCogLineH.Color = CogColorConstants.Green;
            mCogLineV.Color = CogColorConstants.Green;

            mCogLineH.Interactive = true;
            mCogLineH.GraphicDOFEnable = CogLineDOFConstants.Position;
            mCogLineV.Interactive = true;
            mCogLineV.GraphicDOFEnable = CogLineDOFConstants.Position;

            MarkPoint.Interactive = true;
            MM_DisplayToolbar01.Display   = MM_DISPLAY01;
            MM_DisplayStatusBar01.Display = MM_DISPLAY01;
            MM_DisplayStatusBar01.CoordinateSpaceName = "*\\#\\@";
        }
        private void Form_ManualSet_Load(object sender, EventArgs e)
        {
            m_PatTagNo = Main.AlignUnit[m_AlignNo].m_ManualMatchPatTagNum;
            m_PatNo = Main.AlignUnit[m_AlignNo].m_ManualMatchPatNum;
            m_CamNo = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo,m_PatNo].m_CamNo;
            Main.AlignUnit[m_AlignNo].m_ManualMatchResult = false;

            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                // 통신?
            }
            //else
                //YSH 임의주석, 필요 시 추가
                //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH, 5);

            Initial_Form();
            LB_ALIGNUNIT_NAME.Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo,m_PatNo].m_PatternName.ToString();
            MM_DISPLAY01.InteractiveGraphics.Clear();
            MM_DISPLAY01.Image = Main.vision.CogCamBuf[m_CamNo];
            MM_DISPLAY01.Fit(true);

            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].m_bInterResult == true)
                {
                    mCogLineH.SetFromStartXYEndXY(0, (double)Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.Y], (double)Main.vision.IMAGE_SIZE_X[m_CamNo], (double)Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.Y]);
                    mCogLineV.SetFromStartXYEndXY((double)Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.X], 0, (double)Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.X], (double)Main.vision.IMAGE_SIZE_Y[m_CamNo]);
                }
                else
                {
                    mCogLineH.SetFromStartXYEndXY(0, (double)Main.vision.CogCamBuf[m_CamNo].Height / 2, (double)Main.vision.IMAGE_SIZE_X[m_CamNo], (double)Main.vision.CogCamBuf[m_CamNo].Height / 2);
                    mCogLineV.SetFromStartXYEndXY((double)Main.vision.CogCamBuf[m_CamNo].Width / 2, 0, (double)Main.vision.CogCamBuf[m_CamNo].Width / 2, (double)Main.vision.IMAGE_SIZE_Y[m_CamNo]);
                }
                MM_DISPLAY01.InteractiveGraphics.Add(mCogLineH as ICogGraphicInteractive, "Force Line", false);
                MM_DISPLAY01.InteractiveGraphics.Add(mCogLineV as ICogGraphicInteractive, "Force Line", false);
                nLastUIFocus = UI_FOCUS_HORIZONTAL_LINE;

                if (Main.AlignUnit[m_AlignNo].m_ManualMatchLineType > 0)
                    BTN_CORNER_RETRY.Visible = true;
            }
            else
            {
                MarkPoint.X = (double)Main.vision.CogCamBuf[m_CamNo].Width / 2;
                MarkPoint.Y = (double)Main.vision.CogCamBuf[m_CamNo].Height / 2;
                MM_DISPLAY01.InteractiveGraphics.Add(MarkPoint as ICogGraphicInteractive, "Find MarkerPos", false);
                nLastUIFocus = UI_FOCUS_POINT_MARKER;
                BTN_CORNER_RETRY.Visible = false;
            }

            if (Main.DEFINE.PROGRAM_TYPE != "QD_LPA_PC1" && Main.DEFINE.PROGRAM_TYPE != "QD_LPA_PC2")
                CmdCheck(Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH);

           if (!Main.DEFINE.OPEN_F && !Main.DEFINE.OPEN_CAM)
               timer1.Enabled = true;
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
        private void UPdate_Display()
        {
//             MarkPoint.X = MarkPos_X;
//             MarkPoint.Y = MarkPos_Y;          
//             MarkPoint.Transform.TranslationX = MarkPos_X;
//             MarkPoint.Transform.TranslationY = MarkPos_Y;     
       //     MM_DISPLAY01.InteractiveGraphics.Add(MarkPoint as ICogGraphicInteractive, "Find MarkerPos", false);

        }


        private void BTN_MOVE_UP_Click(object sender, EventArgs e)
        {
            if (nLastUIFocus == UI_FOCUS_HORIZONTAL_LINE || nLastUIFocus == UI_FOCUS_VERTICAL_LINE)
                mCogLineH.Y = mCogLineH.Y - (nScale / MM_DISPLAY01.Zoom);
            else
                MarkPoint.Y = MarkPoint.Y - (nScale / MM_DISPLAY01.Zoom);
        //    UPdate_Display();
        }

        private void BTN_MOVE_LEFT_Click(object sender, EventArgs e)
        {
            if (nLastUIFocus == UI_FOCUS_HORIZONTAL_LINE || nLastUIFocus == UI_FOCUS_VERTICAL_LINE)
                mCogLineV.X = mCogLineV.X - (nScale / MM_DISPLAY01.Zoom);
            else
                MarkPoint.X = MarkPoint.X - (nScale / MM_DISPLAY01.Zoom);
      //      UPdate_Display();
        }

        private void BTN_MOVE_RIGHT_Click(object sender, EventArgs e)
        {
            if (nLastUIFocus == UI_FOCUS_HORIZONTAL_LINE || nLastUIFocus == UI_FOCUS_VERTICAL_LINE)
                mCogLineV.X = mCogLineV.X + (nScale / MM_DISPLAY01.Zoom);
            else
                MarkPoint.X = MarkPoint.X + (nScale / MM_DISPLAY01.Zoom);
        //    UPdate_Display();
        }

        private void BTN_MOVE_DOWN_Click(object sender, EventArgs e)
        {
            if (nLastUIFocus == UI_FOCUS_HORIZONTAL_LINE || nLastUIFocus == UI_FOCUS_VERTICAL_LINE)
                mCogLineH.Y = mCogLineH.Y + (nScale / MM_DISPLAY01.Zoom);
            else
                MarkPoint.Y = MarkPoint.Y + (nScale / MM_DISPLAY01.Zoom);
         //   UPdate_Display();
        }

        private void MM_DISPLAY01_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                if (nLastUIFocus == UI_FOCUS_HORIZONTAL_LINE || nLastUIFocus == UI_FOCUS_VERTICAL_LINE)
                    LB_MARKPOS.Text = "X:" + mCogLineV.X.ToString("0.0000") + " ," + "Y:" + mCogLineH.Y.ToString("0.0000");
            }
            else
            { 
                LB_MARKPOS.Text = "X:" + MarkPoint.X.ToString("0.0000") + " ," + "Y:" + MarkPoint.Y.ToString("0.0000");
            }

            MM_DISPLAY01.Image = Main.vision.CogCamBuf[m_CamNo];

            if (!Main.DEFINE.OPEN_F)
            {
                if (Main.DEFINE.PROGRAM_TYPE != "QD_LPA_PC1" && Main.DEFINE.PROGRAM_TYPE != "QD_LPA_PC2")
                {
                    if (PLCDataTag.RData[Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH] == 0)
                        BTN_EXIT_Click(null, null);
                }
            }
        } 

        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            double ntempX = 0,ntempY = 0;
            string strLog = "";
            Main.DoublePoint TempDP = new Main.DoublePoint();
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                TempDP.X = mCogLineV.X;
                TempDP.Y = mCogLineH.Y;

                if (Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == -1)
                {
                    if (TempDP.X >= 0 && TempDP.X <= Main.vision.IMAGE_SIZE_X[m_CamNo] && TempDP.Y >= 0 && TempDP.Y <= Main.vision.IMAGE_SIZE_Y[m_CamNo])
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(TempDP.X, TempDP.Y, ref ntempX, ref ntempY);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_RobotPosX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosX = ntempX;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_RobotPosY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosY = ntempY;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_Pixel[Main.DEFINE.X] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.X] = TempDP.X;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.m_Pixel[Main.DEFINE.Y] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.Y] = TempDP.Y;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.ManuMathchFlag = true;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.SearchResult = true;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].PMAlignResult.Score = 1.01;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SearchResult = true;
                        Main.AlignUnit[m_AlignNo].m_ManualMatchResult = true;
                    }
                    Main.AlignUnit[m_AlignNo].m_ManualMatchResult = false;
                }
                else if (Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == 0)
                {
                    if (TempDP.X >= 0 && TempDP.X <= Main.vision.IMAGE_SIZE_X[m_CamNo] && TempDP.Y >= 0 && TempDP.Y <= Main.vision.IMAGE_SIZE_Y[m_CamNo])
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].CrossPointList.Add(TempDP);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(TempDP.X, TempDP.Y, ref ntempX, ref ntempY);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].CrossPointList[0].X2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosX = ntempX;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].CrossPointList[0].Y2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosY = ntempY;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.X] = TempDP.X;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.Y] = TempDP.Y;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].m_bInterResult = true;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].SearchResult = true;
                        Main.AlignUnit[m_AlignNo].m_ManualMatchResult = true;

                        strLog = "X*Y (" + ntempX.ToString("0.000") + ", " + ntempY.ToString("0.000") + ") ";
                        Main.AlignUnit[m_AlignNo].LogdataDisplay(strLog, true);

                        Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.LineX;
                        Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.LineY;
                        Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.CrossXY;
                    }
                }
                else
                {
                    BTN_CORNER_RETRY_Click(BTN_CORNER_RETRY, null);
                }

                for (int i = 0; i < Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults.Length; i++)
                {
                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList.Count <= 0)
                    {
                        Main.DoublePoint Temp = new Main.DoublePoint();
                        Temp.X = 0;
                        Temp.Y = 0;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList.Add(Temp);
                    }
                }
            }
            else
            {
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
            }
            BTN_EXIT_Click(null,null);
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                //if (m_PatNo == Main.DEFINE.CAM_SELECT_ALIGN)
                //{
                //    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_X), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].CrossPointList[0].X2 * 1000));
                //    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_Y), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].CrossPointList[0].Y2 * 1000));
                //    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_R_T), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[3].CrossPointList[0].X2 * 1000));
                //    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_SCORE), 0);

                //    if (Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == 1)   // C-Cut
                //    {
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_X), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[1].CrossPointList[0].X2 * 1000));
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_Y), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[1].CrossPointList[0].Y2 * 1000));
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_R_T), 0);
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_SCORE), 0);

                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH3_X), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].CrossPointList[0].X2 * 1000));
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH3_Y), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].CrossPointList[0].Y2 * 1000));
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH3_R_T), 0);
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH3_SCORE), 0);
                //    }
                //    else if (Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == 2)    // R cut
                //    {
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_X), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_RobotPosX * 1000));
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_Y), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_RobotPosY * 1000));
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_R_T), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_RobotRadius * 1000));
                //        Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH2_SCORE), 0);
                //    }
                //}

                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].ManuMatchComplete = true;
                //Main.CCLink_PutBit(Main.DEFINE.CCLINK_OUT_STAGE1_CAM_FORCE_GRAB_COMP, true);
                //Main.CCLink_OffCheckOffHandshake(Main.DEFINE.CCLINK_IN_STAGE1_CAM_FORCE_GRAB_REQ, Main.DEFINE.CCLINK_OUT_STAGE1_CAM_FORCE_GRAB_COMP, Main.DEFINE.NORMAL_HANDSHAKE_TIMEOUT, m_AlignNo);
            }
            else if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                //if (Main.CCLink_IsBit(Main.DEFINE.CCLINK2_IN_FIRST_ALIGN_FORCE_GRAB_REQ))
                //{
                //    Main.CCLink_PutBit(Main.DEFINE.CCLINK2_OUT_FIRST_ALIGN_FORCE_GRAB_COMP, true);
                //    Main.CCLink_OffCheckOffHandshake(Main.DEFINE.CCLINK2_IN_FIRST_ALIGN_FORCE_GRAB_REQ, Main.DEFINE.CCLINK2_OUT_FIRST_ALIGN_FORCE_GRAB_COMP, Main.DEFINE.NORMAL_HANDSHAKE_TIMEOUT, m_AlignNo);
                //}
                //else
                if (Main.CCLink_IsBit(Main.DEFINE.CCLINK2_IN_PRE_ALIGN1_FORCE_GRAB_REQ))
                {
                    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_X), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].PMAlignResult.m_RobotPosX * 1000));    // Left Mark X
                    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_Y), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].PMAlignResult.m_RobotPosY * 1000));    // Left Mark Y
                    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_R_T), 0);
                    Main.CCLink_WriteDWord((ushort)(Main.AlignUnit[m_AlignNo].UNIT_RES_DATA_ADDR + Main.DEFINE.SEARCH1_SCORE), (int)(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].PMAlignResult.Score * 1000));

                    Main.CCLink_PutBit(Main.DEFINE.CCLINK2_OUT_PRE_ALIGN1_FORCE_GRAB_COMP, true);
                    Main.CCLink_OffCheckOffHandshake(Main.DEFINE.CCLINK2_IN_PRE_ALIGN1_FORCE_GRAB_REQ, Main.DEFINE.CCLINK2_OUT_PRE_ALIGN1_FORCE_GRAB_COMP, Main.DEFINE.NORMAL_HANDSHAKE_TIMEOUT, m_AlignNo);
                }
                //else if (Main.CCLink_IsBit(Main.DEFINE.CCLINK2_IN_PRE_ALIGN2_FORCE_GRAB_REQ))
                //{
                //    Main.CCLink_PutBit(Main.DEFINE.CCLINK2_OUT_PRE_ALIGN2_FORCE_GRAB_COMP, true);
                //    Main.CCLink_OffCheckOffHandshake(Main.DEFINE.CCLINK2_IN_PRE_ALIGN2_FORCE_GRAB_REQ, Main.DEFINE.CCLINK2_OUT_PRE_ALIGN2_FORCE_GRAB_COMP, Main.DEFINE.NORMAL_HANDSHAKE_TIMEOUT, m_AlignNo);
                //}
            }
            //else
                //YSH 임의주석, 필요 시 추가
                //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH, 0);
                //             while(true)
                //             {
                //                 if (PLCDataTag.RData[Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.MANUAL_MATCH] == 0)
                //                     break; // 0을 적고 나갔는데 다음 못찾는 패턴때문에 5를쓸때
                //             }
                timer1.Enabled = false;
            Main.AlignUnit[m_AlignNo].m_ManualMatchFlag = false;
            MM_DISPLAY01.InteractiveGraphics.Clear();
            this.Hide();
        }

        private void Form_ManualSet_FormClosed(object sender, FormClosedEventArgs e)
        {
//             timer1.Dispose();
//             MM_DISPLAY01.Dispose();
//             MM_DisplayStatusBar01.Dispose();
//             MM_DisplayToolbar01.Dispose();
        }

        private void BTN_CORNER_RETRY_Click(object sender, EventArgs e)
        {
            Main.DoublePoint TempDP = new Main.DoublePoint();
            TempDP.X = mCogLineV.X;
            TempDP.Y = mCogLineH.Y;
            string strLog = "";

            for (int i = 0; i < Main.DEFINE.FINDLINE_MAX - 1; i++)
            {
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList.Clear();
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].m_bInterSearched = false;
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].m_bInterResult = false;
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].m_bLoadingLimitOver_X = false;
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].m_bLoadingLimitOver_Y = false;
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].m_FoundSubLineNum = -1;

                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].Pixel[Main.DEFINE.X] = 0;
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].Pixel[Main.DEFINE.Y] = 0;

                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].SearchResult = false;
            }

            // 1. X, Y 교점 (모든 경우에 필요)
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].m_bInterSearched = true;

            if (TempDP.X >= 0 && TempDP.X <= Main.vision.IMAGE_SIZE_X[m_CamNo] && TempDP.Y >= 0 && TempDP.Y <= Main.vision.IMAGE_SIZE_Y[m_CamNo])
            {
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].m_bInterResult = true;

                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].CrossPointList.Add(TempDP);

                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.X] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].Pixel[Main.DEFINE.X] = TempDP.X;
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].Pixel[Main.DEFINE.Y] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[0].Pixel[Main.DEFINE.Y] = TempDP.Y;

                Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.LineX;
                Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.LineY;
                Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.CrossXY;

                // Diag Position Correction (Shape C)
                {
                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLinePara[Main.DEFINE.MAIN, 2].m_UseCheck)
                    {
                        // Position correction
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].RunParams.ExpectedLineSegment.StartX = TempDP.X + Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLinePara[Main.DEFINE.MAIN, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].RunParams.ExpectedLineSegment.StartY = TempDP.Y + Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLinePara[Main.DEFINE.MAIN, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y;

                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].RunParams.ExpectedLineSegment.EndX = TempDP.X + Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLinePara[Main.DEFINE.MAIN, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X2;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].RunParams.ExpectedLineSegment.EndY = TempDP.Y + Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLinePara[Main.DEFINE.MAIN, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y2;
                        }

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Run();
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].FindLineTool = new CogFindLineTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2]);

                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results != null && Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.Count > 0
                            && Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLine() != null)
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].SearchResult = true;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].Pixel[Main.DEFINE.X] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLineSegment().MidpointX;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].Pixel[Main.DEFINE.Y] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLineSegment().MidpointY;

                            //MM_DISPLAY01.InteractiveGraphics.Remove("Diag Line");
                            for (int i = 0; i < MM_DISPLAY01.InteractiveGraphics.ZOrderGroups.Count; i++)
                            {
                                if (MM_DISPLAY01.InteractiveGraphics.ZOrderGroups[i] == "Diag Line")
                                {
                                    MM_DISPLAY01.InteractiveGraphics.Remove("Diag Line");
                                }
                            }
                            MM_DISPLAY01.InteractiveGraphics.Add(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLine() as ICogGraphicInteractive, "Diag Line", false);

                            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                            {
                                Main.DoublePoint Temp = new Main.DoublePoint();
                                Temp.X2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLineSegment().Rotation * Main.DEFINE.degree;
                                Temp.X2 = (90 - Temp.X2) * (-1);
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].CrossPointList.Add(Temp);

                                //Ret |= (ushort)(1 << 2);
                            }

                            if (m_PatNo == Main.DEFINE.CAM_SELECT_INSPECT)
                            {
                                // Vision에서 Inspection Size 구하기 위함
                                if (Main.CCLink_IsBit(Main.DEFINE.CCLINK_IN_STAGE1_FINE_ALIGN_MODE))
                                {
                                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionPosRobot_X[2] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].Pixel[Main.DEFINE.X];    // Glass Line 좌표
                                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionPosRobot_Y[2] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].Pixel[Main.DEFINE.Y];
                                }
                                else if (Main.CCLink_IsBit(Main.DEFINE.CCLINK_IN_STAGE1_INSPECTION_MODE))
                                {
                                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionSizeRobot_X[2] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLineSegment().DistanceToPoint(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionPosRobot_X[2], Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionPosRobot_Y[2]);    // Cutting POL Line Gap Size
                                }
                            }
                        }
                        else
                        {
                            //Ret = false;
                        }
                    }
                    else
                    {
                        // No FINDLinePara[2].m_UseCheck error
                    }
                }


                // 2. X, Diag 교점
                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" && Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == 1
                    && (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results != null
                    && Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.Count > 0
                    && Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLine() != null))
                {
                    mCrossLineTool[1] = new CogIntersectLineLineTool();
                    mCrossLineTool[1].InputImage = MM_DISPLAY01.Image;
                    mCrossLineTool[1].LineA = mCogLineH;
                    mCrossLineTool[1].LineB = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLine();
                    mCrossLineTool[1].Run();
                    if (mCrossLineTool[1].Intersects)
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[1].m_bInterSearched = true;

                        if (mCrossLineTool[1].X >= 0 && mCrossLineTool[1].X <= mCrossLineTool[1].InputImage.Width && mCrossLineTool[1].Y >= 0
                            && mCrossLineTool[1].Y <= mCrossLineTool[1].InputImage.Height)
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[1].m_bInterResult = true;
                            //Ret &= true;
                            Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.CrossXD;

                            Main.DoublePoint Temp = new Main.DoublePoint();
                            Temp.X = /*FINDLineResults[0].Pixel[DEFINE.X] = Pixel[DEFINE.X] = PixelFindLine[DEFINE.X] =*/ (mCrossLineTool[1].X);
                            Temp.Y = /*FINDLineResults[0].Pixel[DEFINE.Y] = Pixel[DEFINE.Y] = PixelFindLine[DEFINE.Y] =*/ (mCrossLineTool[1].Y);
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[1].CrossPointList.Add(Temp);
                        }
                    }
                }

                // 3. Y, Diag 교점
                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" && Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == 1
                    && (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results != null
                    && Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.Count > 0
                    && Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLine() != null))
                {
                    mCrossLineTool[2] = new CogIntersectLineLineTool();
                    mCrossLineTool[2].InputImage = MM_DISPLAY01.Image;
                    mCrossLineTool[2].LineA = mCogLineV;
                    mCrossLineTool[2].LineB = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineTools[Main.DEFINE.MAIN, 2].Results.GetLine();
                    mCrossLineTool[2].Run();
                    if (mCrossLineTool[2].Intersects)
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].m_bInterSearched = true;

                        if (mCrossLineTool[2].X >= 0 && mCrossLineTool[2].X <= mCrossLineTool[2].InputImage.Width && mCrossLineTool[2].Y >= 0 && mCrossLineTool[2].Y <= mCrossLineTool[2].InputImage.Height)
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].m_bInterResult = true;
                            //Ret &= true;
                            Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.CrossYD;

                            Main.DoublePoint Temp = new Main.DoublePoint();
                            Temp.X = /*FINDLineResults[0].Pixel[DEFINE.X] = Pixel[DEFINE.X] = PixelFindLine[DEFINE.X] =*/ (mCrossLineTool[2].X);
                            Temp.Y = /*FINDLineResults[0].Pixel[DEFINE.Y] = Pixel[DEFINE.Y] = PixelFindLine[DEFINE.Y] =*/ (mCrossLineTool[2].Y);
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[2].CrossPointList.Add(Temp);
                        }
                    }
                }


                // R-Cut
                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" && Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == 2)
                {
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].Pixel[Main.DEFINE.X] = 0;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].Pixel[Main.DEFINE.Y] = 0;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].R = 0;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_UseCheck = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CirclePara[0].m_UseCheck;

                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CirclePara[0].m_UseCheck)
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].InputImage = MM_DISPLAY01.Image as CogImage8Grey;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].RunParams.ExpectedCircularArc.CenterX = TempDP.X + Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CirclePara[0].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].RunParams.ExpectedCircularArc.CenterY = TempDP.Y + Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CirclePara[0].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].Run();

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].CircleTool = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleToolCopy(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0]);

                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].Results == null || Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].Results.Count <= 0)
                        {
                            //Ret = false;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].SearchResult = false;
                        }
                        else
                        {
                            Main.AlignUnit[m_AlignNo].m_ManualResult |= (ushort)Main.AlignUnitTag.FindLineConstants.CircleR;

                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].SearchResult = true;

                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].Pixel[Main.DEFINE.XPOS] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].Results.GetCircle().CenterX;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].Pixel[Main.DEFINE.YPOS] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].Results.GetCircle().CenterY;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].R = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleTools[0].Results.GetCircle().Radius;
                        }//if
                    }
                }

                double dTempScalar = 0, dTempV2R_X = 0, dTempV2R_Y = 0;
                if (Main.CCLink_IsBit(Main.DEFINE.CCLINK_IN_STAGE1_INSPECTION_MODE))
                {
                    
                    for (int i = 0; i < 3; i++)
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2RScalar(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionSizeRobot_X[i], ref dTempScalar);

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionSizeRobot_X[i] = dTempScalar;
                    }
                }

                for (int i = 0; i < Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults.Length; i++)
                {
                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList.Count > 0)
                    {
                        if (i == 3) // POL Edge
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2RScalar(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].X, ref dTempScalar);
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].X2 = dTempScalar;
                        }
                        else if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].X,
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].Y, ref dTempV2R_X, ref dTempV2R_Y);

                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].X2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosX = dTempV2R_X;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].Y2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosY = dTempV2R_Y;

                            if (i == 1)
                            {
                                strLog = "X*C (" + dTempV2R_X.ToString("0.000") + ", " + dTempV2R_Y.ToString("0.000") + ") ";
                                Main.AlignUnit[m_AlignNo].LogdataDisplay(strLog, true);
                            }
                            else if (i == 2)
                            {
                                strLog = "Y*C (" + dTempV2R_X.ToString("0.000") + ", " + dTempV2R_Y.ToString("0.000") + ") ";
                                Main.AlignUnit[m_AlignNo].LogdataDisplay(strLog, true);
                            }
                        }
                        else if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                        {
                            if (i > 1) continue;

                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2RByCAL(
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].X,
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].Y, ref dTempV2R_X, ref dTempV2R_Y);

                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].X2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosX = dTempV2R_X;
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList[0].Y2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_RobotPosY = dTempV2R_Y;
                        }
                    }
                    else
                    {
                        Main.DoublePoint Temp = new Main.DoublePoint();
                        Temp.X = 0;
                        Temp.Y = 0;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].FINDLineResults[i].CrossPointList.Add(Temp);
                    }
                }

                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults.Length > 0 && Main.AlignUnit[m_AlignNo].m_ManualMatchLineType == 2)
                {
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].Pixel[Main.DEFINE.XPOS],
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].Pixel[Main.DEFINE.YPOS], ref dTempV2R_X, ref dTempV2R_Y);
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_RobotPosX = dTempV2R_X;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_RobotPosY = dTempV2R_Y;

                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2RScalar(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].R,
                        ref Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_RobotRadius);

                    if (Main.CCLink_IsBit(Main.DEFINE.CCLINK_IN_STAGE1_INSPECTION_MODE))
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].InspectionSizeRobot_X[2] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CircleResults[0].m_RobotRadius;
                    }
                }

            }   // Manual X, Y are OK
        }   // BTN_CORNER_RETRY_Click
    }
}

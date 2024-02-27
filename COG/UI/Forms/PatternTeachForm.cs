using COG.Class;
using COG.Class.Units;
using COG.Core;
using COG.Helper;
using COG.Settings;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Implementation;
using Cognex.VisionPro.LineMax;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.SearchMax;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace COG.UI.Forms
{
    public partial class PatternTeachForm : Form
    {
        public const int ORIGIN_SIZE = 120;

        private bool _isFormLoad { get; set; } = false;

        private bool _selectedAmpMark { get; set; } = false;

        private int _selectedMarkIndex { get; set; } = 0; // 0 : Main, 1~ : Sub

        public int StageUnitNo { get; set; } = 0;

        public bool IsLeft { get; set; } = false;

        private double ZoomBackup { get; set; } = 0;

        private List<CogRecordDisplay> MarkDisplayList = new List<CogRecordDisplay>();

        private List<Label> MarkLabelList = new List<Label>();

        private CogImageFileTool DisplayImageTool = new CogImageFileTool();

        private CogRecordDisplay CogDisplay = null;

        private ICogImage CogDisplayImage { get; set; } = null;

        private CogPointMarker OriginMarkPoint { get; set; } = null;

        public PatternTeachForm()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);

       
            CogDisplay = new CogRecordDisplay();
            CogDisplay.MouseUp += new MouseEventHandler(Display_MauseUP);
            CogDisplay = PT_DISPLAY_CONTROL.CogDisplay00;
            CogDisplay.Changed += PT_Display01_Changed;

            PT_DisplayToolbar01.Display = CogDisplay;
            PT_DisplayStatusBar01.Display = CogDisplay;
        }

        private void InitializeUI()
        {
            BTN_LIVEMODE.Checked = false;
            BTN_LIVEMODE.BackColor = Color.DarkGray;

            this.TopMost = false;

            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                string ntempSub;
                if (i == 0)
                    ntempSub = "MAIN_PAT" + i.ToString();
                else
                    ntempSub = "SUB__PAT" + i.ToString();
                CB_SUB_PATTERN.Items.Add(ntempSub);
            }

            CB_SUB_PATTERN.SelectedIndex = 0;

        }

        private void PatternTeachForm_Load(object sender, EventArgs e)
        {
            if (StaticConfig.VirtualMode)
                BTN_IMAGE_OPEN.Visible = true;

            _isFormLoad = true;
            _selectedAmpMark = true;
            _selectedMarkIndex = 0;

            InitializeUI();

            TeachingData.Instance().UpdateTeachingData();

            InspModel inspModel = ModelManager.Instance().CurrentModel;

            if(inspModel != null)
            {
                this.Text = TeachingData.Instance().GetStageUnit(StageUnitNo).Name;
                LoadAmpMark();

                UpdateMarkData();
            }

            UpdateMarkInfo();
            ClearDisplayGraphic();
            ClearMarkButtonBackColor();

            
        }

        private void LoadAmpMark()
        {
            MarkDisplayList.Clear();
            MarkLabelList.Clear();
          
            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                string controlName = "PT_SubDisplay_" + i.ToString("00");
                CogRecordDisplay display = (CogRecordDisplay)this.Controls["TABC_MANU"].Controls["TAB_00"].Controls[controlName];
                display.Visible = true;

                MarkDisplayList.Add(display);

                controlName = "LB_PATTERN_" + i.ToString("00");
                Label label = (Label)this.Controls["TABC_MANU"].Controls["TAB_00"].Controls[controlName];
                label.Visible = true;
                MarkLabelList.Add(label);
            }
        }

        private void UpdateMarkData()
        {
            if (GetUnit() is Unit unit)
            {
                var markUnit = GetMarkUnit();
                for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
                {
                    var display = MarkDisplayList[i];
                    display.StaticGraphics.Clear();
                    display.InteractiveGraphics.Clear();

                    var markTagList = markUnit.TagList;
                    var markTool = markTagList[i].Tool;
                    if (markTool != null)
                    {
                        if(markTool.Pattern.Trained)
                        {
                            CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();

                            if (markTool.Pattern.GetTrainedPatternImageMask() is ICogImage cogImage)
                            {
                                display.Image = markTool.Pattern.GetTrainedPatternImage();

                                var maskGraphic = CreateMaskGraphic(cogImage as CogImage8Grey);
                                PatternInfo.Add(maskGraphic);


                                CogRectangle trainRegion = new CogRectangle(markTool.Pattern.TrainRegion as CogRectangle);
                                trainRegion.GraphicDOFEnable = CogRectangleDOFConstants.Position;
                                trainRegion.Interactive = false;
                                PatternInfo.Add(trainRegion);

                                CogCoordinateAxes orgin = new CogCoordinateAxes();
                                orgin.LineStyle = CogGraphicLineStyleConstants.Dot;
                                orgin.Transform.TranslationX = markTool.Pattern.Origin.TranslationX;
                                orgin.Transform.TranslationY = markTool.Pattern.Origin.TranslationY;
                                orgin.GraphicDOFEnable = CogCoordinateAxesDOFConstants.Position;
                                PatternInfo.Add(orgin);

                                display.InteractiveGraphics.AddList(PatternInfo, "Pattern", false);
                            }
                        }
                        else
                            display.Image = null;
                    }
                }
            }
        }

        private CogMaskGraphic CreateMaskGraphic(CogImage8Grey mask)
        {
            CogMaskGraphic cogMaskGraphic = new CogMaskGraphic();
            for (short index = 0; index < (short)256; ++index)
            {
                CogColorConstants cogColorConstants;
                CogMaskGraphicTransparencyConstants transparencyConstants;
                if (index < (short)64)
                {
                    cogColorConstants = CogColorConstants.DarkRed;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.Half;
                }
                else if (index < (short)128)
                {
                    cogColorConstants = CogColorConstants.Yellow;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.Half;
                }
                else if (index < (short)192)
                {
                    cogColorConstants = CogColorConstants.Red;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.None;
                }
                else
                {
                    cogColorConstants = CogColorConstants.Yellow;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.Full;
                }
                cogMaskGraphic.SetColorMap((byte)index, cogColorConstants);
                cogMaskGraphic.SetTransparencyMap((byte)index, transparencyConstants);
            }
            cogMaskGraphic.Image = mask;
            cogMaskGraphic.Color = CogColorConstants.None;

            return cogMaskGraphic;
        }

        private Unit GetUnit()
        {
            InspModel inspModel = ModelManager.Instance().CurrentModel;
            if (inspModel == null)
                return null;

            Unit unit = null;
            if (IsLeft)
                unit = TeachingData.Instance().GetStageUnit(StageUnitNo).LeftUnit;
            else
                unit = TeachingData.Instance().GetStageUnit(StageUnitNo).RightUnit;

            return unit;
        }
            
        private MarkUnit GetMarkUnit()
        {
            var unit = GetUnit();

            if (_selectedAmpMark)
                return unit?.AmpMark;
            else
                return unit?.BondingMark;
        }

        private void UpdateMarkInfo()
        {
            var markUnit = GetMarkUnit();

            NUD_PAT_SCORE.Value = (decimal)markUnit.Score;

            if (_selectedMarkIndex == 0)
            {
                CB_SUBPAT_USE.Visible = false;
            }
            else
            {
                CB_SUBPAT_USE.Visible = true;
                CB_SUBPAT_USE.Checked = markUnit.TagList[_selectedMarkIndex].Use;
            }

            if (markUnit != null)
            {
                if(markUnit.TagList[_selectedMarkIndex].Tool is CogSearchMaxTool searchMaxTool)
                {
                    if(searchMaxTool.Pattern.Trained)
                        SetOrginMark(searchMaxTool.Pattern.Origin.TranslationX, searchMaxTool.Pattern.Origin.TranslationY);

                }
                for (int i = 0; i < markUnit.TagList.Count; i++)
                {
                    var tag = markUnit.TagList[i];

                    if (tag.Use)
                        MarkLabelList[i].BackColor = Color.LawnGreen;
                    else
                        MarkLabelList[i].BackColor = Color.WhiteSmoke;
                }
            }
        }

        private void Form_PatternTeach_FormClosed(object sender, FormClosedEventArgs e)
        {
            //timer1.Enabled = false;

            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
            //    {
            //        PT_Pattern[i, j].Dispose();
            //    }
            //}
            //PT_BlobToolBlock.Dispose();
        }

        private void BTN_PAT_CHANGE_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;

            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }

        private void RBTN_PAT_Click(object sender, EventArgs e)
        {
            //RadioButton TempBTN = (RadioButton)sender;
            //int m_Number;
            //m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 1, 1));
            //if (m_PatNo == m_Number) return;

            //nDistanceShow[m_PatNo] = false;
            //m_PatNo = m_Number;
           
            //m_PatNo_Sub = 0;
            //CB_SUB_PATTERN.SelectedIndex = 0;
            //LightRadio[0].Checked = true;
            //TABC_MANU.SelectedIndex = M_TOOL_MODE = Main.DEFINE.M_CNLSEARCHTOOL;
            //Pattern_Change();
        }

        private void LightCheck(int nM_TOOL_MODE)
        {
            //if (!Main.AlignUnit[m_AlignNo].LightUseCheck(m_PatNo))
            //{
            //    int nTempPatNo = Main.DEFINE.OBJ_L;
            //    try
            //    {
            //        if (m_PatNo == Main.DEFINE.OBJ_L && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.OBJ_R)) nTempPatNo = Main.DEFINE.OBJ_R;
            //        if (m_PatNo == Main.DEFINE.OBJ_R && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.OBJ_L)) nTempPatNo = Main.DEFINE.OBJ_L;
            //        if (m_PatNo == Main.DEFINE.TAR_L && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.TAR_R)) nTempPatNo = Main.DEFINE.TAR_R;
            //        if (m_PatNo == Main.DEFINE.TAR_R && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.TAR_L)) nTempPatNo = Main.DEFINE.TAR_L;

            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, nTempPatNo].SetAllLight(nM_TOOL_MODE);
            //    }
            //    catch
            //    {

            //    }
            //}
        }

        private void Pattern_Change()
        {
            //BTN_BackColor();
            //m_CamNo = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo;
            //LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            //LABEL_MESSAGE(LB_MESSAGE1, "", System.Drawing.Color.Lime);
            //Light_Select();
            //LightCheck(M_TOOL_MODE);
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(M_TOOL_MODE);
            //if (!bROIFinealignTeach)
            //    PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];

            //OriginImage = (CogImage8Grey)Main.vision.CogCamBuf[m_CamNo];
            //PT_DISPLAY_CONTROL.Resuloution = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CalX[0];
            //// CUSTOM CROSS
            //PT_DISPLAY_CONTROL.UseCustomCross = Main.vision.USE_CUSTOM_CROSS[m_CamNo];
            //PT_DISPLAY_CONTROL.CustomCross = new PointF(Main.vision.CUSTOM_CROSS_X[m_CamNo], Main.vision.CUSTOM_CROSS_Y[m_CamNo]);
            //DisplayClear();
            //Main.DisplayRefresh(PT_Display01);
            ////     if (BTN_DISNAME_01.BackColor.Name != "SkyBlue") CrossLine();
            //if (PT_DISPLAY_CONTROL.CrossLineChecked) CrossLine();
            ////--------------------CNLSEARCH-------------------------------------------
            //#region CNLSEARCH
            //m_RetiMode = 0;
            //if (bROIFinealignTeach)
            //{
            //    FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = PT_Display01.Image;
            //    NUD_PAT_SCORE.Value = (decimal)dFinealignMarkScore;
            //    if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion.GetType().Name != "CogRectangle")
            //    {
            //        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
            //    }

            //    PatMaxTrainRegion = new CogRectangle(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion as CogRectangle);
            //    MarkORGPoint.X = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationX;
            //    MarkORGPoint.Y = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationY;

            //    if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].SearchRegion == null)
            //    {
            //        PatMaxSearchRegion = new CogRectangle();
            //        PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
            //    }
            //    else
            //    {
            //        PatMaxSearchRegion = new CogRectangle(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].SearchRegion as CogRectangle);
            //    }

            //    for (int i = 0; i < Main.DEFINE.SUBPATTERNMAX; i++)
            //    {
            //        SUBPATTERN_LABELDISPLAY(PT_Pattern_USE[nROIFineAlignIndex, i], i);
            //        DrawTrainedPattern(PT_SubDisplay[i], FinealignMark[nROIFineAlignIndex, i]);
            //    }

            //    if (m_PatNo_Sub == 0)
            //    {
            //        CB_SUBPAT_USE.Visible = false;
            //    }
            //    else
            //    {
            //        CB_SUBPAT_USE.Visible = true;
            //        CB_SUBPAT_USE.Checked = PT_Pattern_USE[nROIFineAlignIndex, m_PatNo_Sub];
            //    }
            //}
            //else
            //{
            //    PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //    PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);

            //    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion.GetType().Name != "CogRectangle")
            //    {
            //        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
            //    }

            //    PatMaxTrainRegion = new CogRectangle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion as CogRectangle);
            //    MarkORGPoint.X = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX;
            //    MarkORGPoint.Y = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY;

            //    if (PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion == null)
            //    {
            //        PatMaxSearchRegion = new CogRectangle();
            //        PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
            //    }
            //    else
            //    {
            //        PatMaxSearchRegion = new CogRectangle(PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion as CogRectangle);
            //    }

            //    for (int i = 0; i < Main.DEFINE.SUBPATTERNMAX; i++)
            //    {
            //        SUBPATTERN_LABELDISPLAY(PT_Pattern_USE[m_PatNo, i], i);
            //        DrawTrainedPattern(PT_SubDisplay[i], PT_Pattern[m_PatNo, i]);
            //    }

            //    if (m_PatNo_Sub == 0)
            //    {
            //        CB_SUBPAT_USE.Visible = false;
            //    }
            //    else
            //    {
            //        CB_SUBPAT_USE.Visible = true;
            //        CB_SUBPAT_USE.Checked = PT_Pattern_USE[m_PatNo, m_PatNo_Sub];
            //    }
            //    NUD_PAT_SCORE.Value = (decimal)PT_AcceptScore[m_PatNo];
            //    NUD_PAT_GSCORE.Value = (decimal)PT_GAcceptScore[m_PatNo];
            //}

            //if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_PMAlign_Use == false)
            //{
            //    label13.Visible = false;
            //    NUD_PAT_GSCORE.Visible = false;
            //}
            //#endregion
        }

        #region DRAW & REFRESH IMAGE
        private void Draw_Label(CogRecordDisplay nDisplay, string resultText, int index)
        {
            //int i;
            //CogGraphicLabel Label = new CogGraphicLabel();
            //i = index;
            //float nFontSize = 0;

            //double baseZoom = 0;
            //if ((double)nDisplay.Width / nDisplay.Image.Width < (double)nDisplay.Height / nDisplay.Image.Height)
            //{
            //    baseZoom = ((double)nDisplay.Width - 22) / nDisplay.Image.Width;
            //    nFontSize = (float)((nDisplay.Image.Width / Main.DEFINE.FontSize) * baseZoom);
            //}
            //else
            //{
            //    baseZoom = ((double)nDisplay.Height - 22) / nDisplay.Image.Height;
            //    nFontSize = (float)((nDisplay.Image.Height / Main.DEFINE.FontSize) * baseZoom);
            //}


            //double nFontpitch = (nFontSize / nDisplay.Zoom);
            //Label.Text = resultText;
            //Label.Color = CogColorConstants.Cyan;
            //Label.Font = new Font(Main.DEFINE.FontStyle, nFontSize);
            //Label.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;
            //Label.X = (nDisplay.Image.Width - (nDisplay.Image.Width / (nDisplay.Zoom / baseZoom))) / 2 - nDisplay.PanX;
            //Label.Y = (nDisplay.Image.Height - (nDisplay.Image.Height / (nDisplay.Zoom / baseZoom))) / 2 - nDisplay.PanY + (i * nFontpitch);


            //nDisplay.StaticGraphics.Add(Label as ICogGraphic, "Result Text");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (bLiveStop == false)
            //{
            //    RefreshTeach();
            //}
        }
        delegate void dGrabRefresh(CogRecordDisplay nDisplay, ICogImage nImageBuf);
        public static void GrabDisRefresh_(CogRecordDisplay nDisplay, ICogImage nImageBuf)
        {

            if (nDisplay.InvokeRequired)
            {
                dGrabRefresh call = new dGrabRefresh(GrabDisRefresh_);
                nDisplay.Invoke(call, nDisplay, nImageBuf);
            }
            else
            {
                //jyh 임시 막음
                nDisplay.Image = nImageBuf;
            }
        }
      
        private void RefreshTeach()
        {
            //Main.vision.Grab_Flag_Start[m_CamNo] = true;
            //GrabDisRefresh_(PT_Display01, Main.vision.CogCamBuf[m_CamNo]);
        }
        #endregion

        #region 조명조절관련
        private void BTN_LIGHT_UP_Click(object sender, EventArgs e)
        {
            if (TBAR_LIGHT.Maximum == TBAR_LIGHT.Value)
                return;
            TBAR_LIGHT.Value++;
        }
        private void BTN_LIGHT_DOWN_Click(object sender, EventArgs e)
        {
            if (TBAR_LIGHT.Minimum == TBAR_LIGHT.Value)
                return;
            TBAR_LIGHT.Value--;
        }
        private void RBTN_LIGHT_0_CheckedChanged(object sender, EventArgs e)
        {
            Light_Select();
        }
        private void TBAR_LIGHT_ValueChanged(object sender, EventArgs e)
        {
           // Light_Change(m_SelectLight);
        }
        private void Light_Select()
        {
            //bool nLightUse = false;
            //for (int i = 0; i < Main.DEFINE.Light_PatMaxCount; i++)
            //{
            //    Light_Text[i].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[i, 0].ToString();
            //    LightRadio[i].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_Light_Name[i];
            //    if (LightRadio[i].Checked)
            //    {
            //        m_SelectLight = i;
            //        LightRadio[i].BackColor = System.Drawing.Color.LawnGreen;
            //    }
            //    else
            //    {
            //        LightRadio[i].BackColor = System.Drawing.Color.DarkGray;
            //    }

            //    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightCtrl[i] < 0)
            //    {
            //        Light_Text[i].Visible = false;
            //        LightRadio[i].Visible = false;
            //    }
            //    else
            //    {
            //        Light_Text[i].Visible = true;
            //        LightRadio[i].Visible = true;
            //        TBAR_LIGHT.Visible = true;
            //        BTN_LIGHT_UP.Visible = true;
            //        BTN_LIGHT_DOWN.Visible = true;
            //        nLightUse = true;
            //    }
            //}
            //if (!nLightUse)
            //{
            //    TBAR_LIGHT.Visible = false;
            //    BTN_LIGHT_UP.Visible = false;
            //    BTN_LIGHT_DOWN.Visible = false;
            //}
            //if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightCtrl[m_SelectLight] >= 0)
            //    TBAR_LIGHT.Value = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_SelectLight, 0];
        }
        private void Light_Change(int m_LightNum)
        {
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetLight(m_LightNum, TBAR_LIGHT.Value);
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_LightNum, 0] = TBAR_LIGHT.Value;
            //Light_Text[m_LightNum].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_LightNum, 0].ToString();
        }
        private void LB_LIGHT_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //Label TempLB = (Label)sender;
                //int nNum;
                //nNum = Convert.ToInt16(TempLB.Name.Substring(TempLB.Name.Length - 1, 1));
                //Form_LightSet formLight = new Form_LightSet(m_AlignNo, m_PatTagNo, m_PatNo, nNum);
                //formLight.ShowDialog();
                //formLight.Dispose();
                //Light_Select();
            }
        }
        #endregion

        #region 버튼클릭이벤트들

        private void BTN_IMAGE_OPEN_Click(object sender, EventArgs e)
        {
            AppsStatus.Instance().LiveStop = true;
            timer1.Enabled = false;

            BTN_LIVEMODE.BackColor = Color.DarkGray;

            openFileDlg.ReadOnlyChecked = true;
            openFileDlg.Filter = "Bmp File(*.bmp)|*.bmp;,|Jpg File(*.jpg)|*.jpg";

            if(openFileDlg.ShowDialog() == DialogResult.OK)
            {
                if (openFileDlg.FileName == "")
                    return;
                DisposeDisplayImage();


                CogDisplayImage = LoadImage(openFileDlg.FileName);

                CogDisplay.Image = CogDisplayImage;
                CogDisplay.DrawingEnabled = false;
                CogDisplay.Fit(true);
                CogDisplay.DrawingEnabled = true;
            }
           
        }

        private CogImage8Grey LoadImage(string filePath)
        {
            CogImage8Grey cogImage = null;

            string dirPath = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);

            VisionProHelper.GetImageFile(DisplayImageTool, filePath);

            if (fileName.Substring(fileName.Length - 3) == "jpg")
            {
                if (fileName.Substring(fileName.Length - 6) == "OV.jpg")
                {
                    MessageBox.Show("This JPG File is 'Overlay Image'.\r\nSelect Origin JPG Image.");
                    return null;
                }
            }
            CogImageConvertTool img = new CogImageConvertTool();
            img.InputImage = DisplayImageTool.OutputImage;
            img.Run();

            cogImage = img.OutputImage as CogImage8Grey;

            return cogImage;
        }

        private void BTN_TOOL_SET_Click(object sender, EventArgs e)
        {
            //Button TempBTN = (Button)sender;
            //try
            //{
            //    switch (TempBTN.Text)
            //    {
            //        case "SEARCHMAX": //CogCNLSearch
            //            PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //            ToolTeach.TT_SearchMaxTool = PT_Pattern[m_PatNo, m_PatNo_Sub];
            //            ToolTeach.m_ToolTextName = "CogSearchMaxTool";
            //            break;

            //        case "PMALIGN":
            //            PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //            ToolTeach.TT_PMAlign = PT_GPattern[m_PatNo, m_PatNo_Sub];
            //            ToolTeach.m_ToolTextName = "CogPMAlignTool";
            //            break;
            //    }
            //    ToolTeach.m_AlignNo = m_AlignNo;
            //    ToolTeach.m_PatNo = m_PatNo;
            //    ToolTeach.ShowDialog();
            //    if (TempBTN.Text == "SEARCHMAX" || TempBTN.Text == "PMALIGN") { CB_SUB_PATTERN_SelectionChangeCommitted(null, null); }
            //    if (TempBTN.Text == "CALIPER") { Caliper_Change(); }
            //    if (TempBTN.Text == "BLOB") { Blob_Change(); }
            //    if (TempBTN.Text == "FINDLINE") { FINDLINE_Change(); }
            //    if (TempBTN.Text == "CIRCLE") { Circle_Change(); }
            //}
            //catch
            //{

            //}
        }

        private void BTN_DISNAME_01_Click(object sender, EventArgs e)
        {
            Button TempBTN = (Button)sender;
            //int m_Number;
            //m_Number = TempBTN.TabIndex;

            //if (TempBTN.BackColor.Name == "SkyBlue")
            //{
            //    TempBTN.BackColor = Color.Plum;
            //    CrossLine();
            //}
            //else
            //{
            //    TempBTN.BackColor = Color.SkyBlue;
            //    DisplayClear();
            //    nDistanceShow[m_PatNo] = false;
            //    LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
            //}
        }
        bool nPatternCopy = false;
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            //formMessage.LB_MESSAGE.Text = "Did You Check [APPLY]?";
            //if (!formMessage.Visible)
            //{
            //    formMessage.ShowDialog();
            //}

            //Form_Password formpassword = new Form_Password(true);
            //formpassword.ShowDialog();

            //if (!formpassword.LOGINOK)
            //{
            //    nPatternCopy = false;
            //    formpassword.Dispose();
            //    return;
            //}
            //formpassword.Dispose();

            //string strParaName = "";
            //#region CNLSEARCH SAVE
            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    strParaName = "PATTERN SCORE";
            //    CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_ACCeptScore, PT_AcceptScore[i]);
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_ACCeptScore = PT_AcceptScore[i];
            //    strParaName = "GPATTERN SCORE";
            //    CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_GACCeptScore, PT_GAcceptScore[i]);
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_GACCeptScore = PT_GAcceptScore[i];

            //    for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
            //    {
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern[j] = new CogSearchMaxTool(PT_Pattern[i, j]);

            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern_USE[j] = PT_Pattern_USE[i, j];
            //        if (j == 0) Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern_USE[j] = true;

            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].GPattern[j] = new CogPMAlignTool(PT_GPattern[i, j]);
            //    }
            //}
            //#endregion

            //#region Inspection


            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    //shkang_s
            //    for (int j = 0; j < tempCaliperNum.Count; j++)
            //    {
            //        //해당 번호의 ROI의 Line,Circle 확인
            //        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_enumROIType == 0)  //Line
            //        {
            //            //Threshold
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "ContrastThreshold", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold, Main.DEFINE.CHANGEPARA);
            //            //FilterSize
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "FilterSize", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, Main.DEFINE.CHANGEPARA);
            //            //Caliper Count
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Count", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers, Main.DEFINE.CHANGEPARA);
            //            //Caliper Projection Length
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Projection Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength, Main.DEFINE.CHANGEPARA);
            //            //Caliper Search Length
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Search Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength, Main.DEFINE.CHANGEPARA);
            //            //관로 폭 Spec - 무시갯수
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore != m_TeachParameter[tempCaliperNum[j]].IDistgnore)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore, m_TeachParameter[tempCaliperNum[j]].IDistgnore, Main.DEFINE.CHANGEPARA);
            //            //관로 폭 Spec - Distance Min
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance != m_TeachParameter[tempCaliperNum[j]].dSpecDistance)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance, m_TeachParameter[tempCaliperNum[j]].dSpecDistance, Main.DEFINE.CHANGEPARA);
            //            //관로 폭 Spec - Distance Max
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax != m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax, m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax, Main.DEFINE.CHANGEPARA);
            //        }
            //        else   //Circle
            //        {
            //            //Threshold
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "ContrastThreshold", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold, Main.DEFINE.CHANGEPARA);
            //            //FilterSize
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "FilterSize", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, Main.DEFINE.CHANGEPARA);
            //            //Caliper Count
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Count", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers, Main.DEFINE.CHANGEPARA);
            //            //Caliper Projection Length
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Projection Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength, Main.DEFINE.CHANGEPARA);
            //            //Caliper Search Length
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Search Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength, Main.DEFINE.CHANGEPARA);
            //            //관로 폭 Spec - 무시갯수
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore != m_TeachParameter[tempCaliperNum[j]].IDistgnore)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore, m_TeachParameter[tempCaliperNum[j]].IDistgnore, Main.DEFINE.CHANGEPARA);
            //            //관로 폭 Spec - Distance Min
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance != m_TeachParameter[tempCaliperNum[j]].dSpecDistance)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance, m_TeachParameter[tempCaliperNum[j]].dSpecDistance, Main.DEFINE.CHANGEPARA);
            //            //관로 폭 Spec - Distance Max
            //            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax != m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax)
            //                Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax, m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax, Main.DEFINE.CHANGEPARA);
            //        }
            //    }
            //    //shkang_e


            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter = m_TeachParameter;
            //    var temp = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[0];
            //    if (temp.m_CogBlobTool[0].Region != null)
            //    {
            //        CogPolygon ROITracking = (CogPolygon)temp.m_CogBlobTool[0].Region;
            //        double dx = ROITracking.GetVertexX(0);
            //    }

            //}
            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    for (int j = 0; j < 4; j++)
            //    {
            //        if (j < 2)
            //        {
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LeftOrigin[j] = LeftOrigin[j];
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].RightOrigin[j] = RightOrigin[j];
            //        }
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_TrackingLine[j] = m_TeachLine[j];
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_BondingAlignLine[j] = m_TeachAlignLine[j];   //shkang Save Bonding Align Data

            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dOriginDistanceX = dBondingAlignOriginDistX;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dOriginDistanceY = dBondingAlignOriginDistY;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dDistanceSpecX = dBondingAlignDistSpecX;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dDistanceSpecY = dBondingAlignDistSpecY;
            //    }
            //}
            //if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceSpecX != dObjectDistanceSpecX)
            //    Save_ChangeParaLog("ChangePara", "m_dObjectDistanceSpecX", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceSpecX, dObjectDistanceSpecX, Main.DEFINE.CHANGEPARA);
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceSpecX = dObjectDistanceSpecX;
            //if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceX != dObjectDistanceX)
            //    Save_ChangeParaLog("ChangePara", "m_dObjectDistanceX", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceX, dObjectDistanceX, Main.DEFINE.CHANGEPARA);
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceX = dObjectDistanceX;

            ////YSH ROI Finealign
            //for (int i = 0; i < Main.DEFINE.Pattern_Max; i++)
            //{
            //    for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
            //    {
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMark[i, j] = FinealignMark[i, j];
            //    }
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_bFInealignFlag = m_bROIFinealignFlag;
            //    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignThetaSpec != m_dROIFinealignT_Spec)
            //        Save_ChangeParaLog("ChangePara", "m_FinealignThetaSpec", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignThetaSpec, m_dROIFinealignT_Spec, Main.DEFINE.CHANGEPARA);
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignThetaSpec = m_dROIFinealignT_Spec;
            //    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMarkScore != dFinealignMarkScore)
            //        Save_ChangeParaLog("ChangePara", "m_FinealignMarkScore", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMarkScore, dFinealignMarkScore, Main.DEFINE.CHANGEPARA);
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMarkScore = dFinealignMarkScore;
            //}

            //#endregion

            //Main.AlignUnit[m_AlignNo].m_Tray_Pocket_X = TRAY_POCKET_X;
            //Main.AlignUnit[m_AlignNo].m_Tray_Pocket_Y = TRAY_POCKET_Y;
            //Main.AlignUnit[m_AlignNo].TrayBlobMode = CB_TRAY_BlobMode.Checked;

            //// CUSTOM CROSS
            //Main.vision.USE_CUSTOM_CROSS[m_CamNo] = PT_DISPLAY_CONTROL.UseCustomCross;
            //Main.vision.CUSTOM_CROSS_X[m_CamNo] = (int)PT_DISPLAY_CONTROL.CustomCross.X;
            //Main.vision.CUSTOM_CROSS_Y[m_CamNo] = (int)PT_DISPLAY_CONTROL.CustomCross.Y;

            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_DISPLAY_CONTROL.CustomCross.X, PT_DISPLAY_CONTROL.CustomCross.Y,
            //                               ref Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dCustomCrossX, ref Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dCustomCrossY);

            //Main.AlignUnit[m_AlignNo].Save(m_PatTagNo);
            //Main.AlignUnit[m_AlignNo].Load(m_PatTagNo);

            //#region Stage Pattern COPY
            //if (Main.AlignUnit[m_AlignNo].m_AlignName == "PBD")
            //{
            //    if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC2" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC4")
            //    {
            //        string OrgName = "PBD", TarName = "PBD_STAGE";
            //        for (int i = 0; i < Main.DEFINE.SUBPATTERNMAX; i++)
            //        {
            //            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].Pattern[i] = new CogSearchMaxTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].Pattern[i]);
            //            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].Pattern[i] = new CogSearchMaxTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].Pattern[i]);

            //            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].GPattern[i] = new CogPMAlignTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].GPattern[i]);
            //            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].GPattern[i] = new CogPMAlignTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].GPattern[i]);

            //            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].Pattern_USE[i] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].Pattern_USE[i];
            //            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].Pattern_USE[i] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].Pattern_USE[i];


            //        }

            //        for (int i = 0; i < Main.DEFINE.Light_PatMaxCount; i++)
            //        {
            //            for (int j = 0; j < Main.DEFINE.Light_ToolMaxCount; j++)
            //            {
            //                Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightValue[i, j] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].m_LightValue[i, j];
            //                Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LightValue[i, j] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].m_LightValue[i, j];
            //            }
            //        }
            //        Main.AlignUnit[TarName].Save(m_PatTagNo);
            //    }
            //}
            //#endregion

            //#region Pattern Tag COPY
            //if (nPatternCopy)
            //{
            //    string TempName = Main.AlignUnit[m_AlignNo].m_AlignName;

            //    if (TempName == "PBD" || TempName == "PBD_STAGE" || TempName == "PBD_FOF" || TempName == "FPC_ALIGN")
            //    {
            //        for (int i = 0; i < Main.AlignUnit[TempName].m_AlignPatTagMax; i++)
            //        {
            //            for (int j = 0; j < Main.AlignUnit[TempName].m_AlignPatMax[i]; j++)
            //            {
            //                Main.AlignUnit[TempName].PAT[i, j].m_ACCeptScore = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].m_ACCeptScore;
            //                Main.AlignUnit[TempName].PAT[i, j].m_GACCeptScore = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].m_GACCeptScore;

            //                for (int k = 0; k < Main.DEFINE.SUBPATTERNMAX; k++)
            //                {
            //                    Main.AlignUnit[TempName].PAT[i, j].Pattern_USE[k] = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].Pattern_USE[k];
            //                    Main.AlignUnit[TempName].PAT[i, j].Pattern[k] = new CogSearchMaxTool(Main.AlignUnit[TempName].PAT[m_PatTagNo, j].Pattern[k]);
            //                    Main.AlignUnit[TempName].PAT[i, j].GPattern[k] = new CogPMAlignTool(Main.AlignUnit[TempName].PAT[m_PatTagNo, j].GPattern[k]);
            //                }
            //                for (int k = 0; k < Main.DEFINE.PATTERNTAG_MAX; k++)
            //                {
            //                    for (int ii = 0; ii < Main.DEFINE.Light_PatMaxCount; ii++)
            //                    {
            //                        for (int a = 0; a < Main.DEFINE.Light_ToolMaxCount; a++)
            //                        {
            //                            //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_L].m_Light_Name = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_Light_Name;
            //                            //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_R].m_Light_Name = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_Light_Name;
            //                            //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_L].m_LightCtrl = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightCtrl;
            //                            //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_R].m_LightCtrl = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LightCtrl;
            //                            //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_L].m_LightCH = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightCH;
            //                            //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_R].m_LightCH = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LightCH;
            //                            //                                    Main.AlignUnit[TempName].PAT[k, Main.DEFINE.OBJ_L].m_LightValue[ii, a] = Main.AlignUnit[TempName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightValue[ii, a];
            //                            Main.AlignUnit[TempName].PAT[k, j].m_LightValue[ii, a] = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].m_LightValue[ii, a];
            //                        }
            //                    }
            //                }
            //                //----------------------------------------------------------------------------------------------------------------------------------

            //                //----------------------------------------------------------------------------------------------------------------------------------
            //                //                         for (int jj = 0; jj < Main.DEFINE.Light_PatMaxCount; jj++)
            //                //                         {
            //                //                             for (int kk = 0; kk < Main.DEFINE.Light_ToolMaxCount; kk++)
            //                //                             {
            //                //                                 Main.AlignUnit[m_AlignNo].PAT[i, j].m_LightValue[jj, kk] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, j].m_LightValue[jj, kk];
            //                //                             }
            //                //                         }

            //            }
            //            Main.AlignUnit[m_AlignNo].Save(i);
            //        }

            //    }
            //    nPatternCopy = false;
            //}
            //#endregion

            //bROIFinealignTeach = false;
            //m_PatNo_Sub = 0;
            //CB_SUB_PATTERN.SelectedIndex = 0;
            //timer1.Enabled = false;
            //DisplayClear();
            //if (chkUseRoiTracking.Checked)
            //{
            //    chkUseRoiTracking.Checked = false;
            //}
            //if (chkUseLoadImageTeachMode.Checked)
            //{
            //    chkUseLoadImageTeachMode.Checked = false;
            //}
            //tempCaliperNum.Clear();
            //iCountClick = 0;
            //this.Hide();
        }// BTN_SAVE_Click
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            //shkang_s 파라미터저장시 로그 변수 초기화
            //tempCaliperNum.Clear();
            //iCountClick = 0;
            ////shkang_e
            //bLiveStop = false;
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_FinealignMarkScore = m_dTempFinealignMarkscore;
            //Main.AlignUnit[m_AlignNo].Load(m_PatTagNo);
            //bROIFinealignTeach = false;
            //timer1.Enabled = false;
            //m_PatNo_Sub = 0;
            //CB_SUB_PATTERN.SelectedIndex = 0;
            ////BTN_PATTERN_COPY.Visible = false;
            //DisplayClear();
            //if (chkUseRoiTracking.Checked)
            //{
            //    chkUseRoiTracking.Checked = false;
            //}
            //if (chkUseLoadImageTeachMode.Checked)
            //{
            //    chkUseLoadImageTeachMode.Checked = false;
            //}
            this.Hide();
        }

        #endregion

        private void ClearDisplayGraphic()
        {
            CogDisplay.StaticGraphics.Clear();
            CogDisplay.InteractiveGraphics.Clear();
        }

        public void SetInteractiveGraphics(string groupName, ICogRecord record)
        {
            if (record == null)
                return;

            foreach (CogRecord subRecord in record.SubRecords)
            {
                if (typeof(ICogGraphic).IsAssignableFrom(subRecord.ContentType))
                {
                    if (subRecord.Content != null)
                        CogDisplay.InteractiveGraphics.Add(subRecord.Content as ICogGraphicInteractive, groupName, false);
                }
                else if (typeof(CogGraphicCollection).IsAssignableFrom(subRecord.ContentType))
                {
                    if (subRecord.Content != null)
                    {
                        CogGraphicCollection graphics = subRecord.Content as CogGraphicCollection;

                        foreach (ICogGraphic graphic in graphics)
                            CogDisplay.InteractiveGraphics.Add(graphic as ICogGraphicInteractive, groupName, false);
                    }
                }
                else if (typeof(CogGraphicInteractiveCollection).IsAssignableFrom(subRecord.ContentType))
                {
                    if (subRecord.Content != null)
                        CogDisplay.InteractiveGraphics.AddList(subRecord.Content as CogGraphicInteractiveCollection, groupName, false);
                }

                SetInteractiveGraphics(groupName, subRecord);
            }
        }

        #region 패턴 등록 관련
        private void BTN_PATTERN_Click(object sender, EventArgs e)
        {
            if (CogDisplayImage == null)
                return;

            ClearDisplayGraphic();
            ClearMarkButtonBackColor();
            BTN_PATTERN.BackColor = Color.LawnGreen;

            PT_DISPLAY_CONTROL.CrossLine();

            var tag = GetMarkUnit().TagList[_selectedMarkIndex];
            if (tag.Tool.Pattern.Trained)
            {
                double x = tag.Tool.Pattern.Origin.TranslationX;
                double y = tag.Tool.Pattern.Origin.TranslationY;
                SetOrginMark(x, y);
            }
            else
            {
                SetNewROI();
            }

            DrawROI(CogSearchMaxCurrentRecordConstants.TrainRegion);
        }

        private void ClearMarkButtonBackColor()
        {
            BTN_PATTERN.BackColor = Color.DarkGray;
            BTN_ORIGIN.BackColor = Color.DarkGray;
            BTN_PATTERN_SEARCH_SET.BackColor = Color.DarkGray;
        }

        private void SetOrginMark(double x, double y)
        {
            if(OriginMarkPoint == null)
            {
                OriginMarkPoint = new CogPointMarker();
                OriginMarkPoint.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
                OriginMarkPoint.Interactive = true;
                OriginMarkPoint.LineStyle = CogGraphicLineStyleConstants.Dot;
                OriginMarkPoint.SelectedColor = CogColorConstants.Cyan;
                OriginMarkPoint.DragColor = CogColorConstants.Cyan;
            }
            OriginMarkPoint.X = x;
            OriginMarkPoint.Y = y;
        }

        public void AddROI()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            if (ModelManager.Instance().CurrentModel == null)
                return;

            SetNewROI();
        }

        private void SetNewROI()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            double centerX = CogDisplayImage.Width / 2.0;
            double centerY = CogDisplayImage.Height / 2.0;

            CogRectangle roi = VisionProHelper.CreateRectangle(centerX, centerY, 100, 100);
            CogRectangle searchRoi = VisionProHelper.CreateRectangle(roi.CenterX, roi.CenterY, roi.Width * 2, roi.Height * 2);

            SetOrginMark(centerX, centerY);

            var currentParam = GetMarkUnit().TagList[_selectedMarkIndex];
            currentParam?.SetTrainRegion(roi);
            currentParam.SetSearchRegion(searchRoi);
            currentParam?.SetOrginMark(OriginMarkPoint);
        }

        private void DrawROI(CogSearchMaxCurrentRecordConstants constants)
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var tag = GetMarkUnit().TagList[_selectedMarkIndex];

            constants = constants | CogSearchMaxCurrentRecordConstants.InputImage | CogSearchMaxCurrentRecordConstants.TrainImage;
            ;//| CogSearchMaxCurrentRecordConstants.TrainImage;

            //constants = CogSearchMaxCurrentRecordConstants.All;

            CogRectangle rect = new CogRectangle();
            rect.X = 500;
            rect.Y = 1000;
            rect.Width = 100;
            rect.Height = 100;
            rect.Interactive = true;
            rect.Color = CogColorConstants.Red;
            rect.GraphicDOFEnable = CogRectangleDOFConstants.Position | CogRectangleDOFConstants.Size;
            tag.Tool.SearchRegion = rect;
            tag.Tool.CurrentRecordEnable = CogSearchMaxCurrentRecordConstants.SearchRegion;

            SetInteractiveGraphics("tool", tag.Tool.CreateCurrentRecord());
            //PT_DISPLAY_CONTROL.CogDisplay00.InteractiveGraphics.Clear();
            
            //CogGraphicInteractiveCollection collect2 = new CogGraphicInteractiveCollection();
            //collect2.Add(rect);
            //CogDisplay.InteractiveGraphics.AddList(collect2, "tool", false);
            CogDisplay.InteractiveGraphics.Add(OriginMarkPoint, "tool", false);

            if (tag.Tool.Pattern.Trained == false)
            {
                CogGraphicInteractiveCollection collect = new CogGraphicInteractiveCollection();

                var trainRegion = tag.Tool.Pattern.TrainRegion as CogRectangle;
                collect.Add(trainRegion);

                CogDisplay.InteractiveGraphics.AddList(collect, "tool", false);
            }
        }

        private void CNLSearch_DrawOverlay()
        {
            //CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();
            //PatternInfo.Add(PatMaxTrainRegion);
            ////         PatternInfo.Add(PatMaxORGPoint);

            //PT_Display01.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);
        }

        private void BTN_BackColor(object sender, EventArgs e)
        {
            //nDistanceShow[m_PatNo] = false;
            //LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
            //LABEL_MESSAGE(LB_MESSAGE1, "", System.Drawing.Color.Red);

            //BTN_BackColor();
            //Button TempBTN = (Button)sender;
            //TempBTN.BackColor = System.Drawing.Color.LawnGreen;
        }

        private void BTN_BackColor()
        {
            BTN_PATTERN.BackColor = System.Drawing.Color.DarkGray;
            BTN_ORIGIN.BackColor = System.Drawing.Color.DarkGray;
            BTN_PATTERN_SEARCH_SET.BackColor = System.Drawing.Color.DarkGray;
        }

        private void BTN_PATTERN_ORIGIN_Click(object sender, EventArgs e)
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var currentParam = GetMarkUnit().TagList[_selectedMarkIndex];
            var trainRegion = currentParam?.Tool.Pattern.TrainRegion as CogRectangle;

            double newX = trainRegion.X + (trainRegion.Width / 2);
            double newY = trainRegion.Y + (trainRegion.Height / 2);

            SetOrginMark(newX, newY);
            currentParam?.SetOrginMark(OriginMarkPoint);

            DrawROI(CogSearchMaxCurrentRecordConstants.TrainRegion);
        }

        private void BTN_ORIGIN_Click(object sender, EventArgs e)
        {
            //m_RetiMode = M_ORIGIN;
            //BTN_BackColor(sender, e);
        }
        private void BTN_PATTERN_SEARCH_SET_Click(object sender, EventArgs e)
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            ClearMarkButtonBackColor();
            ClearDisplayGraphic();
            BTN_PATTERN_SEARCH_SET.BackColor = Color.LawnGreen;

            var tag = GetMarkUnit().TagList[_selectedMarkIndex];
            //if (tag.Tool.Pattern.Trained == false)
            //{
            //    SetNewROI();
            //}
            //else
            //{

            //}
            //CogSearchMaxCurrentRecordConstants consta = CogSearchMaxCurrentRecordConstants.SearchRegion || CogSearchMaxCurrentRecordConstants.TrainRegion;
            DrawROI(CogSearchMaxCurrentRecordConstants.TrainRegion);

            //m_RetiMode = M_SEARCH;
            //BTN_BackColor(sender, e);
            //DisplayClear();

            //if (bROIFinealignTeach)
            //{
            //    if (FinealignMark[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
            //        PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
            //}
            //else
            //{
            //    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
            //        PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
            //}

            //PatMaxSearchRegion.GraphicDOFEnable = CogRectangleDOFConstants.Position | CogRectangleDOFConstants.Size;
            //PatMaxSearchRegion.Color = CogColorConstants.Orange;
            //PatMaxSearchRegion.Interactive = true;

            //CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();

            //PatternInfo.Add(PatMaxSearchRegion);
            //PT_Display01.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);

            //DisplayFit(PT_Display01);
        }

        private void BTN_PATTERN_SEARCH_SET_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion = null;
                //PatMaxSearchRegion = new CogRectangle();
                //PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
                //BTN_PATTERN_SEARCH_SET_Click(sender, null);
            }
        }
        private static void DisplayFit(CogRecordDisplay Display)
        {
            Display.AutoFitWithGraphics = true;
            Display.Fit(true);
        }
        private CogImage8Grey CopyIMG(ICogImage IMG)
        {
            return new CogImage8Grey();

            //if (IMG == null)
            //    return new CogImage8Grey();

            //CogImage8Grey returnIMG;

            //returnIMG = new CogImage8Grey(IMG as CogImage8Grey);
            //return returnIMG;


        }
        private void BTN_APPLY_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            //    {
            //        if (bROIFinealignTeach)
            //        {
            //            if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            //            {
            //                FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImageMask = null;
            //                FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImageMaskOffsetX = 0;
            //                FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImageMaskOffsetY = 0;
            //            }

            //            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImage = PT_Display01.Image;
            //            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle(PatMaxTrainRegion);

            //            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationX = MarkORGPoint.X;
            //            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationY = MarkORGPoint.Y;

            //            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Train();

            //            DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], FinealignMark[nROIFineAlignIndex, m_PatNo_Sub]);
            //            LABEL_MESSAGE(LB_MESSAGE, "Train OK", System.Drawing.Color.Lime);
            //        }
            //        else
            //        {
            //            if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            //            {
            //                PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMask = null;
            //                PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetX = 0;
            //                PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetY = 0;

            //                PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMask = null;
            //                PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetX = 0;
            //                PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetY = 0;
            //            }

            //            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //            //PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangleAffine(PatMaxTrainRegion);
            //            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle(PatMaxTrainRegion);



            //            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX = MarkORGPoint.X;
            //            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY = MarkORGPoint.Y;

            //            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Train();

            //            DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], PT_Pattern[m_PatNo, m_PatNo_Sub]);
            //            LABEL_MESSAGE(LB_MESSAGE, "Train OK", System.Drawing.Color.Lime);

            //            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //            //      PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangleAffine(PatMaxTrainRegion);
            //            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle(PatMaxTrainRegion);
            //            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX = MarkORGPoint.X;
            //            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY = MarkORGPoint.Y;
            //            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.Train();
            //        }


            //    }
            //    if (m_RetiMode == M_SEARCH)
            //    {
            //        if (bROIFinealignTeach)
            //        {
            //            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].SearchRegion = new CogRectangle(PatMaxSearchRegion);
            //        }
            //        else
            //        {
            //            PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion = new CogRectangle(PatMaxSearchRegion);
            //            PT_GPattern[m_PatNo, m_PatNo_Sub].SearchRegion = new CogRectangle(PatMaxSearchRegion);
            //        }
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            //}
        }
        private void BTN_PATTERN_DELETE_Click(object sender, EventArgs e)
        {
            //DialogResult result = MessageBox.Show("Do you want to Delete Pattern Number: " + CB_SUB_PATTERN.Text + " ?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //if (result == DialogResult.Yes)
            //{
            //    if (bROIFinealignTeach)
            //    {
            //        FinealignMark[m_PatNo, m_PatNo_Sub].Pattern = new CogSearchMaxPattern();
            //        FinealignMark[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
            //        DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], FinealignMark[m_PatNo, m_PatNo_Sub]);
            //    }
            //    else
            //    {
            //        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern = new CogSearchMaxPattern();
            //        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
            //        PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern = new CogPMAlignPattern();
            //        //        DrawTrainedPattern(PT_SubDisplay_00, PT_Pattern[m_PatNo, m_PatNo_Sub]);
            //        DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], PT_Pattern[m_PatNo, m_PatNo_Sub]);
            //    }

            //}
        }
        private void CB_SUB_PATTERN_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int index = CB_SUB_PATTERN.SelectedIndex;
            if (index < 0)
                return;

            ChangeMark(index);
        }

        public static void DrawTrainedPattern(CogRecordDisplay Display, CogSearchMaxTool TempPMAlignTool)
        {
            //Main.DisplayClear(Display);

            //CogSearchMaxTool PMAlignTool = new CogSearchMaxTool(TempPMAlignTool);
            //if (PMAlignTool.Pattern.Trained)
            //{
            //    Display.Image = PMAlignTool.Pattern.GetTrainedPatternImage();

            //    CogRectangle TrainRegion = new CogRectangle(PMAlignTool.Pattern.TrainRegion as CogRectangle);
            //    TrainRegion.GraphicDOFEnable = CogRectangleDOFConstants.Position;
            //    TrainRegion.Interactive = false;

            //    CogCoordinateAxes ORGPoint = new CogCoordinateAxes();
            //    ORGPoint.LineStyle = CogGraphicLineStyleConstants.Dot;
            //    ORGPoint.Transform.TranslationX = PMAlignTool.Pattern.Origin.TranslationX;
            //    ORGPoint.Transform.TranslationY = PMAlignTool.Pattern.Origin.TranslationY;
            //    ORGPoint.GraphicDOFEnable = CogCoordinateAxesDOFConstants.Position;

            //    CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();
            //    //VisionPro 9.5 Ver 이상
            //    if (PMAlignTool.Pattern.GetTrainedPatternImageMask() != null) PatternInfo.Add(CreateMaskGraphic(PMAlignTool.Pattern.TrainImage.SelectedSpaceName, PMAlignTool.Pattern.GetTrainedPatternImageMask()));
            //    PatternInfo.Add(TrainRegion);
            //    PatternInfo.Add(ORGPoint);

            //    Display.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);

            //    DisplayFit(Display);
            //}
            //else
            //{
            //    Display.Image = null;
            //}
        }
        private static CogMaskGraphic CreateMaskGraphic(string SelectedSpaceName, CogImage8Grey mask)
        {
            return new CogMaskGraphic();
            //CogMaskGraphic cogMaskGraphic = new CogMaskGraphic();
            //for (short index = 0; index < (short)256; ++index)
            //{
            //    CogColorConstants cogColorConstants;
            //    CogMaskGraphicTransparencyConstants transparencyConstants;
            //    if (index < (short)64)
            //    {
            //        cogColorConstants = CogColorConstants.DarkRed;
            //        transparencyConstants = CogMaskGraphicTransparencyConstants.Half;
            //    }
            //    else if (index < (short)128)
            //    {
            //        cogColorConstants = CogColorConstants.Yellow;
            //        transparencyConstants = CogMaskGraphicTransparencyConstants.Half;
            //    }
            //    else if (index < (short)192)
            //    {
            //        cogColorConstants = CogColorConstants.Red;
            //        transparencyConstants = CogMaskGraphicTransparencyConstants.None;
            //    }
            //    else
            //    {
            //        cogColorConstants = CogColorConstants.Yellow;
            //        transparencyConstants = CogMaskGraphicTransparencyConstants.Full;
            //    }
            //    cogMaskGraphic.SetColorMap((byte)index, cogColorConstants);
            //    cogMaskGraphic.SetTransparencyMap((byte)index, transparencyConstants);
            //}
            //cogMaskGraphic.Image = mask;
            //cogMaskGraphic.Color = CogColorConstants.None;
            //if (SelectedSpaceName == "#")
            //{
            //    ((ICogGraphic)cogMaskGraphic).SelectedSpaceName = "_TrainImage#";
            //}
            //return cogMaskGraphic;
        }
        private void CB_SUBPAT_USE_CheckedChanged(object sender, EventArgs e)
        {
            //if (CB_SUBPAT_USE.Checked)
            //{
            //    PT_Pattern_USE[m_PatNo, m_PatNo_Sub] = true;
            //    CB_SUBPAT_USE.BackColor = System.Drawing.Color.LawnGreen;

            //}
            //else
            //{
            //    PT_Pattern_USE[m_PatNo, m_PatNo_Sub] = false;
            //    CB_SUBPAT_USE.BackColor = System.Drawing.Color.DarkGray;

            //}
            //SUBPATTERN_LABELDISPLAY(CB_SUBPAT_USE.Checked, m_PatNo_Sub);
        }
        #endregion

        private void BTN_PATTERN_RUN_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
            //    m_Timer.StartTimer();
            //    CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //    DisplayClear();
            //    List_NG.Items.Clear();
            //    switch (Convert.ToInt32(TABC_MANU.SelectedTab.Tag))
            //    {
            //        case Main.DEFINE.M_CNLSEARCHTOOL: //CogCNLSearch
            //            #region CNLSEARCH
            //            Save_SystemLog("Mark Search Start", Main.DEFINE.CMD);
            //            lock (mlock)
            //            {
            //                Search_PATCNL();
            //            }
            //            #endregion
            //            break;

            //        case Main.DEFINE.M_CALIPERTOOL: //CogCALIPERTOOL
            //            #region CALIPERTOOL
            //            RefreshDisplay2();
            //            if (ThresValue_Sts)
            //                Search_Caliper(false);
            //            else
            //                Search_Caliper(true);
            //            #endregion
            //            break;

            //        case Main.DEFINE.M_FINDLINETOOL: //CogFINDLineTOOL
            //            #region COGFINDLine
            //            RefreshDisplay2();
            //            if (ThresValue_Sts)
            //            {
            //                Search_FindLine(false);
            //                Search_Circle(false);
            //            }
            //            else
            //            {
            //                Search_FindLine(true);
            //                Search_Circle(true);
            //            }
            //            #endregion
            //            break;

            //        case Main.DEFINE.M_FINDCIRCLETOOL:
            //            #region CIRCLETOOL
            //            RefreshDisplay2();
            //            if (ThresValue_Sts)
            //            {
            //                Search_FindLine(false);
            //                Search_Circle(false);
            //            }
            //            else
            //            {
            //                Search_FindLine(true);
            //                Search_Circle(true);
            //            }
            //            #endregion
            //            break;
            //        case Main.DEFINE.M_INSPECTION:
            //            #region INSPECITON
            //            if (m_enumROIType == enumROIType.Line)
            //            {
            //                Test_FindLine();
            //            }
            //            else
            //            {
            //                Test_FindCricle();
            //            }
            //            #endregion
            //            break;
            //        case Main.DEFINE.M_ALIGNINPECTION:
            //            Test_TrackingLine();
            //            break;
            //    }
            //    Lab_Tact.Text = m_Timer.GetElapsedTime().ToString() + " ms";
            //    //    if (BTN_DISNAME_01.BackColor.Name != "SkyBlue") CrossLine();
            //    if (PT_DISPLAY_CONTROL.CrossLineChecked) CrossLine();
            //}//try
            //catch (System.Exception ex)
            //{
            //    LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            //}


        }

        private void Test_FindLine()
        {
            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //double[] Result;
            //int ignore;
            //bool bRet = false;
            ////2023 0615 YSH
            ////티칭창에서 단일 Search시엔, Searck 방식 플래그에 따라 동작하게끔 함
            ////단일 티칭 확인 가능용도

            //if (m_bInspDirectionChange)
            //    GaloDirectionConvertInspection(0, (int)enumROIType.Line, m_TempFindLineTool, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore);
            //else
            //    GaloOppositeInspection(m_iGridIndex, (int)enumROIType.Line, m_TempFindLineTool, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore);

            //PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            ////
            //ResultGride(Result);
        }
        private void Test_TrackingLine()
        {
            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //m_TempTrackingLine.InputImage = (CogImage8Grey)PT_Display01.Image;
            //m_TempTrackingLine.Run();
            //if (m_TempTrackingLine.Results != null || m_TempTrackingLine.Results.Count > 0)
            //{
            //    resultGraphics.Add(m_TempTrackingLine.Results.GetLine());
            //    PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);

            //}
        }
        private void Test_FindCricle()
        {
            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //double[] Result;
            //int ignore;
            //GaloOppositeInspection(m_iGridIndex, (int)enumROIType.Circle, m_TempFindCircleTool, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore);
            //PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);

            //ResultGride(Result);
        }
        private void ResultGride(double[] ResulteData)
        {
            //if (ResulteData != null)
            //{
            //    dataGridView_Result.Rows.Clear();
            //    string[] strResultData = new string[7];
            //    for (int i = 0; i < ResulteData.GetLength(0); i++)
            //    {
            //        strResultData[0] = i.ToString();
            //        strResultData[1] = "0";

            //        double dDist = ResulteData[i];
            //        strResultData[3] = string.Format("{0:F3}", dDist/** PixelResolution/1000*/);
            //        dataGridView_Result.Rows.Add(strResultData);
            //    }
            //}
        }
        private void TABC_MANU_Selecting(object sender, TabControlCancelEventArgs e)
        {
            //switch (TABC_MANU.SelectedIndex)
            //{
            //    case Main.DEFINE.M_CNLSEARCHTOOL:
            //        switch (Main.AlignUnit[m_AlignNo].m_AlignName)
            //        {
            //            case "1st PREALIGN":
            //                TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDLINETOOL;
            //                break;
            //            default:
            //                //TABC_MANU.SelectedIndex = Main.DEFINE.M_CNLSEARCHTOOL;
            //                break;
            //        }
            //        break;

            //    case Main.DEFINE.M_BLOBTOOL:
            //        switch (Main.AlignUnit[m_AlignNo].m_AlignName)
            //        {
            //            case "IC_TRAY":
            //                break;
            //            case "ACF_BLOB":
            //                break;
            //            case "FOF_ACF_PRE":
            //                break;
            //            case "FOP_ACF_PRE":
            //                break;
            //            case "SCANNER HEAD CAM1":
            //            case "ALIGN INSP CAM2":
            //            case "ALIGN INSP CAM3":
            //            case "ALIGN INSP CAM4":
            //            case "1st PREALIGN":
            //                TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDLINETOOL;
            //                break;
            //            default:
            //                //TABC_MANU.SelectedIndex = Main.DEFINE.M_CNLSEARCHTOOL;
            //                break;
            //        }
            //        break;

            //    default:
            //        break;
            //}
        }
        private void TABC_MANU_SelectedIndexChanged(object sender, EventArgs e)
        {
            //LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            //LABEL_MESSAGE(LB_MESSAGE1, "", System.Drawing.Color.Lime);

            //M_TOOL_MODE = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            //if (bROIFinealignTeach)
            //{
            //    if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) != 0)
            //    {
            //        TABC_MANU.SelectedIndex = 0;
            //        return;
            //    }
            //}
            //if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 6 || Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
            //{
            //    if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
            //        btn_Inspection_Test.Visible = true;
            //    else
            //        btn_Inspection_Test.Visible = false;

            //    if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
            //    {
            //        UpdateParamUI();
            //        _useROITracking = false;
            //        chkUseRoiTracking.Checked = _useROITracking;
            //        _eTabSelect = enumTabSelect.Insp;
            //    }
            //    else
            //    {
            //        _eTabSelect = enumTabSelect.ThetaOrigin;
            //        //2023 0223 YSH 창 진입시 자재얼라인 패널 Show
            //        RDB_ROI_FINEALIGN.PerformClick();
            //    }
            //    m_enumAlignROI = enumAlignROI.Left1_1;
            //    btn_TOP_Inscription.BackColor = Color.Green;
            //    btn_Top_Circumcription.BackColor = Color.DarkGray;
            //    btn_Bottom_Inscription.BackColor = Color.DarkGray;
            //    btn_Bottom_Circumcription.BackColor = Color.DarkGray;
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (i < 2)
            //        {
            //            LeftOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].LeftOrigin[i];
            //            RightOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].RightOrigin[i];
            //        }
            //        m_TeachLine[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i];
            //        if (m_TeachLine[i] == null)
            //            m_TeachLine[i] = new CogFindLineTool();
            //        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] == null)
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] = new CogFindLineTool();
            //        //Bonding Align
            //        m_TeachAlignLine[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i];
            //        if (m_TeachAlignLine[i] == null)
            //            m_TeachAlignLine[i] = new CogCaliperTool();
            //        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i] == null)
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i] = new CogCaliperTool();
            //    }
            //    lab_LeftOriginX.Text = LeftOrigin[0].ToString("F3");
            //    lab_LeftOriginY.Text = LeftOrigin[1].ToString("F3");
            //    lab_RightOriginX.Text = RightOrigin[0].ToString("F3");
            //    lab_RightOriginY.Text = RightOrigin[1].ToString("F3");
            //    m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_1];
            //    lblOkDistanceValueX.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceX);
            //    lblOkDistanceValueY.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceY);
            //    lblAlignSpecValueX.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecX);
            //    lblAlignSpecValueY.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecY);
            //    dBondingAlignOriginDistX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceX;
            //    dBondingAlignOriginDistY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceY;
            //    dBondingAlignDistSpecX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecX;
            //    dBondingAlignDistSpecY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecY;
            //    lblObjectDistanceXValue.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceX);
            //    lblObjectDistanceXSpecValue.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceSpecX);
            //    dObjectDistanceSpecX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceSpecX;
            //    dObjectDistanceX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceX;


            //    if (OriginImage != null)
            //        PT_Display01.Image = OriginImage;
            //    Get_FindConerParameter();
            //}
            //if (M_TOOL_MODE > 0 && M_TOOL_MODE < 5)
            //{
            //    TABC_MANU.SelectedIndex = 5;
            //    return;
            //}
            //else if (m_PatNo == 1 && M_TOOL_MODE == 5)
            //{
            //    TABC_MANU.SelectedIndex = 6;
            //}

            //if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5 || Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 6)
            //    M_TOOL_MODE = 0;
            //if (bROICopy)
            //    for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = false;
            //else
            //    for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = true;

            //BTN_TOOLSET[M_TOOL_MODE].Visible = true;
            ////if (M_TOOL_MODE == Main.DEFINE.M_CNLSEARCHTOOL) BTN_TOOLSET[Main.DEFINE.M_PMALIGNTOOL].Visible = true;

            //Light_Select();
            //LightCheck(M_TOOL_MODE);
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(M_TOOL_MODE);
            //DisplayClear();
            //nDistanceShow[m_PatNo] = false;

            //m_TABCHANGE_MODE = true;
            //switch (M_TOOL_MODE)
            //{
            //    case Main.DEFINE.M_CNLSEARCHTOOL:
            //        if (bROIFinealignTeach)
            //            BTN_RETURNPAGE.Visible = true;
            //        else
            //            BTN_RETURNPAGE.Visible = false;
            //        Pattern_Change();
            //        break;
            //    case Main.DEFINE.M_BLOBTOOL:
            //        CB_BLOB_MARK_USE.Checked = PT_Blob_MarkUSE[m_PatNo];
            //        CB_BLOB_CALIPER_USE.Checked = PT_Blob_CaliperUSE[m_PatNo];
            //        m_SelectBlob = 0;
            //        CB_BLOB_COUNT.SelectedIndex = 0;
            //        Inspect_Cnt.Value = PT_Blob_InspCnt[m_PatNo];
            //        Blob_Change();
            //        break;

            //    case Main.DEFINE.M_CALIPERTOOL:
            //        CB_CALIPER_MARK_USE.Checked = PT_Caliper_MarkUSE[m_PatNo];
            //        RBTN_CALIPER00.Checked = true;
            //        m_SelectCaliper = 0;
            //        Caliper_Change();
            //        break;

            //    case Main.DEFINE.M_FINDLINETOOL:
            //        CB_FINDLINE_MARK_USE.Checked = PT_FindLine_MarkUSE[m_PatNo];
            //        RBTN_FINDLINE00.Checked = true;
            //        m_SelectFindLine = 0;
            //        FINDLINE_Change();
            //        break;

            //    case Main.DEFINE.M_FINDCIRCLETOOL:
            //        CB_CIRCLE_MARK_USE.Checked = PT_Circle_MarkUSE[m_PatNo];
            //        RBTN_CIRCLE00.Checked = true;
            //        m_SelectCircle = 0;
            //        Circle_Change();
            //        break;
            //}
            //m_TABCHANGE_MODE = false;
        }
        private void RefreshDisplay2()
        {
            //try
            //{
            //    CogImage8Grey nTempImage = new CogImage8Grey();
            //    nTempImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //    bool TargetPosUse = false;
            //    switch (M_TOOL_MODE)
            //    {
            //        case Main.DEFINE.M_BLOBTOOL:
            //        case Main.DEFINE.M_CALIPERTOOL:
            //        case Main.DEFINE.M_FINDLINETOOL:
            //        case Main.DEFINE.M_FINDCIRCLETOOL:
            //            if ((PT_Caliper_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_CALIPERTOOL)
            //                || (PT_Blob_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
            //                || (PT_FindLine_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_FINDLINETOOL)
            //                || (PT_Circle_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_FINDCIRCLETOOL))
            //            {
            //                TargetPosUse = true;
            //                LightCheck(Main.DEFINE.M_LIGHT_CNL);
            //                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //                Main.SearchDelay(100);
            //                if (!Search_PATCNL())
            //                {
            //                    PatResult.TranslationX = 0;
            //                    PatResult.TranslationY = 0;
            //                    return;
            //                }
            //            }
            //            else if ((PT_Blob_CaliperUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL))
            //            {
            //                TargetPosUse = true;
            //                LightCheck(Main.DEFINE.M_LIGHT_CALIPER);
            //                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(Main.DEFINE.M_LIGHT_CALIPER);
            //                Main.SearchDelay(100);

            //                if (!Search_Caliper(true))
            //                {
            //                    PatResult.TranslationX = 0;
            //                    PatResult.TranslationY = 0;
            //                    return;
            //                }
            //            }
            //            else
            //            {
            //                PatResult.TranslationX = 0;
            //                PatResult.TranslationY = 0;
            //            }
            //            if (TargetPosUse)
            //            {
            //                LightCheck(M_TOOL_MODE);
            //                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(M_TOOL_MODE);
            //                Main.SearchDelay(100);
            //                //그랩이 되기전에 다음으로 넘어가기 때문에 넣음.
            //                Main.vision.Grab_Flag_End[m_CamNo] = false;
            //                Main.vision.Grab_Flag_Start[m_CamNo] = true;

            //                while (!Main.vision.Grab_Flag_End[m_CamNo] && !Main.DEFINE.OPEN_F)
            //                {
            //                    Main.SearchDelay(1);
            //                }
            //                nTempImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //            }
            //            if (M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
            //            {
            //                for (int i = 0; i < Main.DEFINE.BLOB_CNT_MAX; i++)
            //                    PT_BlobTools[m_PatNo, i].InputImage = nTempImage;
            //            }
            //            if (M_TOOL_MODE == Main.DEFINE.M_CALIPERTOOL)
            //            {
            //                for (int i = 0; i < Main.DEFINE.CALIPER_MAX; i++)
            //                    PT_CaliperTools[m_PatNo, i].InputImage = nTempImage;
            //            }
            //            if (M_TOOL_MODE == Main.DEFINE.M_FINDLINETOOL)
            //            {
            //                PT_FindLineTool.InputImage = nTempImage;
            //                PT_LineMaxTool.InputImage = nTempImage;
            //                for (int ii = 0; ii < Main.DEFINE.SUBLINE_MAX; ii++)
            //                {
            //                    for (int i = 0; i < Main.DEFINE.FINDLINE_MAX; i++)
            //                    {
            //                        PT_FindLineTools[m_PatNo, ii, i].InputImage = nTempImage;
            //                        PT_LineMaxTools[m_PatNo, ii, i].InputImage = nTempImage;
            //                    }
            //                }

            //                // JHKIM 호-직선 연계
            //                PT_CircleTool.InputImage = nTempImage;
            //            }
            //            if (M_TOOL_MODE == Main.DEFINE.M_FINDCIRCLETOOL)
            //            {
            //                PT_CircleTool.InputImage = nTempImage;
            //                for (int i = 0; i < Main.DEFINE.CIRCLE_MAX; i++)
            //                    PT_CircleTools[m_PatNo, i].InputImage = nTempImage;

            //                // JHKIM 호-직선 연계
            //                for (int ii = 0; ii < Main.DEFINE.SUBLINE_MAX; ii++)
            //                {
            //                    for (int i = 0; i < Main.DEFINE.FINDLINE_MAX; i++)
            //                    {
            //                        PT_FindLineTools[m_PatNo, ii, i].InputImage = nTempImage;
            //                        PT_LineMaxTools[m_PatNo, ii, i].InputImage = nTempImage;
            //                    }
            //                }
            //            }
            //            break;
            //    }//switch

            //}// try
            //catch
            //{

            //}
        }
        private bool Search_PATCNL()
        {
            return true;
            //bool nRet = false;
            //bool nRetSearch_CNL = false;
            //bool nRetSearch_PMA = false;

            //if (bROIFinealignTeach == true)
            //{
            //    if (bLiveStop == false)
            //    {
            //        Main.vision.Grab_Flag_End[m_CamNo] = false;
            //        Main.vision.Grab_Flag_Start[m_CamNo] = true;

            //        while (!Main.vision.Grab_Flag_End[m_CamNo] && !Main.DEFINE.OPEN_F)
            //        {
            //            Main.SearchDelay(1);
            //        }

            //        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //    }
            //    else
            //    {
            //        Save_SystemLog("Mark image Load", Main.DEFINE.CMD);
            //        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = PT_Display01.Image;
            //    }

            //    FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Run();
            //    Save_SystemLog("Mark Search start", Main.DEFINE.CMD);
            //    List<CogCompositeShape> ResultGraphic = new List<CogCompositeShape>();

            //    if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results != null)
            //    {
            //        if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count >= 1) nRetSearch_CNL = true;
            //    }
            //    if (nRetSearch_CNL)
            //    {
            //        Save_SystemLog("Mark G1", Main.DEFINE.CMD);
            //        if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count >= 1)
            //        {
            //            if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count >= 1)
            //            {
            //                for (int j = 0; j < FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count; j++)
            //                {
            //                    if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results != null)
            //                    {
            //                        ResultGraphic.Add(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[j].CreateResultGraphics(Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.MatchRegion | Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.Origin));
            //                    }
            //                }
            //            }
            //            if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score >= dFinealignMarkScore)
            //            {
            //                LABEL_MESSAGE(LB_MESSAGE, "Mark OK! " + "Score: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Lime);
            //            }
            //            else
            //            {
            //                LABEL_MESSAGE(LB_MESSAGE, "Mark NG! " + "Score: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Red);
            //            }

            //            if (!_useROITracking)
            //            {
            //                Draw_Label(PT_Display01, "Mark     X: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000"), 1);//
            //                Draw_Label(PT_Display01, "Mark     Y: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000"), 2);
            //                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score >= dFinealignMarkScore)
            //                    Draw_Label(PT_Display01, "Mark     OK! " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
            //                else
            //                    Draw_Label(PT_Display01, "Mark     NG! " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
            //            }
            //            nRet = true;

            //            PatResult.TranslationX = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX;
            //            PatResult.TranslationY = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY;

            //            string X = "X: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000");
            //            string Y = "Y: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000");
            //            LABEL_MESSAGE(LB_MESSAGE1, X + ", " + Y, System.Drawing.Color.Lime);

            //            double tempDataX = 0, tempDataY = 0;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX, FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY,
            //                                   ref tempDataX, ref tempDataY);

            //            string strLog = tempDataX.ToString("0.000") + "," + tempDataY.ToString("0.000");
            //            Save_SystemLog(strLog, Main.DEFINE.DATA);
            //            Save_SystemLog("Label ", Main.DEFINE.CMD);
            //        }

            //        for (int i = 0; i < ResultGraphic.Count; i++)
            //        {
            //            PT_Display01.StaticGraphics.Add(ResultGraphic[i] as ICogGraphic, "Mark");
            //        }
            //    }
            //    else
            //    {
            //        LABEL_MESSAGE(LB_MESSAGE, "Mark NG! ", System.Drawing.Color.Red);
            //        Save_SystemLog("Label NG ", Main.DEFINE.CMD);
            //    }

            //    return nRet;
            //}
            //else
            //{
            //    #region CNLSEARCH
            //    if (bLiveStop == false)
            //    {
            //        Main.vision.Grab_Flag_End[m_CamNo] = false;
            //        Main.vision.Grab_Flag_Start[m_CamNo] = true;

            //        while (!Main.vision.Grab_Flag_End[m_CamNo] && !Main.DEFINE.OPEN_F)
            //        {
            //            Main.SearchDelay(1);
            //        }

            //        PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //        PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //    }
            //    else
            //    {
            //        Save_SystemLog("Mark image Load", Main.DEFINE.CMD);
            //        PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = PT_Display01.Image;
            //        PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = PT_Display01.Image;
            //    }
            //    PT_Pattern[m_PatNo, m_PatNo_Sub].Run();
            //    Save_SystemLog("Mark Search start", Main.DEFINE.CMD);
            //    List<CogCompositeShape> ResultGraphic = new List<CogCompositeShape>();

            //    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results != null)
            //    {
            //        if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1) nRetSearch_CNL = true;
            //    }
            //    if (nRetSearch_CNL)
            //    {
            //        Save_SystemLog("Mark G1", Main.DEFINE.CMD);
            //        if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1)
            //        {

            //            if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1)
            //            {
            //                for (int j = 0; j < PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count; j++)
            //                {
            //                    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results != null)
            //                    {
            //                        ResultGraphic.Add(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[j].CreateResultGraphics(Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.MatchRegion | Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.Origin));
            //                    }
            //                }
            //            }
            //            Save_SystemLog("Mark G end", Main.DEFINE.CMD);
            //            if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score >= PT_AcceptScore[m_PatNo])
            //            {
            //                LABEL_MESSAGE(LB_MESSAGE, "Mark OK! " + "Score: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Lime);
            //            }
            //            else
            //            {
            //                LABEL_MESSAGE(LB_MESSAGE, "Mark NG! " + "Score: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Red);
            //            }

            //            if (!_useROITracking)
            //            {
            //                Draw_Label(PT_Display01, "Mark     X: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000"), 1);//
            //                Draw_Label(PT_Display01, "Mark     Y: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000"), 2);
            //                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score >= PT_AcceptScore[m_PatNo])
            //                    Draw_Label(PT_Display01, "Mark     OK! " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
            //                else
            //                    Draw_Label(PT_Display01, "Mark     NG! " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
            //                //shkang_Test_s(게이지 데이터용)
            //                //Draw_Label(PT_Display01, "TEST     X: " + ((PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX - (OriginImage.Width / 2)) * 13.36 / 1000).ToString("0.000") + "mm", 4);
            //                //Draw_Label(PT_Display01, "TEST     Y: " + ((PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY - (OriginImage.Height / 2)) * 13.36 / 1000).ToString("0.000")+ "mm", 5);
            //                //shkang_Test_e
            //            }
            //            nRet = true;

            //            PatResult.TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX;
            //            PatResult.TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY;

            //            string X = "X: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000");
            //            string Y = "Y: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000");
            //            LABEL_MESSAGE(LB_MESSAGE1, X + ", " + Y, System.Drawing.Color.Lime);

            //            double tempDataX = 0, tempDataY = 0;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
            //                                   ref tempDataX, ref tempDataY);

            //            string strLog = tempDataX.ToString("0.000") + "," + tempDataY.ToString("0.000");
            //            Save_SystemLog(strLog, Main.DEFINE.DATA);
            //            Save_SystemLog("Label ", Main.DEFINE.CMD);
            //        }
            //    }
            //    else
            //    {
            //        LABEL_MESSAGE(LB_MESSAGE, "Mark NG! ", System.Drawing.Color.Red);
            //        Save_SystemLog("Label NG ", Main.DEFINE.CMD);
            //    }

            //    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_PMAlign_Use)
            //    {
            //        PT_GPattern[m_PatNo, m_PatNo_Sub].Run();
            //        //      PT_Display01.Record = PT_GPattern[m_PatNo, m_PatNo_Sub].CreateLastRunRecord();

            //        if (PT_GPattern[m_PatNo, m_PatNo_Sub].Results != null)
            //        {
            //            if (PT_GPattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1) nRetSearch_PMA = true;
            //        }
            //        if (nRetSearch_PMA)
            //        {
            //            Save_SystemLog("Graphy add ", Main.DEFINE.CMD);
            //            ResultGraphic.Add(PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion | CogPMAlignResultGraphicConstants.MatchFeatures | CogPMAlignResultGraphicConstants.Origin));

            //            if (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].Score >= PT_GAcceptScore[m_PatNo])
            //            {
            //                LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + "\n" + "GMark OK! " + "Score: " + PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].Score.ToString("0.000") + "%", System.Drawing.Color.Lime);
            //            }
            //            else
            //            {
            //                LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + "\n" + "GMark NG! " + "Score: " + PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].Score.ToString("0.000") + "%", System.Drawing.Color.Red);
            //            }

            //            Draw_Label(PT_Display01, "GMark  X: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000"), 3);
            //            Draw_Label(PT_Display01, "GMark  Y: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000"), 4);

            //            string X = "G X: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000");
            //            string Y = "Y: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000");
            //            LABEL_MESSAGE(LB_MESSAGE1, LB_MESSAGE1.Text + "\n" + X + ", " + Y, System.Drawing.Color.Lime);

            //            double tempDataX = 0, tempDataY = 0, tempDataX2 = 0, tempDataY2 = 0;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
            //                                   ref tempDataX, ref tempDataY);
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
            //                                   ref tempDataX2, ref tempDataY2);

            //            string strLog = tempDataX.ToString("0.000") + "," + tempDataY.ToString("0.000") + "," + tempDataX2.ToString("0.000") + "," + tempDataY2.ToString("0.000");
            //            Save_SystemLog(strLog, Main.DEFINE.DATA);
            //        }
            //        else
            //        {
            //            LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + "\n" + "GMark NG! ", System.Drawing.Color.Red);
            //        }
            //    }
            //    for (int i = 0; i < ResultGraphic.Count; i++)
            //    {
            //        PT_Display01.StaticGraphics.Add(ResultGraphic[i] as ICogGraphic, "Mark");
            //    }
            //    Save_SystemLog("Mark Search end", Main.DEFINE.CMD);
            //    ////////////////수정할것 
            //    //       DisplayFit(PT_Display01);
            //    return nRet;
            //    #endregion
            //}

        }
        private bool Search_Caliper(bool nALLSEARCH)
        {
            return true;
            //bool nRet = true;
            //string strLog = "";
            //bool TempSelect = false;
            //int nStartNum = 0;
            //int nLastNum = 0;

            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //double[] tempData = new double[2];
            //double[] tempDataMark = new double[2];
            //long tempYLength = new long();

            //if (nALLSEARCH)
            //{
            //    nStartNum = 0;
            //    nLastNum = Main.DEFINE.CALIPER_MAX;
            //}
            //else
            //{
            //    nStartNum = m_SelectCaliper;
            //    nLastNum = m_SelectCaliper + 1;
            //}

            //for (int i = nStartNum; i < nLastNum; i++)
            //{
            //    if (PT_CaliPara[m_PatNo, i].m_UseCheck)
            //    {
            //        TempSelect = true;
            //        int nTempPlusMinus = 1;

            //        if (PT_Caliper_MarkUSE[m_PatNo])
            //        {
            //            (PT_CaliperTools[m_PatNo, i].Region as CogRectangleAffine).CenterX = PatResult.TranslationX + PT_CaliPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
            //            (PT_CaliperTools[m_PatNo, i].Region as CogRectangleAffine).CenterY = PatResult.TranslationY + PT_CaliPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
            //        }

            //        if (Main.ALIGNINSPECTION_USE(m_AlignNo))
            //        {
            //            PT_CaliperTools[m_PatNo, i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CaliperToolPairRun(PT_CaliperTools[m_PatNo, i], out nTempPlusMinus);
            //        }
            //        else
            //        {
            //            PT_CaliperTools[m_PatNo, i].Run();
            //        }

            //        if (PT_CaliperTools[m_PatNo, i].Results != null && PT_CaliperTools[m_PatNo, i].Results.Count > 0)
            //        {
            //            for (int j = 0; j < PT_CaliperTools[m_PatNo, i].Results.Count; j++)
            //            {
            //                resultGraphics.Add(PT_CaliperTools[m_PatNo, i].Results[j].CreateResultGraphics(CogCaliperResultGraphicConstants.Edges));
            //            }
            //            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //            //---------------------------------------------------------------------------------------------------------------------------------

            //            #region COF_LENGTH
            //            if (Main.AlignUnit[m_AlignNo].m_AlignName == "COF_CUTTING_ALIGN1" || Main.AlignUnit[m_AlignNo].m_AlignName == "COF_CUTTING_ALIGN2")
            //            {
            //                if (Main.GetCaliperDirection(Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, i].Region.Rotation)) == Main.DEFINE.X)
            //                {
            //                    // PatResult.TranslationX = PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionX;
            //                }
            //                if (Main.GetCaliperDirection(Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, i].Region.Rotation)) == Main.DEFINE.Y)
            //                {
            //                    // PatResult.TranslationY = PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionY;

            //                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(0, PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionY,
            //                    ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
            //                    if (PT_Caliper_MarkUSE[m_PatNo])
            //                    {
            //                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
            //                        ref tempDataMark[Main.DEFINE.X], ref tempDataMark[Main.DEFINE.Y]);
            //                        tempYLength = (long)(Math.Abs(tempDataMark[Main.DEFINE.Y] - tempData[Main.DEFINE.Y]));
            //                        LABEL_MESSAGE(LB_MESSAGE, "COF Y_LENGTH: " + tempYLength.ToString("00") + " um", System.Drawing.Color.Lime);
            //                    }
            //                }
            //            }
            //            #endregion

            //            #region BEAM_WIDTH
            //            for (int j = 0; j < PT_CaliperTools[m_PatNo, i].Results.Count; j++)
            //            {
            //                if (PT_CaliperTools[m_PatNo, i].RunParams.EdgeMode == CogCaliperEdgeModeConstants.Pair
            //                    && PT_CaliperTools[m_PatNo, i].Results.Edges.Count > 1)
            //                {
            //                    double dWidth = 0;
            //                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2RScalar(PT_CaliperTools[m_PatNo, i].Results[0].Width, ref dWidth);
            //                    strLog += i.ToString() + " " + dWidth.ToString("0.000") + " ";
            //                }
            //            }
            //            #endregion
            //            //---------------------------------------------------------------------------------------------------------------------------------
            //        }
            //        else
            //        {
            //            nRet = false;
            //            LABEL_MESSAGE(LB_MESSAGE, i.ToString("00") + " Caliper: Search NG! Check!!!", System.Drawing.Color.Red);
            //        }
            //    }
            //}

            //LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);

            //if (PT_CaliperTools[m_PatNo, m_SelectCaliper].Results != null && PT_CaliperTools[m_PatNo, m_SelectCaliper].Results.Count > 0 && PT_CaliPara[m_PatNo, m_SelectCaliper].m_UseCheck)
            //{
            //    DrawLastRegionData(PT_CALIPER_SUB_Display, PT_CaliperTools[m_PatNo, m_SelectCaliper]);
            //}
            //else
            //{
            //    Main.DisplayClear(PT_CALIPER_SUB_Display);
            //    PT_CALIPER_SUB_Display.Image = null;
            //}
            //if (!TempSelect)
            //{
            //    LABEL_MESSAGE(LB_MESSAGE, "All Caliper Not Use!!", System.Drawing.Color.Red);
            //    nRet = false;
            //}
            //return nRet;
        }
        #region MOVE_SIZE_LBMSSAGE
        private void BTN_MOVE_Click(object sender, EventArgs e)
        {
            //double nMoveDataX = 0, nMoveDataY = 0; //공통으로 쓸수 있도록 코딩.

            //int nMode = 0;
            //nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            //try
            //{
            //    Button TempBTN = (Button)sender;
            //    switch (TempBTN.Text.ToUpper().Trim())
            //    {
            //        case "LEFT":
            //            nMoveDataX = -1;
            //            nMoveDataY = 0;
            //            break;

            //        case "RIGHT":
            //            nMoveDataX = 1;
            //            nMoveDataY = 0;
            //            break;

            //        case "UP":
            //            nMoveDataX = 0;
            //            nMoveDataY = -1;
            //            break;

            //        case "DOWN":
            //            nMoveDataX = 0;
            //            nMoveDataY = 1;
            //            break;
            //    }

            //    nMoveDataX /= PT_Display01.Zoom;
            //    nMoveDataY /= PT_Display01.Zoom;

            //    if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
            //    {
            //        if (m_RetiMode == M_PATTERN) { PatMaxTrainRegion.X += nMoveDataX; PatMaxTrainRegion.Y += nMoveDataY; }
            //        if (m_RetiMode == M_SEARCH) { PatMaxSearchRegion.X += nMoveDataX; PatMaxSearchRegion.Y += nMoveDataY; }
            //        if (m_RetiMode == M_ORIGIN) { MarkORGPoint.X += nMoveDataX; MarkORGPoint.Y += nMoveDataY; }
            //    }
            //    if (nMode == Main.DEFINE.M_BLOBTOOL)
            //    {
            //        BlobTrainRegion.CenterX += nMoveDataX;
            //        BlobTrainRegion.CenterY += nMoveDataY;
            //    }
            //    if (nMode == Main.DEFINE.M_CALIPERTOOL)
            //    {
            //        PTCaliperRegion.CenterX += nMoveDataX;
            //        PTCaliperRegion.CenterY += nMoveDataY;
            //    }
            //    if (nMode == Main.DEFINE.M_FINDLINETOOL)
            //    {
            //        PT_FindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
            //        PT_FindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;

            //        PT_FindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
            //        PT_FindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
            //    }
            //    if (nMode == Main.DEFINE.M_FINDCIRCLETOOL)
            //    {
            //        PT_CircleTool.RunParams.ExpectedCircularArc.CenterX += nMoveDataX;
            //        PT_CircleTool.RunParams.ExpectedCircularArc.CenterY += nMoveDataY;

            //    }
            //    if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
            //    {
            //        if (Chk_All_Select.Checked == true)
            //        {
            //            CogGraphicInteractiveCollection GraphicCollection = new CogGraphicInteractiveCollection();

            //            Parallel.For(0, m_TeachParameter.Count, i =>
            //            {
            //                var Tempdata = m_TeachParameter[i];

            //                if ((enumROIType)Tempdata.m_enumROIType == (enumROIType)enumROIType.Line)
            //                {
            //                    //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
            //                    //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
            //                    //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;
            //                    //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
            //                    //TrackLineROI(Tempdata.m_FindLineTool);

            //                    m_TeachParameter[i].m_FindLineTool.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
            //                    m_TeachParameter[i].m_FindLineTool.RunParams.CaliperRunParams.Edge1Polarity = CogCaliperPolarityConstants.LightToDark;

            //                    //GraphicCollection.Add((ICogGraphic)Tempdata.m_FindLineTool.CreateCurrentRecord());

            //                    //GraphicData.Add(SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge));

            //                    PT_Display01.InteractiveGraphics.AddList(GraphicCollection, "GraphicCollection", false);
            //                }
            //                else
            //                {
            //                    //Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterX += nMoveDataX;
            //                    //Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterY += nMoveDataY;

            //                    m_TeachParameter[i].m_FindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
            //                    m_TeachParameter[i].m_FindCircleTool.RunParams.CaliperRunParams.Edge1Polarity = CogCaliperPolarityConstants.DarkToLight;

            //                    //TrackCircleROI(Tempdata.m_FindCircleTool);
            //                }
            //            });


            //            //for (int i = 0; i < m_TeachParameter.Count; i++)
            //            //{
            //            //    var Tempdata = m_TeachParameter[i];

            //            //    if ((enumROIType)Tempdata.m_enumROIType == (enumROIType)enumROIType.Line)
            //            //    {
            //            //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
            //            //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
            //            //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;
            //            //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
            //            //        TrackLineROI(Tempdata.m_FindLineTool);
            //            //    }
            //            //    else
            //            //    {
            //            //        Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterX += nMoveDataX;
            //            //        Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterY += nMoveDataY;
            //            //        TrackCircleROI(Tempdata.m_FindCircleTool);
            //            //    }
            //            //}
            //        }
            //        else
            //        {
            //            if (m_TempFindLineTool != null)
            //            {
            //                m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
            //                m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;

            //                m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
            //                m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
            //            }
            //        }
            //    }
            //}
            //catch
            //{

            //}
        }
        private void BTN_SIZE_Click(object sender, EventArgs e)
        {
            //double nMoveDataX = 0, nMoveDataY = 0; //공통으로 쓸수 있도록 코딩.

            //int nMode = 0;
            //nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            //try
            //{
            //    Button TempBTN = (Button)sender;
            //    switch (TempBTN.Text.ToUpper())
            //    {
            //        case "X_DEC":
            //            nMoveDataX = -2;
            //            nMoveDataY = 0;
            //            break;

            //        case "X_INC":
            //            nMoveDataX = 2;
            //            nMoveDataY = 0;
            //            break;

            //        case "Y_DEC":
            //            nMoveDataX = 0;
            //            nMoveDataY = -2;
            //            break;

            //        case "Y_INC":
            //            nMoveDataX = 0;
            //            nMoveDataY = 2;
            //            break;
            //    }

            //    if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
            //    {
            //        if (m_RetiMode == M_PATTERN) { PatMaxTrainRegion.SetCenterWidthHeight(PatMaxTrainRegion.CenterX, PatMaxTrainRegion.CenterY, PatMaxTrainRegion.Width += nMoveDataX, PatMaxTrainRegion.Height += nMoveDataY); }
            //        if (m_RetiMode == M_SEARCH) { PatMaxSearchRegion.SetCenterWidthHeight(PatMaxSearchRegion.CenterX, PatMaxSearchRegion.CenterY, PatMaxSearchRegion.Width += nMoveDataX, PatMaxSearchRegion.Height += nMoveDataY); }
            //    }

            //    if (nMode == Main.DEFINE.M_BLOBTOOL)
            //    {
            //        BlobTrainRegion.SetCenterLengthsRotationSkew(BlobTrainRegion.CenterX, BlobTrainRegion.CenterY, BlobTrainRegion.SideXLength += nMoveDataX, BlobTrainRegion.SideYLength += nMoveDataY, BlobTrainRegion.Rotation, BlobTrainRegion.Skew);
            //    }

            //    if (nMode == Main.DEFINE.M_CALIPERTOOL)
            //    {
            //        PTCaliperRegion.SideXLength += nMoveDataX;
            //        PTCaliperRegion.SideYLength += nMoveDataY;
            //    }

            //    if (nMode == Main.DEFINE.M_FINDLINETOOL)
            //    {
            //        PT_FindLineTool.RunParams.CaliperProjectionLength += nMoveDataX;
            //        PT_FindLineTool.RunParams.CaliperSearchLength += nMoveDataY;
            //    }

            //    PSizeLabel();
            //}
            //catch
            //{
            //}
        }
        private void BTN_SIZE_INPUT(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            //{
            //    double nSizeDataX = 0, nSizeDataY = 0; //공통으로 쓸수 있도록 코딩.
            //    double nMinSizeX = 0, nMinSizeY = 0;
            //    double nInputMinSizeX = 2, nInputMinSizeY = 2;
            //    int nMode = 0;
            //    nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            //    try
            //    {

            //        if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
            //        {
            //            if (m_RetiMode == M_PATTERN)
            //            {
            //                nSizeDataX = PatMaxTrainRegion.Width;
            //                nSizeDataY = PatMaxTrainRegion.Height;
            //            }
            //            if (m_RetiMode == M_SEARCH)
            //            {
            //                nSizeDataX = PatMaxSearchRegion.Width;
            //                nSizeDataY = PatMaxSearchRegion.Height;
            //            }
            //        }

            //        if (nMode == Main.DEFINE.M_BLOBTOOL)
            //        {
            //            nSizeDataX = BlobTrainRegion.SideXLength;
            //            nSizeDataY = BlobTrainRegion.SideYLength;
            //        }

            //        if (nMode == Main.DEFINE.M_CALIPERTOOL)
            //        {
            //            nSizeDataX = PTCaliperRegion.SideXLength;
            //            nSizeDataY = PTCaliperRegion.SideYLength;
            //            nInputMinSizeX = nMinSizeX = PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.FilterHalfSizeInPixels * 2 + 2.5;
            //        }

            //        if (nMode == Main.DEFINE.M_FINDLINETOOL)
            //        {
            //            nSizeDataX = PT_FindLineTool.RunParams.CaliperProjectionLength;
            //            nSizeDataY = PT_FindLineTool.RunParams.CaliperSearchLength;
            //            nInputMinSizeY = nMinSizeY = (PT_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels * 2 + 2.5);
            //        }
            //        if (nMode == Main.DEFINE.M_FINDCIRCLETOOL)
            //        {
            //            nSizeDataX = PT_CircleTool.RunParams.CaliperProjectionLength;
            //            nSizeDataY = PT_CircleTool.RunParams.CaliperSearchLength;
            //            nInputMinSizeY = nMinSizeY = (PT_CircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels * 2 + 2.5);
            //        }

            //        Button TempBTN = (Button)sender;
            //        switch (TempBTN.Text.ToUpper())
            //        {
            //            case "X_DEC":
            //            case "X_INC":
            //                KeyPadForm KeyPadForm = new KeyPadForm(nInputMinSizeX, 50000, nSizeDataX, "X AREA SIZE", 1);
            //                KeyPadForm.ShowDialog();
            //                if (KeyPadForm.m_data > nMinSizeX) nSizeDataX = KeyPadForm.m_data;

            //                break;
            //            case "Y_DEC":
            //            case "Y_INC":

            //                KeyPadForm KeyPadForm1 = new KeyPadForm(nInputMinSizeY, 50000, nSizeDataY, "Y AREA SIZE", 1);
            //                KeyPadForm1.ShowDialog();
            //                if (KeyPadForm1.m_data > nMinSizeY) nSizeDataY = KeyPadForm1.m_data;
            //                break;
            //        }

            //        if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
            //        {
            //            if (m_RetiMode == M_PATTERN) { PatMaxTrainRegion.SetCenterWidthHeight(PatMaxTrainRegion.CenterX, PatMaxTrainRegion.CenterY, nSizeDataX, nSizeDataY); }
            //            if (m_RetiMode == M_SEARCH) { PatMaxSearchRegion.SetCenterWidthHeight(PatMaxSearchRegion.CenterX, PatMaxSearchRegion.CenterY, nSizeDataX, nSizeDataY); }
            //        }

            //        if (nMode == Main.DEFINE.M_BLOBTOOL)
            //        {
            //            BlobTrainRegion.SetCenterLengthsRotationSkew(BlobTrainRegion.CenterX, BlobTrainRegion.CenterY, nSizeDataX, nSizeDataY, BlobTrainRegion.Rotation, BlobTrainRegion.Skew);
            //        }

            //        if (nMode == Main.DEFINE.M_CALIPERTOOL)
            //        {
            //            PTCaliperRegion.SideXLength = nSizeDataX;
            //            PTCaliperRegion.SideYLength = nSizeDataY;
            //        }

            //        if (nMode == Main.DEFINE.M_FINDLINETOOL)
            //        {
            //            PT_FindLineTool.RunParams.CaliperProjectionLength = nSizeDataX;
            //            PT_FindLineTool.RunParams.CaliperSearchLength = nSizeDataY;
            //        }
            //        if (nMode == Main.DEFINE.M_FINDCIRCLETOOL)
            //        {
            //            PT_CircleTool.RunParams.CaliperProjectionLength = nSizeDataX;
            //            PT_CircleTool.RunParams.CaliperSearchLength = nSizeDataY;
            //        }
            //        PSizeLabel();
            //    }
            //    catch
            //    {

            //    }
            //}
        }
        private void ORGSizeFit()
        {
            try
            {
                int nZoomSize = 1;

                nZoomSize = (int)(CogDisplay.Zoom * ORIGIN_SIZE);
                if (nZoomSize < 1)
                    OriginMarkPoint.SizeInScreenPixels = ORIGIN_SIZE;
                else
                    OriginMarkPoint.SizeInScreenPixels = nZoomSize;
            }
            catch
            {

            }
        }
        private void PSizeLabel()
        {
            //int nMode = 0;
            //nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);

            //if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
            //{
            //    if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN) { LABEL_MESSAGE(LB_MESSAGE1, "X:" + PatMaxTrainRegion.Width.ToString("0.0") + " , " + "Y:" + PatMaxTrainRegion.Height.ToString("0.0"), System.Drawing.Color.GreenYellow); }
            //    if (m_RetiMode == M_SEARCH) { LABEL_MESSAGE(LB_MESSAGE1, "X:" + PatMaxSearchRegion.Width.ToString("0.0") + " , " + "Y:" + PatMaxSearchRegion.Height.ToString("0.0"), System.Drawing.Color.GreenYellow); }
            //}

            //if (nMode == Main.DEFINE.M_BLOBTOOL)
            //{
            //    LABEL_MESSAGE(LB_MESSAGE1, "X:" + BlobTrainRegion.SideXLength.ToString("0.0") + " , " + "Y:" + BlobTrainRegion.SideYLength.ToString("0.0"), System.Drawing.Color.GreenYellow);
            //}

            //if (nMode == Main.DEFINE.M_CALIPERTOOL)
            //{
            //    LABEL_MESSAGE(LB_MESSAGE1, "X:" + PTCaliperRegion.SideXLength.ToString("0.0") + " , " + "Y:" + PTCaliperRegion.SideYLength.ToString("0.0"), System.Drawing.Color.GreenYellow);
            //}
            //if (nMode == Main.DEFINE.M_FINDLINETOOL)
            //{
            //    LABEL_MESSAGE(LB_MESSAGE1, "X:" + PT_FindLineTool.RunParams.CaliperProjectionLength.ToString("0.0") + " , " + "Y:" + PT_FindLineTool.RunParams.CaliperSearchLength.ToString("0.0"), System.Drawing.Color.GreenYellow);
            //}
        }
        private void LABEL_MESSAGE(Label nlabel, string nText, Color nColor)
        {
            nlabel.ForeColor = nColor;
            nlabel.Text = nText;
        }

        #region Distance
        private void DistanceLine()
        {
            //for (int i = 0; i < Main.DEFINE.Pattern_Max; i++)
            //{
            //    for (int j = 0; j < 2; j++)
            //    {
            //        MarkPoint[i, j] = new CogPointMarker();
            //        MarkPoint[i, j].LineStyle = CogGraphicLineStyleConstants.Dot;
            //        if (j == 0)
            //        {
            //            MarkPoint[i, j].Color = CogColorConstants.Green;
            //            MarkPoint[i, j].SelectedColor = CogColorConstants.Green;
            //        }
            //        else
            //        {
            //            MarkPoint[i, j].Color = CogColorConstants.Red;
            //            MarkPoint[i, j].SelectedColor = CogColorConstants.Red;
            //        }
            //        MarkPoint[i, j].GraphicDOFEnable = CogPointMarkerDOFConstants.All;
            //        MarkPoint[i, j].Interactive = true;
            //        MarkPoint[i, j].SizeInScreenPixels = nCrossSize;
            //        MarkPoint[i, j].X = (double)Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_CamNo] + (50 * j);
            //        MarkPoint[i, j].Y = (double)Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_CamNo];
            //    }
            //}


        }
        private void PT_Display_DoubleClick(object sender, EventArgs e)
        {
            //CogDisplay TempLB = (CogDisplay)sender;
            //try
            //{
            //    int nNum;
            //    nNum = Convert.ToInt16(TempLB.Name.Substring(TempLB.Name.Length - 1, 1));
            //    if (nNum == 2)
            //    {
            //        bool nMarkUse = false;
            //        if (M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
            //            if (PT_Blob_MarkUSE[m_PatNo]) nMarkUse = true;
            //        if (M_TOOL_MODE == Main.DEFINE.M_CALIPERTOOL)
            //            if (PT_Caliper_MarkUSE[m_PatNo] || PT_Blob_CaliperUSE[m_PatNo]) nMarkUse = true;
            //        if (M_TOOL_MODE == Main.DEFINE.M_FINDLINETOOL)
            //            if (PT_FindLine_MarkUSE[m_PatNo]) nMarkUse = true;

            //        if (nMarkUse)
            //        {
            //            for (int j = 0; j < 2; j++)
            //            {
            //                MarkPoint[m_PatNo, j].X = (double)Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] + (50 * j) - PatResult.TranslationX;
            //                MarkPoint[m_PatNo, j].Y = (double)Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] - PatResult.TranslationY;
            //            }
            //        }
            //        else
            //        {
            //            for (int j = 0; j < 2; j++)
            //            {
            //                MarkPoint[m_PatNo, j].X = (double)Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] + (50 * j);
            //                MarkPoint[m_PatNo, j].Y = (double)Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo];
            //            }
            //        }
            //    }
            //    TempLB.InteractiveGraphics.Add(MarkPoint[m_PatNo, 0] as ICogGraphicInteractive, "Distance", false);
            //    TempLB.InteractiveGraphics.Add(MarkPoint[m_PatNo, 1] as ICogGraphicInteractive, "Distance", false);
            //    nDistanceShow[m_PatNo] = true;
            //}
            //catch
            //{

            //}
        }
        private void PT_Display_MouseUp(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    CogDisplay TempLB = (CogDisplay)sender;
            //    try
            //    {
            //        if (nDistanceShow[m_PatNo])
            //        {
            //            nDistance.InputImage = TempLB.Image;

            //            double nStartX = 0, nStartY = 0;
            //            double nEndX = 10, nEndY = 10;

            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(MarkPoint[m_PatNo, 0].X, MarkPoint[m_PatNo, 0].Y, ref nStartX, ref nStartY);
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(MarkPoint[m_PatNo, 1].X, MarkPoint[m_PatNo, 1].Y, ref nEndX, ref nEndY);

            //            nDistance.StartX = nStartX;
            //            nDistance.StartY = nStartY;

            //            nDistance.EndX = nEndX;
            //            nDistance.EndY = nEndY;
            //            nDistance.Run();
            //            LABEL_MESSAGE(LB_MESSAGE, nDistance.Distance.ToString("0.0") + " um" + " , " + (Main.DEFINE.degree * nDistance.Angle).ToString("0.000") + " Deg", System.Drawing.Color.Red);

            //            nDistance.StartX = MarkPoint[m_PatNo, 0].X;
            //            nDistance.StartY = MarkPoint[m_PatNo, 0].Y;

            //            nDistance.EndX = MarkPoint[m_PatNo, 1].X;
            //            nDistance.EndY = MarkPoint[m_PatNo, 1].Y;
            //            nDistance.Run();
            //            LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + " , " + nDistance.Distance.ToString("0.0") + " Pixel", System.Drawing.Color.Red);
            //        }
            //        PSizeLabel();
            //    }
            //    catch
            //    {
            //        LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
            //    }
            //}
        }
        #endregion


        private void LB_CAMCENTER_DoubleClick(object sender, EventArgs e)
        {
            //if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            //{
            //    MarkORGPoint.X = Main.vision.IMAGE_CENTER_X[m_CamNo];
            //    MarkORGPoint.Y = Main.vision.IMAGE_CENTER_Y[m_CamNo];

            //}
        }

        private void CogMarkDisplay_Click(object sender, EventArgs e)
        {
            CogRecordDisplay TempNum = (CogRecordDisplay)sender;
            int index = Convert.ToInt16(TempNum.Name.Substring(TempNum.Name.Length - 2, 2));

            ChangeMark(index);
        }

        private void ChangeMark(int index)
        {
            if (_selectedMarkIndex < 0)
                return;

            var prevMark = GetMarkUnit().TagList[_selectedMarkIndex];
            prevMark.SetOrginMark(OriginMarkPoint);

            _selectedMarkIndex = index;
            CB_SUB_PATTERN.SelectedIndex = index;

            if (index == 0)
                BTN_MAINORIGIN_COPY.Visible = false;
            else
                BTN_MAINORIGIN_COPY.Visible = true;

            UpdateMarkInfo();
            ClearDisplayGraphic();
            ClearMarkButtonBackColor();
        }

        private void BTN_MAINORIGIN_COPY_Click(object sender, EventArgs e)
        {
            //if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            //{
            //    bool SearchResult = false;
            //    if (PT_Pattern[m_PatNo, 0].Pattern.Trained == false)
            //    {
            //        MarkORGPoint.SetCenterRotationSize(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 0, M_ORIGIN_SIZE);
            //        ORGSizeFit();
            //    }
            //    else
            //    {
            //        PT_Pattern[m_PatNo, 0].Run();
            //        if (PT_Pattern[m_PatNo, 0].Results != null)
            //        {
            //            if (PT_Pattern[m_PatNo, 0].Results.Count >= 1) SearchResult = true;
            //        }
            //        if (SearchResult)
            //        {
            //            MarkORGPoint.X = PatResult.TranslationX;  //PT_Pattern[m_PatNo, 0].Pattern.Origin.TranslationX;
            //            MarkORGPoint.Y = PatResult.TranslationY; // PT_Pattern[m_PatNo, 0].Pattern.Origin.TranslationY;
            //        }
            //    }

            //}
        }
        private void LB_PATTERN_08_Click(object sender, EventArgs e)
        {
            Label TempNum = (Label)sender;
            int index = Convert.ToInt16(TempNum.Name.Substring(TempNum.Name.Length - 2, 2));

            if(GetMarkUnit() is MarkUnit unit)
            {
                var tag = unit.TagList[index];
                tag.Use = !tag.Use;
                if (_selectedMarkIndex == index)
                {
                    CB_SUBPAT_USE.Visible = true;
                    CB_SUBPAT_USE.Checked = tag.Use;
                }
                if (tag.Use)
                    MarkLabelList[index].BackColor = Color.LawnGreen;
                else
                    MarkLabelList[index].BackColor = Color.WhiteSmoke;
            }
        }

        private void PT_Display01_Changed(object sender, CogChangedEventArgs e)
        {
            if (AppsStatus.Instance().UI_STATUS == UI_STATUS.TEACH_FORM)
            {
                if (CogDisplay.Zoom != ZoomBackup)
                {
                    ZoomBackup = CogDisplay.Zoom;
                    ORGSizeFit();
                }
            }
        }
        private void BTN_PATTERN_MASK_Click(object sender, EventArgs e)
        {
            ////2023 0225 YSH ROI Finealign 
            //if (bROIFinealignTeach)
            //{
            //    if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Trained)
            //    {
            //        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = CopyIMG(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.GetTrainedPatternImage());
            //        FormPatternMask.BackUpSearchMaxTool = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub];
            //        FormPatternMask.ShowDialog();

            //        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub] = FormPatternMask.BackUpSearchMaxTool;

            //        DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], FinealignMark[nROIFineAlignIndex, m_PatNo_Sub]);
            //    }
            //}
            //else
            //{
            //    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained)
            //    {
            //        //                 PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
            //        //                 PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);

            //        PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage);
            //        PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage);

            //        FormPatternMask.BackUpSearchMaxTool = PT_Pattern[m_PatNo, m_PatNo_Sub];
            //        FormPatternMask.BackUpPMAlignTool = PT_GPattern[m_PatNo, m_PatNo_Sub];
            //        FormPatternMask.ShowDialog();

            //        PT_Pattern[m_PatNo, m_PatNo_Sub] = FormPatternMask.BackUpSearchMaxTool;
            //        PT_GPattern[m_PatNo, m_PatNo_Sub] = FormPatternMask.BackUpPMAlignTool;

            //        DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], PT_Pattern[m_PatNo, m_PatNo_Sub]);
            //    }
            //}

        }
        private void BTN_PATTERN_SCORE_Click(object sender, EventArgs e)
        {

        }
        private void LB_RECTANGLE_Click(object sender, EventArgs e)
        {
            //if (M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
            //{
            //    BlobTrainRegion.Rotation = 0;
            //    BlobTrainRegion.SetCenterLengthsRotationSkew(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 200, 200, 0, 0);
            //}
        }
        private void BTN_PATTERN_COPY_Click(object sender, EventArgs e)
        {
            ////2022 0902 YSH   
            ////현재 인덱스를 기준으로 Caliper Data 모두 copy, save, load 진행
            //DialogResult result = MessageBox.Show("Do you want to Vision Data Copy?", "COPY", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //if (result == DialogResult.Yes)
            //{
            //    for (int i = 0; i < 2; i++)
            //    {
            //        for (int j = 0; j < 2; j++)
            //        {
            //            Main.AlignUnit[i].PAT[j, 0].m_InspParameter = m_TeachParameter;
            //            for (int k = 0; k < 4; k++)
            //            {
            //                Main.AlignUnit[i].PAT[j, 0].m_TrackingLine[k] = m_TeachLine[k];
            //                Main.AlignUnit[i].PAT[j, 0].m_BondingAlignLine[k] = m_TeachAlignLine[k];    //shkang
            //            }
            //            Main.AlignUnit[i].PAT[j, 0].m_FinealignMark = FinealignMark;
            //            Main.AlignUnit[i].PAT[j, 0].m_bFInealignFlag = m_bROIFinealignFlag;
            //            Main.AlignUnit[i].PAT[j, 0].m_FinealignThetaSpec = m_dROIFinealignT_Spec;
            //            Main.AlignUnit[i].PAT[j, 0].m_FinealignMarkScore = dFinealignMarkScore;

            //        }
            //    }

            //    for (int i = 0; i < 2; i++)
            //    {
            //        for (int j = 0; j < 2; j++)
            //        {
            //            Main.AlignUnit[i].Save(j);
            //            Main.AlignUnit[i].Load(j);
            //        }
            //    }
            //}

        }

        #endregion

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, bool oldPara, bool newPara)
        {
            //string strLog = "CAM" + nAlignUnit.ToString();

            //if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
            //    strLog += " ALIGN ";
            //else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
            //    strLog += " INSPECTION ";

            //if (oldPara != newPara)
            //{
            //    strLog += ParaName + " [" + oldPara.ToString() + "] ▶▷▶ [" + newPara.ToString() + "]";
            //    Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            //}
        }

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, int oldPara, int newPara)
        {
            //string strLog = "CAM" + nAlignUnit.ToString();

            //if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
            //    strLog += " ALIGN ";
            //else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
            //    strLog += " INSPECTION ";

            //if (oldPara != newPara)
            //{
            //    strLog += ParaName + " [" + oldPara + "] ▶▷▶ [" + newPara + "]";
            //    Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            //}
        }

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, double oldPara, double newPara)
        {
            //string strLog = "CAM" + nAlignUnit.ToString();

            //if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
            //    strLog += " ALIGN ";
            //else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
            //    strLog += " INSPECTION ";

            //if (oldPara != newPara)
            //{
            //    strLog += ParaName + " [" + oldPara.ToString("0.0000") + "] ▶▷▶ [" + newPara.ToString("0.0000") + "]";
            //    Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            //}
        }

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, string oldPara, string newPara)
        {
            //string strLog = "CAM" + nAlignUnit.ToString();

            //if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
            //    strLog += " ALIGN ";
            //else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
            //    strLog += " INSPECTION ";

            //if (oldPara != newPara)
            //{
            //    strLog += ParaName + " [" + oldPara + "] ▶▷▶ [" + newPara + "]";
            //    Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            //}
        }

        object syncLock_Log = new object();
        private void Save_SystemLog(string nMessage, string nType)
        {
            //string nFolder;
            //string nFileName = "";
            //nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            //if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            //if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            //string Date;
            //Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            //lock (syncLock_Log)
            //{
            //    try
            //    {
            //        switch (nType)
            //        {
            //            case Main.DEFINE.CHANGEPARA:
            //                nFileName = "ChangePara.txt";
            //                nMessage = Date + nMessage;
            //                break;
            //            case Main.DEFINE.DATA:
            //                nFileName = "Data.csv";
            //                nMessage = Date + nMessage;
            //                break;
            //            case Main.DEFINE.CMD:
            //                nFileName = "CMD.txt";
            //                nMessage = Date + nMessage;
            //                break;
            //        }

            //        StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
            //        SW.WriteLine(nMessage);
            //        SW.Close();
            //    }
            //    catch
            //    {

            //    }
            //}
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //if (PT_DISPLAY_CONTROL.CrossLineChecked) CrossLine();
            //timer2.Enabled = false;
        }


        #region SD BIO
        private void ROIType(object sender, EventArgs e)
        {
            //Button Btn = (Button)sender;
            //if (Convert.ToInt32(Btn.Tag.ToString()) == 0)
            //{
            //    m_enumROIType = enumROIType.Line;
            //}
            //else
            //{
            //    m_enumROIType = enumROIType.Circle;
            //}
            //Set_InspParams();
        }

        private void btn_ROI_SHOW_Click(object sender, EventArgs e)
        {
           // ExecuteROIShow();
        }

        private void ExecuteROIShow()
        {
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //UpDataTool();
            //SetText();
            ////shkang_s
            //string strTemp;
            //int itype;
            //strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
            //if (strTemp == "Line")
            //    itype = 0;
            //else
            //    itype = 1;
            //m_enumROIType = (enumROIType)itype;
            ////shkang_e
            //if (m_enumROIType == enumROIType.Line)
            //    TrackLineROI(m_TempFindLineTool);
            //else
            //    TrackCircleROI(m_TempFindCircleTool);
        }

        private void chkUseRoiTracking_CheckedChanged(object sender, EventArgs e)
        {
            //PrevCenterX = 0;
            //PrevCenterY = 0;
            //PrevMarkX = 0;
            //PrevMarkY = 0;
            //m_bTrakingRoot[m_BlobROI] = false;
            //if (chkUseRoiTracking.Checked == true)
            //{
            //    //Live Mode On상태일 시, Off로 변경
            //    if (BTN_LIVEMODE.Checked)
            //    {
            //        BTN_LIVEMODE.Checked = false;
            //        BTN_LIVEMODE.BackColor = Color.DarkGray;
            //    }
            //    PT_Display01.Image = OriginImage;

            //    if (FinalTracking() == true)
            //        _useROITracking = chkUseRoiTracking.Checked;
            //    UpDataTool();
            //    SetText();

            //    //shkang_s
            //    string strTemp;
            //    int itype;
            //    strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
            //    if (strTemp == "Line")
            //        itype = 0;
            //    else
            //        itype = 1;
            //    m_enumROIType = (enumROIType)itype;
            //    //shkang_e

            //    if (m_enumROIType == enumROIType.Line)
            //        TrackLineROI(m_TempFindLineTool);
            //    else
            //        TrackCircleROI(m_TempFindCircleTool);
            //}
            //else
            //{
            //    PT_Display01.Image = OriginImage;
            //}
        }

        private bool TrackingROI(double PatPointX, double PatPointY, double dTransX, double dTransY)
        {
            return true;
            //double ROIX = 0, ROIY = 0, RotT = 0;
            //bool Res = false;
            //if (LineTrakingROI(dTransX, dTransY, ref ROIX, ref ROIY, ref RotT))
            //{
            //    Res = true;
            //    if (!m_bROIFinealignFlag)
            //    {
            //        CogFixtureTool mCogFixtureTool = new CogFixtureTool();
            //        mCogFixtureTool.InputImage = PT_Display01.Image;
            //        CogTransform2DLinear TempData = new CogTransform2DLinear();
            //        TempData.TranslationX = PatPointX;
            //        TempData.TranslationY = PatPointY;
            //        TempData.Rotation = RotT;
            //        mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
            //        mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
            //        mCogFixtureTool.Run();
            //        PT_Display01.InteractiveGraphics.Clear();
            //        PT_Display01.StaticGraphics.Clear();

            //        PT_Display01.Image = mCogFixtureTool.OutputImage;
            //    }

            //}
            //else
            //{
            //}
            //return Res;
        }

        private bool LineTrakingROI(double dTransX, double dTransY, ref double ROIX, ref double ROIY, ref double RotT)
        {
            return true;
            //bool Res = false;
            //try
            //{
            //    CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //    PT_Display01.InteractiveGraphics.Clear();
            //    PT_Display01.StaticGraphics.Clear();
            //    resultGraphics.Clear();
            //    double[] dx = new double[4];
            //    double[] dy = new double[4];
            //    bool bSearchRes = Search_PATCNL();
            //    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score <= PT_AcceptScore[m_PatNo])
            //    {
            //        MessageBox.Show("Mark Search Fail");
            //        return false;
            //    }
            //    if (bSearchRes == true)
            //    {
            //        double TranslationX = dTransX;
            //        double TranslationY = dTransY;
            //        CogIntersectLineLineTool[] CrossPoint = new CogIntersectLineLineTool[2];
            //        CogLine[] Line = new CogLine[4];
            //        for (int i = 0; i < 4; i++)
            //        {
            //            if (i < 2)
            //            {
            //                CrossPoint[i] = new CogIntersectLineLineTool();
            //                CrossPoint[i].InputImage = (CogImage8Grey)PT_Display01.Image;
            //            }
            //            Line[i] = new CogLine();
            //            m_TeachLine[i].InputImage = (CogImage8Grey)PT_Display01.Image;
            //            double TempStartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX;
            //            double TempStartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY;
            //            double TempEndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX;
            //            double TempEndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY;


            //            double StartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX - TranslationX;
            //            double StartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY - TranslationY;
            //            double EndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX - TranslationX;
            //            double EndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY - TranslationY;

            //            m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = StartA_X;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = StartA_Y;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = EndA_X;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = EndA_Y;

            //            m_TeachLine[i].Run();

            //            if (m_TeachLine[i].Results != null)
            //            {
            //                //shkang_
            //                if (Line[i] == null)
            //                    Line[i] = new CogLine();

            //                Line[i] = m_TeachLine[i].Results.GetLine();
            //                if (i < 2)
            //                    Line[i].Color = CogColorConstants.Blue;
            //                else
            //                    Line[i].Color = CogColorConstants.Orange;
            //            }
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = TempStartA_X;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = TempStartA_Y;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = TempEndA_X;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = TempEndA_Y;
            //        }

            //        CrossPoint[0].LineA = Line[0];
            //        CrossPoint[0].LineB = Line[1];
            //        CrossPoint[1].LineA = Line[2];
            //        CrossPoint[1].LineB = Line[3];
            //        for (int i = 0; i < 2; i++)
            //        {
            //            CogGraphicLabel ThetaLabelTest = new CogGraphicLabel();
            //            ThetaLabelTest.Font = new Font(Main.DEFINE.FontStyle, 15, FontStyle.Bold);
            //            ThetaLabelTest.Color = CogColorConstants.Green;
            //            if (Line[0] == null || Line[1] == null) return false;
            //            if (Line[2] == null || Line[3] == null) return false;
            //            CrossPoint[i].Run();
            //            if (CrossPoint[i] != null)
            //            {
            //                dCrossX[i] = CrossPoint[i].X;
            //                dCrossY[i] = CrossPoint[i].Y;
            //                dAngle[i] = CrossPoint[i].Angle;
            //                CogPointMarker PointMark = new CogPointMarker();
            //                PointMark.Color = CogColorConstants.Green;
            //                PointMark.SizeInScreenPixels = 50;
            //                PointMark.X = CrossPoint[i].X;
            //                PointMark.Y = CrossPoint[i].Y;
            //                PointMark.Rotation = dAngle[i];
            //                if (i == 0)
            //                {
            //                    ThetaLabelTest.X = 350;
            //                    ThetaLabelTest.Y = 100;
            //                    ThetaLabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
            //                }
            //                else
            //                {
            //                    ThetaLabelTest.X = 350;
            //                    ThetaLabelTest.Y = 250;
            //                    ThetaLabelTest.Text = string.Format("Right Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
            //                }
            //            }
            //        }
            //    }
            //    double TrackingX, TrackingY, dRotT;
            //    TrackingROICalculate(out TrackingX, out TrackingY, out dRotT);
            //    RotT = dRotT;
            //    ROIX = TrackingX;
            //    ROIY = TrackingY;

            //    Res = true;
            //}
            //catch
            //{
            //    Res = false;
            //    return Res;
            //}
            //return Res;
        }
        private void TrackingROICalculate(out double TrackingX, out double TrackingY, out double RotT)
        {
            TrackingX = 0;
            TrackingY = 0;
            RotT = 0;
            //TrackingX = 0;
            //TrackingY = 0;
            //RotT = 0;
            //double dx, dy, dTeachT, dRotT;
            //dx = ((dCrossX[1] + dCrossX[0]) / 2) - ((RightOrigin[0] + LeftOrigin[0]) / 2);
            //dy = ((dCrossY[1] + dCrossY[0]) / 2) - ((RightOrigin[1] + LeftOrigin[1]) / 2);
            //double[] pntCenter = new double[2] { 0, 0 };
            //double dRotDx = dCrossX[1] - dCrossX[0];
            //double dRotDy = dCrossY[1] - dCrossY[0];
            //dRotT = Math.Atan2(dRotDy, dRotDx);
            //if (dRotT > 180.0) dRotT -= 360.0;

            //dTeachT = Math.Atan2(RightOrigin[1] - LeftOrigin[1], RightOrigin[0] - LeftOrigin[0]);
            //if (dTeachT > 180.0) dTeachT -= 360.0;

            //dRotT -= dTeachT;
            //RotT = dRotT;
            //pntCenter[0] = (dCrossX[1] + dCrossX[0]) / 2;
            //pntCenter[1] = (dCrossY[1] + dCrossY[0]) / 2;
            //double[] dTaget = new double[2];
            //dTaget[0] = (dCrossX[1] + dCrossX[0]) / 2;
            //dTaget[1] = (dCrossY[1] + dCrossY[0]) / 2;
            //TrackingX = dTaget[0];
            //TrackingY = dTaget[1];
        }

        private void RotationTransform(double[] apntCenter, double apntOffsetX, double apntOffsetY, double adAngle, ref double[] apntTarget)
        {

            //double[] pntTempPos = apntTarget;

            //pntTempPos[0] = pntTempPos[0] + apntOffsetX;
            //pntTempPos[1] = pntTempPos[1] + apntOffsetY;
            //apntTarget[0] = apntCenter[0] + ((Math.Cos(adAngle) * (pntTempPos[0] - apntCenter[0]) - (Math.Sin(adAngle) * (pntTempPos[1] - apntCenter[1]))));
            //apntTarget[1] = apntCenter[1] + ((Math.Sin(adAngle) * (pntTempPos[0] - apntCenter[0]) + (Math.Cos(adAngle) * (pntTempPos[1] - apntCenter[1]))));
        }

        private void TrackLineROI(CogFindLineTool cogFindLineTool)
        {
            //cogFindLineTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //cogFindLineTool.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage | CogFindLineCurrentRecordConstants.CaliperRegions | CogFindLineCurrentRecordConstants.ExpectedLineSegment |
            //                                            CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;
            //Display.SetInteractiveGraphics(PT_Display01, cogFindLineTool.CreateCurrentRecord(), false);

            //if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
            //    PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void TrackCircleROI(CogFindCircleTool cogFindCircleTool)
        {
            //CogCircularArc Arc = cogFindCircleTool.RunParams.ExpectedCircularArc;
            //double centerx1 = Arc.CenterX;
            //cogFindCircleTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //cogFindCircleTool.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.InputImage | CogFindCircleCurrentRecordConstants.CaliperRegions | CogFindCircleCurrentRecordConstants.ExpectedCircularArc |
            //                                               CogFindCircleCurrentRecordConstants.InteractiveCaliperSize;
            //Display.SetInteractiveGraphics(PT_Display01, cogFindCircleTool.CreateCurrentRecord(), false);
            //if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
            //    PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void SetText()
        {
            //CogCaliperPolarityConstants Polarity;
            //int TmepIndex = 0;
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    LAB_Insp_Threshold.Text = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //    LAB_Caliper_Cnt.Text = m_TempFindLineTool.RunParams.NumCalipers.ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
            //    lblParamFilterSizeValue.Text = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //    LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
            //    m_TempFindLineTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
            //    Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
            //    TmepIndex = (int)Polarity;
            //    Combo_Polarity1.SelectedIndex = TmepIndex - 1;
            //    Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
            //    TmepIndex = (int)Polarity;
            //    Combo_Polarity2.SelectedIndex = TmepIndex - 1;
            //}
            //else
            //{
            //    LAB_Insp_Threshold.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //    LAB_Caliper_Cnt.Text = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
            //    LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
            //    lblParamFilterSizeValue.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
            //    Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
            //    TmepIndex = (int)Polarity;
            //    Combo_Polarity1.SelectedIndex = TmepIndex - 1;
            //    Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
            //    TmepIndex = (int)Polarity;
            //    Combo_Polarity2.SelectedIndex = TmepIndex - 1;
            //}
            //text_Dist_Ignre.Text = m_dDist_ignore.ToString();
            //text_Spec_Dist.Text = m_SpecDist.ToString();
            //text_Spec_Dist_Max.Text = m_SpecDistMax.ToString();

            //lblEdgeThreshold.Text = m_TeachParameter[m_iGridIndex].iThreshold.ToString();
            //chkUseEdgeThreshold.Checked = m_TeachParameter[m_iGridIndex].bThresholdUse;
            //lblTopCutPixel.Text = m_TeachParameter[m_iGridIndex].iTopCutPixel.ToString();
            //lblBottomCutPixel.Text = m_TeachParameter[m_iGridIndex].iBottomCutPixel.ToString();
            //lblMaskingValue.Text = m_TeachParameter[m_iGridIndex].iMaskingValue.ToString();
            //lblIgnoreSize.Text = m_TeachParameter[m_iGridIndex].iIgnoreSize.ToString();
            //lblEdgeCaliperThreshold.Text = m_TeachParameter[m_iGridIndex].iEdgeCaliperThreshold.ToString();
            //lblEdgeCaliperFilterSize.Text = m_TeachParameter[m_iGridIndex].iEdgeCaliperFilterSize.ToString();
        }
        private void FindLineROI()
        {

            //m_TempFindLineTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //m_TempFindLineTool.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage | CogFindLineCurrentRecordConstants.CaliperRegions | CogFindLineCurrentRecordConstants.ExpectedLineSegment |
            //                                            CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;
            //Display.SetInteractiveGraphics(PT_Display01, m_TempFindLineTool.CreateCurrentRecord(), false);
            //if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
            //    PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }
        private void DrawRoiLine()
        {
            //m_TempTrackingLine.InputImage = (CogImage8Grey)PT_Display01.Image;
            //m_TempTrackingLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage | CogFindLineCurrentRecordConstants.CaliperRegions | CogFindLineCurrentRecordConstants.ExpectedLineSegment |
            //                                           CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;
            //Display.SetInteractiveGraphics(PT_Display01, m_TempTrackingLine.CreateCurrentRecord(), false);
            //if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
            //    PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }
        private void CircleROI()
        {

            //m_TempFindCircleTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //m_TempFindCircleTool.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.InputImage | CogFindCircleCurrentRecordConstants.CaliperRegions | CogFindCircleCurrentRecordConstants.ExpectedCircularArc |
            //                                               CogFindCircleCurrentRecordConstants.InteractiveCaliperSize;
            //Display.SetInteractiveGraphics(PT_Display01, m_TempFindCircleTool.CreateCurrentRecord(), false);
            //if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
            //    PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void TrackingCaliperROI()
        {
            //PT_Display01.StaticGraphics.Clear();
            //PT_Display01.InteractiveGraphics.Clear();
            //m_TempCaliperTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //CogRectangleAffine _cogRectAffine = new CogRectangleAffine();

            //if (m_TempCaliperTool.Region == null)
            //{
            //    _cogRectAffine.GraphicDOFEnable = CogRectangleAffineDOFConstants.Position | CogRectangleAffineDOFConstants.Size | CogRectangleAffineDOFConstants.Skew | CogRectangleAffineDOFConstants.Rotation;
            //    _cogRectAffine.Interactive = true;
            //    _cogRectAffine.SetCenterLengthsRotationSkew((PT_Display01.Image.Width / 2 - PT_Display01.PanX), (PT_Display01.Image.Height / 2 - PT_Display01.PanY), 500, 500, 0, 0);
            //    m_TempCaliperTool.Region = _cogRectAffine;
            //}
            //PT_Display01.InteractiveGraphics.Add(m_TempCaliperTool.Region, "Caliper", false);
        }
        private void Caliper_Count(object sender, EventArgs e)
        {
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //Button btn = (Button)sender;
            //int iCaliperCnt = Convert.ToInt32(LAB_Caliper_Cnt.Text);
            //if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            //{
            //    //Up
            //    iCaliperCnt++;
            //}
            //else
            //{
            //    //Down
            //    if (iCaliperCnt == 1) return;
            //    iCaliperCnt--;
            //}
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    m_TempFindLineTool.RunParams.NumCalipers = iCaliperCnt;
            //    FindLineROI();
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.NumCalipers = iCaliperCnt;
            //    CircleROI();
            //}

            //LAB_Caliper_Cnt.Text = iCaliperCnt.ToString();
        }
        private void Caliper_ProjectionLenth(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //double iProjectionLenth = Convert.ToDouble(LAB_CALIPER_PROJECTIONLENTH.Text);
            //if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            //{
            //    //Up
            //    iProjectionLenth++;
            //}
            //else
            //{
            //    //Down
            //    if (iProjectionLenth == 1) return;
            //    iProjectionLenth--;
            //}
            //if (m_enumROIType == enumROIType.Line)
            //    m_TempFindLineTool.RunParams.CaliperProjectionLength = iProjectionLenth;
            //else
            //    m_TempFindCircleTool.RunParams.CaliperProjectionLength = iProjectionLenth;

            //LAB_CALIPER_PROJECTIONLENTH.Text = iProjectionLenth.ToString();
        }
        private void Insp_Threshold(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //double iThreshold = Convert.ToDouble(LAB_Insp_Threshold.Text);
            //if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            //{
            //    //Up
            //    iThreshold++;
            //}
            //else
            //{
            //    //Down
            //    if (iThreshold < 0) return;
            //    iThreshold--;
            //}

            //if (m_enumROIType == enumROIType.Line)
            //    m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold = iThreshold;
            //else
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold = iThreshold;
            //LAB_Insp_Threshold.Text = iThreshold.ToString();

        }
        private void Insp_SearchLenth(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //double iSearchLenth = Convert.ToDouble(LAB_CALIPER_SEARCHLENTH.Text);
            //if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            //{
            //    //Up
            //    iSearchLenth++;
            //}
            //else
            //{
            //    //Down
            //    if (iSearchLenth < 1) return;
            //    iSearchLenth--;
            //}
            //if (m_enumROIType == enumROIType.Line)
            //    m_TempFindLineTool.RunParams.CaliperSearchLength = iSearchLenth;
            //else
            //    m_TempFindCircleTool.RunParams.CaliperSearchLength = iSearchLenth;
            //LAB_CALIPER_SEARCHLENTH.Text = iSearchLenth.ToString();
        }

        private void Dist_Ignore(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int iIngroe = Convert.ToInt32(text_Dist_Ignre.Text);
            //if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            //{
            //    //Up
            //    iIngroe++;
            //}
            //else
            //{
            //    //Down
            //    if (iIngroe < 0) return;
            //    iIngroe--;
            //}
            //m_dDist_ignore = iIngroe;
            //text_Dist_Ignre.Text = iIngroe.ToString();
        }

        private void BTN_INSP_ADD_Click(object sender, EventArgs e)
        {
            //if (CHK_ROI_CREATE.Checked == false)
            //{
            //    CHK_ROI_CREATE.Checked = true;
            //}
            //if (MessageBox.Show("Are you sure you want to ROI Copy it?", "ROI Copy", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
            //    PT_Display01.InteractiveGraphics.Clear();
            //    PT_Display01.StaticGraphics.Clear();
            //    string[] strData = new string[19];
            //    int iNo = DataGridview_Insp.RowCount;
            //    if (iNo == 0)
            //        iNo = 0;
            //    else
            //        iNo -= 1;

            //    CogCaliperPolarityConstants Polarity;
            //    if (m_TeachParameter.Count < iNo)
            //        m_TeachParameter.Add(ResetStruct());
            //    var TempData = m_TeachParameter[iNo];
            //    TempData.m_enumROIType = (Main.PatternTag.SDParameter.enumROIType)m_enumROIType;
            //    strData[0] = string.Format("{0:00}", (iNo + 1).ToString());

            //    if (m_enumROIType == enumROIType.Line)
            //    {
            //        strData[1] = "Line";
            //        strData[2] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX);
            //        strData[3] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY);
            //        strData[4] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX);
            //        strData[5] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY);
            //        strData[6] = string.Format("{0:F3}", 0);
            //        strData[7] = string.Format("{0:F3}", 0);
            //        strData[8] = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //        strData[9] = m_TempFindLineTool.RunParams.NumCalipers.ToString();
            //        strData[10] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
            //        strData[11] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
            //        Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
            //        strData[12] = ((int)Polarity).ToString();
            //        Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
            //        strData[13] = ((int)Polarity).ToString();
            //        strData[14] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //        strData[15] = m_dDist_ignore.ToString();
            //        strData[16] = string.Format("{0:F2}", m_SpecDist);
            //        //strData[17] = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels);
            //        strData[17] = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //        strData[18] = string.Format("{0:F2}", m_SpecDistMax);

            //        TempData.m_FindLineTool = m_TempFindLineTool;
            //    }
            //    else
            //    {

            //        strData[1] = "Circle";
            //        strData[2] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX);
            //        strData[3] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterY);
            //        strData[4] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.Radius);
            //        strData[5] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleStart);
            //        strData[6] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan);
            //        strData[7] = string.Format("{0:F3}", 0);
            //        strData[8] = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //        strData[9] = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
            //        strData[10] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
            //        strData[11] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
            //        Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
            //        strData[12] = ((int)Polarity).ToString();
            //        Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
            //        strData[13] = ((int)Polarity).ToString();
            //        TempData.m_FindCircleTool = m_TempFindCircleTool;
            //        strData[14] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //        strData[15] = m_dDist_ignore.ToString();
            //        strData[16] = string.Format("{0:F2}", m_SpecDist);
            //        //strData[17] = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels);
            //        strData[17] = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //        strData[18] = string.Format("{0:F2}", m_SpecDistMax);
            //    }
            //    DataGridview_Insp.Rows.Add(strData);
            //    m_TeachParameter.Add(TempData);
            //    CHK_ROI_CREATE.Checked = false;
            //}
            //else
            //{
            //    CHK_ROI_CREATE.Checked = false;
            //    return;
            //}
        }

        private void BTN_INSP_DELETE_Click(object sender, EventArgs e)
        {
            //if (m_iGridIndex < 0) return;
            //if (MessageBox.Show("Are you sure you want to delete it?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
            //    DataGridview_Insp.Rows.RemoveAt(m_iGridIndex);
            //    m_TeachParameter.RemoveAt(m_iGridIndex);
            //}
            //else
            //{
            //    return;
            //}
        }
        private void initTracking()
        {
            //m_enumAlignROI = enumAlignROI.Left1_1;
            //btn_TOP_Inscription.BackColor = Color.Green;
            //btn_Top_Circumcription.BackColor = Color.DarkGray;
            //btn_Bottom_Inscription.BackColor = Color.DarkGray;
            //btn_Bottom_Circumcription.BackColor = Color.DarkGray;
            //for (int i = 0; i < 4; i++)
            //{
            //    if (i < 2)
            //    {
            //        LeftOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].LeftOrigin[i];
            //        RightOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].RightOrigin[i];
            //    }
            //    m_TeachLine[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i];
            //    if (m_TeachLine[i] == null)
            //        m_TeachLine[i] = new CogFindLineTool();
            //    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] == null)
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] = new CogFindLineTool();
            //}
            //lab_LeftOriginX.Text = LeftOrigin[0].ToString("F3");
            //lab_LeftOriginY.Text = LeftOrigin[1].ToString("F3");
            //lab_RightOriginX.Text = RightOrigin[0].ToString("F3");
            //lab_RightOriginY.Text = RightOrigin[1].ToString("F3");
            //m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_1];
            //Get_FindConerParameter();
        }
        public void Init_ListBox()
        {
            //init_ComboPolarity();
            //m_TeachParameter = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_InspParameter;

            //CogCaliperPolarityConstants Polarity;
            //DataGridview_Insp.Rows.Clear();

            //DataGridview_Insp.ClearSelection();
            //DataGridview_Insp.CurrentCell = null;
            //initTracking();
            //if (m_TeachParameter.Count() <= 0)
            //{
            //    m_TeachParameter = new List<Main.PatternTag.SDParameter>();
            //    m_TeachParameter.Add(ResetStruct());
            //}

            //for (int i = 0; i < m_TeachParameter.Count; i++)
            //{
            //    string[] strData = new string[21];
            //    var Tempdata = m_TeachParameter[i];
            //    bool bThre = Tempdata.bThresholdUse ? true : false;


            //    strData[0] = i.ToString();
            //    if (i == 0)
            //    {
            //        m_iHistoramROICnt = Tempdata.iHistogramROICnt;
            //        for (int iHitoCnt = 0; iHitoCnt < m_iHistoramROICnt; iHitoCnt++)
            //        {
            //            m_bTrakingRootHisto[iHitoCnt] = false;
            //        }

            //        lab_Histogram_ROI_Count.Text = m_iHistoramROICnt.ToString();
            //    }
            //    if (enumROIType.Line == (enumROIType)Tempdata.m_enumROIType)
            //    {
            //        strData[1] = "Line";
            //        strData[2] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartX);
            //        strData[3] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartY);
            //        strData[4] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndX);
            //        strData[5] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndY);
            //        strData[6] = string.Format("{0:F3}", 0);
            //        strData[7] = string.Format("{0:F3}", 0);
            //        strData[8] = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //        strData[9] = Tempdata.m_FindLineTool.RunParams.NumCalipers.ToString();
            //        strData[10] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.CaliperProjectionLength);
            //        strData[11] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.CaliperSearchLength);
            //        Polarity = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
            //        strData[12] = ((int)Polarity).ToString();
            //        Polarity = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
            //        strData[13] = ((int)Polarity).ToString();
            //        strData[14] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //        strData[15] = Tempdata.IDistgnore.ToString();
            //        strData[16] = string.Format("{0:F2}", Tempdata.dSpecDistance);
            //        strData[17] = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //        strData[18] = string.Format("{0:F2}", Tempdata.dSpecDistanceMax);
            //        strData[19] = bThre.ToString();
            //        strData[20] = Tempdata.iThreshold.ToString();

            //    }
            //    else
            //    {
            //        strData[1] = "Circle";
            //        strData[2] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterX);
            //        strData[3] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterY);
            //        strData[4] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.Radius);
            //        strData[5] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.AngleStart);
            //        strData[6] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.AngleSpan);
            //        strData[7] = string.Format("{0:F3}", 0);
            //        strData[8] = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //        strData[9] = Tempdata.m_FindCircleTool.RunParams.NumCalipers.ToString();
            //        strData[10] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.CaliperProjectionLength);
            //        strData[11] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.CaliperSearchLength);
            //        Polarity = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
            //        strData[12] = ((int)Polarity).ToString();
            //        Polarity = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
            //        strData[13] = ((int)Polarity).ToString();
            //        strData[14] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //        strData[15] = Tempdata.IDistgnore.ToString();
            //        strData[16] = string.Format("{0:F2}", Tempdata.dSpecDistance);
            //        strData[17] = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //        strData[18] = string.Format("{0:F2}", Tempdata.dSpecDistanceMax);
            //        strData[19] = bThre.ToString();
            //        strData[20] = Tempdata.iThreshold.ToString();
            //    }
            //    DataGridview_Insp.Rows.Add(strData);

            //}
            //SetText();
        }
        private void UpDataTool()
        {
            //double dEdgeWidth;
            ////shkang_s
            //string strTemp;
            //int itype;
            //strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
            //if (strTemp == "Line")
            //    itype = 0;
            //else
            //    itype = 1;
            //m_enumROIType = (enumROIType)itype;
            ////shkang_e
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    ////강성현 주석/////////////////// m_FL 관련 주석해  
            //    CogFindLineTool m_FL = new CogFindLineTool();
            //    m_FL.RunParams.ExpectedLineSegment.StartX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
            //    m_FL.RunParams.ExpectedLineSegment.StartY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
            //    m_FL.RunParams.ExpectedLineSegment.EndX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
            //    m_FL.RunParams.ExpectedLineSegment.EndY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
            //    m_FL.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
            //    m_FL.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
            //    m_FL.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
            //    m_FL.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
            //    m_FL.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
            //    m_FL.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
            //    m_FL.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
            //    m_FL.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
            //    /////////////////////////////////////////////////////////////
            //    m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
            //    m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
            //    m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
            //    dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
            //    ////강성현 주석///////////////////
            //    //m_FL.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

            //    LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
            //    LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
            //    LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();
            //    lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

            //    LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));
            //    m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
            //    m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

            //    m_TempFindLineTool = new CogFindLineTool();
            //    if (bROICopy)
            //        m_TempFindLineTool = m_FL;
            //    else
            //        m_TempFindLineTool = m_TeachParameter[m_iGridIndex].m_FindLineTool;
            //}
            //else
            //{
            //    ////강성현 주석///////////////////
            //    CogFindCircleTool m_FC = new CogFindCircleTool();
            //    m_FC.RunParams.ExpectedCircularArc.CenterX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
            //    m_FC.RunParams.ExpectedCircularArc.CenterY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
            //    m_FC.RunParams.ExpectedCircularArc.Radius = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
            //    m_FC.RunParams.ExpectedCircularArc.AngleStart = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
            //    m_FC.RunParams.ExpectedCircularArc.AngleSpan = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value);
            //    m_FC.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
            //    m_FC.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
            //    m_FC.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
            //    m_FC.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
            //    m_FC.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
            //    m_FC.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
            //    m_FC.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
            //    m_FC.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
            //    ///////////////////////////////////////////////
            //    m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
            //    m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
            //    m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
            //    ///강성현 주석/////////////////////////////////////////
            //    m_FC.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);
            //    //////////////////////////////
            //    LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
            //    LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
            //    LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();

            //    dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
            //    lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);
            //    LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));
            //    m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
            //    m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

            //    m_TempFindCircleTool = new CogFindCircleTool();
            //    if (bROICopy)
            //        m_TempFindCircleTool = m_FC;
            //    else
            //        m_TempFindCircleTool = m_TeachParameter[m_iGridIndex].m_FindCircleTool;
            //}
        }
        private void DataGridview_Insp_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex < 0)
            //    return;

            //m_iGridIndex = e.RowIndex;
            //int itype;
            //string strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
            //if (strTemp == "Line")
            //    itype = 0;
            //else
            //    itype = 1;

            //m_enumROIType = (enumROIType)itype;
            //double dEdgeWidth;

            //if (m_enumROIType == enumROIType.Line)
            //{
            //    CogFindLineTool m_FL = new CogFindLineTool();
            //    m_FL.RunParams.ExpectedLineSegment.StartX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
            //    m_FL.RunParams.ExpectedLineSegment.StartY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
            //    m_FL.RunParams.ExpectedLineSegment.EndX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
            //    m_FL.RunParams.ExpectedLineSegment.EndY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
            //    m_FL.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
            //    m_FL.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
            //    m_FL.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
            //    m_FL.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
            //    m_FL.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
            //    m_FL.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
            //    m_FL.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
            //    m_FL.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
            //    m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
            //    m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
            //    m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
            //    dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
            //    //m_FL.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

            //    LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
            //    LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
            //    LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();
            //    lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);
            //    LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));

            //    m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
            //    m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

            //    m_TempFindLineTool = new CogFindLineTool();

            //    if (bROICopy)
            //        m_TempFindLineTool = m_FL;
            //    else
            //        m_TempFindLineTool = m_TeachParameter[m_iGridIndex].m_FindLineTool;

            //    // Line은 EdgeWidth, Polarity2 미사용
            //    label59.Visible = false;
            //    LAB_EDGE_WIDTH.Visible = false;
            //    lblParamEdgeWidthValueUp.Visible = false;
            //    lblParamEdgeWidthValueDown.Visible = false;
            //    label58.Visible = false;
            //    Combo_Polarity2.Visible = false;
            //}
            //else
            //{
            //    CogFindCircleTool m_FC = new CogFindCircleTool();
            //    m_FC.RunParams.ExpectedCircularArc.CenterX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
            //    m_FC.RunParams.ExpectedCircularArc.CenterY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
            //    m_FC.RunParams.ExpectedCircularArc.Radius = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
            //    m_FC.RunParams.ExpectedCircularArc.AngleStart = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
            //    m_FC.RunParams.ExpectedCircularArc.AngleSpan = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value);
            //    m_FC.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
            //    m_FC.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
            //    m_FC.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
            //    m_FC.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
            //    m_FC.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
            //    m_FC.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
            //    m_FC.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
            //    m_FC.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
            //    m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
            //    m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
            //    m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
            //    m_FC.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

            //    LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
            //    LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
            //    LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();
            //    lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

            //    dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
            //    LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));

            //    m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
            //    m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

            //    m_TempFindCircleTool = new CogFindCircleTool();

            //    if (bROICopy)
            //        m_TempFindCircleTool = m_FC;
            //    else
            //        m_TempFindCircleTool = m_TeachParameter[m_iGridIndex].m_FindCircleTool;

            //    // Circle은 EdgeWidth, Polarity2 미사용
            //    label59.Visible = true;
            //    LAB_EDGE_WIDTH.Visible = true;
            //    lblParamEdgeWidthValueUp.Visible = true;
            //    lblParamEdgeWidthValueDown.Visible = true;
            //    label58.Visible = true;
            //    Combo_Polarity2.Visible = true;
            //}
            //SetText();
            //UpdateParamUI();

            //btn_ROI_SHOW.PerformClick();
            ////Set_InspParams();
        }


        private void Set_InspParams()
        {
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    m_TempFindLineTool = new CogFindLineTool();
            //    if (m_TempFindLineTool == null) return;
            //    LAB_Insp_Threshold.Text = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //    LAB_Caliper_Cnt.Text = m_TempFindLineTool.RunParams.NumCalipers.ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
            //    LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
            //    LAB_EDGE_WIDTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position * 2);
            //    lblParamFilterSizeValue.Text = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //}
            //else
            //{
            //    m_TempFindCircleTool = new CogFindCircleTool();
            //    if (m_TempFindCircleTool == null) return;
            //    LAB_Insp_Threshold.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //    LAB_Caliper_Cnt.Text = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
            //    LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
            //    LAB_EDGE_WIDTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position * 2);
            //    lblParamFilterSizeValue.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //}
        }

        private void Comb_Section_SelectedIndexChanged(object sender, EventArgs e)
        {
            //_PrePointX = null;
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //var TempBlob = m_TeachParameter[0];
            //m_CogBlobTool[m_BlobROI] = TempBlob.m_CogBlobTool[m_BlobROI];
            //if (_useROITracking)
            //{
            //    if (m_CogBlobTool[m_BlobROI].Region != null)
            //    {
            //        CogPolygon PolygonROI = (CogPolygon)m_CogBlobTool[m_BlobROI].Region;
            //        if (dBlobPrevTranslationX > 0 && (m_PrevROINo == m_BlobROI))
            //        {
            //            m_bTrakingRoot[m_BlobROI] = false;
            //            int numVertice = PolygonROI.NumVertices;
            //            for (int i = 0; i < numVertice; i++)
            //            {
            //                double dx2 = PolygonROI.GetVertexX(i);
            //                double dy2 = PolygonROI.GetVertexY(i);
            //                dx2 += (dBlobPrevTranslationX);
            //                dy2 += (dBlobPrevTranslationY);
            //                PolygonROI.SetVertex(i, dx2, dy2);
            //            }
            //            m_CogBlobTool[m_BlobROI].Region = PolygonROI;
            //        }
            //        else if (m_bTrakingRoot[m_BlobROI] == true && m_bTrakingRoot[m_BlobROI] == true)
            //        {
            //            int numVertice = PolygonROI.NumVertices;
            //            for (int i = 0; i < numVertice; i++)
            //            {
            //                double dx2 = PolygonROI.GetVertexX(i);
            //                double dy2 = PolygonROI.GetVertexY(i);
            //                dx2 += (dBlobPrevTranslationX);
            //                dy2 += (dBlobPrevTranslationY);
            //                PolygonROI.SetVertex(i, dx2, dy2);
            //            }
            //            m_CogBlobTool[m_BlobROI].Region = PolygonROI;
            //            m_bTrakingRoot[m_BlobROI] = false;
            //        }
            //        m_PrevROINo = m_BlobROI;

            //        double dx = PolygonROI.GetVertexX(0);
            //        m_CogBlobTool[m_BlobROI].RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
            //        if (m_CogBlobTool[m_BlobROI] == null)
            //        {
            //            m_CogBlobTool[m_BlobROI] = new CogBlobTool();
            //            m_CogBlobTool[m_BlobROI].RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
            //            m_CogBlobTool[m_BlobROI].RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
            //        }
            //    }
            //    else
            //    {
            //        _useROITracking = false;
            //        chkUseRoiTracking.Checked = false;
            //    }
            //}
            //Get_BlobParameter();
        }

        private void Display_MauseUP(object sender, EventArgs e)
        {
            //if (PT_Display01.InteractiveGraphics == null) return;
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    if (m_TempFindLineTool == null) return;
            //    LAB_Insp_Threshold.Text = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //    LAB_Caliper_Cnt.Text = m_TempFindLineTool.RunParams.NumCalipers.ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
            //    LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
            //    lblParamFilterSizeValue.Text = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //}
            //else
            //{
            //    if (m_TempFindCircleTool == null) return;
            //    LAB_Insp_Threshold.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //    LAB_Caliper_Cnt.Text = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
            //    LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
            //    LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
            //    lblParamFilterSizeValue.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            //}

        }

        private void btn_Param_Apply_Click(object sender, EventArgs e)
        {

            //if (m_iGridIndex < 0) return;
            //string strTemp = "";
            //iCountClick += 1;
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //m_iCount = DataGridview_Insp.Rows.Count;
            //if (Chk_All_Select.Checked == false)
            //{
            //    var TempData = m_TeachParameter[m_iGridIndex];
            //    double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
            //    TempData.m_enumROIType = (Main.PatternTag.SDParameter.enumROIType)m_enumROIType;
            //    if (m_enumROIType == enumROIType.Line)
            //    {
            //        strTemp = "Line";

            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value = strTemp;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value = 0;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[7].Value = 0;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value = m_TempFindLineTool.RunParams.NumCalipers;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity);
            //        if (m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position == 0)
            //        {

            //            m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //            m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //        }
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value = m_dDist_ignore.ToString();
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);

            //        TempData.IDistgnore = m_dDist_ignore;
            //        TempData.dSpecDistance = m_SpecDist;
            //        TempData.dSpecDistanceMax = m_SpecDistMax;
            //        TempData.m_FindLineTool = new CogFindLineTool();
            //        TempData.m_FindLineTool = m_TempFindLineTool;
            //        //}
            //    }
            //    else
            //    {
            //        strTemp = "Circle";

            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value = strTemp;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterY;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.Radius;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleStart;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[7].Value = 0;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value = m_TempFindCircleTool.RunParams.NumCalipers;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity);
            //        if (m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position == 0)
            //        {
            //            m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //            m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //        }
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value = string.Format("{0:F2}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value = m_dDist_ignore.ToString();
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            //        DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);
            //        TempData.IDistgnore = m_dDist_ignore;
            //        TempData.dSpecDistance = m_SpecDist;
            //        TempData.dSpecDistanceMax = m_SpecDistMax;
            //        TempData.m_FindCircleTool = m_TempFindCircleTool;
            //        //}
            //    }

            //    m_TeachParameter[m_iGridIndex].bThresholdUse = chkUseEdgeThreshold.Checked;
            //    m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(lblEdgeThreshold.Text);
            //    m_TeachParameter[m_iGridIndex].iEdgeCaliperThreshold = Convert.ToInt16(lblEdgeCaliperThreshold.Text);
            //    m_TeachParameter[m_iGridIndex].iEdgeCaliperFilterSize = Convert.ToInt16(lblEdgeCaliperFilterSize.Text);
            //    m_TeachParameter[m_iGridIndex].iTopCutPixel = Convert.ToInt16(lblTopCutPixel.Text);
            //    m_TeachParameter[m_iGridIndex].iBottomCutPixel = Convert.ToInt16(lblBottomCutPixel.Text);
            //    m_TeachParameter[m_iGridIndex].iMaskingValue = Convert.ToInt16(lblMaskingValue.Text);
            //    m_TeachParameter[m_iGridIndex].iIgnoreSize = Convert.ToInt16(lblIgnoreSize.Text);


            //    DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value = m_TeachParameter[m_iGridIndex].bThresholdUse;
            //    DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value = m_TeachParameter[m_iGridIndex].iThreshold;

            //    m_TeachParameter[m_iGridIndex] = TempData;
            //    dInspPrevTranslationX = 0;
            //    dInspPrevTranslationY = 0;
            //}
            //else
            //{
            //    chkUseRoiTracking.Checked = false;
            //    Thread.Sleep(100);
            //    for (int i = 0; i < m_TeachParameter.Count; i++)
            //    {
            //        var TempData = m_TeachParameter[i];
            //        double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
            //        double dThreshold = Convert.ToDouble(LAB_Insp_Threshold.Text);
            //        if ((enumROIType)TempData.m_enumROIType == enumROIType.Line)
            //        {

            //            m_TempFindLineTool = TempData.m_FindLineTool;
            //            strTemp = "Line";
            //            if (_useROITracking)
            //            {

            //            }
            //            else
            //            {
            //                DataGridview_Insp.Rows[i].Cells[1].Value = strTemp;
            //                DataGridview_Insp.Rows[i].Cells[2].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX;
            //                DataGridview_Insp.Rows[i].Cells[3].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY;
            //                DataGridview_Insp.Rows[i].Cells[4].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX;
            //                DataGridview_Insp.Rows[i].Cells[5].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY;
            //                DataGridview_Insp.Rows[i].Cells[6].Value = 0;
            //                DataGridview_Insp.Rows[i].Cells[7].Value = 0;
            //                DataGridview_Insp.Rows[i].Cells[8].Value = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold;
            //                DataGridview_Insp.Rows[i].Cells[9].Value = m_TempFindLineTool.RunParams.NumCalipers;
            //                DataGridview_Insp.Rows[i].Cells[10].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
            //                DataGridview_Insp.Rows[i].Cells[11].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
            //                DataGridview_Insp.Rows[i].Cells[12].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity);
            //                DataGridview_Insp.Rows[i].Cells[13].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity);

            //                m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //                m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //                //}
            //                DataGridview_Insp.Rows[i].Cells[14].Value = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //                //DataGridview_Insp.Rows[i].Cells[15].Value = m_dDist_ignore.ToString();
            //                DataGridview_Insp.Rows[i].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
            //                DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            //                DataGridview_Insp.Rows[i].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);
            //                // TempData.IDistgnore = m_dDist_ignore;
            //                TempData.dSpecDistance = m_SpecDist;
            //                TempData.dSpecDistanceMax = m_SpecDistMax;
            //                TempData.m_FindLineTool = new CogFindLineTool();
            //                TempData.m_FindLineTool = m_TempFindLineTool;
            //            }
            //        }
            //        else
            //        {
            //            m_TempFindCircleTool = TempData.m_FindCircleTool;
            //            strTemp = "Circle";

            //            if (_useROITracking)
            //            {

            //            }
            //            else
            //            {
            //                DataGridview_Insp.Rows[i].Cells[1].Value = strTemp;
            //                DataGridview_Insp.Rows[i].Cells[2].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX;
            //                DataGridview_Insp.Rows[i].Cells[3].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterY;
            //                DataGridview_Insp.Rows[i].Cells[4].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.Radius;
            //                DataGridview_Insp.Rows[i].Cells[5].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleStart;
            //                DataGridview_Insp.Rows[i].Cells[6].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan;
            //                DataGridview_Insp.Rows[i].Cells[7].Value = 0;
            //                DataGridview_Insp.Rows[i].Cells[8].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold;
            //                DataGridview_Insp.Rows[i].Cells[9].Value = m_TempFindCircleTool.RunParams.NumCalipers;
            //                DataGridview_Insp.Rows[i].Cells[10].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
            //                DataGridview_Insp.Rows[i].Cells[11].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
            //                DataGridview_Insp.Rows[i].Cells[12].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity);
            //                DataGridview_Insp.Rows[i].Cells[13].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity);

            //                m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //                m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //                DataGridview_Insp.Rows[i].Cells[14].Value = string.Format("{0:F2}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
            //                DataGridview_Insp.Rows[i].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
            //                DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
            //                DataGridview_Insp.Rows[i].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);
            //                TempData.dSpecDistance = m_SpecDist;
            //                TempData.dSpecDistanceMax = m_SpecDistMax;
            //                TempData.m_FindCircleTool = m_TempFindCircleTool;
            //            }
            //        }
            //        m_TeachParameter[i] = TempData;
            //    }
            //}
            ////shkang_s
            //if (iCountClick == 1)
            //{
            //    tempCaliperNum.Add(m_iGridIndex);
            //}
            //else
            //{
            //    if (tempCaliperNum[iCountClick - 2] == m_iGridIndex)
            //    {
            //        iCountClick = iCountClick - 1;
            //    }
            //    else
            //    {
            //        tempCaliperNum.Add(m_iGridIndex);
            //    }
            //}
            ////shkang_e
        }


        private void Comb_Section_Click(object sender, EventArgs e)
        {
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
        }

        private void btn_TrimOrigin_Click(object sender, EventArgs e)
        {
            //PT_Display01.Image = OriginImage;
        }

        private void LAB_Insp_Threshold_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_Insp_Threshold.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //double dThreshold = KeyPad.m_data;
            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold = dThreshold;
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold = dThreshold;
            //}
            //LAB_Insp_Threshold.Text = ((int)dThreshold).ToString();
        }

        private void LAB_Caliper_Cnt_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_Caliper_Cnt.Text);
            //KeyPadForm KeyPad = new KeyPadForm(2, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //int CaliperCnt = (int)KeyPad.m_data;
            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.NumCalipers = CaliperCnt;
            //    FindLineROI();
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.NumCalipers = CaliperCnt;
            //    CircleROI();
            //}
            //LAB_Caliper_Cnt.Text = CaliperCnt.ToString();
        }

        private void LAB_CALIPER_PROJECTIONLENTH_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_CALIPER_PROJECTIONLENTH.Text);
            //KeyPadForm KeyPad = new KeyPadForm(1, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //double CaliperProjectionLenth = KeyPad.m_data;
            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.CaliperSearchLength = CaliperProjectionLenth;
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.CaliperSearchLength = CaliperProjectionLenth;
            //}
            //LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", CaliperProjectionLenth);
        }
        private void LAB_EDGE_WIDTH_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
            //KeyPadForm KeyPad = new KeyPadForm(1, 100, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //double dEdgeWidth = KeyPad.m_data;

            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //    m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //}

            //LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", dEdgeWidth);
        }

        private bool GaloOppositeInspection(int nROI, int toolType, object tool, CogImage8Grey cogImage, out double[] ResultData, ref CogGraphicInteractiveCollection GraphicData, out int NonCaliperCnt)
        {
            ResultData = new double[2];
            NonCaliperCnt = 1;
            return true;
            //NonCaliperCnt = 0;
            //if (toolType == (int)enumROIType.Line)
            //{
            //    bool MoveTypeY = false;
            //    //2023 0130 YSH
            //    bool bRes = true;
            //    CogFindLineTool m_LineTool = tool as CogFindLineTool;

            //    int[] nCaliperCount = new int[2];
            //    CogFindLineTool[] SingleFindLine = new CogFindLineTool[2];
            //    PointF[,] RawSearchData = new PointF[2, 100];
            //    ResultData = new double[100];

            //    CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();
            //    double startPosX = m_LineTool.RunParams.ExpectedLineSegment.StartX;
            //    double startPosY = m_LineTool.RunParams.ExpectedLineSegment.StartY;
            //    double EndPosX = m_LineTool.RunParams.ExpectedLineSegment.EndX;
            //    double EndPosY = m_LineTool.RunParams.ExpectedLineSegment.EndY;
            //    double MovePos1, MovePos2;
            //    double Move = m_LineTool.RunParams.CaliperSearchLength / 2;
            //    double diretion = m_LineTool.RunParams.CaliperSearchDirection;
            //    double HalfSearchLength = m_LineTool.RunParams.CaliperSearchLength / 2;
            //    double TempSearchLength = m_LineTool.RunParams.CaliperSearchLength;
            //    double searchDirection = m_LineTool.RunParams.CaliperSearchDirection;
            //    CogCaliperPolarityConstants edgePolarity = m_LineTool.RunParams.CaliperRunParams.Edge0Polarity;

            //    double Cal_StartX = 0;
            //    double Cal_StartY = 0;
            //    double Cal_EndX = 0;
            //    double Cal_EndY = 0;

            //    double noneEdge_Threshold = 0;
            //    int noeEdge_FilterSize = 0;

            //    try
            //    {
            //        if (!m_bROIFinealignFlag)
            //        {
            //            if (Math.Abs(EndPosY - startPosY) < 100)
            //            {
            //                MoveTypeY = true;
            //                if (startPosX > EndPosX)
            //                {
            //                    diretion *= -1;
            //                }
            //            }
            //            else
            //            {
            //                MoveTypeY = false;
            //                if (startPosY > EndPosY)
            //                {
            //                    diretion *= -1;
            //                }
            //            }
            //        }

            //        #region FindLine Search
            //        CogFixtureTool mCogFixtureTool2 = new CogFixtureTool();

            //        bool isTwiceFixture = false;
            //        double dist = 0;

            //        for (int i = 0; i < 2; i++)
            //        {
            //            SingleFindLine[i] = new CogFindLineTool();
            //            SingleFindLine[i] = m_LineTool;
            //            noneEdge_Threshold = SingleFindLine[i].RunParams.CaliperRunParams.ContrastThreshold;
            //            noeEdge_FilterSize = SingleFindLine[i].RunParams.CaliperRunParams.FilterHalfSizeInPixels;

            //            if (i == 1)
            //            {
            //                //하나의 FindLineTool방향만 정반대로 돌려서 Search진행
            //                dist = SingleFindLine[i].RunParams.CaliperSearchDirection;
            //                SingleFindLine[i].RunParams.CaliperSearchDirection *= (-1);
            //                SingleFindLine[i].RunParams.CaliperRunParams.Edge0Polarity = SingleFindLine[i].RunParams.CaliperRunParams.Edge1Polarity;

            //                if (m_bROIFinealignFlag)
            //                {
            //                    double Calrotation = m_dTempFineLineAngle - SingleFindLine[i].RunParams.ExpectedLineSegment.Rotation;

            //                    Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.StartX, SingleFindLine[i].RunParams.ExpectedLineSegment.StartY,
            //                            SingleFindLine[i].RunParams.CaliperSearchLength / 2, Calrotation, out Cal_StartX, out Cal_StartY);

            //                    Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.EndX, SingleFindLine[i].RunParams.ExpectedLineSegment.EndY,
            //                            SingleFindLine[i].RunParams.CaliperSearchLength / 2, Calrotation, out Cal_EndX, out Cal_EndY);

            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = Cal_StartX;
            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = Cal_EndX;
            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = Cal_StartY;
            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = Cal_EndY;
            //                }
            //                else
            //                {
            //                    if (!MoveTypeY)
            //                    {
            //                        if (diretion < 0)
            //                        {
            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX + Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX + Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
            //                        }
            //                        else
            //                        {
            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX - Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX - Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (diretion < 0)
            //                        {

            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY - Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY - Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
            //                        }
            //                        else
            //                        {
            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY + Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY + Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
            //                        }
            //                    }
            //                }
            //            }

            //            SingleFindLine[i].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
            //            SingleFindLine[i].LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;

            //            if (m_TeachParameter[nROI].bThresholdUse == true && i == 1)
            //            {
            //                // Crop 처리
            //                var transform = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempFixtureTrans;
            //                var cropResult = GetCropImage(cogImage, SingleFindLine[i], transform, out CogRectangle cropRect);

            //                EdgeAlgorithm edgeAlgorithm = new EdgeAlgorithm();
            //                edgeAlgorithm.Threshold = m_TeachParameter[nROI].iThreshold;

            //                var image = cropResult.Item1 as CogImage8Grey;
            //                image.CoordinateSpaceTree = new CogCoordinateSpaceTree();
            //                image.SelectedSpaceName = "@";

            //                edgeAlgorithm.IgnoreSize = m_TeachParameter[nROI].iIgnoreSize;
            //                Mat convertImage = edgeAlgorithm.Inspect(image, ref SingleFindLine[i], cropResult.Item2, transform, cropRect);

            //                if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
            //                    convertImage.Save(@"D:\convertImage.bmp");

            //                double lengthX = Math.Abs(SingleFindLine[i].RunParams.ExpectedLineSegment.StartX - SingleFindLine[i].RunParams.ExpectedLineSegment.EndX);
            //                double lengthY = Math.Abs(SingleFindLine[i].RunParams.ExpectedLineSegment.StartY - SingleFindLine[i].RunParams.ExpectedLineSegment.EndY);

            //                int searchedValue = -1;
            //                List<Point> boundRectPointList = new List<Point>();

            //                if (lengthX > lengthY) // 가로
            //                {
            //                    double startX = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX;
            //                    double startY = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY;
            //                    double endX = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX;
            //                    double endY = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY;
            //                    transform.MapPoint(startX, startY, out double orgStartX, out double orgStartY);
            //                    transform.MapPoint(endX, endY, out double orgEndX, out double orgEndY);

            //                    transform.MapPoint(cropRect.X, cropRect.Y, out double mappingStartX, out double mappingStartY);

            //                    if (orgStartX > orgEndX) // 화살표 방향 아래에서 위
            //                    {
            //                        var minPosY = edgeAlgorithm.GetVerticalMinEdgeTopPosY(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
            //                        if (minPosY.Count > 0)
            //                        {
            //                            searchedValue = minPosY.Min();
            //                            int maskX = (int)mappingStartX;
            //                            int maskY = searchedValue + (int)mappingStartY;

            //                            Rectangle rect = new Rectangle((int)mappingStartX, 0, convertImage.Width, maskY);

            //                            int maskWidth = convertImage.Width;
            //                            int maskHeight = rect.Height;

            //                            boundRectPointList.Add(new Point(maskX, maskY));
            //                            boundRectPointList.Add(new Point(maskX, maskY - convertImage.Height));
            //                            boundRectPointList.Add(new Point(maskX + maskWidth, maskY - convertImage.Height));
            //                            boundRectPointList.Add(new Point(maskX + maskWidth, maskY));
            //                        }
            //                    }
            //                    else// 화살표 방향 위에서 아래
            //                    {
            //                        var edgePointList = edgeAlgorithm.GetVerticalEdgeBottomPos(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
            //                        if (edgePointList.Count > 0)
            //                        {
            //                            var target = edgePointList.OrderByDescending(edgePoint => edgePoint.PointY);
            //                            var minEdge = target.Last();
            //                            var maxEdge = target.First();

            //                            int leftTopY = (int)mappingStartY;
            //                            int rightTopY = (int)mappingStartY;

            //                            int leftTopTempY = minEdge.PointY > maxEdge.PointY ? maxEdge.PointY : minEdge.PointY;
            //                            int rightTopTempY = minEdge.PointY > maxEdge.PointY ? minEdge.PointY : maxEdge.PointY;

            //                            leftTopY += leftTopTempY;
            //                            rightTopY += rightTopTempY;

            //                            searchedValue = 1;

            //                            int maskX = (int)mappingStartX;
            //                            int maskY = (int)mappingStartY; // Y 좌표 설정

            //                            boundRectPointList.Add(new Point(maskX, leftTopY));
            //                            boundRectPointList.Add(new Point(maskX + convertImage.Width, rightTopY));
            //                            boundRectPointList.Add(new Point(maskX + convertImage.Width, rightTopY + convertImage.Height));
            //                            boundRectPointList.Add(new Point(maskX, leftTopY + convertImage.Height));
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    double startX = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX;
            //                    double startY = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY;
            //                    double endX = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX;
            //                    double endY = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY;
            //                    transform.MapPoint(startX, startY, out double orgStartX, out double orgStartY);
            //                    transform.MapPoint(endX, endY, out double orgEndX, out double orgEndY);

            //                    transform.MapPoint(cropRect.X, cropRect.Y, out double mappingStartX, out double mappingStartY);
            //                    if (orgStartX > orgEndX) // 화살표 방향 오른쪽에서 왼쪽
            //                    {
            //                        searchedValue = edgeAlgorithm.GetHorizontalMinEdgePosY(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
            //                        if (searchedValue >= 0)
            //                        {
            //                            // 마스크를 그릴 영역의 X, Y 좌표 계산
            //                            int maskX = searchedValue + (int)mappingStartX; // X 좌표 설정
            //                            int maskY = (int)mappingStartY; // Y 좌표 설정

            //                            Rectangle rect = new Rectangle((int)mappingStartX, 0, convertImage.Width, maskY);

            //                            // 마스크를 그릴 영역의 너비와 높이 계산
            //                            int maskWidth = convertImage.Width; // 너비 설정
            //                            int maskHeight = convertImage.Height; // 높이 설정

            //                            boundRectPointList.Add(new Point(maskX, maskY));
            //                            boundRectPointList.Add(new Point(maskX - convertImage.Width, maskY));
            //                            boundRectPointList.Add(new Point(maskX - convertImage.Width, maskY + maskHeight));
            //                            boundRectPointList.Add(new Point(maskX, maskY + maskHeight));
            //                        }
            //                    }
            //                    else // 화살표 방향 왼쪽에서 오른쪽
            //                    {
            //                        var edgePointList = edgeAlgorithm.GetHorizontalEdgePos(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
            //                        if (edgePointList.Count > 0)
            //                        {
            //                            var target = edgePointList.OrderByDescending(edgePoint => edgePoint.PointX);
            //                            var minEdge = target.Last();
            //                            var maxEdge = target.First();

            //                            int leftTopTempX = minEdge.PointY > maxEdge.PointY ? maxEdge.PointX : minEdge.PointX;
            //                            int leftBottomTempX = minEdge.PointY > maxEdge.PointY ? minEdge.PointX : maxEdge.PointX;

            //                            int leftTopX = (int)mappingStartX;
            //                            int leftBottomX = (int)mappingStartX;

            //                            leftTopX += leftTopTempX;
            //                            leftBottomX += leftBottomTempX;

            //                            searchedValue = 1;

            //                            //searchedValue = min;
            //                            int maskX = (int)mappingStartX;
            //                            int maskY = (int)mappingStartY; // Y 좌표 설정

            //                            boundRectPointList.Add(new Point(leftTopX, maskY));
            //                            boundRectPointList.Add(new Point(leftTopX + convertImage.Width, maskY));
            //                            boundRectPointList.Add(new Point(leftBottomX + convertImage.Width, maskY + convertImage.Height));
            //                            boundRectPointList.Add(new Point(leftBottomX, maskY + convertImage.Height));

            //                        }
            //                    }
            //                    //   

            //                }

            //                if (searchedValue >= 0)
            //                {
            //                    int MaskingValue = m_TeachParameter[nROI].iMaskingValue; // UI 에 빼야함
            //                    MCvScalar maskingColor = new MCvScalar(MaskingValue);

            //                    Mat matImage = edgeAlgorithm.GetConvertMatImage(cogImage.CopyBase(CogImageCopyModeConstants.CopyPixels) as CogImage8Grey);
            //                    CvInvoke.FillPoly(matImage, new VectorOfPoint(boundRectPointList.ToArray()), maskingColor);
            //                    //matImage.Save(@"D:\matImage.bmp");

            //                    var filterImage = edgeAlgorithm.GetConvertCogImage(matImage);

            //                    SingleFindLine[i].RunParams.CaliperRunParams.ContrastThreshold = m_TeachParameter[nROI].iEdgeCaliperThreshold;
            //                    SingleFindLine[i].RunParams.CaliperRunParams.FilterHalfSizeInPixels = m_TeachParameter[nROI].iEdgeCaliperFilterSize;
            //                    SingleFindLine[i].InputImage = (CogImage8Grey)filterImage;
            //                    List_NG.Items.Add("Found Gray Area.");

            //                    if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
            //                    {
            //                        CogImageFileBMP bmp3 = new CogImageFileBMP();
            //                        bmp3.Open(@"D:\filterImage.bmp", CogImageFileModeConstants.Write);
            //                        bmp3.Append(filterImage);
            //                        bmp3.Close();
            //                    }
            //                }
            //                else
            //                {
            //                    // Edge 못찾은 경우
            //                    SingleFindLine[i].RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
            //                    SingleFindLine[i].RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
            //                    SingleFindLine[i].InputImage = cogImage;
            //                    List_NG.Items.Add("Not Found Gray Area.");
            //                }

            //                if (cogImage.SelectedSpaceName == "@\\Fixture\\Fixture")
            //                    isTwiceFixture = true;

            //                if (searchedValue >= 0)
            //                {
            //                    mCogFixtureTool2.InputImage = SingleFindLine[i].InputImage;
            //                    mCogFixtureTool2.RunParams.UnfixturedFromFixturedTransform = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempFixtureTrans;
            //                    mCogFixtureTool2.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
            //                    mCogFixtureTool2.Run();

            //                    SingleFindLine[i].InputImage = (CogImage8Grey)mCogFixtureTool2.OutputImage;
            //                }
            //                else
            //                    isTwiceFixture = true;
            //            }
            //            else
            //            {

            //                SingleFindLine[i].InputImage = cogImage;
            //            }

            //            SingleFindLine[i].Run();

            //            if (SingleFindLine[i].Results == null)
            //            {
            //                m_LineTool.RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
            //                m_LineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
            //                m_LineTool.RunParams.CaliperSearchDirection = searchDirection;
            //                m_LineTool.RunParams.CaliperRunParams.Edge0Polarity = edgePolarity;
            //                m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
            //                m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
            //                m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
            //                m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;
            //                continue;
            //            }

            //            //Search OK
            //            if (SingleFindLine[i].Results != null || SingleFindLine[i].Results.Count > 0)
            //            {
            //                ResultData = new double[SingleFindLine[i].Results.Count];
            //                for (int j = 0; j < SingleFindLine[i].Results.Count; j++)
            //                {
            //                    if (isTwiceFixture)
            //                    {
            //                        var graphic = SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge);

            //                        foreach (var item in graphic.Shapes)
            //                        {
            //                            if (item is CogLineSegment line)
            //                            {
            //                                cogImage.GetTransform("@", cogImage.SelectedSpaceName).MapPoint(line.StartX, line.StartY, out double mX, out double mY);
            //                                mCogFixtureTool2.RunParams.UnfixturedFromFixturedTransform.MapPoint(line.StartX, line.StartY, out double mappingStartX, out double mappingStartY);
            //                                line.StartX = mappingStartX;
            //                                line.StartY = mappingStartY;

            //                                mCogFixtureTool2.RunParams.UnfixturedFromFixturedTransform.MapPoint(line.EndX, line.EndY, out double mappingEndX, out double mappingEndY);
            //                                line.EndX = mappingEndX;
            //                                line.EndY = mappingEndY;
            //                            }
            //                        }

            //                        GraphicData.Add(graphic);
            //                    }
            //                    else
            //                    {
            //                        //
            //                        var graphic = SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge);
            //                        GraphicData.Add(graphic);
            //                    }

            //                    if (SingleFindLine[i].Results[j].CaliperResults.Count == 1)
            //                    {
            //                        RawSearchData[i, j].X = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionX;
            //                        RawSearchData[i, j].Y = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionY;
            //                    }
            //                    else
            //                    {
            //                        RawSearchData[i, j].X = 0;
            //                        RawSearchData[i, j].Y = 0;
            //                        NonCaliperCnt++;
            //                    }
            //                }

            //            }
            //            //Search NG
            //            else
            //            {
            //                bRes = false;
            //            }
            //        }
            //        #endregion

            //        #region Result Data Calculate
            //        for (int i = 0; i < SingleFindLine[0].Results.Count; i++)
            //        {
            //            //두 점 사이의 거리 
            //            ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
            //            Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
            //        }
            //        #endregion

            //        m_LineTool.RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
            //        m_LineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
            //        m_LineTool.RunParams.CaliperSearchDirection = searchDirection;
            //        m_LineTool.RunParams.CaliperRunParams.Edge0Polarity = edgePolarity;
            //        m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
            //        m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
            //        m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
            //        m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;

            //        return bRes;
            //    }
            //    catch (Exception err)
            //    {
            //        m_LineTool.RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
            //        m_LineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
            //        m_LineTool.RunParams.CaliperSearchDirection = searchDirection;
            //        m_LineTool.RunParams.CaliperRunParams.Edge0Polarity = edgePolarity;
            //        m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
            //        m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
            //        m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
            //        m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;

            //        string LogMsg;
            //        LogMsg = "Inspeciton Excetion NG"; LogMsg += "Error : " + err.ToString();
            //        List_NG.Items.Add(LogMsg);
            //        List_NG.Items.Add(nROI.ToString());
            //        ResultData = new double[] { };
            //        NonCaliperCnt = 0;
            //        GraphicData = new CogGraphicInteractiveCollection();
            //        return false;
            //    }

            //}
            //else   //Circle Tool
            //{
            //    try
            //    {
            //        CogFindCircleTool m_CircleTool = tool as CogFindCircleTool;
            //        bool bRes = true;
            //        int nCaliperCount;
            //        CogFindCircleTool[] SingleCircleLine = new CogFindCircleTool[2];
            //        PointF[,] RawSearchData = new PointF[2, 100];
            //        ResultData = new double[100];
            //        /*GraphicData = new CogGraphicInteractiveCollection()*/
            //        CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();

            //        #region FindLine Search
            //        m_CircleTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
            //        m_CircleTool.InputImage = cogImage;
            //        m_CircleTool.Run();

            //        if (m_CircleTool.Results != null)
            //        {
            //            nCaliperCount = m_CircleTool.Results.Count;
            //            NonCaliperCnt = 0;

            //        }
            //        else
            //        {
            //            NonCaliperCnt = 0;
            //            return false;
            //        }
            //        //Search OK
            //        if (m_CircleTool.Results != null || m_CircleTool.Results.Count > 0)
            //        {
            //            ResultData = new double[m_CircleTool.Results.Count];
            //            for (int j = 0; j < m_CircleTool.Results.Count; j++)
            //            {

            //                GraphicData.Add(m_CircleTool.Results[j].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge));
            //                if (m_CircleTool.Results[j].CaliperResults.Count >= 1)
            //                {
            //                    RawSearchData[0, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionX;
            //                    RawSearchData[0, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionY;
            //                    RawSearchData[1, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionX;
            //                    RawSearchData[1, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionY;
            //                }
            //                else
            //                {
            //                    RawSearchData[0, j].X = 0;
            //                    RawSearchData[0, j].Y = 0;
            //                    RawSearchData[1, j].X = 0;
            //                    RawSearchData[1, j].Y = 0;
            //                    NonCaliperCnt++;
            //                }
            //            }
            //        }
            //        //Search NG
            //        else
            //        {
            //            bRes = false;
            //        }
            //        #endregion

            //        #region Result Data Calculate
            //        for (int i = 0; i < m_CircleTool.Results.Count; i++)
            //        {
            //            //두 점 사이의 거리 
            //            ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
            //            Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
            //        }
            //        #endregion

            //        return bRes;
            //    }
            //    catch (Exception err)
            //    {
            //        string LogMsg;
            //        LogMsg = "Inspeciton Excetion NG"; LogMsg += "Error : " + err.ToString();
            //        List_NG.Items.Add(LogMsg);
            //        List_NG.Items.Add(nROI.ToString());
            //        ResultData = new double[] { };
            //        NonCaliperCnt = 0;
            //        GraphicData = new CogGraphicInteractiveCollection();
            //        return false;
            //    }
            //}
        }

        public IntPtr GetIntptr(CogImage8Grey image, out int stride)
        {
            unsafe
            {
                var cogPixelData = image.Get8GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, image.Width, image.Height);
                IntPtr ptrData = cogPixelData.Scan0;
                stride = cogPixelData.Stride;

                return ptrData;
            }
        }

        public enum EdgeDirection
        {

        }
        //private Tuple<CogImage8Grey, EdgeDirection> GetCropImage(CogImage8Grey cogImage, CogFindLineTool tool, CogTransform2DLinear transform, out CogRectangle cropRect)
        //{
        //    //cropRect = new CogRectangle();
        //    //EdgeDirection direction = EdgeDirection.Top;

        //    //double MinLineDegreeStand = 1.396;
        //    //double MaxLineDegreeStand = 1.745;

        //    ////1.가로, 세로 확인                     
        //    //if (Math.Abs(tool.RunParams.ExpectedLineSegment.Rotation) > MinLineDegreeStand &&
        //    //   Math.Abs(tool.RunParams.ExpectedLineSegment.Rotation) < MaxLineDegreeStand)
        //    //{
        //    //    direction = EdgeDirection.Left;
        //    //    //2.세로인 경우, 사분면 중 어디에 위치해 있는지 확인
        //    //    if (tool.RunParams.ExpectedLineSegment.StartY < 0) //음수 1,4분면
        //    //    {
        //    //        //3.Start Y, End Y 중 어떤게 상단에 위치해 있는지 확인
        //    //        if (tool.RunParams.ExpectedLineSegment.StartY <
        //    //            tool.RunParams.ExpectedLineSegment.EndY)
        //    //        {
        //    //            //Start Y가 상단에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.StartX - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY;
        //    //            cropRect.Width = tool.RunParams.CaliperSearchLength;
        //    //            cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
        //    //        }
        //    //        else
        //    //        {
        //    //            //End Y가 상단에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.EndX - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY;
        //    //            cropRect.Width = tool.RunParams.CaliperSearchLength;
        //    //            cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
        //    //        }
        //    //    }
        //    //    else //양수 2,3분면
        //    //    {
        //    //        //3.Start Y, End Y 중 어떤게 상단에 위치해 있는지 확인
        //    //        if (tool.RunParams.ExpectedLineSegment.StartY <
        //    //            tool.RunParams.ExpectedLineSegment.EndY)
        //    //        {
        //    //            //Start Y가 상단에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.StartX - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY;
        //    //            cropRect.Width = tool.RunParams.CaliperSearchLength;
        //    //            cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
        //    //        }
        //    //        else
        //    //        {
        //    //            //End Y가 상단에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.EndX - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY;
        //    //            cropRect.Width = tool.RunParams.CaliperSearchLength;
        //    //            cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    direction = EdgeDirection.Top;
        //    //    //2.가로인 경우, 사분면 중 어디에 위치해 있는지 확인
        //    //    if (tool.RunParams.ExpectedLineSegment.StartX < 0) //음수 3,4분면
        //    //    {
        //    //        //3.Start X, End X 중 어떤게 좌측에 위치해 있는지 확인
        //    //        if (tool.RunParams.ExpectedLineSegment.StartX <
        //    //           tool.RunParams.ExpectedLineSegment.EndX)
        //    //        {
        //    //            //Start X가  좌측에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.StartX;
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
        //    //            cropRect.Height = tool.RunParams.CaliperSearchLength;
        //    //        }
        //    //        else
        //    //        {
        //    //            //End X가 좌측에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.EndX;
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
        //    //            cropRect.Height = tool.RunParams.CaliperSearchLength;
        //    //        }

        //    //    }
        //    //    else //양수 1,2분면
        //    //    {
        //    //        //3.Start X, End X 중 어떤게 좌측에 위치해 있는지 확인
        //    //        if (tool.RunParams.ExpectedLineSegment.StartX <
        //    //           tool.RunParams.ExpectedLineSegment.EndX)
        //    //        {
        //    //            //Start X가 좌측에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.StartX;
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
        //    //            cropRect.Height = tool.RunParams.CaliperSearchLength;
        //    //        }
        //    //        else
        //    //        {
        //    //            //End X가 좌측에 있음
        //    //            cropRect.X = tool.RunParams.ExpectedLineSegment.EndX;
        //    //            cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY - (tool.RunParams.CaliperSearchLength / 2);
        //    //            cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
        //    //            cropRect.Height = tool.RunParams.CaliperSearchLength;
        //    //        }
        //    //    }

        //    //}

        //    //EdgeAlgorithm edge = new EdgeAlgorithm();
        //    //Mat mat = edge.GetConvertMatImage(cogImage);
        //    ////mat.Save(@"D:\test.bmp");


        //    //transform.MapPoint(cropRect.X, cropRect.Y, out double cropX, out double cropY);
        //    //Rectangle rectFromMat = new Rectangle();
        //    //rectFromMat.X = (int)cropX;
        //    //rectFromMat.Y = (int)cropY;
        //    //rectFromMat.Width = (int)cropRect.Width;
        //    //rectFromMat.Height = (int)cropRect.Height;

        //    //Mat cropMat = edge.CropRoi(mat, rectFromMat);

        //    //if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
        //    //    cropMat.Save(@"D:\cropMat.bmp");

        //    //mat.Dispose();

        //    //return new Tuple<CogImage8Grey, EdgeDirection>(edge.GetConvertCogImage(cropMat), direction);
        //}

        private bool GaloDirectionConvertInspection(int nROI, int toolType, object tool, CogImage8Grey cogImage, out double[] ResultData, ref CogGraphicInteractiveCollection GraphicData, out int NonCaliperCnt)
        {
            ResultData = new double[2];
            NonCaliperCnt = 1;
            return true;
            //try
            //{
            //    NonCaliperCnt = 0;
            //    if (toolType == (int)enumROIType.Line)
            //    {
            //        bool MoveTypeY = false;
            //        //2023 0130 YSH
            //        bool bRes = true;
            //        CogFindLineTool m_LineTool = tool as CogFindLineTool;

            //        int[] nCaliperCount = new int[2];
            //        CogFindLineTool[] SingleFindLine = new CogFindLineTool[2];
            //        PointF[,] RawSearchData = new PointF[2, 100];
            //        ResultData = new double[100];
            //        CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();
            //        double startPosX = m_LineTool.RunParams.ExpectedLineSegment.StartX;
            //        double startPosY = m_LineTool.RunParams.ExpectedLineSegment.StartY;
            //        double EndPosX = m_LineTool.RunParams.ExpectedLineSegment.EndX;
            //        double EndPosY = m_LineTool.RunParams.ExpectedLineSegment.EndY;
            //        double MovePos1, MovePos2;
            //        double Move = m_LineTool.RunParams.CaliperSearchLength / 2;
            //        double diretion = m_LineTool.RunParams.CaliperSearchDirection;
            //        double HalfSearchLength = m_LineTool.RunParams.CaliperSearchLength / 2;
            //        double TempSearchLength = m_LineTool.RunParams.CaliperSearchLength;

            //        double Cal_StartX = 0;
            //        double Cal_StartY = 0;
            //        double Cal_EndX = 0;
            //        double Cal_EndY = 0;

            //        if (!m_bROIFinealignFlag)
            //        {
            //            if (Math.Abs(EndPosY - startPosY) < 100)
            //            {
            //                MoveTypeY = true;
            //                if (startPosX > EndPosX)
            //                {
            //                    diretion *= -1;
            //                }
            //            }
            //            else
            //            {
            //                MoveTypeY = false;
            //                if (startPosY > EndPosY)
            //                {
            //                    diretion *= -1;
            //                }
            //            }
            //        }

            //        #region FindLine Search
            //        for (int i = 0; i < 2; i++) //Left, Right 의미
            //        {
            //            SingleFindLine[i] = new CogFindLineTool(m_LineTool);
            //            //SingleFindLine[i] = m_LineTool;                     

            //            //2023.06.15 YSH
            //            //기존방식대로 Search 못했을 경우에만 방향 변경하여 재 Search 동작함.
            //            if (m_bInspDirectionChange)
            //            {
            //                //Search 방향 변경
            //                SingleFindLine[i].RunParams.CaliperSearchDirection *= (-1);
            //                //극성 변경
            //                SingleFindLine[i].RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
            //                //Caliper Search Length 절반으로 줄임
            //                SingleFindLine[i].RunParams.CaliperSearchLength = HalfSearchLength;
            //            }

            //            if (i == 1)
            //            {
            //                //하나의 FindLineTool방향만 정반대로 돌려서 Search진행
            //                double dist = SingleFindLine[i].RunParams.CaliperSearchDirection;
            //                SingleFindLine[i].RunParams.CaliperSearchDirection *= (-1);

            //                if (m_bROIFinealignFlag)
            //                {
            //                    double Calrotation = m_dTempFineLineAngle - SingleFindLine[i].RunParams.ExpectedLineSegment.Rotation;

            //                    Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.StartX, SingleFindLine[i].RunParams.ExpectedLineSegment.StartY,
            //                            SingleFindLine[i].RunParams.CaliperSearchLength, Calrotation, out Cal_StartX, out Cal_StartY);

            //                    Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.EndX, SingleFindLine[i].RunParams.ExpectedLineSegment.EndY,
            //                            SingleFindLine[i].RunParams.CaliperSearchLength, Calrotation, out Cal_EndX, out Cal_EndY);

            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = Cal_StartX;
            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = Cal_EndX;
            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = Cal_StartY;
            //                    SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = Cal_EndY;
            //                }
            //                else
            //                {
            //                    if (!MoveTypeY)
            //                    {
            //                        if (diretion < 0)
            //                        {
            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX + Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX + Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
            //                        }
            //                        else
            //                        {
            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX - Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX - Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (diretion < 0)
            //                        {

            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY - Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY - Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
            //                        }
            //                        else
            //                        {
            //                            MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY + Move;
            //                            MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY + Move;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
            //                            SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
            //                        }
            //                    }
            //                }

            //            }


            //            SingleFindLine[i].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
            //            SingleFindLine[i].LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;
            //            SingleFindLine[i].InputImage = cogImage;
            //            SingleFindLine[i].Run();

            //            nCaliperCount[i] = SingleFindLine[i].Results.Count;
            //            //Search OK
            //            if (SingleFindLine[i].Results != null || SingleFindLine[i].Results.Count > 0)
            //            {
            //                ResultData = new double[SingleFindLine[i].Results.Count];
            //                for (int j = 0; j < SingleFindLine[i].Results.Count; j++)
            //                {
            //                    GraphicData.Add(SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge));
            //                    //GraphicData.Add(SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge | CogFindLineResultGraphicConstants.CaliperRegion));
            //                    if (SingleFindLine[i].Results[j].CaliperResults.Count == 1)
            //                    {
            //                        RawSearchData[i, j].X = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionX;
            //                        RawSearchData[i, j].Y = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionY;
            //                    }
            //                    else
            //                    {
            //                        RawSearchData[i, j].X = 0;
            //                        RawSearchData[i, j].Y = 0;
            //                        NonCaliperCnt++;

            //                    }
            //                }

            //            }
            //            //Search NG
            //            else
            //            {
            //                bRes = false;
            //            }

            //        }


            //        #endregion

            //        #region Result Data Calculate
            //        //두 FindLine에서 찾은 Caliper 개수가 상이할때
            //        if (nCaliperCount[0] != nCaliperCount[1])
            //        {

            //        }


            //        //for (int i = 0; i < SingleFindLine[0].Results.Count; i++)
            //        //{
            //        //    //두 점 사이의 거리 
            //        //    ResultData[i] = Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
            //        //    Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)));
            //        //}

            //        for (int i = 0; i < SingleFindLine[0].Results.Count; i++)
            //        {
            //            //두 점 사이의 거리 
            //            ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
            //            Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
            //        }

            //        #endregion
            //        m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
            //        m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
            //        m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
            //        m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;
            //        return bRes;
            //    }
            //    else   //Circle Tool
            //    {
            //        CogFindCircleTool m_CircleTool = tool as CogFindCircleTool;
            //        bool bRes = true;
            //        int nCaliperCount;
            //        CogFindCircleTool[] SingleCircleLine = new CogFindCircleTool[2];
            //        PointF[,] RawSearchData = new PointF[2, 100];
            //        ResultData = new double[100];
            //        /*GraphicData = new CogGraphicInteractiveCollection()*/
            //        CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();

            //        #region FindLine Search
            //        //for (int i = 0; i < 2; i++) //Left, Right 의미
            //        //{
            //        //    SingleCircleLine[i] = new CogFindCircleTool();
            //        //    SingleCircleLine[i] = m_CircleTool;
            //        //    if (i == 1) //하나의 FindLineTool방향만 정반대로 돌려서 Search진행
            //        //    {
            //        //        CogFindCircleSearchDirectionConstants DirType = SingleCircleLine[i].RunParams.CaliperSearchDirection;
            //        //        if (DirType == CogFindCircleSearchDirectionConstants.Inward)
            //        //            SingleCircleLine[i].RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
            //        //        else
            //        //            SingleCircleLine[i].RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;
            //        //        double MoveX;
            //        //        if (SingleCircleLine[i].RunParams.CaliperSearchDirection == CogFindCircleSearchDirectionConstants.Inward)
            //        //        {
            //        //            if(dAngle >0)
            //        //               MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX - Move;
            //        //            else
            //        //               MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX + Move;
            //        //            SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX = MoveX;
            //        //        }
            //        //        else
            //        //        {
            //        //            if (dAngle > 0)
            //        //                MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX + Move;
            //        //            else
            //        //                MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX - Move;
            //        //            SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX = MoveX;
            //        //        }
            //        //    }
            //        m_CircleTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
            //        m_CircleTool.InputImage = cogImage;
            //        m_CircleTool.Run();

            //        //SingleCircleLine[i].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;


            //        if (m_CircleTool.Results != null)
            //        {
            //            nCaliperCount = m_CircleTool.Results.Count;
            //            NonCaliperCnt = 0;

            //        }
            //        else
            //        {
            //            NonCaliperCnt = 0;
            //            return false;
            //        }
            //        //Search OK
            //        if (m_CircleTool.Results != null || m_CircleTool.Results.Count > 0)
            //        {
            //            ResultData = new double[m_CircleTool.Results.Count];
            //            for (int j = 0; j < m_CircleTool.Results.Count; j++)
            //            {

            //                GraphicData.Add(m_CircleTool.Results[j].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge));
            //                if (m_CircleTool.Results[j].CaliperResults.Count >= 1)
            //                {
            //                    RawSearchData[0, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionX;
            //                    RawSearchData[0, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionY;
            //                    RawSearchData[1, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionX;
            //                    RawSearchData[1, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionY;
            //                }
            //                else
            //                {
            //                    RawSearchData[0, j].X = 0;
            //                    RawSearchData[0, j].Y = 0;
            //                    RawSearchData[1, j].X = 0;
            //                    RawSearchData[1, j].Y = 0;
            //                    NonCaliperCnt++;
            //                }
            //            }
            //        }
            //        //Search NG
            //        else
            //        {
            //            bRes = false;
            //        }


            //        #endregion
            //        //double dx1 = SingleCircleLine[0].Results[0].CaliperResults[0].Edge0.PositionX;
            //        //double dx2 = SingleCircleLine[1].Results[1].CaliperResults[0].Edge0.PositionX;
            //        #region Result Data Calculate
            //        //두 FindLine에서 찾은 Caliper 개수가 상이할때


            //        for (int i = 0; i < m_CircleTool.Results.Count; i++)
            //        {
            //            //두 점 사이의 거리 
            //            ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
            //            Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
            //        }

            //        #endregion

            //        return bRes;
            //    }
            //}
            //catch (Exception err)
            //{
            //    // PAT[m_PatTagNo, 0].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    string LogMsg;
            //    //LogMsg = "Inspeciton Excetion NG Type:" + m_ROYTpe.ToString() + " " + "ROI No:" + nRoi.ToString() + "CaliperIndex:" + jCaliperIndex.ToString();
            //    //LogdataDisplay(LogMsg, true);
            //    LogMsg = "Inspeciton Excetion NG"; LogMsg += "Error : " + err.ToString();
            //    //LogdataDisplay(LogMsg, true);
            //    List_NG.Items.Add(LogMsg);
            //    List_NG.Items.Add(nROI.ToString());
            //    ResultData = new double[] { };
            //    NonCaliperCnt = 0;
            //    GraphicData = new CogGraphicInteractiveCollection();
            //    return false;
            //}


        }

        private void init_ComboPolarity()
        {
            ////기존 관로검사 : Combo_Polarity1,2,3 
            //Combo_Polarity1.Items.Clear();
            //Combo_Polarity2.Items.Clear();
            //Combo_Polarity3.Items.Clear();
            //cmbEdgePolarityType.Items.Clear();
            //string[] strName = new string[3];
            //strName[0] = "Dark -> Light";
            //strName[1] = "Light -> Dark";
            //strName[2] = "Don't Care";
            //for (int i = 0; i < 3; i++)
            //{
            //    Combo_Polarity1.Items.Add(strName[i]);
            //    Combo_Polarity2.Items.Add(strName[i]);
            //    Combo_Polarity3.Items.Add(strName[i]);

            //    //Bonding Area Align Polarity : cmbEdgePolarityType
            //    cmbEdgePolarityType.Items.Add(strName[i]);
            //}
            //Combo_Polarity1.SelectedIndex = 2;
            //Combo_Polarity2.SelectedIndex = 2;
            //Combo_Polarity3.SelectedIndex = 2;

            ////Bonding Area Align Polarity : cmbEdgePolarityType
            //cmbEdgePolarityType.SelectedIndex = 2;

        }

        private void LAB_CALIPER_SEARCHLENTH_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_CALIPER_SEARCHLENTH.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //double CaliperSearchLenth = KeyPad.m_data;
            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.CaliperSearchLength = CaliperSearchLenth;
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.CaliperSearchLength = CaliperSearchLenth;
            //}
            //LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", CaliperSearchLenth);
        }
        #endregion

        private void Combo_Polarity1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CogCaliperPolarityConstants Polarity;
            //int TempIndex = 0;
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    TempIndex = Combo_Polarity1.SelectedIndex;
            //    Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);

            //    m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            //}
            //else
            //{
            //    TempIndex = Combo_Polarity1.SelectedIndex;
            //    Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);

            //    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            //}
        }

        private void Combo_Polarity2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CogCaliperPolarityConstants Polarity;
            //int TempIndex = 0;
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    TempIndex = Combo_Polarity2.SelectedIndex;
            //    Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);

            //    m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity = Polarity;
            //}
            //else
            //{
            //    TempIndex = Combo_Polarity2.SelectedIndex;
            //    Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);

            //    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity = Polarity;
            //}
        }

        private void btn_Inspection_Test_Click(object sender, EventArgs e)
        {
            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //try
            //{
            //    CogStopwatch Stopwatch = new CogStopwatch();

            //    CogGraphicLabel[] Label;

            //    float nFontSize = (float)((PT_Display01.Height / Main.DEFINE.FontSize) * PT_Display01.Zoom);
            //    Stopwatch.Start();
            //    PT_Display01.InteractiveGraphics.Clear();
            //    PT_Display01.StaticGraphics.Clear();

            //    resultGraphics.Clear();
            //    //PT_Display01.Image = OriginImage;
            //    PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
            //    //bool bSearchRes = Search_PATCNL();
            //    bool[] bROIRes;
            //    bool bRes = true;
            //    double[] dDistance;
            //    List_NG.Items.Clear();
            //    bool bSearchRes = true;
            //    int ignore = 0;
            //    //Live Mode On상태일 시, Off로 변경
            //    if (BTN_LIVEMODE.Checked)
            //    {
            //        BTN_LIVEMODE.Checked = false;
            //        BTN_LIVEMODE.BackColor = Color.DarkGray;
            //    }
            //    if (bSearchRes == true)
            //    {
            //        if (!FinalTracking()) return;
            //        dDistance = new double[m_TeachParameter.Count];
            //        bROIRes = new bool[m_TeachParameter.Count];

            //        //bsi ksh ex)
            //        //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].

            //        double[,,] InspData = new double[m_TeachParameter.Count, 100, 4];
            //        for (int i = 0; i < m_TeachParameter.Count; i++)
            //        //Parallel.For(0, m_TeachParameter.Count, i =>
            //        {
            //            if (i == 0)
            //            {
            //                for (int iHistogram = 0; iHistogram < m_TeachParameter[i].iHistogramROICnt; iHistogram++)
            //                {
            //                    CogGraphicLabel HistogramValue = new CogGraphicLabel();
            //                    HistogramValue.Font = new Font(Main.DEFINE.FontStyle, 15);
            //                    double ResulteCenterX, ResulteCenterY;
            //                    CogHistogramTool InspeHistogramTool = m_TeachParameter[i].m_CogHistogramTool[iHistogram];
            //                    CogRectangleAffine Rect = (CogRectangleAffine)InspeHistogramTool.Region;
            //                    ResulteCenterX = Rect.CenterX;
            //                    ResulteCenterY = Rect.CenterY;
            //                    InspeHistogramTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //                    InspeHistogramTool.Run();
            //                    if (InspeHistogramTool.Result.Mean > m_TeachParameter[i].iHistogramSpec[iHistogram])
            //                    {
            //                        CogRectangleAffine Result = new CogRectangleAffine();
            //                        Result = (CogRectangleAffine)InspeHistogramTool.Region;
            //                        Result.Color = CogColorConstants.Red;
            //                        HistogramValue.Color = CogColorConstants.Red;
            //                        HistogramValue.X = ResulteCenterX;
            //                        HistogramValue.Y = ResulteCenterY;
            //                        HistogramValue.Text = string.Format("{0:F3}", InspeHistogramTool.Result.Mean);
            //                        PT_Display01.StaticGraphics.Add(HistogramValue, "Histogram");
            //                        PT_Display01.StaticGraphics.Add(Result, "Histogram1");
            //                        string LogMsg;
            //                        LogMsg = string.Format("Inspection NG Histogram ROI:{0:D}", iHistogram + 1); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
            //                        LogMsg += "\n";
            //                        List_NG.Items.Add(LogMsg);
            //                    }
            //                    else
            //                    {
            //                        CogRectangleAffine Result = new CogRectangleAffine();
            //                        Result = (CogRectangleAffine)InspeHistogramTool.Region;
            //                        Result.Color = CogColorConstants.Blue;
            //                        HistogramValue.Color = CogColorConstants.Green;
            //                        HistogramValue.X = ResulteCenterX;
            //                        HistogramValue.Y = ResulteCenterY;
            //                        HistogramValue.Text = string.Format("{0:F3}", InspeHistogramTool.Result.Mean);
            //                        PT_Display01.StaticGraphics.Add(HistogramValue, "Histogram");
            //                        PT_Display01.StaticGraphics.Add(Result, "Histogram1");
            //                    }
            //                }
            //            }
            //            m_enumROIType = (enumROIType)m_TeachParameter[i].m_enumROIType;
            //            if (enumROIType.Line == m_enumROIType)
            //            {
            //                CogFindLineTool InspCogFindLine = new CogFindLineTool();
            //                InspCogFindLine = m_TeachParameter[i].m_FindLineTool;
            //                CogGraphicInteractiveCollection subresultGraphics = new CogGraphicInteractiveCollection();
            //                double[] Result;
            //                if (!GaloOppositeInspection(i, (int)enumROIType.Line, InspCogFindLine, (CogImage8Grey)PT_Display01.Image, out Result, ref subresultGraphics, out ignore))
            //                {
            //                    if (m_bInspDirectionChange)
            //                    {
            //                        subresultGraphics.Clear();
            //                        bRes = GaloDirectionConvertInspection(0, (int)enumROIType.Line, InspCogFindLine, (CogImage8Grey)PT_Display01.Image, out Result, ref subresultGraphics, out ignore);
            //                    }

            //                    if (!bRes)
            //                    {
            //                        double dCenterX = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointX;
            //                        double dCenterY = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointY;
            //                        double dAngle = InspCogFindLine.RunParams.ExpectedLineSegment.Rotation;
            //                        double dLenth = InspCogFindLine.RunParams.ExpectedLineSegment.Length;
            //                        CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
            //                        CogNGRectAffine.Color = CogColorConstants.Red;
            //                        CogNGRectAffine.CenterX = dCenterX;
            //                        CogNGRectAffine.CenterY = dCenterY;
            //                        CogNGRectAffine.SideXLength = dLenth;
            //                        CogNGRectAffine.SideYLength = 100;
            //                        CogNGRectAffine.Rotation = dAngle;
            //                        resultGraphics.Add(CogNGRectAffine);
            //                        string LogMsg;
            //                        LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
            //                        LogMsg += "\n";
            //                        List_NG.Items.Add(LogMsg);
            //                        bRes = false;
            //                        for (int k = 0; k < subresultGraphics.Count; k++)
            //                        {
            //                            resultGraphics.Add(subresultGraphics[k]);
            //                        }
            //                        continue;
            //                    }

            //                }
            //                bROIRes[i] = InspResultData(Result, m_TeachParameter[i].dSpecDistance, m_TeachParameter[i].dSpecDistanceMax, m_TeachParameter[i].IDistgnore, ignore);
            //                if (bROIRes[i] == false)
            //                {
            //                    if (m_bInspDirectionChange)
            //                    {
            //                        subresultGraphics.Clear();
            //                        bRes = GaloDirectionConvertInspection(0, (int)enumROIType.Line, InspCogFindLine, (CogImage8Grey)PT_Display01.Image, out Result, ref subresultGraphics, out ignore);
            //                    }

            //                    if (!bRes)
            //                    {
            //                        double dCenterX = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointX;
            //                        double dCenterY = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointY;
            //                        double dAngle = InspCogFindLine.RunParams.ExpectedLineSegment.Rotation;
            //                        double dLenth = InspCogFindLine.RunParams.ExpectedLineSegment.Length;
            //                        CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
            //                        CogNGRectAffine.Color = CogColorConstants.Red;
            //                        CogNGRectAffine.CenterX = dCenterX;
            //                        CogNGRectAffine.CenterY = dCenterY;
            //                        CogNGRectAffine.SideXLength = dLenth;
            //                        CogNGRectAffine.SideYLength = 100;
            //                        CogNGRectAffine.Rotation = dAngle;
            //                        resultGraphics.Add(CogNGRectAffine);
            //                        string LogMsg;
            //                        LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
            //                        LogMsg += "\n";
            //                        List_NG.Items.Add(LogMsg);
            //                        bRes = false;
            //                        for (int k = 0; k < subresultGraphics.Count; k++)
            //                        {
            //                            resultGraphics.Add(subresultGraphics[k]);
            //                        }
            //                        continue;
            //                    }
            //                    else
            //                    {
            //                        bROIRes[i] = InspResultData(Result, m_TeachParameter[i].dSpecDistance, m_TeachParameter[i].dSpecDistanceMax, m_TeachParameter[i].IDistgnore, ignore);
            //                        if (bROIRes[i] == false)
            //                        {
            //                            double dCenterX = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointX;
            //                            double dCenterY = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointY;
            //                            double dAngle = InspCogFindLine.RunParams.ExpectedLineSegment.Rotation;
            //                            double dLenth = InspCogFindLine.RunParams.ExpectedLineSegment.Length;
            //                            CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
            //                            CogNGRectAffine.Color = CogColorConstants.Red;
            //                            CogNGRectAffine.CenterX = dCenterX;
            //                            CogNGRectAffine.CenterY = dCenterY;
            //                            CogNGRectAffine.SideXLength = dLenth;
            //                            CogNGRectAffine.SideYLength = 100;
            //                            CogNGRectAffine.Rotation = dAngle;
            //                            resultGraphics.Add(CogNGRectAffine);
            //                            string LogMsg;
            //                            LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
            //                            LogMsg += "\n";
            //                            List_NG.Items.Add(LogMsg);
            //                            bRes = false;
            //                            for (int k = 0; k < subresultGraphics.Count; k++)
            //                            {
            //                                resultGraphics.Add(subresultGraphics[k]);
            //                            }
            //                            continue;
            //                        }
            //                    }
            //                }
            //                for (int k = 0; k < subresultGraphics.Count; k++)
            //                {
            //                    resultGraphics.Add(subresultGraphics[k]);
            //                }

            //            }
            //            else   //Circle
            //            {
            //                CogFindCircleTool InspCogCircleLine = new CogFindCircleTool();
            //                InspCogCircleLine = m_TeachParameter[i].m_FindCircleTool;
            //                double[] Result;
            //                if (!GaloOppositeInspection(i, (int)enumROIType.Circle, InspCogCircleLine, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore))
            //                {
            //                    double dStartX = InspCogCircleLine.RunParams.ExpectedCircularArc.StartX;
            //                    double dStartY = InspCogCircleLine.RunParams.ExpectedCircularArc.StartY;
            //                    double dEndX = InspCogCircleLine.RunParams.ExpectedCircularArc.EndX;
            //                    double dEndY = InspCogCircleLine.RunParams.ExpectedCircularArc.EndY;

            //                    CogFindLineTool cogTempLine = new CogFindLineTool();
            //                    cogTempLine.RunParams.ExpectedLineSegment.StartX = dStartX;
            //                    cogTempLine.RunParams.ExpectedLineSegment.StartY = dStartY;
            //                    cogTempLine.RunParams.ExpectedLineSegment.EndX = dEndX;
            //                    cogTempLine.RunParams.ExpectedLineSegment.EndY = dEndY;

            //                    CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
            //                    CogNGRectAffine.Color = CogColorConstants.Red;
            //                    CogNGRectAffine.CenterX = cogTempLine.RunParams.ExpectedLineSegment.MidpointX;
            //                    CogNGRectAffine.CenterY = cogTempLine.RunParams.ExpectedLineSegment.MidpointY;
            //                    CogNGRectAffine.SideXLength = cogTempLine.RunParams.ExpectedLineSegment.Length;
            //                    CogNGRectAffine.SideYLength = 100;
            //                    CogNGRectAffine.Rotation = cogTempLine.RunParams.ExpectedLineSegment.Rotation;
            //                    resultGraphics.Add(CogNGRectAffine);
            //                    string LogMsg;
            //                    LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
            //                    LogMsg += "\n";
            //                    List_NG.Items.Add(LogMsg);
            //                    bRes = false;
            //                    continue;
            //                }
            //                bROIRes[i] = InspResultData(Result, m_TeachParameter[i].dSpecDistance, m_TeachParameter[i].dSpecDistanceMax, m_TeachParameter[i].IDistgnore, ignore);

            //                if (bROIRes[i] == false)
            //                {
            //                    double dStartX = InspCogCircleLine.RunParams.ExpectedCircularArc.StartX;
            //                    double dStartY = InspCogCircleLine.RunParams.ExpectedCircularArc.StartY;
            //                    double dEndX = InspCogCircleLine.RunParams.ExpectedCircularArc.EndX;
            //                    double dEndY = InspCogCircleLine.RunParams.ExpectedCircularArc.EndY;

            //                    CogFindLineTool cogTempLine = new CogFindLineTool();
            //                    cogTempLine.RunParams.ExpectedLineSegment.StartX = dStartX;
            //                    cogTempLine.RunParams.ExpectedLineSegment.StartY = dStartY;
            //                    cogTempLine.RunParams.ExpectedLineSegment.EndX = dEndX;
            //                    cogTempLine.RunParams.ExpectedLineSegment.EndY = dEndY;

            //                    CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
            //                    CogNGRectAffine.Color = CogColorConstants.Red;
            //                    CogNGRectAffine.CenterX = cogTempLine.RunParams.ExpectedLineSegment.MidpointX;
            //                    CogNGRectAffine.CenterY = cogTempLine.RunParams.ExpectedLineSegment.MidpointY;
            //                    CogNGRectAffine.SideXLength = cogTempLine.RunParams.ExpectedLineSegment.Length;
            //                    CogNGRectAffine.SideYLength = 100;
            //                    CogNGRectAffine.Rotation = cogTempLine.RunParams.ExpectedLineSegment.Rotation;
            //                    resultGraphics.Add(CogNGRectAffine);
            //                    string LogMsg;
            //                    LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
            //                    LogMsg += "\n";
            //                    List_NG.Items.Add(LogMsg);
            //                    bRes = false;
            //                    continue;
            //                }

            //            }
            //        }
            //        ReultView(bRes, bROIRes, dDistance);
            //        if (bRes == true)
            //        {
            //            CogGraphicLabel LabelText = new CogGraphicLabel();
            //            LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //            LabelText.Color = CogColorConstants.Green;
            //            LabelText.Text = "OK";
            //            if (m_bROIFinealignFlag == true) //기능 ON/OFF 시 Overlay 위치 구분 shkang
            //            {
            //                if (Main.DEFINE.UNIT_TYPE == "VENT")
            //                {
            //                    if (Main.ProjectInfo == "_1WELL_VENT")
            //                    {
            //                        LabelText.X = 1500;
            //                        LabelText.Y = 3100;
            //                    }
            //                    else
            //                    {
            //                        LabelText.X = 500;
            //                        LabelText.Y = 3100;
            //                    }
            //                }
            //                else if (Main.DEFINE.UNIT_TYPE == "PATH")
            //                {
            //                    if (Main.ProjectInfo == "_1WELL_PATH")
            //                    {
            //                        LabelText.X = 1000;
            //                        LabelText.Y = 3000;
            //                    }
            //                    else
            //                    {
            //                        LabelText.X = 2000;
            //                        LabelText.Y = 3000;
            //                    }
            //                }
            //            }
            //            else   //사용 X
            //            {
            //                if (Main.DEFINE.UNIT_TYPE == "VENT")
            //                {
            //                    LabelText.X = 2000;
            //                    LabelText.Y = 1000;
            //                }
            //                else if (Main.DEFINE.UNIT_TYPE == "PATH")
            //                {
            //                    LabelText.X = 0;
            //                    LabelText.Y = 900;
            //                }
            //            }

            //            if (resultGraphics == null)
            //                resultGraphics = new CogGraphicInteractiveCollection();
            //            resultGraphics.Add(LabelText);
            //        }
            //        else
            //        {
            //            CogGraphicLabel LabelText = new CogGraphicLabel();
            //            LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //            LabelText.Color = CogColorConstants.Red;
            //            LabelText.Text = "NG";
            //            if (m_bROIFinealignFlag == true) //기능 ON/OFF 시 Overlay 위치 구분 shkang
            //            {
            //                if (Main.DEFINE.UNIT_TYPE == "VENT")
            //                {
            //                    if (Main.ProjectInfo == "_1WELL_VENT")
            //                    {
            //                        LabelText.X = 1500;
            //                        LabelText.Y = 3100;
            //                    }
            //                    else
            //                    {
            //                        LabelText.X = 500;
            //                        LabelText.Y = 3100;
            //                    }
            //                }
            //                else if (Main.DEFINE.UNIT_TYPE == "PATH")
            //                {
            //                    if (Main.ProjectInfo == "_1WELL_PATH")
            //                    {
            //                        LabelText.X = 1000;
            //                        LabelText.Y = 3000;
            //                    }
            //                    else
            //                    {
            //                        LabelText.X = 2000;
            //                        LabelText.Y = 3000;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if (Main.DEFINE.UNIT_TYPE == "VENT")
            //                {
            //                    LabelText.X = 2000;
            //                    LabelText.Y = 1000;

            //                }
            //                else if (Main.DEFINE.UNIT_TYPE == "PATH")
            //                {
            //                    LabelText.X = 0;
            //                    LabelText.Y = 900;
            //                }
            //            }
            //            if (resultGraphics == null)
            //                resultGraphics = new CogGraphicInteractiveCollection();
            //            resultGraphics.Add(LabelText);
            //        }
            //        //PT_Display01.Image.SelectedSpaceName = "@";
            //        PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //        resultGraphics.Clear();
            //        GC.Collect();
            //        Stopwatch.Stop();
            //        Lab_Tact.Text = string.Format("{0:F3}", Stopwatch.Seconds);

            //    }

            //}
            ////             catch(System.Exception n) // cyh - 예외처리 메시지 띄우는거
            ////             {
            ////                 MessageBox.Show(n.ToString());
            ////             }
            //catch (Exception err)
            //{
            //    resultGraphics.Clear();
            //    GC.Collect();

            //    string LogMsg;
            //    LogMsg = "Inspection Error = " + err.Message.ToString();
            //    MessageBox.Show(LogMsg);
            //}


        }

        private bool InspResultData(double[] Dist, double SpecMin, double SpecMax, int SpecIgnore, int CurrentIgnor) // cyh - 매뉴얼시 데이터 나오는곳
        {
            return true;
            //bool Res = true;
            //CurrentIgnor = 0;   //ignore 개수 초기화
            //m_DistIgnoreCnt = CurrentIgnor;
            //for (int i = 0; i < Dist.Length; i++)
            //{
            //    if (Dist[i] > SpecMin && Dist[i] < SpecMax)
            //    {
            //    }
            //    else
            //    {
            //        m_DistIgnoreCnt++;
            //        if (SpecIgnore < m_DistIgnoreCnt)
            //        {
            //            Res = false;
            //            return Res;
            //        }
            //        else
            //            continue;
            //    }
            //}

            //return Res;
        }
        private void ReultView(bool Res, bool[] bROIResult, double[] bDist)
        {
            dataGridView_Result.Rows.Clear();
            string[] strData = new string[7];
            int Count = 0;
            if (Res == false)
            {
                for (int i = 0; i < bROIResult.Length; i++)
                {
                    if (bROIResult[i] == false)
                    {
                        Count++;
                        strData[0] = Count.ToString();
                        strData[1] = i.ToString();
                        strData[2] = "0";
                        strData[3] = "0";
                        strData[4] = "0";
                        strData[5] = "0";
                        strData[6] = string.Format("{0:F3}", bDist[i]);
                    }
                }
                dataGridView_Result.Rows.Add(strData);
            }
        }

        private PointF GetOriginGap()
        {
            return new PointF();
            //PointF patternMatchingGap = new PointF();

            //bool bSearchRes = Search_PATCNL();
            //if (bSearchRes == true)
            //{
            //    patternMatchingGap.X = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX);
            //    patternMatchingGap.Y = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY);
            //}
            //else
            //{
            //    patternMatchingGap.X = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX);
            //    patternMatchingGap.Y = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY);
            //    MessageBox.Show("AMP Module Mark NG!");
            //}

            //return patternMatchingGap;
        }

        private void TrackingTest(bool isTracking)
        {
            //if (isTracking == true)
            //{
            //    foreach (var item in m_TeachLine)
            //    {
            //        item.RunParams.ExpectedLineSegment.StartX -= GetOriginGap().X;
            //        item.RunParams.ExpectedLineSegment.StartY -= GetOriginGap().Y;
            //        item.RunParams.ExpectedLineSegment.EndX -= GetOriginGap().X;
            //        item.RunParams.ExpectedLineSegment.EndY -= GetOriginGap().Y;
            //    }
            //}
            //else
            //{
            //    foreach (var item in m_TeachLine)
            //    {
            //        item.RunParams.ExpectedLineSegment.StartX += GetOriginGap().X;
            //        item.RunParams.ExpectedLineSegment.StartY += GetOriginGap().Y;
            //        item.RunParams.ExpectedLineSegment.EndX += GetOriginGap().X;
            //        item.RunParams.ExpectedLineSegment.EndY += GetOriginGap().Y;
            //    }
            //}
        }

        private void btnAlignInspPos(object sender, EventArgs e)
        {
            //RadioButton btn = (RadioButton)sender;
            //PT_Display01.StaticGraphics.Clear();
            //PT_Display01.InteractiveGraphics.Clear();
            //int iAlignPos = Convert.ToInt32(btn.Tag.ToString());

            //switch (iAlignPos)
            //{
            //    case (int)enumAlignROI.Left1_1:
            //        m_enumAlignROI = enumAlignROI.Left1_1;
            //        btn_TOP_Inscription.BackColor = Color.Green;
            //        btn_Top_Circumcription.BackColor = Color.DarkGray;
            //        btn_Bottom_Inscription.BackColor = Color.DarkGray;
            //        btn_Bottom_Circumcription.BackColor = Color.DarkGray;
            //        m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_1];
            //        break;

            //    case (int)enumAlignROI.Left1_2:
            //        m_enumAlignROI = enumAlignROI.Left1_2;
            //        btn_TOP_Inscription.BackColor = Color.DarkGray;
            //        btn_Top_Circumcription.BackColor = Color.Green;
            //        btn_Bottom_Inscription.BackColor = Color.DarkGray;
            //        btn_Bottom_Circumcription.BackColor = Color.DarkGray;
            //        m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_2];
            //        break;

            //    case (int)enumAlignROI.Right1_1:
            //        m_enumAlignROI = enumAlignROI.Right1_1;
            //        btn_TOP_Inscription.BackColor = Color.DarkGray;
            //        btn_Top_Circumcription.BackColor = Color.DarkGray;
            //        btn_Bottom_Inscription.BackColor = Color.Green;
            //        btn_Bottom_Circumcription.BackColor = Color.DarkGray;
            //        m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Right1_1];
            //        break;

            //    case (int)enumAlignROI.Right1_2:
            //        m_enumAlignROI = enumAlignROI.Right1_2;
            //        btn_TOP_Inscription.BackColor = Color.DarkGray;
            //        btn_Top_Circumcription.BackColor = Color.DarkGray;
            //        btn_Bottom_Inscription.BackColor = Color.DarkGray;
            //        btn_Bottom_Circumcription.BackColor = Color.Green;
            //        m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Right1_2];
            //        break;

            //}

            //Get_FindConerParameter();
            //DrawRoiLine();
        }

        private void Get_FindConerParameter()
        {
            //LAB_Align_Threshold.Text = m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            //LAB_Align_Caliper_Cnt.Text = m_TempTrackingLine.RunParams.NumCalipers.ToString();
            //LAB_Align_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempTrackingLine.RunParams.CaliperProjectionLength);
            //LAB_Align_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempTrackingLine.RunParams.CaliperSearchDirection);
            //lab_Ignore.Text = m_TempTrackingLine.RunParams.NumToIgnore.ToString();
            //int nPolarity = (int)m_TempTrackingLine.RunParams.CaliperRunParams.Edge0Polarity;
            //Combo_Polarity3.SelectedIndex = nPolarity - 1;
            //lblThetaFilterSizeValue.Text = m_TempTrackingLine.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
        }

        private void Align_Threshold(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            //double dThr = Convert.ToDouble(LAB_Align_Threshold.Text);
            //if (iUpdown == 0)
            //{
            //    if (dThr == 255) return;
            //    dThr++;
            //}
            //else
            //{
            //    if (dThr == 1) return;
            //    dThr--;
            //}
            //LAB_Align_Threshold.Text = dThr.ToString();
            //m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold = dThr;
        }
        private void Align_ProjectionLenth(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            //double dProjectionLenth = Convert.ToDouble(LAB_Align_CALIPER_PROJECTIONLENTH.Text);
            //if (iUpdown == 0)
            //{
            //    dProjectionLenth++;
            //}
            //else
            //{
            //    if (dProjectionLenth == 1) return;
            //    dProjectionLenth--;
            //}
            //LAB_Align_CALIPER_PROJECTIONLENTH.Text = dProjectionLenth.ToString();
            //m_TempTrackingLine.RunParams.CaliperProjectionLength = dProjectionLenth;
        }
        private void Align_CaliperCnt(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            //int iCaliperCnt = Convert.ToInt32(LAB_Align_Caliper_Cnt.Text);
            //if (iUpdown == 0)
            //{
            //    iCaliperCnt++;
            //}
            //else
            //{
            //    if (iCaliperCnt == 1) return;
            //    iCaliperCnt--;
            //}
            //LAB_Align_Caliper_Cnt.Text = iCaliperCnt.ToString();
            //m_TempTrackingLine.RunParams.NumCalipers = iCaliperCnt;
        }

        private void LAB_Align_Caliper_Cnt_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_Align_Caliper_Cnt.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //int CaliperCnt = (int)KeyPad.m_data;

            //m_TempTrackingLine.RunParams.NumCalipers = CaliperCnt;
            //DrawRoiLine();

            //LAB_Align_Caliper_Cnt.Text = CaliperCnt.ToString();
        }

        private void LAB_Align_Threshold_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_Align_Threshold.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //double dThreshold = KeyPad.m_data;

            //m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold = dThreshold;

            //LAB_Align_Threshold.Text = ((int)dThreshold).ToString();
        }

        private void LAB_Align_CALIPER_PROJECTIONLENTH_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_Align_CALIPER_PROJECTIONLENTH.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //double CaliperProjectionLenth = KeyPad.m_data;

            //m_TempTrackingLine.RunParams.CaliperSearchLength = CaliperProjectionLenth;

            //LAB_Align_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", CaliperProjectionLenth);
        }

        private void LAB_Align_CALIPER_SEARCHLENTH_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_Align_CALIPER_SEARCHLENTH.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //double CaliperSearchLenth = KeyPad.m_data;

            //m_TempTrackingLine.RunParams.CaliperSearchLength = CaliperSearchLenth;

            //LAB_Align_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", CaliperSearchLenth);
        }

        private void lab_Ignore_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LAB_Align_Caliper_Cnt.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //int iIgnoreCnt = (int)KeyPad.m_data;

            //m_TempTrackingLine.RunParams.NumToIgnore = iIgnoreCnt;
            //DrawRoiLine();

            //lab_Ignore.Text = iIgnoreCnt.ToString();
        }

        private void Combo_Polarity3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CogCaliperPolarityConstants Polarity;
            //int TempIndex = 0;
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    TempIndex = Combo_Polarity3.SelectedIndex;
            //    Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
            //    m_TempTrackingLine.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            //}
            //else
            //{
            //    TempIndex = Combo_Polarity3.SelectedIndex;
            //    Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
            //    m_TempTrackingLine.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            //}
        }

        private void btn_align_roi_show_Click(object sender, EventArgs e)
        {
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //DrawRoiLine();
            //return;
        }

        private void btn_AlginApply_Click(object sender, EventArgs e)
        {
            //PT_Display01.StaticGraphics.Clear();
            //PT_Display01.InteractiveGraphics.Clear();

            //m_TeachLine[(int)m_enumAlignROI] = m_TempTrackingLine;
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
            //PT_Display01.Image = OriginImage;
            //bROIFinealignTeach = false;
            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //resultGraphics.Clear();
            //double dInspectionDistanceX;
            //double[] dx = new double[4];
            //double[] dy = new double[4];
            //double Top_AlignX = 0, Top_AlignY = 0, Bottom_AlignX = 0, Bottom_AlignY = 0;

            //chkUseTracking.Checked = false;
            //bool bSearchRes = Search_PATCNL();
            //if (bSearchRes == true)
            //{
            //    double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            //    double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
            //    CogIntersectLineLineTool[] CrossPoint = new CogIntersectLineLineTool[2];
            //    CogLine[] Line = new CogLine[4];
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (i < 2)
            //        {
            //            CrossPoint[i] = new CogIntersectLineLineTool();
            //            CrossPoint[i].InputImage = (CogImage8Grey)PT_Display01.Image;
            //        }
            //        Line[i] = new CogLine();
            //        m_TeachLine[i].InputImage = (CogImage8Grey)PT_Display01.Image;
            //        double TempStartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX;
            //        double TempStartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY;
            //        double TempEndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX;
            //        double TempEndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY;

            //        double StartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX - TranslationX;
            //        double StartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY - TranslationY;
            //        double EndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX - TranslationX;
            //        double EndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY - TranslationY;

            //        m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = StartA_X;
            //        m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = StartA_Y;
            //        m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = EndA_X;
            //        m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = EndA_Y;

            //        m_TeachLine[i].Run();

            //        if (m_TeachLine[i].Results != null)
            //        {
            //            Line[i] = m_TeachLine[i].Results.GetLine();
            //            //shkang_
            //            if (Line[i] == null)
            //                Line[i] = new CogLine();

            //            if (i < 2)
            //                Line[i].Color = CogColorConstants.Blue;
            //            else
            //                Line[i].Color = CogColorConstants.Orange;
            //            resultGraphics.Add(Line[i]);
            //        }
            //        else
            //        {
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = TempStartA_X;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = TempStartA_Y;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = TempEndA_X;
            //            m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = TempEndA_Y;
            //            MessageBox.Show("Please CrossLine Checking");
            //            return;
            //        }

            //        m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = TempStartA_X;
            //        m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = TempStartA_Y;
            //        m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = TempEndA_X;
            //        m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = TempEndA_Y;
            //    }

            //    //shkang_s 
            //    //필름 밀림 융착 검사 (자재 X거리 검출)
            //    CogGraphicLabel LabelTest = new CogGraphicLabel();
            //    LabelTest.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //    LabelTest.Color = CogColorConstants.Green;
            //    double dPixelResoultion = 13.36;
            //    dInspectionDistanceX = Line[3].X - Line[1].X;   //X 거리 검출
            //    dInspectionDistanceX = dInspectionDistanceX * dPixelResoultion / 1000;
            //    if (dObjectDistanceX + dObjectDistanceSpecX <= dInspectionDistanceX) //NG - Film NG
            //    {
            //        LabelTest.X = 1000;
            //        LabelTest.Y = 180;
            //        LabelTest.Color = CogColorConstants.Red;
            //        LabelTest.Text = string.Format("Film NG, X:{0:F3}", dInspectionDistanceX);
            //    }
            //    else   //OK - Film OK
            //    {
            //        LabelTest.X = 1000;
            //        LabelTest.Y = 180;
            //        LabelTest.Color = CogColorConstants.Green;
            //        LabelTest.Text = string.Format("Film OK, X:{0:F3}", dInspectionDistanceX);
            //    }
            //    resultGraphics.Add(LabelTest);
            //    //shkang_e

            //    CrossPoint[0].LineA = Line[0];
            //    CrossPoint[0].LineB = Line[1];
            //    CrossPoint[1].LineA = Line[2];
            //    CrossPoint[1].LineB = Line[3];
            //    for (int i = 0; i < 2; i++)
            //    {
            //        CogGraphicLabel LineLabelTest = new CogGraphicLabel();
            //        LineLabelTest.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //        LineLabelTest.Color = CogColorConstants.Green;

            //        CrossPoint[i].Run();
            //        if (CrossPoint[i] != null)
            //        {
            //            dCrossX[i] = CrossPoint[i].X;
            //            dCrossY[i] = CrossPoint[i].Y;
            //            if (i == 0)
            //            {
            //                LineLabelTest.X = 100;
            //                LineLabelTest.Y = 100;
            //                LineLabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
            //            }
            //            else
            //            {
            //                LineLabelTest.X = 100;
            //                LineLabelTest.Y = 200;
            //                LineLabelTest.Text = string.Format("Right Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
            //            }
            //            //resultGraphics.Add(LineLabelTest);
            //        }
            //    }
            //    PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //}
        }

        private void Ignore_Distance(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int dIgnoredist = Convert.ToInt32(text_Dist_Ignre.Text);
            //if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            //{
            //    //Up
            //    dIgnoredist++;
            //}
            //else
            //{
            //    //Down
            //    if (dIgnoredist < 0) return;
            //    dIgnoredist--;
            //}
            //m_dDist_ignore = dIgnoredist;
            //text_Dist_Ignre.Text = m_dDist_ignore.ToString();
        }

        private void text_Dist_Ignre_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(text_Dist_Ignre.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 100, nCurData, "Input Data", 0);
            //KeyPad.ShowDialog();
            //int iIgnoreData = (int)KeyPad.m_data;
            //m_dDist_ignore = iIgnoreData;
            //text_Dist_Ignre.Text = m_dDist_ignore.ToString();
        }

        private void text_Spec_Dist_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(text_Spec_Dist.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 100, nCurData, "Input Data", 0);
            //KeyPad.ShowDialog();
            //double dIgnoredist = KeyPad.m_data;
            //m_SpecDist = dIgnoredist;
            //text_Spec_Dist.Text = m_SpecDist.ToString();
        }

        private void chkUseLoadImageTeachMode_CheckedChanged(object sender, EventArgs e)
        {
            //해당 변수가 True 일 경우, Live Mode Off
            //bLiveStop = chkUseLoadImageTeachMode.Checked;
        }

        private void Chk_All_Select_CheckedChanged(object sender, EventArgs e)
        {
            //if (Chk_All_Select.Checked == true)
            //    bAllSelect = true;
            //else
            //    bAllSelect = false;
        }

        private void lab_Histogram_ROI_Count_Click(object sender, EventArgs e)
        {
            //var stucTemp = m_TeachParameter[0];
            //int iHistogramROICnt = Convert.ToInt32(lab_Histogram_ROI_Count.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 32, iHistogramROICnt, "Input Data", 0);
            //KeyPad.ShowDialog();
            //iHistogramROICnt = (int)KeyPad.m_data;
            //lab_Histogram_ROI_Count.Text = iHistogramROICnt.ToString();
            //m_iHistoramROICnt = iHistogramROICnt;
            //stucTemp.iHistogramROICnt = m_iHistoramROICnt;
            //m_TeachParameter[0] = stucTemp;
        }

        private void combo_Histogram_ROI_NO_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (m_HistoROI != combo_Histogram_ROI_NO.SelectedIndex)
            //{
            //    m_HistoROI = combo_Histogram_ROI_NO.SelectedIndex;

            //    PrevCenterX = 0;
            //    PrevCenterY = 0;
            //    PrevMarkX = 0;
            //    PrevMarkY = 0;
            //}
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //m_HistoROI = combo_Histogram_ROI_NO.SelectedIndex;
            //var TempBlob = m_TeachParameter[0];
            //if (TempBlob.m_CogHistogramTool[m_HistoROI] == null)
            //    TempBlob.m_CogHistogramTool[m_HistoROI] = new CogHistogramTool();
            //m_CogHistogramTool[m_HistoROI] = TempBlob.m_CogHistogramTool[m_HistoROI];
            //Get_Histogram_Parameter();
            //button8.PerformClick();
        }
        private void Get_Histogram_Parameter()
        {
            //var Temp = m_TeachParameter[0];
            //lab_Spec_GrayVale.Text = Temp.iHistogramSpec[m_HistoROI].ToString();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            //if (chkUseRoiTracking.Checked == false)
            //    chkUseRoiTracking.Checked = true;
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //CogRectangleAffine ROIRect = new CogRectangleAffine();
            //if (m_CogHistogramTool[m_HistoROI].Region == null)
            //{
            //    ROIRect.SetCenterLengthsRotationSkew(100, 100, 200, 100, 0, 0);
            //    ROIRect.Color = CogColorConstants.Green;
            //    //ROIRect.GraphicDOFEnable = CogRectangleDOFConstants.Position | CogRectangleDOFConstants.Size;
            //    ROIRect.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
            //    ROIRect.Interactive = true;
            //    m_CogHistogramTool[m_HistoROI].Region = ROIRect;
            //}
            //else
            //{
            //    ROIRect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;
            //    ROIRect.Interactive = true;
            //    m_CogHistogramTool[m_HistoROI].Region = ROIRect;
            //}

            //m_CogHistogramTool[m_HistoROI].InputImage = PT_Display01.Image;
            //m_CogHistogramTool[m_HistoROI].CurrentRecordEnable = CogHistogramCurrentRecordConstants.InputImage | CogHistogramCurrentRecordConstants.Region;

            //Display.SetInteractiveGraphics(PT_Display01, m_CogHistogramTool[m_HistoROI].CreateCurrentRecord(), false);
            //if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
            //    PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void lab_Spec_GrayVale_Click(object sender, EventArgs e)
        {
            //var stucTemp = m_TeachParameter[0];
            //int SpecGrayVal = Convert.ToInt32(lab_Spec_GrayVale.Text);
            //KeyPadForm KeyPad = new KeyPadForm(1, 255, SpecGrayVal, "Input Data", 0);
            //KeyPad.ShowDialog();
            //SpecGrayVal = (int)KeyPad.m_data;
            //lab_Spec_GrayVale.Text = SpecGrayVal.ToString();
            //stucTemp.iHistogramSpec[m_HistoROI] = SpecGrayVal;
            //m_TeachParameter[0] = stucTemp;
        }

        private void btn_HistogramTest_Click(object sender, EventArgs e)
        {
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //CogRectangleAffine Rect = new CogRectangleAffine();
            //if (PT_Display01.Image == null && m_CogHistogramTool[m_HistoROI] == null) return;
            //PT_Display01.Image = OriginImage;
            //bool bSearchRes = Search_PATCNL();
            //if (bSearchRes == true)
            //{
            //    double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            //    double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;

            //    PT_Display01.Image = OriginImage;

            //    if (_useROITracking)
            //    {
            //        if (!FinalTracking()) return;
            //        TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
            //        m_CogHistogramTool[m_HistoROI].InputImage = (CogImage8Grey)PT_Display01.Image;
            //        Rect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;
            //    }
            //    else
            //        Rect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;

            //    m_CogHistogramTool[m_HistoROI].Run();
            //    if (m_CogHistogramTool[m_HistoROI].Result == null)
            //    {
            //        MessageBox.Show("Histogram Result NG");
            //        return;
            //    }
            //    //int GrayVal = m_CogHistogramTool[m_HistoROI].Result.Median;
            //    double GrayVal = m_CogHistogramTool[m_HistoROI].Result.Mean;
            //    int Spec = Convert.ToInt32(lab_Spec_GrayVale.Text);
            //    CogGraphicLabel result = new CogGraphicLabel();
            //    result.Font = new Font(Main.DEFINE.FontStyle, 15);
            //    result.X = Rect.CenterX;
            //    result.Y = Rect.CenterY;
            //    if (Spec > GrayVal)
            //    {
            //        Rect.Color = CogColorConstants.Blue;
            //        result.Color = CogColorConstants.Green;
            //    }
            //    else
            //    {
            //        Rect.Color = CogColorConstants.Red;
            //        result.Color = CogColorConstants.Red;
            //    }
            //    result.Text = string.Format("{0:F3}", GrayVal);
            //    PT_Display01.StaticGraphics.Add(result, "Result01");
            //    PT_Display01.StaticGraphics.Add(Rect, "Result01");
            //}
        }

        private void btn_Histogram_Apply_Click(object sender, EventArgs e)
        {
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.StaticGraphics.Clear();
            //if (_useROITracking)
            //{
            //    CogRectangleAffine Rect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;
            //    double CenterX = Rect.CenterX + PrevMarkX;
            //    double CenterY = Rect.CenterY + PrevMarkY;
            //}
            //var Temp = m_TeachParameter[0];
            //Temp.m_CogHistogramTool[m_HistoROI] = m_CogHistogramTool[m_HistoROI];
            //Temp.iHistogramSpec[m_HistoROI] = Convert.ToInt32(lab_Spec_GrayVale.Text);
            //m_TeachParameter[0].m_CogHistogramTool[m_HistoROI] = m_CogHistogramTool[m_HistoROI];
            //m_TeachParameter[0].iHistogramSpec[m_HistoROI] = Temp.iHistogramSpec[m_HistoROI];
            //m_bTrakingRootHisto[m_HistoROI] = true;
        }

        private void btn_Origin_Point_Apply_Click(object sender, EventArgs e)
        {
            //LeftOrigin[0] = dCrossX[0];
            //LeftOrigin[1] = dCrossY[0];
            //RightOrigin[0] = dCrossX[1];
            //RightOrigin[1] = dCrossY[1];
            //lab_LeftOriginX.Text = LeftOrigin[0].ToString("F3");
            //lab_LeftOriginY.Text = LeftOrigin[1].ToString("F3");
            //lab_RightOriginX.Text = RightOrigin[0].ToString("F3");
            //lab_RightOriginY.Text = RightOrigin[1].ToString("F3");
        }

        private bool FinalTracking()
        {
            return true;
            //bool bRes = true;
            //double dGapX = new double();
            //double dGapY = new double();
            //double dGapT = new double();
            //double dGapT_degree = new double();

            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            ////ROIFinealign 사용 시 ROIFinealign만 사용
            //if (m_bROIFinealignFlag)
            //{
            //    PT_Display01.Image = OriginImage;
            //    bool bSearchRes = Search_PATCNL();
            //    if (bSearchRes == true)
            //    {
            //        double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            //        double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
            //        //m_bROIFinealignFlag = true 일때, FixureImage 미사용
            //        //Film Size 측정용도로 사용
            //        bRes = TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
            //        if (bRes)
            //        {
            //            bRes = Main.AlignUnit[m_AlignNo].ROIFinealign(FinealignMark, OriginImage, out dGapX, out dGapY, out dGapT, ref resultGraphics);
            //            if (!bRes)
            //            {
            //                MessageBox.Show("ROI FineAlign Fail");
            //                return bRes;
            //            }
            //        }
            //        else
            //        {
            //            MessageBox.Show("Material Align Fail");
            //            return bRes;
            //        }

            //    }
            //    //2023 0228 YSH Finealign Spec 부분 추후 필요 시 수정 요망
            //    dGapT_degree = dGapT * 180 / Math.PI;
            //    if (-m_dROIFinealignT_Spec < dGapT_degree && dGapT_degree < m_dROIFinealignT_Spec)//Spec Check
            //    {
            //        CogFixtureTool mCogFixtureTool = new CogFixtureTool();
            //        mCogFixtureTool.InputImage = PT_Display01.Image;//TrackingROI() 결과 Fixure 이미지
            //        CogTransform2DLinear TempData = new CogTransform2DLinear();
            //        TempData.TranslationX = dGapX;
            //        TempData.TranslationY = dGapY;
            //        TempData.Rotation = dGapT;
            //        m_dTempFineLineAngle = dGapT;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempFixtureTrans = TempData;
            //        mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
            //        mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
            //        mCogFixtureTool.Run();
            //        //TempTrackingImage =(CogImage8Grey) mCogFixtureTool.OutputImage;
            //        PT_Display01.Image = (CogImage8Grey)mCogFixtureTool.OutputImage;
            //    }
            //    else
            //    {
            //        MessageBox.Show("ROI Finealign Theta Spec Out");
            //        bRes = false;
            //    }
            //}
            //else//ROIFinealign 미사용 시 BondingAlign 사용
            //{
            //    bRes = BondingAlignInspectionTest(out dGapX, out dGapY);
            //    if (!bRes)
            //        return bRes;

            //    CogFixtureTool mCogFixtureTool = new CogFixtureTool();
            //    mCogFixtureTool.InputImage = PT_Display01.Image;
            //    CogTransform2DLinear TempData = new CogTransform2DLinear();
            //    if (Main.DEFINE.UNIT_TYPE == "VENT")
            //    {
            //        TempData.TranslationX = dGapX;
            //        TempData.TranslationY = dGapY;
            //    }
            //    if (Main.DEFINE.UNIT_TYPE == "PATH")
            //    {
            //        TempData.TranslationX = -dGapX;
            //        TempData.TranslationY = dGapY;
            //    }

            //    mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
            //    mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
            //    mCogFixtureTool.Run();
            //    PT_Display01.Image = (CogImage8Grey)mCogFixtureTool.OutputImage;
            //}

            //PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //return bRes;
        }

        private bool BondingAlignInspectionTest(out double dFinalTrackingX, out double dFinalTrackingY)
        {
            dFinalTrackingX = 0;
            dFinalTrackingY = 0;
            return true;
            //double dPixelResolution = 13.36;
            //bool bRes = true;
            //CogGraphicLabel LabelTest = new CogGraphicLabel();
            //LabelTest.Font = new Font(Main.DEFINE.FontStyle, 15, FontStyle.Bold);


            //dFinalTrackingX = new double();
            //dFinalTrackingY = new double();
            //PT_Display01.StaticGraphics.Clear();
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.Image = OriginImage;
            //bool bSearchRes = Search_PATCNL();
            //if (bSearchRes == true)
            //{
            //    double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            //    double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;

            //    //TrackingROI(TranslationX, TranslationY);
            //    TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
            //    //if(!TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY)) return false;     
            //    CogGraphicInteractiveCollection[] resultGraphics = new CogGraphicInteractiveCollection[4];

            //    for (int i = 0; i < 4; i++)
            //    {
            //        resultGraphics[i] = new CogGraphicInteractiveCollection();
            //        m_TeachAlignLine[i].InputImage = PT_Display01.Image;
            //        m_TeachAlignLine[i].Run();

            //        if (m_TeachAlignLine[i].Results != null && m_TeachAlignLine[i].Results.Count > 0)
            //        {
            //            resultGraphics[i].Add(m_TeachAlignLine[i].Results[0].CreateResultGraphics(CogCaliperResultGraphicConstants.Edges));
            //        }
            //        else
            //        {
            //            bRes = false;
            //            dFinalTrackingX = 0;
            //            dFinalTrackingY = 0;
            //            return bRes;
            //        }
            //        PT_Display01.InteractiveGraphics.AddList(resultGraphics[i], "RESULT", false);
            //    }
            //    //Calculation Bonding Align X,Y
            //    double dBonding_AlignX = 0;
            //    double dBonding_AlignY = 0;
            //    dBonding_AlignX = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.X2].Results[0].Edge0.PositionX - m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionX);
            //    dBonding_AlignY = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.Y2].Results[0].Edge0.PositionY - m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionY);

            //    dBonding_AlignX = dBonding_AlignX * dPixelResolution / 1000;
            //    dBonding_AlignY = dBonding_AlignY * dPixelResolution / 1000;

            //    dBonding_AlignX = Convert.ToDouble(lblOkDistanceValueX.Text.ToString()) - dBonding_AlignX;
            //    dBonding_AlignY = Convert.ToDouble(lblOkDistanceValueY.Text.ToString()) - dBonding_AlignY;
            //    dFinalTrackingX = dBonding_AlignX / dPixelResolution * 1000;
            //    dFinalTrackingY = dBonding_AlignY / dPixelResolution * 1000;


            //    double dCheckDistX, dCheckDistY;
            //    dCheckDistX = Math.Abs(dFinalTrackingX);
            //    dCheckDistY = Math.Abs(dFinalTrackingY);
            //    dCheckDistX = dCheckDistX * dPixelResolution / 1000;
            //    dCheckDistY = dCheckDistY * dPixelResolution / 1000;

            //    //Overlay 추가
            //    if (dBondingAlignDistSpecX > dCheckDistX && dBondingAlignDistSpecY > dCheckDistY)   // OK
            //    {
            //        LabelTest.X = 5000;
            //        LabelTest.Y = -6000;
            //        LabelTest.Text = string.Format("Bonding Align OK, X: {0:F3} Y: {1:F3}", dCheckDistX, dCheckDistY);
            //        LabelTest.Color = CogColorConstants.Green;
            //        // LabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
            //    }
            //    else   // NG
            //    {
            //        bRes = false;
            //        if (dBondingAlignDistSpecX < dCheckDistX)
            //        {
            //            LabelTest.X = 1500;
            //            LabelTest.Y = 100;
            //            LabelTest.Text = string.Format("Bonding Align NG, X: {0:F2}", dCheckDistX);
            //            LabelTest.Color = CogColorConstants.Red;
            //            CogRectangle resultRect = new CogRectangle();
            //            resultRect.X = m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionX;
            //            resultRect.Y = m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionY - 150;
            //            resultRect.Width = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.X2].Results[0].Edge0.PositionX - m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionX);
            //            resultRect.Height = 300;
            //            resultRect.Color = CogColorConstants.Red;
            //            PT_Display01.InteractiveGraphics.Add(resultRect, "Result", false);
            //        }
            //        if (dBondingAlignDistSpecY < dCheckDistY)
            //        {
            //            LabelTest.X = 1500;
            //            LabelTest.Y = 200;
            //            LabelTest.Text = string.Format("Bonding Align NG, Y: {0:F2}", dCheckDistY);
            //            LabelTest.Color = CogColorConstants.Red;
            //            CogRectangle resultRect = new CogRectangle();
            //            resultRect.X = m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionX;
            //            resultRect.Y = m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionY;
            //            resultRect.Width = 100;
            //            resultRect.Height = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.Y2].Results[0].Edge0.PositionX - m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionX);
            //            resultRect.Color = CogColorConstants.Red;
            //            PT_Display01.InteractiveGraphics.Add(resultRect, "Result", false);
            //            //LabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
            //        }
            //    }
            //    PT_Display01.InteractiveGraphics.Add(LabelTest, "Result", false);
            //}
            //return bRes;
        }

        private void lblParamFilterSizeValue_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(lblParamFilterSizeValue.Text);
            //KeyPadForm KeyPad = new KeyPadForm(1, 255, nCurData, "Input Data", 2);
            //KeyPad.ShowDialog();
            //int FilterSize = (int)KeyPad.m_data;

            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FilterSize;
            //    FindLineROI();
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FilterSize;
            //    CircleROI();
            //}
            //lblParamFilterSizeValue.Text = FilterSize.ToString();
        }

        private void lblParamFilterSizeValueUpDown_Click(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            //int dFilterSize = Convert.ToInt32(lblParamFilterSizeValue.Text);
            //if (iUpdown == 0)
            //{
            //    if (dFilterSize == 255) return;
            //    dFilterSize++;
            //}
            //else
            //{
            //    if (dFilterSize == 2) return;
            //    dFilterSize--;
            //}
            //lblParamFilterSizeValue.Text = dFilterSize.ToString();
            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
            //    FindLineROI();
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
            //    CircleROI();
            //}
        }

        private void text_Spec_Dist_Max_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(text_Spec_Dist_Max.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 100, nCurData, "Input Data", 0);
            //KeyPad.ShowDialog();
            //double dIgnoredist = KeyPad.m_data;
            //m_SpecDistMax = dIgnoredist;
            //text_Spec_Dist_Max.Text = m_SpecDistMax.ToString();
        }

        private void lblEdgeDirection_Click(object sender, EventArgs e)
        {
            //if (m_enumROIType == enumROIType.Line)
            //{
            //    m_TempFindLineTool.RunParams.CaliperSearchDirection *= (-1);
            //}
            //else
            //{
            //    CogFindCircleSearchDirectionConstants DirType = m_TempFindCircleTool.RunParams.CaliperSearchDirection;
            //    if (DirType == CogFindCircleSearchDirectionConstants.Inward)
            //        m_TempFindCircleTool.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
            //    else
            //        m_TempFindCircleTool.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;
            //}
        }

        private void lblObjectDistanceXSpecValue_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(lblObjectDistanceXSpecValue.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 3000, nCurData, "Input Data", 0);
            //KeyPad.ShowDialog();
            //double dDistanceSpecX = KeyPad.m_data;

            //lblObjectDistanceXSpecValue.Text = dDistanceSpecX.ToString();
            //dObjectDistanceSpecX = Convert.ToDouble(lblObjectDistanceXSpecValue.Text);
        }

        private void lblObjectDistanceXValue_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(lblObjectDistanceXValue.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 3000, nCurData, "Input Data", 0);
            //KeyPad.ShowDialog();
            //double dDistanceX = KeyPad.m_data;

            //lblObjectDistanceXValue.Text = dDistanceX.ToString();
            //dObjectDistanceX = Convert.ToDouble(lblObjectDistanceXValue.Text);
        }

        private void lblThetaFilterSizeValue_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(lblThetaFilterSizeValue.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //int dFilterSize = Convert.ToInt32(KeyPad.m_data);

            //lblThetaFilterSizeValue.Text = Convert.ToString(dFilterSize);
            //m_TempTrackingLine.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
        }

        private void lblThetaFilterSizeValueUpDown_Click(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            //double dFilterSize = Convert.ToDouble(lblThetaFilterSizeValue.Text);
            //if (iUpdown == 0)
            //{
            //    if (dFilterSize == 255) return;
            //    dFilterSize++;
            //}
            //else
            //{
            //    if (dFilterSize == 2) return;
            //    dFilterSize--;
            //}
            //lblThetaFilterSizeValue.Text = dFilterSize.ToString();
            //m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold = dFilterSize;
        }

        private void RDB_ALIGN_TEACH_MODE_Click(object sender, EventArgs e)
        {
            RadioButton TempRadioButton = (RadioButton)sender;


            switch (Convert.ToInt32(TempRadioButton.Tag))
            {
                case 1://자재 얼라인 
                    PANEL_MATERIAL_ALIGN.Visible = true;
                    PANEL_MATERIAL_ALIGN.Location = new System.Drawing.Point(5, 60);
                    PANEL_MATERIAL_ALIGN.Size = new Size(897, 615);
                    PANEL_ROI_FINEALIGN.Visible = false;
                    TempRadioButton.BackColor = Color.LimeGreen;
                    RDB_ROI_FINEALIGN.BackColor = Color.DarkGray;
                    chkUseTracking.Visible = true;
                    break;

                case 3://ROI Fine얼라인             
                    PANEL_MATERIAL_ALIGN.Visible = false;
                    PANEL_ROI_FINEALIGN.Visible = true;
                    PANEL_ROI_FINEALIGN.Location = new System.Drawing.Point(5, 60);
                    PANEL_ROI_FINEALIGN.Size = new Size(740, 206);
                    TempRadioButton.BackColor = Color.LimeGreen;
                    RDB_MATERIAL_ALIGN.BackColor = Color.DarkGray;
                    chkUseTracking.Visible = false;
                    break;

                default:
                    break;
            }
        }

        private void BTN_RETURNPAGE_Click(object sender, EventArgs e)
        {
            //bROIFinealignTeach = false;
            //TABC_MANU.SelectTab(TAB_06);
            //m_PatNo_Sub = 0;
            //RDB_ROI_FINEALIGN.PerformClick();
        }

        private void BTN_ROI_FINEALIGN_TEST_Click(object sender, EventArgs e)
        {
            //bool bRet;
            //double dGapX;
            //double dGapY;
            //double dGapT;
            //double dGapT_degree;
            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

            ////bool bSearchRes = Search_PATCNL();
            //CogGraphicLabel LabelText = new CogGraphicLabel();

            //PT_Display01.StaticGraphics.Clear();
            //PT_Display01.InteractiveGraphics.Clear();
            //bRet = Main.AlignUnit[m_AlignNo].ROIFinealign(FinealignMark, OriginImage, out dGapX, out dGapY, out dGapT, ref resultGraphics);
            //if (bRet)
            //{
            //    if (Main.DEFINE.UNIT_TYPE == "VENT")
            //    {
            //        LabelText.X = 2000;
            //        LabelText.Y = 1000;
            //    }
            //    else if (Main.DEFINE.UNIT_TYPE == "PATH")
            //    {
            //        LabelText.X = 1800;
            //        LabelText.Y = 900;
            //    }

            //    //2023 0228 YSH Finealign Spec 부분 추후 필요 시 수정 요망
            //    dGapT_degree = dGapT * 180 / Math.PI;
            //    if (-m_dROIFinealignT_Spec < dGapT_degree && dGapT_degree < m_dROIFinealignT_Spec)//Spec Check
            //    {
            //        CogFixtureTool mCogFixtureTool = new CogFixtureTool();
            //        mCogFixtureTool.InputImage = OriginImage;
            //        CogTransform2DLinear TempData = new CogTransform2DLinear();
            //        TempData.TranslationX = dGapX;
            //        TempData.TranslationY = dGapY;
            //        TempData.Rotation = dGapT;
            //        mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
            //        mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
            //        mCogFixtureTool.Run();
            //        PT_Display01.Image = mCogFixtureTool.OutputImage;
            //        LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //        LabelText.Color = CogColorConstants.Green;
            //        LabelText.Text = "ROI FINEALIGN OK";
            //        //현재 이미지 Theta 값
            //        LBL_ROI_FINEALIGN_CURRENT_T.ForeColor = Color.Green;
            //        LBL_ROI_FINEALIGN_CURRENT_T.Text = string.Format("{0:F3}°", dGapT_degree);


            //    }
            //    else
            //    {
            //        LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //        LabelText.Color = CogColorConstants.Red;
            //        LabelText.Text = "ROI FINEALIGN SPEC OUT";
            //        //현재 이미지 Theta 값
            //        LBL_ROI_FINEALIGN_CURRENT_T.ForeColor = Color.Red;
            //        LBL_ROI_FINEALIGN_CURRENT_T.Text = string.Format("{0:F3}°", dGapT_degree);
            //    }
            //    resultGraphics.Add(LabelText);

            //}
            //else
            //{
            //    LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //    LabelText.Color = CogColorConstants.Red;
            //    if (Main.DEFINE.UNIT_TYPE == "VENT")
            //    {
            //        LabelText.X = 2000;
            //        LabelText.Y = 1000;
            //    }
            //    else if (Main.DEFINE.UNIT_TYPE == "PATH")
            //    {
            //        LabelText.X = 1800;
            //        LabelText.Y = 900;
            //    }
            //    LabelText.Text = "ROI FINEALIGN FAIL!";
            //    resultGraphics.Add(LabelText);
            //}
            //PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
        }

        private void BTN_ROI_FINEALIGN_Click(object sender, EventArgs e)
        {
            //Button TempBtn = (Button)sender;

            //if (TempBtn.Name.Equals("BTN_ROI_FINEALIGN_LEFTMARK"))
            //    nROIFineAlignIndex = (int)enumROIFineAlignPosition.Left;
            //else
            //    nROIFineAlignIndex = (int)enumROIFineAlignPosition.Right;

            //bROIFinealignTeach = true;
            //TABC_MANU.SelectedIndex = 0;
        }

        private void LBL_ROI_FINEALIGN_SPEC_T_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(LBL_ROI_FINEALIGN_SPEC_T.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 6, nCurData, "Input Data", 0);
            //KeyPad.ShowDialog();
            //double dTheta = KeyPad.m_data;

            //m_dROIFinealignT_Spec = dTheta;

            //LBL_ROI_FINEALIGN_SPEC_T.Text = dTheta.ToString();
        }

        private void CMB_USE_ROIFINEALIGN_CheckedChanged(object sender, EventArgs e)
        {
        //    m_bROIFinealignFlag = CMB_USE_ROIFINEALIGN.Checked;
        //    //2023 0228 YSH
        //    //ROI Finealign 기능사용시엔 Bonding얼라인 미사용
        //    //ROI Finealign 기능미사용시엔 Bonding얼라인 사용
        //    if (m_bROIFinealignFlag)
        //        RDB_BONDING_ALIGN.Visible = true;
        //    else
        //        RDB_BONDING_ALIGN.Visible = true;
        }

        private void BTN_LIVEMODE_Click(object sender, EventArgs e)
        {
            //if (BTN_LIVEMODE.Checked)
            //{
            //    timer1.Enabled = true;  //Live On
            //    bLiveStop = false;
            //    //PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
            //    BTN_LIVEMODE.BackColor = Color.LimeGreen;
            //}
            //else
            //{
            //    timer1.Enabled = false;  //Live Off
            //    bLiveStop = true;
            //    BTN_LIVEMODE.BackColor = Color.DarkGray;
            //}
            //DisplayClear();
            //Main.DisplayRefresh(PT_Display01);
        }

        private void CHK_ROI_CREATE_CheckedChanged(object sender, EventArgs e)
        {
            //bROICopy = CHK_ROI_CREATE.Checked;
            //if (bROICopy)
            //    for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = false;
            //else
            //    for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = true;
        }

        private string RemoveSourceCode(string input)
        {
            int lastIndex = input.LastIndexOf('\\');
            if (lastIndex != -1)
                return input.Substring(0, lastIndex);

            return input;
        }

        private void btnImagePrev_Click(object sender, EventArgs e)
        {
            //btnImagePrev.Enabled = false;
            //if (CurrentImageNumber < 0) return;
            //string[] files;
            //if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
            //{
            //    files = Directory.GetFiles(CurrentFolderPath, "*UP.jpg");
            //}
            //else
            //{
            //    files = Directory.GetFiles(CurrentFolderPath, "*.bmp");
            //}

            //if (CurrentImageNumber < files.Length)
            //{
            //    if (CurrentImageNumber != 0) CurrentImageNumber--;
            //    else
            //    {
            //        MessageBox.Show("First Image!!");
            //        btnImagePrev.Enabled = true;
            //        return;
            //    }
            //    string FileName = "";

            //    FileName = files[CurrentImageNumber];

            //    //ICogImage RefCogImage = null;
            //    if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
            //    {
            //        if (FileName != "")
            //        {
            //            if (Main.vision.CogImgTool[m_CamNo] == null)
            //                Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
            //            Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
            //            CogImageConvertTool img = new CogImageConvertTool();
            //            img.InputImage = Main.vision.CogImgTool[m_CamNo].OutputImage;
            //            img.Run();
            //            Main.vision.CogCamBuf[m_CamNo] = img.OutputImage;
            //            //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
            //        }
            //    }
            //    else
            //    {
            //        if (FileName != "")
            //        {
            //            if (Main.vision.CogImgTool[m_CamNo] == null)
            //                Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
            //            Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
            //            Main.vision.CogCamBuf[m_CamNo] = Main.vision.CogImgTool[m_CamNo].OutputImage;
            //            //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
            //        }
            //    }
            //    PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
            //    OriginImage = (CogImage8Grey)Main.vision.CogCamBuf[m_CamNo];
            //    DisplayClear();
            //    Main.DisplayRefresh(PT_Display01);
            //}

            ////검사
            //btn_Inspection_Test.PerformClick();
            //btnImagePrev.Enabled = true;
        }

        private void btnImageNext_Click(object sender, EventArgs e)
        {
            //btnImageNext.Enabled = false;
            //if (CurrentImageNumber < 0) return;
            //string[] files;
            //if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
            //{
            //    files = Directory.GetFiles(CurrentFolderPath, "*UP.jpg");
            //}
            //else
            //{
            //    files = Directory.GetFiles(CurrentFolderPath, "*.bmp");
            //}

            //if (CurrentImageNumber < files.Length - 1)
            //{
            //    CurrentImageNumber++;
            //    string FileName = "";

            //    FileName = files[CurrentImageNumber];

            //    //ICogImage RefCogImage = null;
            //    //shkang_s
            //    if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
            //    {
            //        if (FileName != "")
            //        {
            //            if (Main.vision.CogImgTool[m_CamNo] == null)
            //                Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
            //            Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
            //            CogImageConvertTool img = new CogImageConvertTool();
            //            img.InputImage = Main.vision.CogImgTool[m_CamNo].OutputImage;
            //            img.Run();
            //            Main.vision.CogCamBuf[m_CamNo] = img.OutputImage;
            //            //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
            //        }
            //    }
            //    else
            //    {
            //        if (FileName != "")
            //        {
            //            if (Main.vision.CogImgTool[m_CamNo] == null)
            //                Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
            //            Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
            //            Main.vision.CogCamBuf[m_CamNo] = Main.vision.CogImgTool[m_CamNo].OutputImage;
            //            //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
            //        }
            //    }
            //    PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
            //    OriginImage = (CogImage8Grey)Main.vision.CogCamBuf[m_CamNo];
            //    DisplayClear();
            //    Main.DisplayRefresh(PT_Display01);
            //}
            //else
            //{
            //    MessageBox.Show("Last Image!!");
            //    btnImageNext.Enabled = true;
            //    return;
            //}
            ////검사
            //btn_Inspection_Test.PerformClick();
            //btnImageNext.Enabled = true;
        }

        private void chkUseInspDirectionChange_CheckedChanged(object sender, EventArgs e)
        {
            //m_bInspDirectionChange = chkUseInspDirectionChange.Checked;
        }

        private void lblEdgeThreshold_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(lblEdgeThreshold.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //int nEdgeThreshold = (int)KeyPad.m_data;

            //lblEdgeThreshold.Text = nEdgeThreshold.ToString();
            //m_TeachParameter[m_iGridIndex].iThreshold = nEdgeThreshold;
        }

        private void chkUseEdgeThreshold_Click(object sender, EventArgs e)
        {
            //UpdateParamUI();
            //btn_Param_Apply.PerformClick();
        }

        private void Save_ChangeParaLog(string nMessage, string paraName, double oldPara, double newPara, string nType)
        {
            //string nFolder;
            //string nFileName = "";
            //nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            //if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            //if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            //string Date;
            //Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            //nMessage = nMessage + "_" + "INSPECTION" + (m_AlignNo + 1) + "_" + "PANEL" + (m_PatTagNo + 1) + "_";
            //nMessage = nMessage + paraName + "_";
            //nMessage = nMessage + oldPara + "->" + newPara;

            //lock (syncLock_Log)
            //{
            //    try
            //    {
            //        switch (nType)
            //        {
            //            case Main.DEFINE.CHANGEPARA:
            //                nFileName = "ChangeParaLog.txt";
            //                nMessage = Date + nMessage;
            //                break;
            //        }

            //        StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
            //        SW.WriteLine(nMessage);
            //        SW.Close();
            //    }
            //    catch
            //    {

            //    }
            //}
        }

        private void Save_ChangeParaLog(string nMessage, string paraName, int oldPara, int newPara, string nType)
        {
            //string nFolder;
            //string nFileName = "";
            //nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            //if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            //if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            //string Date;
            //Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            //nMessage = nMessage + "_" + "INSPECTION" + (m_AlignNo + 1) + "_" + "PANEL" + (m_PatTagNo + 1) + "_";
            //nMessage = nMessage + paraName + "_";
            //nMessage = nMessage + oldPara + "->" + newPara;

            //lock (syncLock_Log)
            //{
            //    try
            //    {
            //        switch (nType)
            //        {
            //            case Main.DEFINE.CHANGEPARA:
            //                nFileName = "ChangeParaLog.txt";
            //                nMessage = Date + nMessage;
            //                break;
            //        }

            //        StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
            //        SW.WriteLine(nMessage);
            //        SW.Close();
            //    }
            //    catch
            //    {

            //    }
            //}
        }

        private void Save_ChangeParaLog(string nMessage, double dRoiNo, string paraName, double oldPara, double newPara, string nType)
        {
            //string nFolder;
            //string nFileName = "";
            //nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            //if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            //if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            //string Date;
            //Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            //nMessage = nMessage + "_" + "INSPECTION" + (m_AlignNo + 1) + "_" + "PANEL" + (m_PatTagNo + 1) + "_";
            //nMessage = nMessage + "ROI No." + dRoiNo + "_";
            //nMessage = nMessage + paraName + "_";
            //nMessage = nMessage + oldPara + "->" + newPara;

            //lock (syncLock_Log)
            //{
            //    try
            //    {
            //        switch (nType)
            //        {
            //            case Main.DEFINE.CHANGEPARA:
            //                nFileName = "ChangeParaLog.txt";
            //                nMessage = Date + nMessage;
            //                break;
            //        }

            //        StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
            //        SW.WriteLine(nMessage);
            //        SW.Close();
            //    }
            //    catch
            //    {

            //    }
            //}
        }

        private void lblIgnoreSize_Click(object sender, EventArgs e)
        {
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblIgnoreSize.Text.ToString()), "Input Data", 0);
            //KeyPad.ShowDialog();
            //int ignoreSize = (int)KeyPad.m_data;

            //m_TeachParameter[m_iGridIndex].iIgnoreSize = ignoreSize;
            //lblIgnoreSize.Text = ignoreSize.ToString();
        }

        private void UpdateParamUI()
        {
            //if (chkUseEdgeThreshold.Checked)
            //{
            //    pnlOrgParam.Visible = false;
            //    if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
            //        pnlEdgeParam.Visible = true;
            //    else
            //        pnlEdgeParam.Visible = false;

            //    pnlParam.Controls.Clear();
            //    pnlEdgeParam.Dock = DockStyle.Fill;
            //    pnlParam.Controls.Add(pnlEdgeParam);
            //}
            //else
            //{
            //    pnlOrgParam.Visible = true;
            //    pnlEdgeParam.Visible = false;
            //    pnlParam.Controls.Clear();
            //    pnlOrgParam.Dock = DockStyle.Fill;
            //    pnlParam.Controls.Add(pnlOrgParam);
            //}
        }

        private void lblEdgeCaliperThreshold_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(lblEdgeCaliperThreshold.Text);
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            //KeyPad.ShowDialog();
            //int threshold = (int)KeyPad.m_data;
            //m_TeachParameter[m_iGridIndex].iEdgeCaliperThreshold = threshold;

            //lblEdgeCaliperThreshold.Text = threshold.ToString();
        }

        private void lblEdgeCaliperFilterSize_Click(object sender, EventArgs e)
        {
            //double nCurData = Convert.ToDouble(lblEdgeCaliperFilterSize.Text);
            //KeyPadForm KeyPad = new KeyPadForm(1, 255, nCurData, "Input Data", 2);
            //KeyPad.ShowDialog();
            //int FilterSize = (int)KeyPad.m_data;
            //m_TeachParameter[m_iGridIndex].iEdgeCaliperFilterSize = FilterSize;

            //lblEdgeCaliperFilterSize.Text = FilterSize.ToString();

        }

        private void lblTopCutPixel_Click(object sender, EventArgs e)
        {
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblTopCutPixel.Text.ToString()), "Input Data", 0);
            //KeyPad.ShowDialog();
            //int topCutPixel = (int)KeyPad.m_data;
            //m_TeachParameter[m_iGridIndex].iTopCutPixel = topCutPixel;

            //lblTopCutPixel.Text = topCutPixel.ToString();
        }

        private void lblBottomCutPixel_Click(object sender, EventArgs e)
        {
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblBottomCutPixel.Text.ToString()), "Input Data", 0);
            //KeyPad.ShowDialog();
            //int bottomCutPixel = (int)KeyPad.m_data;
            //m_TeachParameter[m_iGridIndex].iBottomCutPixel = bottomCutPixel;

            //lblBottomCutPixel.Text = bottomCutPixel.ToString();
        }

        private void lblMaskingValue_Click(object sender, EventArgs e)
        {
            //KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblMaskingValue.Text.ToString()), "Input Data", 0);
            //KeyPad.ShowDialog();
            //int maskingValue = (int)KeyPad.m_data;
            //m_TeachParameter[m_iGridIndex].iMaskingValue = maskingValue;

            //lblMaskingValue.Text = maskingValue.ToString();
        }

        private void lblParamEdgeWidthValueUpDown_Click(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            //double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
            //if (iUpdown == 0)
            //{
            //    if (dEdgeWidth == 100) return;
            //    dEdgeWidth++;
            //}
            //else
            //{
            //    if (dEdgeWidth == 1) return;
            //    dEdgeWidth--;
            //}

            //if (enumROIType.Line == m_enumROIType)
            //{
            //    m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //    m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //}
            //else
            //{
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
            //    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            //}
            //LAB_EDGE_WIDTH.Text = dEdgeWidth.ToString();
        }

        private void DrawComboboxCenterAlign(object sender, DrawItemEventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;

                if (cmb != null)
                {
                    e.DrawBackground();

                    if (cmb.Name.ToString().ToLower().Contains("group"))
                        cmb.ItemHeight = lblPolarity1.Height - 6;
                    else
                        cmb.ItemHeight = lblPolarity1.Height - 6;

                    if (e.Index >= 0)
                    {
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        sf.Alignment = StringAlignment.Center;

                        Brush brush = new SolidBrush(cmb.ForeColor);

                        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                            brush = SystemBrushes.HighlightText;

                        e.Graphics.DrawString(cmb.Items[e.Index].ToString(), cmb.Font, brush, e.Bounds, sf);
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                throw;
            }
        }

        private void Combo_Polarity_DrawItem(object sender, DrawItemEventArgs e)
        {
            DrawComboboxCenterAlign(sender, e);
        }

        private void chkUseTracking_CheckedChanged(object sender, EventArgs e)
        {
            if (_isFormLoad == false)
                return;

            TrackingTest(chkUseTracking.Checked);
        }

        private void NUD_PAT_SCORE_ValueChanged(object sender, EventArgs e)
        {

        }

        public void DisposeDisplayImage()
        {
            if(CogDisplayImage != null)
            {
                if (CogDisplayImage is CogImage8Grey grey)
                {
                    grey.Dispose();
                    grey = null;
                }
                if (CogDisplayImage is CogImage24PlanarColor color)
                {
                    color.Dispose();
                    color = null;
                }
            }
        }
    }
}

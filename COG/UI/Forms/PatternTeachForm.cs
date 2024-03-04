using COG.Class;
using COG.Class.Core;
using COG.Class.Data;
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace COG.UI.Forms
{
    public enum TabPageType
    {
        AmpMark = 0,
        BondingMark = 1,
        Inspection = 2,
    }

    public partial class PatternTeachForm : Form
    {
        private bool _isNotUpdate { get; set; } = false;

        private Algorithm Algorithm { get; set; } = new Algorithm();

        private TabPageType TabPageType = TabPageType.AmpMark;

        private bool _tabLock { get; set; } = false;

        private int _prevSelectedRowIndex { get; set; } = -1;

        private bool _selectedAmpMark { get; set; } = false;

        private bool _isSelectedBondingMarkUp { get; set; } = false;

        public bool _fixedTabControl { get; set; } = false;

        public bool _prevFixedTabControl { get; set; } = false;

        public const int ORIGIN_SIZE = 120;

        private bool _isFormLoad { get; set; } = false;

        private int _selectedMarkIndex { get; set; } = 0; // 0 : Main, 1~ : Sub

        private int _prevSelectedTabNo { get; set; } = -1;

        public int StageUnitNo { get; set; } = 0;

        public bool IsLeft { get; set; } = false;

        private double ZoomBackup { get; set; } = 0;

        private FilmROIType _selectedFileROIType { get; set; } = FilmROIType.Left_Top;

        private PatternMaskForm PatternMaskForm { get; set; } = new PatternMaskForm();

        private MessageForm MessageForm = new MessageForm(2);

        private List<CogRecordDisplay> MarkDisplayList = new List<CogRecordDisplay>();

        private List<Label> MarkLabelList = new List<Label>();

        private CogImageFileTool DisplayImageTool = new CogImageFileTool();

        private CogRecordDisplay CogDisplay = null;

        private ICogImage CogDisplayImage { get; set; } = null;

        private CogPointMarker OriginMarkPoint { get; set; } = null;

        private Unit CurrentUnit { get; set; } = null;

        public PatternTeachForm()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
       
            CogDisplay = new CogRecordDisplay();
            CogDisplay.MouseUp += new MouseEventHandler(Display_MauseUP);
            CogDisplay = PT_DISPLAY_CONTROL.CogDisplay00;
            CogDisplay.Changed += PT_Display01_Changed;

            PT_DisplayToolbar01.Display = CogDisplay;

            InitializeUI();
        }

        private void PatternTeachForm_Load(object sender, EventArgs e)
        {
            if (StaticConfig.VirtualMode)
                BTN_IMAGE_OPEN.Visible = true;

            InitializeData();
            TeachingData.Instance().UpdateTeachingData();

            InspModel inspModel = ModelManager.Instance().CurrentModel;
            if (inspModel == null)
                return;

            if (IsLeft)
                CurrentUnit = TeachingData.Instance().GetStageUnit(StageUnitNo).Left;
            else
                CurrentUnit = TeachingData.Instance().GetStageUnit(StageUnitNo).Right;

            TabPageType = TabPageType.Inspection;

            UpdateMarkInfo();
            UpdateFileAlimParam();
            DataGridview_Insp.Rows.Clear();
            UpdateInspInfo();
            UpdateInspParam();

            TABC_MANU.SelectTab(TAB_02);

            this.Text = TeachingData.Instance().GetStageUnit(StageUnitNo).Name;
        }

        private void InitializeUI()
        {
            BTN_LIVEMODE.Checked = false;
            BTN_LIVEMODE.BackColor = Color.DarkGray;

            this.TopMost = false;

            #region ComboBox
            CB_SUB_PATTERN.Items.Clear();
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
            #endregion

            #region Display
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
            #endregion
        }

        private void InitializeData()
        {
            BTN_RETURNPAGE.Visible = false;
            _selectedAmpMark = true;
            _selectedMarkIndex = 0;
            _fixedTabControl = false;
            _prevSelectedTabNo = -1;
            _prevSelectedRowIndex = -1;
            _tabLock = false;
            _isNotUpdate = false;

            ClearMarkButton();
            ClearDisplayGraphic();
        }

        private void ClearMarkButton()
        {
            BTN_PATTERN.BackColor = Color.DarkGray;
            BTN_ORIGIN.BackColor = Color.DarkGray;
            BTN_PATTERN_SEARCH_SET.BackColor = Color.DarkGray;
        }

        private void ClearDisplayGraphic()
        {
            CogDisplay.StaticGraphics.Clear();
            CogDisplay.InteractiveGraphics.Clear();
        }

        private void SetTabPageType(TabPageType type)
        {
            TabPageType = type;
        }

        private List<MarkTool> GetMarkToolList()
        {
            List<MarkTool> markToolList = null;

            if (_selectedAmpMark)
            {
                markToolList = CurrentUnit.Mark.Amp.MarkToolList;
            }
            else
            {
                if (_isSelectedBondingMarkUp)
                    markToolList = CurrentUnit.Mark.Bonding.UpMarkToolList;
                else
                    markToolList = CurrentUnit.Mark.Bonding.DownMarkToolList;
            }
            return markToolList;
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
                unit = TeachingData.Instance().GetStageUnit(StageUnitNo).Left;
            else
                unit = TeachingData.Instance().GetStageUnit(StageUnitNo).Right;

            return unit;
        }

        private CogFindLineTool GetCurrentFilmAlignTool()
        {
            var unit = GetUnit();
            var filmLineTool = unit.FilmAlign.GetTool(_selectedFileROIType);
            var lineTool = filmLineTool.FindLineTool;

            return lineTool;
        }

        private void UpdateMarkInfo()
        {
            double score = 0;
            if(_selectedAmpMark)
                score = CurrentUnit.Mark.Amp.Score;
            else
            {
                score = CurrentUnit.Mark.Bonding.Score;
                LBL_ROI_FINEALIGN_SPEC_T.Text = CurrentUnit.FilmAlign.AlignSpec_T.ToString();
                lblObjectDistanceXValue.Text = CurrentUnit.FilmAlign.AmpModuleDistanceX.ToString();
                lblObjectDistanceXSpecValue.Text = CurrentUnit.FilmAlign.FilmAlignSpecX.ToString();
            }

            if (score <= 0)
                score = 1;
            NUD_PAT_SCORE.Value = (decimal)score;

            if (_selectedMarkIndex == 0)
            {
                CB_SUBPAT_USE.Visible = false;
            }
            else
            {
                CB_SUBPAT_USE.Visible = true;
                CB_SUBPAT_USE.Checked = CurrentUnit.Mark.Use[_selectedMarkIndex];
            }

        
            #region Mark Train Image Display
            var markToolList = GetMarkToolList();
            for (int i = 0; i < StaticConfig.PATTERN_MAX_COUNT; i++)
            {
                var markTool = markToolList[i].SearchMaxTool;

                var use = CurrentUnit.Mark.Use[_selectedMarkIndex];

                if (use)
                    MarkLabelList[i].BackColor = Color.LawnGreen;
                else
                    MarkLabelList[i].BackColor = Color.WhiteSmoke;

                var display = MarkDisplayList[i];
                display.StaticGraphics.Clear();
                display.InteractiveGraphics.Clear();

                if (markTool != null)
                {
                    if (markTool.Pattern.Trained)
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
            #endregion
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

        private void BTN_PATTERN_Click(object sender, EventArgs e)
        {
            if (CogDisplayImage == null)
                return;

            ClearDisplayGraphic();

            ClearMarkButton();
            BTN_PATTERN.BackColor = Color.LawnGreen;

            PT_DISPLAY_CONTROL.CrossLine();

            var markToolList = GetMarkToolList();

            if (markToolList != null)
            {
                var tool = markToolList[_selectedMarkIndex].SearchMaxTool;
                if (tool.Pattern.Trained)
                {
                    double x = tool.Pattern.Origin.TranslationX;
                    double y = tool.Pattern.Origin.TranslationY;
                    SetOrginMark(x, y);
                }
                else
                {
                    SetNewROI();
                }

                DrawTrainRegion();
            }
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

        private void SetNewROI()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            double centerX = CogDisplayImage.Width / 2.0;
            double centerY = CogDisplayImage.Height / 2.0;

            CogRectangle roi = VisionProHelper.CreateRectangle(centerX, centerY, 100, 100);
            CogRectangle searchRoi = VisionProHelper.CreateRectangle(roi.CenterX, roi.CenterY, roi.Width * 2, roi.Height * 2);

            SetOrginMark(centerX, centerY);

            var markToolList = GetMarkToolList();

            if(markToolList != null)
            {
                var currentParam = markToolList[_selectedMarkIndex];
                currentParam?.SetTrainRegion(roi);
                currentParam.SetSearchRegion(searchRoi);
                currentParam?.SetOrginMark(OriginMarkPoint);
            }
        }

        private void DrawSearchRegion()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var markToolList = GetMarkToolList();

            if (markToolList != null)
            {
                var tool = markToolList[_selectedMarkIndex].SearchMaxTool;

                tool.InputImage = CogDisplayImage;
                tool.CurrentRecordEnable = CogSearchMaxCurrentRecordConstants.InputImage | CogSearchMaxCurrentRecordConstants.SearchRegion;

                SetInteractiveGraphics("tool", tool.CreateCurrentRecord());
            }
        }

        private void DrawTrainRegion()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var markToolList = GetMarkToolList();

            if (markToolList != null)
            {
                var tool = markToolList[_selectedMarkIndex].SearchMaxTool;

                if (tool.Pattern.Trained == false)
                {
                    tool.Pattern.TrainImage = CogDisplayImage;
                    tool.Pattern.Train();
                }

                tool.CurrentRecordEnable = CogSearchMaxCurrentRecordConstants.InputImage | CogSearchMaxCurrentRecordConstants.TrainImage
                    | CogSearchMaxCurrentRecordConstants.TrainRegion;

                SetInteractiveGraphics("tool", tool.CreateCurrentRecord());
            }
        }

        private void DrawOriginMark()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var markToolList = GetMarkToolList();

            if (markToolList != null)
            {
                var tool = markToolList[_selectedMarkIndex].SearchMaxTool;

                tool.InputImage = CogDisplayImage;
                tool.CurrentRecordEnable = CogSearchMaxCurrentRecordConstants.InputImage | CogSearchMaxCurrentRecordConstants.TrainImage
                    | CogSearchMaxCurrentRecordConstants.TrainRegion;

                SetInteractiveGraphics("tool", tool.CreateCurrentRecord());
                CogDisplay.InteractiveGraphics.Add(OriginMarkPoint, "tool", false);
            }
               
        }

        private void BTN_PATTERN_ORIGIN_Click(object sender, EventArgs e)
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var markToolList = GetMarkToolList();

            if (markToolList != null)
            {
                var currentParam = markToolList[_selectedMarkIndex];
                var trainRegion = currentParam.SearchMaxTool.Pattern.TrainRegion as CogRectangle;

                double newX = trainRegion.X + (trainRegion.Width / 2);
                double newY = trainRegion.Y + (trainRegion.Height / 2);

                SetOrginMark(newX, newY);
                currentParam?.SetOrginMark(OriginMarkPoint);

                DrawOriginMark();
            }
        }

        private void BTN_ORIGIN_Click(object sender, EventArgs e)
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            ClearDisplayGraphic();

            ClearMarkButton();
            BTN_ORIGIN.BackColor = Color.LawnGreen;

            DrawOriginMark();
        }

        private void BTN_PATTERN_SEARCH_SET_Click(object sender, EventArgs e)
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;
            ClearDisplayGraphic();

            ClearMarkButton();
            BTN_PATTERN_SEARCH_SET.BackColor = Color.LawnGreen;
            DrawSearchRegion();
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

        private void BTN_APPLY_Click(object sender, EventArgs e)
        {
        }

        private void BTN_PATTERN_DELETE_Click(object sender, EventArgs e)
        {
            if (_selectedMarkIndex < 0)
                return;

            DialogResult result = MessageBox.Show("Do you want to Delete Pattern Number: " + CB_SUB_PATTERN.Text + " ?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                var markToolList = GetMarkToolList();
                markToolList[_selectedMarkIndex].Dispose();
                markToolList[_selectedMarkIndex].SetTool(new CogSearchMaxTool());

                UpdateMarkInfo();
            }
        }

        private void CB_SUB_PATTERN_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int index = CB_SUB_PATTERN.SelectedIndex;
            if (index < 0)
                return;

            ChangeMark(index);
        }

        private void CB_SUBPAT_USE_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void BTN_PATTERN_RUN_Click(object sender, EventArgs e)
        {
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
            ClearDisplayGraphic();
            List_NG.Items.Clear();

            switch (Convert.ToInt32(TABC_MANU.SelectedTab.Tag))
            {
                case 0: // Mark
                    LoggerHelper.Save_SystemLog("Mark Search Start", LogType.Cmd);
                    RunMarkForTest();
                    break;
                case 1: // Amp Film Align
                    LoggerHelper.Save_SystemLog("Amp Film Align", LogType.Cmd);
                    RunAmpFilmAlignForTest();
                    break;
                default:
                    break;
            }
        }

        private void RunMarkForTest()
        {
            if (CogDisplayImage == null)
                return;

            PT_DISPLAY_CONTROL.CrossLine();

            double score = (double)NUD_PAT_SCORE.Value;
            MarkTool markTool = null;

            if (_selectedAmpMark)
            {
                markTool = CurrentUnit.Mark.Amp.MarkToolList[_selectedMarkIndex];
            }
            else
            {
                if (_isSelectedBondingMarkUp)
                    markTool = CurrentUnit.Mark.Bonding.UpMarkToolList[_selectedMarkIndex];
                else
                    markTool = CurrentUnit.Mark.Bonding.DownMarkToolList[_selectedMarkIndex];
            }

            LoggerHelper.Save_SystemLog("Mark Search start", LogType.Cmd);
            Stopwatch sw = Stopwatch.StartNew();

            var markResult = Algorithm.FindMark(CogDisplayImage as CogImage8Grey, markTool);

            sw.Stop();
            Lab_Tact.Text = sw.ElapsedMilliseconds.ToString() + "ms";
            LoggerHelper.Save_SystemLog($"Mark Search Tact Time : {sw.ElapsedMilliseconds}ms", LogType.Cmd);
           
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            if (markResult == null)
            {
                string message = "Mark NG! " + "Not Found Mark!!";
                LABEL_MESSAGE(LB_MESSAGE, message, System.Drawing.Color.Red);
                LABEL_MESSAGE(LB_MESSAGE1, "", Color.Lime);
            }
            else
            {
                string temp = (markResult.Score * 100).ToString("0.000");
                string foundX = markResult.FoundPos.X.ToString("0.000");
                string foundY = markResult.FoundPos.Y.ToString("0.000");

                string pointMessage = $"X: {foundX}, Y: {foundY}";
                LABEL_MESSAGE(LB_MESSAGE1, pointMessage, Color.Lime);

                LoggerHelper.Save_SystemLog(pointMessage, LogType.Data);
                LoggerHelper.Save_SystemLog("Label ", LogType.Cmd);

                DrawDisplayLabel(CogDisplay, $"Mark     X: {foundX}", 0);
                DrawDisplayLabel(CogDisplay, $"Mark     Y: {foundY}", 1);

                if (markResult.Score >= score)
                {
                    string message = $"Mark OK! Score: {temp}%";
                    LABEL_MESSAGE(LB_MESSAGE, message, Color.Lime);

                    DrawDisplayLabel(CogDisplay, $"Mark     OK! {temp}", 2);
                }
                else
                {
                    string message = $"Mark NG! Score: {temp}%";
                    LABEL_MESSAGE(LB_MESSAGE, message, Color.Red);

                    DrawDisplayLabel(CogDisplay, $"Mark     NG! {temp}", 2);
                }
                resultGraphics.Add(markResult?.ResultGraphics);
            }
            CogDisplay.InteractiveGraphics.AddList(resultGraphics, "Result", false);
        }

        private void RunAmpFilmAlignForTest()
        {
            if (CogDisplayImage == null)
                return;

            ClearDisplayGraphic();
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

            CogGraphicLabel LabelText = new CogGraphicLabel();
            LabelText.X = CogDisplay.Width / 2;
            LabelText.Y = 0;
            resultGraphics.Add(LabelText);

            LoggerHelper.Save_SystemLog("Amp Film Align start", LogType.Cmd);
            Stopwatch sw = Stopwatch.StartNew();

            var alignResult = Algorithm.RunAmpFlimAlign(CogDisplayImage as CogImage8Grey, CurrentUnit.FilmAlign);

            sw.Stop();

            Lab_Tact.Text = sw.ElapsedMilliseconds.ToString() + "ms";
            LoggerHelper.Save_SystemLog($"Amp Film Align Tact Time : {sw.ElapsedMilliseconds}ms", LogType.Cmd);

            if(alignResult.Judgement == Judgement.OK)
            {
                LabelText.Font = new Font(StaticConfig.FontStyle, 20, FontStyle.Bold);
                LabelText.Color = CogColorConstants.Green;
                LabelText.Text = string.Format("Film OK, X:{0:F3}", alignResult.GetDistanceX_mm().ToString("F3"));
                resultGraphics.Add(LabelText);
            }
            else
            {
                LabelText.Font = new Font(StaticConfig.FontStyle, 20, FontStyle.Bold);
                LabelText.Color = CogColorConstants.Red;
                LabelText.Text = string.Format("Film NG, X:{0:F3}", alignResult.GetDistanceX_mm().ToString("F3"));
                resultGraphics.Add(LabelText);
            }

            foreach (var fileResult in alignResult.FilmAlignResult)
            {
                var value = fileResult.Type.ToString().ToUpper();
                if (value.Contains("LEFT"))
                    fileResult.Line.Color = CogColorConstants.Blue;
                else
                    fileResult.Line.Color = CogColorConstants.Orange;

                CogDisplay.InteractiveGraphics.Add(fileResult.Line, "Result", false);
            }

            CogDisplay.InteractiveGraphics.AddList(resultGraphics, "Result", false);
        }

        private void DrawDisplayLabel(CogRecordDisplay display, string message, int index)
        {
            int i;
            CogGraphicLabel Label = new CogGraphicLabel();
            i = index;
            float nFontSize = 0;

            double baseZoom = 0;
            if ((double)display.Width / display.Image.Width < (double)display.Height / display.Image.Height)
            {
                baseZoom = ((double)display.Width - 22) / display.Image.Width;
                nFontSize = (float)((display.Image.Width / StaticConfig.FontSize) * baseZoom);
            }
            else
            {
                baseZoom = ((double)display.Height - 22) / display.Image.Height;
                nFontSize = (float)((display.Image.Height / StaticConfig.FontSize) * baseZoom);
            }

            double nFontpitch = (nFontSize / display.Zoom);
            Label.Text = message;
            Label.Color = CogColorConstants.Cyan;
            Label.Font = new Font(StaticConfig.FontStyle, nFontSize);
            Label.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;
            Label.X = (display.Image.Width - (display.Image.Width / (display.Zoom / baseZoom))) / 2 - display.PanX;
            Label.Y = (display.Image.Height - (display.Image.Height / (display.Zoom / baseZoom))) / 2 - display.PanY + (i * nFontpitch);


            display.StaticGraphics.Add(Label as ICogGraphic, "Result Text");
        }

        private void TABC_MANU_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            TabPageType tabPageType = (TabPageType)index;

            if (_prevSelectedTabNo == (int)tabPageType)
                return;

            if (_tabLock)
            {
                TABC_MANU.SelectTab(TAB_00);
                return;
            }

            if (tabPageType == TabPageType.Inspection)
            {

            }
            else
            {
                UpdateData();
                if (_fixedTabControl == false)
                {
                    if (tabPageType == TabPageType.AmpMark)
                    {
                        _selectedAmpMark = true;
                        BTN_RETURNPAGE.Visible = false;
                    }
                    else if (tabPageType == TabPageType.BondingMark)
                    {
                        _selectedAmpMark = false;
                        BTN_RETURNPAGE.Visible = true;
                    }
                }
                else
                {
                    if(_selectedAmpMark == false)
                        _tabLock = true;
                }

                UpdateMarkInfo();
            }

            LABEL_MESSAGE(LB_MESSAGE, "", Color.Lime);
            LABEL_MESSAGE(LB_MESSAGE1, "", Color.Lime);

            _prevSelectedTabNo = index;
        }

        private void UpdateData()
        {
            if(_prevSelectedTabNo < 0)
            {
                UpdateMarkInfo();
                return;
            }

            #region Mark
            if (_selectedAmpMark)
                CurrentUnit.Mark.Amp.Score = (double)NUD_PAT_SCORE.Value;
            else
                CurrentUnit.Mark.Bonding.Score = (double)NUD_PAT_SCORE.Value;

            if (CB_SUBPAT_USE.Visible)
                CurrentUnit.Mark.Use[_selectedMarkIndex] = CB_SUBPAT_USE.Checked;
            #endregion
        }

        #region MOVE_SIZE_LBMSSAGE
        private void BTN_MOVE_Click(object sender, EventArgs e)
        {
            double nMoveDataX = 0, nMoveDataY = 0; //공통으로 쓸수 있도록 코딩.

            int nMode = 0;
            nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            try
            {
                Button TempBTN = (Button)sender;
                switch (TempBTN.Text.ToUpper().Trim())
                {
                    case "LEFT":
                        nMoveDataX = -1;
                        nMoveDataY = 0;
                        break;

                    case "RIGHT":
                        nMoveDataX = 1;
                        nMoveDataY = 0;
                        break;

                    case "UP":
                        nMoveDataX = 0;
                        nMoveDataY = -1;
                        break;

                    case "DOWN":
                        nMoveDataX = 0;
                        nMoveDataY = 1;
                        break;
                }

                nMoveDataX /= CogDisplay.Zoom;
                nMoveDataY /= CogDisplay.Zoom;

                if (CogDisplayImage == null || CogDisplay.Image == null)
                    return;

                switch (Convert.ToInt32(TABC_MANU.SelectedTab.Tag))
                {
                    case 0: // Mark
                        MoveMark(nMoveDataX, nMoveDataY);
                        break;
                    case 1: // Amp Film Align
                        MoveFileAlign(nMoveDataX, nMoveDataY);
                        break;
                    case 2: // Insp Param
                        MoveInspParam(nMoveDataX, nMoveDataY);
                        break;
                    default:
                        break;
                }
               
            }
            catch
            {

            }
        }

        private void MoveMark(double offsetX, double offsetY)
        {
            var markToolList = GetMarkToolList();
            var searchMaxTool = markToolList[_selectedMarkIndex].SearchMaxTool;

            if(BTN_PATTERN.BackColor == Color.LawnGreen)
            {
                var trainRegion = searchMaxTool.Pattern.TrainRegion as CogRectangle;
                trainRegion.X += offsetX;
                trainRegion.Y += offsetY;
                DrawTrainRegion();
            }
            else if(BTN_PATTERN_SEARCH_SET.BackColor == Color.LawnGreen)
            {
                var searchRegion = searchMaxTool.SearchRegion as CogRectangle;
                searchRegion.X += offsetX;
                searchRegion.Y += offsetY;
                DrawSearchRegion();
            }
            else if(BTN_ORIGIN.BackColor == Color.LawnGreen)
            {
                var originPoint = searchMaxTool.Pattern.Origin;
                originPoint.TranslationX += offsetX;
                originPoint.TranslationY += offsetY;

                SetOrginMark(originPoint.TranslationX, originPoint.TranslationY);
                DrawOriginMark();
            }
        }

        private void MoveFileAlign(double offsetX, double offsetY)
        {
            var unit = GetUnit();
            if (unit == null)
                return;

            if(unit.FilmAlign.GetTool(_selectedFileROIType) is FilmAlignTool alignTool)
            {
                alignTool.FindLineTool.RunParams.ExpectedLineSegment.StartX += offsetX;
                alignTool.FindLineTool.RunParams.ExpectedLineSegment.StartY += offsetY;

                alignTool.FindLineTool.RunParams.ExpectedLineSegment.EndX += offsetX;
                alignTool.FindLineTool.RunParams.ExpectedLineSegment.EndY += offsetY;

                DrawFilmAlignLine();
            }
        }

        private void MoveInspParam(double offsetX, double offsetY)
        {
            if(GetCurrentInspParam() is GaloInspTool inspTool)
            {
                if (inspTool.Type == GaloInspType.Line)
                {
                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.StartX += offsetX;
                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.StartY += offsetY;

                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.EndX += offsetX;
                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.EndY += offsetY;
                }
                else
                {
                    double centerX = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.CenterX + offsetX;
                    double centerY = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.CenterY + offsetY;

                    double radius = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.Radius;
                    double angleStart = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.AngleStart;
                    double angleSpan = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.AngleSpan;

                    inspTool.FindCircleTool.RunParams.ExpectedCircularArc.SetCenterRadiusAngleStartAngleSpan(centerX, centerY, radius, angleStart, angleSpan);
                }
                DrawInspParam();
            }
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
            UpdateData();

            CogRecordDisplay TempNum = (CogRecordDisplay)sender;
            int index = Convert.ToInt16(TempNum.Name.Substring(TempNum.Name.Length - 2, 2));

            ChangeMark(index);
        }

        private void ChangeMark(int index)
        {
            if (_selectedMarkIndex < 0)
                return;

            var markToolList = GetMarkToolList();

            var prevMark = markToolList[_selectedMarkIndex];
            prevMark.SetOrginMark(OriginMarkPoint);

            _selectedMarkIndex = index;
            CB_SUB_PATTERN.SelectedIndex = index;

            if (index == 0)
                BTN_MAINORIGIN_COPY.Visible = false;
            else
                BTN_MAINORIGIN_COPY.Visible = true;

            UpdateMarkInfo();
            ClearDisplayGraphic();
            ClearMarkButton();
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

            CurrentUnit.Mark.Use[index] = !CurrentUnit.Mark.Use[index];
            if (_selectedMarkIndex == index)
            {
                CB_SUBPAT_USE.Visible = true;
                CB_SUBPAT_USE.Checked = CurrentUnit.Mark.Use[index];
            }
            if (CurrentUnit.Mark.Use[index])
                MarkLabelList[index].BackColor = Color.LawnGreen;
            else
                MarkLabelList[index].BackColor = Color.WhiteSmoke;
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
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var markToolList = GetMarkToolList();

            if (markToolList != null)
            {
                var markTool = markToolList[_selectedMarkIndex];
                var tool = markTool.SearchMaxTool;

                if (tool.Pattern.Trained)
                {
                    tool.InputImage = new CogImage8Grey(CogDisplayImage as CogImage8Grey);
                    PatternMaskForm.BackUpSearchMaxTool = tool;

                    PatternMaskForm.ShowDialog();

                    markTool.SetMaskingImage(PatternMaskForm.BackUpSearchMaxTool.Pattern.TrainImageMask);
                    tool.Pattern.Train();

                    UpdateMarkInfo();
                }
            }
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

        private void chkUseRoiTracking_CheckedChanged(object sender, EventArgs e)
        {
            if (_isNotUpdate)
                return;

            if (CogDisplayImage == null | CurrentUnit == null)
            {
                chkUseRoiTracking.Checked = false;
                return;
            }

            SetBondingTrackingOnOff(chkUseRoiTracking.Checked);

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

        private bool SetBondingTrackingOnOff(bool isTracking)
        {
            if (ModelManager.Instance().CurrentModel == null)
                return false;

            ClearDisplayGraphic();

            var inputImage = CogDisplay.Image as CogImage8Grey;
            var upMarkToolList = CurrentUnit.Mark.Bonding.UpMarkToolList;
            var downMarkToolList = CurrentUnit.Mark.Bonding.DownMarkToolList;

            LoggerHelper.Save_SystemLog("Mark Search start", LogType.Cmd);


            Stopwatch sw = Stopwatch.StartNew();

            double score = CurrentUnit.Mark.Bonding.Score;
            var upMarkResult = Algorithm.FindMark(inputImage, upMarkToolList, score);
            var downMarkResult = Algorithm.FindMark(inputImage, downMarkToolList, score);

            sw.Stop();

            if (upMarkResult == null || downMarkResult == null)
                return false;

            var g1 = upMarkResult.ReferencePos.X - upMarkResult.FoundPos.X;
            var g2 = downMarkResult.ReferencePos.X - downMarkResult.FoundPos.X;
            if (isTracking)
            {
                var coordinate = TeachingData.Instance().BondingCoordinate;
                coordinate.SetReferenceData(upMarkResult.ReferencePos, downMarkResult.ReferencePos);
                coordinate.SetTargetData(upMarkResult.FoundPos, downMarkResult.FoundPos);
                coordinate.ExecuteCoordinate();
            }
            else
            {
                var coordinate = TeachingData.Instance().BondingCoordinate;
                coordinate.SetReferenceData(upMarkResult.FoundPos, downMarkResult.FoundPos);
                coordinate.SetTargetData(upMarkResult.ReferencePos, downMarkResult.ReferencePos);
                coordinate.ExecuteCoordinate();
            }
            SetTrackingInspParam();
            DrawInspParam();

            return true;
        }

        private void SetTrackingInspParam()
        {
            var unit = GetUnit();
            var coordinate = TeachingData.Instance().BondingCoordinate;

            foreach (var inspTool in unit.Insp.GaloInspToolList)
            {
                if (inspTool.Type == GaloInspType.Line)
                {
                    float prevStartX = (float)inspTool.FindLineTool.RunParams.ExpectedLineSegment.StartX;
                    float prevStartY = (float)inspTool.FindLineTool.RunParams.ExpectedLineSegment.StartY;

                    PointF coordStartPoint = coordinate.GetCoordinate(new PointF(prevStartX, prevStartY));
                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.StartX = coordStartPoint.X;
                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.StartY = coordStartPoint.Y;

                    float prevEndX = (float)inspTool.FindLineTool.RunParams.ExpectedLineSegment.EndX;
                    float prevEndY = (float)inspTool.FindLineTool.RunParams.ExpectedLineSegment.EndY;

                    PointF coordEndPoint = coordinate.GetCoordinate(new PointF(prevEndX, prevEndY));
                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.EndX = coordEndPoint.X;
                    inspTool.FindLineTool.RunParams.ExpectedLineSegment.EndY = coordEndPoint.Y;
                }
                else
                {
                    float prevCenterX = (float)inspTool.FindCircleTool.RunParams.ExpectedCircularArc.CenterX;
                    float prevCenterY = (float)inspTool.FindCircleTool.RunParams.ExpectedCircularArc.CenterY;

                    PointF coordCenterPoint = coordinate.GetCoordinate(new PointF(prevCenterX, prevCenterY));
                    double radius = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.Radius;
                    double angleStart = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.AngleStart;
                    double angleSpan = inspTool.FindCircleTool.RunParams.ExpectedCircularArc.AngleSpan;

                    inspTool.FindCircleTool.RunParams.ExpectedCircularArc.SetCenterRadiusAngleStartAngleSpan(coordCenterPoint.X, coordCenterPoint.Y, radius, angleStart, angleSpan);
                }
            }
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
            if (_prevSelectedRowIndex < 0)
                return;
            if (GetUnit() is Unit unit)
            {
                if (MessageBox.Show("Are you sure you want to delete it?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    unit.Insp.GaloInspToolList.RemoveAt(_prevSelectedRowIndex);
                    DataGridview_Insp.Rows.Clear();
                    DataGridview_Insp.ClearSelection();
                    DataGridview_Insp.CurrentCell = null;

                    _prevSelectedRowIndex = -1;
                    UpdateInspInfo(unit.Insp.GaloInspToolList.Count);
                    UpdateInspParam();
                }
                else
                {
                    return;
                }
            }
        }
      

        private void DataGridview_Insp_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            _prevSelectedRowIndex = e.RowIndex;
            UpdateInspParam();
            DrawInspParam();
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
      
        #endregion

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

        private void btnAlignInspPos(object sender, EventArgs e)
        {
            RadioButton btn = (RadioButton)sender;

            ClearDisplayGraphic();

            int alignNo = Convert.ToInt32(btn.Tag.ToString());
            _selectedFileROIType = (FilmROIType)alignNo;

            switch (alignNo)
            {
                case (int)FilmROIType.Left_Top:
                    btn_TOP_Inscription.BackColor = Color.Green;
                    btn_Top_Circumcription.BackColor = Color.DarkGray;
                    btn_Bottom_Inscription.BackColor = Color.DarkGray;
                    btn_Bottom_Circumcription.BackColor = Color.DarkGray;
                    break;

                case (int)FilmROIType.Left_Side:
                    btn_TOP_Inscription.BackColor = Color.DarkGray;
                    btn_Top_Circumcription.BackColor = Color.Green;
                    btn_Bottom_Inscription.BackColor = Color.DarkGray;
                    btn_Bottom_Circumcription.BackColor = Color.DarkGray;
                    break;

                case (int)FilmROIType.Right_Top:
                    btn_TOP_Inscription.BackColor = Color.DarkGray;
                    btn_Top_Circumcription.BackColor = Color.DarkGray;
                    btn_Bottom_Inscription.BackColor = Color.Green;
                    btn_Bottom_Circumcription.BackColor = Color.DarkGray;
                    break;

                case (int)FilmROIType.Right_Side:
                    btn_TOP_Inscription.BackColor = Color.DarkGray;
                    btn_Top_Circumcription.BackColor = Color.DarkGray;
                    btn_Bottom_Inscription.BackColor = Color.DarkGray;
                    btn_Bottom_Circumcription.BackColor = Color.Green;
                    break;

            }

            UpdateFileAlimParam();
            DrawFilmAlignLine();
        }

        private void UpdateFileAlimParam()
        {
            var unit = GetUnit();
            var filmLineTool = unit.FilmAlign.GetTool(_selectedFileROIType);
            var lineTool = filmLineTool.FindLineTool;

            LAB_Align_Threshold.Text = lineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            LAB_Align_Caliper_Cnt.Text = lineTool.RunParams.NumCalipers.ToString();
            LAB_Align_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", lineTool.RunParams.CaliperProjectionLength);
            LAB_Align_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", lineTool.RunParams.CaliperSearchDirection);
            lab_Ignore.Text = lineTool.RunParams.NumToIgnore.ToString();
            int nPolarity = (int)lineTool.RunParams.CaliperRunParams.Edge0Polarity;
            Combo_FilAlign_Polarity.SelectedIndex = nPolarity - 1;
            lblThetaFilterSizeValue.Text = lineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
        }

        private void DrawFilmAlignLine()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            var unit = GetUnit();
            var filmLineTool = unit.FilmAlign.GetTool(_selectedFileROIType);
            var tool = filmLineTool.FindLineTool;
            tool.InputImage = CogDisplayImage as CogImage8Grey;
            tool.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage | CogFindLineCurrentRecordConstants.CaliperRegions | CogFindLineCurrentRecordConstants.ExpectedLineSegment |
                                                       CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;

            SetInteractiveGraphics("tool", tool.CreateCurrentRecord());
        }

        private void UpdateInspInfo(int count = -1)
        {
            var unit = GetUnit();
            var inspTool = unit.Insp.GaloInspToolList;

            int inspCount = 0;
            if (count == -1)
                inspCount = unit.Insp.Count;
            else
                inspCount = count;


            for (int i = 0; i < inspCount; i++)
            {
                var tool = inspTool[i];

                List<string> rowDataList = new List<string>();
                rowDataList.Add(i.ToString());

                if(tool.Type == GaloInspType.Line)
                {
                    var runParam = tool.FindLineTool.RunParams;

                    rowDataList.Add("Line");
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedLineSegment.StartX));
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedLineSegment.StartY));
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedLineSegment.EndX));
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedLineSegment.EndY));
                    rowDataList.Add(string.Format("{0:F3}", 0));
                    rowDataList.Add(string.Format("{0:F3}", 0));

                    rowDataList.Add(runParam.CaliperRunParams.ContrastThreshold.ToString());
                    rowDataList.Add(runParam.NumCalipers.ToString());
                    rowDataList.Add(string.Format("{0:F3}", runParam.CaliperProjectionLength));
                    rowDataList.Add(string.Format("{0:F3}", runParam.CaliperSearchLength));
                    rowDataList.Add(((int)runParam.CaliperRunParams.Edge0Polarity).ToString());
                    rowDataList.Add(((int)runParam.CaliperRunParams.Edge1Polarity).ToString());
                    rowDataList.Add(string.Format("{0:F3}", runParam.CaliperRunParams.Edge0Position * 2));
                    rowDataList.Add(tool.Distgnore.ToString());
                    rowDataList.Add(string.Format("{0:F2}", tool.SpecDistance));
                    rowDataList.Add(runParam.CaliperRunParams.FilterHalfSizeInPixels.ToString());
                    rowDataList.Add(string.Format("{0:F2}", tool.SpecDistanceMax));
                    rowDataList.Add(tool.DarkArea.ThresholdUse.ToString());
                    rowDataList.Add(tool.DarkArea.Threshold.ToString());
                }
                else
                {
                    var runParam = tool.FindCircleTool.RunParams;
                    rowDataList.Add("Circle");
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedCircularArc.CenterX));
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedCircularArc.CenterY));
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedCircularArc.Radius));
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedCircularArc.AngleStart));
                    rowDataList.Add(string.Format("{0:F3}", runParam.ExpectedCircularArc.AngleSpan));
                    rowDataList.Add(string.Format("{0:F3}", 0));
                    rowDataList.Add(runParam.CaliperRunParams.ContrastThreshold.ToString());
                    rowDataList.Add(runParam.NumCalipers.ToString());
                    rowDataList.Add(string.Format("{0:F3}", runParam.CaliperProjectionLength));
                    rowDataList.Add(string.Format("{0:F3}", runParam.CaliperSearchLength));
                    rowDataList.Add(((int)runParam.CaliperRunParams.Edge0Polarity).ToString());
                    rowDataList.Add(((int)runParam.CaliperRunParams.Edge1Polarity).ToString());
                    rowDataList.Add(string.Format("{0:F3}", runParam.CaliperRunParams.Edge0Position * 2));
                    rowDataList.Add(tool.Distgnore.ToString());
                    rowDataList.Add(string.Format("{0:F2}", tool.SpecDistance));
                    rowDataList.Add(runParam.CaliperRunParams.FilterHalfSizeInPixels.ToString());
                    rowDataList.Add(string.Format("{0:F2}", tool.SpecDistanceMax));
                    rowDataList.Add(tool.DarkArea.ThresholdUse.ToString());
                    rowDataList.Add(tool.DarkArea.Threshold.ToString());
                }
                DataGridview_Insp.Rows.Add(rowDataList.ToArray());
            }
            if (DataGridview_Insp.Rows.Count > 0)
                _prevSelectedRowIndex = 0;
            else
                _prevSelectedRowIndex = -1;

            UpdateParamUI();
        }

        private void UpdateInspParam()
        {
            if (_prevSelectedRowIndex < 0)
                return;

            var param = GetUnit().Insp.GaloInspToolList[_prevSelectedRowIndex];

            chkUseEdgeThreshold.Checked = param.DarkArea.ThresholdUse;
            lblEdgeThreshold.Text = param.DarkArea.Threshold.ToString();
            lblEdgeCaliperThreshold.Text = param.DarkArea.EdgeCaliperThreshold.ToString();
            lblEdgeCaliperFilterSize.Text = param.DarkArea.EdgeCaliperFilterSize.ToString();
            lblTopCutPixel.Text = param.DarkArea.TopCutPixel.ToString();
            lblBottomCutPixel.Text = param.DarkArea.BottomCutPixel.ToString();
            lblMaskingValue.Text = param.DarkArea.MaskingValue.ToString();
            lblIgnoreSize.Text = param.DarkArea.IgnoreSize.ToString();

            text_Dist_Ignre.Text = param.Distgnore.ToString();
            text_Spec_Dist.Text = param.SpecDistance.ToString();
            text_Spec_Dist_Max.Text = param.SpecDistanceMax.ToString();

            CogCaliperPolarityConstants Polarity;
            int TmepIndex = 0;
            if (param.Type == GaloInspType.Line)
            {
                LAB_Insp_Threshold.Text = param.FindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = param.FindLineTool.RunParams.NumCalipers.ToString();
                lblParamFilterSizeValue.Text = param.FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", param.FindLineTool.RunParams.CaliperProjectionLength);
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", param.FindLineTool.RunParams.CaliperSearchLength);
                param.FindLineTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;

                Polarity = param.FindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
                Combo_Polarity1.SelectedIndex = (int)Polarity - 1;

                Polarity = param.FindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
                Combo_Polarity2.SelectedIndex = (int)Polarity - 1;

                label59.Visible = false;
                LAB_EDGE_WIDTH.Visible = false;
                lblParamEdgeWidthValueUp.Visible = false;
                lblParamEdgeWidthValueDown.Visible = false;
                label58.Visible = false;
                Combo_Polarity2.Visible = false;
            }
            else
            {
                LAB_Insp_Threshold.Text = param.FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = param.FindCircleTool.RunParams.NumCalipers.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", param.FindCircleTool.RunParams.CaliperProjectionLength);
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", param.FindCircleTool.RunParams.CaliperSearchLength);
                lblParamFilterSizeValue.Text = param.FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();

                double dEdgeWidth =Convert.ToDouble(DataGridview_Insp.Rows[_prevSelectedTabNo].Cells[14].Value);
                LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));

                param.FindCircleTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;

                Polarity = param.FindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
                Combo_Polarity1.SelectedIndex = (int)Polarity - 1;

                Polarity = param.FindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
                Combo_Polarity2.SelectedIndex = (int)Polarity - 1;

                label59.Visible = true;
                LAB_EDGE_WIDTH.Visible = true;
                lblParamEdgeWidthValueUp.Visible = true;
                lblParamEdgeWidthValueDown.Visible = true;
                label58.Visible = true;
                Combo_Polarity2.Visible = true;
            }
            UpdateParamUI();
        }

        private void DrawInspParam()
        {
            if (CogDisplayImage == null || CogDisplay.Image == null)
                return;

            ClearDisplayGraphic();
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                if(inspTool.Type == GaloInspType.Line)
                {
                    inspTool.FindLineTool.InputImage = CogDisplayImage as CogImage8Grey;
                    inspTool.FindLineTool.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage 
                                                                | CogFindLineCurrentRecordConstants.CaliperRegions 
                                                                | CogFindLineCurrentRecordConstants.ExpectedLineSegment 
                                                                | CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection 
                                                                | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;
                    SetInteractiveGraphics("tool", inspTool.FindLineTool.CreateCurrentRecord());
                }
                else
                {
                    inspTool.FindCircleTool.InputImage = CogDisplayImage as CogImage8Grey;
                    inspTool.FindCircleTool.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.InputImage 
                                                                | CogFindCircleCurrentRecordConstants.CaliperRegions 
                                                                | CogFindCircleCurrentRecordConstants.ExpectedCircularArc 
                                                                | CogFindCircleCurrentRecordConstants.InteractiveCaliperSize;
                    SetInteractiveGraphics("tool", inspTool.FindCircleTool.CreateCurrentRecord());
                }
            }
        }

        private GaloInspTool GetCurrentInspParam()
        {
            if (DataGridview_Insp.SelectedRows.Count <= 0)
                return null;

            var param = GetUnit().Insp.GaloInspToolList[_prevSelectedRowIndex];

            return param;
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

        private void btn_FilmAlignTest_Click(object sender, EventArgs e)
        {
            RunAmpFilmAlignForTest();
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
            UpdateData();

            _fixedTabControl = false;
            _tabLock = false;
            TABC_MANU.SelectTab(TAB_01);
            _isFormLoad = true;

            RDB_ROI_FINEALIGN.PerformClick();
        }

        private void BTN_ROI_FINEALIGN_TEST_Click(object sender, EventArgs e)
        {
            if (ModelManager.Instance().CurrentModel == null)
                return;

            var inputImage = CogDisplay.Image;

            double score = CurrentUnit.Mark.Bonding.Score;

            ClearDisplayGraphic();
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

            CogGraphicLabel LabelText = new CogGraphicLabel();
            LabelText.X = CogDisplay.Width /2;
            LabelText.Y = 0;
            resultGraphics.Add(LabelText);

            var upMarkToolList = CurrentUnit.Mark.Bonding.UpMarkToolList;
            var downMarkToolList = CurrentUnit.Mark.Bonding.DownMarkToolList;

            var reuslt = Algorithm.FindBondingMark(inputImage as CogImage8Grey, upMarkToolList, downMarkToolList, score, CurrentUnit.FilmAlign.AlignSpec_T);

            if(reuslt.Judgement == Judgement.OK)
            {
                resultGraphics.Add(reuslt.UpMarkResult?.ResultGraphics);
                resultGraphics.Add(reuslt.DownMarkResult?.ResultGraphics);

                LBL_ROI_FINEALIGN_CURRENT_T.ForeColor = Color.Green;
                LBL_ROI_FINEALIGN_CURRENT_T.Text = string.Format("{0:F3}°", reuslt.FoundDegree);

                LabelText.Font = new Font(StaticConfig.FontStyle, 20, FontStyle.Bold);
                LabelText.Color = CogColorConstants.Green;
                LabelText.Text = "ROI FINEALIGN OK";
                resultGraphics.Add(LabelText);
            }
            else
            {
                //검사 실패
                LBL_ROI_FINEALIGN_CURRENT_T.ForeColor = Color.Red;
                LBL_ROI_FINEALIGN_CURRENT_T.Text = string.Format("{0:F3}°", reuslt.FoundDegree);

                LabelText.Font = new Font(StaticConfig.FontStyle, 20, FontStyle.Bold);
                LabelText.Color = CogColorConstants.Red;
                LabelText.Text = "ROI FINEALIGN SPEC OUT";
            }
            CogDisplay.InteractiveGraphics.AddList(resultGraphics, "Result", false);
        }

        private void BTN_ROI_FINEALIGN_Click(object sender, EventArgs e)
        {
            Button TempBtn = (Button)sender;

            if (TempBtn.Name.Equals("BTN_ROI_FINEALIGN_LEFTMARK"))
                _isSelectedBondingMarkUp = true;
            else
                _isSelectedBondingMarkUp = false;

            _selectedAmpMark = false;

            _fixedTabControl = true;
            TABC_MANU.SelectedIndex = 0;
        }

        private void LBL_ROI_FINEALIGN_SPEC_T_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LBL_ROI_FINEALIGN_SPEC_T.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 6, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();

            double dTheta = KeyPad.m_data;

            CurrentUnit.FilmAlign.AlignSpec_T = dTheta;
            LBL_ROI_FINEALIGN_SPEC_T.Text = dTheta.ToString();
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
      
        private void chkUseEdgeThreshold_Click(object sender, EventArgs e)
        {
            UpdateParamUI();
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

        private void UpdateParamUI()
        {
            if (chkUseEdgeThreshold.Checked)
            {
                pnlOrgParam.Visible = false;
                if (AppsStatus.Instance().CurrentUser == User.MAKER)
                    pnlEdgeParam.Visible = true;
                else
                    pnlEdgeParam.Visible = false;

                pnlParam.Controls.Clear();
                pnlEdgeParam.Dock = DockStyle.Fill;
                pnlParam.Controls.Add(pnlEdgeParam);
            }
            else
            {
                pnlOrgParam.Visible = true;
                pnlEdgeParam.Visible = false;
                pnlParam.Controls.Clear();
                pnlOrgParam.Dock = DockStyle.Fill;
                pnlParam.Controls.Add(pnlOrgParam);
            }
        }

        private void chkUseTracking_CheckedChanged(object sender, EventArgs e)
        {
            if (_isNotUpdate)
                return;

            if (CogDisplayImage == null | CurrentUnit == null)
            {
                chkUseTracking.Checked = false;
                return;
            }

            SetAmpTrackingOnOff(chkUseTracking.Checked);
        }

        private bool SetAmpTrackingOnOff(bool isTracking)
        {
            double score = (double)NUD_PAT_SCORE.Value;
            MarkTool markTool = markTool = CurrentUnit.Mark.Amp.MarkToolList[_selectedMarkIndex];

            ClearDisplayGraphic();

            LoggerHelper.Save_SystemLog("Mark Search start", LogType.Cmd);

            Stopwatch sw = Stopwatch.StartNew();

            var markResult = Algorithm.FindMark(CogDisplayImage as CogImage8Grey, markTool);

            sw.Stop();

            if (markResult == null)
                return false;

            if(isTracking)
            {
                var coordinate = TeachingData.Instance().AmpCoordinate;
                coordinate.SetReferenceData(markResult.ReferencePos);
                coordinate.SetTargetData(markResult.FoundPos);
                coordinate.ExecuteCoordinate(CurrentUnit);

                DrawFilmAlignLine();
            }
            else
            {
                var coordinate = TeachingData.Instance().AmpCoordinate;
                coordinate.SetReferenceData(markResult.FoundPos);
                coordinate.SetTargetData(markResult.ReferencePos);
                coordinate.ExecuteCoordinate(CurrentUnit);

                DrawFilmAlignLine();
            }

            return true;
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

        #region File Align 값 변경 버튼
        private void lblObjectDistanceXSpecValue_Click(object sender, EventArgs e)
        {
            double curData = Convert.ToDouble(lblObjectDistanceXSpecValue.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 3000, curData, "Input Data", 0);
            KeyPad.ShowDialog();
            double specX = KeyPad.m_data;

            CurrentUnit.FilmAlign.FilmAlignSpecX = specX;
            lblObjectDistanceXSpecValue.Text = specX.ToString();
        }

        private void lblObjectDistanceXValue_Click(object sender, EventArgs e)
        {
            double curData = Convert.ToDouble(lblObjectDistanceXValue.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 3000, curData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dDistanceX = KeyPad.m_data;

            CurrentUnit.FilmAlign.AmpModuleDistanceX = dDistanceX;
            lblObjectDistanceXValue.Text = dDistanceX.ToString();
        }

        private void LAB_Align_Threshold_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_Threshold.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double dThreshold = KeyPad.m_data;

            GetCurrentFilmAlignTool().RunParams.CaliperRunParams.ContrastThreshold = dThreshold;

            LAB_Align_Threshold.Text = ((int)dThreshold).ToString();
        }

        private void Align_Threshold(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dThr = Convert.ToDouble(LAB_Align_Threshold.Text);
            if (iUpdown == 0)
            {
                if (dThr == 255)
                    return;
                dThr++;
            }
            else
            {
                if (dThr == 1)
                    return;
                dThr--;
            }
            LAB_Align_Threshold.Text = dThr.ToString();
            GetCurrentFilmAlignTool().RunParams.CaliperRunParams.ContrastThreshold = dThr;
        }

        private void LAB_Align_Caliper_Cnt_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_Caliper_Cnt.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int CaliperCnt = (int)KeyPad.m_data;

            GetCurrentFilmAlignTool().RunParams.NumCalipers = CaliperCnt;
            DrawFilmAlignLine();

            LAB_Align_Caliper_Cnt.Text = CaliperCnt.ToString();
        }

        private void Align_CaliperCnt(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            int iCaliperCnt = Convert.ToInt32(LAB_Align_Caliper_Cnt.Text);
            if (iUpdown == 0)
            {
                iCaliperCnt++;
            }
            else
            {
                if (iCaliperCnt == 1)
                    return;
                iCaliperCnt--;
            }
            LAB_Align_Caliper_Cnt.Text = iCaliperCnt.ToString();
            GetCurrentFilmAlignTool().RunParams.NumCalipers = iCaliperCnt;
        }

        private void lblThetaFilterSizeValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblThetaFilterSizeValue.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int dFilterSize = Convert.ToInt32(KeyPad.m_data);

            lblThetaFilterSizeValue.Text = Convert.ToString(dFilterSize);
            GetCurrentFilmAlignTool().RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
        }

        private void lblThetaFilterSizeValueUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dFilterSize = Convert.ToDouble(lblThetaFilterSizeValue.Text);
            if (iUpdown == 0)
            {
                if (dFilterSize == 255)
                    return;
                dFilterSize++;
            }
            else
            {
                if (dFilterSize == 2)
                    return;
                dFilterSize--;
            }
            lblThetaFilterSizeValue.Text = dFilterSize.ToString();
            GetCurrentFilmAlignTool().RunParams.CaliperRunParams.ContrastThreshold = dFilterSize;
        }

        private void lab_Ignore_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_Caliper_Cnt.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int iIgnoreCnt = (int)KeyPad.m_data;

            GetCurrentFilmAlignTool().RunParams.NumToIgnore = iIgnoreCnt;
            DrawFilmAlignLine();

            lab_Ignore.Text = iIgnoreCnt.ToString();
        }

        private void lblIgnoreValueUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            int ignore = Convert.ToInt32(lab_Ignore.Text);
            if (iUpdown == 0)
            {
                if (ignore == 255)
                    return;
                ignore++;
            }
            else
            {
                if (ignore == 0)
                    return;
                ignore--;
            }
            lab_Ignore.Text = ignore.ToString();
            GetCurrentFilmAlignTool().RunParams.NumToIgnore = ignore;
        }

        private void LAB_Align_CALIPER_PROJECTIONLENTH_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_CALIPER_PROJECTIONLENTH.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double CaliperProjectionLenth = KeyPad.m_data;

            GetCurrentFilmAlignTool().RunParams.CaliperSearchLength = CaliperProjectionLenth;

            LAB_Align_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", CaliperProjectionLenth);
        }

        private void Align_ProjectionLenth(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dProjectionLenth = Convert.ToDouble(LAB_Align_CALIPER_PROJECTIONLENTH.Text);
            if (iUpdown == 0)
            {
                dProjectionLenth++;
            }
            else
            {
                if (dProjectionLenth == 1)
                    return;
                dProjectionLenth--;
            }
            LAB_Align_CALIPER_PROJECTIONLENTH.Text = dProjectionLenth.ToString();
            GetCurrentFilmAlignTool().RunParams.CaliperProjectionLength = dProjectionLenth;
        }

        private void LAB_Align_CALIPER_SEARCHLENTH_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_CALIPER_SEARCHLENTH.Text);
            KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double CaliperSearchLenth = KeyPad.m_data;

            GetCurrentFilmAlignTool().RunParams.CaliperSearchLength = CaliperSearchLenth;

            LAB_Align_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", CaliperSearchLenth);
        }

        private void Align_Length_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double length = Convert.ToDouble(LAB_Align_CALIPER_SEARCHLENTH.Text);
            if (iUpdown == 0)
            {
                length++;
            }
            else
            {
                if (length == 1)
                    return;
                length--;
            }
            LAB_Align_CALIPER_SEARCHLENTH.Text = length.ToString();
            GetCurrentFilmAlignTool().RunParams.CaliperSearchLength = length;
        }

        private void Combo_FilAlign_Polarity_SelectedIndexChanged(object sender, EventArgs e)
        {
            int TempIndex = Combo_FilAlign_Polarity.SelectedIndex;
            GetCurrentFilmAlignTool().RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
        }
        #endregion

        #region InspParam 값 변경
        private void LAB_Insp_Threshold_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double curData = Convert.ToDouble(LAB_Insp_Threshold.Text);
                KeyPadForm KeyPad = new KeyPadForm(0, 255, curData, "Input Data", 1);
                KeyPad.ShowDialog();
                double threshold = KeyPad.m_data;

                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperRunParams.ContrastThreshold = threshold;
                else
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold = threshold;

                LAB_Insp_Threshold.Text = ((int)threshold).ToString();
            }
        }

        private void Insp_Threshold(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double iThreshold = Convert.ToDouble(LAB_Insp_Threshold.Text);
                if (Convert.ToInt32(btn.Tag.ToString()) == 0)
                    iThreshold++;
                else
                {
                    if (iThreshold < 0)
                        return;
                    iThreshold--;
                }

                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperRunParams.ContrastThreshold = iThreshold;
                else
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold = iThreshold;
                LAB_Insp_Threshold.Text = iThreshold.ToString();
            }
        }

        private void LAB_Caliper_Cnt_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double curData = Convert.ToDouble(LAB_Caliper_Cnt.Text);
                KeyPadForm KeyPad = new KeyPadForm(2, 255, curData, "Input Data", 1);
                KeyPad.ShowDialog();
                int CaliperCnt = (int)KeyPad.m_data;
                if (inspTool.Type == GaloInspType.Line)
                {
                    inspTool.FindLineTool.RunParams.NumCalipers = CaliperCnt;
                }
                else
                {
                    inspTool.FindCircleTool.RunParams.NumCalipers = CaliperCnt;
                }
                LAB_Caliper_Cnt.Text = CaliperCnt.ToString();

                DrawInspParam();
            }
        }

        private void Caliper_Count(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                Button btn = (Button)sender;
                int iCaliperCnt = Convert.ToInt32(LAB_Caliper_Cnt.Text);
                if (Convert.ToInt32(btn.Tag.ToString()) == 0)
                {
                    //Up
                    iCaliperCnt++;
                }
                else
                {
                    //Down
                    if (iCaliperCnt == 1)
                        return;
                    iCaliperCnt--;
                }
                if (inspTool.Type == GaloInspType.Line)
                {
                    inspTool.FindLineTool.RunParams.NumCalipers = iCaliperCnt;
                }
                else
                {
                    inspTool.FindCircleTool.RunParams.NumCalipers = iCaliperCnt;
                }

                LAB_Caliper_Cnt.Text = iCaliperCnt.ToString();
                DrawInspParam();
            }
        }

        private void lblParamFilterSizeValue_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(lblParamFilterSizeValue.Text);
                KeyPadForm KeyPad = new KeyPadForm(1, 255, nCurData, "Input Data", 2);
                KeyPad.ShowDialog();
                int FilterSize = (int)KeyPad.m_data;

                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FilterSize;
                else
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FilterSize;

                lblParamFilterSizeValue.Text = FilterSize.ToString();

                DrawInspParam();
            }
        }

        private void lblParamFilterSizeValueUpDown_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                Button btn = (Button)sender;
                int iUpdown = Convert.ToInt32(btn.Tag.ToString());
                int dFilterSize = Convert.ToInt32(lblParamFilterSizeValue.Text);
                if (iUpdown == 0)
                {
                    if (dFilterSize == 255)
                        return;
                    dFilterSize++;
                }
                else
                {
                    if (dFilterSize == 2)
                        return;
                    dFilterSize--;
                }
                lblParamFilterSizeValue.Text = dFilterSize.ToString();
                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
                else
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;

                DrawInspParam();
            }
        }

        private void LAB_CALIPER_PROJECTIONLENTH_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(LAB_CALIPER_PROJECTIONLENTH.Text);
                KeyPadForm KeyPad = new KeyPadForm(1, 255, nCurData, "Input Data", 1);
                KeyPad.ShowDialog();
                double CaliperProjectionLenth = KeyPad.m_data;
                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperSearchLength = CaliperProjectionLenth;
                else
                    inspTool.FindCircleTool.RunParams.CaliperSearchLength = CaliperProjectionLenth;

                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", CaliperProjectionLenth);
                DrawInspParam();
            }
        }

        private void Caliper_ProjectionLenth(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                Button btn = (Button)sender;
                double iProjectionLenth = Convert.ToDouble(LAB_CALIPER_PROJECTIONLENTH.Text);
                if (Convert.ToInt32(btn.Tag.ToString()) == 0)
                {
                    //Up
                    iProjectionLenth++;
                }
                else
                {
                    //Down
                    if (iProjectionLenth == 1)
                        return;
                    iProjectionLenth--;
                }
                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperProjectionLength = iProjectionLenth;
                else
                    inspTool.FindCircleTool.RunParams.CaliperProjectionLength = iProjectionLenth;

                LAB_CALIPER_PROJECTIONLENTH.Text = iProjectionLenth.ToString();
            }
        }

        private void LAB_CALIPER_SEARCHLENTH_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(LAB_CALIPER_SEARCHLENTH.Text);
                KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
                KeyPad.ShowDialog();
                double CaliperSearchLenth = KeyPad.m_data;
                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperSearchLength = CaliperSearchLenth;
                else
                    inspTool.FindCircleTool.RunParams.CaliperSearchLength = CaliperSearchLenth;

                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", CaliperSearchLenth);
            }
        }

        private void Insp_SearchLenth(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                Button btn = (Button)sender;
                double iSearchLenth = Convert.ToDouble(LAB_CALIPER_SEARCHLENTH.Text);
                if (Convert.ToInt32(btn.Tag.ToString()) == 0)
                {
                    //Up
                    iSearchLenth++;
                }
                else
                {
                    //Down
                    if (iSearchLenth < 1)
                        return;
                    iSearchLenth--;
                }
                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperSearchLength = iSearchLenth;
                else
                    inspTool.FindCircleTool.RunParams.CaliperSearchLength = iSearchLenth;
                LAB_CALIPER_SEARCHLENTH.Text = iSearchLenth.ToString();
            }
        }

        private void LAB_EDGE_WIDTH_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
                KeyPadForm KeyPad = new KeyPadForm(1, 100, nCurData, "Input Data", 1);
                KeyPad.ShowDialog();
                double dEdgeWidth = KeyPad.m_data;

                if (inspTool.Type == GaloInspType.Line)
                {
                    inspTool.FindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                    inspTool.FindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                }
                else
                {
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                }

                LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", dEdgeWidth);
            }
        }

        private void lblParamEdgeWidthValueUpDown_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                Button btn = (Button)sender;
                int iUpdown = Convert.ToInt32(btn.Tag.ToString());
                double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
                if (iUpdown == 0)
                {
                    if (dEdgeWidth == 100)
                        return;
                    dEdgeWidth++;
                }
                else
                {
                    if (dEdgeWidth == 1)
                        return;
                    dEdgeWidth--;
                }

                if (inspTool.Type == GaloInspType.Line)
                {
                    inspTool.FindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                    inspTool.FindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                }
                else
                {
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                }
                LAB_EDGE_WIDTH.Text = dEdgeWidth.ToString();
            }
        }

        private void text_Dist_Ignre_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(text_Dist_Ignre.Text);
                KeyPadForm KeyPad = new KeyPadForm(0, 100, nCurData, "Input Data", 0);
                KeyPad.ShowDialog();
                int iIgnoreData = (int)KeyPad.m_data;
                inspTool.Distgnore = iIgnoreData;
                text_Dist_Ignre.Text = iIgnoreData.ToString();
            }
        }

        private void Ignore_Distance(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                Button btn = (Button)sender;
                int dIgnoredist = Convert.ToInt32(text_Dist_Ignre.Text);
                if (Convert.ToInt32(btn.Tag.ToString()) == 0)
                {
                    //Up
                    dIgnoredist++;
                }
                else
                {
                    //Down
                    if (dIgnoredist < 0)
                        return;
                    dIgnoredist--;
                }
                inspTool.Distgnore = dIgnoredist;
                text_Dist_Ignre.Text = dIgnoredist.ToString();
            }
        }

        private void text_Spec_Dist_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(text_Spec_Dist.Text);
                KeyPadForm KeyPad = new KeyPadForm(0, 100, nCurData, "Input Data", 0);
                KeyPad.ShowDialog();
                double specDistance = KeyPad.m_data;
                inspTool.SpecDistance = specDistance;
                text_Spec_Dist.Text = specDistance.ToString();
            }
        }

        private void text_Spec_Dist_Max_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(text_Spec_Dist_Max.Text);
                KeyPadForm KeyPad = new KeyPadForm(0, 100, nCurData, "Input Data", 0);
                KeyPad.ShowDialog();
                double specDistanceMax = KeyPad.m_data;
                inspTool.SpecDistanceMax = specDistanceMax;
                text_Spec_Dist_Max.Text = specDistanceMax.ToString();
            }
        }

        private void Combo_Polarity1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                int index = Combo_Polarity1.SelectedIndex;
                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(index + 1);
                else
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(index + 1);
            }
        }

        private void Combo_Polarity2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                int index = Combo_Polarity2.SelectedIndex;
                if (inspTool.Type == GaloInspType.Line)
                    inspTool.FindLineTool.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)(index + 1);
                else
                    inspTool.FindCircleTool.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)(index + 1);
            }
        }

        private void lblEdgeThreshold_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(lblEdgeThreshold.Text);
                KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
                KeyPad.ShowDialog();
                int nEdgeThreshold = (int)KeyPad.m_data;

                lblEdgeThreshold.Text = nEdgeThreshold.ToString();
                inspTool.DarkArea.Threshold = nEdgeThreshold;
            }
        }

        private void lblEdgeCaliperThreshold_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(lblEdgeCaliperThreshold.Text);
                KeyPadForm KeyPad = new KeyPadForm(0, 255, nCurData, "Input Data", 1);
                KeyPad.ShowDialog();
                int threshold = (int)KeyPad.m_data;
                inspTool.DarkArea.EdgeCaliperThreshold = threshold;

                lblEdgeCaliperThreshold.Text = threshold.ToString();
            }
        }

        private void lblEdgeCaliperFilterSize_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                double nCurData = Convert.ToDouble(lblEdgeCaliperFilterSize.Text);
                KeyPadForm KeyPad = new KeyPadForm(1, 255, nCurData, "Input Data", 2);
                KeyPad.ShowDialog();
                int FilterSize = (int)KeyPad.m_data;
                inspTool.DarkArea.EdgeCaliperFilterSize = FilterSize;

                lblEdgeCaliperFilterSize.Text = FilterSize.ToString();
            }
        }

        private void lblTopCutPixel_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblTopCutPixel.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int topCutPixel = (int)KeyPad.m_data;
                inspTool.DarkArea.TopCutPixel = topCutPixel;

                lblTopCutPixel.Text = topCutPixel.ToString();
            }
        }

        private void lblBottomCutPixel_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblBottomCutPixel.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int bottomCutPixel = (int)KeyPad.m_data;
                inspTool.DarkArea.BottomCutPixel = bottomCutPixel;

                lblBottomCutPixel.Text = bottomCutPixel.ToString();
            }
        }

        private void lblMaskingValue_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblMaskingValue.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int maskingValue = (int)KeyPad.m_data;
                inspTool.DarkArea.MaskingValue = maskingValue;

                lblMaskingValue.Text = maskingValue.ToString();
            }
        }

        private void lblIgnoreSize_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblIgnoreSize.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int ignoreSize = (int)KeyPad.m_data;

                inspTool.DarkArea.IgnoreSize = ignoreSize;
                lblIgnoreSize.Text = ignoreSize.ToString();
            }
        }
        #endregion

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

        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            MessageForm.LB_MESSAGE.Text = "Did You Check [APPLY]?";
            if (!MessageForm.Visible)
            {
                MessageForm.ShowDialog();
            }

            PasswordForm passwordForm = new PasswordForm(true);
            passwordForm.ShowDialog();

            if (!passwordForm.LOGINOK)
            {
                passwordForm.Dispose();
                return;
            }
            passwordForm.Dispose();

            _isNotUpdate = true;

            if (chkUseTracking.Checked)
            {
                SetAmpTrackingOnOff(false);
            }

            if (chkUseRoiTracking.Checked)
            {
                SetBondingTrackingOnOff(false);
            }

            if(ModelManager.Instance().CurrentModel is InspModel inspModel)
                inspModel.Save(StaticConfig.ModelPath);
            // Todo : 누가할래?
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
            _isNotUpdate = true;

            if (chkUseTracking.Checked)
            {
                chkUseTracking.Checked = false;
                SetAmpTrackingOnOff(false);
            }

            if (chkUseRoiTracking.Checked)
            {
                chkUseRoiTracking.Checked = false;
                SetBondingTrackingOnOff(false);
            }

            SystemManager.Instance().ReLoadModel();
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

    }
}

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
using Emgu.CV;
using Emgu.CV.Flann;
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

    public enum AddRoiType
    {
        None,
        Line,
        Circle,
    }

    public partial class PatternTeachForm : Form
    {
        private string _currentImageDir { get; set; } = "";

        private int _currentImageIndex { get; set; } = -1;

        private bool _isNotUpdate { get; set; } = false;

        private AlgorithmTool AlgorithmTool { get; set; } = new AlgorithmTool();

        private TabPageType TabPageType = TabPageType.AmpMark;

        private AddRoiType AddRoiType { get; set; } = AddRoiType.None;

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

        private CogImage8Grey CogDisplayImage { get; set; } = null;

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

            cbxDarkMaskingEdgeType.Items.Clear();
            foreach (DarkMaskingDirection type in Enum.GetValues(typeof(DarkMaskingDirection)))
                cbxDarkMaskingEdgeType.Items.Add(type.ToString());

            cbxDarkMaskingEdgeType.SelectedIndex = 0;
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
            AddRoiType = AddRoiType.None;

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
                LBL_ROI_FINEALIGN_SPEC_T.Text = CurrentUnit.Mark.Bonding.AlignSpec_T.ToString();
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
                if(openFileDlg.FileName.Contains("OV.jpg"))
                {
                    MessageBox.Show("This JPG File is 'Overlay Image'.\r\nSelect Origin JPG Image.");
                    return;
                }

                DisposeDisplayImage();


                CogDisplayImage = LoadImage(openFileDlg.FileName);

                string extenstion = Path.GetExtension(openFileDlg.FileName);
                _currentImageDir = Path.GetDirectoryName(openFileDlg.FileName);


                if (extenstion == ".jpg")
                {
                    string[] files = Directory.GetFiles(_currentImageDir, "*UP.jpg");
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (openFileDlg.FileName == files[i])
                        {
                            _currentImageIndex = i;
                            break;
                        }
                    }
                }
                else if(extenstion == ".bmp")
                {
                    string[] files = Directory.GetFiles(_currentImageDir, "*.bmp");

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (openFileDlg.FileName == files[i])
                        {
                            _currentImageIndex = i;
                            break;
                        }
                    }
                }

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
            if (CogDisplayImage == null || CogDisplay.Image == null || OriginMarkPoint == null)
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
                case 2: // Insp
                    LoggerHelper.Save_SystemLog("Galo Inspection", LogType.Cmd);
                    RunGaloInspectForTest();
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

            var markResult = AlgorithmTool.FindMark(CogDisplayImage as CogImage8Grey, markTool);

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

            var alignResult = AlgorithmTool.RunAmpFlimAlign(CogDisplayImage as CogImage8Grey, CurrentUnit.FilmAlign);

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

        private void RunGaloInspectForTest()
        {
            if (CogDisplayImage == null | _prevSelectedRowIndex < 0)
                return;
            LoggerHelper.Save_SystemLog("Inspection start", LogType.Cmd);
            Stopwatch sw = Stopwatch.StartNew();

            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                CogImage8Grey binaryImage = CogDisplayImage.CopyBase(CogImageCopyModeConstants.CopyPixels) as CogImage8Grey;
                if(inspTool.Type == GaloInspType.Line)
                {
                    CogRectangleAffine rect = new CogRectangleAffine();

                    var lineResult = AlgorithmTool.RunGaloLineInspection(CogDisplayImage as CogImage8Grey, binaryImage, inspTool, ref rect, true);
                    sw.Stop();
                    
                    Lab_Tact.Text = sw.ElapsedMilliseconds.ToString() + "ms";
                    LoggerHelper.Save_SystemLog($"Inspection Tact Time : {sw.ElapsedMilliseconds}ms", LogType.Cmd);

                    CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

                    foreach (var result in lineResult.InsideResult.GraphicsList)
                        resultGraphics.Add(result);

                    foreach (var result in lineResult.OutsideResult.GraphicsList)
                        resultGraphics.Add(result);

                    CogDisplay.InteractiveGraphics.AddList(resultGraphics, "Result", false);

                    dataGridView_Result.Rows.Clear();

                    var distanceList = lineResult.GetDistance();
                    string[] strResultData = new string[4];
                    for (int i = 0; i < distanceList.Count; i++)
                    {
                        strResultData[0] = i.ToString();
                        strResultData[1] = "0";
                        strResultData[3] = string.Format("{0:F3}", distanceList[i]);
                        dataGridView_Result.Rows.Add(strResultData);
                    }

                    VisionProHelper.DisposeDisplay(cogDisplayInSide);
                    VisionProHelper.DisposeDisplay(cogDisplayOutSide);
                    cogDisplayInSide.Image = null;
                    cogDisplayOutSide.Image = null;

                    cogDisplayInSide.Image = lineResult.InsideResult.EdgeEnhanceImage;
                    cogDisplayOutSide.Image = lineResult.OutsideResult.EdgeEnhanceImage;
                }
                else
                {
                   var circleInspResult = AlgorithmTool.RunGaloCircleInspection(CogDisplayImage as CogImage8Grey, inspTool, true);

                    sw.Stop();

                    Lab_Tact.Text = sw.ElapsedMilliseconds.ToString() + "ms";
                    LoggerHelper.Save_SystemLog($"Inspection Tact Time : {sw.ElapsedMilliseconds}ms", LogType.Cmd);

                    CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

                    foreach (var result in circleInspResult.ResultGraphics)
                        resultGraphics.Add(result);

                    CogDisplay.InteractiveGraphics.AddList(resultGraphics, "Result", false);

                    dataGridView_Result.Rows.Clear();

                    var distanceList = circleInspResult.GetDistance();
                    string[] strResultData = new string[4];
                    for (int i = 0; i < distanceList.Count; i++)
                    {
                        strResultData[0] = i.ToString();
                        strResultData[1] = "0";
                        strResultData[3] = string.Format("{0:F3}", distanceList[i]);
                        dataGridView_Result.Rows.Add(strResultData);
                    }
                }

                VisionProHelper.Save(binaryImage, @"D:\InspectLine0_binary.bmp");
            }
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

            double jogMoveX = Convert.ToDouble(txtOffsetX.Text);
            double jogMoveY = Convert.ToDouble(txtOffsetY.Text);

            int nMode = 0;
            nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            try
            {
                Button TempBTN = (Button)sender;
                switch (TempBTN.Text.ToUpper().Trim())
                {
                    case "LEFT":
                        nMoveDataX = -1 * jogMoveX;
                        nMoveDataY = 0;
                        break;

                    case "RIGHT":
                        nMoveDataX = 1 * jogMoveX;
                        nMoveDataY = 0;
                        break;

                    case "UP":
                        nMoveDataX = 0;
                        nMoveDataY = -1 * jogMoveY;
                        break;

                    case "DOWN":
                        nMoveDataX = 0;
                        nMoveDataY = 1 * jogMoveY;
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
            List<int> selectedIndexList = new List<int>();

            foreach (DataGridViewRow item in DataGridview_Insp.SelectedRows)
            {
                var index = Convert.ToInt32(item.Cells[0].Value);

                selectedIndexList.Add(index);

                var param = GetUnit().Insp.GaloInspToolList[index];

                if (param is GaloInspTool inspTool)
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
                }
            }

            DrawGaloRois(selectedIndexList);
        }

        private void DrawGaloRois(List<int> selectedIndex)
        {
            foreach (var index in selectedIndex)
            {
                var galoTool = GetUnit().Insp.GaloInspToolList[index];

                if (galoTool is GaloInspTool inspTool)
                {
                    if (galoTool.Type == GaloInspType.Line)
                        SetInteractiveGraphics("tool", inspTool.FindLineTool.CreateCurrentRecord());

                    if (galoTool.Type == GaloInspType.Circle)
                        SetInteractiveGraphics("tool", inspTool.FindCircleTool.CreateCurrentRecord());
                }
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
                if (OriginMarkPoint == null)
                    return;

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
            Button Btn = (Button)sender;
            if (Convert.ToInt32(Btn.Tag.ToString()) == 0)
                AddRoiType = AddRoiType.Line;
            else
                AddRoiType = AddRoiType.Circle;
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
            var upMarkResult = AlgorithmTool.FindMark(inputImage, upMarkToolList, score);
            var downMarkResult = AlgorithmTool.FindMark(inputImage, downMarkToolList, score);

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
            //AddRoiType
            var unit = GetUnit();
            bool isEdit = false;
            if (AddRoiType == AddRoiType.None)
            {
                if(GetCurrentInspParam() is GaloInspTool inspTool)
                {
                    if (MessageBox.Show("Are you sure you want to ROI Copy it?", "ROI Copy", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        isEdit = true;
                        unit.Insp.GaloInspToolList.Add(inspTool.DeepCopy());
                    }
                }
            }
            else if(AddRoiType == AddRoiType.Line)
            {
                isEdit = true;
                GaloInspTool inspTool = new GaloInspTool();
                inspTool.Type = GaloInspType.Line;
                inspTool.SetLineTool(new CogFindLineTool());
                inspTool.SetCircleTool(new CogFindCircleTool());
                unit.Insp.GaloInspToolList.Add(inspTool);
            }
            else if(AddRoiType == AddRoiType.Circle)
            {
                isEdit = true;
                GaloInspTool inspTool = new GaloInspTool();
                inspTool.Type = GaloInspType.Circle;
                inspTool.SetLineTool(new CogFindLineTool());
                inspTool.SetCircleTool(new CogFindCircleTool());
                unit.Insp.GaloInspToolList.Add(inspTool);
            }

            if(isEdit)
            {
                DataGridview_Insp.Rows.Clear();
                UpdateInspInfo(unit.Insp.GaloInspToolList.Count);
                UpdateInspParam();
            }
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
                    return;
            }
        }

        private void DataGridview_Insp_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            _prevSelectedRowIndex = e.RowIndex;
            AddRoiType = AddRoiType.None;

            UpdateInspParam();
            DrawInspParam();
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
        #endregion

        private void btn_Inspection_Test_Click(object sender, EventArgs e)
        {
            ClearDisplayGraphic();
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

            dataGridView_Result.Rows.Clear();
            List_NG.Items.Clear();
            Stopwatch sw = Stopwatch.StartNew();
            CogImage8Grey binaryImage = CogDisplayImage.CopyBase(CogImageCopyModeConstants.CopyPixels) as CogImage8Grey;
            for (int i = 0; i < GetUnit().Insp.GaloInspToolList.Count; i++)
            {
                var inspTool = GetUnit().Insp.GaloInspToolList[i];

                if (inspTool.Type == GaloInspType.Line)
                {
                    CogRectangleAffine rect = new CogRectangleAffine();

                    var lineResult = AlgorithmTool.RunGaloLineInspection(CogDisplayImage as CogImage8Grey, binaryImage, inspTool, ref rect, false);

                    foreach (var result in lineResult.InsideResult.GraphicsList)
                        resultGraphics.Add(result);

                    foreach (var result in lineResult.OutsideResult.GraphicsList)
                        resultGraphics.Add(result);

                    if(lineResult.Judgement != Judgement.OK)
                    {
                        List_NG.Items.Add($"Inspection NG ROI:{i}");

                        CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
                        CogNGRectAffine.Color = CogColorConstants.Red;
                        CogNGRectAffine.CenterX = inspTool.FindLineTool.RunParams.ExpectedLineSegment.MidpointX;
                        CogNGRectAffine.CenterY = inspTool.FindLineTool.RunParams.ExpectedLineSegment.MidpointY;
                        CogNGRectAffine.SideXLength = inspTool.FindLineTool.RunParams.ExpectedLineSegment.Length;
                        CogNGRectAffine.SideYLength = 100;
                        CogNGRectAffine.Rotation = inspTool.FindLineTool.RunParams.ExpectedLineSegment.Rotation;
                        resultGraphics.Add(CogNGRectAffine);
                    }
                }
                else
                {
                    var circleInspResult = AlgorithmTool.RunGaloCircleInspection(CogDisplayImage as CogImage8Grey, inspTool, false);
                    foreach (var result in circleInspResult.ResultGraphics)
                        resultGraphics.Add(result);

                    if (circleInspResult.Judgement != Judgement.OK)
                    {
                        List_NG.Items.Add($"Inspection NG ROI:{i}");

                        var toolRunparam = inspTool.FindCircleTool.RunParams;
                        CogFindLineTool cogTempLine = new CogFindLineTool();
                        cogTempLine.RunParams.ExpectedLineSegment.StartX = toolRunparam.ExpectedCircularArc.StartX;
                        cogTempLine.RunParams.ExpectedLineSegment.StartY = toolRunparam.ExpectedCircularArc.StartY;
                        cogTempLine.RunParams.ExpectedLineSegment.EndX = toolRunparam.ExpectedCircularArc.EndX;
                        cogTempLine.RunParams.ExpectedLineSegment.EndY = toolRunparam.ExpectedCircularArc.EndY;

                        CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
                        CogNGRectAffine.Color = CogColorConstants.Red;
                        CogNGRectAffine.CenterX = cogTempLine.RunParams.ExpectedLineSegment.MidpointX;
                        CogNGRectAffine.CenterY = cogTempLine.RunParams.ExpectedLineSegment.MidpointY;
                        CogNGRectAffine.SideXLength = cogTempLine.RunParams.ExpectedLineSegment.Length;
                        CogNGRectAffine.SideYLength = 100;
                        CogNGRectAffine.Rotation = cogTempLine.RunParams.ExpectedLineSegment.Rotation;
                        resultGraphics.Add(CogNGRectAffine);
                    }
                }
            }
            sw.Stop();

            Lab_Tact.Text = sw.ElapsedMilliseconds.ToString() + "ms";
            LoggerHelper.Save_SystemLog($"Inspection Full Tact Time : {sw.ElapsedMilliseconds}ms", LogType.Cmd);


            VisionProHelper.Save(binaryImage, @"D:\InspectLine0_binary.bmp");


            CogDisplay.InteractiveGraphics.AddList(resultGraphics, "Result", false);
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
            
            ckbUseDarkEdge.Checked = param.DarkArea.ThresholdUse;
            cbxDarkMaskingEdgeType.SelectedIndex = (int)param.DarkArea.MaskingDirection;

            lblEdgeThreshold.Text = param.DarkArea.Threshold.ToString();
            lblEdgeCaliperThreshold.Text = param.DarkArea.EdgeCaliperThreshold.ToString();
            lblEdgeCaliperFilterSize.Text = param.DarkArea.EdgeCaliperFilterSize.ToString();

            lblInsideTopCutPixel.Text = param.DarkArea.StartCutPixel.ToString();
            lblInsideBottomCutPixel.Text = param.DarkArea.EndCutPixel.ToString();
            lblOutsideTopCutPixel.Text = param.DarkArea.OutsideStartCutPixel.ToString();
            lblOutsideBottomCutPixel.Text = param.DarkArea.OutsideEndCutPixel.ToString();

            lblMaskingValue.Text = param.DarkArea.MaskingValue.ToString();
            lblIgnoreSize.Text = param.DarkArea.IgnoreSize.ToString();

            text_Dist_Ignre.Text = param.Distgnore.ToString();
            text_Spec_Dist.Text = param.SpecDistance.ToString();
            text_Spec_Dist_Max.Text = param.SpecDistanceMax.ToString();

            cogDisplayInSide.Image = null;
            cogDisplayOutSide.Image = null;

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
                if (Polarity == CogCaliperPolarityConstants.DontCare)
                    Combo_Polarity1.SelectedIndex = 0;
                else
                    Combo_Polarity1.SelectedIndex = (int)Polarity - 1;

                Polarity = param.FindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
                if (Polarity == CogCaliperPolarityConstants.DontCare)
                    Combo_Polarity2.SelectedIndex = 0;
                else
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

                double dEdgeWidth =Convert.ToDouble(DataGridview_Insp.Rows[_prevSelectedRowIndex].Cells[14].Value);
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

            var inputImage = CogDisplay.Image;//.CopyBase(CogImageCopyModeConstants.CopyPixels);

            double score = CurrentUnit.Mark.Bonding.Score;

            ClearDisplayGraphic();
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

            CogGraphicLabel LabelText = new CogGraphicLabel();
            LabelText.X = CogDisplay.Width /2;
            LabelText.Y = 0;
            resultGraphics.Add(LabelText);

            var upMarkToolList = CurrentUnit.Mark.Bonding.UpMarkToolList;
            var downMarkToolList = CurrentUnit.Mark.Bonding.DownMarkToolList;

            var reuslt = AlgorithmTool.FindBondingMark(inputImage as CogImage8Grey, upMarkToolList, downMarkToolList, score, CurrentUnit.Mark.Bonding.AlignSpec_T);

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

            CurrentUnit.Mark.Bonding.AlignSpec_T = dTheta;
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
            btnImagePrev.Enabled = false;
            if (_currentImageIndex < 0)
                return;

            btnImageNext.Enabled = true;

            string[] files;
            if (openFileDlg.SafeFileName.Substring(openFileDlg.SafeFileName.Length - 3) == "jpg")
            {
                files = Directory.GetFiles(_currentImageDir, "*UP.jpg");
            }
            else
            {
                files = Directory.GetFiles(_currentImageDir, "*.bmp");
            }

            if (_currentImageIndex < files.Length)
            {
                if (_currentImageIndex != 0)
                    _currentImageIndex--;
                else
                {
                    MessageBox.Show("First Image!!");
                    btnImagePrev.Enabled = true;
                    return;
                }

                string fileName = files[_currentImageIndex];

                CogDisplayImage?.Dispose();
                CogDisplayImage = LoadImage(fileName);

                CogDisplay.Image = CogDisplayImage;
                ClearDisplayGraphic();
            }
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

            chkUseRoiTracking.Checked = true;
            if (SetBondingTrackingOnOff(true) == false)
            {
                chkUseRoiTracking.Checked = false;
            }
            else
            {
                //검사
                btn_Inspection_Test.PerformClick();
                btnImagePrev.Enabled = true;
            }
            _isNotUpdate = false;
        }

        private void btnImageNext_Click(object sender, EventArgs e)
        {
            btnImageNext.Enabled = false;
            if (_currentImageIndex < 0)
                return;
            btnImagePrev.Enabled = true;

            string[] files;
            if (openFileDlg.SafeFileName.Substring(openFileDlg.SafeFileName.Length - 3) == "jpg")
            {
                files = Directory.GetFiles(_currentImageDir, "*UP.jpg");
            }
            else
            {
                files = Directory.GetFiles(_currentImageDir, "*.bmp");
            }

            if (_currentImageIndex < files.Length - 1)
            {
                _currentImageIndex++;

                string fileName = files[_currentImageIndex];

                CogDisplayImage?.Dispose();
                CogDisplayImage = LoadImage(fileName);

                CogDisplay.Image = CogDisplayImage;
                ClearDisplayGraphic();
            }
            else
            {
                MessageBox.Show("Last Image!!");
                btnImageNext.Enabled = true;
                return;
            }
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

            chkUseRoiTracking.Checked = true;
            if(SetBondingTrackingOnOff(true) == false)
            {
                chkUseRoiTracking.Checked = false;
            }
            else
            {
                //검사
                btn_Inspection_Test.PerformClick();
                btnImageNext.Enabled = true;
            }
            _isNotUpdate = false;
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
            if (chkDarkAlgorithm.Checked)
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

            var markResult = AlgorithmTool.FindMark(CogDisplayImage as CogImage8Grey, markTool);

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
                KeyPadForm KeyPad = new KeyPadForm(1, 255, nCurData, "Input Data", 1);
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
        private void ckbUseDarkEdge_CheckStateChanged(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                inspTool.DarkArea.ThresholdUse = ckbUseDarkEdge.Checked;
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

        private void lblInsideTopCutPixel_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblInsideTopCutPixel.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int topCutPixel = (int)KeyPad.m_data;
                inspTool.DarkArea.StartCutPixel = topCutPixel;

                lblInsideTopCutPixel.Text = topCutPixel.ToString();
            }
        }

        private void lblInsideBottomCutPixel_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblInsideBottomCutPixel.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int bottomCutPixel = (int)KeyPad.m_data;
                inspTool.DarkArea.EndCutPixel = bottomCutPixel;

                lblInsideBottomCutPixel.Text = bottomCutPixel.ToString();
            }
        }

        private void lblOutsideTopCutPixel_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblOutsideTopCutPixel.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int topCutPixel = (int)KeyPad.m_data;
                inspTool.DarkArea.OutsideStartCutPixel = topCutPixel;

                lblOutsideTopCutPixel.Text = topCutPixel.ToString();
            }
        }

        private void lblOutsideBottomCutPixel_Click(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                KeyPadForm KeyPad = new KeyPadForm(0, 255, Convert.ToInt16(lblOutsideBottomCutPixel.Text.ToString()), "Input Data", 0);
                KeyPad.ShowDialog();
                int bottomCutPixel = (int)KeyPad.m_data;
                inspTool.DarkArea.OutsideEndCutPixel = bottomCutPixel;

                lblOutsideBottomCutPixel.Text = bottomCutPixel.ToString();
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
                chkUseTracking.Checked = false;
                SetAmpTrackingOnOff(false);
            }

            if (chkUseRoiTracking.Checked)
            {
                chkUseRoiTracking.Checked = false;
                SetBondingTrackingOnOff(false);
            }

            UpdateData();
            
            if(ModelManager.Instance().CurrentModel is InspModel inspModel)
            {
                SystemManager.Instance().ShowProgerssBar(1, true, 0);
                string filePath = StaticConfig.ModelPath + AppsConfig.Instance().ProjectName;
                inspModel.Save(filePath, StageUnitNo);

                SystemManager.Instance().ShowProgerssBar(1, true, 1);
            }
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

        private void DataGridview_Insp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Down) || e.KeyChar == Convert.ToChar(Keys.Up))
            {

            }
            else
            {
                e.Handled = true;
            }
        }

        private void DataGridview_Insp_KeyDown(object sender, KeyEventArgs e)
        {
            int currentRowIndex = ((DataGridView)sender).CurrentCell.RowIndex;
            if (e.KeyCode == Keys.Down)
            {
                if (currentRowIndex + 1 > DataGridview_Insp.Rows.Count - 1)
                    return;
                ((DataGridView)sender).Rows[currentRowIndex + 1].Selected = true;

                _prevSelectedRowIndex = currentRowIndex;
                AddRoiType = AddRoiType.None;

                UpdateInspParam();
                DrawInspParam();
            }
            if (e.KeyCode == Keys.Up)
            {
                if (currentRowIndex - 1 < 0)
                    return;
                ((DataGridView)sender).Rows[currentRowIndex - 1].Selected = true;

                _prevSelectedRowIndex = currentRowIndex;
                AddRoiType = AddRoiType.None;

                UpdateInspParam();
                DrawInspParam();
            }
        }

        private void cbxDarkMaskingEdgeType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GetCurrentInspParam() is GaloInspTool inspTool)
            {
                int index = cbxDarkMaskingEdgeType.SelectedIndex;
                if(index >= 0)
                    inspTool.DarkArea.MaskingDirection = (DarkMaskingDirection)index;
            }
        }

    }
}

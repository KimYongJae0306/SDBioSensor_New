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
using System.IO;
using System.Runtime.InteropServices;

using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ToolBlock;
using System.Net.NetworkInformation;
using Cog.Framework.Settings;

namespace COG
{
    public partial class MainForm : Form
    {
        //private static int nDisMax = 4;//갯수는 메인화면 디스플레이 갯수랑 통일

        //private List<DataGridView> GridView_Log = new List<DataGridView>();
        //private DataGridView[] InspecGridView = new DataGridView[Main.DEFINE.AlignUnit_Max];

        //private List<ListBox> ListBox_Log = new List<ListBox>();
        //private List<ListBox> ListBox_Length = new List<ListBox>();

        //private List<CogRecordDisplay> cogDisplay = new List<CogRecordDisplay>();
        //private List<Button> cogDisplayButton = new List<Button>();

        //private static int[] cogDisplayCamNo = new int[nDisMax];
        //private List<CogDisplayToolbarV2> cogDiToolBar = new List<CogDisplayToolbarV2>();
        //private List<CogDisplayStatusBarV2> cogDisStatuBar = new List<CogDisplayStatusBarV2>();
        //private bool threadFlag;
        //private Thread ThreadProcM;
        //private int nSelectStageNum = 0;

        //private Form_PatternSelect Pattern_Select = new Form_PatternSelect();
        //private Form_LogView Formlogview = new Form_LogView();
        //private Form_Melsec Melsec = new Form_Melsec();
        //private Form_CalDisplay formCalDis = new Form_CalDisplay();
        //private Form_Message formMessage = new Form_Message();
        //private Form_SetUp form_setup = new Form_SetUp();
        //public Form_Permission FormPermission = new Form_Permission();


        //private Form_RCS form_RCS = new Form_RCS();

        //private Form_TrayDataView form_trayDataview = new Form_TrayDataView();

        //private Main.MTickTimer timerTemp = new Main.MTickTimer();

        //private Form_LiveView[] formLiveview = new Form_LiveView[Main.DEFINE.CAM_MAX];

        //private DateTime mCurrentTime = new DateTime();

        //private Form_ManualSet[] FormManualSet = new Form_ManualSet[Main.DEFINE.AlignUnit_Max];
        //private Form_Chart[] formChart = new Form_Chart[Main.DEFINE.AlignUnit_Max];
        //private Form_NGMonitor[] FormNgMonitor = new Form_NGMonitor[Main.DEFINE.AlignUnit_Max];
        ////  private Form_Chart formChart = new Form_Chart();

        //private List<Label>[] LB_INSP = new List<Label>[8];

        //private List<Label> LIST_LB_PROC_TIME = new List<Label>();
        //private List<Label> LIST_LB_POINT_NG_CNT = new List<Label>();
        //private List<PictureBox> LIST_PB_CAM_CONSTAT = new List<PictureBox>();
        //private List<PictureBox> LIST_PB_CAM_DISCONSTAT = new List<PictureBox>();
        //private List<PictureBox> LIST_PB_LIGHT_CONSTAT = new List<PictureBox>();
        //private List<PictureBox> LIST_PB_LIGHT_DISCONSTAT = new List<PictureBox>();

        //private int[] nMainLiveFlag = new int[Main.DEFINE.CAM_MAX];

        //private int nModelChangeTime = 0;

        public MainForm()
        {
            InitializeComponent();
           
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AddSystemInfo();

            if (CogLicense.GetLicensedFeatures(false, false).Count == 0)
            {
                MessageBox.Show("Cognex USB Lincense not detected");
            }

            TAB_IMG_DISPLAY.Appearance = TabAppearance.Buttons;
            TAB_IMG_DISPLAY.SizeMode = TabSizeMode.Fixed;
            TAB_IMG_DISPLAY.ItemSize = new Size(0, 1); // TAB 우측 버튼부분 숨기려고 만듬

            StaticConfig.Initialize();
        }

        private void AddSystemInfo()
        {
            string _Assemblyname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            DateTime lastWriteTime = File.GetLastWriteTimeUtc(Application.StartupPath + "\\" + _Assemblyname + ".exe").ToLocalTime();

            string LoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string message = "";
            message += this.Text + "  " + "Build: " + lastWriteTime.ToString();
            message += "          " + "Login: " + LoginDate.ToString() + "  STANDARD 표준 장비" + "   " + StaticConfig.PROGRAM_TYPE.ToString();

            this.Text = message;
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
        }

        private void ReadModuleID()
        {
        //    string LogMsg = "";
        //    string m_data;
        //    char m_CharData;
        //    long dataNum;
        //    int Cellid_READ_Address = 0;

        //    Cellid_READ_Address = Main.DEFINE.MX_ARRAY_RSTAT_OFFSET + Main.DEFINE.MODULED_NUM;
        //    Main.machine.m_strModuleID = "";

        //    for (int i = 0; i < 10; i++)
        //    {
        //        dataNum = PLCDataTag.RData[Cellid_READ_Address + i] & 0x00ff;       //RData 1개 = 2byte => 한글자 1byte 
        //        m_CharData = Convert.ToChar(dataNum);
        //        m_data = m_CharData.ToString();
        //        if (m_data == "\0") break;
        //        Main.machine.m_strModuleID += m_CharData.ToString();     //하위 글자

        //        dataNum = (PLCDataTag.RData[Cellid_READ_Address + i] >> 8) & 0x00ff;
        //        m_CharData = Convert.ToChar(dataNum);
        //        m_data = m_CharData.ToString();
        //        if (m_data == "\0") break;
        //        Main.machine.m_strModuleID += m_CharData.ToString();     //상위 글자
        //    }

        //    LogMsg = "Module_ID" + " <- " + Main.machine.m_strModuleID;
        //    Main.AlignUnit[0].LogdataDisplay(LogMsg, true);
        }

        private void Allocate_Array()
        {
            //int[,] nDefaultName = new int[Main.DEFINE.DISPLAY_MAX, 3];
            //int nAlignUnit = 0;
            //int nPatTag = 1;
            //int nPat = 2;

            //if (Main.DEFINE.OPEN_F || Main.DEFINE.OPEN_CAM)
            //{
            //    BTN_CMDTEST.Visible = true;
            //    TB_COMMANDTEST.Visible = true;
            //    BTN_CCLINKTEST.Visible = true;
            //    BTN_MXTEST.Visible = true;
            //}

            ////            if (Main.DEFINE.PROGRAM_TYPE == "FOF_PC1" || Main.DEFINE.PROGRAM_TYPE == "TFOF_PC1") BTN_TRAY_VIEW.Visible = true;


            //#region Allocate_Control
            //int nTabNum = 0;
            //cogDisplay.Clear();
            //for (int i = 0; i < nDisMax; i++)
            //{
            //    string nTempName;
            //    int nNum;
            //    nNum = (i + 1);

            //    if (i < 8)
            //    {
            //        nTabNum = 0;
            //    }
            //    else
            //    {
            //        if (i < 16)
            //            nTabNum = 1;
            //        else
            //            nTabNum = 2;
            //    }

            //    nTempName = "MA_Display" + nNum.ToString("00");
            //    CogRecordDisplay nType1 = (CogRecordDisplay)this.Controls["TAB_IMG_DISPLAY"].Controls["Tab_Num_" + nTabNum.ToString()].Controls[nTempName];
            //    cogDisplay.Add(nType1); //TAB_IMG_DISPLAY

            //    nNum = (i + 1);
            //    nTempName = "BTN_DISNAME_" + nNum.ToString("00");

            //    Button nButton = (Button)this.Controls["TAB_IMG_DISPLAY"].Controls["Tab_Num_" + nTabNum.ToString()].Controls[nTempName];
            //    cogDisplayButton.Add(nButton);

            //}

            //for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //{
            //    string nTempName;
            //    int nNum;
            //    nNum = i + 1;

            //    nTempName = "LB_Lisi_" + nNum.ToString("00");
            //    ListBox nType1 = (ListBox)this.Controls["TAB_LOGDISPLAY"].Controls["tabPage" + i.ToString()].Controls[nTempName];
            //    ListBox_Log.Add(nType1);

            //    nTempName = "DG_VIEW_" + nNum.ToString("00");
            //    DataGridView nType2 = (DataGridView)this.Controls["TAB_LOGDISPLAY"].Controls["tabPage" + i.ToString()].Controls[nTempName];
            //    GridView_Log.Add(nType2);

            //    nTempName = "LB_Lisi_LENGTH_" + nNum.ToString("00");
            //    ListBox nType3 = (ListBox)this.Controls["TAB_LOGDISPLAY"].Controls["tabPage" + i.ToString()].Controls[nTempName];
            //    ListBox_Length.Add(nType3);

            //    FormManualSet[i] = new Form_ManualSet();
            //}

            //for (int i = 0; i < Main.DEFINE.DISPLAY_MAX; i++)
            //{
            //    cogDisplay[i].Visible = true;
            //    cogDisplayButton[i].Visible = true;
            //}

            //TAB_LOGDISPLAY.TabPages[0].Text = "CAM 1 (INSPECTION 1,2)";
            //TAB_LOGDISPLAY.TabPages[1].Text = "CAM 2 (INSPECTION 3,4)";

            //while (true)
            //{
            //    if (TAB_LOGDISPLAY.Controls.Count == Main.DEFINE.AlignUnit_Max) break;
            //    TAB_LOGDISPLAY.Controls.RemoveAt(TAB_LOGDISPLAY.Controls.Count - 1);
            //}

            //for (int i = 0; i < 3; i++)
            //{
            //    LB_INSP[i] = new List<Label>();
            //    switch (i)
            //    {
            //        case 0:
            //            LB_INSP[0].Add(LB_INSPEC_0_00); LB_INSP[0].Add(LB_INSPEC_0_01); LB_INSP[0].Add(LB_INSPEC_0_02); LB_INSP[0].Add(LB_INSPEC_0_03); LB_INSP[0].Add(LB_INSPEC_0_04);
            //            LB_INSP[0].Add(LB_INSPEC_0_05); LB_INSP[0].Add(LB_INSPEC_0_06); LB_INSP[0].Add(LB_INSPEC_0_07);
            //            break;

            //        case 1:
            //            LB_INSP[1].Add(LB_INSPEC_1_00); LB_INSP[1].Add(LB_INSPEC_1_01); LB_INSP[1].Add(LB_INSPEC_1_02); LB_INSP[1].Add(LB_INSPEC_1_03); LB_INSP[1].Add(LB_INSPEC_1_04);
            //            LB_INSP[1].Add(LB_INSPEC_1_05); LB_INSP[1].Add(LB_INSPEC_1_06); LB_INSP[1].Add(LB_INSPEC_1_07);
            //            break;

            //        case 2:
            //            LB_INSP[2].Add(LB_INSPEC_2_00); LB_INSP[2].Add(LB_INSPEC_2_01); LB_INSP[2].Add(LB_INSPEC_2_02); LB_INSP[2].Add(LB_INSPEC_2_03); LB_INSP[2].Add(LB_INSPEC_2_04);
            //            LB_INSP[2].Add(LB_INSPEC_2_05); LB_INSP[2].Add(LB_INSPEC_2_06); LB_INSP[2].Add(LB_INSPEC_2_07);
            //            break;
            //    }
            //}

            //if (Main.DEFINE.PROGRAM_TYPE == "ATT_AREA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            //{
            //    LIST_LB_PROC_TIME.Add(LB_PROCTIME_1);
            //    LIST_LB_PROC_TIME.Add(LB_PROCTIME_2);
            //    LIST_LB_PROC_TIME.Add(LB_PROCTIME_3);
            //    LIST_LB_PROC_TIME.Add(LB_PROCTIME_4);

            //    LIST_LB_POINT_NG_CNT.Add(LB_NG_COUNT_P1);
            //    LIST_LB_POINT_NG_CNT.Add(LB_NG_COUNT_P2);
            //    LIST_LB_POINT_NG_CNT.Add(LB_NG_COUNT_P3);
            //    LIST_LB_POINT_NG_CNT.Add(LB_NG_COUNT_P4);
            //}
            //#endregion

            //#region DisplatButton Text Input
            //for (int i = 0; i < Main.DEFINE.DISPLAY_MAX; i++)
            //    cogDisplayButton[i].Text = Main.Common.VIEW_NAME[i];
            //DisplayViewLocation(Main.Common.VIEW_Pos, Main.Common.VIEW_Size);
            //#endregion
        }

        private void DisplayViewLocation(string[] Location, string[] nSize)
        {
            //if (Location.Length != Main.DEFINE.DISPLAY_MAX) MessageBox.Show("DisplayViewPosition Count Check");

            //int[] SizeX = new int[Main.DEFINE.DISPLAY_MAX]; int[] SizeY = new int[Main.DEFINE.DISPLAY_MAX];
            //int[] PosX = new int[Main.DEFINE.DISPLAY_MAX]; int[] PosY = new int[Main.DEFINE.DISPLAY_MAX];

            //int[] nDisTabNo = new int[Main.DEFINE.DISPLAY_MAX];

            //int[] nTabNo = new int[Main.DEFINE.DISPLAY_MAX];
            //int[] nDisNo = new int[Main.DEFINE.DISPLAY_MAX];
            //int[] nSizeX = new int[Main.DEFINE.DISPLAY_MAX];
            //int[] nSizeY = new int[Main.DEFINE.DISPLAY_MAX];

            //int nTabAmt = 1;

            //int TempTabNo = 0;

            //for (int i = 0; i < Location.Length; i++)
            //{
            //    TempTabNo = Convert.ToInt16(Location[i].ToString().Substring(0, 1)) - 1;
            //    nDisNo[i] = Convert.ToInt16(Location[i].ToString().Substring(Location[i].Length - 1, 1)) - 1;

            //    nSizeX[i] = Convert.ToInt16((nSize[i].ToString().Trim()).ToString().Substring(0, 1));
            //    nSizeY[i] = Convert.ToInt16((nSize[i].ToString().Trim()).ToString().Substring(nSize[i].ToString().Trim().Count() - 1, 1));

            //    nDisTabNo[i] = TempTabNo;

            //    if (TempTabNo > 0) nTabAmt = 2;
            //    if (TempTabNo > 1) nTabAmt = 3;
            //}

            //#region 각 View_Tab 에 따라 표시되는 갯수에따라 사이즈 조정, 위치 조정.

            //Point[] nDisPos = new Point[Main.DEFINE.DISPLAY_MAX];
            //Point[] nBtnPos = new Point[Main.DEFINE.DISPLAY_MAX];


            //int nWidth__Cnt = Main.Common.VIEW_WIDTH_CNT[0];
            //int nHeight_Cnt = 2;

            //int BtnGap = cogDisplayButton[0].Height;

            ////실제 TAB_IMG_DISPLAY.Height 길이에서 12만큼은 밑단이 안보이기때문에 계산시 길이값 축소계산

            //int View_Width = ((TAB_IMG_DISPLAY.Width - 16)) / nWidth__Cnt;
            //int ViewHeight = ((TAB_IMG_DISPLAY.Height - 12) - (BtnGap * nHeight_Cnt)) / nHeight_Cnt;
            //int nTempCnt = 0;

            //for (int i = 0; i < Main.DEFINE.DISPLAY_MAX; i++)
            //{
            //    nWidth__Cnt = Main.Common.VIEW_WIDTH_CNT[nDisTabNo[i]];
            //    View_Width = ((TAB_IMG_DISPLAY.Width - 16)) / nWidth__Cnt;

            //    PosX[i] = (View_Width + 1) * (nDisNo[i] % nWidth__Cnt);
            //    PosY[i] = (ViewHeight + 1) * (nDisNo[i] / nWidth__Cnt);

            //    SizeX[i] = View_Width * nSizeX[i];

            //    if (nSizeY[i] == 2)
            //        SizeY[i] = ViewHeight * nSizeY[i] + BtnGap + 1; // 세로가 갈때 버튼 길이만큼 늘려 줄라고.
            //    else
            //        SizeY[i] = ViewHeight * nSizeY[i];

            //    nBtnPos[i].X = PosX[i];
            //    nBtnPos[i].Y = PosY[i] + (BtnGap * (nDisNo[i] / nWidth__Cnt));

            //    nDisPos[i].X = PosX[i];
            //    nDisPos[i].Y = PosY[i] + (BtnGap * (nDisNo[i] / nWidth__Cnt)) + BtnGap;

            //    cogDisplayButton[i].Location = nBtnPos[i];
            //    cogDisplayButton[i].Width = SizeX[i];

            //    cogDisplay[i].Location = nDisPos[i];
            //    cogDisplay[i].Width = SizeX[i];
            //    cogDisplay[i].Height = SizeY[i];// -BtnGap;
            //}
            //#endregion

            //#region 각 View_Tab 에 따라 표시되는 갯수에따라 Tab에 맞는 창 띄우기.
            //for (int i = 0; i < nTabAmt; i++)
            //{
            //    for (int j = 0; j < Main.DEFINE.DISPLAY_MAX; j++)
            //    {
            //        if (nDisTabNo[j] == 1)                      // 두번째 TAB
            //        {
            //            RBTN_TAB_1.Visible = true;
            //            Tab_Num_1.Controls.Add(cogDisplay[j]);
            //            Tab_Num_1.Controls.Add(cogDisplayButton[j]);
            //        }
            //        if (nDisTabNo[j] == 2)   // 세번째 TAB
            //        {
            //            RBTN_TAB_2.Visible = true;
            //            Tab_Num_2.Controls.Add(cogDisplay[j]);
            //            Tab_Num_2.Controls.Add(cogDisplayButton[j]);
            //        }
            //    }
            //}
            //#endregion
        }
        private void DisplayViewPosition(int Col, int Row)
        {
            //if ((Col * Row) > nDisMax) MessageBox.Show("DisplayViewPosition Count Check");
            //int SizeX, SizeY;
            //int PitchX, PitchY;

            //SizeX = TAB_IMG_DISPLAY.Width / Col - 1;
            //SizeY = TAB_IMG_DISPLAY.Height / Row - cogDisplayButton[0].Height - 2;

            //PitchX = TAB_IMG_DISPLAY.Width / Col;
            //PitchY = TAB_IMG_DISPLAY.Height / Row;

            //for (int i = 0; i < nDisMax; i++)
            //{
            //    cogDisplay[i].Width = SizeX;
            //    cogDisplayButton[i].Width = SizeX;

            //    cogDisplay[i].Height = SizeY;
            //}

            //Point[,] nDisPos = new Point[Row, Col];
            //Point[,] nBtnPos = new Point[Row, Col];

            //for (int i = 0; i < Row; i++)
            //{
            //    for (int j = 0; j < Col; j++)
            //    {
            //        nDisPos[i, j].X = cogDisplay[0].Location.X + (PitchX * j);
            //        nBtnPos[i, j].X = cogDisplayButton[0].Location.X + (PitchX * j);

            //        nDisPos[i, j].Y = cogDisplay[0].Location.Y + (PitchY * i);
            //        nBtnPos[i, j].Y = cogDisplayButton[0].Location.Y + (PitchY * i);
            //    }
            //}


            //for (int i = 0; i < Row; i++)
            //{
            //    for (int j = 0; j < Col; j++)
            //    {
            //        cogDisplay[i * Col + j].Location = nDisPos[i, j];
            //        cogDisplayButton[i * Col + j].Location = nBtnPos[i, j];
            //    }
            //}
        }
        private void LiveFormHide()
        {
            //Melsec.BTN_EXIT_Click(null, null);
            //for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            //{
            //    try
            //    {
            //        if (formLiveview[i] == null)
            //            formLiveview[i] = new Form_LiveView(i);
            //        formLiveview[i].BTN_EXIT_Click(null, null);
            //    }
            //    catch
            //    {
            //        if (i < Main.DEFINE.CAM_MAX)
            //            formLiveview[i] = new Form_LiveView(i);
            //    }
            //}
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //string value = string.Empty;
            //LB_DISPLAY_CURRENT.Text = Main.ProjectName + " - " + Main.ProjectInfo;

            //string LogMsg = "";
            //string m_data;
            //int nCMD = 0;
            //char m_CharData;
            //int dataNum;
            //int TIME_READ_Address = 0;
            //string m_TIME_ID_Temp = "";

            //LB_TIME.Text = DateTime.Now.ToString();

            //if (Main.PLC_AUTO_READY
            //    && Main.Status.MC_STATUS == Main.DEFINE.MC_STOP
            //    && Main.Status.MC_MODE == Main.DEFINE.MC_MAINFORM
            //    && Main.CCLink_IsBit(Main.DEFINE.CCLINK_OUT_VISION_BUSY) == false)
            //{
            //    BTN_START_Click(null, null);
            //}

            //if (Main.Status.MC_STATUS == Main.DEFINE.MC_RUN)
            //{
            //    for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //    {
            //        if (Main.AlignUnit[i].m_ManualMatchFlag && !Main.AlignUnit[i].m_ManualMatchRunning)
            //        {
            //            Main.AlignUnit[i].m_ManualMatchRunning = true;
            //            ManualSetForm(FormManualSet[i], i);
            //            Main.AlignUnit[i].m_ManualMatchRunning = false;
            //        }
            //        if (Main.AlignUnit[i].m_NgImage_MonitorFlag)
            //        {
            //            Main.AlignUnit[i].m_NgImage_MonitorFlag = false;
            //            NgMonitorFormSet(FormNgMonitor[i], Main.AlignUnit[i].m_PatTagNo);
            //        }
            //    }
            //}
            //for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //{
            //    for (int j = 0; j < Main.AlignUnit[i].m_AlignPatTagMax; j++)
            //    {
            //        for (int k = 0; k < Main.AlignUnit[i].m_AlignPatMax[j]; k++)
            //        {
            //            if (!Main.DEFINE.OPEN_F)
            //            {
            //                if (Main.AlignUnit[i].PAT[j, k].m_CamNo > Main.DEFINE.MIL_CAM_MAX && Main.vision.CogImageBlock[Main.AlignUnit[i].PAT[j, k].m_CamNo].RunStatus.ProcessingTime > Main.DEFINE.CAMEARA_TIMEOUT)
            //                {
            //                    BTN_STOP_Click(null, null);
            //                    formMessage.LB_MESSAGE.Text = "Please check the connection of the " + Main.AlignUnit[i].PAT[j, k].m_PatternName + "camera cable Stop Mode";
            //                    if (!formMessage.Visible)
            //                    {
            //                        formMessage.ShowDialog();
            //                        Save_SystemLog(formMessage.LB_MESSAGE.Text, Main.DEFINE.CMD);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
         
            //if (Main.Status.MC_STATUS == Main.DEFINE.MC_STOP && Main.Status.MC_MODE != Main.DEFINE.MC_CAMERAFORM)
            //    Main.Refresh_Unit();
      
        }
        private void timer_Directory_Tick(object sender, EventArgs e)
        {
            //Main.machine.LogDirDeleteFlag = true;
        }

        public void SetTimerDirInterval(int nDays)
        {
            //// Log Check Period를 설정할 당시 시간을 저장한 파일 로드
            //FileInfo info = new FileInfo(Main.DEFINE.SYS_DATADIR + "OLD_LOG_CHECK_FILE.dat");
            //int nRestSeconds = (int)((DateTime.Now.Ticks - info.LastWriteTime.Ticks) / 10000000); // Ticks to Seconds

            //if (nRestSeconds > nDays * 24 * 60 * 60)
            //    timer_Directory.Interval = nDays * 24 * 60 * 60;
            //else
            //    timer_Directory.Interval = (nDays * 24 * 60 * 60 - nRestSeconds);
        }

        private void BTN_PROJECT_Click(object sender, EventArgs e)
        {
            //if (!MessageShow()) return;
            //if (!MessageShowPermission()) return;

            //if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2" && Main.MODEL_COPY == true)
            //{
            //    Main.ProjectRename(Main.MODEL_COPY_NAME, Main.MODEL_COPY_INFO);
            //    Main.MODEL_COPY = false;
            //}

            //Form_Project formProject = new Form_Project();
            //formProject.ShowDialog();
        }
        private void BTN_TEACH_Click(object sender, EventArgs e)
        {
            //if (!MessageShow())
            //    return;

            //LiveFormHide();

            //Main.Status.MC_MODE = Main.DEFINE.MC_TEACHFORM;
            //Pattern_Select.ShowDialog();
            //Main.Status.MC_MODE = Main.DEFINE.MC_MAINFORM;
        }
        private void BTN_SETUP_Click(object sender, EventArgs e)
        {
            //if (!MessageShowPermission()) return;
            //Main.Status.MC_MODE = Main.DEFINE.MC_SETUPFORM;
            //timer_Directory.Enabled = false;
            //form_setup.ShowDialog();
            //SetTimerDirInterval(Main.machine.m_nOldLogCheckPeriod);
            //timer_Directory.Enabled = true;
            //Main.Status.MC_MODE = Main.DEFINE.MC_MAINFORM;
        }
        private void BTN_MELSEC_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    Melsec.BTN_EXIT_Click(null, null);
            //    Melsec.Show();
            //    Melsec.Form_Melsec_Load();
            //}
            //catch
            //{
            //    //FormCCLink = new Form_CCLink();
            //    Melsec = new Form_Melsec();
            //}
        }
        private void BTN_LOGVIEW_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    Formlogview.Show();
            //    Formlogview.ControlUpDate();
            //}
            //catch
            //{
            //    Formlogview = new Form_LogView();
            //}
        }
        private void BTN_CALDIS_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    formCalDis.Show();
            //    formCalDis.ControlUpDate();
            //}
            //catch
            //{
            //    formCalDis = new Form_CalDisplay();
            //}

        }
        private void BTN_CAMERASET_Click(object sender, EventArgs e)
        {
            //if (!MessageShow()) return;
            //bool nResult = false;
            //Form_Password formpassword = new Form_Password(true);
            //formpassword.ShowDialog();
            //nResult = formpassword.LOGINOK;
            //formpassword.Dispose();
            //if (nResult)
            //{
            //    Main.Status.MC_MODE = Main.DEFINE.MC_CAMERAFORM;
            //    Form_CameraSet formcamera = new Form_CameraSet();
            //    formcamera.ShowDialog();
            //    formcamera.Dispose();
            //    Main.Status.MC_MODE = Main.DEFINE.MC_MAINFORM;
            //}
            //LB_CAMERA.Visible = true;
        }
        private void BTN_LIVEVIEW_Click(object sender, EventArgs e)
        {
            //Button nBtn = (Button)sender;

            //int CamNo = (Convert.ToInt32(nBtn.Name.Substring(nBtn.Name.Length - 2, 2)) - 1);

            //try
            //{
            //    formLiveview[cogDisplayCamNo[CamNo]].BTN_EXIT_Click(null, null);
            //    formLiveview[cogDisplayCamNo[CamNo]].Show();
            //    formLiveview[cogDisplayCamNo[CamNo]].FormLoad();

            //}
            //catch
            //{
            //    if (CamNo < Main.DEFINE.CAM_MAX)
            //        formLiveview[cogDisplayCamNo[CamNo]] = new Form_LiveView(cogDisplayCamNo[CamNo]);
            //}
        }


        private void BTN_PASSWORD_Click(object sender, EventArgs e)
        {
            //Form_Password formpassword = new Form_Password(false);
            //formpassword.ShowDialog();
            //formpassword.Dispose();
        }
        private void RBTN_TAB_Click(object sender, EventArgs e)
        {
            //RadioButton TempBTN = (RadioButton)sender;
            //int m_Number;
            //m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 1, 1));
            //TAB_IMG_DISPLAY.SelectedIndex = m_Number;
        }
        private void RBTN_TAB_Checked_Click(object sender, EventArgs e)
        {
            //RadioButton TempBTN = (RadioButton)sender;
            //if (TempBTN.Checked)
            //    TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            //else
            //    TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }

        private void BTN_OVERLAY_CLEAR_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < Main.DEFINE.DISPLAY_MAX; i++)
            //{
            //    Main.DisplayClear(cogDisplay[i]);
            //}
            //for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //{
            //    ListBox_Log[i].Items.Clear();
            //}
            //for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //{
            //    ListBox_Length[i].Items.Clear();
            //}
        }
        private void BTN_START_Click(object sender, EventArgs e)
        {
            //if (Main.DEFINE.OPEN_F)
            //{
            //    for (int i = 0; i < Main.DEFINE.DISPLAY_MAX; i++)
            //    {
            //        Main.DisplayClear(cogDisplay[i]);
            //    }
             
            //    for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //    {
            //        Main.AlignUnit[i].m_UnitBusy = false;
            //        Main.AlignUnit[i].WriteCSVLogFile("111.222,333.444", Main.DEFINE.CAM_SELECT_INSPECT);
            //        Main.AlignUnit[i].WriteCSVLogFile("555.666,777.888", Main.DEFINE.CAM_SELECT_ALIGN);
            //    }
            //}

            //LiveFormHide();
            //this.BTN_STOP.Visible = true;
            //this.BTN_START.Visible = false;

            //if (Main.Status.MC_LIGHT == Main.DEFINE.MC_LIGHT_OFF)
            //{
            //    BTN_LIGHT_ON_Click(null, null);
            //}

            //Main.Status.MC_STATUS = Main.DEFINE.MC_RUN;

            //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + Main.DEFINE.VIS_READY, 9000);
       
            //BTN_FIT_IMAGE_Click(null, null);

            //ReadModuleID();
        }
        private void BTN_STOP_Click(object sender, EventArgs e)
        {
            //this.BTN_START.Visible = true;
            //this.BTN_STOP.Visible = false;
            ////shkang_s 20230706
            ////STOP 시 Main Display Overlay Clear
            //for (int k = 0; k < Main.DEFINE.DISPLAY_MAX; k++)
            //{
            //    Main.DisplayClear(cogDisplay[k]);
            //}
            ////shkang_e
            //Main.Status.MC_STATUS = Main.DEFINE.MC_STOP;

            ////2022 05 09 YSH
            //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + Main.DEFINE.VIS_READY, 0);

            //if (Main.DEFINE.OPEN_F)
            //{
            //    for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //    {
            //        Main.AlignUnit[i].m_UnitBusy = false;
            //        Main.AlignUnit[i].WriteCSVLogFile("111.222,333.444", Main.DEFINE.CAM_SELECT_ALIGN);
            //        Main.AlignUnit[i].WriteCSVLogFile("555.666,777.888", Main.DEFINE.CAM_SELECT_INSPECT);
            //    }
            //}

            //if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2" && Main.MODEL_COPY == true)
            //{
            //    Main.ProjectRename(Main.MODEL_COPY_NAME, Main.MODEL_COPY_INFO);
            //    Main.MODEL_COPY = false;
            //}
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            //if (Main.Status.MC_STATUS != Main.DEFINE.MC_STOP) return;
            //DialogResult result = MessageBox.Show("Do you want EXIT?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //if (result == DialogResult.Yes)
            //{
            //    Save_SystemLog("PROGRAM END", Main.DEFINE.CMD);
            //    Main.Thread_Stop();
            //    Main.ThreadCAM_Stop();
            //    if (ThreadProcM.IsAlive)
            //        ThreadProcM.Abort();
            //    _mUI_Info.DeInitialize();

            //    this.Close();
            //}
        }
        private void Form_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            //try
            //{
            //    timer1.Enabled = false;
            //    timerStatus.Enabled = false;
            //    timer_Directory.Enabled = false;

            //    for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //    {
            //        FormManualSet[i]?.Dispose();
            //        formChart[i]?.Dispose();
            //    }
            //    for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            //    {
            //        formLiveview[i]?.Close();
            //        formLiveview[i]?.Dispose();
            //    }

            //    //       Main.SystemSave();
            //    Thread.Sleep(50);
            //    Thread_Stop();
            //    Main.Thread_Stop();
            //    Main.ThreadCAM_Stop();
            //    Main.Vision_Close();
            //    Thread.Sleep(10);
            //    _mUI_Info.Dispose();
            //    Melsec.Dispose();
            //    Formlogview.Dispose();
            //    form_trayDataview.Dispose();
            //    Pattern_Select.PatternTagSelect.PatternTeach.Dispose();
            //    Pattern_Select.PatternTagSelect.Dispose();
            //    //Pattern_Select.PatternTeach.Dispose();
            //    Pattern_Select.Dispose();
            //    formCalDis.Dispose();
            //    formMessage.Dispose();
            //    Thread.Sleep(10);
            //    Light.Port_Close();
            //    Thread.Sleep(50);
            //}
            //catch (System.Exception ex)
            //{
            //    MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            //}
        }

        #region Thread_관련
        private void Thread_Initial_Start()
        {
            //ThreadProcM = new Thread(new ThreadStart(ThreadProc_M));
            //threadFlag = true;
            //ThreadProcM.Start();
        }
        private void ThreadProc_M()
        {
            //while (threadFlag)
            //{
            //    if (Main.Status.MC_STATUS == Main.DEFINE.MC_RUN)
            //    {
            //        for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //        {
            //            ListBox_TABLength_Display(i);
            //            ListBox_Display(i);
            //            ProcStatus_Display(i);
            //            for (int jj = 0; jj < Main.AlignUnit[i].m_AlignPatTagMax; jj++)
            //            {
            //                Grid_Display(i, jj);
            //                for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[jj]; j++)
            //                {
            //                    Overlay_Display(i, jj, j);
            //                }
            //            }
            //        }
            //    }
            //    if (Main.Status.MC_MODE == Main.DEFINE.MC_MAINFORM && Main.Status.MC_STATUS == Main.DEFINE.MC_STOP)
            //    {
            //        for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            //        {
            //            if (Main.vision.Grab_Flag_End[i])
            //            {
            //                for (int j = 0; j < Main.DEFINE.DISPLAY_MAX; j++)
            //                {
            //                    cogDisplay[+j].Image = Main.vision.CogCamBuf[cogDisplayCamNo[j]];
            //                }
            //            }
            //        }
            //    }
            //    else if (Main.Status.MC_MODE == Main.DEFINE.MC_MAINFORM && Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            //    {
            //        for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            //        {
            //            if (nMainLiveFlag[i] > 0 && Main.vision.Grab_Flag_End[i])
            //            {
            //                cogDisplay[i].Image = Main.vision.CogCamBuf[cogDisplayCamNo[i]];
            //            }
            //        }
            //    }
            //    else if (Main.Status.MC_STATUS == Main.DEFINE.MC_RUN && Main.DEFINE.PROGRAM_TYPE == "ATT_AREA_PC1") //display 및 overlay 뿌려주는 곳 cyh
            //    {
            //        int DisplayIndex = 0;
            //        for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //        {
            //            for (int j = 0; j < Main.AlignUnit[i].m_AlignPatTagMax; j++)
            //            {
            //                if (i == 0 && j == 1)
            //                    DisplayIndex = 2;
            //                else if (i == 1 && j == 1)
            //                    DisplayIndex = 3;
            //                else
            //                    DisplayIndex = i;

            //                if (Main.AlignUnit[i].PAT[j, 0].GrabComplete == true)
            //                {
            //                    cogDisplay[DisplayIndex].InteractiveGraphics.Clear();
            //                    cogDisplay[DisplayIndex].Image = Main.AlignUnit[i].PAT[j, 0].TempImage;
            //                    Main.AlignUnit[i].PAT[j, 0].GrabComplete = false;
            //                }

            //                if (Main.AlignUnit[i].PAT[j, 0].InspComplete == true)
            //                {
            //                    Main.AlignUnit[i].PAT[j, 0].InspComplete = false;
            //                    if (Main.AlignUnit[i].PAT[j, 0].resultDipGraphics != null)
            //                    {
            //                        if (Main.AlignUnit[i].PAT[j, 0].FixtureImage != null)
            //                            cogDisplay[DisplayIndex].Image = Main.AlignUnit[i].PAT[j, 0].FixtureImage;

            //                        cogDisplay[DisplayIndex].InteractiveGraphics.AddList(Main.AlignUnit[i].PAT[j, 0].resultDipGraphics, "Result", false);
            //                        Main.AlignUnit[i].PAT[j, 0].resultDipGraphics.Clear();
            //                    }
            //                    string strRes;
            //                    if (Main.AlignUnit[i].PAT[j, 0].bResult == true)
            //                        strRes = "OK";
            //                    else
            //                        strRes = "NG";

            //                    Main.AlignUnit[i].PAT[j, 0].Save_Image(strRes, cogDisplay[DisplayIndex]);
            //                }
            //            }
            //        }
            //    }
            //    Thread.Sleep(50);
            //}
        }
        private void Thread_Stop()
        {
            //threadFlag = false;
            //if (ThreadProcM != null)
            //{
            //    if (ThreadProcM.IsAlive) ThreadProcM.Abort();
            //}
        }
        #endregion

        #region Display Invoke 관련
        delegate void dVisionSkipeDisplay(bool bFlag);
        public void VisionSkipDisplay(bool bFlag)
        {
            //if (LB_DISPLAY_VISION_SKIP.InvokeRequired)
            //{
            //    LB_DISPLAY_VISION_SKIP.Invoke((MethodInvoker)delegate
            //    {
            //        if (bFlag == true)
            //            LB_DISPLAY_VISION_SKIP.Visible = true;
            //        else
            //            LB_DISPLAY_VISION_SKIP.Visible = false;
            //    });
            //}
            //else
            //{
            //    if (bFlag == true)
            //        LB_DISPLAY_VISION_SKIP.Visible = true;
            //    else
            //        LB_DISPLAY_VISION_SKIP.Visible = false;
            //}
        }

        //delegate void dManualSetForm(Form_ManualSet nForm, int m_AlignNo);
        //private void ManualSetForm(Form_ManualSet nForm, int m_AlignNo)
        //{
            //if (nForm.InvokeRequired)
            //{
            //    dManualSetForm call = new dManualSetForm(ManualSetForm);
            //    nForm.Invoke(call, nForm, m_AlignNo);
            //}
            //else
            //{
            //    nForm.m_AlignNo = m_AlignNo;
            //    nForm.ShowDialog();
            //    GC.Collect();
            //    //                 Main.AlignUnit[m_AlignNo].ManualMatch = false;
            //    //                 Main.AlignUnit[m_AlignNo].ManualMatch_Result = ret;
            //}
       // }
        //delegate void dNgMonitorFormSet(Form_NGMonitor nForm, int m_PatTag);
        //private void NgMonitorFormSet(Form_NGMonitor nForm, int m_PatTag)
        //{
            //if (nForm.InvokeRequired)
            //{
            //    dNgMonitorFormSet call = new dNgMonitorFormSet(NgMonitorFormSet);
            //    nForm.Invoke(call, nForm, m_PatTag);
            //}
            //else
            //{
            //    if (nForm.IsFormLoad)
            //    {
            //        nForm.m_PatTagNo = m_PatTag;
            //        nForm.Form_ImageChange();
            //    }
            //    else
            //    {
            //        nForm.m_PatTagNo = m_PatTag;
            //        nForm.Form_ImageChange();
            //        nForm.ShowDialog();

            //    }
            //    GC.Collect();
            //}
       // }
        delegate void dGrabRefresh(CogRecordDisplay nDisplay, ICogImage nImageBuf);
        public static void GrabDisRefresh(CogRecordDisplay nDisplay, ICogImage nImageBuf)
        {
            //if (nDisplay.InvokeRequired)
            //{
            //    dGrabRefresh call = new dGrabRefresh(GrabDisRefresh);
            //    nDisplay.Invoke(call, nDisplay, nImageBuf);
            //}
            //else
            //{
            //    nDisplay.Image = nImageBuf;
            //}
        }
        object syncLock = new object();
        private void ListBox_TABLength_Display(int AlignNo)
        {
            //string LogMessage = " ";
            //lock (syncLock)
            //{
            //    try
            //    {
            //        if (Main.AlignUnit[AlignNo].m_LogStringLength.Count > 0)
            //        {
            //            LogMessage = Main.AlignUnit[AlignNo].m_LogStringLength.Dequeue();
            //            //InsertList(LB_Lisi_LENGTH, LogMessage);
            //            InsertList(ListBox_Length[AlignNo], LogMessage);
            //        }
            //    }
            //    catch (System.Exception ex)
            //    {
            //    }
            //    finally
            //    {

            //    }
            //}
        }
        private void ListBox_Display(int AlignNo)
        {
            //string LogMessage = " ";
            ////Mutex_lock_LoglistBox[AlignNo].WaitOne();
            //try
            //{
            //    if (Main.AlignUnit[AlignNo].m_LogString.Count > 0)
            //    {
            //        LogMessage = Main.AlignUnit[AlignNo].m_LogString.Dequeue();
            //        InsertList(ListBox_Log[AlignNo], LogMessage);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    //            MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            //}
            //finally
            //{
            //    //Mutex_lock_LoglistBox[AlignNo].ReleaseMutex();
            //}
        }

        delegate void dInsertList(ListBox nlistbox, string str);
        public static void InsertList(ListBox nlistbox, string str)
        {
            //if (nlistbox.InvokeRequired)
            //{
            //    dInsertList call = new dInsertList(InsertList);
            //    nlistbox.Invoke(call, nlistbox, str);
            //}
            //else
            //{
            //    if (str != null)
            //    {
            //        nlistbox.Items.Insert(nlistbox.Items.Count, str);
            //        if (nlistbox.Items.Count > 100)
            //            nlistbox.Items.RemoveAt(0);
            //        nlistbox.SelectedIndex = (nlistbox.Items.Count - 1);
            //    }
            //}
        }





        private void Inspec_Grid_Display(int AlignNo)
        {
            //             Mutex_lock_InspecGridView[AlignNo].WaitOne();
            //             try
            //             {
            //                 if (Main.AlignUnit[AlignNo].m_InspecGridString.Count > 1)
            //                 {
            //                     InspecInsertGridView(InspecGridView[AlignNo], Main.AlignUnit[AlignNo].m_InspecGridString, Main.AlignUnit[AlignNo].nSearchPosition);
            //                 }
            //             }
            //             catch (System.Exception ex)
            //             {
            //                 Main.AlignUnit[AlignNo].m_InspecGridString.Clear();
            //          //       MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            //             }
            //             finally
            //             {
            //                 Mutex_lock_InspecGridView[AlignNo].ReleaseMutex();
            //             }
        }

        private void Grid_Display(int AlignNo, int PatMax)
        {
            //try
            //{
            //    if (Main.AlignUnit[AlignNo].m_MainGridString.Count > Main.AlignUnit[AlignNo].m_AlignPatMax[PatMax] - 1)
            //    {
            //        InsertGridView(GridView_Log[AlignNo], Main.AlignUnit[AlignNo].m_MainGridString);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    Main.AlignUnit[AlignNo].m_MainGridString.Clear();
            //}
            //finally
            //{
            //}
        }
        delegate void dInsertGridView(DataGridView nlistbox, Queue<string[]> str);
        public static void InsertGridView(DataGridView nGridView, Queue<string[]> str)
        {
            //if (nGridView.InvokeRequired)
            //{
            //    dInsertGridView call = new dInsertGridView(InsertGridView);
            //    nGridView.Invoke(call, nGridView, str);
            //}
            //else
            //{
            //    nGridView.Rows.Clear();
            //    lock(str)
            //    {
            //        while (str.Count > 0)
            //        {
            //            string[] LogMessage;
            //            LogMessage = str.Dequeue();
            //            nGridView.Rows.Add(LogMessage);
            //            Thread.Sleep(0);
            //        }
            //    }
               
            //}
        }

        private void Overlay_Display(int AlignNo, int PatTagNo, int PatNo)
        {
            //int nDisNo;

            //if (Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DrawPat == 1)
            //{
            //    nDisNo = Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DisNo;

            //    //Mutex_lock_CogDisplay[nDisNo].WaitOne();
            //    try
            //    {
            //        InsertDisplayPAT(cogDisplay[nDisNo], AlignNo, PatTagNo, PatNo);
            //    }
            //    catch (System.Exception ex)
            //    {
            //        //       MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            //    }
            //    finally
            //    {
            //        Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DrawPat = 0;
            //        //Mutex_lock_CogDisplay[nDisNo].ReleaseMutex();
            //    }
            //}
            //if (Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DrawPat_CAL == 1)
            //{
            //    nDisNo = Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DisNo;

            //    //Mutex_lock_CogDisplay[nDisNo].WaitOne();
            //    try
            //    {
            //        InsertDisplayPAT_CAL(cogDisplay[nDisNo], AlignNo, PatTagNo, PatNo);
            //    }
            //    catch (System.Exception ex)
            //    {
            //        //       MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            //    }
            //    finally
            //    {
            //        Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DrawPat_CAL = 0;
            //        //Mutex_lock_CogDisplay[nDisNo].ReleaseMutex();
            //    }
            //}
            //if (Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DrawPat_CAL_THETA == 1)
            //{
            //    nDisNo = Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DisNo;

            //    //Mutex_lock_CogDisplay[nDisNo].WaitOne();
            //    try
            //    {
            //        InsertDisplayPAT_CAL_Theta(cogDisplay[nDisNo], AlignNo, PatTagNo, PatNo);
            //    }
            //    catch (System.Exception ex)
            //    {
            //        //       MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            //    }
            //    finally
            //    {
            //        Main.AlignUnit[AlignNo].PAT[PatTagNo, PatNo].m_DrawPat_CAL_THETA = 0;
            //        //Mutex_lock_CogDisplay[nDisNo].ReleaseMutex();
            //    }
            //}

            //if (Main.AlignUnit[AlignNo].DrawAll_Pat[PatTagNo] > 0)
            //{
            //    //  int nDrawPatNo = 0; //-> Display Count -> List nDrawPatNo 리스트로 선언해서 저거 카운터 만큼 For문 돌리기
            //    List<int> nDrawPatNo = new List<int>(); //-> Display Count -> List nDrawPatNo 리스트로 선언해서 저거 카운터 만큼 For문 돌리기
            //    List<int> nDrawType = new List<int>();
            //    switch (Main.AlignUnit[AlignNo].DrawAll_Pat[PatTagNo])
            //    {

            //        case Main.DEFINE.OBJ_ALL:
            //            nDrawPatNo.Add(0);
            //            nDrawType.Add(Main.DEFINE.OBJ_ALL);
            //            break;
            //        case Main.DEFINE.TAR_ALL:
            //            nDrawPatNo.Add(2);
            //            nDrawType.Add(Main.DEFINE.TAR_ALL);
            //            break;

            //        case Main.DEFINE.LEFT_ALL:
            //            nDrawPatNo.Add(0);
            //            nDrawType.Add(Main.DEFINE.LEFT_ALL);
            //            break;

            //        case Main.DEFINE.RIGHT_ALL:
            //            nDrawPatNo.Add(1);
            //            nDrawType.Add(Main.DEFINE.RIGHT_ALL);
            //            break;

            //        case Main.DEFINE.OBJTAR_ALL:
            //            if (Main.AlignUnit[AlignNo].m_AlignType[PatTagNo] == Main.DEFINE.M_2CAM2SHOT)
            //            {
            //                nDrawPatNo.Add(0);
            //                nDrawType.Add(Main.DEFINE.LEFT_ALL);

            //                nDrawPatNo.Add(1);
            //                nDrawType.Add(Main.DEFINE.RIGHT_ALL);
            //            }
            //            else
            //            {
            //                nDrawPatNo.Add(0);
            //                nDrawType.Add(Main.DEFINE.OBJTAR_ALL);
            //            }
            //            break;

            //        case Main.DEFINE.CHIPPAT_ALL:
            //            nDrawPatNo.Add(0);
            //            nDrawType.Add(Main.DEFINE.CHIPPAT_ALL);
            //            break;
            //    }

            //    for (int i = 0; i < nDrawPatNo.Count; i++)
            //    {
            //        nDisNo = Main.AlignUnit[AlignNo].PAT[Main.AlignUnit[AlignNo].m_PatTagNo, nDrawPatNo[i]].m_DisNo;
            //        try
            //        {
            //            InsertDisplay(cogDisplay[nDisNo], AlignNo, PatTagNo, PatNo, nDrawType[i]);
            //        }
            //        catch (System.Exception ex)
            //        {
            //        }
            //        finally
            //        {
            //        }
            //    }
            //    Main.AlignUnit[AlignNo].DrawAll_Pat[PatTagNo] = 0;
            //}

        }
        delegate void dDrawResult(CogRecordDisplay nDisplay, int nAlignNo, int nPatTagNo, int nPatNo);
        delegate void dDrawResult_ALL(CogRecordDisplay nDisplay, int nAlignNo, int nPatTagNo, int nPatNo, int nType);
        public static void InsertDisplayPAT(CogRecordDisplay nDisplay, int nAlignNo, int nPatTagNo, int nPatNo)
        {
            //if (nDisplay.InvokeRequired)
            //{
            //    dDrawResult call = new dDrawResult(InsertDisplayPAT);
            //    nDisplay.Invoke(call, nDisplay, nAlignNo, nPatTagNo, nPatNo);
            //}
            //else
            //{
            //    Main.AlignUnit[nAlignNo].PAT[nPatTagNo, nPatNo].DrawResult(nDisplay);

            //    int nDisNo = (Convert.ToInt32(nDisplay.Name.Substring(nDisplay.Name.Length - 2, 2)) - 1);
            //    Main.CrossLine(nDisplay, cogDisplayCamNo[nDisNo]);
            //}
        }
        public static void InsertDisplayPAT_CAL(CogRecordDisplay nDisplay, int nAlignNo, int nPatTagNo, int nPatNo)
        {
            //if (nDisplay.InvokeRequired)
            //{
            //    dDrawResult call = new dDrawResult(InsertDisplayPAT_CAL);
            //    nDisplay.Invoke(call, nDisplay, nAlignNo, nPatTagNo, nPatNo);
            //}
            //else
            //{
            //    Main.AlignUnit[nAlignNo].PAT[nPatTagNo, nPatNo].DrawResultCalibration(nDisplay);
            //}
        }
        public static void InsertDisplayPAT_CAL_Theta(CogRecordDisplay nDisplay, int nAlignNo, int nPatTagNo, int nPatNo)
        {
            //if (nDisplay.InvokeRequired)
            //{
            //    dDrawResult call = new dDrawResult(InsertDisplayPAT_CAL_Theta);
            //    nDisplay.Invoke(call, nDisplay, nAlignNo, nPatTagNo, nPatNo);
            //}
            //else
            //{
            //    Main.AlignUnit[nAlignNo].PAT[nPatTagNo, nPatNo].DrawResultCalibration_Theta(nDisplay);
            //}
        }

        public static void InsertDisplay(CogRecordDisplay nDisplay, int nAlignNo, int nPatTagNo, int nPatNo, int nType)
        {
            //if (nDisplay.InvokeRequired)
            //{
            //    dDrawResult_ALL call = new dDrawResult_ALL(InsertDisplay);
            //    nDisplay.Invoke(call, nDisplay, nAlignNo, nPatTagNo, nPatNo, nType);
            //}
            //else
            //{
            //    Main.AlignUnit[nAlignNo].DrawResultALL(nDisplay, nType);
            //}
        }


        private void ProcStatus_Display(int AlignNo)
        {
            //string strProcTime = " ";
            //try
            //{
            //    if (Main.AlignUnit[AlignNo].m_bDisplayStatus == true)
            //    {
            //        Main.AlignUnit[AlignNo].m_bDisplayStatus = false;
            //        strProcTime = Main.AlignUnit[AlignNo].m_lInOutTime.ToString() + " ms";
            //        if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            //        {
            //            if (AlignNo == 0)
            //                DispProcStatus(LIST_LB_PROC_TIME[2], strProcTime);
            //            else
            //                DispProcStatus(LIST_LB_PROC_TIME[AlignNo - 1], strProcTime);
            //        }
            //        else
            //            DispProcStatus(LIST_LB_PROC_TIME[AlignNo], strProcTime);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //}
            //finally
            //{
            //}
        }

        delegate void dDispProcStatus(Label aLabel, string str);
        public static void DispProcStatus(Label aLabel, string str)
        {
            //if (aLabel.InvokeRequired)
            //{
            //    dDispProcStatus call = new dDispProcStatus(DispProcStatus);
            //    aLabel.Invoke(call, aLabel, str);
            //}
            //else
            //{
            //    if (str != null)
            //    {
            //        aLabel.Text = str;
            //    }
            //}
        }



        #endregion

        public void button1_Click(object sender, EventArgs e)
        {
            //int icmd, iunit, iCam, iPad;
            //try
            //{
            //    if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            //    {
            //        iunit = Convert.ToInt16(TB_COMMANDTEST.Text.Substring(0, 1));
            //        iCam = Convert.ToInt16(TB_COMMANDTEST.Text.Substring(1, 1));
            //        iPad = Convert.ToInt16(TB_COMMANDTEST.Text.Substring(2, 1));
            //        icmd = Convert.ToInt16(TB_COMMANDTEST.Text.Substring(3, 3));
            //        Main.AlignUnit[iunit].ExecuteSearch(iCam, iPad, (ushort)icmd);
            //    }
            //    else
            //    {
            //        iunit = Convert.ToInt16(TB_COMMANDTEST.Text.Substring(0, 2));
            //        icmd = Convert.ToInt16(TB_COMMANDTEST.Text.Substring(2, 4));
            //        Main.AlignUnit[iunit].m_Cmd = icmd;
            //    }
            //}
            //catch
            //{

            //}
        }
        private void button2_Click(object sender, EventArgs e)
        {

        }
        private bool MessageShow()
        {
            return true;
            //if (Main.Status.MC_STATUS != Main.DEFINE.MC_STOP)
            //{
            //    formMessage.LB_MESSAGE.Text = "Machine STOP!!";
            //    formMessage.ShowDialog();
            //    return false;
            //}
            //return true;
        }

        private bool MessageShowPermission()
        {
            return true;
            //if (Main.machine.Permission == Main.ePermission.OPERATOR)
            //{
            //    formMessage.LB_MESSAGE.Text = "Operator는 접근 할 수 없습니다!!";
            //    formMessage.ShowDialog();
            //    return false;
            //}
            //return true;
        }
           

        private void BTN_LIGHT_INITIAL_Click(object sender, EventArgs e)
        {
            //Light.Port_Refresh();
        }
        private void BTN_LIGHT_ON_Click(object sender, EventArgs e)
        {
            //if (Main.Status.MC_STATUS != Main.DEFINE.MC_RUN)
            //{
            //    for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //    {
            //        if (Main.DEFINE.PROGRAM_TYPE == "COP_PC3" || Main.DEFINE.PROGRAM_TYPE == "OLB_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC4")
            //        {
            //            Main.AlignUnit[i].PAT[0, 0].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //        }
            //        else
            //        {
            //            for (int j = 0; j < 1; j++)
            //            {
            //                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            //                    Main.AlignUnit[i].PAT[0, j].SetAllLight(Main.DEFINE.M_LIGHT_LINE);
            //                else
            //                    Main.AlignUnit[i].PAT[0, 0].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //            }
            //        }

            //    }
            //    Main.Status.MC_LIGHT = Main.DEFINE.MC_LIGHT_ON;
            //}
        }
        private void BTN_LIGHT_OFF_Click(object sender, EventArgs e)
        {
            //if (Main.Status.MC_STATUS != Main.DEFINE.MC_RUN)
            //{
            //    for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //    {
            //        for (int j = 0; j < Main.DEFINE.Pattern_Max; j++)
            //        {
            //            Main.AlignUnit[i].PAT[0, j].SetAllLightOFF();
            //        }
            //    }
            //    Main.Status.MC_LIGHT = Main.DEFINE.MC_LIGHT_OFF;
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
            //            case Main.DEFINE.CMD:
            //                nFileName = "SystemLog.txt";
            //                nMessage = Date + nMessage;
            //                break;
            //            case Main.DEFINE.LIGHTCTRL:
            //                nFileName = "CommsLog.txt";
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

        private void Save_ChangeParaLog(string nMessage, string nType)
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
            //            case Main.DEFINE.CMD:
            //                nFileName = "ChangeParaLog.txt";
            //                nMessage = Date + nMessage;
            //                break;
            //            case Main.DEFINE.LIGHTCTRL:
            //                nFileName = "CommsLog.txt";
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



        private void LogFolderShow_Click(object sender, EventArgs e)
        {
            //if (Directory.Exists(Main.LogdataPath))
            //{
            //    System.Diagnostics.Process.Start(Main.LogdataPath);
            //}
        }
        private void BTN_INSPECT_SHOW_Click(object sender, EventArgs e)
        {
            //if (GB_INSPECTION.Visible)
            //{
            //    GB_INSPECTION.Visible = false;
            //    BTN_INSPECT_SHOW.BackColor = System.Drawing.Color.DarkGray;
            //}
            //else
            //{
            //    GB_INSPECTION.Visible = true;
            //    BTN_INSPECT_SHOW.BackColor = System.Drawing.Color.GreenYellow;
            //}
        }
        private void LB_Lisi_DoubleClick(object sender, EventArgs e)
        {
            //try
            //{
            //    int nAlignNo;
            //    nAlignNo = Convert.ToInt16(((System.Windows.Forms.Control)(sender)).Name.Substring(((System.Windows.Forms.Control)(sender)).Name.Length - 2, 2));
            //    nAlignNo = nAlignNo - 1;
            //    //GridView_Log
            //    if (!formChart[nAlignNo].Visible)
            //    {
            //        formChart[nAlignNo].m_AlignNo = nAlignNo;
            //        formChart[nAlignNo].m_PatTagNo = 0;
            //        if (Main.AlignUnit[nAlignNo].m_AlignName == "AOI_INSPECTION"
            //            || Main.AlignUnit[nAlignNo].m_AlignName == "FOB_INSPECT"
            //            || Main.AlignUnit[nAlignNo].m_AlignName == "FOF_INSPECTION1"
            //            || Main.AlignUnit[nAlignNo].m_AlignName == "FOF_INSPECTION2"
            //            || Main.AlignUnit[nAlignNo].m_AlignName == "FOP_INSPECTION1")
            //            formChart[nAlignNo].DISPLAY_MODE = 1;
            //        else
            //            formChart[nAlignNo].DISPLAY_MODE = 0;
            //        formChart[nAlignNo].Show();
            //        formChart[nAlignNo].Form_Load();
            //    }
            //}
            //catch
            //{

            //}
        }
        private void LB_Lisi_LENGTH_DoubleClick(object sender, EventArgs e)
        {
            //try
            //{
            //    int nAlignNo;
            //    nAlignNo = Convert.ToInt16(((System.Windows.Forms.Control)(sender)).Name.Substring(((System.Windows.Forms.Control)(sender)).Name.Length - 2, 2));
            //    nAlignNo = nAlignNo - 1;

            //    if (!formChart[nAlignNo].Visible)
            //    {
            //        formChart[nAlignNo].m_AlignNo = nAlignNo;
            //        formChart[nAlignNo].m_PatTagNo = 0;
            //        formChart[nAlignNo].DISPLAY_MODE = 2;
            //        formChart[nAlignNo].Show();
            //        formChart[nAlignNo].Form_Load();
            //    }
            //}
            //catch
            //{

            //}
        }
        private void MA_Display_DoubleClick(object sender, EventArgs e)
        {
            //CogRecordDisplay nDis = (CogRecordDisplay)sender;

            //int nDisNo = (Convert.ToInt32(nDis.Name.Substring(nDis.Name.Length - 2, 2)) - 1);
            //Main.CrossLine(nDis, cogDisplayCamNo[nDisNo]);
        }
        private void BTN_LIGHT_FPC_Click(object sender, EventArgs e)
        {

            //if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC4")
            //{
            //    for (int i = Main.AlignUnit["PBD1"].m_AlignPatTagMax - 1; i >= 0; i--)
            //    {
            //        Main.AlignUnit["PBD1"].PAT[i, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //        Main.AlignUnit["PBD1"].PAT[i, Main.DEFINE.OBJ_R].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    }
            //    for (int i = Main.AlignUnit["PBD2"].m_AlignPatTagMax - 1; i >= 0; i--)
            //    {
            //        Main.AlignUnit["PBD2"].PAT[i, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //        Main.AlignUnit["PBD2"].PAT[i, Main.DEFINE.OBJ_R].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    }
            //}
            //if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC1")
            //{
            //    Main.AlignUnit[2].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_BLOB);
            //    Main.AlignUnit[3].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_BLOB);
            //}
            //if (Main.DEFINE.PROGRAM_TYPE == "FOF_PC1")
            //{
            //    Main.AlignUnit[0].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_BLOB);
            //    Main.AlignUnit[1].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_BLOB);
            //    Main.AlignUnit[2].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_BLOB);
            //    Main.AlignUnit[3].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_BLOB);
            //}
        }
        private void BTN_LIGHT_PANEL_Click(object sender, EventArgs e)
        {
            //if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC4")
            //{
            //    for (int i = Main.AlignUnit["PBD1"].m_AlignPatTagMax - 1; i >= 0; i--)
            //    {
            //        Main.AlignUnit["PBD1"].PAT[i, Main.DEFINE.TAR_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //        Main.AlignUnit["PBD1"].PAT[i, Main.DEFINE.TAR_R].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    }
            //    for (int i = Main.AlignUnit["PBD2"].m_AlignPatTagMax - 1; i >= 0; i--)
            //    {
            //        Main.AlignUnit["PBD2"].PAT[i, Main.DEFINE.TAR_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //        Main.AlignUnit["PBD2"].PAT[i, Main.DEFINE.TAR_R].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    }
            //}
            //if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC1")
            //{
            //    Main.AlignUnit[2].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    Main.AlignUnit[3].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //}
            //if (Main.DEFINE.PROGRAM_TYPE == "FOF_PC1")
            //{
            //    Main.AlignUnit[0].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    Main.AlignUnit[1].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    Main.AlignUnit[2].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //    Main.AlignUnit[3].PAT[0, Main.DEFINE.OBJ_L].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
            //}
        }
        private void LB_CAMERASHOW_Click(object sender, EventArgs e)
        {
            //if (((System.Windows.Forms.MouseEventArgs)(e)).Button == System.Windows.Forms.MouseButtons.Right)
            //{
            //    if (LB_CAMERA.Visible)
            //    {
            //        LB_CAMERA.Visible = false;
            //    }
            //    else
            //    {
            //        LB_CAMERA.Visible = true;
            //    }
            //}
        }
        private void BTN_TRAY_VIEW_Click(object sender, EventArgs e)
        {
            //form_trayDataview.Show();
            //form_trayDataview.ControlUpDate();
        }

        private bool CameraStatus(ref string nMsg)
        {
            return true;
            //bool nRet = true;
            //nMsg = "";
            //NetworkInterface[] network = NetworkInterface.GetAllNetworkInterfaces();
            //for (int ii = 0; ii < Main.DEFINE.CAM_MAX; ii++)
            //{
            //    for (int i = 0; i < network.Length; i++)
            //    {
            //        if (network[i].NetworkInterfaceType == NetworkInterfaceType.Ethernet && Main.vision.CamName[ii] == network[i].Name)
            //        {
            //            if (network[i].OperationalStatus == OperationalStatus.Down)
            //            {
            //                nMsg = Main.vision.CamName[ii] + "_ Camera disconnect";
            //                nRet = false;
            //                break;
            //            }
            //            else
            //            {
            //            }
            //        }
            //    }
            //}
            //return nRet;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //Main.machine.Inspection_Onf = ((CheckBox)sender).Checked;
        }

        private void BTN_CCLINKTEST_Click(object sender, EventArgs e)
        {
            //ushort iAddr;
            //bool bCmd;
            //try
            //{
            //    iAddr = Convert.ToUInt16(TB_COMMANDTEST.Text.Substring(0, 5), 16);
            //    bCmd = TB_COMMANDTEST.Text.Substring(5, 1).Equals("1");
            //    Main.CCLink_PutBit(iAddr, bCmd);
            //}
            //catch
            //{

            //}
        }

        private void BTN_MXTEST_Click(object sender, EventArgs e)
        {
            //int iAddr;
            //int iCmd;
            //try
            //{
            //    iAddr = Convert.ToInt32(TB_COMMANDTEST.Text.Substring(0, 6));
            //    iCmd = Convert.ToInt32(TB_COMMANDTEST.Text.Substring(6, 4));

            //    //2022 05 09 YSH
            //    Main.WriteDevice(iAddr, iCmd);
            //}
            //catch
            //{

            //}
        }

        private void BTN_FIT_IMAGE_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < Main.DEFINE.DISPLAY_MAX; i++)
            //{
            //    cogDisplay[i].Fit(false);
            //}
        }

        private void timerStatus_Tick(object sender, EventArgs e)
        {
            //if (!Main.DEFINE.OPEN_F)
            //{
            //    // GIGE CAMERA
            //    string nMssage = "";
            //    if (!CameraStatus(ref nMssage))
            //    {
            //        BTN_STOP_Click(null, null);
            //        formMessage.LB_MESSAGE.Text = nMssage + "Start Mode";
            //        if (!formMessage.Visible)
            //        {
            //            //formMessage.ShowDialog();
            //            Save_SystemLog(formMessage.LB_MESSAGE.Text, Main.DEFINE.CMD);
            //        }
            //    }

            //    // RCS Temp
            //    if (Main.RCSCHECK == true && Main.Status.MC_MODE == Main.DEFINE.MC_ERROR)
            //    {
            //        Main.RCSCHECK = false;
            //        BTN_RCS.BackColor = System.Drawing.Color.DarkRed;
            //    }
            //    else
            //    {
            //        Main.RCSCHECK = true;
            //        BTN_RCS.BackColor = System.Drawing.Color.DarkGray;
            //    }
            //    //2023 0110 YSH Vision Skip 표시 기능
            //    VisionSkipDisplay(Main.bVisionSkip);
            //}

        }

        private void BTN_RCS_Click(object sender, EventArgs e)
        {
            //Main.Status.MC_MODE = Main.DEFINE.MC_RCSFORM;

            //form_RCS.ShowDialog();

            //Main.Status.MC_MODE = Main.DEFINE.MC_MAINFORM;
        }

        private void BTN_VISION_RESET_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            //{
            //    Main.AlignUnit[i].m_UnitBusy = false;
            //}
        }

        private void CB_LIVE_CHECK_CAM_Click(object sender, EventArgs e)
        {
            //CheckBox tempCB = (CheckBox)sender;

            //int nCamNo = (Convert.ToInt32(tempCB.Name.Substring(tempCB.Name.Length - 1, 1)) - 1);

            //if (tempCB.Checked)
            //{
            //    // Live true
            //    nMainLiveFlag[nCamNo] = 1;
            //}
            //else
            //{
            //    // false
            //    nMainLiveFlag[nCamNo] = 0;
            //}
        }

        private void CB_LIVE_CHECK_CAM_CheckedChanged(object sender, EventArgs e)
        {
            //CheckBox tempCB = (CheckBox)sender;

            //if (tempCB.Checked)
            //{
            //    tempCB.BackColor = System.Drawing.Color.LawnGreen;
            //}
            //else
            //{
            //    tempCB.BackColor = System.Drawing.Color.DarkGray;
            //}
        }

        

        public void BTN_PERMISSION_Click(object sender, EventArgs e)
        {
            //if (!MessageShow()) return;

            //Main.Status.MC_MODE = Main.DEFINE.MC_PERMISSIONFORM;
            //FormPermission.ShowDialog();
            //Main.Status.MC_MODE = Main.DEFINE.MC_MAINFORM;

            //if(Main.machine.Permission == Main.ePermission.OPERATOR)
            //{
            //    BTN_PERMISSION.Text = "PERMISSON\r\nOPERATOR";
            //}
            //else if(Main.machine.Permission == Main.ePermission.ENGINEER)
            //{ 
            //    BTN_PERMISSION.Text = "PERMISSON\r\nENGINEER";

            //}
            //else if(Main.machine.Permission == Main.ePermission.MAKER)
            //{
            //    BTN_PERMISSION.Text = "PERMISSON\r\nMAKER";
            //}
        }

    }//Form_Main : Form
}// nameSpace

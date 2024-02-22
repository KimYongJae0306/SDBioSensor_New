using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COG
{
    public partial class Form_SetUp : Form
    {
        private bool bModLogCheckPeriod = false;

        private string[] Option = new string[] { "OVERLAY IMG SAVE", "Image(Default JPG) BMP", "LOG DATA SAVE", "LENGTH CHECK USE" }; 
        public Form_SetUp()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            InitialDataGrid();
        }
        private void Form_SetUp_Load(object sender, EventArgs e)
        {
            LB_LANGUAGE.Visible = true;
            switch (COG.Main.ReadRegistryLan())
            {
                case Main.DEFINE.KOREA:
                    RBTN_LAN00.Checked = true;
                    break;
                case Main.DEFINE.CHINA:
                    RBTN_LAN01.Checked = true;
                    break;
                case Main.DEFINE.ENGLISH:
                    RBTN_LAN02.Checked = true;
                    break;
            }
//             if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC3")
//             {
                for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
                {
                    if (Main.AlignUnit[i].m_AlignName == "PBD" || Main.AlignUnit[i].m_AlignName == "PBD1" || Main.AlignUnit[i].m_AlignName == "PBD2")
                        GB_MAP_FUNCTION.Visible = true;
                }
             //   GB_TABLENGTH.Visible = true;
//            }
            ControlUpDate();
            LB_MAPANGLE_DISPLAY(LB_MAP_DATA_INPUT);

            LB_RETRY_COUNT.Text = Main.machine.m_RetryCount.ToString();
            CKD_USE_RETRY.Checked = Main.machine.m_bRetryUse;
            if (CKD_USE_RETRY.Checked == true) 
                LB_RETRY_COUNT.Visible = true;
            else
                LB_RETRY_COUNT.Visible = false;

             PN_USE_RETRY.Location = new System.Drawing.Point(5, 155);
        }
        private void InitialDataGrid()
        {
            // 0     DGV_MANUAL_MATCH
            // 1    DGV_CALIBRATION_DATA
            // 2    DGV_STANDARD_DATA
            // 3    DGV_IMAGE_SAVE
            // 4    DGV_LCHECK_DATA
            // 5    DGV_PMALIGN_USE
            // 6    DGV_DELAY
            // 7    DGV_STANDARD_ANGLE
            // 8    DGV_NG_DISPLAY
            // 9    DGV_LCHECK_STANDARD
            // 10   DGV_SAVEOPTION_DATA
            // 11   DGV_LINEMAX_USE
            // 12   DGV_CAMERA_DIST_DATA

            int DataGridViewCount = 13;
            DataGridView tempdataGridView = new DataGridView();

            for (int j = 0; j < DataGridViewCount; j++)
            {
                if (j == 0) 
                    tempdataGridView = DGV_MANUAL_MATCH;
                if (j == 1) 
                    tempdataGridView = DGV_CALIBRATION_DATA; 
                if (j == 2) 
                    tempdataGridView = DGV_STANDARD_DATA; 
                if (j == 3)
                    tempdataGridView = DGV_IMAGE_SAVE;
                if (j == 4)
                    tempdataGridView = DGV_LCHECK_DATA;
                if (j == 5)
                    tempdataGridView = DGV_PMALIGN_USE;
                if (j == 6)
                    tempdataGridView = DGV_DELAY_DATA;
                if (j == 7)
                    tempdataGridView = DGV_STANDARD_ANGLE;
                if (j == 8)
                    tempdataGridView = DGV_NG_DISPLAY;
                if (j == 9)
                    tempdataGridView = DGV_SAVEOPTION_DATA;
                if (j == 10)
                    tempdataGridView = DGV_LCHECK_STANDARD;
                if (j == 11)
                    tempdataGridView = DGV_LINEMAX_USE;
                if (j == 12)
                    tempdataGridView = DGV_CAMERA_DIST_DATA;

                if (j == 9)
                {
                    if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                        tempdataGridView.RowCount = Option.Length - 1;
                    else
                        tempdataGridView.RowCount = Option.Length;
                }
                else
                    tempdataGridView.RowCount = Main.DEFINE.AlignUnit_Max;

                System.Windows.Forms.DataGridViewCellStyle[] dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle[tempdataGridView.RowCount];
                for (int i = 0; i < tempdataGridView.RowCount; i++)
                {
                    dataGridViewCellStyle[i] = new DataGridViewCellStyle();
                    if (i % 2 == 0)
                        dataGridViewCellStyle[i].BackColor = System.Drawing.Color.Bisque;
                    else
                        dataGridViewCellStyle[i].BackColor = System.Drawing.Color.LightCyan;

                    tempdataGridView.Rows[i].DefaultCellStyle = dataGridViewCellStyle[i];
                    tempdataGridView.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }
        private DataGridViewColumn DataGridViewColumnCheckBoxSet(string ColumName, string CheckName)
        {
            DataGridViewCheckBoxColumn column = new DataGridViewCheckBoxColumn();
            column.HeaderText = CheckName;
            column.Name = ColumName;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            column.FlatStyle = FlatStyle.Standard;
            // column.ThreeState = true;
            column.CellTemplate = new DataGridViewCheckBoxCell(false);

            return column;
        }
        private DataGridViewColumn DataGridViewColumnTextBoxSet(string ColumName, string CheckName)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.HeaderText = CheckName;
            column.Name = ColumName;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            column.CellTemplate = new DataGridViewTextBoxCell();
            return column;
        }
        public void ControlUpDate()
        {
            Main.SystemLoad();
            #region DGV_DISPLAY

            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
             {

                     for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[0] + 1; j++)
                     {
                         DGV_MANUAL_MATCH[j, i].Style.ForeColor = System.Drawing.Color.Black;
                         switch (j)
                         {
                             case 0:
                                 DGV_MANUAL_MATCH[j, i].Value = Main.AlignUnit[i].m_AlignName;
                                 break;
                             default:
                                 DGV_MANUAL_MATCH[j, i].Value = Main.AlignUnit[i].PAT[0, j - 1].m_Manu_Match_Use;
                                 break;
                         }
                     }
                 
                 for (int j = 0; j < DGV_CALIBRATION_DATA.ColumnCount; j++)
                 {
                     DGV_CALIBRATION_DATA[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     if (Main.AlignUnit[i].m_MOT_NOT_USE[j])
                     {
                         DGV_CALIBRATION_DATA[j, i].Selected = false;
                         DGV_CALIBRATION_DATA[j, i].Style.BackColor = System.Drawing.Color.Black;
                     }

                     switch (j)
                     {
                         case 0:
                             DGV_CALIBRATION_DATA[j, i].Value = Main.AlignUnit[i].m_Cal_MOVE_X.ToString();
                             break;
                         case 1:
                             DGV_CALIBRATION_DATA[j, i].Value = Main.AlignUnit[i].m_Cal_MOVE_Y.ToString();
                             break;
                         case 2:
                             DGV_CALIBRATION_DATA[j, i].Value = (Main.AlignUnit[i].m_Cal_MOVE_T1 / 1000.0).ToString();
                             break;
                         case 3:
                             DGV_CALIBRATION_DATA[j, i].Value = (Main.AlignUnit[i].m_Cal_MOVE_T2 / 1000.0).ToString();
                             break;
                     }
                 }

                 for (int j = 0; j < DGV_CAMERA_DIST_DATA.ColumnCount; j++)
                {
                    DGV_CAMERA_DIST_DATA[j, i].Style.ForeColor = System.Drawing.Color.Black;
                    switch (j)
                    {
                        case 0:
                            DGV_CAMERA_DIST_DATA[j, i].Value = Main.AlignUnit[i].m_CamOffsetX;
                            break;
                        case 1:
                            DGV_CAMERA_DIST_DATA[j, i].Value = Main.AlignUnit[i].m_CamOffsetY;
                            break;
                    }
                }                

                 for (int j = 0; j < DGV_STANDARD_DATA.ColumnCount; j++)
                 {
                     DGV_STANDARD_DATA[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_STANDARD_DATA[j, i].Value = Main.AlignUnit[i].m_Standard[Main.DEFINE.X].ToString();
                             break;
                         case 1:
                             DGV_STANDARD_DATA[j, i].Value = Main.AlignUnit[i].m_Standard[Main.DEFINE.Y].ToString();
                             break;
                         case 2:
                             DGV_STANDARD_DATA[j, i].Value = Main.AlignUnit[i].m_Standard[Main.DEFINE.T].ToString();
                             break;
                         case 3:
                             DGV_STANDARD_DATA[j, i].Value = Main.AlignUnit[i].m_RepeatLimit.ToString();
                             break;
                     }
                 }
                 for (int j = 0; j < DGV_LCHECK_STANDARD.ColumnCount; j++)
                 {
                     DGV_LCHECK_STANDARD[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_LCHECK_STANDARD[j, i].Value = Main.AlignUnit[i].m_OBJ_Standard_Length.ToString();
                             break;
                         case 1:
                             DGV_LCHECK_STANDARD[j, i].Value = Main.AlignUnit[i].m_TAR_Standard_Length.ToString();
                             break;
                         case 2:
                             DGV_LCHECK_STANDARD[j, i].Value = "";
                             break;
                     }
                 }
                 for (int j = 0; j < DGV_LCHECK_DATA.ColumnCount; j++)
                 {
                     DGV_LCHECK_DATA[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_LCHECK_DATA[j, i].Value = Main.AlignUnit[i].m_Length_Tolerance.ToString();
                             break;
                         case 1:
                             DGV_LCHECK_DATA[j, i].Value = Main.AlignUnit[i].m_LengthCheck_Use;
                             break;
                     }
                 }

                 for (int j = 0; j < DGV_IMAGE_SAVE.ColumnCount; j++)
                 {
                     DGV_IMAGE_SAVE[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_IMAGE_SAVE[j, i].Value = Main.AlignUnit[i].m_GD_ImageSave_Use;
                             break;
                         case 1:
                             DGV_IMAGE_SAVE[j, i].Value = Main.AlignUnit[i].m_NG_ImageSave_Use;
                             break;
                     }
                 }
                 for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[0]  + 1; j++)
                 {
                     DGV_PMALIGN_USE[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_PMALIGN_USE[j, i].Value = Main.AlignUnit[i].m_AlignName;
                             break;
                         default:
                             DGV_PMALIGN_USE[j, i].Value = Main.AlignUnit[i].PAT[0, j - 1].m_PMAlign_Use;
                             break;
                     }
                 }
                for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[0] + 1; j++)
                {
                    DGV_LINEMAX_USE[j, i].Style.ForeColor = System.Drawing.Color.Black;
                    switch (j)
                    {
                        case 0:
                            DGV_LINEMAX_USE[j, i].Value = Main.AlignUnit[i].m_AlignName;
                            break;
                        default:
                            DGV_LINEMAX_USE[j, i].Value = Main.AlignUnit[i].PAT[0, j - 1].m_UseLineMax;
                            break;
                    }
                }

                for (int j = 0; j < DGV_DELAY_DATA.ColumnCount; j++)
                 {
                     DGV_DELAY_DATA[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_DELAY_DATA[j, i].Value = Main.AlignUnit[i].m_AlignDelay.ToString();
                             break;
                     }
                 }

                 for (int j = 0; j < DGV_STANDARD_ANGLE.ColumnCount; j++)
                 {
                     DGV_STANDARD_ANGLE[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_STANDARD_ANGLE[j, i].Value = Main.AlignUnit[i].m_StandardMark_T.ToString();
                             break;
                     }
                 }

                 for (int j = 0; j < DGV_NG_DISPLAY.ColumnCount; j++)
                 {
                     DGV_NG_DISPLAY[j, i].Style.ForeColor = System.Drawing.Color.Black;
                     switch (j)
                     {
                         case 0:
                             DGV_NG_DISPLAY[j, i].Value = Main.AlignUnit[i].m_Blob_NG_View_Use;
                             break;
                     }
                 }

             }
            for (int j = 0; j < Option.Length; j++)
            {
                if ((Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2") && j > 2)
                    break;

                DGV_SAVEOPTION_DATA[0, j].Style.ForeColor = System.Drawing.Color.Black;
                DGV_SAVEOPTION_DATA[0, j].Value = Option[j].ToString();
                switch (j)
                {
                    case 0:
                        DGV_SAVEOPTION_DATA[1, j].Value = (Main.machine.Overlay_Image_Onf);
                        break;
                    case 1:
                        DGV_SAVEOPTION_DATA[1, j].Value = (Main.machine.BMP_ImageSave_Onf);
                        break;
                    case 2:
                        DGV_SAVEOPTION_DATA[1, j].Value = (Main.machine.LogMsg_Onf);
                        break;
                    case 3:
                        DGV_SAVEOPTION_DATA[1, j].Value = (Main.machine.LengthCheck_Onf);
                        break;
                }
            }
            #endregion


            CB_OPTION_02.Checked = (Main.machine.MAP_Function_Onf); BTN_OPTION_Click(CB_OPTION_02, null);
            CB_OPTION_04.Checked = (Main.machine.MAP_Limit_Onf); BTN_OPTION_Click(CB_OPTION_04, null);
            LB_MAP_DATA_INPUT.Text = Main.machine.MAP_Function_Data.ToString();
            MAP_Limit_High.Text = Main.machine.MAP_High.ToString();
            MAP_Limit_Low.Text = Main.machine.MAP_Low.ToString();
            TB_LOG_CHECK_PERIOD.Text = Main.machine.m_nOldLogCheckPeriod.ToString();
            TB_LOG_CHECK_SPACE.Text = Main.machine.m_nOldLogCheckSpace.ToString();
            LB_SETUP_CCLINK_COMM_DELAY.Text = Main.machine.m_nCCLinkCommDelay_ms.ToString();
        }
        private void DataUpDate()
        {
            
            #region DGV_DISPLAY
            double ntemp;
            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                for (int jj = 0; jj < Main.AlignUnit[i].m_AlignPatTagMax; jj++)
                {
                    for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[jj]; j++)
                    {

                        Main.AlignUnit[i].PAT[jj, j].m_Manu_Match_Use = (bool)DGV_MANUAL_MATCH[j + 1, i].Value;
                    }
                }
                
                for (int j = 0; j < DGV_CALIBRATION_DATA.ColumnCount; j++)
                {
                    switch (j)
                    {

                        case Main.DEFINE.X:
                            long.TryParse(DGV_CALIBRATION_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_Cal_MOVE_X);
                            if (Main.AlignUnit[i].m_MOT_NOT_USE[Main.DEFINE.X]) Main.AlignUnit[i].m_Cal_MOVE_X = 0;
                            break;
                        case Main.DEFINE.Y:
                            long.TryParse(DGV_CALIBRATION_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_Cal_MOVE_Y);
                            if (Main.AlignUnit[i].m_MOT_NOT_USE[Main.DEFINE.Y]) Main.AlignUnit[i].m_Cal_MOVE_Y = 0;
                            break;
                        case Main.DEFINE.T:
                            double.TryParse(DGV_CALIBRATION_DATA[j, i].Value.ToString(), out ntemp);
                            Main.AlignUnit[i].m_Cal_MOVE_T1 = (long)(ntemp * 1000);
                            if (Main.AlignUnit[i].m_MOT_NOT_USE[Main.DEFINE.T]) Main.AlignUnit[i].m_Cal_MOVE_T1 = 0;
                            break;
                        case Main.DEFINE.T2:
                            double.TryParse(DGV_CALIBRATION_DATA[j, i].Value.ToString(), out ntemp);
                            Main.AlignUnit[i].m_Cal_MOVE_T2 = (long)(ntemp * 1000);
                            if (Main.AlignUnit[i].m_MOT_NOT_USE[Main.DEFINE.T2]) Main.AlignUnit[i].m_Cal_MOVE_T2 = 0;
                            break;
                    }
                }

                for (int j = 0; j < DGV_CAMERA_DIST_DATA.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case Main.DEFINE.X:
                            long.TryParse(DGV_CAMERA_DIST_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_CamOffsetX);
                            break;
                        case Main.DEFINE.Y:
                            long.TryParse(DGV_CAMERA_DIST_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_CamOffsetY);
                            break;
                    }
                }

                for (int j = 0; j < DGV_STANDARD_DATA.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case 0:
                            double.TryParse(DGV_STANDARD_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_Standard[Main.DEFINE.X]);
                            break;
                        case 1:
                            double.TryParse(DGV_STANDARD_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_Standard[Main.DEFINE.Y]);
                            break;
                        case 2:
                            double.TryParse(DGV_STANDARD_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_Standard[Main.DEFINE.T]);
                            break;
                        case 3:
                            int.TryParse(DGV_STANDARD_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_RepeatLimit);
                            break;
                    }
                }
                for (int j = 0; j < DGV_LCHECK_STANDARD.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case 0:
                            double.TryParse(DGV_LCHECK_STANDARD[j, i].Value.ToString(), out Main.AlignUnit[i].m_OBJ_Standard_Length);
                            break;
                        case 1:
                            double.TryParse(DGV_LCHECK_STANDARD[j, i].Value.ToString(), out Main.AlignUnit[i].m_TAR_Standard_Length);
                            break;
                    }
                }
                for (int j = 0; j < DGV_LCHECK_DATA.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case 0:
                            double.TryParse(DGV_LCHECK_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_Length_Tolerance);
                            break;
                        case 1:
                            Main.AlignUnit[i].m_LengthCheck_Use = (bool)DGV_LCHECK_DATA[j, i].Value;
                            break;
                    }
                }
             
                for (int j = 0; j < DGV_IMAGE_SAVE.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case 0:
                            Main.AlignUnit[i].m_GD_ImageSave_Use = (bool)DGV_IMAGE_SAVE[j, i].Value;
                            break;
                        case 1:
                            Main.AlignUnit[i].m_NG_ImageSave_Use = (bool)DGV_IMAGE_SAVE[j, i].Value;
                            break;
                    }
                }
                for (int jj = 0; jj < Main.AlignUnit[i].m_AlignPatTagMax; jj++)
                {
                    for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[jj]; j++)
                    {

                        Main.AlignUnit[i].PAT[jj, j].m_PMAlign_Use = (bool)DGV_PMALIGN_USE[j + 1, i].Value;
                    }
                }
                for (int jj = 0; jj < Main.AlignUnit[i].m_AlignPatTagMax; jj++)
                {
                    for (int j = 0; j < Main.AlignUnit[i].m_AlignPatMax[jj]; j++)
                    {

                        Main.AlignUnit[i].PAT[jj, j].m_UseLineMax = (bool)DGV_LINEMAX_USE[j + 1, i].Value;
                    }
                }

                for (int j = 0; j < DGV_DELAY_DATA.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case 0:
                            int.TryParse(DGV_DELAY_DATA[j, i].Value.ToString(), out Main.AlignUnit[i].m_AlignDelay);
                            break;
                    }
                }

                for (int j = 0; j < DGV_STANDARD_ANGLE.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case 0:
                            double.TryParse(DGV_STANDARD_ANGLE[j, i].Value.ToString(), out Main.AlignUnit[i].m_StandardMark_T);
                            break;
                    }
                }

                for (int j = 0; j < DGV_NG_DISPLAY.ColumnCount; j++)
                {
                    switch (j)
                    {
                        case 0:
                            Main.AlignUnit[i].m_Blob_NG_View_Use = (bool)DGV_NG_DISPLAY[j, i].Value;
                            break;
                    }
                }             
            }
            for (int j = 0; j < Option.Length; j++)
            {
                if ((Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2") && j > 2)
                    break;

                switch (j)
                {
                    case 0:
                        Main.machine.Overlay_Image_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                    case 1:
                        Main.machine.BMP_ImageSave_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                    case 2:
                        Main.machine.LogMsg_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                    case 3:
                        Main.machine.LengthCheck_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                }
            }
            #endregion

//                 Main.machine.LogMsg_Onf             = CB_OPTION_00.Checked;
//                 Main.machine.Overlay_Image_Onf      = CB_OPTION_01.Checked;
//                 Main.machine.LengthCheck_Onf        = CB_OPTION_03.Checked;
//                Main.machine.BMP_ImageSave_Onf      = CB_BMP.Checked;

            Main.machine.MAP_Function_Onf = CB_OPTION_02.Checked;
            Main.machine.MAP_Limit_Onf = CB_OPTION_04.Checked;
            double.TryParse(LB_MAP_DATA_INPUT.Text, out Main.machine.MAP_Function_Data);
            int.TryParse(MAP_Limit_High.Text, out Main.machine.MAP_High);
            int.TryParse(MAP_Limit_Low.Text, out Main.machine.MAP_Low);
            int.TryParse(TB_LOG_CHECK_PERIOD.Text, out Main.machine.m_nOldLogCheckPeriod);
            int.TryParse(TB_LOG_CHECK_SPACE.Text, out Main.machine.m_nOldLogCheckSpace);
            int.TryParse(LB_SETUP_CCLINK_COMM_DELAY.Text, out Main.machine.m_nCCLinkCommDelay_ms);
            int.TryParse(LB_RETRY_COUNT.Text, out Main.machine.m_RetryCount);   //shkang

            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                Main.machine.m_bUseLoadingLimit = CB_USE_LOADING_LIMIT.Checked;
                int.TryParse(LB_SETUP_LOADING_X_LIMIT.Text, out Main.machine.m_nLoadingLimitX_um);
                int.TryParse(LB_SETUP_LOADING_Y_LIMIT.Text, out Main.machine.m_nLoadingLimitY_um);
                int.TryParse(LB_SETUP_INSP_LOWER_LIMIT.Text, out Main.machine.m_nInspLowLimit_um);
                int.TryParse(LB_SETUP_INSP_HIGHER_LIMIT.Text, out Main.machine.m_nInspHighLimit_um);
            }
            else if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                Main.machine.m_bUseAlign1AngleLimit = CB_USE_1ST_ALIGN_ANGLE_NG.Checked;
                float.TryParse(LB_SETUP_1ST_ALIGN_CORNER_NG.Text, out Main.machine.m_f1stAlignCornerAngleLimit);
                float.TryParse(LB_SETUP_1ST_ALIGN_VERTICAL_NG.Text, out Main.machine.m_f1stAlignVerticalAngleLimit);
            }
            /*
            int.TryParse(LB_FPC_PICKER1_DIS_X.Text, out Main.machine.m_Fpcpicker1_Dis_X);
            int.TryParse(LB_FPC_PICKER1_DIS_Y.Text, out Main.machine.m_Fpcpicker1_Dis_Y);
            int.TryParse(LB_FPC_PICKER2_DIS_X.Text, out Main.machine.m_Fpcpicker2_Dis_X);
            int.TryParse(LB_FPC_PICKER2_DIS_Y.Text, out Main.machine.m_Fpcpicker2_Dis_Y);
             * */
        }
        private void BTN_OPTION_Click(object sender, EventArgs e)
        {
            CheckBox TempBTN = (CheckBox)sender;
            if (TempBTN.Checked)
            {
                TempBTN.BackColor = System.Drawing.Color.GreenYellow;
            }
            else
            {
                TempBTN.BackColor = System.Drawing.Color.White;
            }
            

        }
        private void DGV_INPUT_DATA_Click(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView TempDGV = (DataGridView)sender;
            if (TempDGV.CurrentCell.GetType().Name != "DataGridViewTextBoxCell") return;

            DataGridViewTextBoxCell nCurrentCell = new DataGridViewTextBoxCell();
            nCurrentCell = TempDGV.CurrentCell as DataGridViewTextBoxCell;
            if (nCurrentCell.InheritedStyle.BackColor == Color.Black)return;

            try
            {
                double nCurData = 0;
                Double.TryParse(nCurrentCell.Value.ToString(), out nCurData);
                Form_KeyPad form_keypad = new Form_KeyPad(-1000000, 1000000, nCurData, "DATA_SETTING", 1);
                form_keypad.ShowDialog();
                nCurrentCell.Value = form_keypad.m_data.ToString();
            }
            catch
            {
                    
            }
        }
        private void DGV_LCHECK_DATA_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView TempDGV = (DataGridView)sender;
            if ((TempDGV.CurrentCell.OwningColumn).HeaderText == "GET")
            {
                DialogResult result = MessageBox.Show("Do you want to Input Measured Data ? " + "\n" + "Measured Length(" + "OBJ:" + (long)Main.AlignUnit[TempDGV.CurrentCell.RowIndex].m_OBJ_Mea_Dis + " ,TAR:" + (long)Main.AlignUnit[TempDGV.CurrentCell.RowIndex].m_TAR_Mea_Dis + ")", "Input", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    TempDGV[0, TempDGV.CurrentCell.RowIndex].Value = (long)Main.AlignUnit[TempDGV.CurrentCell.RowIndex].m_OBJ_Mea_Dis;
                    TempDGV[1, TempDGV.CurrentCell.RowIndex].Value = (long)Main.AlignUnit[TempDGV.CurrentCell.RowIndex].m_TAR_Mea_Dis;
                }
            }
            if (TempDGV.CurrentCell.GetType().Name != "DataGridViewTextBoxCell") return;
            DataGridViewTextBoxCell nCurrentCell = new DataGridViewTextBoxCell();
            nCurrentCell = TempDGV.CurrentCell as DataGridViewTextBoxCell;
            try
            {
                double nCurData = 0;
                Double.TryParse(nCurrentCell.Value.ToString(), out nCurData);
                Form_KeyPad form_keypad = new Form_KeyPad(-500000, 500000, nCurData, "DATA_SETTING", 1);
                form_keypad.ShowDialog();
                nCurrentCell.Value = form_keypad.m_data.ToString();
            }
            catch
            {

            }
        }
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            DataUpDate();
            Main.SystemSave();
            if (bModLogCheckPeriod) Main.SaveOldLogCheckFile();
            this.Hide();
            //this.Close();
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            Main.SystemLoad();
            this.Hide();
            //  this.Close();
        }

        private void LB_MAP_DATA_INPUT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(-100, 100, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            TempLB.Text = form_keypad.m_data.ToString();
            LB_MAPANGLE_DISPLAY(TempLB);
        }
        private void LB_MAPANGLE_DISPLAY(object sender)
        {
            double nTempData = 0;
            Label TempLB = (Label)sender;
            try
            {
                double.TryParse(TempLB.Text, out nTempData);
                if (TempLB.Name == "LB_MAP_ANGLE")
                {
                    nTempData = Math.Tan(nTempData * Main.DEFINE.radian) / 2;
                    LB_MAP_DATA_INPUT.Text = nTempData.ToString("0.00");
                    Main.machine.MAP_Function_Data = nTempData;
                }
                else
                {
                    Main.machine.MAP_Function_Data = nTempData;
                    nTempData = Math.Atan(nTempData * 2) * Main.DEFINE.degree;
                    LB_MAP_ANGLE.Text = nTempData.ToString("0");
                }
            }
            catch
            {

            }
        }
        private void MAP_Limit_High_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(-200, 200, nCurData, "HIgh DATA", 1);
            form_keypad.ShowDialog();
            TempLB.Text = form_keypad.m_data.ToString();

        }
        private void MAP_Limit_Low_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(-200, 200, nCurData, "Low DATA", 1);
            form_keypad.ShowDialog();
            TempLB.Text = form_keypad.m_data.ToString();

        }

        private void LB_DX_DY_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(-50000000, 50000000, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            TempLB.Text = form_keypad.m_data.ToString();
        }

        private void BTN_CALCULATE_Click(object sender, EventArgs e)
        {

            double nTemp = 0;
            double nDx = 0,nDY = 0;
            try 
            { 
                     double.TryParse(LB_DX.Text, out nDx);
                   double.TryParse(LB_DY.Text, out nDY);
                   nTemp = Math.Atan(nDY / nDx) * Main.DEFINE.degree;
            }
            catch
            {

            }
            LB_STANDARD_MARK_T.Text = nTemp.ToString();
        }

        private void CB_OPTION_05_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                if (Main.AlignUnit[i].m_OBJ_Mea_Dis == 0 && Main.AlignUnit[i].m_TAR_Mea_Dis == 0) return;

                Main.AlignUnit[i].m_OBJ_Standard_Length = Main.AlignUnit[i].m_OBJ_Mea_Dis;
                Main.AlignUnit[i].m_TAR_Standard_Length = Main.AlignUnit[i].m_TAR_Mea_Dis;

                if ((bool)DGV_LCHECK_DATA[1, i].Value)
                {
                    DGV_LCHECK_STANDARD[0, i].Value = Main.AlignUnit[i].m_OBJ_Standard_Length.ToString("0");
                    DGV_LCHECK_STANDARD[1, i].Value = Main.AlignUnit[i].m_TAR_Standard_Length.ToString("0");
                }
            }
        }

        private void RBTN_Button_Color_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }
        private void RBTN_LAN_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            int m_Number;
            m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 1, 1));

            switch (m_Number)
            {
                case 0:
                    Main.WriteRegistryLan(Main.DEFINE.KOREA.ToString());
                    break;
                case 1:
                    Main.WriteRegistryLan(Main.DEFINE.CHINA.ToString());
                    break;
                case 2:
                    Main.WriteRegistryLan(Main.DEFINE.ENGLISH.ToString());
                    break;
            }

        }

        private void LB_LANGUAGESHOW_Click(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.MouseEventArgs)(e)).Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (LB_LANGUAGE.Visible)
                {
                    LB_LANGUAGE.Visible = false;
                }
                else
                {
                    LB_LANGUAGE.Visible = true;
                }
            }
        }

        private bool FileCopy(string strOriginFile, string strCopyFile) 
        { 
            System.IO.FileInfo fi = new System.IO.FileInfo(strOriginFile); 
            long iSize = 0; 
            long iTotalSize = fi.Length; //1024 버퍼 사이즈 임의로...
            byte[] bBuf = new byte[104857600]; //동일 파일이 존재하면 삭제 하고 다시하기 위해... 

            if (System.IO.File.Exists(strCopyFile)) 
            {
                System.IO.File.Delete(strCopyFile);
            } //원본 파일 열기...
            System.IO.FileStream fsIn = new System.IO.FileStream(strOriginFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read); //대상 파일 만들기...
            System.IO.FileStream fsOut = new System.IO.FileStream(strCopyFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            while (iSize < iTotalSize) 
            { 
                try
                {
                    int iLen = fsIn.Read(bBuf, 0, bBuf.Length); iSize += iLen; fsOut.Write(bBuf, 0, iLen);
                }
                catch (Exception ex)
                { //파일 연결 해제...
                    fsOut.Flush();
                    fsOut.Close();
                    fsIn.Close(); //에러시 삭제... 
                    if (System.IO.File.Exists(strCopyFile))
                    {
                        System.IO.File.Delete(strCopyFile); 
                    }
                } 
                    return false; 
            } 
            //파일 연결 해제... 
            fsOut.Flush(); 
            fsOut.Close();
            fsIn.Close();
            return true;
        
        }

        private bool FolderCopy(string strOriginFolder, string strCopyFolder)
        { 
            //폴더가 없으면 만듬...
            if (!System.IO.Directory.Exists(strCopyFolder)) 
            { 
                System.IO.Directory.CreateDirectory(strCopyFolder); 
            }
            //파일 목록 불러오기...
            string[] files = System.IO.Directory.GetFiles(strOriginFolder);
            //폴더 목록 불러오기... 
            string[] folders = System.IO.Directory.GetDirectories(strOriginFolder);

            foreach (string file in files) 
            {
                string name = System.IO.Path.GetFileName(file); 
                string dest = System.IO.Path.Combine(strCopyFolder, name);

                FileCopy(file, dest);
            } 
            // foreach 안에서 재귀 함수를 통해서 폴더 복사 및 파일 복사 진행 완료  
            foreach (string folder in folders) 
            { 
                string name = System.IO.Path.GetFileName(folder); 
                string dest = System.IO.Path.Combine(strCopyFolder, name); 
                
                FolderCopy(folder, dest); 
            } 

            return true; 
        }

        private void BTN_DATASAVE_Click(object sender, EventArgs e)
        {

            //원본 폴더 또는 복사 대상 폴더를 선택 하지 않았을 경우 
            if (txtOriginFolder.Text == "" || txtCopyFolder.Text == "")
            { 
                MessageBox.Show("원본 폴더 또는 복사 대상 폴더를 선택해 주세요.", "확 인", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); 
                return; 
            }

            BTN_DATASAVE.Text = "폴더 복사중...";
             BTN_DATASAVE.Refresh();

            if (FolderCopy(txtOriginFolder.Text, txtCopyFolder.Text)) 
            { 
                MessageBox.Show("폴더 복사가 완료 되었습니다.", "확 인", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); 
            } 
            else 
            {
                MessageBox.Show("폴더 복사가 실패 하였습니다.", "확 인", MessageBoxButtons.OK, MessageBoxIcon.Error ); 
            }

            BTN_DATASAVE.Text = "폴더 복사";

        }

        private void BTN_OPEN1_Click(object sender, EventArgs e)
        {
            //원본 폴더 열기... 
            FolderBrowserDialog fbd = new FolderBrowserDialog(); 
            if (fbd.ShowDialog() == DialogResult.OK) 
            { 
                txtOriginFolder.Text = fbd.SelectedPath;
            } 

        }

        private void BTN_OPEN2_Click(object sender, EventArgs e)
        {
            //원본 폴더 열기... 
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtCopyFolder.Text = fbd.SelectedPath;
            } 
        }

        private void BTN_SELECT_Click(object sender, EventArgs e)
        {
            

            Form_KeyBoard formkeyboard_Info = new Form_KeyBoard("Period", 0);
            formkeyboard_Info.ShowDialog();

            TB_TIME.Text = string.Format("{0:000}"+ "일", Convert.ToInt16(formkeyboard_Info.m_ResultString));

            DateTimePicker Timer1 = new DateTimePicker();
            Timer1.Format = DateTimePickerFormat.Time;
            Controls.Add(Timer1);
        }

        private void TB_LOG_CHECK_PERIOD_Click(object sender, EventArgs e)
        {
            TextBox TempTB = (TextBox)sender;
            int nCurData = 0;
            Int32.TryParse(TempTB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(0, 365, nCurData, "DATA_SETTING", 1);
            form_keypad.ShowDialog();
            TempTB.Text = Convert.ToInt32(form_keypad.m_data).ToString();
            if (nCurData != (int)form_keypad.m_data)
                bModLogCheckPeriod = true;
        }

        private void LB_SETUP_LOADING_X_LIMIT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(0, 5000, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            int nNewData = (int)form_keypad.m_data;
            TempLB.Text = nNewData.ToString();
        }

        private void LB_SETUP_LOADING_Y_LIMIT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(0, 5000, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            int nNewData = (int)form_keypad.m_data;
            TempLB.Text = nNewData.ToString();
        }

        private void LB_SETUP_INSP_LOWER_LIMIT_Click(object sender, EventArgs e)
        {

        }

        private void LB_SETUP_INSP_HIGHER_LIMIT_Click(object sender, EventArgs e)
        {

        }

        private void CB_USE_LOADING_LIMIT_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_USE_LOADING_LIMIT.Checked == true)
            {
                Main.machine.m_bUseLoadingLimit = true;
                LB_SETUP_LOADING_X_LIMIT.Enabled = true;
                LB_SETUP_LOADING_Y_LIMIT.Enabled = true;
            }
            else
            {
                Main.machine.m_bUseLoadingLimit = false;
                LB_SETUP_LOADING_X_LIMIT.Enabled = false;
                LB_SETUP_LOADING_Y_LIMIT.Enabled = false;
            }
        }

        private void CB_USE_INSP_LIMIT_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_USE_INSP_LIMIT.Checked == true)
            {
                Main.machine.m_bUseInspLimit = true;
                LB_SETUP_INSP_LOWER_LIMIT.Enabled = true;
                LB_SETUP_INSP_HIGHER_LIMIT.Enabled = true;
            }
            else
            {
                Main.machine.m_bUseInspLimit = false;
                LB_SETUP_INSP_LOWER_LIMIT.Enabled = false;
                LB_SETUP_INSP_HIGHER_LIMIT.Enabled = false;
            }
        }

        private void TB_LOG_CHECK_SPACE_Click(object sender, EventArgs e)
        {
            TextBox TempTB = (TextBox)sender;
            int nCurData = 0;
            Int32.TryParse(TempTB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(100, 1600, nCurData, "DATA_SETTING", 1);
            form_keypad.ShowDialog();
            TempTB.Text = Convert.ToInt32(form_keypad.m_data).ToString();
        }

        private void LB_SETUP_1ST_ALIGN_VERTICAL_NG_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(0, 90, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            float fNewData = (float)form_keypad.m_data;
            TempLB.Text = fNewData.ToString("0.0");
        }

        private void LB_SETUP_1ST_ALIGN_CORNER_NG_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(0, 90, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            float fNewData = (float)form_keypad.m_data;
            TempLB.Text = fNewData.ToString("0.0");
        }

        private void CB_USE_1ST_ALIGN_ANGLE_NG_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_USE_1ST_ALIGN_ANGLE_NG.Checked == true)
            {
                Main.machine.m_bUseAlign1AngleLimit = true;
                LB_SETUP_1ST_ALIGN_CORNER_NG.Enabled = true;
                LB_SETUP_1ST_ALIGN_VERTICAL_NG.Enabled = true;
            }
            else
            {
                Main.machine.m_bUseAlign1AngleLimit = false;
                LB_SETUP_1ST_ALIGN_CORNER_NG.Enabled = false;
                LB_SETUP_1ST_ALIGN_VERTICAL_NG.Enabled = false;
            }
        }

        private void LB_SETUP_CCLINK_COMM_DELAY_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(10, 10000, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            int nNewData = (int)form_keypad.m_data;
            TempLB.Text = nNewData.ToString();
        }

        private void CKD_USE_RETRY_CheckedChanged(object sender, EventArgs e)
        {
            if (CKD_USE_RETRY.Checked == true)
            {
                LB_RETRY_COUNT.Visible = true;
                Main.machine.m_bRetryUse = true;
                //Main.machine.m_RetryCount = 1;
                LB_RETRY_COUNT.Text = Main.machine.m_RetryCount.ToString();
            }
            else
            {
                LB_RETRY_COUNT.Visible = false;
                Main.machine.m_bRetryUse = false;
            }
        }

        private void LB_RETRY_COUNT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            int nRetryCnt = Main.machine.m_RetryCount;
            int.TryParse(TempLB.Text, out nRetryCnt);
            Form_KeyPad form_keypad = new Form_KeyPad(1, 2, nRetryCnt, "INPUT RETRY COUNT (1 ~ 2)", 1);
            form_keypad.ShowDialog();
            int nNewData = (int)form_keypad.m_data;
            TempLB.Text = nNewData.ToString();
            Main.machine.m_RetryCount = Convert.ToInt32(TempLB.Text);
        }

    } //Form_SetUp
}// COG

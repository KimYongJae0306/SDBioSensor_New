﻿using COG.Core;
using COG.Helper;
using COG.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COG.UI.Forms
{
    public partial class SetUpForm : Form
    {
        #region 필드
        private bool _bModLogCheckPeriod = false;
        #endregion

        #region 속성
        private string[] Option = new string[] { "OVERLAY IMG SAVE", "Image(Default JPG) BMP", "LOG DATA SAVE", "LENGTH CHECK USE" };
        #endregion

        #region 생성자
        public SetUpForm()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            InitialDataGrid();
        }
        #endregion

        #region 메서드
        private void Form_SetUp_Load(object sender, EventArgs e)
        {
            LB_LANGUAGE.Visible = true;
            switch (LanguageHelper.ReadRegistryLan())
            {
                case LanguageHelper.KOREA:
                    RBTN_LAN00.Checked = true;
                    break;
                case LanguageHelper.CHINA:
                    RBTN_LAN01.Checked = true;
                    break;
                case LanguageHelper.ENGLISH:
                    RBTN_LAN02.Checked = true;
                    break;
            }

            ControlUpDate();

            LB_RETRY_COUNT.Text = AppsConfig.Instance().m_RetryCount.ToString();
            CKD_USE_RETRY.Checked = AppsConfig.Instance().m_RetryUse;

            if (CKD_USE_RETRY.Checked == true)
                LB_RETRY_COUNT.Visible = true;
            else
                LB_RETRY_COUNT.Visible = false;

            PN_USE_RETRY.Location = new System.Drawing.Point(5, 155);
        }

        private void InitialDataGrid()
        {
            DGV_IMAGE_SAVE.RowCount = 2;
            for (int i = 0; i < DGV_IMAGE_SAVE.RowCount; i++)
            {
                DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
                if (i % 2 == 0)
                    dataGridViewCellStyle.BackColor = System.Drawing.Color.Bisque;
                else
                    dataGridViewCellStyle.BackColor = System.Drawing.Color.LightCyan;

                DGV_IMAGE_SAVE.Rows[i].DefaultCellStyle = dataGridViewCellStyle;
                DGV_IMAGE_SAVE.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            DGV_SAVEOPTION_DATA.RowCount = Option.Length;
            for (int i = 0; i < DGV_SAVEOPTION_DATA.RowCount; i++)
            {
                DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
                if (i % 2 == 0)
                    dataGridViewCellStyle.BackColor = System.Drawing.Color.Bisque;
                else
                    dataGridViewCellStyle.BackColor = System.Drawing.Color.LightCyan;

                DGV_SAVEOPTION_DATA.Rows[i].DefaultCellStyle = dataGridViewCellStyle;
                DGV_SAVEOPTION_DATA.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        public void ControlUpDate()
        {
            AppsConfig.Instance().Load();

            #region DGV_DISPLAY
            InspModel inspModel = ModelManager.Instance().CurrentModel;

            if (inspModel != null)
            {
                for (int i = 0; i < StaticConfig.STAGE_MAX_COUNT; i++)
                {
                    for (int j = 0; j < DGV_IMAGE_SAVE.ColumnCount; j++)
                    {
                        DGV_IMAGE_SAVE[j, i].Style.ForeColor = System.Drawing.Color.Black;
                        switch (j)
                        {
                            case 0:
                                DGV_IMAGE_SAVE[j, i].Value = inspModel.StageUnitList[i].m_GD_ImageSave_Use;
                                break;
                            case 1:
                                DGV_IMAGE_SAVE[j, i].Value = inspModel.StageUnitList[i].m_NG_ImageSave_Use;
                                break;
                        }
                    }
                }
            }

            for (int j = 0; j < Option.Length; j++)
            {
                DGV_SAVEOPTION_DATA[0, j].Style.ForeColor = System.Drawing.Color.Black;
                DGV_SAVEOPTION_DATA[0, j].Value = Option[j].ToString();
                switch (j)
                {
                    case 0:
                        DGV_SAVEOPTION_DATA[1, j].Value = (AppsConfig.Instance().Overlay_Image_Onf);
                        break;
                    case 1:
                        DGV_SAVEOPTION_DATA[1, j].Value = (AppsConfig.Instance().BMP_ImageSave_Onf);
                        break;
                    case 2:
                        DGV_SAVEOPTION_DATA[1, j].Value = (AppsConfig.Instance().LogMsg_Onf);
                        break;
                    case 3:
                        DGV_SAVEOPTION_DATA[1, j].Value = (AppsConfig.Instance().LengthCheck_Onf);
                        break;
                }
            }
            #endregion


            TB_LOG_CHECK_PERIOD.Text = AppsConfig.Instance().m_OldLogCheckPeriod.ToString();
            TB_LOG_CHECK_SPACE.Text = AppsConfig.Instance().m_OldLogCheckSpace.ToString();
        }

        private void DataUpDate()
        {

            #region DGV_DISPLAY

            InspModel inspModel = ModelManager.Instance().CurrentModel;

            if (inspModel != null)
            {
                for (int i = 0; i < StaticConfig.STAGE_MAX_COUNT; i++)
                {
                    for (int j = 0; j < DGV_IMAGE_SAVE.ColumnCount; j++)
                    {
                        switch (j)
                        {
                            case 0:
                                inspModel.StageUnitList[i].m_GD_ImageSave_Use = (bool)DGV_IMAGE_SAVE[j, i].Value;
                                break;
                            case 1:
                                inspModel.StageUnitList[i].m_NG_ImageSave_Use = (bool)DGV_IMAGE_SAVE[j, i].Value;
                                break;
                        }
                    }
                }
            }

            for (int j = 0; j < Option.Length; j++)
            {
                switch (j)
                {
                    case 0:
                        AppsConfig.Instance().Overlay_Image_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                    case 1:
                        AppsConfig.Instance().BMP_ImageSave_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                    case 2:
                        AppsConfig.Instance().LogMsg_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                    case 3:
                        AppsConfig.Instance().LengthCheck_Onf = (bool)DGV_SAVEOPTION_DATA[1, j].Value;
                        break;
                }
            }
            #endregion

            int value = AppsConfig.Instance().m_OldLogCheckPeriod;
            int.TryParse(TB_LOG_CHECK_PERIOD.Text, out value);

            value = AppsConfig.Instance().m_OldLogCheckSpace;
            int.TryParse(TB_LOG_CHECK_SPACE.Text, out value);

            value = AppsConfig.Instance().m_RetryCount;
            int.TryParse(LB_RETRY_COUNT.Text, out value);   //shkang
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

        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            DataUpDate();
            AppsConfig.Instance().Save();

            if (_bModLogCheckPeriod)
                StaticConfig.OldLogCheckFile.SetData("SYSTEM", "LAST_CHECK", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            this.Hide();
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            AppsConfig.Instance().Load();
            this.Hide();
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
                    LanguageHelper.WriteRegistryLan(LanguageHelper.KOREA.ToString());
                    break;
                case 1:
                    LanguageHelper.WriteRegistryLan(LanguageHelper.CHINA.ToString());
                    break;
                case 2:
                    LanguageHelper.WriteRegistryLan(LanguageHelper.ENGLISH.ToString());
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

        private void TB_LOG_CHECK_PERIOD_Click(object sender, EventArgs e)
        {
            TextBox TempTB = (TextBox)sender;
            int nCurData = 0;
            Int32.TryParse(TempTB.Text, out nCurData);
            KeyPadForm form_keypad = new KeyPadForm(0, 365, nCurData, "DATA_SETTING", 1);
            form_keypad.ShowDialog();
            TempTB.Text = Convert.ToInt32(form_keypad.m_data).ToString();
            if (nCurData != (int)form_keypad.m_data)
                _bModLogCheckPeriod = true;
        }

        private void LB_SETUP_LOADING_X_LIMIT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            KeyPadForm form_keypad = new KeyPadForm(0, 5000, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            int nNewData = (int)form_keypad.m_data;
            TempLB.Text = nNewData.ToString();
        }

        private void LB_SETUP_LOADING_Y_LIMIT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            KeyPadForm form_keypad = new KeyPadForm(0, 5000, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            int nNewData = (int)form_keypad.m_data;
            TempLB.Text = nNewData.ToString();
        }

        private void TB_LOG_CHECK_SPACE_Click(object sender, EventArgs e)
        {
            TextBox TempTB = (TextBox)sender;
            int nCurData = 0;
            Int32.TryParse(TempTB.Text, out nCurData);
            KeyPadForm form_keypad = new KeyPadForm(100, 1600, nCurData, "DATA_SETTING", 1);
            form_keypad.ShowDialog();
            TempTB.Text = Convert.ToInt32(form_keypad.m_data).ToString();
        }

        private void LB_SETUP_1ST_ALIGN_VERTICAL_NG_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            KeyPadForm form_keypad = new KeyPadForm(0, 90, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            float fNewData = (float)form_keypad.m_data;
            TempLB.Text = fNewData.ToString("0.0");
        }

        private void LB_SETUP_1ST_ALIGN_CORNER_NG_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            KeyPadForm form_keypad = new KeyPadForm(0, 90, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            float fNewData = (float)form_keypad.m_data;
            TempLB.Text = fNewData.ToString("0.0");
        }

        private void CKD_USE_RETRY_CheckedChanged(object sender, EventArgs e)
        {
            if (CKD_USE_RETRY.Checked == true)
            {
                LB_RETRY_COUNT.Visible = true;
                AppsConfig.Instance().m_RetryUse = true;
                //Main.machine.m_RetryCount = 1;
                LB_RETRY_COUNT.Text = AppsConfig.Instance().m_RetryCount.ToString();
            }
            else
            {
                LB_RETRY_COUNT.Visible = false;
                AppsConfig.Instance().m_RetryUse = false;
            }
        }

        private void LB_RETRY_COUNT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            int nRetryCnt = AppsConfig.Instance().m_RetryCount;
            int.TryParse(TempLB.Text, out nRetryCnt);
            KeyPadForm form_keypad = new KeyPadForm(1, 2, nRetryCnt, "INPUT RETRY COUNT (1 ~ 2)", 1);
            form_keypad.ShowDialog();
            int nNewData = (int)form_keypad.m_data;
            TempLB.Text = nNewData.ToString();
            AppsConfig.Instance().m_RetryCount = Convert.ToInt32(TempLB.Text);
        }
        #endregion
    }
}

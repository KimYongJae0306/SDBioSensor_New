using COG.Helper;
using COG.Settings;
using System;
using System.Windows.Forms;

namespace COG.UI.Forms
{
    public partial class PasswordForm : Form
    {
        private string Mode = "MT";
        public bool LOGINOK = false;
        public bool MASTER_MODE = false;
        public string TempPassword = "";
        public string SelectPermission = "";
        public bool PW_VIEW = false;
        public string NewPassWord { get; set; } = "";
        public string NewPassWordConfirm { get; set; } = "";

        private MessageForm formMessage = new MessageForm();

        public PasswordForm(bool nMASTERMODE)
        {
            InitializeComponent();
            MASTER_MODE = nMASTERMODE;
        }

        private void Form_Password_Load(object sender, EventArgs e)
        {
            RBTN_Mode_0.Checked = true;
            CB_PW_VIEW.Checked = true;
        }

        private void LB_MTMODE_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                LB_CURRENT_PW.Text = PasswordHelper.ReadRegistry(Mode);
            }
        }

        private void LB_INPUT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;

            TempLB.Text = "";
            KeyBoardForm formkeyboard_Info = new KeyBoardForm("Password", 2);
            formkeyboard_Info.ShowDialog();
            TempPassword = formkeyboard_Info.m_ResultString;

            if (PW_VIEW)
            {
                foreach (var temp in TempPassword)
                    TempLB.Text += "*";
            }
            else
            {
                TempLB.Text = TempPassword;
            }
        }

        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            LB_MESSAGE.ForeColor = System.Drawing.Color.Red;
            if (AppsStatus.Instance().CurrentUser == User.ENGINEER)
            {
                var engineerPassword = AppsConfig.Instance().m_EngineerPassword;

                if (engineerPassword == null)
                    engineerPassword = "";

                if (TempPassword != engineerPassword)
                {
                    LB_MESSAGE.Text = "Current PassWord MisMatch";
                    return;
                }
                if (NewPassWord != NewPassWordConfirm)
                {
                    LB_MESSAGE.Text = "Confirm PassWord MisMatch";
                    return;
                }

                engineerPassword = NewPassWord;

                StaticConfig.SystemFile.SetData("PERMISSION_ENGINEER", "PASSWORD", engineerPassword);

                LB_MESSAGE.ForeColor = System.Drawing.Color.BlueViolet;
                LB_MESSAGE.Text = "Change successful";
            }
            else if(AppsStatus.Instance().CurrentUser == User.MAKER)
            {
                var makerPassword = AppsConfig.Instance().m_MakerPassword;

                if (makerPassword == null)
                    makerPassword = "";

                if (TempPassword != makerPassword)
                {
                    LB_MESSAGE.Text = "Current PassWord MisMatch";
                    return;
                }
                if (NewPassWord != NewPassWordConfirm)
                {
                    LB_MESSAGE.Text = "Confirm PassWord MisMatch";
                    return;
                }

                makerPassword = NewPassWord;
                StaticConfig.SystemFile.SetData("PERMISSION_MAKER", "PASSWORD", makerPassword);

                LB_MESSAGE.ForeColor = System.Drawing.Color.BlueViolet;
                LB_MESSAGE.Text = "Change successful";
            }
            return;
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BTN_MODE_CHANGE_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;

            if (TempBTN.Checked)
            {
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
                Mode = TempBTN.Text;
                LB_MTMODE.Text = Mode + " Mode";

                LB_MESSAGE.Text = "";
                LB_CURRENT_PW.Text = "";
                LB_NEW_PW.Text = "";
                LB_CONFIRM_PW.Text = "";
            }
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }

        private void CB_PASSWORD_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_PASSWORD.Checked)
            { 
                formMessage.LB_MESSAGE.Text = "1. Current Password : 현재 비밀번호 입력\r\n2. New Password : 새로운 비밀번호 입력\r\n3. Confirm Password : 비밀번호 확인";
                formMessage.ShowDialog();
                CB_PASSWORD.BackColor = System.Drawing.Color.Honeydew;
                PASSWORD_CHANGE_DISPLAY(true);
            }
            else
            {
                CB_PASSWORD.BackColor = System.Drawing.Color.DarkGray;
                PASSWORD_CHANGE_DISPLAY(false);
            }
        }

        private void PASSWORD_CHANGE_DISPLAY(bool nShow)
        {
            label1.Visible = nShow;
            label2.Visible = nShow;
            LB_NEW_PW.Visible = nShow;
            LB_CONFIRM_PW.Visible = nShow;
            BTN_SAVE.Visible = nShow;
            if (nShow)
                this.Height = 440;
            else
                this.Height = 330;
        }

        private void BTN_RUN_Click(object sender, EventArgs e)
        {
            if (MASTER_MODE)
            {
                if (TempPassword == PasswordHelper.ReadRegistry(Mode) || TempPassword == "") 
                {
                    LOGINOK = true;
                }
                else
                {
                    LB_MESSAGE.ForeColor = System.Drawing.Color.Red;
                    LB_MESSAGE.Text = "You do not have access rights ,접속 권한 없습니다.";
                    LOGINOK = false;
                    return;
                }
                this.Close();
            }
            else
            {
                //if (TempPassword == Main.ReadRegistry(Mode) || TempPassword == "1" || TempPassword == "2")
                if (AppsStatus.Instance().CurrentUser == User.ENGINEER)
                {
                    if (TempPassword == AppsConfig.Instance().m_EngineerPassword)
                    {
                        if (MASTER_MODE)
                        {
                            //if (TempPassword == "VISION12" || TempPassword == "1")
                            if (TempPassword == PasswordHelper.ReadRegistry(Mode))
                            {
                                LOGINOK = true;
                            }
                            else
                            {
                                LB_MESSAGE.ForeColor = System.Drawing.Color.Red;
                                LB_MESSAGE.Text = "You do not have access rights ,접속 권한 없습니다.";
                                LOGINOK = false;
                                return;
                            }
                        }
                        else
                        {
                            LOGINOK = true;

                        }
                        this.Close();
                    }
                    else
                    {
                        LB_MESSAGE.ForeColor = System.Drawing.Color.Red;
                        LB_MESSAGE.Text = "Current PassWord MisMatch";
                        LOGINOK = false;
                        return;
                    }
                }
                else if(AppsStatus.Instance().CurrentUser == User.MAKER)
                {
                    if (TempPassword == AppsConfig.Instance().m_MakerPassword)
                    {
                        if (MASTER_MODE)
                        {
                            //if (TempPassword == "VISION12" || TempPassword == "1")
                            if (TempPassword == PasswordHelper.ReadRegistry(Mode))
                            {
                                LOGINOK = true;
                            }
                            else
                            {
                                LB_MESSAGE.ForeColor = System.Drawing.Color.Red;
                                LB_MESSAGE.Text = "You do not have access rights ,접속 권한 없습니다.";
                                LOGINOK = false;
                                return;
                            }
                        }
                        else
                        {
                            LOGINOK = true;

                        }
                        this.Close();
                    }
                    else
                    {
                        LB_MESSAGE.ForeColor = System.Drawing.Color.Red;
                        LB_MESSAGE.Text = "Current PassWord MisMatch";
                        LOGINOK = false;
                        return;
                    }
                }
            }
        }

        private void LB_PWFORMAT_DoubleClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to Password Reset? ", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                PasswordHelper.WriteRegistry(Mode, "");
            }
        }

        private void LB_PWFORMAT_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                LB_CURRENT_PW.Text = PasswordHelper.ReadRegistry(Mode);
            }
        }

        private void CB_PW_VIEW_CheckedChanged(object sender, EventArgs e)
        {
            LB_CURRENT_PW.Text = "";

            if (CB_PW_VIEW.Checked)
            {
                CB_PW_VIEW.BackColor = System.Drawing.Color.Honeydew;
                PW_VIEW = true;
                LB_CURRENT_PW.Text = TempPassword;
            }
            else
            {
                CB_PW_VIEW.BackColor = System.Drawing.Color.DarkGray;
                PW_VIEW = false;

                foreach (var temp in TempPassword)
                    LB_CURRENT_PW.Text += "*";
            }
        }

        private void LB_NEW_INPUT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;

            TempLB.Text = "";
            KeyBoardForm formkeyboard_Info = new KeyBoardForm("Password", 2);
            formkeyboard_Info.ShowDialog();

            NewPassWord = formkeyboard_Info.m_ResultString;

            if (PW_VIEW)
            {
                foreach (var temp in TempPassword)
                    TempLB.Text += "*";
            }
            else
            {
                TempLB.Text = TempPassword;
            }
        }

        private void LB_NEW_CONFIRM_INPUT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;

            TempLB.Text = "";
            KeyBoardForm formkeyboard_Info = new KeyBoardForm("Password", 2);
            formkeyboard_Info.ShowDialog();

            NewPassWordConfirm = formkeyboard_Info.m_ResultString;

            if (PW_VIEW)
            {
                foreach (var temp in TempPassword)
                    TempLB.Text += "*";
            }
            else
            {
                TempLB.Text = TempPassword;
            }
        }
    }
}

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
    public partial class Form_Password : Form
    {

        private string Mode = "MT";
        public bool LOGINOK = false;
        public bool MASTER_MODE = false;
        public string TempPassword = "";
        public string SelectPermission = "";
        public bool PW_VIEW = false;
        private Form_Message formMessage = new Form_Message();

        public Form_Password(bool nMASTERMODE)
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
                LB_CURRENT_PW.Text = Main.ReadRegistry(Mode);
            }
        }
        private void LB_INPUT_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;

            TempLB.Text = "";
            Form_KeyBoard formkeyboard_Info = new Form_KeyBoard("Password", 2);
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
            if (Main.machine.PermissionCheck == Main.ePermission.ENGINEER)
            {
                if (Main.machine.m_EngineerPassword == null)
                    Main.machine.m_EngineerPassword = "";
                if (TempPassword != Main.machine.m_EngineerPassword)
                {
                    LB_MESSAGE.Text = "Current PassWord MisMatch";
                    return;
                }
                if (LB_NEW_PW.Text != LB_CONFIRM_PW.Text)
                {
                    LB_MESSAGE.Text = "Confirm PassWord MisMatch";
                    return;
                }
                Main.machine.m_EngineerPassword = LB_NEW_PW.Text;
                Main.SystemFile.SetData("PERMISSION_ENGINEER", "PASSWORD", Main.machine.m_EngineerPassword);
                LB_MESSAGE.ForeColor = System.Drawing.Color.BlueViolet;
                LB_MESSAGE.Text = "Change successful";
            }
            else if(Main.machine.PermissionCheck == Main.ePermission.MAKER)
            {
                if (Main.machine.m_MakerPassword == null)
                    Main.machine.m_MakerPassword = "";
                if (TempPassword != Main.machine.m_MakerPassword)
                {
                    LB_MESSAGE.Text = "Current PassWord MisMatch";
                    return;
                }
                if (LB_NEW_PW.Text != LB_CONFIRM_PW.Text)
                {
                    LB_MESSAGE.Text = "Confirm PassWord MisMatch";
                    return;
                }
                Main.machine.m_MakerPassword = LB_NEW_PW.Text;
                Main.SystemFile.SetData("PERMISSION_MAKER", "PASSWORD", Main.machine.m_MakerPassword);
                LB_MESSAGE.ForeColor = System.Drawing.Color.BlueViolet;
                LB_MESSAGE.Text = "Change successful";
            }
            return;
            //shkang_s  기존 비밀번호설정 주석 -->Permission 비밀번호 등록
            //if (TempPassword != Main.ReadRegistry(Mode))
            //{
            //    LB_MESSAGE.Text = "Current PassWord MisMatch";
            //    return;
            //}
            //if (LB_NEW_PW.Text != LB_CONFIRM_PW.Text)
            //{
            //    LB_MESSAGE.Text = "Confirm PassWord MisMatch";
            //    return;
            //}
            //LB_MESSAGE.ForeColor = System.Drawing.Color.BlueViolet;
            //Main.WriteRegistry(Mode, LB_NEW_PW.Text);
            //            Main.WriteRegistry(Mode, TempPassword);
            //LB_MESSAGE.Text = "Change successful";
            //return;
            //shkang_e
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
                if (TempPassword == Main.ReadRegistry(Mode) || TempPassword == "") 
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
                if (Main.machine.PermissionCheck == Main.ePermission.ENGINEER)
                {
                    if (TempPassword == Main.machine.m_EngineerPassword)
                    {
                        if (MASTER_MODE)
                        {
                            //if (TempPassword == "VISION12" || TempPassword == "1")
                            if (TempPassword == Main.ReadRegistry(Mode))
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
                else if(Main.machine.PermissionCheck == Main.ePermission.MAKER)
                {
                    if (TempPassword == Main.machine.m_MakerPassword)
                    {
                        if (MASTER_MODE)
                        {
                            //if (TempPassword == "VISION12" || TempPassword == "1")
                            if (TempPassword == Main.ReadRegistry(Mode))
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
                Main.WriteRegistry(Mode, "");
            }
        }

        private void LB_PWFORMAT_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                LB_CURRENT_PW.Text = Main.ReadRegistry(Mode);
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
    }
}

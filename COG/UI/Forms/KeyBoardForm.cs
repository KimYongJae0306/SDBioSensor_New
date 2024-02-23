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
    public partial class KeyBoardForm : Form
    {
        private const int CODE = 2;

        private int m_Mode;
        public string m_DisplayStr = "";
        private string m_BackString = "";
        public string m_ResultString;

        public KeyBoardForm(string DlgName = " ",int Mode = 1,string DisplayStr = "")
        {
            InitializeComponent();
            this.Text = DlgName;
            m_Mode = Mode;
            m_DisplayStr = DisplayStr;
        }
    
        private void KeyBoardForm_Load(object sender, EventArgs e)
        {
            m_BackString = m_DisplayStr;
            DataUpdate();

            if(m_Mode == 0)
            {
                LB_HIDE_0.Visible = true;
                LB_HIDE_1.Visible = true;
                LB_HIDE_2.Visible = true;
                LB_HIDE_3.Visible = true;
                LB_HIDE_4.Visible = true;
            }
        }
        private void DataUpdate()
        {
            if (m_Mode == 1)
            {
                LB_DISPLAY.Text = m_DisplayStr;
            }
            else
            {
                if(m_Mode == 2)// Password
                {
                    string TempData = "";
                    foreach (var temp in m_DisplayStr)
                    {
                        TempData += "*";
                    }
                    LB_DISPLAY.Text = TempData;
                }
                else
                    LB_DISPLAY.Text = m_DisplayStr;
            }
        }

        private void BTN_NUM_Click(object sender, EventArgs e)
        {
            Button TempBTN = (Button)sender;
            m_DisplayStr = m_DisplayStr + TempBTN.Text.ToString();
            DataUpdate();
        }

        private void BT_CL_Click(object sender, EventArgs e)
        {
            m_DisplayStr = "";
            DataUpdate();
        }

        private void BT_BACK_Click(object sender, EventArgs e)
        {
            if (m_DisplayStr.Length > 0)
            {
                m_DisplayStr = m_DisplayStr.Substring(0, m_DisplayStr.Length - 1);
                DataUpdate();
            }
        }

        private void BTN_SPACE_Click(object sender, EventArgs e)
        {
            m_DisplayStr = m_DisplayStr+" ";
            DataUpdate();
        }

        private void BT_ENT_Click(object sender, EventArgs e)
        {
            if (m_DisplayStr == "" && this.Text != "Password")
            {
                MessageBox.Show("Enter the name!!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            m_ResultString = m_DisplayStr;
            this.Close();
        }

        private void BT_CANCEL_Click(object sender, EventArgs e)
        {
            m_ResultString = m_BackString;
            this.Close();
        }














    }
}

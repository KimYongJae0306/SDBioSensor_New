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
    public partial class Form_LightSet : Form
    {
        private int nAlignNum = 0;
        private int nPatTagNum = 0;
        private int nPatNum = 0;
        private int nLightNum = 0;

        public Form_LightSet(int AlignNum ,int PatTagNum , int PatNum , int LightNum)
        {
            nAlignNum = AlignNum;
            nPatTagNum = PatTagNum;
            nPatNum = PatNum;
            nLightNum = LightNum;
            InitializeComponent();
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                label19.Visible = false;
                label20.Visible = false;
                label21.Visible = false;
                label22.Visible = false;
            }
        }

        private void Form_LightSet_Load(object sender, EventArgs e)
        {
            TB_CONTROL_NUMBER.Text = (Main.AlignUnit[nAlignNum].PAT[nPatTagNum,nPatNum].m_LightCtrl[nLightNum]+1).ToString();
            TB_CHANNEL_NUMBER.Text = (Main.AlignUnit[nAlignNum].PAT[nPatTagNum,nPatNum].m_LightCH[nLightNum]+1).ToString();
            TB_CHANNEL_NAME.Text = Main.AlignUnit[nAlignNum].PAT[nPatTagNum,nPatNum].m_Light_Name[nLightNum];
        }

        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            Main.AlignUnit[nAlignNum].PAT[nPatTagNum,nPatNum].m_LightCtrl[nLightNum] = int.Parse(TB_CONTROL_NUMBER.Text) - 1;
            Main.AlignUnit[nAlignNum].PAT[nPatTagNum,nPatNum].m_LightCH[nLightNum] = int.Parse(TB_CHANNEL_NUMBER.Text) - 1;
            Main.AlignUnit[nAlignNum].PAT[nPatTagNum,nPatNum].m_Light_Name[nLightNum] = TB_CHANNEL_NAME.Text;
            this.Close();
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void LB_NOTUSE_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            TB_CONTROL_NUMBER.Text = TempLB.Text;
        }
        private void LB_CH_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            TB_CHANNEL_NUMBER.Text = TempLB.Text;
        }
        private void label6_Click(object sender, EventArgs e)
        {
            TB_CHANNEL_NAME.Text = "BOX_LIGHT";
        }

        private void label1_Click(object sender, EventArgs e)
        {
            TB_CHANNEL_NAME.Text = "BACK_LIGHT";
        }

        private void label3_Click(object sender, EventArgs e)
        {
            TB_CHANNEL_NAME.Text = "COAXIAL_LIGHT";
        }

        private void label23_Click(object sender, EventArgs e)
        {
            TB_CHANNEL_NAME.Text = "SIDE_LIGHT";
        }
        private void label25_Click(object sender, EventArgs e)
        {
            TB_CHANNEL_NAME.Text = "TOOL_LIGHT";
        }

        private void label4_Click(object sender, EventArgs e)
        {
            TB_CHANNEL_NAME.Text = "";
        }




    }
}

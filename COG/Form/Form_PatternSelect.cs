using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;

namespace COG
{
    public partial class Form_PatternSelect : Form
    {
        public Form_PatternTagSelect PatternTagSelect = new Form_PatternTagSelect();
        //public Form_PatternTeach PatternTeach = new Form_PatternTeach();

        private List<Button> BTN_UNIT = new List<Button>();
        private List<Label> LB_CALNAME = new List<Label>();

        private List<Point> BTN_Location = new List<Point>();

          
        public Form_PatternSelect()
        {
            InitializeComponent();
            Allocate_Array();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);



        }

        private void Allocate_Array()
        {
            for (int i = 0; i < 18; i++)
            {
                string nTempName;
                nTempName = "BTN_UNIT_" + i.ToString("00");
                Button nType1 = (Button)this.Controls[nTempName];
                BTN_UNIT.Add(nType1);
                BTN_Location.Add(nType1.Location);
            }
            if (Main.DEFINE.NON_STANDARD)
            {
                for (int i = 0; i < 6; i++)
                {
                    BTN_UNIT[i].Location = BTN_Location[ 5 - i];
                }              
            }
            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                BTN_UNIT[i].Visible = true;
            }



//             if(!Main.DEFINE.ACF_UNIT_TYPE)
//             {
//                 for (int i = 8; i < Main.DEFINE.AlignUnit_Max; i++)
//                 {
//                     BTN_UNIT[i].Location = BTN_UNIT[i + 1].Location; 
//                 }
            //             }

//             if (Main.DEFINE.PROGRAM_TYPE == "FOF_PC2")
//             {
//                 for (int i = 0; i < 4; i++)
//                 {
//                     string nTempName;
//                     nTempName = "LB_CALNAME_" + i.ToString("00");
//                     Label nLbType = (Label)this.Controls[nTempName];
//                     LB_CALNAME.Add(nLbType);
//                 }
//                 for (int i = 0; i < 4; i++)
//                 {
//                     LB_CALNAME[i].Visible = true;
//                 }
//             }
        }
        private void DOPOGI_Type()
        {

        }
        private void BTN_TEACH_Click(object sender, EventArgs e)
        {
            Button TempBTN = (Button) sender;
            int m_Number;
            m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length-2, 2));


            if (Main.AlignUnit[m_Number].m_AlignPatTagMax > 1)
            {
                PatternTagSelect.m_AlignNo = m_Number;
                PatternTagSelect.ShowDialog();
            }
            else
            {
                //PatternTeach.m_AlignNo = m_Number;
                //PatternTeach.m_PatTagNo = 0;
                //PatternTeach.Init_ListBox();
                //PatternTeach.ShowDialog();
            }
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Form_PatternSelect_FormClosed(object sender, FormClosedEventArgs e)
        {
  //          PatternTeach.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (CB_TOOLSHOW.Visible) 
                CB_TOOLSHOW.Visible  = false;
            else
                CB_TOOLSHOW.Visible = true;

        }

        private void Form_PatternSelect_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                BTN_UNIT[i].Text = Main.AlignUnit[i].m_AlignName;

                if (Main.AlignUnit[i].m_AlignName == "FBD_BLOB1")
                {
                    BTN_UNIT[i].Text = "FBD_BLOB_BAKUP1_3";
                }
                if (Main.AlignUnit[i].m_AlignName == "FBD_BLOB2")
                {
                    BTN_UNIT[i].Text = "FBD_BLOB_BAKUP2_4";
                }
                if (Main.AlignUnit[i].m_AlignName == "CRD_PRE1")
                {
                     BTN_UNIT[i].Text = "UPPER_DIS1";
                }
                if(Main.AlignUnit[i].m_AlignName == "CRD_PRE2")
                {
                    BTN_UNIT[i].Text = "UPPER_DIS2";
                }
                if (Main.AlignUnit[i].m_AlignName == "CRD_PRE3")
                {
                    BTN_UNIT[i].Text = "LOW_DIS1";
                }
                if (Main.AlignUnit[i].m_AlignName == "CRD_PRE4")
                {
                    BTN_UNIT[i].Text = "LOW_DIS2";
                }
            }
        }

    }
}

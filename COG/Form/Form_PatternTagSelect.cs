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
    public partial class Form_PatternTagSelect : Form
    {
        public Form_PatternTeach PatternTeach = new Form_PatternTeach();
        private List<Button> BTN_UNIT = new List<Button>();
        public int m_AlignNo;

        public Form_PatternTagSelect()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            Allocate_Array();
        }

        private void Allocate_Array()
        {
            for (int i = 0; i < 4; i++)
            {
                string nTempName; 
                nTempName = "BTN_PATTERN_TAG_" + i.ToString("00");
                Button nType1 = (Button)this.Controls[nTempName];
                BTN_UNIT.Add(nType1);
            }
        }

        private void Form_PatternTagSelect_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatTagMax; i++)
            {
                BTN_UNIT[i].Visible = true;
                // BTN_UNIT[i].Text = "STAGE " + (i+1).ToString();

                //증평 부턴 2Cam 2Shot 으로 변경
                //Stage위에 Panel 1~4 순서대로 로딩 되었을때 기준
                //Cam 사이 Distance로 인해
                //        ────   ────
                //       │1Shot │ │2Shot  │
                //Cam1 - │Panel1│ │Panel2 │
                //Cam2 - │Panel3│ │Panel4 │
                //        ────   ────
                //          촬상    촬상         

                if (m_AlignNo == 0)
                {
                    BTN_UNIT[i].Text = "INSPECTION " + (m_AlignNo + i + 1).ToString();
                    //if (i == 1)
                    //    BTN_UNIT[i].Text = "PANEL " + (m_AlignNo + i).ToString();
                }
                else
                {
                    BTN_UNIT[i].Text = "INSPECTION " + (m_AlignNo + i + 2).ToString();
                    //if (i == 1)
                    //    BTN_UNIT[i].Text = "PANEL " + (m_AlignNo + i + 2).ToString();
                }
      
                        
            }

            if (Main.AlignUnit[m_AlignNo].m_AlignName == "FBD_FPC")
            {
                BTN_UNIT[0].Text = "FBD_FPC_ALIGN";
                BTN_UNIT[1].Text = "FBD_FPC_BLOB";
            }
           
            if (Main.AlignUnit[m_AlignNo].m_AlignName == "UPPER_INSPECT")
            {
                BTN_UNIT[0].Text = "FRONT_INSPECT";
                BTN_UNIT[1].Text = "REAR_INSPECT";
            }
            if (Main.AlignUnit[m_AlignNo].m_AlignName == "LOW_INSPECT")
            {
                BTN_UNIT[0].Text = "FRONT_INSPECT";
                BTN_UNIT[1].Text = "REAR_INSPECT";
            }
        }

        private void BTN_PATTERN_TAG_Click(object sender, EventArgs e)
        {
            Button TempBTN = (Button)sender;
            int m_Number;
            m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));

            PatternTeach.m_ToolShow = false;

            PatternTeach.m_AlignNo = m_AlignNo;
            PatternTeach.m_PatTagNo = m_Number;
            PatternTeach.m_iSection = 0;
            PatternTeach.Init_ListBox();
            PatternTeach.ShowDialog();
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < BTN_UNIT.Count; i++)
            {
                BTN_UNIT[i].Visible = false;
            }
            Main.AlignUnit[m_AlignNo].m_PatTagNo = 0;
            this.Hide();
        }

    }
}

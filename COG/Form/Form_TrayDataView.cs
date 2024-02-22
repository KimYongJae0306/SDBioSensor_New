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
    public partial class Form_TrayDataView : Form
    {
        public Form_TrayDataView()
        {
            InitializeComponent();
        }
    //    private static int m_AlignNum = 0;

        private static string m_AlignNum = "FPC_TRAY1";
        private static string m_AlignName1 = "FPC_TRAY1";
        private static string m_AlignName2 = "FPC_TRAY2";
        private void Form_TrayDataView_Load(object sender, EventArgs e)
        {
            for(int i=0; i< Main.DEFINE.AlignUnit_Max;i++)
            {
                if(Main.AlignUnit[i].m_AlignName == "FPC_TRAY2")
                {
                    BTN_Tray1.Visible = true;
                    BTN_Tray2.Visible = true;
                    break;
                }
            }
        }
        public void ControlUpDate()
        {
            DataGridView tempdataGridView = new DataGridView();

            tempdataGridView = DGV_TRAY_DATA;


            tempdataGridView.Rows.Clear();

            tempdataGridView.RowCount = Main.AlignUnit[m_AlignNum].m_Tray_Pocket_X * Main.AlignUnit[m_AlignNum].m_Tray_Pocket_Y;

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


            for (int i = 0; i < Main.AlignUnit[m_AlignNum].m_TrayResultData.Count; i++)
            {

                for (int j = 0; j < DGV_TRAY_DATA.ColumnCount; j++)
                {
                    DGV_TRAY_DATA[j, i].Style.ForeColor = System.Drawing.Color.Black;
                    switch (j)
                    {
                        case 0:
                            DGV_TRAY_DATA[j, i].Value = (i + 1).ToString();
                            break;
                        case 1:
                            DGV_TRAY_DATA[j, i].Value = ((long)Main.AlignUnit[m_AlignNum].m_TrayResultData[i].AlignData.X).ToString();
                            break;
                        case 2:
                            DGV_TRAY_DATA[j, i].Value = ((long)Main.AlignUnit[m_AlignNum].m_TrayResultData[i].AlignData.Y).ToString();
                            break;
                        case 3:
                            DGV_TRAY_DATA[j, i].Value = ((long)Main.AlignUnit[m_AlignNum].m_TrayResultData[i].AlignData.T).ToString();
                            break;
                        case 4:
                            DGV_TRAY_DATA[j, i].Value = Main.AlignUnit[m_AlignNum].m_TrayResultData[i].SearchResult.ToString();
                            break;
                    }
                }
            }
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void BTN_Tray2_Click(object sender, EventArgs e)
        {
            m_AlignNum = m_AlignName2;
            ControlUpDate();
        }

        private void BTN_Tray1_Click(object sender, EventArgs e)
        {
            m_AlignNum = m_AlignName1;
            ControlUpDate();
        }
    }
}

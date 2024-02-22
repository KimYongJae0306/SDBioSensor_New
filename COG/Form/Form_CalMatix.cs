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
    public partial class Form_CalMatix : Form
    {
        private int nColInfo = 2;//Unit Name , Info ,  
        private int nNum = 9 ;
        private int Scrollbarsize = 18;

        private List<Label> nAlignLabel = new List<Label>();
        public Form_CalMatix()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            dataGridView.RowCount = nNum * Main.DEFINE.AlignUnit_Max;
            dataGridView.ColumnCount = (Main.DEFINE.PATTERNTAG_MAX * Main.DEFINE.Pattern_Max) + nColInfo;
            InitialDataGrid();              
        }
        private void Form_CalMatix_Load(object sender, EventArgs e)
        {
            ControlUpDate();
        }
        private void InitialDataGrid()
        {
            System.Windows.Forms.DataGridViewCellStyle[] dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle[dataGridView.RowCount];
            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                for (int j = 0; j < nNum; j++)
                {
                    dataGridViewCellStyle[(i * nNum) + j] = new DataGridViewCellStyle();
                    if (i % 2 == 0)
                        dataGridViewCellStyle[(i * nNum) + j].BackColor = System.Drawing.Color.LightSkyBlue;
                    else
                        dataGridViewCellStyle[(i * nNum) + j].BackColor = System.Drawing.Color.NavajoWhite;

                    dataGridView.Rows[(i * nNum) + j].DefaultCellStyle = dataGridViewCellStyle[(i * nNum) + j];
                    dataGridView.Rows[(i * nNum) + j].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                }
            }
            for (int i = 0; i < Main.DEFINE.PATTERNTAG_MAX - 1; i++)
            {
                for (int j = 0; j < Main.DEFINE.Pattern_Max; j++)
                {
                    dataGridView.Columns[(i * Main.DEFINE.Pattern_Max) + j + Main.DEFINE.Pattern_Max + nColInfo].HeaderText = (i + 1).ToString() + "_" + dataGridView.Columns[j + nColInfo].HeaderText;
                    dataGridView.Columns[(i * Main.DEFINE.Pattern_Max) + j + Main.DEFINE.Pattern_Max + nColInfo].Resizable = DataGridViewTriState.True;
                    dataGridView.Columns[(i * Main.DEFINE.Pattern_Max) + j + Main.DEFINE.Pattern_Max + nColInfo].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView.Columns[(i * Main.DEFINE.Pattern_Max) + j + Main.DEFINE.Pattern_Max + nColInfo].SortMode = DataGridViewColumnSortMode.NotSortable;

                }
            }
            int nTempWidth = 0;
            for (int i = 0; i < dataGridView.ColumnCount; i++)
            {
                nTempWidth += dataGridView.Columns[i].Width;
            }
            dataGridView.Size = new System.Drawing.Size(nTempWidth + (Scrollbarsize * Main.DEFINE.PATTERNTAG_MAX), dataGridView.Size.Height); // 10 -> Scoll Size
        }
        public void ControlUpDate()
        {
            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                for (int j = 0; j < Main.DEFINE.Pattern_Max + nColInfo; j++)
                {

                    if( j ==0)
                    {
                        dataGridView[j, 0 + (i * nNum)].Value = Main.AlignUnit[i].m_AlignName;
                    }
                    else if (j == 1)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            dataGridView[j, k + (i * 9)].Value = k;
                        }
                    }
                    else
                    {
                        for (int jj = 0; jj < Main.AlignUnit[i].m_AlignPatTagMax; jj++)
                        {
                            if(j < Main.AlignUnit[i].m_AlignPatMax[jj] + nColInfo)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), k + (i * 9)].Value = Main.AlignUnit[i].PAT[jj, j - 2].CALMATRIX[k].ToString("0.0000000000"); //Matrix 소수점 10자리까지 표시 하고 저장할꺼임
                                }
                            }
                       }
                        
                    }
                }
            }
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}

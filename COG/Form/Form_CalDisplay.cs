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
    public partial class Form_CalDisplay : Form
    {
        private int nColInfo = 2;//Unit Name , Info ,  
        private int nNum = 7;//Unit 별 Row 갯수 
        private int Scrollbarsize = 18;

        private double TempResolution = 0;

        private List<Label> nAlignLabel = new List<Label>();
        private Label[,] nCalListX = new Label[2,2];
        private Label[,] nCalListY = new Label[2, 2];

        private double[] nCalPixel = new double[2];
        private double nDistance = new double();
        private double nResultCalc = new double();

        public Form_CalDisplay()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            dataGridView.RowCount = nNum * Main.DEFINE.AlignUnit_Max;
            dataGridView.ColumnCount = (Main.DEFINE.PATTERNTAG_MAX * Main.DEFINE.Pattern_Max) + nColInfo;
            InitialDataGrid();

            nCalListX[0, 0] = LB_CENTER_B_LX; nCalListX[0, 1] = LB_CENTER_T_LX; nCalListX[1, 0] = LB_CENTER_B_RX; nCalListX[1, 1] = LB_CENTER_T_RX;
            nCalListY[0, 0] = LB_CENTER_B_LY; nCalListY[0, 1] = LB_CENTER_T_LY; nCalListY[1, 0] = LB_CENTER_B_RY; nCalListY[1, 1] = LB_CENTER_T_RY;
        }
        private void InitialDataGrid()
        {
            System.Windows.Forms.DataGridViewCellStyle[] dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle[dataGridView.RowCount];
            for(int i =0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
                for (int j = 0; j < nNum; j++)
                {
                    dataGridViewCellStyle[(i * nNum) + j] = new DataGridViewCellStyle();
                    if (i % 2 ==0)
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
            dataGridView.Size = new System.Drawing.Size(nTempWidth + (Scrollbarsize * Main.DEFINE.PATTERNTAG_MAX), (dataGridView.Rows[0].Height * nNum * Main.DEFINE.AlignUnit_Max) + dataGridView.Rows[0].Height); // 10 -> Scoll Size


         
        }
        public void ControlUpDate()
        {

            //if (Main.DEFINE.PROGRAM_TYPE == "FOF_PC1" || Main.DEFINE.PROGRAM_TYPE == "TFOF_PC1") GB_TRAY_CAL.Visible = true;
            for (int i = 0; i < Main.DEFINE.AlignUnit_Max; i++)
            {
               if( Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_1" || Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_2" || Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_3" || Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_4" || Main.AlignUnit[i].m_AlignName == "ART_PROBE")
                GB_Reel_Caldata.Visible = true;
                for (int j = 0; j < Main.DEFINE.Pattern_Max + nColInfo; j++)
                {
                    if (j== 0)
                    {
                        dataGridView[j, 0 + (i * nNum)].Value = Main.AlignUnit[i].m_AlignName;
                    }
                    else if(j==1)
                    {
                        dataGridView[j, 0 + (i * nNum)].Value = "Resolution X";
                        dataGridView[j, 1 + (i * nNum)].Value = "Resolution Y";
                        dataGridView[j, 2 + (i * nNum)].Value = "CCD Theta X";
                        dataGridView[j, 3 + (i * nNum)].Value = "CCD Theta Y";
                        dataGridView[j, 4 + (i * nNum)].Value = "Center X | Y";
                        dataGridView[j, 5 + (i * nNum)].Value = "CalPos X | Y";
                        dataGridView[j, 6 + (i * nNum)].Value = "FixPos X | Y";
                    }
                    else 
                    {
                        for(int jj = 0; jj < Main.AlignUnit[i].m_AlignPatTagMax ; jj++)
                        {
                            if (j < Main.AlignUnit[i].m_AlignPatMax[jj] + nColInfo)
                            {
                                dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 0 + (i * nNum)].Value = Main.AlignUnit[i].PAT[jj, j - nColInfo].m_CalX[0].ToString("0.000");
                                dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 1 + (i * nNum)].Value = Main.AlignUnit[i].PAT[jj, j - nColInfo].m_CalY[0].ToString("0.000");
                                dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 2 + (i * nNum)].Value = Main.AlignUnit[i].PAT[jj, j - nColInfo].CAMCCDTHETA[0, Main.DEFINE.XPOS].ToString("0.000");
                                dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 3 + (i * nNum)].Value = Main.AlignUnit[i].PAT[jj, j - nColInfo].CAMCCDTHETA[0, Main.DEFINE.YPOS].ToString("0.000");

                                if(j%2 == 0)
                                {
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 4 + (i * nNum)].Value = Main.AlignUnit[i].m_CenterX[jj].ToString("0.000");
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 5 + (i * nNum)].Value = Main.AlignUnit[i].m_CalMotoPosX[jj].ToString("0.000");
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 6 + (i * nNum)].Value = Main.AlignUnit[i].m_CalDisplayCX[jj].ToString("0.000");                                    
                                }
                                else
                                {
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 4 + (i * nNum)].Value = Main.AlignUnit[i].m_CenterY[jj].ToString("0.000");
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 5 + (i * nNum)].Value = Main.AlignUnit[i].m_CalMotoPosY[jj].ToString("0.000");
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 6 + (i * nNum)].Value = Main.AlignUnit[i].m_CalDisplayCY[jj].ToString("0.000");
                                }
                                #region
                                double m_OBJ_Distance = (PLCDataTag.RData[Main.AlignUnit[i].ALIGN_UNIT_ADDR + Main.DEFINE.PLC_OBJ_MARK_LENTH + 1] << 16) | (PLCDataTag.RData[Main.AlignUnit[i].ALIGN_UNIT_ADDR + Main.DEFINE.PLC_OBJ_MARK_LENTH] & 0x0000ffff);

                                if (Math.Abs(Main.AlignUnit[i].m_CalDisplayCX[jj]) > m_OBJ_Distance &&
                                    (Main.AlignUnit[i].m_AlignName == "PBD1" || Main.AlignUnit[i].m_AlignName == "PBD2" || Main.AlignUnit[i].m_AlignName == "PBD_FOF1" || Main.AlignUnit[i].m_AlignName == "PBD_FOF2"))
                                {
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 4 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Red;
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 5 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Red;
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 6 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Red;
                                }
                                else
                                {
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 4 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Black;
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 5 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Black;
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 6 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Black;
                                }
                                #endregion

                                #region
                                if (Math.Abs(Main.AlignUnit[i].PAT[jj, j - nColInfo].CAMCCDTHETA[0, Main.DEFINE.XPOS]) > 0.100)
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 2 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Red;
                                else
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 2 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Black;

                                if (Math.Abs(Main.AlignUnit[i].PAT[jj, j - nColInfo].CAMCCDTHETA[0, Main.DEFINE.YPOS]) > 0.100)
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 3 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Red;
                                else
                                    dataGridView[j + (jj * Main.DEFINE.Pattern_Max), 3 + (i * nNum)].Style.ForeColor = System.Drawing.Color.Black;
                                #endregion
                            }
                        }
                        #region
                        //                         dataGridView1[j, 0 + (i * nNum)].Value = Main.AlignUnit[i].PAT[j-2].m_CalX[0].ToString("0.000");
//                         dataGridView1[j, 1 + (i * nNum)].Value = Main.AlignUnit[i].PAT[j-2].m_CalY[0].ToString("0.000");
//                         dataGridView1[j, 2 + (i * nNum)].Value = Main.AlignUnit[i].PAT[j-2].CAMCCDTHETA[0, Main.DEFINE.XPOS].ToString("0.000");
//                         dataGridView1[j, 3 + (i * nNum)].Value = Main.AlignUnit[i].PAT[j-2].CAMCCDTHETA[0, Main.DEFINE.YPOS].ToString("0.000");
//                         dataGridView1[j, 4 + (i * nNum)].Value = Main.AlignUnit[i].m_CenterX[0].ToString("0.000");
//                         dataGridView1[j, 5 + (i * nNum)].Value = Main.AlignUnit[i].m_CenterY[0].ToString("0.000");

//                         if (Math.Abs(Main.AlignUnit[i].PAT[j-2].CAMCCDTHETA[0, Main.DEFINE.XPOS]) > 0.005)
//                             dataGridView1[j, 2 + (i * 6)].Style.ForeColor = System.Drawing.Color.Red;
//                         else
//                             dataGridView1[j, 2 + (i * 6)].Style.ForeColor = System.Drawing.Color.Black;
// 
//                         if (Math.Abs(Main.AlignUnit[i].PAT[j-2].CAMCCDTHETA[0, Main.DEFINE.YPOS]) > 0.005)
//                             dataGridView1[j, 3 + (i * 6)].Style.ForeColor = System.Drawing.Color.Red;
//                         else
//                             dataGridView1[j, 3 + (i * 6)].Style.ForeColor = System.Drawing.Color.Black;
                        #endregion
                    }


                }
            }

        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void BTN_UNIT_01_Click(object sender, EventArgs e)
        {
            Form_CalMatix formCalmatix = new Form_CalMatix();
            formCalmatix.Show();
        }

        private void BTN_COPY_Click(object sender, EventArgs e)
        {
            int m_PatTagNo = 0;
             int nPatNum = 0;
             DialogResult result = MessageBox.Show("CAL_DATA COPY?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
             if (result == DialogResult.Yes)
             {
                 if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC2")
                 {
                     for (int i = 0; i < 4; i++)
                     {
                         //---------------------------------------------------------------------------------------------------------------------------------------------------------------
                         Array.Copy(Main.AlignUnit[0].PAT[m_PatTagNo, nPatNum].CALMATRIX, Main.AlignUnit[0].PAT[m_PatTagNo + i, 0].CALMATRIX, Main.AlignUnit[0].PAT[m_PatTagNo, nPatNum].CALMATRIX.Length);
                         Main.AlignUnit[0].PAT[m_PatTagNo + i, 0].m_CalX[0] = Main.AlignUnit[0].PAT[m_PatTagNo, nPatNum].m_CalX[0];
                         Main.AlignUnit[0].PAT[m_PatTagNo + i, 0].m_CalY[0] = Main.AlignUnit[0].PAT[m_PatTagNo, nPatNum].m_CalY[0];
                         Main.AlignUnit[0].PAT[m_PatTagNo + i, 0].CAMCCDTHETA[0, Main.DEFINE.XPOS] = Main.AlignUnit[0].PAT[m_PatTagNo, nPatNum].CAMCCDTHETA[0, Main.DEFINE.XPOS];
                         Main.AlignUnit[0].PAT[m_PatTagNo + i, 0].CAMCCDTHETA[0, Main.DEFINE.YPOS] = Main.AlignUnit[0].PAT[m_PatTagNo, nPatNum].CAMCCDTHETA[0, Main.DEFINE.YPOS];
                         Main.AlignUnit[0].PAT[m_PatTagNo + i, 0].Save();
                         //---------------------------------------------------------------------------------------------------------------------------------------------------------------
                     }//FOR
//                      for (int i = 0; i < 4; i++)
//                      {
//                          if (i == Main.DEFINE.OBJ_L || i == Main.DEFINE.OBJ_R)
//                          {
//                              nPatNum = i;
//                          }
//                          if (i == Main.DEFINE.TAR_L || i == Main.DEFINE.TAR_R)
//                          {
//                              nPatNum = i - 2;
//                          }
//                          //---------------------------------------------------------------------------------------------------------------------------------------------------------------
//                          Array.Copy(Main.AlignUnit[1].PAT[m_PatTagNo, nPatNum].CALMATRIX, Main.AlignUnit[0].PAT[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN, i].CALMATRIX, Main.AlignUnit[1].PAT[m_PatTagNo, nPatNum].CALMATRIX.Length);
//                          Main.AlignUnit[0].PAT[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN, i].m_CalX[0] = Main.AlignUnit[1].PAT[m_PatTagNo, nPatNum].m_CalX[0];
//                          Main.AlignUnit[0].PAT[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN, i].m_CalY[0] = Main.AlignUnit[1].PAT[m_PatTagNo, nPatNum].m_CalY[0];
//                          Main.AlignUnit[0].PAT[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN, i].CAMCCDTHETA[0, Main.DEFINE.XPOS] = Main.AlignUnit[1].PAT[m_PatTagNo, nPatNum].CAMCCDTHETA[0, Main.DEFINE.XPOS];
//                          Main.AlignUnit[0].PAT[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN, i].CAMCCDTHETA[0, Main.DEFINE.YPOS] = Main.AlignUnit[1].PAT[m_PatTagNo, nPatNum].CAMCCDTHETA[0, Main.DEFINE.YPOS];
//                          Main.AlignUnit[0].PAT[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN, i].Save();
//                          //---------------------------------------------------------------------------------------------------------------------------------------------------------------
                     }//FOR
 
//                      Main.AlignUnit[0].m_CenterX[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN] = Main.AlignUnit[0].m_CenterX[Main.DEFINE.PBD_HEAD__THETA_ALIGN];
//                      Main.AlignUnit[0].m_CenterY[Main.DEFINE.PBD_PANELSTAGE_XY_ALIGN] = Main.AlignUnit[0].m_CenterY[Main.DEFINE.PBD_HEAD__THETA_ALIGN];
//                      Main.CenterXYSave(0);
 
              }//IF
        }

        private void LB_DX_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            double nCurData = 0;
            Double.TryParse(TempLB.Text, out nCurData);
            Form_KeyPad form_keypad = new Form_KeyPad(-0, 50000000, nCurData, "INPUT DATA", 1);
            form_keypad.ShowDialog();
            TempLB.Text = form_keypad.m_data.ToString();

            double nTemp = 0;
            double nDX = 0, nDY = 2048;
            try
            {
                double.TryParse(LB_DX.Text, out nDX);
                nDX *= 1000;

                nTemp = nDX / nDY;
                TempResolution = nTemp;
            }
            catch
            {

            }
            LB_Resolution.Text = nTemp.ToString();
        }

        private void BTN_COPY_DATA_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("TRAY CAL_DATA COPY?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                    if (Main.DEFINE.PROGRAM_TYPE == "FOF_PC1" || Main.DEFINE.PROGRAM_TYPE == "TFOF_PC1")
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            //---------------------------------------------------------------------------------------------------------------------------------------------------------------
                            Main.AlignUnit["FPC_TRAY1"].PAT[0, i].m_CalX[0] = TempResolution;
                            Main.AlignUnit["FPC_TRAY1"].PAT[0, i].m_CalY[0] = TempResolution;

                            Main.AlignUnit["FPC_TRAY2"].PAT[0, i].m_CalX[0] = TempResolution;
                            Main.AlignUnit["FPC_TRAY2"].PAT[0, i].m_CalY[0] = TempResolution;
                          //  Main.AlignUnit["TRAY_ALIGN"].PAT[0, i].Save();

                            for (int j = 0; j < 2; j++)
                            {
                                Main.SystemFile.SetData(Main.AlignUnit["FPC_TRAY1"].PAT[0, i].m_PatternName, "CAL_X" + j.ToString(), Main.AlignUnit["FPC_TRAY1"].PAT[0, i].m_CalX[j]);
                                Main.SystemFile.SetData(Main.AlignUnit["FPC_TRAY1"].PAT[0, i].m_PatternName, "CAL_Y" + j.ToString(), Main.AlignUnit["FPC_TRAY1"].PAT[0, i].m_CalY[j]);

                                Main.SystemFile.SetData(Main.AlignUnit["FPC_TRAY2"].PAT[0, i].m_PatternName, "CAL_X" + j.ToString(), Main.AlignUnit["FPC_TRAY2"].PAT[0, i].m_CalX[j]);
                                Main.SystemFile.SetData(Main.AlignUnit["FPC_TRAY2"].PAT[0, i].m_PatternName, "CAL_Y" + j.ToString(), Main.AlignUnit["FPC_TRAY2"].PAT[0, i].m_CalY[j]);
                            }

                            //---------------------------------------------------------------------------------------------------------------------------------------------------------------
                        }//FOR
                    }
                    ControlUpDate();
            }

        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView nTControl = (DataGridView)sender;

//            nTemp.SelectedCells[0].Value;

            if(nTControl.SelectedCells[0].Value != null)
            {
                string sTemp = nTControl.SelectedCells[0].Value.ToString();

                int nUnit = nTControl.CurrentCellAddress.Y / nNum;
                int nPatTag = (nTControl.CurrentCellAddress.X - nColInfo) / 4;
                
                double nTempX = Main.AlignUnit[nUnit].m_CalDisplayCX[nPatTag];
                double nTempY = Main.AlignUnit[nUnit].m_CalDisplayCY[nPatTag];
                
                int nTPosX, nTPosY;
                if (nTempX > 0) nTPosX = 1; else if (nTempX < 0) nTPosX = 0; else nTPosX = 0;
                if (nTempY > 0) nTPosY = 1; else if (nTempY < 0) nTPosY = 0; else nTPosY = 0;

                for (int i = 0; i < 2; i++ )
                {
                    for(int j = 0; j < 2;j++)
                    {
                        nCalListX[i, j].Text = "";
                        nCalListY[i, j].Text = "";
                    }

                }

                LB_UNIT_NAME.Text = Main.AlignUnit[nUnit].m_AlignName + "_" + nPatTag.ToString();
                nCalListX[nTPosX, nTPosY].Text = (nTempX / 1000).ToString("0.000");
                nCalListY[nTPosX, nTPosY].Text = (nTempY / 1000).ToString("0.000");  

            }
        }

        private void BTN_REEL_CAL_Click(object sender, EventArgs e)
        {
            Button TempBTN = (Button)sender;
            int m_Number;
            m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));
            int nAlignNum = 0;
            bool nSelect = false;


            for(int i = 0 ; i < Main.DEFINE.AlignUnit_Max;i++)
            {
                if (Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_1" || Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_2" || Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_3" || Main.AlignUnit[i].m_AlignName == "REEL_ALIGN_4" || Main.AlignUnit[i].m_AlignName == "ART_PROBE" || Main.AlignUnit[i].m_AlignName == "ART_PROBE" || Main.AlignUnit[i].m_AlignName == "ART_PROBE")
               {
                    nSelect = true;
               }
            }
            if(!nSelect)return;

            if(m_Number == 0)
            {
                nAlignNum = Main.AlignUnit["REEL_ALIGN_1"].m_AlignNo;
 
            }
            else
            {
                nAlignNum = Main.AlignUnit["REEL_ALIGN_2"].m_AlignNo;
            }
            double nCurData = 0;
            nCurData = Main.AlignUnit[nAlignNum].PAT[0, 0].m_CalX[0];

            Form_KeyPad form_keypad = new Form_KeyPad(0, 200, nCurData, "Low DATA", 1);
            form_keypad.ShowDialog();

            double nRetData = form_keypad.m_data;


            for(int i = 0; i < Main.AlignUnit[nAlignNum].m_AlignPatMax[0];i++)
            {
                Main.AlignUnit[nAlignNum].PAT[0, i].m_CalX[0] = nRetData;
                Main.AlignUnit[nAlignNum].PAT[0, i].m_CalY[0] = nRetData;
                Main.AlignUnit[nAlignNum].PAT[0, i].CAMCCDTHETA[0, Main.DEFINE.XPOS]  = 0;
                Main.AlignUnit[nAlignNum].PAT[0, i].CAMCCDTHETA[0, Main.DEFINE.YPOS]  = 0;
                Main.AlignUnit[nAlignNum].PAT[0, i].SaveCal();
            }
            ControlUpDate();
        }

        private void LB_PIXEL_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;
            int m_Number = Convert.ToInt16(TempLB.Name.Substring(TempLB.Name.Length - 2, 2));

            double nCurData = nCalPixel[m_Number];

            Form_KeyPad form_keypad = new Form_KeyPad(0, 5000, nCurData, TempLB.Name, 1);
            form_keypad.ShowDialog();

            nCalPixel[m_Number] = form_keypad.m_data;
            TempLB.Text = nCalPixel[m_Number].ToString("0.000");
        }

        private void LB_DISTANCE_Click(object sender, EventArgs e)
        {
            Label TempLB = (Label)sender;

            double nCurData = nDistance;

            Form_KeyPad form_keypad = new Form_KeyPad(0, 1000000, nCurData, TempLB.Name, 1);
            form_keypad.ShowDialog();

            nDistance = form_keypad.m_data;
            TempLB.Text = nDistance.ToString();
        }

        private void BTN_CALCULATION_Click(object sender, EventArgs e)
        {
            nResultCalc = nDistance / (Math.Abs(nCalPixel[0] - nCalPixel[1]));

            LB_RESULT.Text = nResultCalc.ToString("0.000");
        }

//------------------------------------------------------

  }  
}
    

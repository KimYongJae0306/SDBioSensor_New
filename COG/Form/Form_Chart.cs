using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;

using JAS.Controls;

using System.IO;

namespace COG
{
    public partial class Form_Chart : Form
    {
        public Form_Chart()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            LightRadio.Add(RBTN_TAG_00);
            LightRadio.Add(RBTN_TAG_01);
            LightRadio.Add(RBTN_TAG_02);
            LightRadio.Add(RBTN_TAG_03);
        }
        private const int AxisX = 0;
        private const int AxisY = 1;

        public int m_AlignNo;
        public int m_PatTagNo;

        private const String DATAX = "ALIGN_X";
        private const String DATAY = "ALIGN_Y";
        private const String DATAT = "ALIGN_T";

        private const String DATAW = "WIDTH";
        private const String DATAH = "HEIGHT";

        String DATA_TAB_L = "TAB_L";
        String DATA_TAB_R = "TAB_R";

        //-------------------------------------------------
        public int DISPLAY_MODE = 0;
        private const int MODE_ALIGNXYT = 0;
        private const int MODE_ALIGNINSPECTION = 1;
        private const int MODE_TABLENGTH = 2;
        //-------------------------------------------------
        private List<RadioButton> LightRadio = new List<RadioButton>();

        public void Form_Load()
        {
            CB_OBJ_DATA.Checked = false;
            if (Main.AlignUnit[m_AlignNo].m_AlignName == "PBD")
            {
                CB_OBJ_DATA.Visible = true;
            }
            else
            {
                CB_OBJ_DATA.Visible = false;
            }

            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatTagMax; i++)
            {
                LightRadio[i].Visible = true;
            }
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Form_MouseWheelEvent);
            timer1.Enabled = true;
            LightRadio[0].Checked = true;
            RefreshDisplay();
        }
        private void RBTN_PAT_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            int m_Number;
            m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 1, 1));
            if (m_PatTagNo == m_Number) return;
            m_PatTagNo = m_Number;
            RefreshDisplay();
        }
        private void BTN_PAT_CHANGE_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;

            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }
        public void RefreshDisplay()
        {
            switch (DISPLAY_MODE)
            {
                case MODE_ALIGNXYT:
                    DrawAlignXYT(MSChart);
                    break;
                case MODE_ALIGNINSPECTION:
                    DrawAlignInspection(MSChart);
                    break;
                case MODE_TABLENGTH:
                    DrawTABLength(MSChart);
                    break;
            }
        }
        private void DrawAlignXYT(Chart nChart)
        {
            try
            {
                nChart.ChartAreas.Clear();
                nChart.Series.Clear();
                nChart.Titles.Clear();

                AddTitles(nChart, Main.AlignUnit[m_AlignNo].m_AlignName);

                CreateChart(nChart, DATAX, DATAX, Color.Blue);
                CreateChart(nChart, DATAY, DATAY, Color.Red);
                CreateChart(nChart, DATAT, DATAT, Color.Purple);

                //    GraphInputData(nChart.Series[DATAX], LToA(Main.AlignUnit[m_AlignNo].m_LogStageX[m_PatTagNo]));            
                //    GraphInputData(nChart.Series[DATAY], LToA(Main.AlignUnit[m_AlignNo].m_LogStageY[m_PatTagNo]));
                //   GraphInputData(nChart.Series[DATAT], LToA(Main.AlignUnit[m_AlignNo].m_LogStageT[m_PatTagNo]));
                if (CB_OBJ_DATA.Checked)
                {
                    GraphInputData(nChart, DATAX, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].m_LogCenterX[m_PatTagNo]);
                    GraphInputData(nChart, DATAY, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].m_LogCenterY[m_PatTagNo]);
                    GraphInputData(nChart, DATAT, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].m_LogCenterT[m_PatTagNo]);
                }
                else
                {
                    GraphInputData(nChart, DATAX, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].m_LogStageX[m_PatTagNo]);
                    GraphInputData(nChart, DATAY, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].m_LogStageY[m_PatTagNo]);
                    GraphInputData(nChart, DATAT, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].m_LogStageT[m_PatTagNo]);
                }


            }
            catch (System.Exception ex)
            {

            }
        }
        private void GraphInputData(Chart nChart, String nChartAreaName, int nAxes, Color nColor, List<double> nAxis_Data)
        {
            try
            {
                GraphInputData(nChart.Series[nChartAreaName], LToA(nAxis_Data));
                string nTitleName;
                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                    nTitleName = "STDEV:    " + SampleStandardDeviation(nAxis_Data).ToString("0.000") + "\n" + "AVE    :    " + nAxis_Data.Average().ToString("0.000") + "\n" + "MAX    :   " + nAxis_Data.Max().ToString("0.000") + "\n" + "MIN    :    " + nAxis_Data.Min().ToString("0.000") + "\n" + "MAX-MIN :    " + Math.Abs(nAxis_Data.Max()-nAxis_Data.Min()).ToString("0.000");
                else
                    nTitleName = "STDEV:    " + SampleStandardDeviation(nAxis_Data).ToString("00.0") + "\n" + "AVE    :    " + nAxis_Data.Average().ToString("00.0") + "\n" + "MAX    :   " + nAxis_Data.Max().ToString("00.0") + "\n" + "MIN    :    " + nAxis_Data.Min().ToString("00.0");
                nChart.ChartAreas[nChartAreaName].Axes[nAxes].LabelStyle.ForeColor = nColor;
                nChart.ChartAreas[nChartAreaName].Axes[nAxes].TitleForeColor = nColor;
                nChart.ChartAreas[nChartAreaName].Axes[nAxes].Title = nTitleName;
                nChart.ChartAreas[nChartAreaName].Axes[nAxes].TitleAlignment = StringAlignment.Near;
            }
            catch
            {

            }

        }
        private static double StandardDeviation(List<double> numberSet, double divisor)
        {
            double mean = numberSet.Average();
            return Math.Sqrt(numberSet.Sum(x => Math.Pow(x - mean, 2)) / divisor);
        }

        static double PopulationStandardDeviation(List<double> numberSet)
        {
            return StandardDeviation(numberSet, numberSet.Count);
        }

        static double SampleStandardDeviation(List<double> numberSet)
        {
            return StandardDeviation(numberSet, numberSet.Count - 1);
        }

        private void DrawAlignInspection(Chart nChart)
        {
            try
            {
                nChart.ChartAreas.Clear();
                nChart.Series.Clear();
                nChart.Titles.Clear();


                AddTitles(nChart, Main.AlignUnit[m_AlignNo].m_AlignName);

                string DataW, DataH;
                for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatTagMax; i++)
                {
                    DataW = DATAW + i.ToString();
                    DataH = DATAH + i.ToString();

                    CreateChart(nChart, DataW, DataW, Color.Blue);
                    CreateChart(nChart, DataH, DataH, Color.Red);

                    GraphInputData(nChart.Series[DataW], LToA(Main.AlignUnit[m_AlignNo].Aligndata[i].nLengthWidth));
                    GraphInputData(nChart.Series[DataH], LToA(Main.AlignUnit[m_AlignNo].Aligndata[i].nLengthHeight));

                    DrawStripLine(nChart.ChartAreas[DataW].AxisY, DATAW + "_SPEC", Main.AlignUnit[m_AlignNo].m_Standard[Main.DEFINE.WIDTH_], Color.Green);
                    DrawStripLine(nChart.ChartAreas[DataH].AxisY, DATAH + "_SPEC", Main.AlignUnit[m_AlignNo].m_Standard[Main.DEFINE.HEIGHT], Color.Green);
                }

            }
            catch (System.Exception ex)
            {

            }
        }
        private void DrawTABLength(Chart nChart)
        {
            try
            {
                nChart.ChartAreas.Clear();
                nChart.Series.Clear();
                nChart.Titles.Clear();

                AddTitles(nChart, Main.AlignUnit[m_AlignNo].m_AlignName);

                if (Main.AlignUnit[m_AlignNo].m_AlignName == "PBD" || Main.AlignUnit[m_AlignNo].m_AlignName == "PBD_FOF")
                {
                    DATA_TAB_L = "OBJ_Distance";
                    DATA_TAB_R = "TAR_Distance";
                }



                CreateChart(nChart, DATA_TAB_L, DATA_TAB_L, Color.Blue);
                //   GraphInputData(nChart.Series[DATA_TAB_L], LToA(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LogLengthY));
                GraphInputData(nChart, DATA_TAB_L, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LogLengthY);

                CreateChart(nChart, DATA_TAB_R, DATA_TAB_R, Color.Blue);
                //    GraphInputData(nChart.Series[DATA_TAB_R], LToA(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LogLengthY));
                GraphInputData(nChart, DATA_TAB_R, AxisX, Color.Purple, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LogLengthY);
                //             DrawStripLine(nChart.ChartAreas[DATA_TAB_L].AxisY, DATAW + "_SPEC", Main.AlignUnit[m_AlignNo].m_Standard[Main.DEFINE.WIDTH_], Color.Green);

            }
            catch (System.Exception ex)
            {

            }
        }
        private void CreateChart(Chart nChart, String nChartAreaName, String nGraphName, Color nColor)
        {
            try
            {
                //                nChart.Legends.Add(nChartAreaName);

                nChart.ChartAreas.Add(nChartAreaName);
                nChart.ChartAreas[nChartAreaName].BackColor = Color.Cornsilk;
                nChart.ChartAreas[nChartAreaName].BackGradientStyle = GradientStyle.TopBottom;
                nChart.ChartAreas[nChartAreaName].AxisX.ScaleView.Zoomable = true;
                nChart.ChartAreas[nChartAreaName].AxisY.ScaleView.Zoomable = true;
                nChart.ChartAreas[nChartAreaName].CursorX.AutoScroll = true;
                nChart.ChartAreas[nChartAreaName].CursorY.AutoScroll = true;

                nChart.ChartAreas[nChartAreaName].Axes[AxisX].IntervalAutoMode = IntervalAutoMode.VariableCount;
                nChart.ChartAreas[nChartAreaName].Axes[AxisX].MajorGrid.LineDashStyle = ChartDashStyle.DashDot;

                nChart.ChartAreas[nChartAreaName].Axes[AxisY].LabelStyle.ForeColor = nColor;
                nChart.ChartAreas[nChartAreaName].Axes[AxisY].TitleForeColor = nColor;
                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                    nChart.ChartAreas[nChartAreaName].Axes[AxisY].Title = nChartAreaName + " (mm)";
                else
                    nChart.ChartAreas[nChartAreaName].Axes[AxisY].Title = nChartAreaName + " (um)";
                nChart.ChartAreas[nChartAreaName].Axes[AxisY].TextOrientation = TextOrientation.Horizontal;
                nChart.ChartAreas[nChartAreaName].Axes[AxisY].MajorGrid.LineDashStyle = ChartDashStyle.DashDot;
                nChart.ChartAreas[nChartAreaName].Axes[AxisY].MajorGrid.LineColor = Color.HotPink;

                AddGraphSeries(nChart, nChartAreaName, nGraphName, nColor);


            }
            catch
            {

            }
        }
        private void AddGraphSeries(Chart nChart, String nChartAreaName, String nGraphName, Color nColor)
        {
            try
            {
                nChart.Series.Add(nGraphName);
                nChart.Series[nGraphName].ChartArea = nChartAreaName;
                nChart.Series[nGraphName].ChartType = SeriesChartType.Line;
                //                nChart.Series[nGraphName].Legend = nChartAreaName;
                nChart.Series[nGraphName].IsValueShownAsLabel = true;
                nChart.Series[nGraphName].LabelForeColor = nColor;
                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                    nChart.Series[nGraphName].LabelFormat = "{0.000}";
                else
                    nChart.Series[nGraphName].LabelFormat = "{0.0}";
                nChart.Series[nGraphName].Color = nColor;
                nChart.Series[nGraphName].YValueType = ChartValueType.Int32;
                nChart.Series[nGraphName].MarkerSize = 5;
                nChart.Series[nGraphName].MarkerStyle = MarkerStyle.Circle;
                nChart.Series[nGraphName].MarkerColor = System.Drawing.Color.Green;
            }
            catch
            {

            }
        }
        private void DrawStripLine(Axis nAxis, string nDisplayText, double nPosData, Color nColor)
        {
            try
            {
                StripLine nStripeLine = new StripLine();

                nStripeLine.StripWidth = 1;
                nStripeLine.IntervalOffset = nPosData;
                nStripeLine.ToolTip = nPosData.ToString();

                nStripeLine.BackColor = nColor;
                nStripeLine.ForeColor = nColor;
                nStripeLine.Text = nDisplayText;

                nAxis.StripLines.Add(nStripeLine);
            }
            catch
            {

            }

        }
        private void AddTitles(Chart nChart, string nTitleName)
        {

            nChart.Titles.Add(nTitleName);
            nChart.Titles[0].Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Bold);
        }
        private double[] LToA(List<double> nList)
        {
            double[] ndefault = new double[1];
            if (nList == null) return ndefault;
            double[] nArray = new double[nList.Count];
            try
            {

                for (int i = 0; i < nList.Count; i++)
                {
                    nArray[i] = nList[i];
                }

            }
            catch
            {
            }
            return nArray;
        }
        private long[] LToA(List<long> nList)
        {
            long[] ndefault = new long[1];
            if (nList == null) return ndefault;
            long[] nArray = new long[nList.Count];
            try
            {

                for (int i = 0; i < nList.Count; i++)
                {
                    nArray[i] = nList[i];
                }

            }
            catch
            {
            }
            return nArray;
        }

        private void GraphInputData(Series nSeries, double[] nAxisY_Data)
        {
            try
            {
                nSeries.Points.Clear();
                nSeries.Points.DataBindY(nAxisY_Data);
            }
            catch
            {

            }

        }
        private void GraphInputData(Series nSeries, long[] nAxisY_Data)
        {
            try
            {
                nSeries.Points.Clear();
                nSeries.Points.DataBindY(nAxisY_Data);
            }
            catch
            {

            }

        }

        //-------------------------------------------------------------------------------------------
        private void BTN_UPDATE_Click(object sender, EventArgs e)
        {
            RefreshDisplay();
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            switch (DISPLAY_MODE)
            {
                case MODE_ALIGNXYT:
                    Main.AlignUnit[m_AlignNo].Aligndata_RemoveAt(m_PatTagNo);
                    break;
                case MODE_ALIGNINSPECTION:
                    for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatTagMax; i++)
                    {
                        Main.AlignUnit[m_AlignNo].Aligndata[i].Clear();
                    }
                    break;
                case MODE_TABLENGTH:
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LogLengthY.Clear();
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LogLengthY.Clear();
                    break;
            }
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Hide();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshDisplay();
        }
        private void Form_MouseWheelEvent(object sender, MouseEventArgs e)
        {
            //             if (MSChart.ChartAreas[0].AxisX.ScaleView.ViewMaximum > 1000)
            //                 return;
            //             if(e.Delta < 0)
            //             {
            //                 MSChart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            //             }
            //             if(e.Delta > 0)
            //             {
            //                 MSChart.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
            //                 MSChart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            //                 double nMin = MSChart.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
            //                 double nMax = MSChart.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
            // 
            //                 Int64 posXStart = (Int64)(MSChart.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) - (nMax - nMin) / 2);
            //                 Int64 posXFinish = (Int64)(MSChart.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) + (nMax - nMin) / 2);
            //                  MSChart.ChartAreas[0].AxisX.ScaleView.Zoom(posXStart, posXFinish);
            //             }
        }
        private void chartTracking_MouseEnter(object sender, EventArgs e)
        {
            this.MSChart.Focus();
        }
        private void chartTracking_MouseLeave(object sender, EventArgs e)
        {
            this.MSChart.Parent.Focus();
        }
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {

        }

        private void MSChart_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "Chart";
                saveFileDialog1.Filter = "Bitmap(*.bmp)|*.bmp";
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    Chart nChart = (Chart)sender;
                    nChart.SaveImage(saveFileDialog1.FileName, ChartImageFormat.Bmp);
                }
            }
            catch
            {

            }
        }

        private void CB_OBJ_DATA_CheckedChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }
        //----------------------------------------------------------------------------------------------------------


    }//Form
}//COG

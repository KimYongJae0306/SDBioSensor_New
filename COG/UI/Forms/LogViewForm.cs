using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;

using System.Diagnostics.PerformanceData;
using System.Diagnostics;
using System.Collections;
using System.Management;
using COG.Settings;

namespace COG.UI.Forms
{
    public partial class LogViewForm : Form
    {

        DriveInfo CDriveInfo;

        int nCpuTemp = 0, maxMem = 0;
        double nMemoryTemp = 0;

        private List<TextBox>[] TB_AlignData = new List<TextBox>[3];


        public LogViewForm()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            tabControl1.Controls.RemoveAt(4);
            tabControl1.Controls.RemoveAt(3);

            for (int i = 0; i < 3; i++)
            {
                TB_AlignData[i] = new List<TextBox>();
                switch (i)
                {
                    case 0:
                        TB_AlignData[0].Add(TB_STAGE1_00); TB_AlignData[0].Add(TB_STAGE1_01); TB_AlignData[0].Add(TB_STAGE1_02); TB_AlignData[0].Add(TB_STAGE1_03); TB_AlignData[0].Add(TB_STAGE1_04);
                        TB_AlignData[0].Add(TB_STAGE1_05); TB_AlignData[0].Add(TB_STAGE1_06); TB_AlignData[0].Add(TB_STAGE1_07); TB_AlignData[0].Add(TB_STAGE1_08);
                        break;

                    case 1:
                        TB_AlignData[1].Add(TB_STAGE2_00); TB_AlignData[1].Add(TB_STAGE2_01); TB_AlignData[1].Add(TB_STAGE2_02); TB_AlignData[1].Add(TB_STAGE2_03); TB_AlignData[1].Add(TB_STAGE2_04);
                        TB_AlignData[1].Add(TB_STAGE2_05); TB_AlignData[1].Add(TB_STAGE2_06); TB_AlignData[1].Add(TB_STAGE2_07); TB_AlignData[1].Add(TB_STAGE2_08);
                        break;

                    case 2:
                        TB_AlignData[2].Add(TB_STAGE3_00); TB_AlignData[2].Add(TB_STAGE3_01); TB_AlignData[2].Add(TB_STAGE3_02); TB_AlignData[2].Add(TB_STAGE3_03); TB_AlignData[2].Add(TB_STAGE3_04);
                        TB_AlignData[2].Add(TB_STAGE3_05); TB_AlignData[2].Add(TB_STAGE3_06); TB_AlignData[2].Add(TB_STAGE3_07); TB_AlignData[2].Add(TB_STAGE3_08);
                        break;
                }
            }

            DGB_ALIGN.Add(DGB_ALIGN_00); DGB_ALIGN.Add(DGB_ALIGN_01); DGB_ALIGN.Add(DGB_ALIGN_02);



        }
        private void Form_LogView_Load(object sender, EventArgs e)
        {
            LV_DisplayToolbar01.Display = LV_Display01;
            LV_DisplayStatusBar01.Display = LV_Display01;
            LV_MonthCalendar01.SelectionStart = System.DateTime.Now;

        }
        public void ControlUpDate()
        {
            dataGridView1.Rows.Clear();
            timer1.Enabled = true;
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            for (int i = LV_TreeView01.Nodes.Count; i > 0; i--)
            {
                LV_TreeView01.Nodes.RemoveAt(i - 1);
            }

            DateTime date;
            date  = LV_MonthCalendar01.SelectionStart;   
            DirectoryInfo nSelectDir = new DirectoryInfo(StaticConfig.SYS_DATADIR + StaticConfig.LOG_DATADIR + date.ToString("yyyyMMdd"));

            if (Directory.Exists(nSelectDir.FullName))
            {
                TreeNode nNode = new TreeNode(nSelectDir.Name);
                LV_TreeView01.Nodes.Add(nNode);
                SubDir(nSelectDir, nNode);
            }
        }

        private void SubDir(DirectoryInfo dir, TreeNode n)
        {
            try
            {
                 FileInfo[] files = dir.GetFiles();
                foreach (FileInfo files2 in files)
                {
                    TreeNode c = new TreeNode(files2.Name);
                    n.Nodes.Add(c);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo dir2 in dirs)
                {
                    TreeNode c = new TreeNode(dir2.Name);
                    n.Nodes.Add(c);

                    files = dir2.GetFiles();
                    foreach (FileInfo files2 in files)
                    {
                        TreeNode cc = new TreeNode(files2.Name);
                        c.Nodes.Add(cc);
                    }

                    try
                    {
                        if ((dir2.GetDirectories()).Length > 0)
                        {
                            this.SubDir(dir2, c);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {

            }
        }
        private void LV_TreeView01_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }
        private void LV_TreeView01_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                    DisplayNode(Path.GetExtension(LV_TreeView01.SelectedNode.FullPath), LV_TreeView01.SelectedNode.FullPath);
            }
            catch
            {

            }
        }
        private void DisplayNode(string nType , string nFullPath)
        {
            try
            {
                switch (nType.ToLower())
                {
                    case ".bmp":
                    case ".jpg":
                    case ".Png":
                    case ".peg":
                        LB_PATH_IMAGE_FILE.Text = "";
                        LogFileLoad_Image(StaticConfig.SYS_DATADIR + StaticConfig.LOG_DATADIR + nFullPath);
                        LB_PATH_IMAGE_FILE.Text = nFullPath;
                          tabControl1.SelectedIndex = 0;
                        break;
                    case ".txt":
                        LB_PATH_COMMAND_FILE.Text = "";
                        LogFileLoad_Command(StaticConfig.SYS_DATADIR + StaticConfig.LOG_DATADIR + nFullPath);
                        LB_PATH_COMMAND_FILE.Text = nFullPath;
                         tabControl1.SelectedIndex = 1;
                        break;
                     case ".csv":
                        if(tabControl1.SelectedIndex == 3)
                        {
                            LB_PATH_COMMAND_FILE.Text = "";
                            AlignFileLoad_Command(StaticConfig.SYS_DATADIR + StaticConfig.LOG_DATADIR + nFullPath);
                            LB_PATH_COMMAND_FILE.Text = nFullPath;
                        }
                        else
                        {
                            LB_PATH_COMMAND_FILE.Text = "";
                            InspcetionFileLoad_Command(StaticConfig.SYS_DATADIR + StaticConfig.LOG_DATADIR + nFullPath);
                            LB_PATH_COMMAND_FILE.Text = nFullPath;
                        }
                        tabControl1.SelectedIndex = 2;
                        break;
                }
            }
            catch
            {

            }
        }
        private void LogFileLoad_Image(string _path)
        {
            CogImageFile imagefile = new CogImageFile();
            Bitmap bitmap = new Bitmap(_path);
            CogImage24PlanarColor image = new CogImage24PlanarColor(bitmap);
            LV_Display01.Image = image;
            LV_Display01.AutoFitWithGraphics = true;
        }
        private void LogFileLoad_Command(string _path)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(_path); //선택된 파일로 StreamReader 생성 
            string txtContent = sr.ReadToEnd(); //파일내용을 모두 읽음 
            LV_RichTextBox01.Text = txtContent;
            sr.Close(); 
            sr.Dispose(); 
        }
        private void InspcetionFileLoad_Command(string _path)
        {
            string nType = "null";
            nType = LV_TreeView01.SelectedNode.Text.ToString().Substring(LV_TreeView01.SelectedNode.Text.Length - 9, 9);
            if (nType != "_Data.csv") return;

            dataGridView1.Rows.Clear();
            dataGridView1.AutoSize = false;
            string[] ReadAll = File.ReadAllLines(_path);
            int Count = 0;
            for (int i = ReadAll.Length; i > 0; i--)
            {
                string[] row = ReadAll[i - 1].Split(',', '\t');
                dataGridView1.Rows.Add(row);
                Count++; if (Count > 100) break;
            }

            #region

//             FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read); // @기호는 문자열에 포함된 \등의 일반문자열로 인식
//             StreamReader sr = new StreamReader(file, System.Text.Encoding.Default);
//             sr.BaseStream.Seek(0, SeekOrigin.Begin);
//             
//             int rowcnt = 0;
//             dataGridView1.Rows.Clear();
// //            DataGridHeadNameChange(_path);
// 
//             while (sr.Peek() > -1)
//             {
//                 string[] row = sr.ReadLine().Split(',', '\t');                
// //                dataGridView1.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
// 
//                 dataGridView1.Rows.Add(row);
//                 if (dataGridView1.RowCount - 1 > 100) { dataGridView1.Rows.RemoveAt(0); }
// 
//                 rowcnt = dataGridView1.Rows.Count - 2;
//                 dataGridView1.Rows[rowcnt].Height = 30;
//             }
//             sr.Close();
//             file.Close();

            #endregion
        }

        private void AlignFileLoad_Command(string _path)
        {
            return;
            string nType = "null";
            nType = LV_TreeView01.SelectedNode.Text.ToString().Substring(LV_TreeView01.SelectedNode.Text.Length - 15, 15);
            if (nType != "AlignResult.csv") return;

            FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read); // @기호는 문자열에 포함된 \등의 일반문자열로 인식
            StreamReader sr = new StreamReader(file, System.Text.Encoding.Default);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            int rowcnt = 0;
            int Backup_Cnt = 0;
            int[] Num = new int[12];
            int[] Current_Num = new int[12];
            int[] BackUp_Num = new int[12];
            int[] Min_Num = new int[12];
            int[] Max_Num = new int[12];
            int[] Max_Min_GAP = new int[12];

            dataGridView2.Rows.Clear();
            //            DataGridHeadNameChange2(_path);

            for (int i = 7; i < 10; i++ )
            {
                Min_Num[i] = 0;
                Max_Num[i] = 0;
            }

            while (sr.Peek() > -1)
            {
                string[] row = sr.ReadLine().Split(',', '\t'); 
 //               string[] row = sr.ReadToEnd().Split(',', '\t');
                //                dataGridView2.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dataGridView2.Rows.Add(row);
                Backup_Cnt++;

                for (int i = 7; i < 10; i++)
                {
                    Current_Num[i] = Convert.ToInt32(row[i]);
                    BackUp_Num[i] += Current_Num[i];
//                    Num[i] = BackUp_Num[i] / Backup_Cnt;

                    if (Backup_Cnt == 1)
                    {
                        Max_Num[i] = Min_Num[i] = Current_Num[i];
                    }
                    if (Current_Num[i] < Min_Num[i]) Min_Num[i] = Current_Num[i];
                    if (Current_Num[i] > Max_Num[i]) Max_Num[i] = Current_Num[i];
                }

                if (dataGridView2.RowCount - 1 > 3000)
                {
                    dataGridView2.Rows.RemoveAt(0);
                }

                rowcnt = dataGridView2.Rows.Count - 2;
                dataGridView2.Rows[rowcnt].Height = 30;
            }

            for (int i = 7; i < 10; i++) Num[i] = BackUp_Num[i] / Backup_Cnt;
               
            sr.Close();
            file.Close();

            dataGridView3.Rows.Clear();

            for(int i = 7; i < 10; i++)
            {
                Max_Min_GAP[i] = Max_Num[i] - Min_Num[i];
            }
            
            string[] row2 = new string[] { Num[7].ToString(), Num[8].ToString(), Num[9].ToString(), Min_Num[7].ToString(), Min_Num[8].ToString(), Min_Num[9].ToString() 
                                            , Max_Num[7].ToString(), Max_Num[8].ToString(), Max_Num[9].ToString(), Max_Min_GAP[7].ToString(), Max_Min_GAP[8].ToString(), Max_Min_GAP[9].ToString() };
            dataGridView3.Rows.Add(row2);

        }

        private void DataGridHeadNameChange(string _path)
        {
            string nType = "";
            nType = _path.ToString().Substring(_path.Length - 9, 5);

            int nDataType;

            if (nType == "PIXEL") nDataType = 0;
            else nDataType = 1;
            

            dataGridView1.Columns.Clear();

            string[][] nHeaderName = new string[2][];
            nHeaderName[0] = new string[] { "TIME", "POS", "FL_X", "FL_Y", "FR_X", "FR_Y", "RL_X", "RL_Y", "RR_X", "RR_Y" };
            nHeaderName[1] = new string[] { "TIME", "FL_X", "FL_Y", "FR_X", "FR_Y", "RL_X", "RL_Y", "RR_X", "RR_Y", "X", "Y", "T" };

            dataGridView1.Columns.Clear();

            for (int i = 0; i < nHeaderName[nDataType].Length; i++)
            {
                dataGridView1.Columns.Add(i.ToString(), nHeaderName[nDataType][i]);
            }
        }


        private void LV_RichTextBox01_MouseDown(object sender, MouseEventArgs e)
        {
// 
//             if (e.Button == MouseButtons.Right && LB_PATH_COMMAND_FILE.Text.Length > 0)
//             {
//                 //    saveFileDialog1.Filter = "Excel files (*.xls)|*.xls";
//                 saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
//                 if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
//                 && saveFileDialog1.FileName.Length > 0)
//                 {
//                     LV_RichTextBox01.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
//                 }
//             }
        }

        private void CheckProcessor()
        {
//             try
//             {
//                 int value = 0;
//                 value = (int)CPUCounter.NextValue();
//                 SetProgessValue(progressBar1, value);
// 
//                 if (nCpuTemp < value)
//                     nCpuTemp = value;
// 
//                 LabelChange(lblCpu, "Use CPU : " + value.ToString() + "%");
//                 UpdateProgress(progressBar1);
//             }
//             catch (Exception e)
//             {
//                 MessageBox.Show(e.Message);
//             }
        }
        private void CheckMemoryD()
        {
//             int checkMemory = CheckMemory();
//             int memoryCountNextValue = (int)MemoryCounter.NextValue();
//             int maximum = checkMemory / 1024;
//             int value = (int)((checkMemory - memoryCountNextValue) / 1024);
//             double nNowMemory = ((double)checkMemory - memoryCountNextValue) / 1024;
// 
//             if (nMemoryTemp < nNowMemory)
//                 nMemoryTemp = nNowMemory;
// 
//             LabelChange(lblMemory, "Use Memory : " + nNowMemory.ToString("0.00") + "GB");
//             SetProgessMax(progressBar4, maximum);
//             SetProgessValue(progressBar4, value);
//             UpdateProgress(progressBar4);
        }
        private void HDDCheck(string HDDName)
        {
            CDriveInfo = new DriveInfo(HDDName);
            long lCTotal = CDriveInfo.TotalSize / 1024 / 1024;
            long lC = CDriveInfo.TotalFreeSpace / 1024 / 1024;
            double lCPercentage = 0.0;
            int value = 0;

            if (HDDName == "C")
            {
                lCPercentage = (double)((double)lC / (double)lCTotal);
                value = (100 - (int)(lCPercentage * 100));
                SetProgessValue(progressBar2, value);
                LabelChange(lblCDrive, "" + lC + " / " + lCTotal + "MB");
                UpdateProgress(progressBar2);
            }

            if (HDDName == "D")
            {
                lCPercentage = (double)((double)lC / (double)lCTotal);
                value = (100 - (int)(lCPercentage * 100));
                SetProgessValue(progressBar3, value);
                LabelChange(lblDDrive, "" + lC + " / " + lCTotal + "MB");
                UpdateProgress(progressBar3);
            }
        }
        private int CheckMemory()
        {
            ObjectQuery WinQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(WinQuery);
            ManagementObjectCollection queryCollection = searcher.Get();

            ulong memory = 0;
            foreach (ManagementObject item in queryCollection)
            {
                memory = ulong.Parse(item["TotalVisibleMemorySize"].ToString());
            }
            maxMem = (int)(memory / 1024);

            return maxMem;
        }

        #region Memory Check
        delegate void dGetProgessValue(ProgressBar prg, ref int value);
        public void GetProgessValue(ProgressBar prg, ref int value)
        {
            if (prg.InvokeRequired)
            {
                dGetProgessValue call = new dGetProgessValue(GetProgessValue);
                prg.Invoke(call, prg, value);
            }
            else
            {
                value = prg.Value;
            }
        }

        delegate void dSetProgessValue(ProgressBar prg, int value);
        public void SetProgessValue(ProgressBar prg, int value)
        {
            if (prg.InvokeRequired)
            {
                dSetProgessValue call = new dSetProgessValue(SetProgessValue);
                prg.Invoke(call, prg, value);
            }
            else
            {
                prg.Value = value;
            }
        }

        delegate void dSetProgessMax(ProgressBar prg, int max);
        public void SetProgessMax(ProgressBar prg, int max)
        {
            if (prg.InvokeRequired)
            {
                dSetProgessMax call = new dSetProgessMax(SetProgessMax);
                prg.Invoke(call, prg, max);
            }
            else
            {
                prg.Maximum = max;
            }
        }

        delegate void dUpdateProgress(ProgressBar prg);
        public void UpdateProgress(ProgressBar prg)
        {
            if (prg.InvokeRequired)
            {
                dUpdateProgress call = new dUpdateProgress(UpdateProgress);
                prg.Invoke(call, prg);
            }
            else
            {
                prg.Update();
            }
        }

        delegate void dLabelChange(Label lb, string str);
        public void LabelChange(Label lb, string str)
        {
            if (lb.InvokeRequired)
            {
                dLabelChange call = new dLabelChange(LabelChange);
                lb.Invoke(call, lb, str);
            }
            else
            {
                lb.Text = str;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            CheckProcessor();
            CheckMemoryD();
            HDDCheck("C");
            HDDCheck("D");
        }
        #endregion

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Hide();
        }



        private List<DataGridView> DGB_ALIGN = new List<DataGridView>();
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void AlignData_Dispaly(int nAlignUnitNum)
        {

#if false
            DataGridView nTempDataGrid = DGB_ALIGN[nAlignUnitNum];
            Main.DAlignData nAligndata = Main.AlignUnit[nAlignUnitNum].Aligndata;

            double[,] BackUp_Num = new double[3,3];
//            double[,] BackUp_Num = new double[,]{{0,0,0},{0,0,0}};
          

            nTempDataGrid.Rows.Clear();
            for (int i = 0; i < nAligndata.NUM.Count; i++)
            {
                nTempDataGrid.Rows.Add();
                nTempDataGrid[0, i].Value = nTempDataGrid.RowCount;
                nTempDataGrid[1, i].Value = nAligndata.P_CELLID[i];
                nTempDataGrid[2, i].Value = nAligndata.W_CELLID[i];
                nTempDataGrid[3, i].Value = (int)nAligndata.X[i];
                nTempDataGrid[4, i].Value = (int)nAligndata.Y[i];
                nTempDataGrid[5, i].Value = (int)nAligndata.T[i];
                nTempDataGrid[6, i].Value = (int)nAligndata.P_CX[i];
                nTempDataGrid[7, i].Value = (int)nAligndata.P_CY[i];
                nTempDataGrid[8, i].Value = (int)nAligndata.W_CX[i];
                nTempDataGrid[9, i].Value = (int)nAligndata.W_CY[i];
                nTempDataGrid[10, i].Value = nAligndata.Result_sts[i];
                nTempDataGrid[11, i].Value = nAligndata.m_Point_Num[i];

                BackUp_Num[nAlignUnitNum, 0] += nAligndata.X[i];
                BackUp_Num[nAlignUnitNum, 1] += nAligndata.Y[i];
                BackUp_Num[nAlignUnitNum, 2] += nAligndata.T[i];
            }
            if (nAligndata.NUM.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    BackUp_Num[nAlignUnitNum, i] = BackUp_Num[nAlignUnitNum, i] / nAligndata.NUM.Count;
                }
            }

            if (nAligndata.NUM.Count > 0)
            {
                for (int j = 0; j < 9; j++)
                {
                    switch (j)
                    {
                        case 0: TB_AlignData[nAlignUnitNum][j].Text = nAligndata.X.Min().ToString("0"); break;
                        case 1: TB_AlignData[nAlignUnitNum][j].Text = nAligndata.X.Max().ToString("0"); break;
                        case 2: TB_AlignData[nAlignUnitNum][j].Text = BackUp_Num[nAlignUnitNum, 0].ToString("0"); break;

                        case 3: TB_AlignData[nAlignUnitNum][j].Text = nAligndata.Y.Min().ToString("0"); break;
                        case 4: TB_AlignData[nAlignUnitNum][j].Text = nAligndata.Y.Max().ToString("0"); break;
                        case 5: TB_AlignData[nAlignUnitNum][j].Text = BackUp_Num[nAlignUnitNum, 1].ToString("0"); break;

                        case 6: TB_AlignData[nAlignUnitNum][j].Text = nAligndata.T.Min().ToString("0"); break;
                        case 7: TB_AlignData[nAlignUnitNum][j].Text = nAligndata.T.Max().ToString("0"); break;
                        case 8: TB_AlignData[nAlignUnitNum][j].Text = BackUp_Num[nAlignUnitNum, 2].ToString("0"); break;
                    }
                }
            }
#endif
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1_Click(null, null);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1_Click(null, null);

        }

        private void BTN_SIZE_DOWN_Click(object sender, EventArgs e)
        {
            LV_RichTextBox01.Font = new Font("맑은 고딕", LV_RichTextBox01.Font.Size - 1);
        }

        private void BTN_SIZE_UP_Click(object sender, EventArgs e)
        {
            LV_RichTextBox01.Font = new Font("맑은 고딕", LV_RichTextBox01.Font.Size + 1);
        }


 




   
        
    }

}

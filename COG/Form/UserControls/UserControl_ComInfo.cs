using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.IO;

using ChartDirector;

namespace COG
{   
    public partial class UserControl_Info : UserControl
    {
        private Thread _Thread;
        private bool _mRun = true;

        private double _mTotalMemory;
        private double _mNowMemory;

        private int _mUP_CPU = 0;
        private int _mUP_RAM = 0;
        //private int _mUP_PROC = 0;
        private int _mMode = 0;

        private int _mUP_HDDC = 0;
        private int _mUP_HDDD = 0;

        public static string _mProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

        private PerformanceCounter __CPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");        
        private PerformanceCounter __RAM = new PerformanceCounter("Memory", "Available MBytes");

        public UserControl_Info(int mode)
        {
            InitializeComponent();            
            _mMode = mode;
        }
        
        /// <summary>
        /// 하드용량 업데이트
        /// </summary>
        /// <param name="_winChartViewer"></param>
        /// <param name="_value"></param>
        /// <param name="_title"></param>
        private void usageDrive(WinChartViewer _winChartViewer, double[] _value, string _title)
        {
            string[] labels = { "Usage", "Free" };
            PieChart _chart = new PieChart(190, 150);
            _chart.setPieSize(100, 80, 30);
            _chart.setColors(Chart.whiteOnBlackPalette);
            _chart.addTitle(" HDD " + _title + ":");
            _chart.set3D();

            _chart.setData(_value, labels);
            _chart.setExplode(0);

            if (_winChartViewer.InvokeRequired)
            {
                _winChartViewer.BeginInvoke(new Action(() =>
                {
                    _winChartViewer.Chart = _chart;
                }));
            }
            else
            {
                _winChartViewer.Chart = _chart;
            }
        }

        private void MainShortUISet()
        {
            CpuLabel.Hide();
            RamLabel.Hide();
            lblCPU.Hide();
            lblRAM.Hide();

            progressBar_CPU.Hide();
            progressBar_RAM.Hide();

            float currentSize = HDD_C_Label.Font.Size;
            currentSize = 3.0F;
            HDD_C_Label.Location = new Point(10, 25);
            HDD_D_Label.Location = new Point(10, 75);

            lblHDDC.Location = new Point(255, 25);
            lblHDDD.Location = new Point(255, 75);

            progressBar_HDDC.Location = new Point(80, 10);
            progressBar_HDDC.Size = new Size(160, 40);

            progressBar_HDDD.Location = new Point(80, 60);
            progressBar_HDDD.Size = new Size(160, 40);
        }

        private void MainResizingUISet()
        {
            _mTotalMemory = getPhysicalMemory();

            // 241,100
            this.Width = 240;
            this.Height = 120;

            // 296, 117
            panel5.Width = 240;
            panel5.Height = 120;
            panel5.BackColor = System.Drawing.Color.White;

            // postion
            CpuLabel.Location = new Point(2, 2);
            RamLabel.Location = new Point(2, 32);
            HDD_C_Label.Location = new Point(2, 62);
            HDD_D_Label.Location = new Point(2, 92);

            progressBar_CPU.Location = new Point(2, 2);
            progressBar_RAM.Location = new Point(2, 32);
            progressBar_HDDC.Location = new Point(2, 62);
            progressBar_HDDD.Location = new Point(2, 92);

            // Size
            progressBar_CPU.Size = new Size(180, 26);
            progressBar_RAM.Size = new Size(180, 26);
            progressBar_HDDC.Size = new Size(180, 26);
            progressBar_HDDD.Size = new Size(180, 26);

            lblCPU.Location = new Point(200, 2);
            lblRAM.Location = new Point(200, 32);
            lblHDDC.Location = new Point(200, 62);
            lblHDDD.Location = new Point(200, 92);

            lblCPU.ForeColor = System.Drawing.Color.Black;
            lblRAM.ForeColor = System.Drawing.Color.Black;
            lblHDDC.ForeColor = System.Drawing.Color.Black;
            lblHDDD.ForeColor = System.Drawing.Color.Black;

            _Thread = new Thread(new ThreadStart(CheckSystem));
            _Thread.Start();
        }

        private void UcComputer_Load(object sender, EventArgs e)
        {
            switch (_mMode)
            {
                case 0 :
                    MainShortUISet();
                    _Thread = new Thread(new ThreadStart(CheckShortSystem));
                    _Thread.Start();
                    break;
                case 1 :
                    _mTotalMemory = getPhysicalMemory();
                    _Thread = new Thread(new ThreadStart(CheckSystem));
                    _Thread.Start();
                    break;
                case 2 :
                    MainResizingUISet();
                    break;
            }
        }

        /// <summary>
        /// 프로그래스바 업데이트
        /// </summary>
        /// <param name="_ProgressBar"></param>
        /// <param name="_value"></param>
        private void UpdateProgressBar(ProgressBar _ProgressBar, int _value)
        {
            try
            {
                if (_ProgressBar.InvokeRequired)
                {
                    _ProgressBar.BeginInvoke(new Action(() =>
                    {
                        if (_ProgressBar.Minimum <= _value && _ProgressBar.Maximum >= _value)
                        {
                            _ProgressBar.Value = _value;
                        }
                        else
                        {
                            _ProgressBar.Value = _ProgressBar.Minimum;
                        }
                    }));
                }
                else
                {
                    if (_ProgressBar.Minimum <= _value && _ProgressBar.Maximum >= _value)
                    {
                        _ProgressBar.Value = _value;
                    }
                    else
                    {
                        _ProgressBar.Value = _ProgressBar.Minimum;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// label 업데이트
        /// </summary>
        /// <param name="_ctrl"></param>
        /// <param name="_value"></param>
        private void UpdatelblInvoke(Control _ctrl, string _value)
        {
            if (_ctrl.InvokeRequired)
            {
                _ctrl.BeginInvoke(new Action(() =>
                {
                    _ctrl.Text = _value + "%";
                }));
            }
            else
            {
                _ctrl.Text = _value + "%";
            }
        }

        /// <summary>
        /// 메모리 크기 가져오기
        /// </summary>
        /// <returns></returns>
        private double getPhysicalMemory()
        {
            try
            {
                ObjectQuery _ObjectQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(_ObjectQuery);
                ManagementObjectCollection queryCollection = searcher.Get();

                ulong _Memory = 0;
                foreach (ManagementObject item in queryCollection)
                {
                    _Memory = ulong.Parse(item["TotalVisibleMemorySize"].ToString());   // byte
                }
                return (double)(_Memory / 1024);    // MByte
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 하드 사용량 및 남은용량 가져오기
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        private double[] getDrive(string _type)
        {
            double _Tsize = 0d;
            double _Ssize = 0d;

            try
            {
                DriveInfo _DriveInfo = new DriveInfo(_type);

                _Tsize = _DriveInfo.TotalSize / (1024 * 1024 * 1024);
                _Ssize = _DriveInfo.TotalFreeSpace / (1024 * 1024 * 1024);

                return new double[2] { _Ssize, _Tsize };
            }
            catch
            {
                return new double[2] { 0, 0 };
            }
        }
        /// <summary>
        /// 하드 사용량 및 남은용량 가져오기
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        private int getDrivePer(string _type)
        {
            double _Tsize = 0d;
            double _Ssize = 0d;
            int _UsingPer = 0;

            try
            {
                DriveInfo _DriveInfo = new DriveInfo(_type);

                _Tsize = _DriveInfo.TotalSize / (1024 * 1024 * 1024);
                _Ssize = _DriveInfo.TotalFreeSpace / (1024 * 1024 * 1024);
                _UsingPer = (int)((_Tsize - _Ssize) * 100 / _Tsize);
                return _UsingPer;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 정보 갱신
        /// </summary>
        private void CheckSystem()
        {
            do
            {
                //__CPU.NextValue();
                //System.Threading.Thread.Sleep(1000);
                _mUP_CPU = (int)__CPU.NextValue();
                UpdateProgressBar(progressBar_CPU, _mUP_CPU);
                UpdatelblInvoke(lblCPU, _mUP_CPU.ToString());

                _mNowMemory = (double)(__RAM.NextValue());  // MByte
                _mUP_RAM = (int)((1 - (_mNowMemory / _mTotalMemory)) * 100);
                UpdateProgressBar(progressBar_RAM, _mUP_RAM);
                UpdatelblInvoke(lblRAM, _mUP_RAM.ToString());
                
                //System.Threading.Thread.Sleep(10);
                //_mUP_PROC = (int)__PROC_CPU.NextValue();
                //UpdateProgressBar(progressBar_PROC, _mUP_PROC);
                //UpdatelblInvoke(lblPROC, _mUP_PROC.ToString());
                
                System.Threading.Thread.Sleep(10);
                _mUP_HDDC = getDrivePer("C");
                UpdateProgressBar(progressBar_HDDC, _mUP_HDDC);
                UpdatelblInvoke(lblHDDC, _mUP_HDDC.ToString());

                System.Threading.Thread.Sleep(10);
                _mUP_HDDD = getDrivePer("D");
                UpdateProgressBar(progressBar_HDDD, _mUP_HDDD);
                UpdatelblInvoke(lblHDDD, _mUP_HDDD.ToString());

                System.Threading.Thread.Sleep(2000);

            } while (_mRun);

        }

        /// <summary>
        /// 정보 갱신
        /// </summary>
        private void CheckShortSystem()
        {
            do
            {                   
                System.Threading.Thread.Sleep(10);
                _mUP_HDDC = getDrivePer("C");
                UpdateProgressBar(progressBar_HDDC, _mUP_HDDC);
                UpdatelblInvoke(lblHDDC, _mUP_HDDC.ToString());

                System.Threading.Thread.Sleep(10);
                _mUP_HDDD = getDrivePer("D");
                UpdateProgressBar(progressBar_HDDD, _mUP_HDDD);
                UpdatelblInvoke(lblHDDD, _mUP_HDDD.ToString());

                System.Threading.Thread.Sleep(3000);

            } while (_mRun);

        }

        /// <summary>
        /// 리소스 반환
        /// </summary>
        public void DeInitialize()
        {
            _mRun = false;
            try
            {
                if (_Thread != null) 
                {
                    _Thread.Abort();
                }
            }
            catch { }
        }
    }
}

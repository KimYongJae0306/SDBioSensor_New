using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.CompilerServices; //CallerMemberName

using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.Implementation;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.Exceptions;
using Microsoft.Win32; //Regedit_Password

namespace JAS.Controls.Display
{

    public enum ManuTool
    {
        MOUSE_MODE,
        POINT2POINT,
        POINT2LINE,
        SQUARE,
        POINT3ARC,
        CUSTOM_CROSS,
        CROSS_LINE,
    }
    public partial class Display : UserControl, INotifyPropertyChanged
    {
        public Display()
        {
            InitializeComponent();
            //           string nLicence;
            //             nLicence = ReadRegistry("Get");
            //             if (nLicence == "TRUE")
            nCoglicence = true;
            LIST_CB_BTN.Add(CB_BTN_MOUSE_MODE);
            LIST_CB_BTN.Add(CB_BTN_POINT2POINT);
            LIST_CB_BTN.Add(CB_BTN_POINT2LINE);
            LIST_CB_BTN.Add(CB_BTN_SQUARE);
            LIST_CB_BTN.Add(CB_BTN_ARC);
            LIST_CB_BTN.Add(CB_BTN_CROSS_CUSTOM);
            LIST_CB_BTN.Add(CB_BTN_CROSS_LINE);

            //-----------------------------------------------------------------------------------------------------------------------------
            TIP_00.AutoPopDelay = 10000;
            TIP_00.InitialDelay = 500;
            TIP_00.ReshowDelay = 500;
            TIP_00.ShowAlways = true;
            TIP_00.IsBalloon = true;
            //-----------------------------------------------------------------------------------------------------------------------------
            TIP_00.SetToolTip(this.BTN_IMG_LOAD, "Load Image File");
            TIP_00.SetToolTip(this.BTN_UNDO, "Graphic Undo");
            TIP_00.SetToolTip(this.BTN_DELALL, "Graphic All Delete");
            TIP_00.SetToolTip(this.BTN_FIT, "Display Fitting");

            TIP_00.SetToolTip(this.CB_BTN_MOUSE_MODE, "Display Mouse Mode");
            TIP_00.SetToolTip(this.CB_BTN_POINT2POINT, "measuring from point to point");
            TIP_00.SetToolTip(this.CB_BTN_POINT2LINE, "measuring from line to line");
            TIP_00.SetToolTip(this.CB_BTN_SQUARE, "square area measurement");
            TIP_00.SetToolTip(this.CB_BTN_ARC, "measure R from 3 points");
            TIP_00.SetToolTip(this.CB_BTN_CROSS_CUSTOM, "Custom Cross Line");
            TIP_00.SetToolTip(this.CB_BTN_CROSS_LINE, "Cross Line");
            //-----------------------------------------------------------------------------------------------------------------------------

            cogDisplayStatusBar00.Display = CogDisplay00;
            m_CogLabel.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
            m_CogLabel.Color = CogColorConstants.Green;
            m_CogLabel.SelectedColor = CogColorConstants.Green;
            for (int i = 0; i < m_CogLine.Length; i++)
            {
                m_CogLine[i] = new CogLine();
            }
            for (int i = 0; i < m_Cogcircle.Length; i++)
            {
                m_Cogcircle[i] = new CogCircle();
            }
            DisplayManuConstants =
                DisplayEnableConstants.LoadImage |
                DisplayEnableConstants.DisplayFit |
                DisplayEnableConstants.CrossLine6 |
                DisplayEnableConstants.TouchMove0 |
                DisplayEnableConstants.PointToPoint1 |
                DisplayEnableConstants.LineToLine2 |
                DisplayEnableConstants.Arc3Points |
                DisplayEnableConstants.CrossCustom |
                DisplayEnableConstants.Undo |
                DisplayEnableConstants.Delete |
                DisplayEnableConstants.Square5;
        }



        #region Event
        private string _Name;
        private string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region private  Var
        List<CheckBox> LIST_CB_BTN = new List<CheckBox>();
        DoublePoint nDistance = new DoublePoint();
        CogColorConstants m_LabelBackColor = CogColorConstants.Black;
        //----------------------------------------------------------------------------//
        CogGraphicLabel m_CogLabel = new CogGraphicLabel();
        CogDistancePointPointTool m_CogPointTool = new CogDistancePointPointTool();
        CogAngleLineLineTool m_CogLineAngleTool = new CogAngleLineLineTool();
        //CogArc
        //----------------------------------------------------------------------------//
        CogLine[] m_CogLine = new CogLine[10];
        CogLineSegment m_CogSegment = new CogLineSegment();

        CogPointMarker M_CogCrossLine = new CogPointMarker();

        CogLineSegment m_CogSegmentOverlay0 = new CogLineSegment();
        CogLineSegment m_CogSegmentOverlay1 = new CogLineSegment();
        CogLineSegment m_CogSegmentOverlay2 = new CogLineSegment();
        CogLineSegment m_CogSegmentOverlay3 = new CogLineSegment();
        CogDistanceSegmentLineTool m_CogSegmentLineTool = new CogDistanceSegmentLineTool();
        DoublePoint nPos = new DoublePoint();
        //----------------------------------------------------------------------------//

        CogCircle[] m_Cogcircle = new CogCircle[10];
        CogFitCircle m_CogcircleFit = new CogFitCircle();
        CogFitCircleTool m_CogcircleFitTool = new CogFitCircleTool();
        //----------------------------------------------------------------------------//
        CogRectangle m_CogRectangle = new CogRectangle();
        //-----------------------------------------------------------------------------
        bool m_MouseDownFlag = false;
        DoublePoint m_Start = new DoublePoint();
        DoublePoint m_EndPos = new DoublePoint();

        static int M_TOOL_CNT = 10;
        static int M_SELECT_ = 0;
        int[] M_MOUSE_ = new int[M_TOOL_CNT];
        int[] M_Graphic_ = new int[M_TOOL_CNT];
        List<GraphicgroupName> M_GraphicgroupName = new List<GraphicgroupName>();

        private MTickTimer m_Timer = new MTickTimer();

        PointF m_CustomCross = new Point();
        bool m_UseCustomCross = false;
        private struct MTickTimer
        {
            public DateTime timeStart;
            public DateTime timeEnd;

            public void StartTimer()
            {
                timeStart = DateTime.Now;
            }

            public long GetEllapseTime()
            {
                timeEnd = DateTime.Now;
                return (timeEnd.Ticks - timeStart.Ticks) / 10000; //리턴값 1ms 
            }
        }

        private class GraphicgroupName
        {
            public string GroupName;
            public bool StaticType;
            public GraphicgroupName(string n_GraphicgroupName, bool n_GraphicsStaticType)
            {
                this.GroupName = n_GraphicgroupName;
                this.StaticType = n_GraphicsStaticType;
            }

        }
        private bool nCoglicence = false;
        private class DoublePoint
        {
            public double X;
            public double Y;
            public double T;

            public double L;

            public double X2;
            public double Y2;
            public double T2;
            public DoublePoint()
            {
                this.X = 0;
                this.Y = 0;
                this.T = 0;
                this.L = 0;

                this.X2 = 0;
                this.Y2 = 0;
                this.T2 = 0;
            }
        }
        #endregion

        //-------------------------------------------------PUBLIC--------------------
        #region public

        public PointF CustomCross
        {
            get
            {
                return m_CustomCross;
            }

            set
            {
                m_CustomCross = value;
            }
        }

        public bool UseCustomCross
        {
            get
            {
                return m_UseCustomCross;
            }

            set
            {
                m_UseCustomCross = value;
            }
        }

        public bool CrossLineChecked
        {
            get
            {
                return CB_BTN_CROSS_LINE.Checked;
            }
        }
        /// <summary> 
        /// Get measurement information string
        /// </summary>
        public Queue<string[]> M_ResultDataS = new Queue<string[]>();
        /// <summary> 
        /// Get measurement Data
        /// </summary>
        public List<double> M_ResultData = new List<double>();
        /// <summary> 
        /// Set Measurement Data Plus Or minus
        /// </summary>
        public int m_DIR = 1;
        /// <summary> 
        /// Display Image
        /// </summary>
        public ICogImage Image
        {
            get
            {
                return CogDisplay00.Image;
            }

            set
            {
                if (value != null)
                    CogDisplay00.Image = value;
            }
        }
        /// <summary> 
        /// Get , Set Display 1pixel Resolution
        /// </summary>
        public double Resuloution
        {
            get
            {
                return (double)this.NUD_RESOLUTION.Value;
            }
            set
            {
                if (value < (double)NUD_RESOLUTION.Minimum)
                    NUD_RESOLUTION.Value = 1;
                else
                {
                    if (value > (double)NUD_RESOLUTION.Maximum)
                        NUD_RESOLUTION.Maximum = (decimal)value;
                    NUD_RESOLUTION.Value = (decimal)value;
                }
            }
        }

        public void CrossLine()
        {
            LIST_CB_BTN[(int)ManuTool.CROSS_LINE].Checked = true;
            CB_BTN_CROSS_LINE_Click(CB_BTN_CROSS_LINE, null);
        }

        public void CustomCrossLine()
        {
            LIST_CB_BTN[(int)ManuTool.CUSTOM_CROSS].Checked = true;
            CB_BTN_CROSS_CUSTOM_Click(CB_BTN_CROSS_CUSTOM, null);
        }

        public void DisplayClear()
        {
            BTN_DELALL_Click(null, null);
        }
        #endregion

        private DisplayEnableConstants _DisplayManuConstants = new DisplayEnableConstants();
        public DisplayEnableConstants DisplayManuConstants
        {
            get
            {
                return this._DisplayManuConstants;
            }
            set
            {
                _DisplayManuConstants = value;
                ShowHideManu(value);
            }
        }
        private void ShowHideManu(DisplayEnableConstants _Data)
        {

            if (_Data.HasFlag(DisplayEnableConstants.None) || _Data != null)
            {
                BTN_IMG_LOAD.Visible = true;
                BTN_FIT.Visible = false;
                CB_BTN_MOUSE_MODE.Visible = false;               //Touch Move
                CB_BTN_CROSS_LINE.Visible = false;       // Cross Line
                CB_BTN_CROSS_CUSTOM.Visible = false;       // Cross Line
                CB_BTN_POINT2POINT.Visible = false;       // Arc
                CB_BTN_POINT2LINE.Visible = false;       // Line to Line
                CB_BTN_ARC.Visible = false;       // Line to Line
                CB_BTN_SQUARE.Visible = false;       //사각 
                BTN_UNDO.Visible = false;        // 
                BTN_DELALL.Visible = false;      // delet
            }
            if (_Data.HasFlag(DisplayEnableConstants.All))
            {
                BTN_IMG_LOAD.Visible = true;
                BTN_FIT.Visible = true;

                CB_BTN_MOUSE_MODE.Visible = true;      //Touch Move           
                CB_BTN_POINT2POINT.Visible = true;       //Point to Point
                CB_BTN_POINT2LINE.Visible = true;       // Line to Line
                CB_BTN_ARC.Visible = true;              // ARc
                CB_BTN_SQUARE.Visible = true;       //사각 
                CB_BTN_CROSS_LINE.Visible = true;       // Cross Line
                CB_BTN_CROSS_CUSTOM.Visible = true;       // Cross Line

                BTN_UNDO.Visible = true;        // 
                BTN_DELALL.Visible = true;      // delet
            }



            if (_Data.HasFlag(DisplayEnableConstants.LoadImage)) BTN_IMG_LOAD.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.DisplayFit)) BTN_FIT.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.TouchMove0)) CB_BTN_MOUSE_MODE.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.PointToPoint1)) CB_BTN_POINT2POINT.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.LineToLine2)) CB_BTN_POINT2LINE.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.Arc3Points)) CB_BTN_ARC.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.Square5)) CB_BTN_SQUARE.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.CrossLine6)) CB_BTN_CROSS_LINE.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.CrossCustom)) CB_BTN_CROSS_CUSTOM.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.Undo)) BTN_UNDO.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.Delete)) BTN_DELALL.Visible = true;
            if (_Data.HasFlag(DisplayEnableConstants.LoadImageNotUse)) BTN_IMG_LOAD.Visible = false;



        }
        //---------------------------------------------------------------------------

        #region Licence
        private const string PASSWORD_DIR = "Software\\CogLicence";
        private static string ReadRegistry(string _Mode)
        {
            RegistryKey reg = Registry.CurrentUser;
            reg = reg.OpenSubKey(PASSWORD_DIR, true);

            if (reg == null) return "";

            if (null != reg.GetValue(_Mode))
            {
                return Convert.ToString(reg.GetValue(_Mode));
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region Private method
        private void CBBOX_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox TempBTN = (CheckBox)sender;

            if (TempBTN.Checked)
            {
                CogDisplay00.MouseMode = CogDisplayMouseModeConstants.Pointer;
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
                int Temp_SELECT_TOOL = 0;
                for (int i = 0; i < LIST_CB_BTN.Count; i++)
                {
                    if (LIST_CB_BTN[i].Name != TempBTN.Name)
                    {
                        LIST_CB_BTN[i].Checked = false;
                    }
                    else
                        Temp_SELECT_TOOL = i;
                }

                //if (m_UseCustomCross)
                //    LIST_CB_BTN[(int)ManuTool.CUSTOM_CROSS].Checked = true;
                //else if (Temp_SELECT_TOOL != (int)ManuTool.CUSTOM_CROSS)
                //    LIST_CB_BTN[(int)ManuTool.CUSTOM_CROSS].Checked = false;
                
                //  M_SELECT_ = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));
                M_SELECT_ = Temp_SELECT_TOOL;
            }
            else
            {
                M_SELECT_ = 0;
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
            }

        }
        private void CB_BTN_MOUSE_MODE_Click(object sender, EventArgs e)
        {
            if (LIST_CB_BTN[(int)ManuTool.MOUSE_MODE].Checked)
                CogDisplay00.MouseMode = CogDisplayMouseModeConstants.Pan;
            else
                CogDisplay00.MouseMode = CogDisplayMouseModeConstants.Pointer;
        }


        private void CogDisplay00_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && M_SELECT_ != 0 && CogDisplay00.MouseMode == CogDisplayMouseModeConstants.Pointer && nCoglicence)
            {
                try
                {

                    if (M_SELECT_ == 1)
                    {
                        switch (M_MOUSE_[M_SELECT_])
                        {
                            case 0:
                                m_CogLabel.BackgroundColor = CogColorConstants.None;
                                MapPoint(CogDisplay00, e.X, e.Y, out m_Start.X, out m_Start.Y);
                                m_CogPointTool.InputImage = CogDisplay00.Image;
                                m_CogPointTool.StartX = m_Start.X;
                                m_CogPointTool.StartY = m_Start.Y;
                                m_CogPointTool.LastRunRecordDiagEnable = CogDistancePointPointLastRunRecordDiagConstants.All;
                                M_MOUSE_[M_SELECT_]++;
                                m_MouseDownFlag = true;
                                break;

                            case 1:
                                M_Graphic_[M_SELECT_]++;
                                M_MOUSE_[M_SELECT_]++;
                                m_CogLabel.BackgroundColor = m_LabelBackColor;
                                break;

                            case 2:
                                AddResultdata(nDistance.L);
                                M_Graphic_[M_SELECT_]++;
                                m_MouseDownFlag = false;
                                M_MOUSE_[M_SELECT_] = 0;
                                break;
                        }
                    }

                    if (M_SELECT_ == 2)
                    {
                        double nTemplength = 0.1;
                        if (CogDisplay00.Image != null)
                            nTemplength = (Math.Sqrt(Math.Pow(CogDisplay00.Image.Width, 2) + Math.Pow(CogDisplay00.Image.Height, 2)));

                        switch (M_MOUSE_[M_SELECT_])
                        {
                            case 0:
                                m_CogLabel.BackgroundColor = CogColorConstants.None;
                                m_MouseDownFlag = true;
                                MapPoint(CogDisplay00, e.X, e.Y, out m_Start.X, out m_Start.Y);

                                m_CogLine[0] = new CogLine();
                                m_CogLine[0].SetFromStartXYEndXY(m_Start.X, m_Start.Y, m_Start.X + 10, m_Start.Y + 10);

                                m_CogPointTool.InputImage = CogDisplay00.Image;
                                m_CogPointTool.StartX = m_Start.X;
                                m_CogPointTool.StartY = m_Start.Y;
                                m_CogPointTool.LastRunRecordDiagEnable = CogDistancePointPointLastRunRecordDiagConstants.EndPoint | CogDistancePointPointLastRunRecordDiagConstants.InputImageByReference | CogDistancePointPointLastRunRecordDiagConstants.StartPoint;

                                ///     m_CogPointTool.LastRunRecordDiagEnable = CogDistancePointPointLastRunRecordDiagConstants.All;
                                M_MOUSE_[M_SELECT_]++;
                                break;

                            case 1:
                                MapPoint(CogDisplay00, e.X, e.Y, out m_Start.X, out m_Start.Y);
                                m_CogLine[0].GetXYRotation(out nPos.X, out nPos.Y, out nPos.T);
                                m_CogLine[1] = new CogLine(m_CogLine[0]);
                                m_CogLine[1].SetXYRotation(m_Start.X, m_Start.Y, nPos.T);

                                //                          m_CogSegmentOverlay0.SetStartEnd(m_CogPointTool.StartX, m_CogPointTool.StartY, m_Start.X, m_Start.Y);
                                //                          m_CogSegmentOverlay1.SetStartEnd(m_CogPointTool.StartX, m_CogPointTool.StartY, m_Start.X, m_Start.Y);

                                m_CogSegmentOverlay0.SetStartLengthRotation(m_Start.X, m_Start.Y, nTemplength, nPos.T);
                                m_CogSegmentOverlay1.SetStartLengthRotation(m_Start.X, m_Start.Y, nTemplength, nPos.T + CogMisc.DegToRad(180));

                                m_CogSegment.SetStartLengthRotation(m_Start.X, m_Start.Y, 0.1, nPos.T);

                                M_Graphic_[M_SELECT_]++;
                                //                                 m_CogLabel = new CogGraphicLabel();
                                //                                 m_CogLabel.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                                //                                 m_CogLabel.Color = CogColorConstants.Green;
                                //                                 m_CogLabel.SelectedColor = CogColorConstants.Green;

                                m_CogSegmentLineTool.InputImage = CogDisplay00.Image;
                                m_CogSegmentLineTool.Line = m_CogLine[0];

                                M_MOUSE_[M_SELECT_]++;

                                break;

                            case 2:
                                m_CogLine[1].GetXYRotation(out nPos.X, out nPos.Y, out nPos.T);
                                m_CogSegmentOverlay2.SetStartLengthRotation(nPos.X, nPos.Y, nTemplength, nPos.T);
                                m_CogSegmentOverlay3.SetStartLengthRotation(nPos.X, nPos.Y, nTemplength, nPos.T + CogMisc.DegToRad(180));
                                m_CogLabel.BackgroundColor = m_LabelBackColor;
                                M_Graphic_[M_SELECT_]++;
                                M_MOUSE_[M_SELECT_]++;
                                break;

                            case 3:

                                M_Graphic_[M_SELECT_]++;
                                m_MouseDownFlag = false;
                                M_MOUSE_[M_SELECT_] = 0;
                                AddResultdata(nDistance.L);
                                break;


                        }
                    }

                    if (M_SELECT_ == 3)
                    {
                        switch (M_MOUSE_[M_SELECT_])
                        {
                            case 0:
                                m_CogLabel.BackgroundColor = CogColorConstants.None;
                                MapPoint(CogDisplay00, e.X, e.Y, out m_Start.X, out m_Start.Y);
                                m_CogPointTool.InputImage = CogDisplay00.Image;
                                m_CogPointTool.StartX = m_Start.X;
                                m_CogPointTool.StartY = m_Start.Y;
                                m_CogPointTool.LastRunRecordDiagEnable = CogDistancePointPointLastRunRecordDiagConstants.All;


                                //                                 m_CogLabel.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                                //                                 m_CogLabel.Color = CogColorConstants.Green;
                                //                                 m_CogLabel.SelectedColor = CogColorConstants.Green;


                                M_MOUSE_[M_SELECT_]++;
                                m_MouseDownFlag = true;
                                break;

                            case 1:
                                M_Graphic_[M_SELECT_]++;
                                M_MOUSE_[M_SELECT_]++;
                                m_CogLabel.BackgroundColor = m_LabelBackColor;
                                break;

                            case 2:
                                M_Graphic_[M_SELECT_]++;
                                m_MouseDownFlag = false;
                                M_MOUSE_[M_SELECT_] = 0;
                                AddResultdata(nDistance.L);
                                break;
                        }
                    }

                    if (M_SELECT_ == 4) // Arc
                    {
                        switch (M_MOUSE_[M_SELECT_])
                        {
                            case 0: // 첫번 째 점 클릭
                                m_CogLabel.BackgroundColor = CogColorConstants.None;
                                MapPoint(CogDisplay00, e.X, e.Y, out m_Start.X, out m_Start.Y);
                                m_CogPointTool.InputImage = CogDisplay00.Image;
                                m_CogPointTool.StartX = m_Start.X;
                                m_CogPointTool.StartY = m_Start.Y;
                                m_CogPointTool.LastRunRecordDiagEnable = CogDistancePointPointLastRunRecordDiagConstants.All;

                                m_CogcircleFitTool.InputImage = CogDisplay00.Image;
                                m_CogcircleFitTool.RunParams.NumPoints = 3;
                                m_CogcircleFitTool.RunParams.SetPoint(0, m_Start.X, m_Start.Y);
                                //                                 m_CogLabel.Alignment = CogGraphicLabelAlignmentConstants.BaselineCenter;
                                //                                 m_CogLabel.Color = CogColorConstants.Green;
                                //                                 m_CogLabel.SelectedColor = CogColorConstants.Green;


                                M_MOUSE_[M_SELECT_]++;
                                m_MouseDownFlag = true;
                                break;

                            case 1: // 두번 째 점 클릭
                                M_Graphic_[M_SELECT_]++;
                                M_MOUSE_[M_SELECT_]++;
                               
                                break;

                            case 2: // 세번 째 점 클릭
                                M_Graphic_[M_SELECT_]++;
                                M_MOUSE_[M_SELECT_]++;
                                m_CogLabel.BackgroundColor = m_LabelBackColor;
                                break;

                            case 3: // Label
                                M_Graphic_[M_SELECT_]++;
                                m_MouseDownFlag = false;
                                M_MOUSE_[M_SELECT_] = 0;
                                AddResultdata(nDistance.L);
                                break;
                        }
                    }

                    if (M_SELECT_ == 5) // Custom Cross
                    {
                        switch (M_MOUSE_[M_SELECT_])
                        {
                            case 0:
                                CB_BTN_CROSS_CUSTOM_Click(CB_BTN_CROSS_CUSTOM, null);

                                m_CogLabel.BackgroundColor = CogColorConstants.None;
                                MapPoint(CogDisplay00, e.X, e.Y, out m_Start.X, out m_Start.Y);
                                m_CustomCross.X = (float)m_Start.X;
                                m_CustomCross.Y = (float)m_Start.Y;

                                LIST_CB_BTN[(int)ManuTool.CUSTOM_CROSS].Checked = false;
                                CB_BTN_CROSS_CUSTOM_Click(CB_BTN_CROSS_CUSTOM, null);

                                LIST_CB_BTN[(int)ManuTool.CUSTOM_CROSS].Checked = true;
                                CB_BTN_CROSS_CUSTOM_Click(CB_BTN_CROSS_CUSTOM, null);

                                //List<int> sizeInScreenPixels = new List<int>();
                                //sizeInScreenPixels.Add(CogDisplay00.Size.Width);
                                //sizeInScreenPixels.Add(CogDisplay00.Size.Height);
                                //M_CogCustomCross.SetCenterRotationSize(M_CogCustomCross.X, M_CogCustomCross.Y, 0, sizeInScreenPixels.Max() * 2);
                                //CogDisplay00.InteractiveGraphics.Add(M_CogCustomCross, GetGroupName(M_Graphic_[M_SELECT_]), false);
                                //SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]), false);

                                break;
                                //M_MOUSE_[M_SELECT_]++;

                                //    m_MouseDownFlag = true;
                                //    break;

                                //case 1:
                                //    M_MOUSE_[M_SELECT_]++;
                                //    m_CogLabel.BackgroundColor = m_LabelBackColor;
                                //    M_Graphic_[M_SELECT_]++;
                                //    m_MouseDownFlag = false;
                                //    M_MOUSE_[M_SELECT_] = 0;
                                //    break;
                        }
                    }
                }
                catch
                {

                }

            }
        }
        private void CogDisplay00_MouseMove(object sender, MouseEventArgs e)
        {
            DoublePoint m_End = new DoublePoint();
            MapPoint(CogDisplay00, e.X, e.Y, out m_End.X, out m_End.Y);
            //   LB_MOUSE_POS.Text = m_End.X.ToString("0.000") + " , " + m_End.Y.ToString("0.000");

            if (M_SELECT_ != 0 && m_MouseDownFlag && CogDisplay00.MouseMode == CogDisplayMouseModeConstants.Pointer && M_MOUSE_[M_SELECT_] > 0 && nCoglicence)
            {
                try
                {


                    if (M_SELECT_ == 1)
                    {
                        if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                        {
                            //          DoublePoint nDistance = new DoublePoint();
                            if (M_MOUSE_[M_SELECT_] == 1)
                            {


                                m_CogPointTool.EndX = m_EndPos.X = m_End.X;
                                m_CogPointTool.EndY = m_EndPos.Y = m_End.Y;
                                m_CogPointTool.Run();
                                GetDistance(m_Start, m_EndPos, out nDistance);

                                if (m_CogPointTool.RunStatus.Result == CogToolResultConstants.Accept)
                                {
                                    for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                    {
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                        }
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_], "_"))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_], "_"));
                                        }

                                    }

                                    SetGraphics(CogDisplay00, m_CogPointTool.CreateLastRunRecord(), GetGroupName(M_Graphic_[M_SELECT_]));

                                    double nTempLength = nDistance.L * m_DIR;
                                    m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), nTempLength.ToString("00.00"));
                                    CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_], "_"));

                                    SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));
                                }
                            }
                            if (M_MOUSE_[M_SELECT_] == 2)
                            {
                                for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                {
                                    if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                    {
                                        CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                    }
                                }
                                //        GetDistance(m_Start, m_EndPos, out nDistance);
                                double nTempLength = nDistance.L * m_DIR;
                                m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), nTempLength.ToString("00.00"));
                                CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_]));
                                SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));

                            }
                        }
                    }

                    if (M_SELECT_ == 2)
                    {
                        if (M_MOUSE_[M_SELECT_] == 1)
                        {
                            if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                            {

                                m_CogLine[0].SetFromStartXYEndXY(m_Start.X, m_Start.Y, m_End.X, m_End.Y);
                                m_CogPointTool.InputImage = CogDisplay00.Image;
                                m_CogPointTool.EndX = m_EndPos.X = m_End.X;
                                m_CogPointTool.EndY = m_EndPos.Y = m_End.Y;
                                m_CogPointTool.Run();

                                for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                {
                                    if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                    {
                                        CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                    }
                                }
                                SetGraphics(CogDisplay00, m_CogPointTool.CreateLastRunRecord(), GetGroupName(M_Graphic_[M_SELECT_]));
                                CogDisplay00.StaticGraphics.Add(m_CogLine[0], GetGroupName(M_Graphic_[M_SELECT_]));
                                SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));

                            }
                        }
                        if (M_MOUSE_[M_SELECT_] == 2)
                        {
                            if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                            {
                                //DoublePoint nDistance = new DoublePoint();
                                GetDistance(m_Start, m_End, out nDistance);
                                m_EndPos.X = m_End.X;
                                m_EndPos.Y = m_End.Y;

                                m_CogLine[1].SetXYRotation(m_End.X, m_End.Y, nPos.T);
                                m_CogSegment.SetStartLengthRotation(m_End.X, m_End.Y, 0.1, nPos.T);

                                m_CogSegmentLineTool.InputImage = CogDisplay00.Image;
                                m_CogSegmentLineTool.Segment = m_CogSegment;
                                m_CogSegmentLineTool.Run();


                                if (m_CogSegmentLineTool.RunStatus.Result == CogToolResultConstants.Accept)
                                {
                                    nDistance.L = m_CogSegmentLineTool.Distance;
                                    for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                    {
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));

                                        }
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_], "__"))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_], "__"));
                                        }
                                    }
                                    nDistance.L = GetRobotValue(nDistance.L);
                                    double nTempLength = nDistance.L * m_DIR;

                                    m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), nTempLength.ToString("00.00"));
                                    SetGraphics(CogDisplay00, m_CogSegmentLineTool.CreateLastRunRecord(), GetGroupName(M_Graphic_[M_SELECT_]));
                                    CogDisplay00.StaticGraphics.Add(m_CogLine[1], GetGroupName(M_Graphic_[M_SELECT_]));
                                    CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_], "__"));
                                    //                                     CogDisplay00.StaticGraphics.Add(m_CogSegmentOverlay0, GetGroupName(M_Graphic_[M_SELECT_]));
                                    //                                     CogDisplay00.StaticGraphics.Add(m_CogSegmentOverlay1, GetGroupName(M_Graphic_[M_SELECT_]));
                                    SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));
                                }
                            }
                        }
                        if (M_MOUSE_[M_SELECT_] == 3)
                        {
                            if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                            {
                                m_EndPos.X = m_End.X;
                                m_EndPos.Y = m_End.Y;

                                if (m_CogSegmentLineTool.RunStatus.Result == CogToolResultConstants.Accept)
                                {
                                    for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                    {
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                        }
                                    }
                                    double nTempLength = nDistance.L * m_DIR;
                                    m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), nTempLength.ToString("00.00"));
                                    CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_]));
                                    //                                     CogDisplay00.StaticGraphics.Add(m_CogSegmentOverlay2, GetGroupName(M_Graphic_[M_SELECT_]));
                                    //                                     CogDisplay00.StaticGraphics.Add(m_CogSegmentOverlay3, GetGroupName(M_Graphic_[M_SELECT_]));
                                    SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));
                                }
                            }
                        }


                    }


                    if (M_SELECT_ == 3)
                    {
                        if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                        {
                            if (M_MOUSE_[M_SELECT_] == 1)
                            {
                                m_CogPointTool.EndX = m_EndPos.X = m_End.X;
                                m_CogPointTool.EndY = m_EndPos.Y = m_End.Y;
                                m_CogPointTool.Run();

                                GetDistance(m_Start, m_EndPos, out nDistance);

                                if (m_CogPointTool.RunStatus.Result == CogToolResultConstants.Accept)
                                {
                                    for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                    {
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                        }
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_], "_"))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_], "_"));
                                        }

                                    }

                                    SetGraphics(CogDisplay00, m_CogPointTool.CreateLastRunRecord(), GetGroupName(M_Graphic_[M_SELECT_]));

                                    if (Math.Abs(m_EndPos.X - m_Start.X) > 1 && Math.Abs(m_EndPos.Y - m_Start.Y) > 1)
                                    {
                                        m_CogRectangle.SetCenterWidthHeight(m_Start.X + (m_EndPos.X - m_Start.X) / 2, m_Start.Y + (m_EndPos.Y - m_Start.Y) / 2, Math.Abs(m_EndPos.X - m_Start.X), Math.Abs(m_EndPos.Y - m_Start.Y));
                                        CogDisplay00.StaticGraphics.Add(m_CogRectangle, GetGroupName(M_Graphic_[M_SELECT_]));
                                    }
                                    double nTempLength = nDistance.L * m_DIR;
                                    m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), "L:" + nTempLength.ToString("00.00") + " ,X:" + nDistance.X.ToString("00.00") + " ,Y:" + nDistance.Y.ToString("00.00") + " ,T:" + nDistance.T.ToString("00.00"));
                                    CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_], "_"));

                                    SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));
                                }
                            }
                            if (M_MOUSE_[M_SELECT_] == 2)
                            {
                                for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                {
                                    if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                    {
                                        CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                    }
                                }
                                // GetDistance(m_Start, m_EndPos, out nDistance);
                                double nTempLength = nDistance.L * m_DIR;
                                m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), "L:" + nTempLength.ToString("00.00") + " ,X:" + nDistance.X.ToString("00.00") + " ,Y:" + nDistance.Y.ToString("00.00") + " ,T:" + nDistance.T.ToString("00.00"));
                                CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_]));
                                SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));

                            }
                        }
                    }

                    if (M_SELECT_ == 4)
                    {
                        if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                        {
                            if (M_MOUSE_[M_SELECT_] == 1)   // 점 하나만 확정된 상태
                            {
                                if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)   // 두번 째 점은 움직이고 있는 상태
                                {
                                    m_CogPointTool.EndX = m_EndPos.X = m_End.X;
                                    m_CogPointTool.EndY = m_EndPos.Y = m_End.Y;
                                    m_CogPointTool.Run();

                                    m_CogcircleFitTool.RunParams.SetPoint(1, m_End.X, m_End.Y);

                                    for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                    {
                                        if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                        {
                                            CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                        }
                                    }
                                    SetGraphics(CogDisplay00, m_CogPointTool.CreateLastRunRecord(), GetGroupName(M_Graphic_[M_SELECT_]));

                                    SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));
                                }
                            }
                            if (M_MOUSE_[M_SELECT_] == 2)   // 점 두개가 확정된 상태
                            {
                                if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)   // 세번 째 점은 움직이고 있는 상태
                                {
                                    m_EndPos.X = m_End.X;
                                    m_EndPos.Y = m_End.Y;

                                    m_CogcircleFitTool.RunParams.SetPoint(2, m_End.X, m_End.Y);
                                    m_CogcircleFitTool.Run();

                                    if (m_CogcircleFitTool.RunStatus.Result == CogToolResultConstants.Accept)
                                    {
                                        nDistance.L = m_CogcircleFitTool.Result.GetCircle().Radius;
                                        m_Cogcircle[0].SetCenterRadius(m_CogcircleFitTool.Result.GetCircle().CenterX,
                                            m_CogcircleFitTool.Result.GetCircle().CenterY,
                                            m_CogcircleFitTool.Result.GetCircle().Radius);
                                        for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                        {
                                            if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                            {
                                                CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));

                                            }
                                            if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_], "_"))
                                            {
                                                CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_], "_"));
                                            }
                                        }
                                        nDistance.L = GetRobotValue(nDistance.L);
                                        double nTempLength = nDistance.L;

                                        m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), nTempLength.ToString("00.00"));
                                        SetGraphics(CogDisplay00, m_CogcircleFitTool.CreateLastRunRecord(), GetGroupName(M_Graphic_[M_SELECT_]));
                                        CogDisplay00.StaticGraphics.Add(m_Cogcircle[0], GetGroupName(M_Graphic_[M_SELECT_]));
                                        CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_], "_"));

                                        SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));
                                    }
                                }
                            }
                            if (M_MOUSE_[M_SELECT_] == 3)
                            {
                                if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                                {
                                    m_EndPos.X = m_End.X;
                                    m_EndPos.Y = m_End.Y;

                                    if (m_CogSegmentLineTool.RunStatus.Result == CogToolResultConstants.Accept)
                                    {
                                        for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                                        {
                                            if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                                            {
                                                CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                                            }
                                        }
                                        double nTempLength = nDistance.L;
                                        m_CogLabel.SetXYText(m_End.X, (m_End.Y - 5), nTempLength.ToString("00.00"));
                                        CogDisplay00.StaticGraphics.Add(m_CogLabel, GetGroupName(M_Graphic_[M_SELECT_]));
                                        //                                     CogDisplay00.StaticGraphics.Add(m_CogSegmentOverlay2, GetGroupName(M_Graphic_[M_SELECT_]));
                                        //                                     CogDisplay00.StaticGraphics.Add(m_CogSegmentOverlay3, GetGroupName(M_Graphic_[M_SELECT_]));
                                        SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]));
                                    }
                                }
                            }
                        }
                    }


                    //if (M_SELECT_ == 6)
                    //{
                    //    if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)
                    //    {
                    //        if (M_MOUSE_[M_SELECT_] == 1)   // 점 하나만 확정된 상태
                    //        {
                    //            if (m_Start.X != m_End.X || m_Start.Y != m_End.Y)   // 두번 째 점은 움직이고 있는 상태
                    //            {
                    //                M_CogCustomCross.X = m_EndPos.X = m_End.X;
                    //                M_CogCustomCross.Y = m_EndPos.Y = m_End.Y;

                    //                for (int i = 0; i < CogDisplay00.StaticGraphics.ZOrderGroups.Count; i++)
                    //                {
                    //                    if (CogDisplay00.StaticGraphics.ZOrderGroups[i] == GetGroupName(M_Graphic_[M_SELECT_]))
                    //                    {
                    //                        CogDisplay00.StaticGraphics.Remove(GetGroupName(M_Graphic_[M_SELECT_]));
                    //                    }
                    //                }

                    //                List<int> sizeInScreenPixels = new List<int>();
                    //                sizeInScreenPixels.Add(CogDisplay00.Size.Width);
                    //                sizeInScreenPixels.Add(CogDisplay00.Size.Height);
                    //                M_CogCustomCross.SetCenterRotationSize((CogDisplay00.Image.Width / 2), (CogDisplay00.Image.Height / 2), 0, sizeInScreenPixels.Max() * 2);
                    //                CogDisplay00.InteractiveGraphics.Add(M_CogCustomCross, GetGroupName(M_Graphic_[M_SELECT_]), false);
                    //                SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]), false);
                    //            }
                    //        }
                    //    }
                    //}
                }
                catch
                {

                }
            }
        }
        private void AddResultdata(double nTempData)
        {
            double nTempLength = nTempData * m_DIR;
            M_ResultData.Add(nTempLength);
            string[] nMessage = new string[1];

            string nToolname;
            int nNum = GetToolNumber(M_SELECT_.ToString(), out nToolname);
            nMessage = new string[] { M_ResultData.Count.ToString("00"), nToolname, nTempLength.ToString("00.00") };

            M_ResultDataS.Enqueue(nMessage);

        }
        public static void SetInteractiveGraphics(CogDisplay display, ICogRecord _ICogRecord, bool _bGraphicIndex)
        {
            foreach (Cognex.VisionPro.Implementation.CogRecord _record in _ICogRecord.SubRecords)
            {
                if (typeof(ICogImage).IsAssignableFrom(_record.ContentType))
                {
                    if (_record.Content != null)
                    {
                        display.Image = (ICogImage)_record.Content;
                    }
                }
                else if (typeof(ICogGraphic).IsAssignableFrom(_record.ContentType))
                {
                    if (_record.Content != null)
                    {
                        display.InteractiveGraphics.Add(_record.Content as ICogGraphicInteractive, _bGraphicIndex ? "Result0" : "Result1", false);
                    }
                }
                else if (typeof(CogGraphicCollection).IsAssignableFrom(_record.ContentType))
                {
                    if (_record.Content != null)
                    {
                        CogGraphicCollection graphics = _record.Content as CogGraphicCollection;
                        foreach (ICogGraphic graphic in graphics)
                        {
                            //graphic.Color = CogColorConstants.Red;
                            display.InteractiveGraphics.Add(graphic as ICogGraphicInteractive, _bGraphicIndex ? "Result0" : "Result1", false);
                        }
                    }
                }
                else if (typeof(CogGraphicInteractiveCollection).IsAssignableFrom(_record.ContentType))
                {
                    if (_record.Content != null)
                    {
                        display.InteractiveGraphics.AddList(_record.Content as CogGraphicInteractiveCollection, _bGraphicIndex ? "Result0" : "Result1", false);
                    }
                }

                SetInteractiveGraphics(display, _record, _bGraphicIndex);
            }
        }
        public static void SetGraphics(CogRecordDisplay display, ICogRecord record, string groupName)
        {
            try
            {
                foreach (Cognex.VisionPro.Implementation.CogRecord _record in record.SubRecords)
                {
                    if (typeof(ICogGraphic).IsAssignableFrom(_record.ContentType))
                    {
                        if (_record.Content != null)
                            display.StaticGraphics.Add(_record.Content as ICogGraphicInteractive, groupName);
                    }

                    else if (typeof(CogGraphicCollection).IsAssignableFrom(_record.ContentType))
                    {
                        if (_record.Content != null)
                        {
                            CogGraphicCollection graphics = _record.Content as CogGraphicCollection;
                            foreach (ICogGraphic graphic in graphics)
                            {
                                display.StaticGraphics.Add(graphic as ICogGraphicInteractive, groupName);
                            }
                        }
                    }
                    else if (typeof(CogGraphicInteractiveCollection).IsAssignableFrom(_record.ContentType))
                    {
                        if (_record.Content != null)
                        {
                            display.StaticGraphics.AddList(_record.Content as CogGraphicCollection, groupName);
                        }
                    }

                    SetGraphics(display, _record, groupName);
                }
            }
            catch
            {

            }
        }
        private string GetGroupName(int GraphicCount, string nChar = null)
        {
            string ngroupName = "";
            string nID = "";
            try
            {
                switch (M_SELECT_)
                {
                    case 1:
                        nID = "A";
                        if (M_MOUSE_[M_SELECT_] == 1)
                        {
                            if (nChar == null)
                                ngroupName = nID + GraphicCount.ToString("00");
                            else
                                ngroupName = nID + nChar + (GraphicCount + 1).ToString("00");
                        }
                        if (M_MOUSE_[M_SELECT_] == 2)
                        {
                            ngroupName = nID + "_" + GraphicCount.ToString("00");
                        }
                        break;

                    case 2:
                        nID = "B";
                        if (M_MOUSE_[M_SELECT_] == 1)
                            ngroupName = nID + nChar + GraphicCount.ToString("00");
                        if (M_MOUSE_[M_SELECT_] == 2)
                        {
                            ngroupName = nID + nChar + (GraphicCount + 1).ToString("00");
                        }
                        if (M_MOUSE_[M_SELECT_] == 3)
                        {
                            ngroupName = nID + "__" + GraphicCount.ToString("00");
                        }
                        break;

                    case 3:
                        nID = "C";
                        if (M_MOUSE_[M_SELECT_] == 1)
                        {
                            if (nChar == null)
                                ngroupName = nID + GraphicCount.ToString("00");
                            else
                                ngroupName = nID + nChar + (GraphicCount + 1).ToString("00");
                        }
                        if (M_MOUSE_[M_SELECT_] == 2)
                        {
                            ngroupName = nID + "_" + GraphicCount.ToString("00");
                        }
                        break;

                    case 4:
                        nID = "D";
                        if (M_MOUSE_[M_SELECT_] == 1)
                            ngroupName = nID + nChar + GraphicCount.ToString("00");
                        if (M_MOUSE_[M_SELECT_] == 2)
                        {
                            ngroupName = nID + nChar + (GraphicCount + 1).ToString("00");
                        }
                        if (M_MOUSE_[M_SELECT_] == 3)
                        {
                            ngroupName = nID + "_" + GraphicCount.ToString("00");
                        }
                        break;

                    case 5:
                        nID = "E";
                        ngroupName = nID + GraphicCount.ToString("00");
                        break;

                    case 6:
                        nID = "F";
                        ngroupName = nID + GraphicCount.ToString("00");
                        break;
                }
            }
            catch
            {

            }
            return ngroupName;
        }
        private void SetGroupName(string groupName, bool nStaticGraphics = true)
        {
            bool ret = true;
            for (int i = 0; i < M_GraphicgroupName.Count; i++)
            {
                if (M_GraphicgroupName[i].GroupName == groupName)
                    ret = false;
            }
            if (ret)
            {
                M_GraphicgroupName.Add(new GraphicgroupName(groupName, nStaticGraphics));
            }
        }
        void MapPoint(CogRecordDisplay Display, double eX, double eY, out double mappedX, out double mappedY)
        {
            Display.GetTransform("#", "*").MapPoint(eX, eY, out mappedX, out mappedY);
        }
        void GetDistance(DoublePoint nStartPoint, DoublePoint nEndPoint, out DoublePoint nDistance)
        {
            try
            {
                double nRusolution = 1;
                nRusolution = (double)NUD_RESOLUTION.Value;

                double nDx, nDy;
                nDistance = new DoublePoint();

                nDx = Math.Abs(nEndPoint.X - nStartPoint.X);
                nDy = Math.Abs(nEndPoint.Y - nStartPoint.Y);

                nDistance.X = nDx * nRusolution;
                nDistance.Y = nDy * nRusolution;
                nDistance.L = (Math.Sqrt(Math.Pow(nDx, 2) + Math.Pow(nDy, 2))) * nRusolution;
                nDistance.T = CogMisc.RadToDeg(Math.Atan(nDy / nDx));
            }
            catch
            {
                nDistance = new DoublePoint();
            }
        }
        private double GetRobotValue(double nPixelData)
        {
            double nRobotData = 0;
            double nRusolution = 1;
            nRusolution = (double)NUD_RESOLUTION.Value;

            nRobotData = nPixelData * nRusolution;

            return nRobotData;
        }
        private int GetToolNumber(string groupID, out string ToolName)
        {
            int nRet = 1;
            ToolName = "";
            switch (groupID)
            {
                case "A":   // Group Name일 때
                case "1":   // Tool Number일 때
                    nRet = 1;
                    ToolName = "2점간";
                    break;

                case "B":
                case "2":
                    nRet = 2;
                    ToolName = "평행선";
                    break;

                case "C":
                case "3":
                    nRet = 3;
                    ToolName = "2점간사각";
                    break;

                case "D":
                case "4":
                    nRet = 4;
                    ToolName = "원호";
                    break;

                case "E":
                case "5":
                    nRet = 5;
                    ToolName = "커스텀크로스";
                    break;


                case "F":
                case "6":
                    nRet = 6;
                    ToolName = "크로스";
                    break;
            }
            return nRet;
        }
        private void BTN_UNDO_Click(object sender, EventArgs e)
        {
            int nMinusCnt = 1;
            try
            {
                if (M_GraphicgroupName.Count > 0)
                {
                    int nIndex = Convert.ToInt16(M_GraphicgroupName[M_GraphicgroupName.Count - 1].GroupName.Substring(M_GraphicgroupName[M_GraphicgroupName.Count - 1].GroupName.Length - 2, 2));
                    switch (M_GraphicgroupName[M_GraphicgroupName.Count - 1].GroupName.Substring(0, 1))
                    {
                        case "A":
                            nMinusCnt = 2;
                            break;

                        case "B":
                            nMinusCnt = 3;
                            break;

                        case "C":
                            nMinusCnt = 2;
                            break;

                        case "D":
                            nMinusCnt = 3;
                            break;

                        case "E":
                            nMinusCnt = 1;
                            break;

                        case "F":
                            nMinusCnt = 1;
                            break;

                    }
                    string nTemp;
                    M_Graphic_[GetToolNumber(M_GraphicgroupName[M_GraphicgroupName.Count - 1].GroupName.Substring(0, 1), out nTemp)] -= nMinusCnt;
                    for (int i = 0; i < nMinusCnt; i++)
                    {
                        if (M_GraphicgroupName[M_GraphicgroupName.Count - 1].StaticType)
                        {
                            CogDisplay00.StaticGraphics.Remove(M_GraphicgroupName[M_GraphicgroupName.Count - 1].GroupName);
                        }
                        else
                        {
                            CogDisplay00.InteractiveGraphics.Remove(M_GraphicgroupName[M_GraphicgroupName.Count - 1].GroupName);
                        }

                        M_GraphicgroupName.RemoveAt(M_GraphicgroupName.Count - 1);
                    }

                }
            }
            catch
            {


            }
        }
        private void BTN_DELALL_Click(object sender, EventArgs e)
        {
            m_MouseDownFlag = false;
            timer1.Enabled = false;
            for (int i = 0; i < M_Graphic_.Length; i++)
            {
                M_Graphic_[i] = 0;
            }
            try
            {
                M_GraphicgroupName.Clear();
                CogDisplay00.StaticGraphics.Clear();
                CogDisplay00.InteractiveGraphics.Clear();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
            }
        }
        private void BTN_IMG_LOAD_Click(object sender, EventArgs e)
        {
            CogImageFileTool ImageFileTool = new CogImageFileTool();
            openFileDialog1.ReadOnlyChecked = true;
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                try
                {
                    ImageFileTool.Operator.Open(openFileDialog1.FileName, CogImageFileModeConstants.Read);
                    ImageFileTool.Run();
                    if (ImageFileTool.RunStatus.Result == CogToolResultConstants.Accept)
                    {
                        CogDisplay00.Image = ImageFileTool.OutputImage;
                        DisplayFit();
                    }
                }
                catch (System.Exception ex)
                {

                }

            }
        }
        private void CB_BTN_CROSS_LINE_Click(object sender, EventArgs e)
        {
            try
            {
                if (LIST_CB_BTN[(int)ManuTool.CROSS_LINE].Checked)
                {
#if true
                    M_Graphic_[M_SELECT_] = 0;

                    CogColorConstants _TempColor = m_CogSegment.Color;

                    m_CogSegment.Interactive = true;
                    m_CogSegment.GraphicDOFEnable = CogLineSegmentDOFConstants.All;
                    m_CogSegment.Color = CogColorConstants.Green;
                    m_CogSegment.SetStartEnd(0, (CogDisplay00.Image.Height / 4) * 2, CogDisplay00.Image.Width, (CogDisplay00.Image.Height / 4) * 2);
                    CogDisplay00.StaticGraphics.Add(m_CogSegment, GetGroupName(M_Graphic_[M_SELECT_]));

                    m_CogSegment.SetStartEnd((CogDisplay00.Image.Width / 4) * 2, 0, (CogDisplay00.Image.Width / 4) * 2, CogDisplay00.Image.Height);
                    CogDisplay00.StaticGraphics.Add(m_CogSegment, GetGroupName(M_Graphic_[M_SELECT_]));

                    m_CogSegment.Color = _TempColor;
                    SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]), true);
                    M_Graphic_[M_SELECT_]++;
#endif
                    #region
                    //                     M_Graphic_[M_SELECT_] = 0;
                    // 
                    //                     CogColorConstants _TempColor = m_CogSegment.Color;
                    // 
                    //                     //                     for (int i = 0; i < m_CogSegment_Cross.Length; i++)
                    //                     //                     {
                    //                     //                         m_CogSegment_Cross[i] = new CogLineSegment();
                    //                     //                         m_CogSegment_Cross[i].Interactive = true;
                    //                     //                         m_CogSegment_Cross[i].GraphicDOFEnable = CogLineSegmentDOFConstants.All;
                    //                     //                         m_CogSegment_Cross[i].Color = CogColorConstants.Magenta;
                    //                     //                     }
                    //                     // 
                    //                     //                     m_CogSegment_Cross[0].SetStartEnd(0, (CogDisplay00.Image.Height / 4) * 2, CogDisplay00.Image.Width, (CogDisplay00.Image.Height / 4) * 2);
                    //                     //                     CogDisplay00.InteractiveGraphics.Add(m_CogSegment_Cross[0], GetGroupName(M_Graphic_[M_SELECT_]), false);
                    //                     // 
                    //                     //                     m_CogSegment_Cross[1].SetStartEnd((CogDisplay00.Image.Width / 4) * 2, 0, (CogDisplay00.Image.Width / 4) * 2, CogDisplay00.Image.Height);
                    //                     //                     CogDisplay00.InteractiveGraphics.Add(m_CogSegment_Cross[1], GetGroupName(M_Graphic_[M_SELECT_]), false);
                    // 
                    //                     M_CogCrossLine.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
                    //                     M_CogCrossLine.Interactive = true;
                    //                     List<int> sizeInScreenPixels = new List<int>();
                    //                     sizeInScreenPixels.Add(CogDisplay00.Size.Width);
                    //                     sizeInScreenPixels.Add(CogDisplay00.Size.Height);
                    // 
                    //                     M_CogCrossLine.SetCenterRotationSize((CogDisplay00.Image.Width / 2), (CogDisplay00.Image.Height / 2), 0, sizeInScreenPixels.Max() * 2);
                    //                     CogDisplay00.InteractiveGraphics.Add(M_CogCrossLine, GetGroupName(M_Graphic_[M_SELECT_]), false);
                    //                     SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]), false);
                    //                     M_Graphic_[M_SELECT_]++;
                    #endregion
                }
                else
                {

                    if (M_GraphicgroupName.Count > 0)
                    {
                        int nDeleteCount = 0;
                        for (int i = 0; i < M_GraphicgroupName.Count; i++/*i = M_GraphicgroupName.Count - 1*/)
                        {
                            if (M_GraphicgroupName[i].GroupName.Substring(0, 1) == "F")
                            {
                                if (M_GraphicgroupName[i].StaticType)
                                    CogDisplay00.StaticGraphics.Remove(M_GraphicgroupName[i].GroupName);
                                else
                                    CogDisplay00.InteractiveGraphics.Remove(M_GraphicgroupName[i].GroupName);

                                M_GraphicgroupName.RemoveAt(i);
                                nDeleteCount++;
                            }
                            if (M_GraphicgroupName.Count <= 0 || nDeleteCount == M_Graphic_[(int)ManuTool.CROSS_LINE])
                                break;
                        }
                        M_Graphic_[(int)ManuTool.CROSS_LINE] = 0;

                    }
                }
            }
            catch
            {

            }
        }

        private void BTN_FIT_Click(object sender, EventArgs e)
        {
            DisplayFit(CogDisplay00);
        }
        delegate void dGrabRefresh(Cognex.VisionPro.Display.CogDisplay CogDisplay);
        private static void DisplayFit(Cognex.VisionPro.Display.CogDisplay CogDisplay)
        {

            if (CogDisplay.InvokeRequired)
            {
                dGrabRefresh call = new dGrabRefresh(DisplayFit);
                CogDisplay.Invoke(call, CogDisplay);
            }
            else
            {
                CogDisplay.AutoFit = true;
                CogDisplay.Fit(false);
                ///           CogDisplay.Fit(true);
            }
        }
        public void DisplayFit()
        {
            DisplayFit(CogDisplay00);
        }
        private void UC_DISPLAY_Load(object sender, EventArgs e)
        {

        }

        #endregion

        private void CB_BTN_CROSS_CUSTOM_Click(object sender, EventArgs e)
        {
            try
            {
                if (LIST_CB_BTN[(int)ManuTool.CUSTOM_CROSS].Checked)
                {
                    m_UseCustomCross = true;
                    M_SELECT_ = (int)ManuTool.CUSTOM_CROSS;

                    M_Graphic_[M_SELECT_] = 0;

                    CogColorConstants _TempColor = m_CogSegment.Color;

                    m_CogSegment.Interactive = true;
                    m_CogSegment.GraphicDOFEnable = CogLineSegmentDOFConstants.All;
                    m_CogSegment.Color = CogColorConstants.Magenta;
                    m_CogSegment.SetStartEnd(0, m_CustomCross.Y, CogDisplay00.Image.Width, m_CustomCross.Y);
                    CogDisplay00.StaticGraphics.Add(m_CogSegment, GetGroupName(M_Graphic_[M_SELECT_]));

                    m_CogSegment.SetStartEnd(m_CustomCross.X, 0, m_CustomCross.X, CogDisplay00.Image.Height);
                    CogDisplay00.StaticGraphics.Add(m_CogSegment, GetGroupName(M_Graphic_[M_SELECT_]));

                    m_CogSegment.Color = _TempColor;
                    SetGroupName(GetGroupName(M_Graphic_[M_SELECT_]), true);
                    M_Graphic_[M_SELECT_]++;

                    CB_BTN_CROSS_CUSTOM.BackColor = System.Drawing.Color.LawnGreen;
                }
                else
                {
                    m_UseCustomCross = false;

                    M_SELECT_ = (int)ManuTool.MOUSE_MODE;

                    if (M_GraphicgroupName.Count > 0)
                    {
                        int nDeleteCount = 0;
                        for (int i = 0; i < M_GraphicgroupName.Count; i++/* i = M_GraphicgroupName.Count - 1*/)
                        {
                            if (M_GraphicgroupName[i].GroupName.Substring(0, 1) == "E")
                            {
                                if (M_GraphicgroupName[i].StaticType)
                                    CogDisplay00.StaticGraphics.Remove(M_GraphicgroupName[i].GroupName);
                                else
                                    CogDisplay00.InteractiveGraphics.Remove(M_GraphicgroupName[i].GroupName);

                                M_GraphicgroupName.RemoveAt(i);
                                nDeleteCount++;
                            }
                            if (M_GraphicgroupName.Count <= 0 || nDeleteCount == M_Graphic_[(int)ManuTool.CUSTOM_CROSS])
                                break;
                        }
                        M_Graphic_[(int)ManuTool.CUSTOM_CROSS] = 0;

                        CB_BTN_CROSS_CUSTOM.BackColor = System.Drawing.Color.DarkGray;
                    }
                }
            }
            catch
            {

            }
        }

        private void CB_BTN_CROSS_CUSTOM_CheckedChanged(object sender, EventArgs e)
        {
            //if (UseCustomCross)
            //    CB_BTN_CROSS_CUSTOM.BackColor = System.Drawing.Color.LawnGreen;
            //else
            //    CB_BTN_CROSS_CUSTOM.BackColor = System.Drawing.Color.DarkGray;
        }
    }
    [Flags]
    public enum DisplayEnableConstants
    {
        None = -1,
        All = 8192, //All과 LoadImageNotUse 같이쓸려고 8192로 선언함. 
        LoadImage = 1,
        DisplayFit = 2,
        Undo = 4,
        Delete = 8,
        TouchMove0 = 16,
        PointToPoint1 = 32,
        LineToLine2 = 64,
        Arc3Points = 128,
        Square5 = 512,
        CrossLine6 = 1024,
        CrossCustom = 2048,
        LoadImageNotUse = 4096,

    }
}

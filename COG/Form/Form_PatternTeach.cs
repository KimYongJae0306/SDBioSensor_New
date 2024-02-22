//#define SD_BIO_VENT
//#define SD_BIO_PATH 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;



using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.CNLSearch;
using Cognex.VisionPro.Implementation;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.SearchMax;
using Cognex.VisionPro.LineMax;
using Cognex.VisionPro.Inspection;
using JAS.Controls.Display;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.Util;
using System.Runtime.InteropServices;

namespace COG
{
    public partial class Form_PatternTeach : Form
    {
        public bool m_ToolShow;
        public int m_AlignNo;
        public int m_PatTagNo;
        private int m_CamNo;
        private int m_RetiMode;
        private int m_PatNo;
        private int m_SelectLight = 0;

        private int m_PatNo_Sub = 0;
        private int m_SelectBlob = 0;
        private int m_SelectCaliper = 0;
        private int m_SelectFindLine = 0;
        private int m_LineSubNo = 0;
        private int m_SelectCircle = 0;

        private bool m_TABCHANGE_MODE = false;
        private bool m_PatchangeFlag = false; //Blob번호변경이나 , caliper 번호변경시
        private bool ThresValue_Sts = false;
        private bool NUD_Initial_Flag = false;

        private int M_TOOL_MODE = 0;

        private const int M_PATTERN = 1;
        private const int M_SEARCH = 2;
        private const int M_ORIGIN = 3;

        private const int M_ORIGIN_SIZE_S = 120;
        private int M_ORIGIN_SIZE = 120;
        private double ZoomBackup = new double();
        private bool bROICopy = false;

        private CogSearchMaxTool[,] PT_Pattern = new CogSearchMaxTool[Main.DEFINE.Pattern_Max, Main.DEFINE.SUBPATTERNMAX]; //MAIN PAT ++[m_PatNo,m_PatSubNo]
        private CogPMAlignTool[,] PT_GPattern = new CogPMAlignTool[Main.DEFINE.Pattern_Max, Main.DEFINE.SUBPATTERNMAX]; //MAIN PAT ++[m_PatNo,m_PatSubNo]
        private bool bLiveStop = false;
        private bool[,] PT_Pattern_USE = new bool[Main.DEFINE.Pattern_Max, Main.DEFINE.SUBPATTERNMAX];
        private double[] PT_AcceptScore = new double[Main.DEFINE.Pattern_Max];
        private double[] PT_GAcceptScore = new double[Main.DEFINE.Pattern_Max];
        CogRectangle PatMaxTrainRegion;
        CogRectangle PatMaxSearchRegion;
        CogPointMarker MarkORGPoint = new CogPointMarker();

        private List<CogFindLineTool> PT_PMFindLineTool = new List<CogFindLineTool>();
        private CogBlobTool PT_PMBlobTool = new CogBlobTool();

        private List<Label> Light_Text = new List<Label>();
        private List<RadioButton> LightRadio = new List<RadioButton>();
        private List<RadioButton> RBTN_PAT = new List<RadioButton>();
        private List<RadioButton> RBTN_CALIPER = new List<RadioButton>();
        private List<RadioButton> RBTN_FINDLINE = new List<RadioButton>();
        private List<RadioButton> RBTN_CIRCLE = new List<RadioButton>();
        private List<RadioButton> RBTN_LINEMAX_H_COND = new List<RadioButton>();
        private List<RadioButton> RBTN_LINEMAX_V_COND = new List<RadioButton>();
        private List<RadioButton> RBTN_CALIPER_METHOD = new List<RadioButton>();
        private List<RadioButton> RBTN_CIR_CALIPER_METHOD = new List<RadioButton>();

        private List<Button> BTN_TOOLSET = new List<Button>();
        private List<string> TOOLTYPE = new List<string>();

        private CogToolBlock PT_BlobToolBlock = new CogToolBlock();
        private Form_ToolTeach ToolTeach = new Form_ToolTeach();
        private Form_PatternMask FormPatternMask = new Form_PatternMask();

        private Main.MTickTimer m_Timer = new Main.MTickTimer();

        private CogFixtureTool PT_FixtureTool = new CogFixtureTool();
        private CogTransform2DLinear PatResult = new CogTransform2DLinear();


        CogPointMarker[,] MarkPoint = new CogPointMarker[Main.DEFINE.Pattern_Max, 2];
        CogDistancePointPointTool nDistance = new CogDistancePointPointTool();
        bool[] nDistanceShow = new bool[Main.DEFINE.Pattern_Max];
        int nCrossSize = 200;
        #region  BLOB
        private Main.BlobTagData[,] PT_BlobPara = new Main.BlobTagData[Main.DEFINE.Pattern_Max, Main.DEFINE.BLOB_CNT_MAX];  //para
        private CogBlobTool[,] PT_BlobTools = new CogBlobTool[Main.DEFINE.Pattern_Max, Main.DEFINE.BLOB_CNT_MAX];
        CogRectangleAffine BlobTrainRegion = new CogRectangleAffine();
        private bool[] PT_Blob_MarkUSE = new bool[Main.DEFINE.Pattern_Max];
        private bool[] PT_Blob_CaliperUSE = new bool[Main.DEFINE.Pattern_Max];
        private int[] PT_Blob_InspCnt = new int[Main.DEFINE.Pattern_Max];
        #endregion

        #region CALIPER
        private static Main.CaliperTagData[,] PT_CaliPara = new Main.CaliperTagData[Main.DEFINE.Pattern_Max, Main.DEFINE.CALIPER_MAX];
        private CogCaliperTool[,] PT_CaliperTools = new CogCaliperTool[Main.DEFINE.Pattern_Max, Main.DEFINE.CALIPER_MAX];
        private CogRectangleAffine PTCaliperRegion;
        private CogRectangleAffine[] PTCaliperDividedRegion;
        private bool[] PT_Caliper_MarkUSE = new bool[Main.DEFINE.Pattern_Max];
        private string[] PT_CaliperName = new string[Main.DEFINE.CALIPER_MAX];
        #endregion
        private enum enumTabSelect { Insp = 0, ThetaOrigin }
        private enumTabSelect _eTabSelect = new enumTabSelect();
        #region FINDLine
        private static Main.FindLineTagData[,,] PT_FindLinePara = new Main.FindLineTagData[Main.DEFINE.Pattern_Max, Main.DEFINE.SUBLINE_MAX, Main.DEFINE.FINDLINE_MAX];
        private CogFindLineTool[,,] PT_FindLineTools = new CogFindLineTool[Main.DEFINE.Pattern_Max, Main.DEFINE.SUBLINE_MAX, Main.DEFINE.FINDLINE_MAX];
        private CogFindLineTool PT_FindLineTool;
        private bool[] PT_FindLine_MarkUSE = new bool[Main.DEFINE.Pattern_Max];
        private CogIntersectLineLineTool[] PT_LineLineCrossPoints = new CogIntersectLineLineTool[3];
        //  private bool FINDLineRegionChange = false;
        // FPC Tray
        private List<CogCircle> LineEdge_CircleList = new List<CogCircle>();
        private double RectangleAngle;
        Main.DoublePoint[] TrayPocketPoint = new Main.DoublePoint[Main.DEFINE.TRAY_POCKET_LIMIT];
        List<CogPointMarker> MarkerPointList = new List<CogPointMarker>();

        private double[] PT_TRAY_GUIDE_DISX = new double[Main.DEFINE.Pattern_Max];
        private double[] PT_TRAY_GUIDE_DISY = new double[Main.DEFINE.Pattern_Max];
        private double[] PT_TRAY_PITCH_DISX = new double[Main.DEFINE.Pattern_Max];
        private double[] PT_TRAY_PITCH_DISY = new double[Main.DEFINE.Pattern_Max];
        //        private double[,] PT_TRAY_POINT_OFFSET = new double[Main.DEFINE.TRAY_POCKET_X, Main.DEFINE.TRAY_POCKET_Y];
        #region SD BIO

        //SD BIO 
        private enum enumROIType { Line = 0, Circle = 1 };
        private enum enumAlignROI { Left1_1 = 0, Left1_2 = 1, Right1_1 = 2, Right1_2 = 3 }
        private enum eBondingAlignPosition
        {
            X1 = 0,
            X2,
            Y1,
            Y2,
        }
        private enum enumROIFineAlignPosition { Left = 0, Right = 1 };

        private eBondingAlignPosition _bondingAlignPosition = eBondingAlignPosition.X1;
        private enumROIType m_enumROIType = new enumROIType();
        private enumAlignROI m_enumAlignROI = new enumAlignROI();
        private CogFindLineTool m_TempFindLineTool = new CogFindLineTool();
        private CogFindCircleTool m_TempFindCircleTool = new CogFindCircleTool();
        //private CogFindCornerTool m_TempFindCorner = new CogFindCornerTool();
        private CogFindLineTool m_TempTrackingLine = new CogFindLineTool();
        private CogFindLineTool[] m_TeachLine = new CogFindLineTool[4];
        private CogBlobTool[] m_CogBlobTool = new CogBlobTool[10];
        private CogHistogramTool[] m_CogHistogramTool = new CogHistogramTool[32];
        private CogCaliperTool m_TempCaliperTool = new CogCaliperTool();
        private CogCaliperTool[] m_TeachAlignLine = new CogCaliperTool[4];
        private CogCaliperTool m_TempTrackingCaliper = new CogCaliperTool();
        private CogSearchMaxTool[,] FinealignMark = new CogSearchMaxTool[Main.DEFINE.Pattern_Max, Main.DEFINE.SUBPATTERNMAX];
        private CogImage8Grey m_FixtureImage = new CogImage8Grey();
        private CogImage8Grey[] m_SectionImage;
        private CogImage8Grey OriginImage;
        private bool m_bROIFinealignFlag = true;
        private double m_dTempFinealignMarkscore;
        private double m_dROIFinealignT_Spec;
        private double m_dTempFineLineAngle;
        private bool m_bInspDirectionChange = false;


        private int m_iGridIndex;
        private double[] dCrossX = new double[2] { 0, 0 };
        private double[] dCrossY = new double[2] { 0, 0 };
        private double[] dAngle = new double[2] { 0, 0 };
        private double[] LeftOrigin = new double[2] { 0, 0 };
        private double[] RightOrigin = new double[2] { 0, 0 };
        public int m_iSection = 0;
        private int m_iCount;
        private double m_SpecDist = 0;
        private double m_SpecDistMax = 0;
        private int m_dDist_ignore = 0;
        private int m_DistIgnoreCnt = 0;
        private int m_BlobROI = 0;
        private int m_HistoROI = 0;
        private int m_PrevROINo = 0;
        private int m_iHistoramROICnt = 0;
        private int nROIFineAlignIndex = (int)enumROIFineAlignPosition.Left;
        private double dFinealignMarkScore = 0;
        private double dBlobPrevTranslationX = 0;
        private double dBlobPrevTranslationY = 0;
        private double dInspPrevTranslationX = 0;
        private double dInspPrevTranslationY = 0;
        private double dInspPrevTranslationT = 0;
        private bool[] m_bTrakingRoot = new bool[4] { false, false, false, false };
        private bool[] m_bTrakingRootHisto = new bool[32];
        private bool bROIFinealignTeach = false;
        private double PrevCenterX = 0;
        private double PrevCenterY = 0;
        private double PrevMarkX = 0;
        private double PrevMarkY = 0;
        private double dBondingAlignOriginDistX;
        private double dBondingAlignOriginDistY;
        private double dBondingAlignDistSpecX;
        private double dBondingAlignDistSpecY;
        private double dObjectDistanceX;
        private double dObjectDistanceSpecX;

        private bool _useROITracking = false;
        public bool UseROITracking
        {
            get { return _useROITracking; }
            set { _useROITracking = value; }
        }
        private CogPolygon _PrePolyGon = null;
        public CogPolygon mPrePolyGon
        {
            get { return _PrePolyGon; }
            set { _PrePolyGon = value; }
        }
        private double[] _PrePointX;
        public double[] mPrePointX
        {
            get { return _PrePointX; }
            set { _PrePointX = value; }
        }
        private double[] _PrePointY;
        public double[] mPrePointY
        {
            get { return _PrePointY; }
            set { _PrePointY = value; }
        }
        public class TrackingElement
        {
            public class LineTrackingElement
            {

                private CogFindLineTool _trackingLine = null;
                public CogFindLineTool TrackingLine
                {
                    get { return _trackingLine; }
                    set { _trackingLine = new CogFindLineTool(); }
                }

                private double _startX = 0.0;
                public double StartX
                {
                    get { return _startX; }
                    set { _startX = value; }
                }

                private double _startY = 0.0;
                public double StartY
                {
                    get { return _startY; }
                    set { _startY = value; }
                }

                private double _endX = 0.0;
                public double EndX
                {
                    get { return _endX; }
                    set { _endX = value; }
                }

                private double _endY = 0.0;
                public double EndY
                {
                    get { return _endY; }
                    set { _endY = value; }
                }
                private double _AngleT = 0.0;
                private double AngleT
                {
                    get { return _AngleT; }
                    set { _AngleT = value; }
                }
                private double _caliperSearchLength = 0.0;
                public double CaliperSearchLength
                {
                    get { return _caliperSearchLength; }
                    set { _caliperSearchLength = value; }
                }

                private double _caliperProjectionLength = 0.0;
                public double CaliperProjectionLength
                {
                    get { return _caliperProjectionLength; }
                    set { _caliperProjectionLength = value; }
                }

                private double _caliperSearchDirection = 0.0;
                public double CaliperSearchDirection
                {
                    get { return _caliperSearchDirection; }
                    set { _caliperSearchDirection = value; }
                }

                private int _numberOfCalipers = 0;
                public int NumberOfCalipers
                {
                    get { return _numberOfCalipers; }
                    set { _numberOfCalipers = value; }
                }

                public LineTrackingElement Copy()
                {
                    LineTrackingElement element = new LineTrackingElement();

                    element.TrackingLine = this.TrackingLine;
                    element.TrackingLine.RunParams.ExpectedLineSegment.StartX = this.StartX;
                    element.TrackingLine.RunParams.ExpectedLineSegment.StartY = this.StartY;
                    element.TrackingLine.RunParams.ExpectedLineSegment.EndX = this.EndX;
                    element.TrackingLine.RunParams.ExpectedLineSegment.EndY = this.EndY;

                    element.TrackingLine.RunParams.CaliperSearchLength = this.CaliperSearchLength;
                    element.TrackingLine.RunParams.CaliperProjectionLength = this.CaliperProjectionLength;
                    element.TrackingLine.RunParams.CaliperSearchDirection = this.CaliperSearchDirection;
                    element.TrackingLine.RunParams.NumCalipers = this.NumberOfCalipers;
                    return element;
                }
                public LineTrackingElement ShallowCopy()
                {
                    return (LineTrackingElement)this.MemberwiseClone();
                }

            }

            public class CircleTrackingElement
            {
                public CogFindCircleTool _trackingCircle = null;
                public CogFindCircleTool TrackingCircle
                {
                    get { return _trackingCircle; }
                    set { _trackingCircle = new CogFindCircleTool(); }
                }
                private double _RotationT = 0.0;
                public double RotationT
                {
                    get { return _RotationT; }
                    set { _RotationT = value; }
                }
                private double _centerX = 0.0;
                public double CenterX
                {
                    get { return _centerX; }
                    set { _centerX = value; }
                }

                private double _centerY = 0.0;
                public double CenterY
                {
                    get { return _centerY; }
                    set { _centerY = value; }
                }

                private double _startX = 0.0;
                public double StartX
                {
                    get { return _startX; }
                    set { _startX = value; }
                }

                private double _startY = 0.0;
                public double StartY
                {
                    get { return _startY; }
                    set { _startY = value; }
                }

                private double _endX = 0.0;
                public double EndX
                {
                    get { return _endX; }
                    set { _endX = value; }
                }

                private double _endY = 0.0;
                public double EndY
                {
                    get { return _endY; }
                    set { _endY = value; }
                }

                private double _caliperSearchLength = 0.0;
                public double CaliperSearchLength
                {
                    get { return _caliperSearchLength; }
                    set { _caliperSearchLength = value; }
                }

                private double _caliperProjectionLength = 0.0;
                public double CaliperProjectionLength
                {
                    get { return _caliperProjectionLength; }
                    set { _caliperProjectionLength = value; }
                }

                private double _caliperSearchDirection = 0.0;
                public double CaliperSearchDirection
                {
                    get { return _caliperSearchDirection; }
                    set { _caliperSearchDirection = value; }
                }

                private int _numberOfCalipers = 0;
                public int NumberOfCalipers
                {
                    get { return _numberOfCalipers; }
                    set { _numberOfCalipers = value; }
                }

                private double _radiusConstraint = 0.0;
                public double RadiusConstraint
                {
                    get { return _radiusConstraint; }
                    set { _radiusConstraint = value; }
                }

                private double _angleSpan = 0.0;
                public double AngleSpan
                {
                    get { return _angleSpan; }
                    set { _angleSpan = value; }
                }

                private double _angleStart = 0.0;
                public double AngleStart
                {
                    get { return _angleStart; }
                    set { _angleStart = value; }
                }

                private double _arcLength = 0.0;
                public double ArcLength
                {
                    get { return _arcLength; }
                    set { _arcLength = value; }
                }

                private double _radius = 0.0;
                public double Radius
                {
                    get { return _radius; }
                    set { _radius = value; }
                }

                public CircleTrackingElement Copy()
                {
                    CircleTrackingElement element = new CircleTrackingElement();

                    element.TrackingCircle = this.TrackingCircle;
                    element.TrackingCircle.RunParams.ExpectedCircularArc.CenterX = this.CenterX;
                    element.TrackingCircle.RunParams.ExpectedCircularArc.CenterY = this.CenterY;
                    element.TrackingCircle.RunParams.CaliperSearchLength = this.CaliperSearchLength;
                    element.TrackingCircle.RunParams.CaliperProjectionLength = this.CaliperProjectionLength;
                    element.TrackingCircle.RunParams.NumCalipers = this.NumberOfCalipers;
                    element.TrackingCircle.RunParams.RadiusConstraint = this.RadiusConstraint;
                    element.TrackingCircle.RunParams.ExpectedCircularArc.AngleSpan = this.AngleSpan;
                    element.TrackingCircle.RunParams.ExpectedCircularArc.AngleStart = this.AngleStart;
                    element.TrackingCircle.RunParams.ExpectedCircularArc.Radius = this.Radius;

                    return element;
                }
                public CircleTrackingElement ShallowCopy()
                {
                    return (CircleTrackingElement)this.MemberwiseClone();
                }
            }

            private double _translationX;
            public double TranslationX
            {
                get { return _translationX; }
                set { _translationX = value; }
            }

            private double _translationY;
            public double TranslationY
            {
                get { return _translationY; }
                set { _translationY = value; }
            }
            private double _translationT;
            public double TranslationT
            {
                get { return _translationT; }
                set { _translationT = value; }
            }
            public TrackingElement Copy()
            {
                TrackingElement element = new TrackingElement();

                element.TranslationX = this.TranslationX;
                element.TranslationY = this.TranslationY;
                element.TranslationT = this.TranslationT;
                return element;
            }
        }

        TrackingElement Tracking = new TrackingElement();
        TrackingElement.LineTrackingElement TrackingLine = new TrackingElement.LineTrackingElement();
        TrackingElement.CircleTrackingElement TrackingCircle = new TrackingElement.CircleTrackingElement();

        List<double> List_CenterX = new List<double>();
        List<double> List_CenterY = new List<double>();
        List<double> List_LenthX = new List<double>();
        List<double> List_LenthY = new List<double>();
        bool GraphicIndex = true;
        private List<Main.PatternTag.SDParameter> m_TeachParameter = new List<Main.PatternTag.SDParameter>();
        #endregion
        //==================== LINEMAX ====================//
        private CogLineMaxTool[,,] PT_LineMaxTools = new CogLineMaxTool[Main.DEFINE.Pattern_Max, Main.DEFINE.SUBLINE_MAX, Main.DEFINE.FINDLINE_MAX];
        private CogLineMaxTool PT_LineMaxTool;

        //==================================================//
        private string[] PT_FINDLineName = new string[Main.DEFINE.FINDLINE_MAX];
        private object m_lock = new object();
        private object m_inspLock = new object();
        #endregion

        #region CIRCLE
        private static Main.FindCircleTagData[,] PT_CirclePara = new Main.FindCircleTagData[Main.DEFINE.Pattern_Max, Main.DEFINE.CIRCLE_MAX];
        private CogFindCircleTool[,] PT_CircleTools = new CogFindCircleTool[Main.DEFINE.Pattern_Max, Main.DEFINE.CIRCLE_MAX];
        private CogFindCircleTool PT_CircleTool;

        private bool[] PT_Circle_MarkUSE = new bool[Main.DEFINE.Pattern_Max];
        private const int M_PLUS = 1;
        private const int M_MINUS = -1;
        private const int M_SEARCHLEGNTH = 0;
        private const int M_PROJECTION = 1;
        private const int M_RADIUS = 2;
        #endregion

        private List<CogRecordDisplay> PT_SubDisplay = new List<CogRecordDisplay>();
        private List<Label> LB_PATTERN = new List<Label>();

        CogPointMarker PatORGPoint = new CogPointMarker();

        CogPointMarker FirstPocketPos = new CogPointMarker();
        CogPointMarker X_PocketPitchPos = new CogPointMarker();
        CogPointMarker Y_PocketPitchPos = new CogPointMarker();

        private int TRAY_POCKET_X = 1;
        private int TRAY_POCKET_Y = 1;

        private Form_Message formMessage = new Form_Message(2);

        private CogRecordDisplay PT_Display01 = new CogRecordDisplay();
        private object mlock;

        //parameter temp 
        List<int> tempCaliperNum = new List<int>();
        public int iCountClick = 0;
        //Image Prev,Next
        private int CurrentImageNumber = -1;
        private string CurrentFolderPath = "";

        public Form_PatternTeach()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            Allocate_Array();
            PT_Display01.MouseUp += new MouseEventHandler(Display_MauseUP);

            m_TeachParameter = new List<Main.PatternTag.SDParameter>();
            m_TeachParameter.Add(ResetStruct());
            mlock = new object();
            //for (int i = 0; i < 10; i++)
            //    m_CogBlobTool[i] = new CogBlobTool();
        }
        private void Allocate_Array()
        {
            PT_Display01 = PT_DISPLAY_CONTROL.CogDisplay00;
            PT_Display01.Changed += PT_Display01_Changed;

            PT_DisplayToolbar01.Display = PT_Display01;
            PT_DisplayStatusBar01.Display = PT_Display01;
            //---------------LIGHT_SETTING----------------
            Light_Text.Add(LB_LIGHT_0);
            //Light_Text.Add(LB_LIGHT_1);
            //Light_Text.Add(LB_LIGHT_2);
            //Light_Text.Add(LB_LIGHT_3);

            LightRadio.Add(RBTN_LIGHT_0);
            //LightRadio.Add(RBTN_LIGHT_1);
            //LightRadio.Add(RBTN_LIGHT_2);
            //LightRadio.Add(RBTN_LIGHT_3);

            RBTN_PAT.Add(RBTN_PAT_0);
            RBTN_PAT.Add(RBTN_PAT_1);
            RBTN_PAT.Add(RBTN_PAT_2);
            RBTN_PAT.Add(RBTN_PAT_3);
            RBTN_PAT.Add(RBTN_PAT_4);
            RBTN_PAT.Add(RBTN_PAT_5);
            RBTN_PAT.Add(RBTN_PAT_6);
            RBTN_PAT.Add(RBTN_PAT_7);

            BTN_TOOLSET.Add(BTN_TOOL_00);

            BTN_TOOLSET.Add(BTN_TOOL_03);
            BTN_TOOLSET.Add(BTN_TOOL_04);

            RBTN_CALIPER.Add(RBTN_CALIPER00);
            RBTN_CALIPER.Add(RBTN_CALIPER01);
            RBTN_CALIPER.Add(RBTN_CALIPER02);
            RBTN_CALIPER.Add(RBTN_CALIPER03);

            RBTN_FINDLINE.Add(RBTN_FINDLINE00);
            RBTN_FINDLINE.Add(RBTN_FINDLINE01);
            RBTN_FINDLINE.Add(RBTN_FINDLINE02);
            RBTN_FINDLINE.Add(RBTN_FINDLINE03);
            RBTN_FINDLINE.Add(RBTN_FINDLINE_CIRCLE);

            RBTN_LINEMAX_H_COND.Add(RBTN_HORICON_YMIN);
            RBTN_LINEMAX_H_COND.Add(RBTN_HORICON_YMAX);
            RBTN_LINEMAX_V_COND.Add(RBTN_VERTICON_XMIN);
            RBTN_LINEMAX_V_COND.Add(RBTN_VERTICON_XMAX);

            RBTN_CALIPER_METHOD.Add(RBTN_CALIPER_METHOD_SCORE);
            RBTN_CALIPER_METHOD.Add(RBTN_CALIPER_METHOD_POS);

            RBTN_CIR_CALIPER_METHOD.Add(RBTN_CIR_CALIPER_METHOD_SCORE);
            RBTN_CIR_CALIPER_METHOD.Add(RBTN_CIR_CALIPER_METHOD_POS);

            RBTN_CIRCLE.Add(RBTN_CIRCLE00);
            RBTN_CIRCLE.Add(RBTN_CIRCLE01);
            RBTN_CIRCLE.Add(RBTN_CIRCLE_LINE00);
            RBTN_CIRCLE.Add(RBTN_CIRCLE_LINE01);
            RBTN_CIRCLE.Add(RBTN_CIRCLE_LINE02);

            TOOLTYPE.Add("CogPMAlignTool1");
            TOOLTYPE.Add("CogFindLineTool1");
            TOOLTYPE.Add("CogFindLineTool2");

            TOOLTYPE.Add("CogFindLineTool3");
            TOOLTYPE.Add("CogFindLineTool4");
            TOOLTYPE.Add("CogCNLSearchTool1");

            for (int i = 0; i < Main.DEFINE.SUBPATTERNMAX; i++)
            {
                string ntempSub;
                if (i == 0)
                    ntempSub = "MAIN_PAT" + i.ToString();
                else
                    ntempSub = "SUB__PAT" + i.ToString();
                CB_SUB_PATTERN.Items.Add(ntempSub);

                string nTempName;
                nTempName = "PT_SubDisplay_" + i.ToString("00");
                CogRecordDisplay nType = (CogRecordDisplay)this.Controls["TABC_MANU"].Controls["TAB_00"].Controls[nTempName];
                PT_SubDisplay.Add(nType);
                PT_SubDisplay[i].Visible = true;


                nTempName = "LB_PATTERN_" + i.ToString("00");
                Label nType1 = (Label)this.Controls["TABC_MANU"].Controls["TAB_00"].Controls[nTempName];
                LB_PATTERN.Add(nType1);
                LB_PATTERN[i].Visible = true;
            }

            for (int i = 0; i < Main.DEFINE.Pattern_Max; i++)
            {
                for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
                {
                    PT_Pattern[i, j] = new CogSearchMaxTool();
                    PT_GPattern[i, j] = new CogPMAlignTool();
                    PT_Pattern[i, j].Pattern.TrainRegion = new CogRectangle();
                    PT_GPattern[i, j].Pattern.TrainRegion = new CogRectangle();
                    FinealignMark[i, j] = new CogSearchMaxTool();
                    FinealignMark[i, j].Pattern.TrainRegion = new CogRectangle();
                }
                for (int j = 0; j < Main.DEFINE.CALIPER_MAX; j++)
                {
                    PT_CaliperTools[i, j] = new CogCaliperTool();
                    PT_CaliPara[i, j] = new Main.CaliperTagData();
                }
                for (int j = 0; j < Main.DEFINE.BLOB_CNT_MAX; j++)
                {
                    PT_BlobTools[i, j] = new CogBlobTool();
                    PT_BlobPara[i, j] = new Main.BlobTagData();
                }
                for (int k = 0; k < Main.DEFINE.SUBLINE_MAX; k++)
                {
                    for (int j = 0; j < Main.DEFINE.FINDLINE_MAX; j++)
                    {
                        PT_FindLinePara[i, k, j] = new Main.FindLineTagData();
                        PT_FindLineTools[i, k, j] = new CogFindLineTool();
                        PT_LineMaxTools[i, k, j] = new CogLineMaxTool();
                    }
                }
                for (int j = 0; j < Main.DEFINE.TRAY_POCKET_LIMIT; j++)
                {
                    TrayPocketPoint[i] = new Main.DoublePoint();
                }
                for (int j = 0; j < Main.DEFINE.CIRCLE_MAX; j++)
                {
                    PT_CircleTools[i, j] = new CogFindCircleTool();
                    PT_CirclePara[i, j] = new Main.FindCircleTagData();
                }
            }

            for (int i = 0; i < Main.DEFINE.BLOB_CNT_MAX; i++)
            {
                string ntempBlob;
                ntempBlob = "BLOB_REGION" + i.ToString();
                CB_BLOB_COUNT.Items.Add(ntempBlob);
            }
            for (int i = 0; i < Main.DEFINE.CALIPER_MAX; i++)
            {
                RBTN_CALIPER[i].Visible = true;
            }
            for (int i = 0; i < Main.DEFINE.FINDLINE_MAX; i++)
            {
                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2" && i >= 3)
                    break;

                RBTN_FINDLINE[i].Visible = true;
            }
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
                RBTN_FINDLINE[4].Visible = true;    // 200624 JHKIM 원호 추가
            else
                RBTN_FINDLINE[4].Visible = false;

            for (int i = 0; i < Main.DEFINE.SUBLINE_MAX; i++)
            {
                string ntempSub;
                if (i == 0)
                    ntempSub = "MAIN_LINE" + i.ToString();
                else
                    ntempSub = "SUB__LINE" + i.ToString();
                CB_FINDLINE_SUBLINE.Items.Add(ntempSub);
            }

            for (int i = 0; i < Main.DEFINE.CIRCLE_MAX; i++)
            {
                RBTN_CIRCLE[i].Visible = true;
            }
            RBTN_CIRCLE[2].Visible = true;    // 200624 JHKIM 원호 추가
            RBTN_CIRCLE[3].Visible = true;
            RBTN_CIRCLE[4].Visible = true;

            for (int i = 0; i < 2; i++)
            {
                RBTN_LINEMAX_H_COND[i].Visible = false;
                RBTN_LINEMAX_V_COND[i].Visible = false;
            }

            MarkORGPoint.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
            MarkORGPoint.Interactive = true;
            MarkORGPoint.LineStyle = CogGraphicLineStyleConstants.Dot;
            MarkORGPoint.SelectedColor = CogColorConstants.Cyan;
            MarkORGPoint.DragColor = CogColorConstants.Cyan;


            FirstPocketPos.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
            FirstPocketPos.Interactive = true;
            FirstPocketPos.LineStyle = CogGraphicLineStyleConstants.Dot;
            FirstPocketPos.Color = CogColorConstants.Orange;
            FirstPocketPos.SelectedColor = CogColorConstants.Orange;
            FirstPocketPos.DragColor = CogColorConstants.Orange;
            FirstPocketPos.SizeInScreenPixels = M_ORIGIN_SIZE;
            FirstPocketPos.LineWidthInScreenPixels = 3;
            FirstPocketPos.SelectedLineWidthInScreenPixels = 3;
            FirstPocketPos.TipText = "First Pos";

            X_PocketPitchPos.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
            X_PocketPitchPos.Interactive = true;
            X_PocketPitchPos.LineStyle = CogGraphicLineStyleConstants.Dot;
            X_PocketPitchPos.GraphicType = CogPointMarkerGraphicTypeConstants.InwardArrow;
            X_PocketPitchPos.Color = CogColorConstants.Blue;
            X_PocketPitchPos.SelectedColor = CogColorConstants.Blue;
            X_PocketPitchPos.DragColor = CogColorConstants.Blue;
            X_PocketPitchPos.SizeInScreenPixels = M_ORIGIN_SIZE - 60;
            X_PocketPitchPos.LineWidthInScreenPixels = 2;
            X_PocketPitchPos.SelectedLineWidthInScreenPixels = 2;
            X_PocketPitchPos.TipText = "X PITCH";

            Y_PocketPitchPos.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
            Y_PocketPitchPos.Interactive = true;
            Y_PocketPitchPos.LineStyle = CogGraphicLineStyleConstants.Dot;
            Y_PocketPitchPos.GraphicType = CogPointMarkerGraphicTypeConstants.InwardArrow;
            Y_PocketPitchPos.Color = CogColorConstants.Red;
            Y_PocketPitchPos.SelectedColor = CogColorConstants.Red;
            Y_PocketPitchPos.DragColor = CogColorConstants.Red;
            Y_PocketPitchPos.SizeInScreenPixels = M_ORIGIN_SIZE - 60;
            Y_PocketPitchPos.LineWidthInScreenPixels = 2;
            Y_PocketPitchPos.SelectedLineWidthInScreenPixels = 2;
            Y_PocketPitchPos.TipText = "Y PITCH";

            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                PT_CaliperName[0] = "CALIPER TOP";
                PT_CaliperName[1] = "CALIPER RIGHT";
                PT_CaliperName[2] = "CALIPER BOTTOM";
                PT_CaliperName[3] = "CALIPER LEFT";

                PT_FINDLineName[0] = "HORIZONTAL LINE";
                PT_FINDLineName[1] = "VERTICAL LINE";
                PT_FINDLineName[2] = "DIAGONAL LINE";
                PT_FINDLineName[3] = "POL EDGE LINE";
            }
            else if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                PT_FINDLineName[0] = "HORIZONTAL LINE";
                PT_FINDLineName[1] = "VERTICAL1 LINE";
                PT_FINDLineName[2] = "VERTICAL2 LINE";
                RBTN_FINDLINE[3].Visible = false;
            }
        }
        private void Form_PatternTeach_Load(object sender, EventArgs e)
        {
            _isFormLoad = false;

            if (Main.DEFINE.OPEN_F)
                BTN_IMAGE_OPEN.Visible = true;

            BTN_LIVEMODE.Checked = false;
            BTN_LIVEMODE.BackColor = Color.DarkGray;
            this.Text = Main.AlignUnit[m_AlignNo].m_AlignName;
            this.TopMost = false;
            m_PatNo = 0;
            m_RetiMode = 0;
            m_CamNo = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo;

            RBTN_FINDLINE00.Checked = true;
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2" && m_CamNo == Main.DEFINE.CAM_SELECT_1ST_ALIGN))
                TABC_MANU.SelectedIndex = M_TOOL_MODE = Main.DEFINE.M_FINDLINETOOL;
            else
                TABC_MANU.SelectedIndex = M_TOOL_MODE = Main.DEFINE.M_CNLSEARCHTOOL;

            m_PatNo_Sub = 0;
            m_LineSubNo = 0;
            m_SelectBlob = 0;
            m_SelectCaliper = 0;
            m_SelectFindLine = 0;
            m_SelectCircle = 0;
            button1.BringToFront();
            CB_SUB_PATTERN.SelectedIndex = 0;
            CB_BLOB_COUNT.SelectedIndex = 0;
            CB_FINDLINE_SUBLINE.SelectedIndex = 0;
            LightRadio[0].Checked = true;

            GB_TRAY.Visible = false;
            BTN_MAINORIGIN_COPY.Visible = false;
            BTN_PATTERN_COPY.Visible = true;
            BTN_RETURNPAGE.Visible = false;
            bROICopy = false;
            CHK_ROI_CREATE.Checked = bROICopy;
            if (Main.AlignUnit[m_AlignNo].m_AlignName == "PBD" || Main.AlignUnit[m_AlignNo].m_AlignName == "PBD_STAGE" || Main.AlignUnit[m_AlignNo].m_AlignName == "PBD_FOF")
            {
                BTN_PATTERN_COPY.Visible = true;
            }

            if (Main.ALIGNINSPECTION_USE(m_AlignNo))
            {
                CB_BLOB_COUNT.Items[Main.DEFINE.WIDTH_] = "WIDTH__BLOB_REGION";
                CB_BLOB_COUNT.Items[Main.DEFINE.HEIGHT] = "HEIGHT_BLOB_REGION";

                RBTN_CALIPER00.Text = "WIDTH  CALIPER";
                RBTN_CALIPER01.Text = "HEIGHT CALIPER";

                label15.Visible = true;
                CB_EDGEPAIRCHECK.Visible = true;
                #region
                for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
                {
                    for (int j = 0; j < Main.DEFINE.CALIPER_MAX; j++)
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_UseCheck = true;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j].RunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                    }
                }
                for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
                {
                    for (int j = 0; j < Main.DEFINE.BLOB_CNT_MAX; j++)
                    {
                        if (CB_BLOB_USE.Checked)
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_UseCheck = true;
                        }
                        else
                        {
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_UseCheck = false;
                        }

                    }
                }
                #endregion
            }
            else if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                label15.Visible = true;
                CB_EDGEPAIRCHECK.Visible = true;

                RBTN_CALIPER00.Text = "TOP";
                RBTN_CALIPER01.Text = "RIGHT";
                RBTN_CALIPER02.Text = "BOTTOM";
                RBTN_CALIPER03.Text = "LEFT";
            }
            else
            {
                label15.Visible = false;
                CB_EDGEPAIRCHECK.Visible = false;

                RBTN_CALIPER00.Text = "CALIPER 00";
                RBTN_CALIPER01.Text = "CALIPER 01";
            }



            if (Main.vision.CogCamBuf[m_CamNo].Width > 2000)
            {
                M_ORIGIN_SIZE = M_ORIGIN_SIZE_S * 2;
            }
            else
            {
                M_ORIGIN_SIZE = M_ORIGIN_SIZE_S;
            }

            CB_TRAY_BlobMode.Checked = Main.AlignUnit[m_AlignNo].TrayBlobMode;

            #region
            //for (int i = 0; i < BTN_TOOLSET.Count; i++)
            //{
            //    BTN_TOOLSET[i].Visible = false;
            //}

            //for (int i = 0; i < Main.DEFINE.Pattern_Max; i++)
            //{
            //    RBTN_PAT[i].Visible = false;
            //}
            //BTN_TOOLSET[3].Visible = true;
            //BTN_TOOLSET[4].Visible = true;
            //BTN_TOOLSET[2].Visible = true;
            //BTN_TOOLSET[Main.DEFINE.M_CNLSEARCHTOOL].Visible = true;

            Main.AlignUnit[m_AlignNo].m_PatTagNo = m_PatTagNo;
            if (Main.BLOBINSPECTION_USE(m_AlignNo))
            {
                CB_BLOB_MARK_USE.Visible = false;
                Inspect_Cnt.Visible = true;
                LB_BLOBINSPECT.Visible = true; LB_BLOB_INSPECT1.Visible = true; LB_BLOB_INSPECT2.Visible = true; LB_BLOB_INSPECT3.Visible = true;
            }
            else
            {
                Inspect_Cnt.Visible = false;
                LB_BLOBINSPECT.Visible = false; LB_BLOB_INSPECT1.Visible = false; LB_BLOB_INSPECT2.Visible = false; LB_BLOB_INSPECT3.Visible = false;
            }

            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                RBTN_PAT[i].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_PatternName;
                RBTN_PAT[i].Visible = true;

                if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2" && m_AlignNo == 0 && i == 1)   // 1CAM 1SHOT
                    RBTN_PAT[i].Visible = false;


                PT_AcceptScore[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_ACCeptScore;
                PT_GAcceptScore[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_GACCeptScore;

                PT_Caliper_MarkUSE[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Caliper_MarkUse;

                PT_Blob_MarkUSE[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Blob_MarkUse;
                PT_Blob_CaliperUSE[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Blob_CaliperUse;
                PT_Blob_InspCnt[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_Blob_InspCnt;

                PT_FindLine_MarkUSE[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLine_MarkUse;
                PT_Circle_MarkUSE[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Circle_MarkUse;

                PT_TRAY_GUIDE_DISX[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_GUIDE_DISX;
                PT_TRAY_GUIDE_DISY[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_GUIDE_DISY;
                PT_TRAY_PITCH_DISX[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_PITCH_DISX;
                PT_TRAY_PITCH_DISY[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_PITCH_DISY;
            }

            if (Main.DEFINE.PROGRAM_TYPE == "ATT_AREA_PC1")
                RBTN_PAT[1].Visible = false;

            if (Main.AlignUnit[m_AlignNo].m_AlignName == "REEL_ALIGN_1" || Main.AlignUnit[m_AlignNo].m_AlignName == "REEL_ALIGN_2" || Main.AlignUnit[m_AlignNo].m_AlignName == "REEL_ALIGN_3" || Main.AlignUnit[m_AlignNo].m_AlignName == "REEL_ALIGN_4" || Main.AlignUnit[m_AlignNo].m_AlignName == "ART_PROBE")
            {
                RBTN_PAT[1].Visible = false;
            }

            if (Main.AlignUnit[m_AlignNo].m_AlignName == "FPC_TRAY1" || Main.AlignUnit[m_AlignNo].m_AlignName == "FPC_TRAY2")
            {
                RBTN_PAT[1].Visible = false;
                GB_TRAY.Visible = true;
            }
            else
            {
                PB_FOF_FPC.Visible = false;
                PB_TFOF_PANEL.Visible = false;
            }

            if ((Main.DEFINE.PROGRAM_TYPE == "FOF_PC7" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC8") & (Main.AlignUnit[m_AlignNo].m_AlignName == "ACF_BLOB1_1" || Main.AlignUnit[m_AlignNo].m_AlignName == "ACF_BLOB1_2" || Main.AlignUnit[m_AlignNo].m_AlignName == "ACF_BLOB2_1" || Main.AlignUnit[m_AlignNo].m_AlignName == "ACF_BLOB2_2"))
            {
                RBTN_PAT[0].Text = "ACF_BLOB_L";
                RBTN_PAT[1].Text = "ACF_BLOB_R";
                RBTN_PAT[2].Text = "BACK_UP_INSPECTION_L";
                RBTN_PAT[3].Text = "BACK_UP_INSPECTION_R";
            }

            if (Main.AlignUnit[m_AlignNo].m_AlignName == "BACKUP_INSPECTION1" || Main.AlignUnit[m_AlignNo].m_AlignName == "BACKUP_INSPECTION2")
            {
                RBTN_PAT[0].Text = "BACKUP_INSPECTION1_L";
                RBTN_PAT[1].Text = "BACKUP_INSPECTION1_R";
                RBTN_PAT[2].Text = "BACKUP_INSPECTION2_L";
                RBTN_PAT[3].Text = "BACKUP_INSPECTION2_R";
            }


            for (int i = 0; i < Main.DEFINE.Pattern_Max; i++)
            {
                for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
                {
                    //                     PT_Pattern[i, j].Dispose();
                    //                     PT_Pattern[i, j] = new CogSearchMaxTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern[j]);
                    PT_Pattern[i, j] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern[j];
                    PT_Pattern_USE[i, j] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern_USE[j];

                    //                     PT_GPattern[i, j].Dispose();
                    //                     PT_GPattern[i, j] = new CogPMAlignTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].GPattern[j]);
                    PT_GPattern[i, j] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].GPattern[j];


                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMark[i, j] != null)
                        FinealignMark[i, j] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMark[i, j];
                }
            }
            #endregion

            #region BLOB Parameter 수정한거
            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                for (int j = 0; j < Main.DEFINE.BLOB_CNT_MAX; j++)
                {
                    PT_BlobTools[i, j].Dispose();
                    PT_BlobTools[i, j] = new CogBlobTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobTools[j]);
                    PT_BlobPara[i, j].m_UseCheck = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_UseCheck;

                    if (Main.ALIGNINSPECTION_USE(m_AlignNo))
                    {
                        if (j < 2)
                            PT_BlobPara[i, j].m_UseCheck = true;
                        else
                            PT_BlobPara[i, j].m_UseCheck = false;
                    }

                    for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
                    {
                        PT_BlobPara[i, j].m_TargetToCenter[k] = new Main.DoublePoint();
                        PT_BlobPara[i, j].m_TargetToCenter[k].X = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_TargetToCenter[k].X;
                        PT_BlobPara[i, j].m_TargetToCenter[k].Y = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_TargetToCenter[k].Y;
                    }
                }
            }
            #endregion

            #region CALIPER PARAMETER
            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                for (int j = 0; j < Main.DEFINE.CALIPER_MAX; j++)
                {
                    PT_CaliperTools[i, j].Dispose();
                    PT_CaliperTools[i, j] = new CogCaliperTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j]);
                    PT_CaliperTools[i, j].RunParams.EdgeMode = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j].RunParams.EdgeMode;
                    PT_CaliPara[i, j].m_UseCheck = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_UseCheck;
                    // 210203 ATT
                    PT_CaliPara[i, j].m_bCOPMode = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_bCOPMode;
                    PT_CaliPara[i, j].m_nCOPROICnt = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_nCOPROICnt;
                    PT_CaliPara[i, j].m_nCOPROIOffset = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_nCOPROIOffset;

                    for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
                    {
                        PT_CaliPara[i, j].m_TargetToCenter[k] = new Main.DoublePoint();
                        PT_CaliPara[i, j].m_TargetToCenter[k].X = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_TargetToCenter[k].X;
                        PT_CaliPara[i, j].m_TargetToCenter[k].Y = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_TargetToCenter[k].Y;
                    }

                    if (Main.AlignUnit[m_AlignNo].m_AlignName == "CRD_PRE1" || Main.AlignUnit[m_AlignNo].m_AlignName == "CRD_PRE2" || Main.AlignUnit[m_AlignNo].m_AlignName == "CRD_PRE3" || Main.AlignUnit[m_AlignNo].m_AlignName == "CRD_PRE4")
                    {
                        if (m_PatTagNo > 0) //1번 태크 부터 검사. 0번은 얼라인이여서 . 
                        {
                            PT_CaliperTools[i, j].RunParams.SingleEdgeScorers.Clear();
                            CogCaliperScorerPositionNeg nItem = new CogCaliperScorerPositionNeg();
                            PT_CaliperTools[i, j].RunParams.SingleEdgeScorers.Add(nItem);
                        }
                    }
                }
            }


            #endregion

            #region FINDLine PARAMETER
            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                for (int ii = 0; ii < Main.DEFINE.SUBLINE_MAX; ii++)
                {
                    for (int j = 0; j < Main.DEFINE.FINDLINE_MAX; j++)
                    {
                        PT_FindLineTools[i, ii, j].Dispose();
                        PT_FindLineTools[i, ii, j] = new CogFindLineTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j]);
                        PT_LineMaxTools[i, ii, j] = new CogLineMaxTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j]);
                        PT_FindLinePara[i, ii, j].m_UseCheck = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_UseCheck;
                        PT_FindLinePara[i, ii, j].m_UsePairCheck = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_UsePairCheck;
                        PT_FindLinePara[i, ii, j].m_LinePosition = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_LinePosition;
                        PT_FindLinePara[i, ii, j].m_LineCaliperMethod = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_LineCaliperMethod;

                        for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
                        {
                            PT_FindLinePara[i, ii, j].m_TargetToCenter[k] = new Main.DoublePoint();
                            PT_FindLinePara[i, ii, j].m_TargetToCenter[k].X = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].X;
                            PT_FindLinePara[i, ii, j].m_TargetToCenter[k].Y = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].Y;
                            PT_FindLinePara[i, ii, j].m_TargetToCenter[k].X2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].X2;
                            PT_FindLinePara[i, ii, j].m_TargetToCenter[k].Y2 = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].Y2;
                        }
                    }
                }
            }

            TRAY_POCKET_X = Main.AlignUnit[m_AlignNo].m_Tray_Pocket_X;
            TRAY_POCKET_Y = Main.AlignUnit[m_AlignNo].m_Tray_Pocket_Y;
            #endregion

            #region CIRCLE PARAMETER
            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                for (int j = 0; j < Main.DEFINE.CIRCLE_MAX; j++)
                {
                    PT_CircleTools[i, j].Dispose();
                    PT_CircleTools[i, j] = new CogFindCircleTool(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j]);
                    PT_CirclePara[i, j].m_UseCheck = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_UseCheck;
                    PT_CirclePara[i, j].m_CircleCaliperMethod = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_CircleCaliperMethod;


                    for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
                    {
                        PT_CirclePara[i, j].m_TargetToCenter[k] = new Main.DoublePoint();
                        PT_CirclePara[i, j].m_TargetToCenter[k].X = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_TargetToCenter[k].X;
                        PT_CirclePara[i, j].m_TargetToCenter[k].Y = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_TargetToCenter[k].Y;
                    }
                }
            }
            #endregion

            //Bonding Align
            for (int i = 0; i < 4; i++)
            {
                m_TeachAlignLine[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i];
                if (m_TeachAlignLine[i] == null)
                    m_TeachAlignLine[i] = new CogCaliperTool();
                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i] == null)
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i] = new CogCaliperTool();
            }

            m_bROIFinealignFlag = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_bFInealignFlag;
            CMB_USE_ROIFINEALIGN.Checked = m_bROIFinealignFlag;
            //2023 0228 YSH
            //ROI Finealign 기능사용시엔 Bonding얼라인 미사용
            //ROI Finealign 기능미사용시엔 Bonding얼라인 사용
            if (m_bROIFinealignFlag)
                RDB_BONDING_ALIGN.Visible = false;
            else
                RDB_BONDING_ALIGN.Visible = true;
            m_dROIFinealignT_Spec = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_FinealignThetaSpec;
            LBL_ROI_FINEALIGN_SPEC_T.Text = m_dROIFinealignT_Spec.ToString();

            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_FinealignMarkScore == 0)
                dFinealignMarkScore = 0.5;
            else
                dFinealignMarkScore = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_FinealignMarkScore;

            m_bInspDirectionChange = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_bInspDirectionChangeFlag;

            if (m_bInspDirectionChange)
                chkUseInspDirectionChange.Checked = true;
            else
                chkUseInspDirectionChange.Checked = false;

            Pattern_Change();

            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                //LB_TOOLBLOCKEDIT_HIDE.Location = new System.Drawing.Point(905, 1);
                //LB_TOOLBLOCKEDIT_HIDE.Size = new System.Drawing.Size(610, 65);
                //LB_TOOLBLOCKEDIT_HIDE.Visible = true;
                LB_MARK_COMMENT.Text = "TEACH FOR CALIBRATION (NORMAL NOT USE)";
                LB_MARK_COMMENT.Visible = true;

                LB_CALIPER_COMMENT.Text = "TEACH FOR BEAM SIZE CHECK (NORMAL NOT USE)";
                LB_CALIPER_COMMENT.Visible = true;

                LB_FINDLINE_MARK_USE_HIDE.Location = new System.Drawing.Point(128, 128);
                LB_FINDLINE_MARK_USE_HIDE.Size = new System.Drawing.Size(123, 52);
                LB_FINDLINE_MARK_USE_HIDE.Visible = true;

                LB_FINDCIRCLE_MARK_USE_HIDE.Location = new System.Drawing.Point(128, 128);
                LB_FINDCIRCLE_MARK_USE_HIDE.Size = new System.Drawing.Size(123, 52);
                LB_FINDCIRCLE_MARK_USE_HIDE.Visible = true;

                TABC_MANU.TabPages[3].Text = "C-CUT_TEACH";
                TABC_MANU.TabPages[4].Text = "R-CUT_TEACH";

                FINDLINE_Change();
            }
            else if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
            {
                LB_FINDLINE_MARK_USE_HIDE.Location = new System.Drawing.Point(128, 128);
                LB_FINDLINE_MARK_USE_HIDE.Size = new System.Drawing.Size(123, 52);
                LB_FINDLINE_MARK_USE_HIDE.Visible = true;

                LB_FINDCIRCLE_MARK_USE_HIDE.Location = new System.Drawing.Point(128, 128);
                LB_FINDCIRCLE_MARK_USE_HIDE.Size = new System.Drawing.Size(123, 52);
                LB_FINDCIRCLE_MARK_USE_HIDE.Visible = true;
            }

            BTN_DISNAME_01.BackColor = System.Drawing.Color.SkyBlue;
            BTN_DISNAME_01_Click(BTN_DISNAME_01, null);

            RBTN_PAT[0].Checked = true;

            DisplayFit(PT_CALIPER_SUB_Display);
            timer1.Enabled = false;  //Live

            lblOkDistanceValueX.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceX);
            lblOkDistanceValueY.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceY);
            lblAlignSpecValueX.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecX);
            lblAlignSpecValueY.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecY);
            dBondingAlignOriginDistX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceX;
            dBondingAlignOriginDistY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceY;
            dBondingAlignDistSpecX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecX;
            dBondingAlignDistSpecY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecY;
            lblObjectDistanceXValue.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceX);
            lblObjectDistanceXSpecValue.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceSpecX);
            dObjectDistanceSpecX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceSpecX;
            dObjectDistanceX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceX;
            // 200618 JHKIM 살림
            //TABC_MANU.TabPages[3].Text = "";
            //TABC_MANU.TabPages[4].Text = ""; // GVO R&D에서 사용하지 않는 TOOL은 숨김.
            //if (TABC_MANU.TabPages[3].Text == "")
            //    BTN_TOOL_03.Visible = false;
            //if (TABC_MANU.TabPages[4].Text == "")
            //    BTN_TOOL_04.Visible = false;
            //TABC_MANU.TabPages[3].Text = "FIND LINE";
            //TABC_MANU.TabPages[4].Text = "FIND CIRCLE"; // GVO R&D에서 사용하지 않는 TOOL은 숨김.
            ////if (TABC_MANU.TabPages[3].Text == "")
            //    BTN_TOOL_03.Visible = true;
            ////if (TABC_MANU.TabPages[4].Text == "")
            //    BTN_TOOL_04.Visible = true;

            // ksh 사용하지않는 TabPage Visible false 및 User에따라 Teaching 권한 변경
            if (Main.machine.Permission == Main.ePermission.OPERATOR)
            {
                TABC_MANU.TabPages.Remove(TAB_01);
                TABC_MANU.TabPages.Remove(TAB_02);
                TABC_MANU.TabPages.Remove(TAB_03);
                TABC_MANU.TabPages.Remove(TAB_04);
                TABC_MANU.TabPages.Remove(TAB_05);
                gbxThetaSpecSetting.Visible = false;
                RDB_MATERIAL_ALIGN.Visible = false;
                BTN_PATTERN_COPY.Visible = false;
                btn_Inspection_Test.Visible = false;
                gbxToolSetting.Visible = false;

                TABC_MANU.SelectTab(TAB_00);
            }
            else
            {
                TABC_MANU.TabPages.Remove(TAB_01);
                TABC_MANU.TabPages.Remove(TAB_02);
                TABC_MANU.TabPages.Remove(TAB_03);
                TABC_MANU.TabPages.Remove(TAB_04);
                TABC_MANU.TabPages.Remove(TAB_05);
                TABC_MANU.TabPages.Add(TAB_05);
                gbxThetaSpecSetting.Visible = true;
                RDB_MATERIAL_ALIGN.Visible = true;
                BTN_PATTERN_COPY.Visible = true;
                btn_Inspection_Test.Visible = true;
                gbxToolSetting.Visible = true;

                TABC_MANU.SelectTab(TAB_05);
            }

            DisplayFit(PT_Display01);

            chkUseTracking.Checked = false;
            _isFormLoad = true;
        }// Form_PatternTeach_Load

        private bool _isFormLoad = false;

        private void Form_PatternTeach_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;

            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
                {
                    PT_Pattern[i, j].Dispose();
                }
            }
            PT_BlobToolBlock.Dispose();
            PT_PMBlobTool.Dispose();
        }
        private void BTN_PAT_CHANGE_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;

            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }
        private void RBTN_PAT_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            int m_Number;
            m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 1, 1));
            if (m_PatNo == m_Number) return;

            nDistanceShow[m_PatNo] = false;
            m_PatNo = m_Number;
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1" || (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2" && m_CamNo == Main.DEFINE.CAM_SELECT_1ST_ALIGN))
            {
                RBTN_FINDLINE00.Checked = true;
                m_SelectFindLine = 0;
                m_LineSubNo = 0;
                CB_FINDLINE_SUBLINE.SelectedIndex = 0;
                TABC_MANU.SelectedIndex = M_TOOL_MODE = Main.DEFINE.M_FINDLINETOOL;
                FINDLINE_Change();
            }
            else
            {
                m_PatNo_Sub = 0;
                CB_SUB_PATTERN.SelectedIndex = 0;
                LightRadio[0].Checked = true;
                TABC_MANU.SelectedIndex = M_TOOL_MODE = Main.DEFINE.M_CNLSEARCHTOOL;
                Pattern_Change();
            }

        }
        private void LightCheck(int nM_TOOL_MODE)
        {
            if (!Main.AlignUnit[m_AlignNo].LightUseCheck(m_PatNo))
            {
                int nTempPatNo = Main.DEFINE.OBJ_L;
                try
                {
                    if (m_PatNo == Main.DEFINE.OBJ_L && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.OBJ_R)) nTempPatNo = Main.DEFINE.OBJ_R;
                    if (m_PatNo == Main.DEFINE.OBJ_R && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.OBJ_L)) nTempPatNo = Main.DEFINE.OBJ_L;
                    if (m_PatNo == Main.DEFINE.TAR_L && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.TAR_R)) nTempPatNo = Main.DEFINE.TAR_R;
                    if (m_PatNo == Main.DEFINE.TAR_R && Main.AlignUnit[m_AlignNo].LightUseCheck(Main.DEFINE.TAR_L)) nTempPatNo = Main.DEFINE.TAR_L;

                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, nTempPatNo].SetAllLight(nM_TOOL_MODE);
                }
                catch
                {

                }
            }
        }
        private void Pattern_Change()
        {
            BTN_BackColor();
            m_CamNo = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo;
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            LABEL_MESSAGE(LB_MESSAGE1, "", System.Drawing.Color.Lime);
            Light_Select();
            LightCheck(M_TOOL_MODE);
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(M_TOOL_MODE);
            if (!bROIFinealignTeach)
                PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];

            OriginImage = (CogImage8Grey)Main.vision.CogCamBuf[m_CamNo];
            PT_DISPLAY_CONTROL.Resuloution = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CalX[0];
            // CUSTOM CROSS
            PT_DISPLAY_CONTROL.UseCustomCross = Main.vision.USE_CUSTOM_CROSS[m_CamNo];
            PT_DISPLAY_CONTROL.CustomCross = new PointF(Main.vision.CUSTOM_CROSS_X[m_CamNo], Main.vision.CUSTOM_CROSS_Y[m_CamNo]);
            DisplayClear();
            Main.DisplayRefresh(PT_Display01);
            //     if (BTN_DISNAME_01.BackColor.Name != "SkyBlue") CrossLine();
            if (PT_DISPLAY_CONTROL.CrossLineChecked) CrossLine();
            //--------------------CNLSEARCH-------------------------------------------
            #region CNLSEARCH
            m_RetiMode = 0;
            if (bROIFinealignTeach)
            {
                FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = PT_Display01.Image;
                NUD_PAT_SCORE.Value = (decimal)dFinealignMarkScore;
                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion.GetType().Name != "CogRectangle")
                {
                    FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
                }

                PatMaxTrainRegion = new CogRectangle(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion as CogRectangle);
                MarkORGPoint.X = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationX;
                MarkORGPoint.Y = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationY;

                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].SearchRegion == null)
                {
                    PatMaxSearchRegion = new CogRectangle();
                    PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
                }
                else
                {
                    PatMaxSearchRegion = new CogRectangle(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].SearchRegion as CogRectangle);
                }

                for (int i = 0; i < Main.DEFINE.SUBPATTERNMAX; i++)
                {
                    SUBPATTERN_LABELDISPLAY(PT_Pattern_USE[nROIFineAlignIndex, i], i);
                    DrawTrainedPattern(PT_SubDisplay[i], FinealignMark[nROIFineAlignIndex, i]);
                }

                if (m_PatNo_Sub == 0)
                {
                    CB_SUBPAT_USE.Visible = false;
                }
                else
                {
                    CB_SUBPAT_USE.Visible = true;
                    CB_SUBPAT_USE.Checked = PT_Pattern_USE[nROIFineAlignIndex, m_PatNo_Sub];
                }
            }
            else
            {
                PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);

                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion.GetType().Name != "CogRectangle")
                {
                    PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
                }

                PatMaxTrainRegion = new CogRectangle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion as CogRectangle);
                MarkORGPoint.X = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX;
                MarkORGPoint.Y = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY;

                if (PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion == null)
                {
                    PatMaxSearchRegion = new CogRectangle();
                    PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
                }
                else
                {
                    PatMaxSearchRegion = new CogRectangle(PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion as CogRectangle);
                }

                for (int i = 0; i < Main.DEFINE.SUBPATTERNMAX; i++)
                {
                    SUBPATTERN_LABELDISPLAY(PT_Pattern_USE[m_PatNo, i], i);
                    DrawTrainedPattern(PT_SubDisplay[i], PT_Pattern[m_PatNo, i]);
                }

                if (m_PatNo_Sub == 0)
                {
                    CB_SUBPAT_USE.Visible = false;
                }
                else
                {
                    CB_SUBPAT_USE.Visible = true;
                    CB_SUBPAT_USE.Checked = PT_Pattern_USE[m_PatNo, m_PatNo_Sub];
                }
                NUD_PAT_SCORE.Value = (decimal)PT_AcceptScore[m_PatNo];
                NUD_PAT_GSCORE.Value = (decimal)PT_GAcceptScore[m_PatNo];
            }





            if (Main.AlignUnit[m_AlignNo].m_AlignName == "FPC_TRAY1" || Main.AlignUnit[m_AlignNo].m_AlignName == "FPC_TRAY2")
            {
                NUD_Initial_Flag = true;
                NUD_GUIDEDISX.Value = (decimal)PT_TRAY_GUIDE_DISX[m_PatNo];
                NUD_GUIDEDISY.Value = (decimal)PT_TRAY_GUIDE_DISY[m_PatNo];

                NUD_PITCHDISX.Value = (decimal)PT_TRAY_PITCH_DISX[m_PatNo];
                NUD_PITCHDISY.Value = (decimal)PT_TRAY_PITCH_DISY[m_PatNo];

                NUD_POCKETCOUNT_X_00.Value = TRAY_POCKET_X;
                NUD_POCKETCOUNT_Y_01.Value = TRAY_POCKET_Y;
                NUD_Initial_Flag = false;
            }

            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_PMAlign_Use == false)
            {
                label13.Visible = false;
                NUD_PAT_GSCORE.Visible = false;
            }
            #endregion
            //-----------------------------------------------------------------------
        }

        #region DRAW & REFRESH IMAGE
        private void Draw_Label(CogRecordDisplay nDisplay, string resultText, int index)
        {
            int i;
            CogGraphicLabel Label = new CogGraphicLabel();
            i = index;
            float nFontSize = 0;

            //            double nManuFont = 0;            
            //            if (Main.Status.MC_MODE == Main.DEFINE.MC_TEACHFORM) nManuFont = 0.5;

            double baseZoom = 0;
            if ((double)nDisplay.Width / nDisplay.Image.Width < (double)nDisplay.Height / nDisplay.Image.Height)
            {
                baseZoom = ((double)nDisplay.Width - 22) / nDisplay.Image.Width;
                nFontSize = (float)((nDisplay.Image.Width / Main.DEFINE.FontSize) * baseZoom);
            }
            else
            {
                baseZoom = ((double)nDisplay.Height - 22) / nDisplay.Image.Height;
                nFontSize = (float)((nDisplay.Image.Height / Main.DEFINE.FontSize) * baseZoom);
            }


            double nFontpitch = (nFontSize / nDisplay.Zoom);
            Label.Text = resultText;
            Label.Color = CogColorConstants.Cyan;
            Label.Font = new Font(Main.DEFINE.FontStyle, nFontSize);
            Label.Alignment = CogGraphicLabelAlignmentConstants.TopLeft;
            Label.X = (nDisplay.Image.Width - (nDisplay.Image.Width / (nDisplay.Zoom / baseZoom))) / 2 - nDisplay.PanX;
            Label.Y = (nDisplay.Image.Height - (nDisplay.Image.Height / (nDisplay.Zoom / baseZoom))) / 2 - nDisplay.PanY + (i * nFontpitch);


            nDisplay.StaticGraphics.Add(Label as ICogGraphic, "Result Text");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (bLiveStop == false)
            {
                RefreshTeach();
                //OriginImage = (CogImage8Grey)PT_Display01.Image;
                //if(chkUseRoiTracking.Checked == true)
                //{
                //    if (Search_PATCNL())
                //    {
                //        double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                //        double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                //        TrackingROI(TranslationX, TranslationY);
                //    }
                //}
            }
        }
        delegate void dGrabRefresh(CogRecordDisplay nDisplay, ICogImage nImageBuf);
        public static void GrabDisRefresh_(CogRecordDisplay nDisplay, ICogImage nImageBuf)
        {

            if (nDisplay.InvokeRequired)
            {
                dGrabRefresh call = new dGrabRefresh(GrabDisRefresh_);
                nDisplay.Invoke(call, nDisplay, nImageBuf);
            }
            else
            {
                //jyh 임시 막음
                nDisplay.Image = nImageBuf;
            }
        }
        private void CrossLine()
        {
            PT_DISPLAY_CONTROL.CrossLine();
            //             CogLine mCogLine1 = new CogLine();
            //             CogLine mCogLine2 = new CogLine();
            //             mCogLine1.Color = CogColorConstants.Magenta;
            //             mCogLine2.Color = CogColorConstants.Magenta;
            //             mCogLine1.SetFromStartXYEndXY(0, (double)Main.vision.IMAGE_CENTER_Y[m_CamNo], (double)Main.vision.IMAGE_SIZE_X[m_CamNo], (double)Main.vision.IMAGE_CENTER_Y[m_CamNo]);
            //             PT_Display01.StaticGraphics.Add(mCogLine1 as ICogGraphic, "Find MarkerPos");
            // 
            //             mCogLine2.SetFromStartXYEndXY((double)Main.vision.IMAGE_CENTER_X[m_CamNo], 0, (double)Main.vision.IMAGE_CENTER_X[m_CamNo], (double)Main.vision.IMAGE_SIZE_Y[m_CamNo]);
            //             PT_Display01.StaticGraphics.Add(mCogLine2 as ICogGraphic, "Find MarkerPos");
        }
        private void RefreshTeach()
        {
            Main.vision.Grab_Flag_Start[m_CamNo] = true;
            GrabDisRefresh_(PT_Display01, Main.vision.CogCamBuf[m_CamNo]);
        }
        #endregion

        #region 조명조절관련
        private void BTN_LIGHT_UP_Click(object sender, EventArgs e)
        {
            if (TBAR_LIGHT.Maximum == TBAR_LIGHT.Value) return;
            TBAR_LIGHT.Value++;
        }
        private void BTN_LIGHT_DOWN_Click(object sender, EventArgs e)
        {
            if (TBAR_LIGHT.Minimum == TBAR_LIGHT.Value) return;
            TBAR_LIGHT.Value--;
        }
        private void RBTN_LIGHT_0_CheckedChanged(object sender, EventArgs e)
        {
            Light_Select();
        }
        private void TBAR_LIGHT_ValueChanged(object sender, EventArgs e)
        {
            Light_Change(m_SelectLight);
        }
        private void Light_Select()
        {
            bool nLightUse = false;
            for (int i = 0; i < Main.DEFINE.Light_PatMaxCount; i++)
            {
                //Light_Text[i].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[i, M_TOOL_MODE].ToString();
                Light_Text[i].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[i, 0].ToString();
                LightRadio[i].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_Light_Name[i];
                if (LightRadio[i].Checked)
                {
                    m_SelectLight = i;
                    LightRadio[i].BackColor = System.Drawing.Color.LawnGreen;
                }
                else
                {
                    LightRadio[i].BackColor = System.Drawing.Color.DarkGray;
                }

                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightCtrl[i] < 0)
                {
                    Light_Text[i].Visible = false;
                    LightRadio[i].Visible = false;
                }
                else
                {
                    Light_Text[i].Visible = true;
                    LightRadio[i].Visible = true;
                    TBAR_LIGHT.Visible = true;
                    BTN_LIGHT_UP.Visible = true;
                    BTN_LIGHT_DOWN.Visible = true;
                    nLightUse = true;
                }
            }
            if (!nLightUse)
            {
                TBAR_LIGHT.Visible = false;
                BTN_LIGHT_UP.Visible = false;
                BTN_LIGHT_DOWN.Visible = false;
            }
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightCtrl[m_SelectLight] >= 0)
                TBAR_LIGHT.Value = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_SelectLight, 0];
            //20220902 YSH Tool Insp 에서 조명조절시 인덱스 초과
            //TBAR_LIGHT.Value = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_SelectLight, M_TOOL_MODE];
        }
        private void Light_Change(int m_LightNum)
        {
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetLight(m_LightNum, TBAR_LIGHT.Value);
            //20220902 YSH Tool Insp 에서 조명조절시 인덱스 초과
            //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_LightNum, M_TOOL_MODE] = TBAR_LIGHT.Value;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_LightNum, 0] = TBAR_LIGHT.Value;
            Light_Text[m_LightNum].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_LightNum, 0].ToString();
            //20220902 YSH Tool Insp 에서 조명조절시 인덱스 초과
            //Light_Text[m_LightNum].Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_LightValue[m_LightNum, M_TOOL_MODE].ToString();
            //             if (M_TOOL_MODE == M_BLOBTOOL || M_TOOL_MODE == M_CALIPERTOOL || M_TOOL_MODE == M_FINDLINETOOL) 
            //                 RefreshDisplay2();
        }
        private void LB_LIGHT_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Label TempLB = (Label)sender;
                int nNum;
                nNum = Convert.ToInt16(TempLB.Name.Substring(TempLB.Name.Length - 1, 1));
                Form_LightSet formLight = new Form_LightSet(m_AlignNo, m_PatTagNo, m_PatNo, nNum);
                formLight.ShowDialog();
                formLight.Dispose();
                Light_Select();
            }
        }
        #endregion

        #region 버튼클릭이벤트들

        private void BTN_IMAGE_OPEN_Click(object sender, EventArgs e)
        {
            //openFileDialog1.CheckFileExists = true;
            bLiveStop = true;
            timer1.Enabled = false;
            BTN_LIVEMODE.BackColor = Color.DarkGray;

            string filePath = "";
            openFileDialog1.ReadOnlyChecked = true;
            openFileDialog1.Filter = "Bmp File(*.bmp)|*.bmp;,|Jpg File(*.jpg)|*.jpg";
            //openFileDialog1.InitialDirectory = 
            openFileDialog1.ShowDialog();
            
            if (openFileDialog1.FileName != "")
            {
                if (Main.vision.CogImgTool[m_CamNo] == null)
                    Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
                Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], openFileDialog1.FileName);
                Main.vision.CogCamBuf[m_CamNo] = Main.vision.CogImgTool[m_CamNo].OutputImage;

                filePath = openFileDialog1.FileName;
                CurrentFolderPath = RemoveSourceCode(filePath);
                //shkang_s OK 이미지 Teaching을 위해 JPG->Cog8greyImage 변환
                if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
                {
                    if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 6) == "OV.jpg")
                    {
                        MessageBox.Show("This JPG File is 'Overlay Image'.\r\nSelect Origin JPG Image.");
                        return;
                    }

                    string[] files = Directory.GetFiles(CurrentFolderPath, "*UP.jpg");

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (filePath == files[i])
                        {
                            CurrentImageNumber = i;
                            break;
                        }
                    }

                    CogImageConvertTool img = new CogImageConvertTool();
                    img.InputImage = Main.vision.CogCamBuf[m_CamNo];
                    img.Run();
                    Main.vision.CogCamBuf[m_CamNo]= img.OutputImage;
                }
                //shkang_e
                else if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "bmp")
                {
                    string[] files = Directory.GetFiles(CurrentFolderPath, "*.bmp");

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (filePath == files[i])
                        {
                            CurrentImageNumber = i;
                            break;
                        }
                    }
                }


                PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
                OriginImage = (CogImage8Grey)Main.vision.CogCamBuf[m_CamNo];
                if (_useROITracking)
                    FinalTracking();
                DisplayClear();
                if (timer1.Enabled == true)
                    timer1.Enabled = false;
                Main.DisplayRefresh(PT_Display01);
            }
        }
        private void BTN_TOOL_SET_Click(object sender, EventArgs e)
        {
            Button TempBTN = (Button)sender;
            try
            {
                switch (TempBTN.Text)
                {
                    case "SEARCHMAX": //CogCNLSearch
                        PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        ToolTeach.TT_SearchMaxTool = PT_Pattern[m_PatNo, m_PatNo_Sub];
                        ToolTeach.m_ToolTextName = "CogSearchMaxTool";
                        break;

                    case "PMALIGN":
                        PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        ToolTeach.TT_PMAlign = PT_GPattern[m_PatNo, m_PatNo_Sub];
                        ToolTeach.m_ToolTextName = "CogPMAlignTool";
                        break;

                    case "BLOB":
                        RefreshDisplay2();
                        ToolTeach.TT_BlobTool = PT_BlobTools[m_PatNo, m_SelectBlob];
                        ToolTeach.m_ToolTextName = "CogBlobTool";
                        break;

                    case "CALIPER":
                        if (_eTabSelect == enumTabSelect.ThetaOrigin)
                        {
                            RefreshDisplay2();
                            ToolTeach.TT_CALTool = m_TeachAlignLine[(int)_bondingAlignPosition];
                            ToolTeach.m_ToolTextName = "CogCaliperTool";
                        }
                        else
                        {
                            RefreshDisplay2();
                            ToolTeach.TT_CALTool = PT_CaliperTools[m_PatNo, m_SelectCaliper];
                            ToolTeach.m_ToolTextName = "CogCaliperTool";
                        }
                        break;

                    case "LINE":
                        if (_eTabSelect == enumTabSelect.Insp)
                        {
                            RefreshDisplay2();
                            ToolTeach.TT_FindLine = m_TempFindLineTool;
                            ToolTeach.m_ToolTextName = "CogFindLineTool";
                        }
                        else if (_eTabSelect == enumTabSelect.ThetaOrigin)
                        {
                            RefreshDisplay2();
                            ToolTeach.TT_FindLine = m_TempTrackingLine;
                            ToolTeach.m_ToolTextName = "CogFindLineTool";
                        }
                        else
                        {
                            RefreshDisplay2();
                            ToolTeach.TT_FindLine = PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine];
                            ToolTeach.TT_FindLine = m_TempTrackingLine;
                            ToolTeach.m_ToolTextName = "CogFindLineTool";
                        }
                        break;

                    case "CIRCLE":
                        if (_eTabSelect == enumTabSelect.Insp)
                        {
                            RefreshDisplay2();
                            ToolTeach.TT_FindCircle = m_TempFindCircleTool;
                            ToolTeach.m_ToolTextName = "CogFindCircleTool";
                        }
                        else
                        {
                            RefreshDisplay2();
                            ToolTeach.TT_FindCircle = PT_CircleTools[m_PatNo, m_SelectCircle];
                            ToolTeach.m_ToolTextName = "CogFindCircleTool";
                        }
                        break;
                }
                ToolTeach.m_AlignNo = m_AlignNo;
                ToolTeach.m_PatNo = m_PatNo;
                ToolTeach.ShowDialog();
                if (TempBTN.Text == "SEARCHMAX" || TempBTN.Text == "PMALIGN") { CB_SUB_PATTERN_SelectionChangeCommitted(null, null); }
                if (TempBTN.Text == "CALIPER") { Caliper_Change(); }
                if (TempBTN.Text == "BLOB") { Blob_Change(); }
                if (TempBTN.Text == "FINDLINE") { FINDLINE_Change(); }
                if (TempBTN.Text == "CIRCLE") { Circle_Change(); }
            }
            catch
            {

            }
        }
        private void BTN_DISNAME_01_Click(object sender, EventArgs e)
        {
            Button TempBTN = (Button)sender;
            int m_Number;
            m_Number = TempBTN.TabIndex;

            if (TempBTN.BackColor.Name == "SkyBlue")
            {
                TempBTN.BackColor = Color.Plum;
                CrossLine();
            }
            else
            {
                TempBTN.BackColor = Color.SkyBlue;
                DisplayClear();
                nDistanceShow[m_PatNo] = false;
                LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
            }
        }
        bool nPatternCopy = false;
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            formMessage.LB_MESSAGE.Text = "Did You Check [APPLY]?";
            if (!formMessage.Visible)
            {
                formMessage.ShowDialog();
            }

            Form_Password formpassword = new Form_Password(true);
            formpassword.ShowDialog();

            if (!formpassword.LOGINOK)
            {
                nPatternCopy = false;
                formpassword.Dispose();
                return;
            }
            formpassword.Dispose();

            string strParaName = "";
            #region CNLSEARCH SAVE
            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                strParaName = "PATTERN SCORE";
                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_ACCeptScore, PT_AcceptScore[i]);
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_ACCeptScore = PT_AcceptScore[i];
                strParaName = "GPATTERN SCORE";
                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_GACCeptScore, PT_GAcceptScore[i]);
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_GACCeptScore = PT_GAcceptScore[i];

                for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
                {
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern[j] = new CogSearchMaxTool(PT_Pattern[i, j]);

                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern_USE[j] = PT_Pattern_USE[i, j];
                    if (j == 0) Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Pattern_USE[j] = true;

                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].GPattern[j] = new CogPMAlignTool(PT_GPattern[i, j]);
                }
            }
            #endregion

            #region Inspection


            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                //shkang_s
                for (int j = 0; j < tempCaliperNum.Count; j++) 
                {
                    //해당 번호의 ROI의 Line,Circle 확인
                    if(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_enumROIType == 0)  //Line
                    {
                        //Threshold
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "ContrastThreshold", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold, Main.DEFINE.CHANGEPARA);
                        //FilterSize
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "FilterSize", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, Main.DEFINE.CHANGEPARA);
                        //Caliper Count
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Count", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.NumCalipers, Main.DEFINE.CHANGEPARA);
                        //Caliper Projection Length
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Projection Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperProjectionLength, Main.DEFINE.CHANGEPARA);
                        //Caliper Search Length
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength != m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Search Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength, m_TeachParameter[tempCaliperNum[j]].m_FindLineTool.RunParams.CaliperSearchLength, Main.DEFINE.CHANGEPARA);
                        //관로 폭 Spec - 무시갯수
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore != m_TeachParameter[tempCaliperNum[j]].IDistgnore)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore, m_TeachParameter[tempCaliperNum[j]].IDistgnore, Main.DEFINE.CHANGEPARA);
                        //관로 폭 Spec - Distance Min
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance != m_TeachParameter[tempCaliperNum[j]].dSpecDistance)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance, m_TeachParameter[tempCaliperNum[j]].dSpecDistance, Main.DEFINE.CHANGEPARA);
                        //관로 폭 Spec - Distance Max
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax != m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax, m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax, Main.DEFINE.CHANGEPARA);
                    }
                    else   //Circle
                    {
                        //Threshold
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "ContrastThreshold", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold, Main.DEFINE.CHANGEPARA);
                        //FilterSize
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "FilterSize", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels, Main.DEFINE.CHANGEPARA);
                        //Caliper Count
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Count", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.NumCalipers, Main.DEFINE.CHANGEPARA);
                        //Caliper Projection Length
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Projection Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperProjectionLength, Main.DEFINE.CHANGEPARA);
                        //Caliper Search Length
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength != m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Caliper Search Length", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength, m_TeachParameter[tempCaliperNum[j]].m_FindCircleTool.RunParams.CaliperSearchLength, Main.DEFINE.CHANGEPARA);
                        //관로 폭 Spec - 무시갯수
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore != m_TeachParameter[tempCaliperNum[j]].IDistgnore)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].IDistgnore, m_TeachParameter[tempCaliperNum[j]].IDistgnore, Main.DEFINE.CHANGEPARA);
                        //관로 폭 Spec - Distance Min
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance != m_TeachParameter[tempCaliperNum[j]].dSpecDistance)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistance, m_TeachParameter[tempCaliperNum[j]].dSpecDistance, Main.DEFINE.CHANGEPARA);
                        //관로 폭 Spec - Distance Max
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax != m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax)
                            Save_ChangeParaLog("ChangePara", tempCaliperNum[j], "Spec - IgnoreCnt", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[tempCaliperNum[j]].dSpecDistanceMax, m_TeachParameter[tempCaliperNum[j]].dSpecDistanceMax, Main.DEFINE.CHANGEPARA);
                    }
                }
                //shkang_e


                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter = m_TeachParameter;
                var temp = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_InspParameter[0];
                if (temp.m_CogBlobTool[0].Region != null)
                {
                    CogPolygon ROITracking = (CogPolygon)temp.m_CogBlobTool[0].Region;
                    double dx = ROITracking.GetVertexX(0);
                }

            }
            for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j < 2)
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LeftOrigin[j] = LeftOrigin[j];
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].RightOrigin[j] = RightOrigin[j];
                    }
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_TrackingLine[j] = m_TeachLine[j];
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_BondingAlignLine[j] = m_TeachAlignLine[j];   //shkang Save Bonding Align Data

                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dOriginDistanceX = dBondingAlignOriginDistX;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dOriginDistanceY = dBondingAlignOriginDistY;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dDistanceSpecX = dBondingAlignDistSpecX;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_dDistanceSpecY = dBondingAlignDistSpecY;
                }
            }
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceSpecX != dObjectDistanceSpecX)
                Save_ChangeParaLog("ChangePara", "m_dObjectDistanceSpecX", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceSpecX, dObjectDistanceSpecX, Main.DEFINE.CHANGEPARA);
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceSpecX = dObjectDistanceSpecX;
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceX != dObjectDistanceX)
                Save_ChangeParaLog("ChangePara", "m_dObjectDistanceX", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceX, dObjectDistanceX, Main.DEFINE.CHANGEPARA);
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_dObjectDistanceX = dObjectDistanceX;

            //YSH ROI Finealign
            for (int i = 0; i < Main.DEFINE.Pattern_Max; i++)
            {
                for (int j = 0; j < Main.DEFINE.SUBPATTERNMAX; j++)
                {
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMark[i, j] = FinealignMark[i, j];
                }
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_bFInealignFlag = m_bROIFinealignFlag;
                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignThetaSpec != m_dROIFinealignT_Spec)
                    Save_ChangeParaLog("ChangePara", "m_FinealignThetaSpec", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignThetaSpec, m_dROIFinealignT_Spec, Main.DEFINE.CHANGEPARA);
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignThetaSpec = m_dROIFinealignT_Spec;
                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMarkScore != dFinealignMarkScore)
                    Save_ChangeParaLog("ChangePara", "m_FinealignMarkScore", Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMarkScore, dFinealignMarkScore, Main.DEFINE.CHANGEPARA);
                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_FinealignMarkScore = dFinealignMarkScore;
            }

            #endregion

            #region BLOB SAVE 수정한거
            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Blob_MarkUse = PT_Blob_MarkUSE[i];
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Blob_CaliperUse = PT_Blob_CaliperUSE[i];
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_Blob_InspCnt = PT_Blob_InspCnt[i];
            //    for (int j = 0; j < Main.DEFINE.BLOB_CNT_MAX; j++)
            //    {
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobTools[j] = new CogBlobTool(PT_BlobTools[i, j]);
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_UseCheck = PT_BlobPara[i, j].m_UseCheck;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_Blob_InspCnt = PT_Blob_InspCnt[i];
            //        for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
            //        {
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_TargetToCenter[k].X = PT_BlobPara[i, j].m_TargetToCenter[k].X;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].BlobPara[j].m_TargetToCenter[k].Y = PT_BlobPara[i, j].m_TargetToCenter[k].Y;
            //        }
            //    }
            //}
            //#endregion

            //#region CALIPER SAVE
            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Caliper_MarkUse = PT_Caliper_MarkUSE[i];
            //    for (int j = 0; j < Main.DEFINE.CALIPER_MAX; j++)
            //    {
            //        {
            //            strParaName = PT_CaliperName[j] + " CALIPER USE";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_UseCheck, PT_CaliPara[i, j].m_UseCheck);
            //            strParaName = PT_CaliperName[j] + " THRESHOLD";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j].RunParams.ContrastThreshold, PT_CaliperTools[i, j].RunParams.ContrastThreshold);
            //            strParaName = PT_CaliperName[j] + " DIRECTION";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j].Region.Rotation, PT_CaliperTools[i, j].Region.Rotation);
            //            strParaName = PT_CaliperName[j] + " POLARITY";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j].RunParams.Edge0Polarity.ToString(), PT_CaliperTools[i, j].RunParams.Edge0Polarity.ToString());
            //            strParaName = PT_CaliperName[j] + " PAIR USE";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j].RunParams.EdgeMode.ToString(), PT_CaliperTools[i, j].RunParams.EdgeMode.ToString());
            //        }
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperTools[j] = new CogCaliperTool(PT_CaliperTools[i, j]);
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_UseCheck = PT_CaliPara[i, j].m_UseCheck;

            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_bCOPMode = PT_CaliPara[i, j].m_bCOPMode;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_nCOPROICnt = PT_CaliPara[i, j].m_nCOPROICnt;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_nCOPROIOffset = PT_CaliPara[i, j].m_nCOPROIOffset;

            //        for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
            //        {
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_TargetToCenter[k].X = PT_CaliPara[i, j].m_TargetToCenter[k].X;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CaliperPara[j].m_TargetToCenter[k].Y = PT_CaliPara[i, j].m_TargetToCenter[k].Y;
            //        }
            //    }
            //}
            //#endregion

            //#region FINDLine SAVE
            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLine_MarkUse = PT_FindLine_MarkUSE[i];
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_GUIDE_DISX = PT_TRAY_GUIDE_DISX[i];
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_GUIDE_DISY = PT_TRAY_GUIDE_DISY[i];
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_PITCH_DISX = PT_TRAY_PITCH_DISX[i];
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].TRAY_PITCH_DISY = PT_TRAY_PITCH_DISY[i];

            //    for (int ii = 0; ii < Main.DEFINE.SUBLINE_MAX; ii++)
            //    {
            //        for (int j = 0; j < Main.DEFINE.FINDLINE_MAX; j++)
            //        {
            //            {
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " USE";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_UseCheck, PT_FindLinePara[i, ii, j].m_UseCheck);
            //            }

            //            if (!Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_UseLineMax)
            //            {
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " THRESHOLD";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.CaliperRunParams.ContrastThreshold, PT_FindLineTools[i, ii, j].RunParams.CaliperRunParams.ContrastThreshold);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " DIRECTION";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.CaliperSearchDirection, PT_FindLineTools[i, ii, j].RunParams.CaliperSearchDirection);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " POLARITY";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.CaliperRunParams.Edge0Polarity.ToString(), PT_FindLineTools[i, ii, j].RunParams.CaliperRunParams.Edge0Polarity.ToString());
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " CALIPER COUNT";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.NumCalipers, PT_FindLineTools[i, ii, j].RunParams.NumCalipers);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " IGNORE COUNT";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.NumToIgnore, PT_FindLineTools[i, ii, j].RunParams.NumToIgnore);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " FILTER SIZE";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.CaliperRunParams.FilterHalfSizeInPixels, PT_FindLineTools[i, ii, j].RunParams.CaliperRunParams.FilterHalfSizeInPixels);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " SEARCH LENGTH";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.CaliperSearchLength, PT_FindLineTools[i, ii, j].RunParams.CaliperSearchLength);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " PROJECTION LENGTH";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.CaliperProjectionLength, PT_FindLineTools[i, ii, j].RunParams.CaliperProjectionLength);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " LINE STARTX";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.ExpectedLineSegment.StartX, PT_FindLineTools[i, ii, j].RunParams.ExpectedLineSegment.StartX);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " LINE STARTY";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.ExpectedLineSegment.StartY, PT_FindLineTools[i, ii, j].RunParams.ExpectedLineSegment.StartY);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " LINE ENDX";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.ExpectedLineSegment.EndX, PT_FindLineTools[i, ii, j].RunParams.ExpectedLineSegment.EndX);
            //                strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " LINE ENDY";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j].RunParams.ExpectedLineSegment.EndY, PT_FindLineTools[i, ii, j].RunParams.ExpectedLineSegment.EndY);
            //            }
            //            else
            //            {
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " THRESHOLD";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.EdgeDetectionParams.ContrastThreshold, PT_LineMaxTools[i, ii, j].RunParams.EdgeDetectionParams.ContrastThreshold);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " DIRECTION";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.ExpectedLineNormal.Angle, PT_LineMaxTools[i, ii, j].RunParams.ExpectedLineNormal.Angle);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " POLARITY";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.Polarity.ToString(), PT_LineMaxTools[i, ii, j].RunParams.Polarity.ToString());
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " GRAD KERNEL SIZE";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.EdgeDetectionParams.GradientKernelSizeInPixels, PT_LineMaxTools[i, ii, j].RunParams.EdgeDetectionParams.GradientKernelSizeInPixels);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " PROJECTION LENGTH";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.EdgeDetectionParams.ProjectionLengthInPixels, PT_LineMaxTools[i, ii, j].RunParams.EdgeDetectionParams.ProjectionLengthInPixels);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " ANGLE TOLERANCE";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.EdgeAngleTolerance, PT_LineMaxTools[i, ii, j].RunParams.EdgeAngleTolerance);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " DIST TOLERANCE";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.DistanceTolerance, PT_LineMaxTools[i, ii, j].RunParams.DistanceTolerance);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " LINE ANGLE TOLERANCE";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.LineAngleTolerance, PT_LineMaxTools[i, ii, j].RunParams.LineAngleTolerance);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " COVERAGE THRESHOLD";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.CoverageThreshold, PT_LineMaxTools[i, ii, j].RunParams.CoverageThreshold);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " LENGTH THRESHOLD";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].RunParams.LengthThreshold, PT_LineMaxTools[i, ii, j].RunParams.LengthThreshold);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " REGION CENTER X";
            //                CheckChangedParams(m_AlignNo, i, strParaName, (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].Region as CogRectangleAffine).CenterX, (PT_LineMaxTools[i, ii, j].Region as CogRectangleAffine).CenterX);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " REGION CENTER Y";
            //                CheckChangedParams(m_AlignNo, i, strParaName, (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].Region as CogRectangleAffine).CenterY, (PT_LineMaxTools[i, ii, j].Region as CogRectangleAffine).CenterY);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " REGION X LENGTH";
            //                CheckChangedParams(m_AlignNo, i, strParaName, (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].Region as CogRectangleAffine).SideXLength, (PT_LineMaxTools[i, ii, j].Region as CogRectangleAffine).SideXLength);
            //                strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " REGION Y LENGTH";
            //                CheckChangedParams(m_AlignNo, i, strParaName, (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j].Region as CogRectangleAffine).SideYLength, (PT_LineMaxTools[i, ii, j].Region as CogRectangleAffine).SideYLength);
            //            }
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLineTools[ii, j] = new CogFindLineTool(PT_FindLineTools[i, ii, j]);
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].LineMaxTools[ii, j] = new CogLineMaxTool(PT_LineMaxTools[i, ii, j]);

            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_UseCheck = PT_FindLinePara[i, ii, j].m_UseCheck;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_UsePairCheck = PT_FindLinePara[i, ii, j].m_UsePairCheck;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_LinePosition = PT_FindLinePara[i, ii, j].m_LinePosition;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_LineCaliperMethod = PT_FindLinePara[i, ii, j].m_LineCaliperMethod;
            //            for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
            //            {
            //                if (j == 2 && k == Main.DEFINE.M_FINDLINETOOL)
            //                {
            //                    if (!Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_UseLineMax)
            //                    {
            //                        strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " CENTER OFFSET X1";
            //                        CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].X, PT_FindLinePara[i, ii, j].m_TargetToCenter[k].X);
            //                        strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " CENTER OFFSET Y1";
            //                        CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].Y, PT_FindLinePara[i, ii, j].m_TargetToCenter[k].Y);
            //                        strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " CENTER OFFSET X2";
            //                        CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].X2, PT_FindLinePara[i, ii, j].m_TargetToCenter[k].X2);
            //                        strParaName = PT_FINDLineName[j] + " FINDLINE #" + ii.ToString() + " CENTER OFFSET Y2";
            //                        CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].Y2, PT_FindLinePara[i, ii, j].m_TargetToCenter[k].Y2);
            //                    }
            //                    else
            //                    {
            //                        strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " CENTER OFFSET X";
            //                        CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].X, PT_FindLinePara[i, ii, j].m_TargetToCenter[k].X);
            //                        strParaName = PT_FINDLineName[j] + " LINEMAX #" + ii.ToString() + " CENTER OFFSET Y";
            //                        CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].Y, PT_FindLinePara[i, ii, j].m_TargetToCenter[k].Y);
            //                    }
            //                }

            //                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].X = PT_FindLinePara[i, ii, j].m_TargetToCenter[k].X;
            //                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].Y = PT_FindLinePara[i, ii, j].m_TargetToCenter[k].Y;

            //                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].X2 = PT_FindLinePara[i, ii, j].m_TargetToCenter[k].X2;
            //                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].FINDLinePara[ii, j].m_TargetToCenter[k].Y2 = PT_FindLinePara[i, ii, j].m_TargetToCenter[k].Y2;
            //            }
            //        }
            //    }
            //}
            #endregion

            #region CIRCLE SAVE
            //for (int i = 0; i < Main.AlignUnit[m_AlignNo].m_AlignPatMax[m_PatTagNo]; i++)
            //{
            //    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].Circle_MarkUse = PT_Circle_MarkUSE[i];
            //    for (int j = 0; j < Main.DEFINE.CIRCLE_MAX; j++)
            //    {
            //        {
            //            strParaName = "CIRCLE #" + j + " USE";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_UseCheck, PT_CirclePara[i, j].m_UseCheck);
            //            strParaName = "CIRCLE #" + j + " THRESHOLD";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.CaliperRunParams.ContrastThreshold, PT_CircleTools[i, j].RunParams.CaliperRunParams.ContrastThreshold);
            //            strParaName = "CIRCLE #" + j + " DIRECTION";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.CaliperSearchDirection.ToString(), PT_CircleTools[i, j].RunParams.CaliperSearchDirection.ToString());
            //            strParaName = "CIRCLE #" + j + " POLARITY";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.CaliperRunParams.Edge0Polarity.ToString(), PT_CircleTools[i, j].RunParams.CaliperRunParams.Edge0Polarity.ToString());
            //            strParaName = "CIRCLE #" + j + " CALIPER COUNT";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.NumCalipers, PT_CircleTools[i, j].RunParams.NumCalipers);
            //            strParaName = "CIRCLE #" + j + " IGNORE COUNT";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.NumToIgnore, PT_CircleTools[i, j].RunParams.NumToIgnore);
            //            strParaName = "CIRCLE #" + j + " SEARCH LENGTH";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.CaliperSearchLength, PT_CircleTools[i, j].RunParams.CaliperSearchLength);
            //            strParaName = "CIRCLE #" + j + " PROJECTION LENGTH";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.CaliperProjectionLength, PT_CircleTools[i, j].RunParams.CaliperProjectionLength);
            //            strParaName = "CIRCLE #" + j + " CIRCLE RADIUS";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.ExpectedCircularArc.Radius, PT_CircleTools[i, j].RunParams.ExpectedCircularArc.Radius);
            //            strParaName = "CIRCLE #" + j + " CIRCLE MIDX";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.ExpectedCircularArc.MidpointX, PT_CircleTools[i, j].RunParams.ExpectedCircularArc.MidpointX);
            //            strParaName = "CIRCLE #" + j + " CIRCLE MIDY";
            //            CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j].RunParams.ExpectedCircularArc.MidpointY, PT_CircleTools[i, j].RunParams.ExpectedCircularArc.MidpointY);
            //        }
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CircleTools[j] = new CogFindCircleTool(PT_CircleTools[i, j]);
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_UseCheck = PT_CirclePara[i, j].m_UseCheck;
            //        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_CircleCaliperMethod = PT_CirclePara[i, j].m_CircleCaliperMethod;

            //        for (int k = 0; k < Main.DEFINE.M_TOOLMAXCOUNT; k++)
            //        {
            //            if (k == Main.DEFINE.M_FINDLINETOOL)
            //            {
            //                strParaName = "CIRCLE #" + j + " CENTER OFFSET X";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_TargetToCenter[k].X, PT_CirclePara[i, j].m_TargetToCenter[k].X);
            //                strParaName = "CIRCLE #" + j + " CENTER OFFSET Y";
            //                CheckChangedParams(m_AlignNo, i, strParaName, Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_TargetToCenter[k].Y, PT_CirclePara[i, j].m_TargetToCenter[k].Y);
            //            }
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_TargetToCenter[k].X = PT_CirclePara[i, j].m_TargetToCenter[k].X;
            //            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].CirclePara[j].m_TargetToCenter[k].Y = PT_CirclePara[i, j].m_TargetToCenter[k].Y;
            //        }
            //    }
            //}

            #endregion

            Main.AlignUnit[m_AlignNo].m_Tray_Pocket_X = TRAY_POCKET_X;
            Main.AlignUnit[m_AlignNo].m_Tray_Pocket_Y = TRAY_POCKET_Y;
            Main.AlignUnit[m_AlignNo].TrayBlobMode = CB_TRAY_BlobMode.Checked;

            // CUSTOM CROSS
            Main.vision.USE_CUSTOM_CROSS[m_CamNo] = PT_DISPLAY_CONTROL.UseCustomCross;
            Main.vision.CUSTOM_CROSS_X[m_CamNo] = (int)PT_DISPLAY_CONTROL.CustomCross.X;
            Main.vision.CUSTOM_CROSS_Y[m_CamNo] = (int)PT_DISPLAY_CONTROL.CustomCross.Y;

            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_DISPLAY_CONTROL.CustomCross.X, PT_DISPLAY_CONTROL.CustomCross.Y,
                                           ref Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dCustomCrossX, ref Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dCustomCrossY);

            Main.AlignUnit[m_AlignNo].Save(m_PatTagNo);
            Main.AlignUnit[m_AlignNo].Load(m_PatTagNo);

            #region Stage Pattern COPY
            if (Main.AlignUnit[m_AlignNo].m_AlignName == "PBD")
            {
                if (Main.DEFINE.PROGRAM_TYPE == "OLB_PC2" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC3" || Main.DEFINE.PROGRAM_TYPE == "FOF_PC4")
                {
                    string OrgName = "PBD", TarName = "PBD_STAGE";
                    for (int i = 0; i < Main.DEFINE.SUBPATTERNMAX; i++)
                    {
                        Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].Pattern[i] = new CogSearchMaxTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].Pattern[i]);
                        Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].Pattern[i] = new CogSearchMaxTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].Pattern[i]);

                        Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].GPattern[i] = new CogPMAlignTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].GPattern[i]);
                        Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].GPattern[i] = new CogPMAlignTool(Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].GPattern[i]);

                        Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].Pattern_USE[i] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].Pattern_USE[i];
                        Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].Pattern_USE[i] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].Pattern_USE[i];


                    }

                    for (int i = 0; i < Main.DEFINE.Light_PatMaxCount; i++)
                    {
                        for (int j = 0; j < Main.DEFINE.Light_ToolMaxCount; j++)
                        {
                            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightValue[i, j] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_L].m_LightValue[i, j];
                            Main.AlignUnit[TarName].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LightValue[i, j] = Main.AlignUnit[OrgName].PAT[m_PatTagNo, Main.DEFINE.TAR_R].m_LightValue[i, j];
                        }
                    }
                    Main.AlignUnit[TarName].Save(m_PatTagNo);
                }
            }
            #endregion

            #region Pattern Tag COPY
            if (nPatternCopy)
            {
                string TempName = Main.AlignUnit[m_AlignNo].m_AlignName;

                if (TempName == "PBD" || TempName == "PBD_STAGE" || TempName == "PBD_FOF" || TempName == "FPC_ALIGN")
                {
                    for (int i = 0; i < Main.AlignUnit[TempName].m_AlignPatTagMax; i++)
                    {
                        for (int j = 0; j < Main.AlignUnit[TempName].m_AlignPatMax[i]; j++)
                        {
                            Main.AlignUnit[TempName].PAT[i, j].m_ACCeptScore = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].m_ACCeptScore;
                            Main.AlignUnit[TempName].PAT[i, j].m_GACCeptScore = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].m_GACCeptScore;

                            for (int k = 0; k < Main.DEFINE.SUBPATTERNMAX; k++)
                            {
                                Main.AlignUnit[TempName].PAT[i, j].Pattern_USE[k] = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].Pattern_USE[k];
                                Main.AlignUnit[TempName].PAT[i, j].Pattern[k] = new CogSearchMaxTool(Main.AlignUnit[TempName].PAT[m_PatTagNo, j].Pattern[k]);
                                Main.AlignUnit[TempName].PAT[i, j].GPattern[k] = new CogPMAlignTool(Main.AlignUnit[TempName].PAT[m_PatTagNo, j].GPattern[k]);
                            }
                            for (int k = 0; k < Main.DEFINE.PATTERNTAG_MAX; k++)
                            {
                                for (int ii = 0; ii < Main.DEFINE.Light_PatMaxCount; ii++)
                                {
                                    for (int a = 0; a < Main.DEFINE.Light_ToolMaxCount; a++)
                                    {
                                        //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_L].m_Light_Name = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_Light_Name;
                                        //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_R].m_Light_Name = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_Light_Name;
                                        //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_L].m_LightCtrl = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightCtrl;
                                        //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_R].m_LightCtrl = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LightCtrl;
                                        //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_L].m_LightCH = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightCH;
                                        //                                     Main.AlignUnit[m_AlignNo].PAT[k, Main.DEFINE.OBJ_R].m_LightCH = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, Main.DEFINE.OBJ_R].m_LightCH;
                                        //                                    Main.AlignUnit[TempName].PAT[k, Main.DEFINE.OBJ_L].m_LightValue[ii, a] = Main.AlignUnit[TempName].PAT[m_PatTagNo, Main.DEFINE.OBJ_L].m_LightValue[ii, a];
                                        Main.AlignUnit[TempName].PAT[k, j].m_LightValue[ii, a] = Main.AlignUnit[TempName].PAT[m_PatTagNo, j].m_LightValue[ii, a];
                                    }
                                }
                            }
                            //----------------------------------------------------------------------------------------------------------------------------------

                            //----------------------------------------------------------------------------------------------------------------------------------
                            //                         for (int jj = 0; jj < Main.DEFINE.Light_PatMaxCount; jj++)
                            //                         {
                            //                             for (int kk = 0; kk < Main.DEFINE.Light_ToolMaxCount; kk++)
                            //                             {
                            //                                 Main.AlignUnit[m_AlignNo].PAT[i, j].m_LightValue[jj, kk] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, j].m_LightValue[jj, kk];
                            //                             }
                            //                         }

                        }
                        Main.AlignUnit[m_AlignNo].Save(i);
                    }

                }
                nPatternCopy = false;
            }
            #endregion

            bROIFinealignTeach = false;
            m_PatNo_Sub = 0;
            CB_SUB_PATTERN.SelectedIndex = 0;
            timer1.Enabled = false;
            DisplayClear();
            if (chkUseRoiTracking.Checked)
            {
                chkUseRoiTracking.Checked = false;
            }
            if (chkUseLoadImageTeachMode.Checked)
            {
                chkUseLoadImageTeachMode.Checked = false;
            }
            tempCaliperNum.Clear();
            iCountClick = 0;
            this.Hide();
        }// BTN_SAVE_Click
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            //shkang_s 파라미터저장시 로그 변수 초기화
            tempCaliperNum.Clear();
            iCountClick = 0;
            //shkang_e
            bLiveStop = false;
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_FinealignMarkScore = m_dTempFinealignMarkscore;
            Main.AlignUnit[m_AlignNo].Load(m_PatTagNo);
            bROIFinealignTeach = false;
            timer1.Enabled = false;
            m_PatNo_Sub = 0;
            CB_SUB_PATTERN.SelectedIndex = 0;
            //BTN_PATTERN_COPY.Visible = false;
            DisplayClear();
            if (chkUseRoiTracking.Checked)
            {
                chkUseRoiTracking.Checked = false;
            }
            if (chkUseLoadImageTeachMode.Checked)
            {
                chkUseLoadImageTeachMode.Checked = false;
            }
            this.Hide();
        }

        #endregion

        #region 패턴 등록 관련
        private void BTN_PATTERN_Click(object sender, EventArgs e)
        {
            m_RetiMode = M_PATTERN;
            BTN_BackColor(sender, e);
            DisplayClear();
            CrossLine();

            //2023 0225 YSH ROI Finealign 
            if (bROIFinealignTeach)
            {
                //FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = PT_Display01.Image;
                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Trained == false)
                {
                    if (PatMaxTrainRegion == null)
                        PatMaxTrainRegion = new CogRectangle(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion as CogRectangle);

                    PatMaxTrainRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 100, 100);
                }

                PatMaxTrainRegion.GraphicDOFEnable = CogRectangleDOFConstants.Position | CogRectangleDOFConstants.Size; //| CogRectangleAffineDOFConstants.Rotation
                PatMaxTrainRegion.Interactive = true;

                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Trained == false)
                {
                    MarkORGPoint.SetCenterRotationSize(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 0, M_ORIGIN_SIZE);
                    ORGSizeFit();
                }
                CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();
                PatternInfo.Add(PatMaxTrainRegion);
                //            PatternInfo.Add(PatMaxORGPoint);
                PatternInfo.Add(MarkORGPoint);
                PT_Display01.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);
            }
            else
            {
                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                {
                    if (PatMaxTrainRegion == null)
                        PatMaxTrainRegion = new CogRectangle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion as CogRectangle);

                    PatMaxTrainRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 100, 100);
                }

                PatMaxTrainRegion.GraphicDOFEnable = CogRectangleDOFConstants.Position | CogRectangleDOFConstants.Size; //| CogRectangleAffineDOFConstants.Rotation
                PatMaxTrainRegion.Interactive = true;


                //             if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                //                 PatMaxORGPoint.SetOriginLengthAspectRotationSkew(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], M_ORIGIN_SIZE, 1, 0, 0);
                // 
                //             PatMaxORGPoint.GraphicDOFEnable = CogCoordinateAxesDOFConstants.Position;
                //             PatMaxORGPoint.Interactive = true;

                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                {
                    MarkORGPoint.SetCenterRotationSize(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 0, M_ORIGIN_SIZE);
                    ORGSizeFit();
                }
                CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();
                PatternInfo.Add(PatMaxTrainRegion);
                //            PatternInfo.Add(PatMaxORGPoint);
                PatternInfo.Add(MarkORGPoint);
                PT_Display01.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);

            }


        }

        private void CNLSearch_DrawOverlay()
        {
            CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();
            PatternInfo.Add(PatMaxTrainRegion);
            //         PatternInfo.Add(PatMaxORGPoint);

            PT_Display01.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);
        }

        private void BTN_BackColor(object sender, EventArgs e)
        {
            nDistanceShow[m_PatNo] = false;
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
            LABEL_MESSAGE(LB_MESSAGE1, "", System.Drawing.Color.Red);

            BTN_BackColor();
            Button TempBTN = (Button)sender;
            TempBTN.BackColor = System.Drawing.Color.LawnGreen;
        }

        private void BTN_BackColor()
        {
            BTN_PATTERN.BackColor = System.Drawing.Color.DarkGray;
            BTN_ORIGIN.BackColor = System.Drawing.Color.DarkGray;
            BTN_PATTERN_SEARCH_SET.BackColor = System.Drawing.Color.DarkGray;
        }

        private void BTN_PATTERN_ORIGIN_Click(object sender, EventArgs e)
        {
            if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            {
                //                 if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                //                     PatMaxORGPoint.SetOriginLengthAspectRotationSkew(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], M_ORIGIN_SIZE, 1, 0, 0);
                //                 PatMaxORGPoint.OriginX = PatMaxTrainRegion.CenterX;
                //                 PatMaxORGPoint.OriginY = PatMaxTrainRegion.CenterY;



                if (bROIFinealignTeach)
                {
                    if (FinealignMark[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                    {
                        MarkORGPoint.SetCenterRotationSize(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 0, M_ORIGIN_SIZE);
                        ORGSizeFit();
                    }
                }
                else
                {
                    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                    {
                        MarkORGPoint.SetCenterRotationSize(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 0, M_ORIGIN_SIZE);
                        ORGSizeFit();
                    }
                }


                MarkORGPoint.X = PatMaxTrainRegion.CenterX;
                MarkORGPoint.Y = PatMaxTrainRegion.CenterY;
            }

        }
        private void BTN_ORIGIN_Click(object sender, EventArgs e)
        {
            m_RetiMode = M_ORIGIN;
            //    PatMaxORGPoint.DisplayedXAxisLength = 50 * PT_Display01.Zoom;
            BTN_BackColor(sender, e);
        }
        private void BTN_PATTERN_SEARCH_SET_Click(object sender, EventArgs e)
        {
            m_RetiMode = M_SEARCH;
            BTN_BackColor(sender, e);
            DisplayClear();


            if (bROIFinealignTeach)
            {
                if (FinealignMark[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                    PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
            }
            else
            {
                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained == false)
                    PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
            }

            PatMaxSearchRegion.GraphicDOFEnable = CogRectangleDOFConstants.Position | CogRectangleDOFConstants.Size;
            PatMaxSearchRegion.Color = CogColorConstants.Orange;
            PatMaxSearchRegion.Interactive = true;

            CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();

            PatternInfo.Add(PatMaxSearchRegion);
            PT_Display01.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);

            DisplayFit(PT_Display01);


        }
        private void BTN_PATTERN_SEARCH_SET_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion = null;
                PatMaxSearchRegion = new CogRectangle();
                PatMaxSearchRegion.SetCenterWidthHeight(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], Main.vision.IMAGE_SIZE_X[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA, Main.vision.IMAGE_SIZE_Y[m_CamNo] - Main.DEFINE.DEFAULT_SEARCH_AREA);
                BTN_PATTERN_SEARCH_SET_Click(sender, null);
            }
        }
        private static void DisplayFit(CogRecordDisplay Display)
        {
            // Display.Fit(false);
            //   Display.AutoFitWithGraphics = false;
            Display.AutoFitWithGraphics = true;
            Display.Fit(true);
        }
        private CogImage8Grey CopyIMG(ICogImage IMG)
        {
            if (IMG == null)
                return new CogImage8Grey();

            CogImage8Grey returnIMG;

            returnIMG = new CogImage8Grey(IMG as CogImage8Grey);
            return returnIMG;


        }
        private void BTN_APPLY_Click(object sender, EventArgs e)
        {
            //             if ((!Main.machine.EngineerMode) && m_PatNo_Sub == 0)
            //             {
            //                 MessageBox.Show("\tNot Engineer Mode!\n   You Can Setting Only SubPattern.");
            //                 return;
            //             }
            try
            {
                if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
                {
                    if (bROIFinealignTeach)
                    {
                        if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
                        {
                            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImageMask = null;
                            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImageMaskOffsetX = 0;
                            FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImageMaskOffsetY = 0;
                        }

                        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainImage = PT_Display01.Image;
                        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle(PatMaxTrainRegion);

                        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationX = MarkORGPoint.X;
                        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Origin.TranslationY = MarkORGPoint.Y;

                        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Train();

                        DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], FinealignMark[nROIFineAlignIndex, m_PatNo_Sub]);
                        LABEL_MESSAGE(LB_MESSAGE, "Train OK", System.Drawing.Color.Lime);
                    }
                    else
                    {
                        if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
                        {
                            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMask = null;
                            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetX = 0;
                            PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetY = 0;

                            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMask = null;
                            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetX = 0;
                            PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImageMaskOffsetY = 0;
                        }

                        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        //PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangleAffine(PatMaxTrainRegion);
                        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle(PatMaxTrainRegion);



                        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX = MarkORGPoint.X;
                        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY = MarkORGPoint.Y;

                        PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Train();

                        DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], PT_Pattern[m_PatNo, m_PatNo_Sub]);
                        LABEL_MESSAGE(LB_MESSAGE, "Train OK", System.Drawing.Color.Lime);

                        PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        //      PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangleAffine(PatMaxTrainRegion);
                        PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle(PatMaxTrainRegion);
                        PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX = MarkORGPoint.X;
                        PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY = MarkORGPoint.Y;
                        PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.Train();
                    }


                }
                if (m_RetiMode == M_SEARCH)
                {
                    if (bROIFinealignTeach)
                    {
                        FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].SearchRegion = new CogRectangle(PatMaxSearchRegion);
                    }
                    else
                    {
                        PT_Pattern[m_PatNo, m_PatNo_Sub].SearchRegion = new CogRectangle(PatMaxSearchRegion);
                        PT_GPattern[m_PatNo, m_PatNo_Sub].SearchRegion = new CogRectangle(PatMaxSearchRegion);
                    }
                }
            }
            catch (System.Exception ex)
            {
                LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            }

        }
        private void BTN_PATTERN_DELETE_Click(object sender, EventArgs e)
        {
            //             if ((!Main.machine.EngineerMode) && m_PatNo_Sub == 0)
            //             {
            //                 MessageBox.Show("\tNot Engineer Mode!\n   You Can Setting Only SubPattern.");
            //                 return;
            //             }
            DialogResult result = MessageBox.Show("Do you want to Delete Pattern Number: " + CB_SUB_PATTERN.Text + " ?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                if (bROIFinealignTeach)
                {
                    FinealignMark[m_PatNo, m_PatNo_Sub].Pattern = new CogSearchMaxPattern();
                    FinealignMark[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
                    DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], FinealignMark[m_PatNo, m_PatNo_Sub]);
                }
                else
                {
                    PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern = new CogSearchMaxPattern();
                    PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainRegion = new CogRectangle();
                    PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern = new CogPMAlignPattern();
                    //        DrawTrainedPattern(PT_SubDisplay_00, PT_Pattern[m_PatNo, m_PatNo_Sub]);
                    DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], PT_Pattern[m_PatNo, m_PatNo_Sub]);
                }

            }
        }
        private void CB_SUB_PATTERN_SelectionChangeCommitted(object sender, EventArgs e)
        {
            m_PatNo_Sub = CB_SUB_PATTERN.SelectedIndex;
            if (m_PatNo_Sub == 0)
                BTN_MAINORIGIN_COPY.Visible = false;
            else
                BTN_MAINORIGIN_COPY.Visible = true;
            Pattern_Change();
        }
        public static void DrawTrainedPattern(CogRecordDisplay Display, CogSearchMaxTool TempPMAlignTool)
        {
            Main.DisplayClear(Display);

            CogSearchMaxTool PMAlignTool = new CogSearchMaxTool(TempPMAlignTool);
            if (PMAlignTool.Pattern.Trained)
            {
                Display.Image = PMAlignTool.Pattern.GetTrainedPatternImage();

                //VisionPro 9.4 Ver 이하
                //PMAlignTool.Pattern.TrainImageMaskOffsetX = 0;
                //PMAlignTool.Pattern.TrainImageMaskOffsetY = 0;
                //PMAlignTool.CurrentRecordEnable = CogSearchMaxCurrentRecordConstants.TrainImage | CogSearchMaxCurrentRecordConstants.TrainImageMask;
                //Display.Record = PMAlignTool.CreateCurrentRecord();

                CogRectangle TrainRegion = new CogRectangle(PMAlignTool.Pattern.TrainRegion as CogRectangle);
                TrainRegion.GraphicDOFEnable = CogRectangleDOFConstants.Position;
                TrainRegion.Interactive = false;

                CogCoordinateAxes ORGPoint = new CogCoordinateAxes();
                ORGPoint.LineStyle = CogGraphicLineStyleConstants.Dot;
                ORGPoint.Transform.TranslationX = PMAlignTool.Pattern.Origin.TranslationX;
                ORGPoint.Transform.TranslationY = PMAlignTool.Pattern.Origin.TranslationY;
                ORGPoint.GraphicDOFEnable = CogCoordinateAxesDOFConstants.Position;

                CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();
                //VisionPro 9.5 Ver 이상
                if (PMAlignTool.Pattern.GetTrainedPatternImageMask() != null) PatternInfo.Add(CreateMaskGraphic(PMAlignTool.Pattern.TrainImage.SelectedSpaceName, PMAlignTool.Pattern.GetTrainedPatternImageMask()));
                PatternInfo.Add(TrainRegion);
                PatternInfo.Add(ORGPoint);

                Display.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);

                DisplayFit(Display);
            }
            else
            {
                Display.Image = null;
            }
        }
        private static CogMaskGraphic CreateMaskGraphic(string SelectedSpaceName, CogImage8Grey mask)
        {
            CogMaskGraphic cogMaskGraphic = new CogMaskGraphic();
            for (short index = 0; index < (short)256; ++index)
            {
                CogColorConstants cogColorConstants;
                CogMaskGraphicTransparencyConstants transparencyConstants;
                if (index < (short)64)
                {
                    cogColorConstants = CogColorConstants.DarkRed;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.Half;
                }
                else if (index < (short)128)
                {
                    cogColorConstants = CogColorConstants.Yellow;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.Half;
                }
                else if (index < (short)192)
                {
                    cogColorConstants = CogColorConstants.Red;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.None;
                }
                else
                {
                    cogColorConstants = CogColorConstants.Yellow;
                    transparencyConstants = CogMaskGraphicTransparencyConstants.Full;
                }
                cogMaskGraphic.SetColorMap((byte)index, cogColorConstants);
                cogMaskGraphic.SetTransparencyMap((byte)index, transparencyConstants);
            }
            cogMaskGraphic.Image = mask;
            cogMaskGraphic.Color = CogColorConstants.None;
            if (SelectedSpaceName == "#")
            {
                ((ICogGraphic)cogMaskGraphic).SelectedSpaceName = "_TrainImage#";
            }
            return cogMaskGraphic;
        }
        private void CB_SUBPAT_USE_CheckedChanged(object sender, EventArgs e)
        {
            //  CB_SUBPAT_USE
            if (CB_SUBPAT_USE.Checked)
            {
                PT_Pattern_USE[m_PatNo, m_PatNo_Sub] = true;
                CB_SUBPAT_USE.BackColor = System.Drawing.Color.LawnGreen;

            }
            else
            {
                PT_Pattern_USE[m_PatNo, m_PatNo_Sub] = false;
                CB_SUBPAT_USE.BackColor = System.Drawing.Color.DarkGray;

            }
            SUBPATTERN_LABELDISPLAY(CB_SUBPAT_USE.Checked, m_PatNo_Sub);
        }
        private void SUBPATTERN_LABELDISPLAY(bool nUSE, int nPatSubNo)
        {
            if (nUSE)
            {
                LB_PATTERN[nPatSubNo].BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                LB_PATTERN[nPatSubNo].BackColor = System.Drawing.Color.WhiteSmoke;
            }
        }
        private void NUD_PAT_SCORE_ValueChanged(object sender, EventArgs e)
        {
            if (bROIFinealignTeach)
            {
                //ROIFinealign 함수 동작 시 AlignUnit에 있는 변수로 동작하여 백업변수 필요.
                dFinealignMarkScore = (double)NUD_PAT_SCORE.Value;
                m_dTempFinealignMarkscore = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_FinealignMarkScore;
                //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_FinealignMarkScore = dFinealignMarkScore;
            }
            else
            {
                PT_AcceptScore[m_PatNo] = (double)NUD_PAT_SCORE.Value;
            }
            //             if (!(Main.DEFINE.PROGRAM_TYPE == "FOF_PC5" && Main.AlignUnit[m_AlignNo].m_AlignName == "AOI_INSPECTION"))
            //             {
            //                 if ((!Main.machine.EngineerMode) && (double)NUD_PAT_SCORE.Value <= 0.75)
            //                 {
            //                     PT_AcceptScore[m_PatNo] = 0.75;
            //                 }
            //             }

        }
        private void NUD_PAT_GSCORE_ValueChanged(object sender, EventArgs e)
        {
            PT_GAcceptScore[m_PatNo] = (double)NUD_PAT_GSCORE.Value;
        }
        #endregion
        private void BTN_PATTERN_RUN_Click(object sender, EventArgs e)
        {
            try
            {
                //if (bLiveStop != true)
                //    RefreshTeach(); //-> 확인후 삭제 할것. 
                LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
                m_Timer.StartTimer();
                CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
                DisplayClear();
                List_NG.Items.Clear();
                switch (Convert.ToInt32(TABC_MANU.SelectedTab.Tag))
                {
                    case Main.DEFINE.M_CNLSEARCHTOOL: //CogCNLSearch
                        #region CNLSEARCH
                        Save_SystemLog("Mark Search Start", Main.DEFINE.CMD);
                        lock (mlock)
                        {
                            Search_PATCNL();
                        }
                        #endregion
                        break;

                    case Main.DEFINE.M_BLOBTOOL: //CogBLOBTOOL
                        #region BLOBTOOL
                        RefreshDisplay2();
                        if (ThresValue_Sts)
                            Search_BLOB(false);
                        else
                            Search_BLOB(true);
                        #endregion
                        break;

                    case Main.DEFINE.M_CALIPERTOOL: //CogCALIPERTOOL
                        #region CALIPERTOOL
                        RefreshDisplay2();
                        if (ThresValue_Sts)
                            Search_Caliper(false);
                        else
                            Search_Caliper(true);
                        #endregion
                        break;

                    case Main.DEFINE.M_FINDLINETOOL: //CogFINDLineTOOL
                        #region COGFINDLine
                        RefreshDisplay2();
                        if (ThresValue_Sts)
                        {
                            Search_FindLine(false);
                            Search_Circle(false);
                        }
                        else
                        {
                            Search_FindLine(true);
                            Search_Circle(true);
                        }
                        #endregion
                        break;

                    case Main.DEFINE.M_FINDCIRCLETOOL:
                        #region CIRCLETOOL
                        RefreshDisplay2();
                        if (ThresValue_Sts)
                        {
                            Search_FindLine(false);
                            Search_Circle(false);
                        }
                        else
                        {
                            Search_FindLine(true);
                            Search_Circle(true);
                        }
                        #endregion
                        break;
                    case Main.DEFINE.M_INSPECTION:
                        #region INSPECITON
                        if (m_enumROIType == enumROIType.Line)
                        {
                            Test_FindLine();
                        }
                        else
                        {
                            Test_FindCricle();
                        }
                        #endregion
                        break;
                    case Main.DEFINE.M_ALIGNINPECTION:
                        Test_TrackingLine();
                        break;
                }
                Lab_Tact.Text = m_Timer.GetElapsedTime().ToString() + " ms";
                //    if (BTN_DISNAME_01.BackColor.Name != "SkyBlue") CrossLine();
                if (PT_DISPLAY_CONTROL.CrossLineChecked) CrossLine();
            }//try
            catch (System.Exception ex)
            {
                LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            }


        }

        private void Test_FindLine()
        {
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            double[] Result;
            int ignore;
            bool bRet = false;
            //2023 0615 YSH
            //티칭창에서 단일 Search시엔, Searck 방식 플래그에 따라 동작하게끔 함
            //단일 티칭 확인 가능용도

            if (m_bInspDirectionChange)
                GaloDirectionConvertInspection(0, (int)enumROIType.Line, m_TempFindLineTool, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore);
            else
                GaloOppositeInspection(m_iGridIndex, (int)enumROIType.Line, m_TempFindLineTool, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore);

            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //
            ResultGride(Result);
            //m_TempFindLineTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //m_TempFindLineTool.Run();
            //double[,] ResultData;
            //double[] ResultCalDistanceX;
            //double[] ResultCalDistanceY;
            //if (m_TempFindLineTool.Results != null || m_TempFindLineTool.Results.Count > 0)
            //{
            //    ResultData = new double[m_TempFindLineTool.Results.Count, 4];
            //    for (int i = 0; i < m_TempFindLineTool.Results.Count; i++)
            //    {
            //        if (m_TempFindLineTool.Results[i].Found == true)
            //        {
            //            ResultData[i, 0] = m_TempFindLineTool.Results[i].CaliperResults[0].Edge0.PositionX;
            //            ResultData[i, 1] = m_TempFindLineTool.Results[i].CaliperResults[0].Edge0.PositionY;
            //            ResultData[i, 2] = m_TempFindLineTool.Results[i].CaliperResults[0].Edge1.PositionX;
            //            ResultData[i, 3] = m_TempFindLineTool.Results[i].CaliperResults[0].Edge1.PositionY;
            //            //resultGraphics.Add(m_TempFindLineTool.Results[i].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge | CogFindLineResultGraphicConstants.DataPoint));
            //        }
            //        else
            //        {
            //            ResultData[i, 0] = 0;
            //            ResultData[i, 1] = 0;
            //            ResultData[i, 2] = 0;
            //            ResultData[i, 3] = 0;
            //        }
            //    }

            //    //shkangs_s
            //    ResultCalDistanceX = new double[m_TempFindLineTool.Results.Count - 1];
            //    ResultCalDistanceY = new double[m_TempFindLineTool.Results.Count - 1];
            //    double dAvgValueX = 0;
            //    double dAvgValueY = 0;
            //    double dSpecX = 15;     //버튼으로 바꿀수 있도록
            //    double dSpecY = 15;     //버튼으로 바꿀수 있도록
            //    for (int i = 0; i < m_TempFindLineTool.Results.Count - 1; i++) 
            //    {
            //        ResultCalDistanceX[i] = Math.Abs(((ResultData[i + 1, 0] + ResultData[i + 1, 2]) / 2) - ((ResultData[i, 0] + ResultData[i, 2]) / 2));
            //        ResultCalDistanceY[i] = Math.Abs(((ResultData[i + 1, 1] + ResultData[i + 1, 3]) / 2) - ((ResultData[i, 1] + ResultData[i, 3]) / 2));
            //        dAvgValueX = dAvgValueX + ResultCalDistanceX[i];
            //        dAvgValueY = dAvgValueY + ResultCalDistanceY[i];
            //    }
            //    dAvgValueX = dAvgValueX / ResultCalDistanceX.Count();
            //    dAvgValueY = dAvgValueY / ResultCalDistanceY.Count();
            //    for (int i = 0; i < m_TempFindLineTool.Results.Count - 1; i++)
            //    {
            //        if ((ResultCalDistanceX[i] >= dAvgValueX + dSpecX) || (ResultCalDistanceY[i] >= dAvgValueY + dSpecY))
            //        {
            //            if (ResultData[i, 0] == 0 && ResultData[i, 1] == 0 && ResultData[i, 2] == 0 && ResultData[i, 3] == 0)
            //            {
            //                resultGraphics.Add(m_TempFindLineTool.Results[i-1].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge | CogFindLineResultGraphicConstants.DataPoint));
            //            }
            //            else
            //            {
            //                ResultData[i + 1, 0] = 0;
            //                ResultData[i + 1, 1] = 0;
            //                ResultData[i + 1, 2] = 0;
            //                ResultData[i + 1, 3] = 0;
            //            }
            //        }
            //        else
            //        {
            //            resultGraphics.Add(m_TempFindLineTool.Results[i].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge | CogFindLineResultGraphicConstants.DataPoint));
            //        }
            //    }
            //    //shkang_e
            //    PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //    ResultGride(ResultData);
            //}
        }
        private void Test_TrackingLine()
        {
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            m_TempTrackingLine.InputImage = (CogImage8Grey)PT_Display01.Image;
            m_TempTrackingLine.Run();
            //double[,] ResultData;
            if (m_TempTrackingLine.Results != null || m_TempTrackingLine.Results.Count > 0)
            {
                resultGraphics.Add(m_TempTrackingLine.Results.GetLine());
                PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);

            }
        }
        private void Test_FindCricle()
        {
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            double[] Result;
            int ignore;
            GaloOppositeInspection(m_iGridIndex, (int)enumROIType.Circle, m_TempFindCircleTool, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore);
            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);

            ResultGride(Result);
            //CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //m_TempFindCircleTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            //m_TempFindCircleTool.Run();
            //double[,] ResultData;
            //if (m_TempFindCircleTool.Results != null || m_TempFindCircleTool.Results.Count > 0)
            //{
            //    ResultData = new double[m_TempFindCircleTool.Results.Count, 4];
            //    for (int i = 0; i < m_TempFindCircleTool.Results.Count; i++)
            //    {
            //        if (m_TempFindCircleTool.Results[i].CaliperResults.Count != 0)
            //        {
            //            ResultData[i, 0] = m_TempFindCircleTool.Results[i].CaliperResults[0].Edge0.PositionX;
            //            ResultData[i, 1] = m_TempFindCircleTool.Results[i].CaliperResults[0].Edge0.PositionY;

            //            ResultData[i, 2] = m_TempFindCircleTool.Results[i].CaliperResults[0].Edge1.PositionX;
            //            ResultData[i, 3] = m_TempFindCircleTool.Results[i].CaliperResults[0].Edge1.PositionY;
            //        }
            //        resultGraphics.Add(m_TempFindCircleTool.Results[i].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge | CogFindCircleResultGraphicConstants.DataPoint));
            //    }
            //    PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //    ResultGride(ResultData);
            //}
        }
        private void ResultGride(double[] ResulteData)
        {
            //double PixelResolution = 13.36;
            if (ResulteData != null)
            {
                dataGridView_Result.Rows.Clear();
                string[] strResultData = new string[7];
                for (int i = 0; i < ResulteData.GetLength(0); i++)
                {
                    strResultData[0] = i.ToString();
                    strResultData[1] = "0";

                    double dDist = ResulteData[i];
                    strResultData[3] = string.Format("{0:F3}", dDist/** PixelResolution/1000*/);
                    dataGridView_Result.Rows.Add(strResultData);
                }
            }
        }
        private void TABC_MANU_Selecting(object sender, TabControlCancelEventArgs e)
        {
            switch (TABC_MANU.SelectedIndex)
            {
                case Main.DEFINE.M_CNLSEARCHTOOL:
                    switch (Main.AlignUnit[m_AlignNo].m_AlignName)
                    {
                        case "1st PREALIGN":
                            TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDLINETOOL;
                            break;
                        default:
                            //TABC_MANU.SelectedIndex = Main.DEFINE.M_CNLSEARCHTOOL;
                            break;
                    }
                    break;

                case Main.DEFINE.M_BLOBTOOL:
                    switch (Main.AlignUnit[m_AlignNo].m_AlignName)
                    {
                        case "IC_TRAY":
                            break;
                        case "ACF_BLOB":
                            break;
                        case "FOF_ACF_PRE":
                            break;
                        case "FOP_ACF_PRE":
                            break;
                        case "SCANNER HEAD CAM1":
                        case "ALIGN INSP CAM2":
                        case "ALIGN INSP CAM3":
                        case "ALIGN INSP CAM4":
                        case "1st PREALIGN":
                            TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDLINETOOL;
                            break;
                        default:
                            //TABC_MANU.SelectedIndex = Main.DEFINE.M_CNLSEARCHTOOL;
                            break;
                    }
                    break;

                case Main.DEFINE.M_CALIPERTOOL:
                    switch (Main.AlignUnit[m_AlignNo].m_AlignName)
                    {
                        case "SCANNER HEAD CAM1":
                        case "ALIGN INSP CAM2":
                        case "ALIGN INSP CAM3":
                        case "ALIGN INSP CAM4":
                            break;
                        case "1st PREALIGN":
                            TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDLINETOOL;
                            break;
                        default:
                            //TABC_MANU.SelectedIndex = Main.DEFINE.M_CALIPERTOOL;
                            break;
                    }
                    break;

                case Main.DEFINE.M_FINDCIRCLETOOL:
                    switch (Main.AlignUnit[m_AlignNo].m_AlignName)
                    {
                        case "1st PREALIGN":
                            //TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDLINETOOL;
                            break;
                        case "2nd PREALIGN":
                            //TABC_MANU.SelectedIndex = Main.DEFINE.M_CNLSEARCHTOOL;
                            break;
                    }
                    break;

                default:
                    break;
            }
        }
        private void TABC_MANU_SelectedIndexChanged(object sender, EventArgs e)
        {
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            LABEL_MESSAGE(LB_MESSAGE1, "", System.Drawing.Color.Lime);

            M_TOOL_MODE = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            if (bROIFinealignTeach)
            {
                if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) != 0)
                {
                    TABC_MANU.SelectedIndex = 0;
                    return;
                }
            }
            if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 6 || Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
            {
                if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
                    btn_Inspection_Test.Visible = true;
                else
                    btn_Inspection_Test.Visible = false;

                if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
                {
                    UpdateParamUI();
                    _useROITracking = false;
                    chkUseRoiTracking.Checked = _useROITracking;
                    _eTabSelect = enumTabSelect.Insp;
                }
                else
                {
                    _eTabSelect = enumTabSelect.ThetaOrigin;
                    //2023 0223 YSH 창 진입시 자재얼라인 패널 Show
                    RDB_ROI_FINEALIGN.PerformClick();
                }
                m_enumAlignROI = enumAlignROI.Left1_1;
                btn_TOP_Inscription.BackColor = Color.Green;
                btn_Top_Circumcription.BackColor = Color.DarkGray;
                btn_Bottom_Inscription.BackColor = Color.DarkGray;
                btn_Bottom_Circumcription.BackColor = Color.DarkGray;
                for (int i = 0; i < 4; i++)
                {
                    if (i < 2)
                    {
                        LeftOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].LeftOrigin[i];
                        RightOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].RightOrigin[i];
                    }
                    m_TeachLine[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i];
                    if (m_TeachLine[i] == null)
                        m_TeachLine[i] = new CogFindLineTool();
                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] == null)
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] = new CogFindLineTool();
                    //Bonding Align
                    m_TeachAlignLine[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i];
                    if (m_TeachAlignLine[i] == null)
                        m_TeachAlignLine[i] = new CogCaliperTool();
                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i] == null)
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_BondingAlignLine[i] = new CogCaliperTool();
                }
                lab_LeftOriginX.Text = LeftOrigin[0].ToString("F3");
                lab_LeftOriginY.Text = LeftOrigin[1].ToString("F3");
                lab_RightOriginX.Text = RightOrigin[0].ToString("F3");
                lab_RightOriginY.Text = RightOrigin[1].ToString("F3");
                m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_1];
                lblOkDistanceValueX.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceX);
                lblOkDistanceValueY.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceY);
                lblAlignSpecValueX.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecX);
                lblAlignSpecValueY.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecY);
                dBondingAlignOriginDistX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceX;
                dBondingAlignOriginDistY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dOriginDistanceY;
                dBondingAlignDistSpecX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecX;
                dBondingAlignDistSpecY = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dDistanceSpecY;
                lblObjectDistanceXValue.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceX);
                lblObjectDistanceXSpecValue.Text = Convert.ToString(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceSpecX);
                dObjectDistanceSpecX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceSpecX;
                dObjectDistanceX = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_dObjectDistanceX;


                if (OriginImage != null)
                    PT_Display01.Image = OriginImage;
                Get_FindConerParameter();
            }
            if (M_TOOL_MODE > 0 && M_TOOL_MODE < 5)
            {
                TABC_MANU.SelectedIndex = 5;
                return;
            }
            else if (m_PatNo == 1 && M_TOOL_MODE == 5)
            {
                TABC_MANU.SelectedIndex = 6;
            }

            if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5 || Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 6)
                M_TOOL_MODE = 0;
            if (bROICopy)
                for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = false;
            else
                for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = true;

            BTN_TOOLSET[M_TOOL_MODE].Visible = true;
            //if (M_TOOL_MODE == Main.DEFINE.M_CNLSEARCHTOOL) BTN_TOOLSET[Main.DEFINE.M_PMALIGNTOOL].Visible = true;

            Light_Select();
            LightCheck(M_TOOL_MODE);
            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(M_TOOL_MODE);
            DisplayClear();
            nDistanceShow[m_PatNo] = false;

            m_TABCHANGE_MODE = true;
            switch (M_TOOL_MODE)
            {
                case Main.DEFINE.M_CNLSEARCHTOOL:
                    if (bROIFinealignTeach)
                        BTN_RETURNPAGE.Visible = true;
                    else
                        BTN_RETURNPAGE.Visible = false;
                    Pattern_Change();
                    break;
                case Main.DEFINE.M_BLOBTOOL:
                    CB_BLOB_MARK_USE.Checked = PT_Blob_MarkUSE[m_PatNo];
                    CB_BLOB_CALIPER_USE.Checked = PT_Blob_CaliperUSE[m_PatNo];
                    m_SelectBlob = 0;
                    CB_BLOB_COUNT.SelectedIndex = 0;
                    Inspect_Cnt.Value = PT_Blob_InspCnt[m_PatNo];
                    Blob_Change();
                    break;

                case Main.DEFINE.M_CALIPERTOOL:
                    CB_CALIPER_MARK_USE.Checked = PT_Caliper_MarkUSE[m_PatNo];
                    RBTN_CALIPER00.Checked = true;
                    m_SelectCaliper = 0;
                    Caliper_Change();
                    break;

                case Main.DEFINE.M_FINDLINETOOL:
                    CB_FINDLINE_MARK_USE.Checked = PT_FindLine_MarkUSE[m_PatNo];
                    RBTN_FINDLINE00.Checked = true;
                    m_SelectFindLine = 0;
                    FINDLINE_Change();
                    break;

                case Main.DEFINE.M_FINDCIRCLETOOL:
                    CB_CIRCLE_MARK_USE.Checked = PT_Circle_MarkUSE[m_PatNo];
                    RBTN_CIRCLE00.Checked = true;
                    m_SelectCircle = 0;
                    Circle_Change();
                    break;
            }
            m_TABCHANGE_MODE = false;
        }
        private void RefreshDisplay2()
        {
            try
            {
                CogImage8Grey nTempImage = new CogImage8Grey();
                nTempImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                bool TargetPosUse = false;
                switch (M_TOOL_MODE)
                {
                    case Main.DEFINE.M_BLOBTOOL:
                    case Main.DEFINE.M_CALIPERTOOL:
                    case Main.DEFINE.M_FINDLINETOOL:
                    case Main.DEFINE.M_FINDCIRCLETOOL:
                        if ((PT_Caliper_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_CALIPERTOOL)
                            || (PT_Blob_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
                            || (PT_FindLine_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_FINDLINETOOL)
                            || (PT_Circle_MarkUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_FINDCIRCLETOOL))
                        {
                            TargetPosUse = true;
                            LightCheck(Main.DEFINE.M_LIGHT_CNL);
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
                            Main.SearchDelay(100);
                            if (!Search_PATCNL())
                            {
                                PatResult.TranslationX = 0;
                                PatResult.TranslationY = 0;
                                return;
                            }
                        }
                        else if ((PT_Blob_CaliperUSE[m_PatNo] && M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL))
                        {
                            TargetPosUse = true;
                            LightCheck(Main.DEFINE.M_LIGHT_CALIPER);
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(Main.DEFINE.M_LIGHT_CALIPER);
                            Main.SearchDelay(100);

                            if (!Search_Caliper(true))
                            {
                                PatResult.TranslationX = 0;
                                PatResult.TranslationY = 0;
                                return;
                            }
                        }
                        else
                        {
                            PatResult.TranslationX = 0;
                            PatResult.TranslationY = 0;
                        }
                        if (TargetPosUse)
                        {
                            LightCheck(M_TOOL_MODE);
                            Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].SetAllLight(M_TOOL_MODE);
                            Main.SearchDelay(100);
                            //그랩이 되기전에 다음으로 넘어가기 때문에 넣음.
                            Main.vision.Grab_Flag_End[m_CamNo] = false;
                            Main.vision.Grab_Flag_Start[m_CamNo] = true;

                            while (!Main.vision.Grab_Flag_End[m_CamNo] && !Main.DEFINE.OPEN_F)
                            {
                                Main.SearchDelay(1);
                            }
                            nTempImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        }
                        if (M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
                        {
                            for (int i = 0; i < Main.DEFINE.BLOB_CNT_MAX; i++)
                                PT_BlobTools[m_PatNo, i].InputImage = nTempImage;
                        }
                        if (M_TOOL_MODE == Main.DEFINE.M_CALIPERTOOL)
                        {
                            for (int i = 0; i < Main.DEFINE.CALIPER_MAX; i++)
                                PT_CaliperTools[m_PatNo, i].InputImage = nTempImage;
                        }
                        if (M_TOOL_MODE == Main.DEFINE.M_FINDLINETOOL)
                        {
                            PT_FindLineTool.InputImage = nTempImage;
                            PT_LineMaxTool.InputImage = nTempImage;
                            for (int ii = 0; ii < Main.DEFINE.SUBLINE_MAX; ii++)
                            {
                                for (int i = 0; i < Main.DEFINE.FINDLINE_MAX; i++)
                                {
                                    PT_FindLineTools[m_PatNo, ii, i].InputImage = nTempImage;
                                    PT_LineMaxTools[m_PatNo, ii, i].InputImage = nTempImage;
                                }
                            }

                            // JHKIM 호-직선 연계
                            PT_CircleTool.InputImage = nTempImage;
                        }
                        if (M_TOOL_MODE == Main.DEFINE.M_FINDCIRCLETOOL)
                        {
                            PT_CircleTool.InputImage = nTempImage;
                            for (int i = 0; i < Main.DEFINE.CIRCLE_MAX; i++)
                                PT_CircleTools[m_PatNo, i].InputImage = nTempImage;

                            // JHKIM 호-직선 연계
                            for (int ii = 0; ii < Main.DEFINE.SUBLINE_MAX; ii++)
                            {
                                for (int i = 0; i < Main.DEFINE.FINDLINE_MAX; i++)
                                {
                                    PT_FindLineTools[m_PatNo, ii, i].InputImage = nTempImage;
                                    PT_LineMaxTools[m_PatNo, ii, i].InputImage = nTempImage;
                                }
                            }
                        }
                        break;
                }//switch

            }// try
            catch
            {

            }
        }
        private bool Search_PATCNL()
        {
            bool nRet = false;
            bool nRetSearch_CNL = false;
            bool nRetSearch_PMA = false;

            if (bROIFinealignTeach == true)
            {
                if (bLiveStop == false)
                {
                    Main.vision.Grab_Flag_End[m_CamNo] = false;
                    Main.vision.Grab_Flag_Start[m_CamNo] = true;

                    while (!Main.vision.Grab_Flag_End[m_CamNo] && !Main.DEFINE.OPEN_F)
                    {
                        Main.SearchDelay(1);
                    }

                    FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                }
                else
                {
                    Save_SystemLog("Mark image Load", Main.DEFINE.CMD);
                    FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = PT_Display01.Image;
                }

                FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Run();
                Save_SystemLog("Mark Search start", Main.DEFINE.CMD);
                List<CogCompositeShape> ResultGraphic = new List<CogCompositeShape>();

                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results != null)
                {
                    if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count >= 1) nRetSearch_CNL = true;
                }
                if (nRetSearch_CNL)
                {
                    Save_SystemLog("Mark G1", Main.DEFINE.CMD);
                    if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count >= 1)
                    {
                        if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count >= 1)
                        {
                            for (int j = 0; j < FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results.Count; j++)
                            {
                                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results != null)
                                {
                                    ResultGraphic.Add(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[j].CreateResultGraphics(Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.MatchRegion | Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.Origin));
                                }
                            }
                        }
                        if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score >= dFinealignMarkScore)
                        {
                            LABEL_MESSAGE(LB_MESSAGE, "Mark OK! " + "Score: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Lime);
                        }
                        else
                        {
                            LABEL_MESSAGE(LB_MESSAGE, "Mark NG! " + "Score: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Red);
                        }

                        if (!_useROITracking)
                        {
                            Draw_Label(PT_Display01, "Mark     X: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000"), 1);//
                            Draw_Label(PT_Display01, "Mark     Y: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000"), 2);
                            if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score >= dFinealignMarkScore)
                                Draw_Label(PT_Display01, "Mark     OK! " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
                            else
                                Draw_Label(PT_Display01, "Mark     NG! " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
                        }
                        nRet = true;

                        PatResult.TranslationX = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX;
                        PatResult.TranslationY = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY;

                        string X = "X: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000");
                        string Y = "Y: " + (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE1, X + ", " + Y, System.Drawing.Color.Lime);

                        double tempDataX = 0, tempDataY = 0;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationX, FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Results[0].GetPose().TranslationY,
                                               ref tempDataX, ref tempDataY);

                        string strLog = tempDataX.ToString("0.000") + "," + tempDataY.ToString("0.000");
                        Save_SystemLog(strLog, Main.DEFINE.DATA);
                        Save_SystemLog("Label ", Main.DEFINE.CMD);
                    }

                    for (int i = 0; i < ResultGraphic.Count; i++)
                    {
                        PT_Display01.StaticGraphics.Add(ResultGraphic[i] as ICogGraphic, "Mark");
                    }
                }
                else
                {
                    LABEL_MESSAGE(LB_MESSAGE, "Mark NG! ", System.Drawing.Color.Red);
                    Save_SystemLog("Label NG ", Main.DEFINE.CMD);
                }

                return nRet;
            }
            else
            {
                #region CNLSEARCH
                if (bLiveStop == false)
                {
                    Main.vision.Grab_Flag_End[m_CamNo] = false;
                    Main.vision.Grab_Flag_Start[m_CamNo] = true;

                    while (!Main.vision.Grab_Flag_End[m_CamNo] && !Main.DEFINE.OPEN_F)
                    {
                        Main.SearchDelay(1);
                    }

                    PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                    PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                }
                else
                {
                    Save_SystemLog("Mark image Load", Main.DEFINE.CMD);
                    PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = PT_Display01.Image;
                    PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = PT_Display01.Image;
                }
                PT_Pattern[m_PatNo, m_PatNo_Sub].Run();
                Save_SystemLog("Mark Search start", Main.DEFINE.CMD);
                List<CogCompositeShape> ResultGraphic = new List<CogCompositeShape>();

                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results != null)
                {
                    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1) nRetSearch_CNL = true;
                }
                if (nRetSearch_CNL)
                {
                    Save_SystemLog("Mark G1", Main.DEFINE.CMD);
                    if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1)
                    {

                        if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1)
                        {
                            for (int j = 0; j < PT_Pattern[m_PatNo, m_PatNo_Sub].Results.Count; j++)
                            {
                                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results != null)
                                {
                                    ResultGraphic.Add(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[j].CreateResultGraphics(Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.MatchRegion | Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.Origin));
                                }
                            }
                        }
                        Save_SystemLog("Mark G end", Main.DEFINE.CMD);
                        if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score >= PT_AcceptScore[m_PatNo])
                        {
                            LABEL_MESSAGE(LB_MESSAGE, "Mark OK! " + "Score: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Lime);
                        }
                        else
                        {
                            LABEL_MESSAGE(LB_MESSAGE, "Mark NG! " + "Score: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", System.Drawing.Color.Red);
                        }

                        if (!_useROITracking)
                        {
                            Draw_Label(PT_Display01, "Mark     X: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000"), 1);//
                            Draw_Label(PT_Display01, "Mark     Y: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000"), 2);
                            if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score >= PT_AcceptScore[m_PatNo])
                                Draw_Label(PT_Display01, "Mark     OK! " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
                            else
                                Draw_Label(PT_Display01, "Mark     NG! " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score * 100).ToString("0.000") + "%", 3); // cyh -> 삭제해도됨
                            //shkang_Test_s(게이지 데이터용)
                            //Draw_Label(PT_Display01, "TEST     X: " + ((PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX - (OriginImage.Width / 2)) * 13.36 / 1000).ToString("0.000") + "mm", 4);
                            //Draw_Label(PT_Display01, "TEST     Y: " + ((PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY - (OriginImage.Height / 2)) * 13.36 / 1000).ToString("0.000")+ "mm", 5);
                            //shkang_Test_e
                        }
                        nRet = true;

                        PatResult.TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX;
                        PatResult.TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY;

                        string X = "X: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000");
                        string Y = "Y: " + (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE1, X + ", " + Y, System.Drawing.Color.Lime);

                        double tempDataX = 0, tempDataY = 0;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
                                               ref tempDataX, ref tempDataY);

                        string strLog = tempDataX.ToString("0.000") + "," + tempDataY.ToString("0.000");
                        Save_SystemLog(strLog, Main.DEFINE.DATA);
                        Save_SystemLog("Label ", Main.DEFINE.CMD);
                    }
                }
                else
                {
                    LABEL_MESSAGE(LB_MESSAGE, "Mark NG! ", System.Drawing.Color.Red);
                    Save_SystemLog("Label NG ", Main.DEFINE.CMD);
                }

                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_PMAlign_Use)
                {
                    PT_GPattern[m_PatNo, m_PatNo_Sub].Run();
                    //      PT_Display01.Record = PT_GPattern[m_PatNo, m_PatNo_Sub].CreateLastRunRecord();

                    if (PT_GPattern[m_PatNo, m_PatNo_Sub].Results != null)
                    {
                        if (PT_GPattern[m_PatNo, m_PatNo_Sub].Results.Count >= 1) nRetSearch_PMA = true;
                    }
                    if (nRetSearch_PMA)
                    {
                        Save_SystemLog("Graphy add ", Main.DEFINE.CMD);
                        ResultGraphic.Add(PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion | CogPMAlignResultGraphicConstants.MatchFeatures | CogPMAlignResultGraphicConstants.Origin));

                        if (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].Score >= PT_GAcceptScore[m_PatNo])
                        {
                            LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + "\n" + "GMark OK! " + "Score: " + PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].Score.ToString("0.000") + "%", System.Drawing.Color.Lime);
                        }
                        else
                        {
                            LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + "\n" + "GMark NG! " + "Score: " + PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].Score.ToString("0.000") + "%", System.Drawing.Color.Red);
                        }

                        Draw_Label(PT_Display01, "GMark  X: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000"), 3);
                        Draw_Label(PT_Display01, "GMark  Y: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000"), 4);

                        string X = "G X: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX).ToString("0.000");
                        string Y = "Y: " + (PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY).ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE1, LB_MESSAGE1.Text + "\n" + X + ", " + Y, System.Drawing.Color.Lime);

                        double tempDataX = 0, tempDataY = 0, tempDataX2 = 0, tempDataY2 = 0;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
                                               ref tempDataX, ref tempDataY);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_GPattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
                                               ref tempDataX2, ref tempDataY2);

                        string strLog = tempDataX.ToString("0.000") + "," + tempDataY.ToString("0.000") + "," + tempDataX2.ToString("0.000") + "," + tempDataY2.ToString("0.000");
                        Save_SystemLog(strLog, Main.DEFINE.DATA);
                    }
                    else
                    {
                        LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + "\n" + "GMark NG! ", System.Drawing.Color.Red);
                    }
                }
                for (int i = 0; i < ResultGraphic.Count; i++)
                {
                    PT_Display01.StaticGraphics.Add(ResultGraphic[i] as ICogGraphic, "Mark");
                }
                Save_SystemLog("Mark Search end", Main.DEFINE.CMD);
                ////////////////수정할것 
                //       DisplayFit(PT_Display01);
                return nRet;
                #endregion
            }

        }
        public CogBlobTool BlobToolPairRun(CogBlobTool nSourceTool, int nDirection, out int nPlusMinus)
        {
            int TempPlusMinus = 1; ;
            CogBlobTool nCopyTool = new CogBlobTool();
            try
            {
                nCopyTool = new CogBlobTool(nSourceTool);


                CogRectangle nTempRect = new CogRectangle();
                nTempRect.SetCenterWidthHeight(((CogRectangleAffine)nCopyTool.Region).CenterX, ((CogRectangleAffine)nCopyTool.Region).CenterY, ((CogRectangleAffine)nCopyTool.Region).SideXLength, ((CogRectangleAffine)nCopyTool.Region).SideYLength);

                CogRectangleAffine nBackUpRectAffine = new CogRectangleAffine((CogRectangleAffine)nCopyTool.Region);

                CogRectangle nBackUpRect = new CogRectangle(nTempRect);
                CogRectangle nSearchRect = new CogRectangle(nTempRect);
                // CogRectangle nBackUpRect = new CogRectangle((CogRectangle)nCopyTool.Region);
                // CogRectangle nSearchRect = new CogRectangle((CogRectangle)nCopyTool.Region);

                if (nDirection == Main.DEFINE.HEIGHT)
                { //-----------------------------------------------------------------------------------------+++++++++                        
                    nSearchRect.SetCenterWidthHeight(nBackUpRect.CenterX, nBackUpRect.CenterY - (nBackUpRect.Height / 4), nBackUpRect.Width, nBackUpRect.Height / 2);
                    nCopyTool.Region = new CogRectangle(nSearchRect);
                    nCopyTool.Run();
                    TempPlusMinus = -1;
                    //-----------------------------------------------------------------------------------------
                    if (nCopyTool.Results == null || nCopyTool.Results.GetBlobs().Count <= 0)
                    {
                        nSearchRect.SetCenterWidthHeight(nBackUpRect.CenterX, nBackUpRect.CenterY + (nBackUpRect.Height / 4), nBackUpRect.Width, nBackUpRect.Height / 2);
                        nCopyTool.Region = new CogRectangle(nSearchRect);
                        nCopyTool.Run();
                        TempPlusMinus = 1;
                    }
                }
                else //(nDirection == Main.DEFINE.WIDTH_)
                {
                    { //-----------------------------------------------------------------------------------------+++++++++                             
                        nSearchRect.SetCenterWidthHeight(nBackUpRect.CenterX - (nBackUpRect.Width / 4), nBackUpRect.CenterY, nBackUpRect.Width / 2, nBackUpRect.Height);
                        nCopyTool.Region = new CogRectangle(nSearchRect);
                        nCopyTool.Run();
                        TempPlusMinus = 1;
                        //-----------------------------------------------------------------------------------------
                        if (nCopyTool.Results == null || nCopyTool.Results.GetBlobs().Count <= 0)
                        {
                            nSearchRect.SetCenterWidthHeight(nBackUpRect.CenterX + (nBackUpRect.Width / 4), nBackUpRect.CenterY, nBackUpRect.Width / 2, nBackUpRect.Height);
                            nCopyTool.Region = new CogRectangle(nSearchRect);
                            nCopyTool.Run();
                            TempPlusMinus = -1;
                        }
                    }
                }
                if (nCopyTool.Results != null)
                {
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[1].BlobToolResult = new CogBlobResults(PT_BlobTools[m_PatNo, 1].Results);
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[1].Pixel[Main.DEFINE.X] = 0;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[1].Pixel[Main.DEFINE.Y] = 0;

                    List<Main.BlobResult> tempBlobResult = new List<Main.BlobResult>();
                    tempBlobResult.Add(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[1]);
                    Main.DrawOverlayBlobTool(PT_Display01, tempBlobResult);



                }
                //-----------------------------------------------------------------------------------------

                nBackUpRectAffine.SetCenterLengthsRotationSkew(nBackUpRect.CenterX, nBackUpRect.CenterY, nBackUpRect.Width, nBackUpRect.Height, 0, 0);
                nCopyTool.Region = new CogRectangleAffine(nBackUpRectAffine);
            }
            catch
            {

            }
            finally
            {
            }
            nPlusMinus = TempPlusMinus;
            return nCopyTool;
        }
        private bool Search_BLOB(bool nALLSEARCH)
        {
            bool nRet = true;
            bool TempSelect = false;
            int nStartNum = 0;
            int nLastNum = 0;

            if (nALLSEARCH)
            {
                nStartNum = 0;
                nLastNum = Main.DEFINE.BLOB_CNT_MAX;
            }
            else
            {
                nStartNum = m_SelectBlob;
                nLastNum = m_SelectBlob + 1;
            }

            for (int i = nStartNum; i < nLastNum; i++)
            {
                if (PT_BlobPara[m_PatNo, i].m_UseCheck)
                {
                    TempSelect = true;

                    if (PT_Blob_MarkUSE[m_PatNo])
                    {
                        (PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterX = PatResult.TranslationX + PT_BlobPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                        (PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterY = PatResult.TranslationY + PT_BlobPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
                    }
                    if (PT_Blob_CaliperUSE[m_PatNo])
                    {
                        (PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterX = PatResult.TranslationX + PT_BlobPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CALIPERTOOL].X;
                        (PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterY = PatResult.TranslationY + PT_BlobPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CALIPERTOOL].Y;
                    }

                    PT_BlobTools[m_PatNo, i].InputImage = PT_BlobTools[m_PatNo, m_SelectBlob].InputImage;


                    //                     if (Main.ALIGNINSPECTION_USE(m_AlignNo))
                    //                     {
                    //                         int nNum = 0;
                    //                         PT_BlobTools[m_PatNo, i] = BlobToolPairRun(PT_BlobTools[m_PatNo, i], i, out nNum);
                    //                     }

                    PT_BlobTools[m_PatNo, i].Run();
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[i].SearchRegion = new CogRectangleAffine(PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine);
                    if (PT_BlobTools[m_PatNo, i].Results != null)
                    {
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[i].BlobToolResult = new CogBlobResults(PT_BlobTools[m_PatNo, i].Results);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[i].Pixel[Main.DEFINE.X] = 0;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[i].Pixel[Main.DEFINE.Y] = 0;

                        List<Main.BlobResult> tempBlobResult = new List<Main.BlobResult>();
                        tempBlobResult.Add(Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[i]);
                        Main.DrawOverlayBlobTool(PT_Display01, tempBlobResult);
                        nRet = false;
                    }

                }
            }
            if (!TempSelect)
            {
                LABEL_MESSAGE(LB_MESSAGE, "All Blob Not Use!!", System.Drawing.Color.Red);
                nRet = false;
            }

            LB_List.Items.Clear();
            string A = "";

            if (PT_BlobTools[m_PatNo, m_SelectBlob].Results != null && PT_BlobPara[m_PatNo, m_SelectBlob].m_UseCheck)
            {
                if (PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs().Count > 0)
                {
                    for (int i = 0; i < PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs().Count; i++)
                    {
                        A = "";
                        A = A + "  X =" + PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].CenterOfMassX.ToString("0");
                        A = A + "  Y =" + PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].CenterOfMassY.ToString("0");
                        A = A + "  Area =" + PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].Area.ToString();
                        LB_List.Items.Add(A);
                    }

                    if (Main.BLOBINSPECTION_USE(m_AlignNo))
                    {
                        BlobMinMax_Control();
                        CogPointMarker MarkPoint = new CogPointMarker();
                        MarkPoint.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
                        MarkPoint.Color = CogColorConstants.Yellow;
                        MarkPoint.SizeInScreenPixels = 50;

                        if (m_SelectBlob < Main.DEFINE.BLOB_INSP_LIMIT_CNT)
                        {
                            if ((m_SelectBlob % 2) == 0)
                            {
                                MarkPoint.X = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[m_SelectBlob].VertexResults[2, 0];
                                MarkPoint.Y = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[m_SelectBlob].VertexResults[2, 1];
                                PT_Display01.StaticGraphics.Add(MarkPoint as ICogGraphic, "Search Region");
                            }
                            else
                            {
                                MarkPoint.X = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[m_SelectBlob].VertexResults[2, 0];
                                MarkPoint.Y = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[m_SelectBlob].VertexResults[3, 1];
                                PT_Display01.StaticGraphics.Add(MarkPoint as ICogGraphic, "Search Region");
                            }
                        }
                    }
                }
                PT_BLOB_SUB_Display.Image = PT_BlobTools[m_PatNo, m_SelectBlob].Results.CreateBlobImage();
                DisplayFit(PT_BLOB_SUB_Display);
            }
            else
            {
                Main.DisplayClear(PT_BLOB_SUB_Display);
                PT_BLOB_SUB_Display.Image = null;
            }
            return nRet;
        }
        private bool Search_Caliper(bool nALLSEARCH)
        {
            bool nRet = true;
            string strLog = "";
            bool TempSelect = false;
            int nStartNum = 0;
            int nLastNum = 0;

            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            double[] tempData = new double[2];
            double[] tempDataMark = new double[2];
            long tempYLength = new long();

            if (nALLSEARCH)
            {
                nStartNum = 0;
                nLastNum = Main.DEFINE.CALIPER_MAX;
            }
            else
            {
                nStartNum = m_SelectCaliper;
                nLastNum = m_SelectCaliper + 1;
            }

            for (int i = nStartNum; i < nLastNum; i++)
            {
                if (PT_CaliPara[m_PatNo, i].m_UseCheck)
                {
                    TempSelect = true;
                    int nTempPlusMinus = 1;

                    if (PT_Caliper_MarkUSE[m_PatNo])
                    {
                        (PT_CaliperTools[m_PatNo, i].Region as CogRectangleAffine).CenterX = PatResult.TranslationX + PT_CaliPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                        (PT_CaliperTools[m_PatNo, i].Region as CogRectangleAffine).CenterY = PatResult.TranslationY + PT_CaliPara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
                    }

                    if (Main.ALIGNINSPECTION_USE(m_AlignNo))
                    {
                        PT_CaliperTools[m_PatNo, i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].CaliperToolPairRun(PT_CaliperTools[m_PatNo, i], out nTempPlusMinus);
                    }
                    else
                    {
                        PT_CaliperTools[m_PatNo, i].Run();
                    }

                    if (PT_CaliperTools[m_PatNo, i].Results != null && PT_CaliperTools[m_PatNo, i].Results.Count > 0)
                    {
                        for (int j = 0; j < PT_CaliperTools[m_PatNo, i].Results.Count; j++)
                        {
                            resultGraphics.Add(PT_CaliperTools[m_PatNo, i].Results[j].CreateResultGraphics(CogCaliperResultGraphicConstants.Edges));
                        }
                        PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
                        //---------------------------------------------------------------------------------------------------------------------------------

                        #region COF_LENGTH
                        if (Main.AlignUnit[m_AlignNo].m_AlignName == "COF_CUTTING_ALIGN1" || Main.AlignUnit[m_AlignNo].m_AlignName == "COF_CUTTING_ALIGN2")
                        {
                            if (Main.GetCaliperDirection(Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, i].Region.Rotation)) == Main.DEFINE.X)
                            {
                                // PatResult.TranslationX = PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionX;
                            }
                            if (Main.GetCaliperDirection(Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, i].Region.Rotation)) == Main.DEFINE.Y)
                            {
                                // PatResult.TranslationY = PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionY;

                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(0, PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionY,
                                ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                if (PT_Caliper_MarkUSE[m_PatNo])
                                {
                                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationX, PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].GetPose().TranslationY,
                                    ref tempDataMark[Main.DEFINE.X], ref tempDataMark[Main.DEFINE.Y]);
                                    tempYLength = (long)(Math.Abs(tempDataMark[Main.DEFINE.Y] - tempData[Main.DEFINE.Y]));
                                    LABEL_MESSAGE(LB_MESSAGE, "COF Y_LENGTH: " + tempYLength.ToString("00") + " um", System.Drawing.Color.Lime);
                                }
                            }
                        }
                        #endregion

                        #region BEAM_WIDTH
                        for (int j = 0; j < PT_CaliperTools[m_PatNo, i].Results.Count; j++)
                        {
                            if (PT_CaliperTools[m_PatNo, i].RunParams.EdgeMode == CogCaliperEdgeModeConstants.Pair
                                && PT_CaliperTools[m_PatNo, i].Results.Edges.Count > 1)
                            {
                                double dWidth = 0;
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2RScalar(PT_CaliperTools[m_PatNo, i].Results[0].Width, ref dWidth);
                                strLog += i.ToString() + " " + dWidth.ToString("0.000") + " ";
                            }
                        }
                        #endregion
                        //---------------------------------------------------------------------------------------------------------------------------------
                    }
                    else
                    {
                        nRet = false;
                        LABEL_MESSAGE(LB_MESSAGE, i.ToString("00") + " Caliper: Search NG! Check!!!", System.Drawing.Color.Red);
                    }
                }
            }

            LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);

            if (PT_CaliperTools[m_PatNo, m_SelectCaliper].Results != null && PT_CaliperTools[m_PatNo, m_SelectCaliper].Results.Count > 0 && PT_CaliPara[m_PatNo, m_SelectCaliper].m_UseCheck)
            {
                DrawLastRegionData(PT_CALIPER_SUB_Display, PT_CaliperTools[m_PatNo, m_SelectCaliper]);
            }
            else
            {
                Main.DisplayClear(PT_CALIPER_SUB_Display);
                PT_CALIPER_SUB_Display.Image = null;
            }
            if (!TempSelect)
            {
                LABEL_MESSAGE(LB_MESSAGE, "All Caliper Not Use!!", System.Drawing.Color.Red);
                nRet = false;
            }
            return nRet;
        }
        private bool Search_FindLine(bool nALLSEARCH)
        {
            bool nRet = true;
            ushort temp = 0;
            bool TempSelect = false;
            int nStartNum = 0;
            int nLastNum = 0;
            int nTargetCenterIdx = 0;

            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            double[] tempData = new double[2];
            double[] tempDataMark = new double[2];
            string strLog = "";

            if (nALLSEARCH)
            {
                nStartNum = 0;
                nLastNum = Main.DEFINE.FINDLINE_MAX;
            }
            else
            {
                nStartNum = m_SelectFindLine;
                nLastNum = m_SelectFindLine + 1;
            }

            for (int i = nStartNum; i < nLastNum; i++)
            {
                if (PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_UseCheck)
                {
                    temp |= PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_LinePosition;

                    TempSelect = true;

                    if (PT_FindLine_MarkUSE[m_PatNo])
                    {
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                        {
                            (PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Region as CogRectangleAffine).CenterX = PatResult.TranslationX + PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                            (PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Region as CogRectangleAffine).CenterY = PatResult.TranslationY + PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
                        }
                        else
                        {
                            PT_FindLineTools[m_PatNo, m_LineSubNo, i].RunParams.ExpectedLineSegment.StartX = PatResult.TranslationX + PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                            PT_FindLineTools[m_PatNo, m_LineSubNo, i].RunParams.ExpectedLineSegment.StartY = PatResult.TranslationY + PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;

                            PT_FindLineTools[m_PatNo, m_LineSubNo, i].RunParams.ExpectedLineSegment.EndX = PatResult.TranslationX + PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X2;
                            PT_FindLineTools[m_PatNo, m_LineSubNo, i].RunParams.ExpectedLineSegment.EndY = PatResult.TranslationY + PT_FindLinePara[m_PatNo, m_LineSubNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y2;
                        }
                    }

                    if (i == 2) // Diag
                    {
                        continue;
                    }

                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Run();
                    else
                        PT_FindLineTools[m_PatNo, m_LineSubNo, i].Run();

                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                    {
                        int nLineIdx = 0;

                        if (PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Results[0].GetLine() != null)
                        {
                            for (int j = 0; j < PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Results.Count; j++)
                            {
                                resultGraphics.Add(PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Results[j].CreateResultGraphics(CogLineMaxResultGraphicConstants.FoundLine));
                            }
                            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);

                            if ((nLastNum - nStartNum) < 2)
                            {
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(i, PT_FindLinePara[m_PatNo, m_LineSubNo, i], PT_LineMaxTools[m_PatNo, m_LineSubNo, i], ref nLineIdx);

                                //// Horizontal Y Min Y Max
                                //if (i==0 && PT_LineMaxTools[m_PatNo, i].Results.Count > 1)
                                //{
                                //    for (int h = 1; h < PT_LineMaxTools[m_PatNo, i].Results.Count; h++)
                                //    {
                                //        if (PT_FindLinePara[m_PatNo, i].m_LineMaxHCond == Main.DEFINE.LINEMAX_H_YMIN)
                                //        {
                                //            // Search Y Min Index
                                //            if (PT_LineMaxTools[m_PatNo, i].Results[h].GetLine().Y < PT_LineMaxTools[m_PatNo, i].Results[nLineIdx].GetLine().Y)
                                //            {
                                //                nLineIdx = h;
                                //            }
                                //        }
                                //        else if (PT_FindLinePara[m_PatNo, i].m_LineMaxHCond == Main.DEFINE.LINEMAX_H_YMAX)
                                //        { 
                                //            // Search Y Max Index
                                //            if (PT_LineMaxTools[m_PatNo, i].Results[h].GetLine().Y > PT_LineMaxTools[m_PatNo, i].Results[nLineIdx].GetLine().Y)
                                //            {
                                //                nLineIdx = h;
                                //            }
                                //        }
                                //    }
                                //}
                                //// Horizontal Y Min Y Max
                                //else if (i==1 && PT_LineMaxTools[m_PatNo, i].Results.Count > 1)
                                //{
                                //    for (int v = 1; v < PT_LineMaxTools[m_PatNo, i].Results.Count; v++)
                                //    {
                                //        if (PT_FindLinePara[m_PatNo, i].m_LineMaxVCond == Main.DEFINE.LINEMAX_V_XMIN)
                                //        {
                                //            // Search X Min Index
                                //            if (PT_LineMaxTools[m_PatNo, i].Results[v].GetLine().X < PT_LineMaxTools[m_PatNo, i].Results[nLineIdx].GetLine().X)
                                //            {
                                //                nLineIdx = v;
                                //            }
                                //        }
                                //        else if (PT_FindLinePara[m_PatNo, i].m_LineMaxVCond == Main.DEFINE.LINEMAX_V_XMAX)
                                //        {
                                //            // Search X Max Index
                                //            if (PT_LineMaxTools[m_PatNo, i].Results[v].GetLine().X > PT_LineMaxTools[m_PatNo, i].Results[nLineIdx].GetLine().X)
                                //            {
                                //                nLineIdx = v;
                                //            }
                                //        }
                                //    }
                                //}

                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Results[nLineIdx].GetLineSegment().StartX, PT_LineMaxTools[m_PatNo, m_LineSubNo, i].Results[nLineIdx].GetLineSegment().StartY,
                                           ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog += "L1 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000");
                                LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                            }
                        }
                        else
                        {
                            nRet = false;
                            LABEL_MESSAGE(LB_MESSAGE, i.ToString("00") + " LineMax: Search NG! Check!!!", System.Drawing.Color.Red);
                            LABEL_MESSAGE(LB_MESSAGE1, i.ToString("00") + PT_LineMaxTools[m_PatNo, m_LineSubNo, i].RunStatus.Exception.Message, System.Drawing.Color.Red);
                        }
                    }
                    else
                    {
                        if (PT_FindLineTools[m_PatNo, m_LineSubNo, i].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, i].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, i].Results.GetLine() != null)
                        {
                            for (int j = 0; j < PT_FindLineTools[m_PatNo, m_LineSubNo, i].Results.Count; j++)
                            {
                                resultGraphics.Add(PT_FindLineTools[m_PatNo, m_LineSubNo, i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge | CogFindLineResultGraphicConstants.DataPoint));
                            }
                            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
                            //---------------------------------------------------------------------------------------------------------------------------------
                            //                         if (Main.GetCaliperDirection(Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, i].Region.Rotation)) == Main.DEFINE.X)
                            //                         {
                            //                             PatResult.TranslationX = PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionX;
                            //                         }
                            //                         if (Main.GetCaliperDirection(Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, i].Region.Rotation)) == Main.DEFINE.Y)
                            //                         {
                            //                             PatResult.TranslationY = PT_CaliperTools[m_PatNo, i].Results[0].Edge0.PositionY;
                            //                         }
                            //---------------------------------------------------------------------------------------------------------------------------------

                            if ((nLastNum - nStartNum) < 2)
                            {
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_FindLineTools[m_PatNo, m_LineSubNo, i].Results.GetLineSegment().StartX, PT_FindLineTools[m_PatNo, m_LineSubNo, i].Results.GetLineSegment().StartY,
                                           ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog += "L1 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000");
                                LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                            }
                        }
                        else
                        {
                            nRet = false;
                            LABEL_MESSAGE(LB_MESSAGE, i.ToString("00") + " FindLine: Search NG! Check!!!", System.Drawing.Color.Red);
                        }
                    }
                }
            }

            //LABEL_MESSAGE(LB_MESSAGE1, "Line Comb : " + temp.ToString("00"), System.Drawing.Color.Blue);

            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
            {
                if ((nLastNum - nStartNum) >= 3)
                {
                    if ((PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results[0].GetLine() != null)
                        && (PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results[0].GetLine() != null))
                    {
                        int nLineIdx1 = 0, nLineIdx2 = 0;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum], ref nLineIdx1);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum + 1, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum + 1], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1], ref nLineIdx2);

                        PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[0].LineA = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results[nLineIdx1].GetLine();
                        PT_LineLineCrossPoints[0].LineB = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results[nLineIdx2].GetLine();
                        PT_LineLineCrossPoints[0].Run();
                        if (PT_LineLineCrossPoints[0].Intersects)
                        {
                            if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                nRet = true;

                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_LineLineCrossPoints[0].X, PT_LineLineCrossPoints[0].Y,
                                    ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog += "P1 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000") + "\n";
                                LABEL_MESSAGE(LB_MESSAGE1, strLog, System.Drawing.Color.Green);

                                ////////////////////////////////
                                if (PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_UseCheck)
                                {
                                    // Position correction
                                    {
                                        (PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Region as CogRectangleAffine).CenterX = PT_LineLineCrossPoints[0].X + PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X;
                                        (PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Region as CogRectangleAffine).CenterY = PT_LineLineCrossPoints[0].Y + PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y;
                                    }

                                    PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Run();

                                    if (PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results[0].GetLine() != null)
                                    {
                                        if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                                        {
                                            strLog += "Angle : " + (PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results[0].GetLine().Rotation * Main.DEFINE.degree).ToString("0.00");
                                            LABEL_MESSAGE(LB_MESSAGE1, strLog, System.Drawing.Color.Green);
                                        }

                                        for (int j = 0; j < PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results.Count; j++)
                                        {
                                            resultGraphics.Add(PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results[j].CreateResultGraphics(CogLineMaxResultGraphicConstants.FoundLine));
                                        }
                                        PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
                                    }
                                    else
                                    {
                                        nRet = false;
                                        LABEL_MESSAGE(LB_MESSAGE, 2.ToString("00") + " FindLine: Search NG! Check!!!", System.Drawing.Color.Red);
                                    }
                                }
                            }
                        }
                    }

                    // X, 대각 교점
                    if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1"
                        && (PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results[0].GetLine() != null)
                        && (PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results[0].GetLine() != null))
                    {
                        int nLineIdx1 = 0, nLineIdx2 = 0;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum], ref nLineIdx1);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum + 2, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum + 2], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2], ref nLineIdx2);

                        PT_LineLineCrossPoints[1] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[1].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[1].LineA = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results[nLineIdx1].GetLine();
                        PT_LineLineCrossPoints[1].LineB = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results[nLineIdx2].GetLine();
                        PT_LineLineCrossPoints[1].Run();
                        if (PT_LineLineCrossPoints[1].Intersects)
                        {
                            if (PT_LineLineCrossPoints[1].X >= 0 && PT_LineLineCrossPoints[1].X <= PT_LineLineCrossPoints[1].InputImage.Width && PT_LineLineCrossPoints[1].Y >= 0 && PT_LineLineCrossPoints[1].Y <= PT_LineLineCrossPoints[1].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[1].CreateLastRunRecord(), "RESULT");

                                nRet = true;
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_LineLineCrossPoints[1].X, PT_LineLineCrossPoints[1].Y,
                                   ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog += "P2 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000");
                                LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                            }
                        }
                    }

                    // Y, 대각 교점
                    if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1"
                        && (PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results[0].GetLine() != null)
                        && (PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results[0].GetLine() != null))
                    {
                        int nLineIdx1 = 0, nLineIdx2 = 0;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum + 1, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum + 1], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1], ref nLineIdx1);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum + 2, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum + 2], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2], ref nLineIdx2);

                        PT_LineLineCrossPoints[2] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[2].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[2].LineA = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results[nLineIdx1].GetLine();
                        PT_LineLineCrossPoints[2].LineB = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results[nLineIdx2].GetLine();
                        PT_LineLineCrossPoints[2].Run();
                        if (PT_LineLineCrossPoints[2].Intersects)
                        {
                            if (PT_LineLineCrossPoints[2].X >= 0 && PT_LineLineCrossPoints[2].X <= PT_LineLineCrossPoints[2].InputImage.Width && PT_LineLineCrossPoints[2].Y >= 0 && PT_LineLineCrossPoints[2].Y <= PT_LineLineCrossPoints[2].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[2].CreateLastRunRecord(), "RESULT");

                                nRet = true;
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_LineLineCrossPoints[2].X, PT_LineLineCrossPoints[2].Y,
                                   ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog = "P3 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000");
                                LABEL_MESSAGE(LB_MESSAGE1, strLog, System.Drawing.Color.Green);
                            }
                        }
                    }
                }
                else if ((nLastNum - nStartNum) >= 2)
                {
                    if ((PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results[0].GetLine() != null)
                        && (PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results[0].GetLine() != null))
                    {
                        int nLineIdx1 = 0, nLineIdx2 = 0;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum], ref nLineIdx1);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(nStartNum + 1, PT_FindLinePara[m_PatNo, m_LineSubNo, nStartNum + 1], PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1], ref nLineIdx2);


                        PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[0].LineA = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum].Results[nLineIdx1].GetLine();
                        PT_LineLineCrossPoints[0].LineB = PT_LineMaxTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results[nLineIdx2].GetLine();
                        PT_LineLineCrossPoints[0].Run();
                        if (PT_LineLineCrossPoints[0].Intersects)
                        {
                            if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                nRet = true;
                            }
                        }
                    }
                }

                if ((Main.AlignUnitTag.FindLineConstants)temp == Main.AlignUnitTag.FindLineConstants.LineX)
                {
                    if (PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results[0].GetLine() != null)
                    {
                        int nLineIdx = 0;
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(0, PT_FindLinePara[m_PatNo, m_LineSubNo, 0], PT_LineMaxTools[m_PatNo, m_LineSubNo, 0], ref nLineIdx);

                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].LastRunRecordDiagEnable = CogLineMaxLastRunRecordDiagConstants.InputImageByReference;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].LastRunRecordEnable = CogLineMaxLastRunRecordConstants.FoundLines;
                        Display.SetGraphics(PT_Display01, PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].CreateLastRunRecord(), "RESULT");
                        strLog = "Line X : " + PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results[nLineIdx].GetLine().X.ToString("0.000") + ", " + PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results[nLineIdx].GetLine().Y.ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                    }
                }
                else if ((Main.AlignUnitTag.FindLineConstants)temp == Main.AlignUnitTag.FindLineConstants.LineY)
                {
                    int nLineIdx = 0;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(1, PT_FindLinePara[m_PatNo, m_LineSubNo, 1], PT_LineMaxTools[m_PatNo, m_LineSubNo, 1], ref nLineIdx);

                    if (PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results[0].GetLine() != null)
                    {
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].LastRunRecordDiagEnable = CogLineMaxLastRunRecordDiagConstants.InputImageByReference;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].LastRunRecordEnable = CogLineMaxLastRunRecordConstants.FoundLines;
                        Display.SetGraphics(PT_Display01, PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].CreateLastRunRecord(), "RESULT");
                        strLog = "Line Y : " + PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results[nLineIdx].GetLine().X.ToString("0.000") + ", " + PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results[nLineIdx].GetLine().Y.ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                    }
                }
                else if ((Main.AlignUnitTag.FindLineConstants)temp == Main.AlignUnitTag.FindLineConstants.LineDiag)
                {
                    int nLineIdx = 0;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(2, PT_FindLinePara[m_PatNo, m_LineSubNo, 2], PT_LineMaxTools[m_PatNo, m_LineSubNo, 2], ref nLineIdx);

                    if (PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results[0].GetLine() != null)
                    {
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].LastRunRecordDiagEnable = CogLineMaxLastRunRecordDiagConstants.InputImageByReference;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].LastRunRecordEnable = CogLineMaxLastRunRecordConstants.FoundLines;
                        Display.SetGraphics(PT_Display01, PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].CreateLastRunRecord(), "RESULT");
                        strLog = "Line D : " + PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results[nLineIdx].GetLine().X.ToString("0.000") + ", " + PT_LineMaxTools[m_PatNo, m_LineSubNo, 2].Results[nLineIdx].GetLine().Y.ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                    }
                }
            }
            else
            {
                if ((nLastNum - nStartNum) >= 3)
                {
                    // X, Y 교점
                    if ((PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.GetLine() != null)
                        && (PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.GetLine() != null))
                    {
                        PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[0].LineA = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.GetLine();
                        PT_LineLineCrossPoints[0].LineB = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.GetLine();
                        PT_LineLineCrossPoints[0].Run();
                        if (PT_LineLineCrossPoints[0].Intersects)
                        {
                            if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                nRet = true;
                                //DoublePoint Temp = new DoublePoint();
                                //Temp.X = FINDLineResults[0].Pixel[DEFINE.X] = Pixel[DEFINE.X] = PixelFindLine[DEFINE.X] = (LineLineTool.X);
                                //Temp.Y = FINDLineResults[0].Pixel[DEFINE.Y] = Pixel[DEFINE.Y] = PixelFindLine[DEFINE.Y] = (LineLineTool.Y);
                                //FINDLineResults[0].CrossPointList.Add(Temp);
                                //LABEL_MESSAGE(LB_MESSAGE, "P1 : " + PT_LineLineCrossPoints[0].X.ToString("0.00") + ", " + PT_LineLineCrossPoints[0].Y.ToString("0.00"), System.Drawing.Color.Green);
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_LineLineCrossPoints[0].X, PT_LineLineCrossPoints[0].Y,
                                    ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog += "P1 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000") + "\n";
                                LABEL_MESSAGE(LB_MESSAGE1, strLog, System.Drawing.Color.Green);

                                ////////////////////////////////
                                if (PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_UseCheck)
                                {
                                    // Position correction
                                    {
                                        PT_FindLineTools[m_PatNo, m_LineSubNo, 2].RunParams.ExpectedLineSegment.StartX = PT_LineLineCrossPoints[0].X + PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X;
                                        PT_FindLineTools[m_PatNo, m_LineSubNo, 2].RunParams.ExpectedLineSegment.StartY = PT_LineLineCrossPoints[0].Y + PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y;

                                        PT_FindLineTools[m_PatNo, m_LineSubNo, 2].RunParams.ExpectedLineSegment.EndX = PT_LineLineCrossPoints[0].X + PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X2;
                                        PT_FindLineTools[m_PatNo, m_LineSubNo, 2].RunParams.ExpectedLineSegment.EndY = PT_LineLineCrossPoints[0].Y + PT_FindLinePara[m_PatNo, m_LineSubNo, 2].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y2;
                                    }

                                    PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Run();

                                    if (PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.GetLine() != null)
                                    {
                                        if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC2")
                                        {
                                            strLog += "Angle : " + (PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.GetLine().Rotation * Main.DEFINE.degree).ToString("0.00");
                                            LABEL_MESSAGE(LB_MESSAGE1, strLog, System.Drawing.Color.Green);
                                        }

                                        for (int j = 0; j < PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.Count; j++)
                                        {
                                            resultGraphics.Add(PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge | CogFindLineResultGraphicConstants.DataPoint));
                                        }
                                        PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
                                    }
                                    else
                                    {
                                        nRet = false;
                                        LABEL_MESSAGE(LB_MESSAGE, 2.ToString("00") + " FindLine: Search NG! Check!!!", System.Drawing.Color.Red);
                                    }
                                }
                            }
                        }
                    }

                    // X, 대각 교점
                    if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1"
                        && (PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.GetLine() != null)
                        && (PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.GetLine() != null))
                    {
                        PT_LineLineCrossPoints[1] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[1].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[1].LineA = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.GetLine();
                        PT_LineLineCrossPoints[1].LineB = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.GetLine();
                        PT_LineLineCrossPoints[1].Run();
                        if (PT_LineLineCrossPoints[1].Intersects)
                        {
                            if (PT_LineLineCrossPoints[1].X >= 0 && PT_LineLineCrossPoints[1].X <= PT_LineLineCrossPoints[1].InputImage.Width && PT_LineLineCrossPoints[1].Y >= 0 && PT_LineLineCrossPoints[1].Y <= PT_LineLineCrossPoints[1].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[1].CreateLastRunRecord(), "RESULT");

                                nRet = true;
                                //DoublePoint Temp = new DoublePoint();
                                //Temp.X = FINDLineResults[0].Pixel[DEFINE.X] = Pixel[DEFINE.X] = PixelFindLine[DEFINE.X] = (LineLineTool.X);
                                //Temp.Y = FINDLineResults[0].Pixel[DEFINE.Y] = Pixel[DEFINE.Y] = PixelFindLine[DEFINE.Y] = (LineLineTool.Y);
                                //FINDLineResults[0].CrossPointList.Add(Temp);
                                //LABEL_MESSAGE(LB_MESSAGE, "P2 : " + PT_LineLineCrossPoints[1].X.ToString("0.00") + ", " + PT_LineLineCrossPoints[1].Y.ToString("0.00"), System.Drawing.Color.Green);
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_LineLineCrossPoints[1].X, PT_LineLineCrossPoints[1].Y,
                                   ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog += "P2 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000");
                                LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                            }
                        }
                    }

                    // Y, 대각 교점
                    if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1"
                        && (PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.GetLine() != null)
                        && (PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.GetLine() != null))
                    {
                        PT_LineLineCrossPoints[2] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[2].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[2].LineA = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.GetLine();
                        PT_LineLineCrossPoints[2].LineB = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 2].Results.GetLine();
                        PT_LineLineCrossPoints[2].Run();
                        if (PT_LineLineCrossPoints[2].Intersects)
                        {
                            if (PT_LineLineCrossPoints[2].X >= 0 && PT_LineLineCrossPoints[2].X <= PT_LineLineCrossPoints[2].InputImage.Width && PT_LineLineCrossPoints[2].Y >= 0 && PT_LineLineCrossPoints[2].Y <= PT_LineLineCrossPoints[2].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[2].CreateLastRunRecord(), "RESULT");

                                nRet = true;
                                //DoublePoint Temp = new DoublePoint();
                                //Temp.X = FINDLineResults[0].Pixel[DEFINE.X] = Pixel[DEFINE.X] = PixelFindLine[DEFINE.X] = (LineLineTool.X);
                                //Temp.Y = FINDLineResults[0].Pixel[DEFINE.Y] = Pixel[DEFINE.Y] = PixelFindLine[DEFINE.Y] = (LineLineTool.Y);
                                //FINDLineResults[0].CrossPointList.Add(Temp);
                                //LABEL_MESSAGE(LB_MESSAGE, "P3 : " + PT_LineLineCrossPoints[2].X.ToString("0.00") + ", " + PT_LineLineCrossPoints[2].Y.ToString("0.00"), System.Drawing.Color.Green);
                                Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(PT_LineLineCrossPoints[2].X, PT_LineLineCrossPoints[2].Y,
                                   ref tempData[Main.DEFINE.X], ref tempData[Main.DEFINE.Y]);
                                strLog = "P3 : " + tempData[Main.DEFINE.X].ToString("0.000") + ", " + tempData[Main.DEFINE.Y].ToString("0.000");
                                LABEL_MESSAGE(LB_MESSAGE1, strLog, System.Drawing.Color.Green);
                            }
                        }
                    }

                }
                else if ((nLastNum - nStartNum) >= 2)
                {
                    if ((PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.GetLine() != null)
                        && (PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.GetLine() != null))
                    {
                        PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                        PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                        PT_LineLineCrossPoints[0].LineA = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum].Results.GetLine();
                        PT_LineLineCrossPoints[0].LineB = PT_FindLineTools[m_PatNo, m_LineSubNo, nStartNum + 1].Results.GetLine();
                        PT_LineLineCrossPoints[0].Run();
                        if (PT_LineLineCrossPoints[0].Intersects)
                        {
                            if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                            {
                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                nRet = true;
                                //DoublePoint Temp = new DoublePoint();
                                //Temp.X = FINDLineResults[0].Pixel[DEFINE.X] = Pixel[DEFINE.X] = PixelFindLine[DEFINE.X] = (LineLineTool.X);
                                //Temp.Y = FINDLineResults[0].Pixel[DEFINE.Y] = Pixel[DEFINE.Y] = PixelFindLine[DEFINE.Y] = (LineLineTool.Y);
                                //FINDLineResults[0].CrossPointList.Add(Temp);
                            }
                        }
                    }
                }

                if ((Main.AlignUnitTag.FindLineConstants)temp == Main.AlignUnitTag.FindLineConstants.LineX)
                {
                    if (PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results.GetLine() != null)
                    {
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 0].LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.InputImageByReference;
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 0].LastRunRecordEnable = CogFindLineLastRunRecordConstants.BestFitLine;
                        Display.SetGraphics(PT_Display01, PT_FindLineTools[m_PatNo, m_LineSubNo, 0].CreateLastRunRecord(), "RESULT");
                        strLog = "Line X : " + PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results.GetLine().X.ToString("0.000") + ", " + PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results.GetLine().Y.ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                    }
                }
                else if ((Main.AlignUnitTag.FindLineConstants)temp == Main.AlignUnitTag.FindLineConstants.LineY)
                {
                    if (PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results.GetLine() != null)
                    {
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 1].LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.InputImageByReference;
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 1].LastRunRecordEnable = CogFindLineLastRunRecordConstants.BestFitLine;
                        Display.SetGraphics(PT_Display01, PT_FindLineTools[m_PatNo, m_LineSubNo, 1].CreateLastRunRecord(), "RESULT");
                        strLog = "Line Y : " + PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results.GetLine().X.ToString("0.000") + ", " + PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results.GetLine().Y.ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                    }
                }
                else if ((Main.AlignUnitTag.FindLineConstants)temp == Main.AlignUnitTag.FindLineConstants.LineDiag)
                {
                    if (PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.GetLine() != null)
                    {
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 2].LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.InputImageByReference;
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 2].LastRunRecordEnable = CogFindLineLastRunRecordConstants.BestFitLine;
                        Display.SetGraphics(PT_Display01, PT_FindLineTools[m_PatNo, m_LineSubNo, 2].CreateLastRunRecord(), "RESULT");
                        strLog = "Line D : " + PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.GetLine().X.ToString("0.000") + ", " + PT_FindLineTools[m_PatNo, m_LineSubNo, 2].Results.GetLine().Y.ToString("0.000");
                        LABEL_MESSAGE(LB_MESSAGE, strLog, System.Drawing.Color.Green);
                    }
                }
            }

            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax &&
                PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].Results.Count > 0 && PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck)
            {
                DrawFINDLineLastRegionData(PT_FINDLINE_SUB_Display, PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine]);
            }
            else if (PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].Results.Count > 0 && PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck)
            {
                DrawFINDLineLastRegionData(PT_FINDLINE_SUB_Display, PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine]);
            }
            else
            {
                Main.DisplayClear(PT_FINDLINE_SUB_Display);
                PT_FINDLINE_SUB_Display.Image = null;
            }
            if (!TempSelect)
            {
                LABEL_MESSAGE(LB_MESSAGE, "All FindLine Not Use!!", System.Drawing.Color.Red);
                nRet = false;
            }
            return nRet;
        }
        private bool Search_Circle(bool nALLSEARCH)
        {
            bool nRet = true;

            bool TempSelect = false;
            int nStartNum = 0;
            int nLastNum = 0;

            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            double[] tempData = new double[2];
            double[] tempDataMark = new double[2];

            if (nALLSEARCH)
            {
                nStartNum = 0;
                nLastNum = Main.DEFINE.CIRCLE_MAX;
            }
            else
            {
                nStartNum = m_SelectCircle;
                nLastNum = m_SelectCircle + 1;
            }

            for (int i = nStartNum; i < nLastNum; i++)
            {
                if (PT_CirclePara[m_PatNo, i].m_UseCheck)
                {
                    TempSelect = true;

                    if (PT_Circle_MarkUSE[m_PatNo])
                    {
                        PT_CircleTools[m_PatNo, i].RunParams.ExpectedCircularArc.CenterX = PatResult.TranslationX + PT_CirclePara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                        PT_CircleTools[m_PatNo, i].RunParams.ExpectedCircularArc.CenterY = PatResult.TranslationY + PT_CirclePara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;

                    }

                    // Position correction
                    {
                        PT_CircleTools[m_PatNo, i].RunParams.ExpectedCircularArc.CenterX = PT_LineLineCrossPoints[0].X + PT_CirclePara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X;
                        PT_CircleTools[m_PatNo, i].RunParams.ExpectedCircularArc.CenterY = PT_LineLineCrossPoints[0].Y + PT_CirclePara[m_PatNo, i].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y;
                    }

                    PT_CircleTools[m_PatNo, i].Run();

                    if (PT_CircleTools[m_PatNo, i].Results != null && PT_CircleTools[m_PatNo, i].Results.Count > 0 && PT_CircleTools[m_PatNo, i].Results.GetCircle() != null
                        )
                    {
                        for (int j = 0; j < PT_CircleTools[m_PatNo, i].Results.Count; j++)
                        {
                            resultGraphics.Add(PT_CircleTools[m_PatNo, i].Results[j].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge | CogFindCircleResultGraphicConstants.DataPoint));
                        }
                        CogPointMarker nCircleCenter = new CogPointMarker();
                        nCircleCenter.Color = CogColorConstants.Purple;
                        nCircleCenter.SetCenterRotationSize(PT_CircleTools[m_PatNo, i].Results.GetCircle().CenterX, PT_CircleTools[m_PatNo, i].Results.GetCircle().CenterY, 0, 20);
                        resultGraphics.Add(nCircleCenter);
                        PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2RScalar(PT_CircleTools[m_PatNo, i].Results.GetCircle().Radius, ref tempData[0]);
                        LABEL_MESSAGE(LB_MESSAGE, "Circle : " + tempData[0].ToString("0.000") + "R", System.Drawing.Color.Green);
                    }
                    else
                    {
                        nRet = false;
                        LABEL_MESSAGE(LB_MESSAGE, i.ToString("00") + " Circle: Search NG! Check!!!", System.Drawing.Color.Red);
                    }
                }
            }

            if (PT_CircleTools[m_PatNo, m_SelectCircle].Results != null && PT_CircleTools[m_PatNo, m_SelectCircle].Results.Count > 0 && PT_CircleTools[m_PatNo, m_SelectCircle].Results.GetCircle() != null && PT_CirclePara[m_PatNo, m_SelectCircle].m_UseCheck)
            {
                DrawLastRegionData(PT_CIRCLE_SUB_Display, PT_CircleTools[m_PatNo, m_SelectCircle]);

            }
            else
            {
                Main.DisplayClear(PT_CIRCLE_SUB_Display);
                PT_CIRCLE_SUB_Display.Image = null;
            }
            if (!TempSelect)
            {
                //LABEL_MESSAGE(LB_MESSAGE, "All Circle Not Use!!", System.Drawing.Color.Red);
                nRet = false;
            }
            return nRet;
        }


        #region BLOB
        private void Blob_Change()
        {
            m_PatchangeFlag = true;
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            RefreshDisplay2();
            BLOB_TBAR_THRES.Value = PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.SegmentationParams.HardFixedThreshold;
            NUD_MinPixel.Value = (decimal)PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.ConnectivityMinPixels;
            CB_POLARITY_BLOB.SelectedIndex = Convert.ToInt16(PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.SegmentationParams.Polarity);  //CogBlobSegmentationPolarityConstants.DarkBlobs;
            CB_BLOB_USE.Checked = PT_BlobPara[m_PatNo, m_SelectBlob].m_UseCheck;
            CB_BLOB_MARK_USE.Checked = PT_Blob_MarkUSE[m_PatNo];
            CB_BLOB_CALIPER_USE.Checked = PT_Blob_CaliperUSE[m_PatNo];
            Inspect_Cnt.Value = PT_Blob_InspCnt[m_PatNo];
            LB_List.Items.Clear();

            Main.DisplayClear(PT_BLOB_SUB_Display);
            PT_BLOB_SUB_Display.Image = null;
            DrawBlobRegion();
            m_PatchangeFlag = false;
        }
        private void DrawBlobRegion()
        {
            if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_BLOBTOOL)
            {
                DisplayClear();
                BlobTrainRegion = new CogRectangleAffine(PT_BlobTools[m_PatNo, m_SelectBlob].Region as CogRectangleAffine);
                if (PT_Blob_MarkUSE[m_PatNo])
                {
                    BlobTrainRegion.CenterX = PatResult.TranslationX + PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                    BlobTrainRegion.CenterY = PatResult.TranslationY + PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
                }
                if (PT_Blob_CaliperUSE[m_PatNo])
                {
                    BlobTrainRegion.CenterX = PatResult.TranslationX + PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CALIPERTOOL].X;
                    BlobTrainRegion.CenterY = PatResult.TranslationY + PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CALIPERTOOL].Y;
                }
                BlobTrainRegion.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                BlobTrainRegion.Interactive = true;

                CogGraphicInteractiveCollection BlobInfo = new CogGraphicInteractiveCollection();
                BlobInfo.Add(BlobTrainRegion);
                PT_Display01.InteractiveGraphics.AddList(BlobInfo, "BLOB_INFO", false);
                Main.DisplayFit(PT_Display01);

            }
        }
        private void CB_BLOB_USE_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_BLOB_USE.Checked)
            {
                CB_BLOB_USE.BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                CB_BLOB_USE.BackColor = System.Drawing.Color.DarkGray;
            }

        }
        private void CB_BLOB_USE_Click(object sender, EventArgs e)
        {
            if (CB_BLOB_USE.Checked)
            {
                PT_BlobPara[m_PatNo, m_SelectBlob].m_UseCheck = true;
            }
            else
            {
                PT_BlobPara[m_PatNo, m_SelectBlob].m_UseCheck = false;
            }
        }
        private void BlobMinMax_Control()
        {
            /////2/////
            //0//////1//
            /////3/////

            double[] VertexValue = new double[4];
            int[,] VertexArray = new int[4, 2];

            int POS_I = 0, POS_J = 1;
            int POS_X = 0, POS_Y = 1;
            int MIN_X = 0, MAX_X = 1, MIN_Y = 2, MAX_Y = 3;
            if (PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs().Count > 0)
            {

                for (int i = 0; i < PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs().Count; i++)
                {
                    for (int j = 0; j < PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertices().Length / 2; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            VertexValue[MIN_X] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexX(j);
                            VertexValue[MAX_X] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexX(j);
                            VertexValue[MIN_Y] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexY(j);
                            VertexValue[MAX_Y] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexY(j);
                        }
                        if (PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexX(j) < VertexValue[MIN_X])
                        {
                            VertexValue[MIN_X] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexX(j);
                            VertexArray[MIN_X, POS_I] = i;
                            VertexArray[MIN_X, POS_J] = j;
                        }
                        if (PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexX(j) > VertexValue[MAX_X])
                        {
                            VertexValue[MAX_X] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexX(j);
                            VertexArray[MAX_X, POS_I] = i;
                            VertexArray[MAX_X, POS_J] = j;
                        }

                        if (PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexY(j) < VertexValue[MIN_Y])
                        {
                            VertexValue[MIN_Y] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexY(j);
                            VertexArray[MIN_Y, POS_I] = i;
                            VertexArray[MIN_Y, POS_J] = j;
                        }
                        if (PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexY(j) > VertexValue[MAX_Y])
                        {
                            VertexValue[MAX_Y] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[i].GetBoundary().GetVertexY(j);
                            VertexArray[MAX_Y, POS_I] = i;
                            VertexArray[MAX_Y, POS_J] = j;
                        }
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[m_SelectBlob].VertexResults[i, POS_X] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[VertexArray[i, POS_I]].GetBoundary().GetVertexX(VertexArray[i, POS_J]);
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].BlobResults[m_SelectBlob].VertexResults[i, POS_Y] = PT_BlobTools[m_PatNo, m_SelectBlob].Results.GetBlobs()[VertexArray[i, POS_I]].GetBoundary().GetVertexY(VertexArray[i, POS_J]);
                }
            }
        }

        bool nCheckBoxFlag = false;
        private void BLOB_MARK_USE_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox TempBTN = (CheckBox)sender;
            if (TempBTN.Checked)
            {
                TempBTN.BackColor = System.Drawing.Color.GreenYellow;
                PT_Blob_MarkUSE[m_PatNo] = true;
                if (CB_BLOB_CALIPER_USE.Checked)
                {
                    CB_BLOB_CALIPER_USE.Checked = false;
                    nCheckBoxFlag = true;
                }
            }
            else
            {
                TempBTN.BackColor = System.Drawing.Color.White;
                PT_Blob_MarkUSE[m_PatNo] = false;

            }
            if (!nCheckBoxFlag && !m_TABCHANGE_MODE)
            {
                RefreshDisplay2();
                DrawBlobRegion();
            }
            nCheckBoxFlag = false;
        }
        private void CB_BLOB_CALIPER_USE_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox TempBTN = (CheckBox)sender;
            if (TempBTN.Checked)
            {
                TempBTN.BackColor = System.Drawing.Color.GreenYellow;
                PT_Blob_CaliperUSE[m_PatNo] = true;

                if (CB_BLOB_MARK_USE.Checked)
                {
                    CB_BLOB_MARK_USE.Checked = false;
                    nCheckBoxFlag = true;
                }
            }
            else
            {
                TempBTN.BackColor = System.Drawing.Color.White;
                PT_Blob_CaliperUSE[m_PatNo] = false;
            }
            if (!nCheckBoxFlag && !m_TABCHANGE_MODE)
            {
                RefreshDisplay2();
                DrawBlobRegion();
            }
            nCheckBoxFlag = false;
        }
        private void CB_BLOB_COUNT_SelectionChangeCommitted(object sender, EventArgs e)
        {
            m_SelectBlob = CB_BLOB_COUNT.SelectedIndex;
            if (!m_TABCHANGE_MODE)
            {
                Blob_Change();
            }
        }
        private void CB_POLARITY_BLOB_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.SegmentationParams.Polarity = (CogBlobSegmentationPolarityConstants)CB_POLARITY_BLOB.SelectedIndex;
        }
        private void NUD_MinPixel_Click(object sender, EventArgs e)
        {
            PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.ConnectivityMinPixels = (int)NUD_MinPixel.Value;
            if (!m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_BLOBTOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void Inspect_Cnt_ValueChanged(object sender, EventArgs e)
        {
            PT_Blob_InspCnt[m_PatNo] = (int)Inspect_Cnt.Value; //= Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_Blob_InspCnt

            for (int i = 0; i < Main.DEFINE.BLOB_INSP_LIMIT_CNT; i++)
                PT_BlobPara[m_PatNo, i].m_UseCheck = false;

            if (PT_Blob_InspCnt[m_PatNo] != 0)
            {
                for (int i = 0; i < 2 * PT_Blob_InspCnt[m_PatNo]; i++)
                    PT_BlobPara[m_PatNo, i].m_UseCheck = true;
            }


        }

        private void BLOB_TBAR_THRES_ValueChanged(object sender, EventArgs e)
        {
            PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.SegmentationParams.HardFixedThreshold = BLOB_TBAR_THRES.Value;
            BLOB_LB_THRES.Text = BLOB_TBAR_THRES.Value.ToString();
            if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_BLOBTOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void BLOB_BTN_THRES_UP_Click(object sender, EventArgs e)
        {
            if (BLOB_TBAR_THRES.Maximum == BLOB_TBAR_THRES.Value) return;

            BLOB_TBAR_THRES.Value++;
        }
        private void BLOB_BTN_THRES_DN_Click(object sender, EventArgs e)
        {
            if (BLOB_TBAR_THRES.Minimum == BLOB_TBAR_THRES.Value) return;

            BLOB_TBAR_THRES.Value--;
        }
        private void BTN_BLOB_Click(object sender, EventArgs e)
        {
            Blob_Change();
        }
        private void BLOB_APPLY_Click(object sender, EventArgs e)
        {
            try
            {
                if (PT_Blob_MarkUSE[m_PatNo])
                {
                    PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X = BlobTrainRegion.CenterX - PatResult.TranslationX;
                    PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y = BlobTrainRegion.CenterY - PatResult.TranslationY;
                }
                if (PT_Blob_CaliperUSE[m_PatNo])
                {
                    PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CALIPERTOOL].X = BlobTrainRegion.CenterX - PatResult.TranslationX;
                    PT_BlobPara[m_PatNo, m_SelectBlob].m_TargetToCenter[Main.DEFINE.M_CALIPERTOOL].Y = BlobTrainRegion.CenterY - PatResult.TranslationY;
                }
                PT_BlobTools[m_PatNo, m_SelectBlob].Region = new CogRectangleAffine(BlobTrainRegion);
                PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.SegmentationParams.HardFixedThreshold = BLOB_TBAR_THRES.Value;
                PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.ConnectivityMinPixels = (int)NUD_MinPixel.Value;
                PT_BlobTools[m_PatNo, m_SelectBlob].RunParams.SegmentationParams.Polarity = (CogBlobSegmentationPolarityConstants)CB_POLARITY_BLOB.SelectedIndex;
                LABEL_MESSAGE(LB_MESSAGE, "Register OK", System.Drawing.Color.Lime);
            }
            catch (System.Exception ex)
            {
                LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            }
            Main.DisplayFit(PT_Display01);
        }
        private void BTN_BLOBCOPY_Click(object sender, EventArgs e)
        {
            //             if (!Main.machine.EngineerMode)
            //             {
            //                 MessageBox.Show("Not Engineer Mode!!!");
            //                 return;
            //             }
            //             if(BlobTrainRegion.Equals(PT_BlobTools[m_PatNo, 0]))
            //             {
            //                 for(int i = 0; i < Main.DEFINE.BLOB_CNT_MAX; i++)
            //                 {
            //                     try
            //                     {
            //                         PT_BlobTools[m_PatNo, i].Region = new CogRectangle(BlobTrainRegion);
            //                         PT_BlobTools[m_PatNo, i].RunParams.SegmentationParams.HardFixedThreshold = BLOB_TBAR_THRES.Value;
            //                         PT_BlobTools[m_PatNo, i].RunParams.ConnectivityMinPixels = (int)NUD_MinPixel.Value;
            //                         PT_BlobTools[m_PatNo, i].RunParams.SegmentationParams.Polarity = (CogBlobSegmentationPolarityConstants)CB_POLARITY_BLOB.SelectedIndex;
            //                         LABEL_MESSAGE(LB_MESSAGE, "Register OK", System.Drawing.Color.Lime);
            // 
            //                         if (Main.BLOBINSPECTION_USE(m_AlignNo))
            //                         {
            //                             if (m_SelectBlob == 0 || m_SelectBlob == 2 || m_SelectBlob == 4)
            //                             {
            //                                 PT_BlobTools[m_PatNo, m_SelectBlob + 1].Region = new CogRectangle(BlobTrainRegion);
            //                                 PT_BlobTools[m_PatNo, m_SelectBlob + 1].RunParams.SegmentationParams.HardFixedThreshold = BLOB_TBAR_THRES.Value;
            //                                 PT_BlobTools[m_PatNo, m_PatNo_Blob + 1].RunParams.ConnectivityMinPixels = (int)NUD_MinPixel.Value;
            //                                 PT_BlobTools[m_PatNo, m_PatNo_Blob + 1].RunParams.SegmentationParams.Polarity = (CogBlobSegmentationPolarityConstants)CB_POLARITY_BLOB.SelectedIndex;
            //                             }
            //                         }
            //                     }
            //                     catch (System.Exception ex)
            //                     {
            //                         LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            //                     }
            //                     Main.DisplayFit(PT_Display02);
            //                 }
            //             }
        }
        #endregion

        #region CALIPER 관련
        private void Caliper_Change()
        {
            m_PatchangeFlag = true;
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            RefreshDisplay2();

            CB_DIRECTION.SelectedIndex = Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, m_SelectCaliper].Region.Rotation);
            TBAR_THRES.Value = Convert.ToInt16(PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.ContrastThreshold);
            CB_POLARITY_CALIPER.SelectedIndex = Convert.ToInt16(PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.Edge0Polarity) - 1;
            CB_CALIPER_USE.Checked = PT_CaliPara[m_PatNo, m_SelectCaliper].m_UseCheck;

            if (Main.GetCaliperPairMode(PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.EdgeMode))
                CB_EDGEPAIRCHECK.Checked = true;
            else
                CB_EDGEPAIRCHECK.Checked = false;

            CB_COP_MODE_CHECK.Checked = PT_CaliPara[m_PatNo, m_SelectCaliper].m_bCOPMode;
            if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_bCOPMode)
            {
                label52.Visible = true; LB_DIVIDE_COUNT.Visible = true; BTN_DIVIDECNT_UP.Visible = true; BTN_DIVIDECNT_DOWN.Visible = true;
                label53.Visible = true; LB_DIVIDE_OFFSET.Visible = true; BTN_DIVIDEOFFSET_UP.Visible = true; BTN_DIVIDEOFFSET_DOWN.Visible = true;

                LB_DIVIDE_COUNT.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt.ToString();
                LB_DIVIDE_OFFSET.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset.ToString();
            }
            else
            {
                label52.Visible = false; LB_DIVIDE_COUNT.Visible = false; BTN_DIVIDECNT_UP.Visible = false; BTN_DIVIDECNT_DOWN.Visible = false;
                label53.Visible = false; LB_DIVIDE_OFFSET.Visible = false; BTN_DIVIDEOFFSET_UP.Visible = false; BTN_DIVIDEOFFSET_DOWN.Visible = false;
            }
            //PTCaliperRegion.Changed += new CogChangedEventHandler(PT_COP_Caliper_Redraw);

            Main.DisplayClear(PT_CALIPER_SUB_Display);
            PT_CALIPER_SUB_Display.Image = null;
            DrawCaliperRegion();
            m_PatchangeFlag = false;
            if (PT_DISPLAY_CONTROL.CrossLineChecked) CrossLine();
        }
        private void DrawCaliperRegion()
        {
            if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_CALIPERTOOL)
            {
                DisplayClear();
                PTCaliperRegion = new CogRectangleAffine(PT_CaliperTools[m_PatNo, m_SelectCaliper].Region);
                if (PT_Caliper_MarkUSE[m_PatNo])
                {
                    PTCaliperRegion.CenterX = PatResult.TranslationX + PT_CaliPara[m_PatNo, m_SelectCaliper].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                    PTCaliperRegion.CenterY = PatResult.TranslationY + PT_CaliPara[m_PatNo, m_SelectCaliper].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
                }
                PTCaliperRegion.GraphicDOFEnable = CogRectangleAffineDOFConstants.Position | CogRectangleAffineDOFConstants.Size;
                PTCaliperRegion.Interactive = true;
                PT_Display01.InteractiveGraphics.Add(PTCaliperRegion, "CALIPER", false);

                if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_bCOPMode && PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt > 0)
                {
                    PTCaliperRegion.GraphicDOFEnable |= CogRectangleAffineDOFConstants.Skew;

                    PTCaliperDividedRegion = new CogRectangleAffine[PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt];

                    double dNewX = PTCaliperRegion.CenterX - (PTCaliperRegion.SideXLength / 2) + (PTCaliperRegion.SideXLength / (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * 2));
                    double dNewY = PTCaliperRegion.CenterY;

                    for (int i = 0; i < PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt; i++)
                    {
                        PTCaliperDividedRegion[i] = new CogRectangleAffine(PT_CaliperTools[m_PatNo, m_SelectCaliper].Region);

                        double dX = PTCaliperRegion.SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * i * Math.Cos(PTCaliperRegion.Rotation);
                        double dY = PTCaliperRegion.SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * i * Math.Sin(PTCaliperRegion.Rotation);

                        PTCaliperDividedRegion[i].SideXLength = PTCaliperDividedRegion[i].SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt;
                        PTCaliperDividedRegion[i].CenterX = dNewX + dX;
                        PTCaliperDividedRegion[i].CenterY = dNewY + dY;

                        PT_Display01.StaticGraphics.Add(PTCaliperDividedRegion[i], "CALIPER");
                    }
                }

                Main.DisplayFit(PT_Display01);
            }
        }

        private void PT_COP_Caliper_Redraw(Object sender, EventArgs e)
        {
            double dNewX = PTCaliperRegion.CenterX - (PTCaliperRegion.SideXLength / 2) + (PTCaliperRegion.SideXLength / (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * 2));
            double dNewY = PTCaliperRegion.CenterY;

            for (int i = 0; i < PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt; i++)
            {
                PTCaliperDividedRegion[i] = new CogRectangleAffine(PT_CaliperTools[m_PatNo, m_SelectCaliper].Region);

                double dX = PTCaliperRegion.SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * i * Math.Cos(PTCaliperRegion.Rotation);
                double dY = PTCaliperRegion.SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * i * Math.Sin(PTCaliperRegion.Rotation);

                PTCaliperDividedRegion[i].SideXLength = PTCaliperDividedRegion[i].SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt;
                PTCaliperDividedRegion[i].CenterX = dNewX + dX;
                PTCaliperDividedRegion[i].CenterY = dNewY + dY;

                PT_Display01.StaticGraphics.Add(PTCaliperDividedRegion[i], "CALIPER");
            }
        }

        private void RBTN_CALIPER_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }
        private void BTN_CALIPER_CHANGE_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            int m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));

            if (TempBTN.Checked)
            {
                m_SelectCaliper = m_Number;
                Caliper_Change();
            }
        }
        private void CB_CALIPER_MARKUSE_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox TempBTN = (CheckBox)sender;
            if (TempBTN.Checked)
            {
                TempBTN.BackColor = System.Drawing.Color.GreenYellow;
                PT_Caliper_MarkUSE[m_PatNo] = true;
            }
            else
            {
                TempBTN.BackColor = System.Drawing.Color.White;
                PT_Caliper_MarkUSE[m_PatNo] = false;
            }

            if (!m_TABCHANGE_MODE)
            {
                RefreshDisplay2();
                DrawCaliperRegion();
            }
        }
        public static void DrawLastRegionData(CogRecordDisplay Display, CogCaliperTool CaliperTool)
        {
            try
            {
                Main.DisplayClear(Display);

                CaliperTool.LastRunRecordDiagEnable = CogCaliperLastRunRecordDiagConstants.TransformedRegionPixels;
                CaliperTool.LastRunRecordEnable = CogCaliperLastRunRecordConstants.FilteredProjectionGraph | CogCaliperLastRunRecordConstants.Edges2; //ProjectionGraph

                for (int i = 0; i < CaliperTool.CreateLastRunRecord().SubRecords.Count; i++)
                {
                    if (CaliperTool.CreateLastRunRecord().SubRecords[i].Annotation == "RegionData")
                        Display.Record = CaliperTool.CreateLastRunRecord().SubRecords[i];
                }
                //                  Display.Record = CaliperTool.CreateLastRunRecord();                
                //                  CogAffineTransformTool Copytool = new CogAffineTransformTool();
                //                  Copytool.InputImage = CaliperTool.InputImage;
                //                  Copytool.Region = CaliperTool.Region;               
                //                  Copytool.Run();
                //                  Display.Image = Copytool.OutputImage;
                DisplayFit(Display);
            }
            catch
            {

            }

        }
        private void CALIPER_PAIR_SET()
        {
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {

            }
            else
            {
                double MaxSize = 100;
                switch (Main.GetCaliperDirection(Main.GetCaliperDirection(PT_CaliperTools[m_PatNo, m_SelectCaliper].Region.Rotation)))
                {
                    case Main.DEFINE.X:
                        MaxSize = PT_CaliperTools[m_PatNo, m_SelectCaliper].Region.SideXLength;
                        MaxSize = 5; // 길이고정 17.01.13
                        break;
                    case Main.DEFINE.Y:
                        MaxSize = PT_CaliperTools[m_PatNo, m_SelectCaliper].Region.SideYLength;
                        MaxSize = 12; // 길이고정 17.01.13
                        break;
                    default:
                        MaxSize = MaxSize;
                        break;
                }
                //            MaxSize = MaxSize / 2;
                //            MaxSize = 5; // 길이고정 17.01.13
                PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.Edge0Position = Convert.ToInt16(-MaxSize);
                PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.Edge1Position = Convert.ToInt16(+MaxSize);
            }

            Main.SetCaliperPairPolarity(PT_CaliperTools[m_PatNo, m_SelectCaliper]);
        }
        private void BTN_CALIPER_APPLY_Click(object sender, EventArgs e)
        {
            try
            {
                if (PTCaliperRegion != null)
                {
                    if (PT_Caliper_MarkUSE[m_PatNo])
                    {
                        PT_CaliPara[m_PatNo, m_SelectCaliper].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X = PTCaliperRegion.CenterX - PatResult.TranslationX;
                        PT_CaliPara[m_PatNo, m_SelectCaliper].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y = PTCaliperRegion.CenterY - PatResult.TranslationY;
                    }

                    PT_CaliperTools[m_PatNo, m_SelectCaliper].Region = new CogRectangleAffine(PTCaliperRegion);
                    PT_CaliperTools[m_PatNo, m_SelectCaliper].Region.Rotation = Main.SetCaliperDirection(CB_DIRECTION.SelectedIndex);
                    PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.ContrastThreshold = TBAR_THRES.Value;
                    PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.Edge0Polarity = (CogCaliperPolarityConstants)(CB_POLARITY_CALIPER.SelectedIndex + 1);
                    if (Main.GetCaliperPairMode(PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.EdgeMode))
                        CALIPER_PAIR_SET();
                    else
                        PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.Edge0Position = 0;

                    if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_bCOPMode && PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt > 0)
                    {
                        DisplayClear();
                        PT_Display01.InteractiveGraphics.Add(PTCaliperRegion, "CALIPER", false);
                        PTCaliperRegion.GraphicDOFEnable |= CogRectangleAffineDOFConstants.Skew;

                        PTCaliperDividedRegion = new CogRectangleAffine[PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt];

                        double dNewX = PTCaliperRegion.CenterX - (PTCaliperRegion.SideXLength / 2) + (PTCaliperRegion.SideXLength / (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * 2));
                        double dNewY = PTCaliperRegion.CenterY;

                        for (int i = 0; i < PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt; i++)
                        {
                            PTCaliperDividedRegion[i] = new CogRectangleAffine(PT_CaliperTools[m_PatNo, m_SelectCaliper].Region);

                            double dX = PTCaliperRegion.SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * i * Math.Cos(PTCaliperRegion.Rotation);
                            double dY = PTCaliperRegion.SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt * i * Math.Sin(PTCaliperRegion.Rotation);

                            PTCaliperDividedRegion[i].SideXLength = PTCaliperDividedRegion[i].SideXLength / PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt;
                            PTCaliperDividedRegion[i].CenterX = dNewX + dX;
                            PTCaliperDividedRegion[i].CenterY = dNewY + dY;

                            PT_Display01.StaticGraphics.Add(PTCaliperDividedRegion[i], "CALIPER");
                        }
                    }

                    LABEL_MESSAGE(LB_MESSAGE, "Register OK", System.Drawing.Color.Lime);
                }
                else
                    LABEL_MESSAGE(LB_MESSAGE, "Select!!! Caliper", System.Drawing.Color.Red);
            }
            catch (System.Exception ex)
            {
                LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            }

            Main.DisplayFit(PT_Display01);
        }
        private void TBAR_THRES_ValueChanged(object sender, EventArgs e)
        {
            PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.ContrastThreshold = TBAR_THRES.Value;
            LB_THRES.Text = TBAR_THRES.Value.ToString();
            if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_CALIPERTOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void BTN_THRES_UP_Click(object sender, EventArgs e)
        {
            if (TBAR_THRES.Maximum == TBAR_THRES.Value) return;
            TBAR_THRES.Value++;
        }
        private void BTN_THRES_DN_Click(object sender, EventArgs e)
        {
            if (TBAR_THRES.Minimum == TBAR_THRES.Value) return;
            TBAR_THRES.Value--;
        }
        private void CB_POLARITY_CALIPER_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.Edge0Polarity = (CogCaliperPolarityConstants)(CB_POLARITY_CALIPER.SelectedIndex + 1);
        }
        private void CB_DIRECTION_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PT_CaliperTools[m_PatNo, m_SelectCaliper].Region.Rotation = Main.SetCaliperDirection(CB_DIRECTION.SelectedIndex);
            if (!m_TABCHANGE_MODE)
            {
                DrawCaliperRegion();
            }
        }
        private void CB_USECHECK_Click(object sender, EventArgs e)
        {
            if (CB_CALIPER_USE.Checked)
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_UseCheck = true;
            }
            else
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_UseCheck = false;
            }
        }
        private void CB_USECHECK_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_CALIPER_USE.Checked)
            {

                CB_CALIPER_USE.BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                CB_CALIPER_USE.BackColor = System.Drawing.Color.DarkGray;
            }
        }
        private void CB_EDGEPAIRCHECK_Click(object sender, EventArgs e)
        {
            if (CB_EDGEPAIRCHECK.Checked)
            {
                //                PT_CaliPara[m_PatNo, m_SelectCaliper].m_UsePairCheck = true;
                PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                CALIPER_PAIR_SET();
            }
            else
            {
                //                PT_CaliPara[m_PatNo, m_SelectCaliper].m_UsePairCheck = false;
                PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
                PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.Edge0Position = 0;
            }
        }
        private void CB_EDGEPAIRCHECK_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_EDGEPAIRCHECK.Checked)
            {
                CB_EDGEPAIRCHECK.BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                CB_EDGEPAIRCHECK.BackColor = System.Drawing.Color.DarkGray;
            }
        }
        #endregion

        #region FINDLine 관련
        private void FINDLINE_Change()
        {
            m_PatchangeFlag = true;
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            m_CamNo = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo;
            Light_Select();
            RefreshDisplay2();

            PT_FindLineTool = new CogFindLineTool(PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine]);
            PT_LineMaxTool = new CogLineMaxTool(PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine]);
            PT_FindLineTool.RunParams.Changed += new Cognex.VisionPro.CogChangedEventHandler(PT_LineSegment_Change);
            PT_LineMaxTool.RunParams.ExpectedLineNormal.Changed += new Cognex.VisionPro.CogChangedEventHandler(PT_LineSegment_Change);
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
            {
                // Direction
                label26.Visible = true; BTN_FINDLINE_REVERSE.Visible = false;
                // Caliper Count
                label20.Visible = false; LB_FINDLINE_CNT.Visible = false; BTN_FINDLINE_CNT_UP.Visible = false; BTN_FINDLINE_CNT_DN.Visible = false;
                // Ignore Count
                label24.Visible = false; NUD_IGNORE_CNT.Visible = false;
                // Filter Half Size
                label28.Visible = false; NUD_FILTERHALFSIZE.Visible = false;
                // Caliper Method
                label48.Visible = false; RBTN_CALIPER_METHOD_SCORE.Visible = false; RBTN_CALIPER_METHOD_POS.Visible = false;

                label37.Visible = true; NUD_ANGLE_TOLERANCE.Visible = true;
                label39.Visible = true; NUD_DIST_TOLERANCE.Visible = true;
                //label40.Visible = true; NUD_MAX_LINENUM.Visible = true; label40.Location = new System.Drawing.Point(267, 255); NUD_MAX_LINENUM.Location = new System.Drawing.Point(368, 256);
                label41.Visible = true; NUD_LINE_ANGLE_TOL.Visible = true;
                label42.Visible = true; NUD_COVERAGE_THRES.Visible = true;
                label43.Visible = true; NUD_LENGTH_THRES.Visible = true;
                label44.Visible = true; NUD_GRADIENT_KERNEL_SIZE.Visible = true; label44.Location = new System.Drawing.Point(267, 169); NUD_GRADIENT_KERNEL_SIZE.Location = new System.Drawing.Point(368, 169);
                label45.Visible = true; NUD_PROJECTION_LENGTH.Visible = true; label45.Location = new System.Drawing.Point(267, 213); NUD_PROJECTION_LENGTH.Location = new System.Drawing.Point(368, 214);
                NUD_LINE_NORMAL_ANGLE.Visible = true; NUD_LINE_NORMAL_ANGLE.Location = new System.Drawing.Point(368, 85);

                try
                {
                    TBAR_FINDLINE_THRES.Value = Convert.ToInt16(PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeDetectionParams.ContrastThreshold);
                    CB_FINDLINE_POLARITY.SelectedIndex = Convert.ToInt16(PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.Polarity) - 1;    // CogLineMaxPolarityConstants
                    NUD_LINE_NORMAL_ANGLE.Value = (decimal)(int)(PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.ExpectedLineNormal.Angle * Main.DEFINE.degree);
                    NUD_GRADIENT_KERNEL_SIZE.Value = (decimal)PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeDetectionParams.GradientKernelSizeInPixels;
                    NUD_PROJECTION_LENGTH.Value = (decimal)PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeDetectionParams.ProjectionLengthInPixels;
                    NUD_MAX_LINENUM.Value = (decimal)PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.MaxNumLines;
                    CB_FINDLINE_USE.Checked = PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck;
                    NUD_ANGLE_TOLERANCE.Value = (decimal)(int)(PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeAngleTolerance * Main.DEFINE.degree);
                    NUD_DIST_TOLERANCE.Value = (decimal)PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.DistanceTolerance;
                    NUD_LINE_ANGLE_TOL.Value = (decimal)(int)(PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.LineAngleTolerance * Main.DEFINE.degree);
                    NUD_COVERAGE_THRES.Value = (decimal)PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CoverageThreshold;
                    NUD_LENGTH_THRES.Value = (decimal)PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.LengthThreshold;

                    if (PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.MaxNumLines > 1)
                    {
                        //label46.Visible = true; RBTN_HORICON_YMIN.Visible = true; RBTN_HORICON_YMAX.Visible = true;
                        //label47.Visible = true; RBTN_VERTICON_XMIN.Visible = true; RBTN_VERTICON_XMAX.Visible = true;

                        //RBTN_LINEMAX_H_COND[PT_FindLinePara[m_PatNo, m_SelectFindLine].m_LineMaxHCond].Checked = true;
                        //RBTN_LINEMAX_V_COND[PT_FindLinePara[m_PatNo, m_SelectFindLine].m_LineMaxVCond].Checked = true;
                    }
                    else
                    {
                        //label46.Visible = false; RBTN_HORICON_YMIN.Visible = false; RBTN_HORICON_YMAX.Visible = false;
                        //label47.Visible = false; RBTN_VERTICON_XMIN.Visible = false; RBTN_VERTICON_XMAX.Visible = false;
                    }
                }
                catch (System.ArgumentException)
                {

                }
            }
            else
            {
                // Direction
                label26.Visible = true; BTN_FINDLINE_REVERSE.Visible = true;
                // Caliper Count
                label20.Visible = true; LB_FINDLINE_CNT.Visible = true; BTN_FINDLINE_CNT_UP.Visible = true; BTN_FINDLINE_CNT_DN.Visible = true;
                // Ignore Count
                label24.Visible = true; NUD_IGNORE_CNT.Visible = true;
                // Filter Half Size
                label28.Visible = true; NUD_FILTERHALFSIZE.Visible = true;
                // Caliper Method
                label48.Visible = true; RBTN_CALIPER_METHOD_SCORE.Visible = true; RBTN_CALIPER_METHOD_POS.Visible = true;
                label48.Location = new System.Drawing.Point(267, 383); RBTN_CALIPER_METHOD_SCORE.Location = new System.Drawing.Point(367, 382); RBTN_CALIPER_METHOD_POS.Location = new System.Drawing.Point(505, 382);

                label37.Visible = false; NUD_ANGLE_TOLERANCE.Visible = false;
                label39.Visible = false; NUD_DIST_TOLERANCE.Visible = false;
                //label40.Visible = false; NUD_MAX_LINENUM.Visible = false;
                label41.Visible = false; NUD_LINE_ANGLE_TOL.Visible = false;
                label42.Visible = false; NUD_COVERAGE_THRES.Visible = false;
                label43.Visible = false; NUD_LENGTH_THRES.Visible = false;
                label44.Visible = false; NUD_GRADIENT_KERNEL_SIZE.Visible = false;
                label45.Visible = false; NUD_PROJECTION_LENGTH.Visible = false;
                NUD_LINE_NORMAL_ANGLE.Visible = false;

                try
                {
                    TBAR_FINDLINE_THRES.Value = Convert.ToInt16(PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.ContrastThreshold);
                    CB_FINDLINE_POLARITY.SelectedIndex = Convert.ToInt16(PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge0Polarity) - 1;
                    LB_FINDLINE_CNT.Text = PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.NumCalipers.ToString();
                    CB_FINDLINE_USE.Checked = PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck;
                    CB_FINDLINE_PAIR_USE.Checked = PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UsePairCheck;
                    NUD_FILTERHALFSIZE.Value = (decimal)PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.FilterHalfSizeInPixels;
                    NUD_IGNORE_CNT.Value = (decimal)PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.NumToIgnore;
                    if (PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UsePairCheck == true)
                    {
                        CB_FINDLINE_1_POLARITY.SelectedIndex = Convert.ToInt16(PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge1Polarity) - 1;
                    }

                    if (PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineCaliperMethod == Main.DEFINE.CLP_METHOD_SCORE)
                    {
                        RBTN_CALIPER_METHOD_SCORE.Checked = true;
                    }
                    else if (PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineCaliperMethod == Main.DEFINE.CLP_METHOD_POS)
                    {
                        RBTN_CALIPER_METHOD_POS.Checked = true;
                    }
                }
                catch (System.ArgumentException)
                {

                }
            }

            if (m_SelectFindLine >= 2)
            {
                CB_FINDLINE_SUBLINE.Enabled = false;
                CB_FINDLINE_SUBLINE.SelectedIndex = m_LineSubNo = 0;
            }
            else
                CB_FINDLINE_SUBLINE.Enabled = true;

            PT_DISPLAY_CONTROL.Resuloution = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CalX[0];
            Main.DisplayClear(PT_FINDLINE_SUB_Display);
            PT_FINDLINE_SUB_Display.Image = null;
            DrawFINDLineRegion();
            m_PatchangeFlag = false;
            PT_DISPLAY_CONTROL.DisplayFit();
            timer2.Enabled = true;
        }
        private void DrawFINDLineRegion()
        {
            if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_FINDLINETOOL)
            {
                DisplayClear();
                if (PT_FindLine_MarkUSE[m_PatNo])
                {
                    if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                    {
                        (PT_LineMaxTool.Region as CogRectangleAffine).CenterX = PatResult.TranslationX + PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                        (PT_LineMaxTool.Region as CogRectangleAffine).CenterY = PatResult.TranslationY + PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
                    }
                    else
                    {
                        PT_FindLineTool.RunParams.ExpectedLineSegment.StartX = PatResult.TranslationX + PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                        PT_FindLineTool.RunParams.ExpectedLineSegment.StartY = PatResult.TranslationY + PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;

                        PT_FindLineTool.RunParams.ExpectedLineSegment.EndX = PatResult.TranslationX + PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X2;
                        PT_FindLineTool.RunParams.ExpectedLineSegment.EndY = PatResult.TranslationY + PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y2;
                    }
                }

                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                {
                    PT_LineMaxTool.CurrentRecordEnable = CogLineMaxCurrentRecordConstants.All;
                    (PT_LineMaxTool.Region as CogRectangleAffine).GraphicDOFEnable = CogRectangleAffineDOFConstants.Position | CogRectangleAffineDOFConstants.Size;
                    (PT_LineMaxTool.Region as CogRectangleAffine).Interactive = true;
                    //PT_Display01.InteractiveGraphics.Add(PTCaliperRegion, "CALIPER", false);
                    PT_Display01.Record = PT_LineMaxTool.CreateCurrentRecord();
                }
                else
                {
                    PT_FindLineTool.CurrentRecordEnable = CogFindLineCurrentRecordConstants.All;
                    PT_Display01.Record = PT_FindLineTool.CreateCurrentRecord();
                }
                Main.DisplayFit(PT_Display01);
            }
        }
        private void RBTN_Button_Color_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }
        private void RBTN_FINDLINE_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;

            if (TempBTN.Name == "RBTN_FINDLINE_CIRCLE") // 200624 JHKIM 원호 추가
            {
                TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDCIRCLETOOL;
                Circle_Change();
            }
            else
            {
                int m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));

                if (TempBTN.Checked)
                {
                    m_SelectFindLine = m_Number;
                    FINDLINE_Change();
                }
            }
        }
        private void CB_FINDLINE_MARK_USE_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox TempBTN = (CheckBox)sender;
            if (TempBTN.Checked)
            {
                TempBTN.BackColor = System.Drawing.Color.GreenYellow;
                PT_FindLine_MarkUSE[m_PatNo] = true;
            }
            else
            {
                TempBTN.BackColor = System.Drawing.Color.White;
                PT_FindLine_MarkUSE[m_PatNo] = false;
            }
            if (!m_TABCHANGE_MODE)
            {
                RefreshDisplay2();
                DrawFINDLineRegion();
            }
        }
        public static void DrawFINDLineLastRegionData(CogRecordDisplay Display, CogFindLineTool FINDLineTool)
        {
            try
            {
                Main.DisplayClear(Display);
                CogFindLineTool tempTool = new CogFindLineTool(FINDLineTool);

                tempTool.LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.All;
                tempTool.LastRunRecordEnable = CogFindLineLastRunRecordConstants.FilteredProjectionGraph | CogFindLineLastRunRecordConstants.BestFitLine;
                Display.Record = tempTool.CreateLastRunRecord();
                DisplayFit(Display);
            }
            catch
            {

            }
        }
        public static void DrawFINDLineLastRegionData(CogRecordDisplay Display, CogLineMaxTool LineMaxTool)
        {
            try
            {
                Main.DisplayClear(Display);
                CogLineMaxTool tempTool = new CogLineMaxTool(LineMaxTool);

                tempTool.LastRunRecordDiagEnable = CogLineMaxLastRunRecordDiagConstants.All;
                tempTool.LastRunRecordEnable = CogLineMaxLastRunRecordConstants.All;
                Display.Record = tempTool.CreateLastRunRecord();
                DisplayFit(Display);
            }
            catch
            {

            }

        }
        private void BTN_FINDLINE_APPLY_Click(object sender, EventArgs e)
        {
            try
            {
                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                {
                    if (PT_FindLine_MarkUSE[m_PatNo])
                    {
                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X = (PT_LineMaxTool.Region as CogRectangleAffine).CenterX - PatResult.TranslationX;
                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y = (PT_LineMaxTool.Region as CogRectangleAffine).CenterY - PatResult.TranslationY;
                    }

                    if (m_SelectFindLine == 2 && PT_FindLinePara[m_PatNo, m_LineSubNo, 0].m_UseCheck == true && PT_FindLinePara[m_PatNo, m_LineSubNo, 1].m_UseCheck)
                    {
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Run();
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Run();

                        int nLineIdx1 = 0, nLineIdx2 = 0;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(0, PT_FindLinePara[m_PatNo, m_LineSubNo, 0], PT_LineMaxTools[m_PatNo, m_LineSubNo, 0], ref nLineIdx1);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(1, PT_FindLinePara[m_PatNo, m_LineSubNo, 1], PT_LineMaxTools[m_PatNo, m_LineSubNo, 1], ref nLineIdx2);

                        if ((PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results[0].GetLine() != null)
                            && (PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results != null && PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results.Count > 0 && PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results[0].GetLine() != null))
                        {
                            PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                            PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                            PT_LineLineCrossPoints[0].LineA = PT_LineMaxTools[m_PatNo, m_LineSubNo, 0].Results[nLineIdx1].GetLine();
                            PT_LineLineCrossPoints[0].LineB = PT_LineMaxTools[m_PatNo, m_LineSubNo, 1].Results[nLineIdx2].GetLine();
                            PT_LineLineCrossPoints[0].Run();
                            if (PT_LineLineCrossPoints[0].Intersects)
                            {
                                if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                                {
                                    Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                    PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X = (PT_LineMaxTool.Region as CogRectangleAffine).CenterX - PT_LineLineCrossPoints[0].X;
                                    PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y = (PT_LineMaxTool.Region as CogRectangleAffine).CenterY - PT_LineLineCrossPoints[0].Y;
                                }
                            }
                        }
                    }

                    PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine] = new CogLineMaxTool(PT_LineMaxTool);

                    try
                    {
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeDetectionParams.ContrastThreshold = TBAR_FINDLINE_THRES.Value;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.Polarity = (CogLineMaxPolarityConstants)(CB_FINDLINE_POLARITY.SelectedIndex + 1);
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.ExpectedLineNormal.Angle = (double)NUD_LINE_NORMAL_ANGLE.Value * Main.DEFINE.radian;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeDetectionParams.GradientKernelSizeInPixels = (int)NUD_GRADIENT_KERNEL_SIZE.Value;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeDetectionParams.ProjectionLengthInPixels = (int)NUD_PROJECTION_LENGTH.Value;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.MaxNumLines = (int)NUD_MAX_LINENUM.Value;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeAngleTolerance = (double)NUD_ANGLE_TOLERANCE.Value * Main.DEFINE.radian;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.DistanceTolerance = (int)NUD_DIST_TOLERANCE.Value;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.LineAngleTolerance = (double)NUD_LINE_ANGLE_TOL.Value * Main.DEFINE.radian;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CoverageThreshold = (double)NUD_COVERAGE_THRES.Value;
                        PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.LengthThreshold = (int)NUD_LENGTH_THRES.Value;

                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck = CB_FINDLINE_USE.Checked;
                    }
                    catch (System.ArgumentException)
                    {

                    }

                    LABEL_MESSAGE(LB_MESSAGE, "LineMax Register OK", System.Drawing.Color.Lime);
                }   // LineMaxTool
                else if (PT_FindLineTools != null)
                {
                    if (PT_FindLine_MarkUSE[m_PatNo])
                    {
                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X = PT_FindLineTool.RunParams.ExpectedLineSegment.StartX - PatResult.TranslationX;
                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y = PT_FindLineTool.RunParams.ExpectedLineSegment.StartY - PatResult.TranslationY;

                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X2 = PT_FindLineTool.RunParams.ExpectedLineSegment.EndX - PatResult.TranslationX;
                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y2 = PT_FindLineTool.RunParams.ExpectedLineSegment.EndY - PatResult.TranslationY;
                    }

                    if (m_SelectFindLine == 2 && PT_FindLinePara[m_PatNo, m_LineSubNo, 0].m_UseCheck == true && PT_FindLinePara[m_PatNo, m_LineSubNo, 1].m_UseCheck)
                    {
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Run();
                        PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Run();

                        if ((PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results.GetLine() != null)
                            && (PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results != null && PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results.Count > 0 && PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results.GetLine() != null))
                        {
                            PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                            PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                            PT_LineLineCrossPoints[0].LineA = PT_FindLineTools[m_PatNo, m_LineSubNo, 0].Results.GetLine();
                            PT_LineLineCrossPoints[0].LineB = PT_FindLineTools[m_PatNo, m_LineSubNo, 1].Results.GetLine();
                            PT_LineLineCrossPoints[0].Run();
                            if (PT_LineLineCrossPoints[0].Intersects)
                            {
                                if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                                {
                                    Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                    PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X = PT_FindLineTool.RunParams.ExpectedLineSegment.StartX - PT_LineLineCrossPoints[0].X;
                                    PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y = PT_FindLineTool.RunParams.ExpectedLineSegment.StartY - PT_LineLineCrossPoints[0].Y;

                                    PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X2 = PT_FindLineTool.RunParams.ExpectedLineSegment.EndX - PT_LineLineCrossPoints[0].X;
                                    PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y2 = PT_FindLineTool.RunParams.ExpectedLineSegment.EndY - PT_LineLineCrossPoints[0].Y;
                                }
                            }
                        }
                    }

                    PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine] = new CogFindLineTool(PT_FindLineTool);

                    try
                    {
                        PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.ContrastThreshold = TBAR_FINDLINE_THRES.Value;
                        PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(CB_FINDLINE_POLARITY.SelectedIndex + 1);
                        PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.NumCalipers = Convert.ToInt16(LB_FINDLINE_CNT.Text);
                        PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.FilterHalfSizeInPixels = (int)NUD_FILTERHALFSIZE.Value;
                        PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.NumToIgnore = (int)NUD_IGNORE_CNT.Value;
                        if (PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UsePairCheck == true)
                            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)(CB_FINDLINE_1_POLARITY.SelectedIndex + 1);

                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck = CB_FINDLINE_USE.Checked;
                        PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UsePairCheck = CB_FINDLINE_PAIR_USE.Checked;

                        if (RBTN_CALIPER_METHOD_SCORE.Checked == true)
                        {
                            PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineCaliperMethod = Main.DEFINE.CLP_METHOD_SCORE;
                            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                            CogCaliperScorerContrast scorer = new CogCaliperScorerContrast();
                            scorer.Enabled = true;
                            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.SingleEdgeScorers.Add(scorer);
                        }
                        else if (RBTN_CALIPER_METHOD_POS.Checked == true)
                        {
                            PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineCaliperMethod = Main.DEFINE.CLP_METHOD_POS;
                            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                            CogCaliperScorerPosition scorer = new CogCaliperScorerPosition();
                            scorer.Enabled = true;
                            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.SingleEdgeScorers.Add(scorer);
                        }
                    }
                    catch (System.ArgumentException)
                    {

                    }

                    LABEL_MESSAGE(LB_MESSAGE, "FindLine Register OK", System.Drawing.Color.Lime);
                }   // PT_FineLineTools != null
                else
                    LABEL_MESSAGE(LB_MESSAGE, "Select!!! FINDLine", System.Drawing.Color.Red);
            }
            catch (System.Exception ex)
            {
                LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            }

            Main.DisplayFit(PT_Display01);
        }

        #region
        private void TBAR_FINDLINE_THRES_ValueChanged(object sender, EventArgs e)
        {
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.EdgeDetectionParams.ContrastThreshold = TBAR_FINDLINE_THRES.Value;
            else
                PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.ContrastThreshold = TBAR_FINDLINE_THRES.Value;
            LB_FINDLINE_THRES.Text = TBAR_FINDLINE_THRES.Value.ToString();
            if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_FINDLINETOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void BTN_FINDLINE_THRES_UP_Click(object sender, EventArgs e)
        {
            if (TBAR_FINDLINE_THRES.Maximum == TBAR_FINDLINE_THRES.Value) return;

            TBAR_FINDLINE_THRES.Value++;
        }
        private void BTN_FINDLINE_THRES_DN_Click(object sender, EventArgs e)
        {
            if (TBAR_FINDLINE_THRES.Minimum == TBAR_FINDLINE_THRES.Value) return;

            TBAR_FINDLINE_THRES.Value--;
        }
        private void NUD_GUIDEDISX_ValueChanged(object sender, EventArgs e)
        {
            PT_TRAY_GUIDE_DISX[m_PatNo] = Convert.ToInt32(NUD_GUIDEDISX.Value);
            if (!NUD_Initial_Flag) FirstPocketPos.X = PT_TRAY_GUIDE_DISX[m_PatNo];
        }
        private void NUD_GUIDEDISY_ValueChanged(object sender, EventArgs e)
        {
            PT_TRAY_GUIDE_DISY[m_PatNo] = Convert.ToInt32(NUD_GUIDEDISY.Value);
            if (!NUD_Initial_Flag) FirstPocketPos.Y = PT_TRAY_GUIDE_DISY[m_PatNo];
        }
        private void NUD_PITCHDISX_ValueChanged(object sender, EventArgs e)
        {
            PT_TRAY_PITCH_DISX[m_PatNo] = Convert.ToInt32(NUD_PITCHDISX.Value);
            if (!NUD_Initial_Flag) X_PocketPitchPos.X = FirstPocketPos.X + PT_TRAY_PITCH_DISX[m_PatNo];
        }
        private void NUD_PITCHDISY_ValueChanged(object sender, EventArgs e)
        {
            PT_TRAY_PITCH_DISY[m_PatNo] = Convert.ToInt32(NUD_PITCHDISY.Value);
            if (!NUD_Initial_Flag) Y_PocketPitchPos.Y = FirstPocketPos.Y + PT_TRAY_PITCH_DISY[m_PatNo];
        }
        private void PT_LineSegment_Change(Object sender, EventArgs e)
        {
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
            {
                NUD_LINE_NORMAL_ANGLE.Value = (decimal)(int)(PT_LineMaxTool.RunParams.ExpectedLineNormal.Angle * Main.DEFINE.degree);
            }
        }
        private void CB_FINDLINE_POLARITY_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                PT_LineMaxTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.Polarity = (CogLineMaxPolarityConstants)(CB_FINDLINE_POLARITY.SelectedIndex + 1);
            else
                PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(CB_FINDLINE_POLARITY.SelectedIndex + 1);
        }
        private void CB_FINDLINE_1_POLARITY_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)(CB_FINDLINE_1_POLARITY.SelectedIndex + 1);
        }
        private void CB_FINDLINE_USE_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_FINDLINE_USE.Checked)
            {
                CB_FINDLINE_USE.BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                CB_FINDLINE_USE.BackColor = System.Drawing.Color.DarkGray;
            }
        }
        private void CB_FINDLINE_USE_Click(object sender, EventArgs e)
        {
            if (CB_FINDLINE_USE.Checked)
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck = true;
            }
            else
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UseCheck = false;
            }
        }
        private void CB_FINDLINE_PAIR_USE_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_FINDLINE_PAIR_USE.Checked)
            {
                CB_FINDLINE_1_POLARITY.Enabled = true;
                PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                CB_FINDLINE_PAIR_USE.BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                CB_FINDLINE_1_POLARITY.Enabled = false;
                PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
                CB_FINDLINE_PAIR_USE.BackColor = System.Drawing.Color.DarkGray;
            }
        }
        private void CB_FINDLINE_PAIR_USE_Click(object sender, EventArgs e)
        {
            if (CB_FINDLINE_PAIR_USE.Checked)
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UsePairCheck = true;
            }
            else
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_UsePairCheck = false;
            }
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge0Position = (double)CB_FINDLINE_POSITION.Value / -2;
            PT_FindLineTools[m_PatNo, m_LineSubNo, m_SelectFindLine].RunParams.CaliperRunParams.Edge1Position = (double)CB_FINDLINE_POSITION.Value / 2;

            if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_FINDLINETOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void BTN_FINDLINE_REVERSE_Click(object sender, EventArgs e)
        {
            //      PT_FindLineTools[m_PatNo, m_SelectFindLine].RunParams.CaliperSearchDirection *= (-1);
            PT_FindLineTool.RunParams.CaliperSearchDirection *= (-1);
            DrawFINDLineRegion();
        }
        private void NUD_IGNORE_CNT_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //  PT_FindLineTools[m_PatNo, m_SelectFindLine].RunParams.NumToIgnore = (int)NUD_IGNORE_CNT.Value;
                PT_FindLineTool.RunParams.NumToIgnore = (int)NUD_IGNORE_CNT.Value;
            }
            catch (System.ArgumentException)
            {

            }
            if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_FINDLINETOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void NUD_FILTERHALFSIZE_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //     PT_FindLineTools[m_PatNo, m_SelectFindLine].RunParams.CaliperRunParams.FilterHalfSizeInPixels = (int)NUD_FILTERHALFSIZE.Value;
                PT_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = (int)NUD_FILTERHALFSIZE.Value;
            }
            catch (System.ArgumentException)
            {

            }
            if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_FINDLINETOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void BTN_FINDLINE_CNT_UP_Click(object sender, EventArgs e)
        {
            try
            {
                // PT_FindLineTools[m_PatNo, m_SelectFindLine].RunParams.NumCalipers++;
                // LB_FINDLINE_CNT.Text = PT_FindLineTools[m_PatNo, m_SelectFindLine].RunParams.NumCalipers.ToString();

                PT_FindLineTool.RunParams.NumCalipers++;
                LB_FINDLINE_CNT.Text = PT_FindLineTool.RunParams.NumCalipers.ToString();
            }
            catch (System.ArgumentException ex)
            {

            }
            DrawFINDLineRegion();
        }
        private void BTN_FINDLINE_CNT_DN_Click(object sender, EventArgs e)
        {
            try
            {
                // PT_FindLineTools[m_PatNo, m_SelectFindLine].RunParams.NumCalipers--;
                // LB_FINDLINE_CNT.Text = PT_FindLineTools[m_PatNo, m_SelectFindLine].RunParams.NumCalipers.ToString();

                PT_FindLineTool.RunParams.NumCalipers--;
                LB_FINDLINE_CNT.Text = PT_FindLineTool.RunParams.NumCalipers.ToString();
            }
            catch (System.ArgumentException ex)
            {
            }
            DrawFINDLineRegion();

        }
        #endregion
        #endregion

        #region CIRCLE
        private void RBTN_CIRCLE_Click(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            if (TempBTN.Name != "RBTN_CIRCLE00") // 200624 JHKIM 원호 추가
            {
                TABC_MANU.SelectedIndex = Main.DEFINE.M_FINDLINETOOL;
                FINDLINE_Change();
            }
            else
            {
                int m_Number = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));

                if (TempBTN.Checked)
                {
                    m_SelectCircle = m_Number;
                    m_PatchangeFlag = true;
                    Circle_Change();

                    CB_CIRCLE_USE.Checked = PT_CirclePara[m_PatNo, m_SelectCircle].m_UseCheck;
                    m_PatchangeFlag = false;
                }
            }
        }
        private void Circle_Change()
        {
            m_PatchangeFlag = true;
            LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Lime);
            RefreshDisplay2();
            PT_CircleTool = new CogFindCircleTool(PT_CircleTools[m_PatNo, m_SelectCircle]);
            PT_CircleTool.RunParams.Changed += new Cognex.VisionPro.CogChangedEventHandler(PT_Circle_params_Change);


            CB_CIRCLE_USE.Checked = PT_CirclePara[m_PatNo, m_SelectCircle].m_UseCheck;

            CB_DIRECTION_CIR.SelectedIndex = Convert.ToInt16(PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperSearchDirection);
            TBAR_THRES_CIR.Value = Convert.ToInt16(PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.ContrastThreshold);
            CB_POLARITY_CIR.SelectedIndex = Convert.ToInt16(PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.Edge0Polarity) - 1;
            LB_CIRCLE_CNT.Text = PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.NumCalipers.ToString();
            NUD_CIRCLE_IGNCNT.Value = (decimal)PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.NumToIgnore;
            LB_SEARCH_CIR.Text = PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperSearchLength.ToString();
            LB_PROJECTION_CIR.Text = PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperProjectionLength.ToString();
            LB_RADIUS_CIR.Text = PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.ExpectedCircularArc.Radius.ToString();

            if (PT_CirclePara[m_PatNo, m_SelectCircle].m_CircleCaliperMethod == Main.DEFINE.CLP_METHOD_SCORE)
            {
                RBTN_CIR_CALIPER_METHOD_SCORE.Checked = true;
            }
            else if (PT_CirclePara[m_PatNo, m_SelectCircle].m_CircleCaliperMethod == Main.DEFINE.CLP_METHOD_POS)
            {
                RBTN_CIR_CALIPER_METHOD_POS.Checked = true;
            }

            PT_CIRCLE_SUB_Display.Image = null;
            Main.DisplayClear(PT_CIRCLE_SUB_Display);
            DrawCircleRegion();
            m_PatchangeFlag = false;
        }
        private void DrawCircleRegion()
        {
            if (TABC_MANU.SelectedIndex == Main.DEFINE.M_FINDCIRCLETOOL)
            {
                DisplayClear();
                if (PT_Circle_MarkUSE[m_PatNo])
                {
                    PT_CircleTool.RunParams.ExpectedCircularArc.CenterX = PatResult.TranslationX + PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X;
                    PT_CircleTool.RunParams.ExpectedCircularArc.CenterY = PatResult.TranslationY + PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y;
                }
                PT_CircleTool.RunParams.ExpectedCircularArc.Color = CogColorConstants.Purple;
                PT_CircleTool.RunParams.ExpectedCircularArc.DragColor = CogColorConstants.Purple;
                PT_Display01.Record = PT_CircleTool.CreateCurrentRecord();
                Main.DisplayFit(PT_Display01);
            }
        }
        public static void DrawLastRegionData(CogRecordDisplay Display, CogFindCircleTool CircleTool)
        {
            try
            {
                Main.DisplayClear(Display);

                for (int i = 0; i < CircleTool.CreateLastRunRecord().SubRecords.Count; i++)
                {
                    if (CircleTool.CreateLastRunRecord().SubRecords[i].Annotation == "RegionData_Caliper0")
                        Display.Record = CircleTool.CreateLastRunRecord().SubRecords[i];
                }
                Main.DisplayFit(Display);
            }
            catch
            {

            }

        }
        private void PT_Circle_params_Change(Object sender, EventArgs e)
        {
            LB_SEARCH_CIR.Text = PT_CircleTool.RunParams.CaliperSearchLength.ToString();
            LB_PROJECTION_CIR.Text = PT_CircleTool.RunParams.CaliperProjectionLength.ToString();
            LB_RADIUS_CIR.Text = PT_CircleTool.RunParams.ExpectedCircularArc.Radius.ToString();
        }
        private void BTN_CIRCLE_APPLY_Click(object sender, EventArgs e)
        {
            try
            {
                if (PT_CircleTool != null)
                {
                    if (PT_Circle_MarkUSE[m_PatNo])
                    {
                        PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].X = PT_CircleTool.RunParams.ExpectedCircularArc.CenterX - PatResult.TranslationX;
                        PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_CNLSEARCHTOOL].Y = PT_CircleTool.RunParams.ExpectedCircularArc.CenterY - PatResult.TranslationY;
                    }

                    if (PT_CirclePara[m_PatNo, m_SelectCircle].m_UseCheck)
                    {
                        if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_UseLineMax)
                        {
                            for (int k = 0; k < Main.DEFINE.SUBLINE_MAX; k++)
                            {
                                if (PT_FindLinePara[m_PatNo, k, 0].m_UseCheck && PT_FindLinePara[m_PatNo, k, 1].m_UseCheck)
                                {
                                    PT_LineMaxTools[m_PatNo, k, 0].Run();
                                    PT_LineMaxTools[m_PatNo, k, 1].Run();

                                    int nLineIdx1 = 0, nLineIdx2 = 0;

                                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(0, PT_FindLinePara[m_PatNo, k, 0], PT_LineMaxTools[m_PatNo, k, 0], ref nLineIdx1);
                                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].GetLineMaxIndex(1, PT_FindLinePara[m_PatNo, k, 1], PT_LineMaxTools[m_PatNo, k, 1], ref nLineIdx2);

                                    if ((PT_LineMaxTools[m_PatNo, k, 0].Results != null && PT_LineMaxTools[m_PatNo, k, 0].Results.Count > 0 && PT_LineMaxTools[m_PatNo, k, 0].Results[0].GetLine() != null)
                                        && (PT_LineMaxTools[m_PatNo, k, 1].Results != null && PT_LineMaxTools[m_PatNo, k, 1].Results.Count > 0 && PT_LineMaxTools[m_PatNo, k, 1].Results[0].GetLine() != null))
                                    {
                                        PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                                        PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                                        PT_LineLineCrossPoints[0].LineA = PT_LineMaxTools[m_PatNo, k, 0].Results[nLineIdx1].GetLine();
                                        PT_LineLineCrossPoints[0].LineB = PT_LineMaxTools[m_PatNo, k, 1].Results[nLineIdx2].GetLine();
                                        PT_LineLineCrossPoints[0].Run();
                                        if (PT_LineLineCrossPoints[0].Intersects)
                                        {
                                            if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                                            {
                                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                                PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X = PT_CircleTool.RunParams.ExpectedCircularArc.CenterX - PT_LineLineCrossPoints[0].X;
                                                PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y = PT_CircleTool.RunParams.ExpectedCircularArc.CenterY - PT_LineLineCrossPoints[0].Y;
                                            }
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int k = 0; k < Main.DEFINE.SUBLINE_MAX; k++)
                            {
                                if (PT_FindLinePara[m_PatNo, k, 0].m_UseCheck && PT_FindLinePara[m_PatNo, k, 1].m_UseCheck)
                                {
                                    PT_FindLineTools[m_PatNo, k, 0].Run();
                                    PT_FindLineTools[m_PatNo, k, 1].Run();

                                    if ((PT_FindLineTools[m_PatNo, k, 0].Results != null && PT_FindLineTools[m_PatNo, k, 0].Results.Count > 0 && PT_FindLineTools[m_PatNo, k, 0].Results.GetLine() != null)
                                        && (PT_FindLineTools[m_PatNo, k, 1].Results != null && PT_FindLineTools[m_PatNo, k, 1].Results.Count > 0 && PT_FindLineTools[m_PatNo, k, 1].Results.GetLine() != null))
                                    {
                                        PT_LineLineCrossPoints[0] = new CogIntersectLineLineTool();
                                        PT_LineLineCrossPoints[0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                                        PT_LineLineCrossPoints[0].LineA = PT_FindLineTools[m_PatNo, k, 0].Results.GetLine();
                                        PT_LineLineCrossPoints[0].LineB = PT_FindLineTools[m_PatNo, k, 1].Results.GetLine();
                                        PT_LineLineCrossPoints[0].Run();
                                        if (PT_LineLineCrossPoints[0].Intersects)
                                        {
                                            if (PT_LineLineCrossPoints[0].X >= 0 && PT_LineLineCrossPoints[0].X <= PT_LineLineCrossPoints[0].InputImage.Width && PT_LineLineCrossPoints[0].Y >= 0 && PT_LineLineCrossPoints[0].Y <= PT_LineLineCrossPoints[0].InputImage.Height)
                                            {
                                                Display.SetGraphics(PT_Display01, PT_LineLineCrossPoints[0].CreateLastRunRecord(), "RESULT");

                                                PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].X = PT_CircleTool.RunParams.ExpectedCircularArc.CenterX - PT_LineLineCrossPoints[0].X;
                                                PT_CirclePara[m_PatNo, m_SelectCircle].m_TargetToCenter[Main.DEFINE.M_FINDLINETOOL].Y = PT_CircleTool.RunParams.ExpectedCircularArc.CenterY - PT_LineLineCrossPoints[0].Y;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    PT_CircleTools[m_PatNo, m_SelectCircle] = new CogFindCircleTool(PT_CircleTool);
                    PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.ContrastThreshold = TBAR_THRES_CIR.Value;
                    PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.NumCalipers = Convert.ToInt16(LB_CIRCLE_CNT.Text);
                    PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.NumToIgnore = (int)NUD_CIRCLE_IGNCNT.Value;
                    PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperSearchDirection = (CogFindCircleSearchDirectionConstants)CB_DIRECTION_CIR.SelectedIndex;
                    PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(CB_POLARITY_CIR.SelectedIndex + 1);

                    if (RBTN_CIR_CALIPER_METHOD_SCORE.Checked == true)
                    {
                        PT_CirclePara[m_PatNo, m_SelectCircle].m_CircleCaliperMethod = Main.DEFINE.CLP_METHOD_SCORE;
                        PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                        CogCaliperScorerContrast scorer = new CogCaliperScorerContrast();
                        scorer.Enabled = true;
                        PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.SingleEdgeScorers.Add(scorer);
                    }
                    else if (RBTN_CIR_CALIPER_METHOD_POS.Checked == true)
                    {
                        PT_CirclePara[m_PatNo, m_SelectCircle].m_CircleCaliperMethod = Main.DEFINE.CLP_METHOD_POS;
                        PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.SingleEdgeScorers.Clear();
                        CogCaliperScorerPosition scorer = new CogCaliperScorerPosition();
                        scorer.Enabled = true;
                        PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.SingleEdgeScorers.Add(scorer);
                    }

                    LABEL_MESSAGE(LB_MESSAGE, "Register OK", System.Drawing.Color.Lime);
                }
                else
                    LABEL_MESSAGE(LB_MESSAGE, "Select!!! Circle", System.Drawing.Color.Red);
            }
            catch (System.Exception ex)
            {
                LABEL_MESSAGE(LB_MESSAGE, ex.Message, System.Drawing.Color.Red);
            }

            Main.DisplayFit(PT_Display01);
        }
        private void CB_CIRCLE_USE_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_CIRCLE_USE.Checked)
            {
                CB_CIRCLE_USE.BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                CB_CIRCLE_USE.BackColor = System.Drawing.Color.DarkGray;
            }
        }
        private void CB_CIRCLE_USE_Click(object sender, EventArgs e)
        {
            if (CB_CIRCLE_USE.Checked)
            {
                PT_CirclePara[m_PatNo, m_SelectCircle].m_UseCheck = true;
            }
            else
            {
                PT_CirclePara[m_PatNo, m_SelectCircle].m_UseCheck = false;
            }
        }
        private void TBAR_THRES_CIR_ValueChanged(object sender, EventArgs e)
        {
            PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.ContrastThreshold = TBAR_THRES_CIR.Value;
            LB_THRES_CIR.Text = TBAR_THRES_CIR.Value.ToString();
            if (!m_PatchangeFlag && TABC_MANU.SelectedIndex == Main.DEFINE.M_FINDCIRCLETOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }
        private void BTN_THRES_CIR_UP_Click(object sender, EventArgs e)
        {
            if (TBAR_THRES_CIR.Maximum == TBAR_THRES_CIR.Value) return;
            TBAR_THRES_CIR.Value++;
        }
        private void BTN_THRES_CIR_DN_Click(object sender, EventArgs e)
        {
            if (TBAR_THRES_CIR.Minimum == TBAR_THRES_CIR.Value) return;
            TBAR_THRES_CIR.Value--;
        }
        private void CB_DIRECTION_CIR_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperSearchDirection = (CogFindCircleSearchDirectionConstants)CB_DIRECTION_CIR.SelectedIndex;
            PT_CircleTool.RunParams.CaliperSearchDirection = (CogFindCircleSearchDirectionConstants)CB_DIRECTION_CIR.SelectedIndex;
            //DrawCircleRegion();
        }
        private void CB_POLARITY_CIR_SelectionChangeCommitted(object sender, EventArgs e)
        {
            PT_CircleTools[m_PatNo, m_SelectCircle].RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)(CB_POLARITY_CIR.SelectedIndex + 1);
        }

        private void CB_CIRCLE_MARK_USE_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox TempBTN = (CheckBox)sender;
            if (TempBTN.Checked)
            {
                TempBTN.BackColor = System.Drawing.Color.GreenYellow;
                PT_Circle_MarkUSE[m_PatNo] = true;
            }
            else
            {
                TempBTN.BackColor = System.Drawing.Color.White;
                PT_Circle_MarkUSE[m_PatNo] = false;
            }
            if (!m_TABCHANGE_MODE)
            {
                RefreshDisplay2();
                DrawCircleRegion();
            }
        }
        #region Move
        private void BTN_SEARCH_CIR_UP_Click(object sender, EventArgs e)
        {
            BTN_CIRCLE_MOVE(M_PLUS, M_SEARCHLEGNTH);
        }
        private void BTN_SEARCH_CIR_DN_Click(object sender, EventArgs e)
        {
            BTN_CIRCLE_MOVE(M_MINUS, M_SEARCHLEGNTH);
        }
        private void BTN_PROJECTION_CIR_UP_Click(object sender, EventArgs e)
        {
            BTN_CIRCLE_MOVE(M_PLUS, M_PROJECTION);
        }
        private void BTN_PROJECTION_CIR_DN_Click(object sender, EventArgs e)
        {
            BTN_CIRCLE_MOVE(M_MINUS, M_PROJECTION);
        }
        private void BTN_RADIUS_CIR_UP_Click(object sender, EventArgs e)
        {
            BTN_CIRCLE_MOVE(M_PLUS, M_RADIUS);
        }
        private void BTN_RADIUS_CIR_DN_Click(object sender, EventArgs e)
        {
            BTN_CIRCLE_MOVE(M_MINUS, M_RADIUS);
        }
        private void BTN_CIRCLE_MOVE(int nValue, int nMode)
        {
            if (nMode == M_SEARCHLEGNTH)
            {
                if (nValue == M_MINUS && PT_CircleTool.RunParams.CaliperSearchLength < 10)
                {
                    PT_CircleTool.RunParams.CaliperSearchLength = 10;
                }
                else
                {
                    PT_CircleTool.RunParams.CaliperSearchLength += nValue;
                }

            }
            if (nMode == M_PROJECTION)
            {
                if (nValue == M_MINUS && PT_CircleTool.RunParams.CaliperProjectionLength < 10)
                {
                    PT_CircleTool.RunParams.CaliperProjectionLength = 10;
                }
                else
                {
                    PT_CircleTool.RunParams.CaliperProjectionLength += nValue;
                }
            }
            if (nMode == M_RADIUS)
            {
                if (nValue == M_MINUS && PT_CircleTool.RunParams.ExpectedCircularArc.Radius < 10)
                {
                    PT_CircleTool.RunParams.ExpectedCircularArc.Radius = 10;
                }
                else
                {
                    PT_CircleTool.RunParams.ExpectedCircularArc.Radius += nValue;
                }
                LB_RADIUS_CIR.Text = PT_CircleTool.RunParams.ExpectedCircularArc.Radius.ToString(); //이 파라미터는 changed에서 안들어감 버그인듯. 다른건들어감
            }
            DrawCircleRegion();
        }
        #endregion

        #endregion

        #region MOVE_SIZE_LBMSSAGE
        private void BTN_MOVE_Click(object sender, EventArgs e)
        {
            double nMoveDataX = 0, nMoveDataY = 0; //공통으로 쓸수 있도록 코딩.

            int nMode = 0;
            nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            try
            {
                Button TempBTN = (Button)sender;
                switch (TempBTN.Text.ToUpper().Trim())
                {
                    case "LEFT":
                        nMoveDataX = -1;
                        nMoveDataY = 0;
                        break;

                    case "RIGHT":
                        nMoveDataX = 1;
                        nMoveDataY = 0;
                        break;

                    case "UP":
                        nMoveDataX = 0;
                        nMoveDataY = -1;
                        break;

                    case "DOWN":
                        nMoveDataX = 0;
                        nMoveDataY = 1;
                        break;
                }

                nMoveDataX /= PT_Display01.Zoom;
                nMoveDataY /= PT_Display01.Zoom;

                if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
                {
                    if (m_RetiMode == M_PATTERN) { PatMaxTrainRegion.X += nMoveDataX; PatMaxTrainRegion.Y += nMoveDataY; }
                    if (m_RetiMode == M_SEARCH) { PatMaxSearchRegion.X += nMoveDataX; PatMaxSearchRegion.Y += nMoveDataY; }
                    if (m_RetiMode == M_ORIGIN) { MarkORGPoint.X += nMoveDataX; MarkORGPoint.Y += nMoveDataY; }
                }
                if (nMode == Main.DEFINE.M_BLOBTOOL)
                {
                    BlobTrainRegion.CenterX += nMoveDataX;
                    BlobTrainRegion.CenterY += nMoveDataY;
                }
                if (nMode == Main.DEFINE.M_CALIPERTOOL)
                {
                    PTCaliperRegion.CenterX += nMoveDataX;
                    PTCaliperRegion.CenterY += nMoveDataY;
                }
                if (nMode == Main.DEFINE.M_FINDLINETOOL)
                {
                    PT_FindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
                    PT_FindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;

                    PT_FindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
                    PT_FindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
                }
                if (nMode == Main.DEFINE.M_FINDCIRCLETOOL)
                {
                    PT_CircleTool.RunParams.ExpectedCircularArc.CenterX += nMoveDataX;
                    PT_CircleTool.RunParams.ExpectedCircularArc.CenterY += nMoveDataY;

                }
                if (Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == 5)
                {
                    if (Chk_All_Select.Checked == true)
                    {
                        CogGraphicInteractiveCollection GraphicCollection = new CogGraphicInteractiveCollection();

                        Parallel.For(0, m_TeachParameter.Count, i =>
                        {
                            var Tempdata = m_TeachParameter[i];

                            if ((enumROIType)Tempdata.m_enumROIType == (enumROIType)enumROIType.Line)
                            {
                                //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
                                //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
                                //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;
                                //Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
                                //TrackLineROI(Tempdata.m_FindLineTool);

                                m_TeachParameter[i].m_FindLineTool.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                                m_TeachParameter[i].m_FindLineTool.RunParams.CaliperRunParams.Edge1Polarity = CogCaliperPolarityConstants.LightToDark;

                                //GraphicCollection.Add((ICogGraphic)Tempdata.m_FindLineTool.CreateCurrentRecord());

                                //GraphicData.Add(SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge));

                                PT_Display01.InteractiveGraphics.AddList(GraphicCollection, "GraphicCollection", false);
                            }
                            else
                            {
                                //Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterX += nMoveDataX;
                                //Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterY += nMoveDataY;

                                m_TeachParameter[i].m_FindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.LightToDark;
                                m_TeachParameter[i].m_FindCircleTool.RunParams.CaliperRunParams.Edge1Polarity = CogCaliperPolarityConstants.DarkToLight;

                                //TrackCircleROI(Tempdata.m_FindCircleTool);
                            }
                        });


                        //for (int i = 0; i < m_TeachParameter.Count; i++)
                        //{
                        //    var Tempdata = m_TeachParameter[i];

                        //    if ((enumROIType)Tempdata.m_enumROIType == (enumROIType)enumROIType.Line)
                        //    {
                        //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
                        //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
                        //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;
                        //        Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
                        //        TrackLineROI(Tempdata.m_FindLineTool);
                        //    }
                        //    else
                        //    {
                        //        Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterX += nMoveDataX;
                        //        Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterY += nMoveDataY;
                        //        TrackCircleROI(Tempdata.m_FindCircleTool);
                        //    }
                        //}
                    }
                    else
                    {
                        if (m_TempFindLineTool != null)
                        {
                            m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX += nMoveDataX;
                            m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX += nMoveDataX;

                            m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY += nMoveDataY;
                            m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY += nMoveDataY;
                        }
                    }
                }
            }
            catch
            {

            }
        }
        private void BTN_SIZE_Click(object sender, EventArgs e)
        {
            double nMoveDataX = 0, nMoveDataY = 0; //공통으로 쓸수 있도록 코딩.

            int nMode = 0;
            nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
            try
            {
                Button TempBTN = (Button)sender;
                switch (TempBTN.Text.ToUpper())
                {
                    case "X_DEC":
                        nMoveDataX = -2;
                        nMoveDataY = 0;
                        break;

                    case "X_INC":
                        nMoveDataX = 2;
                        nMoveDataY = 0;
                        break;

                    case "Y_DEC":
                        nMoveDataX = 0;
                        nMoveDataY = -2;
                        break;

                    case "Y_INC":
                        nMoveDataX = 0;
                        nMoveDataY = 2;
                        break;
                }

                if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
                {
                    if (m_RetiMode == M_PATTERN) { PatMaxTrainRegion.SetCenterWidthHeight(PatMaxTrainRegion.CenterX, PatMaxTrainRegion.CenterY, PatMaxTrainRegion.Width += nMoveDataX, PatMaxTrainRegion.Height += nMoveDataY); }
                    if (m_RetiMode == M_SEARCH) { PatMaxSearchRegion.SetCenterWidthHeight(PatMaxSearchRegion.CenterX, PatMaxSearchRegion.CenterY, PatMaxSearchRegion.Width += nMoveDataX, PatMaxSearchRegion.Height += nMoveDataY); }
                }

                if (nMode == Main.DEFINE.M_BLOBTOOL)
                {
                    BlobTrainRegion.SetCenterLengthsRotationSkew(BlobTrainRegion.CenterX, BlobTrainRegion.CenterY, BlobTrainRegion.SideXLength += nMoveDataX, BlobTrainRegion.SideYLength += nMoveDataY, BlobTrainRegion.Rotation, BlobTrainRegion.Skew);
                }

                if (nMode == Main.DEFINE.M_CALIPERTOOL)
                {
                    PTCaliperRegion.SideXLength += nMoveDataX;
                    PTCaliperRegion.SideYLength += nMoveDataY;
                }

                if (nMode == Main.DEFINE.M_FINDLINETOOL)
                {
                    PT_FindLineTool.RunParams.CaliperProjectionLength += nMoveDataX;
                    PT_FindLineTool.RunParams.CaliperSearchLength += nMoveDataY;
                }

                PSizeLabel();
            }
            catch
            {
            }
        }
        private void BTN_SIZE_INPUT(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                double nSizeDataX = 0, nSizeDataY = 0; //공통으로 쓸수 있도록 코딩.
                double nMinSizeX = 0, nMinSizeY = 0;
                double nInputMinSizeX = 2, nInputMinSizeY = 2;
                int nMode = 0;
                nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);
                try
                {

                    if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
                    {
                        if (m_RetiMode == M_PATTERN)
                        {
                            nSizeDataX = PatMaxTrainRegion.Width;
                            nSizeDataY = PatMaxTrainRegion.Height;
                        }
                        if (m_RetiMode == M_SEARCH)
                        {
                            nSizeDataX = PatMaxSearchRegion.Width;
                            nSizeDataY = PatMaxSearchRegion.Height;
                        }
                    }

                    if (nMode == Main.DEFINE.M_BLOBTOOL)
                    {
                        nSizeDataX = BlobTrainRegion.SideXLength;
                        nSizeDataY = BlobTrainRegion.SideYLength;
                    }

                    if (nMode == Main.DEFINE.M_CALIPERTOOL)
                    {
                        nSizeDataX = PTCaliperRegion.SideXLength;
                        nSizeDataY = PTCaliperRegion.SideYLength;
                        nInputMinSizeX = nMinSizeX = PT_CaliperTools[m_PatNo, m_SelectCaliper].RunParams.FilterHalfSizeInPixels * 2 + 2.5;
                    }

                    if (nMode == Main.DEFINE.M_FINDLINETOOL)
                    {
                        nSizeDataX = PT_FindLineTool.RunParams.CaliperProjectionLength;
                        nSizeDataY = PT_FindLineTool.RunParams.CaliperSearchLength;
                        nInputMinSizeY = nMinSizeY = (PT_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels * 2 + 2.5);
                    }
                    if (nMode == Main.DEFINE.M_FINDCIRCLETOOL)
                    {
                        nSizeDataX = PT_CircleTool.RunParams.CaliperProjectionLength;
                        nSizeDataY = PT_CircleTool.RunParams.CaliperSearchLength;
                        nInputMinSizeY = nMinSizeY = (PT_CircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels * 2 + 2.5);
                    }

                    Button TempBTN = (Button)sender;
                    switch (TempBTN.Text.ToUpper())
                    {
                        case "X_DEC":
                        case "X_INC":
                            Form_KeyPad form_keypad = new Form_KeyPad(nInputMinSizeX, 50000, nSizeDataX, "X AREA SIZE", 1);
                            form_keypad.ShowDialog();
                            if (form_keypad.m_data > nMinSizeX) nSizeDataX = form_keypad.m_data;

                            break;
                        case "Y_DEC":
                        case "Y_INC":

                            Form_KeyPad form_keypad1 = new Form_KeyPad(nInputMinSizeY, 50000, nSizeDataY, "Y AREA SIZE", 1);
                            form_keypad1.ShowDialog();
                            if (form_keypad1.m_data > nMinSizeY) nSizeDataY = form_keypad1.m_data;
                            break;
                    }

                    if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
                    {
                        if (m_RetiMode == M_PATTERN) { PatMaxTrainRegion.SetCenterWidthHeight(PatMaxTrainRegion.CenterX, PatMaxTrainRegion.CenterY, nSizeDataX, nSizeDataY); }
                        if (m_RetiMode == M_SEARCH) { PatMaxSearchRegion.SetCenterWidthHeight(PatMaxSearchRegion.CenterX, PatMaxSearchRegion.CenterY, nSizeDataX, nSizeDataY); }
                    }

                    if (nMode == Main.DEFINE.M_BLOBTOOL)
                    {
                        BlobTrainRegion.SetCenterLengthsRotationSkew(BlobTrainRegion.CenterX, BlobTrainRegion.CenterY, nSizeDataX, nSizeDataY, BlobTrainRegion.Rotation, BlobTrainRegion.Skew);
                    }

                    if (nMode == Main.DEFINE.M_CALIPERTOOL)
                    {
                        PTCaliperRegion.SideXLength = nSizeDataX;
                        PTCaliperRegion.SideYLength = nSizeDataY;
                    }

                    if (nMode == Main.DEFINE.M_FINDLINETOOL)
                    {
                        PT_FindLineTool.RunParams.CaliperProjectionLength = nSizeDataX;
                        PT_FindLineTool.RunParams.CaliperSearchLength = nSizeDataY;
                    }
                    if (nMode == Main.DEFINE.M_FINDCIRCLETOOL)
                    {
                        PT_CircleTool.RunParams.CaliperProjectionLength = nSizeDataX;
                        PT_CircleTool.RunParams.CaliperSearchLength = nSizeDataY;
                    }
                    PSizeLabel();
                }
                catch
                {

                }
            }
        }
        private void ORGSizeFit()
        {
            try
            {
                int nZoomSize = 1;
                //----------------------------------------------------------------------
                nZoomSize = (int)(PT_Display01.Zoom * M_ORIGIN_SIZE);
                if (nZoomSize < 1)
                    MarkORGPoint.SizeInScreenPixels = M_ORIGIN_SIZE;
                else
                    MarkORGPoint.SizeInScreenPixels = nZoomSize;
                //----------------------------------------------------------------------
                nZoomSize = (int)(PT_Display01.Zoom * nCrossSize);
                if (MarkPoint[m_PatNo, 0] != null && MarkPoint[m_PatNo, 1] != null)
                {
                    if (nZoomSize < 1)
                    {
                        MarkPoint[m_PatNo, 0].SizeInScreenPixels = nCrossSize;
                        MarkPoint[m_PatNo, 1].SizeInScreenPixels = nCrossSize;
                    }
                    else
                    {
                        MarkPoint[m_PatNo, 0].SizeInScreenPixels = nZoomSize;
                        MarkPoint[m_PatNo, 1].SizeInScreenPixels = nZoomSize;
                    }
                }
            }
            catch
            {

            }
            //----------------------------------------------------------------------
        }
        private void PSizeLabel()
        {
            int nMode = 0;
            nMode = Convert.ToInt32(TABC_MANU.SelectedTab.Tag);

            if (nMode == Main.DEFINE.M_CNLSEARCHTOOL)
            {
                if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN) { LABEL_MESSAGE(LB_MESSAGE1, "X:" + PatMaxTrainRegion.Width.ToString("0.0") + " , " + "Y:" + PatMaxTrainRegion.Height.ToString("0.0"), System.Drawing.Color.GreenYellow); }
                if (m_RetiMode == M_SEARCH) { LABEL_MESSAGE(LB_MESSAGE1, "X:" + PatMaxSearchRegion.Width.ToString("0.0") + " , " + "Y:" + PatMaxSearchRegion.Height.ToString("0.0"), System.Drawing.Color.GreenYellow); }
            }

            if (nMode == Main.DEFINE.M_BLOBTOOL)
            {
                LABEL_MESSAGE(LB_MESSAGE1, "X:" + BlobTrainRegion.SideXLength.ToString("0.0") + " , " + "Y:" + BlobTrainRegion.SideYLength.ToString("0.0"), System.Drawing.Color.GreenYellow);
            }

            if (nMode == Main.DEFINE.M_CALIPERTOOL)
            {
                LABEL_MESSAGE(LB_MESSAGE1, "X:" + PTCaliperRegion.SideXLength.ToString("0.0") + " , " + "Y:" + PTCaliperRegion.SideYLength.ToString("0.0"), System.Drawing.Color.GreenYellow);
            }
            if (nMode == Main.DEFINE.M_FINDLINETOOL)
            {
                LABEL_MESSAGE(LB_MESSAGE1, "X:" + PT_FindLineTool.RunParams.CaliperProjectionLength.ToString("0.0") + " , " + "Y:" + PT_FindLineTool.RunParams.CaliperSearchLength.ToString("0.0"), System.Drawing.Color.GreenYellow);
            }
        }
        private void LABEL_MESSAGE(Label nlabel, string nText, Color nColor)
        {
            nlabel.ForeColor = nColor;
            nlabel.Text = nText;
        }

        #region Distance
        private void DistanceLine()
        {
            for (int i = 0; i < Main.DEFINE.Pattern_Max; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    MarkPoint[i, j] = new CogPointMarker();
                    MarkPoint[i, j].LineStyle = CogGraphicLineStyleConstants.Dot;
                    if (j == 0)
                    {
                        MarkPoint[i, j].Color = CogColorConstants.Green;
                        MarkPoint[i, j].SelectedColor = CogColorConstants.Green;
                    }
                    else
                    {
                        MarkPoint[i, j].Color = CogColorConstants.Red;
                        MarkPoint[i, j].SelectedColor = CogColorConstants.Red;
                    }
                    MarkPoint[i, j].GraphicDOFEnable = CogPointMarkerDOFConstants.All;
                    MarkPoint[i, j].Interactive = true;
                    MarkPoint[i, j].SizeInScreenPixels = nCrossSize;
                    MarkPoint[i, j].X = (double)Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_CamNo] + (50 * j);
                    MarkPoint[i, j].Y = (double)Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, i].m_CamNo];
                }
            }


        }
        private void PT_Display_DoubleClick(object sender, EventArgs e)
        {
            CogDisplay TempLB = (CogDisplay)sender;
            try
            {
                int nNum;
                nNum = Convert.ToInt16(TempLB.Name.Substring(TempLB.Name.Length - 1, 1));
                if (nNum == 2)
                {
                    bool nMarkUse = false;
                    if (M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
                        if (PT_Blob_MarkUSE[m_PatNo]) nMarkUse = true;
                    if (M_TOOL_MODE == Main.DEFINE.M_CALIPERTOOL)
                        if (PT_Caliper_MarkUSE[m_PatNo] || PT_Blob_CaliperUSE[m_PatNo]) nMarkUse = true;
                    if (M_TOOL_MODE == Main.DEFINE.M_FINDLINETOOL)
                        if (PT_FindLine_MarkUSE[m_PatNo]) nMarkUse = true;

                    if (nMarkUse)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            MarkPoint[m_PatNo, j].X = (double)Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] + (50 * j) - PatResult.TranslationX;
                            MarkPoint[m_PatNo, j].Y = (double)Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] - PatResult.TranslationY;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            MarkPoint[m_PatNo, j].X = (double)Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo] + (50 * j);
                            MarkPoint[m_PatNo, j].Y = (double)Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo];
                        }
                    }
                }
                TempLB.InteractiveGraphics.Add(MarkPoint[m_PatNo, 0] as ICogGraphicInteractive, "Distance", false);
                TempLB.InteractiveGraphics.Add(MarkPoint[m_PatNo, 1] as ICogGraphicInteractive, "Distance", false);
                nDistanceShow[m_PatNo] = true;
            }
            catch
            {

            }
        }
        private void PT_Display_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CogDisplay TempLB = (CogDisplay)sender;
                try
                {
                    if (nDistanceShow[m_PatNo])
                    {
                        nDistance.InputImage = TempLB.Image;

                        double nStartX = 0, nStartY = 0;
                        double nEndX = 10, nEndY = 10;

                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(MarkPoint[m_PatNo, 0].X, MarkPoint[m_PatNo, 0].Y, ref nStartX, ref nStartY);
                        Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].V2R(MarkPoint[m_PatNo, 1].X, MarkPoint[m_PatNo, 1].Y, ref nEndX, ref nEndY);

                        nDistance.StartX = nStartX;
                        nDistance.StartY = nStartY;

                        nDistance.EndX = nEndX;
                        nDistance.EndY = nEndY;
                        nDistance.Run();
                        LABEL_MESSAGE(LB_MESSAGE, nDistance.Distance.ToString("0.0") + " um" + " , " + (Main.DEFINE.degree * nDistance.Angle).ToString("0.000") + " Deg", System.Drawing.Color.Red);

                        nDistance.StartX = MarkPoint[m_PatNo, 0].X;
                        nDistance.StartY = MarkPoint[m_PatNo, 0].Y;

                        nDistance.EndX = MarkPoint[m_PatNo, 1].X;
                        nDistance.EndY = MarkPoint[m_PatNo, 1].Y;
                        nDistance.Run();
                        LABEL_MESSAGE(LB_MESSAGE, LB_MESSAGE.Text + " , " + nDistance.Distance.ToString("0.0") + " Pixel", System.Drawing.Color.Red);
                    }
                    PSizeLabel();
                }
                catch
                {
                    LABEL_MESSAGE(LB_MESSAGE, "", System.Drawing.Color.Red);
                }
            }
        }
        #endregion


        private void LB_CAMCENTER_DoubleClick(object sender, EventArgs e)
        {
            if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            {
                //                 PatMaxORGPoint.OriginX = Main.vision.IMAGE_CENTER_X[m_CamNo];
                //                 PatMaxORGPoint.OriginY = Main.vision.IMAGE_CENTER_Y[m_CamNo];

                MarkORGPoint.X = Main.vision.IMAGE_CENTER_X[m_CamNo];
                MarkORGPoint.Y = Main.vision.IMAGE_CENTER_Y[m_CamNo];

            }
        }
        private void PT_SubDisplay_00_Click_1(object sender, EventArgs e)
        {
            CogRecordDisplay TempNum = (CogRecordDisplay)sender;
            int n_SubNo;
            n_SubNo = Convert.ToInt16(TempNum.Name.Substring(TempNum.Name.Length - 2, 2));
            CB_SUB_PATTERN.SelectedIndex = n_SubNo;
            CB_SUB_PATTERN_SelectionChangeCommitted(null, null);
        }
        private void BTN_MAINORIGIN_COPY_Click(object sender, EventArgs e)
        {
            if (m_RetiMode == M_PATTERN || m_RetiMode == M_ORIGIN)
            {
                bool SearchResult = false;
                if (PT_Pattern[m_PatNo, 0].Pattern.Trained == false)
                {
                    MarkORGPoint.SetCenterRotationSize(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 0, M_ORIGIN_SIZE);
                    ORGSizeFit();
                }
                else
                {
                    PT_Pattern[m_PatNo, 0].Run();
                    if (PT_Pattern[m_PatNo, 0].Results != null)
                    {
                        if (PT_Pattern[m_PatNo, 0].Results.Count >= 1) SearchResult = true;
                    }
                    if (SearchResult)
                    {
                        MarkORGPoint.X = PatResult.TranslationX;  //PT_Pattern[m_PatNo, 0].Pattern.Origin.TranslationX;
                        MarkORGPoint.Y = PatResult.TranslationY; // PT_Pattern[m_PatNo, 0].Pattern.Origin.TranslationY;
                    }
                }

            }
        }
        private void LB_PATTERN_08_Click(object sender, EventArgs e)
        {
            Label TempNum = (Label)sender;
            int n_SubNo;
            n_SubNo = Convert.ToInt16(TempNum.Name.Substring(TempNum.Name.Length - 2, 2));

            if (PT_Pattern_USE[m_PatNo, n_SubNo])
            {
                PT_Pattern_USE[m_PatNo, n_SubNo] = false;
                SUBPATTERN_LABELDISPLAY(PT_Pattern_USE[m_PatNo, n_SubNo], n_SubNo);
            }
            else
            {
                PT_Pattern_USE[m_PatNo, n_SubNo] = true;
                SUBPATTERN_LABELDISPLAY(PT_Pattern_USE[m_PatNo, n_SubNo], n_SubNo);
            }

            if (m_PatNo_Sub == n_SubNo)
                CB_SUBPAT_USE.Checked = PT_Pattern_USE[m_PatNo, m_PatNo_Sub];
        }
        private void PT_Display01_Changed(object sender, CogChangedEventArgs e)
        {
            if (Main.Status.MC_MODE == Main.DEFINE.MC_TEACHFORM)
            {
                if (PT_Display01.Zoom != ZoomBackup)
                {
                    ZoomBackup = PT_Display01.Zoom;
                    ORGSizeFit();
                }
            }
        }
        private void BTN_PATTERN_MASK_Click(object sender, EventArgs e)
        {
            //2023 0225 YSH ROI Finealign 
            if (bROIFinealignTeach)
            {
                if (FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.Trained)
                {
                    FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].InputImage = CopyIMG(FinealignMark[nROIFineAlignIndex, m_PatNo_Sub].Pattern.GetTrainedPatternImage());
                    FormPatternMask.BackUpSearchMaxTool = FinealignMark[nROIFineAlignIndex, m_PatNo_Sub];
                    FormPatternMask.ShowDialog();

                    FinealignMark[nROIFineAlignIndex, m_PatNo_Sub] = FormPatternMask.BackUpSearchMaxTool;

                    DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], FinealignMark[nROIFineAlignIndex, m_PatNo_Sub]);
                }
            }
            else
            {
                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Trained)
                {
                    //                 PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                    //                 PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);

                    PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage);
                    PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(PT_GPattern[m_PatNo, m_PatNo_Sub].Pattern.TrainImage);

                    FormPatternMask.BackUpSearchMaxTool = PT_Pattern[m_PatNo, m_PatNo_Sub];
                    FormPatternMask.BackUpPMAlignTool = PT_GPattern[m_PatNo, m_PatNo_Sub];
                    FormPatternMask.ShowDialog();

                    PT_Pattern[m_PatNo, m_PatNo_Sub] = FormPatternMask.BackUpSearchMaxTool;
                    PT_GPattern[m_PatNo, m_PatNo_Sub] = FormPatternMask.BackUpPMAlignTool;

                    DrawTrainedPattern(PT_SubDisplay[m_PatNo_Sub], PT_Pattern[m_PatNo, m_PatNo_Sub]);
                }
            }

        }
        private void BTN_PATTERN_SCORE_Click(object sender, EventArgs e)
        {
            //             bool nResult = false;
            //             Form_Password formpassword = new Form_Password();
            //             formpassword.ShowDialog();
            //             nResult = formpassword.LOGINOK;
            //             formpassword.Dispose();
            // 
            //             if (nResult)
            //             {
            //                 if (Main.machine.EngineerMode)
            //                 {
            //                     Main.machine.EngineerMode = false;
            //                     BTN_Engineer.BackColor = System.Drawing.Color.DarkGray;
            //                 }
            //                 else
            //                 {
            //                     Main.machine.EngineerMode = true;
            //                     BTN_Engineer.BackColor = System.Drawing.Color.LawnGreen;
            //                 }
            //             }
        }
        private void LB_RECTANGLE_Click(object sender, EventArgs e)
        {
            if (M_TOOL_MODE == Main.DEFINE.M_BLOBTOOL)
            {
                BlobTrainRegion.Rotation = 0;
                BlobTrainRegion.SetCenterLengthsRotationSkew(Main.vision.IMAGE_CENTER_X[m_CamNo], Main.vision.IMAGE_CENTER_Y[m_CamNo], 200, 200, 0, 0);
            }
        }
        private void BTN_PATTERN_COPY_Click(object sender, EventArgs e)
        {

            //Form_RecipeCopy RecipeCopy = new Form_RecipeCopy();
            //RecipeCopy.ShowDialog();
            //int iStageNo = RecipeCopy.StageNo;
            //int iInspType = RecipeCopy.InspectionType;
            //bool bRecipCopy = RecipeCopy.bRecipeCopy;
            //if(bRecipCopy)
            //{
            //    if (iInspType == 0)
            //    {
            //        Main.AlignUnit[iStageNo].PAT[m_PatTagNo, 0].m_InspParameter = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, 0].m_InspParameter;
            //    }
            //}


            //nPatternCopy = true;
            //BTN_SAVE_Click(null, null);

            //2022 0902 YSH   
            //현재 인덱스를 기준으로 Caliper Data 모두 copy, save, load 진행
            DialogResult result = MessageBox.Show("Do you want to Vision Data Copy?", "COPY", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Main.AlignUnit[i].PAT[j, 0].m_InspParameter = m_TeachParameter;
                        for (int k = 0; k < 4; k++)
                        {
                            Main.AlignUnit[i].PAT[j, 0].m_TrackingLine[k] = m_TeachLine[k];
                            Main.AlignUnit[i].PAT[j, 0].m_BondingAlignLine[k] = m_TeachAlignLine[k];    //shkang
                        }
                        Main.AlignUnit[i].PAT[j, 0].m_FinealignMark = FinealignMark;
                        Main.AlignUnit[i].PAT[j, 0].m_bFInealignFlag = m_bROIFinealignFlag;
                        Main.AlignUnit[i].PAT[j, 0].m_FinealignThetaSpec = m_dROIFinealignT_Spec;
                        Main.AlignUnit[i].PAT[j, 0].m_FinealignMarkScore = dFinealignMarkScore;

                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Main.AlignUnit[i].Save(j);
                        Main.AlignUnit[i].Load(j);
                    }
                }
            }

        }

        #region FPC_TRAY_NACHI
        private void BTN_FPC_SEARCH_ALL_Click(CogRecordDisplay Display)
        {
            m_Timer.StartTimer();
            try
            {
                //   BTN_INTERSECTION_RUN_Click(null , null);
                Main.DisplayClear(Display);
                DrawPocketPoint(Display);
                CogRectangle nBackUp_SearchRegion = new CogRectangle();
                CogRectangle nSearchRegion = new CogRectangle();
                List<CogCompositeShape> ResultGraphic = new List<CogCompositeShape>();
                List<CogRectangle> ResultSearchRegion = new List<CogRectangle>();

                List<string> nMessageList = new List<string>();
                string[] nMessage = new string[2];
                List<CogColorConstants> nColorList = new List<CogColorConstants>();

                List<double> nPosXs = new List<double>();
                List<double> nPosYs = new List<double>();

                int PoketNum = 0;
                int nCount_OK = 0;
                int nGCount_OK = 0;
                bool[] Mark_ret = new bool[TRAY_POCKET_X * TRAY_POCKET_Y];
                bool[] GMark_ret = new bool[TRAY_POCKET_X * TRAY_POCKET_Y];

                int nTempPatNo_Sub = 0;

                nTempPatNo_Sub = m_PatNo_Sub;

                PT_Pattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);
                PT_GPattern[m_PatNo, m_PatNo_Sub].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);

                nBackUp_SearchRegion = new CogRectangle(PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle);

                if (MarkerPointList.Count == TRAY_POCKET_X * TRAY_POCKET_Y)
                {
                    for (int nX = 0; nX < TRAY_POCKET_X; nX++)
                    {
                        for (int nY = 0; nY < TRAY_POCKET_Y; nY++)
                        {
                            PoketNum = (nX * TRAY_POCKET_Y) + nY;
                            //---------------------------------------------------------------------------------------------------------------------------------
                            (PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).SetCenterWidthHeight(MarkerPointList[PoketNum].X, MarkerPointList[PoketNum].Y,
                                (PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).Width, (PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).Height);

                            PT_Pattern[m_PatNo, nTempPatNo_Sub].Run();
                            if (PT_Pattern[m_PatNo, nTempPatNo_Sub].Results != null)
                            {
                                if (PT_Pattern[m_PatNo, nTempPatNo_Sub].Results.Count > 0)
                                {
                                    if (PT_Pattern[m_PatNo, nTempPatNo_Sub].Results[0].Score >= PT_AcceptScore[m_PatNo])
                                    {
                                        nMessage[0] = "P" + PoketNum.ToString() + "->  " + (PT_Pattern[m_PatNo, nTempPatNo_Sub].Results[0].Score * 100).ToString("0.00");
                                        nMessageList.Add(nMessage[0]);
                                        nPosXs.Add((PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).X);
                                        nPosYs.Add((PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).Y);
                                        nColorList.Add(CogColorConstants.Green);

                                        nCount_OK++;
                                        Mark_ret[PoketNum] = true;
                                        ResultGraphic.Add(PT_Pattern[m_PatNo, nTempPatNo_Sub].Results[0].CreateResultGraphics(Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.MatchRegion | Cognex.VisionPro.SearchMax.CogSearchMaxResultGraphicConstants.Origin));
                                    }
                                }
                            }

                            nSearchRegion = new CogRectangle(PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle);
                            ResultSearchRegion.Add(nSearchRegion);
                            if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_PMAlign_Use)
                            {
                                (PT_GPattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).SetCenterWidthHeight(MarkerPointList[PoketNum].X, MarkerPointList[PoketNum].Y,
                                                                                                        (PT_GPattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).Width, (PT_GPattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).Height);
                                PT_GPattern[m_PatNo, nTempPatNo_Sub].Run();

                                if (PT_GPattern[m_PatNo, nTempPatNo_Sub].Results != null)
                                {
                                    float nFontSize = 0;
                                    nFontSize = (float)((Display.Image.Height / Main.DEFINE.FontSize) * Display.Zoom) + (float)(8 / Display.Zoom);

                                    if (PT_GPattern[m_PatNo, nTempPatNo_Sub].Results.Count > 0)
                                    {
                                        if (PT_GPattern[m_PatNo, nTempPatNo_Sub].Results[0].Score >= PT_GAcceptScore[m_PatNo])
                                        {
                                            nMessage[1] = "G:" + PoketNum.ToString() + "->  " + (PT_GPattern[m_PatNo, nTempPatNo_Sub].Results[0].Score * 100).ToString("0.00");
                                            nMessageList.Add(nMessage[1]);
                                            nPosXs.Add((PT_GPattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).X);
                                            nPosYs.Add((PT_GPattern[m_PatNo, nTempPatNo_Sub].SearchRegion as CogRectangle).Y + nFontSize);
                                            nColorList.Add(CogColorConstants.Green);

                                            nGCount_OK++;
                                            GMark_ret[PoketNum] = true;
                                            ResultGraphic.Add(PT_GPattern[m_PatNo, nTempPatNo_Sub].Results[0].CreateResultGraphics(CogPMAlignResultGraphicConstants.MatchRegion | CogPMAlignResultGraphicConstants.MatchFeatures | CogPMAlignResultGraphicConstants.Origin));
                                        }
                                    }

                                }
                            }
                            //---------------------------------------------------------------------------------------------------------------------------------                        

                        }
                    }
                }

                Main.DrawOverlayMessage(PT_Display01, nMessageList, nColorList, nPosXs, nPosYs);

                for (int i = 0; i < ResultGraphic.Count; i++)
                {
                    Display.StaticGraphics.Add(ResultGraphic[i] as ICogGraphic, "Mark");
                }
                for (int i = 0; i < ResultSearchRegion.Count; i++)
                {
                    Display.StaticGraphics.Add(ResultSearchRegion[i] as ICogGraphic, "Search Region");
                }
                PT_Pattern[m_PatNo, nTempPatNo_Sub].SearchRegion = new CogRectangle(nBackUp_SearchRegion);
            }
            catch
            {

            }
            Lab_Tact.Text = m_Timer.GetElapsedTime().ToString();
        }
        private void BTN_READ_COUNT_Click(object sender, EventArgs e)
        {
            if (Main.AlignUnit[m_AlignNo].m_AlignName == "FPC_TRAY1" || Main.AlignUnit[m_AlignNo].m_AlignName == "FPC_TRAY2")
            {
                TRAY_POCKET_X = (short)(PLCDataTag.RData[Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.PLC_TRAY_COUNT_X]);
                TRAY_POCKET_Y = (short)(PLCDataTag.RData[Main.AlignUnit[m_AlignNo].ALIGN_UNIT_ADDR + Main.DEFINE.PLC_TRAY_COUNT_Y]);
                if (TRAY_POCKET_X == 0 && TRAY_POCKET_Y == 0)
                {
                    //                    LB_PIXELDIS.Text = "X:" + TRAY_POCKET_X.ToString() +" ,Y:"+ TRAY_POCKET_Y.ToString(); 
                    /*                    TRAY_POCKET_X = TRAY_POCKET_Y = 1;*/
                }

                NUD_POCKETCOUNT_X_00.Value = TRAY_POCKET_X;
                NUD_POCKETCOUNT_Y_01.Value = TRAY_POCKET_Y;
            }
        }
        private void NUD_POCKETCOUNT_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown nTempBTN = (NumericUpDown)sender;
            int m_Number;
            m_Number = Convert.ToInt16(nTempBTN.Name.Substring(nTempBTN.Name.Length - 2, 2));

            if (m_Number == 0) TRAY_POCKET_X = Convert.ToInt32(nTempBTN.Value);
            if (m_Number == 1) TRAY_POCKET_Y = Convert.ToInt32(nTempBTN.Value);
            if (!NUD_Initial_Flag)
                BTN_FPC_SEARCH_ALL_Click(PT_Display01);
        }
        private void DrawPocketPoint(CogRecordDisplay Display)
        {
            double pitchX = PT_TRAY_PITCH_DISX[m_PatNo];
            double pitchY = PT_TRAY_PITCH_DISY[m_PatNo];
            Main.DoublePoint FirstPoint = new Main.DoublePoint();
            List<CogGraphicLabel> nLabel = new List<CogGraphicLabel>();
            CogGraphicLabel nTempLabel = new CogGraphicLabel();

            MarkerPointList.Clear();
            nLabel.Clear();
            int PoketNum = 0;
            for (int nY = 0; nY < TRAY_POCKET_Y; nY++)
            {
                for (int nX = 0; nX < TRAY_POCKET_X; nX++)
                {

                    PoketNum = (nY * TRAY_POCKET_X) + nX;
                    //                     FirstPoint.X = LineEdge_CircleList[Main.DEFINE.FINDLINE_CONNER_NUM].CenterX + PT_TRAY_GUIDE_DISX[m_PatNo] + (nX * pitchX);
                    //                     FirstPoint.Y = LineEdge_CircleList[Main.DEFINE.FINDLINE_CONNER_NUM].CenterY + PT_TRAY_GUIDE_DISY[m_PatNo] + (nY * pitchY);

                    FirstPoint.X = PT_TRAY_GUIDE_DISX[m_PatNo] + (nX * pitchX);
                    FirstPoint.Y = PT_TRAY_GUIDE_DISY[m_PatNo] + (nY * pitchY);

                    CogPointMarker Marker = new CogPointMarker();
                    //                     FirstPoint = RotationChange(LineEdge_CircleList[Main.DEFINE.FINDLINE_CONNER_NUM].CenterX, LineEdge_CircleList[Main.DEFINE.FINDLINE_CONNER_NUM].CenterY, FirstPoint, Angle);

                    Marker.X = FirstPoint.X;
                    Marker.Y = FirstPoint.Y;


                    Marker.GraphicDOFEnable = CogPointMarkerDOFConstants.All;
                    Marker.SizeInScreenPixels = 80; //화면에 표시 되는 + 모양 크기 .
                    MarkerPointList.Add(Marker);


                    nTempLabel.Text = PoketNum.ToString();
                    nTempLabel.X = Marker.X;
                    nTempLabel.Y = Marker.Y;

                    nLabel.Add(nTempLabel);

                }
            }

            for (int i = 0; i < TRAY_POCKET_X * TRAY_POCKET_Y; i++)
                Display.InteractiveGraphics.Add(MarkerPointList[i] as ICogGraphicInteractive, "FINDLINE_" + i.ToString(), false);

        }
        private void BTN_FPC_SEARCH_ALL_Click(object sender, EventArgs e)
        {
            if (TABC_MANU.SelectedIndex != Main.DEFINE.M_BLOBTOOL)
                BTN_FPC_SEARCH_ALL_Click(PT_Display01);
            else
                BTN_FPC_BLOB_SEARCH_ALL_Click(PT_Display01);

            DisplayFit(PT_Display01);
        }
        private void BTN_FIRST_POS_REG_Click(object sender, EventArgs e)
        {
            PT_TRAY_GUIDE_DISX[m_PatNo] = FirstPocketPos.X;
            PT_TRAY_GUIDE_DISY[m_PatNo] = FirstPocketPos.Y;
            PT_TRAY_PITCH_DISX[m_PatNo] = X_PocketPitchPos.X - FirstPocketPos.X;
            PT_TRAY_PITCH_DISY[m_PatNo] = Y_PocketPitchPos.Y - FirstPocketPos.Y;

            NUD_Initial_Flag = true;
            if (PT_TRAY_PITCH_DISX[m_PatNo] < 0) X_PocketPitchPos.Rotation = 0 * Main.DEFINE.radian;
            else X_PocketPitchPos.Rotation = 180 * Main.DEFINE.radian * -1;
            if (PT_TRAY_PITCH_DISY[m_PatNo] < 0) Y_PocketPitchPos.Rotation = 90 * Main.DEFINE.radian;
            else Y_PocketPitchPos.Rotation = 90 * Main.DEFINE.radian * -1;
            NUD_GUIDEDISX.Value = (decimal)PT_TRAY_GUIDE_DISX[m_PatNo];
            NUD_GUIDEDISY.Value = (decimal)PT_TRAY_GUIDE_DISY[m_PatNo];
            NUD_PITCHDISX.Value = (decimal)PT_TRAY_PITCH_DISX[m_PatNo];
            NUD_PITCHDISY.Value = (decimal)PT_TRAY_PITCH_DISY[m_PatNo];
            NUD_Initial_Flag = false;
        }

        private void DisplayClear()
        {
            PT_DISPLAY_CONTROL.DisplayClear();
        }
        private void BTN_FIRST_POS_SHOW_Click(object sender, EventArgs e)
        {
            DisplayClear();
            FirstPocketPos.X = PT_TRAY_GUIDE_DISX[m_PatNo];
            FirstPocketPos.Y = PT_TRAY_GUIDE_DISY[m_PatNo];

            if (PT_TRAY_PITCH_DISX[m_PatNo] < 0) X_PocketPitchPos.Rotation = 0 * Main.DEFINE.radian;
            else X_PocketPitchPos.Rotation = 180 * Main.DEFINE.radian * -1;
            if (PT_TRAY_PITCH_DISY[m_PatNo] < 0) Y_PocketPitchPos.Rotation = 90 * Main.DEFINE.radian;
            else Y_PocketPitchPos.Rotation = 90 * Main.DEFINE.radian * -1;

            X_PocketPitchPos.X = PT_TRAY_GUIDE_DISX[m_PatNo] + PT_TRAY_PITCH_DISX[m_PatNo];
            X_PocketPitchPos.Y = PT_TRAY_GUIDE_DISY[m_PatNo];

            Y_PocketPitchPos.X = PT_TRAY_GUIDE_DISX[m_PatNo];
            Y_PocketPitchPos.Y = PT_TRAY_GUIDE_DISY[m_PatNo] + PT_TRAY_PITCH_DISY[m_PatNo];

            CogGraphicInteractiveCollection PatternInfo = new CogGraphicInteractiveCollection();
            PatternInfo.Add(FirstPocketPos);
            PatternInfo.Add(X_PocketPitchPos);
            PatternInfo.Add(Y_PocketPitchPos);
            PT_Display01.InteractiveGraphics.AddList(PatternInfo, "PATTERN_INFO", false);

            if (Main.DEFINE.PROGRAM_TYPE == "FOF_PC1") PB_FOF_FPC.Visible = true;
            if (Main.DEFINE.PROGRAM_TYPE == "TFOF_PC1") PB_TFOF_PANEL.Visible = true;
        }
        private void BTN_FPC_BLOB_SEARCH_ALL_Click(CogRecordDisplay Display)
        {
            m_Timer.StartTimer();
            try
            {
                Main.DisplayClear(Display);
                DrawPocketPoint(Display);

                CogRectangleAffine BlobSearchRegion = new CogRectangleAffine();
                List<CogRectangleAffine> BlobTrainRegion = new List<CogRectangleAffine>();

                List<CogRectangleAffine> nBlobSearchRegion = new List<CogRectangleAffine>(); //BLOB 설정영역  
                List<CogPolygon> ResultBoundary = new List<CogPolygon>();   // BLOB 찾은결과 경계 영역

                double[] BlobArea = new double[Main.DEFINE.BLOB_CNT_MAX];
                double Score = new double();

                List<string> nMessageList = new List<string>();
                string[] nMessage = new string[2];
                CogColorConstants nColor;
                List<CogColorConstants> nColorList = new List<CogColorConstants>();

                List<double> nPosXs = new List<double>();
                List<double> nPosYs = new List<double>();

                int PoketNum = 0;
                int nCount_OK = 0;
                bool[] Mark_ret = new bool[TRAY_POCKET_X * TRAY_POCKET_Y];

                int nTempPatNo_Sub = 0;
                nTempPatNo_Sub = m_PatNo_Sub;

                double pitchX = PT_TRAY_PITCH_DISX[m_PatNo];
                double pitchY = PT_TRAY_PITCH_DISY[m_PatNo];


                for (int i = 0; i < Main.DEFINE.BLOB_CNT_MAX; i++)
                {
                    if (PT_BlobPara[m_PatNo, i].m_UseCheck)
                    {
                        BlobTrainRegion.Add(new CogRectangleAffine(PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine));
                    }
                }
                BlobSearchRegion = new CogRectangleAffine(PT_BlobTools[m_PatNo, m_SelectBlob].Region as CogRectangleAffine);

                LB_List.Items.Clear();

                PT_BlobTools[m_PatNo, 0].InputImage = CopyIMG(Main.vision.CogCamBuf[m_CamNo]);

                if (MarkerPointList.Count == TRAY_POCKET_X * TRAY_POCKET_Y)
                {
                    for (int nY = 0; nY < TRAY_POCKET_Y; nY++)
                    {
                        for (int nX = 0; nX < TRAY_POCKET_X; nX++)
                        {
                            PoketNum = (nY * TRAY_POCKET_X) + nX;

                            for (int i = 0; i < 1; i++)
                            {
                                BlobArea[i] = new double();
                                if (PT_BlobPara[m_PatNo, i].m_UseCheck)
                                {


                                    //                                    PT_BlobTools[m_PatNo, i].InputImage = PT_BlobTools[m_PatNo, i].InputImage;

                                    //  BlobSearchRegion = new CogRectangleAffine(PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine);

                                    (PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterX = MarkerPointList[PoketNum].X;
                                    (PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterY = MarkerPointList[PoketNum].Y;

                                    //                                     BlobSearchRegion.CenterX = PT_TRAY_GUIDE_DISX[m_PatNo] + (nX * pitchX);
                                    //                                     BlobSearchRegion.CenterY = PT_TRAY_GUIDE_DISY[m_PatNo] + (nY * pitchY);
                                    // 
                                    //                                     BlobSearchRegion.SetCenterLengthsRotationSkew(PT_TRAY_GUIDE_DISX[m_PatNo] + (nX * pitchX),PT_TRAY_GUIDE_DISY[m_PatNo] + (nY * pitchY), BlobSearchRegion.SideXLength, BlobSearchRegion.SideYLength, BlobSearchRegion.Rotation, BlobSearchRegion.Skew);


                                    //                                     BlobSearchRegion.CenterX = PT_TRAY_GUIDE_DISX[m_PatNo] + (nX * Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TRAY_PITCH_DISX);
                                    //                                     BlobSearchRegion.CenterY = PT_TRAY_GUIDE_DISY[m_PatNo] + (nY * Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TRAY_PITCH_DISY);

                                    //                                     BlobSearchRegion.CenterX = BlobTrainRegion[i].CenterX + (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TRAY_PITCH_DISX * (nX));
                                    //                                     BlobSearchRegion.CenterY = BlobTrainRegion[i].CenterY + (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TRAY_PITCH_DISY * (nY));
                                    //     PT_BlobTools[m_PatNo, i].Region = new CogRectangleAffine(BlobSearchRegion);
                                    //    PT_BlobTools[m_PatNo, i].Region = BlobSearchRegion;

                                    PT_BlobTools[m_PatNo, i].Run();
                                    if (PT_BlobTools[m_PatNo, i].Results != null)
                                    {
                                        if (PT_BlobTools[m_PatNo, i].Results.GetBlobs().Count <= 0)
                                        {
                                            int GetBlobsCount = 1;
                                            GetBlobsCount = PT_BlobTools[m_PatNo, i].Results.GetBlobs().Count;
                                            // 
                                            for (int j = 0; j < PT_BlobTools[m_PatNo, i].Results.GetBlobs().Count; j++) //PMBlobResults[i].BlobToolResult.GetBlobs().Count
                                            {
                                                ResultBoundary.Add(PT_BlobTools[m_PatNo, i].Results.GetBlobs()[j].GetBoundary());  //--BLOB RESULTS BOUNDARY
                                                //   BlobArea[i] += PT_BlobTools[m_PatNo, i].Results.GetBlobs()[j].Area;
                                            }

                                            //                                             Score = 100 - ((BlobArea[i] / BlobSearchRegion.Area) * 100);
                                            nMessage[0] = "B " + PoketNum.ToString();// +"->  " + "NG" + " " + Score.ToString("0.0");
                                            Mark_ret[PoketNum] = false;
                                            nColor = CogColorConstants.Red;
                                        }
                                        else
                                        {
                                            for (int j = 0; j < PT_BlobTools[m_PatNo, i].Results.GetBlobs().Count; j++) //PMBlobResults[i].BlobToolResult.GetBlobs().Count
                                            {
                                                ResultBoundary.Add(PT_BlobTools[m_PatNo, i].Results.GetBlobs()[j].GetBoundary());  //--BLOB RESULTS BOUNDARY
                                                //   BlobArea[i] += PT_BlobTools[m_PatNo, i].Results.GetBlobs()[j].Area;
                                            }
                                            Score = 100;
                                            nMessage[0] = "B " + PoketNum.ToString();// +"->  " + "OK" + " " + Score.ToString("0.0");
                                            Mark_ret[PoketNum] = true;
                                            nCount_OK++;
                                            nColor = CogColorConstants.Green;
                                        }
                                        nMessageList.Add(nMessage[0]);
                                        nColorList.Add(nColor);
                                        nPosXs.Add((PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterX);
                                        nPosYs.Add((PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine).CenterY);



                                        //   nBlobSearchRegion.Add(PT_BlobTools[m_PatNo, i].Region as CogRectangleAffine);      //------------------------BLOB SEARCH AREA 
                                    }
                                    Display.StaticGraphics.Add(PT_BlobTools[m_PatNo, i].Region as ICogGraphic, "Search Region");
                                }
                            }
                        }
                    }
                }
                PT_BlobTools[m_PatNo, m_SelectBlob].Region = new CogRectangleAffine(BlobSearchRegion as CogRectangleAffine);


                Main.DrawOverlayMessage(Display, nMessageList, nColorList, nPosXs, nPosYs);

                //------------------------BLOB RESULT BOUNDARY-----------------------------------
                for (int i = 0; i < ResultBoundary.Count; i++)
                {
                    ResultBoundary[i].Color = CogColorConstants.Green;
                    Display.StaticGraphics.Add(ResultBoundary[i] as ICogGraphic, "BLOB Region");
                }
            }
            catch
            {

            }
            Lab_Tact.Text = m_Timer.GetElapsedTime().ToString();
        }


        #endregion

        #endregion

        private void BTN_CIRCLE_CNT_UP_Click(object sender, EventArgs e)
        {
            try
            {
                PT_CircleTool.RunParams.NumCalipers++;
                LB_CIRCLE_CNT.Text = PT_CircleTool.RunParams.NumCalipers.ToString();
            }
            catch (System.ArgumentException ex)
            {

            }
            DrawCircleRegion();
        }

        private void BTN_CIRCLE_CNT_DN_Click(object sender, EventArgs e)
        {
            try
            {
                PT_CircleTool.RunParams.NumCalipers--;
                LB_CIRCLE_CNT.Text = PT_CircleTool.RunParams.NumCalipers.ToString();
            }
            catch (System.ArgumentException ex)
            {

            }
            DrawCircleRegion();
        }

        private void NUD_CIRCLE_IGNCNT_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                PT_CircleTool.RunParams.NumToIgnore = (int)NUD_CIRCLE_IGNCNT.Value;
            }
            catch (System.ArgumentException)
            {

            }
            if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_FINDCIRCLETOOL)
            {
                ThresValue_Sts = true;
                BTN_PATTERN_RUN_Click(null, null);
                ThresValue_Sts = false;
            }
        }

        private void NUD_LINE_NORMAL_ANGLE_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.ExpectedLineNormal.Angle = (double)NUD_LINE_NORMAL_ANGLE.Value * Main.DEFINE.radian;

            //if (!m_PatchangeFlag && !m_TABCHANGE_MODE && Convert.ToInt32(TABC_MANU.SelectedTab.Tag) == Main.DEFINE.M_FINDLINETOOL)
            //{
            //    ThresValue_Sts = true;
            //    BTN_PATTERN_RUN_Click(null, null);
            //    ThresValue_Sts = false;
            //}
        }

        private void NUD_GRADIENT_KERNEL_SIZE_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.EdgeDetectionParams.GradientKernelSizeInPixels = (int)NUD_GRADIENT_KERNEL_SIZE.Value;

            if (NUD_PROJECTION_LENGTH.Value < NUD_GRADIENT_KERNEL_SIZE.Value)
                NUD_PROJECTION_LENGTH.Value = NUD_GRADIENT_KERNEL_SIZE.Value;
        }

        private void NUD_PROJECTION_LENGTH_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.EdgeDetectionParams.ProjectionLengthInPixels = (int)NUD_PROJECTION_LENGTH.Value;
            if (NUD_PROJECTION_LENGTH.Value < NUD_GRADIENT_KERNEL_SIZE.Value)
                NUD_GRADIENT_KERNEL_SIZE.Value = NUD_PROJECTION_LENGTH.Value;
        }

        private void NUD_MAX_LINENUM_ValueChanged(object sender, EventArgs e)
        {
            // 9.1에서 Multiple line 지원 안함...
            PT_LineMaxTool.RunParams.MaxNumLines = (int)NUD_MAX_LINENUM.Value;

            //if (PT_LineMaxTool.RunParams.MaxNumLines > 1)
            //{
            //    label46.Visible = true; RBTN_HORICON_YMIN.Visible = true; RBTN_HORICON_YMAX.Visible = true;
            //    label47.Visible = true; RBTN_VERTICON_XMIN.Visible = true; RBTN_VERTICON_XMAX.Visible = true;

            //    try
            //    {
            //        RBTN_LINEMAX_H_COND[PT_FindLinePara[m_PatNo, m_SelectFindLine].m_LineMaxHCond].Checked = true;
            //        RBTN_LINEMAX_V_COND[PT_FindLinePara[m_PatNo, m_SelectFindLine].m_LineMaxVCond].Checked = true;
            //    }
            //    catch (System.ArgumentException)
            //    {

            //    }
            //}
            //else
            //{
            //    label46.Visible = false; RBTN_HORICON_YMIN.Visible = false; RBTN_HORICON_YMAX.Visible = false;
            //    label47.Visible = false; RBTN_VERTICON_XMIN.Visible = false; RBTN_VERTICON_XMAX.Visible = false;
            //}
        }

        private void NUD_ANGLE_TOLERANCE_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.EdgeAngleTolerance = (double)NUD_ANGLE_TOLERANCE.Value * Main.DEFINE.radian;
        }

        private void NUD_DIST_TOLERANCE_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.DistanceTolerance = (int)NUD_DIST_TOLERANCE.Value;
        }

        private void NUD_LINE_ANGLE_TOL_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.LineAngleTolerance = (double)NUD_LINE_ANGLE_TOL.Value * Main.DEFINE.radian;
        }

        private void NUD_COVERAGE_THRES_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.CoverageThreshold = (double)NUD_COVERAGE_THRES.Value;
        }

        private void NUD_LENGTH_THRES_ValueChanged(object sender, EventArgs e)
        {
            PT_LineMaxTool.RunParams.LengthThreshold = (double)NUD_LENGTH_THRES.Value;
        }

        private void RBTN_LINEMAX_CONDITION_Clicked(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;

            if (TempBTN.Name == "RBTN_HORICON_YMIN")
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineMaxHCond = Main.DEFINE.LINEMAX_H_YMIN;
            }
            else if (TempBTN.Name == "RBTN_HORICON_YMAX")
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineMaxHCond = Main.DEFINE.LINEMAX_H_YMAX;

            }
            else if (TempBTN.Name == "RBTN_VERTICON_XMIN")
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineMaxVCond = Main.DEFINE.LINEMAX_V_XMIN;

            }
            else if (TempBTN.Name == "RBTN_VERTICON_XMAX")
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineMaxVCond = Main.DEFINE.LINEMAX_V_XMAX;

            }
        }

        private void RBTN_CALIPER_METHOD_Clicked(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;

            if (TempBTN.Name == "RBTN_CALIPER_METHOD_SCROE")
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineCaliperMethod = Main.DEFINE.CLP_METHOD_SCORE;
            }
            else if (TempBTN.Name == "RBTN_CALIPER_METHOD_POS")
            {
                PT_FindLinePara[m_PatNo, m_LineSubNo, m_SelectFindLine].m_LineCaliperMethod = Main.DEFINE.CLP_METHOD_POS;
            }
            else if (TempBTN.Name == "RBTN_CIR_CALIPER_METHOD_SCROE")
            {
                PT_CirclePara[m_PatNo, m_SelectCircle].m_CircleCaliperMethod = Main.DEFINE.CLP_METHOD_SCORE;
            }
            else if (TempBTN.Name == "RBTN_CIR_CALIPER_METHOD_POS")
            {
                PT_CirclePara[m_PatNo, m_SelectCircle].m_CircleCaliperMethod = Main.DEFINE.CLP_METHOD_POS;
            }
        }

        private void RBTN_CALIPER_METHOD_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            if (TempBTN.Checked)
                TempBTN.BackColor = System.Drawing.Color.LawnGreen;
            else
                TempBTN.BackColor = System.Drawing.Color.DarkGray;
        }

        private void CB_FINDLINE_SUBLINE_SelectionChangeCommitted(object sender, EventArgs e)
        {
            m_LineSubNo = CB_FINDLINE_SUBLINE.SelectedIndex;

            FINDLINE_Change();
        }

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, bool oldPara, bool newPara)
        {
            string strLog = "CAM" + nAlignUnit.ToString();

            if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
                strLog += " ALIGN ";
            else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
                strLog += " INSPECTION ";

            if (oldPara != newPara)
            {
                strLog += ParaName + " [" + oldPara.ToString() + "] ▶▷▶ [" + newPara.ToString() + "]";
                Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            }
        }

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, int oldPara, int newPara)
        {
            string strLog = "CAM" + nAlignUnit.ToString();

            if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
                strLog += " ALIGN ";
            else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
                strLog += " INSPECTION ";

            if (oldPara != newPara)
            {
                strLog += ParaName + " [" + oldPara + "] ▶▷▶ [" + newPara + "]";
                Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            }
        }

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, double oldPara, double newPara)
        {
            string strLog = "CAM" + nAlignUnit.ToString();

            if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
                strLog += " ALIGN ";
            else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
                strLog += " INSPECTION ";

            if (oldPara != newPara)
            {
                strLog += ParaName + " [" + oldPara.ToString("0.0000") + "] ▶▷▶ [" + newPara.ToString("0.0000") + "]";
                Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            }
        }

        private void CheckChangedParams(int nAlignUnit, int nCamUnit, string ParaName, string oldPara, string newPara)
        {
            string strLog = "CAM" + nAlignUnit.ToString();

            if (nCamUnit == Main.DEFINE.CAM_SELECT_ALIGN)
                strLog += " ALIGN ";
            else if (nCamUnit == Main.DEFINE.CAM_SELECT_INSPECT)
                strLog += " INSPECTION ";

            if (oldPara != newPara)
            {
                strLog += ParaName + " [" + oldPara + "] ▶▷▶ [" + newPara + "]";
                Save_SystemLog(strLog, Main.DEFINE.CHANGEPARA);
            }
        }

        object syncLock_Log = new object();
        private void Save_SystemLog(string nMessage, string nType)
        {
            string nFolder;
            string nFileName = "";
            nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            string Date;
            Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            lock (syncLock_Log)
            {
                try
                {
                    switch (nType)
                    {
                        case Main.DEFINE.CHANGEPARA:
                            nFileName = "ChangePara.txt";
                            nMessage = Date + nMessage;
                            break;
                        case Main.DEFINE.DATA:
                            nFileName = "Data.csv";
                            nMessage = Date + nMessage;
                            break;
                        case Main.DEFINE.CMD:
                            nFileName = "CMD.txt";
                            nMessage = Date + nMessage;
                            break;
                    }

                    StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
                    SW.WriteLine(nMessage);
                    SW.Close();
                }
                catch
                {

                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (PT_DISPLAY_CONTROL.CrossLineChecked) CrossLine();
            timer2.Enabled = false;
        }

        private void LB_SEARCH_CIR_Click(object sender, EventArgs e)
        {
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                using (Form_KeyPad form_keypad = new Form_KeyPad(10, 10000, PT_CircleTool.RunParams.CaliperSearchLength, "CALIPER SEARCH LENGTH", 1))
                {
                    form_keypad.ShowDialog();
                    PT_CircleTool.RunParams.CaliperSearchLength = form_keypad.m_data;
                }
                DrawCircleRegion();
            }
        }

        private void LB_PROJECTION_CIR_Click(object sender, EventArgs e)
        {
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                using (Form_KeyPad form_keypad = new Form_KeyPad(10, 10000, PT_CircleTool.RunParams.CaliperProjectionLength, "CALIPER PROJECTION LENGTH", 1))
                {
                    form_keypad.ShowDialog();
                    PT_CircleTool.RunParams.CaliperProjectionLength = form_keypad.m_data;
                }
                DrawCircleRegion();
            }
        }

        private void LB_RADIUS_CIR_Click(object sender, EventArgs e)
        {
            if (Main.DEFINE.PROGRAM_TYPE == "QD_LPA_PC1")
            {
                using (Form_KeyPad form_keypad = new Form_KeyPad(10, 10000, PT_CircleTool.RunParams.ExpectedCircularArc.Radius, "EXPECED CIRCULAR_ARC RADIUS", 1))
                {
                    form_keypad.ShowDialog();
                    PT_CircleTool.RunParams.ExpectedCircularArc.Radius = form_keypad.m_data;
                    LB_RADIUS_CIR.Text = PT_CircleTool.RunParams.ExpectedCircularArc.Radius.ToString(); //이 파라미터는 changed에서 안들어감 버그인듯. 다른건들어감
                }
                DrawCircleRegion();
            }
        }

        private void BTN_DIVIDECNT_UP_Click(object sender, EventArgs e)
        {
            if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt < 0) PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt = 0;
            try
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt++;
                if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt >= 10)
                    PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt = 10;
                LB_DIVIDE_COUNT.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt.ToString();
            }
            catch (System.ArgumentException ex)
            {
            }
            DrawCaliperRegion();
        }

        private void BTN_DIVIDECNT_DOWN_Click(object sender, EventArgs e)
        {
            if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt < 0) PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt = 0;
            try
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt--;
                if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt < 0)
                    PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt = 0;
                LB_DIVIDE_COUNT.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt.ToString();
            }
            catch (System.ArgumentException ex)
            {
            }
            DrawCaliperRegion();
        }

        private void BTN_DIVIDEOFFSET_UP_Click(object sender, EventArgs e)
        {
            if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset < 0) PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset = 0;
            try
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset++;
                if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset >= 10)
                    PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset = 10;
                LB_DIVIDE_OFFSET.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset.ToString();
            }
            catch (System.ArgumentException ex)
            {
            }
            DrawCaliperRegion();
        }

        private void BTN_DIVIDEOFFSET_DOWN_Click(object sender, EventArgs e)
        {
            if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset < 0) PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset = 0;
            try
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset--;
                if (PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset < 0)
                    PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset = 0;
                LB_DIVIDE_OFFSET.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset.ToString();
            }
            catch (System.ArgumentException ex)
            {
            }
            DrawCaliperRegion();
        }

        private void CB_COP_MODE_CHECK_Click(object sender, EventArgs e)
        {
            if (CB_COP_MODE_CHECK.Checked)
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_bCOPMode = true;
                label52.Visible = true; LB_DIVIDE_COUNT.Visible = true; BTN_DIVIDECNT_UP.Visible = true; BTN_DIVIDECNT_DOWN.Visible = true;
                label53.Visible = true; LB_DIVIDE_OFFSET.Visible = true; BTN_DIVIDEOFFSET_UP.Visible = true; BTN_DIVIDEOFFSET_DOWN.Visible = true;

                LB_DIVIDE_COUNT.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROICnt.ToString();
                LB_DIVIDE_OFFSET.Text = PT_CaliPara[m_PatNo, m_SelectCaliper].m_nCOPROIOffset.ToString();
            }
            else
            {
                PT_CaliPara[m_PatNo, m_SelectCaliper].m_bCOPMode = false;
                label52.Visible = false; LB_DIVIDE_COUNT.Visible = false; BTN_DIVIDECNT_UP.Visible = false; BTN_DIVIDECNT_DOWN.Visible = false;
                label53.Visible = false; LB_DIVIDE_OFFSET.Visible = false; BTN_DIVIDEOFFSET_UP.Visible = false; BTN_DIVIDEOFFSET_DOWN.Visible = false;
            }
        }

        private void CB_COP_MODE_CHECK_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_COP_MODE_CHECK.Checked)
            {
                CB_COP_MODE_CHECK.BackColor = System.Drawing.Color.LawnGreen;
            }
            else
            {
                CB_COP_MODE_CHECK.BackColor = System.Drawing.Color.DarkGray;
            }
        }
        #region SD BIO
        private void ROIType(object sender, EventArgs e)
        {
            Button Btn = (Button)sender;
            if (Convert.ToInt32(Btn.Tag.ToString()) == 0)
            {
                m_enumROIType = enumROIType.Line;
            }
            else
            {
                m_enumROIType = enumROIType.Circle;
            }
            Set_InspParams();
        }

        private void btn_ROI_SHOW_Click(object sender, EventArgs e)
        {
            ExecuteROIShow();
        }

        private void ExecuteROIShow()
        {
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            UpDataTool();
            SetText();
            //shkang_s
            string strTemp;
            int itype;
            strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
            if (strTemp == "Line")
                itype = 0;
            else
                itype = 1;
            m_enumROIType = (enumROIType)itype;
            //shkang_e
            if (m_enumROIType == enumROIType.Line)
                TrackLineROI(m_TempFindLineTool);
            else
                TrackCircleROI(m_TempFindCircleTool);
            //if (_useROITracking)
            //{
            //    TrackingROI();
            //}
            //else
            //{
            //    if (m_enumROIType == enumROIType.Line)
            //        TrackLineROI(m_TempFindLineTool);
            //    else
            //        TrackCircleROI(m_TempFindCircleTool);
            //}
        }

        private void chkUseRoiTracking_CheckedChanged(object sender, EventArgs e)
        {
            //_useROITracking = chkUseRoiTracking.Checked;
            //dBlobPrevTranslationX = 0;
            //dBlobPrevTranslationY = 0;
            //dInspPrevTranslationX = 0;
            //dInspPrevTranslationY = 0;
            //if (PrevCenterX != 0)
            //{
            //    CogRectangle Rect = (CogRectangle)m_CogHistogramTool[m_HistoROI].Region;
            //    Rect.SetCenterWidthHeight(PrevCenterX, PrevCenterY, Rect.Width, Rect.Height);
            //    m_CogHistogramTool[m_HistoROI].Region = Rect;
            //}

            PrevCenterX = 0;
            PrevCenterY = 0;
            PrevMarkX = 0;
            PrevMarkY = 0;
            m_bTrakingRoot[m_BlobROI] = false;
            if (chkUseRoiTracking.Checked == true)
            {
                //Live Mode On상태일 시, Off로 변경
                if (BTN_LIVEMODE.Checked)
                {
                    BTN_LIVEMODE.Checked = false;
                    BTN_LIVEMODE.BackColor = Color.DarkGray;
                }
                PT_Display01.Image = OriginImage;

                if (FinalTracking() == true)
                    _useROITracking = chkUseRoiTracking.Checked;
                UpDataTool();
                SetText();

                //shkang_s
                string strTemp;
                int itype;
                strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
                if (strTemp == "Line")
                    itype = 0;
                else
                    itype = 1;
                m_enumROIType = (enumROIType)itype;
                //shkang_e

                if (m_enumROIType == enumROIType.Line)
                    TrackLineROI(m_TempFindLineTool);
                else
                    TrackCircleROI(m_TempFindCircleTool);
            }
            else
            {
                PT_Display01.Image = OriginImage;
            }
            //ExecuteROIShow();
        }

        private bool TrackingROI(double PatPointX, double PatPointY, double dTransX, double dTransY)
        {
            //if (Search_PATCNL())
            //{
            double ROIX = 0, ROIY = 0, RotT = 0;
            bool Res = false;
            if (LineTrakingROI(dTransX, dTransY, ref ROIX, ref ROIY, ref RotT))
            {
                Res = true;
                if (!m_bROIFinealignFlag)
                {
                    //Tracking.TranslationT = RotT;
                    //TrackingManager(m_enumROIType);
                    CogFixtureTool mCogFixtureTool = new CogFixtureTool();
                    mCogFixtureTool.InputImage = PT_Display01.Image;
                    CogTransform2DLinear TempData = new CogTransform2DLinear();
                    TempData.TranslationX = PatPointX;
                    TempData.TranslationY = PatPointY;
                    //TempData.TranslationX = dTransX;
                    //TempData.TranslationY = dTransY;
                    TempData.Rotation = RotT;
                    mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
                    mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
                    mCogFixtureTool.Run();
                    PT_Display01.InteractiveGraphics.Clear();
                    PT_Display01.StaticGraphics.Clear();

                    PT_Display01.Image = mCogFixtureTool.OutputImage;
                }

            }
            else
            {
                //Res = false;
            }
            return Res;
            //}
            //else
            //    MessageBox.Show("Failed to search for alignment mark");
        }

        private void TrackingManager(enumROIType roiType)
        {
            switch (roiType)
            {
                case enumROIType.Line:
                    LineTrackingProperty(m_TempFindLineTool);
                    break;
                case enumROIType.Circle:
                    CircleTrackingProperty(m_TempFindCircleTool);
                    break;
                default:
                    break;
            }
        }
        private bool LineTrakingROI(double dTransX, double dTransY, ref double ROIX, ref double ROIY, ref double RotT)
        {
            bool Res = false;
            //bool Film_Res = true;
            try
            {
                //double dInspectionDistanceX;
                CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
                PT_Display01.InteractiveGraphics.Clear();
                PT_Display01.StaticGraphics.Clear();
                resultGraphics.Clear();
                double[] dx = new double[4];
                double[] dy = new double[4];
                bool bSearchRes = Search_PATCNL();
                if (PT_Pattern[m_PatNo, m_PatNo_Sub].Results[0].Score <= PT_AcceptScore[m_PatNo])
                {
                    MessageBox.Show("Mark Search Fail");
                    return false;
                }
                if (bSearchRes == true)
                {

                    //double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                    //double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                    double TranslationX = dTransX;
                    double TranslationY = dTransY;
                    CogIntersectLineLineTool[] CrossPoint = new CogIntersectLineLineTool[2];
                    CogLine[] Line = new CogLine[4];
                    for (int i = 0; i < 4; i++)
                    {
                        if (i < 2)
                        {
                            CrossPoint[i] = new CogIntersectLineLineTool();
                            CrossPoint[i].InputImage = (CogImage8Grey)PT_Display01.Image;
                        }
                        Line[i] = new CogLine();
                        m_TeachLine[i].InputImage = (CogImage8Grey)PT_Display01.Image;
                        double TempStartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX;
                        double TempStartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY;
                        double TempEndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX;
                        double TempEndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY;


                        double StartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX - TranslationX;
                        double StartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY - TranslationY;
                        double EndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX - TranslationX;
                        double EndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY - TranslationY;

                        m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = StartA_X;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = StartA_Y;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = EndA_X;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = EndA_Y;

                        m_TeachLine[i].Run();

                        if (m_TeachLine[i].Results != null)
                        {
                            //shkang_
                            if (Line[i] == null)
                                Line[i] = new CogLine();

                            Line[i] = m_TeachLine[i].Results.GetLine();
                            if (i < 2)
                                Line[i].Color = CogColorConstants.Blue;
                            else
                                Line[i].Color = CogColorConstants.Orange;
                            //resultGraphics.Add(Line[i]);
                        }
                        m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = TempStartA_X;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = TempStartA_Y;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = TempEndA_X;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = TempEndA_Y;
                    }

                    //shkang_s 
                    //필름 밀림 융착 검사 (자재 X거리 검출)
                    //CogGraphicLabel LabelTest = new CogGraphicLabel();
                    //LabelTest.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                    //LabelTest.Color = CogColorConstants.Green;

                    //dInspectionDistanceX = Line[3].X - Line[1].X;   //X 거리 검출
                    //if (dObjectDistanceX + dObjectDistanceSpecX <= dInspectionDistanceX) //NG - Film NG
                    //{
                    //    Film_Res = false;
                    //    LabelTest.X = 1000;
                    //    LabelTest.Y = 180;
                    //    LabelTest.Color = CogColorConstants.Red;
                    //    LabelTest.Text = string.Format("Film NG, X:{0:F3}", dInspectionDistanceX);
                    //}
                    //else   //OK - Film OK
                    //{
                    //    Film_Res = true;
                    //    LabelTest.X = 1000;
                    //    LabelTest.Y = 180;
                    //    LabelTest.Color = CogColorConstants.Green;
                    //    LabelTest.Text = string.Format("Film OK, X:{0:F3}", dInspectionDistanceX);
                    //}
                    //resultGraphics.Add(LabelTest);
                    //shkang_e

                    CrossPoint[0].LineA = Line[0];
                    CrossPoint[0].LineB = Line[1];
                    CrossPoint[1].LineA = Line[2];
                    CrossPoint[1].LineB = Line[3];
                    for (int i = 0; i < 2; i++)
                    {
                        CogGraphicLabel ThetaLabelTest = new CogGraphicLabel();
                        ThetaLabelTest.Font = new Font(Main.DEFINE.FontStyle, 15, FontStyle.Bold);
                        ThetaLabelTest.Color = CogColorConstants.Green;
                        if (Line[0] == null || Line[1] == null) return false;
                        if (Line[2] == null || Line[3] == null) return false;
                        CrossPoint[i].Run();
                        if (CrossPoint[i] != null)
                        {
                            dCrossX[i] = CrossPoint[i].X;
                            dCrossY[i] = CrossPoint[i].Y;
                            dAngle[i] = CrossPoint[i].Angle;
                            CogPointMarker PointMark = new CogPointMarker();
                            PointMark.Color = CogColorConstants.Green;
                            PointMark.SizeInScreenPixels = 50;
                            PointMark.X = CrossPoint[i].X;
                            PointMark.Y = CrossPoint[i].Y;
                            PointMark.Rotation = dAngle[i];
                            //2023 0228 YSH 표시위치 이상문제로 주석처리
                            //resultGraphics.Add(PointMark); 
                            if (i == 0)
                            {
                                ThetaLabelTest.X = 350;
                                ThetaLabelTest.Y = 100;
                                ThetaLabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
                            }
                            else
                            {
                                ThetaLabelTest.X = 350;
                                ThetaLabelTest.Y = 250;
                                ThetaLabelTest.Text = string.Format("Right Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
                            }
                            //resultGraphics.Add(ThetaLabelTest);
                        }
                    }
                    //2023 0228 YSH 표시위치 이상문제로 주석처리
                    //PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
                }
                double TrackingX, TrackingY, dRotT;
                TrackingROICalculate(out TrackingX, out TrackingY, out dRotT);
                RotT = dRotT;
                ROIX = TrackingX;
                ROIY = TrackingY;
                //shkang_s 임시 Theta Test
                //CogGraphicLabel testTheta = new CogGraphicLabel();
                //testTheta.Font = new Font(Main.DEFINE.FontStyle, 15, FontStyle.Bold);
                //testTheta.X = -300;
                //testTheta.Y = 2000;
                //testTheta.Text = string.Format("T:{0:F3}", (RotT * 180 / Math.PI));
                //PT_Display01.InteractiveGraphics.Add(testTheta, "", false);
                //shkang_e임시
                Res = true;
            }
            catch
            {
                Res = false;
                return Res;
            }
            //if (Film_Res == false)
            //    Res = Film_Res;
            return Res;
        }
        private void TrackingROICalculate(out double TrackingX, out double TrackingY, out double RotT)
        {
            double dx, dy, dTeachT, dRotT;
            dx = ((dCrossX[1] + dCrossX[0]) / 2) - ((RightOrigin[0] + LeftOrigin[0]) / 2);
            dy = ((dCrossY[1] + dCrossY[0]) / 2) - ((RightOrigin[1] + LeftOrigin[1]) / 2);
            double[] pntCenter = new double[2] { 0, 0 };
            double dRotDx = dCrossX[1] - dCrossX[0];
            double dRotDy = dCrossY[1] - dCrossY[0];
            dRotT = Math.Atan2(dRotDy, dRotDx);
            if (dRotT > 180.0) dRotT -= 360.0;

            dTeachT = Math.Atan2(RightOrigin[1] - LeftOrigin[1], RightOrigin[0] - LeftOrigin[0]);
            if (dTeachT > 180.0) dTeachT -= 360.0;

            dRotT -= dTeachT;
            RotT = dRotT;
            pntCenter[0] = (dCrossX[1] + dCrossX[0]) / 2;
            pntCenter[1] = (dCrossY[1] + dCrossY[0]) / 2;
            double[] dTaget = new double[2];
            dTaget[0] = (dCrossX[1] + dCrossX[0]) / 2;
            dTaget[1] = (dCrossY[1] + dCrossY[0]) / 2;
            // RotationTransform(pntCenter, dx, dy, RotT, ref dTaget);
            TrackingX = dTaget[0];
            TrackingY = dTaget[1];
            //TrackingX = dCrossX[0];
            //TrackingY = dCrossY[0];
        }
        private void RotationTransform(double[] apntCenter, double apntOffsetX, double apntOffsetY, double adAngle, ref double[] apntTarget)
        {

            double[] pntTempPos = apntTarget;

            pntTempPos[0] = pntTempPos[0] + apntOffsetX;
            pntTempPos[1] = pntTempPos[1] + apntOffsetY;
            //double Theta = (adAngle * Math.PI) / 180;
            apntTarget[0] = apntCenter[0] + ((Math.Cos(adAngle) * (pntTempPos[0] - apntCenter[0]) - (Math.Sin(adAngle) * (pntTempPos[1] - apntCenter[1]))));
            apntTarget[1] = apntCenter[1] + ((Math.Sin(adAngle) * (pntTempPos[0] - apntCenter[0]) + (Math.Cos(adAngle) * (pntTempPos[1] - apntCenter[1]))));
        }
        private void LineTrackingProperty(CogFindLineTool obj)
        {
            // Draw Origin
            //TrackLineROI(obj);

            // Property Manager
            TrackingElement trackingElement = new TrackingElement();
            TrackingElement.LineTrackingElement trackingLine = new TrackingElement.LineTrackingElement();
            trackingLine.TrackingLine = obj;

            trackingLine.TrackingLine.RunParams = obj.RunParams;

            //trackingElement.TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            //trackingElement.TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
            Tracking.TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            Tracking.TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
            dInspPrevTranslationX = Tracking.TranslationX;
            dInspPrevTranslationY = Tracking.TranslationY;
            dInspPrevTranslationT = Tracking.TranslationT;
            trackingLine.StartX = obj.RunParams.ExpectedLineSegment.StartX - Tracking.TranslationX;
            trackingLine.StartY = obj.RunParams.ExpectedLineSegment.StartY - Tracking.TranslationY;
            trackingLine.EndX = obj.RunParams.ExpectedLineSegment.EndX - Tracking.TranslationX;
            trackingLine.EndY = obj.RunParams.ExpectedLineSegment.EndY - Tracking.TranslationY;
            trackingLine.CaliperSearchLength = obj.RunParams.CaliperSearchLength;
            trackingLine.CaliperProjectionLength = obj.RunParams.CaliperProjectionLength;
            trackingLine.CaliperSearchDirection = obj.RunParams.CaliperSearchDirection;
            trackingLine.NumberOfCalipers = obj.RunParams.NumCalipers;

            trackingLine.TrackingLine.RunParams.CaliperRunParams.ContrastThreshold = obj.RunParams.CaliperRunParams.ContrastThreshold;
            trackingLine.TrackingLine.RunParams.ExpectedLineSegment.StartX = trackingLine.StartX;
            trackingLine.TrackingLine.RunParams.ExpectedLineSegment.StartY = trackingLine.StartY;
            trackingLine.TrackingLine.RunParams.ExpectedLineSegment.EndX = trackingLine.EndX;
            trackingLine.TrackingLine.RunParams.ExpectedLineSegment.EndY = trackingLine.EndY;
            trackingLine.TrackingLine.RunParams.CaliperSearchLength = trackingLine.CaliperSearchLength;
            trackingLine.TrackingLine.RunParams.CaliperProjectionLength = trackingLine.CaliperProjectionLength;
            trackingLine.TrackingLine.RunParams.CaliperSearchDirection = trackingLine.CaliperSearchDirection;
            trackingLine.TrackingLine.RunParams.NumCalipers = trackingLine.NumberOfCalipers;
            trackingLine.TrackingLine.RunParams.CaliperRunParams.Edge0Polarity = obj.RunParams.CaliperRunParams.Edge0Polarity;
            trackingLine.TrackingLine.RunParams.CaliperRunParams.Edge1Polarity = obj.RunParams.CaliperRunParams.Edge1Polarity;

            // Draw Tracking
            trackingLine.TrackingLine.RunParams.ExpectedLineSegment.LineStyle = CogGraphicLineStyleConstants.Solid;
            trackingLine.TrackingLine.RunParams.ExpectedLineSegment.LineWidthInScreenPixels = 5;
            trackingLine.TrackingLine.RunParams.ExpectedLineSegment.Color = CogColorConstants.Red;

            // Copy Object
            //TrackingLine = trackingLine.Copy();
            TrackingLine = trackingLine.ShallowCopy(); //2022 11 12 copy() 사용시 제대로 카피안됨. ShallowCopy 사용시 정상적으로 카피는 되나 ROI트래킹쪽이 복잡하여 분석시간 추가필요
            TrackLineROI(TrackingLine.TrackingLine);
        }

        private void CircleTrackingProperty(CogFindCircleTool obj)
        {
            // Draw Origin
            //TrackCircleROI(obj);

            // Property Manager
            TrackingElement trackingElement = new TrackingElement();
            TrackingElement.CircleTrackingElement trackingCircle = new TrackingElement.CircleTrackingElement();
            trackingCircle.TrackingCircle = obj;
            trackingCircle.TrackingCircle.RunParams = obj.RunParams;

            Tracking.TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            Tracking.TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
            dInspPrevTranslationX = Tracking.TranslationX;
            dInspPrevTranslationY = Tracking.TranslationY;
            dInspPrevTranslationT = Tracking.TranslationT;
            trackingCircle.CenterX = obj.RunParams.ExpectedCircularArc.CenterX - Tracking.TranslationX;
            trackingCircle.CenterY = obj.RunParams.ExpectedCircularArc.CenterY - Tracking.TranslationY;
            trackingCircle.CaliperSearchLength = obj.RunParams.CaliperSearchLength;
            trackingCircle.CaliperProjectionLength = obj.RunParams.CaliperProjectionLength;
            trackingCircle.NumberOfCalipers = obj.RunParams.NumCalipers;
            trackingCircle.RadiusConstraint = obj.RunParams.RadiusConstraint;
            trackingCircle.AngleSpan = obj.RunParams.ExpectedCircularArc.AngleSpan;
            trackingCircle.AngleStart = obj.RunParams.ExpectedCircularArc.AngleStart;
            trackingCircle.Radius = obj.RunParams.ExpectedCircularArc.Radius;

            trackingCircle.TrackingCircle.RunParams.CaliperRunParams.ContrastThreshold = obj.RunParams.CaliperRunParams.ContrastThreshold;
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.CenterX = trackingCircle.CenterX;
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.CenterY = trackingCircle.CenterY;
            trackingCircle.TrackingCircle.RunParams.CaliperSearchLength = trackingCircle.CaliperSearchLength;
            trackingCircle.TrackingCircle.RunParams.CaliperProjectionLength = trackingCircle.CaliperProjectionLength;
            trackingCircle.TrackingCircle.RunParams.NumCalipers = trackingCircle.NumberOfCalipers;
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.AngleSpan = trackingCircle.AngleSpan;
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.AngleStart = trackingCircle.AngleStart;
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.Radius = trackingCircle.Radius;

            // Draw Tracking
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.LineStyle = CogGraphicLineStyleConstants.Solid;
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.LineWidthInScreenPixels = 5;
            trackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.Color = CogColorConstants.Red;

            // Copy Object
            //TrackingCircle = trackingCircle.Copy();
            TrackingCircle = trackingCircle.ShallowCopy();
            TrackCircleROI(TrackingCircle.TrackingCircle);
        }

        private void TrackLineROI(CogFindLineTool cogFindLineTool)
        {
            cogFindLineTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            cogFindLineTool.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage | CogFindLineCurrentRecordConstants.CaliperRegions | CogFindLineCurrentRecordConstants.ExpectedLineSegment |
                                                        CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;
            Display.SetInteractiveGraphics(PT_Display01, cogFindLineTool.CreateCurrentRecord(), false);

            if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
                PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void TrackCircleROI(CogFindCircleTool cogFindCircleTool)
        {
            CogCircularArc Arc = cogFindCircleTool.RunParams.ExpectedCircularArc;
            double centerx1 = Arc.CenterX;
            cogFindCircleTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            cogFindCircleTool.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.InputImage | CogFindCircleCurrentRecordConstants.CaliperRegions | CogFindCircleCurrentRecordConstants.ExpectedCircularArc |
                                                           CogFindCircleCurrentRecordConstants.InteractiveCaliperSize;
            Display.SetInteractiveGraphics(PT_Display01, cogFindCircleTool.CreateCurrentRecord(), false);
            if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
                PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void SetText()
        {
            CogCaliperPolarityConstants Polarity;
            int TmepIndex = 0;
            if (m_enumROIType == enumROIType.Line)
            {
                LAB_Insp_Threshold.Text = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = m_TempFindLineTool.RunParams.NumCalipers.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
                lblParamFilterSizeValue.Text = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
                m_TempFindLineTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
                TmepIndex = (int)Polarity;
                Combo_Polarity1.SelectedIndex = TmepIndex - 1;
                Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
                TmepIndex = (int)Polarity;
                Combo_Polarity2.SelectedIndex = TmepIndex - 1;
            }
            else
            {
                LAB_Insp_Threshold.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
                lblParamFilterSizeValue.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
                m_TempFindCircleTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
                TmepIndex = (int)Polarity;
                Combo_Polarity1.SelectedIndex = TmepIndex - 1;
                Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
                TmepIndex = (int)Polarity;
                Combo_Polarity2.SelectedIndex = TmepIndex - 1;
            }
            text_Dist_Ignre.Text = m_dDist_ignore.ToString();
            text_Spec_Dist.Text = m_SpecDist.ToString();
            text_Spec_Dist_Max.Text = m_SpecDistMax.ToString();

            lblEdgeThreshold.Text = m_TeachParameter[m_iGridIndex].iThreshold.ToString();
            chkUseEdgeThreshold.Checked = m_TeachParameter[m_iGridIndex].bThresholdUse;
            lblTopCutPixel.Text = m_TeachParameter[m_iGridIndex].iTopCutPixel.ToString();
            lblBottomCutPixel.Text = m_TeachParameter[m_iGridIndex].iBottomCutPixel.ToString();
            lblMaskingValue.Text = m_TeachParameter[m_iGridIndex].iMaskingValue.ToString();
            lblIgnoreSize.Text = m_TeachParameter[m_iGridIndex].iIgnoreSize.ToString();
            lblEdgeCaliperThreshold.Text = m_TeachParameter[m_iGridIndex].iEdgeCaliperThreshold.ToString();
            lblEdgeCaliperFilterSize.Text = m_TeachParameter[m_iGridIndex].iEdgeCaliperFilterSize.ToString();
        }
        private void FindLineROI()
        {

            m_TempFindLineTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            m_TempFindLineTool.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage | CogFindLineCurrentRecordConstants.CaliperRegions | CogFindLineCurrentRecordConstants.ExpectedLineSegment |
                                                        CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;
            Display.SetInteractiveGraphics(PT_Display01, m_TempFindLineTool.CreateCurrentRecord(), false);
            if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
                PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }
        private void DrawRoiLine()
        {
            //PT_Display01.Image = OriginImage;
            m_TempTrackingLine.InputImage = (CogImage8Grey)PT_Display01.Image;
            m_TempTrackingLine.CurrentRecordEnable = CogFindLineCurrentRecordConstants.InputImage | CogFindLineCurrentRecordConstants.CaliperRegions | CogFindLineCurrentRecordConstants.ExpectedLineSegment |
                                                       CogFindLineCurrentRecordConstants.InteractiveCaliperSearchDirection | CogFindLineCurrentRecordConstants.InteractiveCaliperSize;
            Display.SetInteractiveGraphics(PT_Display01, m_TempTrackingLine.CreateCurrentRecord(), false);
            if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
                PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }
        private void CircleROI()
        {

            m_TempFindCircleTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            m_TempFindCircleTool.CurrentRecordEnable = CogFindCircleCurrentRecordConstants.InputImage | CogFindCircleCurrentRecordConstants.CaliperRegions | CogFindCircleCurrentRecordConstants.ExpectedCircularArc |
                                                           CogFindCircleCurrentRecordConstants.InteractiveCaliperSize;
            Display.SetInteractiveGraphics(PT_Display01, m_TempFindCircleTool.CreateCurrentRecord(), false);
            if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
                PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void TrackingCaliperROI()
        {
            PT_Display01.StaticGraphics.Clear();
            PT_Display01.InteractiveGraphics.Clear();
            m_TempCaliperTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            CogRectangleAffine _cogRectAffine = new CogRectangleAffine();

            if (m_TempCaliperTool.Region == null)
            {
                _cogRectAffine.GraphicDOFEnable = CogRectangleAffineDOFConstants.Position | CogRectangleAffineDOFConstants.Size | CogRectangleAffineDOFConstants.Skew | CogRectangleAffineDOFConstants.Rotation;
                _cogRectAffine.Interactive = true;
                _cogRectAffine.SetCenterLengthsRotationSkew((PT_Display01.Image.Width / 2 - PT_Display01.PanX), (PT_Display01.Image.Height / 2 - PT_Display01.PanY), 500, 500, 0, 0);
                m_TempCaliperTool.Region = _cogRectAffine;
            }
            PT_Display01.InteractiveGraphics.Add(m_TempCaliperTool.Region, "Caliper", false);
            //m_TempTrackingCaliper.InputImage = (CogImage8Grey)PT_Display01.Image;
        }
        private void Caliper_Count(object sender, EventArgs e)
        {
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            Button btn = (Button)sender;
            int iCaliperCnt = Convert.ToInt32(LAB_Caliper_Cnt.Text);
            if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            {
                //Up
                iCaliperCnt++;
            }
            else
            {
                //Down
                if (iCaliperCnt == 1) return;
                iCaliperCnt--;
            }
            if (m_enumROIType == enumROIType.Line)
            {
                m_TempFindLineTool.RunParams.NumCalipers = iCaliperCnt;
                FindLineROI();
            }
            else
            {
                m_TempFindCircleTool.RunParams.NumCalipers = iCaliperCnt;
                CircleROI();
            }

            LAB_Caliper_Cnt.Text = iCaliperCnt.ToString();
        }
        private void Caliper_ProjectionLenth(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            double iProjectionLenth = Convert.ToDouble(LAB_CALIPER_PROJECTIONLENTH.Text);
            if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            {
                //Up
                iProjectionLenth++;
            }
            else
            {
                //Down
                if (iProjectionLenth == 1) return;
                iProjectionLenth--;
            }
            if (m_enumROIType == enumROIType.Line)
                m_TempFindLineTool.RunParams.CaliperProjectionLength = iProjectionLenth;
            else
                m_TempFindCircleTool.RunParams.CaliperProjectionLength = iProjectionLenth;

            LAB_CALIPER_PROJECTIONLENTH.Text = iProjectionLenth.ToString();
        }
        private void Insp_Threshold(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            double iThreshold = Convert.ToDouble(LAB_Insp_Threshold.Text);
            if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            {
                //Up
                iThreshold++;
            }
            else
            {
                //Down
                if (iThreshold < 0) return;
                iThreshold--;
            }

            if (m_enumROIType == enumROIType.Line)
                m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold = iThreshold;
            else
                m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold = iThreshold;
            LAB_Insp_Threshold.Text = iThreshold.ToString();

        }
        private void Insp_SearchLenth(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            double iSearchLenth = Convert.ToDouble(LAB_CALIPER_SEARCHLENTH.Text);
            if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            {
                //Up
                iSearchLenth++;
            }
            else
            {
                //Down
                if (iSearchLenth < 1) return;
                iSearchLenth--;
            }
            if (m_enumROIType == enumROIType.Line)
                m_TempFindLineTool.RunParams.CaliperSearchLength = iSearchLenth;
            else
                m_TempFindCircleTool.RunParams.CaliperSearchLength = iSearchLenth;
            LAB_CALIPER_SEARCHLENTH.Text = iSearchLenth.ToString();
        }

        private void Dist_Ignore(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iIngroe = Convert.ToInt32(text_Dist_Ignre.Text);
            if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            {
                //Up
                iIngroe++;
            }
            else
            {
                //Down
                if (iIngroe < 0) return;
                iIngroe--;
            }
            m_dDist_ignore = iIngroe;
            text_Dist_Ignre.Text = iIngroe.ToString();
        }
        Main.PatternTag.SDParameter ResetStruct()
        {
            Main.PatternTag.SDParameter RestData = new Main.PatternTag.SDParameter();
            RestData.m_FindLineTool = new CogFindLineTool();
            RestData.m_FindCircleTool = new CogFindCircleTool();
            RestData.m_enumROIType = new Main.PatternTag.SDParameter.enumROIType();
            RestData.m_CogBlobTool = new CogBlobTool[10];
            for (int i = 0; i < 10; i++)
            {
                RestData.m_CogBlobTool[i] = new CogBlobTool();
                RestData.m_CogBlobTool[i].RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
                RestData.m_CogBlobTool[i].RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
            }
            RestData.CenterX = 0;
            RestData.CenterY = 0;
            RestData.LenthX = 0;
            RestData.LenthY = 0;
            RestData.dSpecDistance = 0;
            RestData.IDistgnore = 0;
            return RestData;


        }

        private void BTN_INSP_ADD_Click(object sender, EventArgs e)
        {
            //m_TeachParameter.Clear();
            if (CHK_ROI_CREATE.Checked == false)
            {
                CHK_ROI_CREATE.Checked = true;
            }
            if (MessageBox.Show("Are you sure you want to ROI Copy it?", "ROI Copy", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                PT_Display01.InteractiveGraphics.Clear();
                PT_Display01.StaticGraphics.Clear();
                string[] strData = new string[19];
                int iNo = DataGridview_Insp.RowCount;
                if (iNo == 0)
                    iNo = 0;
                else
                    iNo -= 1;

                CogCaliperPolarityConstants Polarity;
                if (m_TeachParameter.Count < iNo)
                    m_TeachParameter.Add(ResetStruct());
                var TempData = m_TeachParameter[iNo];
                TempData.m_enumROIType = (Main.PatternTag.SDParameter.enumROIType)m_enumROIType;
                strData[0] = string.Format("{0:00}", (iNo + 1).ToString());

                if (m_enumROIType == enumROIType.Line)
                {
                    strData[1] = "Line";
                    strData[2] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX);
                    strData[3] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY);
                    strData[4] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX);
                    strData[5] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY);
                    strData[6] = string.Format("{0:F3}", 0);
                    strData[7] = string.Format("{0:F3}", 0);
                    strData[8] = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                    strData[9] = m_TempFindLineTool.RunParams.NumCalipers.ToString();
                    strData[10] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
                    strData[11] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
                    Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
                    strData[12] = ((int)Polarity).ToString();
                    Polarity = m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
                    strData[13] = ((int)Polarity).ToString();
                    strData[14] = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
                    strData[15] = m_dDist_ignore.ToString();
                    strData[16] = string.Format("{0:F2}", m_SpecDist);
                    //strData[17] = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels);
                    strData[17] = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
                    strData[18] = string.Format("{0:F2}", m_SpecDistMax);

                    TempData.m_FindLineTool = m_TempFindLineTool;
                }
                else
                {

                    strData[1] = "Circle";
                    strData[2] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX);
                    strData[3] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterY);
                    strData[4] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.Radius);
                    strData[5] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleStart);
                    strData[6] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan);
                    strData[7] = string.Format("{0:F3}", 0);
                    strData[8] = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                    strData[9] = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
                    strData[10] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
                    strData[11] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
                    Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
                    strData[12] = ((int)Polarity).ToString();
                    Polarity = m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
                    strData[13] = ((int)Polarity).ToString();
                    TempData.m_FindCircleTool = m_TempFindCircleTool;
                    strData[14] = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
                    strData[15] = m_dDist_ignore.ToString();
                    strData[16] = string.Format("{0:F2}", m_SpecDist);
                    //strData[17] = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels);
                    strData[17] = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
                    strData[18] = string.Format("{0:F2}", m_SpecDistMax);
                }
                DataGridview_Insp.Rows.Add(strData);
                m_TeachParameter.Add(TempData);
                CHK_ROI_CREATE.Checked = false;
            }
            else
            {
                CHK_ROI_CREATE.Checked = false;
                return;
            }
        }

        private void BTN_INSP_DELETE_Click(object sender, EventArgs e)
        {
            if (m_iGridIndex < 0) return;
            if(MessageBox.Show("Are you sure you want to delete it?","Delete",MessageBoxButtons.YesNo)==DialogResult.Yes)
            {
                DataGridview_Insp.Rows.RemoveAt(m_iGridIndex);
                m_TeachParameter.RemoveAt(m_iGridIndex);
            }
            else
            {
                return;
            }
        }
        private void initTracking()
        {
            m_enumAlignROI = enumAlignROI.Left1_1;
            btn_TOP_Inscription.BackColor = Color.Green;
            btn_Top_Circumcription.BackColor = Color.DarkGray;
            btn_Bottom_Inscription.BackColor = Color.DarkGray;
            btn_Bottom_Circumcription.BackColor = Color.DarkGray;
            for (int i = 0; i < 4; i++)
            {
                if (i < 2)
                {
                    LeftOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].LeftOrigin[i];
                    RightOrigin[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].RightOrigin[i];
                }
                m_TeachLine[i] = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i];
                if (m_TeachLine[i] == null)
                    m_TeachLine[i] = new CogFindLineTool();
                if (Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] == null)
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_TrackingLine[i] = new CogFindLineTool();
            }
            lab_LeftOriginX.Text = LeftOrigin[0].ToString("F3");
            lab_LeftOriginY.Text = LeftOrigin[1].ToString("F3");
            lab_RightOriginX.Text = RightOrigin[0].ToString("F3");
            lab_RightOriginY.Text = RightOrigin[1].ToString("F3");
            m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_1];
            Get_FindConerParameter();
        }
        public void Init_ListBox()
        {
            init_ComboPolarity();
            m_TeachParameter = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_InspParameter;

            CogCaliperPolarityConstants Polarity;
            DataGridview_Insp.Rows.Clear();

            DataGridview_Insp.ClearSelection();
            DataGridview_Insp.CurrentCell = null;
            initTracking();
            if (m_TeachParameter.Count() <= 0)
            {
                m_TeachParameter = new List<Main.PatternTag.SDParameter>();
                m_TeachParameter.Add(ResetStruct());
            }

            for (int i = 0; i < m_TeachParameter.Count; i++)
            {
                string[] strData = new string[21];
                var Tempdata = m_TeachParameter[i];
                bool bThre = Tempdata.bThresholdUse ? true : false;
    

                strData[0] = i.ToString();
                if (i == 0)
                {
                    m_iHistoramROICnt = Tempdata.iHistogramROICnt;
                    for (int iHitoCnt = 0; iHitoCnt < m_iHistoramROICnt; iHitoCnt++)
                    {
                        m_bTrakingRootHisto[iHitoCnt] = false;
                    }
                   
                    lab_Histogram_ROI_Count.Text = m_iHistoramROICnt.ToString();
                }
                if (enumROIType.Line == (enumROIType)Tempdata.m_enumROIType)
                {
                    strData[1] = "Line";
                    strData[2] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartX);
                    strData[3] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.StartY);
                    strData[4] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndX);
                    strData[5] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.ExpectedLineSegment.EndY);
                    strData[6] = string.Format("{0:F3}", 0);
                    strData[7] = string.Format("{0:F3}", 0);
                    strData[8] = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                    strData[9] = Tempdata.m_FindLineTool.RunParams.NumCalipers.ToString();
                    strData[10] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.CaliperProjectionLength);
                    strData[11] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.CaliperSearchLength);
                    Polarity = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.Edge0Polarity;
                    strData[12] = ((int)Polarity).ToString();
                    Polarity = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.Edge1Polarity;
                    strData[13] = ((int)Polarity).ToString();
                    strData[14] = string.Format("{0:F3}", Tempdata.m_FindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
                    strData[15] = Tempdata.IDistgnore.ToString();
                    strData[16] = string.Format("{0:F2}", Tempdata.dSpecDistance);
                    strData[17] = Tempdata.m_FindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
                    strData[18] = string.Format("{0:F2}", Tempdata.dSpecDistanceMax);
                    strData[19] = bThre.ToString();
                    strData[20] = Tempdata.iThreshold.ToString();

                }
                else
                {
                    strData[1] = "Circle";
                    strData[2] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterX);
                    strData[3] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.CenterY);
                    strData[4] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.Radius);
                    strData[5] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.AngleStart);
                    strData[6] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.ExpectedCircularArc.AngleSpan);
                    strData[7] = string.Format("{0:F3}", 0);
                    strData[8] = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                    strData[9] = Tempdata.m_FindCircleTool.RunParams.NumCalipers.ToString();
                    strData[10] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.CaliperProjectionLength);
                    strData[11] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.CaliperSearchLength);
                    Polarity = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.Edge0Polarity;
                    strData[12] = ((int)Polarity).ToString();
                    Polarity = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.Edge1Polarity;
                    strData[13] = ((int)Polarity).ToString();
                    strData[14] = string.Format("{0:F3}", Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
                    strData[15] = Tempdata.IDistgnore.ToString();
                    strData[16] = string.Format("{0:F2}", Tempdata.dSpecDistance);
                    strData[17] = Tempdata.m_FindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
                    strData[18] = string.Format("{0:F2}", Tempdata.dSpecDistanceMax);
                    strData[19] = bThre.ToString();
                    strData[20] = Tempdata.iThreshold.ToString();
                }            
                DataGridview_Insp.Rows.Add(strData); 

            }
            SetText();
        }
        private void UpDataTool()
        {
            double dEdgeWidth;
            //shkang_s
            string strTemp;
            int itype;
            strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
            if (strTemp == "Line")
                itype = 0;
            else
                itype = 1;
            m_enumROIType = (enumROIType)itype;
            //shkang_e
            if (m_enumROIType == enumROIType.Line)
            {
                ////강성현 주석/////////////////// m_FL 관련 주석해  
                CogFindLineTool m_FL = new CogFindLineTool();
                m_FL.RunParams.ExpectedLineSegment.StartX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
                m_FL.RunParams.ExpectedLineSegment.StartY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
                m_FL.RunParams.ExpectedLineSegment.EndX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
                m_FL.RunParams.ExpectedLineSegment.EndY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
                m_FL.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
                m_FL.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
                m_FL.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
                m_FL.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
                m_FL.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
                m_FL.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
                m_FL.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
                m_FL.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
                /////////////////////////////////////////////////////////////
                m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
                m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
                m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
                dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
                ////강성현 주석///////////////////
                //m_FL.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

                LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
                LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
                LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();
                lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

                LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));
                m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
                m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

                m_TempFindLineTool = new CogFindLineTool();
                if (bROICopy)
                    m_TempFindLineTool = m_FL;
                else
                    m_TempFindLineTool = m_TeachParameter[m_iGridIndex].m_FindLineTool;
            }
            else
            {
                ////강성현 주석///////////////////
                CogFindCircleTool m_FC = new CogFindCircleTool();
                m_FC.RunParams.ExpectedCircularArc.CenterX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
                m_FC.RunParams.ExpectedCircularArc.CenterY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
                m_FC.RunParams.ExpectedCircularArc.Radius = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
                m_FC.RunParams.ExpectedCircularArc.AngleStart = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
                m_FC.RunParams.ExpectedCircularArc.AngleSpan = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value);
                m_FC.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
                m_FC.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
                m_FC.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
                m_FC.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
                m_FC.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
                m_FC.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
                m_FC.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
                m_FC.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
                ///////////////////////////////////////////////
                m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
                m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
                m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
                ///강성현 주석/////////////////////////////////////////
                m_FC.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);
                //////////////////////////////
                LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
                LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
                LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();

                dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
                lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);
                LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));
                m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
                m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

                m_TempFindCircleTool = new CogFindCircleTool();
                if (bROICopy)
                    m_TempFindCircleTool = m_FC;
                else
                    m_TempFindCircleTool = m_TeachParameter[m_iGridIndex].m_FindCircleTool;
            }
        }
        private void DataGridview_Insp_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            m_iGridIndex = e.RowIndex;
            int itype;
            string strTemp = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value);
            if (strTemp == "Line")
                itype = 0;
            else
                itype = 1;

            m_enumROIType = (enumROIType)itype;
            double dEdgeWidth;

            if (m_enumROIType == enumROIType.Line)
            {
                CogFindLineTool m_FL = new CogFindLineTool();
                m_FL.RunParams.ExpectedLineSegment.StartX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
                m_FL.RunParams.ExpectedLineSegment.StartY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
                m_FL.RunParams.ExpectedLineSegment.EndX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
                m_FL.RunParams.ExpectedLineSegment.EndY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
                m_FL.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
                m_FL.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
                m_FL.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
                m_FL.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
                m_FL.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
                m_FL.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
                m_FL.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
                m_FL.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
                m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
                m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
                m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
                dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
                //m_FL.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

                LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
                LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
                LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();
                lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);
                LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));

                m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
                m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

                m_TempFindLineTool = new CogFindLineTool();

                if (bROICopy)
                    m_TempFindLineTool = m_FL;
                else
                    m_TempFindLineTool = m_TeachParameter[m_iGridIndex].m_FindLineTool;

                // Line은 EdgeWidth, Polarity2 미사용
                label59.Visible = false;
                LAB_EDGE_WIDTH.Visible = false;
                lblParamEdgeWidthValueUp.Visible = false;
                lblParamEdgeWidthValueDown.Visible = false;
                label58.Visible = false;
                Combo_Polarity2.Visible = false;
            }
            else
            {
                CogFindCircleTool m_FC = new CogFindCircleTool();
                m_FC.RunParams.ExpectedCircularArc.CenterX = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value);
                m_FC.RunParams.ExpectedCircularArc.CenterY = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value);
                m_FC.RunParams.ExpectedCircularArc.Radius = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value);
                m_FC.RunParams.ExpectedCircularArc.AngleStart = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value);
                m_FC.RunParams.ExpectedCircularArc.AngleSpan = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value);
                m_FC.RunParams.CaliperRunParams.ContrastThreshold = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value);
                m_FC.RunParams.NumCalipers = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value);
                m_FC.RunParams.CaliperProjectionLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value);
                m_FC.RunParams.CaliperSearchLength = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value);
                m_FC.RunParams.CaliperRunParams.Edge0Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value);
                m_FC.RunParams.CaliperRunParams.Edge1Polarity = (CogCaliperPolarityConstants)Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value);
                m_FC.RunParams.CaliperRunParams.Edge0Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2) * -1;
                m_FC.RunParams.CaliperRunParams.Edge1Position = (Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value) / 2);
                m_dDist_ignore = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value);
                m_SpecDist = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value);
                m_SpecDistMax = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value);
                m_FC.RunParams.CaliperRunParams.FilterHalfSizeInPixels = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

                LAB_Insp_Threshold.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value).ToString();
                LAB_Caliper_Cnt.Text = Convert.ToInt32(DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value).ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value).ToString();
                LAB_CALIPER_SEARCHLENTH.Text = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value).ToString();
                lblParamFilterSizeValue.Text = Convert.ToString(DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value);

                dEdgeWidth = Convert.ToDouble(DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value);
                LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", Math.Abs(dEdgeWidth));

                m_TeachParameter[m_iGridIndex].bThresholdUse = Convert.ToBoolean(DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value);
                m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value);

                m_TempFindCircleTool = new CogFindCircleTool();

                if (bROICopy)
                    m_TempFindCircleTool = m_FC;
                else
                    m_TempFindCircleTool = m_TeachParameter[m_iGridIndex].m_FindCircleTool;

                // Circle은 EdgeWidth, Polarity2 미사용
                label59.Visible = true;
                LAB_EDGE_WIDTH.Visible = true;
                lblParamEdgeWidthValueUp.Visible = true;
                lblParamEdgeWidthValueDown.Visible = true;
                label58.Visible = true;
                Combo_Polarity2.Visible = true;
            }
            SetText();
            UpdateParamUI();

            btn_ROI_SHOW.PerformClick();
            //Set_InspParams();
        }


        private void Set_InspParams()
        {
            if (m_enumROIType == enumROIType.Line)
            {
                m_TempFindLineTool = new CogFindLineTool();
                if (m_TempFindLineTool == null) return;
                LAB_Insp_Threshold.Text = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = m_TempFindLineTool.RunParams.NumCalipers.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
                LAB_EDGE_WIDTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position * 2);
                lblParamFilterSizeValue.Text = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            }
            else
            {
                m_TempFindCircleTool = new CogFindCircleTool();
                if (m_TempFindCircleTool == null) return;
                LAB_Insp_Threshold.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
                LAB_EDGE_WIDTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position * 2);
                lblParamFilterSizeValue.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            }
        }

        private void Section(CogImage8Grey SectionImage)
        {
            if (SectionImage == null) return;
            List_CenterX.Clear();
            List_CenterY.Clear();
            List_LenthX.Clear();
            List_LenthY.Clear();
            double dWidth = SectionImage.Width;
            double height = SectionImage.Height;
            double dWidthLenth = dWidth / 3;
            double dHeightLenth = height / 2;
            double Centerx = dWidthLenth / 2;
            double CenterY = dHeightLenth / 2;

            m_SectionImage = new CogImage8Grey[6];
            for (int iSection = 0; iSection < 6; iSection++)
            {
                m_SectionImage[iSection] = new CogImage8Grey();

                var Temp = m_TeachParameter[0];
                switch (iSection)
                {
                    case 0:

                        List_CenterX.Add(Centerx);
                        List_CenterY.Add(CenterY);
                        List_LenthX.Add(dWidthLenth);
                        List_LenthY.Add(dHeightLenth);
                        Temp.CenterX = List_CenterX[iSection];
                        Temp.CenterY = List_CenterY[iSection];
                        Temp.LenthX = List_LenthX[iSection];
                        Temp.LenthY = List_LenthY[iSection];
                        break;
                    case 1:
                        List_CenterX.Add(Centerx * 3);
                        List_CenterY.Add(CenterY);
                        List_LenthX.Add(dWidthLenth);
                        List_LenthY.Add(dHeightLenth);
                        Temp.CenterX = List_CenterX[iSection];
                        Temp.CenterY = List_CenterY[iSection];
                        Temp.LenthX = List_LenthX[iSection];
                        Temp.LenthY = List_LenthY[iSection];
                        break;
                    case 2:
                        List_CenterX.Add(Centerx * 5);
                        List_CenterY.Add(CenterY);
                        List_LenthX.Add(dWidthLenth);
                        List_LenthY.Add(dHeightLenth);
                        Temp.CenterX = List_CenterX[iSection];
                        Temp.CenterY = List_CenterY[iSection];
                        Temp.LenthX = List_LenthX[iSection];
                        Temp.LenthY = List_LenthY[iSection];
                        break;
                    case 3:
                        List_CenterX.Add(Centerx);
                        List_CenterY.Add(CenterY * 3);
                        List_LenthX.Add(dWidthLenth);
                        List_LenthY.Add(dHeightLenth);
                        Temp.CenterX = List_CenterX[iSection];
                        Temp.CenterY = List_CenterY[iSection];
                        Temp.LenthX = List_LenthX[iSection];
                        Temp.LenthY = List_LenthY[iSection];
                        break;
                    case 4:
                        List_CenterX.Add(Centerx * 3);
                        List_CenterY.Add(CenterY * 3);
                        List_LenthX.Add(dWidthLenth);
                        List_LenthY.Add(dHeightLenth);
                        Temp.CenterX = List_CenterX[iSection];
                        Temp.CenterY = List_CenterY[iSection];
                        Temp.LenthX = List_LenthX[iSection];
                        Temp.LenthY = List_LenthY[iSection];
                        break;
                    case 5:
                        List_CenterX.Add(Centerx * 5);
                        List_CenterY.Add(CenterY * 3);
                        List_LenthX.Add(dWidthLenth);
                        List_LenthY.Add(dHeightLenth);
                        Temp.CenterX = List_CenterX[iSection];
                        Temp.CenterY = List_CenterY[iSection];
                        Temp.LenthX = List_LenthX[iSection];
                        Temp.LenthY = List_LenthY[iSection];
                        break;
                }
                if (List_CenterX[iSection] <= 0) return;
                m_TeachParameter[0] = Temp;
                CogAffineTransformTool _AffineTransform = new CogAffineTransformTool();
                _AffineTransform.InputImage = SectionImage;
                _AffineTransform.Region.CenterX = List_CenterX[iSection];
                _AffineTransform.Region.CenterY = List_CenterY[iSection];
                _AffineTransform.Region.SideXLength = List_LenthX[iSection];
                _AffineTransform.Region.SideYLength = List_LenthY[iSection];
                _AffineTransform.Run();
                m_SectionImage[iSection] = (CogImage8Grey)_AffineTransform.OutputImage;
            }
        }

        private void Comb_Section_SelectedIndexChanged(object sender, EventArgs e)
        {
            _PrePointX = null;
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            var TempBlob = m_TeachParameter[0];
            m_CogBlobTool[m_BlobROI] = TempBlob.m_CogBlobTool[m_BlobROI];
            if (_useROITracking)
            {
                if (m_CogBlobTool[m_BlobROI].Region != null)
                {
                    CogPolygon PolygonROI = (CogPolygon)m_CogBlobTool[m_BlobROI].Region;
                    if (dBlobPrevTranslationX > 0 && (m_PrevROINo == m_BlobROI))
                    {
                        m_bTrakingRoot[m_BlobROI] = false;
                        int numVertice = PolygonROI.NumVertices;
                        for (int i = 0; i < numVertice; i++)
                        {
                            double dx2 = PolygonROI.GetVertexX(i);
                            double dy2 = PolygonROI.GetVertexY(i);
                            dx2 += (dBlobPrevTranslationX);
                            dy2 += (dBlobPrevTranslationY);
                            PolygonROI.SetVertex(i, dx2, dy2);
                        }
                        m_CogBlobTool[m_BlobROI].Region = PolygonROI;
                    }
                    else if (m_bTrakingRoot[m_BlobROI] == true && m_bTrakingRoot[m_BlobROI] == true)
                    {
                        int numVertice = PolygonROI.NumVertices;
                        for (int i = 0; i < numVertice; i++)
                        {
                            double dx2 = PolygonROI.GetVertexX(i);
                            double dy2 = PolygonROI.GetVertexY(i);
                            dx2 += (dBlobPrevTranslationX);
                            dy2 += (dBlobPrevTranslationY);
                            PolygonROI.SetVertex(i, dx2, dy2);
                        }
                        m_CogBlobTool[m_BlobROI].Region = PolygonROI;
                        m_bTrakingRoot[m_BlobROI] = false;
                    }
                    m_PrevROINo = m_BlobROI;

                    double dx = PolygonROI.GetVertexX(0);
                    m_CogBlobTool[m_BlobROI].RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
                    if (m_CogBlobTool[m_BlobROI] == null)
                    {
                        m_CogBlobTool[m_BlobROI] = new CogBlobTool();
                        m_CogBlobTool[m_BlobROI].RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
                        m_CogBlobTool[m_BlobROI].RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
                    }
                }
                else
                {
                    _useROITracking = false;
                    chkUseRoiTracking.Checked = false;
                }
            }
            Get_BlobParameter();
        }

        private void Display_MauseUP(object sender, EventArgs e)
        {
            if (PT_Display01.InteractiveGraphics == null) return;
            if (m_enumROIType == enumROIType.Line)
            {
                if (m_TempFindLineTool == null) return;
                LAB_Insp_Threshold.Text = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = m_TempFindLineTool.RunParams.NumCalipers.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
                lblParamFilterSizeValue.Text = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            }
            else
            {
                if (m_TempFindCircleTool == null) return;
                LAB_Insp_Threshold.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold.ToString();
                LAB_Caliper_Cnt.Text = m_TempFindCircleTool.RunParams.NumCalipers.ToString();
                LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
                LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
                lblParamFilterSizeValue.Text = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
            }

        }

        private void ApplyTrackingData(enumROIType roiType)
        {
            double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);

            switch (roiType)
            {
                case enumROIType.Line:
                    TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.StartX += dInspPrevTranslationX;
                    TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.StartY += dInspPrevTranslationY;
                    TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.EndX += dInspPrevTranslationX;
                    TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.EndY += dInspPrevTranslationY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value = "Line";
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value = TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.StartX;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value = TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.StartY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value = TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.EndX;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value = TrackingLine.TrackingLine.RunParams.ExpectedLineSegment.EndY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value = 0;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[7].Value = 0;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value = TrackingLine.TrackingLine.RunParams.CaliperRunParams.ContrastThreshold;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value = TrackingLine.TrackingLine.RunParams.NumCalipers;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value = string.Format("{0:F3}", TrackingLine.TrackingLine.RunParams.CaliperProjectionLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value = string.Format("{0:F3}", TrackingLine.TrackingLine.RunParams.CaliperSearchLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value = Convert.ToInt32(TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge0Polarity);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value = Convert.ToInt32(TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge1Polarity);
                    if (TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge0Position == 0)
                    {
                        TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                        TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                    }
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value = string.Format("{0:F2}", TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge0Position * 2);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value = m_dDist_ignore.ToString();
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = TrackingLine.TrackingLine.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
                    // m_TempFindLineTool = TrackingLine.TrackingLine;
                    //m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX -= Tracking.TranslationX;
                    //m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY -= Tracking.TranslationY;
                    //m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX -= Tracking.TranslationX;
                    //m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY -= Tracking.TranslationY;
                    break;
                case enumROIType.Circle:
                    TrackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.CenterX += dInspPrevTranslationX;
                    TrackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.CenterY += dInspPrevTranslationY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value = "Circle";
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value = TrackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.CenterX;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value = TrackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.CenterY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value = TrackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.Radius;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value = TrackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.AngleStart;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value = TrackingCircle.TrackingCircle.RunParams.ExpectedCircularArc.AngleSpan;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[7].Value = 0;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value = TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.ContrastThreshold;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value = TrackingCircle.TrackingCircle.RunParams.NumCalipers;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value = string.Format("{0:F3}", TrackingCircle.TrackingCircle.RunParams.CaliperProjectionLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value = string.Format("{0:F3}", TrackingCircle.TrackingCircle.RunParams.CaliperSearchLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value = Convert.ToInt32(TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.Edge0Polarity);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value = Convert.ToInt32(TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.Edge1Polarity);
                    if (m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position == 0)
                    {
                        TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                        TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.Edge0Position = (dEdgeWidth / 2);
                    }
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value = string.Format("{0:F2}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value = m_dDist_ignore.ToString();
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
                    //m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX -= Tracking.TranslationX;
                    //m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX -= Tracking.TranslationY;
                    break;
                default:
                    break;
            }
        }

        private void btn_Param_Apply_Click(object sender, EventArgs e)
        {

            if (m_iGridIndex < 0) return;
            string strTemp = "";
            iCountClick += 1;
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            m_iCount = DataGridview_Insp.Rows.Count;
            if (Chk_All_Select.Checked == false)
            {
                var TempData = m_TeachParameter[m_iGridIndex];
                double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
                TempData.m_enumROIType = (Main.PatternTag.SDParameter.enumROIType)m_enumROIType;
                if (m_enumROIType == enumROIType.Line)
                {
                    strTemp = "Line";
                  
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value = strTemp;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value = 0;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[7].Value = 0;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value = m_TempFindLineTool.RunParams.NumCalipers;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity);
                    if (m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position == 0)
                    {

                        m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                        m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                    }
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value = m_dDist_ignore.ToString();
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);

                    TempData.IDistgnore = m_dDist_ignore;
                    TempData.dSpecDistance = m_SpecDist;
                    TempData.dSpecDistanceMax = m_SpecDistMax;
                    TempData.m_FindLineTool = new CogFindLineTool();
                    TempData.m_FindLineTool = m_TempFindLineTool;
                    //}
                }
                else
                {
                    strTemp = "Circle";

                    DataGridview_Insp.Rows[m_iGridIndex].Cells[1].Value = strTemp;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[2].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[3].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterY;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[4].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.Radius;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[5].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleStart;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[6].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[7].Value = 0;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[8].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[9].Value = m_TempFindCircleTool.RunParams.NumCalipers;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[10].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[11].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[12].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[13].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity);
                    if (m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position == 0)
                    {
                        m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                        m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                    }
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[14].Value = string.Format("{0:F2}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[15].Value = m_dDist_ignore.ToString();
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
                    DataGridview_Insp.Rows[m_iGridIndex].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);
                    TempData.IDistgnore = m_dDist_ignore;
                    TempData.dSpecDistance = m_SpecDist;
                    TempData.dSpecDistanceMax = m_SpecDistMax;
                    TempData.m_FindCircleTool = m_TempFindCircleTool;
                    //}
                }

                m_TeachParameter[m_iGridIndex].bThresholdUse = chkUseEdgeThreshold.Checked;
                m_TeachParameter[m_iGridIndex].iThreshold = Convert.ToInt16(lblEdgeThreshold.Text);
                m_TeachParameter[m_iGridIndex].iEdgeCaliperThreshold = Convert.ToInt16(lblEdgeCaliperThreshold.Text);
                m_TeachParameter[m_iGridIndex].iEdgeCaliperFilterSize = Convert.ToInt16(lblEdgeCaliperFilterSize.Text);
                m_TeachParameter[m_iGridIndex].iTopCutPixel = Convert.ToInt16(lblTopCutPixel.Text);
                m_TeachParameter[m_iGridIndex].iBottomCutPixel = Convert.ToInt16(lblBottomCutPixel.Text);
                m_TeachParameter[m_iGridIndex].iMaskingValue = Convert.ToInt16(lblMaskingValue.Text);
                m_TeachParameter[m_iGridIndex].iIgnoreSize = Convert.ToInt16(lblIgnoreSize.Text);


                DataGridview_Insp.Rows[m_iGridIndex].Cells[19].Value = m_TeachParameter[m_iGridIndex].bThresholdUse;
                DataGridview_Insp.Rows[m_iGridIndex].Cells[20].Value = m_TeachParameter[m_iGridIndex].iThreshold;

                m_TeachParameter[m_iGridIndex] = TempData;
                dInspPrevTranslationX = 0;
                dInspPrevTranslationY = 0;
            }
            else
            {
                chkUseRoiTracking.Checked = false;
                Thread.Sleep(100);
                for (int i = 0; i < m_TeachParameter.Count; i++)
                {
                    var TempData = m_TeachParameter[i];
                    double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
                    double dThreshold = Convert.ToDouble(LAB_Insp_Threshold.Text);
                    if ((enumROIType)TempData.m_enumROIType == enumROIType.Line)
                    {

                        m_TempFindLineTool = TempData.m_FindLineTool;
                        strTemp = "Line";
                        if (_useROITracking)
                        {
                        
                        }
                        else
                        {
                            DataGridview_Insp.Rows[i].Cells[1].Value = strTemp;
                            DataGridview_Insp.Rows[i].Cells[2].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartX;
                            DataGridview_Insp.Rows[i].Cells[3].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.StartY;
                            DataGridview_Insp.Rows[i].Cells[4].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndX;
                            DataGridview_Insp.Rows[i].Cells[5].Value = m_TempFindLineTool.RunParams.ExpectedLineSegment.EndY;
                            DataGridview_Insp.Rows[i].Cells[6].Value = 0;
                            DataGridview_Insp.Rows[i].Cells[7].Value = 0;
                            DataGridview_Insp.Rows[i].Cells[8].Value = m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold;
                            DataGridview_Insp.Rows[i].Cells[9].Value = m_TempFindLineTool.RunParams.NumCalipers;
                            DataGridview_Insp.Rows[i].Cells[10].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperProjectionLength);
                            DataGridview_Insp.Rows[i].Cells[11].Value = string.Format("{0:F3}", m_TempFindLineTool.RunParams.CaliperSearchLength);
                            DataGridview_Insp.Rows[i].Cells[12].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity);
                            DataGridview_Insp.Rows[i].Cells[13].Value = Convert.ToInt32(m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity);

                            m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                            m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                            //}
                            DataGridview_Insp.Rows[i].Cells[14].Value = string.Format("{0:F2}", m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position * 2);
                            //DataGridview_Insp.Rows[i].Cells[15].Value = m_dDist_ignore.ToString();
                            DataGridview_Insp.Rows[i].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
                            DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
                            DataGridview_Insp.Rows[i].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);
                            // TempData.IDistgnore = m_dDist_ignore;
                            TempData.dSpecDistance = m_SpecDist;
                            TempData.dSpecDistanceMax = m_SpecDistMax;
                            TempData.m_FindLineTool = new CogFindLineTool();
                            TempData.m_FindLineTool = m_TempFindLineTool;
                        }
                    }
                    else
                    {
                        m_TempFindCircleTool = TempData.m_FindCircleTool;
                        strTemp = "Circle";

                        if (_useROITracking)
                        {
                           
                        }
                        else
                        {
                            DataGridview_Insp.Rows[i].Cells[1].Value = strTemp;
                            DataGridview_Insp.Rows[i].Cells[2].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterX;
                            DataGridview_Insp.Rows[i].Cells[3].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.CenterY;
                            DataGridview_Insp.Rows[i].Cells[4].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.Radius;
                            DataGridview_Insp.Rows[i].Cells[5].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleStart;
                            DataGridview_Insp.Rows[i].Cells[6].Value = m_TempFindCircleTool.RunParams.ExpectedCircularArc.AngleSpan;
                            DataGridview_Insp.Rows[i].Cells[7].Value = 0;
                            DataGridview_Insp.Rows[i].Cells[8].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold;
                            DataGridview_Insp.Rows[i].Cells[9].Value = m_TempFindCircleTool.RunParams.NumCalipers;
                            DataGridview_Insp.Rows[i].Cells[10].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperProjectionLength);
                            DataGridview_Insp.Rows[i].Cells[11].Value = string.Format("{0:F3}", m_TempFindCircleTool.RunParams.CaliperSearchLength);
                            DataGridview_Insp.Rows[i].Cells[12].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity);
                            DataGridview_Insp.Rows[i].Cells[13].Value = Convert.ToInt32(m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity);

                            m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                            m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
                            DataGridview_Insp.Rows[i].Cells[14].Value = string.Format("{0:F2}", m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position * 2);
                            DataGridview_Insp.Rows[i].Cells[16].Value = string.Format("{0:F2}", m_SpecDist);
                            DataGridview_Insp.Rows[m_iGridIndex].Cells[17].Value = m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels;
                            DataGridview_Insp.Rows[i].Cells[18].Value = string.Format("{0:F2}", m_SpecDistMax);
                            TempData.dSpecDistance = m_SpecDist;
                            TempData.dSpecDistanceMax = m_SpecDistMax;
                            TempData.m_FindCircleTool = m_TempFindCircleTool;
                        }
                    }
                    m_TeachParameter[i] = TempData;
                }
            }
            //shkang_s
            if (iCountClick == 1)
            {
                tempCaliperNum.Add(m_iGridIndex);
            }
            else
            {
                if (tempCaliperNum[iCountClick - 2] == m_iGridIndex)
                {
                    iCountClick = iCountClick - 1;
                }
                else
                {
                    tempCaliperNum.Add(m_iGridIndex);
                }
            }
            //shkang_e
        }


        private void Comb_Section_Click(object sender, EventArgs e)
        {
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
        }

        private void btn_TrimOrigin_Click(object sender, EventArgs e)
        {
            PT_Display01.Image = OriginImage;
        }

        private void LAB_Insp_Threshold_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Insp_Threshold.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double dThreshold = KeyPad.m_data;
            if (enumROIType.Line == m_enumROIType)
            {
                m_TempFindLineTool.RunParams.CaliperRunParams.ContrastThreshold = dThreshold;
            }
            else
            {
                m_TempFindCircleTool.RunParams.CaliperRunParams.ContrastThreshold = dThreshold;
            }
            LAB_Insp_Threshold.Text = ((int)dThreshold).ToString();
        }

        private void LAB_Caliper_Cnt_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Caliper_Cnt.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(2, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int CaliperCnt = (int)KeyPad.m_data;
            if (enumROIType.Line == m_enumROIType)
            {
                m_TempFindLineTool.RunParams.NumCalipers = CaliperCnt;
                FindLineROI();
            }
            else
            {
                m_TempFindCircleTool.RunParams.NumCalipers = CaliperCnt;
                CircleROI();
            }
            LAB_Caliper_Cnt.Text = CaliperCnt.ToString();
        }

        private void LAB_CALIPER_PROJECTIONLENTH_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_CALIPER_PROJECTIONLENTH.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(1, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double CaliperProjectionLenth = KeyPad.m_data;
            if (enumROIType.Line == m_enumROIType)
            {
                m_TempFindLineTool.RunParams.CaliperSearchLength = CaliperProjectionLenth;
            }
            else
            {
                m_TempFindCircleTool.RunParams.CaliperSearchLength = CaliperProjectionLenth;
            }
            LAB_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", CaliperProjectionLenth);
        }
        private void LAB_EDGE_WIDTH_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(1, 100, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double dEdgeWidth = KeyPad.m_data;

            if (enumROIType.Line == m_enumROIType)
            {
                    m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                    m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            }
            else
            {
                    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            }

            LAB_EDGE_WIDTH.Text = string.Format("{0:F2}", dEdgeWidth);
        }

        private bool GaloOppositeInspection(int nROI, int toolType, object tool, CogImage8Grey cogImage, out double[] ResultData, ref CogGraphicInteractiveCollection GraphicData, out int NonCaliperCnt)
        {
            NonCaliperCnt = 0;
            if (toolType == (int)enumROIType.Line)
            {
                bool MoveTypeY = false;
                //2023 0130 YSH
                bool bRes = true;
                CogFindLineTool m_LineTool = tool as CogFindLineTool;

                int[] nCaliperCount = new int[2];
                CogFindLineTool[] SingleFindLine = new CogFindLineTool[2];
                PointF[,] RawSearchData = new PointF[2, 100];
                ResultData = new double[100];

                CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();
                double startPosX = m_LineTool.RunParams.ExpectedLineSegment.StartX;
                double startPosY = m_LineTool.RunParams.ExpectedLineSegment.StartY;
                double EndPosX = m_LineTool.RunParams.ExpectedLineSegment.EndX;
                double EndPosY = m_LineTool.RunParams.ExpectedLineSegment.EndY;
                double MovePos1, MovePos2;
                double Move = m_LineTool.RunParams.CaliperSearchLength / 2;
                double diretion = m_LineTool.RunParams.CaliperSearchDirection;
                double HalfSearchLength = m_LineTool.RunParams.CaliperSearchLength / 2;
                double TempSearchLength = m_LineTool.RunParams.CaliperSearchLength;
                double searchDirection = m_LineTool.RunParams.CaliperSearchDirection;
                CogCaliperPolarityConstants edgePolarity = m_LineTool.RunParams.CaliperRunParams.Edge0Polarity;

                double Cal_StartX = 0;
                double Cal_StartY = 0;
                double Cal_EndX = 0;
                double Cal_EndY = 0;

                double noneEdge_Threshold = 0;
                int noeEdge_FilterSize = 0;

                try
                {
                    if (!m_bROIFinealignFlag)
                    {
                        if (Math.Abs(EndPosY - startPosY) < 100)
                        {
                            MoveTypeY = true;
                            if (startPosX > EndPosX)
                            {
                                diretion *= -1;
                            }
                        }
                        else
                        {
                            MoveTypeY = false;
                            if (startPosY > EndPosY)
                            {
                                diretion *= -1;
                            }
                        }
                    }

                    #region FindLine Search
                    CogFixtureTool mCogFixtureTool2 = new CogFixtureTool();

                    bool isTwiceFixture = false;
                    double dist = 0;

                    for (int i = 0; i < 2; i++) 
                    {
                        SingleFindLine[i] = new CogFindLineTool();
                        SingleFindLine[i] = m_LineTool;
                        noneEdge_Threshold = SingleFindLine[i].RunParams.CaliperRunParams.ContrastThreshold;
                        noeEdge_FilterSize = SingleFindLine[i].RunParams.CaliperRunParams.FilterHalfSizeInPixels;

                        if (i == 1)
                        {
                            //하나의 FindLineTool방향만 정반대로 돌려서 Search진행
                            dist = SingleFindLine[i].RunParams.CaliperSearchDirection;
                            SingleFindLine[i].RunParams.CaliperSearchDirection *= (-1);
                            SingleFindLine[i].RunParams.CaliperRunParams.Edge0Polarity = SingleFindLine[i].RunParams.CaliperRunParams.Edge1Polarity;

                            if (m_bROIFinealignFlag)
                            {
                                double Calrotation = m_dTempFineLineAngle - SingleFindLine[i].RunParams.ExpectedLineSegment.Rotation;

                                Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.StartX, SingleFindLine[i].RunParams.ExpectedLineSegment.StartY,
                                        SingleFindLine[i].RunParams.CaliperSearchLength / 2, Calrotation, out Cal_StartX, out Cal_StartY);

                                Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.EndX, SingleFindLine[i].RunParams.ExpectedLineSegment.EndY,
                                        SingleFindLine[i].RunParams.CaliperSearchLength / 2, Calrotation, out Cal_EndX, out Cal_EndY);

                                SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = Cal_StartX;
                                SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = Cal_EndX;
                                SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = Cal_StartY;
                                SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = Cal_EndY;
                            }
                            else
                            {
                                if (!MoveTypeY)
                                {
                                    if (diretion < 0)
                                    {
                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX + Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX + Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
                                    }
                                    else
                                    {
                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX - Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX - Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
                                    }
                                }
                                else
                                {
                                    if (diretion < 0)
                                    {

                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY - Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY - Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
                                    }
                                    else
                                    {
                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY + Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY + Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
                                    }
                                }
                            }
                        }

                        SingleFindLine[i].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
                        SingleFindLine[i].LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;

                        if (m_TeachParameter[nROI].bThresholdUse == true && i == 1)
                        {
                            // Crop 처리
                            var transform = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempFixtureTrans;
                            var cropResult = GetCropImage(cogImage, SingleFindLine[i], transform, out CogRectangle cropRect);

                            EdgeAlgorithm edgeAlgorithm = new EdgeAlgorithm();
                            edgeAlgorithm.Threshold = m_TeachParameter[nROI].iThreshold;

                            var image = cropResult.Item1 as CogImage8Grey;
                            image.CoordinateSpaceTree = new CogCoordinateSpaceTree();
                            image.SelectedSpaceName = "@";

                            edgeAlgorithm.IgnoreSize = m_TeachParameter[nROI].iIgnoreSize;
                            Mat convertImage = edgeAlgorithm.Inspect(image, ref SingleFindLine[i], cropResult.Item2, transform, cropRect);

                            if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
                                convertImage.Save(@"D:\convertImage.bmp");

                            double lengthX = Math.Abs(SingleFindLine[i].RunParams.ExpectedLineSegment.StartX - SingleFindLine[i].RunParams.ExpectedLineSegment.EndX);
                            double lengthY = Math.Abs(SingleFindLine[i].RunParams.ExpectedLineSegment.StartY - SingleFindLine[i].RunParams.ExpectedLineSegment.EndY);

                            int searchedValue = -1;
                            List<Point> boundRectPointList = new List<Point>();

                            if (lengthX > lengthY) // 가로
                            {
                                double startX = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX;
                                double startY = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY;
                                double endX = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX;
                                double endY = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY;
                                transform.MapPoint(startX, startY, out double orgStartX, out double orgStartY);
                                transform.MapPoint(endX, endY, out double orgEndX, out double orgEndY);

                                transform.MapPoint(cropRect.X, cropRect.Y, out double mappingStartX, out double mappingStartY);

                                if (orgStartX > orgEndX) // 화살표 방향 아래에서 위
                                {
                                    var minPosY = edgeAlgorithm.GetVerticalMinEdgeTopPosY(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
                                    if (minPosY.Count > 0)
                                    {
                                        searchedValue = minPosY.Min();
                                        int maskX = (int)mappingStartX; 
                                        int maskY = searchedValue + (int)mappingStartY; 

                                        Rectangle rect = new Rectangle((int)mappingStartX, 0, convertImage.Width, maskY);
                                
                                        int maskWidth = convertImage.Width; 
                                        int maskHeight = rect.Height;

                                        boundRectPointList.Add(new Point(maskX, maskY));
                                        boundRectPointList.Add(new Point(maskX, maskY - convertImage.Height));
                                        boundRectPointList.Add(new Point(maskX + maskWidth, maskY - convertImage.Height));
                                        boundRectPointList.Add(new Point(maskX + maskWidth, maskY));
                                    }
                                }
                                else// 화살표 방향 위에서 아래
                                {
                                    var edgePointList = edgeAlgorithm.GetVerticalEdgeBottomPos(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
                                    if (edgePointList.Count > 0)
                                    {
                                        var target = edgePointList.OrderByDescending(edgePoint => edgePoint.PointY);
                                        var minEdge = target.Last(); 
                                        var maxEdge = target.First();

                                        int leftTopY = (int)mappingStartY;
                                        int rightTopY = (int)mappingStartY;

                                        int leftTopTempY = minEdge.PointY > maxEdge.PointY ? maxEdge.PointY : minEdge.PointY;
                                        int rightTopTempY = minEdge.PointY > maxEdge.PointY ? minEdge.PointY : maxEdge.PointY;

                                        leftTopY += leftTopTempY;
                                        rightTopY += rightTopTempY;

                                        searchedValue = 1;
                      
                                        int maskX = (int)mappingStartX;
                                        int maskY = (int)mappingStartY; // Y 좌표 설정

                                        boundRectPointList.Add(new Point(maskX, leftTopY));
                                        boundRectPointList.Add(new Point(maskX + convertImage.Width, rightTopY));
                                        boundRectPointList.Add(new Point(maskX + convertImage.Width, rightTopY + convertImage.Height));
                                        boundRectPointList.Add(new Point(maskX, leftTopY + convertImage.Height));
                                    }
                                }
                            }
                            else
                            {
                                double startX = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX;
                                double startY = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY;
                                double endX = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX;
                                double endY = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY;
                                transform.MapPoint(startX, startY, out double orgStartX, out double orgStartY);
                                transform.MapPoint(endX, endY, out double orgEndX, out double orgEndY);

                                transform.MapPoint(cropRect.X, cropRect.Y, out double mappingStartX, out double mappingStartY);
                                if (orgStartX > orgEndX) // 화살표 방향 오른쪽에서 왼쪽
                                {
                                    searchedValue = edgeAlgorithm.GetHorizontalMinEdgePosY(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
                                    if (searchedValue >= 0)
                                    {
                                        // 마스크를 그릴 영역의 X, Y 좌표 계산
                                        int maskX = searchedValue + (int)mappingStartX; // X 좌표 설정
                                        int maskY = (int)mappingStartY; // Y 좌표 설정

                                        Rectangle rect = new Rectangle((int)mappingStartX, 0, convertImage.Width, maskY);

                                        // 마스크를 그릴 영역의 너비와 높이 계산
                                        int maskWidth = convertImage.Width; // 너비 설정
                                        int maskHeight = convertImage.Height; // 높이 설정

                                        boundRectPointList.Add(new Point(maskX, maskY));
                                        boundRectPointList.Add(new Point(maskX - convertImage.Width, maskY));
                                        boundRectPointList.Add(new Point(maskX - convertImage.Width, maskY + maskHeight));
                                        boundRectPointList.Add(new Point(maskX, maskY + maskHeight));
                                    }
                                }
                                else // 화살표 방향 왼쪽에서 오른쪽
                                {
                                    var edgePointList = edgeAlgorithm.GetHorizontalEdgePos(convertImage, m_TeachParameter[nROI].iTopCutPixel, m_TeachParameter[nROI].iBottomCutPixel);
                                    if(edgePointList.Count > 0)
                                    {
                                        var target = edgePointList.OrderByDescending(edgePoint => edgePoint.PointX);
                                        var minEdge = target.Last();
                                        var maxEdge = target.First();

                                        int leftTopTempX = minEdge.PointY > maxEdge.PointY ? maxEdge.PointX : minEdge.PointX;
                                        int leftBottomTempX = minEdge.PointY > maxEdge.PointY ? minEdge.PointX : maxEdge.PointX;

                                        int leftTopX = (int)mappingStartX;
                                        int leftBottomX = (int)mappingStartX;

                                        leftTopX += leftTopTempX;
                                        leftBottomX += leftBottomTempX;

                                        searchedValue = 1;

                                        //searchedValue = min;
                                        int maskX = (int)mappingStartX;
                                        int maskY = (int)mappingStartY; // Y 좌표 설정

                                        boundRectPointList.Add(new Point(leftTopX, maskY));
                                        boundRectPointList.Add(new Point(leftTopX + convertImage.Width, maskY));
                                        boundRectPointList.Add(new Point(leftBottomX + convertImage.Width, maskY + convertImage.Height));
                                        boundRectPointList.Add(new Point(leftBottomX, maskY + convertImage.Height));

                                    }
                                }
                                //   
                              
                            }

                            if (searchedValue >= 0)
                            {
                                int MaskingValue = m_TeachParameter[nROI].iMaskingValue; // UI 에 빼야함
                                MCvScalar maskingColor = new MCvScalar(MaskingValue);

                                Mat matImage = edgeAlgorithm.GetConvertMatImage(cogImage.CopyBase(CogImageCopyModeConstants.CopyPixels) as CogImage8Grey);
                                CvInvoke.FillPoly(matImage, new VectorOfPoint(boundRectPointList.ToArray()), maskingColor);
                                //matImage.Save(@"D:\matImage.bmp");

                                var filterImage = edgeAlgorithm.GetConvertCogImage(matImage);

                                SingleFindLine[i].RunParams.CaliperRunParams.ContrastThreshold = m_TeachParameter[nROI].iEdgeCaliperThreshold;
                                SingleFindLine[i].RunParams.CaliperRunParams.FilterHalfSizeInPixels = m_TeachParameter[nROI].iEdgeCaliperFilterSize;
                                SingleFindLine[i].InputImage = (CogImage8Grey)filterImage;
                                List_NG.Items.Add("Found Gray Area.");

                                if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
                                {
                                    CogImageFileBMP bmp3 = new CogImageFileBMP();
                                    bmp3.Open(@"D:\filterImage.bmp", CogImageFileModeConstants.Write);
                                    bmp3.Append(filterImage);
                                    bmp3.Close();
                                }
                            }
                            else
                            {
                                // Edge 못찾은 경우
                                SingleFindLine[i].RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
                                SingleFindLine[i].RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
                                SingleFindLine[i].InputImage = cogImage;
                                List_NG.Items.Add("Not Found Gray Area.");
                            }

                            if (cogImage.SelectedSpaceName == "@\\Fixture\\Fixture")
                                isTwiceFixture = true;

                            if (searchedValue >= 0)
                            {
                                mCogFixtureTool2.InputImage = SingleFindLine[i].InputImage;
                                mCogFixtureTool2.RunParams.UnfixturedFromFixturedTransform = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempFixtureTrans;
                                mCogFixtureTool2.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
                                mCogFixtureTool2.Run();

                                SingleFindLine[i].InputImage = (CogImage8Grey)mCogFixtureTool2.OutputImage;
                            }
                            else
                                isTwiceFixture = true;
                        }
                        else
                        {

                            SingleFindLine[i].InputImage = cogImage;
                        }

                        SingleFindLine[i].Run();

                        if (SingleFindLine[i].Results == null)
                        {
                            m_LineTool.RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
                            m_LineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
                            m_LineTool.RunParams.CaliperSearchDirection = searchDirection;
                            m_LineTool.RunParams.CaliperRunParams.Edge0Polarity = edgePolarity;
                            m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
                            m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
                            m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
                            m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;
                            continue;
                        }

                        //Search OK
                        if (SingleFindLine[i].Results != null || SingleFindLine[i].Results.Count > 0)
                        {
                            ResultData = new double[SingleFindLine[i].Results.Count];
                            for (int j = 0; j < SingleFindLine[i].Results.Count; j++)
                            {
                                if (isTwiceFixture)
                                {
                                    var graphic = SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge);

                                    foreach (var item in graphic.Shapes)
                                    {
                                        if (item is CogLineSegment line)
                                        {
                                            cogImage.GetTransform("@", cogImage.SelectedSpaceName).MapPoint(line.StartX, line.StartY, out double mX, out double mY);
                                            mCogFixtureTool2.RunParams.UnfixturedFromFixturedTransform.MapPoint(line.StartX, line.StartY, out double mappingStartX, out double mappingStartY);
                                            line.StartX = mappingStartX;
                                            line.StartY = mappingStartY;

                                            mCogFixtureTool2.RunParams.UnfixturedFromFixturedTransform.MapPoint(line.EndX, line.EndY, out double mappingEndX, out double mappingEndY);
                                            line.EndX = mappingEndX;
                                            line.EndY = mappingEndY;
                                        }
                                    }

                                    GraphicData.Add(graphic);
                                }
                                else
                                {
                                    //
                                    var graphic = SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge);
                                    GraphicData.Add(graphic);
                                }

                                if (SingleFindLine[i].Results[j].CaliperResults.Count == 1)
                                {
                                    RawSearchData[i, j].X = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionX;
                                    RawSearchData[i, j].Y = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionY;
                                }
                                else
                                {
                                    RawSearchData[i, j].X = 0;
                                    RawSearchData[i, j].Y = 0;
                                    NonCaliperCnt++;
                                }
                            }

                        }
                        //Search NG
                        else
                        {
                            bRes = false;
                        }
                    }
                    #endregion

                    #region Result Data Calculate
                    for (int i = 0; i < SingleFindLine[0].Results.Count; i++)
                    {
                        //두 점 사이의 거리 
                        ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
                        Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
                    }
                    #endregion

                    m_LineTool.RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
                    m_LineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
                    m_LineTool.RunParams.CaliperSearchDirection = searchDirection;
                    m_LineTool.RunParams.CaliperRunParams.Edge0Polarity = edgePolarity;
                    m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
                    m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
                    m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
                    m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;

                    return bRes;
                }
                catch (Exception err)
                {
                    m_LineTool.RunParams.CaliperRunParams.ContrastThreshold = noneEdge_Threshold;
                    m_LineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = noeEdge_FilterSize;
                    m_LineTool.RunParams.CaliperSearchDirection = searchDirection;
                    m_LineTool.RunParams.CaliperRunParams.Edge0Polarity = edgePolarity;
                    m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
                    m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
                    m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
                    m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;

                    string LogMsg;
                    LogMsg = "Inspeciton Excetion NG"; LogMsg += "Error : " + err.ToString();
                    List_NG.Items.Add(LogMsg);
                    List_NG.Items.Add(nROI.ToString());
                    ResultData = new double[] { };
                    NonCaliperCnt = 0;
                    GraphicData = new CogGraphicInteractiveCollection();
                    return false;
                }

            }
            else   //Circle Tool
            {
                try
                {
                    CogFindCircleTool m_CircleTool = tool as CogFindCircleTool;
                    bool bRes = true;
                    int nCaliperCount;
                    CogFindCircleTool[] SingleCircleLine = new CogFindCircleTool[2];
                    PointF[,] RawSearchData = new PointF[2, 100];
                    ResultData = new double[100];
                    /*GraphicData = new CogGraphicInteractiveCollection()*/
                    CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();

                    #region FindLine Search
                    m_CircleTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                    m_CircleTool.InputImage = cogImage;
                    m_CircleTool.Run();

                    if (m_CircleTool.Results != null)
                    {
                        nCaliperCount = m_CircleTool.Results.Count;
                        NonCaliperCnt = 0;

                    }
                    else
                    {
                        NonCaliperCnt = 0;
                        return false;
                    }
                    //Search OK
                    if (m_CircleTool.Results != null || m_CircleTool.Results.Count > 0)
                    {
                        ResultData = new double[m_CircleTool.Results.Count];
                        for (int j = 0; j < m_CircleTool.Results.Count; j++)
                        {

                            GraphicData.Add(m_CircleTool.Results[j].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge));
                            if (m_CircleTool.Results[j].CaliperResults.Count >= 1)
                            {
                                RawSearchData[0, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionX;
                                RawSearchData[0, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionY;
                                RawSearchData[1, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionX;
                                RawSearchData[1, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionY;
                            }
                            else
                            {
                                RawSearchData[0, j].X = 0;
                                RawSearchData[0, j].Y = 0;
                                RawSearchData[1, j].X = 0;
                                RawSearchData[1, j].Y = 0;
                                NonCaliperCnt++;
                            }
                        }
                    }
                    //Search NG
                    else
                    {
                        bRes = false;
                    }
                    #endregion

                    #region Result Data Calculate
                    for (int i = 0; i < m_CircleTool.Results.Count; i++)
                    {
                        //두 점 사이의 거리 
                        ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
                        Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
                    }
                    #endregion

                    return bRes;
                }
                catch (Exception err)
                {
                    string LogMsg;
                    LogMsg = "Inspeciton Excetion NG"; LogMsg += "Error : " + err.ToString();
                    List_NG.Items.Add(LogMsg);
                    List_NG.Items.Add(nROI.ToString());
                    ResultData = new double[] { };
                    NonCaliperCnt = 0;
                    GraphicData = new CogGraphicInteractiveCollection();
                    return false;
                }
            }
        }

        public IntPtr GetIntptr(CogImage8Grey image, out int stride)
        {
            unsafe
            {
                var cogPixelData = image.Get8GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, image.Width, image.Height);
                IntPtr ptrData = cogPixelData.Scan0;
                stride = cogPixelData.Stride;

                return ptrData;
            }
        }
        private Tuple<CogImage8Grey, EdgeDirection> GetCropImage(CogImage8Grey cogImage, CogFindLineTool tool, CogTransform2DLinear transform, out CogRectangle cropRect)
        {
            cropRect = new CogRectangle();
            EdgeDirection direction = EdgeDirection.Top;

            double MinLineDegreeStand = 1.396;
            double MaxLineDegreeStand = 1.745;

            //1.가로, 세로 확인                     
            if (Math.Abs(tool.RunParams.ExpectedLineSegment.Rotation) > MinLineDegreeStand &&
               Math.Abs(tool.RunParams.ExpectedLineSegment.Rotation) < MaxLineDegreeStand)
            {
                direction = EdgeDirection.Left;
                //2.세로인 경우, 사분면 중 어디에 위치해 있는지 확인
                if (tool.RunParams.ExpectedLineSegment.StartY < 0) //음수 1,4분면
                {
                    //3.Start Y, End Y 중 어떤게 상단에 위치해 있는지 확인
                    if (tool.RunParams.ExpectedLineSegment.StartY <
                        tool.RunParams.ExpectedLineSegment.EndY)
                    {
                        //Start Y가 상단에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.StartX - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY;
                        cropRect.Width = tool.RunParams.CaliperSearchLength;
                        cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
                    }
                    else
                    {
                        //End Y가 상단에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.EndX - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY;
                        cropRect.Width = tool.RunParams.CaliperSearchLength;
                        cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
                    }
                }
                else //양수 2,3분면
                {
                    //3.Start Y, End Y 중 어떤게 상단에 위치해 있는지 확인
                    if (tool.RunParams.ExpectedLineSegment.StartY <
                        tool.RunParams.ExpectedLineSegment.EndY)
                    {
                        //Start Y가 상단에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.StartX - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY;
                        cropRect.Width = tool.RunParams.CaliperSearchLength;
                        cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
                    }
                    else
                    {
                        //End Y가 상단에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.EndX - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY;
                        cropRect.Width = tool.RunParams.CaliperSearchLength;
                        cropRect.Height = tool.RunParams.ExpectedLineSegment.Length;
                    }
                }
            }
            else
            {
                direction = EdgeDirection.Top;
                //2.가로인 경우, 사분면 중 어디에 위치해 있는지 확인
                if (tool.RunParams.ExpectedLineSegment.StartX < 0) //음수 3,4분면
                {
                    //3.Start X, End X 중 어떤게 좌측에 위치해 있는지 확인
                    if (tool.RunParams.ExpectedLineSegment.StartX <
                       tool.RunParams.ExpectedLineSegment.EndX)
                    {
                        //Start X가  좌측에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.StartX;
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
                        cropRect.Height = tool.RunParams.CaliperSearchLength;
                    }
                    else
                    {
                        //End X가 좌측에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.EndX;
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
                        cropRect.Height = tool.RunParams.CaliperSearchLength;
                    }

                }
                else //양수 1,2분면
                {
                    //3.Start X, End X 중 어떤게 좌측에 위치해 있는지 확인
                    if (tool.RunParams.ExpectedLineSegment.StartX <
                       tool.RunParams.ExpectedLineSegment.EndX)
                    {
                        //Start X가 좌측에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.StartX;
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.StartY - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
                        cropRect.Height = tool.RunParams.CaliperSearchLength;
                    }
                    else
                    {
                        //End X가 좌측에 있음
                        cropRect.X = tool.RunParams.ExpectedLineSegment.EndX;
                        cropRect.Y = tool.RunParams.ExpectedLineSegment.EndY - (tool.RunParams.CaliperSearchLength / 2);
                        cropRect.Width = tool.RunParams.ExpectedLineSegment.Length;
                        cropRect.Height = tool.RunParams.CaliperSearchLength;
                    }
                }

            }

            EdgeAlgorithm edge = new EdgeAlgorithm();
            Mat mat =  edge.GetConvertMatImage(cogImage);
            //mat.Save(@"D:\test.bmp");
           

            transform.MapPoint(cropRect.X, cropRect.Y, out double cropX, out double cropY);
            Rectangle rectFromMat = new Rectangle();
            rectFromMat.X = (int)cropX;
            rectFromMat.Y = (int)cropY;
            rectFromMat.Width = (int)cropRect.Width;
            rectFromMat.Height = (int)cropRect.Height;

            Mat cropMat = edge.CropRoi(mat, rectFromMat);

            if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
                cropMat.Save(@"D:\cropMat.bmp");

            mat.Dispose();

            return new Tuple<CogImage8Grey, EdgeDirection>(edge.GetConvertCogImage(cropMat), direction);
        }

        private bool GaloDirectionConvertInspection(int nROI, int toolType, object tool, CogImage8Grey cogImage, out double[] ResultData, ref CogGraphicInteractiveCollection GraphicData, out int NonCaliperCnt)
        {    
            try
            {
                NonCaliperCnt = 0;
                if (toolType == (int)enumROIType.Line)
                {
                    bool MoveTypeY = false;
                    //2023 0130 YSH
                    bool bRes = true;
                    CogFindLineTool m_LineTool = tool as CogFindLineTool;

                    int[] nCaliperCount = new int[2];
                    CogFindLineTool[] SingleFindLine = new CogFindLineTool[2];
                    PointF[,] RawSearchData = new PointF[2, 100];
                    ResultData = new double[100];
                    CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();
                    double startPosX = m_LineTool.RunParams.ExpectedLineSegment.StartX;
                    double startPosY = m_LineTool.RunParams.ExpectedLineSegment.StartY;
                    double EndPosX = m_LineTool.RunParams.ExpectedLineSegment.EndX;
                    double EndPosY = m_LineTool.RunParams.ExpectedLineSegment.EndY;
                    double MovePos1, MovePos2;
                    double Move = m_LineTool.RunParams.CaliperSearchLength / 2;
                    double diretion = m_LineTool.RunParams.CaliperSearchDirection;
                    double HalfSearchLength = m_LineTool.RunParams.CaliperSearchLength / 2;
                    double TempSearchLength = m_LineTool.RunParams.CaliperSearchLength;

                    double Cal_StartX = 0;
                    double Cal_StartY = 0;
                    double Cal_EndX = 0;
                    double Cal_EndY = 0;

                    if (!m_bROIFinealignFlag)
                    {
                        if (Math.Abs(EndPosY - startPosY) < 100)
                        {
                            MoveTypeY = true;
                            if (startPosX > EndPosX)
                            {
                                diretion *= -1;
                            }
                        }
                        else
                        {
                            MoveTypeY = false;
                            if (startPosY > EndPosY)
                            {
                                diretion *= -1;
                            }
                        }
                    }

                    #region FindLine Search
                    for (int i = 0; i < 2; i++) //Left, Right 의미
                    {
                        SingleFindLine[i] = new CogFindLineTool(m_LineTool);
                        //SingleFindLine[i] = m_LineTool;                     

                        //2023.06.15 YSH
                        //기존방식대로 Search 못했을 경우에만 방향 변경하여 재 Search 동작함.
                        if (m_bInspDirectionChange)
                        {
                            //Search 방향 변경
                            SingleFindLine[i].RunParams.CaliperSearchDirection *= (-1);
                            //극성 변경
                            SingleFindLine[i].RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DarkToLight;
                            //Caliper Search Length 절반으로 줄임
                            SingleFindLine[i].RunParams.CaliperSearchLength = HalfSearchLength;
                        }

                        if (i == 1)
                        {
                            //하나의 FindLineTool방향만 정반대로 돌려서 Search진행
                            double dist = SingleFindLine[i].RunParams.CaliperSearchDirection;
                            SingleFindLine[i].RunParams.CaliperSearchDirection *= (-1);

                            if (m_bROIFinealignFlag)
                            {
                                double Calrotation = m_dTempFineLineAngle - SingleFindLine[i].RunParams.ExpectedLineSegment.Rotation;

                                Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.StartX, SingleFindLine[i].RunParams.ExpectedLineSegment.StartY,
                                        SingleFindLine[i].RunParams.CaliperSearchLength, Calrotation, out Cal_StartX, out Cal_StartY);

                                Main.AlignUnit[m_AlignNo].Position_Calculate(SingleFindLine[i].RunParams.ExpectedLineSegment.EndX, SingleFindLine[i].RunParams.ExpectedLineSegment.EndY,
                                        SingleFindLine[i].RunParams.CaliperSearchLength, Calrotation, out Cal_EndX, out Cal_EndY);

                                SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = Cal_StartX;
                                SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = Cal_EndX;
                                SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = Cal_StartY;
                                SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = Cal_EndY;
                            }
                            else
                            {
                                if (!MoveTypeY)
                                {
                                    if (diretion < 0)
                                    {
                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX + Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX + Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
                                    }
                                    else
                                    {
                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartX - Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndX - Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartX = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndX = MovePos2;
                                    }
                                }
                                else
                                {
                                    if (diretion < 0)
                                    {

                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY - Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY - Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
                                    }
                                    else
                                    {
                                        MovePos1 = SingleFindLine[i].RunParams.ExpectedLineSegment.StartY + Move;
                                        MovePos2 = SingleFindLine[i].RunParams.ExpectedLineSegment.EndY + Move;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.StartY = MovePos1;
                                        SingleFindLine[i].RunParams.ExpectedLineSegment.EndY = MovePos2;
                                    }
                                }
                            }

                        }


                        SingleFindLine[i].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;
                        SingleFindLine[i].LastRunRecordDiagEnable = CogFindLineLastRunRecordDiagConstants.None;
                        SingleFindLine[i].InputImage = cogImage;
                        SingleFindLine[i].Run();

                        nCaliperCount[i] = SingleFindLine[i].Results.Count;
                        //Search OK
                        if (SingleFindLine[i].Results != null || SingleFindLine[i].Results.Count > 0)
                        {
                            ResultData = new double[SingleFindLine[i].Results.Count];
                            for (int j = 0; j < SingleFindLine[i].Results.Count; j++)
                            {
                                GraphicData.Add(SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge));
                                //GraphicData.Add(SingleFindLine[i].Results[j].CreateResultGraphics(CogFindLineResultGraphicConstants.CaliperEdge | CogFindLineResultGraphicConstants.CaliperRegion));
                                if (SingleFindLine[i].Results[j].CaliperResults.Count == 1)
                                {
                                    RawSearchData[i, j].X = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionX;
                                    RawSearchData[i, j].Y = (float)SingleFindLine[i].Results[j].CaliperResults[0].Edge0.PositionY;
                                }
                                else
                                {
                                    RawSearchData[i, j].X = 0;
                                    RawSearchData[i, j].Y = 0;
                                    NonCaliperCnt++;

                                }
                            }

                        }
                        //Search NG
                        else
                        {
                            bRes = false;
                        }

                    }


                    #endregion

                    #region Result Data Calculate
                    //두 FindLine에서 찾은 Caliper 개수가 상이할때
                    if (nCaliperCount[0] != nCaliperCount[1])
                    {

                    }


                    //for (int i = 0; i < SingleFindLine[0].Results.Count; i++)
                    //{
                    //    //두 점 사이의 거리 
                    //    ResultData[i] = Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
                    //    Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)));
                    //}

                    for (int i = 0; i < SingleFindLine[0].Results.Count; i++)
                    {
                        //두 점 사이의 거리 
                        ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
                        Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
                    }

                    #endregion
                    m_LineTool.RunParams.ExpectedLineSegment.StartX = startPosX;
                    m_LineTool.RunParams.ExpectedLineSegment.StartY = startPosY;
                    m_LineTool.RunParams.ExpectedLineSegment.EndX = EndPosX;
                    m_LineTool.RunParams.ExpectedLineSegment.EndY = EndPosY;
                    return bRes;
                }
                else   //Circle Tool
                {
                    CogFindCircleTool m_CircleTool = tool as CogFindCircleTool;
                    bool bRes = true;
                    int nCaliperCount;
                    CogFindCircleTool[] SingleCircleLine = new CogFindCircleTool[2];
                    PointF[,] RawSearchData = new PointF[2, 100];
                    ResultData = new double[100];
                    /*GraphicData = new CogGraphicInteractiveCollection()*/
                    CogDistancePointPointTool DistanceData = new CogDistancePointPointTool();

                    #region FindLine Search
                    //for (int i = 0; i < 2; i++) //Left, Right 의미
                    //{
                    //    SingleCircleLine[i] = new CogFindCircleTool();
                    //    SingleCircleLine[i] = m_CircleTool;
                    //    if (i == 1) //하나의 FindLineTool방향만 정반대로 돌려서 Search진행
                    //    {
                    //        CogFindCircleSearchDirectionConstants DirType = SingleCircleLine[i].RunParams.CaliperSearchDirection;
                    //        if (DirType == CogFindCircleSearchDirectionConstants.Inward)
                    //            SingleCircleLine[i].RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                    //        else
                    //            SingleCircleLine[i].RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;
                    //        double MoveX;
                    //        if (SingleCircleLine[i].RunParams.CaliperSearchDirection == CogFindCircleSearchDirectionConstants.Inward)
                    //        {
                    //            if(dAngle >0)
                    //               MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX - Move;
                    //            else
                    //               MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX + Move;
                    //            SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX = MoveX;
                    //        }
                    //        else
                    //        {
                    //            if (dAngle > 0)
                    //                MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX + Move;
                    //            else
                    //                MoveX = SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX - Move;
                    //            SingleCircleLine[i].RunParams.ExpectedCircularArc.CenterX = MoveX;
                    //        }
                    //    }
                    m_CircleTool.RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.Pair;
                    m_CircleTool.InputImage = cogImage;
                    m_CircleTool.Run();

                    //SingleCircleLine[i].RunParams.CaliperRunParams.EdgeMode = CogCaliperEdgeModeConstants.SingleEdge;


                    if (m_CircleTool.Results != null)
                    {
                        nCaliperCount = m_CircleTool.Results.Count;
                        NonCaliperCnt = 0;

                    }
                    else
                    {
                        NonCaliperCnt = 0;
                        return false;
                    }
                    //Search OK
                    if (m_CircleTool.Results != null || m_CircleTool.Results.Count > 0)
                    {
                        ResultData = new double[m_CircleTool.Results.Count];
                        for (int j = 0; j < m_CircleTool.Results.Count; j++)
                        {

                            GraphicData.Add(m_CircleTool.Results[j].CreateResultGraphics(CogFindCircleResultGraphicConstants.CaliperEdge));
                            if (m_CircleTool.Results[j].CaliperResults.Count >= 1)
                            {
                                RawSearchData[0, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionX;
                                RawSearchData[0, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge0.PositionY;
                                RawSearchData[1, j].X = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionX;
                                RawSearchData[1, j].Y = (float)m_CircleTool.Results[j].CaliperResults[0].Edge1.PositionY;
                            }
                            else
                            {
                                RawSearchData[0, j].X = 0;
                                RawSearchData[0, j].Y = 0;
                                RawSearchData[1, j].X = 0;
                                RawSearchData[1, j].Y = 0;
                                NonCaliperCnt++;
                            }
                        }
                    }
                    //Search NG
                    else
                    {
                        bRes = false;
                    }


                    #endregion
                    //double dx1 = SingleCircleLine[0].Results[0].CaliperResults[0].Edge0.PositionX;
                    //double dx2 = SingleCircleLine[1].Results[1].CaliperResults[0].Edge0.PositionX;
                    #region Result Data Calculate
                    //두 FindLine에서 찾은 Caliper 개수가 상이할때


                    for (int i = 0; i < m_CircleTool.Results.Count; i++)
                    {
                        //두 점 사이의 거리 
                        ResultData[i] = (Math.Sqrt((Math.Pow(RawSearchData[0, i].X - RawSearchData[1, i].X, 2) +
                        Math.Pow(RawSearchData[0, i].Y - RawSearchData[1, i].Y, 2)))) * 13.36 / 1000;
                    }

                    #endregion

                    return bRes;
                }
            }
            catch (Exception err)
            {
                // PAT[m_PatTagNo, 0].SetAllLight(Main.DEFINE.M_LIGHT_CNL);
                string LogMsg;
                //LogMsg = "Inspeciton Excetion NG Type:" + m_ROYTpe.ToString() + " " + "ROI No:" + nRoi.ToString() + "CaliperIndex:" + jCaliperIndex.ToString();
                //LogdataDisplay(LogMsg, true);
                LogMsg = "Inspeciton Excetion NG"; LogMsg += "Error : " + err.ToString();
                //LogdataDisplay(LogMsg, true);
                List_NG.Items.Add(LogMsg);
                List_NG.Items.Add(nROI.ToString());
                ResultData = new double[] { };
                NonCaliperCnt = 0;
                GraphicData = new CogGraphicInteractiveCollection();
                return false;
            }
            

        }

        private void init_ComboPolarity()
        {
            //기존 관로검사 : Combo_Polarity1,2,3 
            Combo_Polarity1.Items.Clear();
            Combo_Polarity2.Items.Clear();
            Combo_Polarity3.Items.Clear();
            cmbEdgePolarityType.Items.Clear();
            string[] strName = new string[3];
            strName[0] = "Dark -> Light";
            strName[1] = "Light -> Dark";
            strName[2] = "Don't Care";
            for (int i = 0; i < 3; i++)
            {
                Combo_Polarity1.Items.Add(strName[i]);
                Combo_Polarity2.Items.Add(strName[i]);
                Combo_Polarity3.Items.Add(strName[i]);

                //Bonding Area Align Polarity : cmbEdgePolarityType
                cmbEdgePolarityType.Items.Add(strName[i]);
            }
            Combo_Polarity1.SelectedIndex = 2;
            Combo_Polarity2.SelectedIndex = 2;
            Combo_Polarity3.SelectedIndex = 2;

            //Bonding Area Align Polarity : cmbEdgePolarityType
            cmbEdgePolarityType.SelectedIndex = 2;

        }
        private void LAB_CALIPER_SEARCHLENTH_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_CALIPER_SEARCHLENTH.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double CaliperSearchLenth = KeyPad.m_data;
            if (enumROIType.Line == m_enumROIType)
            {
                m_TempFindLineTool.RunParams.CaliperSearchLength = CaliperSearchLenth;
            }
            else
            {
                m_TempFindCircleTool.RunParams.CaliperSearchLength = CaliperSearchLenth;
            }
            LAB_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", CaliperSearchLenth);
        }
        #endregion

        private void Combo_Polarity1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CogCaliperPolarityConstants Polarity;
            int TempIndex = 0;
            if (m_enumROIType == enumROIType.Line)
            {
                TempIndex = Combo_Polarity1.SelectedIndex;
                Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
                //if (_useROITracking)
                //{
                //    if (TrackingLine.TrackingLine != null)
                //        TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
                //    else
                //        m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
                //}
                //else
                    m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            }
            else
            {
                TempIndex = Combo_Polarity1.SelectedIndex;
                Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
                //if (_useROITracking)
                //{
                //    if (TrackingCircle.TrackingCircle != null)
                //        TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
                //    else
                //        m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
                //}
                //else
                    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            }
        }

        private void Combo_Polarity2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CogCaliperPolarityConstants Polarity;
            int TempIndex = 0;
            if (m_enumROIType == enumROIType.Line)
            {
                TempIndex = Combo_Polarity2.SelectedIndex;
                Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
                //if (_useROITracking)
                //{
                //    if (TrackingLine.TrackingLine != null)
                //        TrackingLine.TrackingLine.RunParams.CaliperRunParams.Edge1Polarity = Polarity;
                //}
                //else
                    m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Polarity = Polarity;
            }
            else
            {
                TempIndex = Combo_Polarity2.SelectedIndex;
                Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
                //if (_useROITracking)
                //{
                //    if (TrackingCircle.TrackingCircle != null)
                //        TrackingCircle.TrackingCircle.RunParams.CaliperRunParams.Edge1Polarity = Polarity;
                //}
                //else
                    m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Polarity = Polarity;
            }
        }

        private void btn_Inspection_Test_Click(object sender, EventArgs e)
        {
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            try
            {
                CogStopwatch Stopwatch = new CogStopwatch();

                CogGraphicLabel[] Label;

                float nFontSize = (float)((PT_Display01.Height / Main.DEFINE.FontSize) * PT_Display01.Zoom);
                Stopwatch.Start();
                PT_Display01.InteractiveGraphics.Clear();
                PT_Display01.StaticGraphics.Clear();

                resultGraphics.Clear();
                //PT_Display01.Image = OriginImage;
                PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
                //bool bSearchRes = Search_PATCNL();
                bool[] bROIRes;
                bool bRes = true;
                double[] dDistance;
                List_NG.Items.Clear();
                bool bSearchRes = true;
                int ignore = 0;
                //Live Mode On상태일 시, Off로 변경
                if (BTN_LIVEMODE.Checked)
                {
                    BTN_LIVEMODE.Checked = false;
                    BTN_LIVEMODE.BackColor = Color.DarkGray;
                }
                if (bSearchRes == true)
                {
                    if (!FinalTracking()) return;
                    dDistance = new double[m_TeachParameter.Count];
                    bROIRes = new bool[m_TeachParameter.Count];

                    //bsi ksh ex)
                    //Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].

                    double[,,] InspData = new double[m_TeachParameter.Count, 100, 4];
                    for (int i = 0; i < m_TeachParameter.Count; i++)
                    //Parallel.For(0, m_TeachParameter.Count, i =>
                    {
                        if (i == 0)
                        {
                            for (int iHistogram = 0; iHistogram < m_TeachParameter[i].iHistogramROICnt; iHistogram++)
                            {
                                CogGraphicLabel HistogramValue = new CogGraphicLabel();
                                HistogramValue.Font = new Font(Main.DEFINE.FontStyle, 15);
                                double ResulteCenterX, ResulteCenterY;
                                CogHistogramTool InspeHistogramTool = m_TeachParameter[i].m_CogHistogramTool[iHistogram];
                                CogRectangleAffine Rect = (CogRectangleAffine)InspeHistogramTool.Region;
                                ResulteCenterX = Rect.CenterX;
                                ResulteCenterY = Rect.CenterY;
                                InspeHistogramTool.InputImage = (CogImage8Grey)PT_Display01.Image;
                                InspeHistogramTool.Run();
                                if (InspeHistogramTool.Result.Mean > m_TeachParameter[i].iHistogramSpec[iHistogram])
                                {
                                    CogRectangleAffine Result = new CogRectangleAffine();
                                    Result = (CogRectangleAffine)InspeHistogramTool.Region;
                                    Result.Color = CogColorConstants.Red;
                                    HistogramValue.Color = CogColorConstants.Red;
                                    HistogramValue.X = ResulteCenterX;
                                    HistogramValue.Y = ResulteCenterY;
                                    HistogramValue.Text = string.Format("{0:F3}", InspeHistogramTool.Result.Mean);
                                    PT_Display01.StaticGraphics.Add(HistogramValue, "Histogram");
                                    PT_Display01.StaticGraphics.Add(Result, "Histogram1");
                                    string LogMsg;
                                    LogMsg = string.Format("Inspection NG Histogram ROI:{0:D}", iHistogram + 1); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
                                    LogMsg += "\n";
                                    List_NG.Items.Add(LogMsg);
                                }
                                else
                                {
                                    CogRectangleAffine Result = new CogRectangleAffine();
                                    Result = (CogRectangleAffine)InspeHistogramTool.Region;
                                    Result.Color = CogColorConstants.Blue;
                                    HistogramValue.Color = CogColorConstants.Green;
                                    HistogramValue.X = ResulteCenterX;
                                    HistogramValue.Y = ResulteCenterY;
                                    HistogramValue.Text = string.Format("{0:F3}", InspeHistogramTool.Result.Mean);
                                    PT_Display01.StaticGraphics.Add(HistogramValue, "Histogram");
                                    PT_Display01.StaticGraphics.Add(Result, "Histogram1");
                                }
                            }
                        }
                        m_enumROIType = (enumROIType)m_TeachParameter[i].m_enumROIType;
                        if (enumROIType.Line == m_enumROIType)
                        {
                            CogFindLineTool InspCogFindLine = new CogFindLineTool();
                            InspCogFindLine = m_TeachParameter[i].m_FindLineTool;
                            CogGraphicInteractiveCollection subresultGraphics = new CogGraphicInteractiveCollection();
                            double[] Result;
                            if (!GaloOppositeInspection(i, (int)enumROIType.Line, InspCogFindLine, (CogImage8Grey)PT_Display01.Image, out Result, ref subresultGraphics, out ignore))
                            {
                                if(m_bInspDirectionChange)
                                {
                                    subresultGraphics.Clear();
                                    bRes = GaloDirectionConvertInspection(0, (int)enumROIType.Line, InspCogFindLine, (CogImage8Grey)PT_Display01.Image, out Result, ref subresultGraphics, out ignore);                                   
                                }

                                if(!bRes)
                                {
                                    double dCenterX = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointX;
                                    double dCenterY = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointY;
                                    double dAngle = InspCogFindLine.RunParams.ExpectedLineSegment.Rotation;
                                    double dLenth = InspCogFindLine.RunParams.ExpectedLineSegment.Length;
                                    CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
                                    CogNGRectAffine.Color = CogColorConstants.Red;
                                    CogNGRectAffine.CenterX = dCenterX;
                                    CogNGRectAffine.CenterY = dCenterY;
                                    CogNGRectAffine.SideXLength = dLenth;
                                    CogNGRectAffine.SideYLength = 100;
                                    CogNGRectAffine.Rotation = dAngle;
                                    resultGraphics.Add(CogNGRectAffine);
                                    string LogMsg;
                                    LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
                                    LogMsg += "\n";
                                    List_NG.Items.Add(LogMsg);
                                    bRes = false;
                                    for (int k = 0; k < subresultGraphics.Count; k++)
                                    {
                                        resultGraphics.Add(subresultGraphics[k]);
                                    }
                                    continue;
                                }

                            }
                            bROIRes[i] = InspResultData(Result, m_TeachParameter[i].dSpecDistance, m_TeachParameter[i].dSpecDistanceMax, m_TeachParameter[i].IDistgnore, ignore);
                            if (bROIRes[i] == false)
                            {
                                if (m_bInspDirectionChange)
                                {
                                    subresultGraphics.Clear();
                                    bRes = GaloDirectionConvertInspection(0, (int)enumROIType.Line, InspCogFindLine, (CogImage8Grey)PT_Display01.Image, out Result, ref subresultGraphics, out ignore);
                                }

                                if (!bRes)
                                {
                                    double dCenterX = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointX;
                                    double dCenterY = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointY;
                                    double dAngle = InspCogFindLine.RunParams.ExpectedLineSegment.Rotation;
                                    double dLenth = InspCogFindLine.RunParams.ExpectedLineSegment.Length;
                                    CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
                                    CogNGRectAffine.Color = CogColorConstants.Red;
                                    CogNGRectAffine.CenterX = dCenterX;
                                    CogNGRectAffine.CenterY = dCenterY;
                                    CogNGRectAffine.SideXLength = dLenth;
                                    CogNGRectAffine.SideYLength = 100;
                                    CogNGRectAffine.Rotation = dAngle;
                                    resultGraphics.Add(CogNGRectAffine);
                                    string LogMsg;
                                    LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
                                    LogMsg += "\n";
                                    List_NG.Items.Add(LogMsg);
                                    bRes = false;
                                    for (int k = 0; k < subresultGraphics.Count; k++)
                                    {
                                        resultGraphics.Add(subresultGraphics[k]);
                                    }
                                    continue;
                                }
                                else
                                {
                                    bROIRes[i] = InspResultData(Result, m_TeachParameter[i].dSpecDistance, m_TeachParameter[i].dSpecDistanceMax, m_TeachParameter[i].IDistgnore, ignore);   
                                    if(bROIRes[i] == false)
                                    {
                                        double dCenterX = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointX;
                                        double dCenterY = InspCogFindLine.RunParams.ExpectedLineSegment.MidpointY;
                                        double dAngle = InspCogFindLine.RunParams.ExpectedLineSegment.Rotation;
                                        double dLenth = InspCogFindLine.RunParams.ExpectedLineSegment.Length;
                                        CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
                                        CogNGRectAffine.Color = CogColorConstants.Red;
                                        CogNGRectAffine.CenterX = dCenterX;
                                        CogNGRectAffine.CenterY = dCenterY;
                                        CogNGRectAffine.SideXLength = dLenth;
                                        CogNGRectAffine.SideYLength = 100;
                                        CogNGRectAffine.Rotation = dAngle;
                                        resultGraphics.Add(CogNGRectAffine);
                                        string LogMsg;
                                        LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
                                        LogMsg += "\n";
                                        List_NG.Items.Add(LogMsg);
                                        bRes = false;
                                        for (int k = 0; k < subresultGraphics.Count; k++)
                                        {
                                            resultGraphics.Add(subresultGraphics[k]);
                                        }
                                        continue;
                                    }                          
                                }
                            }
                            for (int k = 0; k < subresultGraphics.Count; k++)
                            {
                                resultGraphics.Add(subresultGraphics[k]);
                            }

                        }
                        else   //Circle
                        {
                            CogFindCircleTool InspCogCircleLine = new CogFindCircleTool();
                            InspCogCircleLine = m_TeachParameter[i].m_FindCircleTool;
                            double[] Result;
                            if (!GaloOppositeInspection(i, (int)enumROIType.Circle, InspCogCircleLine, (CogImage8Grey)PT_Display01.Image, out Result, ref resultGraphics, out ignore))
                            {
                                double dStartX = InspCogCircleLine.RunParams.ExpectedCircularArc.StartX;
                                double dStartY = InspCogCircleLine.RunParams.ExpectedCircularArc.StartY;
                                double dEndX = InspCogCircleLine.RunParams.ExpectedCircularArc.EndX;
                                double dEndY = InspCogCircleLine.RunParams.ExpectedCircularArc.EndY;

                                CogFindLineTool cogTempLine = new CogFindLineTool();
                                cogTempLine.RunParams.ExpectedLineSegment.StartX = dStartX;
                                cogTempLine.RunParams.ExpectedLineSegment.StartY = dStartY;
                                cogTempLine.RunParams.ExpectedLineSegment.EndX = dEndX;
                                cogTempLine.RunParams.ExpectedLineSegment.EndY = dEndY;

                                CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
                                CogNGRectAffine.Color = CogColorConstants.Red;
                                CogNGRectAffine.CenterX = cogTempLine.RunParams.ExpectedLineSegment.MidpointX;
                                CogNGRectAffine.CenterY = cogTempLine.RunParams.ExpectedLineSegment.MidpointY;
                                CogNGRectAffine.SideXLength = cogTempLine.RunParams.ExpectedLineSegment.Length;
                                CogNGRectAffine.SideYLength = 100;
                                CogNGRectAffine.Rotation = cogTempLine.RunParams.ExpectedLineSegment.Rotation;
                                resultGraphics.Add(CogNGRectAffine);
                                string LogMsg;
                                LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
                                LogMsg += "\n";
                                List_NG.Items.Add(LogMsg);
                                bRes = false;
                                continue;
                            }
                            bROIRes[i] = InspResultData(Result, m_TeachParameter[i].dSpecDistance, m_TeachParameter[i].dSpecDistanceMax, m_TeachParameter[i].IDistgnore, ignore);

                            if (bROIRes[i] == false)
                            {
                                double dStartX = InspCogCircleLine.RunParams.ExpectedCircularArc.StartX;
                                double dStartY = InspCogCircleLine.RunParams.ExpectedCircularArc.StartY;
                                double dEndX = InspCogCircleLine.RunParams.ExpectedCircularArc.EndX;
                                double dEndY = InspCogCircleLine.RunParams.ExpectedCircularArc.EndY;

                                CogFindLineTool cogTempLine = new CogFindLineTool();
                                cogTempLine.RunParams.ExpectedLineSegment.StartX = dStartX;
                                cogTempLine.RunParams.ExpectedLineSegment.StartY = dStartY;
                                cogTempLine.RunParams.ExpectedLineSegment.EndX = dEndX;
                                cogTempLine.RunParams.ExpectedLineSegment.EndY = dEndY;

                                CogRectangleAffine CogNGRectAffine = new CogRectangleAffine();
                                CogNGRectAffine.Color = CogColorConstants.Red;
                                CogNGRectAffine.CenterX = cogTempLine.RunParams.ExpectedLineSegment.MidpointX;
                                CogNGRectAffine.CenterY = cogTempLine.RunParams.ExpectedLineSegment.MidpointY;
                                CogNGRectAffine.SideXLength = cogTempLine.RunParams.ExpectedLineSegment.Length;
                                CogNGRectAffine.SideYLength = 100;
                                CogNGRectAffine.Rotation = cogTempLine.RunParams.ExpectedLineSegment.Rotation;
                                resultGraphics.Add(CogNGRectAffine);
                                string LogMsg;
                                LogMsg = string.Format("Inspection NG ROI:{0:D}", i); // 실제로 Mark를 못찾는지 확인하는 Log 뿌려줌 - cyh
                                LogMsg += "\n";
                                List_NG.Items.Add(LogMsg);
                                bRes = false;
                                continue;
                            }

                        }
                    }
                    ReultView(bRes, bROIRes, dDistance);
                    if (bRes == true)
                    {
                        CogGraphicLabel LabelText = new CogGraphicLabel();
                        LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                        LabelText.Color = CogColorConstants.Green;
                        LabelText.Text = "OK";
                        if (m_bROIFinealignFlag == true) //기능 ON/OFF 시 Overlay 위치 구분 shkang
                        {
                            if (Main.DEFINE.UNIT_TYPE == "VENT")
                            {
                                if (Main.ProjectInfo == "_1WELL_VENT")
                                {
                                    LabelText.X = 1500;
                                    LabelText.Y = 3100;
                                }
                                else
                                {
                                    LabelText.X = 500;
                                    LabelText.Y = 3100;
                                }
                            }
                            else if (Main.DEFINE.UNIT_TYPE == "PATH")
                            {
                                if (Main.ProjectInfo == "_1WELL_PATH")
                                {
                                    LabelText.X = 1000;
                                    LabelText.Y = 3000;
                                }
                                else
                                {
                                    LabelText.X = 2000;
                                    LabelText.Y = 3000;
                                }
                            }
                        }
                        else   //사용 X
                        {
                            if (Main.DEFINE.UNIT_TYPE == "VENT")
                            {
                                LabelText.X = 2000;
                                LabelText.Y = 1000;
                            }
                            else if (Main.DEFINE.UNIT_TYPE == "PATH")
                            {
                                LabelText.X = 0;
                                LabelText.Y = 900;
                            }
                        }

                        if (resultGraphics == null)
                            resultGraphics = new CogGraphicInteractiveCollection();
                        resultGraphics.Add(LabelText);
                    }
                    else
                    {
                        CogGraphicLabel LabelText = new CogGraphicLabel();
                        LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                        LabelText.Color = CogColorConstants.Red;
                        LabelText.Text = "NG";
                        if (m_bROIFinealignFlag == true) //기능 ON/OFF 시 Overlay 위치 구분 shkang
                        {
                            if (Main.DEFINE.UNIT_TYPE == "VENT")
                            {
                                if (Main.ProjectInfo == "_1WELL_VENT")
                                {
                                    LabelText.X = 1500;
                                    LabelText.Y = 3100;
                                }
                                else
                                {
                                    LabelText.X = 500;
                                    LabelText.Y = 3100;
                                }
                            }
                            else if (Main.DEFINE.UNIT_TYPE == "PATH")
                            {
                                if (Main.ProjectInfo == "_1WELL_PATH")
                                {
                                    LabelText.X = 1000;
                                    LabelText.Y = 3000;
                                }
                                else
                                {
                                    LabelText.X = 2000;
                                    LabelText.Y = 3000;
                                }
                            }
                        }
                        else
                        {
                            if (Main.DEFINE.UNIT_TYPE == "VENT")
                            {
                                LabelText.X = 2000;
                                LabelText.Y = 1000;

                            }
                            else if (Main.DEFINE.UNIT_TYPE == "PATH")
                            {
                                LabelText.X = 0;
                                LabelText.Y = 900;
                            }
                        }
                        if (resultGraphics == null)
                            resultGraphics = new CogGraphicInteractiveCollection();
                        resultGraphics.Add(LabelText);
                    }
                    //PT_Display01.Image.SelectedSpaceName = "@";
                    PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
                    resultGraphics.Clear();
                    GC.Collect();
                    Stopwatch.Stop();
                    Lab_Tact.Text = string.Format("{0:F3}", Stopwatch.Seconds);

                }

            }
            //             catch(System.Exception n) // cyh - 예외처리 메시지 띄우는거
            //             {
            //                 MessageBox.Show(n.ToString());
            //             }
            catch (Exception err)
            {
                resultGraphics.Clear();
                GC.Collect();

                string LogMsg;
                LogMsg = "Inspection Error = " + err.Message.ToString();
                MessageBox.Show(LogMsg);
            }


        }

        private bool InspResultData(double[] Dist, double SpecMin, double SpecMax, int SpecIgnore, int CurrentIgnor) // cyh - 매뉴얼시 데이터 나오는곳
        {
            bool Res = true;
            CurrentIgnor = 0;   //ignore 개수 초기화
            m_DistIgnoreCnt = CurrentIgnor;
            for (int i = 0; i < Dist.Length; i++)
            {
                if (Dist[i] > SpecMin && Dist[i] < SpecMax)
                {
                }
                else
                {
                    m_DistIgnoreCnt++;
                    if (SpecIgnore < m_DistIgnoreCnt)
                    {
                        Res = false;
                        return Res;
                    }
                    else
                        continue;
                }
            }

            return Res;
        }
        private void ReultView(bool Res, bool[] bROIResult, double[] bDist)
        {
            dataGridView_Result.Rows.Clear();
            string[] strData = new string[7];
            int Count = 0;
            if (Res == false)
            {
                for (int i = 0; i < bROIResult.Length; i++)
                {
                    if (bROIResult[i] == false)
                    {
                        Count++;
                        strData[0] = Count.ToString();
                        strData[1] = i.ToString();
                        strData[2] = "0";
                        strData[3] = "0";
                        strData[4] = "0";
                        strData[5] = "0";
                        strData[6] = string.Format("{0:F3}", bDist[i]);
                    }
                }
                dataGridView_Result.Rows.Add(strData);
            }
        }

        private PointF GetOriginGap()
        {
            PointF patternMatchingGap = new PointF();

            bool bSearchRes = Search_PATCNL();
            if (bSearchRes == true)
            {
                patternMatchingGap.X = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX);
                patternMatchingGap.Y = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY);
            }
            else
            {
                patternMatchingGap.X = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX);
                patternMatchingGap.Y = Convert.ToSingle(PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY);
                MessageBox.Show("AMP Module Mark NG!");
            }

            return patternMatchingGap;
        }

        private void TrackingTest(bool isTracking)
        {
            if (isTracking == true)
            {
                foreach (var item in m_TeachLine)
                {
                    item.RunParams.ExpectedLineSegment.StartX -= GetOriginGap().X;
                    item.RunParams.ExpectedLineSegment.StartY -= GetOriginGap().Y;
                    item.RunParams.ExpectedLineSegment.EndX -= GetOriginGap().X;
                    item.RunParams.ExpectedLineSegment.EndY -= GetOriginGap().Y;
                }
            }
            else
            {
                foreach (var item in m_TeachLine)
                {
                    item.RunParams.ExpectedLineSegment.StartX += GetOriginGap().X;
                    item.RunParams.ExpectedLineSegment.StartY += GetOriginGap().Y;
                    item.RunParams.ExpectedLineSegment.EndX += GetOriginGap().X;
                    item.RunParams.ExpectedLineSegment.EndY += GetOriginGap().Y;
                }
            }
        }
        
        private void btnAlignInspPos(object sender, EventArgs e)
        {
            RadioButton btn = (RadioButton)sender;
            PT_Display01.StaticGraphics.Clear();
            PT_Display01.InteractiveGraphics.Clear();
            int iAlignPos = Convert.ToInt32(btn.Tag.ToString());

            switch (iAlignPos)
            {
                case (int)enumAlignROI.Left1_1:
                    m_enumAlignROI = enumAlignROI.Left1_1;
                    btn_TOP_Inscription.BackColor = Color.Green;
                    btn_Top_Circumcription.BackColor = Color.DarkGray;
                    btn_Bottom_Inscription.BackColor = Color.DarkGray;
                    btn_Bottom_Circumcription.BackColor = Color.DarkGray;
                    m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_1];
                    break;

                case (int)enumAlignROI.Left1_2:
                    m_enumAlignROI = enumAlignROI.Left1_2;
                    btn_TOP_Inscription.BackColor = Color.DarkGray;
                    btn_Top_Circumcription.BackColor = Color.Green;
                    btn_Bottom_Inscription.BackColor = Color.DarkGray;
                    btn_Bottom_Circumcription.BackColor = Color.DarkGray;
                    m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Left1_2];
                    break;

                case (int)enumAlignROI.Right1_1:
                    m_enumAlignROI = enumAlignROI.Right1_1;
                    btn_TOP_Inscription.BackColor = Color.DarkGray;
                    btn_Top_Circumcription.BackColor = Color.DarkGray;
                    btn_Bottom_Inscription.BackColor = Color.Green;
                    btn_Bottom_Circumcription.BackColor = Color.DarkGray;
                    m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Right1_1];
                    break;

                case (int)enumAlignROI.Right1_2:
                    m_enumAlignROI = enumAlignROI.Right1_2;
                    btn_TOP_Inscription.BackColor = Color.DarkGray;
                    btn_Top_Circumcription.BackColor = Color.DarkGray;
                    btn_Bottom_Inscription.BackColor = Color.DarkGray;
                    btn_Bottom_Circumcription.BackColor = Color.Green;
                    m_TempTrackingLine = m_TeachLine[(int)enumAlignROI.Right1_2];
                    break;

            }

            Get_FindConerParameter();
            DrawRoiLine();
        }

        private void Get_FindConerParameter()
        {
            LAB_Align_Threshold.Text = m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold.ToString();
            LAB_Align_Caliper_Cnt.Text = m_TempTrackingLine.RunParams.NumCalipers.ToString();
            LAB_Align_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", m_TempTrackingLine.RunParams.CaliperProjectionLength);
            LAB_Align_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", m_TempTrackingLine.RunParams.CaliperSearchDirection);
            lab_Ignore.Text = m_TempTrackingLine.RunParams.NumToIgnore.ToString();
            int nPolarity = (int)m_TempTrackingLine.RunParams.CaliperRunParams.Edge0Polarity;
            Combo_Polarity3.SelectedIndex = nPolarity - 1;
            lblThetaFilterSizeValue.Text = m_TempTrackingLine.RunParams.CaliperRunParams.FilterHalfSizeInPixels.ToString();
        }
        private void Get_BlobParameter()
        {
            _PrePolyGon = null;
            dBlobPrevTranslationX = 0;
            dBlobPrevTranslationY = 0;
        }
        private void Align_Threshold(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dThr = Convert.ToDouble(LAB_Align_Threshold.Text);
            if (iUpdown == 0)
            {
                if (dThr == 255) return;
                dThr++;
            }
            else
            {
                if (dThr == 1) return;
                dThr--;
            }
            LAB_Align_Threshold.Text = dThr.ToString();
            m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold = dThr;
        }
        private void Align_ProjectionLenth(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dProjectionLenth = Convert.ToDouble(LAB_Align_CALIPER_PROJECTIONLENTH.Text);
            if (iUpdown == 0)
            {
                dProjectionLenth++;
            }
            else
            {
                if (dProjectionLenth == 1) return;
                dProjectionLenth--;
            }
            LAB_Align_CALIPER_PROJECTIONLENTH.Text = dProjectionLenth.ToString();
            m_TempTrackingLine.RunParams.CaliperProjectionLength = dProjectionLenth;
        }
        private void Align_CaliperCnt(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            int iCaliperCnt = Convert.ToInt32(LAB_Align_Caliper_Cnt.Text);
            if (iUpdown == 0)
            {
                iCaliperCnt++;
            }
            else
            {
                if (iCaliperCnt == 1) return;
                iCaliperCnt--;
            }
            LAB_Align_Caliper_Cnt.Text = iCaliperCnt.ToString();
            m_TempTrackingLine.RunParams.NumCalipers = iCaliperCnt;
        }
        private void Align_SearchLenth(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dSearchLenth = Convert.ToDouble(LAB_Align_CALIPER_SEARCHLENTH.Text);
            if (iUpdown == 0)
            {
                dSearchLenth++;
            }
            else
            {
                if (dSearchLenth == 1) return;
                dSearchLenth--;
            }
            LAB_Align_CALIPER_SEARCHLENTH.Text = dSearchLenth.ToString();
            m_TempTrackingLine.RunParams.CaliperSearchLength = dSearchLenth;
        }
        private void Align_Ignore(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            int iIgnore = Convert.ToInt32(lab_Ignore.Text);
            if (iUpdown == 0)
            {
                iIgnore++;
            }
            else
            {
                if (iIgnore == 1) return;
                iIgnore--;
            }
            lab_Ignore.Text = iIgnore.ToString();
            m_TempTrackingLine.RunParams.NumToIgnore = iIgnore;
        }

        private void LAB_Align_Caliper_Cnt_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_Caliper_Cnt.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int CaliperCnt = (int)KeyPad.m_data;

            m_TempTrackingLine.RunParams.NumCalipers = CaliperCnt;
            DrawRoiLine();

            LAB_Align_Caliper_Cnt.Text = CaliperCnt.ToString();
        }

        private void LAB_Align_Threshold_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_Threshold.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double dThreshold = KeyPad.m_data;

            m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold = dThreshold;

            LAB_Align_Threshold.Text = ((int)dThreshold).ToString();
        }

        private void LAB_Align_CALIPER_PROJECTIONLENTH_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_CALIPER_PROJECTIONLENTH.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double CaliperProjectionLenth = KeyPad.m_data;

            m_TempTrackingLine.RunParams.CaliperSearchLength = CaliperProjectionLenth;

            LAB_Align_CALIPER_PROJECTIONLENTH.Text = string.Format("{0:F3}", CaliperProjectionLenth);
        }

        private void LAB_Align_CALIPER_SEARCHLENTH_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_CALIPER_SEARCHLENTH.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double CaliperSearchLenth = KeyPad.m_data;

            m_TempTrackingLine.RunParams.CaliperSearchLength = CaliperSearchLenth;

            LAB_Align_CALIPER_SEARCHLENTH.Text = string.Format("{0:F3}", CaliperSearchLenth);
        }

        private void lab_Ignore_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LAB_Align_Caliper_Cnt.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int iIgnoreCnt = (int)KeyPad.m_data;

            m_TempTrackingLine.RunParams.NumToIgnore = iIgnoreCnt;
            DrawRoiLine();

            lab_Ignore.Text = iIgnoreCnt.ToString();
        }

        private void Combo_Polarity3_SelectedIndexChanged(object sender, EventArgs e)
        {
            CogCaliperPolarityConstants Polarity;
            int TempIndex = 0;
            if (m_enumROIType == enumROIType.Line)
            {
                TempIndex = Combo_Polarity3.SelectedIndex;
                Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
                m_TempTrackingLine.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            }
            else
            {
                TempIndex = Combo_Polarity3.SelectedIndex;
                Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
                m_TempTrackingLine.RunParams.CaliperRunParams.Edge0Polarity = Polarity;
            }
        }

        private void btn_align_roi_show_Click(object sender, EventArgs e)
        {
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            DrawRoiLine();
            return;
        }

        private void btn_AlginApply_Click(object sender, EventArgs e)
        {
            PT_Display01.StaticGraphics.Clear();
            PT_Display01.InteractiveGraphics.Clear();

            m_TeachLine[(int)m_enumAlignROI] = m_TempTrackingLine;
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
            PT_Display01.Image = OriginImage;
            bROIFinealignTeach = false;
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            resultGraphics.Clear();
            double dInspectionDistanceX;
            double[] dx = new double[4];
            double[] dy = new double[4];
            double Top_AlignX = 0, Top_AlignY = 0, Bottom_AlignX = 0, Bottom_AlignY = 0;

            chkUseTracking.Checked = false;
            bool bSearchRes = Search_PATCNL();
            if (bSearchRes == true)
            {
                double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                CogIntersectLineLineTool[] CrossPoint = new CogIntersectLineLineTool[2];
                CogLine[] Line = new CogLine[4];
                for (int i = 0; i < 4; i++)
                {
                    if (i < 2)
                    {
                        CrossPoint[i] = new CogIntersectLineLineTool();
                        CrossPoint[i].InputImage = (CogImage8Grey)PT_Display01.Image;
                    }
                    Line[i] = new CogLine();
                    m_TeachLine[i].InputImage = (CogImage8Grey)PT_Display01.Image;
                    double TempStartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX;
                    double TempStartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY;
                    double TempEndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX;
                    double TempEndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY;

                    double StartA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.StartX - TranslationX;
                    double StartA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.StartY - TranslationY;
                    double EndA_X = m_TeachLine[i].RunParams.ExpectedLineSegment.EndX - TranslationX;
                    double EndA_Y = m_TeachLine[i].RunParams.ExpectedLineSegment.EndY - TranslationY;

                    m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = StartA_X;
                    m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = StartA_Y;
                    m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = EndA_X;
                    m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = EndA_Y;

                    m_TeachLine[i].Run();

                    if (m_TeachLine[i].Results != null)
                    {
                        Line[i] = m_TeachLine[i].Results.GetLine();
                        //shkang_
                        if (Line[i] == null)
                            Line[i] = new CogLine();

                        if (i < 2)
                            Line[i].Color = CogColorConstants.Blue;
                        else
                            Line[i].Color = CogColorConstants.Orange;
                        resultGraphics.Add(Line[i]);
                    }
                    else
                    {
                        m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = TempStartA_X;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = TempStartA_Y;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = TempEndA_X;
                        m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = TempEndA_Y;
                        MessageBox.Show("Please CrossLine Checking");
                        return;
                    }

                    m_TeachLine[i].RunParams.ExpectedLineSegment.StartX = TempStartA_X;
                    m_TeachLine[i].RunParams.ExpectedLineSegment.StartY = TempStartA_Y;
                    m_TeachLine[i].RunParams.ExpectedLineSegment.EndX = TempEndA_X;
                    m_TeachLine[i].RunParams.ExpectedLineSegment.EndY = TempEndA_Y;
                }

                //shkang_s 
                //필름 밀림 융착 검사 (자재 X거리 검출)
                CogGraphicLabel LabelTest = new CogGraphicLabel();
                LabelTest.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                LabelTest.Color = CogColorConstants.Green;
                double dPixelResoultion = 13.36;
                dInspectionDistanceX = Line[3].X - Line[1].X;   //X 거리 검출
                dInspectionDistanceX = dInspectionDistanceX * dPixelResoultion / 1000;
                if (dObjectDistanceX + dObjectDistanceSpecX <= dInspectionDistanceX) //NG - Film NG
                {
                    LabelTest.X = 1000;
                    LabelTest.Y = 180;
                    LabelTest.Color = CogColorConstants.Red;
                    LabelTest.Text = string.Format("Film NG, X:{0:F3}", dInspectionDistanceX);
                }
                else   //OK - Film OK
                {
                    LabelTest.X = 1000;
                    LabelTest.Y = 180;
                    LabelTest.Color = CogColorConstants.Green;
                    LabelTest.Text = string.Format("Film OK, X:{0:F3}", dInspectionDistanceX);
                }
                resultGraphics.Add(LabelTest);
                //shkang_e

                CrossPoint[0].LineA = Line[0];
                CrossPoint[0].LineB = Line[1];
                CrossPoint[1].LineA = Line[2];
                CrossPoint[1].LineB = Line[3];
                for (int i = 0; i < 2; i++)
                {
                    CogGraphicLabel LineLabelTest = new CogGraphicLabel();
                    LineLabelTest.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                    LineLabelTest.Color = CogColorConstants.Green;

                    CrossPoint[i].Run();
                    if (CrossPoint[i] != null)
                    {
                        dCrossX[i] = CrossPoint[i].X;
                        dCrossY[i] = CrossPoint[i].Y;
                        if (i == 0)
                        {
                            LineLabelTest.X = 100;
                            LineLabelTest.Y = 100;
                            LineLabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
                        }
                        else
                        {
                            LineLabelTest.X = 100;
                            LineLabelTest.Y = 200;
                            LineLabelTest.Text = string.Format("Right Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
                        }
                        //resultGraphics.Add(LineLabelTest);
                    }
                }
                PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            }
        }

        private void Ignore_Distance(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int dIgnoredist = Convert.ToInt32(text_Dist_Ignre.Text);
            if (Convert.ToInt32(btn.Tag.ToString()) == 0)
            {
                //Up
                dIgnoredist++;
            }
            else
            {
                //Down
                if (dIgnoredist < 0) return;
                dIgnoredist--;
            }
            m_dDist_ignore = dIgnoredist;
            text_Dist_Ignre.Text = m_dDist_ignore.ToString();
        }

        private void text_Dist_Ignre_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(text_Dist_Ignre.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 100, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            int iIgnoreData = (int)KeyPad.m_data;
            m_dDist_ignore = iIgnoreData;
            text_Dist_Ignre.Text = m_dDist_ignore.ToString();
        }

        private void text_Spec_Dist_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(text_Spec_Dist.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 100, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dIgnoredist = KeyPad.m_data;
            m_SpecDist = dIgnoredist;
            text_Spec_Dist.Text = m_SpecDist.ToString();
        }

        private void chkUseLoadImageTeachMode_CheckedChanged(object sender, EventArgs e)
        {
            //20220903 YSH
            //해당 변수가 True 일 경우, Live Mode Off
            bLiveStop = chkUseLoadImageTeachMode.Checked;
        }
    
        private void Chk_All_Select_CheckedChanged(object sender, EventArgs e)
        {
            //if (Chk_All_Select.Checked == true)
            //    bAllSelect = true;
            //else
            //    bAllSelect = false;            
        }

        private void lab_Histogram_ROI_Count_Click(object sender, EventArgs e)
        {
            var stucTemp = m_TeachParameter[0];
            int iHistogramROICnt = Convert.ToInt32(lab_Histogram_ROI_Count.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 32, iHistogramROICnt, "Input Data", 0);
            KeyPad.ShowDialog();
            iHistogramROICnt = (int)KeyPad.m_data;
            lab_Histogram_ROI_Count.Text = iHistogramROICnt.ToString();
            m_iHistoramROICnt = iHistogramROICnt;
            stucTemp.iHistogramROICnt = m_iHistoramROICnt;
            m_TeachParameter[0] = stucTemp;
        }

        private void combo_Histogram_ROI_NO_SelectedIndexChanged(object sender, EventArgs e)
        {
            // _PrePointX = null;
            if (m_HistoROI != combo_Histogram_ROI_NO.SelectedIndex)
            {
                m_HistoROI = combo_Histogram_ROI_NO.SelectedIndex;

                PrevCenterX = 0;
                PrevCenterY = 0;
                PrevMarkX = 0;
                PrevMarkY = 0;
            }
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            m_HistoROI = combo_Histogram_ROI_NO.SelectedIndex;
            var TempBlob = m_TeachParameter[0];
            if (TempBlob.m_CogHistogramTool[m_HistoROI] == null)
                TempBlob.m_CogHistogramTool[m_HistoROI] = new CogHistogramTool();
            m_CogHistogramTool[m_HistoROI] = TempBlob.m_CogHistogramTool[m_HistoROI];
            Get_Histogram_Parameter();
            button8.PerformClick();
        }
        private void Get_Histogram_Parameter()
        {
            var Temp = m_TeachParameter[0];
            lab_Spec_GrayVale.Text = Temp.iHistogramSpec[m_HistoROI].ToString();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (chkUseRoiTracking.Checked == false)
                chkUseRoiTracking.Checked = true;
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            CogRectangleAffine ROIRect = new CogRectangleAffine();
            if (m_CogHistogramTool[m_HistoROI].Region == null)
            {
                ROIRect.SetCenterLengthsRotationSkew(100, 100, 200, 100, 0, 0);
                ROIRect.Color = CogColorConstants.Green;
                //ROIRect.GraphicDOFEnable = CogRectangleDOFConstants.Position | CogRectangleDOFConstants.Size;
                ROIRect.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                ROIRect.Interactive = true;
                m_CogHistogramTool[m_HistoROI].Region = ROIRect;
            }
            else
            {
                ROIRect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;
                ROIRect.Interactive = true;
                m_CogHistogramTool[m_HistoROI].Region = ROIRect;
            }

            m_CogHistogramTool[m_HistoROI].InputImage = PT_Display01.Image;
            m_CogHistogramTool[m_HistoROI].CurrentRecordEnable = CogHistogramCurrentRecordConstants.InputImage | CogHistogramCurrentRecordConstants.Region;
            
            Display.SetInteractiveGraphics(PT_Display01, m_CogHistogramTool[m_HistoROI].CreateCurrentRecord(), false);
            if (-1 < PT_Display01.InteractiveGraphics.ZOrderGroups.IndexOf(GraphicIndex ? "Result0" : "Result1"))
                PT_Display01.InteractiveGraphics.Remove(GraphicIndex ? "Result0" : "Result1");
        }

        private void lab_Spec_GrayVale_Click(object sender, EventArgs e)
        {
            var stucTemp = m_TeachParameter[0];
            int SpecGrayVal = Convert.ToInt32(lab_Spec_GrayVale.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(1, 255, SpecGrayVal, "Input Data", 0);
            KeyPad.ShowDialog();
            SpecGrayVal = (int)KeyPad.m_data;
            lab_Spec_GrayVale.Text = SpecGrayVal.ToString();
            stucTemp.iHistogramSpec[m_HistoROI] = SpecGrayVal;
            //stucTemp.iHistogramROICnt = m_iHistoramROICnt;
            m_TeachParameter[0] = stucTemp;
            //UpdateOverFusionCnt(m_iHistoramROICnt, 1);
        }

        private void btn_HistogramTest_Click(object sender, EventArgs e)
        {
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            CogRectangleAffine Rect = new CogRectangleAffine();
            if (PT_Display01.Image == null && m_CogHistogramTool[m_HistoROI] == null) return;
            PT_Display01.Image = OriginImage;
            bool bSearchRes = Search_PATCNL();
            if (bSearchRes == true)
            {
                double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                //CogRectangleAffine Rect = new CogRectangleAffine();
                // CogRectangleAffine Rect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;
                //double CenterX = Rect.CenterX;
                //double CenterY = Rect.CenterY;

                PT_Display01.Image = OriginImage;
                
                if (_useROITracking)
                {
                    if (!FinalTracking()) return;
                    TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
                    //m_CogHistogramTool[m_HistoROI] = m_TeachParameter[0].m_CogHistogramTool[m_HistoROI];
                    m_CogHistogramTool[m_HistoROI].InputImage = (CogImage8Grey)PT_Display01.Image;
                    Rect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;
                }
                else
                    Rect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;

                //m_CogHistogramTool[m_HistoROI] = m_TeachParameter[0].m_CogHistogramTool[m_HistoROI];
                m_CogHistogramTool[m_HistoROI].Run();
                if (m_CogHistogramTool[m_HistoROI].Result == null)
                {
                    MessageBox.Show("Histogram Result NG");
                    return;
                }
                //int GrayVal = m_CogHistogramTool[m_HistoROI].Result.Median;
                double GrayVal = m_CogHistogramTool[m_HistoROI].Result.Mean;
                int Spec = Convert.ToInt32(lab_Spec_GrayVale.Text);
                CogGraphicLabel result = new CogGraphicLabel();
                result.Font = new Font(Main.DEFINE.FontStyle, 15);
                result.X = Rect.CenterX;
                result.Y = Rect.CenterY;
                if (Spec > GrayVal)
                {
                    Rect.Color = CogColorConstants.Blue;
                    result.Color = CogColorConstants.Green;
                }
                else
                {
                    Rect.Color = CogColorConstants.Red;
                    result.Color = CogColorConstants.Red;
                }
                result.Text = string.Format("{0:F3}", GrayVal);
                PT_Display01.StaticGraphics.Add(result, "Result01");
                PT_Display01.StaticGraphics.Add(Rect, "Result01");
            }
        }

        private void btn_Histogram_Apply_Click(object sender, EventArgs e)
        {
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.StaticGraphics.Clear();
            if (_useROITracking)
            {
                CogRectangleAffine Rect = (CogRectangleAffine)m_CogHistogramTool[m_HistoROI].Region;
                double CenterX = Rect.CenterX + PrevMarkX;
                double CenterY = Rect.CenterY + PrevMarkY;
                //Rect.SetCenterWidthHeight(CenterX, CenterY, Rect.Width, Rect.Height);
                //m_CogHistogramTool[m_HistoROI].Region = Rect;
                //PrevCenterX = 0;
                //PrevCenterY = 0;
                //PrevMarkX = 0;
                //PrevMarkY = 0;
            }
            var Temp = m_TeachParameter[0];
            Temp.m_CogHistogramTool[m_HistoROI] = m_CogHistogramTool[m_HistoROI];
            Temp.iHistogramSpec[m_HistoROI] = Convert.ToInt32(lab_Spec_GrayVale.Text);
            m_TeachParameter[0].m_CogHistogramTool[m_HistoROI] = m_CogHistogramTool[m_HistoROI];
            m_TeachParameter[0].iHistogramSpec[m_HistoROI] = Temp.iHistogramSpec[m_HistoROI];
            m_bTrakingRootHisto[m_HistoROI] = true;
        }

        private void btn_Origin_Point_Apply_Click(object sender, EventArgs e)
        {
            LeftOrigin[0] = dCrossX[0];
            LeftOrigin[1] = dCrossY[0];
            RightOrigin[0] = dCrossX[1];
            RightOrigin[1] = dCrossY[1];
            lab_LeftOriginX.Text = LeftOrigin[0].ToString("F3");
            lab_LeftOriginY.Text = LeftOrigin[1].ToString("F3");
            lab_RightOriginX.Text = RightOrigin[0].ToString("F3");
            lab_RightOriginY.Text = RightOrigin[1].ToString("F3");
        }

        private void rdoSelectXYPosition_Click(object sender, EventArgs e)
        {
            PT_Display01.StaticGraphics.Clear();
            PT_Display01.InteractiveGraphics.Clear();

            RadioButton btn = sender as RadioButton;

            _bondingAlignPosition = (eBondingAlignPosition)Convert.ToInt32(btn.Tag.ToString());
            PT_Display01.Image = OriginImage;
            bool bSearchRes = Search_PATCNL();
            switch (_bondingAlignPosition)
            {
                case eBondingAlignPosition.X1:
                    ClickAlign_X1();
                    rdoAlignX1.BackColor = Color.DarkRed;
                    rdoAlignX2.BackColor = Color.DarkGray;
                    rdoAlignY1.BackColor = Color.DarkGray;
                    rdoAlignY2.BackColor = Color.DarkGray;
                    
                    if (bSearchRes == true)
                    {
                        double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                        double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                        TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
                    }
                    btnAlignShowROI.PerformClick();
                    break; 

                case eBondingAlignPosition.X2:
                    ClickAlign_X2();
                    rdoAlignX1.BackColor = Color.DarkGray;
                    rdoAlignX2.BackColor = Color.DarkRed;
                    rdoAlignY1.BackColor = Color.DarkGray;
                    rdoAlignY2.BackColor = Color.DarkGray;
                    
                    if (bSearchRes == true)
                    {
                        double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                        double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                        TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
                    }
                    btnAlignShowROI.PerformClick();
                    break;

                case eBondingAlignPosition.Y1:
                    ClickAlign_Y1();
                    rdoAlignX1.BackColor = Color.DarkGray;
                    rdoAlignX2.BackColor = Color.DarkGray;
                    rdoAlignY1.BackColor = Color.DarkRed;
                    rdoAlignY2.BackColor = Color.DarkGray;
                    
                    if (bSearchRes == true)
                    {
                        double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                        double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                        TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
                    }
                    btnAlignShowROI.PerformClick();
                    break;

                case eBondingAlignPosition.Y2:
                    ClickAlign_Y2();
                    rdoAlignX1.BackColor = Color.DarkGray;
                    rdoAlignX2.BackColor = Color.DarkGray;
                    rdoAlignY1.BackColor = Color.DarkGray;
                    rdoAlignY2.BackColor = Color.DarkRed;
                    
                    if (bSearchRes == true)
                    {
                        double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                        double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                        TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
                    }
                    btnAlignShowROI.PerformClick();
                    break;

                default:
                    break;
            }
            GetBondingAlignParameters();
        }



        private void GetBondingAlignParameters()
        {
            if (m_TempCaliperTool == null) return;
            lblAlignContrastThresholdValue.Text = m_TempCaliperTool.RunParams.ContrastThreshold.ToString();
            lblFilterSizeValue.Text = m_TempCaliperTool.RunParams.FilterHalfSizeInPixels.ToString();
            //lblScoresValue.Text = m_TempCaliperTool.RunParams.SingleEdgeScorers.ToString();   ???
            int nPolarity = (int)m_TempCaliperTool.RunParams.Edge0Polarity;
            cmbEdgePolarityType.SelectedIndex = nPolarity - 1;
        }

        private void ClickAlign_X1()
        {
            _bondingAlignPosition = eBondingAlignPosition.X1;
            if (m_TeachAlignLine[(int)_bondingAlignPosition] == null)
                m_TeachAlignLine[(int)_bondingAlignPosition] = new CogCaliperTool();

            m_TempCaliperTool = m_TeachAlignLine[(int)_bondingAlignPosition];
        }

        private void ClickAlign_X2()
        {
            _bondingAlignPosition = eBondingAlignPosition.X2;
            if (m_TeachAlignLine[(int)_bondingAlignPosition] == null)
                m_TeachAlignLine[(int)_bondingAlignPosition] = new CogCaliperTool();

            m_TempCaliperTool = m_TeachAlignLine[(int)_bondingAlignPosition];
        }

        private void ClickAlign_Y1()
        {
            _bondingAlignPosition = eBondingAlignPosition.Y1;
            if (m_TeachAlignLine[(int)_bondingAlignPosition] == null)
                m_TeachAlignLine[(int)_bondingAlignPosition] = new CogCaliperTool();
            m_TempCaliperTool = m_TeachAlignLine[(int)_bondingAlignPosition];
        }

        private void ClickAlign_Y2()
        {
            _bondingAlignPosition = eBondingAlignPosition.Y2;
            if (m_TeachAlignLine[(int)_bondingAlignPosition] == null)
                m_TeachAlignLine[(int)_bondingAlignPosition] = new CogCaliperTool();
            m_TempCaliperTool = m_TeachAlignLine[(int)_bondingAlignPosition];
        }

        private void cmbEdgePolarityType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CogCaliperPolarityConstants Polarity;
            int TempIndex = 0;
            TempIndex = cmbEdgePolarityType.SelectedIndex;
            Polarity = (CogCaliperPolarityConstants)(TempIndex + 1);
            m_TempCaliperTool.RunParams.Edge0Polarity = Polarity;
        }

        private void lblAlignContrastThresholdValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblAlignContrastThresholdValue.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(1, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            double dThreshold = KeyPad.m_data;

            m_TempCaliperTool.RunParams.ContrastThreshold = dThreshold;
            lblAlignContrastThresholdValue.Text = ((int)dThreshold).ToString();
        }

        private void BondingAlignThresholdUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dThr = Convert.ToDouble(lblAlignContrastThresholdValue.Text);
            if (iUpdown == 0)
            {
                if (dThr == 255) return;
                dThr++;
            }
            else
            {
                if (dThr == 1) return;
                dThr--;
            }
            lblAlignContrastThresholdValue.Text = dThr.ToString();
            m_TempCaliperTool.RunParams.ContrastThreshold = dThr;
        }

        private void lblFilterSizeValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblFilterSizeValue.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(2, 255, nCurData, "Input Data", 2);
            KeyPad.ShowDialog();
            double dFilterSize = KeyPad.m_data;

            m_TempCaliperTool.RunParams.FilterHalfSizeInPixels = Convert.ToInt32(dFilterSize);
            lblFilterSizeValue.Text = dFilterSize.ToString();
        }

        private void BondingAlignFilterSizeUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            int dFilterSize = Convert.ToInt32(lblFilterSizeValue.Text);
            if (iUpdown == 0)
            {
                if (dFilterSize == 255) return;
                dFilterSize++;
            }
            else
            {
                if (dFilterSize == 2) return;
                dFilterSize--;
            }
            lblFilterSizeValue.Text = dFilterSize.ToString();
            m_TempCaliperTool.RunParams.FilterHalfSizeInPixels = dFilterSize;
        }

        private void btnAlignApply_Click(object sender, EventArgs e)
        {
            //PT_Display01.StaticGraphics.Clear();
            //PT_Display01.InteractiveGraphics.Clear();

            m_TeachAlignLine[(int)_bondingAlignPosition] = m_TempCaliperTool;
        }

        private void lblOkDistanceValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblOkDistanceValueX.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 1000, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dOKDistance = KeyPad.m_data;

            lblOkDistanceValueX.Text = dOKDistance.ToString();
            dBondingAlignOriginDistX = Convert.ToDouble(lblOkDistanceValueX.Text);
        }

        private void lblOkDistanceValueY_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblOkDistanceValueY.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 1000, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dOKDistance = KeyPad.m_data;

            lblOkDistanceValueY.Text = dOKDistance.ToString();
            dBondingAlignOriginDistY = Convert.ToDouble(lblOkDistanceValueY.Text);
        }

        private void lblAlignSpecValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblAlignSpecValueX.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 1000, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dSpec = KeyPad.m_data;

            lblAlignSpecValueX.Text = dSpec.ToString();
            dBondingAlignDistSpecX = Convert.ToDouble(lblAlignSpecValueX.Text);
        }

        private void lblAlignSpecValueY_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblAlignSpecValueY.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 1000, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dSpec = KeyPad.m_data;

            lblAlignSpecValueY.Text = dSpec.ToString();
            dBondingAlignDistSpecY = Convert.ToDouble(lblAlignSpecValueY.Text);
        }

        private void btnAlignShowROI_Click(object sender, EventArgs e)
        {
            TrackingCaliperROI();
        }

        private void btnAlignTest_Click(object sender, EventArgs e)
        {
            BondingAlignTest();
        }

        private void BondingAlignTest()
        {
            PT_Display01.StaticGraphics.Clear();
            PT_Display01.InteractiveGraphics.Clear();

            CogCaliperTool CaliperTool = new CogCaliperTool();

            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            CaliperTool.InputImage = (CogImage8Grey)PT_Display01.Image;
            CaliperTool.RunParams.ContrastThreshold = m_TempCaliperTool.RunParams.ContrastThreshold;
            CaliperTool.RunParams.FilterHalfSizeInPixels = m_TempCaliperTool.RunParams.FilterHalfSizeInPixels;
            CaliperTool.RunParams.Edge0Polarity = m_TempCaliperTool.RunParams.Edge0Polarity;

            CaliperTool.Region = m_TempCaliperTool.Region;
            CaliperTool.Run();

            if (CaliperTool.Results != null && CaliperTool.Results.Count > 0)
            {
                resultGraphics.Add(CaliperTool.Results[0].CreateResultGraphics(CogCaliperResultGraphicConstants.Edges));
                Console.WriteLine(CaliperTool.Results[0].Edge0.PositionX);
            }
            else
            {

            }

            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
        }

        private void lblBondingAlignTest_Click(object sender, EventArgs e)
        {
            //bROIFinealignTeach = false;
            //Live Mode On상태일 시, Off로 변경
            if (BTN_LIVEMODE.Checked)
            {
                BTN_LIVEMODE.Checked = false;
                BTN_LIVEMODE.BackColor = Color.DarkGray;
            }
            FinalTracking();
        }
        private bool FinalTracking()
        {
            bool bRes = true;
            double dGapX = new double();
            double dGapY = new double();
            double dGapT = new double();
            double dGapT_degree = new double();

            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();
            //ROIFinealign 사용 시 ROIFinealign만 사용
            if (m_bROIFinealignFlag)
            {
                PT_Display01.Image = OriginImage;
                bool bSearchRes = Search_PATCNL();
                if (bSearchRes == true)
                {
                    double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                    double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
                    //m_bROIFinealignFlag = true 일때, FixureImage 미사용
                    //Film Size 측정용도로 사용
                    bRes = TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
                    if (bRes)
                    {
                        bRes = Main.AlignUnit[m_AlignNo].ROIFinealign(FinealignMark, OriginImage, out dGapX, out dGapY, out dGapT, ref resultGraphics);
                        if (!bRes)
                        {
                            MessageBox.Show("ROI FineAlign Fail");
                            return bRes;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Material Align Fail");
                        return bRes;
                    }

                }
                //2023 0228 YSH Finealign Spec 부분 추후 필요 시 수정 요망
                dGapT_degree = dGapT * 180 / Math.PI;
                if (-m_dROIFinealignT_Spec < dGapT_degree && dGapT_degree < m_dROIFinealignT_Spec)//Spec Check
                {
                    CogFixtureTool mCogFixtureTool = new CogFixtureTool();
                    mCogFixtureTool.InputImage = PT_Display01.Image;//TrackingROI() 결과 Fixure 이미지
                    CogTransform2DLinear TempData = new CogTransform2DLinear();
                    TempData.TranslationX = dGapX;
                    TempData.TranslationY = dGapY;
                    TempData.Rotation = dGapT;
                    m_dTempFineLineAngle = dGapT;
                    Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].TempFixtureTrans = TempData;
                    mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
                    mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
                    mCogFixtureTool.Run();
                    //TempTrackingImage =(CogImage8Grey) mCogFixtureTool.OutputImage;
                    PT_Display01.Image = (CogImage8Grey)mCogFixtureTool.OutputImage;
                }
                else
                {
                    MessageBox.Show("ROI Finealign Theta Spec Out");
                    bRes = false;
                }
            }
            else//ROIFinealign 미사용 시 BondingAlign 사용
            {
                bRes = BondingAlignInspectionTest(out dGapX, out dGapY);
                if (!bRes)
                    return bRes;

                CogFixtureTool mCogFixtureTool = new CogFixtureTool();
                mCogFixtureTool.InputImage = PT_Display01.Image;
                CogTransform2DLinear TempData = new CogTransform2DLinear();
                if (Main.DEFINE.UNIT_TYPE == "VENT")
                {
                    TempData.TranslationX = dGapX;
                    TempData.TranslationY = dGapY;
                }
                if (Main.DEFINE.UNIT_TYPE == "PATH")
                {
                    TempData.TranslationX = -dGapX;
                    TempData.TranslationY = dGapY;
                }

                mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
                mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
                mCogFixtureTool.Run();
                PT_Display01.Image = (CogImage8Grey)mCogFixtureTool.OutputImage;
            }

            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            return bRes;
        }
        private bool BondingAlignInspectionTest(out double dFinalTrackingX, out double dFinalTrackingY)
        {
            double dPixelResolution = 13.36;
            bool bRes = true;
            CogGraphicLabel LabelTest = new CogGraphicLabel();
            LabelTest.Font = new Font(Main.DEFINE.FontStyle, 15, FontStyle.Bold);


            dFinalTrackingX = new double();
            dFinalTrackingY = new double();
            PT_Display01.StaticGraphics.Clear();
            PT_Display01.InteractiveGraphics.Clear();
            PT_Display01.Image = OriginImage;
            bool bSearchRes = Search_PATCNL();
            if (bSearchRes == true)
            {
                double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
                double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;

                //TrackingROI(TranslationX, TranslationY);
                TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
                //if(!TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY)) return false;     
                CogGraphicInteractiveCollection[] resultGraphics = new CogGraphicInteractiveCollection[4];

                for (int i = 0; i < 4; i++)
                {
                    resultGraphics[i] = new CogGraphicInteractiveCollection();
                    m_TeachAlignLine[i].InputImage = PT_Display01.Image;
                    m_TeachAlignLine[i].Run();

                    if (m_TeachAlignLine[i].Results != null && m_TeachAlignLine[i].Results.Count > 0)
                    {
                        resultGraphics[i].Add(m_TeachAlignLine[i].Results[0].CreateResultGraphics(CogCaliperResultGraphicConstants.Edges));
                    }
                    else
                    {
                        bRes = false;
                        dFinalTrackingX = 0;
                        dFinalTrackingY = 0;
                        return bRes;
                    }
                    PT_Display01.InteractiveGraphics.AddList(resultGraphics[i], "RESULT", false);
                }
                //Calculation Bonding Align X,Y
                double dBonding_AlignX = 0;
                double dBonding_AlignY = 0;
                dBonding_AlignX = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.X2].Results[0].Edge0.PositionX - m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionX);
                dBonding_AlignY = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.Y2].Results[0].Edge0.PositionY - m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionY);

                dBonding_AlignX = dBonding_AlignX * dPixelResolution / 1000;
                dBonding_AlignY = dBonding_AlignY * dPixelResolution / 1000;

                dBonding_AlignX = Convert.ToDouble(lblOkDistanceValueX.Text.ToString()) - dBonding_AlignX;
                dBonding_AlignY = Convert.ToDouble(lblOkDistanceValueY.Text.ToString()) - dBonding_AlignY;
                dFinalTrackingX = dBonding_AlignX / dPixelResolution * 1000;
                dFinalTrackingY = dBonding_AlignY / dPixelResolution * 1000;


                double dCheckDistX, dCheckDistY;
                dCheckDistX = Math.Abs(dFinalTrackingX);
                dCheckDistY = Math.Abs(dFinalTrackingY);
                dCheckDistX = dCheckDistX * dPixelResolution / 1000;
                dCheckDistY = dCheckDistY * dPixelResolution / 1000;

                //Overlay 추가
                if (dBondingAlignDistSpecX > dCheckDistX && dBondingAlignDistSpecY > dCheckDistY)   // OK
                {
                    LabelTest.X = 5000;
                    LabelTest.Y = -6000;
                    LabelTest.Text = string.Format("Bonding Align OK, X: {0:F3} Y: {1:F3}", dCheckDistX, dCheckDistY);
                    LabelTest.Color = CogColorConstants.Green;
                    // LabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
                }
                else   // NG
                {
                    bRes = false;
                    if (dBondingAlignDistSpecX < dCheckDistX)
                    {
                        LabelTest.X = 1500;
                        LabelTest.Y = 100;
                        LabelTest.Text = string.Format("Bonding Align NG, X: {0:F2}", dCheckDistX);
                        LabelTest.Color = CogColorConstants.Red;
                        CogRectangle resultRect = new CogRectangle();
                        resultRect.X = m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionX;
                        resultRect.Y = m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionY - 150;
                        resultRect.Width = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.X2].Results[0].Edge0.PositionX - m_TeachAlignLine[(int)eBondingAlignPosition.X1].Results[0].Edge0.PositionX);
                        resultRect.Height = 300;
                        resultRect.Color = CogColorConstants.Red;
                        PT_Display01.InteractiveGraphics.Add(resultRect, "Result", false);
                    }
                    if (dBondingAlignDistSpecY < dCheckDistY)
                    {
                        LabelTest.X = 1500;
                        LabelTest.Y = 200;
                        LabelTest.Text = string.Format("Bonding Align NG, Y: {0:F2}", dCheckDistY);
                        LabelTest.Color = CogColorConstants.Red;
                        CogRectangle resultRect = new CogRectangle();
                        resultRect.X = m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionX;
                        resultRect.Y = m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionY;
                        resultRect.Width = 100;
                        resultRect.Height = Math.Abs(m_TeachAlignLine[(int)eBondingAlignPosition.Y2].Results[0].Edge0.PositionX - m_TeachAlignLine[(int)eBondingAlignPosition.Y1].Results[0].Edge0.PositionX);
                        resultRect.Color = CogColorConstants.Red;
                        PT_Display01.InteractiveGraphics.Add(resultRect, "Result", false);
                        //LabelTest.Text = string.Format("Left Position X:{0:F3}, Y:{1:F3}", dCrossX[i], dCrossY[i]);
                    }
                }
                PT_Display01.InteractiveGraphics.Add(LabelTest, "Result", false);
            }
            return bRes;
        }

        private void lblParamFilterSizeValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblParamFilterSizeValue.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(1, 255, nCurData, "Input Data", 2);
            KeyPad.ShowDialog();
            int FilterSize = (int)KeyPad.m_data;
            
            if (enumROIType.Line == m_enumROIType)
            {
                m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FilterSize;
                FindLineROI();
            }
            else
            {
                m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = FilterSize;
                CircleROI();
            }
            lblParamFilterSizeValue.Text = FilterSize.ToString();
        }

        private void lblParamFilterSizeValueUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            int dFilterSize = Convert.ToInt32(lblParamFilterSizeValue.Text);
            if (iUpdown == 0)
            {
                if (dFilterSize == 255) return;
                dFilterSize++;
            }
            else
            {
                if (dFilterSize == 2) return;
                dFilterSize--;
            }
            lblParamFilterSizeValue.Text = dFilterSize.ToString();
            if (enumROIType.Line == m_enumROIType)
            {
                m_TempFindLineTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
                FindLineROI();
            }
            else
            {
                m_TempFindCircleTool.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
                CircleROI();
            }
        }

        private void text_Spec_Dist_Max_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(text_Spec_Dist_Max.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 100, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dIgnoredist = KeyPad.m_data;
            m_SpecDistMax = dIgnoredist;
            text_Spec_Dist_Max.Text = m_SpecDistMax.ToString();
        }

        private void lblEdgeDirection_Click(object sender, EventArgs e)
        {
            if (m_enumROIType == enumROIType.Line)
            {
                m_TempFindLineTool.RunParams.CaliperSearchDirection *= (-1);
            }
            else
            {
                CogFindCircleSearchDirectionConstants DirType = m_TempFindCircleTool.RunParams.CaliperSearchDirection;
                if (DirType == CogFindCircleSearchDirectionConstants.Inward)
                    m_TempFindCircleTool.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Outward;
                else
                    m_TempFindCircleTool.RunParams.CaliperSearchDirection = CogFindCircleSearchDirectionConstants.Inward;
            }
        }

        private void lblObjectDistanceXSpecValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblObjectDistanceXSpecValue.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 3000, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dDistanceSpecX = KeyPad.m_data;

            lblObjectDistanceXSpecValue.Text = dDistanceSpecX.ToString();
            dObjectDistanceSpecX = Convert.ToDouble(lblObjectDistanceXSpecValue.Text);
        }

        private void lblObjectDistanceXValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblObjectDistanceXValue.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 3000, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dDistanceX = KeyPad.m_data;

            lblObjectDistanceXValue.Text = dDistanceX.ToString();
            dObjectDistanceX = Convert.ToDouble(lblObjectDistanceXValue.Text);
        }

        private void lblThetaFilterSizeValue_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblThetaFilterSizeValue.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int dFilterSize = Convert.ToInt32(KeyPad.m_data);

            lblThetaFilterSizeValue.Text = Convert.ToString(dFilterSize);
            m_TempTrackingLine.RunParams.CaliperRunParams.FilterHalfSizeInPixels = dFilterSize;
        }

        private void lblThetaFilterSizeValueUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dFilterSize = Convert.ToDouble(lblThetaFilterSizeValue.Text);
            if (iUpdown == 0)
            {
                if (dFilterSize == 255) return;
                dFilterSize++;
            }
            else
            {
                if (dFilterSize == 2) return;
                dFilterSize--;
            }
            lblThetaFilterSizeValue.Text = dFilterSize.ToString();
            m_TempTrackingLine.RunParams.CaliperRunParams.ContrastThreshold = dFilterSize;
        }

        private void RDB_ALIGN_TEACH_MODE_Click(object sender, EventArgs e)
        {
            RadioButton TempRadioButton = (RadioButton)sender;


            switch (Convert.ToInt32(TempRadioButton.Tag))
            {
                case 1://자재 얼라인 
                    PANEL_MATERIAL_ALIGN.Visible = true;
                    PANEL_MATERIAL_ALIGN.Location = new System.Drawing.Point(5, 60);
                    PANEL_MATERIAL_ALIGN.Size = new Size(897, 615);
                    PANEL_BONDING_ALIGN.Visible = false;
                    PANEL_ROI_FINEALIGN.Visible = false;
                    TempRadioButton.BackColor = Color.LimeGreen;
                    RDB_BONDING_ALIGN.BackColor = Color.DarkGray;
                    RDB_ROI_FINEALIGN.BackColor = Color.DarkGray;
                    chkUseTracking.Visible = true;
                    break;

                case 2://본딩 얼라인
                    PANEL_MATERIAL_ALIGN.Visible = false;
                    PANEL_BONDING_ALIGN.Visible = true;
                    PANEL_BONDING_ALIGN.Location = new System.Drawing.Point(5, 60);
                    PANEL_BONDING_ALIGN.Size = new Size(740, 258);
                    PANEL_ROI_FINEALIGN.Visible = false;
                    TempRadioButton.BackColor = Color.LimeGreen;
                    RDB_MATERIAL_ALIGN.BackColor = Color.DarkGray;
                    RDB_ROI_FINEALIGN.BackColor = Color.DarkGray;
                    chkUseTracking.Visible = true;
                    break;

                case 3://ROI Fine얼라인             
                    PANEL_MATERIAL_ALIGN.Visible = false;
                    PANEL_BONDING_ALIGN.Visible = false;
                    PANEL_ROI_FINEALIGN.Visible = true;
                    PANEL_ROI_FINEALIGN.Location = new System.Drawing.Point(5, 60);
                    PANEL_ROI_FINEALIGN.Size = new Size(740, 206);
                    TempRadioButton.BackColor = Color.LimeGreen;
                    RDB_MATERIAL_ALIGN.BackColor = Color.DarkGray;
                    RDB_BONDING_ALIGN.BackColor = Color.DarkGray;
                    m_TempCaliperTool = new CogCaliperTool();
                    chkUseTracking.Visible = false;
                    break;

                default:
                    break;
            }
        }

        private void BTN_RETURNPAGE_Click(object sender, EventArgs e)
        {
            bROIFinealignTeach = false;
            TABC_MANU.SelectTab(TAB_06);
            m_PatNo_Sub = 0;
            RDB_ROI_FINEALIGN.PerformClick();
        }

        private void BTN_ROI_FINEALIGN_TEST_Click(object sender, EventArgs e)
        {
            bool bRet;
            double dGapX;
            double dGapY;
            double dGapT;
            double dGapT_degree;
            CogGraphicInteractiveCollection resultGraphics = new CogGraphicInteractiveCollection();

            //bool bSearchRes = Search_PATCNL();
            CogGraphicLabel LabelText = new CogGraphicLabel();

            //if (bSearchRes == true)
            //{
            //    double TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            //    double TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
            //    TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
            //}
            //else
            //{
            //    LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
            //    LabelText.Color = CogColorConstants.Red;
            //    if (Main.DEFINE.UNIT_TYPE == "VENT")
            //    {
            //        LabelText.X = 2000;
            //        LabelText.Y = 1000;
            //    }
            //    else if (Main.DEFINE.UNIT_TYPE == "PATH")
            //    {
            //        LabelText.X = 0;
            //        LabelText.Y = 900;
            //    }
            //    LabelText.Text = "Please Material Align and Bonding Align Check!!";
            //    resultGraphics.Add(LabelText);
            //    PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
            //    return;
            //}

            PT_Display01.StaticGraphics.Clear();
            PT_Display01.InteractiveGraphics.Clear();
            bRet = Main.AlignUnit[m_AlignNo].ROIFinealign(FinealignMark, OriginImage, out dGapX, out dGapY, out dGapT, ref resultGraphics);
            if (bRet)
            {
                if (Main.DEFINE.UNIT_TYPE == "VENT")
                {
                    LabelText.X = 2000;
                    LabelText.Y = 1000;
                }
                else if (Main.DEFINE.UNIT_TYPE == "PATH")
                {
                    LabelText.X = 1800;
                    LabelText.Y = 900;
                }

                //2023 0228 YSH Finealign Spec 부분 추후 필요 시 수정 요망
                dGapT_degree = dGapT * 180 / Math.PI;
                if (-m_dROIFinealignT_Spec < dGapT_degree && dGapT_degree < m_dROIFinealignT_Spec)//Spec Check
                {
                    CogFixtureTool mCogFixtureTool = new CogFixtureTool();
                    mCogFixtureTool.InputImage = OriginImage;
                    CogTransform2DLinear TempData = new CogTransform2DLinear();
                    TempData.TranslationX = dGapX;
                    TempData.TranslationY = dGapY;
                    TempData.Rotation = dGapT;
                    mCogFixtureTool.RunParams.UnfixturedFromFixturedTransform = TempData;
                    mCogFixtureTool.RunParams.FixturedSpaceNameDuplicateHandling = CogFixturedSpaceNameDuplicateHandlingConstants.Compatibility;
                    mCogFixtureTool.Run();
                    PT_Display01.Image = mCogFixtureTool.OutputImage;
                    LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                    LabelText.Color = CogColorConstants.Green;
                    LabelText.Text = "ROI FINEALIGN OK";
                    //현재 이미지 Theta 값
                    LBL_ROI_FINEALIGN_CURRENT_T.ForeColor = Color.Green;
                    LBL_ROI_FINEALIGN_CURRENT_T.Text = string.Format("{0:F3}°", dGapT_degree);


                }
                else
                {
                    LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                    LabelText.Color = CogColorConstants.Red;
                    LabelText.Text = "ROI FINEALIGN SPEC OUT";
                    //현재 이미지 Theta 값
                    LBL_ROI_FINEALIGN_CURRENT_T.ForeColor = Color.Red;
                    LBL_ROI_FINEALIGN_CURRENT_T.Text = string.Format("{0:F3}°", dGapT_degree);
                }
                resultGraphics.Add(LabelText);

            }
            else
            {
                LabelText.Font = new Font(Main.DEFINE.FontStyle, 20, FontStyle.Bold);
                LabelText.Color = CogColorConstants.Red;
                if (Main.DEFINE.UNIT_TYPE == "VENT")
                {
                    LabelText.X = 2000;
                    LabelText.Y = 1000;
                }
                else if (Main.DEFINE.UNIT_TYPE == "PATH")
                {
                    LabelText.X = 1800;
                    LabelText.Y = 900;
                }
                LabelText.Text = "ROI FINEALIGN FAIL!";
                resultGraphics.Add(LabelText);
            }
            PT_Display01.InteractiveGraphics.AddList(resultGraphics, "RESULT", false);
        }

        private void BTN_ROI_FINEALIGN_Click(object sender, EventArgs e)
        {
            Button TempBtn = (Button)sender;
            //PT_Display01.StaticGraphics.Clear();
            //PT_Display01.InteractiveGraphics.Clear();
            //PT_Display01.Image = OriginImage;
            
            if (TempBtn.Name.Equals("BTN_ROI_FINEALIGN_LEFTMARK"))
                nROIFineAlignIndex = (int)enumROIFineAlignPosition.Left;
            else
                nROIFineAlignIndex = (int)enumROIFineAlignPosition.Right;

            bROIFinealignTeach = true;
            TABC_MANU.SelectedIndex = 0;
        }

        private void LBL_ROI_FINEALIGN_SPEC_T_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(LBL_ROI_FINEALIGN_SPEC_T.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 6, nCurData, "Input Data", 0);
            KeyPad.ShowDialog();
            double dTheta = KeyPad.m_data;

            m_dROIFinealignT_Spec = dTheta;

            LBL_ROI_FINEALIGN_SPEC_T.Text = dTheta.ToString();
        }

        private void CMB_USE_ROIFINEALIGN_CheckedChanged(object sender, EventArgs e)
        {
            m_bROIFinealignFlag = CMB_USE_ROIFINEALIGN.Checked;
            //2023 0228 YSH
            //ROI Finealign 기능사용시엔 Bonding얼라인 미사용
            //ROI Finealign 기능미사용시엔 Bonding얼라인 사용
            if (m_bROIFinealignFlag)
                RDB_BONDING_ALIGN.Visible = true;
            else
                RDB_BONDING_ALIGN.Visible = true;
        }

        private void BTN_LIVEMODE_Click(object sender, EventArgs e)
        {
            if (BTN_LIVEMODE.Checked)
            {
                timer1.Enabled = true;  //Live On
                bLiveStop = false;
                //PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
                BTN_LIVEMODE.BackColor = Color.LimeGreen;
            }
            else
            {
                timer1.Enabled = false;  //Live Off
                bLiveStop = true;
                BTN_LIVEMODE.BackColor = Color.DarkGray;
            }
            DisplayClear();
            Main.DisplayRefresh(PT_Display01);
        }

        private void CHK_ROI_CREATE_CheckedChanged(object sender, EventArgs e)
        {
            bROICopy = CHK_ROI_CREATE.Checked;
            if (bROICopy)
                for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = false;
            else
                for (int i = 0; i < BTN_TOOLSET.Count; i++) BTN_TOOLSET[i].Visible = true;
        }
        private string RemoveSourceCode(string input)
        {
            int lastIndex = input.LastIndexOf('\\');
            if (lastIndex != -1)
            {
                return input.Substring(0, lastIndex);
            }
            return input;
        }

        private void btnImagePrev_Click(object sender, EventArgs e)
        {
            btnImagePrev.Enabled = false;
            if (CurrentImageNumber < 0) return;
            string[] files;
            if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
            {
                files = Directory.GetFiles(CurrentFolderPath, "*UP.jpg");
            }
            else
            {
                files = Directory.GetFiles(CurrentFolderPath, "*.bmp");
            }

            if (CurrentImageNumber < files.Length)
            {
                if (CurrentImageNumber != 0) CurrentImageNumber--;
                else
                {
                    MessageBox.Show("First Image!!");
                    btnImagePrev.Enabled = true;
                    return;
                }
                string FileName = "";

                FileName = files[CurrentImageNumber];

                //ICogImage RefCogImage = null;
                if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
                {
                    if (FileName != "")
                    {
                        if (Main.vision.CogImgTool[m_CamNo] == null)
                            Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
                        Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
                        CogImageConvertTool img = new CogImageConvertTool();
                        img.InputImage = Main.vision.CogImgTool[m_CamNo].OutputImage;
                        img.Run();
                        Main.vision.CogCamBuf[m_CamNo] = img.OutputImage;
                        //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
                    }
                }
                else
                {
                    if (FileName != "")
                    {
                        if (Main.vision.CogImgTool[m_CamNo] == null)
                            Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
                        Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
                        Main.vision.CogCamBuf[m_CamNo] = Main.vision.CogImgTool[m_CamNo].OutputImage;
                        //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
                    }
                }
                PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
                OriginImage = (CogImage8Grey)Main.vision.CogCamBuf[m_CamNo];
                DisplayClear();
                Main.DisplayRefresh(PT_Display01);
            }

            //검사
            btn_Inspection_Test.PerformClick();
            btnImagePrev.Enabled = true;
        }

        private void btnImageNext_Click(object sender, EventArgs e)
        {
            btnImageNext.Enabled = false;
            if (CurrentImageNumber < 0) return;
            string[] files;
            if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
            {
                files = Directory.GetFiles(CurrentFolderPath, "*UP.jpg");
            }
            else
            {
                files = Directory.GetFiles(CurrentFolderPath, "*.bmp");
            }

            if (CurrentImageNumber < files.Length - 1)
            {
                CurrentImageNumber++;
                string FileName = "";

                FileName = files[CurrentImageNumber];

                //ICogImage RefCogImage = null;
                //shkang_s
                if (openFileDialog1.SafeFileName.Substring(openFileDialog1.SafeFileName.Length - 3) == "jpg")
                {
                    if (FileName != "")
                    {
                        if (Main.vision.CogImgTool[m_CamNo] == null)
                            Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
                        Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
                        CogImageConvertTool img = new CogImageConvertTool();
                        img.InputImage = Main.vision.CogImgTool[m_CamNo].OutputImage;
                        img.Run();
                        Main.vision.CogCamBuf[m_CamNo] = img.OutputImage;
                        //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
                    }
                }
                else
                {
                    if (FileName != "")
                    {
                        if (Main.vision.CogImgTool[m_CamNo] == null)
                            Main.vision.CogImgTool[m_CamNo] = new CogImageFileTool();
                        Main.GetImageFile(Main.vision.CogImgTool[m_CamNo], FileName);
                        Main.vision.CogCamBuf[m_CamNo] = Main.vision.CogImgTool[m_CamNo].OutputImage;
                        //Main.vision.CogCamBuf[m_CamNo] = RefCogImage;
                    }
                }
                PT_Display01.Image = Main.vision.CogCamBuf[m_CamNo];
                OriginImage = (CogImage8Grey)Main.vision.CogCamBuf[m_CamNo];
                DisplayClear();
                Main.DisplayRefresh(PT_Display01);
            }
            else
            {
                MessageBox.Show("Last Image!!");
                btnImageNext.Enabled = true;
                return;
            }
            //검사
            btn_Inspection_Test.PerformClick();
            btnImageNext.Enabled = true;
        }

        private void chkUseInspDirectionChange_CheckedChanged(object sender, EventArgs e)
        {
            m_bInspDirectionChange = chkUseInspDirectionChange.Checked;
        }

        private void lblEdgeThreshold_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblEdgeThreshold.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int nEdgeThreshold = (int)KeyPad.m_data;

            lblEdgeThreshold.Text = nEdgeThreshold.ToString();
            m_TeachParameter[m_iGridIndex].iThreshold = nEdgeThreshold;
        }

        private void chkUseEdgeThreshold_Click(object sender, EventArgs e)
        {
            UpdateParamUI();
            btn_Param_Apply.PerformClick();
        }

         //shkang ChangeParaLog Test
        private void Save_ChangeParaLog(string nMessage, string paraName, double oldPara, double newPara, string nType)
        {
            string nFolder;
            string nFileName = "";
            nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            string Date;
            Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            nMessage = nMessage + "_" + "INSPECTION" + (m_AlignNo + 1) + "_" + "PANEL" + (m_PatTagNo + 1) + "_";
            nMessage = nMessage + paraName + "_";
            nMessage = nMessage +  oldPara + "->" + newPara;

            lock (syncLock_Log)
            {
                try
                {
                    switch (nType)
                    {
                        case Main.DEFINE.CHANGEPARA:
                            nFileName = "ChangeParaLog.txt";
                            nMessage = Date + nMessage;
                            break;
                    }

                    StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
                    SW.WriteLine(nMessage);
                    SW.Close();
                }
                catch
                {

                }
            }
        }

        private void Save_ChangeParaLog(string nMessage, string paraName, int oldPara, int newPara, string nType)
        {
            string nFolder;
            string nFileName = "";
            nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            string Date;
            Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            nMessage = nMessage + "_" + "INSPECTION" + (m_AlignNo + 1) + "_" + "PANEL" + (m_PatTagNo + 1) + "_";
            nMessage = nMessage + paraName + "_";
            nMessage = nMessage + oldPara + "->" + newPara;

            lock (syncLock_Log)
            {
                try
                {
                    switch (nType)
                    {
                        case Main.DEFINE.CHANGEPARA:
                            nFileName = "ChangeParaLog.txt";
                            nMessage = Date + nMessage;
                            break;
                    }

                    StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
                    SW.WriteLine(nMessage);
                    SW.Close();
                }
                catch
                {

                }
            }
        }

        private void Save_ChangeParaLog(string nMessage, double dRoiNo, string paraName, double oldPara, double newPara, string nType)
        {
            string nFolder;
            string nFileName = "";
            nFolder = Main.LogdataPath + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(Main.LogdataPath)) Directory.CreateDirectory(Main.LogdataPath);
            if (!Directory.Exists(nFolder)) Directory.CreateDirectory(nFolder);

            string Date;
            Date = DateTime.Now.ToString("[MM_dd HH:mm:ss:fff] ");

            nMessage = nMessage + "_" + "INSPECTION" + (m_AlignNo + 1) + "_" + "PANEL" + (m_PatTagNo + 1) + "_";
            nMessage = nMessage + "ROI No." + dRoiNo + "_";
            nMessage = nMessage + paraName + "_";
            nMessage = nMessage + oldPara + "->" + newPara;

            lock (syncLock_Log)
            {
                try
                {
                    switch (nType)
                    {
                        case Main.DEFINE.CHANGEPARA:
                            nFileName = "ChangeParaLog.txt";
                            nMessage = Date + nMessage;
                            break;
                    }

                    StreamWriter SW = new StreamWriter(nFolder + nFileName, true, Encoding.Unicode);
                    SW.WriteLine(nMessage);
                    SW.Close();
                }
                catch
                {
              
                }
            }
        }

        private void lblMakerVisionData_Click(object sender, EventArgs e)
        {
            Label TempBtn = (Label)sender;
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, Convert.ToInt16(TempBtn.Text.ToString()), "Input Data", 0);
            KeyPad.ShowDialog();
            int iInputData = (int)KeyPad.m_data;

            if (TempBtn.Name.Equals("lblTopCutPixel"))
            {
                m_TeachParameter[m_iGridIndex].iTopCutPixel = iInputData;
                TempBtn.Text = iInputData.ToString();
            }
            else if(TempBtn.Name.Equals("lblBottomCutPixel"))
            {
                m_TeachParameter[m_iGridIndex].iBottomCutPixel= iInputData;
                TempBtn.Text = iInputData.ToString();
            }
            else
            {
                m_TeachParameter[m_iGridIndex].iMaskingValue = iInputData;
                TempBtn.Text = iInputData.ToString();
            }
        }

        private void lblIgnoreSize_Click(object sender, EventArgs e)
        {
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, Convert.ToInt16(lblIgnoreSize.Text.ToString()), "Input Data", 0);
            KeyPad.ShowDialog();
            int ignoreSize = (int)KeyPad.m_data;

            m_TeachParameter[m_iGridIndex].iIgnoreSize = ignoreSize;
            lblIgnoreSize.Text = ignoreSize.ToString();
        }

        private void UpdateParamUI()
        {
            if (chkUseEdgeThreshold.Checked)
            {
                pnlOrgParam.Visible = false;
                if (Main.machine.PermissionCheck == Main.ePermission.MAKER)
                    pnlEdgeParam.Visible = true;
                else
                    pnlEdgeParam.Visible = false;

                pnlParam.Controls.Clear();
                pnlEdgeParam.Dock = DockStyle.Fill;
                pnlParam.Controls.Add(pnlEdgeParam);
            }
            else
            {
                pnlOrgParam.Visible = true;
                pnlEdgeParam.Visible = false;
                pnlParam.Controls.Clear();
                pnlOrgParam.Dock = DockStyle.Fill;
                pnlParam.Controls.Add(pnlOrgParam);
            }
        }

        private void lblEdgeCaliperThreshold_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblEdgeCaliperThreshold.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, nCurData, "Input Data", 1);
            KeyPad.ShowDialog();
            int threshold = (int)KeyPad.m_data;
            m_TeachParameter[m_iGridIndex].iEdgeCaliperThreshold = threshold;

            lblEdgeCaliperThreshold.Text = threshold.ToString();
        }

        private void lblEdgeCaliperFilterSize_Click(object sender, EventArgs e)
        {
            double nCurData = Convert.ToDouble(lblEdgeCaliperFilterSize.Text);
            Form_KeyPad KeyPad = new Form_KeyPad(1, 255, nCurData, "Input Data", 2);
            KeyPad.ShowDialog();
            int FilterSize = (int)KeyPad.m_data;
            m_TeachParameter[m_iGridIndex].iEdgeCaliperFilterSize = FilterSize;

            lblEdgeCaliperFilterSize.Text = FilterSize.ToString();

        }

        private void lblTopCutPixel_Click(object sender, EventArgs e)
        {
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, Convert.ToInt16(lblTopCutPixel.Text.ToString()), "Input Data", 0);
            KeyPad.ShowDialog();
            int topCutPixel = (int)KeyPad.m_data;
            m_TeachParameter[m_iGridIndex].iTopCutPixel = topCutPixel;

            lblTopCutPixel.Text = topCutPixel.ToString();
        }

        private void lblBottomCutPixel_Click(object sender, EventArgs e)
        {
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, Convert.ToInt16(lblBottomCutPixel.Text.ToString()), "Input Data", 0);
            KeyPad.ShowDialog();
            int bottomCutPixel = (int)KeyPad.m_data;
            m_TeachParameter[m_iGridIndex].iBottomCutPixel = bottomCutPixel;

            lblBottomCutPixel.Text = bottomCutPixel.ToString();
        }

        private void lblMaskingValue_Click(object sender, EventArgs e)
        {
            Form_KeyPad KeyPad = new Form_KeyPad(0, 255, Convert.ToInt16(lblMaskingValue.Text.ToString()), "Input Data", 0);
            KeyPad.ShowDialog();
            int maskingValue = (int)KeyPad.m_data;
            m_TeachParameter[m_iGridIndex].iMaskingValue = maskingValue;

            lblMaskingValue.Text = maskingValue.ToString();
        }

        private void lblParamEdgeWidthValueUpDown_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int iUpdown = Convert.ToInt32(btn.Tag.ToString());
            double dEdgeWidth = Convert.ToDouble(LAB_EDGE_WIDTH.Text);
            if (iUpdown == 0)
            {
                if (dEdgeWidth == 100) return;
                dEdgeWidth++;
            }
            else
            {
                if (dEdgeWidth == 1) return;
                dEdgeWidth--;
            }

            if (enumROIType.Line == m_enumROIType)
            {
                m_TempFindLineTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                m_TempFindLineTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            }
            else
            {
                m_TempFindCircleTool.RunParams.CaliperRunParams.Edge0Position = -(dEdgeWidth / 2);
                m_TempFindCircleTool.RunParams.CaliperRunParams.Edge1Position = (dEdgeWidth / 2);
            }
            LAB_EDGE_WIDTH.Text = dEdgeWidth.ToString();
        }

        private void DrawComboboxCenterAlign(object sender, DrawItemEventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;

                if (cmb != null)
                {
                    e.DrawBackground();

                    if (cmb.Name.ToString().ToLower().Contains("group"))
                        cmb.ItemHeight = lblPolarity1.Height - 6;
                    else
                        cmb.ItemHeight = lblPolarity1.Height - 6;

                    if (e.Index >= 0)
                    {
                        StringFormat sf = new StringFormat();
                        sf.LineAlignment = StringAlignment.Center;
                        sf.Alignment = StringAlignment.Center;

                        Brush brush = new SolidBrush(cmb.ForeColor);

                        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                            brush = SystemBrushes.HighlightText;

                        e.Graphics.DrawString(cmb.Items[e.Index].ToString(), cmb.Font, brush, e.Bounds, sf);
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                throw;
            }
        }

        private void Combo_Polarity_DrawItem(object sender, DrawItemEventArgs e)
        {
            DrawComboboxCenterAlign(sender, e);
        }

        private void chkUseTracking_CheckedChanged(object sender, EventArgs e)
        {
            if (_isFormLoad == false)
                return;
            
            TrackingTest(chkUseTracking.Checked);
            
            //PT_Display01.Image = OriginImage;
            //if (chkUseTracking.Checked == true)
            //{
            //    //Live Mode On상태일 시, Off로 변경
            //    if (BTN_LIVEMODE.Checked)
            //    {
            //        BTN_LIVEMODE.Checked = false;
            //        BTN_LIVEMODE.BackColor = Color.DarkGray;
            //    }

            //    double TranslationX;
            //    double TranslationY;
            //    bool bSearchRes = Search_PATCNL();
            //    if (bSearchRes == true)
            //    {
            //        TranslationX = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationX - PatResult.TranslationX;
            //        TranslationY = PT_Pattern[m_PatNo, m_PatNo_Sub].Pattern.Origin.TranslationY - PatResult.TranslationY;
            //        TrackingROI(PatResult.TranslationX, PatResult.TranslationY, TranslationX, TranslationY);
            //    }
            //    else
            //    {
            //        MessageBox.Show("AMP Module Mark NG!");
            //    }
            //}
        }
    }
}
    

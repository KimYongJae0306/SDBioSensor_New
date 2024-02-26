using COG.Core;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Units
{
    public class InspUnit
    {
        public CogFitLineTool FindLineTool { get; set; } = null;

        public CogFindCircleTool FindCircleTool { get; set; } = null;

        public InspType InspType { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double LenthX { get; set; }
        public double LenthY { get; set; }
        public double dSpecDistance { get; set; }
        public double dSpecDistanceMax { get; set; }
        public int Distgnore { get; set; }

        public int HistogramROICnt { get; set; }
        public int[] HistogramSpec { get; set; }

        public bool ThresholdUse { get; set; }
        public int Threshold { get; set; } = 32;
        public int TopCutPixel { get; set; } = 15;
        public int IgnoreSize { get; set; } = 20;
        public int BottomCutPixel { get; set; } = 10;
        public int MaskingValue { get; set; } = 210;
        public int EdgeCaliperThreshold { get; set; } = 55;
        public int EdgeCaliperFilterSize { get; set; } = 10;

        public void Dispose()
        {
            FindLineTool?.Dispose();
            FindLineTool = null;

            FindCircleTool?.Dispose();
            FindCircleTool = null;
        }
    }
}

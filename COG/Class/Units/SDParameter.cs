using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Units
{
    public class SDParameter
    {
        public enumROIType m_enumROIType;

        public CogFindLineTool m_FindLineTool;

        public CogFindCircleTool m_FindCircleTool;

        public double CenterX;

        public double CenterY;

        public double LenthX;

        public double LenthY;

        public double dSpecDistance;

        public double dSpecDistanceMax;

        public int IDistgnore;

        public int iHistogramROICnt;

        public int[] iHistogramSpec;

        public bool bThresholdUse;

        public int iThreshold { get; set; } = 32;

        public int iTopCutPixel { get; set; } = 15;

        public int iIgnoreSize { get; set; } = 20;

        public int iBottomCutPixel { get; set; } = 10;

        public int iMaskingValue { get; set; } = 210;

        public int iEdgeCaliperThreshold { get; set; } = 55;

        public int iEdgeCaliperFilterSize { get; set; } = 10;
    }
    public enum enumROIType
    {
        Line = 0,
        Circle = 1
    };
}

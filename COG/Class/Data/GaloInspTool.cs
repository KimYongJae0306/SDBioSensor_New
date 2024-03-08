using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Data
{
    public class GaloInspTool
    {
        public GaloInspType Type { get; set; }

        public int Distgnore { get; set; }

        public double SpecDistance { get; set; }

        public double SpecDistanceMax { get; set; }

        public DarkAreaInspParam DarkArea = new DarkAreaInspParam();

        public CogFindLineTool FindLineTool { get; private set; } = null;

        public CogFindCircleTool FindCircleTool { get; private set; } = null;

        public void SetLineTool(CogFindLineTool tool)
        {
            FindLineTool?.Dispose();
            FindLineTool = null;
            FindLineTool = tool;
        }

        public void SetCircleTool(CogFindCircleTool tool)
        {
            FindCircleTool?.Dispose();
            FindCircleTool = null;
            FindCircleTool = tool;
        }

        public void Dispose()
        {
            FindLineTool?.Dispose();
            FindCircleTool?.Dispose();
        }

        public GaloInspTool DeepCopy()
        {
            GaloInspTool param = new GaloInspTool();
            param.Type = Type;
            param.Distgnore = Distgnore;
            param.SpecDistance = SpecDistance;
            param.SpecDistanceMax = SpecDistanceMax;
            param.DarkArea = DarkArea.DeepCopy();

            if (FindLineTool != null)
                param.FindLineTool = new CogFindLineTool(FindLineTool);
            if (FindCircleTool != null)
                param.FindCircleTool = new CogFindCircleTool(FindCircleTool);

            return param;
        }
    }

    public class DarkAreaInspParam
    {
        public bool ThresholdUse { get; set; }

        public DarkMaskingDirection MaskingDirection { get; set; } = DarkMaskingDirection.InSide;

        public int Threshold { get; set; } = 32;

        public int StartCutPixel { get; set; } = 15; // Inside

        public int EndCutPixel { get; set; } = 10; // Inside

        public int OutsideStartCutPixel { get; set; } = 15; // Inside

        public int OutsideEndCutPixel { get; set; } = 10; // Inside

        public int IgnoreSize { get; set; } = 20;

        public int MaskingValue { get; set; } = 210;

        public int EdgeCaliperThreshold { get; set; } = 55;

        public int EdgeCaliperFilterSize { get; set; } = 10;

        public DarkAreaInspParam DeepCopy()
        {
            DarkAreaInspParam param = new DarkAreaInspParam();
            param.MaskingDirection = MaskingDirection;
            param.Threshold = Threshold;
            param.StartCutPixel = StartCutPixel;
            param.EndCutPixel = EndCutPixel;
            param.OutsideStartCutPixel = OutsideStartCutPixel;
            param.OutsideEndCutPixel = OutsideEndCutPixel;
            param.IgnoreSize = IgnoreSize;
            param.MaskingValue = MaskingValue;
            param.EdgeCaliperThreshold = EdgeCaliperThreshold;
            param.EdgeCaliperFilterSize = EdgeCaliperFilterSize;

            return param;
        }
    }

    public enum GaloInspType
    {
        Line = 0,
        Circle = 1,
    }
}

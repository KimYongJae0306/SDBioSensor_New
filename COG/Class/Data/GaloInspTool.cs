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

        public int IDistgnore { get; set; }

        public double dSpecDistance { get; set; }

        public double dSpecDistanceMax { get; set; }

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
            param.IDistgnore = IDistgnore;
            param.dSpecDistance = dSpecDistance;
            param.dSpecDistanceMax = dSpecDistanceMax;
            param.DarkArea = DarkArea.DeepCopy();

            if (param.FindLineTool != null)
                param.FindLineTool = new CogFindLineTool(FindLineTool);
            if (param.FindCircleTool != null)
                param.FindCircleTool = new CogFindCircleTool(FindCircleTool);

            return param;
        }
    }

    public class DarkAreaInspParam
    {
        public bool ThresholdUse { get; set; }

        public int Threshold { get; set; } = 32;

        public int TopCutPixel { get; set; } = 15;

        public int IgnoreSize { get; set; } = 20;

        public int BottomCutPixel { get; set; } = 10;

        public int MaskingValue { get; set; } = 210;

        public int EdgeCaliperThreshold { get; set; } = 55;

        public int EdgeCaliperFilterSize { get; set; } = 10;

        public DarkAreaInspParam DeepCopy()
        {
            DarkAreaInspParam param = new DarkAreaInspParam();
            param.Threshold = Threshold;
            param.TopCutPixel = TopCutPixel;
            param.IgnoreSize = IgnoreSize;
            param.BottomCutPixel = BottomCutPixel;
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

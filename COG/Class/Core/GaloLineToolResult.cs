using COG.Helper;
using COG.Settings;
using Cognex.VisionPro;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Core
{
    public class GaloLineToolResult
    {
        public Judgement Judgement { get; set; } = Judgement.FAIL;

        public LineResult InsideResult { get; set; } = new LineResult();

        public LineResult OutsideResult { get; set; } = new LineResult();

        public List<double> GetDistance()
        {
            if (InsideResult.PointList.Count <= 0 || OutsideResult.PointList.Count <= 0)
                return new List<double>();

            List<double> distanceList = new List<double>();
            for (int i = 0; i < InsideResult.PointList.Count; i++)
            {
                var point1 = InsideResult.PointList[i];
                var point2 = OutsideResult.PointList[i];

                var distance = MathHelper.GetDistance(point1, point2);
                distance *= (Settings.StaticConfig.PixelResolution / 1000);
                distanceList.Add(distance);
            }
            return distanceList;
        }

        public void Dispose()
        {
            InsideResult?.Dispose();
            OutsideResult?.Dispose();
        }
    }

    public class GaloLineCaliperResult
    {
        public List<PointF> Edge0PointList { get; set; } = new List<PointF>();

        public List<CogCompositeShape> ResultGraphics { get; set; } = new List<CogCompositeShape>();
       
        public void Dispose()
        {
            Edge0PointList.Clear();
            ResultGraphics.ForEach(x => x.Dispose());
        }
    }
}

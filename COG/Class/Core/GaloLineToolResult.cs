using COG.Helper;
using COG.Settings;
using Cognex.VisionPro;
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

        public GaloLineCaliperResult Line0 { get; set; } = new GaloLineCaliperResult();

        public GaloLineCaliperResult Line1 { get; set; } = new GaloLineCaliperResult();

        public List<double> GetDistance()
        {
            if (Line0.Edge0PointList.Count <= 0 || Line1.Edge0PointList.Count <= 0)
                return new List<double>();

            List<double> distanceList = new List<double>();
            for (int i = 0; i < Line0.Edge0PointList.Count; i++)
            {
                var point1 = Line0.Edge0PointList[i];
                var point2 = Line1.Edge0PointList[i];

                var distance = MathHelper.GetDistance(point1, point2);
                distance *= (Settings.StaticConfig.PixelResolution / 1000);
                distanceList.Add(distance);
            }
            return distanceList;
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

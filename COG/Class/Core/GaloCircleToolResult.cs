using COG.Helper;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Core
{
    public class GaloInspToolResult
    {
        public Judgement Judgement { get; set; } = Judgement.FAIL;

        public List<PointF> Edge0PointList { get; set; } = new List<PointF>();

        public List<PointF> Edge1PointList { get; set; } = new List<PointF>();

        public List<CogCompositeShape> ResultGraphics { get; set; } = new List<CogCompositeShape>();

        public List<double> GetDistance()
        {
            if (Edge0PointList.Count <= 0)
                return new List<double>();

            List<double> distanceList = new List<double>();
            for (int i = 0; i < Edge0PointList.Count; i++)
            {
                var point1 = Edge0PointList[i];
                var point2 = Edge1PointList[i];

                var distance = MathHelper.GetDistance(point1, point2);
                distanceList.Add(distance);
            }
            return distanceList;
        }

        public void Dispose()
        {
            Edge0PointList.Clear();
            Edge1PointList.Clear();
            ResultGraphics.ForEach(x => x.Dispose());
        }
    }
}

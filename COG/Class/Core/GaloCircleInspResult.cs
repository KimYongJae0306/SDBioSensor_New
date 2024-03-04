using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Core
{
    public class GaloCircleInspResult
    {
        public Judgement Judgement { get; set; } = Judgement.FAIL;

        public List<PointF> Edge0PointList { get; set; } = new List<PointF>();

        public List<PointF> Edge1PointList { get; set; } = new List<PointF>();


        //public List<double> GetDistance()
        //{
        //    if (Edge0PointList.Count <= 0)
        //        return new List<double>();

        //    for (int i = 0; i < Edge0PointList.Count; i++)
        //    {
        //        var point1 = Edge0PointList[i];
        //        var point2 = Edge1PointList[i];

        //       // Math.Pow(point1.X - point2.X)
        //    }
        //}
    }
}

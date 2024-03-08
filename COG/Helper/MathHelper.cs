using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Helper
{
    public static class MathHelper
    {
        public static double DegToRad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        public static double RadToDeg(double rad)
        {
            return rad * 180.0 / Math.PI;
        }

        public static double GetSlope(PointF point1, PointF point2)
        {
            double deltaX = Math.Abs(point1.X - point2.X);
            double deltaY = Math.Abs(point1.Y - point2.Y);

            return deltaY / deltaX;
        }

        public static PointF GetOffset(PointF point1, PointF point2)
        {
            PointF offset = new PointF();

            offset.X = point2.X - point1.X;
            offset.Y = point2.Y - point1.Y;

            return offset;
        }

        public static PointF GetCoordinate(PointF centerPoint, double diffAngle, PointF offset, PointF inputPoint, double ratio = 1.0)
        {
            //diffAngle = 0;
            PointF outputPoint = new PointF();

            PointF tempPoint = new PointF();

            tempPoint.X = inputPoint.X - centerPoint.X + offset.X;
            tempPoint.Y = inputPoint.Y - centerPoint.Y + offset.Y;
            tempPoint.Y *= -1;

            //inputPoint.X += offset.X;
            //inputPoint.Y += offset.Y;

            double offsetRadian = DegToRad(diffAngle);// 센터, 옵셋, 각도 출력

            var coordinateX = (tempPoint.X * Math.Cos(offsetRadian)) - (tempPoint.Y * Math.Sin(offsetRadian));
            var coordinateY = (tempPoint.X * Math.Sin(offsetRadian)) + (tempPoint.Y * Math.Cos(offsetRadian));

            coordinateX = coordinateX * ratio;
            coordinateY = coordinateY * ratio;

            outputPoint.X = (float)coordinateX + centerPoint.X;
            outputPoint.Y = ((float)coordinateY * -1.0F) + centerPoint.Y;

            return outputPoint;
        }

        public static double GetRadian(PointF leftPoint, PointF rightPoint)
        {
            double deltaX = rightPoint.X - leftPoint.X;
            double deltaY = rightPoint.Y - leftPoint.Y;
            return Math.Atan2(deltaY, deltaX);
        }

        public static double GetDistance(Point point1, Point point2)
        {
            int dx = point2.X - point1.X;
            int dy = point2.Y - point1.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double GetDistance(PointF point1, PointF point2)
        {
            double dx = point2.X - point1.X;
            double dy = point2.Y - point1.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static PointF GetCenterPoint(PointF point1, PointF point2)
        {
            PointF centerPoint = new PointF();

            double x = (point1.X + point2.X) / 2.0;
            double y = (point1.Y + point2.Y) / 2.0;

            centerPoint.X = Convert.ToSingle(x);
            centerPoint.Y = Convert.ToSingle(y);

            return centerPoint;
        }

        public static List<double> GetDistance(List<PointF> points1, List<PointF> points2)
        {
            if (points1.Count <= 0 || points2.Count <= 0)
                return new List<double>();

            List<double> distanceList = new List<double>();
            for (int i = 0; i < points1.Count; i++)
            {
                var point1 = points1[i];
                var point2 = points2[i];

                var distance = GetDistance(point1, point2);
                distance *= (Settings.StaticConfig.PixelResolution / 1000);
                distanceList.Add(distance);
            }
            return distanceList;
        }
    }
}

using COG.Class.Core;
using COG.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Data
{
    public class BondingCoordinate
    {
        public PointF OffsetPoint { get; private set; } = new PointF();

        public double DiffAngle { get; private set; } = 0.0;

        public double MarkDistanceRatio { get; private set; } = 0.0;

        BondingCoordinateData ReferenceData = null;

        BondingCoordinateData TargetData = null;

        public void SetReferenceData(PointF upPoint, PointF downPoint)
        {
            ReferenceData = new BondingCoordinateData();
            ReferenceData.SetPoint(upPoint, downPoint);
        }

        public void SetTargetData(PointF leftPoint, PointF rightPoint)
        {
            TargetData = new BondingCoordinateData();
            TargetData.SetPoint(leftPoint, rightPoint);
        }

        private void SetOffsetPoint()
        {
            if (ReferenceData == null || TargetData == null)
                return;

            var targetCenterPoint = TargetData.GetCenterPoint();
            var referenceCenterPoint = ReferenceData.GetCenterPoint();

            OffsetPoint = MathHelper.GetOffset(referenceCenterPoint, targetCenterPoint);
        }

        private PointF GetOffsetPoint()
        {
            return OffsetPoint;
        }

        private void SetDiffAngle()
        {
            if (ReferenceData == null || TargetData == null)
                return;

            var referenceRadian = ReferenceData.GetRadian();
            var targetRadian = TargetData.GetRadian();

            var referenceDegree = MathHelper.RadToDeg(referenceRadian);
            var targetDegree = MathHelper.RadToDeg(targetRadian);

            DiffAngle = referenceDegree - targetDegree;
        }

        private double GetDiffAngle()
        {
            return DiffAngle;
        }

        private void SetMarkDistanceRatio()
        {
            MarkDistanceRatio = TargetData.GetMarkToMarkDistance() / ReferenceData.GetMarkToMarkDistance();
        }

        private double GetMarkDistanceRatio()
        {
            return MarkDistanceRatio;
        }

        public void ExecuteCoordinate()
        {
            SetOffsetPoint();
            SetDiffAngle();
            SetMarkDistanceRatio();
        }

        public PointF GetCoordinate(PointF inputPoint)
        {
            var diffAngle = GetDiffAngle();
            var offsetPoint = GetOffsetPoint();
            var markDistanceRatio = GetMarkDistanceRatio();

            // Test for ratio
            //markDistanceRatio = 1.0;

            if (diffAngle == 0.0 && offsetPoint == null)
                return inputPoint;

            var targetCenterPoint = TargetData.GetCenterPoint();
            var referenceCenterPoint = ReferenceData.GetCenterPoint();

            return MathHelper.GetCoordinate(targetCenterPoint, diffAngle, offsetPoint, inputPoint/*, markDistanceRatio*/);
        }
    }

    public class BondingCoordinateData
    {
        public PointF UpPoint { get; private set; }

        public PointF DownPoint { get; private set; }

        public double MarkToMarkDistance { get; private set; }

        public void SetPoint(PointF leftPoint, PointF rightPoint)
        {
            UpPoint = leftPoint;
            DownPoint = rightPoint;
        }

        public PointF GetCenterPoint()
        {
            return MathHelper.GetCenterPoint(UpPoint, DownPoint);
        }

        public double GetRadian()
        {
            return MathHelper.GetRadian(UpPoint, DownPoint);
        }

        public double GetMarkToMarkDistance()
        {
            return MathHelper.GetDistance(UpPoint, DownPoint);
        }

        public static double GetDistance(Point point1, Point point2)
        {
            int dx = point2.X - point1.X;
            int dy = point2.Y - point1.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}

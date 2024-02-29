using COG.Class.Core;
using COG.Class.Units;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Data
{
    public class AmpCoordinate
    {
        private bool _enableCoordinate { get; set; } = false;

        public PointF ReferencePoint { get; private set; } = new PointF();

        public PointF TargetPoint { get; private set; } = new PointF();

        public PointF Offset { get; set; } = new PointF();
        public double OffsetX { get; private set; } = 0;

        public double OffsetY { get; private set; } = 0;

        public void SetReferenceData(PointF referencePoint)
        {
            Offset = new PointF();
            ReferencePoint = referencePoint;
        }

        public void SetTargetData(PointF targetPoint)
        {
            Offset = new PointF();
            TargetPoint = targetPoint;
        }

        public void ExecuteCoordinate(Unit unit)
        {
            float offsetX = TargetPoint.X - ReferencePoint.X;
            float offsetY = TargetPoint.Y - ReferencePoint.Y;

            Offset = new PointF(offsetX, offsetY);

            foreach (var toolList in unit.FilmAlign.ToolList)
            {
                toolList.FindLineTool.RunParams.ExpectedLineSegment.StartX += offsetX;
                toolList.FindLineTool.RunParams.ExpectedLineSegment.StartY += offsetY;
                toolList.FindLineTool.RunParams.ExpectedLineSegment.EndX += offsetX;
                toolList.FindLineTool.RunParams.ExpectedLineSegment.EndY += offsetY;
            }
            _enableCoordinate = true;
        }

        private PointF GetCoordinate(PointF inputPoint)
        {
            return new PointF(inputPoint.X + Offset.X, inputPoint.Y + Offset.Y);
        }
    }
}

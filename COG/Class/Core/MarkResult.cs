using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Core
{
    public class MarkResult
    {
        public PointF ReferencePos { get; set; }

        public float ReferenceWidth { get; set; }

        public float ReferenceHeight { get; set; }

        public PointF FoundPos { get; set; }

        public float Score { get; set; }

        public float Scale { get; set; }

        public double Angle { get; set; }

        public CogCompositeShape ResultGraphics { get; set; }
    }
}

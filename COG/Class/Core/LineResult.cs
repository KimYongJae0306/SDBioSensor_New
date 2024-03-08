using COG.Helper;
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
    public class LineResult
    {
        public bool DetectEdgeAlgorithm { get; set; } = false;

        public CogImage8Grey CropImage { get; set; } = null;

        public CogImage8Grey EdgeEnhanceImage { get; set; } = null;

        public Mat EdgeEnhanceMat { get; set; } = null;

        public Mat ThresholdMat { get; set; } = null;

        public List<PointF> PointList { get; set; } = new List<PointF>();

        public List<CogCompositeShape> GraphicsList { get; set; } = new List<CogCompositeShape>();

        public void Dispose()
        {
            CropImage?.Dispose();
            EdgeEnhanceImage?.Dispose();
            EdgeEnhanceMat?.Dispose();
            ThresholdMat?.Dispose();
            PointList.Clear();
            GraphicsList.ForEach(x => x.Dispose());
            GraphicsList.Clear();
        }
    }
}

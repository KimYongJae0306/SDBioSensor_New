using COG.Class.Data;
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
    public class FilmAlignResult
    {
        public FilmROIType Type { get; set; }

        public bool Found { get; set; } = false;

        public PointF StartReferencePoint { get; set; }

        public PointF EndReferencePoint { get; set; }

        public PointF StartFoundPoint { get; set; }

        public PointF EndFoundPoint { get; set; }

        public CogLine Line { get; set; }

        public void Dispose()
        {
            Line?.Dispose();
        }
    }

    public class AmpFilmAlignResult
    {
        public Judgement Judgement { get; set; } = Judgement.FAIL;

        public List<FilmAlignResult> FilmAlignResult { get; set; } = new List<FilmAlignResult>();

        public double GetDistanceX_mm()
        {
            var leftTop = FilmAlignResult.Where(x => x.Type == FilmROIType.Left_Side).FirstOrDefault();
            var rightTop = FilmAlignResult.Where(x => x.Type == FilmROIType.Right_Side).FirstOrDefault();

            if (leftTop == null | rightTop == null)
                return 0.0;

            // X 거리 검출하는데 Center 끼리 보고있음... (향후 문제되면 수정하기로..)
            var value = Math.Abs(leftTop.Line.X - rightTop.Line.X);
            var value_mm = value * StaticConfig.PixelResolution / 1000;

            return value_mm;
        }

        public FilmAlignResult GetFlimResult(FilmROIType type)
        {
            var filmResult = FilmAlignResult.Where(x => x.Type == type).FirstOrDefault();

            return filmResult;
        }
    }
}

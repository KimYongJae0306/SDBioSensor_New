using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Core
{
    public class InspResult
    {
        public int StageNo { get; set; }

        public bool IsLeft { get; set; }

        public Judgement Judgement = Judgement.FAIL;

        public MarkResult AmpMarkResult { get; set; } = null;

        public BondingMarkResult BondingMarkResult { get; set; } = null;

        public AmpFilmAlignResult AmpFilmAlignResult { get; set; } = null;

        public GaloResult GaloResult { get; set; } = new GaloResult();

    }

    public class GaloResult
    {
        public List<GaloLineToolResult> LineResult = new List<GaloLineToolResult>();

        public List<GaloCircleToolResult> CircleResult = new List<GaloCircleToolResult>();
    }
}

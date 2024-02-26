using COG.Core;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Units
{
    public class InspUnit
    {
        public CogFitLineTool FindLineTool { get; set; } = null;

        public CogFindCircleTool FindCircleTool { get; set; } = null;

        public void Dispose()
        {
            FindLineTool?.Dispose();
            FindLineTool = null;

            FindCircleTool?.Dispose();
            FindCircleTool = null;
        }

        public InspUnit DeepCopy()
        {
            InspUnit unit = new InspUnit();
            unit.FindLineTool = new CogFitLineTool(FindLineTool);
            unit.FindCircleTool = new CogFindCircleTool(FindCircleTool);

            return unit;
        }
    }
}

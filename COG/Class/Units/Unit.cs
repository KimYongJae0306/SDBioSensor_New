using COG.Class.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Units
{
    public class Unit
    {
        public MarkUnit Mark = new MarkUnit();

        public FilmAlignParam FilmAlign { get; set; } = new FilmAlignParam();

        public GaloInspParam Insp = new GaloInspParam();

        public Unit DeepCopy()
        {
            Unit unit = new Unit();

            unit.Mark = Mark.DeepCopy();
            unit.FilmAlign = FilmAlign.DeepCopy();
            unit.Insp = Insp.DeepCopy();

            return unit;
        }

        public void Dispose()
        {
            Mark.Dispose();
            FilmAlign.Dispose();
            Insp.Dispose();
        }

        internal void Save()
        {
            throw new NotImplementedException();
        }
    }
}

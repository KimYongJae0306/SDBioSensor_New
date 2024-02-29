using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class.Units
{
    public class StageUnit
    {
        public string Name { get; set; }

        public bool m_GD_ImageSave_Use { get; set; }

        public bool m_NG_ImageSave_Use { get; set; }

        public Unit Left { get; set; } = new Unit();

        public Unit Right { get; set; } = new Unit();

        public StageUnit DeepCopy()
        {
            StageUnit unit = new StageUnit();
            unit.Name = Name;
            unit.m_GD_ImageSave_Use = m_GD_ImageSave_Use;
            unit.m_NG_ImageSave_Use = m_NG_ImageSave_Use;
            unit.Left = Left.DeepCopy();
            unit.Right = Right.DeepCopy();

            return unit;
        }

        public void Dispose()
        {
            Left.Dispose();
            Right.Dispose();
        }
    }
}

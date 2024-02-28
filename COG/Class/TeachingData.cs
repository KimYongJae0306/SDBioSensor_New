using COG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Class
{
    public class TeachingData
    {
        private static TeachingData _instance = null;

        private List<StageUnit> StageUnitList { get; set; } = new List<StageUnit>();

        public BondingMarkDirection BondingMarkDirection { get; set; } = BondingMarkDirection.Up;

        public static TeachingData Instance()
        {
            if (_instance == null)
                _instance = new TeachingData();

            return _instance;
        }

        public void UpdateTeachingData()
        {
            var inspModel = ModelManager.Instance().CurrentModel as InspModel;
            if (inspModel != null)
            {
                Dispose();
                Initialize(inspModel);
            }
        }

        public void Initialize(InspModel inspModel)
        {
            Dispose();
            lock (StageUnitList)
            {
                foreach (var unit in inspModel.StageUnitList)
                {
                    StageUnitList.Add(unit.DeepCopy());
                }
            }
        }

        public StageUnit GetStageUnit(int alignNo)
        {
            if (StageUnitList.Count - 1 < alignNo)
                return null;

            var unit = StageUnitList[alignNo];
            return unit;
        }

        private void Dispose()
        {
            StageUnitList.ForEach(x => x.Dispose());
            StageUnitList.Clear();
        }
    }

    public enum BondingMarkDirection
    {
        Up,
        Down,
    }

    public enum MarkType
    {
        Amp,
        Bonding_Up,
        Bonding_Down,
    }
}

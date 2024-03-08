using COG.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG
{
    public class ModelManager
    {
        #region 필드  
        private static ModelManager _instance = null;

        private InspModel _currentInspModel;
        #endregion

        #region 속성
        public InspModel CurrentModel
        {
            get => _currentInspModel;
            set
            {
                if (ReferenceEquals(_currentInspModel, value))
                    return;

                _currentInspModel = value;
                CurrentModelChangedEvent?.Invoke(_currentInspModel);
            }
        }
        #endregion

        #region 이벤트
        public event CurrentModelChangedDelegate CurrentModelChangedEvent;
        #endregion

        #region 델리게이트
        public delegate void CurrentModelChangedDelegate(InspModel inspModel);
        #endregion

        #region 생성자
        public static ModelManager Instance()
        {
            if (_instance == null)
            {
                _instance = new ModelManager();
            }

            return _instance;
        }
        #endregion

        #region 메서드
        public void ApplyChangedEvent()
        {
            CurrentModelChangedEvent?.Invoke(CurrentModel);
        }
       
        #endregion
    }
}

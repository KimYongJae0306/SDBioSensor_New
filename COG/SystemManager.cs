﻿using COG.UI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG
{
    class SystemManager
    {
        #region 필드
        private static SystemManager _instance = null;

        private MainForm _mainForm = null;
        #endregion

        #region 속성
        private ProgressBarForm ProgressBarForm { get; set; } = new ProgressBarForm();
        #endregion

        #region 이벤트

        #endregion

        #region 델리게이트

        #endregion

        #region 생성자

        #endregion

        #region 메서드
        public static SystemManager Instance()
        {
            if (_instance == null)
                _instance = new SystemManager();

            return _instance;
        }

        public void SetMainForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            ProgressBarForm.Hide();
        }

        delegate void ShowProgerssBarDelegate(int nMaxValue, bool nSelect, int nValue);
        public void ShowProgerssBar(int nMaxValue, bool nSelect, int nValue)
        {
            if (ProgressBarForm.InvokeRequired)
            {
                ShowProgerssBarDelegate call = new ShowProgerssBarDelegate(ShowProgerssBar);
                ProgressBarForm.Invoke(call, nMaxValue, nSelect, nValue);
            }
            else
            {
                if (nSelect)
                {
                    if (nValue == 0)
                    {
                        ProgressBarForm.Message = "Unit";
                        ProgressBarForm.Maximum = nMaxValue;
                        ProgressBarForm.Show();
                        ProgressBarForm.ProgressMaxSet();
                    }
                    else
                    {
                        ProgressBarForm.progressBar1.Value = nValue;
                    }
                }
                else
                {
                    ProgressBarForm.Hide();
                }
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;
using COG.Settings;
using COG.Core;

namespace COG.UI.Forms
{
    public partial class PatternSelectForm : Form
    {
        #region 속성
        public PatternTagSelectForm PatternTagSelect = new PatternTagSelectForm();

        private List<Button> BTN_UNIT = new List<Button>();

        private List<Label> LB_CALNAME = new List<Label>();

        private List<Point> BTN_Location = new List<Point>();
        #endregion

        #region 생성자
        public PatternSelectForm()
        {
            InitializeComponent();
            Allocate_Array();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
        }

        #endregion

        #region 메서드
        private void Form_PatternSelect_Load(object sender, EventArgs e)
        {
        }

        private void Allocate_Array()
        {
            for (int i = 0; i < 18; i++)
            {
                string nTempName;
                nTempName = "BTN_UNIT_" + i.ToString("00");
                Button nType1 = (Button)this.Controls[nTempName];
                BTN_UNIT.Add(nType1);
                BTN_Location.Add(nType1.Location);
            }

            for (int i = 0; i < StaticConfig.STAGE_MAX_COUNT; i++)
            {
                BTN_UNIT[i].Visible = true;
            }
        }

        private void BTN_TEACH_Click(object sender, EventArgs e)
        {
            InspModel inspModel = ModelManager.Instance().CurrentModel;
            if (inspModel == null)
                return;

            Button TempBTN = (Button)sender;

            int index = Convert.ToInt16(TempBTN.Name.Substring(TempBTN.Name.Length - 2, 2));

            PatternTagSelect.StageNo = index;
            PatternTagSelect.ShowDialog();
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Form_PatternSelect_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (CB_TOOLSHOW.Visible)
                CB_TOOLSHOW.Visible = false;
            else
                CB_TOOLSHOW.Visible = true;

        }
        #endregion
    }
}

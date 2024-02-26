using COG.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COG.UI.Forms
{
    public partial class PatternTagSelectForm : Form
    {

        #region 필드
        #endregion

        #region 속성
        private PatternTeachForm PatternTeach = new PatternTeachForm();

        private List<Button> BTN_UNIT = new List<Button>();

        public int StageNo;
        #endregion

        #region 이벤트
        #endregion

        #region 델리게이트
        #endregion

        #region 생성자
        public PatternTagSelectForm()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            Allocate_Array();
        }
        #endregion

        #region 메서드
        private void Allocate_Array()
        {
            for (int i = 0; i < 2; i++)
            {
                string nTempName;
                nTempName = "BTN_PATTERN_TAG_" + i.ToString("00");
                Button nType1 = (Button)this.Controls[nTempName];
                BTN_UNIT.Add(nType1);
            }
        }

        private void Form_PatternTagSelect_Load(object sender, EventArgs e)
        {
            if(StageNo == 0)
            {
                BTN_PATTERN_TAG_00.Text = "INSPECTION " + (StageNo + 1).ToString();
                BTN_PATTERN_TAG_01.Text = "INSPECTION " + (StageNo + 2).ToString();
            }
            else
            {
                BTN_PATTERN_TAG_00.Text = "INSPECTION " + (StageNo + 2).ToString();
                BTN_PATTERN_TAG_01.Text = "INSPECTION " + (StageNo + 3).ToString();
            }
            BTN_PATTERN_TAG_00.Visible = true;
            BTN_PATTERN_TAG_01.Visible = true;
        }

        private void BTN_PATTERN_TAG_00_Click(object sender, EventArgs e)
        {
            PatternTeach.StageUnitNo = StageNo;
            PatternTeach.IsLeft = true;
            PatternTeach.ShowDialog();
        }

        private void BTN_PATTERN_TAG_01_Click(object sender, EventArgs e)
        {
            PatternTeach.StageUnitNo = StageNo;
            PatternTeach.IsLeft = false;
            PatternTeach.ShowDialog();
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < BTN_UNIT.Count; i++)
            {
                BTN_UNIT[i].Visible = false;
            }
            //ToDo : ? 용도를 모르겠음
            //Main.AlignUnit[m_AlignNo].m_PatTagNo = 0;
            this.Hide();
        }
        #endregion
    }
}

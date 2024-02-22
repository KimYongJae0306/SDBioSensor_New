using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COG
{
    public partial class Form_RecipeCopy : Form
    {
       
        public int StageNo = 0;
        public int InspectionType = 0; //0:Inspection, 1: AlignInp
        public bool bRecipeCopy = false;
        public Form_RecipeCopy()
        {
            InitializeComponent();
            SetItem();
        }
        private void SelectStage(object Sender, EventArgs e)
        {
            RadioButton btn = (RadioButton)Sender;
            StageNo = Convert.ToInt32(btn.Tag.ToString());
            SetItem();
        }
        private void SelectInsp(object Sender, EventArgs e)
        {
            RadioButton btn = (RadioButton)Sender;
            InspectionType = Convert.ToInt32(btn.Tag.ToString());
            SetItem();
        }
        private void SetItem()
        {
            if (StageNo == 0)
            {
                BTN_Select_Stage1.BackColor = Color.Red;
                BTN_Select_Stage2.BackColor = Color.DarkGray;
            }
            else
            {
                BTN_Select_Stage1.BackColor = Color.DarkGray;
                BTN_Select_Stage2.BackColor = Color.Red;
            }
            if(InspectionType ==0)
            {
                BTN_Select_inspection.BackColor = Color.Red;
                BTN_Select_AlignInsp.BackColor = Color.DarkGray;
            }
            else
            {
                BTN_Select_inspection.BackColor = Color.DarkGray;
                BTN_Select_AlignInsp.BackColor = Color.Red;
            }
        }
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            bRecipeCopy = true;
            this.Close();
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            bRecipeCopy = false;
            this.Close();
        }
    }
}

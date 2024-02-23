using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cog.Framework.UI.Forms
{
    public partial class ProgressBarForm : Form
    {
        public int Maximum { get; set; }
        public string Message { get; set; }

        public ProgressBarForm()
        {
            InitializeComponent();
          
        }
        private void ProgressBarForm_Load(object sender, EventArgs e)
        {

        }

        public void ProgressMaxSet()
        {
            this.Text = Message + "Data Loading....";
            progressBar1.Maximum = Maximum;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(progressBar1.Value == progressBar1.Maximum)
            {
                timer1.Enabled = false;
                this.Hide();
            }
        }


    }
}

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
    public partial class Form_ProgressBar : Form
    {
        public int nMaximum;
        public string nMessage;
        public Form_ProgressBar()
        {
            InitializeComponent();
          
        }
        private void Form_ProgressBar_Load(object sender, EventArgs e)
        {

        }
        public void Form_ProgressMaxSet()
        {
            this.Text = nMessage + "Data Loading....";
            progressBar1.Maximum = nMaximum;
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

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
    public partial class Form_ProgressBar_Sub : Form
    {
        public int nMaximum;
        public string nMessage;
        public Form_ProgressBar_Sub()
        {
            InitializeComponent();
        }
        private void Form_ProgressBar_Sub_Load(object sender, EventArgs e)
        {
            Form_ProgressMaxSet();
        }

        public void Form_ProgressMaxSet()
        {
            this.Text = "VPP Data  " + nMessage + ".......";
            progressBar2.Maximum = nMaximum;
            timer1.Enabled = true;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar2.Value == progressBar2.Maximum)
            {
                timer1.Enabled = false;
                this.Hide();
            }
        }


    }
}

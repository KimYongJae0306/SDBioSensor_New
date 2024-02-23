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
    public partial class MessageForm : Form
    {
        private int m_nTimer;

        public MessageForm()
        {
            InitializeComponent();
        }

        public MessageForm(int nTimer = 0)
        {
            InitializeComponent();

            m_nTimer = nTimer;

            if (m_nTimer > 0)
            {
                timer1.Interval = m_nTimer * 1000;
            }
        }

        private void BTN_CLOSE_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Hide();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            BTN_CLOSE_Click(null, null);
        }

        private void Form_Message_Shown(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }
    }
}

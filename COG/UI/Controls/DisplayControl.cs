using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace COG.UI.Controls
{
    public partial class DisplayControl : UserControl
    {
        public string InitImagePath { get; set; } = "";

        public DisplayControl()
        {
            InitializeComponent();
        }

        private void DisplayControl_Load(object sender, EventArgs e)
        {
            if(File.Exists(InitImagePath))
            {
                //CogDisplay.Image = 
            }
        }
    }
}

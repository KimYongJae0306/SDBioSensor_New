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
using Cognex.VisionPro.Implementation.Internal;

namespace COG
{
    public partial class Form_CameraSet : Form
    {
      //  private CogToolBlock CS_CogImageBlock = new CogToolBlock(); //카메라 관련 
        private RadioButton[] nBTN_Cam = new RadioButton[Main.DEFINE.CAM_MAX];
        private CogToolBlock[] CogImageBlock = new CogToolBlock[Main.DEFINE.CAM_MAX];

        public Form_CameraSet()
        {
            InitializeComponent();
        }
        private void Form_CameraSet_Load(object sender, EventArgs e)
        {
            BTN_Initial();
        }
        private void Form_CameraSet_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        private void BTN_Initial()
        {
            Point point= new Point();
            point.X = CS_ToolBlockEdit.Location.X;
            point.Y = BTN_SAVE.Location.Y - BTN_SAVE.Size.Height;
            int X_Pos = CS_ToolBlockEdit.Location.X;

            for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            {
                nBTN_Cam[i] = new RadioButton();
                nBTN_Cam[i].Appearance = System.Windows.Forms.Appearance.Button;
                point.X = X_Pos + (i * (BTN_SAVE.Size.Width + 2));
                nBTN_Cam[i].Location = point;
                nBTN_Cam[i].Size = new Size(BTN_SAVE.Size.Width, BTN_SAVE.Size.Height);
                nBTN_Cam[i].Text = i.ToString() + ":CAM";
                nBTN_Cam[i].AutoSize = false;
                nBTN_Cam[i].BackColor = Color.DarkGray;
                nBTN_Cam[i].Font = new Font("맑은 고딕", 12F);
                nBTN_Cam[i].TextAlign = ContentAlignment.MiddleCenter;
                nBTN_Cam[i].TabIndex = i;
                nBTN_Cam[i].Tag = i;
                nBTN_Cam[i].UseVisualStyleBackColor = true;
                Controls.Add(nBTN_Cam[i]);
                if (i == 0) nBTN_Cam[i].Checked = true;
                nBTN_Cam[i].Click += new System.EventHandler(this.BTN_CAM_SELECT);
            }

            bool nTextPan = false; //카메라이름길이 길어지면 글자크기줄이려고..
            for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            {
                CogImageBlock[i] = new CogToolBlock();
                CogImageBlock[i] = Main.vision.CogImageBlock[i];
                if (i == 0) CS_ToolBlockEdit.Subject = CogImageBlock[i];

                if (Main.vision.CamName[i].Length > 7) nTextPan = true;
            }

            if (Main.DEFINE.CAM_MAX == Main.vision.CamName.Length)
            {
                for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
                {
                //    nBTN_Cam[i].Text = i.ToString() + ": " + Main.vision.CamName[i].ToString();
                    nBTN_Cam[i].Text = Main.vision.CamName[i].ToString();
                    if (nTextPan)
                    {
                        nBTN_Cam[i].Font = new Font("맑은 고딕", 9F);
                    }
                }
            }
        }
        private void BTN_CAM_SELECT(object sender, EventArgs e)
        {
            RadioButton TempBTN = (RadioButton)sender;
            int m_Number;
            m_Number = TempBTN.TabIndex;
            CS_ToolBlockEdit.Subject = Main.vision.CogImageBlock[m_Number];
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            CamVppLoad();
            this.Close();
        }
        private void BTN_RUN_Click(object sender, EventArgs e)
        {
            CS_ToolBlockEdit.Subject.Run();
        }
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            {
                String filename = Main.DEFINE.SYS_DATADIR + Main.DEFINE.CAM_SETDIR + "CCD_" + i + ".vpp";
                try
                {
                    CogSerializer.SaveObjectToFile(CogImageBlock[i], filename);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                }
            }
            this.Close();
        }
        private void CamVppLoad()
        {
            for (int i = 0; i < Main.DEFINE.CAM_MAX; i++)
            {
                String filename = Main.DEFINE.SYS_DATADIR + Main.DEFINE.CAM_SETDIR + "CCD_" + i + ".vpp";
                try
                {
                    Main.vision.CogImageBlock[i] = CogSerializer.LoadObjectFromFile(filename) as CogToolBlock;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Source + ex.Message + ex.StackTrace);
                }
            }
        }

        //------------------------------
    }
}

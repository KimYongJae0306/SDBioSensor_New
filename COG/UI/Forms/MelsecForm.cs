using COG.Core;
using COG.Device.PLC;
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
    public partial class MelsecForm : Form
    {
        public MelsecForm()
        {
            InitializeComponent();
            Allocate_Array();
        }

        CMDForm formCmd = new CMDForm();
        Label[] nLavel_UNIT = new Label[StaticConfig.STAGE_MAX_COUNT];

        Label[] nLavel = new Label[StaticConfig.PLC_READ_SIZE];
        Label[] nLavel_Dis = new Label[StaticConfig.PLC_READ_SIZE];
        private List<Label> nLavel_Mode = new List<Label>();
        public void Form_Melsec_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < StaticConfig.STAGE_MAX_COUNT; i++)
            {
                InspModel inspModel = ModelManager.Instance().CurrentModel;
                if (inspModel != null)
                {
                    nLavel_UNIT[i].Text = inspModel.StageUnitList[i].Name;
                }
            }
        }
        private void BTN_TEACH_Click(object sender, EventArgs e)
        {
            Label TempBTN = (Label)sender;
            int m_Number;
            m_Number = TempBTN.TabIndex;

            int nAddress;
            int nValue;

            KeyPadForm keypadForm = new KeyPadForm();
            keypadForm.ShowDialog();

            nValue = Convert.ToInt16(keypadForm.m_data);
            nAddress = StaticConfig.PC_BaseAddress + m_Number;

            //2022 05 09 YSH
            PlcControlManager.Instance().WriteDevice(nAddress, nValue);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            MemDisplay();
            Application.DoEvents();
        }

        public void BTN_EXIT_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            formCmd.Hide();
            this.Hide();
        }

        private void Allocate_Array()
        {
            Point point;
            int m_Number;
            Label nTempLabel = new Label();
            nTempLabel = LABEL_0_99;
            point = LABEL_0_99.Location;

            for (int i = 0; i < StaticConfig.PLC_READ_SIZE; i++)
            {

                nLavel_Dis[i] = new Label();

                if (i > 399 && i < 500)
                {
                    if (i == 400)
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 399));
                }
                else if (i > 299 && i < 400)
                /*                if (i > 299 && i < 400)*/
                {
                    if (i == 300)
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 299));
                }
                else if (i > 199 && i < 300)
                {
                    if (i == 200)
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 199));
                }
                else if (i > 99 && i < 200)
                {
                    if (i == 100)
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 99));
                }
                else
                {
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i + 1));
                }

                nLavel_Dis[i].Location = point;
                nLavel_Dis[i].Size = new Size(70, 18);

                m_Number = i;

                nLavel_Dis[i].Text = (StaticConfig.PLC_BaseAddress + m_Number).ToString();
                nLavel_Dis[i].AutoSize = false;
                nLavel_Dis[i].BorderStyle = BorderStyle.None;
                nLavel_Dis[i].BackColor = Color.DarkGray;
                nLavel_Dis[i].Font = new Font("맑은 고딕", 12F);
                nLavel_Dis[i].TextAlign = ContentAlignment.TopCenter;
                nLavel_Dis[i].Tag = i;
                Controls.Add(nLavel_Dis[i]);
            }

#if true
            point = nTempLabel.Location;
            point.X = nTempLabel.Location.X + 70;

            for (int i = 0; i < StaticConfig.PLC_READ_SIZE; i++)
            {

                nLavel[i] = new Label();

                if (i > 399 && i < 500) //     399  , 600
                {
                    if (i == 400) //400
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 399)); //399
                }
                else if (i > 299 && i < 400) //     399  , 600
                //if (i > 299 && i < 400) //     399  , 600
                {
                    if (i == 300) //400
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 299)); //399
                }
                else if (i > 199 && i < 300) //     399  , 600
                {
                    if (i == 200) //400
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 199)); //399
                }
                else if (i > 99 && i < 200) //    199   , 400
                {
                    if (i == 100) //200
                        point.X = point.X + (nTempLabel.Size.Width * 1) + 1;
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i - 99)); //199
                }
                else
                {
                    point.Y = nTempLabel.Location.Y + ((nTempLabel.Size.Height + 1) * (i + 1));
                }


                nLavel[i].Location = point;
                nLavel[i].TabIndex = i;
                nLavel[i].Size = new Size(145, 18);
                nLavel[i].AutoSize = false;
                nLavel[i].BorderStyle = BorderStyle.None;
                nLavel[i].BackColor = Color.LightGray;
                nLavel[i].Font = new Font("맑은 고딕", 12F);
                nLavel[i].TextAlign = ContentAlignment.TopCenter;
                nLavel[i].Click += new System.EventHandler(BTN_TEACH_Click);
                nLavel[i].Tag = i;
                Controls.Add(nLavel[i]);

            }
#endif
            for (int i = 0; i < StaticConfig.STAGE_MAX_COUNT; i++)
            {

                nLavel_UNIT[i] = new Label();
                nLavel_UNIT[i].Size = new Size(170, 189);

                point.X = LABEL_300_399.Location.X + LABEL_300_399.Size.Width + 2;
                point.Y = LABEL_300_399.Location.Y + ((i + 1) * 190) + (LABEL_300_399.Size.Height + 1);

                point.X = LABEL_400_499.Location.X + LABEL_400_499.Size.Width + 2;
                point.Y = LABEL_400_499.Location.Y + ((i + 1) * 190) + (LABEL_400_499.Size.Height + 1);

                nLavel_UNIT[i].Location = point;

                nLavel_UNIT[i].TabIndex = i;
                nLavel_UNIT[i].AutoSize = false;
                nLavel_UNIT[i].BorderStyle = BorderStyle.None;
                nLavel_UNIT[i].BackColor = Color.WhiteSmoke;
                nLavel_UNIT[i].Font = new Font("맑은 고딕", 12F);
                nLavel_UNIT[i].TextAlign = ContentAlignment.MiddleCenter;
                nLavel_UNIT[i].Tag = i;
                Controls.Add(nLavel_UNIT[i]);
            }


            for (int i = 0; i < StaticConfig.PLC_READ_SIZE; i++)
            {
                int nNum;
                nNum = i + 1;
                string nLabel = "label" + nNum.ToString();
                Label TempLabel = (Label)this.Controls[nLabel];
                nLavel_Mode.Add(TempLabel);
            }
        }

        private void MemDisplay()
        {
            string nMode;
            int m_Number;

            for (int i = 0; i < StaticConfig.PLC_READ_SIZE; i++)
            {

                nMode = nLavel_Mode[i].Text;

                m_Number = i;

                int ndata;
                ndata = StaticConfig.PLCTag.BData[m_Number];
                nLavel[i].Text = ndata.ToString();
            }
        }

        public void Form_Melsec_Load()
        {
            timer1.Enabled = true;
        }

        private void Form_Melsec_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            formCmd.Show();
        }
    }
}

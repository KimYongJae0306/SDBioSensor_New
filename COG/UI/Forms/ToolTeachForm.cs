using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace COG.UI.Forms
{
    public partial class ToolTeachForm : Form
    {

        public int m_AlignNo;
        public int m_PatTagNo; 
        public int m_PatNo;
        public string m_ToolTextName;

        public CogSearchMaxTool TT_SearchMaxTool;
        public CogPMAlignTool       TT_PMAlign;
        public CogBlobTool          TT_BlobTool;
        public CogCaliperTool       TT_CALTool;
        public CogFindLineTool      TT_FindLine;
        public CogFindCircleTool TT_FindCircle;

        private List<Label> nToolName = new List<Label>();
        

       public ToolTeachForm()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(SystemInformation.PrimaryMonitorSize.Width, SystemInformation.PrimaryMonitorSize.Height);
            tabControl1.ItemSize = new System.Drawing.Size(1, 1); //TAB 옆라인숨김 지우지말것
            nToolName.Add(LB_TOOL_NAME_0);
            nToolName.Add(LB_TOOL_NAME_1);
            nToolName.Add(LB_TOOL_NAME_2);
            nToolName.Add(LB_TOOL_NAME_3);
            nToolName.Add(LB_TOOL_NAME_4);
            nToolName.Add(LB_TOOL_NAME_5);
        }
        private void Form_ToolTeach_Load(object sender, EventArgs e)
        {
            //this.Text = Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo ,m_PatNo].m_PatternName;
            //LB_CAMCENTER.Text = "Camera Center Position_ X:" + Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo].ToString() + " Y:" + Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo,m_PatNo].m_CamNo].ToString();
            //Tool_Change();
        }
        private void Tool_Change()
        {
            string n_ToolType = m_ToolTextName;

             //   n_ToolType = Main.AlignUnit[m_AlignNo].PAT[m_PatNo].PMToolBlock.Tools[m_ToolTextName].GetType().Name.ToString();
                if (n_ToolType == "CogSearchMaxTool")
                {
                //    TT_CNLTool = Main.AlignUnit[m_AlignNo].PAT[m_PatNo].PMToolBlock.Tools[m_ToolTextName] as CogCNLSearchTool;
                    cogSearchMaxEditV_0.Subject = TT_SearchMaxTool;
                }
                if (n_ToolType == "CogPMAlignTool")
                {
                //       TT_PMAlign = Main.AlignUnit[m_AlignNo].PAT[m_PatNo].PMToolBlock.Tools[m_ToolTextName] as CogPMAlignTool;
                    cogPMAlignEditV_0.Subject = TT_PMAlign;                                  
                }
                if (n_ToolType == "CogCaliperTool")
                {
                    //    TT_CNLTool = Main.AlignUnit[m_AlignNo].PAT[m_PatNo].PMToolBlock.Tools[m_ToolTextName] as CogCNLSearchTool;
                    cogCaliperEditV_0.Subject = TT_CALTool;
                }
                if (n_ToolType == "CogFindLineTool")
                {
                    //          TT_FindLine = Main.AlignUnit[m_AlignNo].PAT[m_PatNo].PMToolBlock.Tools[m_ToolTextName] as CogFindLineTool;
                    cogFindLineEditV_0.Subject = TT_FindLine;
                }
                if (n_ToolType == "CogFindCircleTool")
                {
                    cogFindCircleEditV_0.Subject = TT_FindCircle;
                }
//                 n_ToolType = Main.AlignUnit[m_AlignNo].PAT[m_PatNo].PMBlobToolBlock.Tools[m_ToolTextName].GetType().Name.ToString();
                if (n_ToolType == "CogBlobTool")
                {
                    cogBlobEditV_0.Subject = TT_BlobTool;
                }


            for (int i = 0; i < tabControl1.Controls.Count; i++)
            {
                if (tabControl1.Controls[i].Text == n_ToolType)
                {
                    tabControl1.SelectedIndex = i;
                    nToolName[i].Visible = true;
                    nToolName[i].Text = m_ToolTextName;
                }
                else
                {
                    nToolName[i].Visible = false;
                }
            }
        }
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Hide();                 
        }

        private void Form_ToolTeach_FormClosed(object sender, FormClosedEventArgs e)
        {
             TT_SearchMaxTool.Dispose();
             TT_PMAlign.Dispose();
             TT_BlobTool.Dispose();
             TT_CALTool.Dispose();
             TT_FindLine.Dispose();
             TT_FindCircle.Dispose();
             GC.Collect();
        }

        private void PatternLoad()
        {
     //       PatFileName1 = "D:\\PAT_SUB_1.vpp";
       //     TT_PMAlign.Pattern = CogSerializer.LoadObjectFromFile(PatFileName1) as CogPMAlignPattern;       
        }

        private void BTN_ORIGIN_CENTER_Click(object sender, EventArgs e)
        {
            //TT_SearchMaxTool.Pattern.Origin.TranslationX = Main.vision.IMAGE_CENTER_X[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo];
            //TT_SearchMaxTool.Pattern.Origin.TranslationY = Main.vision.IMAGE_CENTER_Y[Main.AlignUnit[m_AlignNo].PAT[m_PatTagNo, m_PatNo].m_CamNo];
        }
    }
}

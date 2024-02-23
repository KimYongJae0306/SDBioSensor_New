using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using COG.Settings;

namespace COG.UI.Forms
{
    public partial class ProjectForm : Form
    {
        public ProjectForm()
        {
            InitializeComponent();
        }

        private void ProjectForm_Load(object sender, EventArgs e)
        {
            GetModelList();
           
        }
        private void DataUpdate()
        {
            LB_DISPLAY_CURRENT.Text = AppsConfig.Instance().ProjectName + " - " + AppsConfig.Instance().ProjectInfo;
        }
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(String section, String key, String def, StringBuilder retVal, int size, String filePath);
        private void GetModelList()
        {
            int index;
            string nName;
            string nDir;
            listModel.Items.Clear();
            if (Directory.Exists(StaticConfig.ModelPath))
            {
                string[] arrModel = Directory.GetDirectories(StaticConfig.ModelPath);
                for (int i = 0; i < arrModel.Length; i++)
                {
                    DirectoryInfo DI = new DirectoryInfo(arrModel[i]);


                    StringBuilder temp = new StringBuilder(80);
                    nDir = StaticConfig.ModelPath + DI.Name + "\\Model.ini";
                    GetPrivateProfileString("PROJECT", "NAME", " ", temp, 80, nDir);
                    nName = DI.Name + temp.ToString();
                    listModel.Items.Add(nName);
                }
            }
            index = listModel.FindString(AppsConfig.Instance().ProjectName);
            listModel.SelectedIndex = index;
            DataUpdate();
        }

 

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            LB_DISPLAY_SELECTE.Text = listModel.SelectedItem.ToString();
        }

        private void BTN_DELETE_Click(object sender, EventArgs e)
        {

            PasswordForm formpassword = new PasswordForm(false);
            formpassword.ShowDialog();

            if (!formpassword.LOGINOK)
            {
                formpassword.Dispose();
                return;
            }
            formpassword.Dispose();

            string modelName = LB_DISPLAY_SELECTE.Text;
            string nName;
            try
            {
                DialogResult result = MessageBox.Show("Do you want to Delete " + modelName + " ?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    if (modelName == "")
                    {
                        MessageBox.Show("Model is not Selected", "error");
                        return;
                    }
                    if (LB_DISPLAY_SELECTE.Text == LB_DISPLAY_CURRENT.Text)
                    {
                        MessageBox.Show("Current models can not be deleted", "error");
                        return;
                    }
                    nName = String.Format("{0:000}", LB_DISPLAY_SELECTE.Text.ToString().Substring(0, 3));
                    modelName = nName;
                    FileDeleteAll(modelName);
                    Directory.Delete(StaticConfig.ModelPath + modelName);

                    GetModelList();
                }
                else if (result == DialogResult.No)
                {
                    MessageBox.Show("Delete Cancel", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {

            }
        }
        private void FileDeleteAll(string _modelName)
        {

            if (Directory.Exists(StaticConfig.ModelPath + _modelName))
            {
                string[] arrFile = Directory.GetFiles(StaticConfig.ModelPath + _modelName);

                for (int i = 0; i < arrFile.Length; i++)
                {
                    DirectoryInfo DI = new DirectoryInfo(arrFile[i]);
                    File.Delete(DI.FullName);
                }
            }
            else
            {
                MessageBox.Show("Source Or Dest Path  Not Exist", "Error");
                return;
            }

        }
        private void BTN_LOAD_Click(object sender, EventArgs e)
        {
            //string selectModel;
            //string nName;

            //DialogResult result = MessageBox.Show("Do you want to Load?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //if (result == DialogResult.Yes)
            //{
            //    nName =  String.Format("{0:000}",listModel.SelectedItem.ToString().Substring(0,3));
            //    selectModel = nName;
            //    if (selectModel == AppsConfig.Instance().ProjectName)
            //    {
            //        MessageBox.Show("Current Model", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    GetModelList();
            //    Main.ProjectLoad(selectModel);
            //    DataUpdate();
            //}
            //else if (result == DialogResult.No)
            //{
            //    MessageBox.Show("Load Cancel", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            //DialogResult result = MessageBox.Show("Do you want to Save?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //if (result == DialogResult.Yes)
            //{
            //    string m_SaveName;
            //    string m_SaveInfo;
            //    KeyBoardForm formkeyboard_Name = new KeyBoardForm("INPUT PROJECT CODE( 0 ~ 999)", 0);
            //    formkeyboard_Name.ShowDialog();
            //    if (formkeyboard_Name.m_ResultString != "")
            //    {
            //        m_SaveName = string.Format("{0:000}", Convert.ToInt16(formkeyboard_Name.m_ResultString));
            //    }
            //    else
            //    {
            //        m_SaveName = "";
            //    }
            //    if (m_SaveName == "" || m_SaveName == AppsConfig.Instance().ProjectName)
            //    {
            //        if (m_SaveName == AppsConfig.Instance().ProjectName)
            //        {
            //            MessageBox.Show("Project name Exist", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            return;
            //        }
            //        MessageBox.Show("Enter the Project name", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }

            //    KeyBoardForm formkeyboard_Info = new KeyBoardForm("INPUT PROJECT NAME");
            //    formkeyboard_Info.ShowDialog();
            //    m_SaveInfo = formkeyboard_Info.m_ResultString;
            //    m_SaveInfo = "_" + m_SaveInfo;


            //    Main.ProjectSave(m_SaveName, m_SaveInfo);

            //    //2022 05 09 YSH
            //    //Main.WriteDevice(PLCDataTag.BASE_RW_ADDR + Main.DEFINE.CURRENT_MODEL_CODE, Convert.ToInt16(Main.ProjectName));

            //    int[] setValue = new int[1];
            //    setValue[0] = Convert.ToInt16(Main.ProjectName);
            //    Main.PLCsocket.WriteDevice_W((PLCDataTag.BASE_RW_ADDR + Main.DEFINE.CURRENT_MODEL_CODE).ToString(), 1, setValue);

            //    GetModelList();

            //}
            //else if (result == DialogResult.No)
            //{
            //    MessageBox.Show("Save Cancel", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }

        private void BTN_UP_Click(object sender, EventArgs e)
        {
            int index = 0;

            index = listModel.SelectedIndex;

            if (index > 0)
                listModel.SelectedIndex = index - 1;
        }

        private void BTN_DOWN_Click(object sender, EventArgs e)
        {
            int index = 0;

            index = listModel.SelectedIndex;

            if (index < listModel.Items.Count - 1)
                listModel.SelectedIndex = index + 1;
        }

        private void BTN_RENAME_Click(object sender, EventArgs e)
        {
            //DialogResult result = MessageBox.Show("Do you want to Rename?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            //if (result == DialogResult.Yes)
            //{
            //    string m_SaveName;
            //    string m_SaveInfo;
            //    string m_SaveInfoBack;
            //    m_SaveName = LB_DISPLAY_SELECTE.Text.Substring(0, 3);
            //    m_SaveInfo = m_SaveInfoBack = LB_DISPLAY_SELECTE.Text.Substring(3, LB_DISPLAY_SELECTE.Text.Length - 3);
            //    KeyBoardForm formkeyboard_Info = new KeyBoardForm("INPUT PROJECT NAME", 1, m_SaveInfo);
            //    formkeyboard_Info.ShowDialog();
            //    m_SaveInfo = formkeyboard_Info.m_ResultString;

            //    if (m_SaveInfo == "" || AppsConfig.Instance().ProjectName == "")
            //    {
            //        MessageBox.Show("Enter the Project name", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    if ( m_SaveInfo == m_SaveInfoBack)
            //    {
            //        //전에 이름 이랑 같으면 리턴
            //        return;
            //    }

            //    bool nRet;
            //    nRet =  Main.ProjectRename(m_SaveName, m_SaveInfo);
            //    GetModelList();

            //    if ((m_SaveName == AppsConfig.Instance().ProjectName) && nRet)
            //    {
            //        AppsConfig.Instance().ProjectInfo = m_SaveInfo;
            //        DataUpdate();
            //    }

            //}
            //else if (result == DialogResult.No)
            //{
            //    MessageBox.Show("Rename Cancel", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //}
        }
    }
}

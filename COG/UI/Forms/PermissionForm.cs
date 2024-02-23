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

namespace COG.UI.Forms
{
    public partial class PermissionForm : Form
    {
        private User _user = User.OPERATOR;

        public PermissionForm()
        {
            InitializeComponent();
        }

        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelectPermission_Click(object sender, EventArgs e)
        {
            SelectPermission(sender);
        }

        private void SelectPermission(object sender)
        {
            Button btn = sender as Button;

            _user = (User)Enum.Parse(typeof(User), btn.Text.Replace(" ", ""));

            if(_user == User.OPERATOR)
            {
                SelectOperator();
            }
            else if(_user == User.ENGINEER)
            {
                SelectEngineer();
            }
            else if (_user == User.MAKER)
            {
                SelectMaker();
            }
        }

        private void SelectOperator()
        {
            AppsStatus.Instance().CurrentUser = _user;
            this.Close();
        }

        private void SelectEngineer()
        {
            AppsStatus.Instance().CurrentUser = _user;
            PasswordForm formpassword = new PasswordForm(false);
            formpassword.ShowDialog();
            if(formpassword.LOGINOK)
            {
                AppsStatus.Instance().CurrentUser = User.ENGINEER;
                this.Close();
            }
        }

        private void SelectMaker()
        {
            AppsStatus.Instance().CurrentUser = _user;
            PasswordForm formpassword = new PasswordForm(false);
            formpassword.ShowDialog();
            if (formpassword.LOGINOK)
            {
                AppsStatus.Instance().CurrentUser = User.MAKER;
                this.Close();
            }
        }
    }
}

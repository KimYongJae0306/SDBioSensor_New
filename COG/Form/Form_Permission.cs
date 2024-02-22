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

namespace COG
{
    public partial class Form_Permission : Form
    {
        public Form_Permission()
        {
            InitializeComponent();
        }

        private Main.ePermission _permission = Main.ePermission.OPERATOR;



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

            _permission = (Main.ePermission)Enum.Parse(typeof(Main.ePermission), btn.Text.Replace(" ", ""));

            //Main.machine.Permission = _permission;
            if(_permission == Main.ePermission.OPERATOR)
            {
                SelectOperator();
            }
            else if(_permission == Main.ePermission.ENGINEER)
            {
                SelectEngineer();
            }
            else if (_permission == Main.ePermission.MAKER)
            {
                SelectMaker();
            }
        }

        private void SelectOperator()
        {
            Main.machine.Permission = _permission;
            this.Close();
        }

        private void SelectEngineer()
        {
            Main.machine.PermissionCheck = _permission;
            Form_Password formpassword = new Form_Password(false);
            formpassword.ShowDialog();
            if(formpassword.LOGINOK)
            {
                Main.machine.Permission = Main.ePermission.ENGINEER;
                this.Close();
            }
        }

        private void SelectMaker()
        {
            Main.machine.PermissionCheck = _permission;
            Form_Password formpassword = new Form_Password(false);
            formpassword.ShowDialog();
            if (formpassword.LOGINOK)
            {
                Main.machine.Permission = Main.ePermission.MAKER;
                this.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Downloader
{
    public partial class Form_Credentials : Form
    {
        private string userName;
        private string password;

        public string UserName
        {
            get { return userName; }
            set 
            { 
                userName = value;
                textBox_UserName.Text = value;
            }
        }

        public string Password
        {
            get { return password; }
            set 
            { 
                password = value;
                textBox_Password.Text = value;
            }
        }

        public Form_Credentials()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            userName = textBox_UserName.Text;
            password = textBox_Password.Text;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

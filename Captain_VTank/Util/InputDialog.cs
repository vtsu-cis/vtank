using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Captain_VTank.Util
{
    public partial class InputDialog : Form
    {
        public string Message
        {
            get
            {
                return label1.Text;
            }

            set
            {
                label1.Text = value;
            }
        }

        public string TitleText
        {
            get
            {
                return Text;
            }

            set
            {
                Text = value;
            }
        }

        public string InputText
        {
            get
            {
                return inputTextBox.Text;
            }
        }

        public InputDialog()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}

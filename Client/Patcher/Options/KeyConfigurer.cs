using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VTankOptionsSpace
{
    public partial class KeyConfigurer : Form
    {
        private Keys keyCode;
        private bool canceled = true;

        public Keys KeyCode
        {
            get
            {
                return keyCode;
            }
        }

        public bool Canceled
        {
            get
            {
                return canceled;
            }
        }

        public KeyConfigurer()
        {
            InitializeComponent();

            this.KeyUp += new KeyEventHandler(KeyConfigurer_KeyHandler);
        }

        void KeyConfigurer_KeyHandler(object sender, KeyEventArgs e)
        {
            keyCode = e.KeyCode;
            canceled = false;
            this.Hide();
        }

        // Cancel.
        private void button1_Click(object sender, EventArgs e)
        {
            canceled = true;
            this.Hide();
        }
    }
}

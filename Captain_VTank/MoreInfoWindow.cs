using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Captain_VTank
{
    public partial class MoreInfoWindow : Form
    {
        public Admin.Account Account
        {
            get;
            private set;
        }

        public MoreInfoWindow(Admin.Account account)
        {
            Account = account;

            InitializeComponent();
        }
    }
}

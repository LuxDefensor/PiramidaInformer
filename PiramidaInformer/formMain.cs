using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiramidaInformer
{
    public partial class formMain : Form
    {
        private Settings settings;
        private DataProvider d;

        public formMain()
        {
            InitializeComponent();
            #region Event handlers
            this.Load += FormMain_Load;
            menuExit.Click += MenuExit_Click;
            #endregion
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                settings = new Settings("Settings.ini");
                d = new DataProvider(settings.Server, settings.Database, settings.UserName, settings.Password);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                Application.Exit();
            }
        }

        private void MenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

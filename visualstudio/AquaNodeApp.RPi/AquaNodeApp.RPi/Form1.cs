using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
using System.Windows.Forms;
using MySql.Data.Common;
using AquaNode.Database.RPi.Interfaces;

namespace AquaNodeApp.RPi
{
    public partial class frmMain : Form
    {
        #region fields
        private IFishTankList _fishTankList;
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Width = 800;
            this.Height = 480;


        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void tsbExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

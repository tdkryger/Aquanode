using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AquaNode.Database.RPi;

namespace AquaNodeApp.RPi.Panels
{
    //TODO: Resize and relocate controls depending on size 
    public partial class ucFishtank : UserControl
    {
#region Fields
        private AquaNode.Database.RPi.FishTank _theTank = null;
#endregion

        #region Properties
        public AquaNode.Database.RPi.FishTank TheTank { get { return _theTank; } set { Init(value); } }
        #endregion

        public ucFishtank()
        {
            InitializeComponent();
            
        }


        #region Private methods
        private void Init(AquaNode.Database.RPi.FishTank theTank)
        {
            _theTank = theTank;
            tbName.Text = _theTank.ToString();
        }

        #endregion
    }
}

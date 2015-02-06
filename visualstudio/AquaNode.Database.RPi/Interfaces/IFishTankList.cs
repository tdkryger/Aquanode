using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AquaNode.Database.RPi.Interfaces
{
    public class IFishTankList
    {
        #region Fields
        private List<FishTank> _fishTanks = new List<FishTank>();
        #endregion

        #region Properties
        public int Count { get { return _fishTanks.Count; } }
        #endregion
    }
}

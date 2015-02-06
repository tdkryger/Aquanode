using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AquaNode.Database.RPi
{
    public class Fish
    {
        #region Properties
        public int Id { get; set; }
        public FishGroup Fishgroup { get; set; }
        public string Genus { get; set; }
        public string Species { get; set; }
        public string GermanTradename { get; set; }
        public string EnglishTradename { get; set; }
        public string DanishTradename { get; set; }
        public string Synonyms { get; set; }
        public string Origin { get; set; }
        public string MaxSize { get; set; }
        public string TemperatureRange { get; set; }
        public string PHRange { get; set; }
        #endregion


        #region Constructors
        public Fish()
        {
            Fishgroup = new FishGroup();
        }
        #endregion
    }
}

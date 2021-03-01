using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPDemo
{
    public class CardDataEventArgs : System.EventArgs
    {
        public CardDataEventArgs() { }
        public CardDataEventArgs(string incomingdata)
        {
            string[] parts = incomingdata.Split(':');
            FacilityId = Convert.ToInt32(parts[0]);
            CardId = Convert.ToInt32(parts[1]);
        }

        public int FacilityId { get; private set; }
        public int CardId { get; private set; }



    }
}

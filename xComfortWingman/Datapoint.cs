using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    class Datapoint : IEquatable<Datapoint>
    {
        /*
         Example data point file:

        DP  Name of DP+channel      Serial      Typ Ch  Mod Cls N/A
        4   AS_Bath_Lamp            2102674     16  0   0   0   0
        8   AD_Hall                 2127659     17  0   0   0   0
        12  AD_LivR_Flooder         2159653     17  0   0   0   0
        30  SP_LivR_Door (left)     534998      2   0   0   1   0
        31  SP_LivR_Door (right)    534998      2   1   0   1   0
        35  SP_Bath                 2069541     1   0   0   1   0
        39  SP_Entrance (left)      2097725     2   0   0   1   0
        40  SP_Entrance (right)     2097725     2   1   0   1   0
        62  ST_Outdoor (Channel A)  2380071     23  0   1   1   #000#049#000#-70#2345287#006
        65  SM_Hall (Channel A)     2389752     29  0   1   1   0
        66  SM_Hall (Channel B)     2389752     29  1   1   1   0
         */

        public int DP { get; set; }
        public string Name { get; set; }
        public int Serial { get; set; }
        public int Type { get; set; }
        public int Channel { get; set; }
        public int Mode { get; set; }
        public int Class { get; set; }
        public string Reserved { get; set; }

        public Protocol.PT_RX.Packet LatestDataValues { get; set; }
        public DateTime? LastUpdate { get; set; }

        public override string ToString()
        {
            return (DP + "\t" + Name + "\t" + Serial + "\t" + Type + "\t" + Channel + "\t" + Mode + "\t" + Class + "\t" + Reserved);
        }

        public override int GetHashCode()
        {
            return DP;
        }

        public bool Equals(Datapoint other)
        {
            if (other == null) return false;
            Datapoint objAsDP = other as Datapoint;
            if (objAsDP == null) return false;
            else return Equals(objAsDP);
        }

        public Datapoint(int dP, string name, int serial, int type, int channel, int mode, int @class, string reserved)
        {
            DP = dP;
            Name = name;
            Serial = serial;
            Type = type;
            Channel = channel;
            Mode = mode;
            Class = @class;
            Reserved = reserved;
        }
    }
}

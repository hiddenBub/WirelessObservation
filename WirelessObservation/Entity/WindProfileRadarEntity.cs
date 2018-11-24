using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Entity
{
    public class WindProfileRadarEntity
    {
        private double alt;
        private double speed;
        private double direction;
        private DateTime timeStamp;

        public double Alt { get => alt; set => alt = value; }
        public double Speed { get => speed; set => speed = value; }
        public double Direction { get => direction; set => direction = value; }
        public DateTime TimeStamp { get => timeStamp; set => timeStamp = value; }

        public WindProfileRadarEntity(double alt,double spd, double dir,DateTime tm)
        {
            TimeStamp = tm;
            Alt = alt;
            Speed = spd;
            Direction = dir;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Entity
{
    public class DataEntity
    {
        public string Time;
        public double WindSpeed = 0;
        public double WindDir = 0;
        public DataEntity(string time, double speed, double dir)
        {
            Time = time;
            WindDir = dir;
            WindSpeed = speed;
        }

        public DataEntity(string time)
        {
            Time = time;
        }

    }
}

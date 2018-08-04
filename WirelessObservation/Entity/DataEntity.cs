using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Entity
{
    class DataEntity
    {
        public string Time;
        public decimal WindSpeed;
        public int WindDir;
        public DataEntity(string time, decimal speed, int dir)
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

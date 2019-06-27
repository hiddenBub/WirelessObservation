using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WirelessObservation.Entity
{
    public class WindDirection
    {
        public static List<string> Dir = new List<string> { "北", "北东北", "东北", "东东北", "东", "东东南", "东南", "南东南", "南", "南西南", "西南", "西西南", "西", "西西北", "西北", "北西北" };
        public static string GetFormatDir(double direction)
        {
            string Cdirection = string.Empty;
            double range = (double) 360 / 16;
            double halfRange = range / 2;
            for (int i = 0;i< Dir.Count;i++)
            {
                double rangeMin = i * range - halfRange;
                double rangeMax = i * range + halfRange;
                if (direction > rangeMin && direction < rangeMax)
                {
                    Cdirection = Dir[i];
                    break;
                }
            }
            
            return Cdirection;
        }
    }
}

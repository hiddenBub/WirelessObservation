using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WirelessObservation.Entity;

namespace WirelessObservation.Model
{
    public class FtAnemograph
    {
        // every raw data regular expression
        private Regex dataPattern = new Regex(@"\$WI,WVP=(\d{3}\.\d{1}),(\d{3}),\S+");

        // data inti height 
        private double height = Vendor.SettingHelper.setting.Collect.InitHeight;

        // data match result
        private MatchCollection matchCollection = null;

        public MatchCollection MatchCollection { get => matchCollection; set => matchCollection = value; }

        public FtAnemograph()
        {
            
        }
        //
        public FtAnemograph(string rawData)
        {
            matchCollection = dataPattern.Matches(rawData);
        }

        /// <summary>
        /// get wind-data
        /// </summary>
        /// <returns></returns>
        public WindProfileRadarEntity GetWindProfileRadarEntities()
        {
            
            // create new wind profile Radar Entity
            WindProfileRadarEntity windProfileRadarEntity = null;
            // initialize variable that can be used;
            DateTime dateTime = Vendor.SettingHelper.setting.Systemd.UtcTime ? DateTime.UtcNow : DateTime.Now;
            DateTime fomat = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            double speed = 0;
            int direction = 0;
            
            if (matchCollection.Count > 0 && matchCollection.Count == 1)
            {
                speed = Convert.ToDouble(matchCollection[0].Groups[1].Value) * 100;
                direction = Convert.ToInt32(matchCollection[0].Groups[2].Value);
            }
            windProfileRadarEntity = new WindProfileRadarEntity(height, speed, direction, fomat);

            return windProfileRadarEntity;
        }
    }
}

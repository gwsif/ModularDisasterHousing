using System;
using System.IO;
using System.Linq;
using System.Globalization;

namespace mdh
{
    public static class RetrieveData
    {
        /// <summary>
        /// Reads water, sewage, and power levels from unit sensors and returns them
        /// </summary>
        /// <returns>
        /// A comma delimited string containing water, sewage, and power.
        /// </returns>
        public static string ReadLevels()
        {
            // Parameters
            double water = 0.0;
            double sewage = 0.0;
            double power = 0.0;

            /*Read the contents of each dummy file
                NOTE: Each value has a separate file.
                This is for testing live changes by echoing
                the test value and overwriting. For example
                echo "0.9" >> water
            */
            
            // Create our arrays
            string[] wLine = File.ReadLines("water").ToArray();
            string[] sLine = File.ReadLines("sewage").ToArray();
            string[] pLine = File.ReadLines("power").ToArray();

            // Assign those values
            water = Convert.ToDouble(wLine[0]);
            sewage = Convert.ToDouble(sLine[0]);
            power = Convert.ToDouble(pLine[0]);

            // Check if valid
            CheckValid(water, "WATER");
            CheckValid(sewage, "SEWAGE");
            CheckValid(power, "POWER");

            /* Echo These back for TEST purposes and format them;
            Console.WriteLine("Water: " + water.ToString("P", CultureInfo.InvariantCulture));
            Console.WriteLine("Sewage: " + sewage.ToString("P", CultureInfo.InvariantCulture));
            Console.WriteLine("Power: " + power.ToString("P", CultureInfo.InvariantCulture));
            */

            // Concatenate values in string with delimiter
            string rtn = water + "," + sewage + "," + power;

            return rtn;
        }

        /// <summary>
        /// Checks if the given values are between 0 and 1 (inclusive) and logs an error if out of bounds.
        /// </summary>
        /// <returns>
        /// void
        /// </returns>
        public static void CheckValid(double in_Value, string attr)
        {
            // if values are not within bounds - echo an error code
            //   and issue it to error log with a timestamp

            String timestamp = DateTime.Now.ToString();

            if(in_Value < 0.0 || in_Value > 1.0) 
            {
                Console.WriteLine("E001 - " + attr + " level not within bounds!" + "\n");

                using (StreamWriter sw = File.AppendText("log.txt"))
                {
                    sw.WriteLine(timestamp + " E001 - " + attr + " level not within bounds!");
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Creates a semicolon delimited string of errors based on level readings
        /// </summary>
        /// <returns>
        /// A semicolon delimited string of errors for one unit
        /// </returns>
        public static string EvaluateLevels(Int32 timestamp, string unitID, double water, double sewage, double power)
        {
            // Establish Error Codes
            var waterLow = "E01";
            var sewageLow = "E02";
            var powerLow = "E03";
            var waterWarn = "E04";
            var sewageWarn = "E05";
            var powerWarn = "E06";
            var gen = "E07"; // to be used if generator returns a fault
            var filt = "E08"; // to be used if filter returns a fault

            // Set holder strings for each value
            string waterErr = "";
            string sewageErr = "";
            string powerErr = "";

            // Set holder string used to hold the generated errors
            string errors = "";

            // Evaluate water
            if (water <= 0.90)
            {
                waterErr = "TIME:" + timestamp + "," + "UID:" + unitID + "," + "CODE:" + waterLow + ",MSG:" + "water level at or below nominal threshold!;";

                if (water <= 0.70)
                {
                    waterErr = "TIME:" + timestamp + "," + "UID:" + unitID + "," + "CODE" + waterWarn + ",MSG:" + "water level at or below critical threshold!;";
                }
            }
            
            // Echo the water error
            /* NOTE: below is where we would call Wake-On-Lan to our smart-plug controlling
                the water pump - for now we simply echo the water error until hardware can be
                procured */
            if (!String.IsNullOrEmpty(waterErr))
            {
                Console.WriteLine(waterErr);
            }

            // Evaluate sewage
            if (sewage >= 0.30)
            {
                sewageErr = "TIME:" + timestamp + "," + "UID:" + unitID + "," + "CODE:" + sewageLow + ",MSG:" + "sewage level at or above nominal threshold!;";

                if (sewage >= 0.50)
                {
                    sewageErr = "TIME:" + timestamp + "," + "UID:" + unitID + "," + "CODE:" + sewageWarn + ",MSG:" + "sewage level at or above critical threshold!;";
                }
            }

            // Echo the sewage error
            if (!String.IsNullOrEmpty(sewageErr))
            {
                Console.WriteLine(sewageErr);
            }

            // Evaluate power
            if (power <= 0.90)
            {
                powerErr = "TIME:" + timestamp + "," + "UID:" + unitID + "," + "CODE:" + powerLow + ",MSG:" + "power level at or below nominal threshold!;";

                if (power <= 0.70)
                {
                    powerErr = "TIME:" + timestamp + "," + "UID:" + unitID + "," + "CODE:" + powerWarn + ",MSG:" + "power level at or below critical threshold!;";
                }
            }

            // Echo the power error
            /* NOTE: below is where we would call Wake-On-Lan to our smart-plug controlling
                the generator - for now we simply echo the power error until hardware can be
                procured */
            if (!String.IsNullOrEmpty(powerErr))
            {
                Console.WriteLine(powerErr);
            }

            // Concatenate
            errors += waterErr + sewageErr + powerErr;

            return errors;
        }

        /// <summary>
        /// Creates files for water, sewage, and power sensors if they do not exist.
        /// </summary>
        /// <returns>
        /// void
        /// </returns>     
        public static void CreateFiles()
        {

            if(!File.Exists("water"))
            {
                FileStream fs = File.Create("water");
                fs.Close();

                StreamWriter sw = new StreamWriter("water");
                sw.WriteLine("0.0");
                sw.Close();
            }

            if(!File.Exists("sewage"))
            {
                FileStream fs = File.Create("sewage");
                fs.Close();

                StreamWriter sw = new StreamWriter("sewage");
                sw.WriteLine("0.0");
                sw.Close();
            }

            if(!File.Exists("power"))
            {
                FileStream fs = File.Create("power");
                fs.Close();
                
                StreamWriter sw = new StreamWriter("power");
                sw.WriteLine("0.0");
                sw.Close();
            }
        }
    }
}

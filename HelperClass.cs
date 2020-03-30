using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

namespace mdh
{
    public static class Shell
    {
        /// <summary>
        /// Runs a given bash command and returns its output
        /// </summary>
        /// <returns>
        /// A string containing bash output
        /// </returns>
        public static string ExecBash(this string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
            
        }

        /// <summary>
        /// Generates a unix timestamp of the present time
        /// </summary>
        /// <returns>
        /// A 32-bit integer representing the unix timestamp
        /// </returns>      
        public static Int32 GenTimeStamp()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return unixTimestamp;
        }

        /// <summary>
        /// Formats level readings from decimal to percentage
        /// </summary>
        /// <returns>
        /// A string containing the percentages
        /// </returns>
        public static string FormatData(string inputstring)
        {
            // Turn the input levels to an array
            string[] levels = inputstring.Split(',');

            // Set levels
            double wat = Convert.ToDouble(levels[0]);
            double sew = Convert.ToDouble(levels[1]);
            double pow = Convert.ToDouble(levels[2]);

            // Format
            string water = wat.ToString("P", CultureInfo.InvariantCulture);
            string sewage = sew.ToString("P", CultureInfo.InvariantCulture);
            string power = pow.ToString("P", CultureInfo.InvariantCulture);

            return water + "," + sewage + "," + power;
        }
    }
}
using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

namespace mdh
{
    public static class Shell
    {
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

        public static Int32 GenTimeStamp() //potentially unused!
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            return unixTimestamp;
        }

        public static string FormatData(string inputstring) //potentially unused!
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
using System;
using System.IO;
using System.Linq;

namespace mdh
{
    public static class ContextSwitch
    {
        public static void SetAsDefault()
        {
            string u = "echo 0 > mode".ExecBash();
            Console.WriteLine("Defaulted to Unit Mode!");
            Console.WriteLine("Use --set-town or --set-city to change mode.");
        }
        public static void SetAsUnit()
        {
            Console.WriteLine("This will setup the current device as a unit!");
            Console.WriteLine("Do you wish to proceed? (Y/n): ");

            string ans = Console.ReadLine();

            if(ans == "y" || ans == "Y")
               {
                    string u = "echo 0 > mode".ExecBash();
                    Console.WriteLine("Operation Completed Successfully!");
               }

            else
               {
                    Console.WriteLine("Operation aborted");
               }
        }

        public static void SetAsTown()
        {
            Console.WriteLine("This will setup the current device as a town!");
            Console.WriteLine("Do you wish to proceed? (Y/n): ");

            string ans = Console.ReadLine();

            if(ans == "y" || ans == "Y")
               {
                    string u = "echo 1 > mode".ExecBash();
                    Console.WriteLine("Operation Completed Successfully!");
               }

            else
               {
                    Console.WriteLine("Operation aborted");
               }
            
        }

        public static void SetAsCity()
        {
            Console.WriteLine("This will setup the current device as a city!");
            Console.WriteLine("Do you wish to proceed? (Y/n): ");

            string ans = Console.ReadLine();

            if(ans == "y" || ans == "Y")
               {
                    string u = "echo 2 > mode".ExecBash();
                    Console.WriteLine("Operation Completed Successfully!");
               }

            else
               {
                    Console.WriteLine("Operation aborted");
               }
        }

        public static byte GetMode()
        {
            // Open the mode file
            string[] lines = File.ReadLines("mode").ToArray();

            // The first line is our mode, convert it to a byte
            byte mode = Convert.ToByte(lines[0]);

            return mode;
        }

        public static void CreateSetup()
        {
             string u = "echo pending > setupstat".ExecBash(); // Set initial setup to pending
             Console.WriteLine("Setup file created");
        }

        public static void FinishSetup()
        {
             string u = "echo done > setupstat".ExecBash();
             Console.WriteLine("Setup successful!");
        }

        public static string GetSetup()
        {
             string mode = "";
             string[] lines = File.ReadLines("setupstat").ToArray();
             
             if (lines == null || lines.Length == 0)
             {
               Console.WriteLine("Invalid Mode Detected! Cannot Read Setup file!"); // if the array is empty set mode to invalid
               mode = "invalid";
             }

             else
             {
               mode = Convert.ToString(lines[0]); // otherwise set mode to file contents.
             }

             return mode;
        }
    }
}
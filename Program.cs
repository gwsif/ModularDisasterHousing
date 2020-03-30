using System;
using System.IO;

namespace mdh
{
    class Program
    {
        static void Main(string[] args)
        {

            #region Command Arguments
            // Check for user input
            for(int i = 0; i <args.Length; i++)
            {
                if(args[i] == "--set-unit")
                {
                    ContextSwitch.SetAsUnit();
                }

                if(args[i] == "--set-town")
                {
                    ContextSwitch.SetAsTown();
                }

                if(args[i] == "--set-city")
                {
                    ContextSwitch.SetAsCity();
                }

                if(args[i] == "--get-mode")
                {
                    Console.WriteLine(ContextSwitch.GetMode());
                }

                if(args[i] == "--tcpscan")
                {
                    NetworkRW.TCPScan();
                }

                if(args[i] == "--gendb")
                {
                    SQLHelper.CreateDB();
                }

                if(args[i] == "--refresh-city")
                {
                    NetworkRW.FindCity();
                }

                if(args[i] == "--send-errors")
                {
                    NetworkRW.SendError();
                }
            }
            #endregion
        
            #region DeviceDefaults
            // If mode is not already set, then set it by default to unit mode
            if (!File.Exists("mode"))
            {
                ContextSwitch.SetAsDefault();
            }

            // Create our dummy files
            if (!File.Exists("water") || !File.Exists("sewage") || !File.Exists("power"))
            {
                RetrieveData.CreateFiles();
            }

            // Create a setupstat file
            if (!File.Exists("setupstat"))
            {
                ContextSwitch.CreateSetup();
            }

            #endregion

            #region ModeDependants
            if (ContextSwitch.GetMode() == 0) // if unit
            {
                // We await a connection from the Town Control
                // by using the TCP Unit Server in Network Read/Write
                Console.WriteLine("Unit Mode Detected!");
                ContextSwitch.FinishSetup(); // Setup can be set to finished
                NetworkRW.TcpUnitServer();
            }

            if (ContextSwitch.GetMode() == 1) // if town
            {
                Console.WriteLine("Town Mode Detected!");

                string stat = ContextSwitch.GetSetup();
                
                // if the database doesn't exist, generate it and populate it
                if(stat == "pending")
                {
                    Console.WriteLine("Generating Database...");
                    SQLHelper.CreateDB(); // create the database
                    Console.WriteLine("Scanning Network...");
                    NetworkRW.TCPScan(); // populate the database
                    Console.WriteLine("Detecting City...");
                    NetworkRW.FindCity(); // find the city
                    ContextSwitch.FinishSetup(); // finish the setup
                }

                // Check setup again
                stat = ContextSwitch.GetSetup();

                if (stat == "done")
                {
                    // Initiate the TCP town client
                    Console.WriteLine("Starting town client...");
                    NetworkRW.TcpTownClient();

                    // When finished, echo the errors to city
                    Console.WriteLine("Sending any errors to city control...");
                    System.Threading.Thread.Sleep(5000); // pause briefly before continuing
                    NetworkRW.SendError();
                    Console.WriteLine("Successfully sent errors");
                }         
            }

            if (ContextSwitch.GetMode() == 2) // if city
            {
                Console.WriteLine("City Mode Detected!");

                string stat = ContextSwitch.GetSetup();

                // if the error database doesn't exist yet generate it, but do not populate it.
                if(stat == "pending")
                {
                    Console.WriteLine("Initializating setup:");
                    Console.WriteLine("Generating Database...");
                    SQLHelper.CreateErrorDB(); // create the error database
                    NetworkRW.CitySetup(); // run the city setup
                }

                //check setup again
                stat = ContextSwitch.GetSetup();
                
                if(stat == "done")
                {
                    // just run the city server
                    NetworkRW.CityServer();
                }
            }

            #endregion
        }
    }
}

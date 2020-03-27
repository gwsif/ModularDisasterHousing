using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks; 

namespace mdh
{
    public static class NetworkRW
    {
        private const string AESKey = "ZkGFxpo9q6HVcq#dJeS%lOFlF";
        private const int portNum = 4053; //port to listen on
        private const string cityID = "CCNCS";
        private const string masterhost = "raspberrypi"; // Only devices with this hostname will have ips and MACs added!
        private const string townackmsg = "TACK"; // message to be sent to city when town finds a city
        private const string cityackmsg = cityID;

        /// <summary>
        /// Returns the MAC address of the NIC, excluding the loopback device
        /// </summary>
        /// <returns>
        /// A string representing the current machines MAC Address
        /// </returns>
        public static string ReturnMAC()
        {
            // Declare some holder values
            string nictype = "";
            string m_addr = "";

            // For every NIC found, loop through and print the MAC address
            // that isn't the loopbackdevice.
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                               
                // Assign the Type
                nictype = nic.NetworkInterfaceType.ToString();

                // If the type is not a loopback, then get it's MAC Address
                if (nictype != "Loopback")
                {
                    //Console.WriteLine("Found MAC Address: " + m_addr);
                    //Console.WriteLine("Type: " + nictype);
                    //Console.WriteLine("Hostname: " + System.Environment.MachineName.ToString());
                    m_addr = nic.GetPhysicalAddress().ToString();

                }
            }

            return m_addr;
        }

        /// <summary>
        /// Returns the IPv4 Address of the local machine
        /// </summary>
        /// <returns>
        /// The Local IPv4 Address
        /// </returns>
        public static string ReturnIP()
        {
            // declare holder string
            string ip4address = "";

            // iterrate through our addresses on the local machine
            foreach (IPAddress ipa in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork) // if it's ipv4 assign it
                {
                    ip4address = ipa.ToString();
                    break;
                }
            }

            return ip4address;
        }

        /// <summary>
        /// fetch the hostname of the current device
        /// </summary>
        /// <returns>
        /// string containing localhost name
        /// </returns>
        public static string ReturnHost()
        {
            string h_name = System.Environment.MachineName.ToString();

            return h_name;

        }

        /// <summary>
        /// Scans the network for IP Addresses and MAC Addresses and prints them out
        /// </summary>
        /// <returns>
        /// void
        /// </returns>
        public static void TCPScan()
        {
            // run nmap for hostnames
            string hstcmd = "nmap -sn 192.168.1.0/24 | awk '/scan report/ {print $5}'";
            var hstouput = hstcmd.ExecBash();

            // Parse the hostnames out into a list
            List<string> hostlist = new List<string>(hstouput.Split('\n'));

            // For every entry, If hostname matches masterhost, add the ip
            foreach (var hostname in hostlist)
            {
                if (hostname == masterhost)
                {

                    // Create a List for the ips
                    List<string> iplist = new List<string>();

                    // Create a List for the MACs
                    List<string> maclist = new List<string>();
                    
                    // Get the ips of the recognized hostnames
                    string getaddr = "nmap -sL 192.168.1.0/24 | grep " + masterhost + "| awk '{print $6}' | tr -d '()'";
                    var ipoutput = getaddr.ExecBash();
                    
                    // Split the outputs and add them to the list
                    iplist = ipoutput.Split('\n').ToList();

                    // Remove the last line of the list (because it's always blank)
                    if(iplist.Any())
                    {
                        iplist.RemoveAt(iplist.Count - 1);
                    }

                    // For each ip in the list, ping it to get the MAC address then add to the list
                    foreach (var address in iplist)
                    {
                        string getresult = "arping -c 1 " + address + " | grep -o -E \'([[:xdigit:]]{1,2}:){5}[[:xdigit:]]{1,2}\'"; // use grep to pull the MAC
                        var result = getresult.ExecBash();
                        
                        if (result == "") //if no result, tell that it's not available
                        {
                            result = "Unavailable";
                        }

                        // format the result to get rid of newline characters
                        //string cleanresult = Regex.Replace(result, @"\n", "");

                        maclist.Add(result); // add it to the list               
                    }

                    // Show entries in the lists together
                    for (var i = 0; i < iplist.Count; i++)
                    {
                        Console.WriteLine("FOUND!");
                        Console.WriteLine("IP: " + iplist[i] + " MAC: " + maclist[i]); //Show on same line

                        // Generate a unique ID
                        Random rnd = new Random();
                        int length = 5;
                        var str = "";
                        for(var j = 0; j < length; j++)
                        {
                            str += ((char)(rnd.Next(1,26) + 64)).ToString();
                        }

                        // Sanitize and add the mac address and its unique id to the table.
                        string mac = maclist[i].Replace("\n", String.Empty);
                        mac = mac.Replace(":", String.Empty);

                        // Insert the row unless it is already present, in which case ignore it. 
                        SQLHelper addvals = new SQLHelper("INSERT OR IGNORE INTO units (mac, ip, unit_id) VALUES('" + mac + "','" + iplist[i] + "','" + str + "')");
                        addvals.Run_Cmd();
                        
                    }
                }
            }
        }

        /// <summary>
        /// Creates a TCP timeserver that listens for incoming connections and then responds with the current levels
        /// </summary>
        /// <returns>
        /// void
        /// </returns>
        public static void TcpUnitServer()
        {
            bool done = false;

            // Listen for connections on our defined port
            var listener = new TcpListener(IPAddress.Any, portNum);
            
            // Echo we're beginning the listener
            Console.WriteLine("Unit server now listening for TCP requests on port " + portNum);
            
            listener.Start();

            while (!done)
            {
                TcpClient client = listener.AcceptTcpClient(); // accept the connection

                NetworkStream ns = client.GetStream(); // establish a network stream

                byte[] byte_levels = Encoding.ASCII.GetBytes(RetrieveData.ReadLevels()); // Retrieve and convert
                
                // try to send our data
                try
                {
                    ns.Write(byte_levels, 0, byte_levels.Length);
                    ns.Close();
                    client.Close();

                    // Show how many bytes we sent on each connection
                    Console.WriteLine("Connection Established... Sent " + byte_levels.Length.ToString() + " Bytes");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                // Commented out for Testing
                //listener.Stop();
            }
        }

        public static void TcpTownClient()
        {
            // Fetch ips in a list
            Console.WriteLine("Fetching IPs");
            SQLHelper getips = new SQLHelper("SELECT ip FROM units");
            getips.Run_Cmd();
            getips.SetIPs();

            // Fetch unit ids in a list
            Console.WriteLine("Fetching IDs");
            SQLHelper getids = new SQLHelper("SELECT unit_id FROM units");
            getids.Run_Cmd();
            getids.SetIDs();
            
            // Make a new list for the ips and ids
            List<string> iplist = getips.Get_List();
            List<string> idlist = getids.Get_List();

            // Now go through down each ip and id in the lists and request the data 
            foreach(var address in iplist)
            {
                foreach(var id in idlist)
                {
                    // if the id is not our city identifier, proceed with asking for levels
                    if (id.ToString() != cityID) // !potential logic error != continues code, == skips... no idea why?
                    {
                        //debugging
                        //var checkme = id;
                        
                        // Tell who we are connecting to
                        Console.WriteLine("Connecting to Unit" + id + "...");

                        // Create a new TCP Client with the address and default port number
                        var client = new TcpClient(address, portNum);

                        // Establish a network Stream
                        NetworkStream ns = client.GetStream();

                        // Setup a byte array
                        byte[] bytes = new byte[1024];

                        // Read the bytes from the network stream into the array
                        int bytesRead = ns.Read(bytes, 0, bytes.Length);

                        // Format to string
                        string received = Encoding.ASCII.GetString(bytes,0,bytesRead);

                        // Turn the input levels to an array
                        string[] levels = received.Split(',');

                        // Turn each level into a double
                        double wat = Convert.ToDouble(levels[0]);
                        double sew = Convert.ToDouble(levels[1]);
                        double pow = Convert.ToDouble(levels[2]);

                        // Generate a timestamp
                        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                        // Insert values with associated unit id into Database
                        SQLHelper insertcmd = new SQLHelper("INSERT INTO status VALUES(" + unixTimestamp + "," + "'" + id + "'," + wat + "," + sew + "," + pow +")");
                        insertcmd.Run_Cmd();

                        // Report levels
                        Console.WriteLine("Unit " + id + " reports " + wat + " water, " + sew + " sewage, " + pow + " power");

                    }

                    // if we are dealing with a city, we need to send the acknowledgement message - sp spin up a server
                    else
                    {
                        // echo we're ignoring
                        Console.WriteLine("Skipping City...");

                    }
                }

            }
        }

        // Sends the town ACK message !USELESS
        public static void SendTownACK(string address)
        {
            // Set up a tcp client using the address and default portnumber
                var client = new TcpClient(address, portNum);

                // establish a network stream
                NetworkStream ns = client.GetStream();

                // setup our bytes array
                byte[] bytes = new byte[1024];

                // populate the byte array with our acknowledgement message
                byte[] byte_cityack = Encoding.ASCII.GetBytes(townackmsg);

                // try to write the acknowledgement message
                try
                {
                    ns.Write(byte_cityack, 0, byte_cityack.Length);
                    ns.Close();
                    client.Close();
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
        }

        public static void SendError()
        {
            string dummyError = "ERR0, Testing Error Message and Error Message Value!";

            SQLHelper getCityControl = new SQLHelper("SELECT ip FROM units WHERE unit_id='" + cityID + "'"); //get city IP
            getCityControl.Run_Cmd();
            getCityControl.SetIPs();
            
            List<string> city_ip_list = getCityControl.Get_List(); // shove city ip in a list (probably un-necessary)

            foreach(var ip in city_ip_list)
            {
                var client = new TcpClient(ip, portNum); // connect to the city ip

                NetworkStream ns = client.GetStream(); // establish the network stream

                Krypto krypto = new Krypto(); // Start Krypto

                string encErr = krypto.Encrypt(dummyError, AESKey); // Encrypt the data


                byte[] outgoing_msg = Encoding.ASCII.GetBytes(encErr); // turn our encrypted message into bytes

                try
                {
                    ns.Write(outgoing_msg,0,outgoing_msg.Length); // try to send it
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.ToString()); // if any errors, print them and break
                    break;
                }


            }

        }

        public static void FindCity()
        {
            // Set ACK message
            string ackmsg = "CCNCS";

            // Fetch ips in a list
            SQLHelper getips = new SQLHelper("SELECT ip FROM units");
            getips.Run_Cmd();
            getips.SetIPs();

            // Fetch unit ids in a list
            SQLHelper getids = new SQLHelper("SELECT unit_id FROM units");
            getids.Run_Cmd();
            getids.SetIDs();
            
            // Make a new list for the ips and ids
            List<string> iplist = getips.Get_List();
            List<string> idlist = getids.Get_List();

            // Now go through down each ip and id in the lists and request the data 
            foreach(var address in iplist)
            {
                foreach(var id in idlist)
                {
                    // Create a new TCP Client with the address and default port number
                    var client = new TcpClient(address, portNum);

                    // Establish a network Stream
                    NetworkStream ns = client.GetStream();

                    // Setup a byte array
                    byte[] incoming_msg = new byte[1024];

                    // Read the bytes from the network stream into the array
                    int bytesRead = ns.Read(incoming_msg, 0, incoming_msg.Length);

                    // Format to string
                    string received = Encoding.ASCII.GetString(incoming_msg,0,bytesRead);
                    
                    // if the message that is received is our defined acknowledement message, we have the town control.
                    if (received == ackmsg)
                    {
                        //Change the unit_ID to CCNCS
                        SQLHelper changeID = new SQLHelper("UPDATE units SET unit_id='CCNCS' WHERE ip= '" + address +"'");
                        changeID.Run_Cmd();

                        // Then send the city ACK message
                        byte[] outgoing_msg = new byte[1024];

                        // populate the byte array with our acknowledgement message
                        byte[] byte_cityack = Encoding.ASCII.GetBytes(townackmsg);

                        // try to write the acknowledgement message
                        try
                        {
                            ns.Write(byte_cityack, 0, byte_cityack.Length);
                            ns.Close();
                            client.Close();
                        }

                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }

                    }
                }
            }
        }

        // Initial mode for city - sends identifier, then awaits for acknowledgement to close listening server
        // returns true if successful, false if unsuccessful.
        public static void CitySetup()
        {
            bool done = false;
            bool status = false; // we set to true if connection was successful!

            // Listen for connections on our defined port
            var listener = new TcpListener(IPAddress.Any, portNum);
            
            // Echo we're beginning the listener
            Console.WriteLine("City server now awaiting TCP connection to Town Control on port " + portNum);
            
            listener.Start();

            TcpClient client = listener.AcceptTcpClient(); // accept the connection

            NetworkStream ns = client.GetStream(); // establish a network stream

            while (!done)
            {   
                byte[] outgoing_msg = Encoding.ASCII.GetBytes(cityID); // Retrieve and convert
                
                // try to send our data
                try
                {
                    // Show we received a connection request
                    Console.WriteLine("Incoming Connection....");

                    ns.Write(outgoing_msg, 0, outgoing_msg.Length);
                    //ns.Close();
                    //client.Close();

                    // Show we sent the data
                    Console.WriteLine("Sent city...");


                    Byte[] data = new Byte[256];

                    String responseString = string.Empty;
                    
                    Int32 bytes = ns.Read(data, 0 , data.Length);
                    
                    Console.WriteLine("Looking for response...");

                    responseString = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                    if (!String.IsNullOrEmpty(responseString))
                    {
                        Console.WriteLine("Success! Received MSG " + responseString);
                        status = true;
                        ContextSwitch.FinishSetup();
                        break;
                    }
                    // START THE CLIENT
                    // IF CLIENT RETURNS TRUE SET DONE = TRUE
                }
                   
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    break;
                }
            }

            // Stop the listener on success
            ns.Close();
            listener.Stop();
        }

        public static void CityServer()
        {
            bool done = false;

            // Listen for connections on our defined port
            var listener = new TcpListener(IPAddress.Any, portNum);
                     
            listener.Start();

            // Echo we've started listening
            Console.WriteLine("City Control Listening on port " + portNum + "...");


            while (!done)
            {     
                //byte[] incoming_msg = Encoding.ASCII.GetBytes(cityID); // Retrieve and convert
                TcpClient client = listener.AcceptTcpClient(); // accept the connection

                NetworkStream ns = client.GetStream(); // establish a network stream
                
                Byte[] data = new Byte[256];

                String responseString = string.Empty;

                // try to send our data
                try
                {                   

                    Int32 bytes = ns.Read(data, 0 , data.Length);
                    
                    Console.WriteLine("Receiving...");

                    responseString = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                    if (!String.IsNullOrEmpty(responseString))
                    {
                        Krypto krypto = new Krypto(); // start krypto
                        string decString = krypto.Decrypt(responseString, AESKey); // Decrept the string
                        Console.WriteLine("Success! Received Error " + decString); // write the message we received
                        ns.Close();
                        client.Close();
                    }

                    else
                    {
                        Console.WriteLine("Unable to receive data...");
                    }
                    // START THE CLIENT
                    // IF CLIENT RETURNS TRUE SET DONE = TRUE
                }
                   
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    break;
                }
            }
        }
      
    }
}
using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace mdh
{
    public class SQLHelper
    {
        string command_string = "";
        string a_return = "";
        List<string> returnlist = new List<string>();
        Int64 a_return_int = 0;

        public SQLHelper (string command_string)
        {
            this.command_string = command_string;
        }

        public string Get_Out()
        {
            return a_return;
        }

        public List<string> Get_List()
        {
            return returnlist;
        }

        /// <summary>
        /// Runs a given SQL command
        /// </summary>
        /// <returns>
        /// none
        /// </returns>
        public void Run_Cmd()
        {
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection("Data Source=mdh.db");
            m_dbConnection.Open();

            // Create our command
            SqliteCommand command = new SqliteCommand(command_string, m_dbConnection);

            // Execute our command
            command.ExecuteNonQuery();

            // Close the connection to the DB
            m_dbConnection.Close();
        }

        public string RunAndReturnOne()
        {
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection("Data Source=mdh.db");
            m_dbConnection.Open();

            // Create our command
            SqliteCommand command = new SqliteCommand(command_string, m_dbConnection);

            // Start the reader
            SqliteDataReader reader = command.ExecuteReader();

            // Declare a holder string
            string aReturn = "";

            // While the reader has data to read append the text to the a_Return string.
            while (reader.Read())
            {
                aReturn = reader.GetString(0);
            }

            // Close the connection to the DB
            m_dbConnection.Close();

            return aReturn;
        }


        /// <summary>
        /// Runs a given SQL command on the Error database
        /// </summary>
        /// <returns>
        /// none
        /// </returns>
        public void Run_ErrCmd()
        {  
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection("Data Source=mdherr.db");
            m_dbConnection.Open();

            // Create our command
            SqliteCommand command = new SqliteCommand(command_string, m_dbConnection);

            // Execute our command
            command.ExecuteNonQuery();

            // Close the connection to the DB
            m_dbConnection.Close();
        }

        /// <summary>
        /// Adds IP Addresses to a list inside the SQLHelper class
        /// </summary>
        /// <returns>
        /// none
        /// </returns>
        public void SetIPs()
        {
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection("Data Source=mdh.db");
            m_dbConnection.Open();

            // Create our command
            SqliteCommand command = new SqliteCommand(command_string, m_dbConnection);

            // Start the reader
            SqliteDataReader reader = command.ExecuteReader();

            // While the reader has data to read, append the text to the areturn string
            while (reader.Read())
            {
                // Declare holder string
                string holder = "";
                
                // Grab the value
                holder = reader.GetString(0);

                // Add to our return list
                returnlist.Add(holder);
            }

            // Close db connection
            m_dbConnection.Close();
        }

        /// <summary>
        /// Adds MAC Addresses to a list inside the SQLHelper class
        /// </summary>
        /// <returns>
        /// none
        /// </returns>
        public void SetIDs()
        {
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection("Data Source=mdh.db");
            m_dbConnection.Open();

            // Create our command
            SqliteCommand command = new SqliteCommand(command_string, m_dbConnection);

            // Start the reader
            SqliteDataReader reader = command.ExecuteReader();

            // While the reader has data to read, append the text to the areturn string
            while (reader.Read())
            {
                // Declare holder string
                string holder = "";

                // Grab the value
                holder = reader.GetString(0);

                // Add to our return list
                returnlist.Add(holder);
            }

            // Close db connection
            m_dbConnection.Close();
        }

        /// <summary>
        /// Generates the main Modular Disaster Housing database
        /// </summary>
        /// <returns>
        /// none
        /// </returns>
        public static void CreateDB()
        {
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection("Data Source=mdh.db");
            m_dbConnection.Open();

            // Create our Tables
            string units_table = ("CREATE TABLE IF NOT EXISTS units (mac TEXT UNIQUE, ip TEXT UNIQUE, unit_id TEXT UNIQUE)");
            string stat_table = ("CREATE TABLE IF NOT EXISTS status (timestamp INTEGER NOT NULL, unit_id TEXT, w_level REAL, s_level REAL, "
                                 + "p_level REAL, PRIMARY KEY(timestamp), FOREIGN KEY(unit_id) REFERENCES units(unit_id))");
            string err_table = ("CREATE TABLE IF NOT EXISTS errors (errcode TEXT NOT NULL, errlevel INTEGER NOT NULL, errmessage TEXT NOT NULL, "
                                 + "unit_ID TEXT NOT NULL, FOREIGN KEY(unit_id) REFERENCES units(unit_id))");
            
            SqliteCommand makeUnits = new SqliteCommand(units_table, m_dbConnection);
            makeUnits.ExecuteNonQuery();
            
            SqliteCommand makeStats = new SqliteCommand(stat_table, m_dbConnection);
            makeStats.ExecuteNonQuery();

            SqliteCommand makeErrors = new SqliteCommand(err_table, m_dbConnection);
            makeErrors.ExecuteNonQuery();

            // Close the connection
            m_dbConnection.Close();
        }

        /// <summary>
        /// Creates the error database used by city control
        /// </summary>
        /// <returns>
        /// none
        /// </returns>
        public static void CreateErrorDB()
        {
            SqliteConnection m_dbConnection;
            m_dbConnection = new SqliteConnection("Data Source=mdherr.db");
            m_dbConnection.Open();

            // Create our table
            string err_table = "CREATE TABLE IF NOT EXISTS errors (timestamp INTEGER, unit_id TEXT, code TEXT, message TEXT)";

            SqliteCommand makeErrors = new SqliteCommand(err_table, m_dbConnection);
            makeErrors.ExecuteNonQuery();

            // Close the connection
            m_dbConnection.Close();
        }
    }
}
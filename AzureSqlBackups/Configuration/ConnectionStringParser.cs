using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace AzureSqlBackups.Configuration
{
    /// <summary>
    /// Reads ConnectionStrings from the app/machine.config.  This code assumes all connection strings are SQL Azure ConnectionStrings
    /// </summary>
    public static class ConnectionStringParser
    {
        public static IEnumerable<SqlAzureConnectionStringParsed> GetAllConnections()
        {
            ConnectionStringSettingsCollection connectionStrings = ConfigurationManager.ConnectionStrings;

            List<SqlAzureConnectionStringParsed> connectionStringSeperated 
                = new List<SqlAzureConnectionStringParsed>();

            int index = 0;
            IEnumerator connectionStringsconnectionStringsEnum = connectionStrings.GetEnumerator();
            while (connectionStringsconnectionStringsEnum.MoveNext())
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[index];

                // Ignore machine.config
                if (
                    settings.Equals(
                        ConfigurationManager.OpenMachineConfiguration().ConnectionStrings.ConnectionStrings[0]) == false)
                {
                    SqlConnectionStringBuilder sqlConnectionStringBuilder =
                        new SqlConnectionStringBuilder(settings.ConnectionString);

                    object server;
                    sqlConnectionStringBuilder.TryGetValue("Server", out server);

                    object username;
                    sqlConnectionStringBuilder.TryGetValue("User ID", out username);

                    object password;
                    sqlConnectionStringBuilder.TryGetValue("Password", out password);

                    object database;
                    sqlConnectionStringBuilder.TryGetValue("Database", out database);

                    connectionStringSeperated.Add(new SqlAzureConnectionStringParsed(server.ToString(), database.ToString(), username.ToString(), password.ToString()));
                }
                index += 1;
            }
            return connectionStringSeperated;
        }
    }
}
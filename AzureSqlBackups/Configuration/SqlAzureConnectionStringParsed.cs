using System;

namespace AzureSqlBackups.Configuration
{
    /// <summary>
    /// An object to hold a parsed SQL Azure Connection String
    /// </summary>
    public struct SqlAzureConnectionStringParsed
    {
        public string Server { get; private set; }
        public string Database { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public SqlAzureConnectionStringParsed(string server, string database, string username, string password) : this()
        {
            Server = server;
            Username = username;
            Password = password;
            Database = database;
        }

        public string BackupFileName
        {
            get { return string.Format("{0}_{1}.bacpac", Database.ToLower(), DateTime.Now.ToString("MM-dd-yyyy")); }
        }

        /// <summary>
        /// Gets a command that matches the format
        /// DacCli -S nmahfef9f7.database.windows.net -U sqlsa@nmahfef9f7 -P P@ssw0rd -B -D L3MLocal -F l3mlocal.bacpac -X
        /// to pass to the DacCli.exe for the backups
        /// </summary>
        /// <returns></returns>
        public string GetCommandForDacCli
        {
            get
            {
                return string.Format("-S {0} -U {1} -P {2} -B -D {3} -F {4} -X",
                              Server, Username, Password, Database.ToLower(), BackupFileName);
            }
        }

        public override string ToString()
        {
            return string.Format("Server: {0}, Database: {1}, Username: {2}, Password: {3}", Server, Database, Username,
                                 Password);
        }
    }
}
using System;
using AzureSqlBackups.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureSqlBackups.Tests.Configuration
{
    // Very much an integrated test. Relies on the app.config for the test harness to determine it can read the 
    // properties from the Azure Connectionstrings
    [TestClass]
    public class ConnectionStringParserTest
    {
        [TestMethod]
        public void ParseAzureConnectionStrings()
        {
            var connectionStringsParsed = ConnectionStringParser.GetAllConnections();

            foreach (var connectionStringSeperated in connectionStringsParsed)
            {
                Assert.IsNotNull(connectionStringSeperated.Database);
                Assert.IsNotNull(connectionStringSeperated.BackupFileName);
                Assert.IsNotNull(connectionStringSeperated.Server);
                Assert.IsNotNull(connectionStringSeperated.Password);
                Assert.IsNotNull(connectionStringSeperated.Username);
                Assert.IsNotNull(connectionStringSeperated.GetCommandForDacCli);

                Console.Out.WriteLine(connectionStringSeperated);
                Console.Out.WriteLine(connectionStringSeperated.GetCommandForDacCli);
            }
        }
    }
}
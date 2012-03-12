using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using AzureSqlBackups.Configuration;

namespace AzureSqlBackups
{
    class Program
    {
        static void Main(string[] args)
        {
            string azureConnectionString = ConfigurationManager.AppSettings.Get("AzureConnectionString");
            int timesToRetry = Convert.ToInt32(ConfigurationManager.AppSettings.Get("TimesToRetry"));

            BlobStorageWrapper blobStorageWrapper = new BlobStorageWrapper(azureConnectionString,
                ConfigurationManager.AppSettings.Get("BlobName"), ConfigurationManager.AppSettings.Get("NumberOfDaysToPersistBackups"));

            DeleteLocalBackups();
            blobStorageWrapper.DeleteOldAzureBackups();

            IEnumerable<SqlAzureConnectionStringParsed> connections = 
                ConnectionStringParser.GetAllConnections();

            List<string> errors = new List<string>();
            foreach (var connectionStringSeperated in connections)
            {
                int count = 0;
                while (count < timesToRetry)
                {
                    Console.Out.WriteLine(connectionStringSeperated);
                    ProcessStartInfo startInfo = new ProcessStartInfo("DacCli.exe");
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.Arguments = connectionStringSeperated.GetCommandForDacCli;

                    var process = Process.Start(startInfo);
                    StreamReader myStreamReader = process.StandardOutput;
                    // Read the standard output of the spawned process.
                    while (process.HasExited == false)
                    {
                        string externalProcessOut = myStreamReader.ReadLine();
                        Console.WriteLine(externalProcessOut);
                    }
                    process.WaitForExit();

                    if (File.Exists(connectionStringSeperated.BackupFileName) == false)
                    {
                        count++;
                    }
                    else
                    {
                        count = timesToRetry;
                    }
                }
                HandleBackupAndUploadErrors(blobStorageWrapper, connectionStringSeperated, azureConnectionString, errors, timesToRetry);
            }

            SendEmailOnError(errors);
        }

        public static void SendEmailOnError(List<string> errors)
        {
            if (errors.Any())
            {
                string subject = errors.Aggregate<string, string>(null, (current, error) => current + (error + "\n"));

                MailMessage message = new MailMessage();

                message.To.Add(ConfigurationManager.AppSettings["LoggingEmail"]);
                message.Body = subject;
                message.Subject = "Sql Azure Backup Failure(s) " + DateTime.Now.ToString("MM-dd-yyyy");

                using (SmtpClient client = new SmtpClient())
                {
                    client.Send(message);
                }
            }
        }

        private static void HandleBackupAndUploadErrors(BlobStorageWrapper blobStorageWrapper, 
            SqlAzureConnectionStringParsed sqlAzureConnectionStringParsed, string azureConnectionString, 
            List<string> errorList, int timesToRetry)
        {
            string filename = sqlAzureConnectionStringParsed.BackupFileName;

            if (File.Exists(filename) == true)
            {
                Console.Out.WriteLine("Started uploading file {0} to {1}", filename, azureConnectionString);
                try
                {
                    blobStorageWrapper.UploadFile(filename);
                    Console.Out.WriteLine("Completed uploading file {0} to {1}", filename, azureConnectionString);
                }
                catch (Exception)
                {
                    string exceptionMessage = string.Format("Error uploading database {0} file {1} to {2}", 
                        sqlAzureConnectionStringParsed.Database,
                        filename,azureConnectionString);
                    errorList.Add(exceptionMessage);
                    Console.Out.WriteLine(exceptionMessage);
                }
            }
            else
            {
                errorList.Add(string.Format("Error downloading database: {0} from Sql Azure tried {1} times.",
                    sqlAzureConnectionStringParsed.Database, timesToRetry));
            }
        }

        private static void DeleteLocalBackups()
        {
            DirectoryInfo directory = new DirectoryInfo(".");
            directory.GetFiles("*.bacpac").ToList()
                .ForEach(f =>
                             {
                                 f.Delete();
                                 Console.Out.WriteLine("File {0} was deleted", f.FullName);
                             });
        }
    }
}

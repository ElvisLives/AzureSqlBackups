using System;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureSqlBackups
{
    public class BlobStorageWrapper
    {
        public string AzureConnectionString { get; private set; }
        public string BlobNameForSqlBackups { get; private set; }
        public int NumberOfDaysToPersistBackups { get; private set; }
        private readonly CloudBlobClient cloudBlobClient;

        /// <summary>
        /// Wraps the operations needed to manipulate and upload SQL Backups to Azure
        /// </summary>
        /// <param name="azureConnectionString"></param>
        /// <param name="blobNameForSqlBackups"></param>
        /// <param name="numberOfDaysToPersistBackups"></param>
        public BlobStorageWrapper(string azureConnectionString, string blobNameForSqlBackups, string numberOfDaysToPersistBackups)
        {
            AzureConnectionString = azureConnectionString;
            BlobNameForSqlBackups = blobNameForSqlBackups;
            NumberOfDaysToPersistBackups = int.Parse(numberOfDaysToPersistBackups) * -1;

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(AzureConnectionString);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        }

        public void UploadFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                var container = cloudBlobClient.GetContainerReference(BlobNameForSqlBackups);

                container.CreateIfNotExist();

                CloudBlob cloudBlob = container.GetBlobReference(fileName);
                cloudBlob.DeleteIfExists();

                cloudBlob.UploadFile(fileName);
                cloudBlob.Metadata.Add("FileName", fileName);
                cloudBlob.SetProperties();
                cloudBlob.SetMetadata();
            }
         }

        public void DeleteOldAzureBackups()
        {
            var container = cloudBlobClient.GetContainerReference(BlobNameForSqlBackups);

            container.CreateIfNotExist();

            var listOfBlobs = container.ListBlobs();

            DateTime numberOfDaysBackToIgnoreBackups = DateTime.UtcNow.AddDays(NumberOfDaysToPersistBackups);

            foreach (var listBlobItem in listOfBlobs)
            {
                CloudBlob cloudBlob = container.GetBlobReference(listBlobItem.Uri.ToString());

                cloudBlob.FetchAttributes();

                if (cloudBlob.Properties.LastModifiedUtc.Date < numberOfDaysBackToIgnoreBackups.Date)
                {
                    cloudBlob.DeleteIfExists();
                }
            }
        }
    }
}
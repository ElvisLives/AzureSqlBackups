using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace BlobBackupDownloader
{
    class Program
    {
        private const string AzureConnectionString = "AzureConnectionString";
        private const string PrefixForBlobs = "PrefixForBlobs";

        static void Main(string[] args)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[AzureConnectionString]);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            IListBlobItem lastBlob = cloudBlobClient.ListBlobsWithPrefix(ConfigurationManager.AppSettings[PrefixForBlobs])
                .OrderBy(x=>x.Container.Properties.LastModifiedUtc).Last();

            Console.Out.WriteLine("----Downloading From----");
            Console.Out.WriteLine(lastBlob.Uri);

            var blobReference = cloudBlobClient.GetBlobReference(lastBlob.Uri.ToString());

            var removeUpTo = lastBlob.Uri.ToString().LastIndexOf("/") + 1;

            var fileName = lastBlob.Uri.ToString().Remove(0, removeUpTo);

            Directory.GetFiles("/", "*.bacpac").ToList().ForEach(File.Delete);

            blobReference.DownloadToFile(fileName, new BlobRequestOptions
                                                       {
                                                           RetryPolicy = RetryPolicies.Retry(3,new TimeSpan(0,0,0,30)),
                                                           Timeout = new TimeSpan(0,30,0)
                                                       });

            Console.Out.WriteLine("----Download Complete----");
        }
    }
}

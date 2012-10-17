using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace BlobBackupDownloader
{
    internal class Program
    {
        private const string AzureConnectionString = "AzureConnectionString";
        private const string PrefixForBlobs = "PrefixForBlobs";
        private const string Retries = "Retries";
        private const string TimeoutInMinutes = "TimeoutInMinutes";

        private static void Main()
        {
            Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bacpac").ToList().ForEach(File.Delete);

            int retries = Convert.ToInt32(ConfigurationManager.AppSettings[Retries]);
            int timeoutInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings[TimeoutInMinutes]);

            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings[AzureConnectionString]);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var blobPrefixes = ConfigurationManager.AppSettings[PrefixForBlobs].Split(',')
                .Select(x => x.Trim());

            foreach (var blobPrefix in blobPrefixes)
            {
                IOrderedEnumerable<IListBlobItem> blobs =
                    cloudBlobClient.ListBlobsWithPrefix(blobPrefix,
                                                        new BlobRequestOptions
                                                            {
                                                                UseFlatBlobListing = true,
                                                                BlobListingDetails = BlobListingDetails.All
                                                            }).ToList().OrderBy(
                                                                x => x.Container.Properties.LastModifiedUtc);

                Console.Out.WriteLine("----Here are the backups I see for blob prefix {0}:----",blobPrefix);

                foreach (var listBlobItem in blobs)
                {
                    Console.Out.WriteLine(listBlobItem.Uri);
                }

                var lastBlob = blobs.OrderBy(x => x.Container.Properties.LastModifiedUtc).Last();

                Console.Out.WriteLine("----Starting download for the latest backup ---- {0}", lastBlob.Uri);

                var blobReference = cloudBlobClient.GetBlobReference(lastBlob.Uri.ToString());

                var removeUpTo = lastBlob.Uri.ToString().LastIndexOf("/") + 1;

                var fileName = lastBlob.Uri.ToString().Remove(0, removeUpTo);

                if (File.Exists(fileName) == true)
                {
                    File.Delete(fileName);
                }

                blobReference.DownloadToFile(fileName, new BlobRequestOptions
                                                           {
                                                               RetryPolicy =
                                                                   RetryPolicies.Retry(retries,
                                                                                       new TimeSpan(0, 0, 0, 30)),
                                                               Timeout = new TimeSpan(0, timeoutInMinutes, 0)
                                                           });

                Console.Out.WriteLine("----Download Complete----");
            }
        }
    }
}
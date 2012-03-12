## Overview 

This a little tool I wrote to automate Sql Azure backups via the DACCLI tool via a scheduled task.  It parses all the connection strings in the app.config for 
Azure Connecton Strings, removes any .bacpac's on the local file system, removes blobs older than the NumberOfDaysToPersistBackups, 
and finally tries to download the .bacpac file to the local file system and then upload it to a blob storage.
It will also send an email on failure to an email account specified.  If you want to run it as a scheduled task, I added the file backup.bat which you can
call from the command line that will pipe the output to a text file.

## What's Next?

Ok, so here is what you need to do...

* Download the code from this repo
* Change app.config to your Azure account information, email information
* Compile it
* Place the complete bin\Debug or bin\Release into a folder and run AzureSqlBackups.exe
* If you want to run it as a scheduled task, use the backup.bat file and run it whenever schedule you see fit

## What properties do I need to set?

* LoggingEmail - The email that you want failure notifications sent to
* AzureConnectionString - The Azure Storage account you want backups saved to
* BlobName - The name of the blob you want backups saved to
* NumberOfDaysToPersistBackups - The number of days you want to keep backups in Blob Storage.  It will auto delete past this number of days.  
For example if you put 7 here, it will delete all blobs older than 7 days in the blobName folder specified.
* TimesToRetry- How many times the backup should be tried via DacCli.exe.  If you put a 3 here, 
it will retry 3 times to call DacClie.exe before giving up and moving on to the next database.

### Note #

* If you aren't sure you are using the a proper Azure SQL Connection String try running the sole unit test
* If you don't want emails, you can set the smtp delivery method to specifiedPickupDirectory or remove the error email method.
* This software is written as is, you can do whatever you want with it. There is no warranty for it, written or implied.
Feel free to modify it, make it better, but tell me thanks if it saved you any time.  Im sure in the next quarter, they(Microsoft) will finally have an automated
SQL Azure backup tool
* It uses the Daccli tool from http://sqldacexamples.codeplex.com/releases

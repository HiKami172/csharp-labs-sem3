using System;
using System.ServiceProcess;
using System.IO;
using System.Threading;

namespace CS_lab2
{
    public partial class Service1 : ServiceBase
    {
        Logger logger;

        public Service1()
        {
            InitializeComponent();
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            logger = new Logger();
            Thread loggerThread = new Thread(new ThreadStart(logger.Start));
            loggerThread.Start();
        }

        protected override void OnStop()
        {
            logger.Stop();
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }

    class Logger
    {
        FileSystemWatcher watcher;
        object obj = new object();
        bool enabled = true;
        string sourceDirectoryPath;
        string targetDirectoryPath;

        public Logger()
        {
            sourceDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\") + @"\sourceDirectory";
            targetDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\") + @"\targetDirectory";

            watcher = new FileSystemWatcher(sourceDirectoryPath);

            watcher.Created += FileCreated;
        }

        public void Start()
        {
            watcher.EnableRaisingEvents = true;
            while(enabled)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }

        private void FileCreated(object sender, FileSystemEventArgs e)
        {
            string date = DateTime.Now.ToString("dd/MM/yyyy");
            string time = DateTime.Now.ToString("hh.mm.ss");

            DirectoryInfo encryptedFileDir = Directory.CreateDirectory(targetDirectoryPath + $"\\{date}\\{time}");

            string fileName = Path.GetFileNameWithoutExtension(e.FullPath);
            string encryptedFilePath = encryptedFileDir.FullName + $"\\{fileName}_encrypted.txt";

            Cryptography.Encrypt(e.FullPath, encryptedFilePath);

            Archive.Compress(encryptedFilePath);

            string fileToDecompress = encryptedFileDir.FullName + $"\\{fileName}_encrypted.gz";
            Archive.Decompress(fileToDecompress);

            string fileToDecrypt = encryptedFileDir.FullName + $"\\{fileName}_encrypted.txt";
            Cryptography.Decrypt(fileToDecrypt);

            File.Delete(encryptedFilePath);
            File.Delete(fileToDecompress);
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using FTPFileWatcher;
using FileManager.Enums;

namespace FileManager
{
    delegate void ProcessHandler(string directoryName);
    delegate void AddDirectoryNameHandler(string name);

    class Logger
    {
        FileSystemWatcher watcher;
        object obj = new object();
        bool enabled = true;
        string sourceDirectoryPath;
        string targetDirectoryPath;
        private static Options.ArchiveCryptManager manager = new Options.ArchiveCryptManager();
        public event ProcessHandler Processed;

        private static readonly Dictionary<EncryptCompressMode, AddDirectoryNameHandler> _addDirectoryName
            = new Dictionary<EncryptCompressMode, AddDirectoryNameHandler>
            {
                { EncryptCompressMode.Compress, manager.compressor.SetArchiveName},
                { EncryptCompressMode.Decompress,  manager.compressor.SetDearchiveName},
                { EncryptCompressMode.Encrypt,  manager.encryptor.SetEncryptName },
                { EncryptCompressMode.CompressAndEncrypt,  manager.compressAndEncrypt.SetCompressAndEncryptName },
                { EncryptCompressMode.Decrypt, manager.encryptor.SetDecryptName }
            };
        private static readonly Dictionary<EncryptCompressMode, ProcessHandler> _operation
            = new Dictionary<EncryptCompressMode, ProcessHandler>
            {
                { EncryptCompressMode.Compress,  manager.ProcessCompress},
                { EncryptCompressMode.Decompress,  manager.ProcessDecompress},
                { EncryptCompressMode.Encrypt,  manager.ProcessEncrypt},
                { EncryptCompressMode.Decrypt, manager.ProcessDecrypt},
                { EncryptCompressMode.CompressAndEncrypt,  manager.ProcessArchiveAndEcrypt}
            };

        public Logger(string sourceDirectoryPath, string targetDirectoryPath, Dictionary<EncryptCompressMode, string> mods)
        {
            this.sourceDirectoryPath = sourceDirectoryPath;
            this.targetDirectoryPath = targetDirectoryPath;

            watcher = new FileSystemWatcher(sourceDirectoryPath);

            watcher.Created += WatcherCreated;

            manager.path.MakePath(sourceDirectoryPath, targetDirectoryPath);
            foreach (var mod in mods)
            {
                Processed += _operation[mod.Key];
                _addDirectoryName[mod.Key](mod.Value);
            }
        }

        public void Start()
        {
            watcher.EnableRaisingEvents = true;
            while (enabled)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }

        private void WatcherCreated(object sender, FileSystemEventArgs e)
        {
            string time = DateTime.Now.ToString("hh.mm.ss");

            try
            {
                var name = new Options.FileOption(Processed, e.Name);
                Thread loggerThread = new Thread(new ThreadStart(name.Process));
                loggerThread.Start();
            }
            catch (Exception ex)
            {
                using (var file = new FileStream(Path.Combine(targetDirectoryPath, e.Name + "_" + time + ".txt"), FileMode.Create))
                {
                    file.Write(Encoding.ASCII.GetBytes(ex.Message), 0, ex.Message.Length);
                }
            }
        }
    }
}
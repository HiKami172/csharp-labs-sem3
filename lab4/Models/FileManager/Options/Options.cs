using FileManager;
using System;
using System.IO;
using System.Threading;

namespace FTPFileWatcher
{
    class Options
    {
        public class FileOption
        {
            private string name;
            private ProcessHandler process;

            public FileOption(ProcessHandler process, string name)
            {
                this.process = process;
                this.name = name;
            }


            public void Process()
            {
                process(name);
            }
        }

        public class PathOption
        {
            public string TargetPath { get; private set; }
            public string SourcePath { get; private set; }

            private static PathOption instance;
            private bool isEmpty = true;

            public static PathOption GetInstance()
            {
                if (instance == null)
                    instance = new PathOption();
                return instance;
            }

            public void MakePath(string targetPath, string sourcePath)
            {
                if (isEmpty == true)
                {
                    TargetPath = targetPath;
                    SourcePath = sourcePath;
                }
                isEmpty = false;
            }
        }

        delegate void SetHandler(string name);

        public class CompressionOption
        {
            public string compressedFileName { get; private set; }
            public string decompressedFileName { get; private set; }

            private PathOption path;

            public CompressionOption()
            {
                path = PathOption.GetInstance();
            }

            public void SetArchiveName(string name)
            {
                compressedFileName = name;
                Directory.CreateDirectory(Path.Combine(path.SourcePath, name));
            }

            public void SetDearchiveName(string name)
            {
                decompressedFileName = name;
                Directory.CreateDirectory(Path.Combine(path.SourcePath, name));
            }
        }

        public class EncryptionOption
        {
            public string encryptedFileName { get; private set; }
            public string decryptedFileName { get; private set; }

            private PathOption path;

            public EncryptionOption()
            {
                path = PathOption.GetInstance();
            }

            public void SetEncryptName(string name)
            {
                encryptedFileName = name;
                Directory.CreateDirectory(Path.Combine(path.SourcePath, name));
            }

            public void SetDecryptName(string name)
            {
                decryptedFileName = name;
                Directory.CreateDirectory(Path.Combine(path.SourcePath, name));
            }
        }

        public class CompressAndEncryptOption
        {
            public string CompressAndEncryptName { get; private set; }

            private PathOption path;

            public CompressAndEncryptOption()
            {
                path = PathOption.GetInstance();
            }

            public void SetCompressAndEncryptName(string name)
            {
                CompressAndEncryptName = name;
                Directory.CreateDirectory(Path.Combine(path.SourcePath, name));
            }
        }

        public class ArchiveCryptManager
        {
            public PathOption path;
            public CompressionOption compressor;
            public EncryptionOption encryptor;
            public CompressAndEncryptOption compressAndEncrypt;

            public ArchiveCryptManager()
            {
                path = PathOption.GetInstance();
                encryptor = new EncryptionOption();
                compressor = new CompressionOption();
                compressAndEncrypt = new CompressAndEncryptOption();
            }

            public void ProcessCompress(string name)
            {
                Process(name);
                Archive.Compress(Path.Combine(path.TargetPath, name), Path.Combine(path.SourcePath, compressor.compressedFileName, name + ".gz"));
            }

            public void ProcessDecompress(string name)
            {
                PredProcess(name + ".gz", compressor.SetArchiveName, compressor.compressedFileName);
                Archive.Decompress(Path.Combine(path.SourcePath, compressor.compressedFileName, name + ".gz"),
                         Path.Combine(path.SourcePath, compressor.decompressedFileName, name));
            }

            public void ProcessEncrypt(string name)
            {
                Process(name);
                Cryptography.Encrypt(Path.Combine(path.TargetPath, name), Path.Combine(path.SourcePath, encryptor.encryptedFileName, name));
            }

            public void ProcessDecrypt(string name)
            {
                PredProcess(name, encryptor.SetEncryptName, encryptor.encryptedFileName);
                Cryptography.Decrypt(Path.Combine(path.SourcePath, encryptor.encryptedFileName, name), Path.Combine(path.SourcePath, encryptor.decryptedFileName, name));
            }

            public void ProcessArchiveAndEcrypt(string name)
            {
                PredProcess(name, encryptor.SetEncryptName, encryptor.encryptedFileName);
                Archive.Compress(Path.Combine(path.SourcePath, encryptor.encryptedFileName, name),
                       Path.Combine(path.SourcePath, compressAndEncrypt.CompressAndEncryptName, name + ".gz"));
            }

            public bool IsFileLocked(string file)
            {
                try
                {
                    using (var stream = File.OpenRead(file)) { }
                    return false;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine(file);
                    return true;
                }
            }

            public void Process(string name)
            {
                int i = 0;
                while (IsFileLocked(Path.Combine(path.TargetPath, name)))
                {
                    i++;
                    if (i == 1000)
                        throw new FileLoadException("Unnable to open file: " + name);
                }
            }

            private void CheckDirectory(SetHandler Set, string directoryName, ProcessHandler process, string fileName)
            {
                if (!Directory.Exists(Path.Combine(path.SourcePath, directoryName)))
                {
                    Set("Wrong directory Path!");
                    process(fileName);
                }
            }

            private void PredProcess(string name, SetHandler set, string path)
            {
                CheckDirectory(set, path, ProcessEncrypt, name);
                while (IsFileLocked(Path.Combine(this.path.SourcePath, path, name))) ;
            }
        }
    }
}

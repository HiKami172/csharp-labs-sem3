using System.IO;
using System.IO.Compression;

namespace FTPFileWatcher
{
    class Archive
    {
        public static void Compress(string sourceFilePath, string compressedFilePath)
        {
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.OpenOrCreate))
            {
                string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
                string fileDirPath = Path.GetDirectoryName(sourceFilePath);

                using (FileStream targetStream = File.Create(compressedFilePath))
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public static void Decompress(string sourceFilePath, string decompressedFilePath)
        {
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.OpenOrCreate))
            {
                string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
                string fileDirPath = Path.GetDirectoryName(sourceFilePath);

                using (FileStream targetStream = File.Create(decompressedFilePath))
                {
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                    }
                }
            }
        }
    }
}

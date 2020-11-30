using System.IO;
using System.IO.Compression;

namespace CS_lab2
{
    class Archive
    {
        public static void Compress(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDirPath = Path.GetDirectoryName(filePath);
                string compressedFilePath = fileDirPath + $"\\{fileName}.gz";

                using (FileStream targetStream = File.Create(compressedFilePath))
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public static void Decompress(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDirPath = Path.GetDirectoryName(filePath);
                string decompressedFilePath = fileDirPath + $"\\{fileName}.txt";

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

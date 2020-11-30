using System.IO;
using System.IO.Compression;

namespace CS_lab2
{
    class Archive
    {
        public static void Compress(string filePath)
        {
            // поток для чтения исходного файла
            using (FileStream sourceStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDirPath = Path.GetDirectoryName(filePath);
                string compressedFilePath = fileDirPath + $"\\{fileName}.gz";

                // поток для записи сжатого файла
                using (FileStream targetStream = File.Create(compressedFilePath))
                {
                    // поток архивации
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream); // копируем байты из одного потока в другой
                    }
                }
            }
        }

        public static void Decompress(string filePath)
        {
            // поток для чтения из сжатого файла
            using (FileStream sourceStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileDirPath = Path.GetDirectoryName(filePath);
                string decompressedFilePath = fileDirPath + $"\\{fileName}.txt";

                // поток для записи восстановленного файла
                using (FileStream targetStream = File.Create(decompressedFilePath))
                {
                    // поток разархивации
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                    }
                }
            }
        }
    }
}

using System;
using System.IO;
using System.Security.Cryptography;

namespace CS_lab2
{
    class Cryptography
    {  
        public static void Encrypt(string sourcefilePath, string encryptedFilePath)
        {
            string textFromFile;

            using (FileStream readStream = File.OpenRead(sourcefilePath))
            {
                byte[] byteArray = new byte[readStream.Length];

                readStream.Read(byteArray, 0, byteArray.Length);
                textFromFile = System.Text.Encoding.Default.GetString(byteArray);
            }

            using (AesManaged aes = new AesManaged())
            {
                byte[] encrypted = EncryptStringToBytes_Aes(textFromFile, aes.Key, aes.IV);

                using (FileStream writeStream = new FileStream(encryptedFilePath, FileMode.OpenOrCreate))
                {
                    writeStream.Write(encrypted, 0, encrypted.Length);
                }

                string encryptionInfoFilePath = Path.GetDirectoryName(encryptedFilePath) + "\\encryptionInfo.txt";
                using (FileStream writeStream = new FileStream(encryptionInfoFilePath, FileMode.OpenOrCreate))
                {
                    writeStream.Write(aes.Key, 0, aes.Key.Length);
                    writeStream.Seek(0, SeekOrigin.End);
                    writeStream.Write(aes.IV, 0, aes.IV.Length);
                }
            }
        }

        public static void Decrypt(string filePath)
        {
            byte[] key = new byte[32];
            byte[] IV = new byte[16];

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileDirPath = Path.GetDirectoryName(filePath);

            string encryptionInfoFile = fileDirPath + $"\\encryptionInfo.txt";
            using (FileStream readStream = File.OpenRead(encryptionInfoFile))
            {
                readStream.Read(key, 0, 32);
                readStream.Read(IV, 0, 16);
            }

            string decryptedText;

            using (FileStream readStream = File.OpenRead(filePath))
            {
                byte[] byteArray = new byte[readStream.Length];

                readStream.Read(byteArray, 0, byteArray.Length);

                decryptedText = DecryptStringFromBytes_Aes(byteArray, key, IV);
            }

            string decryptedFileName = fileName.Remove(fileName.Length - "_encrypted".Length);
            string decryptedFilePath = fileDirPath + $"\\{decryptedFileName}.txt";
            using (FileStream writeStream = new FileStream(decryptedFilePath, FileMode.OpenOrCreate))
            {
                byte[] array = System.Text.Encoding.Default.GetBytes(decryptedText);

                writeStream.Write(array, 0, array.Length);
            }

            File.Delete(encryptionInfoFile);
            File.Delete(filePath);
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}

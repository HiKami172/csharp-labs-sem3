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

            //Read data from file to encrypt
            using (FileStream readStream = File.OpenRead(sourcefilePath))
            {
                byte[] byteArray = new byte[readStream.Length];

                readStream.Read(byteArray, 0, byteArray.Length);
                textFromFile = System.Text.Encoding.Default.GetString(byteArray);
            }

            // Create a new instance of the AesManaged
            // class.  This generates a new key and initialization
            // vector (IV).
            using (AesManaged aes = new AesManaged())
            {
                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptStringToBytes_Aes(textFromFile, aes.Key, aes.IV);

                //Create file with encrypted data
                using (FileStream writeStream = new FileStream(encryptedFilePath, FileMode.OpenOrCreate))
                {
                    writeStream.Write(encrypted, 0, encrypted.Length);
                }

                //Create file with key and IV
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
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}

﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace CoreBackup.Models.Crypto
{
    static class Encryption
    {
        private static string KeyIVFilePath;

        private const int AES256KEYSIZE = 256;
        private const int AES256BLOCKSIZE = 16;
        
        private static byte[] AES256Key;
        private static string AES256KeySTRING;

        private static byte[] AES256IV;
        private static string AES256IVString;


        private static void CreateAESKey()
        {
            AES256Key = CreateByteArray(AES256KEYSIZE);
            AES256KeySTRING = Convert.ToBase64String(AES256Key);
        }

        private static void CreateAESIV()
        {
            AES256IV = CreateByteArray(AES256BLOCKSIZE);
            AES256IVString = Convert.ToBase64String(AES256IV);
        }

        public static byte[] CreateByteArray(int length)
        {
            byte[] result = new byte[length];
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                provider.GetBytes(result);
                return result;
            }
        }

        private static void LoadAES_KeyIV_FromFile()
        {
            //TODO: Import Key and IV from same file
        }

        private static void SaveAES_KeyIV_ToFile()
        {
            //TODO: Export Key and IV to same file
        }

        public static bool AESEncryptFile(string filePath, bool deletePlainFile)
        {
            bool KeyCondition = AES256Key != null && AES256Key.Length > 0;
            bool IVCondition = AES256IV != null && AES256IV.Length > 0;
            if (KeyCondition && IVCondition)
            {
                byte[] salt = CreateByteArray(16);
                // FileStream for Creating Encrypted File
                using (FileStream fs = new FileStream(filePath + ".enc", FileMode.Create))
                {
                    using (Aes aes = new AesManaged())
                    {
                        aes.KeySize = AES256KEYSIZE;
                        aes.Key = AES256Key;
                        aes.IV = AES256IV;
                        aes.Padding = PaddingMode.ISO10126;
                        aes.Mode = CipherMode.CBC;
                        int offset = 0;

                        fs.Write(salt, offset, salt.Length);
                        // FileStream for Encrypting 
                        using (CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            // FileStream for Opening Plain File
                            using (FileStream fsIn = new FileStream(filePath, FileMode.Open))
                            {
                                byte[] buffer = new byte[1];
                                int read;
                                try
                                {
                                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        cs.Write(buffer, 0, read);
                                    }

                                    if (deletePlainFile)
                                    {
                                        File.Delete(filePath);
                                    }

                                    cs.Close();
                                    fs.Close();
                                    fsIn.Close();

                                    return true;
                                }
                                catch (Exception e)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }


        public static bool AESDecryptFile(string filePath, bool keepEncryptedFile)
        {
            bool KeyCondition = AES256Key != null && AES256Key.Length > 0;
            bool IVCondition = AES256IV != null && AES256IV.Length > 0;
            if (KeyCondition && IVCondition)
            {
                byte[] salt = CreateByteArray(16);
                int offset = 0;
                using (FileStream fsIn = new FileStream(filePath, FileMode.Open))
                {
                    fsIn.Read(salt, offset, salt.Length);
                    using (Aes aes = new AesManaged())
                    {
                        aes.KeySize = AES256KEYSIZE;
                        aes.Key = AES256Key;
                        aes.IV = AES256IV;
                        aes.Padding = PaddingMode.ISO10126;
                        aes.Mode = CipherMode.CBC;

                        using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (FileStream fsOut = new FileStream(filePath.Remove(filePath.Length - 4),
                                FileMode.Create))
                            {
                                byte[] buffer = new byte[1];
                                int read;

                                try
                                {
                                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                                    { 
                                        fsOut.Write(buffer, 0, buffer.Length);
                                    }

                                    if (!keepEncryptedFile)
                                    {
                                        File.Delete(filePath);
                                    }

                                    cs.FlushFinalBlock();
                                    fsOut.Close();
                                    fsIn.Close();
                                    cs.Close();

                                    return true;

                                }
                                catch (Exception e)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}
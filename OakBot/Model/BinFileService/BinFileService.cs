using System;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace OakBot.Model
{
    public class BinFileService : IBinFileService
    {
        #region Fields

        // Fixed storage directory in appdata
        private static readonly string BinFilesPath = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + "\\OakBot\\Bin";

        // Cypher key and initial vector for encryption purposes
        // Make sure to change this in release versions > https://www.random.org/bytes/
        private static readonly byte[] CypherKey =
            new byte[16] { 0xcf, 0xc0, 0x8f, 0x9a, 0xec, 0xe0, 0xad, 0xc6, 0x8d, 0x36, 0x25, 0xb5, 0xa6, 0x78, 0x8c, 0xf0 };

        private static readonly byte[] CypherIV =
            new byte[16] { 0xfd, 0x2d, 0xff, 0x67, 0x48, 0x01, 0xd9, 0x94, 0x0c, 0xf7, 0x5c, 0x06, 0xbf, 0x3d, 0x7e, 0x59 };

        #endregion

        #region Constructors

        /// <summary>
        /// BinFileService, handles reading and writing objects as binary formatted.
        /// </summary>
        public BinFileService()
        {
            // Initialize bin directory if needed
            if (!Directory.Exists(BinFilesPath))
                Directory.CreateDirectory(BinFilesPath);
        }

        #endregion

        #region Unencrypted Binary Files

        /// <summary>
        /// Write a serializable object to an unencrypted binary file.
        /// </summary>
        /// <param name="filename">The name of the file, without extention, to be used.</param>
        /// <param name="serializable">A serializable tagged object to store in a binary file.</param>
        /// <returns>On success: true, false otherwise.</returns>
        public bool WriteBinFile(string filename, object serializable)
        {
            try
            {
                using (FileStream fs = new FileStream($"{BinFilesPath}\\{filename}.bin",
                    FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Serialize .net object to binary
                    (new BinaryFormatter()).Serialize(fs, serializable);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deserialize an unecrypted binary file to a serializable object.
        /// </summary>
        /// <param name="filename">The name of the file, without extention, to be read.</param>
        /// <returns>Deserialized object (requires casting), null on error or file not found.</returns>
        public object ReadBinFile(string filename)
        {
            // File does not exists, return null
            if (!File.Exists($"{BinFilesPath}\\{filename}.bin"))
                return null;

            try
            {
                using (FileStream fs = new FileStream($"{BinFilesPath}\\{filename}.bin",
                    FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Deserialize binary to .net object
                    return (new BinaryFormatter()).Deserialize(fs);
                }
            }
            catch
            {
                // not deserializable
                return null;
            }
        }

        #endregion

        #region Encrypted Binary Files

        public bool WriteEncryptedBinFile(string filename, object serializable)
        {
            // Init a symmetric algorithm and set Key and IV
            Rijndael cryptor = Rijndael.Create();
            cryptor.KeySize = 128;
            cryptor.Key = CypherKey;
            cryptor.IV = CypherIV;

            //DESCryptoServiceProvider cryptor = new DESCryptoServiceProvider()
            //{
            //    Key = CypherKey,
            //    IV = CypherIV
            //};

            try
            {
                using (FileStream fs = new FileStream($"{BinFilesPath}\\{filename}.bin", FileMode.Create, FileAccess.Write, FileShare.None)) 
                using (CryptoStream cs = new CryptoStream(fs, cryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // Serialize .net object to binary
                    (new BinaryFormatter()).Serialize(cs, serializable);
                    //(new XmlSerializer(type)).Serialize(cs, serializable);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public object ReadEncryptedBinFile(string filename)
        {
            // File does not exists, return null
            if (!File.Exists($"{BinFilesPath}\\{filename}.bin"))
                return null;

            // Init a symmetric algorithm and set Key and IV
            Rijndael cryptor = Rijndael.Create();
            cryptor.KeySize = 128;
            cryptor.Key = CypherKey;
            cryptor.IV = CypherIV;

            //DESCryptoServiceProvider cryptor = new DESCryptoServiceProvider()
            //{
            //    Key = CypherKey,
            //    IV = CypherIV
            //};

            try
            {
                using (FileStream fs = new FileStream($"{BinFilesPath}\\{filename}.bin", FileMode.Open, FileAccess.Read, FileShare.Read))
                using (CryptoStream cs = new CryptoStream(fs, cryptor.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    // Deserialize binary to .net object
                    return (new BinaryFormatter()).Deserialize(cs);
                    //return (new XmlSerializer(type)).Deserialize(cs);
                }
            
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}

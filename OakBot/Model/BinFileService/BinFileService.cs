using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OakBot.Model
{
    public class BinFileService : IBinFileService
    {
        private static readonly string BinFilesPath = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + "\\OakBot\\Bin";

        public BinFileService()
        {
            // Initialize bin directory if needed
            if (!Directory.Exists(BinFilesPath))
                Directory.CreateDirectory(BinFilesPath);
        }
        
        /// <summary>
        /// Write a serializable object to a binary file.
        /// </summary>
        /// <param name="filename">The name of the file, without extention, to be used.</param>
        /// <param name="serializable">A serializable tagged object to store in a binary file.</param>
        /// <returns>True upon successful serialization, False otherwise.</returns>
        public bool WriteBinFile(string filename, object serializable)
        {
            using (Stream stream = new FileStream($"{BinFilesPath}\\{filename}.bin",
                FileMode.Create, FileAccess.Write, FileShare.None))
            {
                IFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, serializable);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Read a binary file to a object with deserialization.
        /// </summary>
        /// <param name="filename">The name of the file, without extention, to be read.</param>
        /// <returns>Deserialized object, requires casting to proper .NET object.</returns>
        public object ReadBinFile(string filename)
        {
            try
            {
                using (Stream stream = new FileStream($"{BinFilesPath}\\{filename}.bin",
                FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    IFormatter formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}

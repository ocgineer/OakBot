using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.IO;

using OakBot.Model;

namespace OakBot.ViewModel
{
    public class SubDB
    {
        private static readonly string DBFilesPath = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + "\\OakBot\\DB";

        private SQLiteConnection _db;
        private object _lock;

        public SubDB()
        {
            _lock = new object();

            if (!Directory.Exists(DBFilesPath))
                Directory.CreateDirectory(DBFilesPath);

            string dbFile = $"{DBFilesPath}\\SubDB.sqlite";

            if (!File.Exists(dbFile))
            {
                try
                {
                    // Create database file
                    SQLiteConnection.CreateFile(dbFile);

                    // Connect and open the database file
                    SQLiteConnection subDB = new SQLiteConnection(
                        string.Format("Data Source={0};Version=3", dbFile));
                    subDB.Open();

                    // Create database version table and its members
                    SQLiteCommand sqlCmd;
                    sqlCmd = new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS `Subs` (" +
                            "`UserId` TEXT UNIQUE," +
                            "`Name` TEXT," +
                            "'Tier' INTEGER," +
                            "'New' TEXT," +
                            "PRIMARY KEY(UserId))", subDB);
                    sqlCmd.ExecuteNonQuery();

                }
                catch
                {

                }
            }
        }

        public void AddPerson(string username)
        {
            lock (_lock)
            {
                // Add person to database here, threading safe due to lock
            }
        }

        public void RemovePerson(string username)
        {
            lock (_lock)
            {
                // delete user by username, threading safe due to lock
            }
        }
    }
}

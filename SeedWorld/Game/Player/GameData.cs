using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    class GameData
    {
        /// Directory to store save data
        private readonly string saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            + "\\SeedWorld\\";

        private readonly string saveExt = ".db";

        /// <summary>
        /// Player save data
        /// </summary>
        private Player.PlayerData playerData;

        public Player.PlayerData Player
        {
            get { return playerData; }
        }

        /// <summary>
        /// Set serializable types to GameData
        /// </summary>
        public GameData() { }

        /// <summary>
        /// Load the game data from a specific filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool LoadGame(int loadNumber)
        {
            string fileName = saveFolder + "Player_" + loadNumber.ToString() + saveExt;

            bool success = true;
            if (File.Exists(fileName) == false) 
            { 
                Console.WriteLine("File does not exist: " + fileName); 
                return false; 
            }

            FileStream fs = new FileStream(fileName, FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Set new deserialized data
                if (fs.Length != 0)
                {
                    playerData = (Player.PlayerData)formatter.Deserialize(fs);
                }
                else
                {
                    Player.PlayerData data = new Player.PlayerData()
                    {
                        position = new Vector3(200, 150, -20),
                        orientation = 0
                    };
                    playerData = data;
                }
            }
            catch (SerializationException e)
            {
                success = false;
                Console.WriteLine("Failed to deserialize data. Reason: " + e.Message);
                throw;
            }
            finally { fs.Close(); }
            return success;
        }

        /// <summary>
        /// Save the game data with a specific save number
        /// </summary>
        public void SaveGame(int saveNumber)
        {
            // Save to CSIDL_LOCAL_APPDATA
            string fileName = saveFolder + "Player_" + saveNumber.ToString() + saveExt;
            bool success = File.Exists(fileName);
            
            if (success) { }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            
            success = true;
            FileStream fs = new FileStream(fileName, FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                if (playerData != null) formatter.Serialize(fs, playerData);
                // etc....
            }
            catch (SerializationException e)
            {
                success = false;
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally { fs.Close(); }
            // Finish save function
        }
    }
}

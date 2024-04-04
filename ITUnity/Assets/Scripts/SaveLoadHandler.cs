using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks; 

public class SaveLoadHandler : MonoBehaviour
{
    string savePath = "/savedgame.txt";

    public async Task SaveGameAsync(SavedGame savedGame)
    {
        string json = JsonConvert.SerializeObject(savedGame);

        await File.WriteAllTextAsync(Application.persistentDataPath + savePath , json); 
    }

    public async Task<SavedGame> LoadGameAsync()
    {
        string json = File.Exists(Application.persistentDataPath + savePath) ? await File.ReadAllTextAsync(Application.persistentDataPath + savePath) : "";

        // Deserialize JSON string back to array (optional)
        SavedGame savedGame = string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<SavedGame>(json);

        return savedGame; 
    }

    public void DeleteGame()
    {
        if(File.Exists(Application.persistentDataPath + savePath))
        {
            File.Delete(Application.persistentDataPath + savePath);
        }
    }
}

using UnityEngine;
using System.IO;
using System;
public class SaveLoadScript : MonoBehaviour
{
    public string saveFileName = "mansFails.json";

    [Serializable]
    public class GameData
    {
        public int character;
        public string characterName;
    }
    private GameData gameData = new GameData();

    public void SaveGame(int character, String characterName)
    {
        gameData.character = character;
        gameData.characterName = characterName;
        string json = JsonUtility.ToJson(gameData);

        File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, json);
        Debug.Log("Game Saved to " + Application.persistentDataPath + "/" + saveFileName);
    }
    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/" + saveFileName;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Game Loaded from " + path);
        }
        else
        {
            Debug.LogWarning("Save file not found in " + path);
        }
        
    }
}

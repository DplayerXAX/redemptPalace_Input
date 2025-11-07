using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string currentScene;
    public Vector3 playerPosition;
    public List<string> dialogueFlags = new(); 
}

//This script records players' choices and progresses and then save them into a file.
public class SaveManager : MonoBehaviour
{
    public List<string> dialogueFlags = new();
    public static SaveManager Instance { get; private set; }
    public static SaveData CurrentData = new();

    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Start()
    {
        Instance= this;
        LoadGame();
    }

    public static void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game Saved at: " + SavePath);
    }

    public static void LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            CurrentData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Game Loaded.");
        }
        else
        {
            CurrentData = new SaveData
            {
                currentScene = "StartingScene",
                playerPosition = new Vector3(0, 0, 0),
                dialogueFlags = new List<string>()
            };
            SaveGame();
        }
    }

    public static void ClearSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save Cleared.");
        }
    }
}

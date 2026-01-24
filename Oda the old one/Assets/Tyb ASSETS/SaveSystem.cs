using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string directory = Application.persistentDataPath + "/saves/";

    public static void Save(PlayerData data, int slotIndex)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(directory + "save_" + slotIndex + ".json", json);
    }

    public static PlayerData Load(int slotIndex)
    {
        string path = directory + "save_" + slotIndex + ".json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return null;
    }

    // Fonction bonus pour effacer un slot
    public static void DeleteSave(int slotIndex)
    {
        string path = directory + "save_" + slotIndex + ".json";
        if (File.Exists(path)) File.Delete(path);
    }
}

[System.Serializable]
public class PlayerData
{
    public int slotIndex;
    public string lastSceneName;
    public float x, y;
}
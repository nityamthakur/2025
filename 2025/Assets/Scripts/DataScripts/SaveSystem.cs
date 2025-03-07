// GameData class code pulled from Brackeys on YouTube
// https://www.youtube.com/watch?v=XOjd_qU2Ido

using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static string GetSaveFilePath(int slot)
    {
        return Application.persistentDataPath + $"/save_slot_{slot}.fun";
    }

    public static void SaveGame(int slot, GameData data)
    {
        BinaryFormatter formatter = new();
        string path = GetSaveFilePath(slot);

        FileStream stream = new(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log($"Game saved in slot {slot} at {path}");
    }

    public static GameData LoadGame(int slot)
    {
        string path = GetSaveFilePath(slot);
        
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new();
            FileStream stream = new(path, FileMode.Open);
            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();

            //Debug.Log($"Game loaded from slot {slot}");
            return data;
        }
        else
        {
            Debug.LogWarning($"No save file found in slot {slot}");
            return null;
        }
    }

    public static void DeleteSave(int slot)
    {
        string path = GetSaveFilePath(slot);
        
        if (File.Exists(path))
        {
            File.Delete(path); // Deletes the save file
            Debug.Log($"Game save deleted in slot {slot}");
        }
        else
            Debug.LogWarning($"No save file found in slot {slot}");
    }

    public static bool SaveExists(int slot)
    {
        return File.Exists(GetSaveFilePath(slot));
    }
}

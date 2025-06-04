// GameData class code pulled from Brackeys on YouTube
// https://www.youtube.com/watch?v=XOjd_qU2Ido

using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveSystem
{
    private static string GetSaveFilePath(int slot)
    {
        return Application.persistentDataPath + $"/save_slot_{slot}.save";
    }

    public static void SaveGame(int slot, GameData data)
    {
        if (slot == -1)
            return;

        BinaryFormatter formatter = new();
        string finalPath = GetSaveFilePath(slot);
        string tempPath = finalPath + ".tmp";

        try
        {
            using (FileStream stream = new(tempPath, FileMode.Create))
            {
                formatter.Serialize(stream, data);
            }

            // Overwrite the old file *only after* successful write
            if (File.Exists(finalPath))
                File.Delete(finalPath);
            File.Move(tempPath, finalPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed for slot {slot}: {e.Message}");
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }

        Debug.Log($"Save Successful for slot {slot}");
    }

    public static GameData LoadGame(int slot)
    {
        string path = GetSaveFilePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file in slot {slot}");
            return null;
        }

        FileInfo info = new(path);
        if (info.Length == 0)
        {
            Debug.LogWarning($"Save file in slot {slot} is empty. Deleting.");
            File.Delete(path);
            return null;
        }

        try
        {
            using FileStream stream = new(path, FileMode.Open);
            BinaryFormatter formatter = new();
            return formatter.Deserialize(stream) as GameData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load save in slot {slot}: {e.Message}");
            File.Delete(path); // optional — ensures game won’t crash again
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

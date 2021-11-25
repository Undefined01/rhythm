using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Settings
{
    public int BackgroundLightness { get; set; } = 100;
    public int HitSoundEffectVolumn { get; set; } = 100;
    public int HitOffsetMs { get; set; } = 0;
}

public class LevelRecord
{
    public string Name { get; set; }
    public bool Unlocked { get; set; }
    public int BestScore { get; set; }
    public bool AllCombo { get; set; }
    public bool AllPerfect { get; set; }
}

public class Save
{
    public int Version = 0;
    public Settings Settings { get; set; }
    public List<LevelRecord> Level { get; set; }

    public static Save Default()
    {
        var save = new Save();
        save.Settings = new Settings();
        save.Level = new List<LevelRecord>();
        save.Level.Add(new LevelRecord {
            Name = "chapter1_1_晴雪月夜",
            Unlocked = false,
            BestScore = 0,
            AllCombo = false,
            AllPerfect = false,
        });

        return save;
    }
}

public class SaveManager : MonoBehaviour
{
    static string SavePath;

    public static Save Save;

    void Awake()
    {
        SavePath = Application.dataPath + "/Save/save0.xml";
        if (!Directory.Exists(Application.dataPath + "/Save"))
            Directory.CreateDirectory(Application.dataPath + "/Save");
        LoadAll();
    }

    void OnDestroy()
    {
        SaveAll();
    }

    public static void SaveAll()
    {
        var bakPath = $"{SavePath}.bak";
        if (File.Exists(SavePath))
            File.Copy(SavePath, bakPath, true);
        try
        {
            using (var writer = new StreamWriter(SavePath))
            {
                var xz = new XmlSerializer(typeof(Save));
                xz.Serialize(writer, Save);
            }
        }
        catch (Exception)
        {
            if (File.Exists(bakPath))
                File.Copy(bakPath, SavePath, true);
            throw;
        }
    }

    public static void LoadAll()
    {
        if (!File.Exists(SavePath))
        {
            Save = Save.Default();
            SaveAll();
            return;
        }
        using (var reader = new StreamReader(SavePath))
        {
            var xz = new XmlSerializer(typeof(Save));
            Save = (Save)xz.Deserialize(reader);
        }
    }
}
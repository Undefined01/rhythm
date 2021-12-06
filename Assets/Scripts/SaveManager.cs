using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Settings
{
    public float BackgroundBrightness { get; set; } = 1;
    public float HitSoundEffectVolumn { get; set; } = 1;
    public float MusicVolumn { get; set; } = 1;
    public int HitOffsetMs { get; set; } = 0;
}

public class LevelRecord
{
    public string Name { get; set; }
    public string Author { get; set; }
    public int Chapter { get; set; }
    public int Id { get; set; }
    public int BestScore { get; set; }
    public bool FullCombo { get; set; }
    public bool AllPerfect { get; set; }
}

public class StoryRecord
{
    public int Chapter { get; set; }
    public bool Watched { get; set; }
}

public class Save
{
    public int Version = 0;
    public static int CurrentVersion = 2;
    public Settings Settings { get; set; }
    public List<LevelRecord> Level { get; set; }
    public List<StoryRecord> Story { get; set; }

    public static Save Default()
    {
        var save = new Save();
        save.Settings = new Settings();
        save.Version = CurrentVersion;

        save.Level = new List<LevelRecord>();
        save.Level.Add(new LevelRecord {
            Name = "别君赋",
            Author = "Newton-, 萌珑",
            Chapter = 1,
            Id = 1,
            BestScore = 0,
            FullCombo = false,
            AllPerfect = false,
        });
        save.Level.Add(new LevelRecord {
            Name = "舞步生风",
            Author = "带电的笛子Bana-X",
            Chapter = 1,
            Id = 2,
            BestScore = 0,
            FullCombo = false,
            AllPerfect = false,
        });
        save.Level.Add(new LevelRecord {
            Name = "晴雪月夜",
            Author = "ELE.K",
            Chapter = 2,
            Id = 3,
            BestScore = 0,
            FullCombo = false,
            AllPerfect = false,
        });
        save.Level.Add(new LevelRecord {
            Name = "筝鸣",
            Author = "RADI8, JuggShots, Reggie Yang",
            Chapter = 2,
            Id = 4,
            BestScore = 0,
            FullCombo = false,
            AllPerfect = false,
        });

        save.Story = new List<StoryRecord>();
        save.Story.Add(new StoryRecord {
            Chapter = 0,
            Watched = false,
        });
        save.Story.Add(new StoryRecord {
            Chapter = 1,
            Watched = false,
        });
        save.Story.Add(new StoryRecord {
            Chapter = 2,
            Watched = false,
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
        SavePath = Application.persistentDataPath + "/Save/save0.xml";
        if (!Directory.Exists(Application.persistentDataPath + "/Save"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Save");
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
        if (File.Exists(SavePath))
        {
            using (var reader = new StreamReader(SavePath))
            {
                var xz = new XmlSerializer(typeof(Save));
                Save = (Save)xz.Deserialize(reader);
            }
        }
        if ((Save?.Version ?? 0) < Save.CurrentVersion)
        {
            Save = Save.Default();
            SaveAll();
        }
    }
}
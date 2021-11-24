using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Settings
{
    public int HitSoundEffectVolumn { get; set; }
    public int HitOffset { get; set; }
}

public class LevelRecord
{
    public string Name;
    public bool Unlocked;
    public int BestScore;
    public bool AllCombo;
    public bool AllPerfect;
}

public class Save
{
    public Settings Settings;
    public List<LevelRecord> Level;
}

public static class SaveManager
{
    static readonly string SavePath = Application.dataPath + "/Save/save0.xml";

    public static Save Save;

    public static void SaveAll()
    {
        using (var writer = new StreamWriter(SavePath))
        {
            var xz = new XmlSerializer(typeof(Save));
            xz.Serialize(writer, Save);
        }
    }

    public static void LoadAll()
    {
        if (!File.Exists(SavePath))
        {
        }
        using (var reader = new StreamReader(SavePath))
        {
            var xz = new XmlSerializer(typeof(Save));
            Save = (Save)xz.Deserialize(reader);
        }
    }
}
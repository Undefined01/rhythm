using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;

public class StoryUI : UIController
{
    public UIManager UIManager;
    public Tachie Tachie1, Tachie2;
    public Text Text;

    int current = 0;

    List<Sentence> sentences;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.touches.Any(t => t.phase == TouchPhase.Began))
        {
            Step();
        }
    }

    public override void OnEnter(UIController prevUi, string param)
    {
        Debug.Log($"Loading story {param}");
        var StoryAsset = (TextAsset)Resources.Load($"Stories/{param}");
        var StoryStr = System.Text.Encoding.UTF8.GetBytes(StoryAsset.text);
        using (var reader = new System.IO.MemoryStream(StoryStr))
        {
            var xz = new XmlSerializer(typeof(List<Sentence>));
            sentences = (List<Sentence>)xz.Deserialize(reader);
        }
        current = 0;
        Step();
    }

    void Step()
    {
        if (current >= sentences.Count) {
            UIManager.SwitchToUi("Main");
            return;
        }

        var sentence = sentences[current];

        Tachie1.SetIsSpeaking(false);
        Tachie2.SetIsSpeaking(true);
        Tachie2.SetTachie(sentence.Speaker);
        var tmp = Tachie1;
        Tachie1 = Tachie2;
        Tachie2 = tmp;

        Text.text = sentence.Speaker + "\n" + sentence.Texts;
        current++;
    }

    public class Sentence
    {
        public string Speaker;
        public string Texts;
    }
}

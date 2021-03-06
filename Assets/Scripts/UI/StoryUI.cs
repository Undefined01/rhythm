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
    public Text SpeakerText;
    public TextTyper ContentText;

    public List<Sprite> Backgrounds;

    int chapter;
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
        base.OnEnter(prevUi, param);

        Debug.Log($"Loading story {param}");
        chapter = int.Parse(param);
        var StoryAsset = (TextAsset)Resources.Load($"Stories/{param}");
        using (var reader = new System.IO.MemoryStream(StoryAsset.bytes))
        {
            var xz = new XmlSerializer(typeof(List<Sentence>));
            sentences = (List<Sentence>)xz.Deserialize(reader);
        }
        current = 0;
        Tachie1.CleanUp();
        Tachie2.CleanUp();
        Step();
    }

    void Step()
    {
        if (current >= sentences.Count)
        {
            var story = SaveManager.Save.Story.Single(x => x.Chapter == chapter);
            var watchedBefore = story.Watched;
            story.Watched = true;
            SaveManager.SaveAll();

            if (chapter == 0 && !watchedBefore)
                UIManager.SwitchToUi("Tutorial");
            else
                UIManager.SwitchToUi("Main");
            return;
        }

        var sentence = sentences[current];
        if (sentence.Background != null)
        {
            UIManager.ChangeBackgroundImage(Backgrounds.Single(x => x.name == sentence.Background));
        }
        if (sentence.Speaker == "旁白")
        {
            Tachie1.SetIsSpeaking(false);
            SpeakerText.text = "";
            ContentText.TypeText(sentence.Texts, 0.05f);
            current++;
            return;
        }

        if (Tachie1.TachieName != sentence.Speaker)
        {
            var tmp = Tachie1;
            Tachie1 = Tachie2;
            Tachie2 = tmp;
        }
        Tachie1.SetIsSpeaking(true);
        Tachie1.TachieName = sentence.Speaker;
        Tachie2.SetIsSpeaking(false);

        SpeakerText.text = sentence.Speaker;
        ContentText.TypeText(sentence.Texts, 0.05f);
        current++;
    }

    public class Sentence
    {
        public string Speaker;
        public string Texts;
        public string Background;
    }
}

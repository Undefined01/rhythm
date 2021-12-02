using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
class UILocker : MonoBehaviour
{
    public int RequireChapterPassed = 0;
    public int RequireStoryWatched = 0;

    public UnityEvent OnClick;

    public Material Gray;

    private Image image;

    void Start()
    {
        var button = this.GetComponent<Button>();
        button.onClick.AddListener(this.Pressed);
        image = this.GetComponentInChildren<Image>();
    }

    void OnGUI()
    {
        if (CheckCondition())
            image.material = null;
        else
            image.material = Gray;
    }

    public bool CheckCondition()
    {
        var levelPassed = SaveManager.Save.Level.Any(x => x.Chapter >= RequireChapterPassed && x.BestScore >= 500_000);
        var storyWatched = SaveManager.Save.Story.Any(x => x.Chapter >= RequireStoryWatched && x.Watched);
        levelPassed |= RequireChapterPassed <= 0;
        storyWatched |= RequireStoryWatched < 0;
        return levelPassed && storyWatched;
    }

    public void Pressed()
    {
        if (CheckCondition())
            OnClick?.Invoke();
    }
}
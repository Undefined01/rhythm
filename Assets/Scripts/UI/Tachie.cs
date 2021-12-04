using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Tachie : MonoBehaviour
{
    [SerializeField]
    private Material GrayMaterial;

    [SerializeField]
    private List<Sprite> Tachies;

    private Image TachieImage;

    private string _Tachie;
    public string TachieName
    {
        get => _Tachie;
        set {
            var img = Tachies.SingleOrDefault(x => x.name == value);
            _Tachie = value;
            TachieImage.sprite = img ?? Tachies[0];
        }
    }

    void Awake()
    {
        TachieImage = this.GetComponent<Image>();
    }

    public void SetIsSpeaking(bool isSpeaking)
    {
        if (isSpeaking)
            TachieImage.material = null;
        else
            TachieImage.material = GrayMaterial;
    }

    public void CleanUp() {
        TachieName = "empty";
    }
}

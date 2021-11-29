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

    void Start()
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

    public void SetTachie(string image) {
        var img = Tachies.Single(x => x.name == image);
        TachieImage.sprite = img;
    }
}

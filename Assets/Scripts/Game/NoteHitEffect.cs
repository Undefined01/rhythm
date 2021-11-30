using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHitEffect : MonoBehaviour
{
    float TimeToLive = 3;

    void Start() {
        var audio = this.GetComponent<AudioSource>();
        audio.volume = (float)SaveManager.Save.Settings.HitSoundEffectVolumn / 100f;
    }

    void FixedUpdate()
    {
        TimeToLive -= Time.deltaTime;
        if (TimeToLive <= 0)
            GameObject.Destroy(this.gameObject);
    }
}

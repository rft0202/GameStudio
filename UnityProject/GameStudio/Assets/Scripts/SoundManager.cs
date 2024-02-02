using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static GameObject instance;

    [SerializeField]
    AudioSource[] srcs;
    public AudioClip[] sfxs;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = gameObject;
            DontDestroyOnLoad(instance);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySFX(AudioClip clip, float pitch=1)
    {
        int srcNum = FirstOpenSrc();
        srcs[srcNum].pitch = pitch;
        srcs[srcNum].PlayOneShot(clip);
    }

    public void EndAllSFX()
    {
        for (int i = 0; i < srcs.Length; i++)
        {
            srcs[i].Stop();
        }
    }

    public void PauseAllSFX(bool pause)
    {
        for(int i=0; i<srcs.Length; i++)
        {
            if (pause) srcs[i].Pause(); else srcs[i].UnPause();
        }
    }

    int FirstOpenSrc()
    {
        int ind = 0;
        foreach (AudioSource _src in srcs)
        {
            if (!_src.isPlaying) return ind;
            ind++;
        }
        return ind;
    }
}

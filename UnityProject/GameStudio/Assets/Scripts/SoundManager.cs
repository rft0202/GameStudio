using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static GameObject instance;

    [SerializeField]
    AudioSource[] srcs;
    [SerializeField]
    AudioClip[] sfxs;

    float[] sfxsStartTimes = {
    0,0,0.2f,0.2f,0
    };
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

    public void PlaySFX(int clipNum ,float pitch=1)
    {
        int srcNum = FirstOpenSrc();
        AudioSource src = srcs[srcNum];
        src.pitch = pitch;
        src.clip = sfxs[clipNum];
        if(!src.isPlaying)src.Play();
        src.time = sfxsStartTimes[clipNum];
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

using DanmakU.Fireables;
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
    0,0,0.025f,0.2f,0,0.19f,0.2f,0,0,0
    };
    float[] sfxsVolumes = {
    1,1,1,0.8f,1,.7f,1,0.7f,0.4f,0.7f
    };
    int openSrcCnt = 0;
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


    public void PlaySFX(int clipNum ,float pitch=1)
    {
        int srcNum = openSrcCnt;
        AudioSource src = srcs[srcNum];
        src.pitch = pitch;
        src.clip = sfxs[clipNum];
        src.volume = sfxsVolumes[clipNum];
        if(!src.isPlaying)src.Play();
        src.time = sfxsStartTimes[clipNum];
        openSrcCnt++;
        if (openSrcCnt >= srcs.Length) openSrcCnt = 0;
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

    public void ToggleSounds(bool on)
    {
        if (on)
        {
            sfxsVolumes = new[]{
            1f,1,1,0.8f,1,.7f,1,0.7f,0.4f,0.7f
            };
        }
        else
        {
            sfxsVolumes = new[]{
                0f,0,0,0,0,0,0,0,0,0
            };
        }
    }
}

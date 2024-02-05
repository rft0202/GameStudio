using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicScript : MonoBehaviour
{
    //Member variables
    public string[] playsOnScenes;
    public string[] startOnScenes;
    public float maxVol = 1;
    public float fadeRate = 0.1f;
    float targetVol = 1;
    public float loopStart = 0, loopEnd = 0;
    public float songStart = 0;

    private void Start()
    {
        GetComponent<AudioSource>().time = songStart;
    }

    //Public Methods

    public void checkAudio()
    {
        for (int i = 0; i < startOnScenes.Length; i++)
        {
            if (SceneManager.GetActiveScene().name == startOnScenes[i])
            {
                AudioSource _audio = gameObject.GetComponent<AudioSource>();
                if (!_audio.loop)
                    _audio.PlayOneShot(_audio.clip);
                _audio.time = songStart;
                break;
            }
        }
        float _vol = 0;
        for (int i = 0; i < playsOnScenes.Length; i++)
        {
            if (SceneManager.GetActiveScene().name == playsOnScenes[i])
            {
                _vol = maxVol;
                break;
            }
        }
        targetVol = _vol;
        StartCoroutine(fadeVol());
    }

    public void changeVol(float _newVol)
    {
        targetVol = _newVol;
        StartCoroutine(fadeVol());
    }

    public void tempFade(float _fadeToVol, float _afterFadeVol, int _waitSeconds)
    {
        StartCoroutine(TempFade(_fadeToVol,_afterFadeVol,_waitSeconds));
    }

    public void Enter()
    {
        changeVol(maxVol);
    }

    public void Exit()
    {
        changeVol(0);
    }

    //Private Stuff

    private void Update()
    {
        if (loopEnd != 0)
        {
            if (gameObject.GetComponent<AudioSource>().time >= loopEnd)
                gameObject.GetComponent<AudioSource>().time = loopStart;
        }
    }

    IEnumerator fadeVol()
    {
        yield return new WaitForSeconds(fadeRate);
        if (gameObject.GetComponent<AudioSource>().volume > targetVol)
        {
            gameObject.GetComponent<AudioSource>().volume -= fadeRate;
        }
        else if (gameObject.GetComponent<AudioSource>().volume < targetVol)
        {
            gameObject.GetComponent<AudioSource>().volume += fadeRate;
        }

        if (gameObject.GetComponent<AudioSource>().volume != targetVol)
            StartCoroutine(fadeVol());

    }

    IEnumerator TempFade(float _before, float _after, int _wait)
    {
        changeVol(_before);
        yield return new WaitForSeconds(_wait);
        changeVol(_after);
    }
}

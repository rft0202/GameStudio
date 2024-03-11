using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static GameObject instance;
    private int prevScene = 0;
    bool musicOn = true;
    public bool MusicOn { 
        get { 
            return musicOn;
        }
        set {
            musicOn = value;
            if (!value)
            {
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    gameObject.transform.GetChild(i).GetComponent<MusicScript>().Exit();
                }
            }
            checkAudios();
        }
    }
    // Start is called before the first frame update
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

    private void Start()
    {
        checkAudios();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != prevScene)
        {
            checkAudios();
            prevScene = SceneManager.GetActiveScene().buildIndex;
        }
    }

    private void checkAudios()
    {
        if(MusicOn)
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).GetComponent<MusicScript>().checkAudio();
        }
    }
}

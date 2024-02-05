using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Persist through scenes
    public static GameObject instance;
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

    //Score variables
    int[] highscores = {0,0,0,0}; //highscores for the 3 levels
    int currLevel = -1; //0-TestScene, 1-Level1, 2-level2, 3-level3
    int activeScore=0; //Current score while playing level

    //Prev scene vars
    string prevSceneN = "";
    Scene prevSceneS;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene() != prevSceneS) SceneChanged();
    }

    void SceneChanged()
    {
        prevSceneS = SceneManager.GetActiveScene();
        prevSceneN = prevSceneS.name;
        Debug.Log("Scene changed");
        switch (prevSceneN)
        {
            case ("TestScene"): BeganLevel(0); break;
            case ("LevelComplete"): LevelCompleted(); break;
            case ("GameOver"): LevelLost(); break;
            default: break;
        }
    }

    public void AddToActiveScore(int scoreAmt)
    {
        activeScore += scoreAmt;
        Debug.Log("score += "+scoreAmt+", Total: "+activeScore);
    }

    void BeganLevel(int lvlNum)
    {
        Debug.Log("Began level " + lvlNum);
        activeScore = 0;
        currLevel = lvlNum;
    }

    void LevelCompleted()
    {
        if (activeScore >= highscores[currLevel])
        {
            highscores[currLevel] = activeScore;
            Debug.Log("new highscore for level " + currLevel + ": " + activeScore);
        }
    }

    void LevelLost()
    {
        activeScore = 0;
    }
    
}

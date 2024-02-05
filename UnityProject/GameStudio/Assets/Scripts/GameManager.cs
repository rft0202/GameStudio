using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


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

    //PUBLIC VARS
    public GameObject scorePopup, newHighscoreTxt;

    //Score variables
    int[] highscores = {0,0,0,0}; //highscores for the 3 levels
    int currLevel = -1; //0-TestScene, 1-Level1, 2-level2, 3-level3
    int activeScore=0; //Current score while playing level
    TMP_Text scoreLbl;

    //Prev scene vars
    string prevSceneN = "";
    Scene prevSceneS;

    //Score popups
    List<GameObject> scorePopups = new();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Detect screen changes
        if (SceneManager.GetActiveScene() != prevSceneS) SceneChanged();

        //Move score popups
        for(int i=0; i<scorePopups.Count; i++)
        {
            while (scorePopups[i] == null) { if (i >= scorePopups.Count) break; i++; } //just in case
            scorePopups[i].transform.Translate(Vector3.up * Time.deltaTime); //move score popups up!
        }
    }

    void SceneChanged()
    {
        prevSceneS = SceneManager.GetActiveScene();
        prevSceneN = prevSceneS.name;
        Debug.Log("Scene changed");
        if (GameObject.Find("scoreLbl") != null)
            scoreLbl = GameObject.Find("scoreLbl").GetComponent<TMP_Text>();
        switch (prevSceneN)
        {
            case ("TestScene"): BeganLevel(0); break;
            case ("LevelComplete"): LevelCompleted(); break;
            case ("GameOver"): LevelLost(); break;
            default: break;
        }
    }

    public void AddToActiveScore(int scoreAmt, Vector2 popupLocation)
    {
        //Increase score and update level score label
        activeScore += scoreAmt;
        setScoreText();

        //make score popup
        GameObject _popup = Instantiate(scorePopup);
        TMP_Text popTxt = _popup.GetComponent<TMP_Text>();
        popTxt.text = ((scoreAmt>=0)?("+"):("-")) + scoreAmt;
        _popup.transform.position = new Vector3(popupLocation.x, popupLocation.y);
        scorePopups.Add(_popup);
        StartCoroutine(DestroyPopup(_popup));
    }

    IEnumerator DestroyPopup(GameObject p)
    {
        yield return new WaitForSeconds(0.5f);
        scorePopups.Remove(p);
        Destroy(p);
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
            Instantiate(newHighscoreTxt); //if new highscore in level, add little popup text
        }
        //Show score after finishing level
        setScoreText();
    }

    void LevelLost()
    {
        activeScore = 0;
    }

    void setScoreText()
    {
        if (activeScore > 999) //Score always has 4 digits, 10 - bad, 0010 - good
            scoreLbl.text = "" + activeScore; //scorelbl should be called "scoreLbl" in scenes
        else if (activeScore > 99)
            scoreLbl.text = "0" + activeScore;
        else if (activeScore > 9)
            scoreLbl.text = "00" + activeScore;
        else
            scoreLbl.text = "000" + activeScore;
    }
    
}

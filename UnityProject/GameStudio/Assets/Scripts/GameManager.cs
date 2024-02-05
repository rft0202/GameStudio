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
    public GameObject scorePopup;


    //Score variables
    int[] highscores = {0,0,0,0}; //highscores for the 3 levels
    int currLevel = -1; //0-TestScene, 1-Level1, 2-level2, 3-level3
    int activeScore=0; //Current score while playing level

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
            while (scorePopups[i] == null) i++; //just in case
            scorePopups[i].transform.Translate(Vector3.up * Time.deltaTime); //move score popups up!
        }
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

    public void AddToActiveScore(int scoreAmt, Vector2 popupLocation)
    {
        activeScore += scoreAmt;
        Debug.Log("score += "+scoreAmt+", Total: "+activeScore);

        //make score popup
        GameObject _popup = Instantiate(scorePopup);
        TMP_Text popTxt = _popup.GetComponent<TMP_Text>();
        popTxt.text = "+" + scoreAmt;
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
            Debug.Log("new highscore for level " + currLevel + ": " + activeScore);
        }
    }

    void LevelLost()
    {
        activeScore = 0;
    }
    
}

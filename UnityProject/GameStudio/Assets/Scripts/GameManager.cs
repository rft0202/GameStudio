using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

using System.Data;
using System.Data.SqlClient;

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
    [NonSerialized]
    public int[] highscores = {0,0,0,0}; //highscores for the 3 levels
    int currLevel = -1; //0-TestScene, 1-Level1, 2-level2, 3-level3
    int activeScore=0; //Current score while playing level
    TMP_Text scoreLbl;

    //Prev scene vars
    string prevSceneN = "";
    Scene prevSceneS;

    //Score popups
    List<GameObject> scorePopups = new();

    //Leaderboard stuff
    [NonSerialized]
    public bool connectedToLeaderboard = false;
    [NonSerialized]
    public string playerName="AAA";
    [NonSerialized]
    public bool nameSet = false;
    [NonSerialized]
    public int playerID; //Setup if connected, stays same for session, eventually add to player prefs
    [NonSerialized]
    public List<List<List<string>>> localLeaderboardStorage = new(); //level - record - name/score

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("pName")) {
            playerName = PlayerPrefs.GetString("pName", playerName);
            connectedToLeaderboard = PlayerPrefs.GetString("addToLeaderboard")=="TRUE";
            playerID = PlayerPrefs.GetInt("pID");
            nameSet = true;
        }
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
            case ("Tutorial"): BeganLevel(0); break;
            case ("Level1"): BeganLevel(1); break;
            case ("Level2"): BeganLevel(2); break;
            case ("Level3"): BeganLevel(3); break;
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
        if (activeScore > highscores[currLevel])
        {
            highscores[currLevel] = activeScore;
            Instantiate(newHighscoreTxt); //if new highscore in level, add little popup text

            //If allowing share to leaderboard, update record on leaderboard
            if (connectedToLeaderboard)
            {
                UpdateRecord(playerID, currLevel, convertScoreToString(activeScore));
            }
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
        scoreLbl.text = convertScoreToString(activeScore);
    }

    public string convertScoreToString(int _scr)
    {
        if (_scr > 999) //Score always has 4 digits, 10 - bad, 0010 - good
            return "" + _scr; //scorelbl should be called "scoreLbl" in scenes
        else if (_scr > 99)
            return "0" + _scr;
        else if (_scr > 9)
            return "00" + _scr;

        return "000" + _scr;
    }


    private string GetConnected()
    {
        return @"Server=sql.neit.edu\studentsqlserver,4500;Database=Dev_MSatterfield;User Id=Dev_MSatterfield;Password=008016596";
    }
    //Update record script (Important: always use update after nameSetup for a session, eventually add playerprefs to update between sessions and option to reset player data)
    public string UpdateRecord(int pID, int lvl, string highscore)
    {
        string strResult = "";

        SqlConnection Conn = new SqlConnection();

        Conn.ConnectionString = @GetConnected();

        string strSQL = "UPDATE GameStudio_Highscores SET Score=@score WHERE Player_ID=@pID AND Lvl=@lvl";

        SqlCommand comm = new SqlCommand();
        comm.CommandText = strSQL;
        comm.Connection = Conn;

        comm.Parameters.AddWithValue("@score", highscore);
        comm.Parameters.AddWithValue("@pID", pID);
        comm.Parameters.AddWithValue("@lvl", lvl);

        try
        {
            Conn.Open();
            int intRecs = comm.ExecuteNonQuery();
            Conn.Close();
            strResult = $"SUCCESS: Inserted {intRecs} records.";
        }
        catch (Exception e)
        {
            strResult = "ERROR: " + e.Message;
        }

        return strResult;
    }

}

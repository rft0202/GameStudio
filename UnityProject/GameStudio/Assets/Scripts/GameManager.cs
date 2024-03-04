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
    public GameObject screenCurtain;
    RectTransform curtain;
    bool justStarted = true;

    //Enemies to kill until boss appears
    public int enemiesToKill = 0;

    //Score variables
    [NonSerialized]
    public int[] highscores = {0,0,0,0}; //highscores for the 3 levels
    public int currLevel = -1; //0-TestScene, 1-Level1, 2-level2, 3-level3
    int activeScore=0; //Current score while playing level
    TMP_Text scoreLbl;
    float lvlStartTime, lvlEndTime;
    float[] targetLvlCompleteTime = { 60, 60, 70, 80 }; //Target complete time for players (in seconds)
    [NonSerialized]
    public int livesLeft = 0;
    int healthBonusAmt = 150;

    //Prev scene vars
    [NonSerialized]
    public string actualPrevScene = "";
    string prevSceneN = "";
    Scene prevSceneS;

    //Score popups
    List<GameObject> scorePopups = new();
    List<Coroutine> scorePopupCoroutines = new();
    [NonSerialized]
    public bool chargeHelp =true;

    //Leaderboard stuff
    [NonSerialized]
    public bool connectedToLeaderboard = false;
    [NonSerialized]
    public string playerName="AAA";
    [NonSerialized]
    public bool nameSet = false;
    [NonSerialized]
    public int playerID=-1; //Setup if connected, stays same for session, eventually add to player prefs
    [NonSerialized]
    public List<List<List<string>>> localLeaderboardStorage = new(); //level - record - name/score

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("pName")) {
            playerName = PlayerPrefs.GetString("pName");
            Debug.Log(playerName);
            connectedToLeaderboard = PlayerPrefs.GetString("addToLeaderboard")=="TRUE";
            playerID = PlayerPrefs.GetInt("pID");
            highscores[1] = PlayerPrefs.GetInt("h1");
            highscores[2] = PlayerPrefs.GetInt("h2");
            highscores[3] = PlayerPrefs.GetInt("h3");
            nameSet = true;
        }
        curtain = screenCurtain.GetComponent<RectTransform>();
        curtain.localPosition = new Vector2(0, 450);
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
        //prevents errors with popups that were destroyed on prev screen
        clearPopUps();
        changeSceneEnd();

        actualPrevScene = prevSceneN;
        prevSceneS = SceneManager.GetActiveScene();
        prevSceneN = prevSceneS.name;
        //Debug.Log("Scene changed");
        //If theres score label, put score
        if (GameObject.Find("scoreLbl") != null)
            scoreLbl = GameObject.Find("scoreLbl").GetComponent<TMP_Text>();
        //Do something depending on which scene we changed to
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

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(changeScene1(sceneName));
    }

    IEnumerator changeScene1(string _n)
    {
        float _t = 0.25f;
        float cnt = 0;
        while (cnt < _t)
        {
            curtain.localPosition = Vector3.Slerp(curtain.localPosition, Vector3.zero, (cnt / _t));
            cnt += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene(_n);
    }

    void changeSceneEnd()
    {
        if (!justStarted)
        {
            StartCoroutine(changeScene2());
        }
        else{
            justStarted = false;
        }
    }

    IEnumerator changeScene2()
    {
        float _t = 0.25f;
        float cnt = 0;
        while (cnt < _t)
        {
            curtain.localPosition = Vector3.Slerp(curtain.localPosition, Vector3.down*450, (cnt / _t));
            cnt += Time.deltaTime;
            yield return null;
        }
        curtain.localPosition = new Vector2(0,450);
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
        scorePopupCoroutines.Add(StartCoroutine(DestroyPopup(_popup)));
    }

    IEnumerator DestroyPopup(GameObject p)
    {
        yield return new WaitForSeconds(0.5f);
        scorePopups.Remove(p);
        Destroy(p);
    }

    void BeganLevel(int lvlNum)
    {
        //Debug.Log("Began level " + lvlNum);
        activeScore = 0;
        currLevel = lvlNum;
        lvlStartTime = Time.time;
    }

    void LevelCompleted()
    {
        TMP_Text hpLbl=GameObject.Find("hpLbl").GetComponent<TMP_Text>(), timeLbl = GameObject.Find("timeLbl").GetComponent<TMP_Text>(),
            ogScoreLbl = GameObject.Find("ogScoreLbl").GetComponent<TMP_Text>();

        int ogScore = activeScore;
        ogScoreLbl.text = "Score: " + activeScore;

        //Apply time bonus
        lvlEndTime = Time.time; //Get lvl end time
        float timeBonus = calculateScoreTimeBonus(currLevel); //Calc timeBonus

        //Debug.Log(timeBonus); //for testing
        //Add on lives bonus (more health left = higher score)
        int livesBonus = calculateLivesBonus();
        activeScore += livesBonus;

        if (timeBonus < 0) { timeBonus = 0; } //Lets not take away points from player
        activeScore += (int)(activeScore * timeBonus); //Add bonus

        

        if (activeScore > highscores[currLevel])
        {
            highscores[currLevel] = activeScore;
            string _l = "h" + currLevel;
            PlayerPrefs.SetInt(_l, activeScore);
            Instantiate(newHighscoreTxt); //if new highscore in level, add little popup text

            //If allowing share to leaderboard, update record on leaderboard
            if (connectedToLeaderboard)
            {
                UpdateRecord(playerID, currLevel, convertScoreToString(activeScore));
            }
        }
        //Show score after finishing level
        timeBonus = Mathf.Floor(timeBonus*(livesBonus+ogScore)); //round to nearest hundrenth
        lvlEndTime = Mathf.Round(lvlEndTime*100f)*0.01f; //round to nearest hundrenth
        hpLbl.text = "Health: " + livesLeft+" (+"+livesBonus+")";
        timeLbl.text = "Time: " + (lvlEndTime-lvlStartTime) + "s (+" + timeBonus+")";
        setScoreText();
    }

    int calculateLivesBonus()
    {
        return healthBonusAmt*livesLeft;
    }

    void LevelLost()
    {
        activeScore = 0;
    }

    void setScoreText()
    {
        scoreLbl.text = convertScoreToString(activeScore);
    }

    float calculateScoreTimeBonus(int _lvl) //Returns percent (0-1) to add to score, based on complete time
    {
        float pTime = lvlEndTime - lvlStartTime;
        float lvlTargetTime = targetLvlCompleteTime[_lvl];
        //(s-x)/s
        return (lvlTargetTime - pTime) / lvlTargetTime; //Will return negative if player took longer than target time
    }

    public string convertScoreToString(int _scr)
    {
        if (_scr >= 9999) return "9999";
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
        comm.Parameters.AddWithValue("@pID", pID.ToString());
        comm.Parameters.AddWithValue("@lvl", lvl.ToString());

        try
        {
            Conn.Open();
            int intRecs = comm.ExecuteNonQuery();
            Conn.Close();
            strResult = $"SUCCESS: Inserted {intRecs} records.";
        }
        catch (Exception e)
        {
            Conn.Close();
            strResult = "ERROR: " + e.Message;
        }

        return strResult;
    }

    void clearPopUps()
    {
        //This prevents errors w/ popups (literally its only purpose)
        foreach (Coroutine crt in scorePopupCoroutines)
        {
            StopCoroutine(crt);
        }
        scorePopupCoroutines.Clear();
    }

    public string stringHighscores(int index)
    {
        return convertScoreToString(highscores[index]);
    }

    public SqlDataReader FindOnePlayer(int pID, int lvl)
    {
        SqlConnection conn = new SqlConnection();
        SqlCommand comm = new SqlCommand();

        conn.ConnectionString = @GetConnected();
        comm.Connection = conn;
        comm.CommandText = "SELECT * FROM GameStudio_Highscores WHERE Player_ID = @pID AND Lvl=@lvl;";
        comm.Parameters.AddWithValue("@pID", pID.ToString());
        comm.Parameters.AddWithValue("@lvl", lvl.ToString());

        conn.Open();
        return comm.ExecuteReader();
    }

}

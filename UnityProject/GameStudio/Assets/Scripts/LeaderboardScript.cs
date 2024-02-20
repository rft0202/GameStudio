using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using System.Data;
using System.Data.SqlClient;
using System;
using UnityEngine.UI;

public class LeaderboardScript : MonoBehaviour
{
    //PUBLIC VARS
    public Toggle connectedToggle;
    public GameObject nameGroup, scoreGroup;
    public GameObject connectError;
    public GameObject dataMemberTxtPrefab;
    public TMP_Text selectedLvlTxt;
    public TMP_InputField nameInp;

    //PRIVATE VARS
    //GameObject leaderboard; THIS gameobject
    GameManager gm;
    SoundManager sm;
    bool connectedToLeaderboard = false, connectionError=false;
    int selectedLevel = 1;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        nameInp.text = gm.playerName;
        connectedToLeaderboard = gm.connectedToLeaderboard;
        connectedToggle.isOn = connectedToLeaderboard;
        RefreshLeaderboards();
        if(!connectionError)ReadLeaderboard(1);
    }

    public void RefreshLeaderboards()
    {
        gm.localLeaderboardStorage.Clear();
        LoadLeaderboards();
        if(!connectionError)SortLeaderboards();
    }

    public void LevelChange(bool right)
    {
        sm.PlaySFX(4,(right)?1.05f:0.95f);
        //Change selected level
        selectedLevel += (right) ? (1) : (-1);
        if (selectedLevel > 3) selectedLevel = 1;
        else if (selectedLevel<1) selectedLevel = 3;
        //Update text and leaderboard
        ReadLeaderboard(selectedLevel);
        selectedLvlTxt.text = "Level "+selectedLevel.ToString();
    }

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        connectedToLeaderboard = false;
        gm.nameSet = false;
        gm.connectedToLeaderboard = false;
        connectedToggle.isOn = false;
        nameInp.text = "";
        gm.playerID = -1;
        gm.playerName = "";
        gm.highscores[1] = 0;
        gm.highscores[2] = 0;
        gm.highscores[3] = 0;
    }

    public void ConnectToggle()
    {
        sm.PlaySFX(6, UnityEngine.Random.Range(0.95f, 1.05f));
        connectedToLeaderboard = connectedToggle.isOn;
        gm.connectedToLeaderboard = connectedToLeaderboard;
        if (connectedToLeaderboard && gm.playerID==-1) //Player needs to set their name to add to leaderboard
        {
            //Set player name
            if (nameInp.text == "") nameInp.text = "AAA";
            gm.playerName = nameInp.text;
            gm.nameSet = true;
            PlayerPrefs.SetString("pName", gm.playerName);

            //Assign new playerID
            SqlDataReader dr = SearchRecords_DR(1);
            if (dr != null)
            {
                dr.Close();
                SqlConnection Conn = new SqlConnection();
                Conn.ConnectionString = @GetConnected();
                string strSQL = "SELECT COUNT(*) FROM GameStudio_Highscores";
                SqlCommand comm = new SqlCommand();
                comm.CommandText = strSQL;
                comm.Connection = Conn;
                int cnt;
                try
                {
                    Conn.Open();
                    cnt = (int)comm.ExecuteScalar();
                    Conn.Close();
                    gm.playerID = cnt;
                }catch(Exception e)
                {
                    Conn.Close();
                    Debug.Log(e);
                    gm.playerID = 1;
                }

                PlayerPrefs.SetInt("pID", gm.playerID);
            }
            //Add records for lvls 1-3
            AddRecord(1);
            AddRecord(2);
            AddRecord(3);
            RefreshLeaderboards();
        }
        PlayerPrefs.SetString("addToLeaderboard", (connectedToLeaderboard) ? ("TRUE") : ("FALSE"));
    }

    private string GetConnected()
    {
        return @"Server=sql.neit.edu\studentsqlserver,4500;Database=Dev_MSatterfield;User Id=Dev_MSatterfield;Password=008016596";
    }

    public string AddRecord(int lvl)
    {
        string strResult = "";

        SqlConnection Conn = new SqlConnection();

        Conn.ConnectionString = @GetConnected();

        string strSQL = "INSERT INTO GameStudio_Highscores (Player_ID, Name, Lvl, Score) VALUES (@pID, @name, @lvl, @score)";

        SqlCommand comm = new SqlCommand();
        comm.CommandText = strSQL;
        comm.Connection = Conn;

        comm.Parameters.AddWithValue("@pID", gm.playerID.ToString());
        comm.Parameters.AddWithValue("@name", gm.playerName);
        comm.Parameters.AddWithValue("@lvl", lvl.ToString());
        comm.Parameters.AddWithValue("@score", gm.stringHighscores(lvl));

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
            Conn.Close();
        }
        Debug.Log(strResult);

        return strResult;
    }

    public SqlDataReader SearchRecords_DR(int lvl)
    {
        SqlDataReader dr;
        SqlCommand comm = new SqlCommand();

        string strSql = "SELECT Name, Score FROM GameStudio_Highscores WHERE Lvl=@lvl";

        comm.Parameters.AddWithValue("@lvl", lvl.ToString());

        SqlConnection conn = new SqlConnection();

        conn.ConnectionString = GetConnected();

        comm.Connection = conn;
        comm.CommandText = strSql;
        try
        {
            conn.Open();
            dr = comm.ExecuteReader();
            return dr;
        }catch(Exception e)
        {
            conn.Close();
            Debug.Log("Sql error: "+e.Message);
            connectionError = true;
            connectError.SetActive(true);
            connectError.transform.GetChild(0).GetComponent<TMP_Text>().text = "connection error: " + e.Message;
            gameObject.SetActive(false);
        }
        return null;

    }

    public void LoadLeaderboards()
    {
        for (int i = 0; i < 3; i++) //Each level
        {
            gm.localLeaderboardStorage.Add(new()); //add new level section
            int cnt = 0;
            SqlDataReader dr = SearchRecords_DR(i+1);
            if (dr == null) break;
            while (dr.Read())
            {
                gm.localLeaderboardStorage[i].Add(new()); //add new record

                //Enter record's name into local storage
                gm.localLeaderboardStorage[i][cnt].Add(dr["Name"].ToString());

                //Enter record's score into local storage
                gm.localLeaderboardStorage[i][cnt].Add(dr["Score"].ToString());
                cnt++;
            }
        }
    }

    public void ReadLeaderboard(int _lvl)
    {
        //Clear leaderboard
        Transform nameG = nameGroup.transform;
        Transform scoreG = scoreGroup.transform;
        for(int i=nameG.childCount-1; i>0; i--) //Clear all except for headers
        {
            Destroy(nameG.GetChild(i).gameObject);
            Destroy(scoreG.GetChild(i).gameObject);
        }

        _lvl--;
        //Instantiate new leaderboard values
        for(int i=0; i < gm.localLeaderboardStorage[_lvl].Count; i++)
        {
            //Enter record's name into local storage
            GameObject dm = Instantiate(dataMemberTxtPrefab, nameG);
            TMP_Text dmTxt = dm.GetComponent<TMP_Text>();
            dmTxt.text = gm.localLeaderboardStorage[_lvl][i][0];

            //Enter record's score into local storage
            GameObject dm2 = Instantiate(dataMemberTxtPrefab, scoreG);
            TMP_Text dm2Txt = dm2.GetComponent<TMP_Text>();
            dm2Txt.text = gm.localLeaderboardStorage[_lvl][i][1];
        }
    }

    public void SortLeaderboards()
    {
        //for each level
        for (int i = 0; i < 3; i++) {

            gm.localLeaderboardStorage[i].Sort(delegate (List<string> x, List<string> y)
            {
                return y[1].CompareTo(x[1]);
            });

        }
    }

    public void NameChanged()
    {
        if (connectedToLeaderboard)
        {
            gm.playerName = nameInp.text;
            PlayerPrefs.SetString("pName", nameInp.text);
            UpdateRecordName(gm.playerID,nameInp.text);
        }
    }

    public void RefreshScreenBoard()
    {
        sm.PlaySFX(4, 1.25f);
        RefreshLeaderboards();
        if (!connectionError) ReadLeaderboard(selectedLevel);
    }

    public string UpdateRecordName(int pID, string _name)
    {
        string strResult = "";

        SqlConnection Conn = new SqlConnection();

        Conn.ConnectionString = @GetConnected();

        string strSQL = "UPDATE GameStudio_Highscores SET Name=@name WHERE Player_ID=@pID";

        SqlCommand comm = new SqlCommand();
        comm.CommandText = strSQL;
        comm.Connection = Conn;

        comm.Parameters.AddWithValue("@name", _name);
        comm.Parameters.AddWithValue("@pID", pID.ToString());

        try
        {
            Conn.Open();
            int intRecs = comm.ExecuteNonQuery();
            Conn.Close();
            strResult = $"SUCCESS: Updated {intRecs} records.";
        }
        catch (Exception e)
        {
            Conn.Close();
            strResult = "ERROR: " + e.Message;
        }

        return strResult;
    }
}

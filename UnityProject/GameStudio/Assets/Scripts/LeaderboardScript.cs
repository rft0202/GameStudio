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

    //PRIVATE VARS
    //GameObject leaderboard; THIS gameobject
    GameManager gm;
    bool connectedToLeaderboard = false;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        RefreshLeaderboards();
        ReadLeaderboard(1);
    }

    public void RefreshLeaderboards()
    {
        gm.localLeaderboardStorage.Clear();
        LoadLeaderboards();
        SortLeaderboards();
    }

    public void ConnectToggle()
    {
        connectedToLeaderboard = connectedToggle.isOn;
        gm.connectedToLeaderboard = connectedToLeaderboard;
        if (connectedToLeaderboard && !gm.nameSet) //Player needs to set their name to add to leaderboard
        {
            //Give name enter popup

            //Add records for lvls 1-3
        }
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

        comm.Parameters.AddWithValue("@pID", gm.playerID);
        comm.Parameters.AddWithValue("@name", gm.playerName);
        comm.Parameters.AddWithValue("@lvl", lvl);
        comm.Parameters.AddWithValue("@score", gm.highscores[lvl]);

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

    public SqlDataReader SearchRecords_DR(int lvl)
    {
        SqlDataReader dr;
        SqlCommand comm = new SqlCommand();

        string strSql = "SELECT Name, Score FROM GameStudio_Highscores WHERE Lvl=@lvl";

        comm.Parameters.AddWithValue("@lvl", lvl);

        SqlConnection conn = new SqlConnection();

        conn.ConnectionString = @GetConnected();

        comm.Connection = conn;
        comm.CommandText = strSql;

        conn.Open();
        dr = comm.ExecuteReader();
        return dr;

    }

    public void LoadLeaderboards()
    {
        for (int i = 0; i < 3; i++) //Each level
        {
            gm.localLeaderboardStorage.Add(new()); //add new level section
            int cnt = 0;
            SqlDataReader dr = SearchRecords_DR(i+1);
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

    public SqlDataReader FindOnePlayer(int pID)
    {
        SqlConnection conn = new SqlConnection();
        SqlCommand comm = new SqlCommand();

        conn.ConnectionString = @GetConnected();
        comm.Connection = conn;
        comm.CommandText = "SELECT * FROM GameStudio_Highscores WHERE Player_ID = @pID;";
        comm.Parameters.AddWithValue("@pID", pID);

        conn.Open();
        return comm.ExecuteReader();
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

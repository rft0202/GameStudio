using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    SoundManager sm;
    GameManager gm;
    MusicManager mm;

    Toggle t1, t2;

    private void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        mm = GameObject.Find("MusicManager").GetComponent<MusicManager>();

        GameObject _t1 = GameObject.Find("SoundsToggle");
        if (_t1 != null)
        {
            t1 = _t1.GetComponent<Toggle>();
            t1.isOn = sm.SoundsOn;
        }
        GameObject _t2 = GameObject.Find("MusicToggle");
        if (_t2 != null)
        {
            t2 = _t2.GetComponent<Toggle>();
            t2.isOn = mm.MusicOn;
        }
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(4);
        sm.PlaySFX(4);
    }

    public void Level1()
    {
        gm.ChangeScene("Level1");
        sm.PlaySFX(4); 
    }

    public void Level2()
    {
        gm.ChangeScene("Level2");
        sm.PlaySFX(4);
    }

    public void Level3()
    {
        gm.ChangeScene("Level3");
        sm.PlaySFX(4);
    }

    public void ReplayLevel()
    {
        //SceneManager.LoadScene(gm.actualPrevScene);
        gm.ChangeScene(gm.actualPrevScene);
        sm.PlaySFX(4);
    }

    public void NextLevel()
    {
        //int nextLevel = 0;
        string nextLvl = "";
        if(gm.actualPrevScene == "Tutorial")
        {
            //nextLevel = 5;
            nextLvl = "Level1";
        }
        else if(gm.actualPrevScene == "Level1")
        {
            //nextLevel = 6;
            nextLvl = "Level2";
        }
        else if(gm.actualPrevScene == "Level2")
        {
            //nextLevel = 7;
            nextLvl = "Level3";
        }
        else if(gm.actualPrevScene == "Level3")
        {
            //nextLevel = 10; //Win Screen
            nextLvl = "WinScreen";
        }
        //SceneManager.LoadScene(nextLevel);
        gm.ChangeScene(nextLvl);
        sm.PlaySFX(4);
    }

    public void MainMenu()
    {
        Cursor.visible = true;
        gm.ChangeScene("MainMenu");
        sm.PlaySFX(4);
    }

    public void QuitGame()
    {
        Cursor.visible = true;
        Application.Quit();
        sm.PlaySFX(4);
    }

    public void LevelSelect()
    {
        Cursor.visible = true;
        gm.ChangeScene("LevelSelect");
        sm.PlaySFX(4);
    }

    public void Tutorial()
    {
        gm.ChangeScene("Tutorial");
        sm.PlaySFX(4);
    }

    public void Leaderboards()
    {
        gm.ChangeScene("Leaderboards");
        sm.PlaySFX(4);
    }

    public void Credits()
    {
        Cursor.visible = true;
        gm.ChangeScene("Credits");
        sm.PlaySFX(4);
    }

    public void ToggleSounds()
    {
        if(t1!=null)
            sm.ToggleSounds(t1.isOn);
    }
    public void ToggleMusic()
    {
        if (t2 != null)
            mm.MusicOn = t2.isOn;
    }
}
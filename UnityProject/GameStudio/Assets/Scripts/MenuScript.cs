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

    private void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        mm = GameObject.Find("MusicManager").GetComponent<MusicManager>();
        
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
        sm.ToggleSounds(GameObject.Find("SoundsToggle").GetComponent<Toggle>().isOn);
    }
    public void ToggleMusic()
    {
        mm.MusicOn = GameObject.Find("MusicToggle").GetComponent<Toggle>().isOn;
    }
}
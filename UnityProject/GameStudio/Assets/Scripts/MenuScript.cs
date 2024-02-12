using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    SoundManager sm;
    GameManager gm;

    private void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(4);
        sm.PlaySFX(4);
    }

    public void Level1()
    {
        SceneManager.LoadScene("Level1");
        sm.PlaySFX(4); 
    }

    public void Level2()
    {
        SceneManager.LoadScene("Level2");
        sm.PlaySFX(4);
    }

    public void Level3()
    {
        SceneManager.LoadScene("Level3");
        sm.PlaySFX(4);
    }

    public void ReplayLevel()
    {
        SceneManager.LoadScene(gm.actualPrevScene);
        sm.PlaySFX(4);
    }

    public void MainMenu()
    {
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
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
        SceneManager.LoadScene("LevelSelect");
        sm.PlaySFX(4);
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
        sm.PlaySFX(4);
    }
}
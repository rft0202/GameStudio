using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    SoundManager sm;

    private void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
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

    public void PlayTutorial()
    {
        SceneManager.LoadScene("Tutorial");
        sm.PlaySFX(4);
    }
}
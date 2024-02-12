using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectArrows : MonoBehaviour
{
    SoundManager sm;
    public Animator level1Anim;
    public Animator level2Anim;
    public Animator level3Anim;

    //level vars
    public int currLevel = 1;
    public bool level2Unlocked = false;
    public bool level3Unlocked = false;

    private void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        InvokeRepeating("SetCurrLevel", 1.0f, 1.0f);
    }

    //Set Curr Level
    void SetCurrLevel()
    {
        currLevel = level2Anim.GetInteger("currLevel");
    }

    //LEVEL SELECT ARROWS
    public void Left()
    {
        sm.PlaySFX(4);
        if (currLevel == 2)
        {
            //Level 2 to right
            level2Anim.SetTrigger("right");
            //Level 1 to right
            level1Anim.SetTrigger("right");

            currLevel = 1;
            level2Anim.SetInteger("currLevel", currLevel);
        }
        else if(currLevel == 3)
        {
            //Level 2 to right
            level2Anim.SetTrigger("right");
            //Level 3 to right
            level3Anim.SetTrigger("right");

            currLevel = 2;
            level2Anim.SetInteger("currLevel", currLevel);
        }
    }

    public void Right()
    {
        sm.PlaySFX(4);
        if (currLevel == 1 && level2Unlocked)
        {
            //Level 1 to Left
            level1Anim.SetTrigger("left");
            //Level 2 to Left
            level2Anim.SetTrigger("left");

            currLevel = 2;
            level2Anim.SetInteger("currLevel", currLevel);
        }
        else if(currLevel == 2 && level3Unlocked)
        {
            //Level 2 to left
            level2Anim.SetTrigger("left");
            //Level 3 to left
            level3Anim.SetTrigger("left");

            currLevel = 3;
            level2Anim.SetInteger("currLevel", currLevel);
        }
        currLevel = level2Anim.GetInteger("currLevel");
    }
}
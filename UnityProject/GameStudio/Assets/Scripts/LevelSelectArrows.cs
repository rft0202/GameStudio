using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectArrows : MonoBehaviour
{
    SoundManager sm;
    GameManager gm;
    public Animator level1Anim;
    public Animator level2Anim;
    public Animator level3Anim;

    public Toggle showControls;

    //level vars
    public int currLevel = 1;
    public bool level2Unlocked = false;
    public bool level3Unlocked = false;

    //To stop from spamming the arrows
    bool canPressButton = true;
    Button button;
    public Button otherButton;

    private void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        button = gameObject.GetComponent<Button>();
        InvokeRepeating("SetCurrLevel", 1.0f, 1.0f);
    }

    //Show controls at level start Toggle Option
    public void ToggleShowControls()
    {
        sm.PlaySFX(4);
        gm.lvlStartControls = showControls.isOn;
    }

    //Set Curr Level
    void SetCurrLevel()
    {
        currLevel = level2Anim.GetInteger("currLevel");
    }

    //LEVEL SELECT ARROWS
    public void Left()
    {
        if(canPressButton)
        {
            if (currLevel == 2)
            {
                sm.PlaySFX(4);
                //Level 2 to right
                level2Anim.SetTrigger("right");
                //Level 1 to right
                level1Anim.SetTrigger("right");

                currLevel = 1;
                level2Anim.SetInteger("currLevel", currLevel);

                StartCoroutine(ButtonPressDelay());
            }
            else if (currLevel == 3)
            {
                sm.PlaySFX(4);
                //Level 2 to right
                level2Anim.SetTrigger("right");
                //Level 3 to right
                level3Anim.SetTrigger("right");

                currLevel = 2;
                level2Anim.SetInteger("currLevel", currLevel);

                StartCoroutine(ButtonPressDelay());
            }
            else
            {
                sm.PlaySFX(1);
            }
        }
        else
        {
            sm.PlaySFX(1);
        }
    }

    public void Right()
    {
        if(canPressButton)
        {
            if (currLevel == 1 && level2Unlocked)
            {
                sm.PlaySFX(4);
                //Level 1 to Left
                level1Anim.SetTrigger("left");
                //Level 2 to Left
                level2Anim.SetTrigger("left");

                currLevel = 2;
                level2Anim.SetInteger("currLevel", currLevel);

                StartCoroutine(ButtonPressDelay());
            }
            else if (currLevel == 2 && level3Unlocked)
            {
                sm.PlaySFX(4);
                //Level 2 to left
                level2Anim.SetTrigger("left");
                //Level 3 to left
                level3Anim.SetTrigger("left");

                currLevel = 3;
                level2Anim.SetInteger("currLevel", currLevel);

                StartCoroutine(ButtonPressDelay());
            }
            else
            {
                sm.PlaySFX(1);
            }
            currLevel = level2Anim.GetInteger("currLevel");
        }
        else
        {
            sm.PlaySFX(1);
        }
    }

    IEnumerator ButtonPressDelay()
    {
        canPressButton = false;
        button.interactable = false;
        otherButton.interactable = false;
        yield return new WaitForSeconds(1f);
        canPressButton = true;
        button.interactable = true;
        otherButton.interactable = true;
    }
}
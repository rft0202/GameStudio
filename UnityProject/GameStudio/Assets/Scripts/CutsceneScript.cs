using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEditor;
using UnityEngine.UI;

public class CutsceneScript : MonoBehaviour
{
    SoundManager sm;
    GameManager gm;

    //Screen width
    float w;

    public GameObject dogGrabber;
    public GameObject dog;
    public GameObject player;

    float dogGrabberX;
    float dogX;

    Animator dogAnim;
    Animator playerAnim;

    bool cutsceneEnding = false;

    // Start is called before the first frame update
    void Start()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        w = Screen.width;
        Debug.Log(w);

        dogAnim = dog.GetComponent<Animator>();
        playerAnim = player.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        dogGrabberX = dogGrabber.transform.position.x;
        dogX = dog.transform.position.x;

        //Debug.Log("Dog Grabber: " + dogGrabberX);
       // Debug.Log("Dog: " + dogX);
        if(dogGrabberX < w)
        {
            dogAnim.SetBool("dogGrabberOnScreen", true);
        }

        if(dogX > w & !cutsceneEnding)
        {
            cutsceneEnding = true;
            playerAnim.SetBool("dogOnScreen", false);
            StartCoroutine("ChangeToLevelSelect");
        }
    }

    IEnumerator ChangeToLevelSelect()
    {
        //sm.PlaySFX(10); //I want to play the dog bark sound effect :(
        yield return new WaitForSeconds(1.0f);
        gm.ChangeScene("LevelSelect");
    }
}

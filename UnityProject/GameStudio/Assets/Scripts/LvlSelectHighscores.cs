using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LvlSelectHighscores : MonoBehaviour
{
    public TMP_Text[] highscoreLbls;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        for(int i=0; i<highscoreLbls.Length; i++)
        {
            highscoreLbls[i].text = gm.stringHighscores(i+1);
        }
    }
}

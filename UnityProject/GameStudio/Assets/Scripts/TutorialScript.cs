using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public Image w, a, s, d, ctrl, space;
    public Color pressedCol, unpressedCol;
    public GameObject shootInstructionLbl, chargeHelpLbl;
    public GameObject[] enemies;
    List<GameObject> activeEnemies = new();

    int defeatedEnemies=0;
    int tutorialStage = -1;
    bool[] keysPressed = { false, false, false, false, false, false };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //Check if enemies alive
        List<int> enemyRemoveList = new();
        for(int i=0; i<activeEnemies.Count; i++)
        {
            if (activeEnemies[i] == null)
            {
                defeatedEnemies++;
                enemyRemoveList.Add(i);
            }
        }
        //Remove null enemies
        for(int i=0; i < enemyRemoveList.Count; i++)
        {
            activeEnemies.RemoveAt(enemyRemoveList[i]);
        }

        //Tutorial stages
        if(tutorialStage==-1 && checkKeysPressed())
        {
            tutorialStage++;
            shootInstructionLbl.SetActive(true);
            //Play need press click to shoot now
        }
        if (defeatedEnemies == 1 && tutorialStage == 1)
        {
            spawnEnemy(1);
            spawnEnemy(2);
            tutorialStage++;
            chargeHelpLbl.SetActive(true);
        }else if(defeatedEnemies==3 && tutorialStage == 2)
        {
            //Boss enemy
            spawnEnemy(3);
            tutorialStage++;
        }
    }

    public void PlayerMove(InputAction.CallbackContext ctx)
    {
        Vector2 inp = ctx.ReadValue<Vector2>();
        if (inp.x > 0)
        {
            if (!keysPressed[0])
            {
                d.color = pressedCol;
                StartCoroutine(fadeBtn(d));
                keysPressed[0] = true;
            }
        }
        else if (inp.x < 0)
        {
            if (!keysPressed[1])
            {
                a.color = pressedCol;
                StartCoroutine(fadeBtn(a));
                keysPressed[1] = true;
            }
        }
        if (inp.y > 0)
        {
            if (!keysPressed[2])
            {
                w.color = pressedCol;
                StartCoroutine(fadeBtn(w));
                keysPressed[2] = true;
            }
        }
        else if (inp.y < 0)
        {
            if (!keysPressed[3])
            {
                s.color = pressedCol;
                StartCoroutine(fadeBtn(s));
                keysPressed[3] = true;
            }
        }
    }

    public void PlayerTrick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (!keysPressed[5])
            {
                ctrl.color = pressedCol;
                StartCoroutine(fadeBtn(ctrl));
                keysPressed[5] = true;
            }
        }
    }

    public void PlayerDodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (!keysPressed[4])
            {
                space.color = pressedCol;
                StartCoroutine(fadeBtn(space));
                keysPressed[4] = true;
            }
        }
    }

    public void PlayerShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (tutorialStage == 0)
            {
                spawnEnemy(0);
                tutorialStage++;
                shootInstructionLbl.SetActive(false);
            }
        }
    }

    public void PlayerChargeShot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (tutorialStage == 2)
            {
                chargeHelpLbl.SetActive(false);
            }
        }
    }

    void spawnEnemy(int eNum)
    {
        enemies[eNum].GetComponent<EnemyScript>().EnemySpawn(1);
        activeEnemies.Add(enemies[eNum]);
    }

    bool checkKeysPressed()
    {
        for(int i=0; i<6; i++)
        {
            if (!keysPressed[i]) return false;
        }
        return true;
    }

    IEnumerator fadeBtn(Image img)
    {
        while (img.color.a > 0)
        {
            Color col = new Color();
            col.a = img.color.a-0.01f;
            img.color = col;
            yield return null;
        }
    }
}

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
    int tutorialStage = 0;

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

        //
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
        w.color = unpressedCol;
        a.color = unpressedCol;
        s.color = unpressedCol;
        d.color = unpressedCol;
        if (inp.x > 0)
        {
            d.color = pressedCol;
        }else if (inp.x < 0)
        {
            a.color = pressedCol;
        }
        if (inp.y > 0)
        {
            w.color = pressedCol;
        }else if (inp.y < 0)
        {
            s.color = pressedCol;
        }
    }

    public void PlayerTrick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            ctrl.color = pressedCol;
        }
        else
        {
            ctrl.color = unpressedCol;
        }
    }

    public void PlayerDodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            space.color = pressedCol;
        }
        else
        {
            space.color = unpressedCol;
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
}

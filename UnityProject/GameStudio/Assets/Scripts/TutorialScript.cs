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

    Color[] btnUnpressedColors = { Color.red, Color.red, Color.red, Color.red, Color.red, Color.red };
    Color[] btnPressedColors = { Color.red, Color.red, Color.red, Color.red, Color.red, Color.red };

    int defeatedEnemies=0;
    int tutorialStage = -1;
    bool[] keysPressed = { false, false, false, false, false, false };

    public GameObject cloudPrefab;
    public float cloudSpawnRate;
    List<GameObject> clouds = new();
    public GameObject bossSpawnWarning;
    Animator warningAnim;

    SoundManager sm;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<6; i++)
        {
            btnUnpressedColors[i] = unpressedCol;
            btnPressedColors[i] = pressedCol;
        }
        warningAnim = bossSpawnWarning.GetComponent<Animator>();
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        StartCoroutine(spawnCloud());
    }

    // Update is called once per frame
    void Update()
    {
        if (tutorialStage < 1) chargeHelpLbl.SetActive(false);
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
            pressedCol.a = 0.25f;
            unpressedCol.a = 0.25f;
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
            //spawnEnemy(3);
            StartCoroutine(bossWarning());
            tutorialStage++;
        }
    }

    void changeColOpacity(int colInd, float opacity)
    {
        Color newCol1 = pressedCol;
        Color newCol2 = unpressedCol;
        newCol1.a = opacity;
        newCol2.a = opacity;
        btnPressedColors[colInd] = newCol1;
        btnUnpressedColors[colInd] = newCol2;
    }

    public void PlayerMove(InputAction.CallbackContext ctx)
    {
        Vector2 inp = ctx.ReadValue<Vector2>();
        //Make keys still get darker when pressed even after faded
        d.color = btnUnpressedColors[0];
        a.color = btnUnpressedColors[1];
        w.color = btnUnpressedColors[2];
        s.color = btnUnpressedColors[3];
        if (inp.x > 0)
        {
            d.color = btnPressedColors[0];
            if (!keysPressed[0])
            {
                StartCoroutine(fadeBtn(d,0));
                keysPressed[0] = true;
            }
        }
        else if (inp.x < 0)
        {
            a.color = btnPressedColors[1];
            if (!keysPressed[1])
            {
                StartCoroutine(fadeBtn(a,1));
                keysPressed[1] = true;
            }
        }
        if (inp.y > 0)
        {
            w.color = btnPressedColors[2];
            if (!keysPressed[2])
            {
                StartCoroutine(fadeBtn(w,2));
                keysPressed[2] = true;
            }
        }
        else if (inp.y < 0)
        {
            s.color = btnPressedColors[3];
            if (!keysPressed[3])
            {
                StartCoroutine(fadeBtn(s,3));
                keysPressed[3] = true;
            }
        }
    }

    public void PlayerTrick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            ctrl.color = btnPressedColors[5];
            if (!keysPressed[5])
            {
                StartCoroutine(fadeBtn(ctrl,5));
                keysPressed[5] = true;
            }
        }
        else
        {
            ctrl.color = btnUnpressedColors[5];
        }
    }

    public void PlayerDodge(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            space.color = btnPressedColors[4];
            if (!keysPressed[4])
            {
                //space.color = pressedCol;
                StartCoroutine(fadeBtn(space,4));
                keysPressed[4] = true;
            }
        }
        else
        {
            space.color = btnUnpressedColors[4];
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

    IEnumerator fadeBtn(Image img,int _id)
    {
        while (img.color.a > 0.7f)
        {
            changeColOpacity(_id, img.color.a-0.01f);
            img.color = btnUnpressedColors[_id];
            yield return null;
        }
    }

    IEnumerator bossWarning()
    {
        warningAnim.SetTrigger("enter");
        sm.PlaySFX(9);
        yield return new WaitForSeconds(2);
        warningAnim.SetTrigger("exit");
        spawnEnemy(3);

    }

    IEnumerator spawnCloud()
    {
        yield return new WaitForSeconds(cloudSpawnRate);
        clouds.Add(Instantiate(cloudPrefab, new Vector3(Random.Range(-8, 8), Random.Range(-4, 4), 0), Quaternion.identity));
        StartCoroutine(destroyCloud(clouds[^1]));
        StartCoroutine(spawnCloud());
    }

    IEnumerator destroyCloud(GameObject _c)
    {
        yield return new WaitForSeconds(1.5f);
        clouds.Remove(_c);
        Destroy(_c);
    }
}

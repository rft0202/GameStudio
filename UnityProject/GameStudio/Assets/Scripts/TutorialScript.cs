using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public Image w, a, s, d, ctrl, space;
    public Color pressedCol, unpressedCol;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

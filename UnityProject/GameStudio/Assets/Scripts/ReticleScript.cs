using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReticleScript : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Equal to the X and Y Scale of a rectangle sprite that covers the whole screen")]
    private float GameScreenWidth=17.775f,GameScreenHeight=10;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    public void PlayerLook(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 mousePixelPos = Mouse.current.position.ReadValue(); //(0,0) = bottom left corner of scrn
            mousePixelPos = new Vector2(mousePixelPos.x-(Screen.width / 2),mousePixelPos.y-(Screen.height / 2)); //(0,0) is center
            //Convert from pixels to Unity's (approx) Units
            transform.position = new Vector2((mousePixelPos.x / Screen.width) * GameScreenWidth, (mousePixelPos.y / Screen.height) * GameScreenHeight);
        }
    }
}

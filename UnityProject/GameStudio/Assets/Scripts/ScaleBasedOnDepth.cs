using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBasedOnDepth : MonoBehaviour
{

    //PUBLIC VARS
    public float depthViewDistRate = 1.15f;
    public bool useSpriteOrderInLayer=true;
    public float zpos, baseScale=1f;

    //PRIVATE VARS
    SpriteRenderer spriteRend;
    float baseScl;

    // Start is called before the first frame update
    void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        if (spriteRend == null) spriteRend = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        if (useSpriteOrderInLayer)
            zpos = spriteRend.sortingOrder;
        else
            spriteRend.sortingOrder = (int)zpos;
        baseScl = baseScale - 1;
    }

    // Update is called once per frame
    void Update()
    {
        //Set sorting order
        spriteRend.sortingOrder = (int)zpos;

        //Scale
        transform.localScale = (Mathf.Pow(depthViewDistRate, zpos)+baseScl) * Vector3.one;
    }

    public void SetBaseScale(float num)
    {
        baseScale = num;
        baseScl = baseScale - 1;
    }

    public Vector3 GetTargetScale()
    {
        return (Mathf.Pow(depthViewDistRate, zpos) + baseScl) * Vector3.one;
    }
    public Vector3 GetTargetScale(float _zpos)
    {
        return (Mathf.Pow(depthViewDistRate, _zpos) + baseScl) * Vector3.one;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NodePiece : MonoBehaviour
{
    public int value;
    public Point index;
    [HideInInspector]
    public Vector2 pos;
    [HideInInspector]
    public RectTransform rect;
  
    Image img;
    public void Initialize(int v, Point p, Sprite piece)// Bu fonksiyonu pozisyon değiştirdiğimiz de, resetlediğimiz de ya da tekrar yarattığımızda çağırılacak
    {
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();

        value = v;
        SetIndex(p);
        img.sprite = piece;
    }
    public void SetIndex(Point p)
    {
        index = p; 
        ResetPosition();
        UpdateName();
    }

    public void ResetPosition()
    {
       pos = new Vector2(32 + (64 * index.x), -32 - (64 * index.y));
    }

    void UpdateName()//Hata ayıklarken kolaylık olsun diye isim verdim.
    {
        transform.name = "Node [" + index.x + ", " + index.y + "]";
    }
}

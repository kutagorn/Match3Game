using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class NodePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value;
    public Point index;
    [HideInInspector]
    public Vector2 pos;

    [HideInInspector]
    public RectTransform rect;

    bool updating; //Parçayı zaten hareket ettiriyorsak tekrar yakalamanın bir anlamı yok, onu engellemek için kullanılır
  
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

    public void MovePosition(Vector2 move)
    {
        rect.anchoredPosition += move * Time.deltaTime * 16f;
    }

    public void MovePositionTo(Vector2 move)//Yukarda ki ile farkı bir pozisyonu alıp, o poziysona hareket ettirmesidir.
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 16f);
    }

    public bool UpdatePiece()
    {
        if(Vector3.Distance(rect.anchoredPosition, pos) > 1)
        {
            MovePositionTo(pos);
            updating = true; 
            return true;
        }
        else
        {
            rect.anchoredPosition = pos;
            updating = false;
            return false;
        }
        //Parça hareket etmiyorsa "false" dönecek
    }

    void UpdateName()//Hata ayıklarken kolaylık olsun diye isim verdim.
    {
        transform.name = "Node [" + index.x + ", " + index.y + "]";
    }
    public void OnPointerDown(PointerEventData eventData)
    {
       if (updating) return; 
       MovePieces.instance.MovePiece(this);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        MovePieces.instance.DropPiece();
    }
}

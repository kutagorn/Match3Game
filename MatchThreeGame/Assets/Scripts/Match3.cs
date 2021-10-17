using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;
    public Sprite[] pieces;//Bu diziyi kullanarak oyunda kaç tane farklı parça olduğunu ve oyunda kaç tane farklı parça olmasını istediğimizi ayarlayabiliriz.
    int width = 8;//Board un eni
    int height = 8;//Board un boyu
    Node[,] board;

    System.Random random;
    void Start()
    {
    }
    void StartGame()
    {
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        InitializeBoard();
    }
    void InitializeBoard() 
    {
        //board un yaratılısı
        board = new Node[width, height];
        
        for(int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    board[x, y] = new Node((boardLayout.rows[y].row[x]) ?  - 1 : fillPiece(), new Point(x,y));
                }
            }
    }

    void VerifyBoard()//Board oluşturulurken rand. olduğu için 3 tane yan yana koyup puan almasını engellemek amacı ile kullanılır.
    {   
        for (int x = 0; x < width; x++)
        
            {
                for (int y = 0; y < height; y++)
                {
                    Point p = new Point(x, y);
                    int val = getValueAtPoint(p);
                    if  (val <= 0) continue;//val 0 veya -1 ise (boş veya çukur) hiç bir şey yapmadan devam edilir.
                }
            }
    }

    List<Point> isConnected(Point p, bool main) //puan alırsak anlayacağız ki 3 tane yan yana gelmiş ve o 3lünün değiştirilmesi gerekmektedir.
    {
        List<Point> connected = new List<Point>();
        int val = getValueAtPoint(p);
        Point[] directions = 
        {
            Point.up,
            Point.right,
            Point.down,
            Point.left
        };

        foreach(Point dir in directions)//yukarıda, sağda, aşağıda ve solda aynı şekilden 2 tane veya daha fazla var mı diye bakılır.
        {
            List<Point> line = new List<Point>();
            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.add(p, Point.mult(dir,i));
                if(getValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;
                }
            }
            if (same > 1)// Eğer 1'den fazla aynı yönde, aynı şekil var ise match olduğunu anlamaktayız.
            {
                AddPoints(ref connected, line);//Bu puanlar kapsayıcı listeye eklenir.
            }
        }
        for (int i = 0 ; i < 2; i++)//2 tane aynı şeklin ortasındamıyız diye bakılır.
        {
            List<Point> line = new List<Point>();
            int same = 0;
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[i+2])};
            foreach(Point next in check) //Şeklin iki tarafına da bakılır eğer aynılarsa listeye eklenir.
            {
                if(getValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }
            if (same > 1)
            {
                AddPoints(ref connected, line);
            }
            
        }
        for (int i = 0; i < 4; i++)//2x2 var mı diye bakılır.
        {
            List<Point> square = new List<Point>();
            int same = 0;
            int next = i + 1;
            if(next >=4)
            {
                next -= 4;
            }
            Point[] check = { Point.add(p, directions[i]), Point.add(p, directions[next]), Point.add(p, Point.add(directions[i], directions[next]))};
            foreach(Point pnt in check) //Şeklin bütün taraflarına  bakılır eğer aynılarsa listeye eklenir
            {
                if(getValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }
            if (same > 2)
            {
                AddPoints(ref connected, square);
            }
        }
        if(main)// Seçili olan eşleşmede bakşa eşleşme var mı diye bakar
        {
            for(int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, isConnected(connected[i], false));
            }
        }
        if(connected.Count > 0)
        {
            connected.Add(p);
        }
        return connected;
    }
    void AddPoints(ref List<Point> points, List<Point> add)
    {
        /* 
        Contains ile bakmama sebibim Unity iki value eşit mi diye bakma şeklinden kaynaklanmaktadır
        if(point[i] == p) bu tarz bir sorgu kullandığım zaman noktanın tam (exact) value'suna bakmaktadır, 
        ve ben her seferinde yeni point yarattığım için(45.Satır) değerler aynı olmasına rağmen asıl değerleri hiç bir zaman aynı olmayacaktır.
        ve if sorgusu her seferinde false dönecektir. O yüzden kendim yazma gereğinde hissettim. 
        */
        foreach(Point p in add)//Eklemek istediğim "Point"leri döngü içerisine alıyoruz
        {
            bool doAdd = true;//Aldığımız "Point"leri ekleyecekmiyiz diye bakıyoruz
            for(int i = 0; i < points.Count;i++)
            {
                if(points[i].Equals(p))//Point 'de ki noktalar "add" 'in içindeki noktalar ile eşit ise bu içeride zaten o noktanın olduğu anlamına gelir
                {
                    doAdd = false;
                    break;
                }
            }
            if (doAdd) points.Add(p);//Eğer eklemek istiyorsak ("doAdd" true dönüyorsa), ekleme işlemi yapılır
        }
    }

    int fillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / pieces.Length)) + 1; // 0 ile (pieces - 1) arası bir değer rand. alması lazım. O yüzden +1 ekliyoruz
        return val;

    }

    int getValueAtPoint(Point p)//küçük yardımcı fonk. her seferinde yazmayalım diye
    {
        return board[p.x,p.y].value;
    }

    void Update()
    {
    
    }
    string getRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for(int i = 0; i < 20; i++)
            {
                seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
            }
        return seed;
    }
}
[System.Serializable]
public class Node 
{
    public int value; // 0 - Bos,1 - Mavi, 2 - Yesil, 3 - Kırmızı, 4 - Sarı, -1 Cukur
    public Point index;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }
}
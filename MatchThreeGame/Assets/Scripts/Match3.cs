using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;
    [Header("UI Elements")]
    public Sprite[] pieces;//Bu diziyi kullanarak oyunda kaç tane farklı parça olduğunu ve oyunda kaç tane farklı parça olmasını istediğimizi ayarlayabiliriz.
    public RectTransform gameBoard;
    public RectTransform killedBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killedPiece;


    int width = 8;//Board un eni
    int height = 8;//Board un boyu
    int[] fills;
    Node[,] board;

    List<NodePiece> update;
    List<FlippedPieces> flipped;
    List<NodePiece> dead; // Yeniden parça yaratmak yerine zaten oyunda olan ve oyundan çıkan parçalar kullanılır.
    List<KilledPiece> killed;

    System.Random random;
    void Start()
    {
        StartGame();
    }

    void Update()
    {
        List<NodePiece> finishedUpdating = new List<NodePiece>();
        for(int i = 0; i < update.Count; i++)
        {
            NodePiece piece = update[i];
            if(!piece.UpdatePiece()) finishedUpdating.Add(piece); // Update False döner ise ve olması gereken pozisyondaysa, finishedUpdating'e eklenir

        }
        for(int i = 0; i < finishedUpdating.Count; i++)
        {
            NodePiece piece = finishedUpdating[i];
            FlippedPieces flip = getFlipped(piece);
            NodePiece flippedPiece = null;

            int x = (int)piece.index.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

            List<Point> connected = isConnected(piece.index, true);
            bool wasFlipped = (flip != null);
            if(wasFlipped)// Bu güncellemeyi yapacak swipe haraketi yapıldıysa
            {
                flippedPiece = flip.getOtherPiece(piece);
                AddPoints(ref connected, isConnected(flippedPiece.index, true));
            }
            if (connected.Count == 0) //Hiç bir match olmayan bir hamle yapılmıştır.
            {
                if(wasFlipped)// Eğer swipe haraketi yapılmışsa
                    FlipPieces(piece.index, flippedPiece.index, false);// swipe haraketi tekrarlanır ve eski hale donuş olur.
            }
            else // doğru bir match işlemi yapılmıştır
            {
                foreach(Point pnt in connected) // eşleşen tüm şekilleri kaldır
                {
                    KillPiece(pnt);
                    Node node = getNodeAtPoint(pnt);
                    NodePiece nodePiece = node.getPiece();
                    if(nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                        dead.Add(nodePiece);
                    }
                    node.SetPiece(null);

                }
                ApplyGravityToBoard();
            }

            flipped.Remove(flip); // tüm işlemler sonrasında swipe haraketini kaldır
            update.Remove(piece);
        }
    }

    void ApplyGravityToBoard() //Objelerin aşağıya düşmesini sağlayan blok
    {
        for(int x = 0;x < width; x++)
        {
            for(int y = (height - 1); y >= 0; y--)
            {
                Point p = new Point(x, y);
                Node node = getNodeAtPoint(p);
                int val = getValueAtPoint(p);
                if (val != 0) continue; //Eğer çukur değilse, bir şey yapma
                for(int ny = (y-1); ny >= -1; ny--)
                {
                    Point next = new Point(x, ny);
                    int nextVal = getValueAtPoint(next);
                    if (nextVal == 0)
                        continue;
                    if(nextVal != -1) //Sona ulaşmadıysak, ama 0 değilse bunu kullanarak çukuru doldur.
                    {
                        Node got = getNodeAtPoint(next);
                        NodePiece piece = got.getPiece();

                        //Çukuru belirle
                        node.SetPiece(piece);
                        update.Add(piece);

                        //Çukur ile değiştir
                        got.SetPiece(null);
                    }
                    else //Sona ulaştıysak
                    {
                        //Doldur
                        int newVal = fillPiece();
                        NodePiece piece;
                        Point fallPnt = new Point(x, (-1 - fills[x]));//y -1 atamamızın sebebi en yukardan başlasın ve aşağıya düşsün
                        if (dead.Count > 0) 
                        {
                            NodePiece revived = dead[0];
                            revived.gameObject.SetActive(true);
                            revived.rect.anchoredPosition = getPositionFromPoint(fallPnt);
                            piece = revived;
                            
                          
                            dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject obj = Instantiate(nodePiece, gameBoard);
                            NodePiece n = obj.GetComponent<NodePiece>();//candycrush da ki gibi bazen bazı yerlerin dolması için önce kırılması lazım o tarz bişi koyulmak istenirse diye hazırlık.
                            RectTransform rect = obj.GetComponent<RectTransform>();
                            rect.anchoredPosition = getPositionFromPoint(fallPnt);
                            piece = n;
                        }
                        piece.Initialize(newVal, p, pieces[newVal - 1]);
                        Node hole = getNodeAtPoint(p);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        fills[x]++;
                    }
                    break;

                }
            }
        }
    }
    
    FlippedPieces getFlipped(NodePiece p)
    {
        FlippedPieces flip = null;
        for(int i = 0; i < flipped.Count; i++)
        {
            if(flipped[i].getOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
        }
        return flip;
    }       

   

    void StartGame()
    {
        fills = new int[width];
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<NodePiece>();
        flipped = new List<FlippedPieces>();
        dead = new List<NodePiece>();
        killed = new List<KilledPiece>();
        InitializeBoard();
        VerifyBoard();
        InstaniateBoard();
    }
    void InitializeBoard() 
    {
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
        List<int> remove;
        for (int x = 0; x < width; x++)
        
            {
                for (int y = 0; y < height; y++)
                {
                    Point p = new Point(x, y);
                    int val = getValueAtPoint(p);
                    if  (val <= 0) continue;//val 0 veya -1 ise (boş veya çukur) hiç bir şey yapmadan devam edilir.

                    remove = new List<int>();
                    while(isConnected(p, true).Count > 0)
                    {
                        val = getValueAtPoint(p);
                        if(!remove.Contains(val))
                        {
                            remove.Add(val);
                        }
                        setValueAtPoint(p, newValue(ref remove));
                    }
                }
            }
    }
    void InstaniateBoard()
    {
     for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = getNodeAtPoint(new Point(x, y));
                int val = node.value;
                if(val <= 0) continue;
                GameObject p = Instantiate(nodePiece, gameBoard);
                NodePiece piece = p.GetComponent<NodePiece>();
                RectTransform rect = p.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                piece.Initialize(val, new Point(x,y), pieces[val-1]);
                node.SetPiece(piece);
            }
        }
    }

    public void ResetPiece(NodePiece piece)
    {
        piece.ResetPosition();
        update.Add(piece);

    }

    public void FlipPieces(Point one, Point two, bool main)
    {
        if (getValueAtPoint(one) < 0 ) return;  //-1'ler ile yani çukur/boş alanlarla yer değişmemesi lazım.
        Node nodeOne = getNodeAtPoint(one);
        NodePiece pieceOne = nodeOne.getPiece();
        if(getValueAtPoint(two) > 0 )
        {
            Node nodeTwo = getNodeAtPoint(two);
            NodePiece pieceTwo = nodeTwo.getPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);
            
            if(main)
                flipped.Add(new FlippedPieces(pieceOne, pieceTwo));

            update.Add(pieceOne);
            update.Add(pieceTwo);

        }
        else
        {
            ResetPiece(pieceOne);
        }
    }

    void KillPiece(Point p)
    {
        List<KilledPiece> available = new List<KilledPiece>();
        for (int i = 0; i < killed.Count; i++)
            if (!killed[i].falling) available.Add(killed[i]);

        KilledPiece set = null;
        if (available.Count > 0)
            set = available[0];
        else
        {
            GameObject kill = GameObject.Instantiate(killedPiece, killedBoard);
            KilledPiece kPiece = kill.GetComponent<KilledPiece>();
            set = kPiece;
            killed.Add(kPiece);
        }

        int val = getValueAtPoint(p) - 1;
        if (set != null && val >= 0 && val < pieces.Length)
            set.Initialize(pieces[val], getPositionFromPoint(p));
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
        /* Gereksiz
        if(connected.Count > 0)
        {
            connected.Add(p);
        }*/
        return connected;
    }
    void AddPoints(ref List<Point> points, List<Point> add)
    {
        /* 
        Contains ile bakmama sebibim Unity iki value eşit mi diye bakma şeklinden kaynaklanmaktadır
        if(point[i] == p) bu tarz bir sorgu kullandığım zaman noktanın tam (exact) value'suna bakmaktadır, 
        ve ben her seferinde yeni point yarattığım için değerler aynı olmasına rağmen asıl değerleri hiç bir zaman aynı olmayacaktır.
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
        if(p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return -1;
        return board[p.x,p.y].value;
    }

    void setValueAtPoint(Point p, int v)
    {
        board[p.x, p.y].value = v;
    }

    Node getNodeAtPoint(Point p)
    {
        return board[p.x, p.y];
    }

    int newValue(ref List<int> remove)
    {
        List<int> available = new List<int>();//bunlar kullanabileceğim müsait olanlar.
        for (int i = 0; i < pieces.Length; i++)
            available.Add(i + 1);
        foreach (int i in remove)
        {
            available.Remove(i);
        }
        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
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

    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(32 + (64 * p.x), -32 - (64 * p.y));
    }
}
[System.Serializable]
public class Node 
{
    public int value; // 0 - Bos,1 - Mavi, 2 - Yesil, 3 - Kırmızı, 4 - Sarı, -1 Cukur
    public Point index;
    NodePiece piece;

    public Node(int v, Point i)
    {
        value = v;
        index = i;
    }

    public void SetPiece(NodePiece p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) return;
        piece.SetIndex(index);
    }
    public NodePiece getPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class FlippedPieces // Match yoksa fliplenen şekiller geri yerlerine döner
{
    public NodePiece one;
    public NodePiece two;

    public FlippedPieces(NodePiece o, NodePiece t)
    {
        one = o;
        two = t;
    }

    public NodePiece getOtherPiece(NodePiece p)
    {
        if(p == one)
            return two;
        else if(p == two)
            return one;
        else
        return null;
    }

}
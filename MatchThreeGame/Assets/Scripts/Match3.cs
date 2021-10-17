using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    public ArrayLayout boardLayout;
    int width = 8;//Board un eni
    int height = 8;//Board un boyu
    Node[,] board;

    System.Random random;
    void Start()
    {
        StartGame();
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
                    board[x, y] = new Node(-1, new Point(x,y));
                }
            }
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
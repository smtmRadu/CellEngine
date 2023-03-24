using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ConwaysGOL : MonoBehaviour
{
    public Camera cam;
    public int scaleDivision = 6;
    private int width;
    private int height;
    public Shape livingBeingShape = Shape.Boat;
    public int eraserSize = 50;
    public int wallSize = 3;
    public Color aliveCellColor = Color.green;
    public Color wallColor = Color.white;
    public Color deadCellColor = Color.black;

    public float colorLerpPower = 0.01f;
    Color aliveCellTargColor = Color.blue;
    Color bgTargColor = Color.black;

    Resolution screenSize;
    Color[,] matrix;
    Texture2D texture;
    SpriteRenderer sr;
    public bool variableColor = true;

    private void Awake()
    {
        screenSize = Screen.currentResolution;
        width = screenSize.width/scaleDivision;
        height = screenSize.height/scaleDivision;

        Application.targetFrameRate = 45;
        sr = GetComponent<SpriteRenderer>();
        matrix = new Color[width, height];
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
   
        

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                try
                {
                    matrix[i, j] = deadCellColor;
                }
                catch { }
            }
        }
    }
    void Update()
    {
        DrawCells();
        DrawWalls();
        Erase();
        ConwayGameOfLifeAlgorithm();
        RenderMatrix();
        if (variableColor)
        {
            aliveCellColor = Color.Lerp(aliveCellColor, aliveCellTargColor, colorLerpPower);
            cam.backgroundColor = Color.Lerp(cam.backgroundColor, bgTargColor, colorLerpPower );
        }
        if(Time.frameCount%150 == 0) //change target color to a new one
        {
            aliveCellTargColor = new Color(Random.value,Random.value,Random.value, 1);
            bgTargColor = new Color(Random.Range(0f, 0.1f), Random.Range(0f, 0.1f), Random.Range(0f, 0.1f));
        }
    }
    private void DrawCells()
    {
        if (!Input.GetMouseButton(0))
            return;

        if (Input.GetKey(KeyCode.LeftControl))
            return;

        Vector2 mousePosition = Input.mousePosition;
        mousePosition.x = mousePosition.x * width / screenSize.width;
        mousePosition.y = mousePosition.y * height / screenSize.height;
        try
        {
            // boat
            if (matrix[(int)mousePosition.x, (int)mousePosition.y] == deadCellColor)
                matrix[(int)mousePosition.x, (int)mousePosition.y] = aliveCellColor;
            if (matrix[(int)mousePosition.x + 1, (int)mousePosition.y] == deadCellColor)
                matrix[(int)mousePosition.x + 1, (int)mousePosition.y] = aliveCellColor;
            if (matrix[(int)mousePosition.x, (int)mousePosition.y - 1] == deadCellColor)
                matrix[(int)mousePosition.x, (int)mousePosition.y - 1] = aliveCellColor;
            if (matrix[(int)mousePosition.x + 2, (int)mousePosition.y - 1] == deadCellColor)
                matrix[(int)mousePosition.x + 2, (int)mousePosition.y - 1] = aliveCellColor;
            if (matrix[(int)mousePosition.x + 1, (int)mousePosition.y - 2] == deadCellColor)
                matrix[(int)mousePosition.x + 1, (int)mousePosition.y - 2] = aliveCellColor;                          
        }
        catch { }
       
    }
    private void DrawWalls()
    {
        if (!Input.GetMouseButton(0) || !Input.GetKey(KeyCode.LeftControl))
            return;

        Vector2 mousePosition = Input.mousePosition;
        mousePosition.x = mousePosition.x * width / screenSize.width;
        mousePosition.y = mousePosition.y * height / screenSize.height;

        try
        {
            for (int i = -wallSize; i <= wallSize; i++)
            {
                for (int j = -wallSize; j <= wallSize; j++)
                {

                    float deltaDist = Vector2.Distance(mousePosition, new Vector2(mousePosition.x + i, mousePosition.y + j));
                    if(deltaDist < wallSize)
                        matrix[(int)mousePosition.x + i, (int)mousePosition.y - j] = wallColor;
                }

            }
        }
        catch { }
    }
    private void Erase()
    {
        if (!Input.GetMouseButton(1))
            return;

        Vector2 mousePosition = Input.mousePosition;
        
        mousePosition.x = mousePosition.x *  width / screenSize.width;
        mousePosition.y = mousePosition.y * height / screenSize.height;
        try
        {
            for (int i = -eraserSize; i <= eraserSize; i++)
            {
                for (int j = -eraserSize; j <= eraserSize; j++)
                {

                    float deltaDist = Vector2.Distance(new Vector2(mousePosition.x, mousePosition.y), new Vector2(mousePosition.x + i, mousePosition.y + j));
                    if (deltaDist < eraserSize)
                        matrix[(int)mousePosition.x + i, (int)mousePosition.y - j] = deadCellColor;
                }

            }
        }
        catch { }
    }
    void ConwayGameOfLifeAlgorithm()
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                int neighboursCount = NeighboursCount(i, j);
                if (IsWall(matrix[i, j]))
                    continue;
                if(IsDead(matrix[i,j]) && neighboursCount == 3) 
                {
                    matrix[i, j] = aliveCellColor;
                }
                else if (IsAlive(matrix[i,j]) && neighboursCount >= 2 && neighboursCount <= 3)
                {
                    // stays alive
                }
                else
                {
                    matrix[i, j] = deadCellColor;
                }
            }
        }
    }
    int NeighboursCount(int x, int y)
    {
        int count = 0;
        try
        {
            if (IsAlive(matrix[x,y+1]))
            {
                count++;
            }
        }
        catch { }
        try
        {
            if (IsAlive(matrix[x+1, y + 1]))
            {
                count++;
            }
        }
        catch { }
        try
        {
            if (IsAlive(matrix[x + 1, y]))
            {
                count++;
            }
        }
        catch { }
        try
        {
            if (IsAlive(matrix[x + 1, y - 1]))
            {
                count++;
            }
        }
        catch { }

        try
        {
            if (IsAlive(matrix[x, y - 1]))
            {
                count++;
            }
        }
        catch { }
        try
        {
            if (IsAlive(matrix[x - 1, y - 1]))
            {
                count++;
            }
        }
        catch { }
        try
        {
            if (IsAlive(matrix[x - 1, y]))
            {
                count++;
            }
        }
        catch { }
        try
        {
            if (IsAlive(matrix[x - 1, y + 1]))
            {
                count++;
            }
        }
        catch { }

        return count;
    }
    bool IsDead(Color col)
    {
        if (col.Equals(deadCellColor))
            return true;
        return false;
    }
    bool IsAlive(Color col)
    {
        if (!col.Equals(deadCellColor) && !col.Equals(wallColor))
            return true;
        return false;
    }
    bool IsWall(Color col)
    {
        if (col.Equals(wallColor))
            return true;
        return false;
    }

    void RenderMatrix()
    {
        Color[] flatPixels = new Color[width * height];

        for (int h = 0; h < matrix.GetLength(1); h++)
        {
            for (int w = 0; w < matrix.GetLength(0); w++)
            {           
                flatPixels[h*width + w] = matrix[w, h];
            }
        }
        texture.SetPixels(flatPixels);
        texture.Apply();
       
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero);
        
        sr.sprite = sprite;
    }
    public void ResetMatrix()
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = deadCellColor;
            }
        }
    }
    public void QuitApp()
    {
        Application.Quit();
    }
    public void SetVariableColor()
    {
        variableColor = !variableColor;
        
    }
}
public enum Shape
{
    Square,
    Boat,
    Line,
}

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum TileType
{
    Floor,
    Wall,
    SafeHouse,
    Store,
    Sacrifice,
    Observatory,
    Secret,
    Eraser
}


//We want a structure for reading/write map in the scene!
//The problem is what data we want to store?
[System.Serializable]
public class MapData
{
    public List<TileInfo> tiles = new List<TileInfo>();
}

//A simple structure represents a tile in the map~
// Our current structure in the script for the map :
//private Dictionary<Vector3Int, TileType> tileData = new Dictionary<Vector3Int, TileType>();
[System.Serializable]
public class TileInfo
{
    public int x;
    public int y;
    public int type;

}



public class MapEditor : MonoBehaviour
{
    [SerializeField] TMP_InputField mapLoader;
    [SerializeField] TMP_InputField mapSaver;
    [SerializeField] GameObject saveShow;

    private Dictionary<Vector3Int, TileType> tileData = new Dictionary<Vector3Int, TileType>();
    //code for making a map below
    //you don't have to check or know them
    [Header("Tilemap References")]
    public Tilemap tilemap;
    public Tilemap tilemap_Wall;

    [Header("Tiles")]
    [SerializeField] TileBase floorTile;
    [SerializeField] TileBase wallTile;
    [SerializeField] TileBase safeHouseTile;
    [SerializeField] TileBase shopTile;
    [SerializeField] TileBase sacrificeTile;
    [SerializeField] TileBase observatoryTile;
    [SerializeField] TileBase secretTile;

    [Header("Editor Settings")]
    [SerializeField] TileType currentTileType = TileType.Floor;
    [SerializeField] int brushSize = 1;
    [SerializeField] bool isDrawing = false;

    [Header("Grid Settings")]
    [SerializeField] int mapWidth = 100;
    [SerializeField] int mapHeight = 100;

    [Header("Camera")]
    private Camera mainCamera;

    private Vector3Int lastPaintedPosition;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        tilemap.ClearAllTiles();
        tilemap_Wall.ClearAllTiles();
    }

    void Update()
    {
        HandleInput();
        HandleDrawing();
    }

    // check all input
    // mouse button - drawing
    // num - switch brushes
    // bracket - change size
    // ****Ctrl +C/S clear/save map****
    //YOU just to need to do the save parts👆
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            lastPaintedPosition = Vector3Int.zero;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) currentTileType = TileType.Floor;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentTileType = TileType.Wall;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentTileType = TileType.SafeHouse;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentTileType = TileType.Store;
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentTileType = TileType.Sacrifice;
        if (Input.GetKeyDown(KeyCode.Alpha6)) currentTileType = TileType.Observatory;
        if (Input.GetKeyDown(KeyCode.Alpha7)) currentTileType = TileType.Secret;
        if (Input.GetKeyDown(KeyCode.Alpha0)) currentTileType = TileType.Eraser;

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            brushSize = Mathf.Max(1, brushSize - 1);
            Debug.Log($"Brush Size: {brushSize}");
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            brushSize = Mathf.Min(10, brushSize + 1);
            Debug.Log($"Brush Size: {brushSize}");
        }

        if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
        {
            ClearMap();
        }

        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
        {
            saveShow.SetActive(true);
        }
    }

    void HandleDrawing()
    {
        if (!isDrawing) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        if (cellPosition == lastPaintedPosition) return;
        lastPaintedPosition = cellPosition;

        for (int x = -brushSize / 2; x <= brushSize / 2; x++)
        {
            for (int y = -brushSize / 2; y <= brushSize / 2; y++)
            {
                Vector3Int paintPos = cellPosition + new Vector3Int(x, y, 0);

                if (Mathf.Abs(paintPos.x) > mapWidth / 2 || Mathf.Abs(paintPos.y) > mapHeight / 2)
                    continue;

                PaintTile(paintPos);
            }
        }
    }

    void PaintTile(Vector3Int position)
    {
        tilemap.SetTile(position, null);
        tilemap_Wall.SetTile(position, null);

        if (currentTileType == TileType.Eraser)
        {
            if (tileData.ContainsKey(position))
            {
                tileData.Remove(position);
            }
            return;
        }

        tileData[position] = currentTileType;

        TileBase tileToPlace = GetTileForType(currentTileType);

        if (currentTileType == TileType.Wall)
        {
            tilemap_Wall.SetTile(position, tileToPlace);
        }
        else
        {
            tilemap.SetTile(position, tileToPlace);
        }
    }

    TileBase GetTileForType(TileType type)
    {
        switch (type)
        {
            case TileType.Floor:
                return floorTile;
            case TileType.Wall:
                return wallTile;
            case TileType.SafeHouse:
                return safeHouseTile;
            case TileType.Store:
                return shopTile;
            case TileType.Sacrifice:
                return sacrificeTile;
            case TileType.Observatory:
                return observatoryTile;
            case TileType.Secret:
                return secretTile;
            default:
                return floorTile;
        }
    }

    void ClearMap()
    {
        tilemap.ClearAllTiles();
        tilemap_Wall.ClearAllTiles();
        tileData.Clear();
        Debug.Log("Map cleared!");
    }

    //Let's write a function to Save our map structure
    public void SaveMap()
    {

        //private Dictionary<Vector3Int, TileType> tileData = new Dictionary<Vector3Int, TileType>();
        //Our map file name from the input field!
        string fileName = mapSaver.text + ".json";
        // This is the path where we will save our file.
        // Application.persistentDataPath provides a safe, writable location across platforms.
        // Path.Combine combines paths correctly. Unlike simply using "/" or "\\", 
        // it automatically uses the correct separator for the current operating system:
        // Windows uses "\", Mac/Linux use "/".
        string path = Path.Combine(Application.persistentDataPath, fileName);
        saveShow.SetActive(false);
        Debug.LogWarning("Wait...I am trying to save but nothing happen....");

        return;

    }

    public void LoadMap()
    {
        string fileName = mapLoader.text + ".json";
        string path = Path.Combine(Application.persistentDataPath, fileName) ;
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("Map file not found!");
            return;
        }

        string json = System.IO.File.ReadAllText(path);
        MapData mapData = JsonUtility.FromJson<MapData>(json);

        ClearMap();

        foreach (var tile in mapData.tiles)
        {
            Vector3Int pos = new Vector3Int(tile.x, tile.y, 0);
            currentTileType = (TileType)tile.type;
            PaintTile(pos);
        }

        Debug.Log($"Map loaded! Total tiles: {mapData.tiles.Count}");
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 300, 30), $"Current Tool: {currentTileType}", style);
        GUI.Label(new Rect(10, 40, 300, 30), $"Brush Size: {brushSize}", style);
        GUI.Label(new Rect(10, 70, 500, 30), "Keys: 1-7 (Tiles) | 0 (Eraser) | [ ] (Brush Size)", style);
        GUI.Label(new Rect(10, 100, 500, 30), "Ctrl+C (Clear) | Ctrl+S (Save)", style);
    }
}


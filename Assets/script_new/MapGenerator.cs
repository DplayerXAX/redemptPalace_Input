using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;


public enum RoomType
{
    Normal,
    SafeHouse,
    Store,
    Sacrifice,
    Observatory,
    Secret
}

public class ChunkRooms
{
    public Dictionary<RoomType, RectInt> myRooms = new Dictionary<RoomType, RectInt>();
}


public class MapGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int chunkSize = 50;
    public int actualDisChunk = 25;
    [SerializeField] private Dictionary<Vector2Int, bool[,]> chunks = new Dictionary<Vector2Int, bool[,]>();
    [SerializeField] private Dictionary<Vector2Int, ChunkRooms> specialRooms = new Dictionary<Vector2Int, ChunkRooms>();
    private Dictionary<Vector2Int, RectInt> chunkArea = new Dictionary<Vector2Int, RectInt>();
    private Vector2Int currentChunk = Vector2Int.zero;
    [SerializeField] private GameObject Mammon;
    public int loadRadius = 1;
    private Vector2Int lastLoadedChunk = Vector2Int.zero;
    [Header("map setting")]
    public int width = 50;
    public int height = 50;
    public int minRoomSize = 5;
    public int maxDepth = 5;
    [Header("Special Room Settings")]
    public int storeInterval = 3;
    public int secretInterval = 4;
    [SerializeField] private int chunksGenerated = 0;
    [Header("Safe House Settings")]
    public int safeHouseSize = 5;
    private RectInt safeHouse;
    [Header("Tilemap")]
    public Tilemap tilemap;
    public Tilemap tilemap_Wall;
    public TileBase floorTile;
    public TileBase sacrificeTile;
    public TileBase secretTile;
    public TileBase observatoryTile;
    public TileBase wallTile;
    public TileBase safeHouseTile;
    public TileBase shopTile;
    public Vector3 visualOffset = new Vector3(0, 0.25f, 0);
    [Header("Pref")]
    public GameObject playerPrefab;
    public GameObject monsterPrefab;
    //[SerializeField] private int monsterCount = 5;
    public BasicCameraFollow bcf;
    Queue<Vector2Int> loadQueue = new Queue<Vector2Int>();
    public bool[,] dungeonGrid { get; private set; }

    void Awake()
    {

        GenerateChunk(Vector2Int.zero, true);
        BuildTilemap();
        currentChunk = Vector2Int.zero;
        UpdateChunks();
        GenerateCharacters();

    }

    int GetChunkCoord(float pos, int size)
    {
        return (int)(pos / (size / 2));
    }



    void OnEnterRoom(RoomType type, RectInt room)
    {
        switch (type)
        {
            case RoomType.Store:
                Debug.Log("Enter store");
                Time.timeScale = 0f;
                // ShowStoreUI();
                break;

            case RoomType.Sacrifice:
                Debug.Log("Enter sacrifice");
                break;

            case RoomType.Observatory:
                Debug.Log("Enter star");
                break;

            case RoomType.SafeHouse:
                Debug.Log("Enter safe house");
                break;

            case RoomType.Secret:
                Debug.Log("Enter secret room");
                break;

            case RoomType.Normal:
            default:
                Debug.Log("Enter normal room");
                break;
        }
    }


    Vector2Int WorldToChunkCoord(Vector3 worldPos)
    {
        //float gridSize = 12.5f;
        float a = worldPos.x;
        float b = worldPos.y;
        float fx = 0.5f * (a / 25f + b / 12.5f - 1f);
        float fy = 0.5f * (b / 12.5f - a / 25f - 1f);
        int x = Mathf.RoundToInt(fx);
        int y = Mathf.RoundToInt(fy);

        return new Vector2Int(x, y);
    }
    private void Update()
    {

        Vector3 playerPos = bcf.followTarget.position;
        currentChunk = WorldToChunkCoord(playerPos);
        //Debug.Log("I am currently at chunk:"+currentChunk.x +""+ currentChunk.y);
        if (currentChunk != lastLoadedChunk)
        {
            UpdateChunks();
            lastLoadedChunk = currentChunk;
        }
    }

   

    int chunksPerFrame = 1;
    void LateUpdate()
    {
        int count = 0;
        while (loadQueue.Count > 0 && count < chunksPerFrame)
        {
            Vector2Int chunkToLoad = loadQueue.Dequeue();
            GenerateChunk(chunkToLoad);
            count++;
        }
    }

    void UpdateChunks()
    {

        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                Vector2Int targetChunk = currentChunk + new Vector2Int(x, y);
                //Debug.Log($"load {targetChunk.x}--{targetChunk.y}");
                if (!chunks.ContainsKey(targetChunk))
                {
                    GenerateChunk(targetChunk);
                }
            }
        }

        //delete chunks far away
        UnloadDistantChunks();
    }

    void UnloadDistantChunks()
    {
        List<Vector2Int> toRemove = new List<Vector2Int>();

        foreach (var chunk in chunks.Keys)
        {
            if (Mathf.Abs(chunk.x - currentChunk.x) > 1 || Mathf.Abs(chunk.y - currentChunk.y) > 1)
            {
                ClearChunkTiles(chunk);
                toRemove.Add(chunk);
            }
        }

        foreach (var chunk in toRemove)
        {
            chunks.Remove(chunk);
        }
    }

    void ClearChunkTiles(Vector2Int coord)
    {
        Vector3Int start = new Vector3Int(
            coord.x * chunkSize,
            coord.y * chunkSize,
            0
        );

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = start + new Vector3Int(x, y, 0);
                tilemap.SetTile(tilePos, null);
                tilemap_Wall.SetTile(tilePos, null);
            }
        }
    }

    private void GenerateCharacters()
    {
        GameObject player;
        int x = safeHouse.x;
        int y = safeHouse.y;
        Vector3 worldPos = tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
        player = Instantiate(playerPrefab, worldPos, Quaternion.identity);
        bcf.followTarget = player.transform;
        player.SetActive(true);
        player.name = "Player";
        player.GetComponent<Player>().mySafeHouse = safeHouse;
        /*
        for (int i = 0; i < monsterCount; i++)
        {
            do
            {
                x = Random.Range(0, width); y = Random.Range(0, height);
            } while (!dungeonGrid[x, y]);
            Vector3 monsterPos = tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
            GameObject mons = Instantiate(monsterPrefab, monsterPos, Quaternion.identity);
            mons.SetActive(true);
            mons.GetComponent<MonsterMovement>().player = player.transform;

        }*/
    }


    void GenerateChunk(Vector2Int chunkCoord, bool safeIn = false)
    {

        bool[,] chunkGrid = new bool[chunkSize, chunkSize];
        ChunkRooms cr = new ChunkRooms();
        RectInt rootArea = new RectInt(1, 1, chunkSize - 2, chunkSize - 2);
        BSPNode root = new BSPNode(rootArea);
        root.Split(maxDepth, minRoomSize);
        List<RectInt> rooms = new List<RectInt>();
        root.Traverse(node =>
        {
            if (node.IsLeaf && node.CreateRoom(minRoomSize))
            {
                rooms.Add(node.Room.Value);
                FillRectChunk(chunkGrid, node.Room.Value, true);
            }
        });
        if (safeIn)
        {
            int safeX = chunkSize / 2 - safeHouseSize / 2;
            int safeY = chunkSize / 2 - safeHouseSize / 2;
            safeHouse = new RectInt(safeX, safeY, safeHouseSize, safeHouseSize);
            FillRectChunk(chunkGrid, safeHouse, true);
            rooms.Add(safeHouse);
            cr.myRooms[RoomType.SafeHouse] = safeHouse;
        }

        ConnectRoomsChunk(chunkGrid, root);
        chunks[chunkCoord] = chunkGrid;
        CheckAdjacentChunks(chunkCoord, chunkGrid);
        DrawChunk(chunkCoord);
        //chunks[chunkCoord] = chunkGrid;
        //DrawChunk(chunkCoord);
        chunksGenerated++;
    }


    void CheckAdjacentChunks(Vector2Int coord, bool[,] grid)
    {
        //4 directions
        Vector2Int[] dirs = {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

        foreach (var dir in dirs)
        {
            if (chunks.ContainsKey(coord + dir))
            {
                CreateInterChunkConnection(coord, dir, grid);
            }
        }
    }

    void CreateInterChunkConnection(Vector2Int currentCoord,
                              Vector2Int direction,
                              bool[,] currentGrid)
    {
        int connectionY = Random.Range(5, chunkSize - 5);
        int connectionX = Random.Range(5, chunkSize - 5);

        Vector2Int exitPoint;
        if (direction == Vector2Int.up)
        {
            exitPoint = new Vector2Int(connectionX, chunkSize - 1);
        }
        else if (direction == Vector2Int.down)
        {
            exitPoint = new Vector2Int(connectionX, 0);
        }
        else if (direction == Vector2Int.right)
        {
            exitPoint = new Vector2Int(chunkSize - 1, connectionY);
        }
        else
        {
            exitPoint = new Vector2Int(0, connectionY);
        }

        Vector2Int entryPoint = exitPoint + direction * chunkSize;

        CarveConnectionTunnel(currentGrid, exitPoint, 3);

        if (chunks.TryGetValue(currentCoord + direction, out var neighborGrid))
        {
            CarveConnectionTunnel(neighborGrid,
                                 entryPoint - (currentCoord + direction) * chunkSize,
                                 3);
        }
    }

    Vector2Int WorldToLocal(Vector3 worldPos)
    {
        return new Vector2Int(
            (int)(worldPos.x % chunkSize),
            (int)(worldPos.y % chunkSize)
        );
    }

    Vector3 LocalToWorld(Vector2Int localPos, Vector2Int chunkCoord)
    {
        return new Vector3(
            localPos.x + chunkCoord.x * chunkSize,
            localPos.y + chunkCoord.y * chunkSize,
            0
        );
    }

    void CarveConnectionTunnel(bool[,] grid, Vector2Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int px = center.x + x;
                int py = center.y + y;

                if (px >= 0 && px < chunkSize &&
                   py >= 0 && py < chunkSize)
                {
                    grid[px, py] = true;
                }
            }
        }
    }

    bool RoomOverlaps(RectInt newRoom, List<RectInt> rooms)
    {
        foreach (var room in rooms)
        {
            if (room.Overlaps(newRoom)) return true;
        }
        return false;
    }
    Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(room.xMin + room.width / 2, room.yMin + room.height / 2);
    }
    
    void ConnectRoomsChunk(bool[,] grid, BSPNode node)
    {
        if (node.Left != null && node.Right != null)
        {
            //connect left and right node
            Vector2Int a = GetNodeRoomCenter(node.Left);
            Vector2Int b = GetNodeRoomCenter(node.Right);
            CreateCorridorChunk(grid, a, b);

            //recursively connects the rest room
            ConnectRoomsChunk(grid, node.Left);
            ConnectRoomsChunk(grid, node.Right);
        }
    }

    Vector2Int GetNodeRoomCenter(BSPNode node)
    {

        if (node.Room.HasValue)
        {
            RectInt r = node.Room.Value;
            return new Vector2Int(r.xMin + r.width / 2, r.yMin + r.height / 2);
        }
        return node.GetRoomCenter();
    }

    void CreateCorridorChunk(bool[,] grid, Vector2Int a, Vector2Int b)
    {
        //range of corridor
        int corridorWidth = Random.Range(1, 4);

        if (Random.value < 0.5f)
        {
            CreateWideCorridorChunk(grid, a, new Vector2Int(b.x, a.y), corridorWidth);
            CreateWideCorridorChunk(grid, new Vector2Int(b.x, a.y), b, corridorWidth);
        }
        else
        {
            CreateWideCorridorChunk(grid, a, new Vector2Int(a.x, b.y), corridorWidth);
            CreateWideCorridorChunk(grid, new Vector2Int(a.x, b.y), b, corridorWidth);
        }
    }

    void CreateWideCorridorChunk(bool[,] grid, Vector2Int from, Vector2Int to, int width)
    {
        foreach (Vector2Int point in GetLine(from, to))
        {
            for (int dx = -width / 2; dx <= width / 2; dx++)
            {
                for (int dy = -width / 2; dy <= width / 2; dy++)
                {
                    int px = point.x + dx;
                    int py = point.y + dy;

                    //check valid
                    if (px >= 0 && px < chunkSize &&
                        py >= 0 && py < chunkSize)
                    {
                        grid[px, py] = true;
                    }
                }
            }
        }
    }

    void CreateWideCorridor(Vector2Int from, Vector2Int to, int width)
    {
        foreach (Vector2Int point in GetLine(from, to))
        {
            for (int dx = -width / 2; dx <= width / 2; dx++)
            {
                for (int dy = -width / 2; dy <= width / 2; dy++)
                {
                    int px = point.x + dx;
                    int py = point.y + dy;
                    if (InBounds(px, py)) dungeonGrid[px, py] = true;
                }
            }
        }
    }

    void FillRectChunk(bool[,] grid, RectInt rect, bool walkable)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
            for (int y = rect.yMin; y < rect.yMax; y++)
                if (x >= 0 && y >= 0 && x < chunkSize && y < chunkSize)
                    grid[x, y] = walkable;
    }

    void FillLine(Vector2Int from, Vector2Int to, bool walkable)
    {
        foreach (Vector2Int point in GetLine(from, to))
        {
            if (InBounds(point.x, point.y))
                dungeonGrid[point.x, point.y] = walkable;
        }
    }

    bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    void BuildTilemap()
    {
        tilemap.ClearAllTiles();
        tilemap_Wall.ClearAllTiles();

        DrawChunk(currentChunk);
    }

    void DrawChunk(Vector2Int chunkCoord)
    {
        if (!chunks.TryGetValue(chunkCoord, out bool[,] grid))
            return;

        Vector3Int chunkOffset = new Vector3Int(
            chunkCoord.x * chunkSize,
            chunkCoord.y * chunkSize,
            0
        );

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0) + chunkOffset;
                Vector2Int localPos = new Vector2Int(x, y);

                if (chunkCoord == Vector2Int.zero && IsInSafeHouse(localPos))
                {
                    tilemap.SetTile(tilePos, safeHouseTile);
                }
                else if (grid[x, y])
                {
                    tilemap.SetTile(tilePos, floorTile);
                }
                else
                {
                    tilemap_Wall.SetTile(tilePos, wallTile);
                }
            }
        }
    }

    bool IsInSafeHouse(Vector2Int localPos)
    {

        int center = chunkSize / 2;
        int halfSize = safeHouseSize / 2;
        return localPos.x >= center - halfSize &&
               localPos.x <= center + halfSize &&
               localPos.y >= center - halfSize &&
               localPos.y <= center + halfSize;
    }
    int CountRoomConnections(RectInt room, bool[,] grid)
    {
        int connections = 0;
        Vector2Int center = GetRoomCenter(room);

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = center + dir;
            if (neighbor.x >= 0 && neighbor.x < chunkSize &&
                neighbor.y >= 0 && neighbor.y < chunkSize &&
                grid[neighbor.x, neighbor.y])
            {
                connections++;
            }
        }

        return connections;
    }
    List<Vector2Int> GetLine(Vector2Int a, Vector2Int b)
    {
        List<Vector2Int> line = new List<Vector2Int>();

        int dx = Mathf.Abs(b.x - a.x);
        int dy = Mathf.Abs(b.y - a.y);
        int sx = a.x < b.x ? 1 : -1;
        int sy = a.y < b.y ? 1 : -1;
        int err = dx - dy;

        int x = a.x;
        int y = a.y;

        while (true)
        {
            line.Add(new Vector2Int(x, y));
            if (x == b.x && y == b.y) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }

        return line;
    }

    //Represent a Binary Space Partitioning
    //Basically keep dividing rect into smaller area 
    class BSPNode
    {
        public RectInt Area;
        public BSPNode Left, Right;
        public RectInt? Room;

        public bool IsLeaf => Left == null && Right == null;

        public BSPNode(RectInt area)
        {
            Area = area;
        }

        public bool Split(int depth, int minSize)
        {
            if (depth <= 0 || Area.width < 2 * minSize && Area.height < 2 * minSize)
                return false;

            bool splitH = Area.width < Area.height;

            if (splitH)
            {
                int splitY = Random.Range(minSize, Area.height - minSize);
                Left = new BSPNode(new RectInt(Area.xMin, Area.yMin, Area.width, splitY));
                Right = new BSPNode(new RectInt(Area.xMin, Area.yMin + splitY, Area.width, Area.height - splitY));
            }
            else
            {
                int splitX = Random.Range(minSize, Area.width - minSize);
                Left = new BSPNode(new RectInt(Area.xMin, Area.yMin, splitX, Area.height));
                Right = new BSPNode(new RectInt(Area.xMin + splitX, Area.yMin, Area.width - splitX, Area.height));
            }
            return Left.Split(depth - 1, minSize) | Right.Split(depth - 1, minSize);
        }

        public bool CreateRoom(int margin)
        {
            if (!IsLeaf) return false;
            int w = Random.Range(margin, Area.width - 1);
            int h = Random.Range(margin, Area.height - 1);
            int x = Random.Range(Area.xMin + 1, Area.xMax - w - 1);
            int y = Random.Range(Area.yMin + 1, Area.yMax - h - 1);
            Room = new RectInt(x, y, w, h);
            return true;
        }



        Vector2Int GetRectCenter(RectInt rect)
        {
            return new Vector2Int(rect.xMin + rect.width / 2, rect.yMin + rect.height / 2);
        }

        public Vector2Int GetRoomCenter()
        {
            if (Room.HasValue)
            {
                RectInt r = Room.Value;
                return new Vector2Int(r.xMin + r.width / 2, r.yMin + r.height / 2);
            }
            else if (Left != null) return Left.GetRoomCenter();
            else return Right.GetRoomCenter();
        }

        public void Traverse(System.Action<BSPNode> callback)
        {
            callback(this);
            Left?.Traverse(callback);
            Right?.Traverse(callback);
        }
    }
}
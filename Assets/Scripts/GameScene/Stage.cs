using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

public enum AlgorithmTypes
{
    BFS,
    Dijkstra,
    AStar,
}

public class Stage : MonoBehaviour
{   
    public Sprite[] islandSprites;
    public Sprite[] fowSprites;

    public int mapWidth = 20;
    public int mapHeight = 20;
    public Vector2 tileSize = new Vector2(16, 16);

    [SerializeField] [Range(0, 1)]
    private float lakePercent;

    [SerializeField] [Range(0, 1)]
    private float coastErodePercent;

    [SerializeField]
    private int coastErodeIterator;

    [SerializeField] [Range(0, 1)]
    private float treePercent;

    [SerializeField] [Range(0, 1)]
    private float hillPercent;

    [SerializeField]
    private int mountainCount;

    [SerializeField]
    private int dungeonCount;

    [SerializeField]
    private AlgorithmTypes algorithm = AlgorithmTypes.BFS;

    [SerializeField]
    private bool is8WayPathfinding;

    public Map map;

    public GameObject tilePrefab;
    public GameObject fowPrefab;
    public List<GameObject> tileObjs = new List<GameObject>();
    public List<GameObject> fowObjs = new List<GameObject>();

    public GameObject playerPrefab;
    private GameObject player;

    public List<GameObject> coloredTiles = new List<GameObject>();

    public void Start()
    {
        map = new Map();
        OnReset();
    }

    private void CreatePlayer()
    {
        if(player != null)
        {
            Destroy(player);            
        }
        //var position = GetTilePos(map.TownTiles[0].id);
        var position = tileObjs[map.TownTiles[0].id].transform.position;
        player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.GetComponent<Player>().SetStage(this, map.TownTiles[0].id);
        RevealFog(map.TownTiles[0].id, player.GetComponent<Player>().revealFogDistance);
    }

    public void OnReset()
    {
        MakeMap();
        CreateGrid();
        CreateFog();
        CreatePlayer();
        ColorPath();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            OnMouseLeftClick();
        }
    }

    private void OnMouseLeftClick()
    {        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 17f;
        var tileId = ScreenPosToTileId(mousePos);
        Debug.Log($"ID: {tileId}");
    }

    public void MakeMap()
    {
        map.NewMap(mapWidth, mapHeight);
        bool success = map.CreateIsland(coastErodeIterator, coastErodePercent, lakePercent, treePercent,
            hillPercent, mountainCount, dungeonCount, algorithm);

        map.Update8WayTiles();
        Debug.Log($"map created ({mapWidth}, {mapHeight})");
    }

    public void ColorPath()
    {
        ResetColor();
        Debug.Log($"List size: {tileObjs.Count}");
        foreach(var tileId in map.path)
        {
            Debug.Log($"Path: {tileId}");            
            if (tileObjs[tileId] != null)
            {
                var tileGo = tileObjs[tileId];
                if(tileGo == null)
                {
                    Debug.Log($"tile id: {tileId} is null");
                }
                {                    
                    tileGo.GetComponent<SpriteRenderer>().color = Color.red;
                    coloredTiles.Add(tileGo);
                }
            }
        }
    }

    public void ResetColor()
    {
        Debug.Log($"Path color reset");
        foreach(var tileGo in coloredTiles)
        {
            tileGo.GetComponent<SpriteRenderer>().color = Color.white;
        }
        coloredTiles.Clear();
    }

    public void CreateGrid()
    {
        foreach (var go in tileObjs)
        {
            Destroy(go);
        }
        tileObjs.Clear();

        var startPos = Vector3.zero;
        startPos.x += tileSize.x * 0.5f;
        startPos.y -= tileSize.y * 0.5f;       

        var pos = startPos;

        for(int i = 0; i < mapHeight; ++i)
        {
            for(int j = 0; j < mapWidth; ++j)
            {
                var id = i * map.columns + j;
                var newGo = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                newGo.name = $"Tile ({i}, {j})";

                tileObjs.Add(newGo);
                pos.x += tileSize.x;

                //DecorateTile(id);
                var sr = newGo.GetComponent<SpriteRenderer>();
                if (map.tiles[id].autoTileId == (int)TileTypes.Empty)
                {
                    sr.sprite = null;
                }
                else
                {
                    sr.sprite = islandSprites[map.tiles[id].autoTileId];
                }
            }
            pos.x = startPos.x;
            pos.y -= tileSize.y;
        }
    }

    public void CreateFog()
    {
        foreach (var go in fowObjs)
        {
            Destroy(go);
        }
        fowObjs.Clear();

        var startPos = Vector3.zero;
        startPos.x += tileSize.x * 0.5f;
        startPos.y -= tileSize.y * 0.5f;

        var pos = startPos;

        for (int i = 0; i < mapHeight; ++i)
        {
            for (int j = 0; j < mapWidth; ++j)
            {
                var id = i * map.columns + j;
                var newGo = Instantiate(fowPrefab, pos, Quaternion.identity, transform);
                newGo.name = $"Fog ({i}, {j})";

                fowObjs.Add(newGo);
                pos.x += tileSize.x;

                //DecorateTile(id);
                var sr = newGo.GetComponent<SpriteRenderer>();
                //if (map.fows[id].autoTileId == (int)TileTypes.Empty)
                //{
                //    sr.sprite = null;
                //}
                //else
                //{
                //    sr.sprite = fowSprites[map.tiles[id].autoTileId];
                //}
                sr.sprite = fowSprites[map.fows[id].autoFowId];
            }
            pos.x = startPos.x;
            pos.y -= tileSize.y;
        }
    }

    public void RevealFog(int tileId, int distance)
    {
        distance = Mathf.Max(distance, 0);

        int columns = map.columns;
        int rows = map.rows;
        for (int i = -distance; i <= distance; ++i)
        {
            for(int j = -distance; j <= distance; ++j)
            {
                int row = tileId / columns;
                row = Mathf.Clamp(row + i, 0, rows-1);
                
                int col = tileId % columns;
                col = Mathf.Clamp(col + j, 0, columns-1);

                int newId = row * columns + col;
                map.fows[newId].OnReveal();
                //map.fows[newId].revealed = true;                
            }
        }

        distance++;
        for (int i = -distance; i <= distance; ++i)
        {
            for (int j = -distance; j <= distance; ++j)
            {
                int row = tileId / columns;
                row = Mathf.Clamp(row + i, 0, rows - 1);

                int col = tileId % columns;
                col = Mathf.Clamp(col + j, 0, columns - 1);

                int newId = row * columns + col;
                map.fows[newId].UpdateAutoFowId();
                DecorateFog(newId);
            }
        }
    }

    public void DecorateFog(int tileId)
    {
        var sr = fowObjs[tileId].GetComponent<SpriteRenderer>();
        if (map.fows[tileId].autoFowId == -1)
        {
            sr.sprite = null;
        }
        else
        {
            sr.sprite = fowSprites[map.fows[tileId].autoFowId];
        }
    }

    public void DecorateTile(int tileId)
    {
        var sr = tileObjs[tileId].GetComponent<SpriteRenderer>();
        if (map.tiles[tileId].autoTileId == (int)TileTypes.Empty)
        {
            sr.sprite = null;
        }
        else
        {
            sr.sprite = islandSprites[map.tiles[tileId].autoTileId];
        }
    }

    public int ScreenPosToTileId(Vector3 screenPos)
    {    
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return WorldPosToTileId(worldPos);
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        worldPos.x = Mathf.Clamp(worldPos.x ,0, mapWidth * 16 - 1);
        worldPos.y = Mathf.Clamp(worldPos.y, -mapHeight * 16 + 1, 0);

        int column = (int)worldPos.x / 16;
        int row = -(int)worldPos.y / 16;

        int id = row * mapWidth + column;

        return id;
    }

    public Vector3 GetTilePos(int y, int x)
    {
        Vector3 TileWorldPos = new Vector3(x * 16, y * 16);

        return TileWorldPos;
    }

    public Vector3 GetTilePos(int tileId)
    {
        tileId = Mathf.Clamp(tileId, 0, mapWidth * mapHeight);
        int column = tileId % mapWidth;
        int row = tileId / mapWidth;

        return GetTilePos(row, column);
    }

    public List<int> GetPath(int startTileId)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 17f;
        var tileId = ScreenPosToTileId(mousePos);

        Tile start = map.tiles[startTileId];
        Tile goal = map.tiles[tileId];

        if (!map.fows[tileId].revealed)
        {
            Debug.Log($"Not revealed tile");
            return null;
        }

        if(!map.AStar(start, goal, is8WayPathfinding))
            return null;

        if(map.path.Count > 1)
        {
            map.path.RemoveAt(0);
        }
        
        return map.path;
    }
}

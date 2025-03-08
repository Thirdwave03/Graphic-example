using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Map
{
    // graph
    public Tile[] tiles;
    public FogOfWar[] fows;
    public int columns;
    public int rows;

    public List<int> path = new List<int>();

    public Tile[] CoastTiles
    {
        get
        {
            return tiles.Where(x => x.autoTileId >= 0 && x.autoTileId < (int)TileTypes.Grass).ToArray();
        }
    }

    public Tile[] LandTiles =>
        tiles.Where(x => x.autoTileId >= (int)TileTypes.Grass).ToArray();

    public Tile[] TownTiles =>
        tiles.Where(x => x.autoTileId == (int)TileTypes.Town).ToArray();

    public void DestroyMap()
    {
        

    }

    public void NewMap(int width, int height)
    {
        SetTiles(width, height);
        SetFows(width, height);
    }

    private void SetTiles(int width, int height)
    {
        columns = width;
        rows = height;

        tiles = new Tile[rows * columns];
        for (int i = 0; i < tiles.Length; ++i)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        for (int r = 0; r < rows; ++r)
        {
            for (int c = 0; c < columns; ++c)
            {
                int index = r * columns + c;

                if (r - 1 >= 0)
                {
                    tiles[index].SetNeighbor(Sides.Top, tiles[index - columns]);
                }
                if (c + 1 < columns)
                {
                    tiles[index].SetNeighbor(Sides.Right, tiles[index + 1]);
                }
                if (c - 1 >= 0)
                {
                    tiles[index].SetNeighbor(Sides.Left, tiles[index - 1]);
                }
                if (r + 1 < rows)
                {
                    tiles[index].SetNeighbor(Sides.Bottom, tiles[index + columns]);
                }
            }
        }

        for (int i = 0; i < tiles.Length; ++i)
        {
            tiles[i].UpdateAutoTileId();
        }
    }

    private void SetFows(int width, int height)
    {
        columns = width;
        rows = height;

        fows = new FogOfWar[rows * columns];
        for (int i = 0; i < tiles.Length; ++i)
        {
            fows[i] = new FogOfWar();
            fows[i].id = i;
        }

        for (int r = 0; r < rows; ++r)
        {
            for (int c = 0; c < columns; ++c)
            {
                int index = r * columns + c;

                if (r - 1 >= 0)
                {
                    fows[index].SetNeighbor(Sides.Top, fows[index - columns]);
                }
                if (c + 1 < columns)
                {
                    fows[index].SetNeighbor(Sides.Right, fows[index + 1]);
                }
                if (c - 1 >= 0)
                {
                    fows[index].SetNeighbor(Sides.Left, fows[index - 1]);
                }
                if (r + 1 < rows)
                {
                    fows[index].SetNeighbor(Sides.Bottom, fows[index + columns]);
                }
            }
        }

        for (int i = 0; i < tiles.Length; ++i)
        {
            fows[i].UpdateAutoTileId();
        }
    }

    public bool CreateIsland(int erodeIteration, float erodePercent, float lakePercent, float treePercent,
        float hillPercent, int mountainCount, int dungeonCount, AlgorithmTypes algorithmType)
    {
        bool success = false;
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty); 
        for(int i =0; i < erodeIteration; ++i)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }
        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hill);
        DecorateTiles(LandTiles, mountainCount, TileTypes.Mountain);
        DecorateTiles(LandTiles, dungeonCount, TileTypes.Dungeon);

        var townAndCastleCandidate = PickCouple(LandTiles);
        if (townAndCastleCandidate == null)
            return false;

        bool connected = false;
        int count = 0;
        int countLimit = 30;
        while (!connected)
        {
            townAndCastleCandidate = PickCouple(LandTiles);
            success = connected = CheckIsConnected(townAndCastleCandidate[0], townAndCastleCandidate[1], algorithmType);
            count++;            
            if (count >= countLimit)
            {
                Debug.Log($"Exceeded iteration count {countLimit}");                
                return false;
            }
        }
        DecorateTileTownAndCastle(townAndCastleCandidate);
        return success;
    }

    private Tile[] PickCouple(Tile[] tiles)
    {
        if (tiles.Length < 2)
            return null;

        Tile[] tileCouple = new Tile[2];

        ShuffleTiles(tiles);

        for(int i = 0; i < tileCouple.Length; ++i)
        {
            tileCouple[i] = tiles[i];
        }

        return tileCouple;
    }

    private bool CheckIsConnected(Tile townAxis, Tile castleAxis, AlgorithmTypes algorithmType)
    {
        bool success = false;
        switch (algorithmType)
        {
            case AlgorithmTypes.BFS:
                success = CheckIsConnectedBFS(townAxis, castleAxis);
                break;
            case AlgorithmTypes.Dijkstra:
                success = CheckIsConnectedDijkstra(townAxis, castleAxis);
                break;
            case AlgorithmTypes.AStar:
                success = CheckIsConnectedAStar(townAxis, castleAxis);
                break;
        }
        return success;
    }

    private int Heuristic(Tile a, Tile b)
    {
        int ax = a.id % columns;
        int ay = a.id / columns;

        int bx = b.id % columns;
        int by = b.id / columns;
        
        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    private bool CheckIsConnectedDijkstra(Tile townTile, Tile castleTile)
    {
        path.Clear();
        ResetPreviousTile();

        bool success = false;
        var visited = new HashSet<Tile>();
        var pQueue = new PriorityQueueJH<Tile, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        var distances = new int[tiles.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
        }

        distances[townTile.id] = 0;

        pQueue.Enqueue(townTile, distances[townTile.id]);

        while (pQueue.Count > 0)
        {
            var currentTile = pQueue.Dequeue();

            if (currentTile == castleTile)
            {
                success = true;
            }

            //if (visited.Contains(currentTile))
            //{
            //    continue;
            //}

            visited.Add(currentTile);

            foreach (var neighbor in currentTile.neighbors)
            {
                if (neighbor == null)
                {
                    continue;
                }
                if (!(neighbor.autoTileId > 0))
                {
                    continue;
                }
                //if (visited.Contains(neighbor))
                //{
                //    continue;
                //}

                var newDistance = distances[currentTile.id] + 1;

                if (distances[neighbor.id] > newDistance)
                {
                    distances[neighbor.id] = newDistance;

                    neighbor.previous = currentTile;
                    pQueue.Enqueue(neighbor, distances[neighbor.id]);
                }
            }
        }

        if (!success)
        {
            return false;
        }

        Tile step = castleTile;

        while (step != null)
        {
            path.Add(step.id);
            if (step.previous != null)
            {
                step = step.previous;
            }
            else
            {
                step = null;
            }
        }
        path.Reverse();
        return success;
    }

    public bool AStar(Tile start, Tile goal)
    {
        path.Clear();
        ResetPreviousTile();

        bool success = false;
        var pQueue = new PriorityQueueJH<Tile, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        var distances = new int[tiles.Length];
        var scores = new int[tiles.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
            scores[i] = int.MaxValue;
        }

        distances[start.id] = 0;
        scores[start.id] = Heuristic(start, goal);

        pQueue.Enqueue(start, distances[start.id]);

        while (pQueue.Count > 0)
        {
            var currentTile = pQueue.Dequeue();

            if (currentTile == goal)
            {
                success = true;
            }

            foreach (var neighbor in currentTile.neighbors)
            {
                if (neighbor == null)
                {
                    continue;
                }
                if (!(neighbor.autoTileId > 0))
                {
                    continue;
                }

                var newDistance = distances[currentTile.id] + neighbor.Weight;

                if (distances[neighbor.id] > newDistance)
                {
                    distances[neighbor.id] = newDistance;
                    scores[neighbor.id] = distances[neighbor.id] + Heuristic(neighbor, goal);

                    neighbor.previous = currentTile;
                    pQueue.Enqueue(neighbor, scores[neighbor.id]);
                }
            }
        }

        if (!success)
        {
            return false;
        }

        Tile step = goal;

        while (step != null)
        {
            path.Add(step.id);
            if (step.previous != null)
            {
                step = step.previous;
            }
            else
            {
                step = null;
            }
        }
        path.Reverse();
        return success;
    }

    private bool CheckIsConnectedAStar(Tile townTile, Tile castleTile)
    {
        path.Clear();
        ResetPreviousTile();

        bool success = false;
        var pQueue = new PriorityQueueJH<Tile, int>(Comparer<int>.Create((x,y) => x.CompareTo(y)));

        var distances = new int[tiles.Length];
        var scores = new int[tiles.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
            scores[i] = int.MaxValue;
        }

        distances[townTile.id] = 0;
        scores[townTile.id] = Heuristic(townTile, castleTile);

        pQueue.Enqueue(townTile, distances[townTile.id]);

        while (pQueue.Count > 0)
        {
            var currentTile = pQueue.Dequeue();

            if (currentTile == castleTile)
            {
                success = true;
            }

            foreach (var neighbor in currentTile.neighbors)
            {
                if (neighbor == null)
                {
                    continue;
                }
                if (!(neighbor.autoTileId > 0))
                {
                    continue;
                }

                var newDistance = distances[currentTile.id] + neighbor.Weight;

                if (distances[neighbor.id] > newDistance)
                {
                    distances[neighbor.id] = newDistance;
                    scores[neighbor.id] = distances[neighbor.id] + Heuristic(neighbor, castleTile);

                    neighbor.previous = currentTile;
                    pQueue.Enqueue(neighbor, scores[neighbor.id]);
                }
            }
        }

        if (!success)
        {
            return false;
        }

        Tile step = castleTile;        

        while (step != null)
        {
            path.Add(step.id);
            if (step.previous != null)
            {
                step = step.previous;
            }
            else
            {
                step = null;
            }
        }
        path.Reverse();
        return success;
    }

    private void ResetPreviousTile()
    {
        foreach (var tile in tiles)
        {
            if(tile != null)
            {
                tile.previous = null;
            }
        }
    }

    private bool CheckIsConnectedBFS(Tile townAxis, Tile castleAxis)
    {
        path.Clear();
        ResetPreviousTile();

        bool success = false;
        var visited = new HashSet<Tile>();
        var queue = new Queue<Tile>();

        queue.Enqueue(townAxis);

        while(queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (currentTile == castleAxis)
            {
                success = true;
            }

            visited.Add(currentTile);
            
            foreach(var neighbor in currentTile.neighbors)
            {
                if(visited.Contains(neighbor))
                {
                    continue;
                }
                if(neighbor == null)
                {
                    continue;
                }
                if(!(neighbor.autoTileId > 0))
                {
                    continue;
                }
                queue.Enqueue(neighbor);
                visited.Add(neighbor);
                neighbor.previous = currentTile;
            }
        }

        if (!success)
        {
            return false;
        }

        Tile step = castleAxis;

        while (step != null)
        {                        
            path.Add(step.id);
            if (step.previous != null)
            {
                step = step.previous;
            }
            else
            {
                step = null;
            }
        }

        path.Reverse();
        return success;
    }

    private void DecorateTileTownAndCastle(Tile[] tiles)
    {
        tiles[0].autoTileId = (int)TileTypes.Town;
        tiles[1].autoTileId = (int)TileTypes.Castle;
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        ShuffleTiles(tiles);

        int total = Mathf.FloorToInt(tiles.Length * percent);

        for (int i = 0; i < total; ++i)
        {
            switch (tileType)
            {
                case TileTypes.Empty:
                    tiles[i].ClearNeighbors();
                    break;
                case TileTypes.Grass:
                    break;
            }
            tiles[i].autoTileId = (int)tileType;
        }
    }

    public void DecorateTiles(Tile[] tiles, int count, TileTypes tileType)
    {
        ShuffleTiles(tiles);
        count = Mathf.Clamp(count, 0, tiles.Length);

        for (int i = 0; i < count; ++i)
        {
            switch (tileType)
            {
                case TileTypes.Empty:
                    tiles[i].ClearNeighbors();
                    break;
                case TileTypes.Grass:
                    break;
            }
            tiles[i].autoTileId = (int)tileType;
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i >= 0; --i)
        {
            int rand = Random.Range(0, i + 1);

            Tile tile = tiles[i];
            tiles[i] = tiles[rand];
            tiles[rand] = tile;
        }

    }
}

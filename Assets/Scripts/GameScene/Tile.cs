using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

public enum TileTypes
{
    Empty = -1,
    Grass = 15,
    Tree,
    Hill,
    Mountain,
    Town,
    Castle,
    Dungeon,
}

public enum Sides
{
    Bottom,
    Right,
    Left,
    Top,
    Count,
}

public enum Sides8Way
{
    N,
    NE,
    E,
    SE,
    S,
    SW,
    W,
    NW,
    Count,
}

public class Tile
{
    // node
    public int id = 0;
    public Tile[] neighbors = new Tile[(int)Sides.Count];
    public Tile[] neighbors8way = new Tile[(int)Sides8Way.Count];
    public Tile previous;

    public int autoTileId = 0;

    public int Weight => (TileTypes)autoTileId switch
    {        
        TileTypes.Empty => 1,
        TileTypes.Grass => 1,
        TileTypes.Tree => 3,
        TileTypes.Hill => 8,
        TileTypes.Mountain => 20,
        TileTypes.Town => 10,
        TileTypes.Castle => 20,
        TileTypes.Dungeon => 20,
        _ => 1,
    };

    public void SetNeighbor(Sides side, Tile neighbor)
    {
        neighbors[(int)side] = neighbor;
    }

    public void SetNeighbor(Sides8Way side, Tile neighbor)
    {
        neighbors8way[(int)side] = neighbor;
    }

    public void Update8WayNeighbor()
    {
        if (neighbors8way[(int)Sides8Way.N] == null)
        {
            if (neighbors8way[(int)Sides8Way.E] == null)
            {
                neighbors8way[(int)Sides8Way.NE] = null;
            }
            if(neighbors8way[(int)Sides8Way.W] == null)
            {
                neighbors8way[(int)Sides8Way.NW] = null;
            }
        }        
        if (neighbors8way[(int)Sides8Way.S] == null)
        {
            if (neighbors8way[(int)Sides8Way.E] == null)
            {
                neighbors8way[(int)Sides8Way.SE] = null;
            }
            if(neighbors8way[(int)Sides8Way.W] == null)
            {
                neighbors8way[(int)Sides8Way.SW] = null;
            }
        }        
    }

    public void UpdateAutoTileId()
    {
        // DRLU
        autoTileId = 0;
        for (int i= 0;i < neighbors.Length; i++)
        {
            if (neighbors[i] != null)
            {
                autoTileId |= 1 << neighbors.Length - 1 - i;
            }
        }        
    }

    public void RemoveNeighbor(Tile tile)
    {
        for(int i = 0; i< neighbors.Length; ++i)
        {
            if(neighbors[i] == tile)
            {
                neighbors[i] = null;
            }
        }
        UpdateAutoTileId();
    }

    public void ClearNeighbors()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] != null)
            {
                neighbors[i].RemoveNeighbor(this);
                neighbors[i] = null;                
            }
        }
        UpdateAutoTileId();
    }

    public void RemoveNeighbor8Way(Tile tile)
    {
        for (int i = 0; i < neighbors8way.Length; ++i)
        {
            if (neighbors8way[i] == tile)
            {
                neighbors8way[i] = null;
            }
        }
        UpdateAutoTileId();
    }

    public void ClearNeighbors8Way()
    {
        for (int i = 0; i < neighbors8way.Length; i++)
        {
            if (neighbors8way[i] != null)
            {
                neighbors8way[i].RemoveNeighbor8Way(this);
                neighbors8way[i] = null;
            }
        }
        UpdateAutoTileId();
    }
}

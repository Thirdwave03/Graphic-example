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

public class Tile
{
    // node
    public int id = 0;
    public Tile[] neighbors = new Tile[(int)Sides.Count];
    public Tile previous;

    public int autoTileId = 0;

    //public bool isSteppable
    //{
    //    get
    //    {
    //        
    //    }
    //}

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

}

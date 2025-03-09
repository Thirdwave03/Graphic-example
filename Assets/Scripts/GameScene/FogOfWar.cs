using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FogOfWar 
{
    public int id;
    public FogOfWar[] neighbors = new FogOfWar[(int)Sides.Count];
    public FogOfWar previous;

    public int autoFowId = 0;
    public bool revealed = false;

    public void UpdateAutoFowId()
    {
        // DRLU
        if (revealed)
        {
            autoFowId = -1;
            return;
        }
 
        autoFowId = 15;
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] == null)
            {
                continue;
            }

            if (neighbors[i].revealed)
            {
                autoFowId &= ~(1 << neighbors.Length - 1 - i);
            }
        }
    }

    //public void RemoveNeighbor(FogOfWar fow)
    //{
    //    for (int i = 0; i < neighbors.Length; ++i)
    //    {
    //        if (neighbors[i] == fow)
    //        {
    //            neighbors[i] = null;
    //        }
    //    }
    //    UpdateAutoFowId();
    //}

    //public void ClearNeighbors()
    //{
    //    for (int i = 0; i < neighbors.Length; i++)
    //    {
    //        if (neighbors[i] != null)
    //        {
    //            neighbors[i].RemoveNeighbor(this);
    //            neighbors[i] = null;
    //        }
    //    }
    //    UpdateAutoFowId();
    //}

    public void OnReveal()
    {
        revealed = true;
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] != null)
            {
                neighbors[i].UpdateNeighbor(this);
            }
        }
        UpdateAutoFowId();
    }

    public void UpdateNeighbor(FogOfWar fow)
    {
        for (int i = 0; i < neighbors.Length; ++i)
        {
            if (neighbors[i] == fow)
            {
                neighbors[i].revealed = true;
            }
        }
        UpdateAutoFowId();
    }

    public void SetNeighbor(Sides side, FogOfWar neighbor)
    {
        neighbors[(int)side] = neighbor;
    }
}

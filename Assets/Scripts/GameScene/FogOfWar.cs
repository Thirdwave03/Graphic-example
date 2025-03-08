using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FogOfWar : MonoBehaviour
{
    public int id;
    public FogOfWar[] neighbors = new FogOfWar[(int)Sides.Count];
    public FogOfWar previous;

    public int autoTileId = 0;
    public bool revealed = false;

    public void UpdateAutoTileId()
    {
        // DRLU
        autoTileId = (int)TileTypes.Grass;
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] == null)
                continue;

            if (neighbors[i].revealed)
            {
                autoTileId &= ~(1 << neighbors.Length - 1 - i);
            }
        }
    }

    public void Revealed(FogOfWar fow)
    {
        for (int i = 0; i < neighbors.Length; ++i)
        {
            if (neighbors[i] == fow)
            {
                neighbors[i] = null;
            }
        }
        UpdateAutoTileId();
    }

    public void SetNeighbor(Sides side, FogOfWar neighbor)
    {
        neighbors[(int)side] = neighbor;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{  
    public Stage stage;

    private List<int> path = new List<int>();
    private int currentTileId;
    private int targetTileId;
    public int revealFogDistance = 3;

    private Vector3 CurrentTilePos { get; set; }

    private Vector3 TargetTilePos { get; set; }

    private float moveTimer = 0f;
    private float moveDuration = 0.3f;

    private bool isMoving = false;
    private bool IsPathAvailable
    {
        get
        {
            if (path == null)
                return false;

            if (path.Count == 0)
                return false;
            return true;
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            TryGetPath(isMoving);
        }

        if(IsPathAvailable && !isMoving)
        {
            Move();
        }

        if(isMoving)
        {
            UpdatePlayerPos();
        }
    }
    
    private void Move()
    {
        // Need to deal with startTile that is included in path
        isMoving = true;
        moveTimer = 0f;
        targetTileId =  path[0];
        TargetTilePos = stage.tileObjs[targetTileId].transform.position;
        path.RemoveAt(0);
        stage.map.path = path;
        if (stage.coloredTiles.Count > 0)
        {
            stage.coloredTiles[0].GetComponent<SpriteRenderer>().color = Color.white;
            stage.coloredTiles.RemoveAt(0);
        }
        else
        {
            Debug.LogError($"Colored Tiles List empty");
        }
    }

    private void UpdatePlayerPos()
    {
        moveTimer += Time.deltaTime;
        moveTimer = Mathf.Clamp(moveTimer, 0f, moveDuration);
        transform.position = Vector3.Lerp(CurrentTilePos, TargetTilePos, moveTimer / moveDuration);
        
        if(moveTimer >= moveDuration)
        {
            currentTileId = targetTileId;
            CurrentTilePos = stage.tileObjs[currentTileId].transform.position;
            stage.RevealFog(currentTileId, revealFogDistance);
            isMoving = false;
        }
    }

    public void SetStage(Stage stage, int initialTileId)
    {
        this.stage = stage;
        currentTileId = initialTileId;
        CurrentTilePos = stage.tileObjs[currentTileId].transform.position;
    }

    private bool TryGetPath(bool isMoving)
    {
        if (!isMoving)
        {
            path = stage.GetPath(currentTileId);            
        }
        else
        {
            path = stage.GetPath(targetTileId);
        }

        if (path == null)
        {
            stage.ResetColor();
            Debug.Log($"path null");
            return false;
        }

        stage.ColorPath();

        return path.Count > 0;
    }

}

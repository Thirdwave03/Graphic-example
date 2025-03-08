using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    public enum Algorithm
    {
        DFS,
        BFS,
        RecursiveDFS,
        PathFinding,
        PathFindingBFS,
        Dijkstra,
        AStar,
    }

    public UiNode nodePrefab;
    public List<UiNode> uiNodes;
    [SerializeField] private int startNode = 0;
    [SerializeField] private int goalNode = 24;
    [SerializeField] private Algorithm algorithm = Algorithm.DFS;

    private void Start()
    {
        CallAlgorithm();
    }

    public void CallAlgorithm()
    {
        foreach(var uiNode in uiNodes)
        {
            Destroy(uiNode.gameObject);
        }
        uiNodes.Clear();

        int[,] map = new int[5, 5]
        {
            { 1, -1, 1, 1, 1 },
            { 1, -1, 10, 5, 1 },
            { 1, -1, 10, 5, 1 },
            { 1, -1, 10, -1, 1 },
            { 1,  1, 1, 1, 1 },
        };
        var graph = new Graph();
        graph.Init(map);
        InitUiNodes(graph);
        var search = new GraphSearch();
        search.Init(graph);
        switch (algorithm)
        {
            case Algorithm.DFS:
                search.DFS(graph.nodes[startNode]);
                break;
            case Algorithm.BFS:
                search.BFS(graph.nodes[startNode]);
                break;
            case Algorithm.RecursiveDFS:
                search.StartRecursiveDFS(graph.nodes[startNode]);
                break;
            case Algorithm.PathFinding:
                search.PathFinding(graph.nodes[startNode], graph.nodes[goalNode]);
                break;
            case Algorithm.PathFindingBFS:
                search.PathFindingBFS(graph.nodes[startNode], graph.nodes[goalNode]);
                break;
            case Algorithm.Dijkstra:
                search.Dijkstra(graph.nodes[startNode], graph.nodes[goalNode]);
                break;
            case Algorithm.AStar:
                search.AStar(graph.nodes[startNode], graph.nodes[goalNode]);
                break;
        }
        for (int i = 0; i < search.path.Count; ++i)
        {
            var node = search.path[i];
            var color = Color.Lerp(Color.white, Color.green, (float)i / search.path.Count);

            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetText($"ID: {node.id}\nWeight: {node.weight}\nPath: {i}");
        }
    }

    private void InitUiNodes(Graph graph)
    {
        foreach (var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab, transform);
            uiNode.SetNode(node);
            uiNodes.Add(uiNode);
        }
    }


}

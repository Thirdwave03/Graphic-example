using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSearch
{
    private Graph graph;

    // contains path with visited order
    public List<Node> path = new List<Node>();

    public Dictionary<List<List<Node>>, int> pathsDic;

    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    public void DFS(Node node)
    {
        path.Clear();

        var visited = new HashSet<Node>();
        var stack = new Stack<Node>();
        
        stack.Push(node);

        while(stack.Count > 0)
        {
            var currentNode = stack.Pop();
            path.Add(currentNode);
            visited.Add(currentNode);
            foreach(var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent) || stack.Contains(adjacent))
                    continue;

                stack.Push(adjacent);
            }
        }
    }

    private void RecursiveDFS(HashSet<Node> visited, Node node)
    {
        path.Add(node);
        visited.Add(node);
        foreach(var adjacent in node.adjacents)
        {
            if (!adjacent.CanVisit || visited.Contains(adjacent))
                continue;

            RecursiveDFS(visited, adjacent);
        }      
    }

    public void StartRecursiveDFS(Node node)
    {
        path.Clear();
        var visited = new HashSet<Node>();

        RecursiveDFS(visited, node);
    }
    
    public void BFS(Node node)
    {
        path.Clear();
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(node);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            path.Add(currentNode);
            visited.Add(currentNode);
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent) || queue.Contains(adjacent))
                    continue;
                queue.Enqueue(adjacent);
            }
        }
    }
    
    public void LeveledBFS(Node startNode, Node goalNode)
    {
        path.Clear();
        var nodeDic = new Dictionary<Node, int>();
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(startNode);
        int level = 0;
        nodeDic.Add(startNode, level);
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            visited.Add(currentNode);            
            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent) || queue.Contains(adjacent))
                    continue;
                queue.Enqueue(adjacent);
                if(!nodeDic.ContainsKey(adjacent))
                    nodeDic.Add(adjacent, nodeDic[currentNode] + 1);
                if (adjacent == goalNode)
                    break;
            }
        }

        foreach(var nodeLevel in nodeDic)
        {
            Debug.Log($"node: {nodeLevel.Key.id}\nlevel: {nodeLevel.Value}");
        }

        int goalLvl = nodeDic[goalNode];
        var reversePath = new Stack<Node>();

        reversePath.Push(goalNode);
        while(goalLvl > 0)
        {
            var currentNode = reversePath.Peek();
            
            foreach(var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || adjacent == null)
                    continue;
                if (nodeDic[adjacent] == goalLvl - 1)
                {
                    reversePath.Push(adjacent);
                    break;
                }    
            }
            goalLvl--;
        }
        
        while(reversePath.Count > 0)
        {
            var node = reversePath.Pop();
            path.Add(node);
        }
    }
    

    public bool PathFinding (Node startNode, Node goalNode)
    {
        if (!PathValidityCheck(startNode, goalNode))
            return false;

        LeveledBFS(startNode, goalNode);
        return true;
    }

    public bool PathValidityCheck(Node startNode, Node goalNode)
    {
        StartRecursiveDFS(startNode);
        if (path.Contains(goalNode))
            return true;

        path.Clear();
        return false;
    }

    public bool PathFindingBFS(Node start, Node goal)
    {
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(start);
        bool success = false;

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            if(currentNode == goal)
            {
                success = true;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent) || queue.Contains(adjacent))
                    continue;
                queue.Enqueue(adjacent);
                adjacent.previous = currentNode;
            }
        }

        if (!success)
        { 
            return false;
        }

        Node step = goal;
        while (step != null && step.previous != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();
        return false;
    }

    public bool Dijkstra(Node start, Node goal)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var pQueue = new PriorityQueueJH<Node, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        var distances = new int[graph.nodes.Length];
        for(int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
        }

        distances[start.id] = 0;
        pQueue.Enqueue(start, distances[start.id]);
        bool success = false;

        while (pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();

            if(visited.Contains(currentNode))
            {
                continue;
            }

            if(currentNode == goal)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit)
                    continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;

                if (newDistance < distances[adjacent.id])
                {
                    distances[adjacent.id] = newDistance;
                    adjacent.previous = currentNode;
                    pQueue.Enqueue(adjacent, newDistance);
                }                
            }
        }

        if(!success)
        {
            return false;
        }
        Node step = goal;
        while (step != null && step.previous != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return success;
    }

    private int Heuristic(Node a, Node b)
    {
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool AStar(Node start, Node goal)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<Node>();
        var pQueue = new PriorityQueueJH<Node, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        var distances = new int[graph.nodes.Length];
        var scores = new int[graph.nodes.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] = int.MaxValue;
            scores[i] = int.MaxValue;
        }

        distances[start.id] = 0;
        scores[start.id] = Heuristic(start, goal);

        pQueue.Enqueue(start, distances[start.id]);
        bool success = false;

        while (pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();

            if (visited.Contains(currentNode))
            {
                continue;
            }

            if (currentNode == goal)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit)
                    continue;

                var newDistance = distances[currentNode.id] + adjacent.weight;

                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = distances[adjacent.id] + Heuristic(adjacent, goal);

                    adjacent.previous = currentNode;
                    pQueue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        if (!success)
        {
            return false;
        }
        Node step = goal;
        while (step != null && step.previous != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();

        return success;
    }
}

using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridSystem))]
public class Pathfinding : MonoBehaviour
{
    private GridSystem grid;

    private void Awake()
    {
        grid = GetComponent<GridSystem>();
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        PathNode startNode = grid.NodeFromWorldPoint(startPos);
        PathNode targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !targetNode.walkable)
        {
            return null;
        }

        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Seleccionar el nodo con menor F Cost (o menor H Cost en caso de empate)
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || 
                   (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Objetivo alcanzado: reconstruir y devolver la ruta
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Evaluar los 8 vecinos inmediatos
            foreach (PathNode neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                // Coste acumulado para llegar al vecino a través del nodo actual
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }

        return null; // No existe camino transitable
    }

    // Reconstrucción del camino trazando los punteros parent en orden inverso
    private List<Vector3> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector3> waypoints = new List<Vector3>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            waypoints.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }

        waypoints.Reverse();
        return waypoints;
    }

    // Heurística de distancia octil/diagonal (10 para ortogonal, 14 para diagonal: ~10 * sqrt(2))
    private int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }
}

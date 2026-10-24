using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    GridManager grid;

    void Awake()
    {
        grid = GetComponent<GridManager>();
    }

    // Función pública: recibe cualquier punto A y punto B, y devuelve la lista de coordenadas
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // Si el punto de inicio o el de destino están dentro de un obstáculo, no hay camino viable
        if (!startNode.walkable || !targetNode.walkable)
        {
            return new List<Vector3>();
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);
        startNode.gCost = 0;

        while (openSet.Count > 0)
        {
            // Criterio UCS: seleccionamos siempre el nodo con menor coste acumulado
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].gCost < currentNode.gCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Si llegamos a la casilla de destino, reconstruimos y devolvemos la ruta
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Exploramos los vecinos transitables
            foreach (Node neighbor in grid.GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + neighbor.cost;

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // Si la lista abierta se vacía sin alcanzar el destino, no existe ruta posible
        return new List<Vector3>();
    }

    // Reconstruye el camino desde el destino hacia el origen usando los punteros parent
    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        // Convertimos la lista de nodos abstractos en posiciones 3D reales del mundo
        List<Vector3> waypoints = new List<Vector3>();
        for (int i = 0; i < path.Count; i++)
        {
            waypoints.Add(path[i].worldPosition);
        }

        // Mantenemos la línea cian para depuración visual en la ventana Scene
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Debug.DrawLine(waypoints[i] + Vector3.up * 0.5f, 
                           waypoints[i + 1] + Vector3.up * 0.5f, 
                           Color.cyan);
        }

        return waypoints;
    }
}
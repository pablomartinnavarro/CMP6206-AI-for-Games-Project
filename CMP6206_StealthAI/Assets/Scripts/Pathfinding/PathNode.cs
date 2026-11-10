using UnityEngine;

public class PathNode
{
    // Estado y posición
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    // Costes del algoritmo A*
    public int gCost; // Distancia desde el nodo inicial
    public int hCost; // Heurística: distancia estimada hasta el objetivo
    public PathNode parent; // Nodo previo para reconstruir el camino óptimo

    // Función de evaluación f(n) = g(n) + h(n)
    public int FCost => gCost + hCost;

    public PathNode(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}

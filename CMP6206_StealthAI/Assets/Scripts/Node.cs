using UnityEngine;

public class Node
{
    public bool walkable;       // ¿Puede el guardia pasar por aquí? (true = sí, false = muro/caja)
    public Vector3 worldPosition; // Posición exacta en coordenadas 3D del mundo
    public int gridX;           // Columna dentro de la matriz
    public int gridY;           // Fila dentro de la matriz

    public int cost = 1;        // Coste de cruzar este nodo (para UCS/Dijkstra)
    public int gCost;           // Coste acumulado desde el inicio hasta este nodo
    public Node parent;         // Nodo anterior en el camino óptimo (para reconstruir la ruta)

    // Constructor para inicializar el nodo al escanear el mapa
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}
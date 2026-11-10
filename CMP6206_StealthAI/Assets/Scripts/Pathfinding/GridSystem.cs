using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Dimensions & Resolution")]
    [SerializeField] private Vector2 gridWorldSize = new Vector2(30f, 30f);
    [SerializeField] private float nodeRadius = 0.5f;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask unwalkableMask;

    [Header("Debug Visualization")]
    [SerializeField] private bool displayGridGizmos = true;

    private PathNode[,] grid;
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;

    public int MaxSize => gridSizeX * gridSizeY;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new PathNode[gridSizeX, gridSizeY];

        // Punto de origen inferior izquierdo (esquina suroeste) de la cuadrícula en coordenadas del mundo
        Vector3 worldBottomLeft = transform.position 
            - Vector3.right * (gridWorldSize.x / 2f) 
            - Vector3.forward * (gridWorldSize.y / 2f);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Posición central tridimensional de cada celda
                Vector3 worldPoint = worldBottomLeft 
                    + Vector3.right * (x * nodeDiameter + nodeRadius) 
                    + Vector3.forward * (y * nodeDiameter + nodeRadius);

                // Comprobación de colisiones con obstáculos mediante físicas
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius * 0.9f, unwalkableMask);

                grid[x, y] = new PathNode(walkable, worldPoint, x, y);
            }
        }
    }

    // Devuelve los 8 vecinos inmediatos (ortogonales y diagonales) de un nodo
    public List<PathNode> GetNeighbours(PathNode node)
    {
        List<PathNode> neighbours = new List<PathNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // Mapea cualquier posición 3D continua de Unity a la celda discreta correspondiente de la matriz
    public PathNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2f) / gridWorldSize.x;
        float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2f) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.FloorToInt(Mathf.Clamp((gridSizeX * percentX), 0, gridSizeX - 1));
        int y = Mathf.FloorToInt(Mathf.Clamp((gridSizeY * percentY), 0, gridSizeY - 1));

        return grid[x, y];
    }

    private void OnDrawGizmos()
    {
        // Delimitador exterior de la cuadrícula
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1f, gridWorldSize.y));

        // Dibujar cada celda durante la ejecución: blanco (caminable) y rojo (obstáculo)
        if (grid != null && displayGridGizmos)
        {
            foreach (PathNode n in grid)
            {
                Gizmos.color = n.walkable ? new Color(1f, 1f, 1f, 0.25f) : new Color(1f, 0f, 0f, 0.7f);
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}

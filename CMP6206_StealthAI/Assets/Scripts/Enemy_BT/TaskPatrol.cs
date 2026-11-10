using System.Collections.Generic;
using UnityEngine;

public class TaskPatrol : BTNode
{
    private Transform enemyTransform;
    private Pathfinding pathfinding;
    private Transform[] waypoints;
    private float patrolSpeed;
    private float rotationSpeed;
    private float waypointTolerance;

    private int currentWaypointIndex = 0;
    private List<Vector3> currentPath;
    private int pathIndex = 0;

    public TaskPatrol(Transform enemyTransform, Pathfinding pathfinding, Transform[] waypoints, float patrolSpeed, float rotationSpeed, float waypointTolerance = 1.0f)
    {
        this.enemyTransform = enemyTransform;
        this.pathfinding = pathfinding;
        this.waypoints = waypoints;
        this.patrolSpeed = patrolSpeed;
        this.rotationSpeed = rotationSpeed;
        this.waypointTolerance = waypointTolerance;
    }

    public override NodeState Evaluate()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // 1. Si no hay camino calculado o ya llegamos al final del camino actual, pedir nueva ruta A*
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            currentPath = pathfinding.FindPath(enemyTransform.position, targetWaypoint.position);
            pathIndex = 0;

            if (currentPath == null || currentPath.Count == 0)
            {
                // Si estamos en la misma celda que el waypoint, pasar de inmediato al siguiente
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                state = NodeState.SUCCESS;
                return state;
            }
        }

        // 2. Desplazamiento cinemático nodo a nodo
        Vector3 targetPoint = currentPath[pathIndex];
        targetPoint.y = enemyTransform.position.y;

        Vector3 moveDir = (targetPoint - enemyTransform.position).normalized;
        enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, targetPoint, patrolSpeed * Time.deltaTime);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Comprobar avance al siguiente nodo de la cuadrícula
        if (Vector2.Distance(new Vector2(enemyTransform.position.x, enemyTransform.position.z),
                             new Vector2(targetPoint.x, targetPoint.z)) < 0.35f)
        {
            pathIndex++;
        }

        // Comprobar llegada al punto final de la patrulla
        if (Vector2.Distance(new Vector2(enemyTransform.position.x, enemyTransform.position.z),
                             new Vector2(targetWaypoint.position.x, targetWaypoint.position.z)) <= waypointTolerance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            currentPath = null;
            state = NodeState.SUCCESS;
            return state;
        }

        // Mientras siga en tránsito, la tarea permanece activa
        state = NodeState.RUNNING;
        return state;
    }
}
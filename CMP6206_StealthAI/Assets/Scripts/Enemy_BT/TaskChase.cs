using System.Collections.Generic;
using UnityEngine;

public class TaskChase : BTNode
{
    private Transform enemyTransform;
    private Pathfinding pathfinding;
    private float chaseSpeed;
    private float rotationSpeed;

    private List<Vector3> currentPath;
    private int pathIndex = 0;
    private float repathTimer = 0.0f;
    private const float RepathInterval = 0.25f;

    public TaskChase(Transform enemyTransform, Pathfinding pathfinding, float chaseSpeed, float rotationSpeed)
    {
        this.enemyTransform = enemyTransform;
        this.pathfinding = pathfinding;
        this.chaseSpeed = chaseSpeed;
        this.rotationSpeed = rotationSpeed;
    }

    public override NodeState Evaluate()
    {
        object targetObj = GetData("target");

        if (targetObj == null)
        {
            currentPath = null;
            state = NodeState.FAILURE;
            return state;
        }

        Transform targetTransform = (Transform)targetObj;
        float flatDistance = Vector2.Distance(
            new Vector2(enemyTransform.position.x, enemyTransform.position.z),
            new Vector2(targetTransform.position.x, targetTransform.position.z)
        );

        // Si está a quemarropa (< 1.2 m), empujar directamente contra el jugador sin esperar a A*
        if (flatDistance <= 1.2f)
        {
            Vector3 directDir = (targetTransform.position - enemyTransform.position).normalized;
            directDir.y = 0;
            enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, targetTransform.position, chaseSpeed * Time.deltaTime);

            if (directDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(directDir);
                enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            state = NodeState.RUNNING;
            return state;
        }

        // Recalcular ruta periódicamente
        repathTimer += Time.deltaTime;
        if (repathTimer >= RepathInterval || currentPath == null)
        {
            repathTimer = 0f;
            currentPath = pathfinding.FindPath(enemyTransform.position, targetTransform.position);
            pathIndex = 0;

            // Si están en la misma celda de la cuadrícula, asignar la posición continua directamente
            if (currentPath == null || currentPath.Count == 0)
            {
                currentPath = new List<Vector3> { targetTransform.position };
                pathIndex = 0;
            }
        }

        // Seguir los waypoints calculados por A*
        if (currentPath != null && pathIndex < currentPath.Count)
        {
            Vector3 targetPoint = currentPath[pathIndex];
            targetPoint.y = enemyTransform.position.y;

            Vector3 moveDir = (targetPoint - enemyTransform.position).normalized;
            enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, targetPoint, chaseSpeed * Time.deltaTime);

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                enemyTransform.rotation = Quaternion.Slerp(enemyTransform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            if (Vector2.Distance(new Vector2(enemyTransform.position.x, enemyTransform.position.z),
                                 new Vector2(targetPoint.x, targetPoint.z)) < 0.35f)
            {
                pathIndex++;
            }

            state = NodeState.RUNNING;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
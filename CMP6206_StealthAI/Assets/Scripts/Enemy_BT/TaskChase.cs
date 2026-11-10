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
    private const float RepathInterval = 0.3f;

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

        // Si no hay objetivo activo en la memoria, la tarea no puede continuar
        if (targetObj == null)
        {
            currentPath = null;
            state = NodeState.FAILURE;
            return state;
        }

        Transform targetTransform = (Transform)targetObj;

        // Recalcular ruta periódicamente para adaptarse al movimiento en tiempo real del jugador
        repathTimer += Time.deltaTime;
        if (repathTimer >= RepathInterval || currentPath == null)
        {
            repathTimer = 0f;
            currentPath = pathfinding.FindPath(enemyTransform.position, targetTransform.position);
            pathIndex = 0;
        }

        // Desplazamiento a través de los nodos calculados por A*
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

        state = NodeState.SUCCESS;
        return state;
    }
}
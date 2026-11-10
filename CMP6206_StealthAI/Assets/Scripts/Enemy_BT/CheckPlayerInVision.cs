using UnityEngine;

public class CheckPlayerInVision : BTNode
{
    private Transform enemyTransform;
    private Transform playerTransform;
    private LayerMask obstacleMask;
    private float viewRadius;
    private float viewAngle;

    public CheckPlayerInVision(Transform enemyTransform, LayerMask obstacleMask, float viewRadius, float viewAngle)
    {
        this.enemyTransform = enemyTransform;
        this.obstacleMask = obstacleMask;
        this.viewRadius = viewRadius;
        this.viewAngle = viewAngle;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    public override NodeState Evaluate()
    {
        if (playerTransform == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        Vector3 eyePos = enemyTransform.position + Vector3.up * 0.5f;
        Vector3 targetPos = playerTransform.position + Vector3.up * 0.5f;
        Vector3 dirToTarget = (targetPos - eyePos).normalized;
        float dstToTarget = Vector3.Distance(eyePos, targetPos);

        // 1. Filtro de distancia
        if (dstToTarget <= viewRadius)
        {
            // 2. Filtro angular del cono
            if (Vector3.Angle(enemyTransform.forward, dirToTarget) < viewAngle / 2f)
            {
                // 3. Comprobación de oclusión física por coberturas
                if (!Physics.Raycast(eyePos, dirToTarget, dstToTarget, obstacleMask))
                {
                    // Jugador visible: guardamos el objetivo en la Blackboard del árbol
                    parent.SetData("target", playerTransform);
                    state = NodeState.SUCCESS;
                    return state;
                }
            }
        }

        // Si no lo vemos, limpiamos el objetivo directo de la Blackboard
        parent.ClearData("target");
        state = NodeState.FAILURE;
        return state;
    }
}
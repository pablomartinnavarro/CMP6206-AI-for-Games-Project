using UnityEngine;

public class CheckPlayerInVision : BTNode
{
    private Transform enemyTransform;
    private Transform playerTransform;
    private PlayerController playerController;
    private LayerMask obstacleMask;
    private float viewRadius;
    private float viewAngle;
    private float closeContactRadius = 2.0f; // Detección física inmediata en 360 grados

    public CheckPlayerInVision(Transform enemyTransform, LayerMask obstacleMask, float viewRadius, float viewAngle, float closeContactRadius = 2.0f)
    {
        this.enemyTransform = enemyTransform;
        this.obstacleMask = obstacleMask;
        this.viewRadius = viewRadius;
        this.viewAngle = viewAngle;
        this.closeContactRadius = closeContactRadius;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
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

        // 1. Contacto a quemarropa: a menos de 2 metros te detecta en 360° si no hay un muro en medio
        if (dstToTarget <= closeContactRadius)
        {
            if (!Physics.Raycast(eyePos, dirToTarget, dstToTarget, obstacleMask))
            {
                parent.SetData("target", playerTransform);
                state = NodeState.SUCCESS;
                return state;
            }
        }

        // 2. Detección auditiva: si corres o caminas dentro del radio de ruido
        if (playerController != null && dstToTarget <= playerController.currentNoiseRadius)
        {
            parent.SetData("target", playerTransform);
            state = NodeState.SUCCESS;
            return state;
        }

        // 3. Cono de visión frontal angular
        if (dstToTarget <= viewRadius)
        {
            if (Vector3.Angle(enemyTransform.forward, dirToTarget) < viewAngle / 2f)
            {
                if (!Physics.Raycast(eyePos, dirToTarget, dstToTarget, obstacleMask))
                {
                    parent.SetData("target", playerTransform);
                    state = NodeState.SUCCESS;
                    return state;
                }
            }
        }

        parent.ClearData("target");
        state = NodeState.FAILURE;
        return state;
    }
}
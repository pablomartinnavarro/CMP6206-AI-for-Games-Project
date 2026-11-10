using System.Collections.Generic;
using UnityEngine;

public enum FSMState
{
    Patrol,
    Investigate,
    Chase
}

public class EnemyFSM : MonoBehaviour
{
    [Header("Current State")]
    [SerializeField] private FSMState currentState = FSMState.Patrol;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 4.0f;
    [SerializeField] private float rotationSpeed = 10.0f; // Giro más ágil para no desorientarse
    [SerializeField] private float waypointTolerance = 1.2f;

    [Header("Perception - Vision Cone")]
    [SerializeField] private float viewRadius = 12.0f;
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90.0f;
    [SerializeField] private int rayCount = 30; // Densidad de rayos en el abanico visual
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolWaypoints;
    private int currentWaypointIndex = 0;

    [Header("Investigation Settings")]
    [SerializeField] private float searchDuration = 4.0f;
    private float searchTimer = 0.0f;
    private Vector3 lastKnownPosition;

    [Header("Pathfinding References")]
    [SerializeField] private Pathfinding pathfinding;
    private List<Vector3> currentPath;
    private int pathIndex = 0;
    private float pathRecalculateTimer = 0.0f;
    private const float PathUpdateInterval = 0.35f;

    private Transform playerTransform;
    private PlayerController playerController;

    private void Start()
    {
        if (pathfinding == null)
        {
            pathfinding = FindFirstObjectByType<Pathfinding>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }

        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            RequestNewPath(patrolWaypoints[currentWaypointIndex].position);
        }
    }

    private void Update()
    {
        SenseEnvironment();

        switch (currentState)
        {
            case FSMState.Patrol:
                UpdatePatrolState();
                break;
            case FSMState.Chase:
                UpdateChaseState();
                break;
            case FSMState.Investigate:
                UpdateInvestigateState();
                break;
        }

        FollowCurrentPath();
    }

    private void SenseEnvironment()
    {
        if (playerTransform == null) return;

        bool canSeePlayer = EvaluateVision();
        bool canHearPlayer = EvaluateHearing();

        if (canSeePlayer)
        {
            lastKnownPosition = playerTransform.position;
            ChangeState(FSMState.Chase);
        }
        else if (canHearPlayer && currentState != FSMState.Chase)
        {
            lastKnownPosition = playerTransform.position;
            ChangeState(FSMState.Investigate);
        }
    }

    private bool EvaluateVision()
    {
        Vector3 eyePos = transform.position + Vector3.up * 0.5f;
        Vector3 targetPos = playerTransform.position + Vector3.up * 0.5f;
        Vector3 dirToTarget = (targetPos - eyePos).normalized;
        float dstToTarget = Vector3.Distance(eyePos, targetPos);

        if (dstToTarget <= viewRadius)
        {
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
            {
                // Comprobar si hay un obstáculo intermedio entre los ojos del guardia y el jugador
                if (!Physics.Raycast(eyePos, dirToTarget, dstToTarget, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool EvaluateHearing()
    {
        if (playerController == null) return false;

        float distance = GetFlatDistance(transform.position, playerTransform.position);
        return distance <= playerController.currentNoiseRadius;
    }

    private void UpdatePatrolState()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

        Vector3 targetWaypoint = patrolWaypoints[currentWaypointIndex].position;
        bool reachedByDistance = GetFlatDistance(transform.position, targetWaypoint) <= waypointTolerance;
        bool finishedPath = currentPath != null && pathIndex >= currentPath.Count;

        if (reachedByDistance || finishedPath)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            RequestNewPath(patrolWaypoints[currentWaypointIndex].position);
        }
    }

    private void UpdateChaseState()
    {
        pathRecalculateTimer += Time.deltaTime;
        if (pathRecalculateTimer >= PathUpdateInterval)
        {
            pathRecalculateTimer = 0f;
            RequestNewPath(playerTransform.position);
        }

        if (!EvaluateVision())
        {
            ChangeState(FSMState.Investigate);
        }
    }

    private void UpdateInvestigateState()
    {
        float dstToLastKnown = GetFlatDistance(transform.position, lastKnownPosition);
        bool finishedPath = currentPath != null && pathIndex >= currentPath.Count;

        if (dstToLastKnown <= waypointTolerance || finishedPath)
        {
            searchTimer += Time.deltaTime;
            transform.Rotate(Vector3.up * (75f * Time.deltaTime));

            if (searchTimer >= searchDuration)
            {
                searchTimer = 0f;
                ChangeState(FSMState.Patrol);
                RequestNewPath(patrolWaypoints[currentWaypointIndex].position);
            }
        }
    }

    private void ChangeState(FSMState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        pathRecalculateTimer = 0f;

        if (currentState == FSMState.Investigate)
        {
            searchTimer = 0f;
            RequestNewPath(lastKnownPosition);
        }
    }

    private void RequestNewPath(Vector3 targetWorldPosition)
    {
        if (pathfinding == null) return;

        List<Vector3> newPath = pathfinding.FindPath(transform.position, targetWorldPosition);
        if (newPath != null && newPath.Count > 0)
        {
            currentPath = newPath;
            pathIndex = 0;
        }
        else
        {
            currentPath = new List<Vector3> { targetWorldPosition };
            pathIndex = 0;
        }
    }

    private void FollowCurrentPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count) return;

        float currentSpeed = (currentState == FSMState.Chase) ? chaseSpeed : patrolSpeed;
        Vector3 targetPoint = currentPath[pathIndex];
        targetPoint.y = transform.position.y;

        Vector3 moveDir = (targetPoint - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, currentSpeed * Time.deltaTime);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (GetFlatDistance(transform.position, targetPoint) < 0.35f)
        {
            pathIndex++;
        }
    }

    private float GetFlatDistance(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    private void OnDrawGizmos()
    {
        Vector3 eyePos = transform.position + Vector3.up * 0.5f;

        // 1. DIBUJAR EL ABANICO DE RAYOS CÓNICO (FOV Fan)
        float startAngle = -viewAngle / 2f;
        float angleStep = viewAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 rayDir = DirFromAngle(currentAngle, false);

            // Capas combinadas: obstáculos físicos y el propio jugador
            int combinedMask = obstacleMask;
            if (targetMask.value != 0) combinedMask |= targetMask.value;

            RaycastHit hit;
            if (Physics.Raycast(eyePos, rayDir, out hit, viewRadius, combinedMask))
            {
                // Si el rayo impacta contra el jugador, se vuelve rojo
                if (hit.collider.CompareTag("Player"))
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    // Si impacta contra un muro o caja, se corta en el punto de contacto en amarillo
                    Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.45f);
                }
                Gizmos.DrawLine(eyePos, hit.point);
                Gizmos.DrawSphere(hit.point, 0.05f);
            }
            else
            {
                // Rayo libre que no colisiona con nada en verde tenue
                Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);
                Gizmos.DrawLine(eyePos, eyePos + rayDir * viewRadius);
            }
        }

        // 2. DIBUJAR LA RUTA DE NODOS A* CALCULADA
        if (currentPath != null)
        {
            Gizmos.color = (currentState == FSMState.Chase) ? Color.red : Color.cyan;
            for (int i = pathIndex; i < currentPath.Count; i++)
            {
                Gizmos.DrawSphere(currentPath[i], 0.15f);
                if (i == pathIndex)
                    Gizmos.DrawLine(transform.position, currentPath[i]);
                else
                    Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
            }
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
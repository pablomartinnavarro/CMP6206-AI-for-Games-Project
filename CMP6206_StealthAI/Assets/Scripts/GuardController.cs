using UnityEngine;
using System.Collections.Generic;

public enum GuardState
{
    Patrol,
    Chase
}

public class GuardController : MonoBehaviour
{
    [Header("Configuración de Estados")]
    public GuardState currentState = GuardState.Patrol;

    [Header("Ruta de Patrulla")]
    public Transform[] waypoints;
    public float patrolSpeed = 2.5f;

    [Header("Parámetros de Persecución")]
    public float chaseSpeed = 4.0f;
    public Transform playerTarget;
    public float sightMemoryDuration = 1.0f; // Tiempo de gracia antes de rendirse (1 segundo)

    Pathfinding pathfinding;
    GuardVisionSensor visionSensor;

    List<Vector3> currentPath;
    int pathIndex = 0;
    int currentWaypointIndex = 0;
    float pathUpdateTimer = 0f;
    float lostSightTimer = 0f; // Contador para evitar el parpadeo de visión

    void Start()
    {
        pathfinding = FindFirstObjectByType<Pathfinding>();
        visionSensor = GetComponent<GuardVisionSensor>();

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        RequestPathToCurrentWaypoint();
    }

    void Update()
    {
        EvaluateStateTransitions();

        switch (currentState)
        {
            case GuardState.Patrol:
                HandlePatrol();
                break;

            case GuardState.Chase:
                HandleChase();
                break;
        }
    }

    void EvaluateStateTransitions()
    {
        // 1. Si el sensor tiene línea de visión directa
        if (visionSensor != null && visionSensor.canSeePlayer)
        {
            lostSightTimer = 0f; // Reiniciar contador de pérdida

            if (currentState != GuardState.Chase)
            {
                currentState = GuardState.Chase;
                pathUpdateTimer = 0.25f; // Forzar cálculo de ruta INMEDIATO en el primer frame
            }
        }
        else
        {
            // 2. Si te pierde de vista, activa el tiempo de gracia antes de rendirse
            if (currentState == GuardState.Chase)
            {
                lostSightTimer += Time.deltaTime;
                if (lostSightTimer >= sightMemoryDuration)
                {
                    currentState = GuardState.Patrol;
                    RequestPathToCurrentWaypoint();
                }
            }
        }
    }

    void HandlePatrol()
    {
        if (currentPath == null || currentPath.Count == 0) return;
        FollowPath(patrolSpeed, onPathComplete: () =>
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            RequestPathToCurrentWaypoint();
        });
    }

    void HandleChase()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // A. Si está a menos de 1.5 metros, ir directo hacia él sin depender de casillas
        if (distanceToPlayer < 1.5f)
        {
            MoveDirectlyTowards(playerTarget.position, chaseSpeed);
            return;
        }

        // B. Recalcular la ruta por cuadrícula cada 0.25 segundos
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= 0.25f)
        {
            pathUpdateTimer = 0f;
            currentPath = pathfinding.FindPath(transform.position, playerTarget.position);
            pathIndex = 0;
        }

        // C. Si la cuadrícula devolvió camino, seguirlo; si no (ej. jugador pegado a un muro), ir directo
        if (currentPath != null && currentPath.Count > 0)
        {
            FollowPath(chaseSpeed, onPathComplete: null);
        }
        else
        {
            MoveDirectlyTowards(playerTarget.position, chaseSpeed);
        }
    }

    void RequestPathToCurrentWaypoint()
    {
        if (waypoints.Length == 0) return;
        Vector3 targetPos = waypoints[currentWaypointIndex].position;
        currentPath = pathfinding.FindPath(transform.position, targetPos);
        pathIndex = 0;
    }

    void FollowPath(float currentSpeed, System.Action onPathComplete)
    {
        if (currentPath == null || pathIndex >= currentPath.Count) return;

        Vector3 targetPoint = currentPath[pathIndex];
        targetPoint.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, targetPoint, currentSpeed * Time.deltaTime);

        Vector3 direction = (targetPoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }

        if (Vector3.Distance(transform.position, targetPoint) < 0.2f)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                onPathComplete?.Invoke();
            }
        }
    }

    // Movimiento vectorial directo hacia el jugador
    void MoveDirectlyTowards(Vector3 targetPos, float currentSpeed)
    {
        targetPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
    }
}
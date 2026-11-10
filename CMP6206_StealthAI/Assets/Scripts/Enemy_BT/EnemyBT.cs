using System.Collections.Generic;
using UnityEngine;

public class EnemyBT : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 4.2f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float waypointTolerance = 1.0f;

    [Header("Vision Settings")]
    [SerializeField] private float viewRadius = 12.0f;
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 90.0f;
    [SerializeField] private int rayCount = 30;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask targetMask;

    [Header("Patrol Circuit")]
    [SerializeField] private Transform[] patrolWaypoints;

    [Header("Pathfinding Reference")]
    [SerializeField] private Pathfinding pathfinding;

    // Nodo raíz del árbol de comportamiento
    private BTNode rootNode;

    private void Start()
    {
        if (pathfinding == null)
        {
            pathfinding = FindFirstObjectByType<Pathfinding>();
        }

        // Construcción jerárquica del árbol en memoria al iniciar
        rootNode = SetupTree();
    }

    private void Update()
    {
        // En cada frame ejecutamos un ciclo (Tick) del árbol desde la raíz
        if (rootNode != null)
        {
            rootNode.Evaluate();
        }
    }

    /*
       ESTRUCTURA DEL ÁRBOL CONSTRUIDA:
                   [ Selector (Root) ]
                      /          \
                     /            \
       [ Sequence (Chase) ]    [ TaskPatrol ]
          /            \
    [ CheckVision ]  [ TaskChase ]
    */
    private BTNode SetupTree()
    {
        // Rama 1: Persecución (Secuencia AND)
        Sequence chaseSequence = new Sequence(new List<BTNode>
        {
            new CheckPlayerInVision(transform, obstacleMask, viewRadius, viewAngle),
            new TaskChase(transform, pathfinding, chaseSpeed, rotationSpeed)
        });

        // Rama 2: Patrulla (Acción individual)
        TaskPatrol patrolTask = new TaskPatrol(transform, pathfinding, patrolWaypoints, patrolSpeed, rotationSpeed, waypointTolerance);

        // Raíz: Selector de Prioridad (OR)
        Selector root = new Selector(new List<BTNode>
        {
            chaseSequence,
            patrolTask
        });

        return root;
    }

    // Dibujo del cono de visión mediante abanico de rayos Gizmo en la vista Scene
    private void OnDrawGizmos()
    {
        Vector3 eyePos = transform.position + Vector3.up * 0.5f;
        float startAngle = -viewAngle / 2f;
        float angleStep = viewAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 rayDir = DirFromAngle(currentAngle, false);

            int combinedMask = obstacleMask;
            if (targetMask.value != 0) combinedMask |= targetMask.value;

            RaycastHit hit;
            if (Physics.Raycast(eyePos, rayDir, out hit, viewRadius, combinedMask))
            {
                Gizmos.color = hit.collider.CompareTag("Player") ? Color.magenta : new Color(0.2f, 0.6f, 1f, 0.45f);
                Gizmos.DrawLine(eyePos, hit.point);
                Gizmos.DrawSphere(hit.point, 0.05f);
            }
            else
            {
                Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.2f);
                Gizmos.DrawLine(eyePos, eyePos + rayDir * viewRadius);
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
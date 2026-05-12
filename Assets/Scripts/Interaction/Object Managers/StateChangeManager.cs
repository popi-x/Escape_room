using UnityEngine;

public class StateChangeManager : MonoBehaviour
{
    public enum PlaneNormalAxis { Forward, Up, Right }

    [SerializeField] private GameObject pastMesh;
    [SerializeField] private GameObject futureMesh;
    [SerializeField] private Transform wallTransform;

    [Tooltip("Which local axis of the wall points through the plane (perpendicular to its surface).")]
    [SerializeField] private PlaneNormalAxis normalAxis = PlaneNormalAxis.Up;

    [Tooltip("Small dead zone to prevent flickering right at the boundary.")]
    [SerializeField] private float threshold = 0.01f;

    private bool isFuture = false;

    private Vector3 PlaneNormal => normalAxis switch
    {
        PlaneNormalAxis.Forward => wallTransform.forward,
        PlaneNormalAxis.Right   => wallTransform.right,
        _                       => wallTransform.up,
    };

    private void Start()
    {
        ResolveMissingReferences();

        if (wallTransform == null) return;
        float dot = Vector3.Dot(transform.position - wallTransform.position, PlaneNormal);
        SetState(dot > 0f);
    }

    private void Update()
    {
        if (wallTransform == null) return;

        // dot > threshold  → future side  (in front of / above / to the right of the wall)
        // dot < -threshold → past side
        float dot = Vector3.Dot(transform.position - wallTransform.position, PlaneNormal);

        if (!isFuture && dot > threshold)
            SetState(true);
        else if (isFuture && dot < -threshold)
            SetState(false);
    }

    private void SetState(bool future)
    {
        isFuture = future;

        if (pastMesh != null)
            pastMesh.SetActive(!isFuture);

        if (futureMesh != null)
            futureMesh.SetActive(isFuture);
    }

    private void ResolveMissingReferences()
    {
        if (pastMesh == null || futureMesh != null) return;

        Transform pastTransform = pastMesh.transform;

        foreach (Transform child in transform)
        {
            if (child == pastTransform) continue;
            if (child.IsChildOf(pastTransform)) continue;

            futureMesh = child.gameObject;
            return;
        }

        Debug.LogWarning($"[StateChangeManager] {name} has no futureMesh assigned and no alternate direct child to use.", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (wallTransform == null) return;

        Vector3 normal = PlaneNormal;
        Vector3 pos = wallTransform.position;

        // // Draw the plane normal (green = future side)
        // Gizmos.color = Color.green;
        // Gizmos.DrawRay(pos, normal * 0.5f);
        // Gizmos.DrawSphere(pos + normal * 0.5f, 0.04f);

        // // Draw the past-side direction (red)
        // Gizmos.color = Color.red;
        // Gizmos.DrawRay(pos, -normal * 0.5f);
    }
}

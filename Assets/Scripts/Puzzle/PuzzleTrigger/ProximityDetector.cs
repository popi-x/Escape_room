using UnityEngine;
using System;

// Place on a child GameObject with a Sphere Collider (IsTrigger = true).
// Forwards trigger events to whoever subscribes, keeping collision logic off the parent.
public class ProximityDetector : MonoBehaviour
{
    public event Action<Collider> OnEnter;
    public event Action<Collider> OnExit;

    private void OnTriggerEnter(Collider other) => OnEnter?.Invoke(other);
    private void OnTriggerExit(Collider other)  => OnExit?.Invoke(other);
}

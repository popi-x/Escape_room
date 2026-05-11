using UnityEngine;

public class PuzzleMarker : MonoBehaviour
{
    // markerState {undiscovered, locked, unlocked}
    [SerializeField] private GameObject locked;
    [SerializeField] private GameObject unlocked;
    [SerializeField] private Transform markerCenter;

    public enum puzzleState {undiscovered, locked, unlocked};
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

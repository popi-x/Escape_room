using System.Collections;
using UnityEngine;
using EscapeRoom.UI;

// Attach to the KeyslotTrigger plane (the GameObject with the trigger Box Collider).
// Assign LidHinge in the Inspector — LidHinge is the empty parent of LeatherTrunk_Lid.
public class ChestController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lidHinge;
    // [SerializeField] private PuzzleMarker puzzleMarker;
    private PuzzleMarker puzzleMarker;

    [Header("Lid Settings")]
    [SerializeField] private Vector3 openRotation = new Vector3(-90f, 0f, 0f);
    [SerializeField] private float openDuration = 1.5f;
    [SerializeField] private PuzzleMarkerState puzzleStateOnInspect = PuzzleMarkerState.Unlocked;

    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    // [SerializeField] private AudioClip confirmSound;
    // [SerializeField] private AudioClip openingSound;

    [Header("UI")]
    [SerializeField] private GameObject successUI;

    private bool isOpen = false;

    private void Awake()
    {
        puzzleMarker = GetComponent<PuzzleMarker>();
        successUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen || !other.CompareTag("Key"))
            return;

        isOpen = true;

        Debug.Log("Key hit the chest!");
        Destroy(other.gameObject);

        // Freeze key physics so it stays locked in the slot
        if (other.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = true;

        // audioSource.PlayOneShot(confirmSound);
        StartCoroutine(OpenLid());
    }

    private IEnumerator OpenLid()
    {
        // yield return new WaitForSeconds(confirmSound.length);

        Quaternion startRot = lidHinge.localRotation;
        Quaternion endRot = Quaternion.Euler(openRotation);
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            lidHinge.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        
        if (puzzleMarker != null)
            puzzleMarker.SetState(puzzleStateOnInspect);

        lidHinge.localRotation = endRot;
        AudioSource.PlayClipAtPoint(successSound, transform.position);
        successUI.SetActive(true);
    }
}

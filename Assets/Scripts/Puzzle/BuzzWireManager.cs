using UnityEngine;
using TMPro;
 // add this

public class BuzzWireManager : MonoBehaviour
{
    public TextMeshPro statusText;
    public Transform keyStartPosition;
    public GameObject keyPrefab;
    public Transform keySpawnParent;
    public Collider winArea;

    private GameObject currentKey;
    private bool gameOver = false;

    void Start()
    {
        Debug.Log("Starting Buzz Wire Game");
        SpawnKey();
    }

    public void OnBuzz()
    {
        if (gameOver) return;

        ShowStatus("BUZZ! Try again!", Color.red);

        if (currentKey != null)
        {
            // Disabling XRGrabInteractable forces XRI to drop it cleanly
            var interactable = currentKey.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (interactable != null) interactable.enabled = false;

            Destroy(currentKey);
            currentKey = null;
        }

        Invoke(nameof(SpawnKey), 1f);
    }

    public void OnWin()
    {
        if (gameOver) return;
        gameOver = true;
        ShowStatus("You win! You have obtained the key.", Color.green);
    }

    void SpawnKey()
    {
        currentKey = Instantiate(keyPrefab, keyStartPosition.position, keyStartPosition.rotation, keySpawnParent);

        BuzzWireKey keyScript = currentKey.GetComponent<BuzzWireKey>();
        if (keyScript == null)
            Debug.LogError("BuzzWireKey script missing on spawned key! Check your prefab.");
        else
        {
            keyScript.gameManager = this;
            keyScript.winArea = winArea;
        }

        ShowStatus("", Color.white);

    }

    void ShowStatus(string msg, Color color)
    {
        if (statusText == null) return;
        statusText.text = msg;
        statusText.color = color;
    }
}

// using UnityEngine;
// using TMPro;

// public class BuzzWireManager : MonoBehaviour
// {
//     public TextMeshPro statusText;
//     public TextMeshPro strikeText;
//     public Transform keyStartPosition; // where key resets to on buzz
//     public BuzzWireKey key;

//     [Header("Settings")]
//     public int maxStrikes = 3;

//     private int strikes = 0;
//     private bool gameOver = false;

//     void Start()
//     {
//         UpdateUI();
//     }

//     public void OnBuzz()
//     {
//         if (gameOver) return;

//         strikes++;
//         Debug.Log("BUZZ! Strikes: " + strikes);

//         // Haptic feedback on both controllers
//         // TriggerHaptics();

//         if (strikes >= maxStrikes)
//         {
//             GameOver();
//         }
//         else
//         {
//             statusText.text = "BUZZ!";
//             Invoke(nameof(ClearStatus), 1f);
//         }

//         UpdateUI();
//     }

//     public void OnWin()
//     {
//         if (gameOver) return;
//         gameOver = true;
//         statusText.text = "YOU WIN!";
//         statusText.color = Color.green;
//         Debug.Log("Player won!");
//     }

//     void GameOver()
//     {
//         gameOver = true;
//         statusText.text = "GAME OVER";
//         statusText.color = Color.red;

//         // Reset key to start
//         key.transform.position = keyStartPosition.position;
//         key.transform.rotation = keyStartPosition.rotation;
//     }

//     // void TriggerHaptics()
//     // {
//     //     OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
//     //     OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
//     //     Invoke(nameof(StopHaptics), 0.3f);
//     // }

//     // void StopHaptics()
//     // {
//     //     OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
//     //     OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
//     // }

//     void ClearStatus() => statusText.text = "";

//     void UpdateUI()
//     {
//         strikeText.text = $"Strikes: {strikes}/{maxStrikes}";
//     }
// }

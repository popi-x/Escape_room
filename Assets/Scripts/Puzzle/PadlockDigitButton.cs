using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class PadlockDigitButton : MonoBehaviour
{
    public PadlockManager padlock;
    public int digit;

    // Visual feedback
    private Renderer rend;
    private Color normalColor;
    public Color pressedColor = Color.yellow;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Start()
    {
        rend = GetComponent<Renderer>();
        normalColor = rend.material.color;
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable != null) interactable.selectEntered.AddListener(_ => OnSelect());
        if (interactable != null) interactable.activated.AddListener(_ => OnSelect());
        Debug.Log($"[PadlockDigitButton] Start {name}, digit={digit}, unityLayer={LayerMask.LayerToName(gameObject.layer)}({gameObject.layer}), interactionLayerBits={(interactable != null ? interactable.interactionLayers.value.ToString() : "NO_INTERACTABLE")}, padlock={(padlock != null ? padlock.name : "NULL")}, interactable={(interactable != null ? "YES" : "NO")}", this);
    }

    public void OnHover()   
    {
        Debug.Log($"[PadlockDigitButton] Hover {name}, digit={digit}, mode={padlock.inputMode}", this);
        if (padlock.inputMode != PadlockManager.InputMode.Hover) return;
        Press();
    }

    public void OnSelect()
    {
        Debug.Log($"[PadlockDigitButton] Select {name}, digit={digit}, mode={padlock.inputMode}", this);
        if (padlock.inputMode != PadlockManager.InputMode.Trigger) return;
        Press();
    }

    void Press()
    {
        Debug.Log($"[PadlockDigitButton] Press {name}, digit={digit}", this);
        // Log EVERYTHING that enters — remove filter temporarily
        // Debug.Log($"[DigitButton] Hovered by:  interactors");
        // if (!IsController()) return;
        padlock.PlayButtonPressSound();
        padlock.OnDigitPressed(digit);
        rend.material.color = pressedColor;
        Invoke(nameof(ResetColor), 0.2f);
        Debug.Log($"done hover ${digit}");
    }

    void ResetColor() => rend.material.color = normalColor;

    bool IsController(Collider other)
    {
        return other.CompareTag("GameController");
    }
}

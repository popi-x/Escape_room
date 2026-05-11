using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ActionButton : MonoBehaviour
{
    public PadlockManager padlock;
    public bool isBackspace; // true = Backspace, false = Clear

    private Renderer rend;
    private Color normalColor;
    public Color pressedColor = Color.cyan;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Start()
    {
        rend = GetComponent<Renderer>();
        normalColor = rend.material.color;
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable != null) interactable.selectEntered.AddListener(_ => OnSelect());
        if (interactable != null) interactable.activated.AddListener(_ => OnSelect());
        Debug.Log($"[ActionButton] Start {name}, isBackspace={isBackspace}, interactable={(interactable != null ? "YES" : "NO")}, padlock={(padlock != null ? padlock.name : "NULL")}", this);
    }

    public void OnHover()
    {
        Debug.Log($"[ActionButton] Hover {name}, isBackspace={isBackspace}, mode={padlock.inputMode}", this);
        if (padlock.inputMode != PadlockManager.InputMode.Hover) return;
        Press();
    }

    public void OnSelect()
    {
        Debug.Log($"[ActionButton] Select {name}, isBackspace={isBackspace}, mode={padlock.inputMode}", this);
        if (padlock.inputMode != PadlockManager.InputMode.Trigger) return;
        Press();
    }

    void Press()
    {
        Debug.Log($"[ActionButton] Press {name}, isBackspace={isBackspace}", this);
        padlock.PlayButtonPressSound();
        if (isBackspace) padlock.OnBackspacePressed();
        else padlock.OnClearPressed();

        rend.material.color = pressedColor;
        Invoke(nameof(ResetColor), 0.2f);
    }

    void ResetColor() => rend.material.color = normalColor;
}

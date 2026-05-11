using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit;

public class MyInteractable : MonoBehaviour {
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    public int id;

    void Awake() {

    }

    public void OnSelect() {
        Debug.Log("selected");
    }

    public void OnHover() {
        Debug.Log("hovered");
    }
}
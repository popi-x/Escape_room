using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    public InputActionProperty property;

    // Update is called once per frame
    void Update()
    {
        float value = property.action.ReadValue<float>();
        Debug.Log($"Input value: {value}");
    }
}

using System;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    private InputHandler inputHandler;

    private void Awake()
    {
        inputHandler = InputHandler.Instance;
        if (inputHandler == null)
        {
            GameObject inputHandleInstance = new GameObject();
            inputHandleInstance.AddComponent<InputHandler>();
            inputHandler = InputHandler.Instance;
        }
    }

    private void OnEnable()
    {
        inputHandler.OnButtonPress += Foo;
    }

    private void OnDisable()
    {
        inputHandler.OnButtonPress -= Foo;
    }

    private void Foo()
    {
        Debug.Log("Bar");
    }
}

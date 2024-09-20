using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class InputHandler : SingletonMonoBehavior<InputHandler>
{
    public UnityAction OnButtonPress;

    private List<ActionBasedController> allControllers = new List<ActionBasedController>();
    private List<bool> allTriggers = new List<bool>();


    private void OnEnable()
    {
        allControllers = FindObjectsOfType<ActionBasedController>().ToList();
        if(allControllers.Count <= 0) Debug.LogError("No Action Based Controllers present in scene!");
    }

    private void Update()
    {
        allTriggers.Clear();
        foreach (ActionBasedController controller in allControllers)
        {
            allTriggers.Add(controller.activateAction.action.ReadValue<float>() >= 0.5f);
            allTriggers.Add(controller.selectAction.action.ReadValue<float>() >= 0.5f);
        }
        if (allTriggers.Any(trigger => trigger))
        {
            OnButtonPress?.Invoke();
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InputHandler : SingletonMonoBehavior<InputHandler>
{
    public UnityAction OnTriggerPull;
    public UnityAction OnUndoButtonPress;

    [SerializeField] private TextMeshProUGUI tv;

    [SerializeField] private ActionBasedController rightController;

    private List<bool> allTriggers = new List<bool>();
    private bool canInput = true;
    private bool canUndo = true;
    private bool isInPulledState = false;

    private void Update()
    {
        float selectAction = rightController.selectAction.action.ReadValue<float>();
        float activateAction = rightController.activateAction.action.ReadValue<float>();

        if (isInPulledState && selectAction < 0.2f && activateAction < 0.2f) isInPulledState = false;
        if (isInPulledState) return;
        allTriggers.Add(activateAction >= 0.9f);
        allTriggers.Add(selectAction >= 0.9f);

        if (allTriggers.Any(trigger => trigger))
        {
            if (!canInput) return;
            OnTriggerPull?.Invoke();
            canInput = false;
            isInPulledState = true;
            StartCoroutine(InputCooldown());
        }

    }

    private IEnumerator InputCooldown()
    {
        yield return new WaitForSeconds(0.05f);
        canInput = true;
    }

    private IEnumerator UndoCooldown()
    {
        yield return new WaitForSeconds(0.05f);
        canUndo = true;
    }

    public void SimulateInput()
    {
        if (!canInput) return;
        OnTriggerPull?.Invoke();
        canInput = false;
        StartCoroutine(InputCooldown());
    }

    public void SimulateUndo()
    {
        if (!canUndo) return;
        OnUndoButtonPress?.Invoke();
        canUndo = false;
        StartCoroutine(UndoCooldown());
    }
}

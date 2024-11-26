using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class InputHandler : SingletonMonoBehavior<InputHandler>
{
    public UnityAction OnTriggerPull;
    public UnityAction OnCancelTrial;

    [SerializeField] private ActionBasedController rightController;
    [SerializeField] private ActionBasedController leftController;
    [SerializeField] private float undoTimeWindow;

    private bool canInput = true;
    private bool canUndo = true;
    private bool rightIsInPulledState;
    private bool leftIsInPulledState;
    private float undoInputFrameCount;

    private void Update()
    {
        float rightSelectAction = rightController.selectAction.action.ReadValue<float>();
        float rightActivateAction = rightController.activateAction.action.ReadValue<float>();

        if (rightIsInPulledState && rightSelectAction < 0.2f && rightActivateAction < 0.2f)
        {
            rightIsInPulledState = false;
        }
        if (!rightIsInPulledState)
        {
            if (rightActivateAction >= 0.9f || rightSelectAction >= 0.9f)
            {
                if (!canInput) return;
                OnTriggerPull?.Invoke();
                canInput = false;
                rightIsInPulledState = true;
                StartCoroutine(InputCooldown());
            }
        }

        float leftSelectAction = leftController.selectAction.action.ReadValue<float>();
        float leftActivateAction = leftController.activateAction.action.ReadValue<float>();

        if (leftIsInPulledState && leftSelectAction < 0.2f && leftActivateAction < 0.2f)
        {
            leftIsInPulledState = false;
        }

        if(!leftIsInPulledState)
        {
            if (leftSelectAction > 0.9f && leftActivateAction > 0.9f)
            {
                undoInputFrameCount += 1f;
            }
            else undoInputFrameCount = 0f;

            if (undoInputFrameCount > undoTimeWindow/Time.deltaTime)
            {
                if (!canUndo) return;
                OnCancelTrial?.Invoke();
                canUndo = false;
                leftIsInPulledState = true;
                undoInputFrameCount = 0;
                StartCoroutine(UndoCooldown());
            }
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
        OnCancelTrial?.Invoke();
        canUndo = false;
        StartCoroutine(UndoCooldown());
    }
}

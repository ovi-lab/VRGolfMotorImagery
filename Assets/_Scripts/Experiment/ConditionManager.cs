using System;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    [SerializeField] private float totalTrialCount;
    [Range(0, 100), SerializeField] private float percentageError;

    private void Start()
    {

    }
}



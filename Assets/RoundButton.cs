using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoundButton : MonoBehaviour
{
    [SerializeField] private float threshold = 0.1f;
    [SerializeField] private float deadZone = 0.025f;

    private bool isPressed;
    private Vector3 startPositinon;
    private ConfigurableJoint joint;

    public UnityEvent onPressed, onReleased;
    void Start()
    {
        startPositinon = transform.localPosition;
        joint = GetComponent<ConfigurableJoint>();
    }

    void Update()
    {
        if (!isPressed && GetValue() + threshold >= 1)
            Pressed();
        if (isPressed && GetValue() - threshold <= 0)
            Relesed();
    }

    private float GetValue()
    {
        var value = Vector3.Distance(startPositinon, transform.localPosition) / joint.linearLimit.limit;
        if (Math.Abs(value) < deadZone)
            value = 0;
        return Mathf.Clamp(value, -1f, 1f);
    }

    private void Pressed() {
        isPressed = true;
        onPressed.Invoke();
        Debug.Log("pressed");
    }
    private void Relesed() {
        isPressed = false;
        onReleased.Invoke();
        Debug.Log("released");

    }
}

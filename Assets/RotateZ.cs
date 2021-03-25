using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RotateZ : MonoBehaviour
{
    // Start is called before the first frame update
    public bool right = true;
    public HandPresence handPresence;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (right && handPresence.isGrippingRight()) {
            transform.Rotate(new Vector3(0.0f, 0.0f, 200.0f * Time.deltaTime));
        }
        if (!right && handPresence.isGrippingLeft()) {
            transform.Rotate(new Vector3(0.0f, 0.0f, 200.0f * Time.deltaTime));
        }
    }
}

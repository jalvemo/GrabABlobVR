using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class HandPresence : MonoBehaviour
{
    // Start is called before the first frame update
    InputDevice left; 
    bool leftGrip = false;
    InputDevice right; 
    bool rightGrip = false;
    bool restart = false;

    void Start()
    {
        List<InputDevice> devices = new List<InputDevice>();
        //InputDeviceCharacteristics rightCharacteristics = InputDeviceCharacteristics.Right |  InputDeviceCharacteristics.Controller;
        //InputDeviceCharacteristics leftCharacteristics = InputDeviceCharacteristics.Left |  InputDeviceCharacteristics.Controller;
       

        InputDevices.GetDevices(devices);


         //left = devices.Find(device => device.characteristics == InputDeviceCharacteristics.Left);
        left = devices.Find(device => {
            return device.characteristics.HasFlag(InputDeviceCharacteristics.Left);
        });
        right = devices.Find(device => device.characteristics.HasFlag(InputDeviceCharacteristics.Right));

        Debug.Log("X left:" + left.name);
        Debug.Log("X right:" + right.name);


        foreach (var item in devices)
        {
            Debug.Log(item.name + item.characteristics);
        }
        
    }
    // Update is called once per frame
    void Update()
    {

        left.TryGetFeatureValue(CommonUsages.gripButton, out bool leftGripNew);
        right.TryGetFeatureValue(CommonUsages.gripButton, out bool rightGripNew);
        
        left.TryGetFeatureValue(CommonUsages.secondaryButton, out bool leftSecondary);
        right.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rightSecondary);



        if (leftSecondary || rightSecondary) {
            Debug.Log("secondary");
        }

        if (leftSecondary && rightSecondary && !restart) {
            restart = true;
            //Debug.Log("restart");
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (restart && (!leftSecondary || !rightSecondary)) {
            restart = false;
        }


        if (leftGrip != leftGripNew) {
            leftGrip = leftGripNew;
            //Debug.Log("left grip:" + leftGrip);
        }
        
        if (rightGrip != rightGripNew) {
            rightGrip = rightGripNew;
            //Debug.Log("right grip:" + rightGrip);
        }
    }
}


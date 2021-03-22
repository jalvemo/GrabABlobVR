using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class SocketSelector : MonoBehaviour
{
    public enum SocketSelectorType {
        FLOAT,
        BOOL,
        INT
    }
    Dictionary<XRSocketInteractor, string> Sockets;

    public SocketSelectorType valueType; 

    public int optionCount;
    public float minValue = 0;
    public float maxValue = 1;
    public float curentValue = 0;

    public GameObject MenuOptionPrefab;
    public GameObject MenuSpharePrefab;

    private float distance = 0.15f;
    // Start is called before the first frame update
    void Start()
    {
        if (valueType == SocketSelectorType.BOOL) {
            optionCount = 2;
            minValue = 0;
            maxValue = 1;
        }

        
        float step = (maxValue - minValue) / (optionCount - 1);
        for (int i = 0; i < optionCount; i++) {        
            var optionValue = minValue + (step * i);
            CreatSocket(i, optionValue);
            
        }
    }

    void CreatSocket(int i, float optionValue) {
        float offset = -1 * distance * (optionCount - 1) / 2;
        GameObject option = Instantiate(MenuOptionPrefab);
        var socket = option.GetComponent<XRSocketInteractor>();
        socket.onSelectEntered.AddListener((_) => curentValue = optionValue);
        socket.onSelectEntered.AddListener((_) => Debug.Log("value = " + optionValue));
        //option.name = "socket " + position.ToString();       
        option.transform.SetPositionAndRotation(transform.position + new Vector3(0.0f, 0.0f, i * distance + offset), new Quaternion()); 
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

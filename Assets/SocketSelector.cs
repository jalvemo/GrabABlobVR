using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.Events;

public class SocketSelector : MonoBehaviour
{
    public enum SocketSelectorType { FLOAT, BOOL, INT }

    public string displayName;

    public SocketSelectorType valueType; 

    public int optionCount;
    public float minValue = 0;
    public float maxValue = 1;
    public float curentValue = 0;

    public int getInt() {
        return Mathf.RoundToInt(curentValue);
    }

    public UnityAction onChanged = () => {};

    public GameObject SocketMenuOptionPrefab;
    public GameObject SocketMenuSpharePrefab;
    private TextMeshPro headerTextCompontent;
    
    private float distance = 0.15f;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        float offset = -1 * distance * (optionCount + 2.5f) / 2;
        headerTextCompontent = this.GetComponentInChildren<TextMeshPro>(); 
        headerTextCompontent.text = displayName;
        MoveRelativeToThis(headerTextCompontent.transform, new Vector3(0.0f, 0.0f, distance + offset)); 
        headerTextCompontent.rectTransform.rotation = new Quaternion(); //not sure why we need this. but it fixes the text rotation

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
        GameObject option = Instantiate(SocketMenuOptionPrefab);
        var socket = option.GetComponent<XRSocketInteractor>();
        var textComponent = option.GetComponentInChildren<TextMeshPro>();
        textComponent.text = "" + optionValue;
        socket.onSelectEntered.AddListener((_) => {
            curentValue = optionValue;
            onChanged.Invoke();
        });     
       
        float offset = -1 * distance * (optionCount - 1) / 2;
        MoveRelativeToThis(option.transform, new Vector3(0.0f, 0.0f, i * distance + offset));
        if (optionValue == curentValue) {
            GameObject sphare = Instantiate(SocketMenuSpharePrefab);
            MoveRelativeToThis(sphare.transform, new Vector3(0.0f, 0.0f, i * distance + offset));
        }
    }

    void MoveRelativeToThis(Transform gameObjectTransform, Vector3 relativePosition) {
        // todo consider to set translation parent instead
        gameObjectTransform.SetPositionAndRotation(transform.position + relativePosition, new Quaternion()); 
        this.transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);
        gameObjectTransform.RotateAround(this.transform.position, axis, angle);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

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

    public int GetInt() {
        return Mathf.RoundToInt(curentValue);
    }

    public UnityAction onChanged = () => {};
    public UnityEvent onChangedHigher;
    public UnityEvent onChangedLower;
    

    public GameObject SocketMenuOptionPrefab;
    public GameObject SocketMenuSpharePrefab;
    private TextMeshPro headerTextCompontent;
    
    private float distance = 0.15f;
    // Start is called before the first frame update
    //void Awake()
    void Start() 
    {
        float offset = distance * (optionCount) / 2;
        headerTextCompontent = this.GetComponentInChildren<TextMeshPro>(); 
        headerTextCompontent.text = displayName;
        MoveRelativeToThis(headerTextCompontent.transform, new Vector3(0.0f, 0.0f, distance + offset), headerTextCompontent.transform.rotation); 
        //headerTextCompontent.rectTransform.rotation = new Quaternion(); //not sure why we need this. but it fixes the text rotation

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


    List<GameObject> gameObjects = new List<GameObject>();
    bool active = true;
    public void Expand() {
        if (!active) {
            active = true;
            transform.SetPositionAndRotation(transform.position + new Vector3(0.0f,10.0f,0.0f),  new Quaternion());

            //foreach(GameObject g in gameObjects) {
            //    g.transform.SetPositionAndRotation(g.transform.position + new Vector3(0.0f,10.0f,0.0f),  new Quaternion());
            //}
        }
    }
    public void Collapse() {
        if (active) {
            active = false;
            transform.SetPositionAndRotation(transform.position + new Vector3(0.0f,-10.0f,0.0f),  new Quaternion());
            //foreach(GameObject g in gameObjects) {
            //    g.transform.SetPositionAndRotation(g.transform.position + new Vector3(0.0f,-10.0f,0.0f),  new Quaternion());
            //}
        }
    }
    public void ToggleActive() {
        active = !active;
        
    }
    private void RotateAnnimation() {
        //if (active) {
        //    foreach(GameObject g in gameObjects) {
        //        g.transform.Rotate(new Vector3(0.0f,0.0f,100.0f) * Time.deltaTime);
        //        
        //    }
        //}
    }

    void CreatSocket(int i, float optionValue) {
        GameObject option = Instantiate(SocketMenuOptionPrefab);
        option.transform.SetParent(this.transform);

        gameObjects.Add(option);

        var socket = option.GetComponent<XRSocketInteractor>();
        var textComponent = option.GetComponentInChildren<TextMeshPro>();
        if (valueType == SocketSelectorType.BOOL){
            textComponent.text = optionValue > 0 ? "on" : "off";
        }
        else {
            textComponent.text = "" + optionValue;
        }
        socket.selectEntered.AddListener((_) => {
            
            if (optionValue > curentValue) {

                Debug.Log("higher; " + optionValue);
                curentValue = optionValue;
                onChanged.Invoke();
                onChangedHigher.Invoke();
            } else if (optionValue < curentValue) { 
                Debug.Log("lower; " + optionValue);
                curentValue = optionValue;
                onChanged.Invoke();
                onChangedLower.Invoke();
            }
        });     
       
        float offset = -1 * distance * (optionCount - 1) / 2;
        MoveRelativeToThis(option.transform, new Vector3(0.0f, 0.0f, i * distance + offset));
        if (optionValue == curentValue) {
            GameObject sphare = Instantiate(SocketMenuSpharePrefab);
            sphare.transform.SetParent(this.transform);
            MoveRelativeToThis(sphare.transform, new Vector3(0.0f, 0.0f, i * distance + offset));
            //gameObjects.Add(sphare);

        }
    }

    void MoveRelativeToThis(Transform gameObjectTransform, Vector3 relativePosition) {
        MoveRelativeToThis(gameObjectTransform, relativePosition, new Quaternion()); 
    }

    void MoveRelativeToThis(Transform gameObjectTransform, Vector3 relativePosition, Quaternion rotation) {
        // todo consider to set translation parent instead
        gameObjectTransform.SetPositionAndRotation(transform.position, rotation); 
        gameObjectTransform.localPosition = relativePosition;
        //this.transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);
        //gameObjectTransform.RotateAround(this.transform.position, axis, angle);
    }
    // Update is called once per frame
    void Update()
    {
        RotateAnnimation();
    }
}

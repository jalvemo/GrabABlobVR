using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public Menu.ButtonAction action;
    public Menu menu;
    void OnCollisionEnter(Collision collision) {
        Debug.Log("Menu pessed" + action.ToString());
        menu.ButtonPressed(action);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

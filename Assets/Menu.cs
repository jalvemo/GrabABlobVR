    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
 
    public enum ButtonAction{
        UP,DOWN,LEFT,RIGHT
    }

    int mod(int x, int m) {
        return (x%m + m)%m;
    }
    internal void ButtonPressed(ButtonAction action)
    {
         switch (action)
        {
            case ButtonAction.UP:
            selected = mod(selected - 1 , items.Count);
            break;
            case ButtonAction.DOWN:
            selected = mod(selected + 1 , items.Count);
            break;
            case ButtonAction.LEFT:
            break;

            case ButtonAction.RIGHT:
            break;

            default:
            break;
        }
        RefreshText();
    }
    public void Up()
    {
        selected = mod(selected - 1 , items.Count);
        RefreshText();
    }
    public void Down()
    {
        selected = mod(selected + 1 , items.Count);
        RefreshText();
    }
    private void RefreshText() {
        var textCompontent = this.GetComponentInChildren<TextMeshPro>(); 
        textCompontent.text = "";
        for(int i = 0 ; i < items.Count ; i++) {
            textCompontent.text += "\n" + (i == selected ? "* " : "  ") + items[i];
        }
    }
    List<string> items = new List<string> {
        "Speed",
        "Size",
        "Height",

    };
    private int selected = 0;

    void Start()
    {
        RefreshText();

        //this.GetComponent<XRGrabInteractable>().onSelectEntered.AddListener((_) => {
        //    textCompontent.text = "grab";
        //});
    }

    // Update is called once per frame
    void Update()
    {
        
    }
       
   
}

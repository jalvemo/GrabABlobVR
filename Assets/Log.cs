using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Log : MonoBehaviour
{
    private TextMeshPro _text;
    
    private List<string> logs = new List<string>();
    private string stack;
    void Start()
    {
        _text = this.GetComponentInChildren<TextMeshPro>();
        Application.logMessageReceived += LogRecieved;
    }

    public void LogRecieved(string logString, string stackTrace, LogType type)
    {
       
        stack = stackTrace;
         _text.text = logString + "\n" +  _text.text;

        if (_text.text.Length > 5000)
        {
            _text.text = _text.text.Substring(0, 4000);
        }
    }
}

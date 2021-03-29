using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    private TextMeshPro _text;
    // Start is called before the first frame update
    
    public int Score = 0;
    public int Level = 0;

    public string message = "";
    void Start()
    {
        _text = this.GetComponentInChildren<TextMeshPro>(); 
        UpdateText();
    }

    private void UpdateText() {
        if (_text != null) { // somthing updates text on start() before this start...
            _text.text = 
                "Level: " + Level + "\n" +
                "Score: " + Score + "\n" +
                message; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LevelUp(int level = 1) {
        Level += level;
        message = "level up, fall speed increesed";
        UpdateText();
    }

    public void AddScore(int score) {
        Score += score;
        message = "You got " + score;
        UpdateText();
    }

    public void ResetBoard() {
        Level = 0;
        Score = 0;
        message = "";
        UpdateText();
    }
}

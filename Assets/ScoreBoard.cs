using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;


public class ScoreBoard : MonoBehaviour
{
    //scoring inspo https://puyonexus.com/wiki/Scoring
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
        UpdateText();
    }

    private List<List<Blob>> _combo = new List<List<Blob>>();
    private Dictionary<int, List<List<Blob>>> _comboGroupedBySize;

    public void ApplyScore() {
        var blobCount = _combo.Sum(_ => _.Count);
        var scoreToAdd = blobCount * _combo.Count;
        _combo = new List<List<Blob>>();
        Score += scoreToAdd;
    }

    public void AddScore(List<Blob> blobs) {
        _combo.Add(blobs);

        //_comboGroupedBySize = _combo
        //    .GroupBy(_ => _.Count)
        //    .ToDictionary(_ => _.Key, _ => _.ToList());
        
        // todo factor in level in score
        var blobCount = _combo.Sum(_ => _.Count);
        var scoreToAdd = blobCount * _combo.Count;

        message = "Scoreing " + blobCount;
        if (_combo.Count() > 1) {
             message += " X" + _combo.Count() + " Combo = " + scoreToAdd;
        }
        UpdateText();
    }
    
    public void ResetBoard() {
        Level = 0;
        Score = 0;
        message = "";
        _combo = new List<List<Blob>>();
        UpdateText();
    }
}

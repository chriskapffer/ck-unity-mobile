using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class ScoreLabel : MonoBehaviour {

    public string scorePrefix = "Score ";

    private Text _label;
    private Text Label {
        get {
            if (_label == null) {
                _label = GetComponent<Text>();
            }
            return _label;
        }
    }

	// Use this for initialization
	void Start () {
        
	}
	
    public void SetScore(int score) {
        Label.text = scorePrefix + score;
    }
}

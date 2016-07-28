using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class ScoreChangedEvent : UnityEvent<int> { }

public class ScoreChanger : MonoBehaviour {

    [SerializeField]
    private int _score = 1000;
    public int Score {
        get {
            return _score;
        }
        private set {
            if (_score != value) {
                _score = value;
                onScoreChanged.Invoke(value);
            }
        }
    }

    public ScoreChangedEvent onScoreChanged;

	// Use this for initialization
	void Start () {
        onScoreChanged.Invoke(Score);
	}

    public void Decrese(int amount) {
        Score -= amount;
    }

    public void Increase(int amount) {
        Score += amount;
    }
}

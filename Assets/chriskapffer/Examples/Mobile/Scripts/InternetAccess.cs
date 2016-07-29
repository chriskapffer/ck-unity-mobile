using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class InternetAccessChangedEvent : UnityEvent<bool> { }

public class InternetAccess : MonoBehaviour {

    public InternetAccessChangedEvent onAccessChanged;

    /// <summary>
    /// Frequency of how often to check if connection is still there
    /// </summary>
    public float interval = 1f;

	private bool _hasAccess = true;
	public bool HasAccess {
		get {
            return _hasAccess;
        }
        private set {
            if (_hasAccess != value) {
                _hasAccess = value;
                if (onAccessChanged != null) {
                    onAccessChanged.Invoke(_hasAccess);
                }
            }
        }
	}

	// Use this for initialization
	void Start () {
		StartCoroutine(CheckAcces());
	}
		
	private IEnumerator CheckAcces() {
		while (true) {
			yield return new WaitForSeconds(interval);
            HasAccess = Application.internetReachability != NetworkReachability.NotReachable;
		}
	}
}

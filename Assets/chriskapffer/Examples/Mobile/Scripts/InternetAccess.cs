using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class UnityEventBool : UnityEvent<bool> {

}

public class InternetAccess : MonoBehaviour {

	public float interval = 0.2f;
	public UnityEventBool onAccessChanged;
	private bool hasAccess = true;
	public bool HasAccess {
		get { return hasAccess; }
	}

	// Use this for initialization
	void Start () {
		StartCoroutine(CheckAcces());
	}
		
	private IEnumerator CheckAcces() {
		while (true) {
			yield return new WaitForSeconds(interval);
			bool isReachable = Application.internetReachability != NetworkReachability.NotReachable;
			if (hasAccess != isReachable) {
				hasAccess = isReachable;
				if (onAccessChanged != null) {
					onAccessChanged.Invoke(hasAccess);
				}
			}
		}
	}
}

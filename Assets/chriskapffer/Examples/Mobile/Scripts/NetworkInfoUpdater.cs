using UnityEngine;
using System.Collections;

public class NetworkInfoUpdater : MonoBehaviour {

	public float interval = 5;

	// Use this for initialization
	void Start () {
		StartCoroutine(UpdateNetworkInfo(interval));
	}
	
	private IEnumerator UpdateNetworkInfo(float updateInterval) {
		while (true) {
			yield return new WaitForSeconds(updateInterval);
            ChrisKapffer.Mobile.NetworkInfo.GetCurrentNetworkType(true);
		}
	}
}

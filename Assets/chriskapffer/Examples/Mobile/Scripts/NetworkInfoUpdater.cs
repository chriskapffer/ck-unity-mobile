using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using ChrisKapffer.Mobile;

[System.Serializable]
public class NetworkInfoChangedEvent : UnityEvent<string> { }

/// <summary>
/// This class demonstrates two ways in which you can monitor network changes.
/// One is to register to the event that fires automatically when the network changes.
/// The other is to use a coroutine to query the current network state in regular intervals.
/// </summary>
public class NetworkInfoUpdater : MonoBehaviour {

    public NetworkInfoChangedEvent onNetworkChanged;


    private float activeDetectionQueryInterval = 3;
    private bool activelyDetectChanges = false;
    private bool ignoreCachedValue = false;

    private NetworkInfo.NetworkType currentNetworkType = NetworkInfo.NetworkType.unknown;

    // Use this for initialization
    void Start () {
        if (activelyDetectChanges) {
            StartCoroutine(CheckActivelyForChanges());
        } else {
            NetworkInfo.OnNetworkTypeChanged += NetworkTypeChanged;
        }

    }

    void OnApplicationPause(bool pause) {
        if (!activelyDetectChanges) {
            if (pause) {
                NetworkInfo.OnNetworkTypeChanged -= NetworkTypeChanged;
            } else {
                NetworkInfo.OnNetworkTypeChanged += NetworkTypeChanged;
            }
        }
    }

    void OnApplicationQuit() {
        if (!activelyDetectChanges) {
            NetworkInfo.OnNetworkTypeChanged -= NetworkTypeChanged;
        }
    }

    private IEnumerator CheckActivelyForChanges() {
        while (true) {
            yield return new WaitForSeconds(activeDetectionQueryInterval);
            var newNetworkType = NetworkInfo.GetCurrentNetworkType(ignoreCachedValue);
            if (currentNetworkType != newNetworkType) {
                currentNetworkType = newNetworkType;
                NetworkTypeChanged(newNetworkType);
            }
        }
    }

    private void NetworkTypeChanged(NetworkInfo.NetworkType networkType) {
        onNetworkChanged.Invoke(networkType.ToString());
    }
}

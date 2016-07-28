using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ButtonResultLabel : MonoBehaviour {

    private Text _label;
    private Text Label {
        get {
            if (_label == null) {
                _label = GetComponent<Text>();
            }
            return _label;
        }
    }

    private Coroutine hideRoutine = null;

	// Use this for initialization
	void Start () {
        Label.enabled = false;
	}
	
    public void OnButtonPress(int buttonIndex) {
        Label.text = "You pressed button Nr " + buttonIndex;
        Label.enabled = true;
        if (hideRoutine != null) {
            StopCoroutine(hideRoutine);    
        }
        hideRoutine = StartCoroutine(HideAfter(1));
    }

    private IEnumerator HideAfter(float time) {
        yield return new WaitForSeconds(time);
        Label.enabled = false;
    }
}

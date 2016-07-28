using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ButtonPressedEvent : UnityEvent<int> { }

/// <summary>
/// This is a very basic example on how to use the NativePopup module
/// </summary>
public class GenericNativePopup : MonoBehaviour {

    /// <summary>
    /// Popup title
    /// </summary>
    public string title;

    /// <summary>
    /// Popup content
    /// </summary>
    public string message;

    /// <summary>
    /// Any number of buttons
    /// </summary>
    public string[] buttons;

    public ButtonPressedEvent onButtonPressed;

	// Use this for initialization
	void Start () {
	
	}
	
    public void ShowPopup() {
        ChrisKapffer.Mobile.NativePopup.Show(title, message, (pressedIndex) => {
            Debug.Log("You pressed button number: " + pressedIndex);
            onButtonPressed.Invoke(pressedIndex);
        }, buttons);
    }
}

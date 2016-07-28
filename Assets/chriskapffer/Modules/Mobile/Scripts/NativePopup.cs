using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace ChrisKapffer.Mobile {

    /// <summary>
    /// This class allows you to show a native popup message in your game. It is implemented as a singleton, since there can only be one popup at a time.
    /// You can use it for alert messages, reminders, rate me dialogs and so on. It uses the phone's system UI instead of Unity's (exept for the editor).
    /// There is no Windows Phone implementation.
    /// </summary>
	public class NativePopup {

		#region Public Interface

        /// <summary>
        // TODO: allowed button count Android ?
        /// Shows a popup message with an action delegate and one to three buttons.
        /// </summary>
        /// <param name="title">The title or headline of the popup.</param>
        /// <param name="message">Text content of the popup.</param>
        /// <param name="onClose">Action delegate with the index of the pressed button. If the popup was closed without any button being pressed, the index will be -1.</param>
        /// <param name="buttons">One to three strings, used as button titles.</param>
		public static void Show(string title, string message, Action<int> onClose, params string[] buttons) {
			Instance.ShowImpl(title, message, onClose, buttons);
		}
		
		#endregion
		
		#region Singleton Implementation
		
		private static object _instanceAccessor = new object();
#if !NETFX_CORE
        // hide _instance from IntelliSense
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#endif
		private static NativePopup _instance;
		private static NativePopup Instance {
			get {
				lock(_instanceAccessor) {
					if (_instance == null) {
						_instance = new NativePopup();
					}
					return _instance;
				}
			}
			set {
				if (value == null) {
					_instance = null;
				}
			}
		}
		
		private NativePopup() {
            // nothing here
		}
		
		#endregion
		
		#region Private Fields and Methods

		private bool isShowing;
		private Action<int> onCloseAction;
		private delegate void PopupClosedDelegate(int buttonClicked);
		
        /// <summary>
        /// This is a callback method which gets passed on to native code to be accesible from there.
        /// </summary>
        /// <param name="index">Index of the button pressed by the user.</param>
		[MonoPInvokeCallback(typeof(PopupClosedDelegate))]
		private static void _PopupClosed(int index) {
			Instance.PopupClosed(index);
		}

		private void PopupClosed(int index) {
			isShowing = false;
            // invoke action delegate if set
			if (onCloseAction != null) {
				onCloseAction(index);
			}
		}

        /// <summary>
        // TODO: allowed button count Android ?
        /// Shows a popup message with an action delegate and one to three buttons.
        /// </summary>
        /// <param name="title">The title or headline of the popup.</param>
        /// <param name="message">Text content of the popup.</param>
        /// <param name="onClose">Action delegate with the index of the pressed button. If the popup was closed without any button being pressed, the index will be -1.</param>
        /// <param name="buttons">One to three strings, used as button titles.</param>
		private void ShowImpl(string title, string message, Action<int> onClose, params string[] buttons) {
			if (isShowing) {
                // do nothing if it is already visible
				return;
			}
			isShowing = true;
			onCloseAction = onClose;
            // call external method implemented in native code (Objective-C or Java)
			_ShowPopup(title, message, buttons, buttons.Length, _PopupClosed);
		}

		#endregion

		#region Plugin Code

		#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
			#if UNITY_IOS
			const string external_lib = "__Internal";
			#else
			const string external_lib = "nativepopup";
			#endif
			
			[DllImport(external_lib)]
			private static extern void _ShowPopup(string title, string message, string[] buttonTitles, int buttonCount, PopupClosedDelegate callback);
		#else

        // the following code simulates the popup with unity's legacy ui when running the game from within the editor 

        /// <summary>
        /// Replacement method for use in the editor. Signature matches external method with the same name.
        /// </summary>
        /// <param name="title">The title or headline of the popup.</param>
        /// <param name="message">Text content of the popup.</param>
        /// <param name="buttonTitles">Button titles.</param>
        /// <param name="buttonCount">Button count.</param>
        /// <param name="callback">Callback to execute when a button is pressed.</param>
		private void _ShowPopup(string title, string message, string[] buttonTitles, int buttonCount, PopupClosedDelegate callback) {
			var fallback = Singleton.Get<NativePopupFallback>();
			fallback._title = title;
			fallback._message = message;
			fallback._buttonTitles = buttonTitles;
			fallback._buttonCount = buttonCount;
			fallback._callback = callback;
			fallback._visible = true;
		}

        /// <summary>
        /// A MonoBehaviour which creates the ui to simulate the popup when used in the editor.
        /// Should be used as a singleton.
        /// </summary>
		private class NativePopupFallback : MonoBehaviour {
			public string _title;
			public string _message;
			public string[] _buttonTitles;
			public int _buttonCount;
			public PopupClosedDelegate _callback;
			public bool _visible;

			private GUIStyle _boxStyle = null;
			private GUIStyle _buttonStyle = null;

			private void OnGUI() {
				if (_boxStyle == null) {
					_boxStyle = new GUIStyle(GUI.skin.box);
					_boxStyle.alignment = TextAnchor.MiddleCenter;
					_boxStyle.wordWrap = true;
					_boxStyle.fontSize = 24;
				}

				if (_buttonStyle == null) {
					_buttonStyle = new GUIStyle(GUI.skin.button);
					_buttonStyle.wordWrap = true;
					_buttonStyle.fontSize = 24;
				}

				if (!_visible) {
					return;
				}
				
				var top = Screen.height * 0.2f;
				var left = Screen.width * 0.1f;
				var width = Screen.width * 0.8f;
				var height = Screen.height * 0.6f;

				var buttonWidth = width / _buttonCount;
				var buttonHeight = Screen.height * 0.1f;
				var buttonTop = top + height - buttonHeight;


				GUI.Box(new Rect(left, top, width, height), _message, _boxStyle);
				GUI.Box(new Rect(left, top, width, buttonHeight), _title, _boxStyle);

				for (int i = 0; i < _buttonCount; i++) {
					if (GUI.Button(new Rect(left + i * buttonWidth, buttonTop, buttonWidth, buttonHeight), _buttonTitles[i], _buttonStyle)) {
						_visible = false;
						_callback(i);
					}
				}
			}
		}

		#endif

		#endregion
		
	}
}

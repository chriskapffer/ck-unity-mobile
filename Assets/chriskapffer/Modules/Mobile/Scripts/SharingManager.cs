using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace ChrisKapffer.Mobile {

    /// <summary>
    /// This class enables you to share some text and optionaly a screenshot with the system's default sharing methods.
    /// There is no Windows Phone implementation.
    /// </summary>
    public class SharingManager {

    	#region Public Interface

        /// <summary>
        /// Share the specified text, url, appendScreenShot, rect and onFinished.
        /// </summary>
        /// <param name="text">Text to share.</param>
        /// <param name="url">(optional) A link to your game or company website.</param>
        /// <param name="appendScreenShot">(optional) Wether to append a screenshot or not.</param>
        /// <param name="rect">(optional) Specifies the rect the screenshot is taken from.</param>
        /// <param name="onFinished">(optional) Handler with the sharing destination and its success as params.</param>
    	public static void Share(string text, string url = "", bool appendScreenShot = false, Rect rect = new Rect(), Action<string, bool> onFinished = null) {
    		Instance.ShareImpl(text, url, appendScreenShot, rect, onFinished);
    	}

    	#endregion

    	#region Singleton Implementation
    	
    	private static object _instanceAccessor = new object();
    	#if !NETFX_CORE
        // hide _instance from IntelliSense
    	[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    	#endif
    	private static SharingManager _instance;
    	private static SharingManager Instance {
    		get {
    			lock(_instanceAccessor) {
    				if (_instance == null) {
    					_instance = new SharingManager();
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
    	
    	private SharingManager() {
    		
    	}
    	
    	#endregion

    	#region Private Fields and Methods

    	private bool isShowing;
    	private Action<string, bool> onFinishedAction;
    	private delegate void FinishedSharingDelegate(string destination, bool completed);
    	
    	[MonoPInvokeCallback(typeof(FinishedSharingDelegate))]
    	private static void _FinishedSharing(string destination, bool completed) {
    		Instance.FinishedSharing(destination, completed);
    	}
    	
        /// <summary>
        /// This is a callback method which gets passed on to native code to be accesible from there.
        /// </summary>
        /// <param name="destination">Name of the app or service the content was shared with.</param>
        /// <param name="completed">Wether or net the operation was successfull.</param>
    	private void FinishedSharing(string destination, bool completed) {
    		isShowing = false;
    		if (onFinishedAction != null) {
    			onFinishedAction(destination, completed);
    		}
    	}
    	
        /// <summary>
        /// Share the specified text, url, appendScreenShot, rect and onFinished.
        /// </summary>
        /// <param name="text">Text to share.</param>
        /// <param name="url">(optional) A link to your game or company website.</param>
        /// <param name="appendScreenShot">(optional) Wether to append a screenshot or not.</param>
        /// <param name="rect">(optional) Specifies the rect the screenshot is taken from.</param>
        /// <param name="onFinished">(optional) Handler with the sharing destination and its success as params.</param>
        private void ShareImpl(string text, string url = "", bool appendScreenShot = false, Rect rect = new Rect(), Action<string, bool> onFinished = null) {
    		if (isShowing) {
    			return;
    		}
    		isShowing = true;
    		onFinishedAction = onFinished;

    		if (appendScreenShot) {
                // take a screenshot and then share it together with the text
    			StaticCoroutine.Start(TakeScreenShot(rect, (data) => {
    				_Share(text, url, data, (uint)data.Length, _FinishedSharing);
    			}));
    		} else {
    			_Share(text, url, new byte[0], 0, _FinishedSharing);
    		}
    	}

        /// <summary>
        /// Takes a screen shot.
        /// </summary>
        /// <returns>IEnumerator (This method is used as a coroutine)</returns>
        /// <param name="rect">Specifies the rect the screenshot is taken from.</param>
        /// <param name="onCompletion">Handles the recorded image data.</param>
    	private IEnumerator TakeScreenShot(Rect rect, Action<byte[]> onCompletion) {
            // need to wait here until the screen buffer is completely filled and can be read from
    		yield return new WaitForEndOfFrame();
    		var texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
    		texture.ReadPixels(rect, 0, 0, false);
    		texture.Apply();
            // invoke the handler to do something with the screenshot content
    		onCompletion.Invoke(texture.EncodeToJPG());
    	}

    	#endregion

    	#region Plugin Code

    	#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        	#if UNITY_IOS
        	const string external_lib = "__Internal";
        	#else
        	const string external_lib = "sharingmanager";
        	#endif

        	[DllImport(external_lib)]
        	private static extern void _Share(string text, string url, byte[] imageData, uint imageSize, FinishedSharingDelegate callback);
    	#else

        /// <summary>
        /// This is just a placeholder method when running the game from within the editor. It simply writes the content to share on the console.
        /// </summary>
    	private void _Share(string text, string url, byte[] imageData, uint imageSize, FinishedSharingDelegate callback) {
    		Debug.Log("Sharing -> text:\"" + text + "\" url:\"" + url + "\" imageSize:" + imageSize);
    		Debug.LogWarning("Sharing is only supported on Android and iOS devices.");
    		callback("editor", false);
    	}
    	#endif

    	#endregion

    }
}

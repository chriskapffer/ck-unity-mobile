using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace ChrisKapffer.Mobile {

	public delegate void NetworkTypeChangedEventHandler(NetworkInfo.NetworkType currentType);

    /// <summary>
    /// This class allows you to determine the type of the carrier network the phone is currently connected to.
    /// Unity only knows if it is connected via wifi or carrier network or if it has no connection at all.
    /// But since there are huge differences of download speed it does make a difference if you have a LTE or Edge
    /// connection. Knowing which one it is helps you for example to determine wether a video-ad should be shown or not.
    /// 
    /// </summary>
	public class NetworkInfo {
	
        /// <summary>
        /// Different types of networks. Don't change the indices or you will break the mapping of the corresponding iOS and Android network types!
        /// </summary>
		public enum NetworkType {
			unknown   = -1,
			xRTT      =  0,
			CDMA      =  1,
			IDEN      =  2,
			GPRS      =  3,
			Edge      =  4,
			UMTS      =  5, // same as WCDMA
			EVDO_Rev0 =  6,
			EVDO_RevA =  7,
			EVDO_RevB =  8,
			eHRPD     =  9,
			HSPA      = 10,
			HSDPA     = 11,
			HSUPA     = 12,
			HSPAP     = 13,
			LTE       = 14,
		};

		#region Public Interface

        /// <summary>
        /// Occurs when the network type has changed. Notice that we specifically use the
        /// instance accessor here in order to initialize it <see cref="NetworkInfo.Init"/>
        /// </summary>
        public static event NetworkTypeChangedEventHandler OnNetworkTypeChanged {
            add {
                Instance.onNetworkTypeChangedPrivate += value;
                // to trigger a potential change
                Instance.Refresh();
            }
            remove {
                Instance.onNetworkTypeChangedPrivate -= value;             
            }
        }

        /// <summary>
        /// Enable caching if you query the current network type in a high frequency but want to avoid too many native code requests.
        /// </summary>
        /// <value><c>true</c> if caching should be enabled; otherwise, <c>false</c>.</value>
		public static bool CachingEnabled {
			get {
				return Instance.cachingEnabled;
			}
			set {
				Instance.cachingEnabled = value;
			}
		}

        /// <summary>
        /// Gets the type of the current network.
        /// </summary>
        /// <returns>The current network type.</returns>
        /// <param name="ignoreCaching">If set to <c>true</c> native code will be executed to determine the type, otherwise the previously cached value is used.</param>
		public static NetworkType GetCurrentNetworkType(bool ignoreCaching = false) {
			return Instance.GetCurrentNetworkTypeImpl(ignoreCaching);
		}

        /// <summary>
        /// Determines whether the current network type provides a fast enough connection. E.g. for streaming videos
        /// </summary>
        /// <returns><c>true</c> if connected via wifi or to a network type better than edge, <c>false</c> otherwise.</returns>
		public static bool IsCurrentNetworkTypeFast() {
			return Instance.IsCurrentNetworkTypeFastImpl();
		}

		#endregion

		#region Singleton Implementation

		private static object _instanceAccessor = new object();
#if !NETFX_CORE
        // hide _instance from IntelliSense
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#endif
		private static NetworkInfo _instance;
		private static NetworkInfo Instance {
			get {
				lock(_instanceAccessor) {
					if (_instance == null) {
						_instance = new NetworkInfo();
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

		private NetworkInfo() {
			Init();
		}
		
		~NetworkInfo() {
			Deinit();
		}

		#endregion

		#region Private Fields and Methods

		private bool cachingEnabled = true;
		private NetworkType currentType = NetworkType.unknown;
		private delegate void NetworkTypeChangedDelegate(int technologyIdx);

        /// <summary>
        /// Occurs when the network type has changed.
        /// </summary>
        private event NetworkTypeChangedEventHandler onNetworkTypeChangedPrivate;

        /// <summary>
        /// Handles the network type changed event.
        /// This is a callback method which gets passed on to native code to be accesible from there.
        /// </summary>
        /// <param name="index">Index of the new network, the client just got connected with.</param>
		[MonoPInvokeCallback(typeof(NetworkTypeChangedDelegate))]
		private static void _NetworkTypeChanged(int index) {
            // Unfortunately we can not ensure that we are on the main thread at this point. While our iOS plugin
            // doess call this from the main thread our Android plugin does not.
            ThreadHelper.DispatchOnMain(() => {
                Instance.NetworkTypeChangedImpl(index);
            });
		}

        /// <summary>
        /// The implementation of the corresponding callback. <see cref="_NetworkTypeChanged"/>
        /// </summary>
        /// <param name="index">Index of the new network, the client just got connected with.</param>
        private void NetworkTypeChangedImpl(int index) {
            currentType = (NetworkType)index;
            if (onNetworkTypeChangedPrivate != null) {
                onNetworkTypeChangedPrivate(currentType);
            }
        }

        /// <summary>
        /// This enables native plugins to do some initialization before querying the network status
        /// </summary>
		private void Init() {
            // because the _NetworkTypeChanged callback could originate from outside of the main thread, we need to initialize the ThreadHelper here
            // to be able to jump back to the main thread when executing the callback code
            ThreadHelper.Init();
			#if !UNITY_EDITOR
			_RegisterNetworkTypeChangedCallback(_NetworkTypeChanged);
			#endif
            // create our private Monobehaviour instance, to be able to react to application pause and quit
			Singleton.Get<NetworkInfoBehaviour>();
		}
		
        /// <summary>
        /// This enables native plugins to do some clean up when the app quits.
        /// </summary>
		private void Deinit() {
			#if !UNITY_EDITOR
			_CleanupResources();
			#endif
		}

        /// <summary>
        /// Forces an update of <see cref="currentType"/> to the current value.
        /// </summary>
		private void Refresh() {
			GetCurrentNetworkTypeImpl(true);
		}

        /// <summary>
        /// Gets the current network type.
        /// </summary>
        /// <returns>The current network type.</returns>
        /// <param name="ignoreCaching">If set to <c>true</c> native code will be executed to determine the type, otherwise the previously cached value is used.</param>
		private NetworkType GetCurrentNetworkTypeImpl(bool ignoreCaching = false) {
			#if UNITY_EDITOR
			if (Application.isEditor) {
				return NetworkType.unknown;
			}
			#endif
			if (!cachingEnabled || ignoreCaching || currentType == NetworkType.unknown) {
				currentType = (NetworkType)_GetCurrentNetworkType();
			}

			return currentType;
		}

        /// <summary>
        /// Determines whether the current network type provides a fast enough connection. E.g. for streaming videos
        /// </summary>
        /// <returns><c>true</c> if connected via wifi or to a network type better than edge, <c>false</c> otherwise.</returns>
		private bool IsCurrentNetworkTypeFastImpl() {
			if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) {
				return true;
			}
			if (Application.internetReachability == NetworkReachability.NotReachable) {
				return false;
			}

			return GetCurrentNetworkTypeImpl() > NetworkType.Edge;
		}

		#endregion

		#region Unity Hooks

        /// <summary>
        /// A private MonoBehaviour to be able to react to quit and pause events.
        /// </summary>
		private class NetworkInfoBehaviour : MonoBehaviour {
			void OnApplicationPause(bool pause) {
				if (!pause) {
					NetworkInfo.Instance.Refresh();
				}
			}

			void OnApplicationQuit() {
				NetworkInfo.Instance.Deinit();
			}
		}

		#endregion

		#region Plugin Code

		#if UNITY_IOS
		const string external_lib = "__Internal";
		#else
		const string external_lib = "networkinfo";
		#endif

		[DllImport(external_lib)]
		private static extern void _RegisterNetworkTypeChangedCallback(NetworkTypeChangedDelegate callback);

		[DllImport(external_lib)]
		private static extern int _GetCurrentNetworkType();

		[DllImport(external_lib)]
		private static extern void _CleanupResources();

		#endregion

	}
}

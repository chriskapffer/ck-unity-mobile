using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace ChrisKapffer.Mobile {

	public delegate void NetworkTypeChangedEventHandler(NetworkInfo.NetworkType currentType);

	public class NetworkInfo {
	
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

		public static event NetworkTypeChangedEventHandler OnNetworkTypeChanged;

		public static bool CachingEnabled {
			get {
				return Instance.cachingEnabled;
			}
			set {
				Instance.cachingEnabled = value;
			}
		}

		public static NetworkType GetCurrentNetworkType(bool ignoreCaching = false) {
			return Instance.GetCurrentNetworkTypeImpl(ignoreCaching);
		}

		public static bool IsCurrentNetworkTypeFast() {
			return Instance.IsCurrentNetworkTypeFastImpl();
		}

		#endregion

		#region Singleton Implementation

		private static object _instanceAccessor = new object();
#if !NETFX_CORE
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

		[MonoPInvokeCallback(typeof(NetworkTypeChangedDelegate))]
		private static void _NetworkTypeChanged(int index) {
			Instance.currentType = (NetworkType)index;
			//Singleton.Get<ScreenLog>().Print("NETWORK change", Instance.currentType.ToString());
			if (OnNetworkTypeChanged != null) {
				OnNetworkTypeChanged(Instance.currentType);
			}
		}

		private void Init() {
			#if !UNITY_EDITOR
			_RegisterNetworkTypeChangedCallback(_NetworkTypeChanged);
			#endif
			Singleton.Get<NetworkInfoBehaviour>();
		}
		
		private void Deinit() {
			#if !UNITY_EDITOR
			_CleanupResources();
			#endif
		}

		private void Refresh() {
			GetCurrentNetworkTypeImpl(true);
		}

		private NetworkType GetCurrentNetworkTypeImpl(bool ignoreCaching = false) {
			#if UNITY_EDITOR
			if (Application.isEditor) {
				return NetworkType.unknown;
			}
			#endif
			if (!cachingEnabled || ignoreCaching || currentType == NetworkType.unknown) {
				currentType = (NetworkType)_GetCurrentNetworkType();
				//Singleton.Get<ScreenLog>().Print("NETWORK", currentType.ToString());
			}

			return currentType;
		}

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

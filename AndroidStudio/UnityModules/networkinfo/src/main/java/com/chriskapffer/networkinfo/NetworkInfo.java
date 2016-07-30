package com.chriskapffer.networkinfo;


import android.annotation.TargetApi;
import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.ConnectivityManager;
import android.os.Build;
import android.telephony.TelephonyManager;
import android.util.Log;
import android.util.SparseIntArray;

public class NetworkInfo
{
	public class NetworkChangeReceiver extends BroadcastReceiver {
		@Override
		public void onReceive(Context context, Intent intent) {
			NetworkInfo.handleNetworkChange();
		}
	}

	private ConnectivityManager connectivityManager;
	private NetworkChangeReceiver receiver;
	private SparseIntArray networkTypes;
	private Activity activity;
	
	private static final NetworkInfo instance = new NetworkInfo();
	
	private NetworkInfo() {
		connectivityManager = null;
		
		// sorted roughly by speed
		networkTypes = new SparseIntArray();
		networkTypes.put(TelephonyManager.NETWORK_TYPE_UNKNOWN, -1);
		
		// these are generally considered slow
		networkTypes.put(TelephonyManager.NETWORK_TYPE_1xRTT,  0);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_CDMA,   1);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_IDEN,   2);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_GPRS,   3);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_EDGE,   4);
		
		// these are generally considered fast
		networkTypes.put(TelephonyManager.NETWORK_TYPE_UMTS,   5);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_EVDO_0, 6);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_EVDO_A, 7);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_EVDO_B, 8);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_HSPA,  10);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_HSDPA, 11);
		networkTypes.put(TelephonyManager.NETWORK_TYPE_HSUPA, 12);
		
		if(Build.VERSION.SDK_INT >= 11) {
			addTypesAbove11(networkTypes);
		}
		
		if(Build.VERSION.SDK_INT >= 13) {
			addTypesAbove13(networkTypes);
		}
	}

	public static void init(Context context) {
		instance.initImpl(context);
	}
	
	private void initImpl(Context context) {
		connectivityManager = (ConnectivityManager)context.getSystemService(Context.CONNECTIVITY_SERVICE);
		
		// context is actual an activity (at least it should be)
		activity = (Activity)context;
		
		// Registers BroadcastReceiver to track network connection changes.
        IntentFilter filter = new IntentFilter(ConnectivityManager.CONNECTIVITY_ACTION);
        receiver = new NetworkChangeReceiver();
        activity.registerReceiver(receiver, filter);
	}
	
	public static void deinit() {
		instance.deinitImpl();
	}
	
	private void deinitImpl() {
        if (receiver != null) {
        	activity.unregisterReceiver(receiver);
        	Log.d("NetworkInfo", "unregister NetworkChangeReceiver");
        }
	}

	public static void handleNetworkChange() {
		instance.handleNetworkChangeImpl();
	}
	
	private void handleNetworkChangeImpl() {
		networkTypeChanged(getCurrentNetworkTypeImpl());
		Log.d("NetworkInfo", "handleNetworkChange");
	}
	
	public static int getCurrentNetworkType() {
		return instance.getCurrentNetworkTypeImpl();
	}
	
	private int getCurrentNetworkTypeImpl() {
		int failure = -1;
		if (connectivityManager == null) {
			return failure;
		}
		
		android.net.NetworkInfo info = connectivityManager.getActiveNetworkInfo();
		if (info == null || !info.isConnected() || info.getType() == ConnectivityManager.TYPE_WIFI) {
			return failure;
		}
		
		return networkTypes.get(info.getSubtype(), failure);
	}

	@TargetApi(11)
	private void addTypesAbove11(SparseIntArray types) {
		types.put(TelephonyManager.NETWORK_TYPE_EHRPD, 9);
		types.put(TelephonyManager.NETWORK_TYPE_LTE,  14);
	}
	
	@TargetApi(13)
	private void addTypesAbove13(SparseIntArray types) {
		types.put(TelephonyManager.NETWORK_TYPE_HSPAP, 13);
	}
	
	private native void networkTypeChanged(int type);
}

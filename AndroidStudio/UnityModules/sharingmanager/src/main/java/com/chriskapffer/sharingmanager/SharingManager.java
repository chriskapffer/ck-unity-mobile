package com.chriskapffer.sharingmanager;

import java.io.File;
import java.io.FileOutputStream;
import java.nio.ByteBuffer;
import java.util.List;

import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.net.Uri;
import android.os.AsyncTask;
import android.support.v4.content.FileProvider;
import android.util.Log;

public class SharingManager
{
	private Context context;
	private boolean alreadyShowing;
	
	private static final SharingManager instance = new SharingManager();
	
	private SharingManager() {
		alreadyShowing = false;
	}
	
	public static void init(Context context) {
		instance.context = context;
	}

	public static boolean isShowing() {
		return instance.alreadyShowing;
	}
	
	public static void share(String text, String url, ByteBuffer imageData) {
		instance.shareImpl(text, url, imageData);
	}
	
	private void shareImpl(String text, String url, ByteBuffer imageData) {
		if (alreadyShowing) {
			return;
		}
		
		alreadyShowing = true;

		final String textToSend = text + " " + url;
		byte[] data = new byte[imageData.limit()];
		imageData.get(data);
		SaveImageToDiskTask saveTask = new SaveImageToDiskTask(new SaveImageToDiskCompletionListener() {
			@Override
			public void onTaskCompleted(SaveImageToDiskResult result) {
				if (result.success) {
					Intent sendIntent = new Intent();
					sendIntent.setAction(Intent.ACTION_SEND);
					sendIntent.putExtra(Intent.EXTRA_STREAM, result.imageUri);
					sendIntent.putExtra(Intent.EXTRA_TEXT, textToSend);
					sendIntent.setType("image/jpeg");
					
					// grant permission to all apps that can handle this intent
					List<ResolveInfo> resInfoList = context.getPackageManager().queryIntentActivities(sendIntent, PackageManager.MATCH_DEFAULT_ONLY);
					for (ResolveInfo resolveInfo : resInfoList) {
					    String packageName = resolveInfo.activityInfo.packageName;
					    context.grantUriPermission(packageName, result.imageUri, Intent.FLAG_GRANT_READ_URI_PERMISSION);
					}
					
					context.startActivity(Intent.createChooser(sendIntent, "Share your score"));
				}
				alreadyShowing = false;
				sharingFinished("no way of knowing on android", result.success);
			}
		});
		saveTask.execute(data);
	}


	public class SaveImageToDiskResult
	{
		public Uri imageUri;
	    public boolean success;
	    
	    public SaveImageToDiskResult(Uri imageUri, boolean success) {
	    	this.imageUri = imageUri;
	    	this.success = success;
	    }
	}
	
	interface SaveImageToDiskCompletionListener {
	    void onTaskCompleted(SaveImageToDiskResult result);
	}
	
	class SaveImageToDiskTask extends AsyncTask<byte[], Integer, SaveImageToDiskResult> {
	    private SaveImageToDiskCompletionListener listener;

	    public SaveImageToDiskTask(SaveImageToDiskCompletionListener listener){
	        this.listener = listener;
	    }
		
		@Override
		protected void onPostExecute(SaveImageToDiskResult result) {
			listener.onTaskCompleted(result);
		}

		@Override
		protected SaveImageToDiskResult doInBackground(byte[]... imageDataAsJPG) {
			File imageDir = new File(context.getCacheDir(), "ck_shared_images");
			if (!imageDir.exists()) {
				imageDir.mkdir();
			}
			
			File image = new File(imageDir, "score.jpg");
			if (image.exists()) {
				image.delete();
			}

			Uri sharableUri = FileProvider.getUriForFile(context, "com.chriskapffer.sharingmanager.fileprovider", image);
			
			try {
				FileOutputStream foStream = new FileOutputStream(image.getPath());
				foStream.write(imageDataAsJPG[0]);
				foStream.close();
			} catch (java.io.IOException e) {
				Log.e("SharingManager", e.getMessage());
				return new SaveImageToDiskResult(sharableUri, false);
			}
			
			return new SaveImageToDiskResult(sharableUri, true);
		}
	}
	
	private native void sharingFinished(String destination, boolean completed);
}

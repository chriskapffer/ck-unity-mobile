package com.chriskapffer.nativepopup;

import android.annotation.TargetApi;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.DialogInterface.OnCancelListener;
import android.content.DialogInterface.OnClickListener;
import android.content.DialogInterface.OnShowListener;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.ContextThemeWrapper;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;

public class NativePopup
{
	private Context context;
	private boolean alreadyShowing;
	private int buttonCount;

	private static final NativePopup instance = new NativePopup();
	
	private NativePopup() {
		alreadyShowing = false;
	}
	
	public static void init(Context context) {
		instance.context = context;
	}

	public static boolean isShowing() {
		return instance.alreadyShowing;
	}
	
	public static void show(String title, String message, String[] buttons) {
		instance.showImpl(title, message, buttons);
	}
	
	private void showImpl(final String title, final String message, final String[] buttons) {
		if (alreadyShowing) {
			return;
		}

		alreadyShowing = true;
		buttonCount = buttons.length;
		if (buttonCount > 3) {
			Log.w("NativePopup", "An alert dialog can not have more than three buttons.");
			buttonCount = 3;
		}

		// There is a problem here: The alert dialog and its listeners are living on the ui thread, which is
		// not the thread unity is running in. To be able to execute code on the unity thread from within the
		// ui thread we need a Handler object. The handler needs the Looper of the current thread, but the
		// unity thread doesn't use a Looper. Creating one with Looper.prepare() is not advisable, because
		// it halts execution. That's why we don't use a handler here. Instead we have to make sure that we use the
		// proper thread within unity's code.
		//final Handler unityHandler = new Handler();

		//Log.d("NativePopup", "unity_thread_" + Integer.toString((int) Thread.currentThread().getId()));

		final OnClickListener onClickListener = new OnClickListener() {
			@Override
			public void onClick(DialogInterface dialog, int which) {
				//Log.d("NativePopup", "ui_thread_" + Integer.toString((int) Thread.currentThread().getId()));
				final int buttonIdx = which;
//				unityHandler.post(new Runnable() {
//					@Override
//					public void run() {
						alreadyShowing = false;
						popupDialogDismissed(buttonTypeToButtonNumber(buttonIdx));
//					}
//				});
			}
		};

		final OnCancelListener onCancelListener = new OnCancelListener() {
			@Override
			public void onCancel(DialogInterface dialog) {
				//Log.d("NativePopup", "ui_thread_" + Integer.toString((int) Thread.currentThread().getId()));
//				unityHandler.post(new Runnable() {
//					@Override
//					public void run() {
						alreadyShowing = false;
						popupDialogDismissed(-1);
//					}
//				});
			}
		};

		final Activity activity = (Activity)context;
		activity.runOnUiThread(new Runnable() {
			public void run() {
				Context themedContext = new ContextThemeWrapper(activity, getTheme());
				AlertDialog alertDialog = new AlertDialog.Builder(themedContext).create();
				
				alertDialog.setTitle(title);
				alertDialog.setMessage(message);
				for (int i = 0; i < buttonCount; i++) {
					alertDialog.setButton(buttonNumberToButtonType(i), Integer.toString(i), onClickListener);
				}
				fixButtonsForMaterialThemeOnLollipop(alertDialog, buttons);
				alertDialog.setOnCancelListener(onCancelListener);
				alertDialog.setCanceledOnTouchOutside(false);
				alertDialog.setCancelable(true);
				alertDialog.show();
			}
		});
	}

	private void fixButtonsForMaterialThemeOnLollipop(final AlertDialog alertDialog, final String[] buttons) {
		alertDialog.setOnShowListener(new OnShowListener() {
			@Override
			public void onShow(DialogInterface dialog) {
				View mainView = alertDialog.getWindow().getDecorView();
				mainView.setPadding(
						Math.max(0, mainView.getPaddingLeft() - 20),
						mainView.getPaddingTop(),
						Math.max(0, mainView.getPaddingRight() - 20),
						mainView.getPaddingBottom());
				
				View contentView = mainView.findViewById(android.R.id.content);
				int contentWidth = contentView.getWidth() - mainView.getPaddingLeft() - mainView.getPaddingRight();
				int buttonWidth = contentWidth / buttonCount;

				for (int i = 0; i < buttonCount; i++) {
					Button button = alertDialog.getButton(buttonNumberToButtonType(i));
					button.setPadding(10, button.getPaddingTop(), 10, button.getPaddingBottom());
					//button.setTextSize(TypedValue.COMPLEX_UNIT_PX, button.getTextSize());
					button.setText(buttons[i]);
					
					LinearLayout.LayoutParams params = (LinearLayout.LayoutParams)button.getLayoutParams();
					params.weight = 1;
					params.width = buttonWidth;
					button.setLayoutParams(params);
				}
			}
		});
	}
	
	private int buttonTypeToButtonNumber(int type) {
		int number = -1;
		switch(type) {
			case DialogInterface.BUTTON_POSITIVE:
				number = 0;
				break;
			case DialogInterface.BUTTON_NEUTRAL:
				number = 1;
				break;
			case DialogInterface.BUTTON_NEGATIVE:
				number = buttonCount - 1;
				break;
			default:
				break;
		}
		return number;
	}
	
	private int buttonNumberToButtonType(int number) {
		int type = -1;
		switch(number) {
			case 0:
				type = DialogInterface.BUTTON_POSITIVE;
				break;
			case 1:
				type = buttonCount > 2
					? DialogInterface.BUTTON_NEUTRAL
					: DialogInterface.BUTTON_NEGATIVE;
				break;
			case 2:
				type = DialogInterface.BUTTON_NEGATIVE;
				break;
			default:
				break;
		}
		return type;
	}
	
	private int getTheme() {
		int theme = android.R.style.Theme_Dialog;
		if(Build.VERSION.SDK_INT >= 11) {
			theme = getHoloTheme();
		}
		if(Build.VERSION.SDK_INT >= 14) {
			theme = getDefaultTheme();
		}
		return theme;
	}
	
	@TargetApi(11)
	private int getHoloTheme() {
		return android.R.style.Theme_Holo_Dialog;
	}
	
	@TargetApi(14)
	private int getDefaultTheme() {
		return android.R.style.Theme_DeviceDefault_Dialog;
	}
	
	private native void popupDialogDismissed(int button);
}

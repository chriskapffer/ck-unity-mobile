#include <stdlib.h>
#include <jni.h>
#include <android/log.h>

extern "C"
{

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

JavaVM*		java_vm;
jclass      nativePopup_class;
jmethodID	isShowing_method;
jmethodID	show_method;

const char *pluginName = "NativePopup";

///////////////////////////////////////////////////////////////////////////////
// JNI SYSTEM FUNCTIONS
///////////////////////////////////////////////////////////////////////////////

jint JNI_OnLoad(JavaVM* vm, void* reserved)
{
	java_vm = vm;

	JNIEnv* jni_env = 0;
	java_vm->AttachCurrentThread(&jni_env, 0);

	jclass activity_class		= jni_env->FindClass("com/unity3d/player/UnityPlayer");
	jfieldID activity_fieldId	= jni_env->GetStaticFieldID(activity_class, "currentActivity", "Landroid/app/Activity;");
	jobject activity_object		= jni_env->GetStaticObjectField(activity_class, activity_fieldId);

	__android_log_print(ANDROID_LOG_INFO, pluginName, "[%s] activity object = %p\n", __FUNCTION__, activity_object);

	nativePopup_class 		= (jclass)jni_env->NewGlobalRef(jni_env->FindClass("com/chriskapffer/nativepopup/NativePopup"));
	jmethodID init_method	= jni_env->GetStaticMethodID(nativePopup_class, "init", "(Landroid/content/Context;)V");
	jni_env->CallStaticVoidMethod(nativePopup_class, init_method, activity_object);

	isShowing_method	= jni_env->GetStaticMethodID(nativePopup_class, "isShowing", "()Z");
	show_method			= jni_env->GetStaticMethodID(nativePopup_class, "show", "(Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;)V");

	return JNI_VERSION_1_6;		// minimum JNI version
}

///////////////////////////////////////////////////////////////////////////////
// CALLS FROM C# TO JAVA
///////////////////////////////////////////////////////////////////////////////

typedef void (*PopupDialogDismissedCallback)(int);
PopupDialogDismissedCallback popupDialogDismissedCallback;

JNIEXPORT void JNICALL Java_com_chriskapffer_nativepopup_NativePopup_popupDialogDismissed(JNIEnv *env, jobject object, jint button)
{
	if (popupDialogDismissedCallback != NULL) {
		popupDialogDismissedCallback((int)button);
	}
}

void _ShowPopup(const char *title, const char *message, const char *buttonTitles[], int buttonCount, PopupDialogDismissedCallback callback)
{
	JNIEnv* jni_env = 0;
	java_vm->AttachCurrentThread(&jni_env, 0);

	bool isShowing = (bool)jni_env->CallStaticBooleanMethod(nativePopup_class, isShowing_method);
	if (!isShowing) {
		jstring jTitle = jni_env->NewStringUTF(title);
		jstring jMessage = jni_env->NewStringUTF(message);

		jclass string_class = jni_env->FindClass("java/lang/String");
		jobjectArray jButtons = (jobjectArray)jni_env->NewObjectArray(buttonCount, string_class, jni_env->NewStringUTF(""));
		for(int i = 0; i < buttonCount; i++) {
			jni_env->SetObjectArrayElement(jButtons, i, jni_env->NewStringUTF(buttonTitles[i]));
		}

		popupDialogDismissedCallback = callback;
		jni_env->CallStaticVoidMethod(nativePopup_class, show_method, jTitle, jMessage, jButtons);
	}
}

}	// end of extern "C"


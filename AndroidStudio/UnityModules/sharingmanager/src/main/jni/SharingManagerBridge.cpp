#include <stdlib.h>
#include <jni.h>
#include <android/log.h>

extern "C"
{

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

JavaVM*		java_vm;
jclass      sharingManager_class;
jmethodID	isShowing_method;
jmethodID	share_method;

const char *pluginName = "SharingManager";

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

	sharingManager_class	= (jclass)jni_env->NewGlobalRef(jni_env->FindClass("com/chriskapffer/sharingmanager/SharingManager"));
	jmethodID init_method	= jni_env->GetStaticMethodID(sharingManager_class, "init", "(Landroid/content/Context;)V");
	jni_env->CallStaticVoidMethod(sharingManager_class, init_method, activity_object);

	isShowing_method	= jni_env->GetStaticMethodID(sharingManager_class, "isShowing", "()Z");
	share_method		= jni_env->GetStaticMethodID(sharingManager_class, "share", "(Ljava/lang/String;Ljava/lang/String;Ljava/nio/ByteBuffer;)V");

	return JNI_VERSION_1_6;		// minimum JNI version
}

///////////////////////////////////////////////////////////////////////////////
// CALLS FROM C# TO JAVA
///////////////////////////////////////////////////////////////////////////////

typedef void (*SharingFinishedCallback)(const char*, bool);
SharingFinishedCallback sharingFinishedCallback;

JNIEXPORT void JNICALL Java_com_chriskapffer_sharingmanager_SharingManager_sharingFinished(JNIEnv *env, jobject object, jstring destination, jboolean completed)
{
	if (sharingFinishedCallback != NULL) {
		sharingFinishedCallback(env->GetStringUTFChars(destination, 0), (bool)completed);
	}
}

void _Share(const char *text, const char *url, const void *imageData, uint imageSize, SharingFinishedCallback callback)
{
	JNIEnv* jni_env = 0;
	java_vm->AttachCurrentThread(&jni_env, 0);

	bool isShowing = (bool)jni_env->CallStaticBooleanMethod(sharingManager_class, isShowing_method);
	if (!isShowing) {
		jstring jText = jni_env->NewStringUTF(text);
		jstring jUrl = jni_env->NewStringUTF(url);
		jobject jByteBuffer = jni_env->NewDirectByteBuffer(const_cast<void*>(imageData), imageSize);

		sharingFinishedCallback = callback;
		jni_env->CallStaticVoidMethod(sharingManager_class, share_method, jText, jUrl, jByteBuffer);
	}
}

}	// end of extern "C"


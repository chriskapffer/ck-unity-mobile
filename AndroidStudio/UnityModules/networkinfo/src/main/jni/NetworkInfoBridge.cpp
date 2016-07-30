#include <stdlib.h>
#include <jni.h>
#include <android/log.h>

extern "C"
{

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

JavaVM*		java_vm;
jclass      networkInfo_class;
jmethodID	getCurrentNetworkType_method;
jmethodID	deinit_method;

const char *pluginName = "NetworkInfo";

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

	networkInfo_class 		= (jclass)jni_env->NewGlobalRef(jni_env->FindClass("com/chriskapffer/networkinfo/NetworkInfo"));
	jmethodID init_method	= jni_env->GetStaticMethodID(networkInfo_class, "init", "(Landroid/content/Context;)V");
	jni_env->CallStaticVoidMethod(networkInfo_class, init_method, activity_object);

	getCurrentNetworkType_method	= jni_env->GetStaticMethodID(networkInfo_class, "getCurrentNetworkType", "()I");
	deinit_method					= jni_env->GetStaticMethodID(networkInfo_class, "deinit", "()V");

	return JNI_VERSION_1_6;		// minimum JNI version
}

void JNI_OnUnload(JavaVM* vm, void* reserved)
{
	__android_log_print(ANDROID_LOG_INFO, pluginName, "[%s]", __FUNCTION__);

	java_vm = vm;

	JNIEnv* jni_env = 0;
	java_vm->AttachCurrentThread(&jni_env, 0);

	jni_env->CallStaticVoidMethod(networkInfo_class, deinit_method);

	return;
}

///////////////////////////////////////////////////////////////////////////////
// CALLS FROM C# TO JAVA
///////////////////////////////////////////////////////////////////////////////

typedef void (*NetworkTypeChangedCallback)(int);
NetworkTypeChangedCallback networkTypeChangedCallback;

JNIEXPORT void JNICALL Java_com_chriskapffer_networkinfo_NetworkInfo_networkTypeChanged(JNIEnv *env, jobject object, jint type)
{
	if (networkTypeChangedCallback != NULL) {
		networkTypeChangedCallback((int)type);
	}
}

void _RegisterNetworkTypeChangedCallback(NetworkTypeChangedCallback callback)
{
	networkTypeChangedCallback = callback;
}

int _GetCurrentNetworkType()
{
	JNIEnv* jni_env = 0;
	java_vm->AttachCurrentThread(&jni_env, 0);

	int result = (int)jni_env->CallStaticIntMethod(networkInfo_class, getCurrentNetworkType_method);
	return result;
}

void _CleanupResources()
{
	JNIEnv* jni_env = 0;
	java_vm->AttachCurrentThread(&jni_env, 0);

	jni_env->CallStaticVoidMethod(networkInfo_class, deinit_method);
}

}	// end of extern "C"


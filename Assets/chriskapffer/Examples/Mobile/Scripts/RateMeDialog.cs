using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChrisKapffer.Mobile;

/// <summary>
/// Behaviour to atomatically show a popup to ask the user if he/she likes to rate this app.
/// You can specifiy the time interval when to show this message again if user prosponed the question.
/// </summary>
public class RateMeDialog : MonoBehaviour {
	
    /// <summary>
    /// A simple (serializable) representation of a localized message. Contains a headline, text content and three buttons (yes, no, later).
    /// </summary>
    [System.Serializable]
	public class Message {
		public SystemLanguage language;
		public string headline;
		public string text;
		public string yes;
		public string no;
		public string later;

		public Message(SystemLanguage language, string headline, string text, string yes, string no, string later) {
			this.language = language;
			this.headline = headline;
			this.text = text;
			this.yes = yes;
			this.no = no;
			this.later = later;
		}
	}

    /// <summary>
    /// App identifier. Used to compose the app store link.
    /// </summary>
	public string iOS_appId;
    /// <summary>
    /// App name. Used to compose the play store link.
    /// </summary>
	public string android_appName;
    /// <summary>
    /// Human readable name of the app
    /// </summary>
	public string appDisplayName = "My cool app";

    /// <summary>
    /// If true the user will be asked to rate the app on start up (only if he's been using it for some amount of time).
    /// Set it to false to avoid the default behaviour. You can always call ShowRatingDialog directly, whenever you like.
    /// </summary>
	public bool askOnStartUp = true;
    /// <summary>
    /// Specify how many minutes the user has to spent within the app before prompted with the dialog for the first time.
    /// </summary>
	public int askFirstAfterMinutes = 30;
    /// <summary>
    /// Specify how many minutes (in game, not real time) to wait to ask the user again, if he/she postponed his/her decision.
    /// </summary>
	public int askAgainAfterMinutes = 60;

    /// <summary>
    /// A list of predefined rate me dialogs in some of the most common languages. Can be modified from within the editor.
    /// Be sure to include the spaceholders for the app name {0} and new line character {1}
    /// </summary>
	public List<Message> messages = new List<Message>() {
		new Message(SystemLanguage.English,    "Do you like this game?",    "If you enjoy using {0}, would you mind taking a moment to rate it? It won't take more than a minute. Thanks for your support!",                                         "Rate{1}{0}",      "No, thanks",    "Remind me later"),
		new Message(SystemLanguage.Spanish,    "¿Te gusta este juego?",     "Si le gusta utilizar {0}, ¿le importaría valorarlo? No le llevará más de un minuto. ¡Gracias por su colaboración!",                                                     "Valorar{1}{0}",   "No, gracias",   "Recordar más tarde"),
		new Message(SystemLanguage.Portuguese, "Você gosta deste jogo?",    "Se você gosta de usar o {0}, que tal avaliá-lo? Não levará mais de um minuto. Agradecemos o seu apoio!",                                                                "Avaliar o{1}{0}", "Não, obrigado", "Mais tarde"),
		new Message(SystemLanguage.French,     "Vous aimez ce jeu?",        "Si vous aimez {0}, voulez-vous prendre un moment pour l'évaluer? Cela ne vous prendra pas plus d'une minute. Merci de votre soutien!",                                  "Évaluer{1}{0}",   "Non, merci",    "Me rappeler plus tard"),
		new Message(SystemLanguage.German,     "Gefällt dir dieses Spiel?", "Wenn dir {0} Spaß macht, würdest du dir einen Augenblick Zeit nehmen, um es zu bewerten? Es dauert nicht länger als eine Minute. Vielen Dank für deine Unterstützung!", "Bewerte{1}{0}",   "Nein, danke",   "Erinnere mich später"),
		new Message(SystemLanguage.Italian,    "Ti piace questo gioco?",    "Se ti piace {0}, perché non dedichi qualche istante a darne una valutazione? Non richiederà più di un minuto. Grazie per il supporto!",                                 "Valuta{1}{0}",    "No, grazie",    "Ricordamelo più tardi"),
		new Message(SystemLanguage.Russian,    "Нравится ли вам эта игра?", "Если Вам нравится {0}, пожалуйста, поставьте свою оценку. Это займет у Вас не больше одной минуты. Спасибо за поддержку!",                                              "Оценить{1}{0}",   "Нет, спасибо",  "Напомнить позже"),
		new Message(SystemLanguage.Chinese,    "你喜欢这个游戏吗?",            "如果你喜欢使用{0}，你介意花一点时间给它评分吗？不会超过一分钟。感谢您的支持!", "给 {0}{1}评分",   "不，谢谢",        "稍后提醒我"),
		new Message(SystemLanguage.Korean,     "이 게임을 좋아 합니까?",         "{0} 사용이 맘에 드셨나요? 잠시만 시간을 내서 평가를 부탁드리겠습니다. 감사합니다!",   "{0}{1}평가하기", "평가하지 않겠습니다", "다음에 평가하겠습니다"),
		new Message(SystemLanguage.Japanese,   "このゲームは好きですか?",       "楽しんでいただけたならば簡単なご意見をお願いします。{0}を評価しますか",       "{0}{1}の評価",   "いいえ",          "後で見る"),
	};

    /// <summary>
    /// Use this language, if there is no translation for the user's system language.
    /// </summary>
	public SystemLanguage defaultLanguage = SystemLanguage.English;

	private Message defaultMessage;
	private Action onClose = null;

    public enum RatingState {
		NotAskedYet,
		Pending,
		Done,
		Declined,
	}

    public RatingState State {
		get {
			return (RatingState)PlayerPrefs.GetInt("RatingState", 0);
		}
		private set {
			int rating = PlayerPrefs.GetInt("RatingState", 0);
			if (rating != (int)value) {
				PlayerPrefs.SetInt("RatingState", (int)value);
			}
		}
	}

	private float lastUpdateTime;
	private float TimeSinceLastAsked {
		get {
			float tmpValue = PlayerPrefs.GetFloat("TimeSinceLastAsked", 0);
			tmpValue += Time.time - lastUpdateTime;
			TimeSinceLastAsked = tmpValue;
			return tmpValue;
		}
		set {
			PlayerPrefs.SetFloat("TimeSinceLastAsked", value);
			lastUpdateTime = Time.time;
		}
	}

#if UNITY_EDITOR
    /// <summary>
    /// Reset the player prefs value for testing (can do this only in play mode)
    /// </summary>
	public bool resetOnStart = true;
#endif

	void Start () {
#if UNITY_EDITOR
		if (resetOnStart) {
			State = RatingState.NotAskedYet;
		}
#endif
        // find the message, which uses the default language, be angry if there is no result
		defaultMessage = messages.Find(m => m.language == defaultLanguage);
		if (defaultMessage == null) {
			Debug.LogError("No Message specified for language: " + defaultLanguage.ToString());
		}

		if (askOnStartUp) {
			ShowRatingDialog();
		}
	}

	void OnApplicationPause() {
        // we need this to update time values
		ShouldShowDialog();

	}

    /// <summary>
    /// UnityEvents don't like optional parameters
    /// </summary>
    public void ShowOnButtonPress() {
        ShowRatingDialog(0, null, true);
    }
	
    /// <summary>
    /// Call this to show a rate me dialog to the user.
    /// </summary>
    /// <param name="delay">(optional) how long to wait (in seconds) after calling this method to actually show the dialog.</param>
    /// <param name="onClose">(optional) delegate to be invoked after dialog was closed.</param>
    /// <param name="force">(optional) ignore current state and show the dialog even if user said no before (use it for testing only. Nobody likes apps that keep on asking!</param>
    public void ShowRatingDialog(float delay = 0, System.Action onClose = null, bool force = false) {
        if (!ShouldShowDialog() && !force) {
			return;
		}
		this.onClose = onClose;
		Message message = GetMessage();
        // we don't want new lines on iPhon TODO: why? because it can't
		string newlineString = Application.platform == RuntimePlatform.IPhonePlayer ? " " : "\n";
		string[] buttons = { string.Format(message.yes, appDisplayName, newlineString), message.later, message.no };

		if (delay > 0) {
            // start coroutine to wait a few seconds until dialog gets shown
			StartCoroutine(ShowWithDelay(delay, message.headline, string.Format(message.text, appDisplayName), (int index) => {
				OnPopupClosed(index);
			}, buttons));
		} else {
            // show rate me dialog
			NativePopup.Show(message.headline, string.Format(message.text, appDisplayName), (int index) => {
				OnPopupClosed(index);
			}, buttons);
		}
	}

    /// <summary>
    /// Coroutine to be used if rate me dialog should show up with a delay
    /// </summary>
	private IEnumerator ShowWithDelay(float delay, string title, string message, Action<int> onClose, params string[] buttons) {
		yield return new WaitForSeconds(delay);
        // show rate me dialog
		NativePopup.Show(title, message, (int index) => {
			OnPopupClosed(index);
		}, buttons);	
	}

    /// <summary>
    /// Determines wether the dialog should be shown or not (depending on the time values and rating state).
    /// </summary>
    /// <returns><c>true</c>, if dialog should be shown, <c>false</c> otherwise.</returns>
	public bool ShouldShowDialog() {
		if (State == RatingState.Done || State == RatingState.Declined) {
			return false;
		}

		float askAfterElapsedTime = (State == RatingState.NotAskedYet ? askFirstAfterMinutes : askAgainAfterMinutes) * 60;
		return TimeSinceLastAsked > askAfterElapsedTime;
	}

    /// <summary>
    /// Gets the content for the rate me dialog in the same language as the system language.
    /// If there is no translation for the system language, the defaultmessage content will be used.
    /// </summary>
    /// <returns>The message in the appropriate language.</returns>
	private Message GetMessage() {
		Message result = messages.Find(m => m.language == Application.systemLanguage);
		if (result == null) {
			result = defaultMessage;
		}
		return result;
	}

    /// <summary>
    /// Opens the system's browser with the url to the app page of the appropriate store.
    /// </summary>
	private void VisitRatingSite() {
		string url = "";
#if UNITY_IOS
		url = "itms-apps://itunes.apple.com/us/app/apple-store/id" + iOS_appId + "?mt=8";
#elif UNITY_ANDROID
		url = "market://details?id=" + android_appName;
#endif
		Application.OpenURL(url);
	}

    /// <summary>
    /// Opens the rating site, if user wants to rate the app now.
    /// Sets the states necessary to determine if user should be asked again.
    /// </summary>
    /// <param name="buttonIndex">Index of pressed button.</param>
	private void OnPopupClosed(int buttonIndex) {
		switch(buttonIndex) {
			case 0:
				VisitRatingSite();
				State = RatingState.Done;
				break;
			case 2:
				State = RatingState.Declined;
				break;
			default:
				State = RatingState.Pending;
				break;
		}
		TimeSinceLastAsked = 0;
		if (onClose != null) {
            // invoke any action to be executed after the rate me dialog was shown
			onClose.Invoke();
		}
	}
}

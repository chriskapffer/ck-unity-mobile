using UnityEngine;
using System.Collections;
using ChrisKapffer.Mobile;

public class SharingExample : MonoBehaviour {

    /// <summary>
    /// Text that will be shared. Remember to include a placeholder {0} for the score!
    /// </summary>
    public string shareText = "I scored {0} points in #your_game_name_here.";

    /// <summary>
    /// URL to your game or company page. Leave it empty if you don't want it.
    /// </summary>
    public string shareUrl = "http://www.my-cool-company-or-game-page.com/";

    public int seperatorEveryNDigits = 3;

    public string seperator = " ";

    /// <summary>
    /// Use this in conjunction with OnShareButtonPressed which doesn't have a score parameter itself.
    /// </summary>
    public int CachedScore { get; set; }

	// Use this for initialization
	void Start () {
	
	}

    /// <summary>
    /// Converts an integer to string while inserting some seperator characters at specified interval (right to left)
    /// </summary>
    /// <returns>The score string.</returns>
    /// <param name="score">Score.</param>
    /// <param name="digits">Digits.</param>
    private string CreateScoreString(int score, int digits = 1, int seperatorInterval = 3) {
        int offset = seperatorInterval - ((digits - 1) % seperatorInterval);
        string tmp = score.ToString().PadLeft(digits, '0');
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < digits; i++) {
            sb.Append(tmp[i]);
            if ((i + offset) % seperatorInterval == 0) {
                sb.Append(seperator);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Gets the number of digits for a given int value.
    /// </summary>
    /// <returns>The number of digits.</returns>
    /// <param name="value">value to count digits from.</param>
    private int GetNumberOfDigits(int value) {
        if (value == 0) {
            return 1;
        }
        return (int)Mathf.Floor(Mathf.Log10(value) + 1);
    }

    /// <summary>
    /// Triggers the sharing mechanism on our device to share the current score and a screen shot.
    /// </summary>
    /// <param name="score">your current score.</param>
    public void ShareScore(int score) {
        // one could also caputre only parts of the screen
        Rect captureRect = new Rect(0, 0, Screen.width, Screen.height);
        // some editing to make the score look nice
        string scoreString = CreateScoreString(score, GetNumberOfDigits(score)).Trim();
        // share text and captured screen shot
        SharingManager.Share(string.Format(shareText, scoreString), shareUrl, true, captureRect);
    }

    /// <summary>
    /// Triggers the sharing mechanism on our device to share the current score and a screen shot.
    /// Hook this into a ui button's onClick event and remember to set cachedScore before the button gets pressed.
    /// </summary>
    public void OnShareButtonPressed() {
        ShareScore(CachedScore);
    }
}

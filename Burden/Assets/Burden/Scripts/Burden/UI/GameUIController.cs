using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject waitingUI = null;
    [SerializeField]
    private TextMeshProUGUI waitingText = null;

    [SerializeField]
    private Button exitButton = null;
    [SerializeField] Toggle audioToggle;
    public static bool toggledValue = false;
    public void ToggleAudio()
    {
        if (audioToggle != null)
        {
            toggledValue = audioToggle.isOn;
        }
        AudioListener.volume = toggledValue ? 0 : 1;
    }
    private void Start()
    {
        audioToggle.isOn = toggledValue;
        waitingUI.SetActive(true);
        waitingText.text = "...";
    }

    public void AllPlayersHaveJoined()
    {
        ToggleAudio();
        waitingUI.SetActive(false);
        GalleryGameManager.Instance.PlayerReadyToPlay();
    }
    public void SetWaitingText()
    {
        ToggleAudio();
        string text;
        if (ExampleManager.Instance.Avatar == Avatars.Mom)
        {
            text = $"{Avatars.Child},\nare you there?";

            TextToSpeech.Instance.SpeakText(Avatars.Mom, text.Replace('\n', ' '), true);
        }
        else
        {
            text = $"{Avatars.Mom},\nare you there?";

            TextToSpeech.Instance.SpeakText(Avatars.Child, text.Replace('\n', ' '), true);
        }
        waitingText.text = text;
    }
    public void AllowExit(bool allowed)
    {
        exitButton.gameObject.SetActive(allowed);
    }

    public void ButtonOnExit()
    {
        GalleryGameManager.Instance.OnQuitGame();
    }
}
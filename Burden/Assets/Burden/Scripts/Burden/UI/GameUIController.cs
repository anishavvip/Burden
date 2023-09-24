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

    public void ToggleAudio()
    {
        AudioListener.volume = audioToggle.isOn ? 0 : 1;
    }
    private void Start()
    {
        AudioListener.volume = 1;
        waitingUI.SetActive(true);
        waitingText.text = "...";
    }

    public void AllPlayersHaveJoined()
    {
        waitingUI.SetActive(false);
        GalleryGameManager.Instance.PlayerReadyToPlay();
    }
    public void SetWaitingText()
    {
        string text;
        if (ExampleManager.Instance.Avatar == Avatars.Mom)
        {
            text = $"{Avatars.Child},\nare you there?";

            TextToSpeech.Instance.SpeakText(Avatars.Mom, text.Replace('\n', ' '));
        }
        else
        {
            text = $"{Avatars.Mom},\nare you there?";

            TextToSpeech.Instance.SpeakText(Avatars.Child, text.Replace('\n', ' '));
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
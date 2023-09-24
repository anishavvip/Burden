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

    private void Start()
    {
        waitingUI.SetActive(true);
        waitingText.text = "...";
    }
    public void UpdatePlayerReadiness(bool showButton)
    {

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
            text = $"{Avatars.Child},\n are you there?";
        else 
            text = $"{Avatars.Mom},\n are you there?";
        waitingText.text = text;
    }
    public void AllowExit(bool allowed)
    {
        exitButton.gameObject.SetActive(allowed);
    }

    public void ButtonOnReady()
    {

    }

    public void ButtonOnExit()
    {
        GalleryGameManager.Instance.OnQuitGame();
    }
}
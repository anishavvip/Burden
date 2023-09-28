using System.Threading.Tasks;
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

    [SerializeField] GameObject MomTask, ChildTask;
    public static GameUIController Instance;

    PlayerController player;
    int counterTaskDisplay = 0;
    private void Awake()
    {
        Instance = this;
    }
    public async void AlertRepeatingMomTask()
    {
        counterTaskDisplay++;
        MomTask.SetActive(false);
        await Task.Delay(25000 * counterTaskDisplay);
        MomTask.SetActive(true);
    }
    public async void AlertRepeatingChildTask()
    {
        counterTaskDisplay++;
        ChildTask.SetActive(false);
        await Task.Delay(25000 * counterTaskDisplay);
        ChildTask.SetActive(true);
    }

    public void MomTaskDisplay()
    {
        MomTask.SetActive(true);
    }
    public void ChildTaskDisplay()
    {
        ChildTask.SetActive(true);
    }

    public void ToggleAudio()
    {
        if (audioToggle != null)
        {
            toggledValue = audioToggle.isOn;
        }
        AudioListener.volume = toggledValue ? 0 : 1;
    }

    public void Resume()
    {
        player.SetPause(false);
    }
    private void Start()
    {
        audioToggle.isOn = toggledValue;
        waitingUI.SetActive(true);
        waitingText.text = "...";
        player = ExampleManager.GetPlayer();
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
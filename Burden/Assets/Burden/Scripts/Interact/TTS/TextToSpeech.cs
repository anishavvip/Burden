using ReadSpeaker;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TextToSpeech : MonoBehaviour
{
    [Serializable]
    public class AvatarSpeech
    {
        public TTSSpeaker speaker;
        [TextArea(1, 2)]
        public string[] intro;
    }
    int attemptMomCount = 0, attemptChildCount = 0;
    [SerializeField] AvatarSpeech mom, child;
    public static TextToSpeech Instance;
    [SerializeField] TextMeshProUGUI speechText;

    [TextArea(1, 20)]
    public string dummyText;
    [SerializeField] Image playSpeechIndicator;
    int count = 0;
    int startIndex = 0;
    TTSSpeaker currentSpeaker;
    Avatars currentAvatar;
    Action action;
    string[] speechToDisplayList;
    string[] speechList;
    [SerializeField] Sprite play, alert;
    bool isDisappear;
    [SerializeField] GameObject canvas;
    PlayerController player;

    IEnumerator TypeWriter(TextMeshProUGUI textComponent, string stringToDisplay, bool isDisappear)
    {
        for (int i = startIndex; i < stringToDisplay.Length; i++)
        {
            if (stringToDisplay[i] == '*')
            {
                startIndex = i + 1;
                yield return new WaitForSecondsRealtime(1);
            }
            textComponent.text = stringToDisplay.Substring(startIndex, i - startIndex + 1);
            textComponent.text = textComponent.text.Replace("*", "");
            // Retrieves part of the text from string[0] to string[i]
            // We wait x seconds between characters before displaying them

            yield return new WaitForSecondsRealtime(0.1f);
        }
        if (isDisappear)
        {
            yield return new WaitForSecondsRealtime(1);
            textComponent.text = "";
            Refresh();
        }
    }
    public void Hide()
    {
        canvas.SetActive(false);
    }
    public void Show()
    {
        canvas.SetActive(true);
    }
    public void Refresh()
    {
        TTS.InterruptAll();
        StopAllCoroutines();
        speechText.text = "";
        count = 0;
        startIndex = 0;
        speechList = new string[] { };
        speechToDisplayList = new string[] { };
        if (playSpeechIndicator != null)
            playSpeechIndicator.gameObject.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
    }
    public void Hover(string name)
    {
        Refresh();
        name = name.ToLower();
        string speech = "";
        if (name.Contains(Avatars.Mom.ToString().ToLower()))
        {
            attemptMomCount++;
            if (attemptMomCount == 1)
            {
                if (playSpeechIndicator != null)
                {
                    playSpeechIndicator.gameObject.SetActive(true);
                    playSpeechIndicator.sprite = play;
                }
                speech = mom.intro[Random.Range(0, mom.intro.Length)];
                Debug.Log(speech);
                TTS.Say(speech, mom.speaker, TextType.Normal);
            }
        }
        else if (name.Contains(Avatars.Child.ToString().ToLower()))
        {
            attemptChildCount++;
            if (attemptChildCount == 1)
            {
                if (playSpeechIndicator != null)
                {
                    playSpeechIndicator.gameObject.SetActive(true);
                    playSpeechIndicator.sprite = play;
                }
                speech = child.intro[Random.Range(0, child.intro.Length)];
                Debug.Log(speech);
                TTS.Say(speech, child.speaker, TextType.Normal);
            }
        }
        if (speech != "")
        {
            StartCoroutine(TypeWriter(speechText, speech, true));
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        TTS.Init();
    }

    private void Update()
    {
        if (player != null)
        {
            if (player.hasGameBegun)
                if (!player.isPaused)
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        PlayNextLine();
                    }
        }
        else
        {
            if (SceneManager.GetActiveScene().name != "Lobby")
                player = ExampleManager.GetPlayer();
        }
        if (currentSpeaker != null)
        {
            playSpeechIndicator.gameObject.SetActive(speechText.text != "");
        }
    }

    public void PlayNextLine()
    {
        if (speechList != null)
        {
            if (speechList.Length > 1 && count < speechList.Length)
            {
                if (playSpeechIndicator != null)
                {
                    playSpeechIndicator.gameObject.SetActive(true);
                    playSpeechIndicator.sprite = alert;
                }
            }

            StartCoroutine(WaitForSpeech(speechList, currentAvatar, currentSpeaker, speechToDisplayList, isDisappear, action));
        }
    }

    public void SpeakText(Avatars speaker, string speech, bool isDisappear, Action action = null)
    {
        Refresh();
        if (speech == "")
        {
            if (playSpeechIndicator != null)
            {
                playSpeechIndicator.gameObject.SetActive(false);
            }
            return;
        }
        if (isDisappear)
        {
            if (playSpeechIndicator != null)
            {
                playSpeechIndicator.gameObject.SetActive(true);
                playSpeechIndicator.sprite = play;
            }
        }
        string speechToDisplay = speech;
        speechToDisplay = speechToDisplay.Replace("<v1>", "");
        speechToDisplay = speechToDisplay.Replace("</v1>", "");
        speechToDisplay = speechToDisplay.Replace("<v2>", "");
        speechToDisplay = speechToDisplay.Replace("</v2>", "");
        speechToDisplay = speechToDisplay.Replace("<e1>", "");
        speechToDisplay = speechToDisplay.Replace("</e1>", "");
        speechToDisplay = speechToDisplay.Replace("<br>", "*");
        speechToDisplayList = speechToDisplay.Split('\n');

        speech = speech.Replace("<v1>", "<prosody rate=\"-5%\">");
        speech = speech.Replace("</v1>", "</prosody>");

        speech = speech.Replace("<v2>", "<prosody rate=\"10%\">");
        speech = speech.Replace("</v2>", "</prosody>");

        speech = speech.Replace("<e1>", "<emphasis level=\"strong\">");
        speech = speech.Replace("</e1>", "</emphasis>");
        speech = speech.Replace("<br>", "<break time=\"1s\"/>");
        speechList = speech.Split('\n');
        if (speaker == Avatars.Mom)
        {
            currentSpeaker = mom.speaker;
            currentAvatar = Avatars.Mom;
        }
        else
        {
            currentSpeaker = child.speaker;
            currentAvatar = Avatars.Child;
        }
        this.action = action;
        this.isDisappear = isDisappear;
        PlayNextLine();
    }
    IEnumerator WaitForSpeech(string[] speechList, Avatars currentAvatar, TTSSpeaker currentSpeaker, string[] speechToDisplayList, bool isDisappear, Action action = null)
    {
        if (speechList != null)
        {
            if (speechList.Length > 0)
            {
                if (!currentSpeaker.audioSource.isPlaying)
                {
                    if (count == speechList.Length && count != 0)
                    {
                        action?.Invoke();
                        Refresh();
                        yield return null;
                    }
                    if (count < speechList.Length)
                    {
                        startIndex = 0;
                        SpeakLine(currentAvatar, count, speechList, speechToDisplayList, isDisappear);
                        yield return new WaitForSecondsRealtime(2);
                        count++;
                    }
                }
            }
        }
    }

    private void SpeakLine(Avatars speaker, int count, string[] speechList, string[] speechToDisplayList, bool isDisappear)
    {
        if (speechList[count] == "") return;
        TTSSpeechCharacteristics characteristics;
        if (speaker == Avatars.Mom)
        {
            characteristics = mom.speaker.characteristics;
            TTS.Say(speechList[count], characteristics, mom.speaker.audioSource, TextType.SSML);
        }
        else
        {
            characteristics = child.speaker.characteristics;
            TTS.Say(speechList[count], characteristics, child.speaker.audioSource, TextType.SSML);
        }
        StartCoroutine(TypeWriter(speechText, speechToDisplayList[count], isDisappear));
    }
}

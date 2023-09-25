using ReadSpeaker;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TextToSpeech : MonoBehaviour
{
    [Serializable]
    public class AvatarSpeech
    {
        public TTSSpeaker speaker;
        [TextAreaAttribute(1, 2)]
        public string[] intro;
    }
    int attemptMomCount = 0, attemptChildCount = 0;
    [SerializeField] AvatarSpeech mom, child;
    public static TextToSpeech Instance;
    [SerializeField] TextMeshProUGUI speechText;
    WaitForSeconds _delayBetweenCharactersYieldInstruction;
    int startIndex = 0;
    bool isSpeaking = false;
    [TextAreaAttribute(1, 20)]
    public string dummyText;

    public void StartTypeWriterOnText(TextMeshProUGUI textComponent, string stringToDisplay, Action action = null, float delayBetweenCharacters = 0.1f)
    {
        isSpeaking = true;
        startIndex = 0;
        StopAllCoroutines();
        StartCoroutine(TypeWriterCoroutine(textComponent, stringToDisplay, delayBetweenCharacters, action));
    }

    IEnumerator TypeWriterCoroutine(TextMeshProUGUI textComponent, string stringToDisplay, float delayBetweenCharacters, Action action = null)
    {
        // Cache the yield instruction for GC optimization
        _delayBetweenCharactersYieldInstruction = new WaitForSeconds(delayBetweenCharacters);
        // Iterating(looping) through the string's characters

        for (int i = startIndex; i < stringToDisplay.Length; i++)
        {
            if (stringToDisplay[i] == '\n')
            {
                startIndex = i + 1;
                yield return new WaitForSecondsRealtime(2f);
            }

            textComponent.text = stringToDisplay.Substring(startIndex, i - startIndex + 1);
            // Retrieves part of the text from string[0] to string[i]
            // We wait x seconds between characters before displaying them
        }

        yield return new WaitForSecondsRealtime(3f);
        textComponent.text = "";
        isSpeaking = false;
        action?.Invoke();
    }
    public void StopAudio()
    {
        isSpeaking = false;
        TTS.InterruptAll();
        StopAllCoroutines();
        speechText.text = "";
    }
    private void Awake()
    {
        Instance = this;
    }
    public void Hover(string name)
    {
        name = name.ToLower();
        string speech = "";
        if (name.Contains(Avatars.Mom.ToString().ToLower()))
        {
            attemptMomCount++;
            if (attemptMomCount == 1)
            {
                speech = mom.intro[Random.Range(0, mom.intro.Length)];
                TTS.Say(speech, mom.speaker);
            }
        }
        else if (name.Contains(Avatars.Child.ToString().ToLower()))
        {
            attemptChildCount++;
            if (attemptChildCount == 1)
            {
                speech = child.intro[Random.Range(0, child.intro.Length)];
                TTS.Say(speech, child.speaker);
            }
        }
        StartTypeWriterOnText(speechText, speech);

    }
    // Start is called before the first frame update
    void Start()
    {
        TTS.Init();
        SpeakText(Avatars.Mom, dummyText, null);
    }
    public async void SpeakText(Avatars speaker, string speech, Action action = null)
    {
        while (isSpeaking)
        {
            await Task.Delay(1000);
        }
        if (!isSpeaking)
        {
            if (speech == "") return;

            string speechToDisplay = speech;
            speechToDisplay = speechToDisplay.Replace("<v1>", "");
            speechToDisplay = speechToDisplay.Replace("</v1>", "");
            speechToDisplay = speechToDisplay.Replace("<v2>", "");
            speechToDisplay = speechToDisplay.Replace("</v2>", "");
            speechToDisplay = speechToDisplay.Replace("<e1>", "");
            speechToDisplay = speechToDisplay.Replace("</e1>", "");
            speechToDisplay = speechToDisplay.Replace("<br>", "\n");

            speech = "<prosody volume=\"110%\"><emphasis level=\"none\">" + speech + "</emphasis></prosody>";
            speech = speech.Replace("<v1>", "<prosody volume=\"160%\">");
            speech = speech.Replace("</v1>", "</prosody>");

            speech = speech.Replace("<v2>", "<prosody volume=\"130%\">");
            speech = speech.Replace("</v2>", "</prosody>");

            speech = speech.Replace("<e1>", "<emphasis level=\"strong\">");
            speech = speech.Replace("</e1>", "</emphasis>");

            speech = speech.Replace("<br>", "<break/>");
            Debug.Log(speech);
            if (speaker == Avatars.Mom)
            {
                TTS.SayAsync(speech, mom.speaker, TextType.SSML);
            }
            else if (speaker == Avatars.Child)
            {
                TTS.SayAsync(speech, child.speaker, TextType.SSML);
            }
            StartTypeWriterOnText(speechText, speechToDisplay, action);
        }
    }
}

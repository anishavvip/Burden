using ReadSpeaker;
using System;
using System.Collections;
using System.Linq;
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

    [SerializeField] AvatarSpeech mom, child;
    public static TextToSpeech Instance;
    [SerializeField] TextMeshProUGUI speechText;
    WaitForSeconds _delayBetweenCharactersYieldInstruction;
    int startIndex = 0;
    char[] pauseChars = new char[] { ',', '.', '?', '-', '!', ':', ';' };

    public void StartTypeWriterOnText(TextMeshProUGUI textComponent, string stringToDisplay, Action action = null, float delayBetweenCharacters = 0.08f)
    {
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
            if (stringToDisplay[i] == '*' || stringToDisplay[i] == '\n')
            {
                startIndex = i + 1;

                if (stringToDisplay[i] == '\n')
                    yield return new WaitForSeconds(delayBetweenCharacters * 18f);
                else
                    yield return _delayBetweenCharactersYieldInstruction;
            }

            // Retrieves part of the text from string[0] to string[i]
            textComponent.text = stringToDisplay.Substring(startIndex, i - startIndex + 1);
            // We wait x seconds between characters before displaying them

            if (pauseChars.Contains(stringToDisplay[i]))
                yield return new WaitForSeconds(delayBetweenCharacters * 1.8f);
            yield return _delayBetweenCharactersYieldInstruction;
        }
        textComponent.text = "";
        action?.Invoke();
    }
    public void StopAudio()
    {
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
        StopAudio();
        name = name.ToLower();
        string speech = "";
        if (name.Contains(Avatars.Mom.ToString().ToLower()))
        {
            speech = mom.intro[Random.Range(0, mom.intro.Length)];
            TTS.Say(speech, mom.speaker);
        }
        else if (name.Contains(Avatars.Child.ToString().ToLower()))
        {
            speech = child.intro[Random.Range(0, child.intro.Length)];
            TTS.Say(speech, child.speaker);
        }
        StartTypeWriterOnText(speechText, speech);
    }
    // Start is called before the first frame update
    void Start()
    {
        TTS.Init();
        DontDestroyOnLoad(gameObject);
    }
    public void SpeakText(Avatars speaker, string speech, Action action = null)
    {
        StopAudio();
        if (speaker == Avatars.Mom)
        {
            TTS.Say(speech, mom.speaker);
        }
        else if (speaker == Avatars.Child)
        {
            TTS.Say(speech, child.speaker);
        }
        StartTypeWriterOnText(speechText, speech, action);
    }
}

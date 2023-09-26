using System;
using UnityEngine;

public class TriggeredSpeech : MonoBehaviour
{
    [Serializable]
    public class Type
    {
        [TextAreaAttribute(1, 20)]
        public string speech;
        [TextAreaAttribute(1, 20)]
        public string lockedSpeech;
        public bool isLocked = false;
    }
    public Type mom, child;
    PlayerController player;
    [HideInInspector] public bool speechCompleted = true;

    private void Start()
    {
        player = ExampleManager.GetPlayer();
    }
    public void Speech(string avatar)
    {
        if (player.hasGameBegun)
        {
            if (avatar.ToString() == player.prefabName)
            {
                speechCompleted = false;
                //Only Child is locked 
                if (avatar == Avatars.Child.ToString())
                {
                    if (child.isLocked)
                    {
                        TextToSpeech.Instance.SpeakText(Avatars.Child, child.lockedSpeech, false, delegate { speechCompleted = true; });
                    }
                    else
                        TextToSpeech.Instance.SpeakText(Avatars.Child, child.speech, true, delegate { speechCompleted = true; });
                }
                else
                {
                    TextToSpeech.Instance.SpeakText(Avatars.Mom, mom.speech, true, delegate { speechCompleted = true; });
                }
            }
        }
    }
}

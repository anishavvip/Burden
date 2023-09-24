using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    [TextAreaAttribute(1, 20)]
    public string introSpeech, doorBellSpeech;
    [SerializeField] AudioClip doorBell;
    [SerializeField] AudioSource audioSource;
    PlayerController player, opponent;
    public Avatars avatar;
    int rings = 2;

    private void Start()
    {
        opponent = ExampleManager.GetOpponent();
        player = ExampleManager.GetPlayer();
    }
    public void Intro()
    {
        if (player.hasGameBegun)
        {
            if (avatar.ToString() == player.prefabName)
            {
                if (avatar == Avatars.Child)
                    TextToSpeech.Instance.SpeakText(Avatars.Child, introSpeech, DoorBellSpeech);
                else
                    TextToSpeech.Instance.SpeakText(Avatars.Mom, introSpeech, DoorBellSound);
            }
        }
    }
    public async void PlayDoorRing()
    {
        for (int i = 0; i < rings; i++)
        {
            audioSource.PlayOneShot(doorBell);
            await Task.Delay(100);
        }
    }
    public async void DoorBellSound()
    {
        ExampleManager.CustomServerMethod("DoorBell", new object[] { });
        for (int i = 0; i < rings; i++)
        {
            audioSource.PlayOneShot(doorBell);
            await Task.Delay(100);
        }
        if (avatar.ToString() == player.prefabName)
        {
            if (avatar == Avatars.Mom)
            {
                TextToSpeech.Instance.SpeakText(Avatars.Mom, doorBellSpeech);
            }
        }
    }
    async void DoorBellSpeech()
    {
        await Task.Delay(1500);

        if (avatar.ToString() == player.prefabName)
        {
            if (avatar == Avatars.Child)
                TextToSpeech.Instance.SpeakText(Avatars.Child, doorBellSpeech);
        }
    }
}

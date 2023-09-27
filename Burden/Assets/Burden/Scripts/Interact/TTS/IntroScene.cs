using System.Threading.Tasks;
using UnityEngine;

public class IntroScene : MonoBehaviour
{
    [TextAreaAttribute(1, 20)]
    public string introSpeech, doorBellSpeech;
    [SerializeField] AudioClip doorBell;
    [SerializeField] AudioSource audioSource;
    PlayerController player;
    public Avatars avatar;
    int rings = 2;
    [SerializeField] GameObject parcel;
    HiddenKeyData hiddenKeyData = new HiddenKeyData();
    public static bool hasDoorBellRung = false;

    private void Start()
    {
        hasDoorBellRung = false;
        parcel.SetActive(false);
        player = ExampleManager.GetPlayer();
    }
    public void Intro()
    {
        if (player.hasGameBegun)
        {
            if (avatar.ToString() == player.prefabName)
            {
                if (avatar == Avatars.Child)
                    TextToSpeech.Instance.SpeakText(Avatars.Child, introSpeech, false, SendDoorBellData);
                else
                    TextToSpeech.Instance.SpeakText(Avatars.Mom, introSpeech, false, SendDoorBellData);
            }
        }
    }
    private void OnEnable()
    {
        ExampleRoomController.onBellRing += OnBellRing;
    }
    private void OnDisable()
    {
        ExampleRoomController.onBellRing -= OnBellRing;
    }
    private void OnBellRing(HiddenKeyData hiddenKeyData)
    {
        PlayDoorRing(hasDoorBellRung, hiddenKeyData.name);
    }

    async void NotifyUsers(string name)
    {
        if (name == player.prefabName)
        {
            if (avatar.ToString() == player.prefabName)
            {
                if (name == Avatars.Child.ToString())
                {
                    await Task.Delay(1000);
                    TextToSpeech.Instance.SpeakText(Avatars.Child, doorBellSpeech, false, IntroCompletionStatus);
                }
                else
                {
                    TextToSpeech.Instance.SpeakText(Avatars.Mom, doorBellSpeech, false, IntroCompletionStatus);
                }
            }
        }
    }

    async void PlayDoorRing(bool hasDoorBellRung, string name)
    {
        if (!hasDoorBellRung)
        {
            parcel.SetActive(true);
            for (int i = 0; i < rings; i++)
            {
                audioSource.PlayOneShot(doorBell);
                await Task.Delay(100);
            }
        }
        NotifyUsers(name);
    }
    void SendDoorBellData()
    {
        hiddenKeyData.name = player.prefabName;
        ExampleManager.CustomServerMethod("doorBell", new object[] { hiddenKeyData });
        ExampleManager.CustomServerMethod("doorBellRing", new object[] { });
    }

    private void IntroCompletionStatus()
    {
        if (player.prefabName == Avatars.Child.ToString())
            GalleryGameManager.Instance.child.isIntroDone = true;
        else
            GalleryGameManager.Instance.mom.isIntroDone = true;
    }
}

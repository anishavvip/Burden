using System;
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
        if (player.hasGameBegun && !player.isPaused)
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
        hasDoorBellRung = false;
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
                    TextToSpeech.Instance.SpeakText(Avatars.Child, doorBellSpeech, false, DoorBellTriggerIntroCompletionStatus);
                }
                else
                {
                    TextToSpeech.Instance.SpeakText(Avatars.Mom, doorBellSpeech, false, DoorBellTriggerIntroCompletionStatus);
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
    async void SendDoorBellData()
    {
        await Task.Delay(2000);
        hiddenKeyData.name = player.prefabName;
        ExampleManager.CustomServerMethod("doorBell", new object[] { hiddenKeyData });
        ExampleManager.CustomServerMethod("doorBellRing", new object[] { });
    }

    private async void DoorBellTriggerIntroCompletionStatus()
    {
        player.SetPause(true);
        if (player.prefabName == Avatars.Child.ToString())
        {
            GalleryGameManager.Instance.child.isIntroDone = true;
            await Task.Delay(2000);
            ShowTask(Avatars.Child);
        }
        else
        {
            GalleryGameManager.Instance.mom.isIntroDone = true;
            await Task.Delay(2000);
            ShowTask(Avatars.Mom);
        }
    }

    private void ShowTask(Avatars child)
    {
        if (child == Avatars.Child)
        {
            GameUIController.Instance.ChildTaskDisplay();
        }
        else
        {
            GameUIController.Instance.MomTaskDisplay();
        }
    }
}

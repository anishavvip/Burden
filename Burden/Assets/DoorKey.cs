using UnityEngine;

public class DoorKey : MonoBehaviour
{
    [SerializeField] Interactable door;
    [SerializeField] AudioClip doorOpen;
    PlayerController player;
    [TextAreaAttribute(1, 20)]
    public string speech, preSpeech;

    private void Start()
    {
        player = ExampleManager.GetPlayer();
    }
    private void OnMouseOver()
    {
        if (player == null) return;
        if (!player.hasGameBegun) return;
        if (player.prefabName == Avatars.Mom.ToString()) return;

        if (player.SyncData.rightClicked)
        {
            Debug.Log("right clicked");
            if (player.isIntroDone)
            {
                TextToSpeech.Instance.StopAudio();
                if (player.didPlayerTryUnlocking)
                {
                    TextToSpeech.Instance.SpeakText(Avatars.Child, speech);
                }
                else
                {
                    TextToSpeech.Instance.SpeakText(Avatars.Child, preSpeech);
                }
                door.counter++;
                door.isLocked = false;
                door.Interact(player, false);
                ItemDetails itemDetails = new ItemDetails();
                itemDetails.name = ExampleManager.Instance.Avatar.ToString();
                itemDetails.itemName = gameObject.name;

                ExampleManager.CustomServerMethod("itemInteract", new object[] { itemDetails });
                AudioSource.PlayClipAtPoint(doorOpen, door.transform.position);

                gameObject.SetActive(false);
            }
        }
    }
}

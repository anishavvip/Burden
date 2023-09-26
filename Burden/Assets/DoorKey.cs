using System.Linq;
using UnityEngine;

public class DoorKey : MonoBehaviour
{
    [SerializeField]
    Transform[] spawnPos;
    [SerializeField] Interactable door;
    [SerializeField] AudioClip doorOpen;
    PlayerController player;
    [TextAreaAttribute(1, 20)]
    public string speech, preSpeech;
    bool gotPos = false;
    string[] indices;
    string spot;

    private void Start()
    {
        player = ExampleManager.GetPlayer();
        if (player.prefabName == Avatars.Child.ToString())
        {
            int index = Random.Range(0, spawnPos.Length);
            spot = PlayerPrefs.GetString("Spot");
            if (spot != "")
            {
                indices = spot.Split('_');
                if (indices.Length <= spawnPos.Length)
                {
                    while (gotPos == false)
                    {
                        int _index = Random.Range(0, spawnPos.Length);
                        gotPos = Hide(_index);
                    }
                }
                else
                {
                    PlayerPrefs.SetString("Spot", "");
                    indices = new string[] { };
                    spot = "";
                    Hide(index);
                }
            }
            else
            {
                Hide(index);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private bool Hide(int index)
    {
        if (indices != null)
        {
            foreach (var item in indices)
            {
                if (item != null)
                {
                    if (item.Equals(index.ToString()))
                    {
                        return false;
                    }
                }
            }
        }
        transform.position = spawnPos[index].position;
        transform.localRotation = spawnPos[index].localRotation;
        PlayerPrefs.SetString("Spot", spot + "_" + index);

        spot = PlayerPrefs.GetString("Spot");
        return true;
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
                    TextToSpeech.Instance.SpeakText(Avatars.Child, speech, false);
                }
                else
                {
                    TextToSpeech.Instance.SpeakText(Avatars.Child, preSpeech, false);
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

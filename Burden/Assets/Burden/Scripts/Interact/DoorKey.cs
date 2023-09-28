using UnityEngine;
using Random = UnityEngine.Random;

public class DoorKey : MonoBehaviour
{
    public GameObject key;
    [SerializeField]
    Transform[] spawnPos;
    public Interactable door;
    [SerializeField] AudioClip doorOpen;
    PlayerController player;
    [TextAreaAttribute(1, 20)]
    public string preSpeech;
    bool gotPos = false;
    string[] indices;
    string spot;
    public static DoorKey Instance;
    int index = -1;
    bool initialDataShared = false;
    KeyData keyData = new KeyData();
    public static bool unlocked = false;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        unlocked = false;
        initialDataShared = false;
        GetHidingSpot();
    }
    private void GetHidingSpot()
    {
        player = ExampleManager.GetPlayer();
        Debug.Log("setting");
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
        SetKey(index);
        PlayerPrefs.SetString("Spot", spot + "_" + index);
        Debug.Log("Spot:" + spot);
        spot = PlayerPrefs.GetString("Spot");
        this.index = index;
        return true;
    }

    private void Update()
    {
        if (player != null)
            if (!player.isPaused)
                if (player.hasGameBegun && !initialDataShared && player.prefabName == Avatars.Child.ToString())
                {
                    if (unlocked && !door.isLocked || InteractablesHelper.allUnlocked) key.SetActive(false);

                    if (index != -1)
                    {
                        keyData.index = index;
                        Debug.Log("spot" + index);
                        ExampleManager.CustomServerMethod("setKey", new object[] { keyData });
                        initialDataShared = true;
                    }
                }
    }
    private void OnEnable()
    {
        ExampleRoomController.onDoorKeyHide += DoorKeyHide;
    }
    private void OnDisable()
    {
        ExampleRoomController.onDoorKeyHide -= DoorKeyHide;
        unlocked = false;
    }
    private void DoorKeyHide(KeyData keyData)
    {
        SetKey(keyData.index);
    }

    public void SetKey(int index)
    {
        if (index == -1)
        {
            key.SetActive(false);
            return;
        }
        Debug.Log(index);
        key.SetActive(true);
        transform.position = spawnPos[index].position;
        transform.localRotation = spawnPos[index].localRotation;
    }

    private void OnMouseOver()
    {
        if (player == null) return;
        if (player.isPaused) return;
  
        if (!player.hasGameBegun) return;
        if (player.prefabName == Avatars.Mom.ToString()) return;

        if (player.SyncData.rightClicked && door.isLocked && !unlocked)
        {
            Debug.Log("right clicked");
            if (player.isIntroDone)
            {
                TextToSpeech.Instance.Refresh();
                if (player.didPlayerTryUnlocking && door.isLocked)
                {
                    TextToSpeech.Instance.SpeakText(Avatars.Child, preSpeech, false);
                }
            }

            KeyInteractedWith();
        }
    }

    private void KeyInteractedWith()
    {
        if (!door.isLocked && unlocked) return;

        door.counter++;
        door.isLocked = false;
        index = -1;
        unlocked = true;
        InteractablesHelper.allUnlocked = true;
        AudioSource.PlayClipAtPoint(doorOpen, door.transform.position);

        key.SetActive(false);
        keyData.index = index;
        ExampleManager.CustomServerMethod("setKey", new object[] { keyData });

        door.Interact(player, false, true);
    }
}

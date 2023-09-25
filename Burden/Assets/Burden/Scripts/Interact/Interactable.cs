using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    float range = 15f;
    bool open;
    Animator animator;
    PlayerController player;
    public bool isLocked = false;
    [HideInInspector] public int counter = 0;
    [SerializeField] TriggeredSpeech TriggeredSpeech;

    IEnumerator opening()
    {
        animator.Play("Opening");
        open = true;
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator closing()
    {
        animator.Play("Closing");
        open = false;
        yield return new WaitForSeconds(.5f);
    }
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }
        animator = GetComponent<Animator>();
        open = false;
        player = ExampleManager.GetPlayer();
        if (player.prefabName == Avatars.Mom.ToString())
        {
            isLocked = false;
        }
    }
    public void Interact(PlayerController Player, bool isLocked = false)
    {
        if (Player)
        {
            float dist = Vector3.Distance(Player.transform.position, transform.position);
            if (dist < range)
            {
                if (open == false)
                {
                    if (Player.SyncData.rightClicked)
                    {
                        if (Player.isIntroDone && counter == 0)
                        {
                            TriggeredSpeechPlayer(Player);
                        }

                        if (!isLocked)
                            StartCoroutine(opening());
                    }
                }
                else
                {
                    if (open == true)
                    {
                        if (Player.SyncData.rightClicked)
                        {
                            if (Player.isIntroDone && counter == 0)
                            {
                                TriggeredSpeechPlayer(Player);
                            }

                            if (!isLocked)
                                StartCoroutine(closing());
                        }
                    }

                }

            }
        }
    }

    private void TriggeredSpeechPlayer(PlayerController Player)
    {
        if (TriggeredSpeech != null)
        {
            if (TriggeredSpeech.speechCompleted)
            {
                if (Player.prefabName == Avatars.Child.ToString())
                    Player.didPlayerTryUnlocking = true;
                counter++;
                TriggeredSpeech.Speech(Player.prefabName);
            }
        }
    }

    private void OnMouseOver()
    {
        if (!player.hasGameBegun) return;

        Interact(player, isLocked);
        if (isLocked) return;
        ItemDetails itemDetails = new ItemDetails();
        itemDetails.name = ExampleManager.Instance.Avatar.ToString();
        itemDetails.itemName = gameObject.name;

        ExampleManager.CustomServerMethod("itemInteract", new object[] { itemDetails });
    }
}

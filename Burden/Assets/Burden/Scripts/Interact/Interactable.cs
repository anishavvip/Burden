using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    float range = 3f;
    bool open;
    Animator animator;
    PlayerController player;
    public bool isLocked = false;
    [HideInInspector] public int counter = 0;
    [SerializeField] TriggeredSpeech TriggeredSpeech;
    public float extent = 0.5f;
    public bool canPlaceObjectsInside = false;
    public enum DirectionOfPull { X, Z, negX, negZ }
    public DirectionOfPull direction;

    public IEnumerator Open()
    {
        Debug.Log(direction);
        switch (direction)
        {
            case DirectionOfPull.X:
            case DirectionOfPull.negX:
                yield return transform.DOLocalMove(Vector3.right * extent, 0.3f);
                break;
            case DirectionOfPull.Z:
                yield return transform.DOLocalMove(Vector3.forward * extent, 0.3f);
                break;
            case DirectionOfPull.negZ:
                yield return transform.DOLocalMove(Vector3.back * extent, 0.3f);
                break;
        }

        yield return new WaitForSeconds(.5f);
    }
    public IEnumerator Close()
    {
        yield return transform.DOLocalMove(Vector3.zero * extent, 0.3f);

        yield return new WaitForSeconds(.5f);
    }
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
        animator = GetComponent<Animator>();
        if (canPlaceObjectsInside)
        {
            animator.applyRootMotion = true;
            range /= 1.5f;
        }
        open = false;
        player = ExampleManager.GetPlayer();
        if (player.prefabName == Avatars.Mom.ToString())
        {
            isLocked = false;
        }
    }
    public void Interact(PlayerController Player, bool isLocked = false, bool sendData = false)
    {
        if (Player)
        {
            this.isLocked = isLocked;
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

                        if (InteractablesHelper.allUnlocked || Player.prefabName == Avatars.Mom.ToString() || (!isLocked && Player.prefabName == Avatars.Child.ToString()))
                        {
                            if (!canPlaceObjectsInside)
                            {
                                StartCoroutine(opening());
                            }
                            else
                            {
                                open = true;
                                StartCoroutine(Open());
                                return;
                            }
                            if (sendData)
                            {
                                SendData();
                            }
                        }
                    }
                }
                else
                {
                    if (open == true)
                    {
                        if (Player.SyncData.rightClicked)
                        {
                            if (!canPlaceObjectsInside)
                            {
                                StartCoroutine(closing());
                            }
                            else
                            {
                                open = false;
                                StartCoroutine(Close());
                                return;
                            }
                            if (sendData)
                            {
                                SendData();
                            }
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
        Interact(player, isLocked, true);
    }
    private void SendData()
    {
        ItemDetails itemDetails = new ItemDetails();
        itemDetails.name = player.prefabName;
        itemDetails.itemName = gameObject.name;
        itemDetails.isLocked = isLocked;
        ExampleManager.CustomServerMethod("itemInteract", new object[] { itemDetails });
    }
}

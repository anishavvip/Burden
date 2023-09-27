using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    float range = 10;
    bool open;
    Animator animator;
    PlayerController player;
    public bool isLocked = false;
    [HideInInspector] public int counter = 0;
    [SerializeField] TriggeredSpeech TriggeredSpeech;
    public float extent = 0.5f;
    [SerializeField] bool canPlaceObjectsInside = false;
    public enum DirectionOfPull { X, Z, negX, negZ }
    public DirectionOfPull direction;
    public void Open()
    {
        Debug.Log(direction);
        switch (direction)
        {
            case DirectionOfPull.X:
                transform.DOLocalMove(Vector3.right * extent, 0.3f);
                break;
            case DirectionOfPull.Z:
                transform.DOLocalMove(Vector3.forward * extent, 0.3f);
                break;
            case DirectionOfPull.negX:
                transform.DOLocalMove(-Vector3.right * extent, 0.3f);
                break;
            case DirectionOfPull.negZ:
                transform.DOLocalMove(-Vector3.forward * extent, 0.3f);
                break;
        }
    }
    public void Close()
    {
        transform.DOLocalMove(Vector3.zero * extent, 0.3f);
    }
    IEnumerator opening()
    {
        if (!canPlaceObjectsInside)
            animator.Play("Opening");
        else
            Open();
        open = true;
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator closing()
    {
        if (!canPlaceObjectsInside)
            animator.Play("Closing");
        else
            //Close();
        open = false;
        yield return new WaitForSeconds(.5f);
    }
    void Start()
    {
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
                            StartCoroutine(opening());
                    }
                }
                else
                {
                    if (open == true)
                    {
                        if (Player.SyncData.rightClicked)
                        {
                            InteractablesHelper.allUnlocked = true;
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

        ItemDetails itemDetails = new ItemDetails();
        itemDetails.name = ExampleManager.Instance.Avatar.ToString();
        itemDetails.itemName = gameObject.name;
        itemDetails.isLocked = isLocked;
        ExampleManager.CustomServerMethod("itemInteract", new object[] { itemDetails });
    }
}

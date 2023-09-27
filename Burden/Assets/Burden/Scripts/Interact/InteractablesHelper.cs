using DG.Tweening;
using UnityEngine;
using static ExampleRoomController;

public class InteractablesHelper : MonoBehaviour
{
    public DragRigidbody mysteryBox;
    public Interactable[] interactables;
    PlayerController player;
    public static bool allUnlocked = false;

    private void Start()
    {
        player = ExampleManager.GetOpponent();
        RenameInteractables();
    }

    private void RenameInteractables()
    {
        int i = 0;
        foreach (var item in interactables)
        {
            item.name += i;
            i++;
        }
    }
    private void OnEnable()
    {
        onGrabItem += GrabItem;
        onInteractItem += InteractItem;
    }

    private void InteractItem(ItemDetails interactedItem)
    {
        if (player.hasGameBegun)
            foreach (var item in interactables)
            {
                if (item.name == interactedItem.itemName)
                {
                    if (interactedItem.name == player.prefabName)
                    {
                        if (DoorKey.Instance.door.name == item.name)
                        {
                            if (player.prefabName == Avatars.Mom.ToString())
                            {

                                allUnlocked = true;
                                DoorKey.Instance.key.SetActive(false);
                                item.Interact(player, false);
                            }
                        }
                        else
                        {
                            item.isLocked = interactedItem.isLocked;
                            item.Interact(player, item.isLocked);
                        }
                    }
                    return;
                }
            }
    }

    private void OnDisable()
    {
        onInteractItem -= InteractItem;
        onGrabItem -= GrabItem;
    }

    private void GrabItem(GrabDetails input)
    {
        if (player.hasGameBegun)
        {
            if (player.prefabName == input.name)
            {
                Vector3 pos = new Vector3(input.xPos, input.yPos, input.zPos);
                Quaternion rot = new Quaternion(input.xRot, input.yRot, input.zRot, input.wRot);

                if (input.isDrag)
                {
                    mysteryBox.transform.DOMove(pos, 0.18f).SetEase(Ease.Linear);
                    mysteryBox.transform.DOLocalRotateQuaternion(rot, 0.18f).SetEase(Ease.Linear);
                    mysteryBox.Rigidbody.isKinematic = true;
                }
                else
                {
                    mysteryBox.Rigidbody.isKinematic = false;
                }
            }
            else
            {
                mysteryBox.Rigidbody.isKinematic = false;
            }
        }
    }
}

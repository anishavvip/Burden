using System.ComponentModel;
using System.Linq;
using UnityEngine;
using static ExampleRoomController;

public class InteractablesHelper : MonoBehaviour
{
    public DragRigidbody mysteryBox;
    public Interactable[] interactables;
    PlayerController player;

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
                        item.Interact(player);
                        return;
                    }
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
                mysteryBox.transform.position = pos;
                mysteryBox.transform.localRotation = rot;
                if (input.isDrag)
                    mysteryBox.Rigidbody.isKinematic = true;
                else
                    mysteryBox.Rigidbody.isKinematic = false;
            }
            else
            {
                mysteryBox.Rigidbody.isKinematic = false;
            }
        }
    }
}

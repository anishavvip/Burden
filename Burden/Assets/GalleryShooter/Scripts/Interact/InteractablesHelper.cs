using System.Linq;
using UnityEngine;
using static ExampleRoomController;

public class InteractablesHelper : MonoBehaviour
{
    public ObjectGrabbable[] grabbables;
    public Interactable[] interactables;
    PlayerController player;

    private void Start()
    {
        player = FindObjectsOfType<PlayerController>().Where(
            player => player.prefabName != ExampleManager.Instance.Avatar.ToString()).ToList()[0];
        RenameInteractables();
        RenameGrabbables();
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
    private void RenameGrabbables()
    {
        int i = 0;
        foreach (var item in grabbables)
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

    private void GrabItem(GrabDetails grabbedItem)
    {
        //if (player.hasGameBegun)
        //    foreach (var item in grabbables)
        //    {
        //        if (item.name == grabbedItem.itemName)
        //        {
        //            if (grabbedItem.name == player.prefabName)
        //            {
        //                if (player.SyncData.rightClicked)
        //                {
        //                    item.Throw();
        //                    return;
        //                }
        //                else if (player.SyncData.leftClicked)
        //                {
        //                    item.Drop();
        //                    return;
        //                }
        //                else
        //                {
        //                    item.Interact(player);
        //                    return;
        //                }
        //            }
        //        }
        //    }
    }
}

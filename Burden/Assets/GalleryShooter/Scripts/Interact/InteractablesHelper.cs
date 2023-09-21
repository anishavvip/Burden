using System.Linq;
using UnityEngine;

public class InteractablesHelper : MonoBehaviour
{
    public Interactable[] interactables;
    PlayerController player;

    private void Start()
    {
        player = FindObjectsOfType<PlayerController>().Where(
            player => player.prefabName != ExampleManager.Instance.Avatar.ToString()).ToList()[0];
        int i = 0;
        foreach (var item in interactables)
        {
            item.name += i;
            i++;
        }
    }
    private void OnEnable()
    {
        ExampleRoomController.onInteractItem += InteractItem;
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
        ExampleRoomController.onInteractItem -= InteractItem;
    }
}

using System.Linq;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour
{
    float range = 2;
    PlayerController player;
    float throwAmount = 4f;

    void Start()
    {
        player = FindObjectsOfType<PlayerController>().Where(
            player => player.prefabName == ExampleManager.Instance.Avatar.ToString()).ToList()[0];
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        transform.parent = objectGrabPointTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    public void Drop()
    {
        transform.parent = null;
    }

    private void SendDataToServer()
    {
        GrabDetails itemDetails = new GrabDetails();
        itemDetails.name = ExampleManager.Instance.Avatar.ToString();
        itemDetails.itemName = gameObject.name;

        ExampleManager.CustomServerMethod("itemGrab", new object[] { itemDetails });
    }

    private void OnMouseOver()
    {
        Interact(player);
        SendDataToServer();
    }

    public void Interact(PlayerController player)
    {
        if (player)
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist < range)
            {
                //Open
                if (player.objectGrabbable == null)
                {
                    if (player.SyncData.rightClicked)
                    {
                        Grab(player.objectGrabPointTransform);
                        player.objectGrabbable = this;
                        return;
                    }
                }
            }
        }
    }

    public void Throw()
    {
        transform.parent = null;
    }
}
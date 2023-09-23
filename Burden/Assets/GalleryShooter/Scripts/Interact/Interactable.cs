using System.Collections;
using System.Linq;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    float range = 8f;
    bool open;
    Animator animator;
    PlayerController player;

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
        GetComponent<Rigidbody>().isKinematic = true;
        animator = GetComponent<Animator>();
        open = false;
        player = ExampleManager.GetPlayer();
    }
    public void Interact(PlayerController Player)
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
                        StartCoroutine(opening());
                    }
                }
                else
                {
                    if (open == true)
                    {
                        if (Player.SyncData.rightClicked)
                        {
                            StartCoroutine(closing());
                        }
                    }

                }

            }
        }
    }

    private void OnMouseOver()
    {
        if (!player.hasGameBegun) return;
        Interact(player);

        ItemDetails itemDetails = new ItemDetails();
        itemDetails.name = ExampleManager.Instance.Avatar.ToString();
        itemDetails.itemName = gameObject.name;
        
        ExampleManager.CustomServerMethod("itemInteract", new object[] { itemDetails });
    }
}

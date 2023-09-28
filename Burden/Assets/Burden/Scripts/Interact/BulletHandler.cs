using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            AudioSource.PlayClipAtPoint(GalleryGameManager.Instance.ouchClip, collision.transform.position);
            SimpleShoot.shotCount++;
            if (SimpleShoot.shotCount > 3)
            {
                //Ending
                ExampleManager.CustomServerMethod("ending", new object[] { });
            }
            AudioDetails audioDetails = new AudioDetails();
            audioDetails.name = Avatars.Child.ToString();
            audioDetails.audioType = AudioType.Shoot.ToString();
            ExampleManager.CustomServerMethod("syncAudio", new object[] { audioDetails });
        }
    }
}

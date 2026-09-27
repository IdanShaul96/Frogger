using UnityEngine;

public class Home : MonoBehaviour
{
    public GameObject frogPicture;

    private void OnEnable()
    {
        frogPicture.SetActive((true));
    }

    private void OnDisable()
    {
        frogPicture.SetActive((false));
    }

    private void OnTriggerEnter2D(Collider2D other)
     {
         if (other.CompareTag("Player"))
         {
             PlayerController player = other.GetComponent<PlayerController>();
             if (!player.enabled) return;

             if (enabled)
             {
                 player.Death();
                 return;
             }

             enabled = true;
             FindAnyObjectByType<GameManager>().HomeHasBeenOccupied();
         }
     }
}

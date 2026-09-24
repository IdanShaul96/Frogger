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
             Frogger frogger = other.GetComponent<Frogger>();
             if (!frogger.enabled) return;

             if (enabled)
             {
                 frogger.Death();
                 return;
             }

             enabled = true;
             FindAnyObjectByType<GameManager>().HomeHasBeenOccupied();
         }
     }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collide:" + collision.name);
        if (collision.name == "Body")
        {
            Player otherPlayer = collision.transform.root.GetComponent<Player>();
            bool otherPlayerDefending = otherPlayer.GetIsDefending();
            if (!otherPlayerDefending)
            {
                collision.transform.parent.GetComponent<HealthManager>().TakeDamage();
            }
            else
            {
                AudioManager.RandomDeflectSound();
                transform.root.GetComponent<Player>().Stunned();
            }
        }
        else if (collision.name == "Sword")
        {
            Player otherPlayer = collision.transform.root.GetComponent<Player>();
            bool otherPlayerDefending = otherPlayer.GetIsDefending();
            AudioManager.RandomDeflectSound();
            if (!otherPlayerDefending) 
            {
                
                GameManager.instance.ClashCount += 1;
            }
            else
            {
                transform.root.GetComponent<Player>().Stunned();
            }
        }
    }
}

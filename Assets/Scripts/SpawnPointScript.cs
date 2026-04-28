using System;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;

public class SpawnPointScript : MonoBehaviour
{
    [SerializeField] private GameObject Lock;
    [SerializeField] private GameObject Teleporter;
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>().StartSpawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<TopDownCharacterController>().hasKey)
            {
                Lock.SetActive(false);
                Teleporter.SetActive(true);
            }
        }
    }
}

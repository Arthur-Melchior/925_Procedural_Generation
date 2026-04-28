using System;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<TopDownCharacterController>().hasKey = true;
            transform.SetParent(other.transform);
        }
    }
}

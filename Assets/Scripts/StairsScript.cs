using System;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StairsScript : MonoBehaviour
{
    [SerializeField] private Transform tpTop;
    [SerializeField] private Transform tpBottom;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemyScript = other.GetComponent<EnemyScript>();
        if (enemyScript)
        {
            if (enemyScript.currentLayer == 0)
            {
                enemyScript.currentLayer = 1;
                enemyScript.transform.position = tpTop.position;
            }
            else
            {
                enemyScript.currentLayer = 0;
                enemyScript.transform.position = tpBottom.position;
            }
            
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var playerScript = other.GetComponent<TopDownCharacterController>();
        if (playerScript)
        {
            playerScript.currentLayer = playerScript.currentLayer == 0 ? 1 : 0;
        }
    }
}
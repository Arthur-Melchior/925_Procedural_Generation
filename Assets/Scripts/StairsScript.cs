using System;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StairsScript : MonoBehaviour
{
    [SerializeField] private Transform tpTop;
    [SerializeField] private Transform tpBottom;
    public int roomIndex;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemyScript = other.GetComponent<EnemyScript>();
        if (enemyScript)
        {
            if (enemyScript.currentRoomIndex == 0)
            {
                enemyScript.currentRoomIndex = roomIndex;
                enemyScript.transform.position = tpTop.position;
            }
            else
            {
                enemyScript.currentRoomIndex = 0;
                enemyScript.transform.position = tpBottom.position;
            }
            
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var playerScript = other.GetComponent<TopDownCharacterController>();
        if (playerScript)
        {
            GameObject.Find("EnemiesManager").GetComponent<EnemiesManager>().ClearPath();
            playerScript.currentRoomIndex = playerScript.currentRoomIndex == 0 ? roomIndex : 0;
        }
    }
}
using System;
using Cainos.Pixel_Art_Top_Down___Basic.Script;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StairsScript : MonoBehaviour
{
    public TilemapCollider2D wall;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Physics2D.IgnoreCollision(other, wall, true);
        var enemyScript = other.GetComponent<EnemyScript>();
        if (enemyScript)
        {
            enemyScript.currentLayer = enemyScript.currentLayer == 0 ? 1 : 0;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var playerScript = other.GetComponent<TopDownCharacterController>();
        if (playerScript)
        {
            playerScript.currentLayer = playerScript.currentLayer == 0 ? 1 : 0;
        }
        Physics2D.IgnoreCollision(other, wall, false);
    }
}
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StairsScript : MonoBehaviour
{
    public TilemapCollider2D wall;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Physics2D.IgnoreCollision(other, wall, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Physics2D.IgnoreCollision(other, wall, false);
    }
}
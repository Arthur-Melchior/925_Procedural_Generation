using System;
using UnityEngine;

public class WallPasserScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Walls"), true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Walls"), false);
            var position = new Vector2(other.transform.position.x, other.transform.position.y);
            if (position.x < 0 || position.y < 0)
            {
                return;
            }

            var mapPosition = GameObject.Find("DungeonGenerator").GetComponent<DungeonGenerator>()
                .WalkableTiles[(int) position.x, (int) position.y];
            other.gameObject.GetComponent<PlayerScript>().currentRoomIndex = mapPosition.RoomIndex;
        }
    }
}
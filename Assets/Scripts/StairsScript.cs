using System;
using UnityEngine;

public class StairsScript : MonoBehaviour
{
    [SerializeField] private Transform tpTop;
    [SerializeField] private Transform tpBottom;
    public int roomIndex;

    private PlayerScript _playerScript;

    private void Start()
    {
        _playerScript = GameObject.Find("Player").GetComponent<PlayerScript>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemyScript = other.GetComponent<EnemyScript>();
        if (enemyScript && _playerScript.currentRoomIndex != enemyScript.currentRoomIndex)
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
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject.Find("EnemiesManager").GetComponent<EnemiesManager>().ClearPath();
            _playerScript.currentRoomIndex = _playerScript.currentRoomIndex == 0 ? roomIndex : 0;
        }
    }
}
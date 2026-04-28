using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButtonScript : MonoBehaviour
{
   public void StartNewGame()
   {
      SceneManager.LoadScene(1);
   }
}

using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeCardScript : MonoBehaviour
{
    public Image image;
    public TMP_Text nameText;
    public TMP_Text description;
    public UnityEvent<PowerUp> powerUpSelected;
    private PowerUp _powerUp;

    public void FillValues(PowerUp? powerUp)
    {
        _powerUp = powerUp!.Value;
        image.sprite = powerUp!.Value.sprite;
        nameText.text = powerUp!.Value.name;
        description.text = powerUp!.Value.description;
    }

    public void OnClick()
    {
        powerUpSelected.Invoke(_powerUp);
    }
}

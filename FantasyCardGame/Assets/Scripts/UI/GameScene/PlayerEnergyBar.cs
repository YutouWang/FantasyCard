using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyBar : MonoBehaviour
{
    [SerializeField] private Slider energySlider;

    private PlayerData playerData;

    public void Bind(PlayerData data)
    {
        playerData = data;
        energySlider.maxValue = playerData.maxEnergy;
        Refresh();
    }

    public void Refresh()
    {
        energySlider.value = playerData.maxEnergy - playerData.currentEnergy;
    }
}


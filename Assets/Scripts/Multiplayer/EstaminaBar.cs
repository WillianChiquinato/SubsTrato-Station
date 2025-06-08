using UnityEngine;
using UnityEngine.UI;

public class EstaminaBar : MonoBehaviour
{
    [Header("Estamina Bar")]
    public Slider sliderEstamina;
    public float maxEstamina = 50;
    public PlayerMoviment estaminaDoPlayer;

    void Start()
    {
        estaminaDoPlayer = GameObject.FindFirstObjectByType<PlayerMoviment>();

        sliderEstamina = GetComponentInChildren<Slider>();
        maxEstamina = estaminaDoPlayer.estamina;
        sliderEstamina.maxValue = maxEstamina;
    }

    void Update()
    {
        if (estaminaDoPlayer != null)
        {
            if (sliderEstamina.value != estaminaDoPlayer.estamina)
            {
                sliderEstamina.value = estaminaDoPlayer.estamina;
            }
        }
        else
        {
            Debug.Log("Barra de Estamina falhada " + gameObject.name);
        }
    }
}

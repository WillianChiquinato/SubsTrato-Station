using System.Collections;
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
        StartCoroutine(DelayConnectedBars());
        sliderEstamina = GetComponentInChildren<Slider>();
    }

    IEnumerator DelayConnectedBars()
    {
        yield return new WaitForSeconds(1f);
        var ui = UIreferences.Instance;

        estaminaDoPlayer = ui.player.GetComponent<PlayerMoviment>();
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

    public void Boost(int amount, float duration)
    {
        if (estaminaDoPlayer != null)
        {
            estaminaDoPlayer.estamina += amount;
            if (estaminaDoPlayer.estamina > maxEstamina)
            {
                estaminaDoPlayer.estamina = maxEstamina;
            }
            Invoke(nameof(ResetEstamina), duration);
        }
    }

    private void ResetEstamina()
    {
        if (estaminaDoPlayer != null)
        {
            estaminaDoPlayer.estamina = maxEstamina;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public float maxHealth = 100;
    public Health health;

    void Start()
    {
        StartCoroutine(DelayConnectedBars());
        slider = GetComponentInChildren<Slider>();
    }

    IEnumerator DelayConnectedBars()
    {
        yield return new WaitForSeconds(1f);
        var ui = UIreferences.Instance;
        health = ui.player.GetComponent<Health>();

        slider.maxValue = maxHealth;
        health.health = this.maxHealth;
    }

    void Update()
    {
        if (health != null)
        {
            if (slider.value != health.health)
            {
                slider.value = health.health;
            }
        }
        else
        {
            Debug.Log("Barra de vida falhada " + gameObject.name);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public float maxHealth = 100;
    public Health health;

    void Start()
    {
        health = GameObject.FindFirstObjectByType<PlayerMoviment>().GetComponent<Health>();

        slider = GetComponentInChildren<Slider>();
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

using System.Collections;
using TMPro;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Sounds")]
    public AudioSource ContagemRegressiva;
    public AudioSource BossDeathSound;
    private bool hasPlayedDeathSound = false;


    [Header("Revive Settings")]
    public ItemMove ContagemBoss;
    public bool timingReady = false;
    private int startTime = 7;
    private float currentTime;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;

    public Health health;
    public Animator animator;

    public bool canMove
    {
        get
        {
            return animator.GetBool("canMove");
        }
        set
        {
            animator.SetBool("canMove", value);
        }
    }

    void Start()
    {
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();

        ContagemBoss.gameObject.SetActive(false);
        currentTime = startTime;
    }

    void Update()
    {
        animator.SetBool("IsAlive", health.isAlive);
        if (!health.isAlive)
        {
            health.isDead = true;

            if (BossDeathSound != null && !hasPlayedDeathSound)
            {
                AudioSource.PlayClipAtPoint(BossDeathSound.clip, transform.position);
                hasPlayedDeathSound = true;
            }
            animator.ResetTrigger("Hit");

            StartCoroutine(TimingToRevive());
        }
        else
        {
            animator.SetBool("Stunned", false);
            timingReady = false;
            ContagemBoss.gameObject.SetActive(false);
            ContagemRegressiva.Stop();
        }

        if (timingReady)
        {
            ContagemBoss.gameObject.SetActive(true);
            currentTime -= Time.deltaTime;

            if (currentTime > 0)
            {
                ContagemBoss.GetComponent<TextMeshPro>().text = Mathf.Ceil(currentTime).ToString();
            }
            else
            {
                ContagemBoss.GetComponent<TextMeshPro>().text = "";
                health.isAlive = true;
                health.health = 100;
                health.isDead = false;

                ContagemBoss.gameObject.SetActive(false);
                currentTime = startTime;
                timingReady = false;
            }
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0.2f;
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = knockbackDuration;
    }

    public bool IsBeingKnockedBack()
    {
        return knockbackTimer > 0;
    }

    IEnumerator TimingToRevive()
    {
        animator.SetBool("Stunned", true);
        yield return new WaitForSeconds(2f);

        timingReady = true;
        if (!ContagemRegressiva.isPlaying)
        {
            ContagemRegressiva.Play();
        }
    }
}

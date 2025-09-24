using Fusion;
using UnityEngine;

public class CharacterMultiplayer : NetworkBehaviour
{
    [Header("Sounds")]
    public AudioSource PassosSound;
    public AudioSource AttackSound;

    [Header("Movement")]
    public float moveSpeed;
    private Vector3 moveDirection;

    public CharacterController character;
    public CapsuleCollider capsuleColliderCharacter;
    public Animator animator;
    public bool isMoving;

    public int selectedSkinIndex = 0;

    [Header("Gravidade player")]
    public Vector3 velocity;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Attack")]
    public CharacterMultiplayerAttack attackCollider1;
    public CharacterMultiplayerAttack attackCollider2;

    [Header("GroundCheck")]
    public bool isGrounded;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;
    [HideInInspector] public Health health;

    public bool canMove
    {
        get { return animator.GetBool("canMove"); }
        set { animator.SetBool("canMove", value); }
    }

    void Start()
    {
        character = GetComponent<CharacterController>();
        capsuleColliderCharacter = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        attackCollider1 = transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand/ColisorEsquerda").GetComponent<CharacterMultiplayerAttack>();
        attackCollider2 = transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand/ColisorDireita").GetComponent<CharacterMultiplayerAttack>();

        attackCollider1.GetComponent<SphereCollider>().enabled = false;
        attackCollider2.GetComponent<SphereCollider>().enabled = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData inputData)) return;
        if (!Object.HasStateAuthority) return;
        if (character == null) return;

        isGrounded = character.isGrounded;
        animator.SetBool("IsGround", isGrounded);
        animator.SetBool("Jump", isGrounded);
        animator.SetFloat("yVelocity", velocity.y);
        animator.SetBool("IsAlive", health.isAlive);

        if (!health.isAlive)
        {
            canMove = false;
            animator.SetTrigger("Death");
            character.Move(Vector3.zero);
            velocity = Vector3.zero;
            return;
        }

        if (!isGrounded)
        {
            animator.applyRootMotion = false;
        }

        // Knockback
        if (knockbackTimer > 0)
        {
            character.Move(knockbackVelocity * Runner.DeltaTime);
            knockbackTimer -= Runner.DeltaTime;
        }

        if (canMove)
        {
            HandleMovement(inputData);
            HandleRotation(inputData);

            HandleAnimations(inputData);
            HandleAttack(inputData);

            // Jump
            if (isGrounded && inputData.jumpPressed)
            {
                Jump();
            }

            // Gravidade
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            velocity.y += gravity * Runner.DeltaTime;
        }
    }

    void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            // pega o deslocamento da animação
            Vector3 rootMotion = animator.deltaPosition;

            // mantém o controle da gravidade
            rootMotion.y = velocity.y * Runner.DeltaTime;

            character.Move(rootMotion);
        }
    }

    private void HandleMovement(NetworkInputData inputData)
    {
        Vector3 direction = new Vector3(inputData.moveInput.x, 0f, inputData.moveInput.y);
        moveDirection = direction.normalized;

        Vector3 move = moveDirection * moveSpeed;
        move.y = velocity.y;

        character.Move(move * Runner.DeltaTime);
    }

    private void HandleRotation(NetworkInputData inputData)
    {
        Vector3 direction = new Vector3(inputData.moveInput.x, 0f, inputData.moveInput.y);

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * 15f);
        }
    }

    private void HandleAnimations(NetworkInputData inputData)
    {
        if (!Object.HasInputAuthority) return;

        isMoving = inputData.moveInput.magnitude > 0.1f;
        animator.SetBool("Run", isMoving);

        if (isMoving && isGrounded && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (!PassosSound.isPlaying) PassosSound.Play();
        }
        else if (PassosSound.isPlaying)
        {
            PassosSound.Stop();
        }

        animator.SetFloat("horizontal", inputData.moveInput.x);
        animator.SetFloat("vertical", inputData.moveInput.y);
    }


    private void HandleAttack(NetworkInputData inputData)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (!AttackSound.isPlaying)
            {
                AttackSound.Play();
            }
            animator.applyRootMotion = true;

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.15f)
            {
                attackCollider1.GetComponent<SphereCollider>().enabled = true;

                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.3f)
                {
                    attackCollider1.GetComponent<SphereCollider>().enabled = false;

                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.4f)
                    {
                        attackCollider2.GetComponent<SphereCollider>().enabled = true;

                        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f)
                        {
                            attackCollider2.GetComponent<SphereCollider>().enabled = false;
                            if (AttackSound.isPlaying)
                            {
                                AttackSound.Stop();
                            }
                        }
                    }
                }
            }

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                attackCollider1.GetComponent<SphereCollider>().enabled = false;
                attackCollider2.GetComponent<SphereCollider>().enabled = false;

                attackCollider1.ResetAttack();
                attackCollider2.ResetAttack();
                ResetAttack();
            }
        }

        if (inputData.attackPressed && canMove && isGrounded)
        {
            Attack();
        }
    }

    public void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
        canMove = false;
        moveDirection = Vector3.zero;
        Invoke("ResetAttack", 1.5f);
    }

    private void ResetAttack()
    {
        animator.applyRootMotion = false;
        canMove = true;
    }

    public void RpcUpdateSkin(int skinIndex)
    {
        selectedSkinIndex = skinIndex;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Skin"))
            {
                child.gameObject.SetActive(false);
            }
        }

        Transform selectedSkin = transform.Find("Skin" + (skinIndex + 1));
        if (selectedSkin != null)
        {
            selectedSkin.gameObject.SetActive(true);
        }
    }
}


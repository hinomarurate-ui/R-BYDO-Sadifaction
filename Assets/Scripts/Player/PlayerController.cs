using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour, IDamageable
{
    const float DefaultFacingScale = 0.6f;

    [Header("Movement")]
    [FormerlySerializedAs("speed")]
    [SerializeField] float moveSpeed = 6f;
    [FormerlySerializedAs("gravity")]
    [SerializeField] float gravity = 25f;
    [SerializeField] float jumpSpeed = 12f;
    [SerializeField] float jumpHeight = 2f;
    [FormerlySerializedAs("ground")]
    [SerializeField] GroundSensor groundSensor;

    [Header("HP")]
    [SerializeField] int maxHP = 100;
    [SerializeField] int currentHP;

    [Header("Body")]
    [SerializeField] float colliderFriction;
    [SerializeField] float colliderBounciness;

    [Header("Melee")]
    [SerializeField] Transform meleePoint;
    [SerializeField] float meleeRadius = 0.6f;
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] int meleeDamage = 1;
    [SerializeField] float meleeCooltime = 0.25f;
    [SerializeField] float hitDelay = 0.03f;
    [SerializeField] float meleeStepSpeed = 10f;
    [SerializeField] float meleeStepDeceleration = 40f;
    [SerializeField] float meleeKillShakePower = 0.16f;
    [SerializeField] float meleeKillShakeTime = 0.16f;
    [FormerlySerializedAs("ClawS")]
    [SerializeField] AudioClip meleeStartSound;
    [FormerlySerializedAs("ClawSEnd")]
    [SerializeField] AudioClip meleeEndSound;

    [Header("Bydo Claw")]
    [SerializeField] float bydoClawRadius = 2f;
    [SerializeField] int bydoClawDamage = 3;
    [SerializeField] float bydoClawCooltime = 1f;
    [SerializeField] float bydoClawHitDelay = 0.05f;
    [SerializeField] float bydoClawShakePower = 0.3f;
    [SerializeField] float bydoClawShakeTime = 0.2f;
    [SerializeField] float bydoClawStartTime = 0.12f;
    [SerializeField] int bydoClawCount = 3;
    [SerializeField] float bydoClawDistance = 3f;
    [FormerlySerializedAs("bydoClawStepCooltime")]
    [SerializeField] float bydoClawStepDuration = 0.08f;
    [SerializeField] float bydoClawInterval = 0.05f;
    [FormerlySerializedAs("Charge")]
    [SerializeField] AudioClip chargeSound;

    [Header("Bydo Missile")]
    [SerializeField] float bydoMissileCooltime = 1.5f;
    [SerializeField] float bydoMissileStartTime = 0.35f;
    [SerializeField] int bydoMissileCount = 3;
    [SerializeField] float bydoMissileInterval = 0.16f;
    [SerializeField] int bydoMissileDamage = 4;
    [SerializeField] float bydoMissileSeekRange = 16f;
    [SerializeField] float bydoMissileKillShakePower = 0.3f;
    [SerializeField] float bydoMissileKillShakeTime = 0.2f;
    [SerializeField] GameObject bydoMissilePrefab;
    [SerializeField] Sprite bydoMissileChargeSprite;
    [SerializeField] Sprite bydoMissileAttackSprite;
    [FormerlySerializedAs("misairu")]
    [SerializeField] AudioClip missileLaunchSound;

    Animator animator;
    Rigidbody2D body;
    SpriteRenderer spriteRenderer;
    Collider2D bodyCollider;
    AudioSource audioSource;
    PhysicsMaterial2D runtimeBodyMaterial;

    float horizontalInput;
    float verticalSpeed;
    float jumpStartY;
    float lastMeleeTime = -999f;
    float lastBydoClawTime = -999f;
    float lastBydoMissileTime = -999f;
    float currentMeleeStep;
    float currentBydoClawStep;

    bool isGrounded;
    bool isJumping;
    bool jumpHeld;
    bool shotHeld;
    bool jumpQueued;
    bool meleeQueued;
    bool bydoClawQueued;
    bool bydoMissileQueued;
    bool isSpecialActing;

    int meleeComboIndex;

    public int MaxHP { get { return maxHP; } }
    public int CurrentHP { get { return currentHP; } }
    public float HpRate { get { return maxHP <= 0 ? 0f : (float)currentHP / maxHP; } }
    public event Action<int, int> OnHpChanged;

    void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        bodyCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        currentHP = maxHP;
        NotifyHpChanged();
        ApplyBodyMaterial();
    }

    void OnDestroy()
    {
        if(runtimeBodyMaterial != null)
        {
            Destroy(runtimeBodyMaterial);
        }
    }

    public DamageResult TakeDamage(DamageRequest request)
    {
        if(request.Amount <= 0 || currentHP <= 0)
        {
            return DamageResult.Ignored(currentHP, maxHP);
        }

        int previousHP = currentHP;
        currentHP = Mathf.Clamp(currentHP - request.Amount, 0, maxHP);
        if(currentHP != previousHP)
        {
            NotifyHpChanged();
        }

        return new DamageResult(currentHP != previousHP, currentHP <= 0, currentHP, maxHP);
    }

    void Update()
    {
        CaptureInput();
    }

    void FixedUpdate()
    {
        RefreshGroundedState();

        float xSpeed;
        UpdateJumpVelocity();
        RunQueuedActions();

        if(isSpecialActing)
        {
            SetAnimatorBool("run", false);
            xSpeed = 0f;
        }
        else
        {
            xSpeed = ApplyFacingAndRunAnimation(horizontalInput);
        }

        SetAnimatorBool("Shot", !isSpecialActing && shotHeld);
        DecelerateMeleeStep();
        xSpeed += currentMeleeStep + currentBydoClawStep;

        if(body != null)
        {
            body.velocity = new Vector2(xSpeed, verticalSpeed);
        }
    }

    void CaptureInput()
    {
        horizontalInput = IsAimLockHeld() ? 0f : Input.GetAxisRaw("Horizontal");
        jumpHeld = !isSpecialActing && Input.GetButton("Jump");
        shotHeld = !isSpecialActing && Input.GetKey(KeyCode.Z);

        if(!isSpecialActing && Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
        }

        if(!isSpecialActing && Input.GetKeyDown(KeyCode.X))
        {
            PlayOneShot(meleeStartSound);
            meleeQueued = true;
        }

        if(!isSpecialActing && Input.GetKeyDown(KeyCode.E))
        {
            PlayOneShot(chargeSound);
            bydoClawQueued = true;
        }

        if(!isSpecialActing && Input.GetKeyDown(KeyCode.Q))
        {
            PlayOneShot(chargeSound);
            bydoMissileQueued = true;
        }
    }

    bool IsAimLockHeld()
    {
        return Input.GetKey(KeyCode.B) || Input.GetKey(KeyCode.JoystickButton1);
    }

    void RefreshGroundedState()
    {
        isGrounded = groundSensor != null && groundSensor.IsGround();
    }

    void UpdateJumpVelocity()
    {
        if(isGrounded)
        {
            if(!isSpecialActing && jumpQueued)
            {
                jumpQueued = false;
                verticalSpeed = jumpSpeed;
                jumpStartY = transform.position.y;
                isJumping = true;
                SetAnimatorBool("jump", true);
                return;
            }

            verticalSpeed = 0f;
            isJumping = false;
            SetAnimatorBool("jump", false);
            return;
        }

        if(isJumping && jumpHeld && jumpStartY + jumpHeight > transform.position.y)
        {
            verticalSpeed = jumpSpeed;
            return;
        }

        verticalSpeed = -gravity;
        isJumping = false;
    }

    void RunQueuedActions()
    {
        if(isSpecialActing)
        {
            return;
        }

        if(meleeQueued)
        {
            meleeQueued = false;
            TryMelee();
        }

        if(bydoClawQueued)
        {
            bydoClawQueued = false;
            TryBydoClaw();
        }

        if(bydoMissileQueued)
        {
            bydoMissileQueued = false;
            TryBydoMissile();
        }
    }

    float ApplyFacingAndRunAnimation(float direction)
    {
        if(direction > 0f)
        {
            transform.localScale = new Vector3(DefaultFacingScale, DefaultFacingScale, DefaultFacingScale);
            SetAnimatorBool("run", true);
            return moveSpeed;
        }

        if(direction < 0f)
        {
            transform.localScale = new Vector3(-DefaultFacingScale, DefaultFacingScale, DefaultFacingScale);
            SetAnimatorBool("run", true);
            return -moveSpeed;
        }

        SetAnimatorBool("run", false);
        return 0f;
    }

    void DecelerateMeleeStep()
    {
        if(currentMeleeStep != 0f)
        {
            currentMeleeStep = Mathf.MoveTowards(currentMeleeStep, 0f, meleeStepDeceleration * Time.fixedDeltaTime);
        }
    }

    void TryMelee()
    {
        if(Time.time < lastMeleeTime + meleeCooltime)
        {
            return;
        }

        lastMeleeTime = Time.time;
        currentMeleeStep = FacingDirection() * meleeStepSpeed;
        meleeComboIndex = (meleeComboIndex + 1) % 2;
        StartCoroutine(PlayMeleeAttack());
    }

    IEnumerator PlayMeleeAttack()
    {
        SetAnimatorBool(meleeComboIndex == 0 ? "Claw" : "ClawP", true);

        if(hitDelay > 0f)
        {
            yield return new WaitForSeconds(hitDelay);
        }

        DoMeleeHit();

        float rest = Mathf.Max(0f, meleeCooltime - hitDelay);
        if(rest > 0f)
        {
            yield return new WaitForSeconds(rest);
        }

        SetAnimatorBool("Claw", false);
        SetAnimatorBool("ClawP", false);
    }

    void TryBydoClaw()
    {
        if(isSpecialActing || Time.time < lastBydoClawTime + bydoClawCooltime)
        {
            return;
        }

        lastBydoClawTime = Time.time;
        StartCoroutine(RunBydoClawCombo());
    }

    IEnumerator RunBydoClawCombo()
    {
        BeginSpecialAction();
        SetAnimatorBool("EXClaw", true);

        if(bydoClawStartTime > 0f)
        {
            yield return new WaitForSeconds(bydoClawStartTime);
        }

        int attackCount = Mathf.Max(0, bydoClawCount);
        for(int i = 0; i < attackCount; i++)
        {
            SetAnimatorInteger("EXClawC", i + 1);
            PlayOneShot(meleeStartSound);
            yield return StartCoroutine(RunBydoClawStep());

            if(i < attackCount - 1 && bydoClawInterval > 0f)
            {
                yield return new WaitForSeconds(bydoClawInterval);
            }
        }

        PlayOneShot(meleeEndSound);
        currentBydoClawStep = 0f;
        SetAnimatorBool("EXClaw", false);

        if(bydoClawInterval > 0f)
        {
            yield return new WaitForSeconds(bydoClawInterval);
        }

        SetAnimatorInteger("EXClawC", 0);
        EndSpecialAction();
    }

    IEnumerator RunBydoClawStep()
    {
        float stepDuration = Mathf.Max(0f, bydoClawStepDuration);
        currentBydoClawStep = stepDuration > 0f ? FacingDirection() * bydoClawDistance / stepDuration : 0f;

        float clampedHitDelay = Mathf.Clamp(bydoClawHitDelay, 0f, stepDuration);
        if(clampedHitDelay > 0f)
        {
            yield return new WaitForSeconds(clampedHitDelay);
        }

        DoBydoClawHit();

        float rest = Mathf.Max(0f, stepDuration - clampedHitDelay);
        if(rest > 0f)
        {
            yield return new WaitForSeconds(rest);
        }

        currentBydoClawStep = 0f;
    }

    void TryBydoMissile()
    {
        if(isSpecialActing || Time.time < lastBydoMissileTime + bydoMissileCooltime)
        {
            return;
        }

        lastBydoMissileTime = Time.time;
        StartCoroutine(RunBydoMissile());
    }

    IEnumerator RunBydoMissile()
    {
        BeginSpecialAction();
        SetAnimatorBool("EXClaw", false);
        SetAnimatorInteger("EXClawC", 0);
        SetBydoMissilePose(bydoMissileChargeSprite);

        if(bydoMissileStartTime > 0f)
        {
            yield return new WaitForSeconds(bydoMissileStartTime);
        }

        SetBydoMissilePose(bydoMissileAttackSprite);

        int missileCount = Mathf.Max(0, bydoMissileCount);
        for(int i = 0; i < missileCount; i++)
        {
            SpawnBydoMissile(i);
            PlayOneShot(missileLaunchSound);

            if(i < missileCount - 1 && bydoMissileInterval > 0f)
            {
                yield return new WaitForSeconds(bydoMissileInterval);
            }
        }

        if(bydoMissileInterval > 0f)
        {
            yield return new WaitForSeconds(bydoMissileInterval);
        }

        ResetAnimatorPose();
        EndSpecialAction();
    }

    void BeginSpecialAction()
    {
        isSpecialActing = true;
        jumpQueued = false;
        meleeQueued = false;
        bydoClawQueued = false;
        bydoMissileQueued = false;
        currentMeleeStep = 0f;
        currentBydoClawStep = 0f;

        SetAnimatorBool("Shot", false);
        SetAnimatorBool("run", false);
        SetAnimatorBool("Claw", false);
        SetAnimatorBool("ClawP", false);
    }

    void EndSpecialAction()
    {
        isSpecialActing = false;
    }

    void SetBydoMissilePose(Sprite poseSprite)
    {
        if(spriteRenderer == null || poseSprite == null)
        {
            return;
        }

        if(animator != null)
        {
            animator.enabled = false;
        }

        spriteRenderer.sprite = poseSprite;
    }

    void ResetAnimatorPose()
    {
        if(animator != null)
        {
            animator.enabled = true;
        }
    }

    void SpawnBydoMissile(int missileIndex)
    {
        if(bydoMissilePrefab == null)
        {
            return;
        }

        float dirX = FacingDirection();
        Transform shotPoint = transform.Find("ShotPosition");
        Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;
        float centeredIndex = missileIndex - ((Mathf.Max(1, bydoMissileCount) - 1) * 0.5f);
        Vector3 spawnOffset = new Vector3(-0.35f * dirX, -1f + Mathf.Abs(centeredIndex) * 0.35f, 0f);
        GameObject missileObject = Instantiate(bydoMissilePrefab, origin + spawnOffset, Quaternion.identity);
        PlayerHomingMissile missile = missileObject.GetComponent<PlayerHomingMissile>();

        if(missile == null)
        {
            Destroy(missileObject);
            return;
        }

        Vector2 initialDirection = new Vector2(dirX, -0.15f * centeredIndex).normalized;
        missile.Init(
            initialDirection,
            dirX,
            bydoMissileDamage,
            enemyLayers,
            bydoMissileSeekRange,
            bydoMissileKillShakePower,
            bydoMissileKillShakeTime);
    }

    void DoMeleeHit()
    {
        DamageEnemiesInCircle(meleeRadius, meleeDamage, meleeKillShakePower, meleeKillShakeTime);
    }

    void DoBydoClawHit()
    {
        DamageEnemiesInCircle(bydoClawRadius, bydoClawDamage, bydoClawShakePower, bydoClawShakeTime);
    }

    void DamageEnemiesInCircle(float hitRadius, int damage, float shakePower, float shakeTime)
    {
        if(meleePoint == null)
        {
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(meleePoint.position, hitRadius, enemyLayers);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach(Collider2D hit in hits)
        {
            if(hit == null)
            {
                continue;
            }

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if(damageable == null || damagedTargets.Contains(damageable))
            {
                continue;
            }

            damagedTargets.Add(damageable);
            DamageRequest request = new DamageRequest(damage, gameObject, hit.bounds.center, shakePower, shakeTime);
            DamageUtility.ApplyDamage(damageable, request, true);
        }
    }

    void ApplyBodyMaterial()
    {
        if(bodyCollider == null)
        {
            return;
        }

        runtimeBodyMaterial = new PhysicsMaterial2D("PlayerNoFriction")
        {
            friction = colliderFriction,
            bounciness = colliderBounciness
        };
        bodyCollider.sharedMaterial = runtimeBodyMaterial;
    }

    void NotifyHpChanged()
    {
        if(OnHpChanged != null)
        {
            OnHpChanged(currentHP, maxHP);
        }
    }

    float FacingDirection()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        return direction == 0f ? 1f : direction;
    }

    void SetAnimatorBool(string key, bool value)
    {
        if(animator != null)
        {
            animator.SetBool(key, value);
        }
    }

    void SetAnimatorInteger(string key, int value)
    {
        if(animator != null)
        {
            animator.SetInteger(key, value);
        }
    }

    void PlayOneShot(AudioClip clip)
    {
        if(audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, 0.5f);
        }
    }
}

using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] float fallbackDeathTorque = 10f;
    [SerializeField] float fallbackFadeTime = 1f;
    [SerializeField] float fallbackSmashX = 8f;
    [SerializeField] float fallbackSmashY = 5f;
    [SerializeField] int fallbackScore = 100;
    [SerializeField] GameObject fallbackBombPrefab;
    [SerializeField] float fallbackBombLifeTime = 1f;
    [SerializeField] AudioClip fallbackDamageSound;
    [SerializeField] AudioClip fallbackDeathSound;

    EnemyController controller;
    Collider2D bodyCollider;
    Rigidbody2D body;
    Animator animator;
    SpriteRenderer spriteRenderer;
    AudioSource audioSource;
    int currentHealth;
    int maxHealth = 1;
    bool dead;

    public int CurrentHealth { get { return currentHealth; } }
    public int MaxHealth { get { return maxHealth; } }
    public bool IsDead { get { return dead; } }

    void Awake()
    {
        CacheComponents();
    }

    void Start()
    {
        if(controller == null)
        {
            Initialize(GetComponent<EnemyController>());
        }
    }

    public void Initialize(EnemyController owner)
    {
        controller = owner;
        CacheComponents();
        ResolveScoreManager();
        ApplyDefinition(owner != null ? owner.Definition : null);
    }

    public DamageResult TakeDamage(DamageRequest request)
    {
        if(dead || request.Amount <= 0)
        {
            return DamageResult.Ignored(currentHealth, maxHealth);
        }

        currentHealth = Mathf.Clamp(currentHealth - request.Amount, 0, maxHealth);
        PlayOneShot(CurrentDeathSettings().damageSound, fallbackDamageSound, 0.5f);
        StartCoroutine(Flash(new Color32(255, 150, 0, 255), 0.025f));

        bool killed = currentHealth <= 0;
        if(killed)
        {
            Die(request);
        }

        if(controller != null)
        {
            controller.NotifyDamaged(killed);
        }

        return new DamageResult(true, killed, currentHealth, maxHealth);
    }

    void CacheComponents()
    {
        if(bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
        if(body == null) body = GetComponent<Rigidbody2D>();
        if(animator == null) animator = GetComponent<Animator>();
        if(spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if(audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void ApplyDefinition(EnemyDefinition definition)
    {
        maxHealth = definition != null ? Mathf.Max(1, definition.MaxHealth) : Mathf.Max(1, currentHealth);
        currentHealth = maxHealth;
        if(definition != null)
        {
            fallbackScore = definition.Score;
        }
    }

    void ResolveScoreManager()
    {
        if(scoreManager != null)
        {
            return;
        }

        GameObject scoreObject = GameObject.FindWithTag("GameController");
        if(scoreObject != null)
        {
            scoreManager = scoreObject.GetComponent<ScoreManager>();
        }
    }

    void Die(DamageRequest request)
    {
        if(dead)
        {
            return;
        }

        dead = true;
        EnemyDefinition.DeathSettings death = CurrentDeathSettings();
        SpawnBombEffect(death);

        if(controller != null)
        {
            controller.EnterDead();
        }
        else if(animator != null)
        {
            animator.SetTrigger("Death");
        }

        PlayOneShot(death.deathSound, fallbackDeathSound, 0.5f);
        if(scoreManager != null)
        {
            scoreManager.AddScore(controller != null && controller.Definition != null ? controller.Definition.Score : fallbackScore);
        }

        gameObject.layer = LayerMask.NameToLayer("Corpse");
        DisableCombatBehaviours();
        ApplyDeathImpulse(request, death);
        StartCoroutine(Fadeout(death.fadeTime > 0f ? death.fadeTime : fallbackFadeTime));
    }

    void DisableCombatBehaviours()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        for(int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if(behaviour == null || behaviour == this)
            {
                continue;
            }

            if(behaviour is IEnemyMovement || behaviour is IEnemyAttackPattern || behaviour is IEnemyRoutine)
            {
                behaviour.enabled = false;
            }
        }
    }

    EnemyDefinition.DeathSettings CurrentDeathSettings()
    {
        if(controller != null && controller.Definition != null && controller.Definition.Death != null)
        {
            return controller.Definition.Death;
        }

        return new EnemyDefinition.DeathSettings
        {
            deathTorque = fallbackDeathTorque,
            fadeTime = fallbackFadeTime,
            smashX = fallbackSmashX,
            smashY = fallbackSmashY,
            bombPrefab = fallbackBombPrefab,
            bombLifeTime = fallbackBombLifeTime,
            damageSound = fallbackDamageSound,
            deathSound = fallbackDeathSound
        };
    }

    void ApplyDeathImpulse(DamageRequest request, EnemyDefinition.DeathSettings death)
    {
        if(body == null)
        {
            return;
        }

        float dirX = 0f;
        if(request.Source != null)
        {
            dirX = Mathf.Sign(transform.position.x - request.Source.transform.position.x);
        }

        if(dirX == 0f)
        {
            dirX = -Mathf.Sign(transform.localScale.x);
        }

        if(dirX == 0f)
        {
            dirX = -1f;
        }

        float smashX = death.smashX != 0f ? death.smashX : fallbackSmashX;
        float smashY = death.smashY != 0f ? death.smashY : fallbackSmashY;
        float torque = death.deathTorque != 0f ? death.deathTorque : fallbackDeathTorque;
        Vector2 impulse = new Vector2(dirX * smashX, smashY);
        body.AddForce(impulse, ForceMode2D.Impulse);
        body.AddTorque(torque * (impulse.x >= 0f ? -1f : 1f), ForceMode2D.Impulse);
    }

    IEnumerator Fadeout(float fadeTime)
    {
        float elapsed = 0f;
        const float interval = 0.2f;

        while(elapsed < fadeTime)
        {
            yield return StartCoroutine(Flash(new Color32(255, 100, 0, 0), 0.065f));
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        if(bodyCollider != null)
        {
            bodyCollider.enabled = false;
        }

        Destroy(gameObject);
    }

    IEnumerator Flash(Color32 flashColor, float duration)
    {
        if(spriteRenderer == null)
        {
            yield break;
        }

        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = Color.white;
    }

    void SpawnBombEffect(EnemyDefinition.DeathSettings death)
    {
        GameObject bombPrefab = death.bombPrefab != null ? death.bombPrefab : fallbackBombPrefab;
        if(bombPrefab == null)
        {
            return;
        }

        GameObject effect = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        Vector3 scale = transform.lossyScale;
        effect.transform.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f);

        SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null && effectRenderer != null)
        {
            effectRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            effectRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

        float lifeTime = death.bombLifeTime > 0f ? death.bombLifeTime : fallbackBombLifeTime;
        Destroy(effect, lifeTime);
    }

    void PlayOneShot(AudioClip primary, AudioClip fallback, float volume)
    {
        AudioClip clip = primary != null ? primary : fallback;
        if(audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}

using System.Collections;
using UnityEngine;

// 実装意図: HP・被弾・撃破処理を敵共通にし、攻撃側は IDamageable だけを見ればよい構造にする。
public class EnemyHealth : MonoBehaviour, IDamageable
{
    protected Collider2D co;
    protected Rigidbody2D rb;
    protected int HP;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;

    protected bool isDead = false;

    // 実装意図: 既存 prefab の serialized field 名を残し、爆発・SE・スコア設定をそのまま移行する。
    [SerializeField] protected float deathTorque = 10f;
    [SerializeField] protected MonoBehaviour em;
    [SerializeField] protected MonoBehaviour ea;
    [SerializeField] protected float deathcount = 1f;
    [SerializeField] protected float smashX = 8f;
    [SerializeField] protected float smashY = 5f;
    [SerializeField] protected ScoreManager Sm;
    [SerializeField] protected int EnemyScore;
    [SerializeField] protected GameObject Bomb;
    [SerializeField] protected float Bombtimer;

    [SerializeField] protected AudioClip DmgS;
    [SerializeField] protected AudioClip DthS;
    protected AudioSource As;

    EnemyController controller;
    EnemyDeathController deathController;

    protected virtual void Start()
    {
        CacheComponents();
        ResolveScoreManager();

        if(controller == null)
        {
            EnemyController foundController = GetComponent<EnemyController>();
            if(foundController != null)
            {
                Initialize(foundController);
            }
        }
    }

    public virtual void Initialize(EnemyController owner)
    {
        controller = owner;
        CacheComponents();
        ResolveScoreManager();
        ApplyDefinition(owner != null ? owner.Definition : null);
        deathController = new EnemyDeathController();
    }

    public bool Damage(int damage)
    {
        return TakeDamage(damage);
    }

    public virtual bool TakeDamage(int amount)
    {
        // 実装意図: 死亡済みや無効ダメージでは状態遷移を起こさず、攻撃側へ kill 判定だけ返す。
        if(isDead || amount <= 0)
        {
            return false;
        }

        HP -= amount;
        PlayOneShot(DmgS);
        StartCoroutine(Flash());

        bool killed = HP <= 0;
        if(killed)
        {
            Die();
        }

        if(controller != null)
        {
            controller.NotifyDamaged(killed);
        }

        return killed;
    }

    protected virtual void Die()
    {
        if(isDead)
        {
            return;
        }

        isDead = true;

        if(deathController == null)
        {
            deathController = new EnemyDeathController();
        }

        deathController.Play(this, controller);
    }

    public void DestroyYYYYY()
    {
        Destroy(gameObject);
    }

    protected void CacheComponents()
    {
        if(anim == null) anim = GetComponent<Animator>();
        if(co == null) co = GetComponent<Collider2D>();
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if(em == null) em = GetComponent<GroundPatrolMovement>();
        if(ea == null) ea = GetComponent<EnemyAttackController>();
        if(As == null) As = GetComponent<AudioSource>();
        if(spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected void ResolveScoreManager()
    {
        if(Sm != null) return;

        GameObject scoreObject = GameObject.FindWithTag("GameController");
        if(scoreObject != null)
        {
            Sm = scoreObject.GetComponent<ScoreManager>();
        }
    }

    void ApplyDefinition(EnemyDefinition definition)
    {
        if(definition == null)
        {
            if(HP <= 0) HP = 1;
            return;
        }

        HP = definition.MaxHealth;

        EnemyDefinition.DeathSettings death = definition.Death;
        if(death == null) return;

        deathTorque = death.deathTorque;
        deathcount = death.fadeTime;
        smashX = death.smashX;
        smashY = death.smashY;

        EnemyScore = definition.Score;
        if(death.bombPrefab != null) Bomb = death.bombPrefab;
        if(death.bombLifeTime > 0f) Bombtimer = death.bombLifeTime;
        if(death.damageSound != null) DmgS = death.damageSound;
        if(death.deathSound != null) DthS = death.deathSound;
    }

    protected IEnumerator Flash()
    {
        if(spriteRenderer == null) yield break;

        spriteRenderer.color = new Color32(255,150,0,255);
        yield return new WaitForSeconds(0.025f);
        spriteRenderer.color = new Color32(255,255,255,255);
    }

    protected IEnumerator DeathFlash()
    {
        if(spriteRenderer == null) yield break;

        spriteRenderer.color = new Color32(255,100,0,0);
        yield return new WaitForSeconds(0.065f);
        spriteRenderer.color = new Color32(255,255,255,255);
    }

    protected void PlayOneShot(AudioClip clip)
    {
        if(As != null && clip != null)
        {
            As.PlayOneShot(clip,0.5f);
        }
    }

    public class EnemyDeathController
    {
        // 実装意図: 撃破時の演出と物理反応を EnemyHealth から一段分け、死亡処理の置き換え余地を残す。
        public void Play(EnemyHealth health, EnemyController controller)
        {
            if(health == null) return;

            health.SpawnBombEffect();

            if(controller != null)
            {
                controller.EnterDead();
            }
            else if(health.anim != null)
            {
                health.anim.SetTrigger("Death");
            }

            health.PlayOneShot(health.DthS);

            if(health.Sm != null)
            {
                health.Sm.AddScore(health.EnemyScore);
            }

            health.gameObject.layer = LayerMask.NameToLayer("Corpse");

            if(health.em != null) health.em.enabled = false;
            if(health.ea != null) health.ea.enabled = false;

            health.ApplyDeathImpulse();
            health.StartCoroutine(health.Fadeout());
        }
    }

    void ApplyDeathImpulse()
    {
        // 実装意図: プレイヤーの反対方向へ吹き飛ばし、既存の撃破体感を共通死亡処理でも維持する。
        if(rb == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        float dirX = player != null && transform.position.x - player.transform.position.x > 0f ? 1f : -1f;
        Vector2 impulse = new Vector2(dirX,0f) * smashX + Vector2.up * smashY;
        rb.AddForce(impulse, ForceMode2D.Impulse);

        float torqueSign = impulse.x >= 0f ? -1f : 1f;
        rb.AddTorque(deathTorque * torqueSign, ForceMode2D.Impulse);
    }

    IEnumerator Fadeout()
    {
        float elapsed = 0f;
        const float interval = 0.2f;

        while(elapsed < deathcount)
        {
            yield return StartCoroutine(DeathFlash());
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        if(co != null) co.enabled = false;
        DestroyYYYYY();
    }

    void SpawnBombEffect()
    {
        // 実装意図: 敵本体のスケールと描画順を爆発エフェクトへ合わせ、prefab ごとの差を吸収する。
        if(Bomb == null) return;

        GameObject effect = Instantiate(Bomb, transform.position, Quaternion.identity);
        Vector3 scale = transform.lossyScale;
        effect.transform.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f);

        SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null && effectRenderer != null)
        {
            effectRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            effectRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

        Destroy(effect, Bombtimer);
    }
}

// 実装意図: 旧コードや Inspector が参照していたトップレベル名を残すための互換クラス。
public class EnemyDeathController : EnemyHealth.EnemyDeathController
{
}

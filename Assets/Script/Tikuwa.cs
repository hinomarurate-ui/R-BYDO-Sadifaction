using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 // 実装意図: プレイヤー本体として、移動・HP・通常近接・BydoClaw・BydoMissile の入力と実行をまとめる。
 public class Tikuwa : MonoBehaviour
 {
     //インスペクターで設定する
     public float speed; //速度
     public float gravity; //重力
     public float jumpSpeed;//ジャンプする速度
     public float jumpHeight;//高さ制限
     public GroundCheck ground; //接地判定

     [Header("HP")]
     [SerializeField] int maxHP = 100;
     [SerializeField] int currentHP;

     public int MaxHP { get { return maxHP; } }
     public int CurrentHP { get { return currentHP; } }
     public float HpRate { get { return maxHP <= 0 ? 0f : (float)currentHP / maxHP; } }
     public event  System.Action<int, int> OnHpChanged;

     //プライベート変数
     [SerializeField] private float colliderFriction = 0f;
     [SerializeField] private float colliderBounciness = 0f;
     private Collider2D bodyCollider = null;
     private PhysicsMaterial2D runtimeBodyMaterial = null;
     private Animator anim = null;
     private Rigidbody2D rb = null;
     private SpriteRenderer sr = null;
     private bool isGround = false;
     private bool isJump = false;
     private float jumpPos = 0.0f;
     private int clawC = 0;

     private float horizontalKey;
     private bool jumpHeld;
     private bool shotHeld;
     float ySpeed = 0.0f;

     [Header("Melee")]
     [SerializeField] Transform meleePoint;
     [SerializeField] float meleeRadius = 0.6f;
     [SerializeField] LayerMask enemyLayers;
     [SerializeField] int meleeDamage = 1;
     [SerializeField] float meleeCooltime = 0.25f;
     [SerializeField] float hitDelay = 0.03f;
     [SerializeField] float meleeStepSpeed = 10f;
     [SerializeField] float meleeStepDeceleration = 40f; //減速率
     [SerializeField] float meleeKillShakePower = 0.16f;
     [SerializeField] float meleeKillShakeTime = 0.16f;
     [SerializeField] AudioClip ClawS;
     [SerializeField] AudioClip ClawSEnd;
     AudioSource As;

     [Header("BydoClaw")]
     [SerializeField] float bydoClawRadius = 2.0f;
     [SerializeField] int bydoClawDamage = 3;
     [SerializeField] float bydoClawCooltime = 1.0f;
     [SerializeField] float bydoClawHitDelay = 0.05f;
     [SerializeField] float bydoClawStepSpeed = 15f;
     [SerializeField] float bydoClawShakePower = 0.3f;
     [SerializeField] float bydoClawShakeTime = 0.2f;
     [SerializeField] float bydoClawStartTime = 0.12f;
     [SerializeField] int bydoClawCount = 3;
     [SerializeField] float bydoClawDistance = 3f;
     [SerializeField] float bydoClawStepCooltime = 0.08f;
     [SerializeField] float bydoClawInterval = 0.05f;
     [SerializeField] float bydoClawHit = 2.0f;
     [SerializeField] AudioClip Charge;

     [Header("BydoMissile")]
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
     [SerializeField] AudioClip misairu;

     

     

     float lastMeleeTime = -999f;
     float lastBydoClawTime = -999f;
     float lastbydoMissileTime = -999f;
     float currentMeleeStep = 0f;
     bool JumpQueed;
     bool MeleeQueed;
     bool BydoClawQueed;
     float currentBydoClawStep;
     bool bydoMissileQueed;
     bool isExActing;


     void Start()
     {
          // 実装意図: HP/UI 通知と物理素材を開始時に整え、以降の入力処理は cached component を使う。
          //コンポーネントのインスタンスを捕まえる
          anim = GetComponent<Animator>();
          rb = GetComponent<Rigidbody2D>();
          sr = GetComponent<SpriteRenderer>();
          As = GetComponent<AudioSource>();
          bodyCollider = GetComponent<Collider2D>();
          currentHP = maxHP;
          NotifyHpChanged();
          ApplyBodyMaterial();

      }

      public void Damage(int damage)
      {
        // 実装意図: HP の変更通知をここに集約し、UI 側は OnHpChanged だけを見ればよいようにする。
        int beforeHP= currentHP;
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        if(currentHP != beforeHP)
        {
          NotifyHpChanged();
        }
      }

      void NotifyHpChanged()
      {
        if(OnHpChanged != null)
        {
          OnHpChanged(currentHP, maxHP);
        }
      }

      void ApplyBodyMaterial()
      {
        // 実装意図: プレイヤー collider の摩擦を実行時 material に分離し、共有 material を汚さない。
        if(bodyCollider == null) return;

        runtimeBodyMaterial = new PhysicsMaterial2D("PlayerNoFriction");
        runtimeBodyMaterial.friction = colliderFriction;
        runtimeBodyMaterial.bounciness = colliderBounciness;
        bodyCollider.sharedMaterial = runtimeBodyMaterial;
      }


     void Update()
     {
        // 実装意図: 入力は Update でキュー化し、物理反映は FixedUpdate に寄せる。
        horizontalKey = IsAimLockHeld () ? 0f : Input.GetAxisRaw("Horizontal");

        if(!isExActing && Input.GetButtonDown("Jump"))
        JumpQueed = true;

        
        jumpHeld = !isExActing && Input.GetButton("Jump");

        if (!isExActing && Input.GetKeyDown(KeyCode.X))
        {
        PlayOneShot(ClawS);
        MeleeQueed = true;
        }

        if (!isExActing && Input.GetKeyDown(KeyCode.E))
        {
        PlayOneShot(Charge);
        BydoClawQueed = true;
        }

        if (!isExActing && Input.GetKeyDown(KeyCode.Q))
        {
        PlayOneShot(Charge);
        bydoMissileQueed = true;
        }

        shotHeld = !isExActing && Input.GetKey(KeyCode.Z);
        
     }

     bool IsAimLockHeld()
     {
      return Input.GetKey(KeyCode.B) || Input.GetKey(KeyCode.JoystickButton1);
     }

      void FixedUpdate()
      {
          // 実装意図: 移動・ジャンプ・EX 行動の速度合成を 1 箇所に集め、Rigidbody2D への代入を最後にまとめる。
          //接地判定を得る
          isGround = ground.IsGround();

          //キー入力されたら行動する
          float xSpeed = horizontalKey * speed;

          if (isGround)
          {
              if (!isExActing && JumpQueed)
              {
                  JumpQueed = false;
                  ySpeed = jumpSpeed;
                  jumpPos = transform.position.y; //ジャンプした位置を記録する
                  isJump = true;
                  anim.SetBool("jump", true);
              }
              else
              {
                  ySpeed = 0;
                  isJump = false;
                  anim.SetBool("jump", false);
              }
          }
          else if (isJump)
          {
              //上ボタンを押されている。かつ、現在の高さがジャンプした位置から自分の決めた位置より下ならジャンプを継続する
              if (jumpHeld && jumpPos + jumpHeight > transform.position.y)
              {
                  ySpeed = jumpSpeed;
              }
              else
              {
                ySpeed = -gravity;
                  isJump = false;
              }
          }
          else
          {
                ySpeed = -gravity;
              }
          if(!isExActing && MeleeQueed){

            MeleeQueed = false;
            TryMelee();
          }

          if(!isExActing && BydoClawQueed){

            BydoClawQueed = false;
            TryBydoClaw();
          }

          if(!isExActing && bydoMissileQueed){

            bydoMissileQueed = false;
            TryBydoMissile();
          }

          if (isExActing)
          {
            anim.SetBool("run",false);
            xSpeed = 0f;
          }

          else if (horizontalKey > 0)
          {
              transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
              anim.SetBool("run", true);
              xSpeed = speed;
          }
          else if (horizontalKey < 0)
          {
              transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f);
              anim.SetBool("run", true);
              xSpeed = -speed;
          }
          else
          {
              anim.SetBool("run", false);
              xSpeed = 0.0f;
          }
          if(shotHeld){
            anim.SetBool("Shot", true);
          }
          else{
            anim.SetBool("Shot", false);
          }

          if (currentMeleeStep != 0f)
          {
            currentMeleeStep = Mathf.MoveTowards(currentMeleeStep, 0f, meleeStepDeceleration * Time.fixedDeltaTime);
          }
          xSpeed += currentMeleeStep + currentBydoClawStep;

          rb.velocity = new Vector2(xSpeed, ySpeed);
      }

      IEnumerator ClawAnimation(){
        // 実装意図: 通常近接はアニメーション開始後に少し遅れて判定を出し、見た目と当たりを合わせる。
        if(clawC == 0) anim.SetBool("Claw", true);
        else anim.SetBool("ClawP", true); 

        if (hitDelay > 0f) yield return new WaitForSeconds(hitDelay);

        DoMeleeHit();
        float rest = Mathf.Max(0f, bydoClawStepCooltime - hitDelay);
        if (rest > 0f) yield return new WaitForSeconds(rest);

        anim.SetBool("Claw", false); 
        anim.SetBool("ClawP", false); 
      }

      IEnumerator BydoClawCombo()
      {
        // 実装意図: BydoClaw 中は通常入力を止め、連続ステップ攻撃を 1 つの排他行動として扱う。
        isExActing = true;

        JumpQueed = false;
        MeleeQueed = false;
        BydoClawQueed = false;
        currentMeleeStep = 0f;
        currentBydoClawStep = 0f;

        anim.SetBool("Shot", false);
        anim.SetBool("run", false);
        anim.SetBool("Claw", false);
        anim.SetBool("ClawP", false);
        anim.SetBool("EXClaw", true);

        
        

        if (bydoClawStartTime > 0f)
        {
          
          yield return new WaitForSeconds(bydoClawStartTime);
        }

        for (int i = 0; i < bydoClawCount; i++)
        {
          anim.SetInteger("EXClawC",1+i);
          PlayOneShot(ClawS);
          yield return StartCoroutine(BydoClawStep());

          if(i < bydoClawCount - 1 && bydoClawInterval > 0f)
          {
            yield return new WaitForSeconds(bydoClawInterval);
            
          }
          
        }

        PlayOneShot(ClawSEnd);

        currentBydoClawStep = 0f;
        anim.SetBool("EXClaw", false);
        yield return new WaitForSeconds(bydoClawInterval);
        anim.SetInteger("EXClawC",0);

        isExActing = false;

      }

      IEnumerator ChargeAnimation(){
        anim.SetBool("EXClaw", true);
        yield return new WaitForSeconds(2f);

        }

      IEnumerator BydoClawStep(){
        // 実装意図: 1 段ごとに前進速度とヒット判定のタイミングを分け、連撃の手触りを作る。

        float stepSpeed = 0f;
        if (bydoClawStepCooltime > 0f)
        {
          stepSpeed = bydoClawDistance / bydoClawStepCooltime;
        }

        currentBydoClawStep = stepSpeed;
        
        float hitDelayClamped = Mathf.Clamp(bydoClawHitDelay, 0f, bydoClawStepCooltime);

        if (hitDelayClamped > 0f) 
       {

        yield return new WaitForSeconds(hitDelayClamped);

       }
        
        DoBydoClawHit(bydoClawRadius, bydoClawDamage);
        float rest = Mathf.Max(0f, 0.1f - bydoClawHitDelay);
        if (rest > 0f) yield return new WaitForSeconds(rest);

        currentBydoClawStep = 0f;

        
      }

      IEnumerator BydoMissile()
      {
        // 実装意図: ミサイル発射中はプレイヤー操作を一時停止し、専用 pose sprite でチャージ/発射を表現する。
        isExActing = true;
        JumpQueed = false;
        MeleeQueed = false;
        BydoClawQueed = false;
        bydoMissileQueed = false;
        currentMeleeStep = 0f;
        currentBydoClawStep = 0f;

        anim.SetBool("Shot", false);
        anim.SetBool("run", false);
        anim.SetBool("Claw", false);
        anim.SetBool("ClawP", false);
        anim.SetBool("EXClaw", false);
        anim.SetInteger("EXClawC", 0);
        SetBydoMissilePose(bydoMissileChargeSprite);

        if (bydoMissileStartTime > 0f)
        {
          yield return new WaitForSeconds(bydoMissileStartTime);
        }

        SetBydoMissilePose(bydoMissileAttackSprite);

        for (int i = 0; i < bydoMissileCount; i++)
        {
          SpawnBydoMissile(i);
          PlayOneShot(misairu);

          if (i < bydoMissileCount - 1 && bydoMissileInterval > 0f)
          {
            yield return new WaitForSeconds(bydoMissileInterval);
          }
        }

        if (bydoMissileInterval > 0f)
        {
          yield return new WaitForSeconds(bydoMissileInterval);
        }

        ResetAnimatorPose();
        isExActing = false;
      }
      

    void TryMelee()
    {
        // 実装意図: 通常近接は軽い cooldown と交互モーションで連打感を作る。
        if(Time.time < lastMeleeTime + meleeCooltime) return;
        lastMeleeTime = Time.time;

        currentMeleeStep = Mathf.Sign(transform.localScale.x) * meleeStepSpeed;

        clawC = (clawC + 1) % 2;
        StartCoroutine(ClawAnimation());
    }

    void TryBydoClaw()
    {
        // 実装意図: EX 行動の多重起動を防ぎ、BydoClaw の coroutine を単独で走らせる。
        if(isExActing)return;
        if(Time.time < lastBydoClawTime + bydoClawCooltime) return;
        lastBydoClawTime = Time.time;

        StartCoroutine(BydoClawCombo());



      }

      void TryBydoMissile()
      {
        // 実装意図: ミサイルも EX 行動として排他にし、他の攻撃 coroutine と重ならないようにする。
        if(isExActing)return;
        if(Time.time < lastbydoMissileTime + bydoMissileCooltime) return;
        lastbydoMissileTime = Time.time;

        StartCoroutine(BydoMissile());
      }

      void SetBydoMissilePose(Sprite poseSprite)
      {
        // 実装意図: 既存 Animator にミサイル専用 state を増やさず、発射中だけ SpriteRenderer を直接差し替える。
        if(sr == null || poseSprite == null) return;

        if(anim != null)
        {
          anim.enabled = false;
        }

        sr.sprite = poseSprite;
      }

      void ResetAnimatorPose()
      {
        if(anim != null)
        {
          anim.enabled = true;
        }
      }

      void SpawnBydoMissile(int missileIndex)
      {
        // 実装意図: 複数発を上下にずらして出し、各 Missile に追尾対象条件とダメージを渡す。
        if (bydoMissilePrefab == null) return;

        float dirX = Mathf.Sign(transform.localScale.x);
        if (dirX == 0f) dirX = 1f;

        Transform shotPoint = transform.Find("ShotPosition");
        Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;

        float centeredIndex = missileIndex - ((bydoMissileCount - 1) * 0.5f);
        Vector3 spawnOffset = new Vector3(-0.35f * dirX, 1.2f + Mathf.Abs(centeredIndex) * 0.35f, 0f);
        Vector3 spawnPosition = origin + spawnOffset;

        GameObject missileObject = Instantiate(bydoMissilePrefab, spawnPosition, Quaternion.identity);
        Missile missile = missileObject.GetComponent<Missile>();
        if (missile == null)
        {
          Destroy(missileObject);
          return;
        }

        Vector2 initialDirection = new Vector2(dirX, -0.15f * centeredIndex).normalized;
        missile.Init(initialDirection, dirX, bydoMissileDamage, enemyLayers, bydoMissileSeekRange, bydoMissileKillShakePower, bydoMissileKillShakeTime);
      }

      void DoMeleeHit()
      {
        DamageEnemiesInCircle(meleeRadius, meleeDamage, meleeKillShakePower, meleeKillShakeTime);
      }

      void DoBydoClawHit(float hitRadius, int damage)
      {
        DamageEnemiesInCircle(hitRadius, damage, bydoClawShakePower, bydoClawShakeTime);
      }

      void DamageEnemiesInCircle(float hitRadius, int damage, float shakePower, float shakeTime)
      {
        // 実装意図: 敵の実装クラスではなく IDamageable を叩き、同一対象への多重ヒットを防ぐ。
        if (meleePoint == null) return;

        var hits = Physics2D.OverlapCircleAll(meleePoint.position, hitRadius, enemyLayers);

        var damaged = new System.Collections.Generic.HashSet<IDamageable>();

        foreach (var h in hits)
        {
            if (h == null) continue;

            IDamageable damageable = h.GetComponentInParent<IDamageable>();
            if(damageable == null || damaged.Contains(damageable)) continue;

            damaged.Add(damageable);
            if(damageable.TakeDamage(damage))
            {
              ShakeScreen.Shake(shakePower,shakeTime);
            }
            
        }
      }

      void PlayOneShot(AudioClip clip)
      {
        if(As != null && clip != null)
        {
          As.PlayOneShot(clip,0.5f);
        }
      }
 }

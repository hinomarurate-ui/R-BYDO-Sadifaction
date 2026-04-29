using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 public class Tikuwa : MonoBehaviour
 {
     //インスペクターで設定する
     public float speed; //速度
     public float gravity; //重力
     public float jumpSpeed;//ジャンプする速度
     public float jumpHeight;//高さ制限
     public GroundCheck ground; //接地判定

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
     [SerializeField] float meleeStepDeceleration = 40f;
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
          //コンポーネントのインスタンスを捕まえる
          anim = GetComponent<Animator>();
          rb = GetComponent<Rigidbody2D>();
          sr = GetComponent<SpriteRenderer>();
          As = GetComponent<AudioSource>();
          bodyCollider = GetComponent<Collider2D>();
          ApplyBodyMaterial();

      }

      void ApplyBodyMaterial()
      {
        runtimeBodyMaterial = new PhysicsMaterial2D("PlayerNoFriction");
        runtimeBodyMaterial.friction = colliderFriction;
        runtimeBodyMaterial.bounciness = colliderBounciness;
        bodyCollider.sharedMaterial = runtimeBodyMaterial;
      }


     void Update()
     {
        horizontalKey = Input.GetAxisRaw("Horizontal");

        if(!isExActing && Input.GetButtonDown("Jump"))
        JumpQueed = true;

        
        jumpHeld = !isExActing && Input.GetButton("Jump");

        if (!isExActing && Input.GetKeyDown(KeyCode.X))
        {
        As.PlayOneShot(ClawS,0.5f);
        MeleeQueed = true;
        }

        if (!isExActing && Input.GetKeyDown(KeyCode.E))
        {
        As.PlayOneShot(Charge,0.5f);
        BydoClawQueed = true;
        }

        if (!isExActing && Input.GetKeyDown(KeyCode.Q))
        {
        As.PlayOneShot(Charge,0.5f);
        bydoMissileQueed = true;
        }

        shotHeld = !isExActing && Input.GetKey(KeyCode.Z);
        
     }

      void FixedUpdate()
      {
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
              transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
              anim.SetBool("run", true);
              xSpeed = speed;
          }
          else if (horizontalKey < 0)
          {
              transform.localScale = new Vector3(-0.55f, 0.55f, 0.55f);
              anim.SetBool("run", true);
              xSpeed = -speed;
          }
          else
          {
              anim.SetBool("run", false);
              xSpeed = 0.0f;
          }
          if(Input.GetKey(KeyCode.Z)){
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
          As.PlayOneShot(ClawS,0.5f);
          yield return StartCoroutine(BydoClawStep());

          if(i < bydoClawCount - 1 && bydoClawInterval > 0f)
          {
            yield return new WaitForSeconds(bydoClawInterval);
            
          }
          
        }

        As.PlayOneShot(ClawSEnd,0.5f);

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
          As.PlayOneShot(misairu,0.5f);

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
        if(Time.time < lastMeleeTime + meleeCooltime) return;
        lastMeleeTime = Time.time;

        currentMeleeStep = Mathf.Sign(transform.localScale.x) * meleeStepSpeed;

        clawC = (clawC + 1) % 2;
        StartCoroutine(ClawAnimation());
    }

    void TryBydoClaw()
    {
        if(isExActing)return;
        if(Time.time < lastBydoClawTime + bydoClawCooltime) return;
        lastBydoClawTime = Time.time;

        StartCoroutine(BydoClawCombo());



      }

      void TryBydoMissile()
      {
        if(isExActing)return;
        if(Time.time < lastbydoMissileTime + bydoMissileCooltime) return;
        lastbydoMissileTime = Time.time;

        StartCoroutine(BydoMissile());
      }

      void SetBydoMissilePose(Sprite poseSprite)
      {
        anim.enabled = false;
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
        if (bydoMissilePrefab == null) return;

        float dirX = Mathf.Sign(transform.localScale.x);
        if (dirX == 0f) dirX = 1f;

        Transform shotPoint = transform.Find("ShotPosition");
        Vector3 origin = shotPoint != null ? shotPoint.position : transform.position;

        float centeredIndex = missileIndex - ((bydoMissileCount - 1) * 0.5f);
        Vector3 spawnOffset = new Vector3(-0.35f * dirX, 1.2f + Mathf.Abs(centeredIndex) * 0.35f, 0f);
        Vector3 spawnPosition = origin + spawnOffset;

        GameObject missileObject = Instantiate(bydoMissilePrefab, spawnPosition, Quaternion.identity);
        Missile Missile = missileObject.GetComponent<Missile>();

        Vector2 initialDirection = new Vector2(dirX, -0.15f * centeredIndex).normalized;
        Missile.Init(initialDirection, dirX, bydoClawDamage, enemyLayers, bydoMissileSeekRange, bydoMissileKillShakePower, bydoMissileKillShakeTime);
        //a
      }

      void DoMeleeHit()
      {
        if (meleePoint == null) return;

        var hits = Physics2D.OverlapCircleAll(meleePoint.position, meleeRadius, enemyLayers);

        var damaged = new System.Collections.Generic.HashSet<GameObject>();

        foreach (var h in hits)
        {
            if (h == null) continue;
            var go = h.gameObject;
            if (damaged.Contains(go)) continue;
            damaged.Add(go);

            var hp = h.GetComponent<EnemyHP>();
            if(hp != null && hp.Damage(meleeDamage))
            {
              ShakeScreen.Shake(meleeKillShakePower,meleeKillShakeTime);
            }
            
        }
      }

      void DoBydoClawHit(float hitRadius, int damage)
      {
        if (meleePoint == null) return;

        var hits = Physics2D.OverlapCircleAll(meleePoint.position, hitRadius, enemyLayers);

        var damaged = new System.Collections.Generic.HashSet<GameObject>();

        foreach (var h in hits)
        {
            if (h == null) continue;
            var go = h.gameObject;
            if (damaged.Contains(go)) continue;
            damaged.Add(go);

            var hp = h.GetComponent<EnemyHP>();
            if(hp != null && hp.Damage(meleeDamage))
            {
              ShakeScreen.Shake(bydoClawShakePower,bydoClawShakeTime);
            }
            
        }
      }
 }
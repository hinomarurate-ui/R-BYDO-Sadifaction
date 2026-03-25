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
     private Animator anim = null;
     private Rigidbody2D rb = null;
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
     [SerializeField] AudioClip ClawS;
     AudioSource As;

     [Header("BydoClaw")]
     [SerializeField] float bydoClawRadius = 2.0f;
     [SerializeField] int bydoClawDamage = 3;
     [SerializeField] float bydoClawCooltime = 1.0f;
     [SerializeField] float bydoClawHitDelay = 0.05f;
     [SerializeField] float bydoClawStepSpeed = 15f;

     [SerializeField] float bydoClawStartTime = 0.12f;
     [SerializeField] int bydoClawCount = 3;
     [SerializeField] float bydoClawDistance = 3f;
     [SerializeField] float bydoClawStepCooltime = 0.08f;
     [SerializeField] float bydoClawInterval = 0.05f;
     [SerializeField] float bydoClawHit = 2.0f;
     [SerializeField] AudioClip Charge;
     

     float lastMeleeTime = -999f;
     float lastBydoClawTime = -999f;
     float currentMeleeStep = 0f;
     bool JumpQueed;
     bool MeleeQueed;
     bool BydoClawQueed;
     float currentBydoClawStep;
     bool isExActing;


     void Start()
     {
          //コンポーネントのインスタンスを捕まえる
          anim = GetComponent<Animator>();
          rb = GetComponent<Rigidbody2D>();
          As = GetComponent<AudioSource>();

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
            ySpeed = 0;
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
          if(!isExActing && MeleeQueed){

            MeleeQueed = false;
            TryMelee();
          }

          if(!isExActing && BydoClawQueed){

            BydoClawQueed = false;
            TryBydoClaw();
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
            if(hp != null) hp.Damage(meleeDamage);
            
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
            if(hp != null) hp.Damage(damage);
            
        }
      }
 }
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
     [SerializeField] AudioClip ClawS;
     AudioSource As;

     float lastMeleeTime = -999f;
     bool JumpQueed;
     bool MeleeQueed;


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

        if(Input.GetButtonDown("Jump"))
        JumpQueed = true;

        
        jumpHeld = Input.GetButton("Jump");

        if (Input.GetKeyDown(KeyCode.X))
        {
        As.PlayOneShot(ClawS,0.5f);
        MeleeQueed = true;
        }

        shotHeld = Input.GetKey(KeyCode.Z);
        
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
              if (JumpQueed)
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
          if(MeleeQueed){

            MeleeQueed = false;
            TryMelee();
          }
          if (horizontalKey > 0)
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
          if(Input.GetKey(KeyCode.Z)){
            anim.SetBool("Shot", true);
          }
          else{
            anim.SetBool("Shot", false);
          }
          rb.velocity = new Vector2(xSpeed, ySpeed);
      }

      IEnumerator ClawAnimation(){
        if(clawC == 0) anim.SetBool("Claw", true);
        else anim.SetBool("ClawP", true); 

        if (hitDelay > 0f) yield return new WaitForSeconds(hitDelay);

        DoMeleeHit();
        float rest = Mathf.Max(0f, 0.1f - hitDelay);
        if (rest > 0f) yield return new WaitForSeconds(rest);

        anim.SetBool("Claw", false); 
        anim.SetBool("ClawP", false); 
        }
      

    void TryMelee()
      {
        if(Time.time < lastMeleeTime + meleeCooltime) return;
        lastMeleeTime = Time.time;

        clawC = (clawC + 1) % 2;
        StartCoroutine(ClawAnimation());
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
 }
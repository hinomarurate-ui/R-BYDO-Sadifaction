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

     void Start()
     {
          //コンポーネントのインスタンスを捕まえる
          anim = GetComponent<Animator>();
          rb = GetComponent<Rigidbody2D>();
      }

      void FixedUpdate()
      {
          //接地判定を得る
          isGround = ground.IsGround();

          //キー入力されたら行動する
          float horizontalKey = Input.GetAxis("Horizontal");
          float xSpeed = 0.0f;
          float ySpeed = -gravity;
          float verticalKey = Input.GetAxis("Vertical");

          if (isGround)
          {
              if (verticalKey > 0)
              {
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
              if (verticalKey > 0 && jumpPos + jumpHeight > transform.position.y)
              {
                  ySpeed = jumpSpeed;
              }
              else
              {
                  isJump = false;
              }
          }
          if(Input.GetKeyDown(KeyCode.X)){
            clawC += 1;
            clawC = clawC%2;
            StartCoroutine(ClawAnimation());
            
          }
          if (horizontalKey > 0)
          {
              transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
              anim.SetBool("run", true);
              xSpeed = speed;
          }
          else if (horizontalKey < 0)
          {
              transform.localScale = new Vector3(-0.7f, 0.7f, 0.7f);
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
        if(clawC == 0){
        anim.SetBool("Claw", true);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("Claw", false);
        }
        else{
        anim.SetBool("ClawP", true);
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("ClawP", false); 
        }
      }
 }
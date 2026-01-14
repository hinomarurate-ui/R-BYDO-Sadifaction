using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    [SerializeField]
    float MoveSpeed;
    Rigidbody2D rb;
    [SerializeField]
    float Dirx;
    [SerializeField]
    Enemy enemy;
    [SerializeField]
    float JumpImpulse;
    [SerializeField]
    float JumpPower;
    [SerializeField]
    float JumpSlow;
    [SerializeField]
    float MaxHeight;

    float JumpstartY;

    bool Jumping;

    private Animator anim = null;
    

    public void Init(EnemyStatus ES)
    {
        MoveSpeed = ES.MoveSpeed;
    }

    void Start()
    {
        Jumping = false;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
    }
    public void MoveX(float Dirx)
    {
        anim.SetBool("Jump", false);
        rb.velocity = new Vector2(Dirx * MoveSpeed, rb.velocity.y);
    }


    void Update()
    {
        if(enemy.isGround)
        {
            Jumping = false;
            if(enemy.isGake)
            {
                if(!Jumping)
                StartCoroutine(JumpCharge());
                
            }
            else
            {
                MoveX(Dirx);
            }
        }

        
        
    }

    void FixedUpdate()
    {
        float startY = JumpstartY + JumpSlow;
        float endY = JumpstartY + MaxHeight;

        if(Jumping != true)
        {
            return;
        }

        if(rb.velocity.y >= 0f)
        {
            if(rb.position.y < startY)
            {
                rb.AddForce(Vector2.up * JumpPower,ForceMode2D.Force);
            }
            else
            {
                float T = Mathf.InverseLerp(startY,endY,rb.position.y);
                float thrust = Mathf.Lerp(JumpPower,0,T);
                rb.AddForce(Vector2.up * thrust, ForceMode2D.Force);
            }

        }
    }
    void Jump(float Dirx)
    {
        Jumping = true;
        anim.SetBool("Jump", true);
        anim.SetBool("JumpC", false);
        rb.AddForce(Vector2.up * JumpImpulse,ForceMode2D.Impulse);
        rb.velocity = new Vector2(Dirx * MoveSpeed, rb.velocity.y);
    }
    IEnumerator JumpCharge()
    {
        JumpstartY = rb.position.y;
        anim.SetBool("JumpC", true);
        yield return new WaitForSeconds(0.5f);
        Jump(Dirx);
         yield return new WaitForSeconds(0.3f);
    }

    void Fall()
    {
        rb.velocity = new Vector2(Dirx * MoveSpeed, rb.velocity.y - 0.3f);
    }
}

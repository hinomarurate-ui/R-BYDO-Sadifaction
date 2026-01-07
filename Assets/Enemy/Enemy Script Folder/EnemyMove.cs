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
    float JumpV;

    private Animator anim = null;
    

    public void Init(EnemyStatus ES)
    {
        MoveSpeed = ES.MoveSpeed;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
    }
    public void MoveX(float Dirx)
    {
        Debug.Log("ばなな");
        anim.SetBool("Jump", false);
        rb.velocity = new Vector2(Dirx * MoveSpeed, rb.velocity.y);
    }


    void Update()
    {
        if(enemy.isGround)
        {
            if(enemy.isGake)
            {
                StartCoroutine(JumpCharge());
                
            }
            else
            {
                MoveX(Dirx);
            }
        }
        
    }

    void Jump(float Dirx)
    {
        anim.SetBool("Jump", true);
        anim.SetBool("JumpC", false);
        rb.velocity = new Vector2(Dirx * MoveSpeed, JumpV);
    }
    IEnumerator JumpCharge()
    {
        anim.SetBool("JumpC", true);
        yield return new WaitForSeconds(0.5f);
        Jump(Dirx);
         yield return new WaitForSeconds(0.3f);
    }
}

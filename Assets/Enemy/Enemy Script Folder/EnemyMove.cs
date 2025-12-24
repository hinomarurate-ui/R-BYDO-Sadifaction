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
    

    public void Init(EnemyStatus ES)
    {
        MoveSpeed = ES.MoveSpeed;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void MoveX(float Dirx)
    {
        rb.velocity = new Vector2(Dirx * MoveSpeed, rb.velocity.y);
    }


    void Update()
    {
        MoveX(Dirx);
        
    }
}

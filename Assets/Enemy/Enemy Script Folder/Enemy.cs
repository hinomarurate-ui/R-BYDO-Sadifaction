using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GroundCheck ground; //接地判定
    [SerializeField]
    EnemyStatus Status;
    [SerializeField]
    EnemyHP HP;
    [SerializeField]
    EnemyMove Move;
    private bool isGround = false;
    // Start is called before the first frame update
    void Awake()
    {
        HP.Init(Status);
        Move.Init(Status);

    }

        void FixedUpdate()
      {
          //接地判定を得る
          isGround = ground.IsGround();
      }

}

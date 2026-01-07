using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GroundCheck ground; //接地判定
    public GakeChecker gake; //接地判定
    [SerializeField]
    EnemyStatus Status;
    [SerializeField]
    EnemyHP HP;
    [SerializeField]
    EnemyMove Move;
    public bool isGround = false;
    public bool isGake = false;
    // Start is called before the first frame update
    void Awake()
    {
        HP.Init(Status);
        Move.Init(Status);
        ground = transform.Find("GroundChecker").gameObject.GetComponent<GroundCheck>();
        gake = transform.Find("GakeChecker").gameObject.GetComponent<GakeChecker>();

    }

        void FixedUpdate()
      {
          //接地判定を得る
          isGround = ground.IsGround();
          isGake = gake.IsGake();
      }

}

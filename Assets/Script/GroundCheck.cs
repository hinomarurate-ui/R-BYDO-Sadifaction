using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
     private string groundTag = "Ground";
     float Yuka = 0.05f;
     private bool isGround = false; 
     private bool isGroundEnter, isGroundStay, isGroundExit;
     readonly HashSet<Collider2D> TouchGround = new HashSet<Collider2D>();

     Collider2D ownCollider;

    void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
    }

     //接地判定を返すメソッド
　　　//物理判定の更新毎に呼ぶ必要がある
     public bool IsGround()
     {    
          if (ownCollider == null)
          {
            return false;
          }

          TouchGround.RemoveWhere(collision => collision == null || !collision.enabled);

          foreach (Collider2D collision in TouchGround)
          {
            if(IsGroundSurface(collision))
            {
                return true;
            }
          }
          return false;
     }

     private bool IsGroundSurface(Collider2D collision)
     {
        if(collision == null || !collision.CompareTag(groundTag))
        {
            return false;
        }

        float checkerTopY = ownCollider.bounds.max.y;
        float groundsTopY = collision.bounds.max.y;
        return groundsTopY <= checkerTopY + Yuka;
     }

     private void OnTriggerEnter2D(Collider2D collision)
     
     {
        TouchGround.Add(collision);

     }

     private void OnTriggerStay2D(Collider2D collision)
     {
          TouchGround.Add(collision);
     }

     private void OnTriggerExit2D(Collider2D collision)
     {
          TouchGround.Remove(collision);
     }
 }
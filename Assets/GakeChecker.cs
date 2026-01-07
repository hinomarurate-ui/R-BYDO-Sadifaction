using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GakeChecker : MonoBehaviour

{
     private string gakeTag = "Gake";
     private bool isGake = false; 
     private bool isGakeEnter, isGakeStay, isGakeExit;

     //接地判定を返すメソッド
　　　//物理判定の更新毎に呼ぶ必要がある
     public bool IsGake()
     {    
          if (isGakeEnter || isGakeStay)
          {
              isGake = true;
          }
          else if (isGakeExit)
          {
              isGake = false;
          }

          isGakeEnter = false;
          isGakeStay = false;
          isGakeExit = false;
          return isGake;
     }

     private void OnTriggerEnter2D(Collider2D collision)
     {
          if (collision.tag == gakeTag)
          {
              isGakeEnter = true;
          }
     }

     private void OnTriggerStay2D(Collider2D collision)
     {
          if (collision.tag == gakeTag)
          {
              isGakeStay = true;
          }
     }

     private void OnTriggerExit2D(Collider2D collision)
     {
          if (collision.tag == gakeTag)
          {
              isGakeExit = true;
          }
     }
 }
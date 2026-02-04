using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    Collider2D co;
    Rigidbody2D rb;
    int HP;
    private Animator anim = null;

    void Start()
    {
       anim = GetComponent<Animator>();
       co = GetComponent<Collider2D>();
       rb = GetComponent<Rigidbody2D>();

    }


    public void Init(EnemyStatus ES)
    {
        HP = ES.MaxHP;
    }

    public void Damage(int damage)
    {
        HP -= damage;
        StartCoroutine(Flash());
        if(HP < 0){
            DeathAnim();
        }
    }

    void DeathAnim()
    {
        //this.Physics2DLayerMask = Default;
        anim.SetTrigger("Death");

        Vector2 impulse = new Vector2(1,0f) * 8 + Vector2.up * 5;
        rb.AddForce(impulse, ForceMode2D.Impulse);


    }

    public void DestroyYYYYY()
    {
        Destroy(gameObject);
    }

    private IEnumerator Flash()
    {
        SpriteRenderer Texture = GetComponent<SpriteRenderer>();
        Texture.color = new Color32(255,150,0,255);

        yield return new WaitForSeconds(0.01f);
        Texture.color = new Color32(255,255,255,255);

    }

}

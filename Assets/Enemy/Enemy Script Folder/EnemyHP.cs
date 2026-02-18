using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    Collider2D co;
    Rigidbody2D rb;
    int HP;
    private Animator anim = null;

    [SerializeField] float deathTorque = 10f;
    [SerializeField] EnemyMove em;
    [SerializeField] float deathcount = 1f;
    [SerializeField] float smashX = 8f;
    [SerializeField] float smashY = 5f;
    [SerializeField] ScoreManager Sm;
    [SerializeField] int EnemyScore;

    [SerializeField] AudioClip DmgS;
    [SerializeField] AudioClip DthS;
    AudioSource As;

    void Start()
    {
       anim = GetComponent<Animator>();
       co = GetComponent<Collider2D>();
       rb = GetComponent<Rigidbody2D>();
       em = GetComponent<EnemyMove>();
       As = GetComponent<AudioSource>();
       Sm = GameObject.FindWithTag("GameController").GetComponent<ScoreManager>();

    }


    public void Init(EnemyStatus ES)
    {
        HP = ES.MaxHP;
    }

    public void Damage(int damage)
    {
        HP -= damage;
        As.PlayOneShot(DmgS,0.5f);
        StartCoroutine(Flash());
        if(HP < 0){
            DeathAnim();
        }
    }

    void DeathAnim()
    {
        //this.Physics2DLayerMask = Default;
        anim.SetTrigger("Death");
        As.PlayOneShot(DthS,0.5f);
        Sm.AddScore(EnemyScore);
        gameObject.layer = LayerMask.NameToLayer("Corpse");
        em.enabled = false;

        Vector2 impulse = new Vector2(1,0f) *  smashX + Vector2.up * smashY;
        rb.AddForce(impulse, ForceMode2D.Impulse);

        float torqueSign = (impulse.x >= 0f) ? -1f : 1f;
        rb.AddTorque(deathTorque * torqueSign, ForceMode2D.Impulse);

        StartCoroutine (fadeout());


    }

    IEnumerator fadeout()
    {
        float elapsed = 0f;
        float interval = 0.2f;

        while (elapsed < deathcount)
        {  
        yield return StartCoroutine(DeathFlash());
        yield return new WaitForSeconds(interval);
        elapsed += interval;
        }
        co.enabled = false;
        DestroyYYYYY();
    }

    public void DestroyYYYYY()
    {
        Destroy(gameObject);
    }

    private IEnumerator Flash()
    {
        SpriteRenderer Texture = GetComponent<SpriteRenderer>();
        Texture.color = new Color32(255,150,0,255);

        yield return new WaitForSeconds(0.025f);
        Texture.color = new Color32(255,255,255,255);

    }

    private IEnumerator DeathFlash()
    {
        SpriteRenderer Texture = GetComponent<SpriteRenderer>();
        Texture.color = new Color32(255,100,0,0);

        yield return new WaitForSeconds(0.065f);
        Texture.color = new Color32(255,255,255,255);

    }

}

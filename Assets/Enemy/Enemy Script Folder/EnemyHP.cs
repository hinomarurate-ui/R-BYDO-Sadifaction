using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    int HP;

    public void Init(EnemyStatus ES)
    {
        HP = ES.MaxHP;
    }

    public void Damage(int damage)
    {
        HP -= damage;
        StartCoroutine(Flash());
        if(HP < 0){
            Destroy(gameObject);
        }
    }

    private IEnumerator Flash()
    {
        SpriteRenderer Texture = GetComponent<SpriteRenderer>();
        Texture.color = new Color32(255,150,0,255);

        yield return new WaitForSeconds(0.01f);
        Texture.color = new Color32(255,255,255,255);

    }

}

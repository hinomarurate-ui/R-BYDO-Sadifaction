using UnityEngine;

// 実装意図: プレイヤーが指定範囲へ入った瞬間に、近くの EnemyActivator 群を一度だけ起こす。
public class EnemyActivationTrigger : MonoBehaviour
{
    BoxCollider2D triggerZone;
    [SerializeField] bool hasActivated = false;
    [SerializeField] EnemyActivator[] enemies;
    // Start is called before the first frame update
    void Reset()
    {
        ConfigureTriggerZone();
        
    }

    void Awake()
    {
        ConfigureTriggerZone();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 実装意図: 敵弾や他 collider では起動せず、プレイヤー本体の侵入だけを起点にする。
        if(hasActivated)
        {
            return;
        }

        Tikuwa player = collision.GetComponentInParent<Tikuwa>();
        if(player == null)
        {
            return;
        }

        ActivateNearbyEnemies();
    }

    // Update is called once per frame
    void ActivateNearbyEnemies()
    {
        if(enemies != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if(enemies[i] != null)
                {
                    enemies[i].ActivateEnemy();
                }
            }
        }

        hasActivated = true;
        if(triggerZone != null)
        {
            triggerZone.enabled = false;
        }
        
    }

    void ConfigureTriggerZone()
    {
        // 実装意図: Inspector で trigger 設定を忘れても、起動判定用 collider として動くよう補正する。
        triggerZone = GetComponent<BoxCollider2D>();
        if(triggerZone != null)
        {
            triggerZone.isTrigger = true;
        }
    }
}

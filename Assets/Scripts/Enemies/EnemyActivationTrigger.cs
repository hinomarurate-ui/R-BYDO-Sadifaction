using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    BoxCollider2D triggerZone;
    [SerializeField] bool hasActivated = false;
    [SerializeField] EnemyActivator[] enemies;
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
        if(hasActivated)
        {
            return;
        }

        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if(player == null)
        {
            return;
        }

        ActivateNearbyEnemies();
    }

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
        triggerZone = GetComponent<BoxCollider2D>();
        if(triggerZone != null)
        {
            triggerZone.isTrigger = true;
        }
    }
}

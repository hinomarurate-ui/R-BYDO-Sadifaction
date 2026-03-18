using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    BoxCollider2D triggerZone;
    bool hasActivated = false;
    // Start is called before the first frame update
    void Reset()
    {
        triggerZone = GetComponent<BoxCollider2D>();

        triggerZone.isTrigger = true;
        
    }

    void Awake()
    {
        triggerZone = GetComponent<BoxCollider2D>();

        triggerZone.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
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
        Bounds zoneBounds = triggerZone.bounds;
        EnemyActivator[] enemies = FindObjectsOfType<EnemyActivator>();

        for (int i = 0; i < enemies.Length; i++)
        {
            if(zoneBounds.Contains(enemies[i].transform.position))
            {
                enemies[i].ActivateEnemy();
            }
        }

        hasActivated = true;
        triggerZone.enabled = false;
        
    }
}

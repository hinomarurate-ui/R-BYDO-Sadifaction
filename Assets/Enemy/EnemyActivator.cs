using UnityEngine;

public class EnemyActivator : MonoBehaviour
{
    [SerializeField]Enemy enemy;
    [SerializeField]EnemyMove enemyMove;
    [SerializeField]EnemyHP enemyHP;
    [SerializeField]Collider2D bodyCollider;
    [SerializeField]Rigidbody2D bodyRigidbody;
    [SerializeField]bool isActivated = false;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyMove = GetComponent<EnemyMove>();
        enemyHP = GetComponent<EnemyHP>();
        bodyCollider = GetComponent<Collider2D>();
        bodyRigidbody = GetComponent<Rigidbody2D>();
        SetSleepingState();
        
    }

    // Update is called once per frame
    public void ActivateEnemy()
    {
        if(isActivated)
        {
            return;
        }

        isActivated = true;
        bodyRigidbody.simulated = true;
        bodyCollider.enabled = true;
        enemyHP.enabled = true;
        //enemyMove.enabled = true;
        enemy.enabled = true;
        
    }

    void SetSleepingState()
    {
        bodyRigidbody.velocity = Vector2.zero;
        bodyRigidbody.angularVelocity = 0f;
        isActivated = false;
        bodyRigidbody.simulated = false;
        bodyCollider.enabled = false;
        enemyHP.enabled = false;
        enemyMove.enabled = false;
        enemy.enabled = false;
    }
}

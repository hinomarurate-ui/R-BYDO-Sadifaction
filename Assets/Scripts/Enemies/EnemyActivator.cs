using UnityEngine;

public class EnemyActivator : MonoBehaviour
{
    [SerializeField] EnemyController enemy;
    [SerializeField] MonoBehaviour enemyMove;
    [SerializeField] MonoBehaviour enemyAttack;
    [SerializeField] EnemyHealth enemyHP;
    [SerializeField] Collider2D bodyCollider;
    [SerializeField] Rigidbody2D bodyRigidbody;
    [SerializeField] bool isActivated = false;

    MonoBehaviour[] controlledBehaviours;

    void Awake()
    {
        enemy = GetComponent<EnemyController>();
        enemyMove = FindBehaviour<IEnemyMovement>();
        enemyAttack = FindBehaviour<IEnemyAttackPattern>();
        enemyHP = GetComponent<EnemyHealth>();
        bodyCollider = GetComponent<Collider2D>();
        bodyRigidbody = GetComponent<Rigidbody2D>();
        controlledBehaviours = CollectControlledBehaviours();
        SetSleepingState();
    }

    public void ActivateEnemy()
    {
        if(isActivated)
        {
            return;
        }

        isActivated = true;
        SetPhysicsActive(true);
        SetComponentsActive(true);

        if(enemy != null)
        {
            enemy.ActivateRuntime();
        }
    }

    void SetSleepingState()
    {
        isActivated = false;
        ResetBodyVelocity();
        SetPhysicsActive(false);
        SetComponentsActive(false);
    }

    void ResetBodyVelocity()
    {
        if(bodyRigidbody == null) return;

        bodyRigidbody.velocity = Vector2.zero;
        bodyRigidbody.angularVelocity = 0f;
    }

    void SetPhysicsActive(bool active)
    {
        if(bodyRigidbody != null)
        {
            bodyRigidbody.simulated = active;
        }

        if(bodyCollider != null)
        {
            bodyCollider.enabled = active;
        }
    }

    void SetComponentsActive(bool active)
    {
        if(controlledBehaviours != null)
        {
            for(int i = 0; i < controlledBehaviours.Length; i++)
            {
                if(controlledBehaviours[i] != null)
                {
                    controlledBehaviours[i].enabled = active;
                }
            }
        }
    }

    MonoBehaviour FindBehaviour<T>()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        for(int i = 0; i < behaviours.Length; i++)
        {
            if(behaviours[i] is T)
            {
                return behaviours[i];
            }
        }

        return null;
    }

    MonoBehaviour[] CollectControlledBehaviours()
    {
        var behaviours = new System.Collections.Generic.List<MonoBehaviour>();
        MonoBehaviour[] allBehaviours = GetComponents<MonoBehaviour>();

        for(int i = 0; i < allBehaviours.Length; i++)
        {
            MonoBehaviour behaviour = allBehaviours[i];
            if(behaviour == null || behaviour == this)
            {
                continue;
            }

            if(behaviour is EnemyController
                || behaviour is EnemyHealth
                || behaviour is IEnemyMovement
                || behaviour is IEnemyAttackPattern
                || behaviour is IEnemyRoutine)
            {
                behaviours.Add(behaviour);
            }
        }

        return behaviours.ToArray();
    }
}

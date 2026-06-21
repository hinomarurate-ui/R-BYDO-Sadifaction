using UnityEngine;

// 実装意図: 画面外や未起動の敵を眠らせ、Trigger 到達時だけ共通 enemy component 群を有効化する。
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
        // 実装意図: 旧参照欄が未設定でも、新しい interface 実装を自動収集して移行 prefab を動かす。
        enemy = GetComponent<EnemyController>();
        enemyMove = FindBehaviour<IEnemyMovement>();
        enemyAttack = FindBehaviour<IEnemyAttackPattern>();
        enemyHP = GetComponent<EnemyHealth>();
        bodyCollider = GetComponent<Collider2D>();
        bodyRigidbody = GetComponent<Rigidbody2D>();
        controlledBehaviours = CollectControlledBehaviours();
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
        SetPhysicsActive(true);
        SetComponentsActive(true);
    }

    void SetSleepingState()
    {
        // 実装意図: 開始時は物理も行動も止めて、未起動の敵が落下・攻撃しない状態にする。
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
        // 実装意図: 今後 IEnemyRoutine が増えても Activator の Inspector 設定を増やさず制御対象に含める。
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

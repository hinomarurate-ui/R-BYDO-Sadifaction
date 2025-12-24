using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour
{
    [SerializeField]
    Transform ShotPoint;
    [SerializeField]
    GameObject BydoShot;
    [SerializeField]
    float BulletSpeed;
    [SerializeField]
    float ShotCooltime;

    float LastShotTime = 0f;
    Transform PlayerTransform;

    void Start()
    {
     PlayerTransform = this.transform;   
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Z))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if(Time.time < LastShotTime + ShotCooltime)
        {
            return;
        }
        LastShotTime = Time.time;
        GameObject Bullet = Instantiate(BydoShot,ShotPoint.position,Quaternion.identity);
        float Dirx = Mathf.Sign(PlayerTransform.localScale.x);
        Vector2 ShotDir = new Vector2(Dirx,0f);
        Bullet bullet = Bullet.GetComponent<Bullet>();
        bullet.Init(ShotDir,BulletSpeed);
    }
}

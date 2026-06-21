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
    [SerializeField]
    float AimDeadzone = 0.2f;
    AudioSource As;
    [SerializeField] AudioClip ShotS;

    float LastShotTime = 0f;
    Transform PlayerTransform;

    void Start()
    {
        PlayerTransform = transform;
        As = GetComponent<AudioSource>();
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

        if(BydoShot == null || ShotPoint == null)
        {
            return;
        }

        PlayShotSound();
        LastShotTime = Time.time;
        GameObject bulletObject = Instantiate(BydoShot,ShotPoint.position,Quaternion.identity);
        Vector2 ShotDir = getShotDirection();
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if(bullet != null)
        {
            bullet.Init(ShotDir,BulletSpeed);
        }
    }

    Vector2 getShotDirection()
    {
        float faceDir = Mathf.Sign(PlayerTransform.localScale.x);
        if(faceDir == 0f)
        {
            faceDir = 1f;
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        if(Mathf.Abs(inputX) <= AimDeadzone)
        {
            inputX = 0f;
        }
        else
        {
            inputX = Mathf.Sign(inputX);
        }

        if(Mathf.Abs(inputY) <= AimDeadzone)
        {
            inputY = 0f;
        }
        else
        {
            inputY = Mathf.Sign(inputY);
        }

        Vector2 inputDirection = new Vector2(inputX,inputY);
        if(inputDirection.sqrMagnitude > 0f)
        {
            return inputDirection.normalized;
        }

        return new Vector2(faceDir,0f);
    }

    void PlayShotSound()
    {
        if(As != null && ShotS != null)
        {
            As.PlayOneShot(ShotS,0.5f);
        }
    }
}

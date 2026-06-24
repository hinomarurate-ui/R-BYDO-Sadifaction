using UnityEngine;
using UnityEngine.Serialization;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("ShotPoint")]
    Transform shotPoint;

    [SerializeField]
    [FormerlySerializedAs("BydoShot")]
    GameObject bulletPrefab;

    [SerializeField]
    [FormerlySerializedAs("BulletSpeed")]
    float bulletSpeed = 12f;

    [SerializeField]
    [FormerlySerializedAs("ShotCooltime")]
    float shotCooldown = 0.1f;

    [SerializeField]
    [FormerlySerializedAs("AimDeadzone")]
    float aimDeadzone = 0.2f;

    [SerializeField]
    [FormerlySerializedAs("ShotS")]
    AudioClip shotSound;

    AudioSource audioSource;
    Transform ownerTransform;
    float nextShotTime;

    void Awake()
    {
        ownerTransform = transform;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Z))
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if(Time.time < nextShotTime || bulletPrefab == null || shotPoint == null)
        {
            return;
        }

        nextShotTime = Time.time + shotCooldown;
        PlayShotSound();

        GameObject bulletObject = Instantiate(bulletPrefab, shotPoint.position, Quaternion.identity);
        PlayerBullet bullet = bulletObject.GetComponent<PlayerBullet>();
        if(bullet != null)
        {
            bullet.Init(GetShotDirection(), bulletSpeed);
        }
    }

    Vector2 GetShotDirection()
    {
        float inputX = NormalizeAxis(Input.GetAxisRaw("Horizontal"));
        float inputY = NormalizeAxis(Input.GetAxisRaw("Vertical"));
        Vector2 inputDirection = new Vector2(inputX, inputY);

        if(inputDirection.sqrMagnitude > 0f)
        {
            return inputDirection.normalized;
        }

        return new Vector2(FacingDirection(), 0f);
    }

    float NormalizeAxis(float axis)
    {
        return Mathf.Abs(axis) <= aimDeadzone ? 0f : Mathf.Sign(axis);
    }

    float FacingDirection()
    {
        float direction = Mathf.Sign(ownerTransform.localScale.x);
        return direction == 0f ? 1f : direction;
    }

    void PlayShotSound()
    {
        if(audioSource != null && shotSound != null)
        {
            audioSource.PlayOneShot(shotSound, 0.5f);
        }
    }
}

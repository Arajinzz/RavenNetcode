using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float ProjectileSpeed = 10f;

    private Vector3 ProjectileDirection;

    private bool ProjectileSet = false;

    private float LifeTime = 5f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ProjectileSet)
        {
            transform.position += ProjectileDirection * Time.fixedDeltaTime * ProjectileSpeed;
            LifeTime -= Time.fixedDeltaTime;

            // Animate
            transform.Rotate(transform.forward, Time.fixedDeltaTime * 500f);
        }

        if (LifeTime < 0)
        {
            Destroy(gameObject);
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    public void SetProjectileDirection(Vector3 Direction)
    {
        ProjectileDirection = Direction;
        ProjectileDirection = ProjectileDirection.normalized;
        ProjectileSet = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private Transform FirePoint;

    [SerializeField]
    private GameObject Projectile;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue))
            {
                Debug.Log(hit.collider.name);
                Debug.DrawRay(ray.origin, ray.direction * 500f, Color.red, 10);
                Shoot(hit.point);
            }

        }
    }

    private void Shoot(Vector3 hitPoint)
    {
        Vector3 Direction = hitPoint - FirePoint.position;

        Ray ray = new Ray(FirePoint.position, Direction);
        Debug.DrawRay(ray.origin, ray.direction * 500f, Color.green, 10);

        GameObject Bullet = Instantiate(Projectile, FirePoint.position, Quaternion.identity);
        Bullet.GetComponent<Projectile>().SetProjectileDirection(Direction);
    }
}

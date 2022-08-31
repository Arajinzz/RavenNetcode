using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private Transform FirePoint;

    [SerializeField]
    private GameObject Projectile;

    private InputManager inputManager;

    private void Start()
    {
        inputManager = GetComponentInParent<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void Shoot()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue))
        {
            Vector3 Direction = hit.point - FirePoint.position;
            GameObject Bullet = Instantiate(Projectile, FirePoint.position, Quaternion.identity);
            Bullet.GetComponent<Projectile>().SetProjectileDirection(Direction);
        }
    }

    void HandleInput()
    {
        if (!inputManager)
            return;

        if (inputManager.isKeyPressed(InputManager.Key.LeftMouseDown))
        {
            Shoot();
            // Unpress Key, Prevent holding
            inputManager.KeyPressed &= ~InputManager.Key.LeftMouseDown;
        }
    }

}

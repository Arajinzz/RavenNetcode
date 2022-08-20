using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMouvement : MonoBehaviour
{
    InputManager InputController;

    // Start is called before the first frame update
    void Start()
    {
        InputController = GetComponent<InputManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Move(Vector3 hitPoint)
    {
        GetComponent<NavMeshAgent>().destination = hitPoint;
    }

    public void NetMove()
    {
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public enum Key
    {
        None,
        LeftMouseDown,
    }

    public Key KeyPressed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        KeyPressed = Key.None;
        byte[] data = P2PPacket.Compose_NoKeyPressedPacket();

        if (Input.GetMouseButtonDown(0))
        {
            KeyPressed = Key.LeftMouseDown;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    data = P2PPacket.Compose_LeftMouseButtonPressedPacket(hit.point);
                }
            }
            P2PNetworkSend.SendToAllLobby(SteamLobbyManager.Instance.CurrentLobby, data);
        }

    }

}

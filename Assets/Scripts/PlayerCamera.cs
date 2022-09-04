using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    float MouseSens = 5f;

    public Transform PlayerBody;

    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * MouseSens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * MouseSens * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -15f, 15f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (Mathf.Abs(mouseX) > 0.01)
        {
            P2PPacket packet = new P2PPacket(P2PPacket.PacketType.PlayerRotated);
            packet.InsertFloat(mouseX);
            P2PNetworkSend.SendToAllLobby(SteamLobbyManager.Instance.CurrentLobby, packet.buffer.ToArray());
        }
    }
}

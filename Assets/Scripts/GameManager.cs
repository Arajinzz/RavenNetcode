using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    private float timer;
    public int currentTick;
    private float minTimeBetweenTicks;
    private float SERVER_TICK_RATE = 60f;
    private const int BUFFER_SIZE = 60;

    private GameState[] gameStates;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        minTimeBetweenTicks = 1 / SERVER_TICK_RATE;
        gameStates = new GameState[BUFFER_SIZE];
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    private void HandleTick()
    {
        // Push game state
        gameStates[currentTick % BUFFER_SIZE] = new GameState();
    }

    public void Rollback(int frame)
    {
        currentTick = frame;

        GameState state = gameStates[frame];

        Dictionary<SteamId, GameObject> Players = P2PNetworkReceive.Instance.Players;

        // Restore mouvements
        foreach (SteamId key in state.Players.Keys)
        {
            Players[key].transform.position = state.Players[key].playerPosition;
        }
    }
}

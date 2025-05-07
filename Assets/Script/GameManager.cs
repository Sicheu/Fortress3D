using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState { Waiting, Aiming, Firing, End }

public class GameManager : Singleton<GameManager>
{
    public TankController[] tanks;
    public int currentPlayerIndex = 0;
    public TurnState currentTurnState = TurnState.Waiting;

    private void Start()
    {
        StartTurn();
    }
    
    public void StartTurn()
    {
        currentTurnState = TurnState.Aiming;
        
        TankController currentTank = tanks[currentPlayerIndex];
        
        currentTank.SetControl(true);
        
        CameraManager.Instance.TurnChange(currentTank.thirdPersonCamera, currentTank.transform);
    }

    public void EndTurn()
    {
        tanks[currentPlayerIndex].SetControl(false);
        currentPlayerIndex = (currentPlayerIndex + 1) % tanks.Length;
        currentTurnState = TurnState.Waiting;

        Invoke(nameof(StartTurn), 2f); // 다음 턴 시작 전 약간의 대기 시간
    }
}


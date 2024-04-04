using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.MoveFast;

public class GamePlayingState : MonoBehaviour, IActiveState
{
    [SerializeField] private GamePlay gamePlay; 
    public bool Active => gamePlay.IsPlaying();
}
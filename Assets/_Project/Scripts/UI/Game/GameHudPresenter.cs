using System;
using System.Text;
using Game.Shared.Data;
using Game.Shared.Interface;
using Game.UI.Common;
using VContainer.Unity;

namespace Game.UI.Game
{
    // Player list (hold tab or open pause) and pause menu with leave 
    public sealed class GameHudPresenter : IStartable, ITickable, IDisposable
    {
        private readonly GameHudView _view;
        private readonly IGameInput _input;
        private readonly IPlayerRegistry _players;
        private readonly IConnectionService _connection;
        private readonly IInteractionHint _hint;
        private bool _paused;
         
        public GameHudPresenter(GameHudView view, IGameInput input, IPlayerRegistry players, IConnectionService connection, IInteractionHint hint)
        { 
            _view = view;
            _input = input;
            _players = players;
            _hint = hint;
            _connection = connection;
        }

        public void Start()
        { 
            _view.LeaveClicked += _connection.Leave;
            _players.Changed += RenderPlayers;

            _hint.Changed += RenderHint;
            RenderHint(_hint.Current);

            RenderPlayers();
            SetPaused(false);
        }

        public void Tick()
        { 
            if (_input.PausePressed) SetPaused(!_paused);
             
            _view.SetPlayerListVisible(_paused || _input.PlayerListHeld);
        }

        private void SetPaused(bool paused)
        {
            _paused = paused;
            _view.SetPauseVisible(paused);
            _input.SetGameplayEnabled(!paused);
            _view.SetCrosshairVisible(!paused);
        }

        private void RenderPlayers()
        { 
            var text = new StringBuilder();

            foreach (PlayerInfo player in _players.Players)
                text.AppendLine(PlayerLabel.Format(player));

            _view.SetPlayerList(text.ToString());
        }

        public void Dispose()
        { 
            _view.LeaveClicked -= _connection.Leave;
            _players.Changed -= RenderPlayers;
            _hint.Changed -= RenderHint;

            // Leaving the game scene: give the cursor back to the menus.
            _input.SetGameplayEnabled(false);
        }

        private void RenderHint(InteractionHint hint) => _view.SetHint(hint switch
        {
            InteractionHint.CanGrab => "[E] Grab",
            InteractionHint.Holding => "[E] Drop    [LMB] Throw    [X] Delete",
            _ => "[F] Spawn crate"
        });
    }
}
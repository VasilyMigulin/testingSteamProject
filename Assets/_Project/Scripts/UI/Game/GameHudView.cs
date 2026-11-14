using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Game
{
    public sealed class GameHudView : MonoBehaviour
    {
        [SerializeField] private GameObject _playerListPanel;
        [SerializeField] private TMP_Text _playerListText;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _leaveButton; 
        [SerializeField] private GameObject _crosshair;
        [SerializeField] private TMP_Text _hintText;


        public event Action LeaveClicked;
        private void Awake() => _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());

        public void SetPlayerListVisible(bool visible) => _playerListPanel.SetActive(visible); 
        public void SetPlayerList(string text) => _playerListText.text = text; 
        public void SetPauseVisible(bool visible) => _pausePanel.SetActive(visible); 
        public void SetCrosshairVisible(bool visible) => _crosshair.SetActive(visible);
        public void SetHint(string text) => _hintText.text = text;
    }
}
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

        public event Action LeaveClicked;
        private void Awake() => _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());

        public void SetPlayerListVisible(bool visible) => _playerListPanel.SetActive(visible);

        public void SetPlayerList(string text) => _playerListText.text = text;

        public void SetPauseVisible(bool visible) => _pausePanel.SetActive(visible);
    }
}
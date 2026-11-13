using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Lobby
{
    public sealed class LobbyView : MonoBehaviour
    {
        [SerializeField] private RectTransform _playerListRoot;
        [SerializeField] private TMP_Text _playerRowPrefab;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _leaveButton;
         
        private readonly List<TMP_Text> _rows = new();
         
        public event Action StartClicked;
        public event Action LeaveClicked;
         
        private void Awake()
        {
            _startButton.onClick.AddListener(() => StartClicked?.Invoke());
            _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }
         
        public void SetCanStart(bool canStart) => _startButton.gameObject.SetActive(canStart);

        // Reuses existing rows to avoid recreating UI on every change. 
        public void SetPlayers(IReadOnlyList<string> lines)
        {
            while (_rows.Count < lines.Count)
                _rows.Add(Instantiate(_playerRowPrefab, _playerListRoot));


            for (int i = 0; i < _rows.Count; i++)
            {
                bool used = i < lines.Count;
                _rows[i].gameObject.SetActive(used);

                if (used) _rows[i].text = lines[i];
            }
        }
    }
}
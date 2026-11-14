using System;  
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Lobby
{
    public sealed class LobbyView : MonoBehaviour
    { 
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _leaveButton; 
         
        public event Action StartClicked;
        public event Action LeaveClicked;
         
        private void Awake()
        {
            _startButton.onClick.AddListener(() => StartClicked?.Invoke());
            _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }
         
        public void SetCanStart(bool canStart) => _startButton.gameObject.SetActive(canStart); 
    }
}
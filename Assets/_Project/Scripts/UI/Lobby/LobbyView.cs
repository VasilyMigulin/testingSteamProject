using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Lobby
{
    public sealed class LobbyView : MonoBehaviour
    {
        [SerializeField] private Button _inviteButton;
        [SerializeField] private Button _copyCodeButton;
        [SerializeField] private TMP_Text _sessionCodeText;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _leaveButton;

        public event Action InviteClicked;
        public event Action StartClicked;
        public event Action LeaveClicked;
         
        private void Awake()
        {
            _startButton.onClick.AddListener(() => StartClicked?.Invoke());
            _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
            _inviteButton.onClick.AddListener(() => InviteClicked?.Invoke());
            _copyCodeButton.onClick.AddListener(() => GUIUtility.systemCopyBuffer = _sessionCodeText.text);
        }
         
        public void SetCanStart(bool canStart) => _startButton.gameObject.SetActive(canStart);
        public void SetInvite(bool canInvite, string sessionCode)
        { 
            _inviteButton.gameObject.SetActive(canInvite); 
            _sessionCodeText.text = sessionCode;
            _copyCodeButton.gameObject.SetActive(!string.IsNullOrEmpty(sessionCode));
        }
    }
}
using System;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Menu
{
    public sealed class MenuView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _addressInput; 
        [SerializeField] private Button _hostButton; 
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private TMP_Text _statusText;

        public event Action HostClicked;
        public event Action<string> JoinClicked; 
        public event Action LeaveClicked;

        private void Awake()
        {
            _hostButton.onClick.AddListener(() => HostClicked?.Invoke()); 
            _joinButton.onClick.AddListener(() => JoinClicked?.Invoke(_addressInput.text));
            _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void SetCanStart(bool canStart)
        {
            _hostButton.interactable = canStart;
            _joinButton.interactable = canStart;
            _addressInput.interactable = canStart; 
            _leaveButton.interactable = !canStart; 
        }

        public void SetStatus(string text) => _statusText.text = text;
    }
}
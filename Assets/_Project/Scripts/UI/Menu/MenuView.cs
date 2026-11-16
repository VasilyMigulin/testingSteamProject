using Game.Shared.Data;
using System;
using System.Collections.Generic;
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

        [SerializeField] private TMP_InputField _sessionNameInput;
        [SerializeField] private TMP_Dropdown _maxPlayersDropdown;
        [SerializeField] private TMP_Dropdown _visibilityDropdown;
        [SerializeField] private TMP_Text _addressPlaceholder;

        private const int MinPlayers = 2;
        private const int MaxPlayersLimit = 4;

        public string SessionName => string.IsNullOrWhiteSpace(_sessionNameInput.text) ? "Session" : _sessionNameInput.text.Trim();
        public int MaxPlayers => MinPlayers + _maxPlayersDropdown.value; 
        public SessionVisibility Visibility => (SessionVisibility)_visibilityDropdown.value;

        public event Action HostClicked;
        public event Action<string> JoinClicked; 
        public event Action LeaveClicked;

        private void Awake()
        {
            // Options are built from code so they always match MaxPlayers and the SessionVisibility enum.
            var playerOptions = new List<string>();
            for (int count = MinPlayers; count <= MaxPlayersLimit; count++) playerOptions.Add($"{count} players");

            _maxPlayersDropdown.ClearOptions();
            _maxPlayersDropdown.AddOptions(playerOptions);
            _maxPlayersDropdown.value = playerOptions.Count - 1;

            // Order must match SessionVisibility: 0 = FriendsOnly, 1 = Private.
            _visibilityDropdown.ClearOptions();
            _visibilityDropdown.AddOptions(new List<string> { "Friends only", "Private" }); 

            _hostButton.onClick.AddListener(() => HostClicked?.Invoke());
            _joinButton.onClick.AddListener(() => JoinClicked?.Invoke(_addressInput.text));
            _leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void SetCanStart(bool canStart)
        {
            _hostButton.interactable = canStart;
            _joinButton.interactable = canStart;
            _addressInput.interactable = canStart; 
            _sessionNameInput.interactable = canStart;
            _maxPlayersDropdown.interactable = canStart;
            _visibilityDropdown.interactable = canStart;
            _leaveButton.interactable = !canStart;
        }

        public void SetStatus(string text) => _statusText.text = text;
         
        public void SetSteamMode(bool isSteam)
        {
            // Visibility only exists for Steam lobbies.
            _visibilityDropdown.gameObject.SetActive(isSteam);
            _addressPlaceholder.text = isSteam ? "Steam lobby ID" : "IP:port"; 
        }
    }
}
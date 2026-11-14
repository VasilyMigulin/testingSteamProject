using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Common
{
    public sealed class PlayerRowView : MonoBehaviour
    { 
        [SerializeField] private TMP_Text _label; 
        [SerializeField] private TMP_Text _ping;
        [SerializeField] private Button _kickButton;

        private ulong _clientId;
        public event Action<ulong> KickClicked;
        private void Awake() => _kickButton.onClick.AddListener(() => KickClicked?.Invoke(_clientId));
        public void Set(PlayerRow row)
        { 
            _clientId = row.ClientId;
            _label.text = row.Label;
            _ping.text = row.Ping;
            _kickButton.gameObject.SetActive(row.CanKick);
        }
    }
}
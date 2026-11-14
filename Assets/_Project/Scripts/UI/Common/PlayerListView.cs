using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Common
{
    // Reused by the lobby and the in-game pause menu.
    public sealed class PlayerListView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private PlayerRowView _rowPrefab;

        private readonly List<PlayerRowView> _rows = new();
        public event Action<ulong> KickClicked;
        public void Render(IReadOnlyList<PlayerRow> rows)
        {
            while (_rows.Count < rows.Count)
            {
                PlayerRowView row = Instantiate(_rowPrefab, _root);
                row.KickClicked += OnKickClicked;
                _rows.Add(row);
            }

            for (int i = 0; i < _rows.Count; i++)
            { 
                bool used = i < rows.Count;
                _rows[i].gameObject.SetActive(used);
                if (used)  _rows[i].Set(rows[i]);
            }
        }
        private void OnKickClicked(ulong clientId) => KickClicked?.Invoke(clientId);
    }
}
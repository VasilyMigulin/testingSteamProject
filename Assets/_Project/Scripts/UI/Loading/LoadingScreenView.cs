using TMPro;
using UnityEngine;

namespace Game.UI.Loading
{
    public sealed class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _waitingTitle;

        public void SetVisible(bool visible) => _root.SetActive(visible);
        public void SetWaiting(string text) => _waitingTitle.text = text; 
    }
}
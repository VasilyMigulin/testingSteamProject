using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Net.Injection
{
    // NGO instantiates network prefab itself (always on clients, on host via InstantiateAndSpawn)
    // so VContainer never seen those instances. It's handler routes instantiation through the container
    internal sealed class InjectingPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly IObjectResolver _resolver;
        private readonly GameObject _prefab;

        public InjectingPrefabHandler(IObjectResolver resolver, GameObject prefab)
        {
            _resolver = resolver;
            _prefab = prefab;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation) 
        {
            return _resolver.Instantiate(_prefab, position, rotation).GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject) => Object.Destroy(networkObject.gameObject);
    }
}
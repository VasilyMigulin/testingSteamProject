using Unity.Netcode;
using VContainer;
using VContainer.Unity;

namespace Game.Net.Injection
{
    // Registers an injecting handler for every network prefab, so all spawned objects get dependencies.
    public class NetworkPrefabInjector : IStartable
    {
        private readonly NetworkManager _networkManager;
        private readonly IObjectResolver _resolver;

        public NetworkPrefabInjector(NetworkManager networkManager, IObjectResolver resolver)
        {
            _networkManager = networkManager;
            _resolver = resolver;
        }

        public void Start()
        {
            // Lists are used instead of NetworkConfig.Prefabs.Prefabs: the latter is filled only when NGO starts.
            foreach (NetworkPrefabsList list in _networkManager.NetworkConfig.Prefabs.NetworkPrefabsLists)
            {
                foreach (NetworkPrefab prefab in list.PrefabList)
                    _networkManager.PrefabHandler.AddHandler(prefab.Prefab, new InjectingPrefabHandler(_resolver, prefab.Prefab)); 
            }
        }
    }
}
using Game.Shared.Data;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Bootstrap
{ 
    // Runs once the root container is built.
    public sealed class BootstrapEntryPoint : IStartable
    {
        public void Start() => SceneManager.LoadScene(SceneNames.Menu); 
    }
}
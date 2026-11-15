namespace Game.Shared.Interface
{
    // Gameplay reports domain events; the platform decides which stats and achievements they affect.
    public interface IAchievementService
    {
        // Called on the thrower's machine after the host confirmed the throw.
        void ReportItemThrown(); 
    }
}
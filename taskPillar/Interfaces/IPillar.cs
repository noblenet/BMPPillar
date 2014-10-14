using pillarAPI;

namespace PillarAPI.Interfaces
{
    public interface IPillar
    {
        void KillPillar();
        void Initialize(PillarApiSettings pillarApiSettings);
    }
}
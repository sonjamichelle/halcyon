using OpenMetaverse;
using OpenSim.Framework;

namespace OpenSim.Services.Interfaces
{
    public interface ISimulationService
    {
        bool CreateAgent(GridRegion source, GridRegion destination, AgentCircuitData aCircuit, uint flags, EntityTransferContext ctx, out string reason);
        bool UpdateAgent(GridRegion destination, AgentData data, EntityTransferContext ctx);
        bool UpdateAgent(GridRegion destination, AgentPosition data);
        bool CloseAgent(GridRegion destination, UUID id, string auth_code);
    }
}

using System;
using System.IO;
using System.Net;
using System.Text;
using Nini.Config;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Services.Interfaces;

namespace OpenSim.Server.Handlers.Base
{
    /// <summary>
    /// Minimal base connector used by HG handler ports.
    /// </summary>
    public class ServiceConnector
    {
        protected readonly IConfigSource m_Config;
        protected readonly IHttpServer m_Server;

        public ServiceConnector(IConfigSource config, IHttpServer server, string name)
        {
            m_Config = config;
            m_Server = server;
        }
    }
}

namespace OpenSim.Server.Handlers.Simulation
{
    public class AgentDestinationData
    {
        public GridRegion destination;
        public AgentCircuitData aCircuit;
        public bool fromLogin;
    }

    public static class Utils
    {
    }

    /// <summary>
    /// Simplified AgentPostHandler that accepts POSTs but does not yet bridge to full HG logic.
    /// </summary>
    public class AgentPostHandler : BaseStreamHandler
    {
        protected bool m_Proxy;

        public AgentPostHandler(string path) : base("POST", path)
        {
        }

        protected virtual AgentDestinationData CreateAgentDestinationData()
        {
            return new AgentDestinationData();
        }

        protected virtual void UnpackData(OSDMap args, AgentDestinationData data, string remoteAddress)
        {
            if (args == null || data == null)
                return;

            if (args.ContainsKey("destination_region"))
                data.destination = new GridRegion();
        }

        protected virtual GridRegion ExtractGatekeeper(AgentDestinationData d)
        {
            return d.destination;
        }

        protected virtual bool CreateAgent(GridRegion source, GridRegion gatekeeper, GridRegion destination, AgentCircuitData aCircuit, uint teleportFlags, bool fromLogin, EntityTransferContext ctx, out string reason)
        {
            reason = "not implemented";
            return false;
        }

        public override byte[] Handle(string path, Stream request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            httpResponse.StatusCode = (int)HttpStatusCode.NotImplemented;
            return Encoding.UTF8.GetBytes("<llsd><string>not implemented</string></llsd>");
        }
    }
}

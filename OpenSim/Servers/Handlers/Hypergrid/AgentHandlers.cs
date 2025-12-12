/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using OpenSim.Framework.Servers.HttpServer;

namespace OpenSim.Server.Handlers.Hypergrid
{
    public class AgentHandlers : ServiceConnector
    {
        public AgentHandlers(IConfigSource config, IHttpServer server, string configName)
            : base(config, server, configName)
        {
            // Placeholder for HG agent post handlers.
        }
    }
}

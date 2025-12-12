/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Services.Interfaces;

namespace OpenSim.Server.Handlers.Hypergrid
{
    public class UserAgentServerConnector : ServiceConnector
    {
        public UserAgentServerConnector(IConfigSource config, IHttpServer server, string configName)
            : base(config, server, configName)
        {
            // Placeholder for HG UserAgent handler registration.
        }
    }
}

/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Services.Interfaces;

namespace OpenSim.Server.Handlers.Hypergrid
{
    public class GatekeeperServerConnector : ServiceConnector
    {
        public GatekeeperServerConnector(IConfigSource config, IHttpServer server, string configName)
            : base(config, server, configName)
        {
            // Placeholder: hook up HypergridHandlers on the supplied server when HG is enabled.
        }
    }
}

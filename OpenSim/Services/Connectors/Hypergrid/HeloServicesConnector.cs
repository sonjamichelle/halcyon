/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
namespace OpenSim.Services.Connectors.Hypergrid
{
    public class HeloServicesConnector
    {
        public HeloServicesConnector(string serverURI)
        {
            ServerURI = serverURI;
        }

        public string ServerURI { get; private set; }
    }
}

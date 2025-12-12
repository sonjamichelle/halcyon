/*
 * Copyright (c) Contributors, http://opensimulator.org/
 */
using System.Collections;
using System.Net;
using log4net;
using Nwc.XmlRpc;
using OpenMetaverse;
using OpenSim.Services.Interfaces;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

namespace OpenSim.Server.Handlers.Hypergrid
{
    public class HypergridHandlers
    {
        private static readonly ILog m_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IGatekeeperService m_GatekeeperService;

        public HypergridHandlers(IGatekeeperService gatekeeper)
        {
            m_GatekeeperService = gatekeeper;
            m_log.DebugFormat("[HYPERGRID HANDLERS]: Active");
        }

        public XmlRpcResponse LinkRegionRequest(XmlRpcRequest request, IPEndPoint remoteClient)
        {
            Hashtable requestData = (Hashtable)request.Params[0];
            string name = (string)requestData["region_name"];
            if (name == null)
                name = string.Empty;

            m_log.DebugFormat("[HG Handler]: XMLRequest to link to {0} from {1}", (name.Length == 0) ? "default region" : name, remoteClient.Address.ToString());
            bool success = m_GatekeeperService.LinkLocalRegion(name, out UUID regionID, out ulong regionHandle, out string externalName,
                out string imageURL, out string reason, out int sizeX, out int sizeY);

            Hashtable hash = new Hashtable();
            hash["result"] = success.ToString();
            hash["uuid"] = regionID.ToString();
            hash["handle"] = regionHandle.ToString();
            hash["size_x"] = sizeX.ToString();
            hash["size_y"] = sizeY.ToString();
            hash["region_image"] = imageURL;
            hash["external_name"] = externalName;

            XmlRpcResponse response = new XmlRpcResponse();
            response.Value = hash;
            return response;
        }

        public XmlRpcResponse GetRegion(XmlRpcRequest request, IPEndPoint remoteClient)
        {
            Hashtable requestData = (Hashtable)request.Params[0];
            string regionID_str = (string)requestData["region_uuid"];
            UUID regionID = UUID.Zero;
            UUID.TryParse(regionID_str, out regionID);

            UUID agentID = UUID.Zero;
            string agentHomeURI = null;
            if (requestData.ContainsKey("agent_id"))
                agentID = UUID.Parse((string)requestData["agent_id"]);
            if (requestData.ContainsKey("agent_home_uri"))
                agentHomeURI = (string)requestData["agent_home_uri"];

            string message;
            GridRegion regInfo = m_GatekeeperService.GetHyperlinkRegion(regionID, agentID, agentHomeURI, out message);

            Hashtable hash = new Hashtable();
            if (regInfo == null)
            {
                hash["result"] = "false";
            }
            else
            {
                hash["result"] = "true";
                hash["uuid"] = regInfo.RegionID.ToString();
                hash["x"] = regInfo.RegionLocX.ToString();
                hash["y"] = regInfo.RegionLocY.ToString();
                hash["size_x"] = regInfo.RegionSizeX.ToString();
                hash["size_y"] = regInfo.RegionSizeY.ToString();
                hash["region_name"] = regInfo.RegionName;
                hash["hostname"] = regInfo.ExternalHostName;
                hash["http_port"] = regInfo.HttpPort.ToString();
                hash["internal_port"] = regInfo.InternalEndPoint.Port.ToString();
                hash["server_uri"] = regInfo.ServerURI;
            }

            if (message != null)
                hash["message"] = message;

            XmlRpcResponse response = new XmlRpcResponse();
            response.Value = hash;
            return response;
        }
    }
}

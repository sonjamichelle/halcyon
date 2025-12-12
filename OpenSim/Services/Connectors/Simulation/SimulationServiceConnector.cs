/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Minimal port of the OpenSim 0.9.3 SimulationServiceConnector used by the
 * Hypergrid gatekeeper connector. Object transfer methods were omitted to
 * avoid extra region references; this keeps the agent/teleport pipeline intact.
 */

using System;
using System.Collections.Generic;
using OpenSim.Framework;
using OpenSim.Services.Interfaces;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using log4net;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

namespace OpenSim.Services.Connectors.Simulation
{
    internal static class VersionInfo
    {
        public const double SimulationServiceVersionSupportedMin = 0.6;
        public const double SimulationServiceVersionSupportedMax = 0.6;
        public const double SimulationServiceVersionAcceptedMin = 0.3;
        public const double SimulationServiceVersionAcceptedMax = 0.6;
    }

    public class SimulationServiceConnector
    {
        private static readonly ILog m_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Tracks pending UpdateAgent requests; maps URI -> position update.
        private readonly Dictionary<string, AgentPosition> m_updateAgentQueue = new Dictionary<string, AgentPosition>();
        private readonly ExpiringCache<string, bool> _failedSims = new ExpiringCache<string, bool>();

        protected virtual string AgentPath()
        {
            return "agent/";
        }

        protected virtual void PackData(OSDMap args, GridRegion source, AgentCircuitData aCircuit, GridRegion destination, uint flags)
        {
            if (source != null)
            {
                args["source_x"] = OSD.FromInteger(source.RegionLocX);
                args["source_y"] = OSD.FromInteger(source.RegionLocY);
                args["source_name"] = OSD.FromString(source.RegionName);
                args["source_uuid"] = OSD.FromUUID(source.RegionID);
                if (!string.IsNullOrEmpty(source.RawServerURI))
                    args["source_server_uri"] = OSD.FromString(source.RawServerURI);
            }

            args["destination_x"] = OSD.FromInteger(destination.RegionLocX);
            args["destination_y"] = OSD.FromInteger(destination.RegionLocY);
            args["destination_name"] = OSD.FromString(destination.RegionName);
            args["destination_uuid"] = OSD.FromUUID(destination.RegionID);
            args["teleport_flags"] = OSD.FromUInteger(flags);
        }

        public bool CreateAgent(GridRegion source, GridRegion destination, AgentCircuitData aCircuit, uint flags, EntityTransferContext ctx, out string reason)
        {
            reason = string.Empty;

            if (destination == null)
            {
                reason = "Destination not found";
                m_log.Debug("[REMOTE SIMULATION CONNECTOR]: Create agent destination is null");
                return false;
            }

            m_log.DebugFormat("[REMOTE SIMULATION CONNECTOR]: Creating agent at {0}", destination.ServerURI);

            string uri = destination.ServerURI + AgentPath() + aCircuit.AgentID + "/";
            OSD tmpOSD;
            try
            {
                OSDMap args = aCircuit.PackAgentCircuitData();
                if (ctx == null)
                    ctx = new EntityTransferContext();
                OSDMap ctxMap = new OSDMap();
                ctx.Pack(ctxMap);
                args["context"] = ctxMap;
                PackData(args, source, aCircuit, destination, flags);

                OSDMap result = WebUtil.PostToServiceCompressed(uri, args, 30000);
                bool success = result["success"].AsBoolean();
                if (success && result.TryGetValue("_Result", out tmpOSD) && tmpOSD is OSDMap)
                {
                    OSDMap data = (OSDMap)tmpOSD;
                    reason = data["reason"].AsString();
                    success = data["success"].AsBoolean();
                    return success;
                }

                // Try the old version, uncompressed
                result = WebUtil.PostToService(uri, args, 30000, false);

                success = result["success"].AsBoolean();
                if (success)
                {
                    if (result.TryGetValue("_Result", out tmpOSD) && tmpOSD is OSDMap)
                    {
                        OSDMap data = (OSDMap)tmpOSD;
                        reason = data["reason"].AsString();
                        success = data["success"].AsBoolean();

                        m_log.WarnFormat(
                            "[REMOTE SIMULATION CONNECTOR]: Remote simulator {0} did not accept compressed transfer, suggest updating that simulator.",
                            destination.RegionName);
                        return success;
                    }
                }

                    m_log.WarnFormat(
                        "[REMOTE SIMULATION CONNECTOR]: Failed to create agent {0} {1} at remote simulator {2}",
                    aCircuit.FirstName, aCircuit.LastName, destination.RegionName);
                reason = result["Message"] != null ? result["Message"].AsString() : "error";
                return false;
            }
            catch (Exception e)
            {
                m_log.Warn("[REMOTE SIMULATION CONNECTOR]: CreateAgent failed with exception: " + e);
                reason = e.Message;
            }

            return false;
        }

        public bool UpdateAgent(GridRegion destination, AgentData data, EntityTransferContext ctx)
        {
            return UpdateAgent(destination, (IAgentData)data, ctx, 200000); // yes, 200 seconds
        }

        public bool UpdateAgent(GridRegion destination, AgentPosition data)
        {
            bool v;
            if (_failedSims.TryGetValue(destination.ServerURI, out v))
                return false;

            string uri = destination.ServerURI + AgentPath() + data.AgentID + "/";
            lock (m_updateAgentQueue)
            {
                if (m_updateAgentQueue.ContainsKey(uri))
                {
                    // Another thread is already handling updates for this simulator, just update the position and return.
                    m_updateAgentQueue[uri] = data;
                    return true;
                }
                m_updateAgentQueue[uri] = data;
            }

            AgentPosition pos = null;
            bool success = true;
            while (success)
            {
                lock (m_updateAgentQueue)
                {
                    AgentPosition lastpos = pos;
                    pos = m_updateAgentQueue[uri];
                    if (pos == lastpos)
                    {
                        m_updateAgentQueue.Remove(uri);
                        return true;
                    }
                }

                EntityTransferContext ctx = new EntityTransferContext(); // Dummy, not needed for position
                success = UpdateAgent(destination, (IAgentData)pos, ctx, 10000);
            }

            lock (m_updateAgentQueue)
            {
                _failedSims.AddOrUpdate(destination.ServerURI, true, 120);
                m_updateAgentQueue.Remove(uri);
            }
            return false;
        }

        private bool UpdateAgent(GridRegion destination, IAgentData cAgentData, EntityTransferContext ctx, int timeout)
        {
            string uri = destination.ServerURI + AgentPath() + cAgentData.AgentID + "/";

            try
            {
                OSDMap args = cAgentData.Pack();

                args["destination_x"] = OSD.FromInteger(destination.RegionLocX);
                args["destination_y"] = OSD.FromInteger(destination.RegionLocY);
                args["destination_name"] = OSD.FromString(destination.RegionName);
                args["destination_uuid"] = OSD.FromUUID(destination.RegionID);
                if (ctx == null)
                    ctx = new EntityTransferContext();
                OSDMap ctxMap = new OSDMap();
                ctx.Pack(ctxMap);
                args["context"] = ctxMap;

                OSDMap result;
                if (ctx.OutboundVersion >= 0.3)
                {
                    result = WebUtil.PutToServiceCompressed(uri, args, timeout);
                    return result["Success"].AsBoolean();
                }

                result = WebUtil.PutToServiceCompressed(uri, args, timeout);
                if (result["Success"].AsBoolean())
                    return true;
                if (ctx.OutboundVersion < 0.2)
                    result = WebUtil.PutToServiceCompressed(uri, args, timeout);

                return result["Success"].AsBoolean();
            }
            catch (Exception e)
            {
                m_log.Warn("[REMOTE SIMULATION CONNECTOR]: UpdateAgent failed with exception: " + e);
            }

            return false;
        }

        public bool QueryAccess(GridRegion destination, UUID agentID, string agentHomeURI, bool viaTeleport, Vector3 position, List<UUID> featuresAvailable, EntityTransferContext ctx, out string reason)
        {
            Culture.SetCurrentCulture();
            reason = "Failed to contact destination";

            string uri = destination.ServerURI + AgentPath() + agentID + "/" + destination.RegionID + "/";

            OSDMap request = new OSDMap();
            request.Add("viaTeleport", OSD.FromBoolean(viaTeleport));
            request.Add("position", OSD.FromString(position.ToString()));
            request.Add("my_version", OSD.FromString(string.Format("SIMULATION/{0}", VersionInfo.SimulationServiceVersionSupportedMin)));
            request.Add("simulation_service_supported_min", OSD.FromReal(VersionInfo.SimulationServiceVersionSupportedMin));
            request.Add("simulation_service_supported_max", OSD.FromReal(VersionInfo.SimulationServiceVersionSupportedMax));
            request.Add("simulation_service_accepted_min", OSD.FromReal(VersionInfo.SimulationServiceVersionAcceptedMin));
            request.Add("simulation_service_accepted_max", OSD.FromReal(VersionInfo.SimulationServiceVersionAcceptedMax));

            request.Add("context", ctx != null ? ctx.Pack() : new OSDMap());

            OSDArray features = new OSDArray();
            foreach (UUID feature in featuresAvailable)
                features.Add(OSD.FromString(feature.ToString()));

            request.Add("features", features);

            if (agentHomeURI != null)
                request.Add("agent_home_uri", OSD.FromString(agentHomeURI));

            OSD tmpOSD;
            try
            {
                OSDMap result = WebUtil.PostToService(uri, request, 30000, false);

                bool success = result["success"].AsBoolean();
                bool has_Result = false;
                if (result.TryGetValue("_Result", out tmpOSD))
                {
                    has_Result = true;
                    OSDMap data = (OSDMap)tmpOSD;

                    success = data["success"].AsBoolean();
                    reason = data["reason"].AsString();

                    if (data.TryGetValue("negotiated_inbound_version", out tmpOSD) && tmpOSD != null)
                    {
                        ctx.InboundVersion = (float)tmpOSD.AsReal();
                        ctx.OutboundVersion = (float)data["negotiated_outbound_version"].AsReal();
                    }
                    else if (data.TryGetValue("version", out tmpOSD) && tmpOSD != null)
                    {
                        string versionString = tmpOSD.AsString();
                        if (versionString != string.Empty)
                        {
                            string[] parts = versionString.Split(new char[] { '/' });
                            if (parts.Length > 1)
                            {
                                ctx.InboundVersion = float.Parse(parts[1], Culture.FormatProvider);
                                ctx.OutboundVersion = float.Parse(parts[1], Culture.FormatProvider);
                            }
                        }
                    }

                    m_log.DebugFormat(
                        "[REMOTE SIMULATION CONNECTOR]: QueryAccess to {0} returned {1}, reason {2}, version {3}/{4}",
                        uri, success, reason, ctx.InboundVersion, ctx.OutboundVersion);
                }

                if (!success || ctx.InboundVersion == 0f || ctx.OutboundVersion == 0f)
                {
                    if (!has_Result)
                    {
                        if (result.TryGetValue("Message", out tmpOSD))
                        {
                            string message = tmpOSD.AsString();
                            if (message == "Service request failed: [MethodNotAllowed] MethodNotAllowed")
                            {
                                m_log.Info("[REMOTE SIMULATION CONNECTOR]: The above web util error was caused by a TP to a sim that doesn't support QUERYACCESS and can be ignored");
                                return true;
                            }
                            reason = result["Message"];
                        }
                        else
                        {
                            reason = "Communications failure";
                        }
                    }

                    return false;
                }

                featuresAvailable.Clear();

                OSDArray featureArray = null;
                if (result.TryGetValue("features", out tmpOSD))
                    featureArray = tmpOSD as OSDArray;
                if (featureArray != null)
                {
                    foreach (OSD o in featureArray)
                        featuresAvailable.Add(new UUID(o.AsString()));
                }

                if (ctx.OutboundVersion < 0.5)
                    ctx.WearablesCount = 13;
                else if (ctx.OutboundVersion < 0.6)
                    ctx.WearablesCount = 14;
                else
                    ctx.WearablesCount = -1; // send all

                return success;
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[REMOTE SIMULATION CONNECTOR] QueryAcesss failed with exception; {0}", e);
            }

            return false;
        }

        public bool ReleaseAgent(UUID origin, UUID id, string uri)
        {
            try
            {
                WebUtil.ServiceOSDRequest(uri, null, "DELETE", 10000, false, false, false);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[REMOTE SIMULATION CONNECTOR] ReleaseAgent failed with exception; {0}", e);
            }

            return true;
        }

        public bool CloseAgent(GridRegion destination, UUID id, string auth_code)
        {
            string uri = destination.ServerURI + AgentPath() + id + "/" + destination.RegionID + "/?auth=" + auth_code;
            m_log.DebugFormat("[REMOTE SIMULATION CONNECTOR]: CloseAgent {0}", uri);

            try
            {
                WebUtil.ServiceOSDRequest(uri, null, "DELETE", 10000, false, false, false);
            }
            catch (Exception e)
            {
                m_log.WarnFormat("[REMOTE SIMULATION CONNECTOR] CloseAgent failed with exception; {0}", e);
            }

            return true;
        }
    }
}

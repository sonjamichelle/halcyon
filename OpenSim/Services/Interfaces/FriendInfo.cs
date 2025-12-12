/*
 * Minimal FriendInfo port for Hypergrid connectors.
 */

using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace OpenSim.Services.Interfaces
{
    public class FriendInfo
    {
        public UUID PrincipalID { get; set; }
        public string Friend { get; set; }
        public int MyFlags { get; set; }
        public int TheirFlags { get; set; }

        public Dictionary<string, object> ToKeyValuePairs()
        {
            return new Dictionary<string, object>
            {
                { "PrincipalID", PrincipalID.ToString() },
                { "Friend", Friend },
                { "MyFlags", MyFlags },
                { "TheirFlags", TheirFlags }
            };
        }
    }
}

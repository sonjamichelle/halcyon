using System;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Services.Interfaces;

namespace OpenSim.Services.Connectors.Friends
{
    /// <summary>
    /// Minimal IFriendsSimConnector implementation used by HG connectors.
    /// </summary>
    public class FriendsSimConnector : IFriendsSimConnector
    {
        public virtual bool StatusNotify(UUID userID, UUID friendID, bool online)
        {
            return false;
        }

        public virtual bool LocalFriendshipOffered(UUID toID, GridInstantMessage im)
        {
            return false;
        }

        public virtual bool LocalFriendshipApproved(UUID userID, string userName, UUID friendID)
        {
            return false;
        }
    }
}

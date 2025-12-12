using OpenMetaverse;

namespace OpenSim.Services.Interfaces
{
    /// <summary>
    /// Minimal GridInfo stub for HG connectors.
    /// </summary>
    public class GridInfo
    {
        public string GridName { get; set; }
        public string GridNick { get; set; }
        public string LoginURI { get; set; }
        public string GridURI { get; set; }
        public string HomeURL { get; set; }
        public UUID HomeUUID { get; set; }

        public GridInfo()
        {
        }

        public GridInfo(string homeUrl)
        {
            HomeURL = homeUrl;
        }
    }
}

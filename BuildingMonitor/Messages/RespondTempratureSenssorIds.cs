using System.Collections.Generic;

namespace BuildingMonitor.Messages
{
    public sealed class RespondTempratureSenssorIds
    {
        public long RequestId { get; private set; }

        public ISet<string> Ids { get; private set; }

        public RespondTempratureSenssorIds(long requestId, ISet<string> ids)
        {
            this.RequestId = requestId;
            this.Ids = ids;
        }
    }
}

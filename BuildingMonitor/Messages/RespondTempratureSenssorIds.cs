using System.Collections.Immutable;

namespace BuildingMonitor.Messages
{
    public sealed class RespondTempratureSenssorIds
    {
        public long RequestId { get; private set; }

        public IImmutableSet<string> Ids { get; private set; }

        public RespondTempratureSenssorIds(long requestId, IImmutableSet<string> ids)
        {
            this.RequestId = requestId;
            this.Ids = ids;
        }
    }
}

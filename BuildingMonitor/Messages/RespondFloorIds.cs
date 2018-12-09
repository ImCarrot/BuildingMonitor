using System.Collections.Immutable;

namespace BuildingMonitor.Messages
{
    public sealed class RespondFloorIds
    {
        public long RequestId { get; }

        public IImmutableSet<string> Ids { get; }

        public RespondFloorIds(long requestId, IImmutableSet<string> ids)
        {
            this.RequestId = requestId;
            this.Ids = ids;
        }
    }
}


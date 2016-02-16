using DomainBus.Dispatcher;

namespace DomainBus.Sql.ReservedIds
{
    public class ReservedIdRow
    {
        public string Id { get; set; }
        public string Data { get; set; }

        public static string GetId(ReservedIdsSource src) => src.MessageId + src.HandlerType.FullName;
    }
}
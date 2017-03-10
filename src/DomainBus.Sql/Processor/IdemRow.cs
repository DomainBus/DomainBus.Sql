using System;

namespace DomainBus.Sql.Processor
{
    public class IdemRow
    {
        public string MessageId { get; set; }
        public DateTime AddedOn { get; set; }=DateTime.UtcNow;
    }
}
using System;

namespace DomainBus.Sql.Processor
{
    public class ProcessorMessagesRow
    {
        public long ArrivalId { get; set; }
        public Guid MessageId { get; set; }        
        public string Processor { get; set; }
        public byte[] Data { get; set; }
    }
}
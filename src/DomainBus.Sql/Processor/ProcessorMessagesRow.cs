using System;

namespace DomainBus.Sql.Processor
{
    public class ProcessorMessagesRow
    {
        public Guid Id { get; set; }
        public string Processor { get; set; }
        public DateTime ArrivedAt { get; set; }
        public byte[] Data { get; set; }
    }
}
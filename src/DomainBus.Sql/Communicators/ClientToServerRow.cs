using System;

namespace DomainBus.Sql.Communicators
{
    public class ClientToServerRow
    {
        //public const int EndpointConfig = "config";
        //public const string ToRelay = "envelope";

        public int Id { get; set; }
        public string DataId { get; set; }
        public int Type { get; set; }
        public byte[] Data { get; set; }
    }
}
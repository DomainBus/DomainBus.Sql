using System;

namespace DomainBus.Sql.Saga
{
    public class SagaRow
    {
        public string SagaId { get; set; }
        public byte[] Data { get; set; }
        public long Version { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastChangedOn { get; set; }

        public static string GetId(string correlation, Type type)
            => (correlation + type.FullName).MurmurHash().ToBase64();
    }
}
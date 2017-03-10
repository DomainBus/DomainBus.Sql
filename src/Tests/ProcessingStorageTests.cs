using System;
using System.Linq;
using System.Threading.Tasks;
using DomainBus;
using DomainBus.Abstractions;
using DomainBus.Configuration;
using DomainBus.Processing;
using DomainBus.Sql;
using FluentAssertions;
using NSubstitute;

using SqlFu.Builders;
using Xunit;

namespace Tests
{
    public class ProcessingStorageTests
    {
        private IConfigureHost _host;
        private StoragesConfiguration _cfg;
        private IStoreUnhandledMessages _sut;

        public ProcessingStorageTests()
        {
            _host = Substitute.For<IConfigureHost>();
            
            _host.WithProcessingStorage(Arg.Do<IStoreUnhandledMessages>(v => _sut = v));
            _cfg =new StoragesConfiguration(_host,Setup.GetConnection());
            _cfg.EnableProcessorStorage(ifExists: TableExistsAction.DropIt);
            
        }

        //[Fact]
        //public async Task add_get()
        //{
        //    var msg = GetMessage().ToList();
        //    var myEvent = new MyEvent() {TimeStamp = DateTimeOffset.Now.AddDays(-100)};
        //    msg.Add(myEvent);

        //    await _sut.Add("bla", msg);
        //    var all=_sut.GetMessages("bla", 20);
        //    all.ShouldAllBeEquivalentTo(msg);
        //    all.Should().BeInAscendingOrder(d => d.TimeStamp);
        //    all.Cast<MyEvent>().Any(d => d.Some == "Lala").Should().BeTrue();
        //}


        [Fact]
        public async Task mark_as_handled()
        {
            var msg = GetMessage();
            await _sut.Add("bla", msg);
            _sut.MarkMessageHandled("bla",msg.First().Id);

            var all = _sut.GetMessages("bla", 20);
            all.Count().Should().Be(2);
            all.Any(d => d.Id == msg.First().Id).Should().BeFalse();
        }

        IMessage[] GetMessage()
        {
            return new[] {new MyEvent(),new MyEvent(), new MyEvent()};
        }

        [Fact]
        public async Task duplicate_messages_are_ignored()
        {
            var msg = GetMessage();
            await _sut.Add("bla", msg);
            _sut.MarkMessageHandled("bla", msg.First().Id);
            await _sut.Add("bla", new[] {new MyEvent() {Id = msg.First().Id}});

            var all = _sut.GetMessages("bla", 20);
            all.Count().Should().Be(2);
            all.Any(d => d.Id == msg.First().Id).Should().BeFalse();

        }
    }

    public class MyEvent : AbstractEvent
    {
        public string Some { get; set; } = "Lala";

    }
}
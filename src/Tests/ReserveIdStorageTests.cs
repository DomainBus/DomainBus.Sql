using System;
using System.Linq;
using DomainBus.Configuration;
using DomainBus.Dispatcher;
using DomainBus.Processing;
using DomainBus.Sql;
using FluentAssertions;
using NSubstitute;

using SqlFu.Builders;
using Xunit;

namespace Tests
{
    public class ReserveIdStorageTests
    {
        private IConfigureHost _host;
        private IStoreReservedMessagesIds _sut;
        private StoragesConfiguration _cfg;
        private ReservedIdsSource _src;

        public ReserveIdStorageTests()
        {
            _host = Substitute.For<IConfigureHost>();

            _host.WithReserveIdStorage(Arg.Do<IStoreReservedMessagesIds>(v => _sut = v));
            _cfg = new StoragesConfiguration(_host, Setup.GetConnection());
            _cfg.EnableReserveIdStorage(ifExists: TableExistsAction.DropIt);

            _src=new ReservedIdsSource()
            {
                Count = 2,
                HandlerType = GetType(),
                MessageId = Guid.Empty
            };
        }

        [Fact]
        public void add_get()
        {
            var ids = new[] {Guid.NewGuid(), Guid.NewGuid()};
            _sut.Add(_src,ids);
            _sut.Get(_src).ShouldAllBeEquivalentTo(ids);
        }

        [Fact]
        public void not_existing_returns_empty()
        {
            _sut.Get(new ReservedIdsSource() { }).Should().BeEmpty();
        }
    }


}
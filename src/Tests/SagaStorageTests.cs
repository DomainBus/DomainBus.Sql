using DomainBus;
using DomainBus.Configuration;
using DomainBus.Processing;
using DomainBus.Sql;
using DomainBus.Sql.Saga;
using FluentAssertions;
using NSubstitute;
using SqlFu.Builders;
using Xunit;

namespace Tests
{
    public class SagaStorageTests
    {
        private IConfigureHost _host;
        private StoragesConfiguration _cfg;
        private IStoreSagaState _sut;

        public SagaStorageTests()
        {
            _host = Substitute.For<IConfigureHost>();
            _cfg = new StoragesConfiguration(_host, Setup.GetConnection());
            _cfg.EnableSagaStorage(ifExists: TableExistsAction.DropIt);
            _sut = new SagaStorage(Setup.GetConnection());
        }

        [Fact]
        public void save_get()
        {
            _sut.Save(new MySaga(), "id",true);
            var data=_sut.GetSaga("id", typeof (MySaga)) as MySaga;
            data.RawData["hi"].Should().Be("world");
        }


        [Fact]
        public void unexisting_saga_returns_null()
        {
            _sut.Save(new MySaga(), "id", true);
            _sut.GetSaga("f", typeof (MySaga)).Should().BeNull();
        }

        [Fact]
        public void save_saga()
        {
            _sut.Save(new MySaga(), "id", true);
            var data = _sut.GetSaga("id", typeof(MySaga)) as MySaga;
            data.IsCompleted.Should().BeFalse();
            data.IsCompleted = true;
            _sut.Save(data,"id",false);

            var mod = _sut.GetSaga("id", typeof(MySaga)) as MySaga;
            mod.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void duplicate_saga_throws()
        {
            _sut.Save(new MySaga(), "id", true);
            _sut.Invoking(d=>d.Save(new MySaga(), "id", true)).ShouldThrow<SagaExistsException>();
        }
    }


    public class MySaga : ASagaState
    {
        public MySaga()
        {
            this.RawData["hi"] = "world";
        }
    }
}
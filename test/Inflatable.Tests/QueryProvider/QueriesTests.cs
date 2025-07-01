using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using System.Data;
using Xunit;

namespace Inflatable.Tests.QueryProvider
{
    public class QueriesTests
    {
        [Fact]
        public void Add()
        {
            var TestObject = new Queries
            {
                { QueryType.Delete, new Query(typeof(object),CommandType.Text, "ASDF", QueryType.Delete) }
            };
            Assert.Single(TestObject);
            Assert.Single(TestObject.Keys);
            Assert.Single(TestObject.Values);
        }

        [Fact]
        public void Clear()
        {
            var TestObject = new Queries
            {
                { QueryType.Delete, new Query(typeof(object),CommandType.Text, "ASDF", QueryType.Delete) }
            };
            TestObject.Clear();
            Assert.Empty(TestObject);
            Assert.Empty(TestObject.Keys);
            Assert.Empty(TestObject.Values);
        }

        [Fact]
        public void ContainsKey()
        {
            var TestObject = new Queries
            {
                { QueryType.Delete, new Query(typeof(object),CommandType.Text, "ASDF", QueryType.Delete) }
            };
            Assert.True(TestObject.ContainsKey(QueryType.Delete));
            Assert.False(TestObject.ContainsKey(QueryType.Insert));
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new Queries();
            Assert.Empty(TestObject);
            Assert.False(TestObject.IsReadOnly);
            Assert.Empty(TestObject.Keys);
            Assert.Empty(TestObject.Values);
        }

        [Fact]
        public void Index()
        {
            var TestObject = new Queries
            {
                { QueryType.Delete, new Query(typeof(object),CommandType.Text, "ASDF", QueryType.Delete) }
            };
            var QueryObject = TestObject[QueryType.Delete];
            Assert.Equal(CommandType.Text, QueryObject.DatabaseCommandType);
            Assert.Equal("ASDF", QueryObject.QueryString);
            Assert.Equal(QueryType.Delete, QueryObject.QueryType);
        }

        [Fact]
        public void Remove()
        {
            var TestObject = new Queries
            {
                { QueryType.Delete, new Query(typeof(object),CommandType.Text, "ASDF", QueryType.Delete) }
            };
            TestObject.Remove(QueryType.Delete);
            Assert.Empty(TestObject);
            Assert.Empty(TestObject.Keys);
            Assert.Empty(TestObject.Values);
        }
    }
}
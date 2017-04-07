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
            var TestObject = new Queries();
            TestObject.Add(QueryType.Delete, new Query(CommandType.Text, "ASDF", QueryType.Delete));
            Assert.Equal(1, TestObject.Count);
            Assert.Equal(1, TestObject.Keys.Count);
            Assert.Equal(1, TestObject.Values.Count);
        }

        [Fact]
        public void Clear()
        {
            var TestObject = new Queries();
            TestObject.Add(QueryType.Delete, new Query(CommandType.Text, "ASDF", QueryType.Delete));
            TestObject.Clear();
            Assert.Equal(0, TestObject.Count);
            Assert.Empty(TestObject.Keys);
            Assert.Empty(TestObject.Values);
        }

        [Fact]
        public void ContainsKey()
        {
            var TestObject = new Queries();
            TestObject.Add(QueryType.Delete, new Query(CommandType.Text, "ASDF", QueryType.Delete));
            Assert.True(TestObject.ContainsKey(QueryType.Delete));
            Assert.False(TestObject.ContainsKey(QueryType.Insert));
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new Queries();
            Assert.Equal(0, TestObject.Count);
            Assert.False(TestObject.IsReadOnly);
            Assert.Empty(TestObject.Keys);
            Assert.Empty(TestObject.Values);
        }

        [Fact]
        public void Index()
        {
            var TestObject = new Queries();
            TestObject.Add(QueryType.Delete, new Query(CommandType.Text, "ASDF", QueryType.Delete));
            var QueryObject = TestObject[QueryType.Delete];
            Assert.Equal(CommandType.Text, QueryObject.DatabaseCommandType);
            Assert.Equal("ASDF", QueryObject.QueryString);
            Assert.Equal(QueryType.Delete, QueryObject.QueryType);
        }

        [Fact]
        public void Remove()
        {
            var TestObject = new Queries();
            TestObject.Add(QueryType.Delete, new Query(CommandType.Text, "ASDF", QueryType.Delete));
            TestObject.Remove(QueryType.Delete);
            Assert.Equal(0, TestObject.Count);
            Assert.Empty(TestObject.Keys);
            Assert.Empty(TestObject.Values);
        }
    }
}
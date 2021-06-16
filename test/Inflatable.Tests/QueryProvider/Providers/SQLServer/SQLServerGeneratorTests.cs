using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer
{
    public class SQLServerGeneratorTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                new MockDatabaseMapping(),
                new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal(typeof(ConcreteClass1), Result.AssociatedType);
            Assert.Equal(Mappings, Result.MappingInformation);
        }

        [Fact]
        public void GenerateDefaultQueriesConcreteClass1()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                new MockDatabaseMapping(),
                new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n", Result.GenerateQueries(QueryType.Delete, new ConcreteClass1())[0].QueryString);

            Assert.Equal("DECLARE @IInterface1_ID_Temp AS INT;", Result.GenerateDeclarations(QueryType.Insert)[2].QueryString);
            Assert.Equal("DECLARE @BaseClass1_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[1].QueryString);
            Assert.Equal("DECLARE @ConcreteClass1_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[0].QueryString);

            Assert.Equal(@"INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;
SET @IInterface1_ID_Temp=SCOPE_IDENTITY();
SELECT @IInterface1_ID_Temp AS [ID];

INSERT INTO [dbo].[BaseClass1_]([dbo].[BaseClass1_].[BaseClassValue1_],[dbo].[BaseClass1_].[IInterface1_ID_]) VALUES (@BaseClassValue1,@IInterface1_ID_Temp);
SET @BaseClass1_ID_Temp=SCOPE_IDENTITY();

INSERT INTO [dbo].[ConcreteClass1_]([dbo].[ConcreteClass1_].[Value1_],[dbo].[ConcreteClass1_].[BaseClass1_ID_]) VALUES (@Value1,@BaseClass1_ID_Temp);
", Result.GenerateQueries(QueryType.Insert, new ConcreteClass1())[0].QueryString);

            Assert.Equal(@"UPDATE [dbo].[BaseClass1_]
SET [dbo].[BaseClass1_].[BaseClassValue1_]=@BaseClassValue1
FROM [dbo].[BaseClass1_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
WHERE [dbo].[IInterface1_].[ID_]=@ID;

UPDATE [dbo].[ConcreteClass1_]
SET [dbo].[ConcreteClass1_].[Value1_]=@Value1
FROM [dbo].[ConcreteClass1_]
INNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass1_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
WHERE [dbo].[IInterface1_].[ID_]=@ID;
", Result.GenerateQueries(QueryType.Update, new ConcreteClass1())[0].QueryString);

            Assert.Equal(@"SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[BaseClass1_].[BaseClassValue1_] AS [BaseClassValue1],[dbo].[ConcreteClass1_].[Value1_] AS [Value1]
FROM [dbo].[ConcreteClass1_]
INNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass1_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
ORDER BY [dbo].[IInterface1_].[ID_];", Result.GenerateQueries(QueryType.LinqQuery, new ConcreteClass1())[0].QueryString);
        }

        [Fact]
        public void GenerateDefaultQueriesConcreteClass2()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                new MockDatabaseMapping(),
                new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass2>(Mappings, ObjectPool);
            Assert.Equal("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n", Result.GenerateQueries(QueryType.Delete, new ConcreteClass2())[0].QueryString);

            Assert.Equal("DECLARE @IInterface1_ID_Temp AS INT;", Result.GenerateDeclarations(QueryType.Insert)[2].QueryString);
            Assert.Equal("DECLARE @BaseClass1_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[1].QueryString);
            Assert.Equal("DECLARE @ConcreteClass2_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[0].QueryString);

            Assert.Equal(@"INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;
SET @IInterface1_ID_Temp=SCOPE_IDENTITY();
SELECT @IInterface1_ID_Temp AS [ID];

INSERT INTO [dbo].[BaseClass1_]([dbo].[BaseClass1_].[BaseClassValue1_],[dbo].[BaseClass1_].[IInterface1_ID_]) VALUES (@BaseClassValue1,@IInterface1_ID_Temp);
SET @BaseClass1_ID_Temp=SCOPE_IDENTITY();

INSERT INTO [dbo].[ConcreteClass2_]([dbo].[ConcreteClass2_].[InterfaceValue_],[dbo].[ConcreteClass2_].[BaseClass1_ID_]) VALUES (@InterfaceValue,@BaseClass1_ID_Temp);
", Result.GenerateQueries(QueryType.Insert, new ConcreteClass2())[0].QueryString);

            Assert.Equal(@"UPDATE [dbo].[BaseClass1_]
SET [dbo].[BaseClass1_].[BaseClassValue1_]=@BaseClassValue1
FROM [dbo].[BaseClass1_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
WHERE [dbo].[IInterface1_].[ID_]=@ID;

UPDATE [dbo].[ConcreteClass2_]
SET [dbo].[ConcreteClass2_].[InterfaceValue_]=@InterfaceValue
FROM [dbo].[ConcreteClass2_]
INNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass2_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
WHERE [dbo].[IInterface1_].[ID_]=@ID;
", Result.GenerateQueries(QueryType.Update, new ConcreteClass2())[0].QueryString);

            Assert.Equal(@"SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[BaseClass1_].[BaseClassValue1_] AS [BaseClassValue1],[dbo].[ConcreteClass2_].[InterfaceValue_] AS [InterfaceValue]
FROM [dbo].[ConcreteClass2_]
INNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass2_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
ORDER BY [dbo].[IInterface1_].[ID_];", Result.GenerateQueries(QueryType.LinqQuery, new ConcreteClass2())[0].QueryString);
        }

        [Fact]
        public void GenerateDefaultQueriesConcreteClass3()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                new MockDatabaseMapping(),
                new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass3>(Mappings, ObjectPool);

            Assert.Equal("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n", Result.GenerateQueries(QueryType.Delete, new ConcreteClass3())[0].QueryString);

            Assert.Equal("DECLARE @IInterface1_ID_Temp AS INT;", Result.GenerateDeclarations(QueryType.Insert)[1].QueryString);
            Assert.Equal("DECLARE @ConcreteClass3_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[0].QueryString);

            Assert.Equal(@"INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;
SET @IInterface1_ID_Temp=SCOPE_IDENTITY();
SELECT @IInterface1_ID_Temp AS [ID];

INSERT INTO [dbo].[ConcreteClass3_]([dbo].[ConcreteClass3_].[MyUniqueProperty_],[dbo].[ConcreteClass3_].[IInterface1_ID_]) VALUES (@MyUniqueProperty,@IInterface1_ID_Temp);
", Result.GenerateQueries(QueryType.Insert, new ConcreteClass3())[0].QueryString);

            Assert.Equal(@"UPDATE [dbo].[ConcreteClass3_]
SET [dbo].[ConcreteClass3_].[MyUniqueProperty_]=@MyUniqueProperty
FROM [dbo].[ConcreteClass3_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[ConcreteClass3_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
WHERE [dbo].[IInterface1_].[ID_]=@ID;
", Result.GenerateQueries(QueryType.Update, new ConcreteClass3())[0].QueryString);

            Assert.Equal(@"SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[ConcreteClass3_].[MyUniqueProperty_] AS [MyUniqueProperty]
FROM [dbo].[ConcreteClass3_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[ConcreteClass3_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
ORDER BY [dbo].[IInterface1_].[ID_];", Result.GenerateQueries(QueryType.LinqQuery, new ConcreteClass3())[0].QueryString);
        }
    }
}
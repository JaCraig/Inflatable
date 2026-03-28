using Inflatable.ClassMapper;
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
    [Collection("Test collection")]
    public class SQLServerGeneratorTests : TestingFixture
    {
        public SQLServerGeneratorTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void Creation()
        {
            var Mappings = new MappingSource([
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            ],
                new MockDatabaseMapping(),
                new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal(typeof(ConcreteClass1), Result.AssociatedType);
            Assert.Equal(Mappings, Result.MappingInformation);
        }

        [Fact]
        public void GenerateDefaultQueriesConcreteClass1()
        {
            var Mappings = new MappingSource([
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            ],
                new MockDatabaseMapping(),
                new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Delete, new ConcreteClass1())[0].QueryString));

            Assert.Equal("DECLARE @IInterface1_ID_Temp AS INT;", Result.GenerateDeclarations(QueryType.Insert)[2].QueryString);
            Assert.Equal("DECLARE @BaseClass1_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[1].QueryString);
            Assert.Equal("DECLARE @ConcreteClass1_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[0].QueryString);

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;\r\nSET @IInterface1_ID_Temp=SCOPE_IDENTITY();\r\nSELECT @IInterface1_ID_Temp AS [ID];\r\n\r\nINSERT INTO [dbo].[BaseClass1_]([dbo].[BaseClass1_].[BaseClassValue1_],[dbo].[BaseClass1_].[IInterface1_ID_]) VALUES (@BaseClassValue1,@IInterface1_ID_Temp);\r\nSET @BaseClass1_ID_Temp=SCOPE_IDENTITY();\r\n\r\nINSERT INTO [dbo].[ConcreteClass1_]([dbo].[ConcreteClass1_].[Value1_],[dbo].[ConcreteClass1_].[BaseClass1_ID_]) VALUES (@Value1,@BaseClass1_ID_Temp);\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Insert, new ConcreteClass1())[0].QueryString));

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("UPDATE [dbo].[BaseClass1_]\r\nSET [dbo].[BaseClass1_].[BaseClassValue1_]=@BaseClassValue1\r\nFROM [dbo].[BaseClass1_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n\r\nUPDATE [dbo].[ConcreteClass1_]\r\nSET [dbo].[ConcreteClass1_].[Value1_]=@Value1\r\nFROM [dbo].[ConcreteClass1_]\r\nINNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass1_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Update, new ConcreteClass1())[0].QueryString));

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[BaseClass1_].[BaseClassValue1_] AS [BaseClassValue1],[dbo].[ConcreteClass1_].[Value1_] AS [Value1]\r\nFROM [dbo].[ConcreteClass1_]\r\nINNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass1_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nORDER BY [dbo].[IInterface1_].[ID_];"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.LinqQuery, new ConcreteClass1())[0].QueryString));
        }

        [Fact]
        public void GenerateDefaultQueriesConcreteClass2()
        {
            var Mappings = new MappingSource([
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            ],
                new MockDatabaseMapping(),
                new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass2>(Mappings, ObjectPool);
            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Delete, new ConcreteClass2())[0].QueryString));

            Assert.Equal("DECLARE @IInterface1_ID_Temp AS INT;", Result.GenerateDeclarations(QueryType.Insert)[2].QueryString);
            Assert.Equal("DECLARE @BaseClass1_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[1].QueryString);
            Assert.Equal("DECLARE @ConcreteClass2_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[0].QueryString);

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;\r\nSET @IInterface1_ID_Temp=SCOPE_IDENTITY();\r\nSELECT @IInterface1_ID_Temp AS [ID];\r\n\r\nINSERT INTO [dbo].[BaseClass1_]([dbo].[BaseClass1_].[BaseClassValue1_],[dbo].[BaseClass1_].[IInterface1_ID_]) VALUES (@BaseClassValue1,@IInterface1_ID_Temp);\r\nSET @BaseClass1_ID_Temp=SCOPE_IDENTITY();\r\n\r\nINSERT INTO [dbo].[ConcreteClass2_]([dbo].[ConcreteClass2_].[InterfaceValue_],[dbo].[ConcreteClass2_].[BaseClass1_ID_]) VALUES (@InterfaceValue,@BaseClass1_ID_Temp);\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Insert, new ConcreteClass2())[0].QueryString));

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("UPDATE [dbo].[BaseClass1_]\r\nSET [dbo].[BaseClass1_].[BaseClassValue1_]=@BaseClassValue1\r\nFROM [dbo].[BaseClass1_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n\r\nUPDATE [dbo].[ConcreteClass2_]\r\nSET [dbo].[ConcreteClass2_].[InterfaceValue_]=@InterfaceValue\r\nFROM [dbo].[ConcreteClass2_]\r\nINNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass2_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Update, new ConcreteClass2())[0].QueryString));

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[BaseClass1_].[BaseClassValue1_] AS [BaseClassValue1],[dbo].[ConcreteClass2_].[InterfaceValue_] AS [InterfaceValue]\r\nFROM [dbo].[ConcreteClass2_]\r\nINNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass2_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nORDER BY [dbo].[IInterface1_].[ID_];"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.LinqQuery, new ConcreteClass2())[0].QueryString));
        }

        [Fact]
        public void GenerateDefaultQueriesConcreteClass3()
        {
            var Mappings = new MappingSource([
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            ],
                new MockDatabaseMapping(),
                new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var Result = new SQLServerGenerator<ConcreteClass3>(Mappings, ObjectPool);

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Delete, new ConcreteClass3())[0].QueryString));

            Assert.Equal("DECLARE @IInterface1_ID_Temp AS INT;", Result.GenerateDeclarations(QueryType.Insert)[1].QueryString);
            Assert.Equal("DECLARE @ConcreteClass3_ID_Temp AS BIGINT;", Result.GenerateDeclarations(QueryType.Insert)[0].QueryString);

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;\r\nSET @IInterface1_ID_Temp=SCOPE_IDENTITY();\r\nSELECT @IInterface1_ID_Temp AS [ID];\r\n\r\nINSERT INTO [dbo].[ConcreteClass3_]([dbo].[ConcreteClass3_].[MyUniqueProperty_],[dbo].[ConcreteClass3_].[IInterface1_ID_]) VALUES (@MyUniqueProperty,@IInterface1_ID_Temp);\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Insert, new ConcreteClass3())[0].QueryString));

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("UPDATE [dbo].[ConcreteClass3_]\r\nSET [dbo].[ConcreteClass3_].[MyUniqueProperty_]=@MyUniqueProperty\r\nFROM [dbo].[ConcreteClass3_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[ConcreteClass3_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.Update, new ConcreteClass3())[0].QueryString));

            Assert.Equal(TestConnectionStrings.NormalizeLineEndings("SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[ConcreteClass3_].[MyUniqueProperty_] AS [MyUniqueProperty]\r\nFROM [dbo].[ConcreteClass3_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[ConcreteClass3_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nORDER BY [dbo].[IInterface1_].[ID_];"), TestConnectionStrings.NormalizeLineEndings(Result.GenerateQueries(QueryType.LinqQuery, new ConcreteClass3())[0].QueryString));
        }
    }
}
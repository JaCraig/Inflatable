using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using Serilog;
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
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var Result = new SQLServerGenerator<ConcreteClass1>(Mappings);
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
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var Result = new SQLServerGenerator<ConcreteClass1>(Mappings);
            var Queries = Result.GenerateDefaultQueries();
            Assert.Equal(3, Queries.Count);
            Assert.Equal("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n", Queries[QueryType.Delete].QueryString);

            Assert.Equal(@"DECLARE @IInterface1_ID_Temp AS INT;
INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;
SET @IInterface1_ID_Temp=SCOPE_IDENTITY();
SELECT @IInterface1_ID_Temp AS [ID];

DECLARE @BaseClass1_ID_Temp AS BIGINT;
INSERT INTO [dbo].[BaseClass1_]([dbo].[BaseClass1_].[BaseClassValue1_],[dbo].[BaseClass1_].[IInterface1_ID_]) VALUES (@BaseClassValue1,@IInterface1_ID_Temp);
SET @BaseClass1_ID_Temp=SCOPE_IDENTITY();

INSERT INTO [dbo].[ConcreteClass1_]([dbo].[ConcreteClass1_].[Value1_],[dbo].[ConcreteClass1_].[BaseClass1_ID_]) VALUES (@Value1,@BaseClass1_ID_Temp);
", Queries[QueryType.Insert].QueryString);

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
", Queries[QueryType.Update].QueryString);
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
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var Result = new SQLServerGenerator<ConcreteClass2>(Mappings);
            var Queries = Result.GenerateDefaultQueries();
            Assert.Equal(3, Queries.Count);
            Assert.Equal("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n", Queries[QueryType.Delete].QueryString);

            Assert.Equal(@"DECLARE @IInterface1_ID_Temp AS INT;
INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;
SET @IInterface1_ID_Temp=SCOPE_IDENTITY();
SELECT @IInterface1_ID_Temp AS [ID];

DECLARE @BaseClass1_ID_Temp AS BIGINT;
INSERT INTO [dbo].[BaseClass1_]([dbo].[BaseClass1_].[BaseClassValue1_],[dbo].[BaseClass1_].[IInterface1_ID_]) VALUES (@BaseClassValue1,@IInterface1_ID_Temp);
SET @BaseClass1_ID_Temp=SCOPE_IDENTITY();

INSERT INTO [dbo].[ConcreteClass2_]([dbo].[ConcreteClass2_].[InterfaceValue_],[dbo].[ConcreteClass2_].[BaseClass1_ID_]) VALUES (@InterfaceValue,@BaseClass1_ID_Temp);
", Queries[QueryType.Insert].QueryString);

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
", Queries[QueryType.Update].QueryString);
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
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var Result = new SQLServerGenerator<ConcreteClass3>(Mappings);
            var Queries = Result.GenerateDefaultQueries();
            Assert.Equal(3, Queries.Count);
            Assert.Equal("DELETE FROM [dbo].[IInterface1_] WHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n", Queries[QueryType.Delete].QueryString);

            Assert.Equal(@"DECLARE @IInterface1_ID_Temp AS INT;
INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;
SET @IInterface1_ID_Temp=SCOPE_IDENTITY();
SELECT @IInterface1_ID_Temp AS [ID];

INSERT INTO [dbo].[ConcreteClass3_]([dbo].[ConcreteClass3_].[MyUniqueProperty_],[dbo].[ConcreteClass3_].[IInterface1_ID_]) VALUES (@MyUniqueProperty,@IInterface1_ID_Temp);
", Queries[QueryType.Insert].QueryString);

            Assert.Equal(@"UPDATE [dbo].[ConcreteClass3_]
SET [dbo].[ConcreteClass3_].[MyUniqueProperty_]=@MyUniqueProperty
FROM [dbo].[ConcreteClass3_]
INNER JOIN [dbo].[IInterface1_] ON [dbo].[ConcreteClass3_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]
WHERE [dbo].[IInterface1_].[ID_]=@ID;
", Queries[QueryType.Update].QueryString);
        }
    }
}
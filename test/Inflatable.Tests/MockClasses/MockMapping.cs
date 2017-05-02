using Inflatable.Tests.TestDatabases.SimpleTest;

namespace Inflatable.Tests.MockClasses
{
    public class MockMapping : Inflatable.BaseClasses.MappingBaseClass<AllReferencesAndID, MockDatabaseMappingForMockMapping>
    {
        public MockMapping()
        {
            ID(x => x.ID);
        }
    }
}
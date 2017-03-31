namespace Inflatable.Tests.MockClasses
{
    public class MockDatabaseMapping : Interfaces.IDatabase
    {
        public bool Audit => false;

        public string Name => "MockDatabase";

        public int Order => 1;

        public bool Readable => true;

        public bool Update => true;

        public bool Writable => true;

        public bool Optimize => true;
    }
}
namespace Inflatable.Tests.MockClasses
{
    public class SecondMockDatabaseMapping : Interfaces.IDatabase
    {
        public bool Audit => false;

        public string Name => "SecondMockDatabase";

        public int Order => 2;

        public bool Readable => true;

        public bool Update => true;

        public bool Writable => true;

        public bool Optimize => true;
    }
}
namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public interface IMapPropertiesInterfaceWithMap
    {
        int ID { get; set; }

        MapPropertiesWithMapOnInterface MappedClass { get; set; }
    }

    public class MapPropertiesWithMapOnInterface : IMapPropertiesInterfaceWithMap
    {
        public bool BoolValue { get; set; }
        public int ID { get; set; }

        public virtual MapPropertiesWithMapOnInterface MappedClass { get; set; }
    }
}
namespace TestApp.Models
{
    public abstract class ComplexClassBase : ICompexClass
    {
        protected ComplexClassBase()
        {
        }

        public long ID { get; set; }
        public int Value2 { get; set; }

        public long Value3 { get; set; }
    }
}
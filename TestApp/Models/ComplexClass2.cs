using System;
using System.ComponentModel.DataAnnotations;

namespace TestApp.Models
{
    public class ComplexClass2 : IComplexClass2
    {
        public DateTime DateValue { get; set; }

        public long ID { get; set; }

        [MaxLength]
        public string StringVal { get; set; }
    }
}
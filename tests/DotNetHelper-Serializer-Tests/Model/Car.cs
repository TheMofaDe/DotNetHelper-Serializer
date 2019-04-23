using DotNetHelper_Serializer.Attribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetHelper_Serializer_Tests.Model
{

    public class Car
    {
        [SqlColumnAttritube(SetPrimaryKey = true, SetNullable = false, SetStartIncrementAt = 1, SetAutoIncrementBy = 1, MappingIds = new[] { "CarId" })]
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }

        public int Year { get; set; }
    }


    public class CarXRef
    {
        [SqlColumnAttritube(SetPrimaryKey = true, SetNullable = false, SetStartIncrementAt = 1, SetAutoIncrementBy = 1, MappingIds = new[] { "CarId" })]
        public int CarId { get; set; }
        [SqlColumnAttritube(SetNullable = false, MappingIds = new[] { "EmployeeId" })]
        public int EmployeeId { get; set; }

    }
}

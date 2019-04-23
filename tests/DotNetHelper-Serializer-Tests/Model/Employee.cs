using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DotNetHelper_Contracts.Attribute;
using DotNetHelper_Contracts.Enum;
using DotNetHelper_Serializer.Attribute;
using DotNetHelper_Serializer.Extension;
using DotNetHelper_Serializer.Interface;

namespace DotNetHelperUnitTest.Model
{

    public class Employee
    {
        [SqlColumnAttritube(SetAutoIncrementBy = 1,SetPrimaryKey = true,SetNullable = false,MappingIds = new[] { "EmployeeId" })]
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [SqlColumnAttritube(SerializableType = SerializableType.JSON, SetNullable = true)]
        public List<string> JsonTest { get; set; }

    }




}

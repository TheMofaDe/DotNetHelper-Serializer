using DotNetHelperUnitTest.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetHelper_Serializer_Tests.MockData
{
    public static class MockData
    {
        public static Employee TestEmployee { get; } = new Employee() { FirstName = "Joseph" , LastName = "McNeal Jr"};

    }

}

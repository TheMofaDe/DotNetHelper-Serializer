using DotNetHelper_Contracts.Enum.DataSource;
using DotNetHelper_Serializer.DataSource;
using DotNetHelper_Serializer.Interface;
using DotNetHelper_Serializer_Tests.Model;
using DotNetHelperUnitTest.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DotNetHelper_Serializer_Tests
{
  public abstract class TestBase : IDisposable
{
    protected static readonly bool IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

    public static string ConnectionString =>
        IsAppVeyor
            ? @"Server=(local)\SQL2012SP1;Database=master;User ID=sa;Password=Password12!"
            : "Data Source=.;Initial Catalog=tempdb;Integrated Security=True";

   public DataSourceDb DataSourceDb { get; }


    public TestBase(DataSourceDb dataSourceDB)
    {
            DataSourceDb = dataSourceDB;
    }


        protected void CreatePreRequisiteTables()
        {
            DataSourceDb.CreateTableFromClass<Employee>(null, true);
            DataSourceDb.CreateTableFromClass<Car>(null, true);
        }




        [Author("Joseph McNeal Jr", "josephmcnealjr@gmail.com")]
        [Test]
        public virtual void Test_InsertAndPullSingleRecord()
    {

        var recordCount = DataSourceDb.ExecuteDynamicQuery(ActionType.Insert, MockData.MockData.TestEmployee);
        Assert.IsTrue(recordCount == 1, "Dynamically insert single record is broken");


        var employees = DataSourceDb.Get<Employee>(null);
        Assert.IsTrue(employees.Count() == 1, "Dynamically get all records is broken");

    }

        [Author("Joseph McNeal Jr", "josephmcnealjr@gmail.com")]
        [Test]
        public virtual void Test_InsertAndReturnIdentity()
    {
        var record = DataSourceDb.ExecuteDynamicQueryReturnIdentity(ActionType.Insert, MockData.MockData.TestEmployee);
        Assert.IsTrue(record.EmployeeId != 0, "Returning Identity id failed");

    }




        public void Dispose()
        {

        }


    }


}

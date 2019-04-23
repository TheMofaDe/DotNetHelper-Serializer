using DotNetHelper_IO;
using DotNetHelper_Serializer.DataSource;
using DotNetHelper_Serializer_Tests;
using DotNetHelper_Serializer_Tests.Model;
using DotNetHelperUnitTest.Model;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests
{
    [TestFixture]
    [NonParallelizable] 
    public class DatabaseTestFixture : TestBase
    {

     
        public DatabaseTestFixture() : base (new DataSourceDb())
        {
            if (IsAppVeyor)
            {
                DataSourceDb.ConnectionString = ConnectionString;
            }
            else
            {
                DataSourceDb.Server = "localhost";
                DataSourceDb.IntegratedSecurity = true;
                DataSourceDb.Database  = "master" ;
            }
        }


        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            CreatePreRequisiteTables();
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            Assert.IsTrue(DataSourceDb.DropTable("Employee"));
            Assert.IsTrue(DataSourceDb.DropTable("Car"));
        }



        [SetUp]
        public void Init()
        {

        }

        [TearDown]
        public void Cleanup()
        {
            DataSourceDb.ExecuteManualQuery("DELETE FROM Employee").Dispose();
            DataSourceDb.ExecuteManualQuery("DELETE FROM Car").Dispose();
        }






        [Author("Joseph McNeal Jr", "josephmcnealjr@gmail.com")]
        [Test]
        public virtual void Test_GetTableNameNoDatabaseNoSchemaWithBrackets()
        {
            var tableName = "[DataBase].[Schema].Employee";
            var value = DataSourceDb.GetDefaultTableName(tableName, false, false, true);
            Assert.IsTrue(value == "[Employee]", value, tableName);

            var tableName2 = "[Schema].[Employee]";
            var value2 = DataSourceDb.GetDefaultTableName(tableName2, false, false, true);
            Assert.IsTrue(value == "[Employee]", value2, tableName2);


            var tableName3 = "Employee";
            var value3 = DataSourceDb.GetDefaultTableName(tableName3, false, false, true);
            Assert.IsTrue(value == "[Employee]", value3, tableName3);
        }

        [Author("Joseph McNeal Jr", "josephmcnealjr@gmail.com")]
        [Test]
        public virtual void Test_GetTableNameNoDatabaseNoSchemaNoBrackets()
        {
            var tableName = "[DataBase].[Schema].Employee";
            var value = DataSourceDb.GetDefaultTableName(tableName, false, false, false);
            Assert.IsTrue(value == "Employee", value, tableName);

            var tableName2 = "[Schema].[Employee]";
            var value2 = DataSourceDb.GetDefaultTableName(tableName2, false, false, false);
            Assert.IsTrue(value == "Employee", value2, tableName2);


            var tableName3 = "Employee";
            var value3 = DataSourceDb.GetDefaultTableName(tableName3, false, false, false);
            Assert.IsTrue(value == "Employee", value3, tableName3);
        }


      



    }
}
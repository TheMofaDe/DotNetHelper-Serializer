using System.Data;
using System.Linq;
using FastMember;

namespace DotNetHelper_Serializer.Extension
{
   public static class ExtDataTable
    {







        /// <summary>
        /// SetOrdinal of DataTable columns based on the index of the columnNames array. Removes invalid column names first.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order </remarks>
        public static void SetColumnsOrder(this DataTable table, params string[] columnNames)
        {
            var listColNames = columnNames.ToList();

            //Remove invalid column names.
            foreach (var colName in columnNames)
            {
                if (!table.Columns.Contains(colName))
                {
                    listColNames.Remove(colName);
                }
            }

            foreach (var colName in listColNames)
            {
                table.Columns[colName].SetOrdinal(listColNames.IndexOf(colName));
            }

        }

        /// <summary>
        /// SetOrdinal of DataTable columns based on the index of the columnNames array. Removes invalid column names first.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order</remarks>
        public static bool SetColumnPosition(this DataTable table, string columnName, int position)
        {
            if (!table.Columns.Contains(columnName))
            {
                return false;
            }
            table.Columns[columnName].SetOrdinal(position);
            return true;
        }
       


      






        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="type">Null Value Will Be Converted To Null Or DBNull</param>
        /// <returns></returns>
        public static T MapToType<T>(this DataRow row) where T : class
        {

            var accessor = TypeAccessor.Create(typeof(T), true);
            var listOfProps = accessor.GetMembers().ToList();

            var obj = TypeExtension.New<T>.Instance();

            listOfProps.ForEach(delegate (Member member)
            {
                if (listOfProps.Contains(row[$"{member.Name}"]))
                {
                    accessor[obj, member.Name] = row[$"{member.Name}"];
                }

            });

            return obj;
        }

    }
}

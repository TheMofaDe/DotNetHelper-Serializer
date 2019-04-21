using System;
using DotNetHelper_Contracts.Enum;


#pragma warning disable IDE1006 // Naming Styless
// ReSharper disable InconsistentNaming

namespace DotNetHelper_Serializer.Attribute
{


    public enum SQLJoinType
    {
        /// <summary>
        /// SQL-style LEFT OUTER JOIN semantics: All records of the left table are returned. If the right table holds no matching records, the right 
        /// side's columns contain NULL. 
        /// </summary>
        LEFT = 1,
        /// <summary>
        /// SQL-style INNER JOIN semantics: Only records that produce a match are returned.
        /// </summary>
        INNER = 2,

    }


    /// <inheritdoc />
    /// <summary>
    /// This specifies that the following property is also an SQL table
    /// </summary>
    /// <seealso cref="T:System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class SqlTableAttritube : System.Attribute
    {

        /// <summary>
        /// The Sql Table name that this class data belongs to.
        /// </summary>
        /// <value>The map to.</value>
        public string TableName { get; set; } = null;

        public Type XReferenceTable { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether [x reference on delete cascade].
        /// </summary>
        /// <value><c>null</c> if [x reference on delete cascade] contains no value, <c>true</c> if [x reference on delete cascade]; otherwise, <c>false</c>.</value>
        public SQLJoinType JoinType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [setx reference on delete cascade].
        /// </summary>
        /// <value><c>true</c> if [setx reference on delete cascade]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public SQLJoinType SetJoinType
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => JoinType = value;
        }



    }


    /// <summary>
    /// Enum SqlColumnAttritubeMembers
    /// </summary>
    public enum SqlTableAttritubeMembers
    {

        JoinType
        , TableName
        , XReferenceTable
    }


    /// <inheritdoc />
    /// <summary>
    /// Class SqlColumnAttritube.
    /// </summary>
    /// <seealso cref="T:System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class SqlColumnAttritube : System.Attribute
    {
        /// <summary>
        /// Defaults To MAX used for creating table
        /// </summary>
        /// <value>The maximum size of the column.</value>
        public int? MaxColumnSize { get; set; }
        /// <summary>
        /// Gets or sets the size of the set maximum column.
        /// </summary>
        /// <value>The size of the set maximum column.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public int SetMaxColumnSize
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => MaxColumnSize = value;
        }

        /// <summary>
        /// Defaults To MAX used for creating table
        /// </summary>
        /// <value>The maximum size of the column.</value>
        public SerializableType SerializableType { get; set; } = SerializableType.NONE;
        ///// <summary>
        ///// Gets or sets the size of the set maximum column.
        ///// </summary>
        ///// <value>The size of the set maximum column.</value>
        ///// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        //public SerializableType SetSerializableType
        //{
        //    get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
        //    set => SerializableType = value;
        //}

        /// <summary>
        /// Gets or sets the automatic increment by. If this value is set then the property will be treated as an IDENTITY column
        /// </summary>
        /// <value>The automatic increment by.</value>
        public int? AutoIncrementBy { get; set; } = null;
        /// <summary>
        /// Gets or sets the set automatic increment by.
        /// </summary>
        /// <value>The set automatic increment by.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public int SetAutoIncrementBy
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => AutoIncrementBy = value;
        }

        /// <summary>
        /// Gets or sets the start increment at.
        /// </summary>
        /// <value>The start increment at.</value>
        public int? StartIncrementAt { get; set; }
        /// <summary>
        /// Gets or sets the set start increment at.
        /// </summary>
        /// <value>The set start increment at.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public int SetStartIncrementAt
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => StartIncrementAt = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [UTC date time].
        /// </summary>
        /// <value><c>null</c> if [UTC date time] contains no value, <c>true</c> if [UTC date time]; otherwise, <c>false</c>.</value>
        public bool? UtcDateTime { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set UTC date time].
        /// </summary>
        /// <value><c>true</c> if [set UTC date time]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetUtcDateTime
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => UtcDateTime = value;
        }
        /// <summary>
        /// Gets or sets a value indicating whether [primary key].
        /// </summary>
        /// <value><c>null</c> if [primary key] contains no value, <c>true</c> if [primary key]; otherwise, <c>false</c>.</value>
        public bool? PrimaryKey { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set primary key].
        /// </summary>
        /// <value><c>true</c> if [set primary key]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetPrimaryKey
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => PrimaryKey = value;
        }


        /// <summary>
        /// Gets or sets the type of the x reference table.
        /// </summary>
        /// <value>The type of the x reference table.</value>
        public Type xRefTableType { get; set; } = null;
        /// <summary>
        /// Gets or sets the type of the setx reference table.
        /// </summary>
        /// <value>The type of the setx reference table.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public Type SetxRefTableType
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => xRefTableType = value;
        }




        ///// <summary>
        ///// Gets or sets the mappings for keys to join with.
        ///// </summary>
        ///// <value>The x reference table schema.</value>
        //internal Dictionary<string, string> KeysToJoinWith { get; set; } = null;

        ///// <summary>
        ///// Gets or sets the mappings for keys to join with.
        ///// </summary>
        ///// <value>The type of the setx reference table.</value>
        ///// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        //public Dictionary<string, string> SetKeysToJoinWith
        //{
        //    get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
        //    set => KeysToJoinWith = value;
        //}


        /// <summary>
        /// Gets or sets the x reference table schema.
        /// </summary>
        /// <value>The x reference table schema.</value>
        public string xRefTableSchema { get; set; } = null;


        /// <summary>
        /// Gets or sets the name of the x reference table.
        /// </summary>
        /// <value>The name of the x reference table.</value>
        public string xRefTableName { get; set; } = null;


        /// <summary>
        /// Gets or sets the x reference join on column.
        /// </summary>
        /// <value>The x reference join on column.</value>
        public string xRefJoinOnColumn { get; set; } = null;


        /// <summary>
        /// Gets or sets a value indicating whether [x reference on update cascade].
        /// </summary>
        /// <value><c>null</c> if [x reference on update cascade] contains no value, <c>true</c> if [x reference on update cascade]; otherwise, <c>false</c>.</value>
        public bool? xRefOnUpdateCascade { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether [setx reference on update cascade].
        /// </summary>
        /// <value><c>true</c> if [setx reference on update cascade]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetxRefOnUpdateCascade
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            internal set => xRefOnUpdateCascade = value;
        }


        /// <summary>
        /// Gets or sets a value indicating whether [x reference on delete cascade].
        /// </summary>
        /// <value><c>null</c> if [x reference on delete cascade] contains no value, <c>true</c> if [x reference on delete cascade]; otherwise, <c>false</c>.</value>
        public bool? xRefOnDeleteCascade { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [setx reference on delete cascade].
        /// </summary>
        /// <value><c>true</c> if [setx reference on delete cascade]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetxRefOnDeleteCascade
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            internal set => xRefOnDeleteCascade = value;
        }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SqlColumnAttritube"/> is nullable.
        /// </summary>
        /// <value><c>null</c> if [nullable] contains no value, <c>true</c> if [nullable]; otherwise, <c>false</c>.</value>
        public bool? Nullable { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set nullable].
        /// </summary>
        /// <value><c>true</c> if [set nullable]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetNullable
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => Nullable = value;
        }
        /// <summary>
        /// Gets or sets a value indicating whether [API identifier].
        /// </summary>
        /// <value><c>null</c> if [API identifier] contains no value, <c>true</c> if [API identifier]; otherwise, <c>false</c>.</value>
        public bool? ApiId { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set API identifier].
        /// </summary>
        /// <value><c>true</c> if [set API identifier]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetApiId
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => ApiId = value;
        }
        /// <summary>
        /// When A Record Is Be Inserted Or Updated This Column Value Will Be DateTime.Now
        /// </summary>
        /// <value><c>null</c> if [synchronize time] contains no value, <c>true</c> if [synchronize time]; otherwise, <c>false</c>.</value>
        public bool? SyncTime { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set synchronize time].
        /// </summary>
        /// <value><c>true</c> if [set synchronize time]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetSyncTime
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => SyncTime = value;
        }

        /// <summary>
        /// If true property will be use when the class is being used by a DATASOURCE Object
        /// </summary>
        /// <value><c>null</c> if [ignore] contains no value, <c>true</c> if [ignore]; otherwise, <c>false</c>.</value>
        public bool? Ignore { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set ignore].
        /// </summary>
        /// <value><c>true</c> if [set ignore]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial</exception>
        public bool SetIgnore
        {
            get => throw new Exception("Nooo...  Your Using SqlColumnAttritube wrong do not try to get from the Set Property use the orignial ");
            set => Ignore = value;
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>The default value.</value>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the default value THIS IS ONLY WHEN THIS LIBRARY IS CREATING A TABLE SCRIPT 
        /// </summary>
        /// <value>The default value.</value>
        public string TSQLDefaultValue { get; set; }

        /// <summary>
        /// If true property will be use when the class is being used by a DATASOURCE Object
        /// </summary>
        /// <value>The map to.</value>
        public string MapTo { get; set; } = null;



        /// <summary>
        /// Gets or sets the mappings for keys to join with.
        /// </summary>
        /// <value>an array of ids, that will join a column to another table.</value>
        public string[] MappingIds = null;

    }








    /// <summary>
    /// Enum SqlColumnAttritubeMembers
    /// </summary>
    public enum SqlColumnAttritubeMembers
    {
        /// <summary>
        /// The set maximum column size
        /// </summary>
        SetMaxColumnSize

        , SetStartIncrementAt
        /// <summary>
        /// The set automatic increment by
        /// </summary>
        , SetAutoIncrementBy
        /// <summary>
        /// The set UTC date time
        /// </summary>
        , SetUtcDateTime
        /// <summary>
        /// The set primary key
        /// </summary>
        , SetPrimaryKey
        /// <summary>
        /// The set nullable
        /// </summary>
        , SetNullable
        /// <summary>
        /// The set API identifier
        /// </summary>
        , SetApiId
        /// <summary>
        /// The set synchronize time
        /// </summary>
        , SetSyncTime
        /// <summary>
        /// The set ignore
        /// </summary>
        , SetIgnore
        /// <summary>
        /// The map to
        /// </summary>
        , MapTo

        , MappingIds
        /// <summary>
        /// The set default value
        /// </summary>
        , DefaultValue
        /// <summary>
        /// The set default value
        /// </summary>
        , TSQLDefaultValue
        /// <summary>
        /// The setx reference table type
        /// </summary>
        , SetxRefTableType
        /// <summary>
        /// The x reference table schema
        /// </summary>
        , xRefTableSchema
        /// <summary>
        /// The x reference table name
        /// </summary>
        , xRefTableName
        /// <summary>
        /// The x reference join on column
        /// </summary>
        , xRefJoinOnColumn
        /// <summary>
        /// The setx reference on update cascade
        /// </summary>
        , SetxRefOnUpdateCascade
        /// <summary>
        /// The setx reference on delete cascade
        /// </summary>
        , SetxRefOnDeleteCascade
        , SerializableType
    }



    // public class XRefTable<T>(Type type)
    // {
    //     public T MapsTo { get; set; } 
    //     public Dictionary<string,string> ColumnMappings { get; set; } = new Dictionary<string, string>(){ };
    //     public bool AlwaysInsert { get; set; } = false;
    // }



    /// <inheritdoc />
    /// <summary>
    /// Class DataValidationAttritube.
    /// </summary>
    /// <seealso cref="T:System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class DataValidationAttritube : System.Attribute
    {
        /// <summary>
        /// Max Size Property Can Be
        /// </summary>
        /// <value>The maximum size of the length.</value>

        public int? MaxLengthSize { get; set; } = null;
        /// <summary>
        /// Gets or sets the size of the set maximum length.
        /// </summary>
        /// <value>The size of the set maximum length.</value>
        /// <exception cref="Exception">Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL</exception>
        public int SetMaxLengthSize
        {
            get
            {
                if (MaxLengthSize != null) { return MaxLengthSize.Value; } else { throw new Exception("Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL"); }
            }
            //throw new Exception("Nooo...  Your Using DataValidationAttritube wrong do not try to get from the Set Property use the orignial ");
            set => MaxLengthSize = value;
        }

        /// <summary>
        /// Wether Or Null
        /// </summary>
        /// <value><c>null</c> if [require value] contains no value, <c>true</c> if [require value]; otherwise, <c>false</c>.</value>

        public bool? RequireValue { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set require value].
        /// </summary>
        /// <value><c>true</c> if [set require value]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL</exception>
        public bool SetRequireValue
        {
            get
            {
                if (RequireValue != null) { return RequireValue.Value; } else { throw new Exception("Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL"); }
            }
            set => RequireValue = value;
        }


        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public DataType? DataType { get; set; } = DotNetHelper_Contracts.Enum.DataType.Unknown;
        /// <summary>
        /// Gets or sets the type of the set data.
        /// </summary>
        /// <value>The type of the set data.</value>
        /// <exception cref="Exception">Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL</exception>
        public DataType SetDataType
        {
            get
            {
                if (DataType != null) { return DataType.Value; } else { throw new Exception("Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL"); }
            }
            set => DataType = value;
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance can contain numbers.
        /// </summary>
        /// <value><c>null</c> if [can contain numbers] contains no value, <c>true</c> if [can contain numbers]; otherwise, <c>false</c>.</value>
        public bool? CanContainNumbers { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set can contain numbers].
        /// </summary>
        /// <value><c>true</c> if [set can contain numbers]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL</exception>
        public bool SetCanContainNumbers
        {
            get
            {
                if (CanContainNumbers != null) { return CanContainNumbers.Value; } else { throw new Exception("Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL"); }
            }
            set => CanContainNumbers = value;
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance can contain letter.
        /// </summary>
        /// <value><c>null</c> if [can contain letter] contains no value, <c>true</c> if [can contain letter]; otherwise, <c>false</c>.</value>
        public bool? CanContainLetter { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set can contain letter].
        /// </summary>
        /// <value><c>true</c> if [set can contain letter]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL</exception>
        public bool SetCanContainLetter
        {
            get
            {
                if (CanContainLetter != null) { return CanContainLetter.Value; } else { throw new Exception("Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL"); }
            }
            set => CanContainLetter = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DataValidationAttritube"/> is ignore.
        /// </summary>
        /// <value><c>null</c> if [ignore] contains no value, <c>true</c> if [ignore]; otherwise, <c>false</c>.</value>
        public bool? Ignore { get; set; } = null;
        /// <summary>
        /// Gets or sets a value indicating whether [set ignore].
        /// </summary>
        /// <value><c>true</c> if [set ignore]; otherwise, <c>false</c>.</value>
        /// <exception cref="Exception">Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL</exception>
        public bool SetIgnore
        {
            get
            {
                if (Ignore != null) { return Ignore.Value; } else { throw new Exception("Nooo...  Your Using DataValidationAttritube wrong  You Must Set This Attribute Value Before Trying To Get It There IS NO SUCH THING AS NULL"); }
            }
            set => Ignore = value;
        }

    }



    /// <summary>
    /// Enum DataValidationAttritubeMembers
    /// </summary>
    public enum DataValidationAttritubeMembers
    {
        /// <summary>
        /// The set maximum length size
        /// </summary>
        SetMaxLengthSize
        /// <summary>
        /// The set require value
        /// </summary>
        , SetRequireValue
        /// <summary>
        /// The set data type
        /// </summary>
        , SetDataType
        /// <summary>
        /// The set can contain numbers
        /// </summary>
        , SetCanContainNumbers
        /// <summary>
        /// The set can contain letter
        /// </summary>
        , SetCanContainLetter
        /// <summary>
        /// The set ignore
        /// </summary>
        , SetIgnore
    }



}
#pragma warning restore IDE1006 // Naming Styles
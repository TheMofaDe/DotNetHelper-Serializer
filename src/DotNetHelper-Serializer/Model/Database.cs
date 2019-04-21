using System;
using System.Collections.Generic;

namespace DotNetHelper_Serializer.Model
{
    // ReSharper disable InconsistentNaming
    public class QueryBucket
    {
        public DateTime ConnectionStartTime { get; set; } = DateTime.Now;
        public DateTime? ConnectionDisposeTime { get; set; }
        public string Query { get; set; }
        public string ReadableQuery { get; set; }
        public string Server { get; set; }
        public string DatabaseType { get; set; }
        public bool? ExecutedSuccesfully { get; set; }
    }

    public class QueryBucketManager : IDisposable
    {
        public int MaxBucketSize { get; set; } = 50;
        public bool IncludeReadableQuery { get; set; } = false;
        internal List<QueryBucket> Bucket { get; private set; } = new List<QueryBucket>() { };
        public delegate void FullQueryBucketEventHandler(object sender, FullQueryBucketEventArgs e);
        public event FullQueryBucketEventHandler FullBucketReached;


        public delegate void OnBeforeAddToBucketEventHandler(object sender, QueryBucketEventArgs e);
        public event OnBeforeAddToBucketEventHandler BeforeAddToBucket;

        internal object ThreadSafe = new object();
        public void AddBucket(QueryBucket bucket)
        {
            lock (ThreadSafe)
            {
                if (Bucket.Count >= MaxBucketSize)
                {
                    Bucket.Add(bucket);
                    OnFullBucket(new FullQueryBucketEventArgs(Bucket) { });
                    Bucket.Clear();
                }
                else
                {
                    OnBeforeAdd(new QueryBucketEventArgs(bucket) { });
                    Bucket.Add(bucket);
                }
            }
        }

        protected virtual void OnFullBucket(FullQueryBucketEventArgs e)
        {
            FullBucketReached?.Invoke(this, e);
        }


        protected virtual void OnBeforeAdd(QueryBucketEventArgs e)
        {
            BeforeAddToBucket?.Invoke(this, e);
        }



        public void Dispose()
        {

            Bucket?.Clear();
            Bucket = null;
            if (FullBucketReached != null)
                foreach (var d in FullBucketReached?.GetInvocationList())
                {
                    FullBucketReached -= (FullQueryBucketEventHandler)d;
                }
            if (BeforeAddToBucket != null)
                foreach (var d in BeforeAddToBucket?.GetInvocationList())
                {
                    BeforeAddToBucket -= (OnBeforeAddToBucketEventHandler)d;
                }
        }
    }

    public class FullQueryBucketEventArgs : EventArgs
    {
        public List<QueryBucket> BucketOfQueries { get; }

        public FullQueryBucketEventArgs(List<QueryBucket> buckets)
        {
            BucketOfQueries = buckets;
        }
    }


    public class QueryBucketEventArgs : EventArgs
    {
        public QueryBucket QueryBucket { get; }

        public QueryBucketEventArgs(QueryBucket bucket)
        {
            QueryBucket = bucket;
        }
    }





    public class TableDefinition
    {
        public string TABLE_QUALIFIER { get; internal set; }
        public string TABLE_OWNER { get; internal set; }
        public string TABLE_NAME { get; internal set; }
        public string COLUMN_NAME { get; internal set; }
        public short DATA_TYPE { get; internal set; }
        public string TYPE_NAME { get; internal set; }
        public int PRECISION { get; internal set; }
        public int LENGTH { get; internal set; }
        public short SCALE { get; internal set; }
        public short RADIX { get; internal set; }
        public short NULLABLE { get; internal set; }
        public string REMARKS { get; internal set; }
        public string COLUMN_DEF { get; internal set; }
        public short SQL_DATA_TYPE { get; internal set; }
        public short SQL_DATETIME_SUB { get; internal set; }
        public int CHAR_OCTET_LENGTH { get; internal set; }
        public int ORDINAL_POSITION { get; internal set; }
        public string IS_NULLABLE { get; internal set; }
        public byte SS_DATA_TYPE { get; internal set; }
    }

    public class DataSourceProperties
    {
        public List<DbTable> Tables { get; set; } = new List<DbTable>() { };
        public List<DbSchema> Schemas { get; set; } = new List<DbSchema>() { };
        public List<Db> DataBases { get; set; } = new List<Db>() { };
        public DateTime? LastSyncTime { get; set; }
    }

    public class DbTable
    {
        public string TableName { get; set; }
        public int? SchemaId { get; set; }
        public int? ParentObjectId { get; set; }
        public string TableType { get; set; }
        public string TypeDesc { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Owner { get; set; }
        public decimal? UsedBytes { get; set; }
        public decimal? UsedGb { get; set; }
        public string Objtype { get; set; }
        public long? Rows { get; set; }
        public string TableCatalog { get; set; }
        public string TableSchema { get; set; }
        public string TableGuid { get; set; }
        public string Description { get; set; }
        public string TablePropid { get; set; }
        /// <summary>
        /// SQLITE ONLY
        /// </summary>
        public long Rootpage { get; set; }
        /// <summary>
        /// SQLITE ONLY
        /// </summary>
        public string Sql { get; set; }
    }

    public class DbSchema
    {
        public string Name { get; set; }
        public int SchemaId { get; set; }
        public int PrincipalId { get; set; }
    }
    public class Db
    {
        public string Name { get; set; }
        public short? Dbid { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Filename { get; set; }
        public short? Version { get; set; }
    }

}

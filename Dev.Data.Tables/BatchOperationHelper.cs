﻿using Azure;
using System.Collections.Concurrent;
using Azure.Data.Tables;
namespace Dev.Data.Tables
{
    /// <summary>
    /// Used to instantiate multiple TableBatchOperations when the
    /// TableOperation maximum is reached on a single TableBatchOperation
    /// </summary>
    public class BatchOperationHelper
    {
        public const int MaxEntitiesPerBatch = 100;

        private readonly Dictionary<string, List<TableTransactionAction>> _batches = new();

        private readonly TableClient _table;

        public BatchOperationHelper(TableClient table)
        {
            _table = table;
        }

        public virtual void AddEntities<T>(IEnumerable<T> entities) where T : class, ITableEntity, new()
        {
            foreach (T entity in entities)
            {
                AddEntity<T>(entity);
            }
        }
        public virtual void AddEntity<T>(T entity) where T : class, ITableEntity, new()
        {
            GetCurrent(entity.PartitionKey).Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
        }
        public virtual void DeleteEntity(string partitionKey, string rowKey, ETag ifMatch = default)
        {
            GetCurrent(partitionKey).Add(new TableTransactionAction(TableTransactionActionType.Delete, new TableEntity(partitionKey, rowKey), ifMatch));
        }

        public virtual async Task<IEnumerable<Response>> SubmitBatchAsync(CancellationToken cancellationToken = default)
        {
            ConcurrentBag<Response> bag = new ConcurrentBag<Response>();
            List<Task> batches = new List<Task>();
            foreach (KeyValuePair<string, List<TableTransactionAction>> kv in _batches)
            {
                int total = kv.Value.Count;
                int skip = 0;
                int take = total > MaxEntitiesPerBatch ? MaxEntitiesPerBatch : total;

                while (take > 0)
                {
                    batches.Add(_table.SubmitTransactionAsync(kv.Value.Skip(skip).Take(take), cancellationToken)
                        .ContinueWith((result) =>
                        {
                            foreach (var r in result.Result.Value)
                            {
                                bag.Add(r);
                            }
                        }, cancellationToken));

                    skip += take;
                    take = (total - skip) > MaxEntitiesPerBatch ? MaxEntitiesPerBatch : (total - skip);
                }
            }
            await Task.WhenAll(batches).ConfigureAwait(false);
            Clear();
            return bag;
        }

        public virtual void UpdateEntity<T>(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge) where T : class, ITableEntity, new()
        {
            GetCurrent(entity.PartitionKey).Add(new TableTransactionAction(mode == TableUpdateMode.Merge ? TableTransactionActionType.UpdateMerge : TableTransactionActionType.UpdateReplace, entity, ifMatch));
        }

        public virtual void UpsertEntity<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge) where T : class, ITableEntity, new()
        {
            GetCurrent(entity.PartitionKey).Add(new TableTransactionAction(mode == TableUpdateMode.Merge ? TableTransactionActionType.UpsertMerge : TableTransactionActionType.UpsertReplace, entity));
        }

        public void Clear()
        {
            _batches.Clear();
        }

        private List<TableTransactionAction> GetCurrent(string partitionKey)
        {
            if (!_batches.ContainsKey(partitionKey))
            {
                _batches.Add(partitionKey, new List<TableTransactionAction>());
            }

            return _batches[partitionKey];
        }
    }
}

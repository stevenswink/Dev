﻿using Azure;
using Azure.Data.Tables;
using System.Runtime.Serialization;


namespace Dev.Identity.AzureTable.Model
{
    public class IdentityUser : IdentityUser<string>, IGenerateKeys
    {
        public IdentityUser() : base() { }

        public IdentityUser(string userName)
            : this()
        {
            UserName = userName;
        }

        /// <summary>
        /// Generates Row, Partition and Id keys.
        /// All are the same in this case
        /// </summary>
        public void GenerateKeys(IKeyHelper keyHelper)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                Id = keyHelper.GenerateUserId();
            }
            RowKey = PeekRowKey(keyHelper);
            PartitionKey = RowKey;
            KeyVersion = keyHelper.KeyVersion;
        }

        /// <summary>
        /// Generates the RowKey without setting it on the object.
        /// In this case, just returns a key based on username
        /// </summary>
        /// <returns></returns>
        public string PeekRowKey(IKeyHelper keyHelper)
        {
            return keyHelper.GenerateRowKeyUserId(Id);
        }

        public double KeyVersion { get; set; }



        public override string UserName
        {
            get => base.UserName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    base.UserName = value.Trim();
                }
            }
        }
    }

    public class IdentityUser<TKey> : Microsoft.AspNetCore.Identity.IdentityUser<TKey>, ITableEntity
        where TKey : IEquatable<TKey>
    {
        public IdentityUser()
        {
        }


        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// LockoutEnd is stored as LockoutEndDateUtc for backwards compat.
        /// </summary>
        [IgnoreDataMember]
        public override DateTimeOffset? LockoutEnd
        {
            get
            {
                if (LockoutEndDateUtc.HasValue)
                {
                    return new DateTimeOffset?(new DateTimeOffset(LockoutEndDateUtc.Value));
                }

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    LockoutEndDateUtc = value.Value.UtcDateTime;
                }
                else
                {
                    LockoutEndDateUtc = null;
                }
            }
        }


        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } = ETag.All;

    }


}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Cassandra;
using FluentCassandra.Types;

namespace FluentCassandra.Operations
{
	public class MultiGetColumnFamilySlice<CompareWith> : ColumnFamilyOperation<IEnumerable<IFluentColumnFamily<CompareWith>>>
		where CompareWith : CassandraType
	{
		/*
		 * map<string,list<ColumnOrSuperColumn>> multiget_slice(keyspace, keys, column_parent, predicate, consistency_level)
		 */

		public List<string> Keys { get; private set; }

		public CassandraSlicePredicate SlicePredicate { get; private set; }

		public override IEnumerable<IFluentColumnFamily<CompareWith>> Execute(BaseCassandraColumnFamily columnFamily)
		{
			return GetFamilies(columnFamily);
		}

		private IEnumerable<IFluentColumnFamily<CompareWith>> GetFamilies(BaseCassandraColumnFamily columnFamily)
		{
			var parent = new ColumnParent {
				Column_family = columnFamily.FamilyName
			};

			var output = columnFamily.GetClient().multiget_slice(
				columnFamily.Keyspace.KeyspaceName,
				Keys,
				parent,
				SlicePredicate.CreateSlicePredicate(),
				ConsistencyLevel
			);

			foreach (var result in output)
			{
				var r = new FluentColumnFamily<CompareWith>(result.Key, columnFamily.FamilyName, result.Value.Select(col => {
					return ObjectHelper.ConvertColumnToFluentColumn<CompareWith>(col.Column);
				}));
				columnFamily.Context.Attach(r);
				r.MutationTracker.Clear();

				yield return r;
			}
		}

		public MultiGetColumnFamilySlice(IEnumerable<string> keys, CassandraSlicePredicate columnSlicePredicate)
		{
			this.Keys = keys.ToList();
			this.SlicePredicate = columnSlicePredicate;
		}
	}
}
﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenSync.EntityRepositories;

namespace GenSync.UnitTests.Synchronization
{
  internal class TestRepository : IEntityRepository<string, string, int>
  {
    private readonly string _idPrefix;

    public readonly Dictionary<string, Tuple<int, string>> EntityVersionAndContentById = new Dictionary<string, Tuple<int, string>>();
    private int _nextId = 1;


    public int Count
    {
      get { return EntityVersionAndContentById.Count; }
    }

    public TestRepository (string idPrefix)
    {
      _idPrefix = idPrefix;
    }

    public IReadOnlyList<EntityIdWithVersion<string, int>> GetVersions (DateTime @from, DateTime to)
    {
      return EntityVersionAndContentById.Select(kv => EntityIdWithVersion.Create(kv.Key, kv.Value.Item1)).ToList();
    }

    // ReSharper disable once CSharpWarnings::CS1998
    public async Task<IReadOnlyList<EntityWithVersion<string, string>>> Get (ICollection<string> ids)
    {
      return ids.Select (id => EntityWithVersion.Create (id, EntityVersionAndContentById[id].Item2)).ToArray();
    }

    public void Cleanup (IReadOnlyDictionary<string, string> entities)
    {
    }

    public bool Delete (string entityId)
    {
      EntityVersionAndContentById.Remove (entityId);
      return true;
    }

    public EntityIdWithVersion<string, int> Update (string entityId, string entityToUpdate, Func<string, string> entityModifier)
    {
      var kv = EntityVersionAndContentById[entityId];
      EntityVersionAndContentById.Remove (entityId);

      var newValue = entityModifier (kv.Item2);
      var newVersion = kv.Item1 + 1;

      var newEntityId = entityId + "u";

      EntityVersionAndContentById[newEntityId] = Tuple.Create (newVersion, newValue);
      return new EntityIdWithVersion<string, int> (newEntityId, newVersion);
    }

    public EntityIdWithVersion<string, int> UpdateWithoutIdChange (string entityId, Func<string, string> entityModifier)
    {
      var kv = EntityVersionAndContentById[entityId];

      var newValue = entityModifier (kv.Item2);
      var newVersion = kv.Item1 + 1;

      EntityVersionAndContentById[entityId] = Tuple.Create (newVersion, newValue);
      return new EntityIdWithVersion<string, int> (entityId, newVersion);
    }


    public EntityIdWithVersion<string, int> Create (Func<string, string> entityInitializer)
    {
      var newValue = entityInitializer (string.Empty);
      var entityId = _idPrefix + _nextId++;
      EntityVersionAndContentById[entityId] = Tuple.Create (0, newValue);
      return new EntityIdWithVersion<string, int> (entityId, 0);
    }

    public Tuple<int, string> this [string id]
    {
      get { return EntityVersionAndContentById[id]; }
    }
  }
}
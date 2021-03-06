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
using GenSync.ProgressReport;
using GenSync.Synchronization;
using GenSync.Synchronization.StateCreationStrategies;
using GenSync.Synchronization.StateFactories;
using NUnit.Framework;
using Rhino.Mocks;

namespace GenSync.UnitTests.Synchronization
{
  internal abstract class SynchronizerFixtureBase
  {
    protected TestSynchronizerSetup _synchronizerSetup;

    protected ISynchronizer _synchronizer;
    protected IEntitySyncStateFactory<string, int, string, string, int, string> _factory;

    [SetUp ()]
    public virtual void Setup ()
    {
      _synchronizerSetup = new TestSynchronizerSetup();
      _factory = new EntitySyncStateFactory<string, int, string, string, int, string> (
          _synchronizerSetup.EntityMapper,
          _synchronizerSetup.AtypeRepository,
          _synchronizerSetup.BtypeRepository,
          _synchronizerSetup.EntityRelationDataFactory,
           MockRepository.GenerateMock<IExceptionLogger> ()
          );
    }

    protected bool SynchronizeInternal (IInitialSyncStateCreationStrategy<string, int, string, string, int, string> strategy)
    {
      _synchronizer = new Synchronizer<string, int, string, string, int, string> (
          _synchronizerSetup,
          strategy,
          NullTotalProgressFactory.Instance,
          EqualityComparer<string>.Default,
          EqualityComparer<string>.Default,
          MockRepository.GenerateMock<IExceptionLogger>()
          );

      return _synchronizer.Synchronize().Result;
    }

    protected void ExecuteMultipleTimes (Action a)
    {
      a();
      a();
      a();
      a();
    }
  }
}
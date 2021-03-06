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
using System.Reflection;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.InitialEntityMatching;
using GenSync.Synchronization;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Tasks
{
  public class TaskSynchronizationContext : ISynchronizerContext<string, DateTime, TaskItemWrapper, Uri, string, IICalendar>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    private readonly IEntityRelationDataAccess<string, DateTime, Uri, string> _storageDataAccess;
    private readonly TaskMapper _entityMapper;
    private readonly OutlookTaskRepository _atypeRepository;
    private readonly CalDavRepository _btypeRepository;
    private readonly IEntityRelationDataFactory<string, DateTime, Uri, string> _entityRelationDataFactory;


    public TaskSynchronizationContext (NameSpace outlookSession, IEntityRelationDataAccess<string, DateTime, Uri, string> storageDataAccess, Options options, TimeSpan connectTimeout, TimeSpan readWriteTimeout, bool disableCertValidation, bool useSsl3, bool useTls12, IEqualityComparer<Uri> btypeIdEqualityComparer)
    {
      if (outlookSession == null)
        throw new ArgumentNullException ("outlookSession");

      SynchronizationMode = options.SynchronizationMode;
      From = DateTime.Now.AddDays (-options.DaysToSynchronizeInThePast);
      To = DateTime.Now.AddDays (options.DaysToSynchronizeInTheFuture);

      _entityRelationDataFactory = new OutlookEventRelationDataFactory();

      _entityMapper = new TaskMapper (outlookSession.Application.TimeZones.CurrentTimeZone.ID);

      var calendarFolder = (Folder) outlookSession.GetFolderFromID (options.OutlookFolderEntryId, options.OutlookFolderStoreId);
      _atypeRepository = new OutlookTaskRepository (calendarFolder, outlookSession);

      _btypeRepository = new CalDavRepository (
          new CalDavDataAccess (
              new Uri (options.CalenderUrl),
              new CalDavWebClient (
                  options.UserName,
                  options.Password,
                  connectTimeout,
                  readWriteTimeout,
                  disableCertValidation,
                  useSsl3, 
                  useTls12)
              ),
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Todo);

      _storageDataAccess = storageDataAccess;

      InitialEntityMatcher = new InitialTaskEntityMatcher (btypeIdEqualityComparer);
    }


    public IEntityMapper<TaskItemWrapper, IICalendar> EntityMapper
    {
      get { return _entityMapper; }
    }

    public IEntityRepository<TaskItemWrapper, string, DateTime> AtypeRepository
    {
      get { return _atypeRepository; }
    }

    public IEntityRepository<IICalendar, Uri, string> BtypeRepository
    {
      get { return _btypeRepository; }
    }

    public SynchronizationMode SynchronizationMode { get; private set; }
    public DateTime From { get; private set; }
    public DateTime To { get; private set; }
    public IInitialEntityMatcher<TaskItemWrapper, string, DateTime, IICalendar, Uri, string> InitialEntityMatcher { get; private set; }

    public IEntityRelationDataFactory<string, DateTime, Uri, string> EntityRelationDataFactory
    {
      get { return _entityRelationDataFactory; }
    }


    public IEnumerable<IEntityRelationData<string, DateTime, Uri, string>> LoadEntityRelationData ()
    {
      return _storageDataAccess.LoadEntityRelationData();
    }

    public void SaveEntityRelationData (List<IEntityRelationData<string, DateTime, Uri, string>> data)
    {
      _storageDataAccess.SaveEntityRelationData (data);
    }
  }
}
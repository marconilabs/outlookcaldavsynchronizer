﻿using System;
using System.Reflection;
using CalDavSynchronizerTestAutomation.Infrastructure;
using Microsoft.Office.Tools.Ribbon;

namespace CalDavSynchronizerTestAutomation
{
  public partial class CalDavSynchronizerTestRibbon
  {
    private void TestAutomationRibbon_Load (object sender, RibbonUIEventArgs e)
    {
    }

    private void StartTestsButton_Click (object sender, RibbonControlEventArgs e)
    {
      var display = new TestResultDisplay();
      display.Show();
      var runner = new TestRunner (display);
      ManualAssert.Initialize (display);
      OutlookTestContext.Initialize (Globals.ThisAddIn.Application.Session);
      runner.Run (Assembly.GetExecutingAssembly());
    }
  }
}
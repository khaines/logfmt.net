// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
  using System;
  using System.Collections.Generic;
  using Logfmt.ExtensionLogging;
  using Microsoft.Extensions.Logging;
  using Xunit;

  /// <summary>
  /// Tests covering configuration scenarios for the Logfmt.ExtensionLogging provider.
  /// </summary>
  public class ExtensionLoggerConfigurationTests
  {
    /// <summary>
    /// Tests that an empty configuration disables logging for a specific category.
    /// </summary>
    [Fact]
    public void EmptyConfigWithSpecificCategoryDisablesLogging()
    {
      var monitor = new ChangeableOptionsMonitor(new ExtensionLoggerConfiguration());
      using var provider = new ExtensionLoggerProvider(monitor);
      var logger = provider.CreateLogger("SomeCategory");

      Assert.False(logger.IsEnabled(LogLevel.Information));
      Assert.False(logger.IsEnabled(LogLevel.Error));
    }

    /// <summary>
    /// Tests that assigning the same category twice keeps the last value.
    /// </summary>
    [Fact]
    public void DuplicateCategoryAssignmentLastWins()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["cat"] = LogLevel.Warning;
      config.LogLevel["cat"] = LogLevel.Debug;
      var monitor = new ChangeableOptionsMonitor(config);
      using var provider = new ExtensionLoggerProvider(monitor);
      var logger = provider.CreateLogger("cat");

      Assert.True(logger.IsEnabled(LogLevel.Debug));
    }

    /// <summary>
    /// Tests that the Default category level is used when a specific category is not configured.
    /// </summary>
    [Fact]
    public void DefaultCategoryUsedWhenSpecificMissing()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["Default"] = LogLevel.Information;
      var monitor = new ChangeableOptionsMonitor(config);
      using var provider = new ExtensionLoggerProvider(monitor);
      var logger = provider.CreateLogger("unconfigured");

      Assert.True(logger.IsEnabled(LogLevel.Information));
      Assert.True(logger.IsEnabled(LogLevel.Warning));
      Assert.False(logger.IsEnabled(LogLevel.Debug));
    }

    /// <summary>
    /// Tests that an unknown category with no Default configured is disabled.
    /// </summary>
    [Fact]
    public void UnknownCategoryWithoutDefaultIsDisabled()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["specific"] = LogLevel.Debug;
      var monitor = new ChangeableOptionsMonitor(config);
      using var provider = new ExtensionLoggerProvider(monitor);
      var logger = provider.CreateLogger("unknown");

      Assert.False(logger.IsEnabled(LogLevel.Information));
      Assert.False(logger.IsEnabled(LogLevel.Error));
    }

    /// <summary>
    /// Tests that a runtime configuration change through the options monitor is reflected by IsEnabled.
    /// </summary>
    [Fact]
    public void RuntimeLevelChangeReflectedInIsEnabled()
    {
      var initial = new ExtensionLoggerConfiguration();
      initial.LogLevel["Default"] = LogLevel.Warning;
      var monitor = new ChangeableOptionsMonitor(initial);
      using var provider = new ExtensionLoggerProvider(monitor);
      var logger = provider.CreateLogger("cat");

      Assert.False(logger.IsEnabled(LogLevel.Debug));

      var lowered = new ExtensionLoggerConfiguration();
      lowered.LogLevel["Default"] = LogLevel.Debug;
      monitor.Set(lowered);

      Assert.True(logger.IsEnabled(LogLevel.Debug));

      var raised = new ExtensionLoggerConfiguration();
      raised.LogLevel["Default"] = LogLevel.Error;
      monitor.Set(raised);

      Assert.False(logger.IsEnabled(LogLevel.Debug));
      Assert.True(logger.IsEnabled(LogLevel.Error));
    }

    /// <summary>
    /// Tests that resetting the configuration to empty at runtime disables a previously enabled category.
    /// </summary>
    [Fact]
    public void RuntimeConfigResetDisablesCategory()
    {
      var initial = new ExtensionLoggerConfiguration();
      initial.LogLevel["Default"] = LogLevel.Information;
      var monitor = new ChangeableOptionsMonitor(initial);
      using var provider = new ExtensionLoggerProvider(monitor);
      var logger = provider.CreateLogger("cat");

      Assert.True(logger.IsEnabled(LogLevel.Information));

      monitor.Set(new ExtensionLoggerConfiguration());

      Assert.False(logger.IsEnabled(LogLevel.Information));
    }

    /// <summary>
    /// Tests that category matching is case-insensitive, consistent with the provider's case-insensitive logger cache.
    /// </summary>
    [Fact]
    public void CategoryMatchingIsCaseInsensitive()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["MyCategory"] = LogLevel.Debug;
      var monitor = new ChangeableOptionsMonitor(config);
      using var provider = new ExtensionLoggerProvider(monitor);

      var logger = provider.CreateLogger("mycategory");

      Assert.True(logger.IsEnabled(LogLevel.Debug));
    }

    /// <summary>
    /// An <see cref="Microsoft.Extensions.Options.IOptionsMonitor{TOptions}"/> whose value can be changed at runtime.
    /// </summary>
    private sealed class ChangeableOptionsMonitor : Microsoft.Extensions.Options.IOptionsMonitor<ExtensionLoggerConfiguration>
    {
#nullable enable
      private readonly List<Action<ExtensionLoggerConfiguration, string?>> listeners = new List<Action<ExtensionLoggerConfiguration, string?>>();

      public ChangeableOptionsMonitor(ExtensionLoggerConfiguration value)
      {
        this.CurrentValue = value;
      }

      public ExtensionLoggerConfiguration CurrentValue { get; private set; }

      public ExtensionLoggerConfiguration Get(string? name)
      {
        return this.CurrentValue;
      }

      public IDisposable? OnChange(Action<ExtensionLoggerConfiguration, string?> listener)
      {
        this.listeners.Add(listener);
        return new Unsubscriber();
      }

      public void Set(ExtensionLoggerConfiguration value)
      {
        this.CurrentValue = value;
        foreach (var listener in this.listeners)
        {
          listener(value, null);
        }
      }

      private sealed class Unsubscriber : IDisposable
      {
        public void Dispose()
        {
        }
      }
#nullable restore
    }
  }
}

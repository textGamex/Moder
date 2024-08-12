// Distributed under the MIT License. See accompanying file LICENSE or copy
// at https://opensource.org/licenses/MIT).
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Moder.Hosting;

/// <summary>
/// Represents a base class for a user interface thread in a hosted
/// application.
/// </summary>
/// <typeparam name="T">
/// The concrete type of the class extending <see cref="BaseHostingContext" />
/// which will provide the necessary options to setup the User Interface.
/// </typeparam>
public abstract partial class BaseUserInterfaceThread<T> : IDisposable, IUserInterfaceThread
    where T : BaseHostingContext
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger _logger;
    private readonly ManualResetEvent _serviceManualResetEvent = new(false);

    // This manual reset event is signaled when the UI thread completes. It is
    // primarily used in testing environment to ensure that the thread execution
    // completes before the test results are verified.
    private readonly ManualResetEvent _uiThreadCompletion = new(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUserInterfaceThread{T}" /> class.
    /// </summary>
    /// <remarks>
    /// This constructor creates a new thread that runs the UI. The thread is
    /// set to be a background thread with a single-threaded apartment state.
    /// The thread will wait for a signal from the <see cref="_serviceManualResetEvent" />
    /// before starting the user interface. The constructor also calls the
    /// <see cref="BeforeStart" /> and <see cref="OnCompletion" /> methods to
    /// perform any initialization and cleanup tasks.
    /// </remarks>
    /// <param name="lifetime">
    /// The hosted application lifetime. Used when the hosting context
    /// indicates that that the UI and the hosted application lifetimes are
    /// linked.
    /// </param>
    /// <param name="context">
    /// The UI service hosting context, partially populated with the
    /// configuration options for the UI thread.
    /// </param>
    /// <param name="logger">The logger to be used by this class.</param>
    protected BaseUserInterfaceThread(IHostApplicationLifetime lifetime, T context, ILogger logger)
    {
        _hostApplicationLifetime = lifetime;
        HostingContext = context;
        _logger = logger;

        // Create a thread which runs the UI
        var newUiThread = new Thread(
            () =>
            {
                BeforeStart();
                _ = _serviceManualResetEvent.WaitOne(); // wait for the signal to actually start
                HostingContext.IsRunning = true;
                DoStart();
                OnCompletion();
            })
        {
            IsBackground = true,
        };

        // Set the apartment state
        newUiThread.SetApartmentState(ApartmentState.STA);

        // Transition the new UI thread to the RUNNING state. Note that the
        // thread will actually start after the `serviceManualResetEvent` is
        // set.
        newUiThread.Start();
    }

    /// <summary>
    /// Gets the hosting context for the user interface service.
    /// </summary>
    /// <value>
    /// Although never <c>null</c>, the different fields of the hosting context
    /// may or may not contain valid values depending on the current state of
    /// the User Interface thread. Refer to the concrete class documentation.
    /// </value>
    protected T HostingContext { get; }

    /// <summary>
    /// Actually starts the User Interface thread by setting the underlying
    /// <see cref="ManualResetEvent" />.
    /// </summary>
    /// Initially, the User Interface thread is created and transitioned into
    /// the `RUNNING` state, but it is waiting to be explicitly started via the
    /// <see cref="ManualResetEvent" />
    /// so that we can ensure everything
    /// required for the UI is initialized before we start it. The
    /// responsibility for triggering this rests with the User Interface hosted
    /// service.
    public void StartUserInterface() => _serviceManualResetEvent.Set();

    /// <inheritdoc />
    public abstract Task StopUserInterfaceAsync();

    /// <summary>
    /// Wait until the created User Interface Thread completes its
    /// execution.
    /// </summary>
    public void AwaitUiThreadCompletion() => _uiThreadCompletion.WaitOne();

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _serviceManualResetEvent.Dispose();
    }

    /// <summary>
    /// Called before the UI thread is started to do any
    /// initialization work.
    /// </summary>
    protected abstract void BeforeStart();

    /// <summary>
    /// Do the work needed to actually start the User Interface thread.
    /// </summary>
    protected abstract void DoStart();

    /// <summary>
    /// Called upon completion of the UI thread (i.e. no more UI). Will
    /// eventually request the hosted application to stop depending on whether
    /// the UI lifecycle and the application lifecycle are linked or not.
    /// </summary>
    /// <seealso cref="BaseHostingContext.IsLifetimeLinked" />
    private void OnCompletion()
    {
        Debug.Assert(
            HostingContext.IsRunning,
            "Expecting the `IsRunning` flag to be set when `OnCompletion() is called");
        HostingContext.IsRunning = false;
        if (HostingContext.IsLifetimeLinked)
        {
            StoppingHostApplication();

            if (!_hostApplicationLifetime.ApplicationStopped.IsCancellationRequested &&
                !_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                _hostApplicationLifetime.StopApplication();
            }
        }

        _ = _uiThreadCompletion.Set();
    }

    [LoggerMessage(
        SkipEnabledCheck = true,
        Level = LogLevel.Debug,
        Message = "Stopping hosted application due to user interface thread exit.")]
    partial void StoppingHostApplication();
}

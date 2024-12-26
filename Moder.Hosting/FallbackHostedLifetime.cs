// SPDX-License-Identifier: MIT

/*
MIT License
Copyright (c) 2023 dSPACE GmbH, Germany, Contributed by Carsten Igel <CIgel@dspace>
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Moder.Hosting;

internal sealed class FallbackHostedLifetime : IHostedLifetime
{
    private readonly ILogger<FallbackHostedLifetime> _logger;

    internal FallbackHostedLifetime(ILogger<FallbackHostedLifetime> logger)
    {
        _logger = logger;
    }

    public Task<int> StartAsync(Application application, CancellationToken cancellationToken)
    {
        int RunWithCancellationToken()
        {
            try
            {
                application.Run(cancellationToken);
                return ExitCodes.ExitSuccessfully;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Critical))
                {
                    _logger.LogCritical(ex, "Failure while running application");
                }

                return ExitCodes.ExitWithError;
            }
        }
        return Task.Run(RunWithCancellationToken, cancellationToken);
    }

    public Task StopAsync(Application application, CancellationToken cancellationToken)
    {
        return Task.FromException<NotSupportedException>(new NotSupportedException());
    }
}

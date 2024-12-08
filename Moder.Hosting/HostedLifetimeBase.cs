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

using Avalonia.Controls.ApplicationLifetimes;

namespace Moder.Hosting;

internal abstract class HostedLifetimeBase<TRuntime>: IHostedLifetime where TRuntime: IApplicationLifetime  
{
    public int ExitCode { get; } = 0;
    
    protected HostedLifetimeBase(TRuntime runtime)
    {
        Runtime = runtime;
    }

    protected TRuntime Runtime { get; private init; }

    public abstract Task<int> StartAsync(Avalonia.Application application, CancellationToken cancellationToken);
    public abstract Task StopAsync(Avalonia.Application application, CancellationToken cancellationToken);
}
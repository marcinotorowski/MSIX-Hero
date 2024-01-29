// MIT License
// Copyright (C) 2024 Marcin Otorowski
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// https://github.com/marcinotorowski/simpleelevation/blob/develop/LICENSE.md

using System.Security.Principal;
using Dapplo.Log;

namespace Otor.MsixHero.Elevation.Handling;

public abstract class ClientHandler : IDisposable, IAsyncDisposable
{
    private static readonly LogSource Log = new LogSource();

    private readonly Lazy<bool> _isAdmin = new Lazy<bool>(IsAdmin);

    public abstract ValueTask<bool> IsUacHelperRunning();

    public virtual void Dispose()
    {
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask<bool> IsRunningAsAdministratorAsync()
    {
        return ValueTask.FromResult(_isAdmin.Value);
    }

    public abstract ValueTask<bool> StartServerAsync(bool keepElevation);
    
    private static bool IsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            Log.Info().WriteLine("UAC Client -> Running as administrator…");
            return true;
        }

        Log.Info().WriteLine("UAC Client -> Running as administrator…");
        return false;
    }
}
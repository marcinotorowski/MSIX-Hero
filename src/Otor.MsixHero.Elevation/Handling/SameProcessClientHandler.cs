// MIT License
// 
// Copyright(c) 2022 Marcin Otorowski
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

namespace Otor.MsixHero.Elevation.Handling;

public class SameProcessClientHandler : ClientHandler, IDisposable
{
    private readonly Lazy<SimpleElevationServer> _server;

    public SameProcessClientHandler(Func<SimpleElevationServer> serverFactory)
    {
        this._server = new Lazy<SimpleElevationServer>(serverFactory);
    }

    public SameProcessClientHandler(SimpleElevationServer serverFactory)
    {
        this._server = new Lazy<SimpleElevationServer>(serverFactory);
    }

    public override ValueTask<bool> IsUacHelperRunning()
    {
        return ValueTask.FromResult(this._server.IsValueCreated);
    }

    public override ValueTask<bool> StartServerAsync(bool keepElevation)
    {
        var server = this._server.Value;
#pragma warning disable CS4014
        server.StartAsync(keepElevation);
#pragma warning restore CS4014
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
        if (this._server.IsValueCreated)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (this._server.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
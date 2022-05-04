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

using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Microsoft.Win32.SafeHandles;

namespace Otor.MsixHero.Elevation.Ipc.Native
{
    /// <summary>
    /// Native API for Named Pipes
    /// This code was borrowed from PowerShell here:
    /// https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/remoting/common/RemoteSessionNamedPipe.cs#L124-L256
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class NamedPipeNative
    {
        // Pipe open mode
        internal const uint PIPE_ACCESS_DUPLEX = 0x00000003;

        // Pipe modes
        internal const uint PIPE_TYPE_BYTE = 0x00000000;
        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
        internal const uint PIPE_READ_MODE_BYTE = 0x00000000;
        

        [StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            /// <summary>
            /// The size, in bytes, of this structure. Set this value to the size of the SECURITY_ATTRIBUTES structure.
            /// </summary>
            public int NLength;

            /// <summary>
            /// A pointer to a security descriptor for the object that controls the sharing of it.
            /// </summary>
            public IntPtr LPSecurityDescriptor = IntPtr.Zero;

            /// <summary>
            /// A Boolean value that specifies whether the returned handle is inherited when a new process is created.
            /// </summary>
            public bool InheritHandle;

            /// <summary>
            /// Initializes a new instance of the SECURITY_ATTRIBUTES class
            /// </summary>
            public SECURITY_ATTRIBUTES()
            {
                this.NLength = 12;
            }
        }
        
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafePipeHandle CreateNamedPipe(
           string lpName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           SECURITY_ATTRIBUTES securityAttributes);

        public static SECURITY_ATTRIBUTES GetSecurityAttributes(GCHandle securityDescriptorPinnedHandle, bool inheritHandle = false)
        {
            var securityAttributes = new SECURITY_ATTRIBUTES
            {
                InheritHandle = inheritHandle
            };

            securityAttributes.NLength = Marshal.SizeOf(securityAttributes);
            securityAttributes.LPSecurityDescriptor = securityDescriptorPinnedHandle.AddrOfPinnedObject();
            return securityAttributes;
        }
        
        public static NamedPipeServerStream CreateNamedPipe(string pipeName, PipeSecurity pipeSecurity)
        {
            var fullPipeName = @"\\.\pipe\" + pipeName;
            var securityDesc = new CommonSecurityDescriptor(false, false, pipeSecurity.GetSecurityDescriptorBinaryForm(), 0);

            // Create optional security attributes based on provided PipeSecurity.

            var securityDescBuffer = new byte[securityDesc.BinaryLength];
            securityDesc.GetBinaryForm(securityDescBuffer, 0);

            GCHandle? securityDescHandle = GCHandle.Alloc(securityDescBuffer, GCHandleType.Pinned);
            var securityAttributes = NamedPipeNative.GetSecurityAttributes(securityDescHandle.Value);

            // Create named pipe.
            var pipeHandle = NamedPipeNative.CreateNamedPipe(
                fullPipeName,
                NamedPipeNative.PIPE_ACCESS_DUPLEX | 
                NamedPipeNative.FILE_FLAG_FIRST_PIPE_INSTANCE |
                 NamedPipeNative.FILE_FLAG_OVERLAPPED,
                NamedPipeNative.PIPE_TYPE_BYTE | NamedPipeNative.PIPE_READ_MODE_BYTE,
                1,
                1024,
                1024,
                0,
                securityAttributes);

            var lastError = Marshal.GetLastWin32Error();
            securityDescHandle.Value.Free();

            if (pipeHandle.IsInvalid || lastError != 0)
            {
                pipeHandle.Close();
                throw new InvalidOperationException("Could not create pipe server.");
            }

            try
            {
                return new NamedPipeServerStream(
                    PipeDirection.InOut,
                    true, // IsAsync
                    false, // IsConnected
                    pipeHandle);
            }
            catch (Exception)
            {
                pipeHandle.Dispose();
                throw;
            }
        }
    }
}

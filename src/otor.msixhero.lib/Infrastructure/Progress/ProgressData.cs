using System;

namespace otor.msixhero.lib.Infrastructure.Progress
{
    [Serializable]
    public struct ProgressData
    {
        public ProgressData(int progress, string message)
        {
            Progress = progress;
            Message = message;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public int Progress { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Message { get; }
    }
}
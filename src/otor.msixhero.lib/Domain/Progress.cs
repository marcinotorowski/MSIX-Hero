using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using otor.msixhero.lib.BusinessLayer.State;

namespace otor.msixhero.lib.Domain
{
    public class Progress : Progress<ProgressData>
    {
    }
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

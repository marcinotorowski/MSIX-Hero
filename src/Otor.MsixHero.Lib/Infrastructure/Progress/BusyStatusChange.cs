// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

namespace Otor.MsixHero.Lib.Infrastructure.Progress
{
    public class BusyStatusChange : IBusyStatusChange
    {
        public BusyStatusChange(OperationType type, bool isBusy, string message, int progress)
        {
            this.Type = type;
            this.IsBusy = isBusy;
            this.Message = message;
            this.Progress = progress;
        }

        public OperationType Type { get; }

        public bool IsBusy { get; private set; }

        public string Message { get; private set; }

        public int Progress { get; private set; }
    }
}
// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

namespace Otor.MsixHero.Elevation.Ipc;

public class MsixHeroRemoteInvocationException : Exception
{
    public MsixHeroRemoteInvocationException(SerializableExceptionData data)
    {
        this.SerializableData = data;
    }

    public SerializableExceptionData SerializableData { get; }

    public override string Message => this.SerializableData.Message ?? "Remoting exception";

    public override string? StackTrace => this.SerializableData.StackTrace;

    public override Exception GetBaseException()
    {
        var mostInner = this.SerializableData;
        while (mostInner.InnerException != null)
        {
            mostInner = mostInner.InnerException;
        }

        if (mostInner == this.SerializableData.InnerException)
        {
            return this;
        }

        return new MsixHeroRemoteInvocationException(mostInner);
    }

    public override string? HelpLink
    {
        get => this.SerializableData.HelpLink;
        set => this.SerializableData.HelpLink = value;
    }

    public override string? Source
    {
        get => this.SerializableData.Source;
        set => this.SerializableData.Source = value;
    }
}
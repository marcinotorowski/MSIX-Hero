// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public class ContainerValidationArgs
    {
        public ContainerValidationArgs()
        {
            this.SetValid();
        }

        public ContainerValidationArgs(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                this.SetValid();
            }
            else
            {
                this.SetError(errorMessage);
            }
        }

        public void SetError(string message)
        {
            this.IsValid = false;
            this.ValidationMessage = message;
        }

        public void SetValid()
        {
            this.IsValid = true;
            this.ValidationMessage = null;
        }

        public bool IsValid { get; private set; }

        public string ValidationMessage { get; private set; }
    }
}
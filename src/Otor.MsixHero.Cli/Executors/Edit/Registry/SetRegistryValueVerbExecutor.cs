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

using System;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Registry;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry;
using Otor.MsixHero.Cli.Verbs.Edit.Registry;
using Otor.MsixHero.Registry.Converter;
using ValueType = Otor.MsixHero.Registry.Parser.ValueType;

namespace Otor.MsixHero.Cli.Executors.Edit.Registry
{
    public class SetRegistryValueVerbExecutor : BaseEditVerbExecutor<SetRegistryValueEditVerb>
    {
        public SetRegistryValueVerbExecutor(string package, SetRegistryValueEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task<int> Validate()
        {
            var baseResult = await base.Validate().ConfigureAwait(false);
            if (baseResult != StandardExitCodes.ErrorSuccess)
            {
                return baseResult;
            }

            if (!Enum.TryParse<ValueType>(this.Verb.ValueType, true, out var parsed))
            {
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_SetRegistryValue_Error_ParseType_Format, this.Verb.ValueType)).ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            try
            {
                switch (parsed)
                {
                    case ValueType.String:
                    case ValueType.Expandable:
                    case ValueType.Default:
                    case ValueType.None:
                        RawRegistryValueConverter.GetStringFromString(this.Verb.Value);
                        break;
                    case ValueType.DWord:
                        RawRegistryValueConverter.GetDWordFromString(this.Verb.Value);
                        break;
                    case ValueType.QWord:
                        RawRegistryValueConverter.GetQWordFromString(this.Verb.Value);
                        break;
                    case ValueType.Multi:
                        RawRegistryValueConverter.GetMultiValueFromString(this.Verb.Value);
                        break;
                    case ValueType.Binary:
                        RawRegistryValueConverter.GetByteArrayFromString(this.Verb.Value);
                        break;
                    default:
                        await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_SetRegistryValue_Error_TypeNotSupported_Format, this.Verb.ValueType)).ConfigureAwait(false);
                        return StandardExitCodes.ErrorParameter;
                }
            }
            catch (ArgumentException e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            return StandardExitCodes.ErrorSuccess;
        }

        protected override async Task<int> ExecuteOnExtractedPackage(string directoryPath)
        {
            var command = new SetRegistryValue
            {
                RegistryKey = this.Verb.RegistryKey,
                RegistryValueName = this.Verb.RegistryValueName,
                ValueType = Enum.Parse<ValueType>(this.Verb.ValueType, true)
            };

            switch (command.ValueType)
            {
                case ValueType.None:
                case ValueType.Default:
                case ValueType.String:
                case ValueType.Expandable:
                    command.Value = RawRegistryValueConverter.GetStringFromString(this.Verb.Value);
                    break;
                case ValueType.DWord:
                    unchecked
                    {
                        // Note: this conversion is by-design because .NET API for registry expects signed values.
                        command.Value = (int)RawRegistryValueConverter.GetDWordFromString(this.Verb.Value);
                        break;
                    }
                case ValueType.QWord:
                    unchecked
                    {
                        // Note: this conversion is by-design because .NET API for registry expects signed values.
                        command.Value = (long)RawRegistryValueConverter.GetQWordFromString(this.Verb.Value);
                        break;
                    }
                case ValueType.Multi:
                    command.Value = RawRegistryValueConverter.GetMultiValueFromString(this.Verb.Value);
                    break;
                case ValueType.Binary:
                    command.Value = RawRegistryValueConverter.GetByteArrayFromString(this.Verb.Value);
                    break;
                default:
                    return StandardExitCodes.ErrorGeneric;
            }

            var target = RegistryPathConverter.ToCanonicalRegistryPath(command.RegistryKey);

            try
            {
                await new SetRegistryValueExecutor(directoryPath).Execute(command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_SetRegistryValue_Error_Format, this.Verb.RegistryValueName, this.Verb.Value, command.ValueType.ToString("G"), target.Item1, target.Item2)).ConfigureAwait(false);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            await this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_SetRegistryValue_Success_Format, this.Verb.RegistryValueName, this.Verb.Value, command.ValueType.ToString("G"), target.Item1, target.Item2));
            return StandardExitCodes.ErrorSuccess;
        }
    }
}
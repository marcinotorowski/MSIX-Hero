using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Otor.MsixHero.Cli.Resources;

namespace Otor.MsixHero.Cli
{
    public class LocalizableSentenceBuilder : SentenceBuilder
    {
        public override Func<string> RequiredWord => () => Localization.CommandLineParser_SentenceRequiredWord;

        public override Func<string> ErrorsHeadingText => () => Localization.CommandLineParser_SentenceErrorsHeadingText;

        public override Func<string> OptionGroupWord => () => Localization.CommandLineParser_Group;

        public override Func<string> UsageHeadingText => () => Localization.CommandLineParser_SentenceUsageHeadingText;

        public override Func<bool, string> HelpCommandText
        {
            get
            {
                return isOption => isOption
                    ? Localization.CommandLineParser_SentenceHelpCommandTextOption
                    : Localization.CommandLineParser_SentenceHelpCommandTextVerb;
            }
        }

        public override Func<bool, string> VersionCommandText => _ => Localization.CommandLineParser_SentenceVersionCommandText;

        public override Func<Error, string> FormatError
        {
            get
            {
                return error =>
                {
                    switch (error.Tag)
                    {
                        case ErrorType.BadFormatTokenError:
                            return string.Format(Localization.CommandLineParser_SentenceBadFormatTokenError, ((BadFormatTokenError)error).Token);
                        case ErrorType.MissingValueOptionError:
                            return string.Format(Localization.CommandLineParser_SentenceMissingValueOptionError, ((MissingValueOptionError)error).NameInfo.NameText);
                        case ErrorType.UnknownOptionError:
                            return string.Format(Localization.CommandLineParser_SentenceUnknownOptionError, ((UnknownOptionError)error).Token);
                        case ErrorType.MissingRequiredOptionError:
                            var errorMissing = ((MissingRequiredOptionError)error);
                            return errorMissing.NameInfo.Equals(NameInfo.EmptyName)
                                       ? Localization.CommandLineParser_SentenceMissingRequiredOptionError
                                       : string.Format(Localization.CommandLineParser_SentenceMissingRequiredOptionError, errorMissing.NameInfo.NameText);
                        case ErrorType.BadFormatConversionError:
                            var badFormat = ((BadFormatConversionError)error);
                            return badFormat.NameInfo.Equals(NameInfo.EmptyName)
                                       ? Localization.CommandLineParser_SentenceBadFormatConversionErrorValue
                                       : string.Format(Localization.CommandLineParser_SentenceBadFormatConversionErrorOption, badFormat.NameInfo.NameText);
                        case ErrorType.SequenceOutOfRangeError:
                            var seqOutRange = (SequenceOutOfRangeError)error;
                            return seqOutRange.NameInfo.Equals(NameInfo.EmptyName)
                                       ? Localization.CommandLineParser_SentenceSequenceOutOfRangeErrorValue
                                       : string.Format(Localization.CommandLineParser_SentenceSequenceOutOfRangeErrorOption, seqOutRange.NameInfo.NameText);
                        case ErrorType.BadVerbSelectedError:
                            return string.Format(Localization.CommandLineParser_SentenceBadVerbSelectedError, ((BadVerbSelectedError)error).Token);
                        case ErrorType.NoVerbSelectedError:
                            return Localization.CommandLineParser_SentenceNoVerbSelectedError;
                        case ErrorType.RepeatedOptionError:
                            return string.Format(Localization.CommandLineParser_SentenceRepeatedOptionError, ((RepeatedOptionError)error).NameInfo.NameText);
                        case ErrorType.SetValueExceptionError:
                            var setValueError = (SetValueExceptionError)error;
                            return string.Format(Localization.CommandLineParser_SentenceSetValueExceptionError, setValueError.NameInfo.NameText, setValueError.Exception.Message);
                    }

                    throw new InvalidOperationException();
                };
            }
        }

        public override Func<IEnumerable<MutuallyExclusiveSetError>, string> FormatMutuallyExclusiveSetErrors
        {
            get
            {
                return errors =>
                {
                    var bySet = (from e in errors
                                group e by e.SetName into g
                                select new { SetName = g.Key, Errors = g.ToList() }).ToArray();

                    var msgs = bySet.Select(
                        set =>
                        {
                            var names = string.Join(
                                string.Empty,
                                (from e in set.Errors select $"'{e.NameInfo.NameText}', ").ToArray());
                            var namesCount = set.Errors.Count();

                            var incompatible = string.Join(
                                string.Empty,
                                (from x in
                                     (from s in bySet where !s.SetName.Equals(set.SetName) from e in s.Errors select e)
                                    .Distinct()
                                 select $"'{x.NameInfo.NameText}', ").ToArray());
                            
                            return string.Format(Localization.CommandLineParser_SentenceMutuallyExclusiveSetErrors, names.Substring(0, names.Length - 2), incompatible.Substring(0, incompatible.Length - 2));
                        }).ToArray();

                    return string.Join(Environment.NewLine, msgs);
                };
            }
        }
    }
}

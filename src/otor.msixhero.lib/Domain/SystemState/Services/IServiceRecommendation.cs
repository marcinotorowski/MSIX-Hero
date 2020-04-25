using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.Domain.SystemState.Services
{
    public interface IServiceRecommendation
    {
        string Name { get; }

        string ActionPrompt { get; }

        bool ExpectedToRun { get; }

        bool Actual { get; }
    }
}

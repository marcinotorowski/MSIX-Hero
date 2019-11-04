using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Models.Configuration;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure
{
    public interface IConfigurationService
    {
        Task<Configuration> GetConfiguration(CancellationToken token);
    }
}

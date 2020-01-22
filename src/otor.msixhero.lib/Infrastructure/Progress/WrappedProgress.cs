using System;
using System.Collections.Generic;
using System.Linq;

namespace otor.msixhero.lib.Infrastructure.Progress
{
    public class WrappedProgress : IDisposable
    {
        private readonly IProgress<ProgressData> parent;
        private double totalWeight;
        private readonly IDictionary<Progress, double> weights = new Dictionary<Progress, double>();
        private readonly IDictionary<Progress, int> lastProgress = new Dictionary<Progress, int>();

        public WrappedProgress(IProgress<ProgressData> parent)
        {
            this.parent = parent;
        }

        public IProgress<ProgressData> GetChildProgress(double weight)
        {
            if (this.parent == null)
            {
                return null;
            }

            var progress = new Progress();
            this.weights[progress] = weight;
            this.lastProgress[progress] = 0;
            progress.ProgressChanged += this.OnProgressChanged;
            totalWeight += weight;
            return progress;
        }

        public void Dispose()
        {
            foreach (var item in this.weights.Keys)
            {
                item.ProgressChanged -= this.OnProgressChanged;
            }
        }

        private void OnProgressChanged(object sender, ProgressData e)
        {
            if (!this.weights.Any())
            {
                return;
            }

            if (this.weights.Count == 1)
            {
                this.parent.Report(e);
                return;
            }

            var progressInstance = (Progress)sender;
            var weight = this.weights[progressInstance];
            this.lastProgress[progressInstance] = (int)(weight * e.Progress);
            this.parent.Report(new ProgressData((int)(1.0 * this.lastProgress.Values.Sum() / this.totalWeight), e.Message));
        }
    }
}
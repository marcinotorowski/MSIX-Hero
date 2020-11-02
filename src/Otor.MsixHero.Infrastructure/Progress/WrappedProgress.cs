using System;
using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.Infrastructure.Progress
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

            var val = 0;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var key in this.weights.Keys)
            {
                val += this.lastProgress[key];
            }

            var p = new ProgressData((int) (1.0 * val / this.totalWeight), e.Message);
            Console.WriteLine("Reporting progress {0}% {1}..", p.Progress, p.Message);
            this.parent.Report(p);
        }
    }
}
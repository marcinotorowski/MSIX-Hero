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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.Infrastructure.Progress
{
    public class WrappedProgress : IDisposable
    {
        private readonly IList<ChildProgress> childProgressElements = new List<ChildProgress>();
        private readonly IProgress<ProgressData> parent;
        private readonly bool sealOnceStarted;
        private bool hasReported;

        public WrappedProgress(IProgress<ProgressData> parent, bool sealOnceStarted = true)
        {
            this.parent = parent;
            this.sealOnceStarted = sealOnceStarted;
        }

        public IProgress<ProgressData> GetChildProgress(double weight = 1.0)
        {
            if (this.parent == null)
            {
                // If the parent is null, we return an instance of a progress but its results are always 
                // swallowed (not used anywhere).
                return new ChildProgress(weight);
            }

            if (weight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "The weight must be greater than zero.");
            }

            if (this.hasReported && this.sealOnceStarted)
            {
                throw new InvalidOperationException("Cannot add a new progress after at least one of already added has reported anything.");
            }
            
            var progress = new ChildProgress(weight);
            this.childProgressElements.Add(progress);
            progress.ProgressChanged += this.OnProgressChanged;
            return progress;
        }

        public void Dispose()
        {
            foreach (var item in this.childProgressElements)
            {
                item.ProgressChanged -= this.OnProgressChanged;
            }
        }

        private void OnProgressChanged(object sender, ProgressData e)
        {
            this.hasReported = true;
            var coercedProgress = Math.Max(0.0, Math.Min(100.0, e.Progress));
            
            // If this event handler is fired, then the count of progress elements must be 1 or higher. Empty list is not possible.
            if (this.childProgressElements.Count == 1)
            {
                // Short way, no need to calculate weights etc.
                this.parent.Report(new ProgressData((int)coercedProgress, e.Message));
                return;
            }

            var weightedSum = this.childProgressElements.Sum(progress => progress.Weight * Math.Max(0.0, Math.Min(100.0, progress.Last.Progress)));
            var sumWeights = this.childProgressElements.Sum(progress => progress.Weight);     
            
            var p = new ProgressData((int)(weightedSum / sumWeights), e.Message);
            this.parent.Report(p);
        }

        private class ChildProgress : IProgress<ProgressData>
        {
            public ChildProgress(double weight)
            {
                if (weight <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(weight), "The weight must be greater than zero.");
                }    
                
                this.Weight = weight;
                this.Last = new ProgressData(0, null);
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public double Weight { get; private set; }

            // ReSharper disable once MemberCanBePrivate.Local
            public ProgressData Last { get; private set; }
            
            public void Report(ProgressData value)
            {
                this.Last = value;
                this.ProgressChanged?.Invoke(this, value);
            }
            
            public event EventHandler<ProgressData> ProgressChanged;
        }
    }
}
using System;
using NUnit.Framework;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Tests.Ui.Progress
{
    [TestFixture]
    public class ProgressTests
    {
        [Test]
        public void TestZeroChildren()
        {
            var progress = new TestProgress();

            using (new WrappedProgress(progress))
            {
            }
        }
        
        [Test]
        public void TestNullParent()
        {
            using var wrapped = new WrappedProgress(null);
            var p1 = wrapped.GetChildProgress();
            var p2 = wrapped.GetChildProgress();

            // The following may not throw. This class should by design handle cases where parent is null,
            // by simply swallowing sub-progress reports.
            p1.Report(new ProgressData(10, "test"));
            p2.Report(new ProgressData(10, "test"));
        }
        
        [Test]
        public void TestInvalidOperations()
        {
            var progress = new TestProgress();

            using (var wrapped = new WrappedProgress(progress))
            {
                var p1 = wrapped.GetChildProgress();
                p1.Report(new ProgressData(50, "test"));
                
                Assert.Throws<InvalidOperationException>(() => wrapped.GetChildProgress(), "Creating a new child progress should be disallowed once at lest one sub-progress has already reported.");
            }
            
            var progress2 = new TestProgress();

            using (var wrapped = new WrappedProgress(progress2, false))
            {
                var p1 = wrapped.GetChildProgress();
                p1.Report(new ProgressData(50, "test"));

                // This may not throw because of the second argument in WrappedProgress ctor
                wrapped.GetChildProgress();
            }
        }
        
        [Test]
        public void TestZeroWeight()
        {
            var progress = new TestProgress();

            using var wrapped = new WrappedProgress(progress);
            Assert.Throws<ArgumentOutOfRangeException>(() => wrapped.GetChildProgress(0), "Zero weight must throw ArgumentOutOfRange exception.");
            Assert.Throws<ArgumentOutOfRangeException>(() => wrapped.GetChildProgress(-1), "Negative weight must throw ArgumentOutOfRange exception.");
        }

        [Test]
        public void TestMessages()
        {
            var progress = new TestProgress();

            using var wrapped = new WrappedProgress(progress);
            var progressA = wrapped.GetChildProgress();
            var progressB = wrapped.GetChildProgress();

            progressA.Report(new ProgressData(10, "A1"));
            Assert.That(progress.Last.Message, Is.EqualTo("A1"));

            progressA.Report(new ProgressData(20, "A2"));
            Assert.That(progress.Last.Message, Is.EqualTo("A2"));

            progressB.Report(new ProgressData(10, "B1"));
            Assert.That(progress.Last.Message, Is.EqualTo("B1"));

            progressA.Report(new ProgressData(100, "A3"));
            Assert.That(progress.Last.Message, Is.EqualTo("A3"));

            progressB.Report(new ProgressData(100, "B2"));
            Assert.That(progress.Last.Message, Is.EqualTo("B2"));
        }
        
        [Test]
        public void TestWrongChildrenReports()
        {
            var progress = new TestProgress();

            using var wrapped = new WrappedProgress(progress);
            var p1 = wrapped.GetChildProgress();
            var p2 = wrapped.GetChildProgress();

            p1.Report(new ProgressData(200, null));
            Assert.That(progress.Last.Progress, Is.EqualTo(50), "Even though the children may report over 100% completion, the overall progress may not overflow.");
                
            p2.Report(new ProgressData(300, null));
            Assert.That(progress.Last.Progress, Is.EqualTo(100), "Even though the children may report over 100% completion, the overall progress may not overflow.");
        }

        [Test]
        public void TestWrappedProgress()
        {
            var progress = new TestProgress();

            using var wrapped = new WrappedProgress(progress);
            Assert.That(progress.Last.Progress, Is.EqualTo(0));

            const int weight1 = 700;
            const int weight2 = 200;
            const int weight3 = 100;

            var p1 = wrapped.GetChildProgress(weight1);
            var p2 = wrapped.GetChildProgress(weight2);
            var p3 = wrapped.GetChildProgress(weight3);

            p1.Report(new ProgressData(0, null));
            p2.Report(new ProgressData(0, null));
            p3.Report(new ProgressData(0, null));
            Assert.That(progress.Last.Progress, Is.EqualTo(0), "If all sub elements report 0% progress, then the overall progress is also 0%.");

            p1.Report(new ProgressData(100, null));
            // (100% * 700 + 0% * 200 + 0% * 100) / (100 + 200 + 700) = 70%
            Assert.That(progress.Last.Progress, Is.EqualTo(70));

            p2.Report(new ProgressData(50, null));
            // (100% * 700 + 50% * 200 + 0% * 100) / (100 + 200 + 700) = 80%
            Assert.That(progress.Last.Progress, Is.EqualTo(80));

            p3.Report(new ProgressData(10, null));
            // (100% * 700 + 50% * 200 + 10% * 100) / (100 + 200 + 700) = 81%
            Assert.That(progress.Last.Progress, Is.EqualTo(81));

            p2.Report(new ProgressData(100, null));
            // (100% * 700 + 100% * 200 + 10% * 100) / (100 + 200 + 700) = 91%
            Assert.That(progress.Last.Progress, Is.EqualTo(91));

            p3.Report(new ProgressData(100, null));
            // (100% * 700 + 100% * 200 + 100% * 100) / (100 + 200 + 700) = 100%
            Assert.That(progress.Last.Progress, Is.EqualTo(100));

            p3.Report(new ProgressData(0, null));
            // (100% * 700 + 100% * 200 + 0% * 100) / (100 + 200 + 700) = 90%
            Assert.That(progress.Last.Progress, Is.EqualTo(90), "If progress of a child element goes backward, the reported wrapped progress should also reflect this.");
        }

        private class TestProgress : IProgress<ProgressData>
        {
            public ProgressData Last { get; private set; } = new ProgressData(0, null);

            public void Report(ProgressData value)
            {
                this.Last = value;
            }
        }
    }
}

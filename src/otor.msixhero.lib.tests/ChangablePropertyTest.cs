using System;
using NUnit.Framework;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.lib.tests
{
    [TestFixture()]
    public class ChangablePropertyTest
    {
        [Test]
        public void ValidatorTest()
        {
            Func<int, string> validator = i => i % 2 != 0 ? "The number must be even" : null;

            var validatedProperty = new ValidatedChangeableProperty<int>(validator, 0);
            Assert.IsTrue(validatedProperty.IsValidated);
            Assert.IsTrue(validatedProperty.IsValid);
            Assert.IsNull(validatedProperty.ValidationMessage);

            validatedProperty.CurrentValue = 1;
            Assert.IsTrue(validatedProperty.IsValidated);
            Assert.IsFalse(validatedProperty.IsValid);
            Assert.AreEqual("The number must be even", validatedProperty.ValidationMessage);

            validatedProperty.IsValidated = false;
            Assert.IsFalse(validatedProperty.IsValidated);
            Assert.IsTrue(validatedProperty.IsValid);
            Assert.IsNull(validatedProperty.ValidationMessage);
        }

        [Test]
        public void ChangeableContainerTest()
        {
            var prop1 = new ChangeableProperty<string>("initial");
            var prop2 = new ChangeableProperty<string>("initial");

            var container = new ChangeableContainer(prop1, prop2);
            Assert.IsFalse(prop1.IsTouched);
            Assert.IsFalse(prop2.IsTouched);
            Assert.IsFalse(container.IsTouched);
            Assert.IsFalse(prop1.IsDirty);
            Assert.IsFalse(prop2.IsDirty);
            Assert.IsFalse(container.IsDirty);

            prop1.CurrentValue = "other";
            Assert.IsTrue(prop1.IsTouched);
            Assert.IsFalse(prop2.IsTouched);
            Assert.IsTrue(container.IsTouched);
            Assert.IsTrue(prop1.IsDirty);
            Assert.IsFalse(prop2.IsDirty);
            Assert.IsTrue(container.IsDirty);

            prop1.CurrentValue = "initial";
            Assert.IsTrue(prop1.IsTouched);
            Assert.IsFalse(prop2.IsTouched);
            Assert.IsTrue(container.IsTouched);
            Assert.IsFalse(prop1.IsDirty);
            Assert.IsFalse(prop2.IsDirty);
            Assert.IsFalse(container.IsDirty);

            prop1.Reset(ValueResetType.Hard);
            Assert.IsFalse(prop1.IsTouched);
            Assert.IsFalse(prop2.IsTouched);
            Assert.IsFalse(container.IsTouched);
            Assert.IsFalse(prop1.IsDirty);
            Assert.IsFalse(prop2.IsDirty);
            Assert.IsFalse(container.IsDirty);

            prop2.CurrentValue = "second";
            Assert.IsFalse(prop1.IsTouched);
            Assert.IsTrue(prop2.IsTouched);
            Assert.IsTrue(container.IsTouched);
            Assert.IsFalse(prop1.IsDirty);
            Assert.IsTrue(prop2.IsDirty);
            Assert.IsTrue(container.IsDirty);

            container.Reset(ValueResetType.Hard);
            Assert.IsFalse(prop1.IsTouched);
            Assert.IsFalse(prop2.IsTouched);
            Assert.IsFalse(container.IsTouched);
            Assert.IsFalse(prop1.IsDirty);
            Assert.IsFalse(prop2.IsDirty);
            Assert.IsFalse(container.IsDirty);

            prop2.CurrentValue = "second";
            container.Commit();
            Assert.IsFalse(prop1.IsTouched);
            Assert.IsFalse(prop2.IsTouched);
            Assert.IsFalse(container.IsTouched);
            Assert.IsFalse(prop1.IsDirty);
            Assert.IsFalse(prop2.IsDirty);
            Assert.IsFalse(container.IsDirty);
        }

        [Test]
        public void TestReferenceTypes()
        {
            var object1 = new object();
            var object2 = new object();

            var change1 = new ChangeableProperty<object>(object1);
            Assert.AreEqual(object1, change1.CurrentValue);
            Assert.AreEqual(object1, change1.OriginalValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsFalse(change1.IsTouched);

            change1.CurrentValue = object2;
            Assert.AreEqual(object2, change1.CurrentValue);
            Assert.AreEqual(object1, change1.OriginalValue);
            Assert.IsTrue(change1.IsDirty);
            Assert.IsTrue(change1.IsTouched);

            change1.CurrentValue = object1;
            Assert.AreEqual(object1, change1.CurrentValue);
            Assert.AreEqual(object1, change1.OriginalValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsTrue(change1.IsTouched);
        }

        [Test]
        public void TestTouching()
        {
            var change1 = new ChangeableProperty<bool>(true);
            Assert.IsFalse(change1.IsTouched);

            change1.Touch();
            Assert.IsTrue(change1.IsTouched);

            change1.Touch();
            Assert.IsTrue(change1.IsTouched);

            change1.Reset(ValueResetType.Soft);
            Assert.IsTrue(change1.IsTouched);

            change1.Reset(ValueResetType.Hard);
            Assert.IsFalse(change1.IsTouched);
        }

        [Test]
        public void BasicTest()
        {
            bool eventChangingFired;
            bool eventChangedFired;

            EventHandler<ValueChangingEventArgs> handlerChanging = (sender, args) => { eventChangingFired = true; };
            EventHandler<ValueChangedEventArgs> handlerChanged = (sender, args) => { eventChangedFired = true; };

            eventChangingFired = false;
            eventChangedFired = false;
            var change1 = new ChangeableProperty<bool>(true);
            change1.ValueChanging += handlerChanging;
            change1.ValueChanged += handlerChanged;
            Assert.IsTrue(change1.OriginalValue);
            Assert.IsTrue(change1.CurrentValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsFalse(change1.IsTouched);
            Assert.IsFalse(eventChangingFired);
            Assert.IsFalse(eventChangedFired);

            // Changing to the same value should do nothing
            eventChangingFired = false;
            change1.CurrentValue = true;
            Assert.IsTrue(change1.OriginalValue);
            Assert.IsTrue(change1.CurrentValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsFalse(change1.IsTouched);
            Assert.IsFalse(eventChangingFired);
            Assert.IsFalse(eventChangedFired);

            eventChangingFired = false;
            change1.CurrentValue = false;
            Assert.IsTrue(change1.OriginalValue);
            Assert.IsFalse(change1.CurrentValue);
            Assert.IsTrue(change1.IsDirty);
            Assert.IsTrue(change1.IsTouched);
            Assert.IsTrue(eventChangingFired);

            eventChangingFired = false;
            change1.CurrentValue = true;
            Assert.IsTrue(change1.OriginalValue);
            Assert.IsTrue(change1.CurrentValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsTrue(change1.IsTouched);
            Assert.IsTrue(eventChangingFired);
            Assert.IsTrue(eventChangingFired);
        }

        [Test]
        public void TestCancelling()
        {
            EventHandler<ValueChangingEventArgs> cancelHandler = (sender, args) => { args.Cancel = true; };
            var change1 = new ChangeableProperty<bool>(false);

            // Since our handler cancels the change, there should be no difference to the previous result.
            change1.ValueChanging += cancelHandler;
            change1.CurrentValue = true;
            Assert.IsFalse(change1.OriginalValue);
            Assert.IsFalse(change1.CurrentValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsFalse(change1.IsTouched);

            change1.ValueChanging -= cancelHandler;
            change1.CurrentValue = true;
            Assert.IsFalse(change1.OriginalValue);
            Assert.IsTrue(change1.CurrentValue);
            Assert.IsTrue(change1.IsDirty);
            Assert.IsTrue(change1.IsTouched);
        }

        [Test]
        public void TestReset()
        {
            var change1 = new ChangeableProperty<bool>(false);
            change1.CurrentValue = true;
            Assert.IsFalse(change1.OriginalValue, "Original value must not be changed.");
            Assert.IsTrue(change1.CurrentValue);
            Assert.IsTrue(change1.IsDirty);
            Assert.IsTrue(change1.IsTouched);

            change1.Reset();
            Assert.IsFalse(change1.OriginalValue, "Original value must not be changed by reset methods.");
            Assert.IsFalse(change1.CurrentValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsFalse(change1.IsTouched);

            var change2 = new ChangeableProperty<bool>(false);
            change2.Reset(); // calling reset without changing the value
            Assert.IsFalse(change2.OriginalValue);
            Assert.IsFalse(change2.CurrentValue);
            Assert.IsFalse(change2.IsDirty);
            Assert.IsFalse(change2.IsTouched);

            change2.CurrentValue = true;
            change2.Reset(ValueResetType.Soft);
            Assert.IsFalse(change2.OriginalValue, "Original value must not be changed by reset methods.");
            Assert.IsFalse(change2.CurrentValue, "Current value must be reset to the base value after calling reset.");
            Assert.IsFalse(change2.IsDirty, "IsDirty must be False after calling reset.");
            Assert.IsTrue(change2.IsTouched, "When resetting using soft mode, the IsTouched flag must be not changed.");

            change2.CurrentValue = true;
            change2.Reset(ValueResetType.Hard);
            Assert.IsFalse(change2.OriginalValue, "Original value must not be changed by reset methods.");
            Assert.IsFalse(change2.CurrentValue, "Current value must be reset to the base value after calling reset.");
            Assert.IsFalse(change2.IsDirty, "IsDirty must be False after calling reset.");
            Assert.IsFalse(change2.IsTouched, "When resetting using hard mode, the IsTouched flag must be always false.");
        }

        [Test]
        public void TestCommit()
        {
            var change1 = new ChangeableProperty<bool>(false);
            change1.CurrentValue = true;
            Assert.IsFalse(change1.OriginalValue);
            Assert.IsTrue(change1.CurrentValue);
            Assert.IsTrue(change1.IsDirty);
            Assert.IsTrue(change1.IsTouched);

            change1.Commit();
            Assert.IsTrue(change1.OriginalValue);
            Assert.IsTrue(change1.CurrentValue);
            Assert.IsFalse(change1.IsDirty);
            Assert.IsFalse(change1.IsTouched);
        }
    }
}


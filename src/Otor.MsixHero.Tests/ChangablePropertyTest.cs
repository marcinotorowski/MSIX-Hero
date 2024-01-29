// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Linq;
using NUnit.Framework;
using Otor.MsixHero.App.Mvvm.Changeable;

namespace Otor.MsixHero.Tests
{
    [TestFixture()]
    public class ChangablePropertyTest
    {
        [Test]
        public void ValidateObservableCollectionBool()
        {
            var col1 = new[] { true, true, true };
            var changeableCol = new ChangeableCollection<bool>(col1);

            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol.Add(true);
            Assert.That(changeableCol.IsTouched, Is.True);
            Assert.That(changeableCol.IsDirty, Is.True);

            changeableCol.RemoveAt(3);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(2);
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Add(true);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol[0] = false;
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol[0] = true;
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(0);
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Reset(ValueResetType.Soft);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(0);

            changeableCol.Reset(ValueResetType.Hard);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol.Add(true);
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Commit();
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);
        }

        [Test]
        public void ValidateObservableCollectionChangeables()
        {
            var col1 = new[] { "string1", "string2", "string3" };
            var changeableCol = new ChangeableCollection<ChangeableProperty<string>>(col1.Select(c => new ChangeableProperty<string>(c)));

            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol.Add(new ChangeableProperty<string>("string4"));
            Assert.That(changeableCol.IsTouched, Is.True);
            Assert.That(changeableCol.IsDirty, Is.True);

            changeableCol.RemoveAt(3);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(2);
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Add(new ChangeableProperty<string>("string3"));
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Reset(ValueResetType.Hard);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol[0].CurrentValue = "string1a";
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Reset(ValueResetType.Hard);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol[0].CurrentValue = "string1";
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol[0].CurrentValue = "string1a";
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol[0].CurrentValue = "string1";
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol[0].Reset(ValueResetType.Hard);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True, "Resetting a single dirty item should set the dirty flag to true, but touched should remain.");
            
            changeableCol.Reset();
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol[0].CurrentValue = "aaa";
            changeableCol[1].CurrentValue = "bbb";
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);
            changeableCol[0].Reset();
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);
            changeableCol[1].Reset();
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(0);
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Reset(ValueResetType.Soft);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(0);

            changeableCol.Reset(ValueResetType.Hard);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol.Add(new ChangeableProperty<string>("string4"));
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Commit();
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);
        }

        [Test]
        public void ValidateObservableCollectionString()
        {
            var col1 = new[] {"string1", "string2", "string3"};
            var changeableCol = new ChangeableCollection<string>(col1);

            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol.Add("string4");
            Assert.That(changeableCol.IsTouched, Is.True);
            Assert.That(changeableCol.IsDirty, Is.True);

            changeableCol.Remove("string4");
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);
            
            changeableCol.Remove("string3");
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Add("string3");
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol[0] = "string1a";
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol[0] = "string1";
            Assert.That(changeableCol.IsDirty, Is.False);
                Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(0);
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Reset(ValueResetType.Soft);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.RemoveAt(0);

            changeableCol.Reset(ValueResetType.Hard);
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);

            changeableCol.Add("string4");
            Assert.That(changeableCol.IsDirty, Is.True);
            Assert.That(changeableCol.IsTouched, Is.True);

            changeableCol.Commit();
            Assert.That(changeableCol.IsDirty, Is.False);
            Assert.That(changeableCol.IsTouched, Is.False);
        }

        [Test]
        public void ValidatorTest()
        {
            Func<int, string> validator = i => i % 2 != 0 ? "The number must be even" : null;

            var validatedProperty = new ValidatedChangeableProperty<int>("abc", 0, validator);
            Assert.That(validatedProperty.IsValidated, Is.True);
            Assert.That(validatedProperty.IsValid, Is.True);
            Assert.That(validatedProperty.ValidationMessage, Is.Null);

            validatedProperty.CurrentValue = 1;
            Assert.That(validatedProperty.IsValidated, Is.True);
            Assert.That(validatedProperty.IsValid, Is.False);
            Assert.That(validatedProperty.ValidationMessage, Is.EqualTo("abc: The number must be even"));

            validatedProperty.IsValidated = false;
            Assert.That(validatedProperty.IsValidated, Is.False);
            Assert.That(validatedProperty.IsValid, Is.True);
            Assert.That(validatedProperty.ValidationMessage, Is.Null);
        }

        [Test]
        public void ChangeableContainerTest()
        {
            var prop1 = new ChangeableProperty<string>("initial");
            var prop2 = new ChangeableProperty<string>("initial");

            var container = new ChangeableContainer(prop1, prop2);
            Assert.That(prop1.IsTouched, Is.False);
            Assert.That(prop2.IsTouched, Is.False);
            Assert.That(container.IsTouched, Is.False);
            Assert.That(prop1.IsDirty, Is.False);
            Assert.That(prop2.IsDirty, Is.False);
            Assert.That(container.IsDirty, Is.False);

            prop1.CurrentValue = "other";
            Assert.That(prop1.IsTouched, Is.True);
            Assert.That(prop2.IsTouched, Is.False);
            Assert.That(container.IsTouched, Is.True);
            Assert.That(prop1.IsDirty, Is.True);
            Assert.That(prop2.IsDirty, Is.False);
            Assert.That(container.IsDirty, Is.True);

            prop1.CurrentValue = "initial";
            Assert.That(prop1.IsTouched, Is.True);
            Assert.That(prop2.IsTouched, Is.False);
            Assert.That(container.IsTouched, Is.True);
            Assert.That(prop1.IsDirty, Is.False);
            Assert.That(prop2.IsDirty, Is.False);
            Assert.That(container.IsDirty, Is.False);

            prop1.Reset(ValueResetType.Hard);
            Assert.That(prop1.IsTouched, Is.False);
            Assert.That(prop2.IsTouched, Is.False);
            Assert.That(container.IsTouched, Is.False);
            Assert.That(prop1.IsDirty, Is.False);
            Assert.That(prop2.IsDirty, Is.False);
            Assert.That(container.IsDirty, Is.False);

            prop2.CurrentValue = "second";
            Assert.That(prop1.IsTouched, Is.False);
            Assert.That(prop2.IsTouched, Is.True);
            Assert.That(container.IsTouched, Is.True);
            Assert.That(prop1.IsDirty, Is.False);
            Assert.That(prop2.IsDirty, Is.True);
            Assert.That(container.IsDirty, Is.True);

            container.Reset(ValueResetType.Hard);
            Assert.That(prop1.IsTouched, Is.False);
            Assert.That(prop2.IsTouched, Is.False);
            Assert.That(container.IsTouched, Is.False);
            Assert.That(prop1.IsDirty, Is.False);
            Assert.That(prop2.IsDirty, Is.False);
            Assert.That(container.IsDirty, Is.False);

            prop2.CurrentValue = "second";
            container.Commit();
            Assert.That(prop1.IsTouched, Is.False);
            Assert.That(prop2.IsTouched, Is.False);
            Assert.That(container.IsTouched, Is.False);
            Assert.That(prop1.IsDirty, Is.False);
            Assert.That(prop2.IsDirty, Is.False);
            Assert.That(container.IsDirty, Is.False);
        }

        [Test]
        public void TestReferenceTypes()
        {
            var object1 = new object();
            var object2 = new object();

            var change1 = new ChangeableProperty<object>(object1);
            Assert.That(change1.CurrentValue, Is.EqualTo(object1));
            Assert.That(change1.OriginalValue, Is.EqualTo(object1));
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.False);

            change1.CurrentValue = object2;
            Assert.That(change1.CurrentValue, Is.EqualTo(object2));
            Assert.That(change1.OriginalValue, Is.EqualTo(object1));
            Assert.That(change1.IsDirty, Is.True);
            Assert.That(change1.IsTouched, Is.True);

            change1.CurrentValue = object1;
            Assert.That(change1.CurrentValue, Is.EqualTo(object1));
            Assert.That(change1.OriginalValue, Is.EqualTo(object1));
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.True);
        }

        [Test]
        public void TestTouching()
        {
            var change1 = new ChangeableProperty<bool>(true);
            Assert.That(change1.IsTouched, Is.False);

            change1.Touch();
            Assert.That(change1.IsTouched, Is.True);

            change1.Touch();
            Assert.That(change1.IsTouched, Is.True);

            change1.Reset(ValueResetType.Soft);
            Assert.That(change1.IsTouched, Is.True);

            change1.Reset(ValueResetType.Hard);
            Assert.That(change1.IsTouched, Is.False);
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
            Assert.That(change1.OriginalValue, Is.True);
            Assert.That(change1.CurrentValue, Is.True);
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.False);
            Assert.That(eventChangingFired, Is.False);
            Assert.That(eventChangedFired, Is.False);

            // Changing to the same value should do nothing
            eventChangingFired = false;
            change1.CurrentValue = true;
            Assert.That(change1.OriginalValue, Is.True);
            Assert.That(change1.CurrentValue, Is.True);
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.False);
            Assert.That(eventChangingFired, Is.False);
            Assert.That(eventChangedFired, Is.False);

            eventChangingFired = false;
            change1.CurrentValue = false;
            Assert.That(change1.OriginalValue, Is.True);
            Assert.That(change1.CurrentValue, Is.False);
            Assert.That(change1.IsDirty, Is.True);
            Assert.That(change1.IsTouched, Is.True);
            Assert.That(eventChangingFired, Is.True);

            eventChangingFired = false;
            change1.CurrentValue = true;
            Assert.That(change1.OriginalValue, Is.True);
            Assert.That(change1.CurrentValue, Is.True);
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.True);
            Assert.That(eventChangingFired, Is.True);
            Assert.That(eventChangingFired, Is.True);
        }

        [Test]
        public void TestCancelling()
        {
            EventHandler<ValueChangingEventArgs> cancelHandler = (sender, args) => { args.Cancel = true; };
            var change1 = new ChangeableProperty<bool>(false);

            // Since our handler cancels the change, there should be no difference to the previous result.
            change1.ValueChanging += cancelHandler;
            change1.CurrentValue = true;
            Assert.That(change1.OriginalValue, Is.False);
            Assert.That(change1.CurrentValue, Is.False);
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.False);

            change1.ValueChanging -= cancelHandler;
            change1.CurrentValue = true;
            Assert.That(change1.OriginalValue, Is.False);
            Assert.That(change1.CurrentValue, Is.True);
            Assert.That(change1.IsDirty, Is.True);
            Assert.That(change1.IsTouched, Is.True);
        }

        [Test]
        public void TestReset()
        {
            var change1 = new ChangeableProperty<bool>()
            {
                CurrentValue = true
            };

            Assert.That(change1.OriginalValue, Is.False, "Original value must not be changed.");
            Assert.That(change1.CurrentValue, Is.True);
            Assert.That(change1.IsDirty, Is.True);
            Assert.That(change1.IsTouched, Is.True);

            change1.Reset();
            Assert.That(change1.OriginalValue, Is.False, "Original value must not be changed by reset methods.");
            Assert.That(change1.CurrentValue, Is.False);
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.False);

            var change2 = new ChangeableProperty<bool>(false);
            change2.Reset(); // calling reset without changing the value
            Assert.That(change2.OriginalValue, Is.False);
            Assert.That(change2.CurrentValue, Is.False);
            Assert.That(change2.IsDirty, Is.False);
            Assert.That(change2.IsTouched, Is.False);

            change2.CurrentValue = true;
            change2.Reset(ValueResetType.Soft);
            Assert.That(change2.OriginalValue, Is.False, "Original value must not be changed by reset methods.");
            Assert.That(change2.CurrentValue, Is.False, "Current value must be reset to the base value after calling reset.");
            Assert.That(change2.IsDirty, Is.False, "IsDirty must be False after calling reset.");
            Assert.That(change2.IsTouched, Is.True, "When resetting using soft mode, the IsTouched flag must be not changed.");

            change2.CurrentValue = true;
            change2.Reset(ValueResetType.Hard);
            Assert.That(change2.OriginalValue, Is.False, "Original value must not be changed by reset methods.");
            Assert.That(change2.CurrentValue, Is.False, "Current value must be reset to the base value after calling reset.");
            Assert.That(change2.IsDirty, Is.False, "IsDirty must be False after calling reset.");
            Assert.That(change2.IsTouched, Is.False, "When resetting using hard mode, the IsTouched flag must be always false.");
        }

        [Test]
        public void TestCommit()
        {
            var change1 = new ChangeableProperty<bool>
            {
                CurrentValue = true
            };

            Assert.That(change1.OriginalValue, Is.False);
            Assert.That(change1.CurrentValue, Is.True);
            Assert.That(change1.IsDirty, Is.True);
            Assert.That(change1.IsTouched, Is.True);

            change1.Commit();
            Assert.That(change1.OriginalValue, Is.True);
            Assert.That(change1.CurrentValue, Is.True);
            Assert.That(change1.IsDirty, Is.False);
            Assert.That(change1.IsTouched, Is.False);
        }
    }
}


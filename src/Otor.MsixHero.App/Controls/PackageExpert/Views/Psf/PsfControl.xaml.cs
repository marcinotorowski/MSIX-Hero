using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.Appx.Psf.Entities.Interpreter;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views.Psf
{
    /// <summary>
    /// Interaction logic for PsfControl.xaml
    /// </summary>
    public partial class PsfControl : UserControl
    {
        public static readonly DependencyProperty PsfProperty = DependencyProperty.Register("Psf", typeof(InterpretedPsf), typeof(PsfControl), new PropertyMetadata(null, OnPsfChanged));

        private static readonly DependencyPropertyKey HasTraceRulesPropertyKey = DependencyProperty.RegisterReadOnly("HasTraceRules", typeof(bool), typeof(PsfControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HasTraceRulesProperty = HasTraceRulesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey HasRedirectionRulesPropertyKey = DependencyProperty.RegisterReadOnly("HasRedirectionRules", typeof(bool), typeof(PsfControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HasRedirectionRulesProperty = HasRedirectionRulesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey HasElectronRulesPropertyKey = DependencyProperty.RegisterReadOnly("HasElectronRules", typeof(bool), typeof(PsfControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HasElectronRulesProperty = HasElectronRulesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey HasCustomRulesPropertyKey = DependencyProperty.RegisterReadOnly("HasCustomRules", typeof(bool), typeof(PsfControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HasCustomRulesProperty = HasCustomRulesPropertyKey.DependencyProperty;

        public PsfControl()
        {
            InitializeComponent();
        }
        
        public InterpretedPsf Psf
        {
            get => (InterpretedPsf)GetValue(PsfProperty);
            set => SetValue(PsfProperty, value);
        }

        public bool HasTraceRules
        {
            get => (bool)GetValue(HasTraceRulesProperty);
            private set => SetValue(HasTraceRulesPropertyKey, value);
        }

        public bool HasElectronRules
        {
            get => (bool)GetValue(HasElectronRulesProperty);
            private set => SetValue(HasElectronRulesPropertyKey, value);
        }

        public bool HasCustomRules
        {
            get => (bool)GetValue(HasCustomRulesProperty);
            private set => SetValue(HasCustomRulesPropertyKey, value);
        }

        public bool HasRedirectionRules
        {
            get => (bool)GetValue(HasRedirectionRulesProperty);
            private set => SetValue(HasRedirectionRulesPropertyKey, value);
        }

        private static void OnPsfChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (PsfControl)d;
            var newValue = (InterpretedPsf)e.NewValue;

            that.HasCustomRules = newValue?.CustomRules?.Any() == true;
            that.HasElectronRules = newValue?.ElectronRules?.Any() == true;
            that.HasRedirectionRules = newValue?.RedirectionRules?.Any() == true;
            that.HasTraceRules = newValue?.TraceRules?.Any() == true;
        }
    }
}

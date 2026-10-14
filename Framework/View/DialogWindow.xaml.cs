using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;
using System.Collections.Generic;

using Framework.ViewModel;

namespace Framework.View
{
    public partial class DialogWindow : Window
    {
        private readonly DialogVM _dialogVM;
        private bool _isFirstBoxFocused;
        private bool _isAccepted;
        private readonly Func<List<double>, string> _validateValues;

        public DialogWindow(MainVM mainVM, List<string> labels, Func<List<double>, string> validateValues = null)
        {
            InitializeComponent();
            _validateValues = validateValues;
            UiHelper.SetMainWindowAsOwner(this);

            _dialogVM = new DialogVM();

            _dialogVM.Theme = mainVM.Theme;
            _dialogVM.CreateParameters(labels);

            DataContext = _dialogVM;
            Height = System.Math.Max(200, _dialogVM.Parameters.Count * 74 + 134);
        }

        public List<double> GetValues()
        {
            return _isAccepted ? _dialogVM.GetValues() : null;
        }

        #region Event handlers
        private void ParameterBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (_isFirstBoxFocused)
                return;

            _isFirstBoxFocused = true;
            ((TextBox)sender).Focus();
        }

        private void DoneClick(object sender, RoutedEventArgs e)
        {
            int invalidIndex = _dialogVM.FindInvalidParameter();
            if (invalidIndex >= 0)
            {
                MessageBox.Show(this, $"Please enter a number for \"{_dialogVM.Parameters[invalidIndex].ParamText.Trim()}\".");

                var parameterBox = FindParameterBox(invalidIndex);
                parameterBox?.Focus();
                parameterBox?.SelectAll();
                return;
            }

            string validationError = _validateValues?.Invoke(_dialogVM.GetValues());
            if (validationError != null)
            {
                MessageBox.Show(this, validationError);
                var parameterBox = FindParameterBox(0);
                parameterBox?.Focus();
                parameterBox?.SelectAll();
                return;
            }

            _isAccepted = true;
            Close();
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _dialogVM.Cleanup();
        }
        #endregion

        #region Helpers
        private TextBox FindParameterBox(int index)
        {
            var container = parameterList.ContainerFromIndex(index);
            return container == null ? null : FindDescendant<TextBox>(container);
        }

        private static TElement FindDescendant<TElement>(Avalonia.Visual parent)
            where TElement : Avalonia.Visual
        {
            return parent.GetVisualDescendants().OfType<TElement>().FirstOrDefault();
        }
        #endregion
        private void CancelClick(object sender, RoutedEventArgs e) => Close();
    }
}

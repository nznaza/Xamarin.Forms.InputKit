using Plugin.InputKit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Plugin.InputKit.Shared.Controls
{
    public class AutoCompleteEntry : AdvancedEntry
    {
        private AutoCompleteView txtInput;
        public AutoCompleteEntry()
        {

        }

        public IEnumerable<object> ItemsSource { get => txtInput.ItemsSource; set => txtInput.ItemsSource = value; }

        #region BindableProperties
        public static BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(AutoCompleteView.ItemsSource),
                typeof(IEnumerable<object>),
                typeof(AutoCompleteEntry),
                propertyChanged: (bo, nv, ov) => (bo as AutoCompleteEntry).txtInput.ItemsSource = nv as IEnumerable<object>);
        #endregion

        public object SelectedItem => txtInput.SelectedItem; 

        private protected override Entry GetInputEntry()
        {
            txtInput = new AutoCompleteView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
            };
            txtInput.SortingAlgorithm = (text, options) => 
                (options ?? new List<object>())
                .Where(x => x.ToString().ToLowerInvariant().Contains(text.ToLowerInvariant()))
                .OrderBy(o => o.ToString().StartsWith(text, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
                .ThenBy(t => t)
                .ToList();
            return txtInput ;
        }
    }
}

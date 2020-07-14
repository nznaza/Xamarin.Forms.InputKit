using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Plugin.InputKit.Platforms.iOS.Controls;
using UIKit;
using Xamarin.Forms;

namespace Plugin.InputKit.Platforms.iOS.Helpers
{
    public abstract class AutoCompleteViewSource : UITableViewSource
    {
        public ICollection<object> Suggestions { get; set; } = new List<object>();

        public AutoCompleteTextField AutoCompleteTextField { get; set; }

        public abstract void UpdateSuggestions(ICollection<object> suggestions);

        public abstract override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath);

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Suggestions.Count;
        }

        public event EventHandler<SelectedItemChangedEventArgs> Selected;

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            AutoCompleteTextField.AutoCompleteTableView.Hidden = true;
            if (indexPath.Row < Suggestions.Count)
                AutoCompleteTextField.Text = Suggestions.ElementAt(indexPath.Row).ToString();
            AutoCompleteTextField.ResignFirstResponder();
            var item = Suggestions.ToList()[(int)indexPath.Item];
            Selected?.Invoke(tableView, new SelectedItemChangedEventArgs(item));
            // don't call base.RowSelected
        }
    }
}

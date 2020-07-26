using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Application = Android.App.Application;
using Color = Xamarin.Forms.Color;
using AColor = Android.Graphics.Color;
using FormsAppCompat = Xamarin.Forms.Platform.Android.AppCompat;
using Plugin.InputKit.Shared.Controls;
using Plugin.InputKit.Platforms.Droid;
using System.Collections.Specialized;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextField;

[assembly: ExportRenderer(typeof(AutoCompleteView), typeof(AutoCompleteViewRenderer))]
namespace Plugin.InputKit.Platforms.Droid
{
    public class AutoCompleteViewRenderer : FormsAppCompat.ViewRenderer<AutoCompleteView, TextInputLayout>
    {
        public AutoCompleteViewRenderer(Context context) : base(context)
        {
        }

        private AppCompatAutoCompleteTextView NativeControl => Control?.EditText as AppCompatAutoCompleteTextView;

        protected override TextInputLayout CreateNativeControl()
        {

            var textInputLayout = new TextInputLayout(Context);
            var autoComplete = new AppCompatAutoCompleteTextView(Context)
            {
                BackgroundTintList = ColorStateList.ValueOf(GetPlaceholderColor()),
                Text = Element?.Text,
                Hint = Element?.Placeholder,
            };
            textInputLayout.Hint = " ";

            //GradientDrawable gd = new GradientDrawable();
            //gd.SetColor(global::Android.Graphics.Color.Transparent);
            //autoComplete.SetBackground(gd);
            if (Element != null)
            {
                autoComplete.SetHintTextColor(Element.PlaceholderColor.ToAndroid());
                autoComplete.SetTextColor(Element.TextColor.ToAndroid());
            }
            autoComplete.SetMaxLines(1);
            autoComplete.InputType = InputTypes.ClassText;
            textInputLayout.AddView(autoComplete);
            return textInputLayout;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AutoCompleteView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
            {
                // unsubscribe
                NativeControl.ItemClick -= AutoCompleteOnItemSelected;
                var elm = e.OldElement;
                elm.CollectionChanged -= ItemsSourceCollectionChanged;
            }

            if (e.NewElement != null)
            {
                SetNativeControl(CreateNativeControl());
                // subscribe
                SetItemsSource();
                SetThreshold();
                KillPassword();
                //NativeControl.TextChanged += NativeControl_TextChanged;
                NativeControl.ItemClick += AutoCompleteOnItemSelected;

                var elm = e.NewElement;
                elm.CollectionChanged += ItemsSourceCollectionChanged;
            }
        }

        private void Element_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (Element.Text != Control.EditText.Text)
                Control.EditText.Text = Element.Text;
        }

        private void EditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (Element.Text != Control.EditText.Text)
                Element.Text = Control.EditText.Text;
        }

        private void NativeControl_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            //Element.Focus();
            //NativeControl.ShowDropDown();
            //Element.RaiseTextChanged(NativeControl.Text);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == Entry.TextProperty.PropertyName)
            {
                NativeControl.SetText(Element.Text, false);
                Element.OnItemSelectedInternal(Element, new SelectedItemChangedEventArgs(null, -1));
                if (!string.IsNullOrWhiteSpace(Element.Text) && Element.Text.Length >= Element.Threshold)
                {
                    NativeControl.ShowDropDown();
                }
                else
                    NativeControl.DismissDropDown();

            }
            if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
                KillPassword();
            if (e.PropertyName == AutoCompleteView.ItemsSourceProperty.PropertyName)
                SetItemsSource();
            else if (e.PropertyName == AutoCompleteView.ThresholdProperty.PropertyName)
                SetThreshold();
        }

        private void AutoCompleteOnItemSelected(object sender, AdapterView.ItemClickEventArgs args)
        {
            var view = (AutoCompleteTextView)sender;
            Java.Lang.Object obj = view.Adapter.GetItem(args.Position);
            var selectedItemArgs = new SelectedItemChangedEventArgs(obj.GetType().GetProperty("Instance").GetValue(obj, null), args.Position);
            var element = (AutoCompleteView)Element;
            Element.Text = NativeControl.Text;
            //Element.RaiseTextChanged(NativeControl.Text);
            element.OnItemSelectedInternal(Element, selectedItemArgs);
        }

        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var element = (AutoCompleteView)Element;
            ResetAdapter(element);
        }

        private void KillPassword()
        {
            if (Element.IsPassword)
                throw new NotImplementedException("Cannot set IsPassword on a AutoComplete");
        }

        private void ResetAdapter(AutoCompleteView element)
        {
            var adapter = new BoxArrayAdapter(Context,
                Android.Resource.Layout.SimpleDropDownItem1Line,
                element.ItemsSource.ToList(),
                element.SortingAlgorithm);
            NativeControl.Adapter = adapter;

            adapter.NotifyDataSetChanged();
        }

        private void SetItemsSource()
        {
            var element = (AutoCompleteView)Element;
            if (element.ItemsSource == null) return;

            ResetAdapter(element);
        }

        private void SetThreshold()
        {
            var element = (AutoCompleteView)Element;
            NativeControl.Threshold = element.Threshold;
        }

        #region Section 2
        protected AColor GetPlaceholderColor() => Element.PlaceholderColor.ToAndroid(Color.FromHex("#80000000"));
        #endregion
    }

    internal class BoxArrayAdapter : ArrayAdapter
    {
        private readonly IList<object> _objects;
        private readonly Func<string, ICollection<object>, ICollection<object>> _sortingAlgorithm;

        public BoxArrayAdapter(
            Context context,
            int textViewResourceId,
            List<object> objects,
            Func<string, ICollection<object>, ICollection<object>> sortingAlgorithm) : base(context, textViewResourceId, objects)
        {
            _objects = objects;
            _sortingAlgorithm = sortingAlgorithm;
        }

        public override Filter Filter
        {
            get
            {
                return new CustomFilter(_sortingAlgorithm) { Adapter = this, Originals = _objects };
            }
        }
    }

    internal class CustomFilter : Filter
    {
        private readonly Func<string, ICollection<object>, ICollection<object>> _sortingAlgorithm;

        public CustomFilter(Func<string, ICollection<object>, ICollection<object>> sortingAlgorithm)
        {
            _sortingAlgorithm = sortingAlgorithm;
        }

        public BoxArrayAdapter Adapter { private get; set; }
        public IList<object> Originals { get; set; }

        protected override FilterResults PerformFiltering(ICharSequence constraint)
        {
            var results = new FilterResults();
            if (constraint == null || constraint.Length() == 0)
            {
                results.Values = new Java.Util.ArrayList(Originals.ToList());
                results.Count = Originals.Count;
            }
            else
            {
                var values = new Java.Util.ArrayList();
                var sorted = _sortingAlgorithm(constraint.ToString(), Originals).ToList();

                for (var index = 0; index < sorted.Count; index++)
                {
                    var item = sorted[index];
                    values.Add((Java.Lang.Object)item);
                }

                results.Values = values;
                results.Count = sorted.Count;
            }

            return results;
        }

        protected override void PublishResults(ICharSequence constraint, FilterResults results)
        {
            if (results.Count == 0)
                Adapter.NotifyDataSetInvalidated();
            else
            {
                Adapter.Clear();
                var vals = (Java.Util.ArrayList)results.Values;
                foreach (var val in vals.ToArray())
                {
                    Adapter.Add(val);
                }
                Adapter.NotifyDataSetChanged();
            }
        }
    }
}

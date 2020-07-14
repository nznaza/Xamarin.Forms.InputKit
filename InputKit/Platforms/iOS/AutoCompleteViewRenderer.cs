﻿using CoreGraphics;
using Foundation;
using Plugin.InputKit.Platforms.iOS;
using Plugin.InputKit.Platforms.iOS.Controls;
using Plugin.InputKit.Platforms.iOS.Helpers;
using Plugin.InputKit.Shared.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AutoCompleteView), typeof(AutoCompleteViewRenderer))]
namespace Plugin.InputKit.Platforms.iOS
{
    public class AutoCompleteViewRenderer : ViewRenderer<AutoCompleteView, UITextField>
    {
        private AutoCompleteTextField NativeControl { get => Control as AutoCompleteTextField; }
        private AutoCompleteView AutoCompleteEntry => (AutoCompleteView)Element;

        public AutoCompleteViewRenderer()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            Frame = new RectangleF(0, 20, 320, 40);
        }

        protected override UITextField CreateNativeControl()
        {
            var view = new AutoCompleteTextField
            {
                AutoCompleteViewSource = new AutoCompleteDefaultDataSource(),
                SortingAlgorithm = Element.SortingAlgorithm
            };

            if (Element != null)
            {
                view.AttributedPlaceholder = new NSAttributedString(Element.Placeholder ?? "", null, Element.PlaceholderColor.ToUIColor());
                view.Text = Element.Text;
                view.TextColor = Element.TextColor.ToUIColor();
            }
            view.AutoCompleteViewSource.Selected += AutoCompleteViewSourceOnSelected;
            return view;
        }
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            var scrollView = GetParentScrollView(Control);
            var ctrl = UIApplication.SharedApplication.GetTopViewController();

            var relativePosition = UIApplication.SharedApplication.KeyWindow;
            var relativeFrame = NativeControl.Superview.ConvertRectToView(NativeControl.Frame, relativePosition);
            NativeControl.Draw(ctrl, Layer, scrollView, relativeFrame.Y);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AutoCompleteView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
            {
                // unsubscribe
                if (NativeControl?.AutoCompleteViewSource != null)
                    NativeControl.AutoCompleteViewSource.Selected -= AutoCompleteViewSourceOnSelected;
                var elm = (AutoCompleteView)e.OldElement;
                elm.CollectionChanged -= ItemsSourceCollectionChanged;
            }

            if (e.NewElement != null)
            {
                SetNativeControl(CreateNativeControl());
                SetItemsSource();
                SetThreshold();
                KillPassword();
                //NativeControl.EditingChanged += (s, args) => Element.RaiseTextChanged(NativeControl.Text);

                var elm = (AutoCompleteView)e.NewElement;
                elm.CollectionChanged += ItemsSourceCollectionChanged;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == Entry.TextProperty.PropertyName)
            {
                if(NativeControl.Text != Element.Text)
                {
                    NativeControl.Text = Element.Text;
                    Element.OnItemSelectedInternal(Element, new SelectedItemChangedEventArgs(null, -1));
                    if (!string.IsNullOrWhiteSpace(Element.Text) && Element.Text.Length >= Element.Threshold)
                    {
                        NativeControl.ShowAutoCompleteView();
                        NativeControl.UpdateTableViewData();
                    }
                    else
                    {
                        NativeControl.HideAutoCompleteView();
                    }
                }
                else
                {
                    NativeControl.HideAutoCompleteView();
                }

            }
            if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
                KillPassword();
            if (e.PropertyName == AutoCompleteView.ItemsSourceProperty.PropertyName)
                SetItemsSource();
            else if (e.PropertyName == AutoCompleteView.ThresholdProperty.PropertyName)
                SetThreshold();
        }

        private void SetThreshold()
        {
            NativeControl.Threshold = AutoCompleteEntry.Threshold;
        }

        private void SetItemsSource()
        {
            if (AutoCompleteEntry.ItemsSource != null)
            {
                var items = AutoCompleteEntry.ItemsSource.ToList();
                NativeControl.UpdateItems(items);
            }
        }

        private void KillPassword()
        {
            if (Element.IsPassword)
                throw new NotImplementedException("Cannot set IsPassword on a AutoCompleteEntry");
        }

        private void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            SetItemsSource();
        }

        private static UIScrollView GetParentScrollView(UIView element)
        {
            if (element.Superview == null) return null;
            var scrollView = element.Superview as UIScrollView;
            return scrollView ?? GetParentScrollView(element.Superview);
        }

        private void AutoCompleteViewSourceOnSelected(object sender, SelectedItemChangedEventArgs args)
        {
            Element.Text = NativeControl.Text;
            AutoCompleteEntry.OnItemSelectedInternal(Element, args);
        }
    }
}

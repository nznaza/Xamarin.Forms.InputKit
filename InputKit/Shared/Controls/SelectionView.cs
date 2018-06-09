﻿using Plugin.InputKit.Shared.Abstraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Plugin.InputKit.Shared.Controls
{
    public class SelectionView : Grid
    {
        private IList _itemSource;
        private SelectionType _selectionType = SelectionType.Button;
        private IList _disabledSource;
        private int _columnNumber = 2;
        private Color _color;

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SelectionView()
        {
            this.RowSpacing = 0;
            this.ColumnSpacing = 0;
            //this.ChildAdded += SelectionView_ChildAdded;
            //this.ChildRemoved += SelectionView_ChildRemoved;
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Selection Type, More types will be added later
        /// </summary>
        public SelectionType SelectionType { get => _selectionType; set { _selectionType = value; UpdateView(); } }
        ///----------------------------------------------------------
        /// <summary>
        /// Added later
        /// </summary>
        public string IsDisabledPropertyName { get; set; }
        ///----------------------------------------------------------
        /// <summary>
        /// Column of this view
        /// </summary>
        public int ColumnNumber { get => _columnNumber; set { _columnNumber = value; UpdateView(); } }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Disables these options. They can not be choosen
        /// </summary>
        public IList DisabledSource { get => _disabledSource; set { _disabledSource = value; UpdateView(); } }

        [Obsolete("Use ItemsSource instead")]
        public IList ItemSource
        {
            get => ItemsSource;
            set => ItemsSource = value;
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Color of selections
        /// </summary>
        public Color Color { get => _color; set { _color = value; UpdateColor(); OnPropertyChanged(); } }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Items Source of selections
        /// </summary>
        public IList ItemsSource
        {
            get => _itemSource;
            set
            {
                _itemSource = value;
                UpdateEvents(value);
                UpdateView();
            }
        }
        /// <summary>
        /// Sets or Gets SelectedItem of SelectionView
        /// </summary>
        public object SelectedItem
        {
            get
            {

                foreach (var item in this.Children)
                    if (item is ISelection && (item as ISelection).IsSelected)
                        return (item as ISelection).Value;
                return null;
            }
            set
            {

                foreach (var item in this.Children)
                    if (item is ISelection && !(item as ISelection).IsDisabled)
                        (item as ISelection).IsSelected = (item as ISelection).Value == value;
            }
        }
        /// <summary>
        ///Selected Items for the multiple selections, 
        /// </summary>
        public IList SelectedItems
        {
            get
            {
                return this.Children.Where(w => (w is ISelection) && (w as ISelection).IsSelected)?.ToList();
            }
            set
            {
                foreach (var item in this.Children)
                    if (item is ISelection)
                        (item as ISelection).IsSelected = value.Contains((item as ISelection).Value);
            }
        }
        private void UpdateEvents(IList value)
        {
            if (value is INotifyCollectionChanged)
            {
                (value as INotifyCollectionChanged).CollectionChanged -= MultiSelectionView_CollectionChanged;
                (value as INotifyCollectionChanged).CollectionChanged += MultiSelectionView_CollectionChanged;
            }
        }
        private void MultiSelectionView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateView();
        }
        private void UpdateView()
        {
            try
            {
                this.Children.Clear();
                SetValue(SelectedItemProperty, null);
                foreach (var item in ItemsSource)
                {
                    var _View = GetInstance(item);
                    (_View as ISelection).Clicked -= Btn_Clicked;
                    (_View as ISelection).Clicked += Btn_Clicked;

                    if (!String.IsNullOrEmpty(IsDisabledPropertyName)) //Sets if property Disabled
                        (_View as ISelection).IsDisabled = Convert.ToBoolean(item.GetType().GetProperty(IsDisabledPropertyName)?.GetValue(item) ?? false);
                    if (DisabledSource?.Contains(item) ?? false)
                        (_View as ISelection).IsDisabled = true;


                    this.Children.Add(_View, this.Children.Count % ColumnNumber, this.Children.Count / ColumnNumber);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
        private void UpdateColor()
        {
            foreach (var item in this.Children)
            {
                SetInstanceColor(item, this.Color);
            }
        }
        private void Btn_Clicked(object sender, EventArgs e)
        {
            SelectedItem = (sender as ISelection).Value;
            SetValue(SelectedItemProperty, SelectedItem);
        }

        private View GetInstance(object obj)
        {
            switch (SelectionType)
            {
                case SelectionType.Button:
                    return new SelectableButton(obj, this.Color);
                case SelectionType.RadioButton:
                    return new SelectableRadioButton(obj, this.Color);
                case SelectionType.CheckBox:
                    return new SelectableCheckBox(obj, this.Color);

            }
            return null;
        }
        private void SetInstanceColor(View view, Color color)
        {
            switch (SelectionType)
            {
                case SelectionType.Button:
                    {
                        if (view is Button)
                            (view as Button).BackgroundColor = color;
                    }
                    break;
                case SelectionType.RadioButton:
                    {
                        if (view is SelectableRadioButton)
                            (view as SelectableRadioButton).Color = color;
                    }
                    break;
                case SelectionType.CheckBox:
                    {
                        if (view is SelectableCheckBox)
                            (view as SelectableCheckBox).Color = color;
                    }
                    break;
            }
        }


        #region BindableProperties
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(SelectionView), null, propertyChanged: (bo, ov, nv) => (bo as SelectionView).ItemsSource = (IList)nv);
        [Obsolete("This is obsolete, use ItemSource instead")]
        public static readonly BindableProperty ItemSourceProperty = BindableProperty.Create(nameof(ItemSource), typeof(IList), typeof(SelectionView), null, propertyChanged: (bo, ov, nv) => (bo as SelectionView).ItemsSource = (IList)nv);
        public static readonly BindableProperty DisabledSourceProperty = BindableProperty.Create(nameof(DisabledSource), typeof(IList), typeof(SelectionView), null, propertyChanged: (bo, ov, nv) => (bo as SelectionView).DisabledSource = (IList)nv);
        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(SelectionView), null, BindingMode.TwoWay, propertyChanged: (bo, ov, nv) => (bo as SelectionView).SelectedItem = nv);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
    /// <summary>
    /// Types of selectionlist
    /// </summary>
    public enum SelectionType
    {
        Button,
        RadioButton,
        CheckBox,
    }




    public class SelectableButton : Button, ISelection
    {
        private bool _isSelected = false;
        private object _value;
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Default constructor
        /// </summary>
        public SelectableButton()
        {
            this.BorderRadius = 20;
            this.Margin = new Thickness(20, 5);
            UpdateColors();
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Generates with its value
        /// </summary>
        /// <param name="value">Value to keep</param>
        public SelectableButton(object value) : this()
        {
            this.Value = value;
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Colored Constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="backColor"></param>
        public SelectableButton(object value, Color backColor) : this(value)
        {
            this.BackgroundColor = backColor;
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This button is selected or not
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; UpdateColors(); }
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Updates colors, Triggered when color property changed
        /// </summary>
        private void UpdateColors()
        {
            if (IsSelected)
            {
                this.BackgroundColor = Color.Accent;
                this.TextColor = Color.WhiteSmoke;
            }
            else
            {
                this.BackgroundColor = (Color)Button.BackgroundColorProperty.DefaultValue;
                this.TextColor = (Color)Button.TextColorProperty.DefaultValue;
            }
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Value is stored on this control
        /// </summary>
        public object Value { get => _value; set { _value = value; this.Text = value?.ToString(); } }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This button is disabled or not. Disabled buttons(if it's true) can not be choosen.
        /// </summary>
        public bool IsDisabled { get; set; } = false;
    }
    /// <summary>
    /// A Radio Button which ISelection Implemented
    /// </summary>
    public class SelectableRadioButton : RadioButton, ISelection
    {
        private bool _isDisabled;
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SelectableRadioButton(){}
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Constructor with value
        /// </summary>
        /// <param name="value">Value to keep</param>
        public SelectableRadioButton(object value)
        {
            this.Value = value;
            this.Text = value?.ToString();
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Colored Constructor
        /// </summary>
        public SelectableRadioButton(object value, Color color) : this(value)
        {
            this.Color = color;
        }
        ///-----------------------------------------------------------------------------
        /// <summary>
        /// ISelection interface property
        /// </summary>
        public bool IsSelected { get => this.IsChecked; set => this.IsChecked = value; }
    }

    /// <summary>
    /// A CheckBox which ISelection Implemented
    /// </summary>
    public class SelectableCheckBox : CheckBox,ISelection
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SelectableCheckBox()
        {
            this.Type = CheckType.Check;
            this.CheckChanged += (s, e) => this.Clicked?.Invoke(s, e);
        }
        /// <summary>
        /// Constructor with Value
        /// </summary>
        /// <param name="value">Parameter too keep</param>
        public SelectableCheckBox(object value) : this()
        {
            this.Value = value;
            this.Text = value?.ToString();
        }
        /// <summary>
        /// Constructor with Value
        /// </summary>
        /// <param name="value">Parameter too keep</param>
        /// <param name="color">Color of control</param>
        public SelectableCheckBox(object value,Color color) : this(value)
        {
            this.Color = color;
        }
        /// <summary>
        /// Capsulated IsChecked
        /// </summary>
        public bool IsSelected { get => this.IsChecked; set => this.IsChecked = value; }
        /// <summary>
        /// Parameter to keep
        /// </summary>
        public object Value { get; set; }
        public event EventHandler Clicked;
    }
}

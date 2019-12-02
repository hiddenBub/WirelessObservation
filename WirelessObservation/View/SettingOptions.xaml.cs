using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WirelessObservation.Entity;

namespace WirelessObservation.View
{

    /// <summary>
    /// Logic of SettingOptions.xaml
    /// </summary>
    public partial class SettingOptions : Window
    {
        private Setting setting = new Setting();

        private List<OptionItem> settingOption = new List<OptionItem>();

        private Button selected = null;

        private Color c = Color.FromRgb(0xf5, 0xf5, 0xf5);

        private Thickness selectBorder = new Thickness(0, 0, 3, 0);

        private Thickness normalBorder = new Thickness(0, 0, 0, 0);

        /// <summary>
        /// Change the setting options except systemd
        /// </summary>
        public SettingOptions()
        {
            InitializeComponent();
            
            PropertyInfo[] pis = typeof(Setting).GetProperties();
            setting = Vendor.SettingHelper.setting;
            string firstName = "";
            // initialize all components
            for (int i = 0; i < pis.Length; i++)
            {
                string className = pis[i].ToString();
                string[] splited = className.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string settingName = splited.Last();
                if (settingName != "Systemd")
                {
                    var settingItem = setting.GetType().GetProperty(settingName).GetValue(setting, null);
                    settingOption.Add((OptionItem)settingItem);
                    if (i == 0)
                    {
                        firstName = settingName;
                    }
                }
                
            }
            // resort all options
            settingOption.Sort();
            // create left
            CreateSettingTitle();
            // create right
            var firstItem = setting.GetType().GetProperty(firstName).GetValue(setting, null);
            OptionItem[] optionItem = firstItem.GetType().GetProperty("OptionItems").GetValue(firstItem, null) as OptionItem[];
            CreateOption(optionItem);
        }

        /// <summary>
        /// Create left side title 
        /// </summary>
        /// <param name="name"></param>
        private void CreateSettingTitle()
        {
            int i = 0;
            
            foreach (OptionItem oi in settingOption)
            {
                Thickness thickness = i == 0 ? selectBorder : normalBorder;
                if (oi.IsShow)
                {
                    
                    var settingAlias = oi.Alias;
                    var settingName = oi.Name;
                    // set setting title border
                    Border border = new Border
                    {
                        Name = settingName.ToString() + "TBBD",
                        BorderBrush = new SolidColorBrush(Colors.SteelBlue),
                        BorderThickness = thickness,
                        Height = 20,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    Button button = new Button
                    {
                        Name = settingName.ToString() + "TB",
                        Content = settingAlias.ToString(),
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Background = new SolidColorBrush(c),
                        BorderBrush = new SolidColorBrush(Colors.DarkSlateGray),
                        BorderThickness = normalBorder,
                    };
                    if (i == 0)
                    {
                        selected = button;
                    }
                    border.Child = button;
                    SettingTitle.Children.Add(border);
                    button.Click += new RoutedEventHandler(SetOption_Click);
                }
                i++;
            }
            
        }

        /// <summary>
        /// Click action of left bar 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetOption_Click(Object sender, EventArgs e)
        {
            // listen the click event
            Button btn = (Button)sender;

            Border parent = (Border) btn.Parent;
            // loop to remove selected mark
            foreach (Border b in SettingTitle.Children)
            {
                b.BorderThickness = normalBorder;
            }
            
            
            // add selected mark
            parent.BorderThickness = selectBorder;

            // check button selected or not
            if (selected == null || !selected.Equals(btn))
            {
                string name = btn.Name.ToString();
                // get name of setting entity
                string settingName = name.Replace("TB", "");
                var settingItem = setting.GetType().GetProperty(settingName).GetValue(setting, null);
                OptionItem[] optionItem = settingItem.GetType().GetProperty("OptionItems").GetValue(settingItem, null) as OptionItem[];
                CreateOption(optionItem);
                selected = btn;
            }
            
        }

        /// <summary>
        /// Save all options into local Setting instance
        /// </summary>
        private void SaveOption()
        {
            string settingName = selected.Name.Replace("TB", "");
            var settingItem = setting.GetType().GetProperty(settingName).GetValue(setting, null);
            Type type = Type.GetType(new Setting().GetType().Namespace + "." + settingName, true, true);
            
            // get variables of Setting class
            PropertyInfo[] pis = type.GetProperties();
            // transfer the propertise to string 
            List<string> pisList = new List<string>();
            foreach (PropertyInfo property in pis)
            {
                pisList.Add(property.ToString());
            }
            // get variable of sub-class of accessed
            PropertyInfo[] item = typeof(OptionItem).GetProperties();
            List<string> itemList = new List<string>();
            foreach (PropertyInfo property in item)
            {
                itemList.Add(property.ToString());
            }
            // used variable of system
            string[] except = pisList.Except(itemList).ToArray();
            
            // loop the option of setting and set value into local setting instance
            for (int i = 0; i < except.Length; i++)
            {
                // get option name
                string className = except[i].ToString();
                string[] splited = className.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string optionName = splited.Last();
                if (optionName != "OptionItems")
                {
                    string val = "";
                    // get 
                    StackPanel sp = ValueSP.FindName(optionName + "SP") as StackPanel;
                    
                    if (sp == null) continue;
                    int capacity = sp.Children.Count;


                    var childNode = sp.Children[0];
                    string childType = childNode.GetType().Name;
                    if (childType == "TextBox")
                    {
                        TextBox textBox = childNode as TextBox;
                        Type t = settingItem.GetType().GetProperty(optionName).GetValue(settingItem, null).GetType();
                        settingItem.GetType().GetProperty(optionName).SetValue(settingItem, Convert.ChangeType(textBox.Text.ToString(), t));
                    }
                    else if (childType == "ComboBox")
                    {
                        ComboBox comboBox = childNode as ComboBox;
                        Type t = settingItem.GetType().GetProperty(optionName).GetValue(settingItem, null).GetType();
                        settingItem.GetType().GetProperty(optionName).SetValue(settingItem, Convert.ChangeType(comboBox.SelectedValue.ToString(), t));
                        
                    }
                    else if (childType == "RadioButton")
                    {
                        List<string> list = new List<string>();
                        for (int j = 0; j < capacity; j++)
                        {
                            RadioButton radioButton = sp.Children[j] as RadioButton;
                            if ((bool)radioButton.IsChecked) list.Add(radioButton.Content.ToString());
                        }
                        Type t = settingItem.GetType().GetProperty(optionName).GetValue(settingItem, null).GetType();
                        settingItem.GetType().GetProperty(optionName).SetValue(settingItem, Convert.ChangeType(string.Join(",", list), t));
                    }
                    else if (childType == "CheckBox")
                    {
                        List<string> list = new List<string>();
                        for (int j = 0; j < capacity; j++)
                        {
                            CheckBox radioButton = sp.Children[j] as CheckBox;
                            if ((bool)radioButton.IsChecked && radioButton.Content.ToString() != "全选/全不选") list.Add(radioButton.Content.ToString());
                        }
                        settingItem.GetType().GetProperty(optionName).SetValue(settingItem, string.Join(",", list));
                    }
                    //var optionItem = settingItem.GetType().GetProperty(optionName).GetValue(settingItem, null);
                    //string nodetype = childNode.GetType().ToString();
                    //settingOption.Add((OptionItem)settingItem);


                }

            }
            //settingName s = temp as settingName;
        }

        /// <summary>
        /// Use optionItems to create Options dialog frame
        /// </summary>
        /// <param name="optionItems"></param>
        private void CreateOption(OptionItem[] optionItems)
        {
            // save setting to window varibale
            SaveOption();
            // unregisterName
            foreach (StackPanel sp in ValueSP.Children)
            {
                string name = sp.Name;
                ValueSP.UnregisterName(name);
            }
            KeySP.Children.Clear();
            ValueSP.Children.Clear();
            // create nodes
            foreach (OptionItem item in optionItems)
            {
                if (!item.IsShow) continue;
                // set key
                TextBlock keyTB = new TextBlock
                {
                    Text = item.Alias.ToString() + ":",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Background = new SolidColorBrush(c),
                };
                Border borderKey = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.SteelBlue),
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    Height = item.Height,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                borderKey.Child = keyTB;
                KeySP.Children.Add(borderKey);
                // set key end
                // set value
                StackPanel stackPanel = new StackPanel
                {
                    Name = item.Name + "SP",
                    Height = item.Height,
                    Margin = new Thickness(0, 5, 0, 0),
                    Orientation = Orientation.Horizontal,
                };
                
                ValueSP.RegisterName(item.Name + "SP",stackPanel);
                string nodeType = item.NodeType.ToLower();
                // textbox
                if (nodeType == "text")
                {
                    TextBox textBox = new TextBox
                    {
                        Name = item.Name + "TB",
                        Text = item.Values,
                        Width = 300,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    textBox.TextChanged += new TextChangedEventHandler(ValueChanged);
                    
                    stackPanel.Children.Add(textBox);
                    ValueSP.Children.Add(stackPanel);
                }
                // radio box
                else if (nodeType == "radio")
                {
                    // loop create RadioButton and check the value selected or not
                    foreach (string it in item.Options)
                    {
                        bool ischecked = false;
                        if (item.Values != null && item.Values == it) ischecked = true;
                        RadioButton radioButton = new RadioButton
                        {
                            GroupName = item.Name,
                            IsChecked = ischecked,
                            Content = it,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        // add ApplyButton enable event
                        radioButton.Checked += new RoutedEventHandler(ValueChanged);
                        radioButton.Unchecked += new RoutedEventHandler(ValueChanged);
                        // add-to parent frame
                        stackPanel.Children.Add(radioButton);
                    }
                    // add into dialog
                    ValueSP.Children.Add(stackPanel);
                }
                // checkbox
                else if (nodeType == "multiple")
                {
                    // the check all or check none button
                    CheckBox all = new CheckBox
                    {
                        Content = "全选/全不选",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    // add listen event of checked or unchecked
                    all.Checked += new RoutedEventHandler(CheckAllOrNone);
                    all.Unchecked += new RoutedEventHandler(CheckAllOrNone);
                    // add-to parent frame 
                    stackPanel.Children.Add(all);
                    // split the checked value
                    List<string> checkedValue = item.Values.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    // loop create CheckBox button and check the value selected or not
                    foreach (string it in item.Options)
                    {
                        bool ischecked = !string.IsNullOrEmpty(checkedValue.Find((string entity) => entity == it));
                        CheckBox checkBox = new CheckBox
                        {
                            Content = it,
                            IsChecked = ischecked,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        // add ApplyButton enable event
                        checkBox.Unchecked += new RoutedEventHandler(ValueChanged);
                        checkBox.Checked += new RoutedEventHandler(ValueChanged);
                        // add-to parent frame
                        stackPanel.Children.Add(checkBox);
                    }
                    // add into dialog
                    ValueSP.Children.Add(stackPanel);
                }
                // select frame
                else if (nodeType == "select")
                {
                    // get the selected value of all options
                    string selected = string.IsNullOrEmpty(item.Options.ToList().Find((string entity) => entity == item.Values)) ? item.Options[0] : item.Values;
                    // create ComboBox and check the selected value
                    ComboBox combo = new ComboBox
                    {
                        Name = item.Name,
                        SelectedValue = selected,
                        ItemsSource = item.Options,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    // add ApplyButton enable event
                    combo.SelectionChanged += new SelectionChangedEventHandler(ValueChanged);
                    // add ComboBox into the dialog
                    stackPanel.Children.Add(combo);
                    ValueSP.Children.Add(stackPanel);
                }
                // add suffix 
                if (item.Suffix != null && !string.IsNullOrEmpty(item.Suffix))
                {
                    TextBlock suffix = new TextBlock
                    {
                        Text = item.Suffix,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    stackPanel.Children.Add(suffix);
                }
                // set value end
            }
            // create nodes end

        }

        /// <summary>
        ///  logic of Interface value changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueChanged(Object sender, EventArgs e)
        {
            if (!ApplyButton.IsEnabled)
            {
                ApplyButton.IsEnabled = true;
            }
            
        }

        /// <summary>
        /// logic of check all or check none
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckAllOrNone(Object sender, EventArgs e)
        {
            // listen checked or unchecked event
            CheckBox checkBox = (CheckBox)sender;
            // initializes the checked state 
            bool? isChecked = checkBox.IsChecked;
            // get parent frame
            StackPanel parent = checkBox.Parent as StackPanel;
            // loop to change the checked state
            foreach (var obj in parent.Children)
            {
                string objType = obj.GetType().ToString();
                if (objType == "System.Windows.Controls.CheckBox")
                {
                    CheckBox chk = obj as CheckBox;
                    if (chk.Content != "全选/全不选")
                    {
                        chk.IsChecked = isChecked;
                    }
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SaveOption();
            Vendor.SettingHelper.setting = setting;
            Vendor.SettingHelper.Save();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveOption();
            Vendor.SettingHelper.setting = setting;
            Button applyButton = sender as Button;
            applyButton.IsEnabled = false;
        }

        /// <summary>
        /// 根据控件的Name获取控件对象
        /// </summary>
        /// <typeparam name="T">控件类型</typeparam>
        /// <param name="controlName">Name</param>
        /// <returns></returns>
        public T GetControlObject<T>(string controlName)
        {
            try
            {
                Type type = this.GetType();
                FieldInfo fieldInfo = type.GetField(controlName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldInfo != null)
                {
                    T obj = (T)fieldInfo.GetValue(this);
                    return obj;
                }
                else
                {
                    return default(T);
                }


            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}

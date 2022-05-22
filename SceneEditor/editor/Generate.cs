using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SceneEditor.editor
{
    public static class Generate
    {
        public static Expander Expander(string Header, object? content = null)
        {
            Expander exp = new Expander();
            exp.Header = Header;
            exp.Content = content;
            exp.IsExpanded = true;
            return exp;
        }

        public static StackPanel StackPanel(UIElement content)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(content);
            return stackPanel;
        }

        public static ListBoxItem ListBoxItem(object content)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = content is string ? TextBlock(content) : content;
            item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return item;
        }

        public static TextBlock TextBlock(object text)
        {
            TextBlock block = new TextBlock();
            block.Text = text.ToString();
            return block;
        }

        public static TextBox TextBox(object text)
        {
            TextBox block = new TextBox();
            block.Text = text.ToString();
            block.IsReadOnly = false;
            return block;
        }

        public static ScrollViewer ScrollViewer(object content)
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Content = content;
            return scrollViewer;
        }

        public static Thickness Thickness(double thickness)
        {
            return new Thickness(thickness);
        }

        public static Border Border(double thickness)
        {
            Border border = new Border();
            border.BorderThickness = Thickness(thickness);
            return border;
        }

        public static CheckBox CheckBox(object content, bool isChecked = false)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.IsChecked = isChecked;
            checkBox.Content = content;
            return checkBox;
        }

        public static Button Button(string name, RoutedEventHandler Event)
        {
            Button button = new Button();
            button.Content = TextBlock(name);
            button.Click += Event;
            return button;
        }

        public static ComboBoxItem ComboBoxItem(string content)
        {
            ComboBoxItem item = new ComboBoxItem();
            item.Content = TextBlock(content);
            //item.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return item;
        }

        public static TabItem TabItem(RoutedEventHandler buttonEvent)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = "+";
            DockPanel panel = new DockPanel();
            TreeView treeView = new TreeView();
            Button button = Button("Delete this editor", buttonEvent);
            tabItem.Content = panel;
            treeView.VerticalContentAlignment = VerticalAlignment.Stretch;
            treeView.VerticalAlignment = VerticalAlignment.Top;
            treeView.Padding = new Thickness(0, 10, 0, 10);
            button.VerticalAlignment = VerticalAlignment.Bottom;
            button.Margin = new Thickness(0, 10, 0, 10);
            DockPanel.SetDock(treeView, Dock.Top);
            DockPanel.SetDock(button, Dock.Top);
            panel.VerticalAlignment = VerticalAlignment.Stretch;
            panel.Children.Add(treeView);
            panel.Children.Add(button);
            return tabItem;
        }
    }
}

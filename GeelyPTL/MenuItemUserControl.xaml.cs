using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GeelyPTL
{
    /// <summary>
    /// 菜单项。
    /// </summary>
    public partial class MenuItemUserControl : UserControl
    {
        /// <summary>
        /// 图标属性。
        /// </summary>
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(MenuItemUserControl), new PropertyMetadata(null, MenuItemUserControl.Image_PropertyChangedCallback));
        /// <summary>
        /// 标题属性。
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MenuItemUserControl), new PropertyMetadata(string.Empty, MenuItemUserControl.Title_PropertyChangedCallback));
        /// <summary>
        /// 是否激活中属性。
        /// </summary>
        public static readonly DependencyProperty IsActivedProperty = DependencyProperty.Register("IsActived", typeof(bool), typeof(MenuItemUserControl), new PropertyMetadata(false, MenuItemUserControl.IsActived_PropertyChangedCallback));

        private static void Image_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MenuItemUserControl menuItemUserControl = (MenuItemUserControl)d;
            menuItemUserControl.image.Source = (ImageSource)e.NewValue;
        }

        private static void Title_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MenuItemUserControl menuItemUserControl = (MenuItemUserControl)d;
            menuItemUserControl.textBlock.Text = (string)e.NewValue;
        }

        private static void IsActived_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MenuItemUserControl menuItemUserControl = (MenuItemUserControl)d;
            menuItemUserControl.rectangle.Fill = (bool)e.NewValue ? Brushes.Firebrick : Brushes.LightGray;
        }

        /// <summary>
        /// 获取或设置图标。
        /// </summary>
        public ImageSource Image
        {
            get { return (ImageSource)this.GetValue(ImageProperty); }
            set { this.SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// 获取或设置标题。
        /// </summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// 获取或设置是否激活中。
        /// </summary>
        public bool IsActived
        {
            get { return (bool)this.GetValue(IsActivedProperty); }
            set { this.SetValue(IsActivedProperty, value); }
        }

        /// <summary>
        /// 初始化菜单项。
        /// </summary>
        public MenuItemUserControl()
        {
            this.InitializeComponent();
        }
    }
}
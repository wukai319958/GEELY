using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AssortingKanban.ForAssortingKanban;

namespace AssortingKanban
{
    /// <summary>
    /// 托盘上的周转箱控件。
    /// </summary>
    public partial class BoxUserControl : UserControl
    {
        AST_PalletTaskItemDto astPalletTaskItem;
        DoubleAnimation opacityAnimation;

        /// <summary>
        /// 获取或设置库位：1~5。
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// 获取或设置当前任务明细。
        /// </summary>
        public AST_PalletTaskItemDto AST_PalletTaskItem
        {
            get { return this.astPalletTaskItem; }
            set
            {
                if ((this.astPalletTaskItem == null && value == null)
                    || (this.astPalletTaskItem == null && value != null)
                    || (this.astPalletTaskItem != null && value == null)
                    || (this.astPalletTaskItem.AST_PalletTaskItemId != value.AST_PalletTaskItemId)
                    || (this.astPalletTaskItem.PickedQuantity != value.PickedQuantity)
                    || (this.astPalletTaskItem.PickStatus != value.PickStatus))
                {
                    this.astPalletTaskItem = value;

                    this.RefreshUI();
                }
            }
        }

        /// <summary>
        /// 初始化周转箱控件。
        /// </summary>
        public BoxUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.AST_PalletTaskItem = null;

            this.opacityAnimation = new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(400));
            this.opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            this.opacityAnimation.AutoReverse = true;
        }

        /// <summary>
        /// 旋转到指定角度。
        /// </summary>
        /// <param name="angle">指定角度。</param>
        public void Rotate(double angle)
        {
            this.rotateTransform.Angle = angle;
        }

        void RefreshUI()
        {
            this.border.Background = Brushes.LightGray;
            this.border.BeginAnimation(UIElement.OpacityProperty, null);
            this.textBlockPickedQuantity.Text = string.Empty;
            this.textBlockQuantitySplit.Text = string.Empty;
            this.textBlockToPickQuantity.Text = string.Empty;
            this.textBlockMaterialName.Text = string.Empty;

            if (this.astPalletTaskItem != null)
            {
                if (this.astPalletTaskItem.PickStatus == PickStatus.New)
                {
                    this.border.Background = Brushes.White;
                }
                else if (this.astPalletTaskItem.PickStatus == PickStatus.Picking)
                {
                    this.border.Background = Brushes.Goldenrod;
                    this.border.BeginAnimation(UIElement.OpacityProperty, this.opacityAnimation);
                }
                else if (this.astPalletTaskItem.PickStatus == PickStatus.Finished)
                {
                    this.border.Background = Brushes.LightSeaGreen;
                }

                if (this.astPalletTaskItem.PickStatus != PickStatus.New)
                {
                    this.textBlockPickedQuantity.Text = (this.astPalletTaskItem.PickedQuantity ?? 0).ToString();
                    this.textBlockQuantitySplit.Text = " / ";
                }

                this.textBlockToPickQuantity.Text = this.astPalletTaskItem.ToPickQuantity.ToString(CultureInfo.InvariantCulture);
                this.textBlockMaterialName.Text = this.astPalletTaskItem.MaterialName;
            }
        }
    }
}
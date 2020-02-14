using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AssortingKanban.ForAssortingKanban;

namespace AssortingKanban
{
    /// <summary>
    /// 托盘控件。
    /// </summary>
    public partial class PalletUserControl : UserControl
    {
        static readonly TimeSpan UpTimeSpan = TimeSpan.FromSeconds(10);
        static readonly TimeSpan DownTimeSpan = TimeSpan.FromSeconds(10);
        static readonly TimeSpan ActionSplitTimeSpan = TimeSpan.FromSeconds(1);
        static readonly TimeSpan RotationTimeSpan = TimeSpan.FromSeconds(15);
        static readonly TimeSpan ReverseRotationTimeSpan = TimeSpan.FromSeconds(15);

        AST_PalletTaskDto astPalletTask;
        bool inAnimation;

        /// <summary>
        /// 初始化托盘控件。
        /// </summary>
        public PalletUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.boxUserControl1.AST_PalletTaskItem = null;
                this.boxUserControl2.AST_PalletTaskItem = null;
                this.boxUserControl3.AST_PalletTaskItem = null;
                this.boxUserControl4.AST_PalletTaskItem = null;
                this.boxUserControl5.AST_PalletTaskItem = null;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AST_PalletTaskDto astPalletTask = this.DataContext as AST_PalletTaskDto;
            this.AST_PalletTask = astPalletTask;
        }

        /// <summary>
        /// 获取或设置当前任务。
        /// </summary>
        public AST_PalletTaskDto AST_PalletTask
        {
            get { return this.astPalletTask; }
            set
            {
                this.astPalletTask = value;

                this.RefreshUI();
            }
        }

        void RefreshUI()
        {
            if (this.astPalletTask == null)
            {
                this.boxUserControl1.AST_PalletTaskItem = null;
                this.boxUserControl2.AST_PalletTaskItem = null;
                this.boxUserControl3.AST_PalletTaskItem = null;
                this.boxUserControl4.AST_PalletTaskItem = null;
                this.boxUserControl5.AST_PalletTaskItem = null;
            }
            else
            {
                if (this.astPalletTask.PalletType == "04") //标准箱托盘
                {
                    this.Visibility = Visibility.Collapsed;
                    return;
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                }

                AST_PalletTaskItemDto astPalletTaskItem1 = this.astPalletTask.Items.FirstOrDefault(pti => pti.FromPalletPosition == 1);
                AST_PalletTaskItemDto astPalletTaskItem2 = this.astPalletTask.Items.FirstOrDefault(pti => pti.FromPalletPosition == 2);
                AST_PalletTaskItemDto astPalletTaskItem3 = this.astPalletTask.Items.FirstOrDefault(pti => pti.FromPalletPosition == 3);
                AST_PalletTaskItemDto astPalletTaskItem4 = this.astPalletTask.Items.FirstOrDefault(pti => pti.FromPalletPosition == 4);
                AST_PalletTaskItemDto astPalletTaskItem5 = this.astPalletTask.Items.FirstOrDefault(pti => pti.FromPalletPosition == 5);

                this.boxUserControl1.AST_PalletTaskItem = astPalletTaskItem1;
                this.boxUserControl2.AST_PalletTaskItem = astPalletTaskItem2;
                this.boxUserControl3.AST_PalletTaskItem = astPalletTaskItem3;
                this.boxUserControl4.AST_PalletTaskItem = astPalletTaskItem4;
                this.boxUserControl5.AST_PalletTaskItem = astPalletTaskItem5;
            }

            //托盘类型
            if (this.astPalletTask != null)
            {
                if (this.astPalletTask.PalletType == "01")
                {
                    this.column1.Width = new GridLength(0.5, GridUnitType.Star);
                    this.column2.Width = new GridLength(0.5, GridUnitType.Star);
                    this.column3.Width = new GridLength(1, GridUnitType.Star);
                    this.row1.Height = new GridLength(0.6, GridUnitType.Star);
                }
                else
                {
                    this.column1.Width = new GridLength(0);
                    this.column2.Width = new GridLength(0);
                    this.column3.Width = new GridLength(0);
                    this.row1.Height = new GridLength(0);
                }
            }

            //托盘方向与旋转
            if (this.astPalletTask == null)
            {
                this.inAnimation = true;

                this.translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                this.rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);

                this.translateTransform.Y = 0;
                this.rotateTransform.Angle = 0;

                this.boxUserControl1.Rotate(0);
                this.boxUserControl2.Rotate(0);
                this.boxUserControl3.Rotate(0);
                this.boxUserControl4.Rotate(0);
                this.boxUserControl5.Rotate(0);

                this.inAnimation = false;
            }
            else
            {
                if (!this.inAnimation)
                {
                    this.inAnimation = true;

                    if (this.astPalletTask.PalletRotationStatus == PalletRotationStatus.Normal)
                    {
                        this.translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                        this.rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);

                        this.translateTransform.Y = 0;
                        this.rotateTransform.Angle = 0;

                        this.boxUserControl1.Rotate(0);
                        this.boxUserControl2.Rotate(0);
                        this.boxUserControl3.Rotate(0);
                        this.boxUserControl4.Rotate(0);
                        this.boxUserControl5.Rotate(0);

                        this.inAnimation = false;
                    }
                    else if (this.astPalletTask.PalletRotationStatus == PalletRotationStatus.BeginRotation)
                    {
                        DoubleAnimation upAnimation = new DoubleAnimation(0, -50, PalletUserControl.UpTimeSpan);
                        upAnimation.Completed += (sender1, e1) =>
                        {
                            Thread.Sleep(PalletUserControl.ActionSplitTimeSpan);

                            DoubleAnimation angleAnimation = new DoubleAnimation(0, 180, PalletUserControl.RotationTimeSpan);
                            angleAnimation.Completed += (sender2, e2) =>
                            {
                                Thread.Sleep(PalletUserControl.ActionSplitTimeSpan);

                                DoubleAnimation downAnimation = new DoubleAnimation(-50, 0, PalletUserControl.DownTimeSpan);
                                downAnimation.Completed += (sender3, e3) =>
                                {
                                    this.inAnimation = false;
                                };
                                this.translateTransform.BeginAnimation(TranslateTransform.YProperty, downAnimation);
                            };
                            this.rotateTransform.BeginAnimation(RotateTransform.AngleProperty, angleAnimation);
                        };
                        this.translateTransform.BeginAnimation(TranslateTransform.YProperty, upAnimation);
                    }
                    else if (this.astPalletTask.PalletRotationStatus == PalletRotationStatus.Reversed)
                    {
                        this.translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                        this.rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);

                        this.translateTransform.Y = 0;
                        this.rotateTransform.Angle = 180;

                        this.boxUserControl1.Rotate(180);
                        this.boxUserControl2.Rotate(180);
                        this.boxUserControl3.Rotate(180);
                        this.boxUserControl4.Rotate(180);
                        this.boxUserControl5.Rotate(180);

                        this.inAnimation = false;
                    }
                    else if (this.astPalletTask.PalletRotationStatus == PalletRotationStatus.BeginReverseRotation)
                    {
                        DoubleAnimation upAnimation = new DoubleAnimation(0, -50, PalletUserControl.UpTimeSpan);
                        upAnimation.Completed += (sender1, e1) =>
                        {
                            Thread.Sleep(PalletUserControl.ActionSplitTimeSpan);

                            DoubleAnimation angleAnimation = new DoubleAnimation(180, 0, PalletUserControl.ReverseRotationTimeSpan);
                            angleAnimation.Completed += (sender2, e2) =>
                            {
                                Thread.Sleep(PalletUserControl.ActionSplitTimeSpan);

                                DoubleAnimation downAnimation = new DoubleAnimation(-50, 0, PalletUserControl.DownTimeSpan);
                                downAnimation.Completed += (sender3, e3) =>
                                {
                                    this.inAnimation = false;
                                };
                                this.translateTransform.BeginAnimation(TranslateTransform.YProperty, downAnimation);
                            };
                            this.rotateTransform.BeginAnimation(RotateTransform.AngleProperty, angleAnimation);
                        };
                        this.translateTransform.BeginAnimation(TranslateTransform.YProperty, upAnimation);
                    }
                }
            }
        }
    }
}
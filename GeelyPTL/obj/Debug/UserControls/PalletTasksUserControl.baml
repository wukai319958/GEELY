   M S B A M L   `   `   `  ����V  R服务控制台, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9033bf8703064ec69!clr-namespace:GeelyPTL.ConvertersGeelyPTL.Converters  5      2    ,GeelyPTL.UserControls.PalletTasksUserControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      X TXceed.Wpf.Toolkit, Version=3.0.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5      7localConverters!clr-namespace:GeelyPTL.Converters  5      -   5        
�������  � 	Resources  5      9  3GeelyPTL.Converters.PickStatusToForegroundConverter #  pickStatusToForegroundConverter3  -GeelyPTL.Converters.PickStatusToTextConverter  pickStatusToTextConverter3  -GeelyPTL.Converters.PalletTypeToTextConverter  palletTypeToTextConverter��%W   &	        &	      &	 &       5   
   6:     5   
   64     5   
   64   5      .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $
��Left=�5      $��Center=�5      .+�5      &   Xceed.Wpf.Toolkit.DateTimePicker  -   5          Name$ datePickerMin��5   "   $��Center=�5      �� 5      $	�� - ��6   $��Center=�5         5!      -   5        $ datePickerMax��5!   "   $��Center=�5"      �� 5#      -   5         ��Name$ buttonToday��5#      $��今天��5$      $��40��5%      $��25��5&      5'      �� 5(      -   5         ��Name$ textBoxCondition��5(      $��10,0,0,0q�5)      $	��200��5*      $��30��5+      $��Center=�5,      �� 5-      -   5        $ buttonSearch��5-      $��查询��5.      $��10,0,0,0q�5/      $��80��50      $��30��51      52      53      �� 55   
   $ 
Horizontal=�6   $��Right=�56      $��Center=�57      .+�58      �� -   5        $ buttonDetail��58      $��	明细...��59      $��80��5:      $��30��5;      5<      5=      &   System.Windows.Controls.DataGrid  5?   
   -   5          Name$ dataGrid��5?      $��1��5@         AutoGenerateColumns .     ColumnHeaderHeight$ 30Q�5B      	   	RowHeight$	 30Q�5C      
   RowHeaderWidth$
 50Q�5D         GridLinesVisibility$
 None=�5E         AlternatingRowBackground
 �����   
IsReadOnly .    Columns 5J      0  *System.Windows.Controls.DataGridTextColumn  5K      ,  &System.Windows.Controls.DataGridColumn   Header$ 项目��5L   %      Width5  /System.Windows.Controls.DataGridLengthConverter$	 100 5M   %   1	  +System.Windows.Controls.DataGridBoundColumn 	  Binding 5K   %   �� *ProjectCode+5M   %     5N      $ 阶段��5O   %   $	 100 5P   %    5N   %   �� *ProjectStep+5P   %     5Q      $ 批次��5R   %   $	 150 5S   %    5Q   %   �� *	BatchCode+5S   %     5T      $ 	分拣口��5U   %   $	 100 5V   %    5T   %   �� *CFG_Channel.Name+5V   %     5W      $ 托盘��5X   %   $ * 5Y   %    5W   %   �� *CFG_Pallet.Code+5Y   %     5Z      $ 箱型��5[   %   $	 100 5\   %    5Z   %   �� *CFG_Pallet.PalletType+ �� 	Converter# [ 5\   %   4
  .System.Windows.Controls.DataGridTemplateColumn
  5]      $ 分拣状态��6)   $	 100 5^   )    
  CellTemplate 5_      �� 5`      .R�5a      �� ��6(   �� *
PickStatus+# [ ��5b   (   �� *
PickStatus+# [  5c      5e        5f      $ 开始时间��5g   %   $	 150 5h   %    5f   %   �� *
CreateTime+ �� StringFormat$ yyyy-MM-dd HH:mm:ss��5h   %   5j      5k      5l      
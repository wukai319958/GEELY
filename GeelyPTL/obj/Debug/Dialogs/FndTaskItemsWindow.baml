   M S B A M L   `   `   `  ����  服务控制台)    #GeelyPTL.Dialogs.FndTaskItemsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  配送任务的明细��5   	   $	��500��5   	   $	��800��5   	    � WindowStartupLocation$ CenterScreen=�5   	   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��50 �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ dataGrid��5         AutoGenerateColumns .     ColumnHeaderHeight$ 30Q�5         	RowHeight$ 30Q�5         RowHeaderWidth$ 50Q�5         GridLinesVisibility$
 None=�5      	   AlternatingRowBackground
	 �����
   
IsReadOnly
 .    Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ 储位��5   %      Width5  /System.Windows.Controls.DataGridLengthConverter$ 60 5   %   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5   %   �� *CartPosition+5   %     5      $ 物料编码��5   %   $	 100 5   %    5   %   �� *MaterialCode+5   %     5       $ 物料名称��5!   %   $ * 5"   %    5    %   �� *MaterialName+5"   %     5#      $ 物料条码��5$   )   $ * 5%   )    5#   %   �� *MaterialBarcode+5%   )     5&      $ 数量��5'   %   $ 60 5(   %    5&   %   �� *
Quantity+5(   %   5*      �� 5,   
   $��1��6   $��
关闭(_C)��5-      $��80��5.      $��30��5/       �� IsCancel . 50      51      52      
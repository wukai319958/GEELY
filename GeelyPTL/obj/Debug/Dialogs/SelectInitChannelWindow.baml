   M S B A M L   `   `   `  ����  服务控制台.    (GeelyPTL.Dialogs.SelectInitChannelWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$!  拣料区铺线巷道选择��5   	   $	��500��5   	   $	��350��5   	    � WindowStartupLocation$ CenterScreen=�5   	   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��50 �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ dataGrid��5         AutoGenerateColumns .     ColumnHeaderHeight$ 30Q�5         	RowHeight$ 30Q�5         RowHeaderWidth$ 50Q�5         GridLinesVisibility$
 None=�5      	   AlternatingRowBackground
	 �����
   
IsReadOnly
 .    Columns 5      4  .System.Windows.Controls.DataGridTemplateColumn  5      ,  &System.Windows.Controls.DataGridColumn   
IsReadOnly .     HeaderTemplate 5      �� 5      .R�5      ��  ��Name$ CheckAll��6'   -   5        $��Center=�5   9   $��Center=�6V   
 � Tag 6q   �� 6�   5         CellTemplate 5       �� 5!      .R�5"      �� $ cb��6'   -   5        $��Center=�5"   3    6P   ��  d� 	IsChecked 6`   �� *	IsChecked+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�6�   5#      5%      0  *System.Windows.Controls.DataGridTextColumn  5&         Header$ 巷道ID��5'   %      
Visibility$ Hidden=�5(   %   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5&   %   �� *Id+5(   %     5)      $ 巷道编码��5*   %      Width5  /System.Windows.Controls.DataGridLengthConverter$	 100 5+   %    5)   %   �� *Code+5+   %     5,      $ 巷道名称��5-   %   $	 100 5.   %    5,   %   �� *Name+5.   %   50      �� 52   
   $��1��6   $��1��53       �� Orientation$ 
Horizontal=�54      $��Center=�55      .+�56      �� -   5         ��Name$ btnOK��56      $��
确定(_S)��57      $��80��58      $��30��59       �� 	IsDefault . 5:   "   �� 5;      -   5        $ btnClose��5;      $��
关闭(_C)��5<      $��20,0,0,0q�5=      $��80��5>      $��30��5?       �� IsCancel . 5@      5A      5B      5C      
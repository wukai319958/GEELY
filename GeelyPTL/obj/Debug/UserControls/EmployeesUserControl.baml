   M S B A M L   `   `   `  ����  服务控制台0    *GeelyPTL.UserControls.EmployeesUserControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -   5        
�������.��5      � $��10q�6     � RowDefinitions  5   
   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $
��Left=�5      $��Center=�5      .+�5      �� -   5         ��Name$ buttonRefresh��5      $��刷新��5      $��80��5      $��30��5      5      5      �� 5   
   $ 
Horizontal=�6   $��Right=�5      $��Center=�5      .+�5      �� -   5        $ 	buttonAdd��5      $��	新增...��5      $��80��5       $��30��5!      5"      �� 5#      -   5        $ 
buttonEdit��5#      $��	编辑...��5$      $��10,0,0,0q�5%      $��80��5&      $��30��5'      5(      �� 5)      -   5        $ buttonDelete��5)      $��删除��5*      $��10,0,0,0q�5+      $	��100��5,      $��30��5-      5.      5/      &   System.Windows.Controls.DataGrid  51   
   -   5          Name$ dataGrid��51      $��1��52         AutoGenerateColumns .     ColumnHeaderHeight$ 30Q�54         	RowHeight$ 30Q�55         RowHeaderWidth$ 50Q�56         GridLinesVisibility$
 None=�57      	   AlternatingRowBackground
	 �����
   
IsReadOnly
 .    Columns 5<      0  *System.Windows.Controls.DataGridTextColumn  5=      ,  &System.Windows.Controls.DataGridColumn   Header$ 工牌��5>   %      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 5?   %   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5=   %   �� *Code+5?   %     5@      $ 姓名��5A   %   $ * 5B   %    5@   %   �� *Name+5B   %     5C      $ 	登录名��5D   %   $ * 5E   %    5C   %   �� *	LoginName+5E   %   4  .System.Windows.Controls.DataGridCheckBoxColumn  5F      $ 在职��5G   )    5F   )   �� *	IsEnabled+5G   )   5I      5J      5K      
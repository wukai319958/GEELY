   M S B A M L   `   `   `  ����  服务控制台.    (GeelyPTL.Dialogs.EmployeeAddOrEditWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  操作员编辑��5   	   $	��250��5   	   $	��400��5   	    � 
ResizeMode$ NoResize=�5   	    � WindowStartupLocation$ CenterScreen=�5   	   .��5      � $��10q�6    � ColumnDefinitions 5   
   �� 5      $	��100 �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��50 �6   .+�5   
   �� $��	工牌：��6   $��Center=�5      �� 5   
   -   5         ��Name$ textBoxCode��5      $��1��5      $��Center=�5      �� 5   
   $��1��6   $��	姓名：��5      $��Center=�5       �� 5!   
   -   5        $ textBoxName��5!      $��1��5"      $��1��5#      $��Center=�5$      �� 5%   
   $��2��6   $��登录名：��5&      $��Center=�5'      �� 5(   
   -   5        $ textBoxLoginName��5(      $��2��5)      $��1��5*      $��Center=�5+      �� 5,   
   $��3��6   $��登录密码：��5-      $��Center=�5.      G� 5/   
   -   5         G�Name$ passwordBoxPassword��5/      $��3��50      $��1��51      $��Center=�52      �� 53   
   $��4��6   $��	在职：��54      $��Center=�55      �� 56   
   -   5         ��Name$ checkBoxIsEnable��56      $��4��57      $��1��58      $��Center=�59      �� 5;   
   $��5��6   $��1��5<       �� Orientation$ 
Horizontal=�5=      $��Center=�5>      .+�5?      �� -   5        	 ��Name$	 buttonOK��5?      $��
确定(_O)��5@      $��80��5A      $��30��5B      5C      �� 5D      $��
取消(_C)��6   $��10,0,0,0q�5E      $��80��5F      $��30��5G      
 �� IsCancel
 . 5H      5I      5J      5K      
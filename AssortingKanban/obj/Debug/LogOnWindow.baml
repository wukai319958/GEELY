   M S B A M L   `   `   `  ����  分拣看板!    AssortingKanban.LogOnWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  登录到分拣看板��5   	   $	��200��5   	   $	��300��5   	    � WindowStartupLocation$ CenterScreen=�5   	    � 
ResizeMode$ NoResize=�5   	   .��5
      � $��20,20,20,10q�6    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��40 �6    � ColumnDefinitions 5   
   �� 5      $��80 �6   �� 5      $��* �6   .+�5   
   �� $��服务器 IP：��6   $��Center=�5      �� 5   
   -   5         ��Name$ textBoxServerIP��5      $��1��5      $��Center=�5      �� 5   
   $��1��6   $��服务端口：��5      $��Center=�5      �� 5   
   -   5        $ textBoxServicePort��5      $��1��5      $��1��5       $��Center=�5!      �� 5"   
   $��2��6   $��巷道编码：��5#      $��Center=�5$      �� 5%   
   -   5         ��Name$ comboBoxCfgChannelCode��5%      $��1��5&      $��2��5'      $��Center=�5(       �� 
IsEditable . 5)      �� 5+   
   -   5         ��Name$ buttonLogOn��5+      $��3��5,      $��1��5-      $��登录��5.      $
��20,0q�5/      $��30��50      $��Center=�51      $
��Left=�52      53      54      55      
   M S B A M L   `   `   `  ����K  GEasyCaster-Alarm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:EasyCaster_AlarmEasyCaster_Alarm  5   	   !    EasyCaster_Alarm.AuthWindow   5   	   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Z VFontAwesome.WPF, Version=4.7.0.37774, Culture=neutral, PublicKeyToken=0758b07a11a4f466-fa$http://schemas.fontawesome.io/icons/  5   	   *localclr-namespace:EasyCaster_Alarm  5   	   -   5           authWindowTitle  � Title#  �   5	   	   $	��370��63   $	��350��6@    � Topmost$
 True��6L    � Icon$ /Resources/Icona.ico��6t    � 	Resources 5
        ControlButton �� 
TargetType �� CornerRadius �� Padding �� Triggers P� Property	 P� Value
 �� 
TargetName �� Property �� Value �� 
TargetType J� Opacity ��Name��%  &	       '�� �    �� 5   
   " ��60   .R�5      �� $��border��6   ���	#FF484848$��2q�6;   $ 5��6O   #��z��6`   #��z��6�   #��z��6�   # z��6�   ._�5      �� $
��True��6#   6@   5       5      P� 5       �@J�IsMouseOver$
	 True��61   . �5      �� $
 border��64    � �� �#7878786H   5      P� 5       � ��$
	 True��6/   . �5      �� $
 border��64    � �� �	#FF2C628B6H   5      5      �� 5   
   " ��6   .�5      ��  � �� �#ffffff6+   �� 5       � �� �	#FF9696966+   �� 5       � ��$ 1q�60   �� 5        � �� �#bec6db6,   �� 5!       � �� 5"      �� 5#      " ��6&   .R�5$      � .+�5%      �� ���#ffffff���#a3a3a3$��2q�5'   %   $ 5��5(   %   $ 1Q�66   ._�5)   "   � .+�5*   &   �� $ PART_ContentHost��63   ���	#FF787878$��0q�5,   3   $��False��6G   $��0q�5-   3   #��z��6>   5.   #   5/      50      51      53      54      55      .��56      �� .+�57   
   �� -   5         ��Name$ 
auth_title��57      $
��Bold"�6$   $��20(�66   $��Center=�6D   $��Center=�6h     	authTitle#���  6�   $	��350��6�   
 �� Top$ 5��6�   $��5,5,5,5q�6�   �� 58   
   $��Normal"�6   $��Center=�6$   $��Center=�6H     	authPhone#���  6j   $��Normal$�6�   $ 43��6�   $	��350��6�   $��16(�6�   �� 59   
   -   5         ��Name$ auth_verification_block��59      $	 182��66   $y�Hidden=�6G    �� Left$ -1��6[   $��68��6l   .+�5:      �� $��Normal"�6   $��Center=�6(   $��Center=�6L     authVerification#���  6n   $��Normal$�6�   $	��351��6�   $��16(�6�   �� 5;      -   5         ��Name$ auth_verification��5;      $��30��61    �� TextWrapping$
 Wrap=�6=   $	��280��6Q   ���	#FFBEC6DB���	#FFBEC6DB���	#FF787878$��14(�6�   $��Global Sans Serif)�6�   $��0,2,0,0q�6�   $��Center=�6�   $��Center=�6  5<      �� 5=   
   -   5        $ auth_password��5=      $��30��6)   $
 Wrap=�65   $	��280��6I   ���	#FFBEC6DB���	#FFBEC6DB���	#FF787878$��14(�6�   $��Global Sans Serif)�6�   $��0,2,0,0q�6�   $��Center=�6�   $��Center=�6�   $ 35��6   $	 152��61  �� 5>   
   $��Normal"�6   $��Center=�6$   $��Center=�6H     authPassword#���  6j   $��Normal$�6�   $	 104��6�   $	��350��6�   $��16(�6�   �� 5?   
   $��Normal"�6   $��Center=�6$   $��Center=�6H     authPasswordSub#���  6j   $��Normal$�6�   $��11(�6�   $��17��6�   $��5,0,5,0q�6�   ���	#FF5F5F5F$	 135��6�   $	��350��6�   �� 5@   
   -   5        $ 
auth_phone��5@      $��30��6&   $
 Wrap=�62   $	��280��6F   ���	#FFBEC6DB���	#FFBEC6DB���	#FF787878$��14(�6�   $��Global Sans Serif)�6�   $��0,2,0,0q�6�   $��Center=�6�   $��Center=�6�   $ 35��6  $ 74��6.  6>  "  FontAwesome.WPF.ImageAwesome  5A   
   -   5          Name$ auth_spinner��5A         Icon$ Refresh=�60      Spin$
 True��6?   $��48��6K   $��48��6W   $	 156��6b   $	 195��6t   $y�Hidden=�6�   �� 5B   
   -   5         ��Name$ auth_submit��5B      $	��280��6&   $��36��62   #���  6>   $ 35��6i   $	 255��6z    � Cursor$
 Hand��6�   ���	#FFF0F0F0$��Center=�6�   $��1,1,1,1q�6�   .��5C      �� $��Center=�6   $��Center=�68   $��Normal"�6Z     
authSubmit#���  6n   5D      �� 5E   
   -	   5        $ 	error_msg��5E        	authError#���  6#   $ 35��6I   $	 296��6Z   $	��280��6k   $
��Bold"�6w   
���  ��$y�	Collapsed=�6�   $��Center=�6�   $��Center=�6�   ��6�   Z� 6�   �� 5F   
   -
   5        $ success_msg��5F       	 authSuccess#��� 	 6%   $ 35��6M   $	 296��6^   $	��280��6o   $
��Bold"�6{   
��� � �$y�	Collapsed=�6�   $��Center=�6�   $��Center=�6�   ��6�   Z� 6�   5G      5H      
   M S B A M L   `   `   `  ���� K  GEasyCaster Alarm, Version=1.0.4.8, Culture=neutral, PublicKeyToken=null3clr-namespace:EasyCaster_AlarmEasyCaster_Alarm  !    EasyCaster_Alarm.AuthWindow   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     Z VFontAwesome.WPF, Version=4.7.0.37774, Culture=neutral, PublicKeyToken=0758b07a11a4f466-fa$http://schemas.fontawesome.io/icons/  *localclr-namespace:EasyCaster_Alarm  -     �Name$  auth_win��   authWindowTitle � Title# �   $	��420��$	��350�� � Topmost$
 True�� � Icon$ /Resources/Icona.ico�� � 	Resources   ControlButton �� 
TargetType �� CornerRadius �� Padding �� Triggers	 P� Property
 P� Value �� 
TargetName �� Property �� Value �� 
TargetType J� Opacity ��Name��%Q  &	       '�� 	    �� " ��.R��� $��border�����	#FF484848$��2q�$ 5��#��z��#��z��#��z��# z��._��� $
��True�� P� 	 �@J�IsMouseOver$

 True��. ��� $ border�� � �� �#787878P� 	 � ��$

 True��. ��� $ border�� � �� �	#FF2C628B�� " ��.���  � �� �#ffffff��  � �� �	#FF969696��  � ��$ 1q���  � �� �#bec6db��  � �� �� " ��.R�� .+��� ���#ffffff���#a3a3a3$��2q�$ 5��$ 1Q�._�� .+��� $ PART_ContentHost�����	#FF787878$��0q�$��False��$��0q�#��z��.���� .+��� -    ��Name$ 
auth_title��$
��Bold"�$��20(�$��Center=�$��Center=�  	authTitle#���  $	��350��
 �� Top$ 5��$��5,5,5,5q��� $��Normal"�$��Center=�$��Center=�  	authPhone#���  $��Normal$�$ 43��$	��350��$��16(��� -    ��Name$ auth_verification_block��$	 182��$y�Hidden=� �� Left$ -1��$��68��.+��� $��Normal"�$��Center=�$��Center=�  authVerification#���  $��Normal$�$	��351��$��16(��� -    ��Name$ auth_verification��$��30�� �� TextWrapping$
 Wrap=�$	��280�����	#FFBEC6DB���	#FFBEC6DB���	#FF787878$��14(�$��Global Sans Serif)�$��0,2,0,0q�$��Center=�$��Center=��� -   $ auth_password��$��30��$
 Wrap=�$	��280�����	#FFBEC6DB���	#FFBEC6DB���	#FF787878$��14(�$��Global Sans Serif)�$��0,2,0,0q�$��Center=�$��Center=�$ 35��$	 152���� $��Normal"�$��Center=�$��Center=�  authPassword#���  $��Normal$�$	 104��$	��350��$��16(��� $��Normal"�$��Center=�$��Center=�  authPasswordSub#���  $��Normal$�$��11(�$��17��$��5,0,5,0q����	#FF5F5F5F$	 135��$	��350���� -   $ 
auth_phone��$��30��$
 Wrap=�$	��280�����	#FFBEC6DB���	#FFBEC6DB���	#FF787878$��14(�$��Global Sans Serif)�$��0,2,0,0q�$��Center=�$��Center=�$ 35��$ 74��"  FontAwesome.WPF.ImageAwesome  -     Name$ auth_spinner��   Icon$ Refresh=�   Spin$
 True��$��48��$��48��$	 156��$	 195��$y�Hidden=��� -    ��Name$ auth_submit��$	��280��$��36��#���  $ 35��$	 255�� � Cursor$
 Hand�����	#FFF0F0F0$��Center=�$��1,1,1,1q�.���� $��Center=�$��Center=�$��Normal"�  
authSubmit#���  �� -	   $ 
auth_reset��$	��280��$��36��#���  $ 35��$	 295��$
 Hand�����	#FFF0F0F0$��Center=�$��1,1,1,1q�.���� $��Center=�$��Center=�$��Normal"�  	authReset#���  �� -
   $ 	error_msg�� 	 	authError#��� 	 $ 35��$	 336��$	��280��$
��Bold"�
���  ��$y�	Collapsed=�$��Center=�$��Center=���Z� �� -   $ success_msg�� 
 authSuccess#��� 
 $ 35��$	 336��$	��280��$
��Bold"�
��� � �$y�	Collapsed=�$��Center=�$��Center=���Z� 
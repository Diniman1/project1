   M S B A M L   `   `   `  ����B  >WpfApp1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:WpfApp1WpfApp1  5   	       WpfApp1.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   'localclr-namespace:WpfApp1     5   	     � Title$  
MainWindow��5   	   $	��720��6   $
��1360��6)   $��Center=�66   $��Center=�6S   .��5	      � $
��1360��6   $	��717��6   $��Center=�6%   $��Center=�6@   .+�5
   
   �� -   5         ��Name$ formAuthorization��5
      $y�Visible=�6,   $��Center=�6A   $	��690��6^   $��-3,0,-3.4,0.4q�6k   $��Center=�6�   $
��1360��6�   .+�5      �� $��Авторизация��6   $��Center=�6*   $��Center=�6N   $��53��5      $��Center=�6   $	��269��67   $��32(�6C   &   System.Windows.Media.TextOptions   TextHintingMode$ Fixed=�5       �� Left$	 572��64   
 �� Top$	 241��6F   �� 5      -   5         ��Name$ tbLogin��5      $��Center=�6'   $��40��6D    �� TextWrapping$
 Wrap=�6P    �� 	MaxLength$ 16��6d   $�� ��5      $��Center=�6   $��Center=�65   $	��297��6W   $��18(�6c   $	��2pxq�5      $��11 0 0 0q�6(   $	 557��6;   $	 313��6M   �� 5       J� IsHitTestVisible .  $
��Left=�61   $��24��6L   $
��Wrap=�6X   $��
Логин��5      $	��Top=�6!   $	��200��69   $��18(�6E   
�������$	 572��6e   $	 319��6w   ��5      �� 5      	 �� 
TargetType"	 ��6   .�5      �� 
 �� Property
 � y� �� Value$ 	Collapsed=�67    �� Triggers 5      �� 5       �� Binding 6*   �� *Text+ �� ElementName$ tbLogin�� �� Value  6X   .a�5   "   �� 
 � y�$ Visible=�6?   6Q   5      5      5      G� 5      -   5         G�Name$ 
pbPassword��5      $��Center=�6.   $��40��6K    G� Password$  ��6W    G� 	MaxLength$ 16��6c   $��Center=�5       $��Center=�6(   $	��297��6J   $��18(�6V   $	��2pxq�5!      $��11 0 0 0q�6#   $	 557��6X   $	 370��6j   �� 5"      -   5         ��Name$ waterMarkPassword��5"       .  $
��Left=�6L   $��24��6g   $
��Wrap=�6s   $��Пароль��5#      $	��Top=�6   $	��200��63   $��18(�6?   
�������$	 572��6_   $	 377��6q   �� 5$      -   5        $"��Авторизоваться��5$      $
��Left=�6.   $��34��6I   $	��Top=�6U   $	��139��6m   $��14(�5%      $	 633��6@   $	 440��6R   5&      5(      5)      
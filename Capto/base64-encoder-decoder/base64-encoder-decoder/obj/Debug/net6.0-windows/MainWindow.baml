   M S B A M L   `   `   `  ����Q  Mbase64-encoder-decoder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:base64_encoder_decoderbase64_encoder_decoder  5   	   '    !base64_encoder_decoder.MainWindow   5   	   W SPresentationCore, Version=6.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   6local$clr-namespace:base64_encoder_decoder     5   	     � Title$  Base64 Encoder/Decoder��5   	   $	��780��6(   $
��1200��65   .��5	      �  � RowDefinitions 5
   
   �� 5      $	��350 �6   �� 5      $	��350 �6   �� 5      $��30 �6   .+�5   
   �� $��0��6   $��2��6"   .+�5      �� $��Input:��6   �� 5      -   5         ��Name$ tbInput��5   i   $	��300��6   $��10q�6#   $��1q�6/   " �� VerticalScrollBarVisibility$ Visible=�6C    �� TextWrapping$
 Wrap=�6z    �� AcceptsReturn$
 True��6�   5      �� 5   
   $��1��6   $��2��6"   .+�5      �� $��Output:��6   �� 5      -   5        $ tbOutput��5   i   $	��300��6   $��10q�6#   $��1q�6/   $ Visible=�6C   $
 Wrap=�6{   $
 True��6�   5      �� 5   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��1��6W   .+�5      �� -   5         ��Name$ 	btnEncode��5      $��Encode��6(   $	��100��69   6E   �� 5      -   5        $ 	btnDecode��5      $��Decode��6(   $	��100��69   $��	20,0,20,0q�6E   6X   5      5      5      
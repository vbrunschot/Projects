   M S B A M L   `   `   `  ���� I  EMassScannerWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:MassScannerWPFMassScannerWPF      MassScannerWPF.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:MassScannerWPF       � Title$  MassScanner��$	��790��$
��1200��$	��790��$
��1200��.���  � RowDefinitions �� $��60 ��� $��* ��� $��25 ��� $��40 �.+�� $��0��.+���  �� Orientation$ 
Horizontal=�$��Center=�.+��� $��/Resources/icon.png��$��50��$��Center=��� $��MassScanner��$��Center=�$��Center=�$��Bahnschrift SemiBold)�$��48(�$
��Bold"�����    
OrangeFont �� Color# �   � $��2��.+��� $ 
Horizontal=�.+��� -    ��Name$ 	txtStatus��$��	195,0,0,0q�$��Bottom=����� # �   �� -   $ txtHosts��$��2,0,0,0q�$��Bottom=����� # �   �� $��Right=�$��Bottom=�$ 
Horizontal=�.+��� -    ��Name$ 	btnImport��$��Import��$��60��$��20����Z� ���� # �   ���� # �   �� -   $ 	btnExport��$��Export��$��5,0,10,0q�$��60��$��20����Z� ���� # �   ���� # �   � $��3��.+�� -    �Name$ progressBar��$��30��$��	10,0,10,0q��� $��Center=�$��Center=�� $��1�� � ColumnDefinitions �� $	��194 ��� $	��350 ��� $��* �.+�� $��0��.+��� $ Vertical=�.+��� -    ��Name$ brdPingSweep��$��1q�$	��170��$	��180��	 �� CornerRadius$	 4��$	��Top=����� # �   ._��� $ Vertical=�$��5q�$��Center=�$��Center=�.+��� $'��!Initiate a ping sweep on IP range��$��10(�$��0,0,0,5q�$��Center=����� # �   �� $ 
Horizontal=�$	��Top=�$��Center=�$��22��.+��� -   
 ��Name$
 txtSubnet1start��$��35��$	��192�� �� TextAlignment$ Center=�.������ # �   .���� -   $
 txtSubnet2start��$��35��$	��168��$ Center=�$��5,0,0,0q�.������ # �   .���� -	   $
 txtSubnet3start��$��35��$��0��$ Center=�$��5,0,0,0q�.������ # �   .���� -
   $
 txtSubnet4start��$��35��$��0��$ Center=�$��5,0,0,0q�.������ # �   .���� $ 
Horizontal=�$	��Top=�$��Center=�$��22��$��0,5,0,0q�.+��� $��35�� �� TextAlignment$ Center=�����  �� ElementName$ txtSubnet1start�� �� Path$
 Text� �� UpdateSourceTrigger$ PropertyChanged=����� # �   �� $��35��$ Center=�$��5,0,0,0q����� $ txtSubnet2start��$
 Text�$ PropertyChanged=����� # �   �� -   $
 txtSubnet3end��$��35��$	��255��$ Center=�$��5,0,0,0q�.������ # �   .���� -   $
 txtSubnet4end��$��35��$	��255��$ Center=�$��5,0,0,0q�.������ # �   .���� $ Vertical=�$��0,20,0,0q�.+��� -    ��Name$ chbRandomize��$��Randomize host order�� d� 	IsChecked$
 TrueY�$��Center=�$��11(����� # �   �� -   $ btnPingSweep��$��
Ping Sweep��$��30��$	��150��$��0,5,0,0q�
����������� # �   �� -   $ brdPortScan��$��1q�$	��160��$	��180��$	 4��$	��Top=�$��0,10,0,0q�{�.  ���� # �   ._��� $ Vertical=�$��5q�$��Center=�$��Center=�.+��� $,��&Initiate portscans on discovered hosts��$��10(�$��0,0,0,5q����� # �   �� $ Vertical=�$��Center=�.+��� $��Port:��$��Center=�$��Center=��� -   $
 txtPort��$��60��$��22��$��0,5,0,0q�$ Center=�$�� ��.������ # �   .���� -   $ chbGrabBanner��$��Grab banner��$ FalseY�$��Center=�$��11(�$��0,20,0,0q����� # �   �� -   $ btnPortScan��$��	Port Scan��$��30��$	��150��$��0,5,0,0q����	#00000000{�.  ���� # �   �� -   $ brdNmap��$��1q�$	��276��$	��180��$	 4��$	��Top=�$��0,10,0,0q�{�.  ���� # �   ._��� $ Vertical=�$��5q�$��Center=�$��Center=�.+��� $+��%Initiate Nmap scans on selected hosts��$��10(�$��0,0,0,5q�$��Center=����� # �   �� $ Vertical=�$��Center=�.+��� $��Nmap arguments:��$ Center=��� -   $
 txtNmapArgs��$	��160��$��22��$ Center=�$��
-sV -O -T5��$��0,5,0,0q�.������ # �   .��	� -    	�Name$ rdbRunOnPortscan��$��Use portscan results��$
 TrueY�$��	10,10,0,0q����� # �   	� -   $ rdbRunOnPingSweep��$��Use ping sweep results��$��10,0,0,0q����� # �   �� $��or imported hosts��$��30,0,0,0q��� -   $ 	chbScript��$��Run exploit scripts��$ FalseY�$
��Left=�$��11(�$��	25,20,0,0q����� # �   �� -   $ 
chbVersion��$��Version detection��$
 TrueY�$
��Left=�$��11(�$��25,5,0,0q����� # �   �� -   $ chbOSFingerprint��$��OS fingerprinting��$
 TrueY�$
��Left=�$��11(�$��25,5,0,0q����� # �   �� -   $ chbHideWindow��$��Hide window(s)��$
 TrueY�$
��Left=�$��11(�$��25,5,0,0q����� # �   �� -   $ btnNmapScan��$��	Nmap Scan��$��30��$	��150��$��0,10,0,0q�
����������� # �   � $��1��.+��� $��1q�$	 4������ # �   ._��� -    ��Name$ 	lbResults��$��0q�$��5q�� $��2��$��5,0,5,0q�.+��� $��1q�$	 4������ # �   ._��� -   $ lbNmap��$��0q�$��5q����� # �   
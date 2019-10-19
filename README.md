# BIDS_Server
BIDSの情報を様々な方法でで外部に送信するための中継役。

## プロジェクト紹介
- BIDS_Server
ユーザーがBIDS_Serverの機能を使用する際に使用する、CUIアプリケーションです。  

- BIDSsv.communication
通信モジュールの1つです。  
Rock_On氏が開発したcommunication.dllに実装された構造体と同様の構造体および通信方式を定義し、communication.dll対応ソフトウェアに対する情報送信、あるいはcommunication.dllとの対話が可能となります。  
BIDS規格の通信もサポート予定です。(ByteArrayの自動同期機能は使用不可)

- BIDSsv.serial
通信モジュールの1つです。
Arduino等とシリアル通信による対話を可能とします。ポート番号やBaudRateの変更に対応し、汎用性が高くなっています。  
GIPI、およびBIDSid_SerConの代替品となります。

- BIDSsv.tcpcl(開発中)
通信モジュールの1つです。  
BIDSsv.tcpsvを読み込んだBIDS_Serverとセットで用います。  

- BIDSsv.tcpsv(開発中)
通信モジュールの1つです。  
BIDSsv.tcpclを読み込んだBIDS_Serverとセットで用います。

-BIDSsv.udp(開発中)
通信モジュールの1つです。
マルチドロップにより、複数のマシンとの情報共有を可能とします。

- BIDSsv
BIDS_Serverの機能を実装しているライブラリです。ほぼすべての機能はここに実装されており、このライブラリおよび依存関係をもつライブラリを参照することにより、GUIアプリケーションの開発も可能となります。

- IBIDSsv
通信モジュールに必要とされるインターフェースを実装したライブラリです。このライブラリのインターフェースを継承して実装することにより、誰でも通信モジュールを開発することが可能となります。

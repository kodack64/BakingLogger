# BakingLogger
ベーキング関連の道具のモニタリング用のスクリプトです。  
熱電対のモニタである[Omega HH309](http://www.jp.omega.com/pptst/HH309.html)と[Pico Data Logger TC-08](http://akizukidenshi.com/catalog/g/gM-01527/)、および真空計の[M-601GC](https://www.canon-anelva.co.jp/products/component/vacuum/vac_detail08.html)からデータを取得するツールです。
またpythonで監視して定期的に共有フォルダにアップロードすることで深夜も様子をモニタリングできます。

./BakingDataAcquire : C#で書いたRS-232Cのインターフェイスを介して通信して値を取得するプログラム  
./script : pythonで書いた取得ログのプロットと自動アップロードのスクリプト

ちなみにOmegaHH309は購入しても専用のソフトウェアがついているだけで通信プロトコルは公開されていませんが、[類似した製品から温度を取り出す方法](https://forums.ni.com/t5/LabVIEW/Temperature-reading-from-OMEGA-data-logger-thermometer-model/td-p/1899569)が判明しています。

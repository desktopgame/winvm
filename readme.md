# winvm
レジストリ編集ツールです。

# sample
現在のレジストリの中身をダンプする
````
winvm save save_file.text
winvm save --grepKey HID save_file.txt
winvm save --grepAttr Flip save_file.txt
````

ダンプ時のフォーマット
````
@name
レジストリのキー
@begin-attr-list
@begin-attr
キー
値の種類
値
@end-attr
@end-attr-list
...
````

ダンプファイルを使用して実際のレジストリを変更する
````
winvm load save_file.text
````

# サポートされているデータ型
* RegistryValueKind.DWord

# マウススクロールを逆にする
[参考リンク](https://pc-karuma.net/boot-camp-windows-scroll-natural/)
````
winvm save --grepAttr FlipFlop save_file.txt

save_file.txtの FlipFlopWheel/FlipFlopHScroll を全て 1 にする

winvm load save_file.txt
````
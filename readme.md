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
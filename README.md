# TitalyverMessengerForSpotify

Spotifyの再生状態をTitalyver2に送るアプリ。<BR>
ソースに埋め込まれているクライアントIDはデベロッパーモードなので、そのままビルドしただけではアカウント認証できないはずです。
ビルドする気があるのなら自分でSpotify for DevelopersからクライアントIDを作ってください。

## 使用ライブラリ
* https://github.com/JohnnyCrazy/SpotifyAPI-NET<BR>
いろいろ楽できる。OAuth2とか特に。ただちょっとDLLでかい。特にNewtonsoft.Json。.Net5じゃ使わないし。<BR>
Spotifyから情報をもらうときに結構重要な、Request HeaderのAccept-Languageを設定するのが結構めんどくさい。まあ出来る手段があるだけましか。<BR>
もし別アプリじゃなくてiTunes用みたいにTitalyverに動的DLLでくっつくようにしようとすると、必要な部分を自前で実装しないといけないか。<BR>

## 困ること
* 状態変化を知りたいな<BR>
https://qiita.com/tkcm/items/91908dddba7399b7ce77

* むしろ操作できるようにする<BR>
APIからの再生状態変更操作系はプレミアムアカウントに対してしかできない。
何故か（Spotify以外で）できるやつもいるけど（Musixmatchとか）。特別なクライアントIDかアクセストークンか何かか？<BR>
これで困るのはむしろSpotify音源で歌詞のタグ打ちする計画の方だが。
というかこの制限でプチリリメーカーにプレミアムが要るのか？
  
  

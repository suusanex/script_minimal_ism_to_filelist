# 基本の指示

InstallShieldのismファイルを読み込んで、インストール対象のファイルリストを出力するスクリプトを作成します。

ファイルリストは、各ファイル（行）に対して次の要素（列）を持ちます。

1. ファイル名
1. ビルド用ファイルの相対パス（インストーラのビルド元）

InstallShieldの全ての使用に対応するものではなく、下記のように単純化した読み込みルールでの読み込みを行います。

dotnet-scriptで実行するため、全ての処理はmain.csxファイル内に記述し、NuGetパッケージの参照もそのファイル内に記載します。

入力ファイルパスと出力ファイルパスを実行時引数に受け取り、それを処理に使用します。

入力ファイルパスのismファイルを読み込んで、出力ファイルパスへファイルリストを出力します。ファイルリストはCSV形式のUTF-8テキストファイルとします。

## 読み込みルール

### 共通

ismファイルは、XML形式のUTF-8テキストファイルである。

### ファイルリスト

XMLのルート直下の次の要素の中に、ファイルの配列がある。 `<table name="File"></table>`

ファイルは、次のタグが1つのファイルを示す。 `<row></row>`

1つのファイルは、`<td></td>`タグを複数持つ。

1つのファイルの内、`<td></td>`タグの3つ目が、ファイル名を示す。ファイル名は|で区切られている場合があり、区切られている場合は最後の要素をファイル名とする。

1つのファイルの内、`<td></td>`タグの9つ目が、ビルド用ファイルの相対パスを示す。このパスは`<XXXXX>`のような環境変数を持つ場合がある。この場合、別章に示す環境変数のリストを検索し、環境変数と対応した実際の相対パスへ変換する必要がある。この例の場合、XXXXXは環境変数名である。

### 環境変数リスト

XMLのルート直下の次の要素の中に、環境変数の配列がある。 `<table name="ISPathVariable"></table>`

環境変数は、次のタグが1つの環境変数を示す。 `<row></row>`

1つの環境変数は、`<td></td>`タグを複数持つ。

1つの環境変数の内、1つ目の`<td></td>`タグが、環境変数名を示す。

1つの環境変数の内、2つ目の`<td></td>`タグが、環境変数の値を示す。ただし値が数値もしくは空の場合があり、その場合は環境変数名を<と>で囲んだものを値とする。（例:環境変数名が`ISProjectFolder`で値が空の場合は、値を`<ISProjectFolder>`とする）

環境変数の値は、別の環境変数を使用している場合がある。この場合、リストを作成する時に環境変数の解決（環境変数名から環境変数の値への変換）を行う必要がある。



# コミット数をバージョンにして _Version.props を作成する

$baseVersion = "4.0"

# 現在のスクリプトの場所を得る
$path = Split-Path $MyInvocation.MyCommand.Path -Parent

# 現在のブランチのコミットカウントを得る
$branch = git -C $path rev-parse --abbrev-ref HEAD
$commitCount = git -C $path rev-list --count $branch

# バージョン文字列生成
$version = "$baseVersion.$commitCount"
Write-Host $version

# _Version.props を作成
$xml = [xml]"<Project><PropertyGroup><VersionPrefix/></PropertyGroup></Project>"
$xml.Project.PropertyGroup.VersionPrefix = $version
$xml.Save("$path\_Version.props")

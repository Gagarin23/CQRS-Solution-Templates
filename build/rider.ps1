# Получение пути к родительской папке папки build
$buildParentPath = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Definition)

# Настройка относительных путей
$sourceRelativePath = "src" # Относительный путь к исходному проекту относительно родителя папки build
$destinationRelativePath = "RiderTemplate" # Относительный путь к папке шаблона относительно родителя папки build

# Конвертация относительных путей в абсолютные
$sourcePath = Join-Path $buildParentPath $sourceRelativePath
$destinationPath = Join-Path $buildParentPath $destinationRelativePath
$templateConfigPath = Join-Path $destinationPath ".template.config"

# Предварительная очистка целевой папки
if (Test-Path $destinationPath) {
    Remove-Item -Path "$destinationPath\*" -Recurse -Force
} else {
    New-Item -ItemType Directory -Path $destinationPath
}

# Копирование файлов проекта с сохранением структуры папок
Copy-FilesWithFolders -sourceFolder $sourcePath -destFolder $destinationPath -filter "*.csproj"
Copy-FilesWithFolders -sourceFolder $sourcePath -destFolder $destinationPath -filter "*.cs"
Copy-FilesWithFolders -sourceFolder $sourcePath -destFolder $destinationPath -filter "*.json"
Copy-FilesWithFolders -sourceFolder $sourcePath -destFolder $destinationPath -filter "Dockerfile*"

# Создание каталога .template.config, если он не существует
if (-not (Test-Path $templateConfigPath)) {
    New-Item -ItemType Directory -Path $templateConfigPath
}

# Создание файла template.json
$templateJsonContent = @"
{
  "author": "Belov Ilya",
  "name": "ASP.NET_CQRS_Solution",
  "description": "ASP.NET Clean Architecture + CQRS Solution with MediatR.",
  "identity": "BelovIlya.CQRSSolution.8.0",
  "shortName": "cqrssolution",
  "tags": {
    "language": "C#",
    "type": "project"
  },
  "sourceName": "MyProject",
  "symbols": {
    "Framework": {
      "type": "parameter",
      "description": "The target framework for the project.",
      "datatype": "choice",
      "choices": [
        {
          "choice": "net8.0"
        }
      ],
      "defaultValue": "net8.0"
    }
  }
}
"@

$templateJsonPath = Join-Path $templateConfigPath "template.json"
$templateJsonContent | Out-File -FilePath $templateJsonPath

# Вывод завершения
Write-Host "Шаблон проекта успешно создан в $destinationPath"

# Функция для копирования файлов с сохранением структуры папок и исключением определенных папок
function Copy-FilesWithFolders {
  param(
      [string]$sourceFolder,
      [string]$destFolder,
      [string]$filter
  )

  $excludeDirs = @('bin', 'obj', '.idea', '.vs') # Папки для исключения
  $files = Get-ChildItem -Path $sourceFolder -Filter $filter -Recurse -File | Where-Object {
      foreach ($dir in $excludeDirs) {
          if ($_.FullName -like "*\$dir\*") {
              return $false
          }
      }
      return $true
  }

  foreach ($file in $files) {
      $destPath = $file.FullName.Replace($sourceFolder, $destFolder)
      $destDir = Split-Path -Parent $destPath
      if (-not (Test-Path $destDir)) {
          New-Item -ItemType Directory -Path $destDir | Out-Null
      }
      Copy-Item -Path $file.FullName -Destination $destPath
  }
}
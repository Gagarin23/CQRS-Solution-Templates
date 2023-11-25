# Функция для копирования файлов с сохранением структуры папок и исключением определенных папок
function Copy-FilesWithFolders {
    param(
        [string]$sourceFolder,
        [string]$destFolder
    )

    $excludeDirs = @('bin', 'obj', '.idea', '.vs') # Папки для исключения
    $includeExtensions = @('.csproj', '.cs', 'Dockerfile', '.json') # Расширения файлов для включения

    Get-ChildItem -Path $sourceFolder -Recurse -File | Where-Object {
        $fileExt = $_.Extension
        $filePath = $_.FullName
        $shouldInclude = $false

        foreach ($ext in $includeExtensions) {
            if ($fileExt -eq $ext -or $_.Name -like $ext) {
                $shouldInclude = $true
                break
            }
        }

        $shouldExclude = $excludeDirs | Where-Object { $filePath -like "*\$_\*" } | Measure-Object | Select-Object -ExpandProperty Count
        $shouldInclude -and ($shouldExclude -eq 0)
    } | ForEach-Object {
        $destPath = $_.FullName.Replace($sourceFolder, $destFolder)
        $destDir = Split-Path -Parent $destPath
        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir | Out-Null
        }
        Copy-Item -Path $_.FullName -Destination $destPath
    }
}

# Функция для создания корневого .vstemplate файла
function Create-RootVSTemplate {
    param (
        [string]$solutionPath,
        [string[]]$projectNames
    )

    $templateStart = @"
<VSTemplate Version="3.0.0" Type="ProjectGroup" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005">
  <TemplateData>
    <Name>CQRSTemplate</Name>
    <Description>ASP.NET Clean Architecture + CQRS Solution with MediatR.</Description>
    <ProjectType>CSharp</ProjectType>
    <SortOrder>1000</SortOrder>
    <CreateNewFolder>true</CreateNewFolder>
    <DefaultName>CQRSTemplate</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
  </TemplateData>
  <TemplateContent>
    <ProjectCollection>
"@

    $templateEnd = @"
    </ProjectCollection>
  </TemplateContent>
</VSTemplate>
"@

    $projectTemplateLinks = $projectNames | ForEach-Object {
        "<ProjectTemplateLink ProjectName=`"$_`">$_\$_.vstemplate</ProjectTemplateLink>"
    }

    $rootTemplateContent = $templateStart + ($projectTemplateLinks -join "`n") + $templateEnd
    $rootTemplatePath = Join-Path "$solutionPath" "CQRSTemplate.vstemplate"
    $rootTemplateContent | Out-File -FilePath $rootTemplatePath
}

# Функция для создания содержимого .vstemplate файла
function Create-VSTemplateContent {
    param (
        [string]$projectName,
        [string]$projectPath
    )

    function Create-FolderTemplate {
        param (
            [string]$path,
            [string]$relativePath = ''
        )

        $folderTemplate = ''
        $items = Get-ChildItem -Path $path

        foreach ($item in $items) {
            $itemRelativePath = if ($relativePath -eq '') { $item.Name } else { Join-Path $relativePath $item.Name }

            if ($item -is [System.IO.DirectoryInfo]) {
                # Рекурсивный вызов для папки
                $subFolderTemplate = Create-FolderTemplate -path $item.FullName -relativePath $itemRelativePath
                $folderTemplate += "<Folder Name=`"$($item.Name)`" TargetFolderName=`"$($item.Name)`">$subFolderTemplate</Folder>"
            } elseif ($item -is [System.IO.FileInfo]) {
                $folderTemplate += "<ProjectItem ReplaceParameters=`"true`" TargetFileName=`"$item`">$item</ProjectItem>"
            }
        }

        return $folderTemplate
    }

    $vstemplateContent = @"
<VSTemplate Version="3.0.0" xmlns="http://schemas.microsoft.com/developer/vstemplate/2005" Type="Project">
  <TemplateData>
    <Name>$projectName Template</Name>
    <Description>Template $projectName</Description>
    <ProjectType>CSharp</ProjectType>
    <ProjectSubType></ProjectSubType>
    <SortOrder>1000</SortOrder>
    <CreateNewFolder>true</CreateNewFolder>
    <DefaultName>$projectName</DefaultName>
    <ProvideDefaultName>true</ProvideDefaultName>
    <LocationField>Enabled</LocationField>
    <EnableLocationBrowseButton>true</EnableLocationBrowseButton>
    <Icon>TemplateIcon.ico</Icon>
    <CreateInPlace>true</CreateInPlace>
  </TemplateData>
  <TemplateContent>
    <Project TargetFileName="$projectName.csproj" File="$projectName.csproj" ReplaceParameters="true">
      $(Create-FolderTemplate -path $projectPath)
    </Project>
  </TemplateContent>
</VSTemplate>
"@
    return $vstemplateContent
}


# Определение путей к проектам в решении и создание шаблонов
$projectNames = @('Api', 'Application', 'DatabaseMigrator', 'Domain', 'Infrastructure')
$buildParentPath = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Definition)
$destinationPath = Join-Path $buildParentPath "VisualStudioTemplate"

# Предварительная очистка целевой папки
if (Test-Path $destinationPath) {
    Remove-Item -Path "$destinationPath\*" -Recurse -Force
} else {
    New-Item -ItemType Directory -Path $destinationPath
}

foreach ($projectName in $projectNames) {
    $sourcePath = Join-Path "$buildParentPath\src" $projectName
    $destinationPath = Join-Path $buildParentPath "VisualStudioTemplate\temp\$projectName"

    # Предварительная очистка и копирование файлов проекта
    if (Test-Path $destinationPath) {
        Remove-Item -Path "$destinationPath\*" -Recurse -Force
    } else {
        New-Item -ItemType Directory -Path $destinationPath
    }
    Copy-FilesWithFolders -sourceFolder $sourcePath -destFolder $destinationPath -filter "*.*"

    # Создание содержимого файла .vstemplate
    $vstemplateContent = Create-VSTemplateContent -projectName $projectName -projectPath $destinationPath
    $vstemplatePath = Join-Path $destinationPath "$projectName.vstemplate"
    $vstemplateContent | Out-File -FilePath $vstemplatePath
}

# Создание корневого .vstemplate файла
Create-RootVSTemplate -solutionPath "$buildParentPath\VisualStudioTemplate\temp\" -projectNames $projectNames

# Путь для создания ZIP-архива
$zipDestinationPath = Join-Path "$buildParentPath\VisualStudioTemplate" "CQRSTemplate.zip"

# Создание ZIP-архива
Compress-Archive -Path "$buildParentPath\VisualStudioTemplate\temp\*" -DestinationPath $zipDestinationPath

if (Test-Path $destinationPath) {
    Remove-Item -Path "$buildParentPath\VisualStudioTemplate\temp\*" -Recurse -Force
}

# Вывод завершения
Write-Host "Шаблоны Visual Studio и корневой .vstemplate успешно созданы и архивированы в $zipDestinationPath"

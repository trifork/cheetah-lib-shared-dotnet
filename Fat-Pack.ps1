
<#
.SYNOPSIS
Generates a fat NuGet package including all (both direct and nested) project references as '.dll's and all downstream package references.
.EXAMPLE
./Fat-Pack -ProjectPath "./Example.csproj" -DestinationPath "./nuget"
.DESCRIPTION
This cmdlet takes an existing .csproj and packages it as a fat NuGet package. In this context "fat" refers to the inclusion of all referenced projects, direct as well as nested, as .dll's and all downstream package references as direct dependencies of the fat nuget package.
The purpose of this is to alleviate the need for publishing downstream projects as their own NuGet packages.

This enforces that all references packages exist only in a single version and will fail if differing versions of a package are defined in a project and any of its downstream dependencies.

Additionally, this enforces all included projects to have the "GenerateDocumentationFile" attribute set and enforces that the top-level project has the properties 'PackageId', 'PackageVersion', 'Authors' and 'Description'
#>

param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -Path $_ })]
    [ValidatePattern('\.csproj')]
    # The path of the .csproj to pack
    [string] $projectPath,
    [Parameter(Mandatory)]
    # The desired destination of the resulting NuGet package
    [string] $destinationPath,
    # Optional: The desired name of the temporary .csproj used to pack the resulting NuGet package. Defaults to the name of the input with a suffixed .flat, e.g. Test.csproj -> Test.flat.csproj
    [string] $outputFileName,
    # Whether or not to keep the resolved .csproj
    [switch] $keepResolvedProject,
    # Whether or not to allow overwriting an existing file in the location that the resolved .csproj should be placed.
    [switch] $allowOverwrite
)

$ErrorActionPreference = "stop"

function Get-FileAsXml {
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory)]
        [ValidateScript({ Test-Path -Path $_ })]
        [string] $filePath
    )
    [System.Xml.XmlDocument] $xml = Get-Content $filePath
    $xml
}

function Get-RelativeFilePath {
    [OutputType([string])]
    param(
        [parameter(Mandatory)]
        [string] $filePath,
        [parameter(Mandatory)]
        [string] $relativeTo
    )

    if(-not (Test-Path -Path $filePath -PathType Leaf)){
        Write-Error "Attempted to call Get-RelativeFilePath with a non-leaf path: '$filePath'."
    }

    if(-not (Test-Path -Path $relativeTo -PathType Container)){
        $relativeTo = Split-Path $relativeTo
        Write-Warning "Attempted to get file path relative to a non-directory path. Returning file path relative to parent directory of supplied file: '$relativeTo'"
    }

    [System.IO.Path]::GetRelativePath($relativeTo, $filePath)
}

function Get-ProjectReferences {
    [OutputType([System.Xml.XmlElement[]])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    $xmlDocument.Project.GetElementsByTagName("ProjectReference")
}

function Get-PackageReferences {
    [OutputType([System.Xml.XmlElement[]])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    $xmlDocument.Project.GetElementsByTagName("PackageReference")
}

function Get-ReferencedProjects
{
    [OutputType([PSCustomObject[]])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument,
        [switch] $recurse
    )

    $projectReferences = Get-ProjectReferences $xmlDocument

    $projectReferences | ForEach-Object { 
        $projectReference = $_
        $referencePath = $projectReference.GetAttribute("Include")

        # Resolve the referenced project path relative to the directory of our project.
        $referencePath =  Join-Path $(Split-Path $projectPath) $referencePath -Resolve
        $referencedDocument = Get-FileAsXml $referencePath

        if($recurse){
            # Recursively find and return nested project references, so that we can roll it up to the parent.
            $referencedDocument | Get-ReferencedProjects -Recurse
        }

        [PSCustomObject]@{
            FilePath = $referencePath
            Document = $referencedDocument
        }
    }
}

function Add-ItemGroupWithComment {
    [OutputType([System.Xml.XmlElement])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument,
        [parameter(Mandatory)]
        [string] $comment
    )

    $itemGroup = $xmlDocument.CreateElement("ItemGroup")
    $xmlDocument.Project.AppendChild($itemGroup)

    $commentNode = $xmlDocument.CreateComment($comment)
    $itemGroup.AppendChild($commentNode) | Out-Null
    $itemGroup
}

function Add-NestedProjectReferences {
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    # Find the difference between immediate references and nested references
    $directProjectReferencePaths = $xmlDocument | Get-ReferencedProjects | Select-Object -ExpandProperty FilePath
    $nestedProjectReferencePaths = $xmlDocument | Get-ReferencedProjects -Recurse | Select-Object -ExpandProperty FilePath
    
    $missingProjectReferences = $nestedProjectReferencePaths | Where-Object { $directProjectReferencePaths -notcontains $_ }
    
    if($missingProjectReferences.Count -gt 0){
        # Add all indirect project references as direct references
        $itemGroup = $xmlDocument | Add-ItemGroupWithComment -Comment "These project references were imported because of nested project references in an original project reference"

        $missingProjectReferences | ForEach-Object {
            $filePath = $_
            $relativePath = Get-RelativeFilePath -FilePath $filePath -RelativeTo (Split-Path $projectPath)

            $referenceNode = $xmlDocument.CreateElement("ProjectReference")
            $referenceNode.SetAttribute("Include", $relativePath)

            $itemGroup.AppendChild($referenceNode) | Out-Null
        }
    }

    $xmlDocument    
}

function Set-PrivateAssetsAttributeOnReferencedProjects
{
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    # This allows our additional build target added in "Add-AdditionalBuildTarget" to include output of the project reference as a .dll rather than a NuGet reference
    $xmlDocument 
    | Get-ProjectReferences
    | ForEach-Object { $_.SetAttribute("PrivateAssets", "All") }

    $xmlDocument
}

function Add-ReferencedPackagesFromReferencedProjects
{
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    $referencedProjects = $xmlDocument | Get-ReferencedProjects

    $referencedProjects | ForEach-Object {
        $projectFileName = Split-Path $_.FilePath -Leaf
        $projectDocument = $_.Document
        $itemGroup = $xmlDocument | Add-ItemGroupWithComment -Comment "These Package reference were imported from '$projectFileName'"

        $projectDocument 
        | Get-PackageReferences 
        | ForEach-Object { $itemGroup.AppendChild($xmlDocument.ImportNode($_, $true)) | Out-Null }
    }

    $xmlDocument
}

function Confirm-PackageReferenceUniqueness {
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    $packageGroupsWithMultipleVersions = @($xmlDocument 
    | Get-PackageReferences 
    | Group-Object { $_.GetAttribute("Include") }
    | Where-Object {
        # Find all groups of package references that have more than 1 unique version
        $uniqueVersions = $_.Group | ForEach-Object { $_.GetAttribute("Version")} | Select-Object -Unique
        $uniqueVersions.Count -gt 1
     })

    if($packageGroupsWithMultipleVersions.Count -gt 0){
        # Write warnings for each package with multiple versions
        $packageGroupsWithMultipleVersions | ForEach-Object {
            $packageName = $_.Name
            $versions = $_.Group | ForEach-Object { $_.GetAttribute("Version")}
            Write-Warning "Found $($versions.Count) versions of '$packageName' in included projects: $($versions | Join-String -SingleQuote -Separator ', ')"
        }

        # Summarize and fail
        Write-Error "Found $($packageGroupsWithMultipleVersions.Count) packages included in more than 1 version. See other logs for details."
    }

    $xmlDocument
}

function Remove-PackageReferenceDuplicates {
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    $xmlDocument 
    | Confirm-PackageReferenceUniqueness                  # Ensure we have only a single version
    | Get-PackageReferences 
    | Group-Object { $_.GetAttribute("Include") }
    | Where-Object { $_.Count -gt 1 }                     # Find groups of packages that have duplicate references
    | ForEach-Object { $_.Group | Select-Object -Skip 1 } # Skip the first
    | ForEach-Object { $_.ParentNode.RemoveChild($_) }    # Delete the rest
    | Out-Null                                            # Silence the output of Remove-Child

    $xmlDocument
}


function Add-AdditionalBuildTarget
{
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    # This XML is injected into the modified .csproj and is used to make MSBuild include project references with
    # the "PrivateAssets = All" attribute as a .dll, rather than a reference to another nuget package. If you want to use this elsewhere, make sure to remove the '`' escape before $(TargetsForTfmSpecificBuildOutput)
    [xml] $BUILD_TARGET_XML = @"
<Project>
    <PropertyGroup>
        <TargetsForTfmSpecificBuildOutput>`$(TargetsForTfmSpecificBuildOutput);CopyProjectReferencesToPackage</TargetsForTfmSpecificBuildOutput>
    </PropertyGroup>

    <Target Name="CopyProjectReferencesToPackage" DependsOnTargets="BuildOnlySettings;ResolveReferences">
        <ItemGroup>
            <!-- Filter out unnecessary files -->
            <_ReferenceCopyLocalPaths Include="@(ReferenceCopyLocalPaths-&gt;WithMetadataValue('ReferenceSourceTarget', 'ProjectReference')-&gt;WithMetadataValue('PrivateAssets', 'All'))" />
        </ItemGroup>
        
        <ItemGroup>
            <!-- Add file to package with consideration of sub folder. If empty, the root folder is chosen. -->
            <BuildOutputInPackage Include="@(_ReferenceCopyLocalPaths)" TargetPath="%(_ReferenceCopyLocalPaths.DestinationSubDirectory)" />
        </ItemGroup>
    </Target>
</Project>
"@

    $BUILD_TARGET_XML.Project.ChildNodes | ForEach-Object {
        $xmlDocument.Project.AppendChild($xmlDocument.ImportNode($_, $true)) | Out-Null
    }

    $xmlDocument
}

function Test-Node {
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument,
        [parameter(Mandatory)]
        [string] $tagName,
        [parameter(Mandatory)]
        [string] $projectName,
        [string] $requiredValue = ""
    )

    $node = $xmlDocument.GetElementsByTagName($tagName);

    if($node.Count -eq 0){
        Write-Warning "'$projectName' does not have a '$tagName' property"
        return $false
    }

    if($node.Count -gt 1){
        Write-Warning "Expected '$projectName' to only have a single '$tagName' property, but found $($node.Count)"
        return $false
    }

    $nodeText = $node.'#text'

    if($requiredValue -ne "" -and $nodeText.ToLower() -ne $requiredValue.ToLower()){
        Write-Warning "Expected '$projectName' to have a property with name '$tagName' to have value '$requiredValue', but found '$nodeText'"
        return $false
    }

    return $true
}

function Confirm-ProjectProperties {
    [OutputType([System.Xml.XmlDocument])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    $projectName = Split-Path -Path $projectPath -Leaf

    $nodeValidations =  @(
        ($xmlDocument | Test-Node -ProjectName $projectName -TagName "GenerateDocumentationFile" -RequiredValue "true"),
        ($xmlDocument | Test-Node -ProjectName $projectName -TagName "PackageId"),
        ($xmlDocument | Test-Node -ProjectName $projectName -TagName "Authors"),
        ($xmlDocument | Test-Node -ProjectName $projectName -TagName "Description")
    )


    if($nodeValidations -contains $false){
        Write-Error "The project is missing required metadata properties. See previous warning(s) for details"
    }

    $referencedProjectValidations = $xmlDocument | Get-ReferencedProjects | ForEach-Object {
        $document = $_.Document
        $fileName = Split-Path -Path $_.FilePath -Leaf
        $document | Test-Node -ProjectName $fileName -TagName "GenerateDocumentationFile" -RequiredValue "true" 
    }

    if($referencedProjectValidations -contains $false){
        Write-Error "One or more of the referenced projects are missing required properties. See previous warning(s) for details"
    }

    $xmlDocument
}

function Save-Project
{
    [OutputType([string])]
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [System.Xml.XmlDocument] $xmlDocument
    )

    if($outputFileName -eq ""){
        $outputFileName = $(Split-Path $projectPath -Leaf) -Replace ".csproj", ".fat.csproj"
    }

    $outputPath = Join-Path $(Split-Path($projectPath)) $outputFileName

    if($(Test-Path $outputPath) -and -not $allowOverwrite){
        Write-Error "Could not save modified .csproj to $outputPath, since it already exists. This can be resolved by removing the file, specifying a different output file name with -OutputFileName or enabling the -AllowOverwrite switch"
    }

    $xmlDocument.Save($outputPath);
    $outputPath
}

function Publish-Project {
    param(
        [parameter(Mandatory, ValueFromPipeline)]
        [string] $fatProject
    )

    Write-Host

    dotnet pack $fatProject -o $destinationPath | Out-Host
    $fatProject
}

function Remove-Project {
    param([parameter(Mandatory, ValueFromPipeline)]
    [string] $fatProject)

    if(-not $keepResolvedProject){
        Write-Host "Removing resolved project file $fatProject"
        Remove-Item $fatProject
    }
}

# Ensure the project path is absolute, to avoid dealing with messy relative paths
$projectPath = Resolve-Path $projectPath

Get-FileAsXml $projectPath
| Add-NestedProjectReferences
| Set-PrivateAssetsAttributeOnReferencedProjects
| Confirm-ProjectProperties
| Add-ReferencedPackagesFromReferencedProjects
| Remove-PackageReferenceDuplicates
| Add-AdditionalBuildTarget
| Save-Project
| Publish-Project
| Remove-Project
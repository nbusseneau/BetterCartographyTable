param(
  [Parameter(Mandatory = $true)][string]$tag,
  [Parameter(Mandatory = $true)][string]$githubToken,
  [Parameter(Mandatory = $true)][string]$thunderstoreToken
)

$ErrorActionPreference = "Stop"
function StopOnError { if (-not $?) { exit } }

# Make sure repo is up to date
git pull; StopOnError
git push; StopOnError

# Make sure package can build
dotnet build -c Release; StopOnError
tcli build; StopOnError

# Publish GitHub release
$owner = "nbusseneau"
$repo = "BetterCartographyTable"
$workflow = "release.yaml"

$headers = @{
  Authorization = [String]::Format("Bearer {0}", $githubToken)
}

$url = "https://api.github.com/repos/$owner/$repo/actions/workflows/$workflow/dispatches"
$body = @{
  ref    = "main"
  inputs = @{
    tag = $tag
  }
} | ConvertTo-Json
Write-Output "Sending request to workflow_dispatch | Tag: $tag"
Invoke-RestMethod -Method POST -Uri $url -Headers $headers -Body $body

$attempt = 0
do {
  $url = "https://api.github.com/repos/$owner/$repo/releases/latest"
  $release = Invoke-RestMethod -Uri $url -Headers $headers
  $attempt++
  Write-Output "Fetched latest release | Tag: $($release.tag_name) | Attempt: $attempt"
  if ($release.tag_name -ne $tag -and $attempt -ne 10) {
    Start-Sleep -Seconds 10
  }
}
until ($release.tag_name -eq $tag -or $attempt -eq 10)

if ($release.tag_name -ne $tag) {
  Write-Error -Message "Latest release not matching input tag, something went wrong with workflow_dispatch" -ErrorAction Stop
}

# Update repo following version rotation via release
git pull; StopOnError

# Publish package
dotnet build -c Release; StopOnError
tcli publish --token $thunderstoreToken; StopOnError

# Upload asset to Github release
$assetPath = (Get-ChildItem -Path "build/" -Filter "*$tag.zip").FullName
$url = "https://uploads.github.com/repos/$owner/$repo/releases/$($release.id)/assets?name=$assetName"
Write-Output "Uploading package to release | Asset name: $assetName"
Invoke-RestMethod -Method POST -ContentType "application/zip" -Uri $url -Headers $headers -InFile $assetPath

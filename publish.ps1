param(
  [Parameter(Mandatory = $true)][string]$tag,
  [Parameter(Mandatory = $true)][string]$githubToken,
  [Parameter(Mandatory = $true)][string]$thunderstoreToken
)

$owner = "nbusseneau"
$repo = "BetterCartographyTable"
$workflow = "release.yaml"
$packageName = "Better_Cartography_Table"

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
$response = Invoke-RestMethod -Method POST -Uri $url -Headers $headers -Body $body

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

git pull
dotnet build -c Release
tcli publish --token $thunderstoreToken
$assetName = "$owner-$packageName-$tag.zip"
$assetPath = "build/$assetName"

$url = "https://uploads.github.com/repos/$owner/$repo/releases/$($release.id)/assets?name=$assetName"
Write-Output "Uploading package to release | Asset name: $assetName"
$response = Invoke-RestMethod -Method POST -ContentType "application/zip" -Uri $url -Headers $headers -InFile $assetPath

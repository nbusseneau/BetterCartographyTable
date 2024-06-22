param(
  [Parameter(Mandatory = $true)][string]$tag,
  [Parameter(Mandatory = $true)][string]$githubToken,
  [Parameter(Mandatory = $true)][string]$thunderstoreToken
)

$ErrorActionPreference = "Stop"
function StopOnError { if (-not $?) { exit } }

$repo = (Select-String -Path "thunderstore.toml" -Pattern 'websiteUrl = "https://github.com/(.*)"').Matches.Groups[1]
$workflow = "release.yaml"

$headers = @{
  Authorization = [String]::Format("Bearer {0}", $githubToken)
}

$url = "https://api.github.com/repos/$repo/releases/latest"
try { $release = Invoke-RestMethod -Uri $url -Headers $headers }
catch { $statusCode = $_.Exception.Response.StatusCode.value__ }

# Default case: no existing release for this tag (or no release at all, resulting in a 404)
if ($release.tag_name -ne $tag -or $statusCode -eq "404") {
  Write-Output "Latest release $($release.tag_name) not matching input tag $tag, try to make new release"

  # Make sure repo is up to date
  git pull; StopOnError
  git push; StopOnError

  # Make sure package can build
  dotnet build -c Release; StopOnError
  tcli build; StopOnError

  # Trigger release workflow
  $url = "https://api.github.com/repos/$repo/actions/workflows/$workflow/dispatches"
  $body = @{
    ref    = "main"
    inputs = @{
      tag = $tag
    }
  } | ConvertTo-Json
  Write-Output "Sending workflow_dispatch request to $workflow workflow | Tag: $tag"
  Invoke-RestMethod -Method POST -Uri $url -Headers $headers -Body $body

  $attempt = 0
  do {
    $url = "https://api.github.com/repos/$repo/releases/latest"
    try { $release = Invoke-RestMethod -Uri $url -Headers $headers }
    catch { $statusCode = $_.Exception.Response.StatusCode.value__ }
    $attempt++
    if (($release.tag_name -ne $tag -or $statusCode -eq "404") -and $attempt -ne 10) {
      Start-Sleep -Seconds 10
    }
  }
  until ($release.tag_name -eq $tag -or $attempt -eq 10)
  if ($release.tag_name -ne $tag) {
    Write-Error -Message "Latest release $($release.tag_name) still not matching input tag $tag, something went wrong with $workflow workflow" -ErrorAction Stop
  }

  # Update repo following version rotation via release
  git pull; StopOnError
}

# Special case: existing release for this tag, i.e. we already ran this script before but it failed after creating the release
else {
  Write-Output "Latest release $($release.tag_name) matching input tag $tag, check if repo state is in sync with existing release"

  # Make sure repo is synced with release state
  git pull; StopOnError
  $currentHeadTag = git describe --exact-match HEAD; StopOnError
  if ($release.tag_name -ne $currentHeadTag) {
    Write-Error -Message "Latest release $($release.tag_name) not matching current HEAD tag $currentHeadTag, discrepancy between local repo and existing release state" -ErrorAction Stop
  }
}

# Build and publish package to Thunderstore
Write-Output -Message "Start publishing process"
dotnet build -c Release; StopOnError
tcli publish --token $thunderstoreToken; StopOnError

# Upload package to Github release
$package = (Get-ChildItem -Path "build\" -Filter "*$tag.zip")
$url = "https://uploads.github.com/repos/$repo/releases/$($release.id)/assets?name=$($package.Name)"
Write-Output "Uploading package to release | Asset name: $($package.Name)"
Invoke-RestMethod -Method POST -ContentType "application/zip" -Uri $url -Headers $headers -InFile $package.FullName

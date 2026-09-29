<#
.SYNOPSIS
    Captures the plugin log for a manual device check, with step markers.

.DESCRIPTION
    Begin  - raises the plugin's log level to Debug if needed, reloads the plugin and notes the
             time, so only this session's lines are collected. (The reload makes the plugin
             service start a new log file, so the session is found by timestamp, not position.)
    Mark   - records a timestamped marker (e.g. "3 open sources folder") so each step's log lines
             can be told apart. Run it just before doing the step on the device.
    End    - extracts the log lines written since Begin, merges in the markers, writes them to one
             file, prints a summary and restores the original log level.

    The output file is written under %TEMP%\OBSStudioForLogiDeviceCheck. Nothing is sent anywhere.

.EXAMPLE
    ./tools/device-check/device-check.ps1 Begin
    ./tools/device-check/device-check.ps1 Mark "1 idle scene switches"
    ./tools/device-check/device-check.ps1 End
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('Begin', 'Mark', 'End')]
    [String] $Action,

    [Parameter(Position = 1)]
    [String] $Note = ''
)

$ErrorActionPreference = 'Stop'

$LogPath = Join-Path $env:LOCALAPPDATA 'Logi\LogiPluginService\Logs\plugin_logs\OBSStudioForLogi.log'
$ConfigPath = Join-Path $env:APPDATA 'Loupedeck\OBSStudioForLogiPlugin\config.json'
$StateDir = Join-Path $env:TEMP 'OBSStudioForLogiDeviceCheck'
$StatePath = Join-Path $StateDir 'state.json'
$MarkersPath = Join-Path $StateDir 'markers.log'
$DebugLevel = 1   # LogLevel.Debug in src/Helpers/LogLevel.cs

# Same format as the plugin log's timestamps, so markers sort in among its lines.
function Get-LogTimestamp
{
    return (Get-Date).ToString('yyyy-MM-ddTHH-mm-ss-fff')
}

function Invoke-PluginReload
{
    Start-Process 'loupedeck:plugin/OBSStudioForLogi/reload'
}

# The plugin service keeps the log open, so read it with shared access.
function Read-Log
{
    if (-not (Test-Path $LogPath))
    {
        return @()
    }

    $stream = [System.IO.File]::Open($LogPath, 'Open', 'Read', 'ReadWrite')
    try
    {
        $reader = New-Object System.IO.StreamReader($stream)
        $text = $reader.ReadToEnd()
    }
    finally
    {
        $stream.Dispose()
    }

    return $text -split "`r?`n" | Where-Object { $_ -ne '' }
}

switch ($Action)
{
    'Begin'
    {
        New-Item -ItemType Directory -Force -Path $StateDir | Out-Null
        Set-Content -Path $MarkersPath -Value "$(Get-LogTimestamp) | MARK  | BEGIN"

        $state = @{
            BeginTime = Get-LogTimestamp
            ConfigExisted = Test-Path $ConfigPath
            OriginalLogLevel = $null
        }

        if ($state.ConfigExisted)
        {
            $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
            $state.OriginalLogLevel = $config.LogLevel
            if ($null -eq $config.LogLevel -or [Int32] $config.LogLevel -gt $DebugLevel)
            {
                $config | Add-Member -NotePropertyName LogLevel -NotePropertyValue $DebugLevel -Force
                $config | ConvertTo-Json | Set-Content -Path $ConfigPath
                Write-Host "Log level raised to Debug for this check."
            }
            else
            {
                Write-Host "Log level is already Debug or lower - leaving config.json unchanged."
            }
        }
        else
        {
            New-Item -ItemType Directory -Force -Path (Split-Path $ConfigPath) | Out-Null
            @{ LogLevel = $DebugLevel } | ConvertTo-Json | Set-Content -Path $ConfigPath
            Write-Host "No config.json found - created one with Debug logging; End removes it."
        }

        $state | ConvertTo-Json | Set-Content -Path $StatePath
        Invoke-PluginReload
        Write-Host "Plugin reload requested. Wait for the device to show the plugin again, then start the steps."
        Write-Host "Before each step run:  ./tools/device-check/device-check.ps1 Mark `"<step number and name>`""
    }

    'Mark'
    {
        if (-not (Test-Path $StatePath))
        {
            throw "No device check in progress - run Begin first."
        }

        Add-Content -Path $MarkersPath -Value "$(Get-LogTimestamp) | MARK  | $Note"
        Write-Host "Marked: $Note"
    }

    'End'
    {
        if (-not (Test-Path $StatePath))
        {
            throw "No device check in progress - run Begin first."
        }

        $state = Get-Content $StatePath -Raw | ConvertFrom-Json
        Add-Content -Path $MarkersPath -Value "$(Get-LogTimestamp) | MARK  | END"

        # Keep the lines written since Begin, grouping continuation lines (stack traces) with the
        # line they belong to, then merge the markers in by timestamp.
        $timestampPattern = '^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}-\d{3}'
        $records = New-Object System.Collections.Generic.List[Object]
        $keeping = $false
        foreach ($line in Read-Log)
        {
            if ($line -match $timestampPattern)
            {
                $keeping = $line.Substring(0, 23) -ge $state.BeginTime
                if ($keeping)
                {
                    $records.Add([PSCustomObject]@{ Time = $line.Substring(0, 23); Order = 1; Text = $line })
                }
            }
            elseif ($keeping)
            {
                $records[$records.Count - 1].Text += "`n$line"
            }
        }

        foreach ($line in Get-Content $MarkersPath)
        {
            $records.Add([PSCustomObject]@{ Time = $line.Substring(0, 23); Order = 0; Text = $line })
        }

        $merged = $records | Sort-Object Time, Order
        $outputPath = Join-Path $StateDir "device-check-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
        $merged | ForEach-Object { $_.Text } | Set-Content -Path $outputPath

        # Restore the log level.
        if (-not $state.ConfigExisted)
        {
            Remove-Item $ConfigPath -ErrorAction SilentlyContinue
        }
        elseif ($null -ne $state.OriginalLogLevel)
        {
            $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
            if ($config.LogLevel -ne $state.OriginalLogLevel)
            {
                $config | Add-Member -NotePropertyName LogLevel -NotePropertyValue $state.OriginalLogLevel -Force
                $config | ConvertTo-Json | Set-Content -Path $ConfigPath
                Invoke-PluginReload
                Write-Host "Log level restored."
            }
        }

        Remove-Item $StatePath

        $text = ($merged | ForEach-Object { $_.Text }) -join "`n"
        $count = { param($pattern) ([regex]::Matches($text, $pattern)).Count }
        Write-Host ""
        Write-Host "Summary"
        Write-Host "  Markers:                         $(& $count '\| MARK  \|')"
        Write-Host "  Scene source loads:              $(& $count 'Loaded \d+ sources and')"
        Write-Host "  Scene source loads skipped:      $(& $count 'Skipping source load')"
        Write-Host "  Audio membership cache hits:     $(& $count 'Cache hit for audio input scene membership')"
        Write-Host "  Audio membership cache misses:   $(& $count 'Cache miss for audio input scene membership')"
        Write-Host "  Stats polls:                     $(& $count 'polled stats in')"
        Write-Host "  Stats polling started/stopped:   $(& $count 'polling started') / $(& $count 'polling stopped')"
        Write-Host "  Loading tiles rendered:          $(& $count 'Rendering loading tile')"
        Write-Host "  Warnings / errors:               $(& $count '\| WARN ') / $(& $count '\| ERROR')"
        Write-Host ""
        Write-Host "Log extract written to:"
        Write-Host "  $outputPath"
    }
}

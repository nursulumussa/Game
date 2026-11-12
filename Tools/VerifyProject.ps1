$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$version=[regex]::Match((Get-Content "$project\ProjectSettings\ProjectVersion.txt" -Raw),'m_EditorVersion: (\S+)').Groups[1].Value
$engine=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Data"
$scratch=Join-Path ([IO.Path]::GetTempPath()) 'MountainAdventureVerification'
New-Item -ItemType Directory -Path $scratch -Force | Out-Null
$checks=[Collections.Generic.List[string]]::new()
function Pass($s){$checks.Add("PASS: $s");Write-Output "PASS: $s"}
$source=Get-Content "$project\Assets\Scenes\EnemyTrial.unity" -Raw
$ids=@([regex]::Matches($source,'(?m)^--- !u!\d+ &(-?\d+)')|ForEach-Object {$_.Groups[1].Value})
if($ids|Group-Object|Where-Object Count -gt 1){throw 'Duplicate scene file IDs'}
Pass "$($ids.Count) scene IDs are unique"
$set=[Collections.Generic.HashSet[string]]::new();$ids|ForEach-Object {[void]$set.Add($_)}
$missing=@([regex]::Matches($source,'\{fileID: (-?\d+)\}')|ForEach-Object {$_.Groups[1].Value}|Where-Object {$_ -ne '0' -and !$set.Contains($_)})
if($missing.Count){throw 'Dangling local scene reference'}
Pass 'Every local scene reference resolves'
foreach($type in @('EnemyPatrol','EnemyShooter2D','EnemyCharger2D')){
 if(([regex]::Matches($source,"m_EditorClassIdentifier: Assembly-CSharp::$type\r?\n")).Count -ne 1){throw "Need one $type in level"}
 $script=Get-Content "$project\Assets\$type.cs" -Raw
 $states=[regex]::Match($script,'enum State \{([^}]+)\}').Groups[1].Value.Split(',')
 if($states.Count -lt 3){throw "$type has fewer than three states"}
 Pass "${type}: separate FSM with $($states.Count) states"
}
$behaviours=@([regex]::Split($source,'(?m)(?=^--- !u!)')|Where-Object {$_ -match 'm_EditorClassIdentifier: Assembly-CSharp::Enemy(Patrol|Shooter2D|Charger2D)\r?\n'})
foreach($b in $behaviours){
 $wind=[double][regex]::Match($b,'windUp: ([\d.]+)').Groups[1].Value
 $cool=[double][regex]::Match($b,'attackInterval: ([\d.]+)').Groups[1].Value
 if($wind -lt .3 -or $wind -gt .8 -or $cool -le 0){throw 'Invalid attack warning/cooldown'}
 foreach($ref in @('player','eyePoint','stateText')){if($b -notmatch "$ref`: \{fileID: [1-9]\d*\}"){throw "Missing $ref"}}
 if($b -notmatch 'm_Bits: 3328' -or $b -notmatch 'm_Bits: 3072'){throw 'Sight/solid masks differ from planned layer setup'}
}
Pass 'All three attacks have valid serialized wind-up, cooldown, references and masks'
$waypoints=[regex]::Matches($source,'m_Name: (Guard|Charger)_Waypoint_[AB]\r?\n')
if($waypoints.Count -ne 4){throw 'Four stationary waypoints required'}
Pass 'Four explicit patrol waypoints are serialized in the level'
if($source -notmatch 'nextScene: WinScene'){throw 'Goal must lead to WinScene'}
if((Get-Content "$project\Assets\MainMenuController.cs" -Raw) -notmatch 'LoadScene\("EnemyTrial"\)'){throw 'PLAY must launch trial'}
if((Get-Content "$project\ProjectSettings\EditorBuildSettings.asset" -Raw) -notmatch 'Assets/Scenes/EnemyTrial.unity'){throw 'Trial not in build settings'}
Pass 'Menu, build list and goal route are connected'
$meta=Get-Content "$project\Assets\Prefabs\EnemyProjectile.prefab.meta" -Raw
$guid=[regex]::Match($meta,'guid: (\w+)').Groups[1].Value
if($source -notmatch "projectilePrefab: \{fileID: -?\d+, guid: $guid, type: 3\}"){throw 'Shooter projectile reference missing'}
Pass 'Shooter references its projectile prefab'

$refs=@(Get-ChildItem "$engine\NetStandard\ref\2.1.0\*.dll")+@(Get-ChildItem "$engine\Managed\UnityEngine\*.dll")
$refs+=@(Get-Item "$project\Library\ScriptAssemblies\Unity.TextMeshPro.dll","$project\Library\ScriptAssemblies\Unity.InputSystem.dll","$project\Library\ScriptAssemblies\UnityEngine.UI.dll")
$common=@('/nostdlib+','/target:library')+@($refs|ForEach-Object {'/reference:"'+$_.FullName+'"'})
foreach($kind in @('runtime','editor')){
 $files=if($kind -eq 'runtime'){@(Get-ChildItem "$project\Assets\*.cs")}else{@(Get-ChildItem "$project\Assets\Editor\*.cs")}
 $args=$common+@('/out:"'+$scratch+'\'+$kind+'.dll"')+@($files|ForEach-Object {'"'+$_.FullName+'"'})
 $rsp=Join-Path $scratch "$kind.rsp";Set-Content $rsp $args
 $output=& "$engine\NetCoreRuntime\dotnet.exe" "$engine\DotNetSdkRoslyn\csc.dll" "@$rsp" 2>&1
 if($LASTEXITCODE -ne 0){$output|ForEach-Object {Write-Output $_};throw "$kind compile failed"}
 Pass "$kind C# compilation against the installed Unity/package assemblies"
}
$checks.Add('NOT RUN: Unity scene import, Play Mode, Android build, phone playtest, frame rate and visual QA.')
$checks.Add('Background Unity launch failed connecting to its license client; no APK is claimed.')
$report="Verification: $(Get-Date -Format o)`n"+($checks -join "`n")+"`n"
[IO.File]::WriteAllText("$project\Documentation\VERIFICATION.txt",$report,[Text.UTF8Encoding]::new($false))

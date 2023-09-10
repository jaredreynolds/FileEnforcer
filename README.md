# File Enforcer

Force changed files back to what you want.

## Setup Tasks in `appsettings.json`

In `appsettings.json`, add tasks for each source (backup) and target pair, similar to the following.

```json
{
  "settings": {
    "tasks": [
      {
        "source": "C:/etc/hosts.the-one-I-want",
        "target": "C:/etc/hosts",
        "action": "copy"
      }
    ]
  }
}
```

## Install as a Windows Service

1. Copy the executable and your `appsettings.json` file to `C:\Program Files\FileEnforcer`.
2. Execute the following in an _administrative_ PowerShell/cmd:

   ```
   sc.exe create "FileEnforcer" binpath="C:\Program Files\FileEnforcer\FileEnforcer.exe" start=auto
   sc.exe description FileEnforcer "FileEnforcer monitors files for changes and restores them to a preferred state."
   sc.exe start FileEnforcer
   ```

## Uninstall Windows Service

1. Execute the following in an _administrative_ PowerShell/cmd:

   ```
   sc.exe stop FileEnforcer
   sc.exe delete FileEnforcer
   ```

2. Delete the folder `C:\Program Files\FileEnforcer`.

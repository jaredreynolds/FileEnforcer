# File Enforcer

Force changed files back to what you want.

## appsettings.json

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

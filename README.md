# save-data

Save values to JSON files under `Application.persistentDataPath`, with optional encryption.

## SaveData

`SaveData` is a dictionary of `SaveKey` → value that can be written to and read from a file.

```csharp
var saveData = new SaveData();

saveData.SetKey("Position", transform.position);
saveData.Save(path);

saveData.Load(path);
transform.position = saveData.GetKey("Position", Vector3.zero);
```

`Save` and `Load` log exceptions instead of throwing them. `SaveAsync` and `LoadAsync` do the
file work on a background thread and pass exceptions on to the caller.

## SaveVar

`SaveVar<T>` is a serializable field that stores one value in a save file. It keeps the
file path (relative to `Application.persistentDataPath`), the `SaveKey` and a default value.

```csharp
[SerializeField] private SaveVar<int> _coins = new("Player.json", new SaveKey("Coins"));

_coins.Value = 10;                     // written to the file right away
int coins = _coins;                    // implicit conversion to T

_coins.SetValueWithoutNotifying(20);   // kept in memory only, IsDirty == true
_coins.Push();                         // write it to the file...
// _coins.Pull();                      // ...or drop it and reload the file
```

- `Value` / `SetValue` / `SetValueAsync` write to the file at once.
- `SetValueWithoutNotifying` only changes the value in memory and marks it as dirty.
- `Push` writes a dirty value to the file, `Pull` drops it and reloads the file.
  Both throw `InvalidOperationException` when the value is not dirty.
- `ResetToDefault` writes `DefaultValue`.

Every `SaveVar` that points at the same file shares one `DirtableSaveData`, so a push of one
variable does not write the unsaved changes of the others.

Whole files can be handled with the static `SaveVar.IsSaveDataDirty`, `PushSaveData` and
`PullSaveData` (and their async versions).

## DirtableSaveData

`DirtableSaveData` is the in-memory state of one file:

- `Origin` is what the file holds. It is only replaced by a load or a save.
- `Target` holds only the unsaved changes. A key in `Target` is a dirty key.

`GetKey` reads `Target` first and falls back to `Origin`. `Push` merges `Target` into a copy of
`Origin`, saves it and makes it the new `Origin`. `Pull` reloads `Origin` from the file and drops
the changes. When the file does not exist `Origin` is empty, so every set value is dirty.

The path is passed to every call instead of being stored, so the caller has to use the same
path the instance was loaded from.

## Settings

Default settings live in the `DefaultSaveSettings` asset in `Resources`. `SaveSettings` controls
encryption, custom JSON settings and the limit of files kept in a folder.

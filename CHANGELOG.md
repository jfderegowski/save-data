# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.1.0] - 2026-09-26

### Added

- `DirtableSaveData`: Queue when saveing to file

## [2.0.0] - 2026-09-25

### Added

- `DirtableSaveData`: the state of one save file, split into `Origin` (what the file holds)
  and `Target` (the unsaved changes). `IsDirty` / `IsKeyDirty` come from `Target`.
- `SaveVar<T>.SetValueWithoutNotifying`, `SetValueAsync` and `GetValueAsync`.
- Static `SaveVar.GetSaveData`, `IsSaveDataDirty`, `PushSaveData` and `PullSaveData`,
  with async versions, for handling a whole file.

### Changed

- `SaveVar<T>` is rebuilt on `DirtableSaveData`. Setting `Value` now writes to the file right
  away; use `SetValueWithoutNotifying` to only change it in memory.
- `Push` and `Pull` work on the variable's own key only and throw `InvalidOperationException`
  when the value is not dirty. `Pull` reloads the file.
- `PushAsync` and `PullAsync` pass exceptions on to the caller. A value set again while the
  push is awaited stays dirty.

### Removed

- `SaveVarStorage`.
- The equality operators and `IEquatable` implementations of `SaveVar<T>`.
- `SaveVar.GetStringValue`; use `ToString`.

### Fixed

- `SaveVar<T>.PushAsync` pulled the file instead of pushing it.

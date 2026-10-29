# Contributing

## Branches

- **`development`** is the default branch: every pull request targets it.
- **`master`** is the released code. It only receives merges from `development` when a stable version is released.

## Pull requests

- One topic per pull request: small, focused PRs are reviewed and merged much faster than big ones.
- Add a line to `CHANGELOG.md` under `## Unreleased`, in the right section (`BREAKING CHANGES`, `FEATURES`, `IMPROVEMENTS`, `BUG FIXES`).
- Mention which device and firmware you tested on. New commands that a device does not answer can still be added, following the protocol document in `Docs/`, but say they are untested.
- Prefer **Create a merge commit** or **Rebase and merge**: they keep the original commit authors. A branch that has `development` merged into it cannot be rebased: use a merge commit.

## Build and test

You need the .NET 10 SDK. The solution is `Anviz.slnx`.

```
dotnet build
dotnet test
```

The library targets `netstandard2.0` (C# 7.3), the tests and the sample target `net10.0`.

`Anviz.SDK.Tests` runs the library against `FakeDevice`, a device on localhost that speaks the TC_B protocol: it answers commands through a handler, can deliver answers one byte at a time, send packets by itself (like the ping) and drop the connection. Real traffic captured with Wireshark can be stored in `Anviz.SDK.Tests/Captures` (hex bytes, `#` for comments) and compared byte for byte with what the library sends.

The **Test** workflow runs the tests on every push to `development` and `master` and on every pull request.

## Releases

The NuGet package is published by the **.NET** workflow (`.github/workflows/nuget.yml`). The version is **the tag of the most recently created GitHub Release**: nothing in the code needs to change. The workflow runs on pushes to `master`, on tag pushes and manually; it tests, packs with `/p:Version=<tag>` and pushes to NuGet, skipping a version that is already there.

Tags have no `v` prefix: `2.0.18`, `3.0.0`, `3.0.0-beta.1`.

### Stable release

1. In `CHANGELOG.md`, rename `## Unreleased` to `## vX.Y.Z` and merge it to `development`.
2. Open a pull request from `development` to `master` and merge it with a merge commit. The workflow runs, finds the previous release and publishes nothing new.
3. On GitHub, *Releases → Draft a new release*: create the tag `X.Y.Z` on `master` from that page, generate the release notes and publish. The tag push runs the workflow, which publishes `Anviz.SDK.X.Y.Z`.

### Prerelease

1. On GitHub, *Releases → Draft a new release*: create the tag (e.g. `3.0.0-beta.1`) with target **`development`**, tick **Set as a pre-release** and publish. No merge to `master` is needed: the workflow checks out the tag.
2. NuGet lists it as a prerelease: it is installed only with `dotnet add package Anviz.SDK --prerelease` (or *Include prerelease* in Visual Studio).
3. Keep `## Unreleased` in the CHANGELOG until the stable release.

### Pitfalls

- **Create the tag from the release page**, not by pushing it first: the workflow runs on the tag push and would still find the previous release. If that happens, run the workflow again from the Actions tab (*Run workflow*) once the release exists.
- **Do not leave draft releases around**: the workflow takes the most recently created release, and a draft can be returned to it.
- The workflow does not sort by version: the newest release wins. Create releases in version order.
- A manual run, or a push to `master`, publishes the latest release again: harmless, NuGet skips a version that already exists.
- The NuGet API key is the `NUGET_API_KEY` secret of the repository.

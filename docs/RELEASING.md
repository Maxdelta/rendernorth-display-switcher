# Releasing RenderNorth Display Switcher

`Directory.Build.props` is the authoritative version source for the application, About dialog, packages, installer, and filenames.

1. Update `CHANGELOG.md` for the new semantic version.
2. Change `Version`, `AssemblyVersion`, `FileVersion`, and `VersionPrefix` in `Directory.Build.props`.
3. Run `dotnet tool restore`, `.\build.ps1`, and `.\release.ps1 -Version X.Y.Z` locally.
4. Test the normal GUI, both silent commands, both launchers, installer install/uninstall, and portable ZIP.
5. Commit the changes.
6. Create a tag: `git tag -a vX.Y.Z -m "RenderNorth Display Switcher vX.Y.Z"`.
7. Push the branch and tag. GitHub Actions builds the installer, update feed assets, portable ZIP, and checksums.
8. Verify the generated GitHub Release and its notes/assets before announcing it.
9. Test an update from the previous installed version to the new GitHub Release.

The GitHub repository URL is configured once in `Services/UpdateService.cs`. If the owner is not `RenderNorth`, update `GitHubRepositoryUrl` before publishing.

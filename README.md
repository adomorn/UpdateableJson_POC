# UpdateableJson_POC

An ASP.NET Core proof of concept for editing JSON configuration and writing overrides to `.Updatable.json` files.

## Repository layout

- `WebApplication5`: Razor Pages experiment with configuration inspection and editing.
- `WebApplication5/Pages/ConfigManipulator.cs`: reads JSON configuration providers, merges values, and writes override files.
- `WebApplication4`: companion web application experiment.
- `Abstractions` and `DummyClassLib`: interfaces and sample plugin/configuration code.

The solution retains its original name, `WebApplication4.sln`.

## Status

This is exploratory code. It is not a packaged configuration editor or a supported deployment template. It uses reflection to inspect configuration-provider internals and writes files on disk. Review the implementation and use disposable configuration copies when experimenting.

For a standalone library focused on reading and updating nested JSON by path, see [StructuredJson](https://github.com/adomorn/StructuredJson).

## Development priorities

- Document the intended sample application and its setup.
- Add tests for type conversion, invalid values, and write failures.
- Review configuration-provider compatibility and target frameworks before further development.

No project-wide license or CI workflow is currently declared. Licenses under bundled frontend dependencies apply to those dependencies, not to the whole project.

# ![Finbuckle Logo](https://www.finbuckle.com/images/finbuckle-32x32-gh.png) MultiTenant <span class="_version">10.0.2</span>

## About MultiTenant

MultiTenant is an open source multi-tenancy library for modern .NET created and maintained by [Finbuckle LLC](https://www.finbuckle.com).
It enables tenant resolution, per-tenant app behavior, and per-tenant data isolation.

See [https://www.finbuckle.com/MultiTenant](https://www.finbuckle.com/MultiTenant) for more details and documentation.

**This release supports .NET 10.**

Beginning with MultiTenant v10, major version releases align with .NET major version releases.

New development focuses on the latest MultiTenant release version while critical security and severe bug 
fixes will be released for prior versions which target .NET versions supported by Microsoft.

In general, you should target the version of MultiTenant that matches your .NET version.

## Open Source Support

Your support helps keep the project going and is greatly appreciated!

MultiTenant is primarily supported by its [GitHub sponsors](https://github.com/sponsors/Finbuckle) and [contributors](https://github.com/Finbuckle/Finbuckle.MultiTenant/graphs/contributors).

Additional support is provided by the following organizations:

<p><a href="https://www.digitalocean.com/">
  <img src="https://opensource.nyc3.cdn.digitaloceanspaces.com/attribution/assets/SVG/DO_Logo_horizontal_blue.svg" alt="Digital Ocean logo" height="40">
</a></p>

<p><a href="https://www.github.com/">
  <img src="https://github.githubassets.com/assets/GitHub-Logo-ee398b662d42.png" alt="GitHub logo" height="40">
</a></p>

<p><a href="https://www.jetbrains.com/">
  <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.svg" alt="Jetbrains logo" height="40">
</a></p>

## License

This project uses the [Apache 2.0 license](https://www.apache.org/licenses/LICENSE-2.0). See [LICENSE](LICENSE) file for
license information.

## .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our
community. For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct)
or the [CONTRIBUTING.md](CONTRIBUTING.md) file.

## Community

Check out the [GitHub repository](https://github.com/Finbuckle/Finbuckle.MultiTenant) to ask a question, make a request,
or peruse the code!

## Building from Source

From the command line clone the git repository, `cd` into the new directory, and compile with `dotnet build`.

```bash
git clone https://github.com/Finbuckle/Finbuckle.MultiTenant.git
cd Finbuckle.MultiTenant
cd Finbuckle.MultiTenant
dotnet build
```

## Running Unit Tests

Run the unit tests from the command line with `dotnet test` from the solution directory.

```bash
dotnet test
```
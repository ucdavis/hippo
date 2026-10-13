[![Node Version](https://img.shields.io/badge/dynamic/json?label=Node%20Version&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2Fhippo%2Fmain%2FHippo.Web%2FClientApp%2Fpackage.json)](https://img.shields.io/badge/dynamic/json?label=Node%20Version&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2Fhippo%2Fmain%2FHippo.Web%2FClientApp%2Fpackage.json)
![CodeQL Scan](https://github.com/ucdavis/hippo/actions/workflows/codeql-analysis.yml/badge.svg)
[![Build Status](https://dev.azure.com/ucdavis/HiPPO/_apis/build/status%2FHippo%20Web%20Build?branchName=refs%2Fpull%2F188%2Fmerge)](https://dev.azure.com/ucdavis/HiPPO/_build/latest?definitionId=29&branchName=refs%2Fpull%2F188%2Fmerge)

# HIPPO
HI Performance People Onboarding (HIPPO)

Onboarding and user management for High Performance Computing Clusters at UC Davis

# Local Setup

Requires .NET SDK **10.0.401** (runtime **10.0.12**) from https://dotnet.microsoft.com/download. `global.json` allows only patch roll-forward and excludes previews.

Requires nodeJS, version 22.14.0 or higher

Run `dotnet tool restore` from the repository root for the pinned EF CLI. In the `Hippo.Web/ClientApp` folder, run `npm ci --legacy-peer-deps`.  Technically this step is optional but it's useful to do to get things started.

# Run locally

Get the user-secrets file and store it [in the correct location](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows#how-the-secret-manager-tool-works)

In the `Hippo.Web` folder, run:

`npm start`

# Development

Make sure to invoke "Prettier" before committing JS changes.  If using VSCode consider [using the plugin](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode).

If making large JS changes, run `npm test` inside the `Hippo.Web/ClientApp` directory and it will automatically re-run affected tests.

## Integration smoke tests

The Key Vault smoke test is opt-in because it writes to the configured Azure Key Vault. It uses the same user-secrets file as `Hippo.Web`, so make sure the `Azure` section is configured before running it.

To verify that Hippo can write, read, and delete a temporary Key Vault secret, run:

```bash
RUN_KEYVAULT_SMOKE_TESTS=true dotnet test Test/Test.csproj --filter FullyQualifiedName~KeyVaultSmokeTests
```

Without `RUN_KEYVAULT_SMOKE_TESTS=true`, the test exits without contacting Key Vault.

## Configuration after the .NET 10 upgrade

Key Vault uses `Azure.Identity` client-secret authentication. Configure `Azure:TenantId`,
`Azure:ClientId`, `Azure:ClientSecret`, and `Azure:KeyVaultUrl` (the vault HTTPS URL).
The old misspelled `Azure:TentantId` is no longer accepted. Supply secrets through user-secrets
or environment variables (`Azure__TenantId`, etc.); never commit credentials. Secret deletion
starts a soft delete and does not purge or wait for vault-side completion, matching the previous behavior.

Serilog remains the only logging pipeline for the web app and all three jobs. Console output,
request/action/client enrichment, exception details, `Application`, `AppEnvironment`, and job
identity are retained. The EF exception enricher is aligned to 8.4.0 so EF 10 failures
retain structured exception and entity details. `Serilog:AppName` and `Serilog:Environment` still supply event properties.
Serilog exports logs through OTLP and includes ambient trace/span IDs when available.
Hippo does not configure trace collection or export, and no metrics or SQL instrumentation is enabled.

### OTLP connection and local debugging

The web app and all three jobs read standard `OTEL_*` keys through .NET configuration.
Development loads their existing shared user-secrets store (ID `c14737e0-e801-4dfa-bc8d-ce9130c807c6`);
environment variables override user-secrets. Set `ASPNETCORE_ENVIRONMENT=Development` when
running jobs locally. The web app's normal development launch profile already sets this.
User-secrets loading depends on the runtime environment, not the Debug/Release build configuration.

To send logs to Elastic while debugging locally, merge these **placeholder** entries into that
user-secrets store, using your OTLP intake endpoint and its credential:

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "https://<otlp-intake-host>",
  "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
  "OTEL_EXPORTER_OTLP_HEADERS": "Authorization=ApiKey <api-key>",
  "OTEL_RESOURCE_ATTRIBUTES": "deployment.environment.name=Development"
}
```

Alternatively, supply the same settings as environment variables to the process launched by
your debugger or shell:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export OTEL_EXPORTER_OTLP_ENDPOINT='https://<otlp-intake-host>'
export OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
export OTEL_EXPORTER_OTLP_HEADERS='Authorization=ApiKey <api-key>'
export OTEL_RESOURCE_ATTRIBUTES=deployment.environment.name=Development
```

For a local collector instead, use `http://localhost:4318` and omit authentication unless required.
Never commit real endpoints or credentials to application settings, launch profiles, or this document.

The nonblank `OTEL_EXPORTER_OTLP_ENDPOINT` enables log export. Alternatively, configure
`OTEL_EXPORTER_OTLP_LOGS_ENDPOINT`, which takes precedence over the general endpoint.
Without a log endpoint, Hippo writes one startup notice to stderr and logs to console;
it does not fall back to a localhost collector. To disable export, remove or blank both
endpoint keys in the effective configuration, including user-secrets.
`OTEL_EXPORTER_OTLP_TRACES_ENDPOINT`, `OTEL_TRACES_SAMPLER`, and `OTEL_TRACES_SAMPLER_ARG`
are unused and do not enable export or affect logging.
HTTP/protobuf is the default; `OTEL_EXPORTER_OTLP_PROTOCOL=grpc` overrides it.
Invalid active endpoint, protocol, header, or resource settings fail startup with setting
names, without echoing configuration values or credentials.

The general endpoint is a base URL: HTTP export adds `/v1/logs`. The logs-specific
endpoint is used as the complete URL, for example `https://<otlp-intake-host>/v1/logs`.
With gRPC, use the collector's HTTP(S) authority instead.
Use `OTEL_EXPORTER_OTLP_HEADERS` for comma-separated `name=value` authentication headers.
Header and resource values are URI decoded after splitting pairs; encode embedded commas
as `%2C` and equals signs as `%3D`.
Settings are read from .NET configuration without copying them into process environment variables.
Do not assume other signal-specific settings are supported.
See the [sink configuration documentation](https://github.com/serilog/serilog-sinks-opentelemetry).

### Identity and diagnostics

Resource metadata defaults to the executable assembly name for `service.name` and its
informational version (falling back to assembly version) for `service.version`.
`deployment.environment.name` uses `Serilog:Environment` when meaningful, then the web host's
resolved environment or the jobs' `ASPNETCORE_ENVIRONMENT`, finally `Production`.
Blank and `[External]` values are ignored for these environment defaults.
Existing `Application` and `AppEnvironment` event properties remain unchanged.

`OTEL_RESOURCE_ATTRIBUTES` overrides resource defaults; `OTEL_SERVICE_NAME` takes precedence
over `service.name` in those attributes. Leave both shared service-name overrides unset in
App Service and the shared user-secrets store so the web app and WebJobs retain their distinct
assembly names. A per-process override is supported when intentionally naming one executable.
Use `deployment.environment.name=Development` locally and the actual environment remotely;
an optional `service.version` override should identify the application release, not the .NET runtime.

Serilog internal diagnostics always go directly to synchronized stderr, including in Release.
Console logging remains available when delivery fails. Bootstrap console logging covers startup
failures, and each executable flushes the owned logger on success or failure. A healthy console
does not establish that Elastic accepted the logs; check stderr and the destination as well.

### Elastic deployment and verification

1. Configure the App Service with an **OTLP ingestion endpoint**, protocol, and authentication
   headers. The former Elasticsearch URL is not an OTLP intake URL. Elastic's managed OTLP
   endpoint uses `Authorization=ApiKey <api-key>`; other collectors/intake endpoints may need
   different authentication. See [Elastic's OTLP setup](https://www.elastic.co/docs/solutions/observability/get-started/quickstart-elastic-cloud-otel-endpoint).
2. Set `OTEL_RESOURCE_ATTRIBUTES` with the deployed `deployment.environment.name` and, optionally,
   release version. Leave shared `OTEL_SERVICE_NAME` and resource `service.name` unset to avoid
   giving all three WebJobs the web app's identity. Verify which settings each deployed process inherits.
3. After configuring OTLP, remove obsolete `Serilog:ElasticUrl` and `ElasticApm` settings from
   external configuration. They no longer control logging.
4. During a controlled rollout, verify receipt of normal operation logs from `Hippo.Web`,
   `Hippo.Jobs.Notifications`, `Hippo.Jobs.OrderProcess`, and `Hippo.Jobs.PuppetSync`. Use scheduled
   or otherwise authorized job runs; verify local debugging separately with Development metadata.
   Confirm service names, application versions, environment, job identity, and structured context.
5. Check a representative exception's type, message, stack trace, and custom exception fields.
   Check stderr for delivery/authentication failures and verify final logs arrive after job shutdown.
6. Identify the actual destination data streams and document field paths. The old `aspnet-hippo-*`
   index naming is no longer configured by the app. Verify permissions and retention, then reconcile
   Kibana data views, saved searches, dashboards, and alert rules with the new streams and fields.
   Recheck alert matches before retiring the old views.

Automated logging tests use local OTLP receivers and no real Elastic credentials. Live ingestion,
data-stream mapping, retention, and dashboard/alert compatibility require the rollout checks above.

CAS retains the existing authorization-code flow with PAR disabled. Cookie authentication
explicitly retains the former non-XHR redirects and XHR 401/403 behavior, including API endpoints.

## Upgrade verification

```bash
dotnet restore hippo.sln --force
dotnet build hippo.sln --configuration Release
dotnet test Test/Test.csproj --configuration Release --filter 'FullyQualifiedName!~KeyVaultSmokeTests'
dotnet list hippo.sln package --vulnerable --include-transitive
dotnet list hippo.sln package --deprecated --include-transitive
```

Run `npm run citest` and `npm run build` in `Hippo.Web/ClientApp` with Node 22. Publish each
executable with `dotnet publish <project> --configuration Release --output <directory>`.
The web publish target includes the SPA build, and jobs retain `run.cmd` and `settings.job`.
No frontend package upgrade is part of this change.

SQL Server integration tests use `HIPPO_TEST_SQLSERVER` for a local test server. They create
and delete a uniquely named `HippoUpgradeTest_*` database, apply the existing history, and
exercise JSON, ownership, filters, collection queries, bulk operations, and synchronization.
They are skipped when the variable is unset. Never point this setting at a production server.
For published-process smoke tests, set `HIPPO_PUBLISH_ROOT` to a directory containing
`web/` and `jobs/{PuppetSync,OrderProcess,Notifications}/` publish outputs, alongside
`HIPPO_TEST_SQLSERVER`. `PublishedApplicationTests` runs all four executables with isolated
configuration and an unavailable loopback collector, checks startup errors/exit codes, and
uses SIGINT for graceful web shutdown on Unix. Local logging tests use a loopback OTLP
receiver and never contact an external collector.
The Key Vault smoke test remains explicitly opt-in.

Both provider models match their existing snapshots under EF 10. **The existing SQLite migration
history cannot initialize an empty database:** `20231103175224_HippoV2` tries to create
`IX_Clusters_Name` twice. This also reproduces with EF 8.0.14. The history test records this known
blocker with an explicit skip; SQLite model/persistence/service tests still run. No migration or
snapshot was changed, and pending-model warnings are not suppressed. Repairing that history
requires separate approval; do not use fresh SQLite startup as an upgrade acceptance check until
it is repaired.

The NuGet audit found no known vulnerabilities. Remaining deprecations are individually retained
because of the requested dependency targets:

- `Microsoft.AspNetCore.SpaServices.Extensions` 5.0.1 comes through `MvcReact` 2.4.10 →
  `SpaCliMiddleware` 5.0.0; replacing it requires an upstream MvcReact dependency change.
- `xunit` and `xunit.assert` 2.9.3 are the requested v2 framework/assertions.
- `xunit.core`, `xunit.extensibility.core`, and `xunit.extensibility.execution` 2.9.3 are the
  matching v2 runtime dependencies. Moving these to v3 requires a separate test-framework upgrade.

No audit warnings are suppressed. Deployment, Azure configuration changes, collector provisioning,
and external ingestion verification are outside this upgrade.

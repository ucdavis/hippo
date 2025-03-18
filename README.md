[![Node Version](https://img.shields.io/badge/dynamic/json?label=Node%20Version&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2Fhippo%2Fmain%2FHippo.Web%2FClientApp%2Fpackage.json)](https://img.shields.io/badge/dynamic/json?label=Node%20Version&query=%24.engines.node&url=https%3A%2F%2Fraw.githubusercontent.com%2Fucdavis%2Fhippo%2Fmain%2FHippo.Web%2FClientApp%2Fpackage.json)
![CodeQL Scan](https://github.com/ucdavis/hippo/actions/workflows/codeql-analysis.yml/badge.svg)
[![Build Status](https://dev.azure.com/ucdavis/HiPPO/_apis/build/status%2FHippo%20Web%20Build?branchName=refs%2Fpull%2F188%2Fmerge)](https://dev.azure.com/ucdavis/HiPPO/_build/latest?definitionId=29&branchName=refs%2Fpull%2F188%2Fmerge)

# HIPPO
HI Performance People Onboarding (HIPPO)

Onboarding and user management for High Performance Computing Clusters at UC Davis

# Local Setup

Requires .net 8 SDK from https://dotnet.microsoft.com/download

Requires nodeJS, version 22.14.0 or higher

In the `Hippo.Web/ClientApp` folder, run `npm install`.  Technically this step is optional but it's useful to do to get things started.

# Run locally

Get the user-secrets file and store it [in the correct location](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows#how-the-secret-manager-tool-works)

In the `Hippo.Web` folder, run:

`npm start`

# Development

Make sure to invoke "Prettier" before committing JS changes.  If using VSCode consider [using the plugin](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode).

If making large JS changes, run `npm test` inside the `Hippo.Web/ClientApp` directory and it will automatically re-run affected tests.

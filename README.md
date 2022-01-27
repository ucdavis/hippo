# HIPPO
HI Performance People Onboarding (HIPPO)

Onboarding and user management for High Performance Computing Clusters at UC Davis

# Local Setup

Requires .net 6 SDK from https://dotnet.microsoft.com/download

Requires nodeJS, recommended version 14.x

In the `Hippo.Web/ClientApp` folder, run `npm install`.  Technically this step is optional but it's useful to do to get things started.

# Run locally

Get the user-secrets file and store it [in the correct location](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows#how-the-secret-manager-tool-works)

In the `Hippo.Web` folder, run:

`npm start`

# Development

Make sure to invoke "Prettier" before committing JS changes.  If using VSCode consider [using the plugin](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode).

If making large JS changes, run `npm test` inside the `Hippo.Web/ClientApp` directory and it will automatically re-run affected tests.


# LGTM Status

[![Total alerts](https://img.shields.io/lgtm/alerts/g/ucdavis/hippo.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/ucdavis/hippo/alerts/)
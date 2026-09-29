import { act, render, screen } from "@testing-library/react";
import { MemoryRouter, useLocation } from "react-router-dom";
import App from "./App";
import {
  fakeAccounts,
  fakeAppContextNoAccount,
  fakeGroupAdminAppContext,
  fakeRequests,
} from "./test/mockData";
import { AppContextShape, RoleName } from "./types";

const paths: string[] = [];
function LocationProbe() {
  paths.push(useLocation().pathname);
  return null;
}
function contextFor(role?: RoleName, cluster = "caesfarm") {
  const context = structuredClone(fakeAppContextNoAccount);
  context.clusters[0].acceptableUsePolicyUpdatedOn = "2025-01-01T00:00:00Z";
  if (role) context.user.permissions = [{ role, cluster }];
  return context;
}
async function visit(path: string, context: AppContextShape = contextFor()) {
  (globalThis as any).Hippo = context;
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[path]}>
        <LocationProbe />
        <App />
      </MemoryRouter>,
    );
  });
}
beforeEach(() => {
  paths.length = 0;
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => ({ ok: true, status: 200, json: async () => [] })),
  );
});
afterEach(() => vi.unstubAllGlobals());

it.each(["/", "/caesfarm", "/caesfarm/create", "/caesfarm/myaccount"])(
  "newcomers reach creation from %s",
  async (path) => {
    await visit(path);
    expect(paths.at(-1)).toBe("/caesfarm/create");
    expect(screen.getByText(/You don't seem to have an account/)).toBeVisible();
    expect(
      screen.queryByText(/In order to continue accessing this cluster/),
    ).not.toBeInTheDocument();
    expect(screen.getByRole("checkbox")).toBeInTheDocument();
  },
);
it("pending creation goes directly to status without loading account groups", async () => {
  const context = contextFor();
  context.openRequests = [structuredClone(fakeRequests[0])];
  await visit("/caesfarm/myaccount", context);
  expect(paths.at(-1)).toBe("/caesfarm/accountstatus");
  expect(paths).not.toContain("/caesfarm/create");
  expect(fetch).not.toHaveBeenCalled();
});
it.each([
  "",
  "/myaccount",
  "/create",
  "/accountstatus",
  "/admin/groups",
  "/order/details/1",
])("unknown cluster %s goes to selection before fetching", async (suffix) => {
  await visit(`/missing${suffix}`, contextFor("System"));
  expect(paths.at(-1)).toBe("/clusters");
  expect(screen.getByText("Clusters at Hippo")).toBeVisible();
  expect(fetch).not.toHaveBeenCalled();
});
it("an inactive cluster omitted from context goes to selection", async () => {
  const context = contextFor();
  context.clusters = [];
  await visit("/caesfarm/create", context);
  expect(paths.at(-1)).toBe("/clusters");
});
it.each(["/clusters", "/clusteradmin/clusters", "/softwarerequest"])(
  "global route %s remains accessible",
  async (path) => {
    await visit(path, contextFor("System"));
    expect(paths.at(-1)).toBe(path);
    expect(screen.queryByText("Not Authorized")).not.toBeInTheDocument();
  },
);
it.each([
  ["System", "approve", "/api/caesfarm/request/pending"],
  ["ClusterAdmin", "approve", "/api/caesfarm/request/pending"],
  [
    "System",
    "financial/financialdetails",
    "/api/caesfarm/financial/FinancialDetails",
  ],
  [
    "FinancialAdmin",
    "financial/financialdetails",
    "/api/caesfarm/financial/FinancialDetails",
  ],
] as const)("accountless %s can use %s", async (role, path, endpoint) => {
  await visit(`/caesfarm/${path}`, contextFor(role));
  expect(paths.at(-1)).toBe(`/caesfarm/${path}`);
  expect(vi.mocked(fetch).mock.calls.some(([url]) => url === endpoint)).toBe(
    true,
  );
  expect(screen.queryByText("Not Authorized")).not.toBeInTheDocument();
  expect(
    screen.queryByText(/In order to continue accessing this cluster/),
  ).not.toBeInTheDocument();
});
it.each([
  ["FinancialAdmin", "approve", "caesfarm"],
  ["ClusterAdmin", "financial/financialdetails", "caesfarm"],
  ["ClusterAdmin", "approve", "other"],
  ["FinancialAdmin", "financial/financialdetails", "other"],
  ["GroupAdmin", "approve", "caesfarm"],
] as const)(
  "%s does not gain unauthorized access to %s with scope %s",
  async (role, path, scope) => {
    await visit(`/caesfarm/${path}`, contextFor(role, scope));
    expect(screen.getByText("Not Authorized")).toBeVisible();
    expect(fetch).not.toHaveBeenCalled();
  },
);
it("ordinary accountless users cannot access administrative pages", async () => {
  await visit("/caesfarm/approve");
  expect(screen.getByText("Not Authorized")).toBeVisible();
});
it("group membership authorizes GroupAdmin without an explicit permission", async () => {
  const context = structuredClone(fakeGroupAdminAppContext);
  context.user.permissions = [];
  await visit("/caesfarm/approve", context);
  expect(screen.queryByText("Not Authorized")).not.toBeInTheDocument();
  expect(fetch).toHaveBeenCalled();
});
it.each([undefined, "2024-01-01T00:00:00Z", "2026-01-01T00:00:00Z"])(
  "existing account acceptance %s controls renewal",
  async (accepted) => {
    const context = contextFor();
    context.accounts = [
      {
        ...structuredClone(fakeAccounts[0]),
        acceptableUsePolicyAgreedOn: accepted,
      },
    ];
    await visit("/caesfarm/myaccount", context);
    expect(paths.at(-1)).toBe("/caesfarm/myaccount");
    expect(
      !!screen.queryByText(/In order to continue accessing this cluster/),
    ).toBe(accepted !== "2026-01-01T00:00:00Z");
  },
);
it.each(["System", "ClusterAdmin", "FinancialAdmin"] as const)(
  "%s retains its existing-account AUP behavior",
  async (role) => {
    const context = contextFor(role);
    context.accounts = [
      {
        ...structuredClone(fakeAccounts[0]),
        acceptableUsePolicyAgreedOn: undefined,
      },
    ];
    await visit("/caesfarm/myaccount", context);
    expect(
      !!screen.queryByText(/In order to continue accessing this cluster/),
    ).toBe(role === "FinancialAdmin");
  },
);
it("clusters without a policy allow existing accounts through", async () => {
  const context = contextFor();
  context.accounts = [
    {
      ...structuredClone(fakeAccounts[0]),
      acceptableUsePolicyAgreedOn: undefined,
    },
  ];
  context.clusters[0].acceptableUsePolicyUrl = "";
  await visit("/caesfarm/myaccount", context);
  expect(
    screen.queryByText(/In order to continue accessing this cluster/),
  ).not.toBeInTheDocument();
});

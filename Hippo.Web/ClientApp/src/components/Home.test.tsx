import { MemoryRouter } from "react-router-dom";
import App from "../App";
import {
  fakeAccounts,
  fakeGroupAdminAppContext,
  fakeAppContextNoAccount,
  fakeGroups,
} from "../test/mockData";
import { responseMap } from "../test/testHelpers";
import { render, screen, waitFor } from "@testing-library/react";
import "@testing-library/jest-dom";
import { act } from "react-dom/test-utils";

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

const testCluster = fakeAccounts[0].cluster;
const myAccountUrl = `/${testCluster}/myaccount`;
const promise = Promise.resolve(); // void promise for act warning hack

afterEach(() => {
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
});

describe("Basic render", () => {
  beforeEach(() => {
    const groupsResponse = Promise.resolve({
      status: 200,
      ok: true,
      json: () => Promise.resolve(fakeGroups),
    });
    global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAccounts[0].cluster}/group/groups`]: groupsResponse,
      }),
    );
    (global as any).Hippo = fakeGroupAdminAppContext;
  });

  it("renders without crashing", async () => {
    await render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    );
    await act(async () => await promise); //hack to prevent act warning
  });
});

describe("Home Redirect when GroupAdmin", () => {
  beforeEach(() => {
    const groupsResponse = Promise.resolve({
      status: 200,
      ok: true,
      json: () => Promise.resolve(fakeGroups),
    });

    (global as any).Hippo = fakeGroupAdminAppContext;

    global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAccounts[0].cluster}/group/groups`]: groupsResponse,
      }),
    );
  });

  it("renders without crashing", async () => {
    await render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    );
    await act(async () => await promise); //hack to prevent act warning
  });

  it("Shows welcome message", async () => {
    render(
      <MemoryRouter initialEntries={[myAccountUrl]}>
        <App />
      </MemoryRouter>,
    );
    expect(await screen.findByText("Welcome Bob")).toBeVisible();
  });

  it("Shows pending approvals buton", async () => {
    render(
      <MemoryRouter initialEntries={[myAccountUrl]}>
        <App />
      </MemoryRouter>,
    );
    expect(await screen.findByText("Pending Approvals")).toBeVisible();
  });
});

describe("Home Redirect no account", () => {
  beforeEach(() => {
    const groupsResponse = Promise.resolve({
      status: 200,
      ok: true,
      json: () => Promise.resolve(fakeGroups),
    });

    (global as any).Hippo = fakeAppContextNoAccount;

    global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAccounts[0].cluster}/group/groups`]: groupsResponse,
      }),
    );
  });
  it("Renders without crashing", async () => {
    await render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    );
    await act(async () => await promise); //hack to prevent act warning
  });

  it("Shows welcome message", async () => {
    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    );
    expect(
      await screen.findByText(/You don't seem to have an account/i),
    ).toBeVisible();
    await act(async () => await promise); //hack to prevent act warning
  });

  it("Does not shows pending approvals button", async () => {
    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    );
    await waitFor(() => {
      expect(screen.queryByText(/Pending Approvals/i)).not.toBeInTheDocument();
    });
    await act(async () => await promise); //hack to prevent act warning
  });
});

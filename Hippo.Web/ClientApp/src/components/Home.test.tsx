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
import { act } from "react";

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
    global.fetch = vitest
      .fn((url, options) => {
        // allow mock fetch to handle relative urls
        const absoluteUrl = new URL(url, "http://localhost");
        return fetch(absoluteUrl.toString(), options);
      })
      .mockImplementation((x) =>
        responseMap(x, {
          [`/api/${fakeAccounts[0].cluster}/group/groups`]: groupsResponse,
        }),
      );
    (global as any).Hippo = fakeGroupAdminAppContext;
  });

  it("renders without crashing", async () => {
    await act(async () => {
      render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
      );
    });
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

    global.fetch = vitest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAccounts[0].cluster}/group/groups`]: groupsResponse,
      }),
    );
  });

  it("renders without crashing", async () => {
    await act(async () => {
      render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
      );
    });
  });

  it("Shows welcome message", async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={[myAccountUrl]}>
          <App />
        </MemoryRouter>,
      );
    });
    expect(await screen.findByText("Welcome Bob")).toBeVisible();
  });

  it("Shows pending approvals button", async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={[myAccountUrl]}>
          <App />
        </MemoryRouter>,
      );
    });
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

    global.fetch = vitest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAccounts[0].cluster}/group/groups`]: groupsResponse,
      }),
    );
  });
  it("Renders without crashing", async () => {
    await act(async () => {
      render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
      );
    });
  });

  it("Shows welcome message", async () => {
    await act(async () => {
      render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
      );
    });
    expect(
      await screen.findByText(/You don't seem to have an account/i),
    ).toBeVisible();
  });

  it("Does not show pending approvals button", async () => {
    await act(async () => {
      render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
      );
    });
    await waitFor(() => {
      expect(screen.queryByText(/Pending Approvals/i)).not.toBeInTheDocument();
    });
  });
});

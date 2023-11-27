import React from "react";
import ReactDOM from "react-dom";
import { MemoryRouter } from "react-router-dom";
import App from "../App";
import {
  fakeAccounts,
  fakeGroupAdminAppContext,
  fakeAppContextNoAccount,
  fakeGroups,
} from "../test/mockData";
import { responseMap } from "../test/testHelpers";
import { act } from "react-dom/test-utils";

const testCluster = fakeAccounts[0].cluster;
const myAccountUrl = `/${testCluster}/myaccount`;

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
      })
    );
    (global as any).Hippo = fakeGroupAdminAppContext;
  });

  it("renders without crashing", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
        div
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

    global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAccounts[0].cluster}/group/groups`]: groupsResponse,
      })
    );
  });

  it("renders without crashing", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
        div
      );
    });
  });

  it("Shows welcome message", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter initialEntries={[myAccountUrl]}>
          <App />
        </MemoryRouter>,
        div
      );
    });
    expect(div.textContent).toContain(
      "Welcome Bob. Your account is registered with the following group(s):"
    );
  });

  it("Shows pending approvals button", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
        div
      );
    });
    expect(div.textContent).toContain("Pending Approvals");
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
      })
    );
  });
  it("renders without crashing", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
        div
      );
    });
  });

  it("Shows welcome message", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
        div
      );
    });
    expect(div.textContent).toContain("Welcome, Bob");
    expect(div.textContent).toContain("You don't seem to have an account");
  });

  it("Does not shows pending approvals button", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
        div
      );
    });
    expect(div.textContent).not.toContain("Pending Approvals");
  });
});

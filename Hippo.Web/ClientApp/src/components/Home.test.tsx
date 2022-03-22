import React from "react";
import ReactDOM from "react-dom";
import { MemoryRouter } from "react-router-dom";
import App from "../App";
import {
  fakeAccounts,
  fakeAdminAppContext,
  fakeAppContext,
  fakeAdminUsers,
  fakeAppContextNoAccount,
} from "../test/mockData";
import { responseMap } from "../test/testHelpers";
import { act } from "react-dom/test-utils";

afterEach(() => {
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
});

describe("Basic render", () => {
  beforeEach(() => {
    (global as any).Hippo = fakeAppContext;
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

    await new Promise((resolve) => setTimeout(resolve, 1000));
  });
});

describe("Home Redirect when Admin", () => {
  beforeEach(() => {
    const adminUsersResponse = Promise.resolve({
      status: 200,
      ok: true,
      json: () => Promise.resolve(fakeAdminUsers),
    });

    (global as any).Hippo = fakeAdminAppContext;

    global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAdminAppContext.accounts[0].cluster}/admin/index`]:
          adminUsersResponse,
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
    await new Promise((resolve) => setTimeout(resolve, 1000));
  });

  it("Redirects to AdminUsers", async () => {
    const div = document.createElement("div");
    await act(async () => {
      ReactDOM.render(
        <MemoryRouter>
          <App />
        </MemoryRouter>,
        div
      );
    });
    await new Promise((resolve) => setTimeout(resolve, 1000));
    expect(div.textContent).toContain("There are 2 users with admin access");
  });
});

describe("Home Redirect when Sponsor", () => {
  beforeEach(() => {
    (global as any).Hippo = fakeAppContext;
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
    await new Promise((resolve) => setTimeout(resolve, 1000));
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
    await new Promise((resolve) => setTimeout(resolve, 1000));
    expect(div.textContent).toContain(
      "Welcome Bob you already have an account, enjoy farm"
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
    await new Promise((resolve) => setTimeout(resolve, 1000));
    expect(div.textContent).toContain("Pending Approvals");
  });
});

describe("Home Redirect no account", () => {
  beforeEach(() => {
    const sponsorsResponse = Promise.resolve({
      status: 200,
      ok: true,
      json: () => Promise.resolve(fakeAccounts),
    });

    (global as any).Hippo = fakeAppContextNoAccount;

    global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        [`/api/${fakeAccounts[0].cluster}/account/sponsors`]: sponsorsResponse,
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
    await new Promise((resolve) => setTimeout(resolve, 1000));
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
    await new Promise((resolve) => setTimeout(resolve, 1000));
    expect(div.textContent).toContain("Welcome, Bob");
    expect(div.textContent).toContain(
      "You don't seem to have an account on Farm yet."
    );
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
    await new Promise((resolve) => setTimeout(resolve, 1000));
    expect(div.textContent).not.toContain("Pending Approvals");
  });
});

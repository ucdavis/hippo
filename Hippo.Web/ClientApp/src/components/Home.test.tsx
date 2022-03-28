import React from "react";
import ReactDOM from "react-dom";
import { MemoryRouter } from "react-router-dom";
import App from "../App";
import {
  fakeAccounts,
  fakeAppContext,
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
    expect(div.textContent).toContain(
      "Welcome Bob you already have an account"
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
    expect(div.textContent).toContain(
      "You don't seem to have an account"
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
    expect(div.textContent).not.toContain("Pending Approvals");
  });
});

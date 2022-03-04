import React from "react";
import ReactDOM from "react-dom";
import { MemoryRouter } from "react-router-dom";
import App from "../App";
import {
  fakeAccounts,
  fakeAdminAppContext,
  fakeAppContext,
  fakeAdminUsers,
} from "../test/mockData";
import { responseMap } from "../test/testHelpers";

beforeEach(() => {
  const accountResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeAccounts[0]),
  });

  (global as any).Hippo = fakeAppContext;

  global.fetch = jest.fn().mockImplementation((x) =>
    responseMap(x, {
      "/api/account/get": accountResponse,
    })
  );
});

afterEach(() => {
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
});

it("renders without crashing", async () => {
  const div = document.createElement("div");
  ReactDOM.render(
    <MemoryRouter>
      <App />
    </MemoryRouter>,
    div
  );
  await new Promise((resolve) => setTimeout(resolve, 1000));
});

describe("Home Redirect", () => {
  beforeEach(() => {
    const accountResponse = Promise.resolve({
      status: 200,
      ok: true,
      json: () => Promise.resolve(fakeAccounts[0]),
    });

    const adminUsersResponse = Promise.resolve({
      status: 200,
      ok: true,
      json: () => Promise.resolve(fakeAdminUsers),
    });

    (global as any).Hippo = fakeAdminAppContext;

    global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        "/api/account/get": accountResponse,
        "/api/admin/index": adminUsersResponse,
      })
    );
  });
  it("renders without crashing", async () => {
    const div = document.createElement("div");
    ReactDOM.render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
      div
    );
    await new Promise((resolve) => setTimeout(resolve, 1000));
  });

  it("Redirects to AdminUsers when user is Admin", async () => {
    const div = document.createElement("div");
    ReactDOM.render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
      div
    );
    await new Promise((resolve) => setTimeout(resolve, 1000));
    expect(div.textContent).toContain("There are 2 users with admin access");
  });
});

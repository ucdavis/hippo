import { MemoryRouter, Route, Routes } from "react-router-dom";

import {
  fakeAccounts,
  fakeGroupAdminAppContext,
  fakeGroups,
  fakeRawRequests,
} from "../../test/mockData";
import { responseMap } from "../../test/testHelpers";

import App from "../../App";
import { Requests } from "./Requests";
import { ModalProvider } from "react-modal-hook";
import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom";
import userEvent from "@testing-library/user-event";

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

const testCluster = fakeAccounts[0].cluster;
const approveUrl = `/${testCluster}/approve`;

beforeEach(() => {
  const requestResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeRawRequests),
  });
  const approveResponse = Promise.resolve({
    status: 200,
    ok: true,
  });
  const groupsResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeGroups),
  });

  (global as any).Hippo = fakeGroupAdminAppContext;

  global.fetch = jest.fn().mockImplementation((x) =>
    responseMap(x, {
      [`/api/${testCluster}/request/pending`]: requestResponse,
      [`/api/${testCluster}/request/approve/2`]: approveResponse,
      [`/api/${testCluster}/group/groups`]: groupsResponse,
    }),
  );
});

afterEach(() => {
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
});

it("shows pending approvals count", async () => {
  render(
    <MemoryRouter initialEntries={[approveUrl]}>
      <App />
    </MemoryRouter>,
  );
  expect(
    await screen.findByText("There are 2 request(s) awaiting approval"),
  ).toBeVisible();
});

it("shows approval button for each pending account", async () => {
  render(
    <MemoryRouter initialEntries={[approveUrl]}>
      <App />
    </MemoryRouter>,
  );

  expect(
    await screen.findAllByRole("button", { name: "Approve" }),
  ).toHaveLength(2);
});

it("shows reject button for each pending account", async () => {
  render(
    <MemoryRouter initialEntries={[approveUrl]}>
      <App />
    </MemoryRouter>,
  );

  expect(await screen.findAllByRole("button", { name: "Reject" })).toHaveLength(
    2,
  );
});

// //Enable this when the test works
it("displays dialog when reject is clicked", async () => {
  const user = userEvent.setup();
  render(
    <MemoryRouter initialEntries={[approveUrl]}>
      <ModalProvider>
        <Routes>
          <Route path={"/:cluster/approve"} element={<Requests />} />
        </Routes>
      </ModalProvider>
    </MemoryRouter>,
  );
  expect(
    await screen.findByText("There are 2 request(s) awaiting approval"),
  ).toBeVisible();

  await user.click(screen.getAllByRole("button", { name: "Reject" })[0]);

  expect(await screen.findByText("Reject Request")).toBeVisible();
});

it("calls approve and filters list when approve is clicked", async () => {
  const user = userEvent.setup();
  render(
    <MemoryRouter initialEntries={[approveUrl]}>
      <ModalProvider>
        <Routes>
          <Route path={"/:cluster/approve"} element={<Requests />} />
        </Routes>
      </ModalProvider>
    </MemoryRouter>,
  );
  expect(
    await screen.findByText("There are 2 request(s) awaiting approval"),
  ).toBeVisible();

  await user.click(screen.getAllByRole("button", { name: "Approve" })[0]);

  expect(global.fetch).toHaveBeenCalledTimes(2);
  expect(global.fetch).toHaveBeenCalledWith(
    `/api/${testCluster}/request/pending`,
    {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        RequestVerificationToken: "fakeAntiForgeryToken",
      },
    },
  );
  expect(global.fetch).toHaveBeenLastCalledWith(
    `/api/${testCluster}/request/approve/2`,
    {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        RequestVerificationToken: "fakeAntiForgeryToken",
      },
      method: "POST",
    },
  );
});

import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";

import { fakeAccounts, fakeAppContext } from "../../test/mockData";
import { responseMap } from "../../test/testHelpers";

import { act, Simulate } from "react-dom/test-utils";

import App from "../../App";
import { ApproveAccounts } from "./ApproveAccounts";
import { ModalProvider } from "react-modal-hook";

const testCluster = fakeAccounts[0].cluster;
const approveUrl = `/${testCluster}/approve`;

let container: Element;

beforeEach(() => {
  const accountResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeAccounts),
  });
  const approveResponse = Promise.resolve({
    status: 200,
    ok: true,
  });

  (global as any).Hippo = fakeAppContext;
  container = document.createElement("div");
  document.body.appendChild(container);

  global.fetch = jest.fn().mockImplementation((x) =>
    responseMap(x, {
      [`/api/${testCluster}/account/pending`]: accountResponse,
      [`api/${testCluster}/account/approve/1`]: approveResponse,
    })
  );
});

afterEach(() => {
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
  unmountComponentAtNode(container);
  container.remove();
});

it("shows pending approvals count", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <App />
      </MemoryRouter>,
      container
    );
  });
  expect(container.textContent).toContain(
    "There are 2 account(s) awaiting approval"
  );
});

it("shows approval button for each pending account", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <App />
      </MemoryRouter>,
      container
    );
  });
  expect(container.querySelectorAll("button.btn.btn-primary").length).toBe(2);
});

it("shows reject button for each pending account", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <App />
      </MemoryRouter>,
      container
    );
  });
  expect(container.querySelectorAll("button.btn.btn-danger").length).toBe(2);
});

it("approve button has expected text", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <App />
      </MemoryRouter>,
      container
    );
  });
  expect(container.querySelector("button.btn.btn-primary")?.textContent).toBe(
    "Approve"
  );
});
it("reject button has expected text", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <App />
      </MemoryRouter>,
      container
    );
  });
  expect(container.querySelector("button.btn.btn-danger")?.textContent).toBe(
    "Reject"
  );
});

it("table header has expected text", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <App />
      </MemoryRouter>,
      container
    );
  });
  expect(container.querySelector("tr")?.textContent).toBe(
    "NameSubmittedAction"
  );
});

//Enable this when the test works
xit("displays dialog when reject is clicked", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <App />
      </MemoryRouter>,
      container
    );
  });
  console.log(container.innerHTML);
  const rejectButton = container.querySelector(
    "button.btn.btn-danger"
  ) as HTMLButtonElement;
  expect(rejectButton).toBeTruthy();

  Simulate.click(rejectButton);
  console.log(container.innerHTML);
  expect(container.querySelector("div.modal-dialog")).toBeTruthy();
});

it("calls approve and filters list when approve is clicked", async () => {
  await act(async () => {
    render(
      <MemoryRouter initialEntries={[approveUrl]}>
        <ModalProvider>
          <Route path={"/:cluster/approve"}>
            <ApproveAccounts />
          </Route>
        </ModalProvider>
      </MemoryRouter>,
      container
    );
  });
  expect(container.textContent).toContain(
    "There are 2 account(s) awaiting approval"
  );
  //console.log(container.innerHTML);
  const approveButton = container.querySelector(
    "button.btn.btn-primary"
  ) as HTMLButtonElement;
  expect(approveButton).toBeTruthy();
  await act(async () => {
    Simulate.click(approveButton);
  });
  //console.log(container.innerHTML);
  expect(container.textContent).toContain(
    "There are 1 account(s) awaiting approval"
  );

  expect(global.fetch).toHaveBeenCalledTimes(2);
  expect(global.fetch).toHaveBeenCalledWith(
    `/api/${testCluster}/account/pending`,
    {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        RequestVerificationToken: "fakeAntiForgeryToken",
      },
    }
  );
  expect(global.fetch).toHaveBeenLastCalledWith(
    `/api/${testCluster}/account/approve/1`,
    {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        RequestVerificationToken: "fakeAntiForgeryToken",
      },
      method: "POST",
    }
  );
});

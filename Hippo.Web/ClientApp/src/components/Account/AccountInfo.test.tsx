import { act } from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { ModalProvider } from "react-modal-hook";
import { render, screen, waitFor } from "@testing-library/react";
import "@testing-library/jest-dom";
import userEvent from "@testing-library/user-event";
import AppContext from "../../Shared/AppContext";
import { responseMap } from "../../test/testHelpers";
import {
  fakeAppContext,
  fakeSetContext,
  fakeFeatureFlags,
} from "../../test/mockData";
import { GroupModel, PuppetGroupRecord, User } from "../../types";
import { AccountInfo } from "./AccountInfo";

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

vi.mock("../Group/GroupLookup", () => ({
  GroupLookup: ({
    options,
    setSelection,
  }: {
    options: GroupModel[];
    setSelection: (selection?: GroupModel) => void;
  }) => (
    <div>
      {options.map((group) => (
        <button
          key={group.id}
          type="button"
          onClick={() => setSelection(group)}
        >
          {`Select ${group.name}`}
        </button>
      ))}
    </div>
  ),
}));

const fakePI: User = {
  id: 42,
  firstName: "Gene",
  lastName: "Principal",
  email: "gene.pi@ucdavis.edu",
  iam: "1000000042",
  kerberos: "genepi",
  name: "Gene Principal",
};

vi.mock("../../Shared/SearchPerson", () => ({
  SearchPerson: ({
    onChange,
  }: {
    onChange: (user: User | undefined) => void;
  }) => (
    <div>
      <button type="button" onClick={() => onChange(fakePI)}>
        Select PI
      </button>
      <button type="button" onClick={() => onChange(undefined)}>
        Clear PI
      </button>
    </div>
  ),
}));

const accountInfoContext = {
  ...fakeAppContext,
  accounts: [
    {
      ...fakeAppContext.accounts[0],
      memberOfGroups: [],
      adminOfGroups: [],
    },
  ],
  openRequests: [],
  featureFlags: fakeFeatureFlags,
};

const testCluster = accountInfoContext.accounts[0].cluster;
const myAccountUrl = `/${testCluster}/myaccount`;
const fakeGroups: GroupModel[] = [
  {
    id: 101,
    name: "genome-center-grp",
    displayName: "Genome Center",
    admins: [],
    data: {} as PuppetGroupRecord,
  },
  {
    id: 102,
    name: "regular-group",
    displayName: "Regular Group",
    admins: [],
    data: {} as PuppetGroupRecord,
  },
];

const renderAccountInfo = async () => {
  await act(async () => {
    render(
      <AppContext.Provider
        value={[
          {
            ...accountInfoContext,
          },
          fakeSetContext,
        ]}
      >
        <MemoryRouter initialEntries={[myAccountUrl]}>
          <ModalProvider>
            <Routes>
              <Route path="/:cluster/myaccount" element={<AccountInfo />} />
            </Routes>
          </ModalProvider>
        </MemoryRouter>
      </AppContext.Provider>,
    );
  });
};

beforeEach(() => {
  (global as any).Hippo = accountInfoContext;

  const groupsResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeGroups),
  });

  global.fetch = vitest
    .fn((url, options) => {
      const absoluteUrl = new URL(url, "http://localhost");
      return fetch(absoluteUrl.toString(), options);
    })
    .mockImplementation((x) =>
      responseMap(x, {
        [`/api/${testCluster}/group/groups`]: groupsResponse,
      }),
    );
});

afterEach(() => {
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
});

it("requires a supervising PI before confirming genome center access", async () => {
  const user = userEvent.setup();
  await renderAccountInfo();

  const openModalButton = await screen.findByRole("button", {
    name: "Request Access to Another Group",
  });

  await waitFor(() => expect(openModalButton).toBeEnabled());

  await act(async () => {
    await user.click(openModalButton);
  });

  const confirmButton = await screen.findByRole("button", { name: "Confirm" });
  expect(confirmButton).toBeDisabled();

  await act(async () => {
    await user.click(screen.getByRole("button", { name: "Select genome-center-grp" }));
  });

  expect(confirmButton).toBeDisabled();

  await act(async () => {
    await user.click(screen.getByRole("button", { name: "Select PI" }));
  });

  expect(confirmButton).toBeEnabled();
});

it("allows confirming a regular group request without selecting a supervising PI", async () => {
  const user = userEvent.setup();
  await renderAccountInfo();

  const openModalButton = await screen.findByRole("button", {
    name: "Request Access to Another Group",
  });

  await waitFor(() => expect(openModalButton).toBeEnabled());

  await act(async () => {
    await user.click(openModalButton);
  });

  const confirmButton = await screen.findByRole("button", { name: "Confirm" });
  expect(confirmButton).toBeDisabled();

  await act(async () => {
    await user.click(screen.getByRole("button", { name: "Select regular-group" }));
  });

  expect(confirmButton).toBeEnabled();
});

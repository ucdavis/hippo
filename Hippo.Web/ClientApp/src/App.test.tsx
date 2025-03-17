import { MemoryRouter } from "react-router-dom";
import App from "./App";
import { fakeAccounts, fakeAppContext, fakeGroups } from "./test/mockData";
import { render } from "@testing-library/react";
import { responseMap } from "./test/testHelpers";
import { act } from "react";

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

const testCluster = fakeAccounts[0].cluster;
const groupsResponse = Promise.resolve({
  status: 200,
  ok: true,
  json: () => Promise.resolve(fakeGroups),
});

beforeEach(() => {
  (global as any).Hippo = fakeAppContext;
  global.fetch = vitest
    .fn((url, options) => {
      // allow mock fetch to handle relative urls
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
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
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

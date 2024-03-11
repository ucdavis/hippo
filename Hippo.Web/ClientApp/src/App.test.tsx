import { MemoryRouter } from "react-router-dom";
import App from "./App";
import { fakeAppContext } from "./test/mockData";
import { render } from "@testing-library/react";

globalThis.IS_REACT_ACT_ENVIRONMENT = true;

beforeEach(() => {
  (global as any).Hippo = fakeAppContext;
});

afterEach(() => {
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
});

it("renders without crashing", async () => {
  await render(
    <MemoryRouter>
      <App />
    </MemoryRouter>,
  );
});

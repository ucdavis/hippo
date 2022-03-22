import ReactDOM from "react-dom";
import { MemoryRouter } from "react-router-dom";
import App from "./App";
import { fakeAppContext } from "./test/mockData";
import { act } from "react-dom/test-utils";

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

import { act, fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes, useNavigate } from "react-router-dom";
import { ModalProvider } from "react-modal-hook";
import AppContext from "../Shared/AppContext";
import { fakeAppContextNoAccount } from "../test/mockData";
import { OrderModel } from "../types";
import GroupMembers from "./Group/GroupMembers";
import { Details } from "./Order/Details";
import { EditOrder } from "./Order/EditOrder";
import { UpdateChartStrings } from "./Order/UpdateChartStrings";
import { CreateOrder } from "./Order/CreateOrder";
import { OrderStatus } from "./Order/Statuses/status";

// Exercise page loading without unrelated child fetches or form interactions.
vi.mock("./Order/OrderForm/OrderForm", () => ({
  default: ({ orderProp }: { orderProp: OrderModel }) => (
    <div>{orderProp.name || "Blank order"}</div>
  ),
}));
vi.mock("./Order/HistoryTable", () => ({ HistoryTable: () => null }));
vi.mock("./Order/PaymentTable", () => ({ PaymentTable: () => null }));
const pages = [
  {
    name: "group",
    Page: GroupMembers,
    route: "group/:groupId",
    url: "group",
    api: (id: number) => `/api/caesfarm/account/groupMembers?groupId=${id}`,
  },
  {
    name: "details",
    Page: Details,
    route: "order/details/:orderId",
    url: "order/details",
    api: (id: number) => `/api/caesfarm/order/get/${id}`,
  },
  {
    name: "edit",
    Page: EditOrder,
    route: "order/edit/:orderId",
    url: "order/edit",
    api: (id: number) => `/api/caesfarm/order/get/${id}`,
  },
  {
    name: "chart strings",
    Page: UpdateChartStrings,
    route: "order/updatechartstrings/:orderId",
    url: "order/updatechartstrings",
    api: (id: number) => `/api/caesfarm/order/get/${id}`,
  },
  {
    name: "create",
    Page: CreateOrder,
    route: "order/create/:productId?",
    url: "order/create",
    api: (id: number) => `/api/caesfarm/order/GetProduct/${id}`,
  },
];
function response(data: unknown, status = 200): Response {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: async () => data,
  } as Response;
}
function dataFor(group: boolean, name: string) {
  return group
    ? {
        group: { id: 1, name, displayName: name, admins: [], data: {} },
        accounts: [],
        kerberosPendingRemoval: [],
      }
    : {
        id: 1,
        name,
        status: OrderStatus.Created,
        billings: [],
        metaData: [],
        total: "0",
        subTotal: "0",
        balancePending: "0",
        balanceRemaining: "0",
        totalPaid: "0",
        paymentCount: 0,
        historyCount: 0,
      };
}
function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (error: Error) => void;
  const promise = new Promise<T>((yes, no) => {
    resolve = yes;
    reject = no;
  });
  return { promise, resolve, reject };
}
function Next({ url }: { url: string }) {
  const navigate = useNavigate();
  return (
    <>
      <button onClick={() => navigate(`/caesfarm/${url}/2`)}>
        Next resource
      </button>
      <button onClick={() => navigate("/caesfarm/order/create")}>
        Create blank order
      </button>
    </>
  );
}
async function mount(
  page: (typeof pages)[number],
  id = "1",
  role: "System" | "GroupAdmin" = "System",
) {
  const context = structuredClone(fakeAppContextNoAccount);
  context.user.permissions = [{ role, cluster: "caesfarm" }];
  (globalThis as any).Hippo = context;
  let view: ReturnType<typeof render>;
  await act(async () => {
    view = render(
      <AppContext.Provider value={[context, vi.fn()]}>
        <ModalProvider>
          <MemoryRouter initialEntries={[`/caesfarm/${page.url}/${id}`]}>
            <Next url={page.url} />
            <Routes>
              <Route path={`/:cluster/${page.route}`} element={<page.Page />} />
              <Route
                path="/:cluster/product/index"
                element={<div>Choose a product</div>}
              />
            </Routes>
          </MemoryRouter>
        </ModalProvider>
      </AppContext.Provider>,
    );
  });
  return view!;
}
beforeEach(() => {
  vi.stubGlobal("fetch", vi.fn());
  vi.spyOn(window, "alert").mockImplementation(() => {});
});
afterEach(() => {
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});
describe.each(pages)("$name resource loading", (page) => {
  const payload = (name: string) => dataFor(page.name === "group", name);
  it("shows loading followed by the fetched resource", async () => {
    const pending = deferred<Response>();
    vi.mocked(fetch).mockReturnValue(pending.promise);
    await mount(page);
    expect(screen.getByText("Loading ...")).toBeVisible();
    expect(fetch).toHaveBeenCalledWith(page.api(1), expect.anything());
    await act(async () =>
      pending.resolve(response(payload("Loaded resource"))),
    );
    expect(screen.getAllByText(/Loaded resource/).length).toBeGreaterThan(0);
    expect(screen.queryByText("Loading ...")).not.toBeInTheDocument();
    expect(
      screen.queryByText(/There was an error loading/),
    ).not.toBeInTheDocument();
  });
  it.each([
    [404, "404 - Not Found"],
    [401, "Not Authorized"],
    [403, "Not Authorized"],
    [500, "Please refresh and try again."],
  ] as const)("ends loading on HTTP %s", async (status, message) => {
    vi.mocked(fetch).mockResolvedValue(response(null, status));
    await mount(page);
    expect(screen.getByText(message, { exact: false })).toBeVisible();
    expect(screen.queryByText("Loading ...")).not.toBeInTheDocument();
    expect(window.alert).not.toHaveBeenCalled();
  });
  it.each(["network", "json"])(
    "ends loading on %s failure",
    async (failure) => {
      if (failure === "network")
        vi.mocked(fetch).mockRejectedValue(new Error("offline"));
      else
        vi.mocked(fetch).mockResolvedValue({
          ...response(null),
          json: async () => {
            throw new Error("invalid JSON");
          },
        });
      await mount(page);
      expect(screen.getByText(/Please refresh and try again/)).toBeVisible();
      expect(screen.queryByText("Loading ...")).not.toBeInTheDocument();
    },
  );
  it.each(["success", "http", "network"])(
    "ignores stale %s after navigation",
    async (outcome) => {
      const first = deferred<Response>();
      vi.mocked(fetch)
        .mockReturnValueOnce(first.promise)
        .mockResolvedValue(response(payload("Latest resource")));
      await mount(page);
      await act(async () => fireEvent.click(screen.getByText("Next resource")));
      await act(async () => {
        if (outcome === "network") first.reject(new Error("late error"));
        else
          first.resolve(
            response(payload("Stale resource"), outcome === "http" ? 404 : 200),
          );
      });
      expect(screen.getAllByText(/Latest resource/).length).toBeGreaterThan(0);
      expect(
        screen.queryByText(/Stale resource|404 - Not Found|Please refresh/),
      ).not.toBeInTheDocument();
    },
  );
  it("ignores JSON parsing completion after navigation", async () => {
    const body = deferred<unknown>();
    vi.mocked(fetch)
      .mockResolvedValueOnce({ ...response(null), json: () => body.promise })
      .mockResolvedValue(response(payload("Latest resource")));
    await mount(page);
    await act(async () => fireEvent.click(screen.getByText("Next resource")));
    await act(async () => body.resolve(payload("Stale resource")));
    expect(screen.getAllByText(/Latest resource/).length).toBeGreaterThan(0);
    expect(screen.queryByText(/Stale resource/)).not.toBeInTheDocument();
  });
  it("clears failed loading when navigating to a valid resource", async () => {
    vi.mocked(fetch)
      .mockResolvedValueOnce(response(null, 404))
      .mockResolvedValue(response(payload("Recovered resource")));
    await mount(page);
    expect(screen.getByText("404 - Not Found")).toBeVisible();
    await act(async () => fireEvent.click(screen.getByText("Next resource")));
    expect(screen.getAllByText(/Recovered resource/).length).toBeGreaterThan(0);
    expect(screen.queryByText("404 - Not Found")).not.toBeInTheDocument();
  });
  it("clears the old resource while the next one loads", async () => {
    const next = deferred<Response>();
    vi.mocked(fetch)
      .mockResolvedValueOnce(response(payload("Old resource")))
      .mockReturnValue(next.promise);
    await mount(page);
    await act(async () => fireEvent.click(screen.getByText("Next resource")));
    expect(screen.queryByText(/Old resource/)).not.toBeInTheDocument();
    expect(screen.getByText("Loading ...")).toBeVisible();
    await act(async () => next.resolve(response(payload("New resource"))));
  });
  it("ignores rejection after unmount", async () => {
    const pending = deferred<Response>();
    vi.mocked(fetch).mockReturnValue(pending.promise);
    const view = await mount(page);
    view.unmount();
    await act(async () => pending.reject(new Error("unmounted")));
    expect(window.alert).not.toHaveBeenCalled();
  });
});
it("accountless System can create a blank order without fetching a product", async () => {
  await mount(pages[4], "");
  expect(screen.getByText("Blank order")).toBeVisible();
  expect(fetch).not.toHaveBeenCalled();
});
it("non-admin creation without a product redirects to product selection", async () => {
  await mount(pages[4], "", "GroupAdmin");
  expect(screen.getByText("Choose a product")).toBeVisible();
  expect(fetch).not.toHaveBeenCalled();
});
it("keeps the blank order when an earlier product request finishes", async () => {
  const pending = deferred<Response>();
  vi.mocked(fetch).mockReturnValue(pending.promise);
  await mount(pages[4]);
  expect(screen.getByText("Loading ...")).toBeVisible();

  await act(async () =>
    fireEvent.click(screen.getByText("Create blank order")),
  );
  expect(screen.getByText("Blank order")).toBeVisible();
  await act(async () =>
    pending.resolve(response(dataFor(false, "Stale product"))),
  );

  expect(screen.getByText("Blank order")).toBeVisible();
  expect(screen.queryByText("Stale product")).not.toBeInTheDocument();
  expect(fetch).toHaveBeenCalledTimes(1);
});

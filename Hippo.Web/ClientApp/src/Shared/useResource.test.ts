import { act, renderHook } from "@testing-library/react";
import { fakeAppContextNoAccount } from "../test/mockData";
import { useResource } from "./useResource";

type Resource = { name: string };

beforeEach(() => {
  vi.stubGlobal("Hippo", structuredClone(fakeAppContextNoAccount));
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => ({
      ok: true,
      status: 200,
      json: async () => ({ name: "Loaded resource" }),
    })),
  );
});
afterEach(() => vi.unstubAllGlobals());

it("skips fetching for a null URL and supports a local resource", () => {
  const { result, rerender } = renderHook(() => useResource<Resource>(null));
  expect(result.current.data).toBeNull();
  expect(result.current.error).toBeNull();

  act(() => result.current.setData({ name: "Local resource" }));
  rerender();

  expect(result.current.data).toEqual({ name: "Local resource" });
  expect(fetch).not.toHaveBeenCalled();
});

it.each([200, 404])(
  "clears HTTP %s state when the URL becomes null",
  async (status) => {
    vi.mocked(fetch).mockResolvedValue({
      ok: status === 200,
      status,
      json: async () => ({ name: "Loaded resource" }),
    } as Response);
    const { result, rerender } = renderHook(
      ({ url }: { url: string | null }) => useResource<Resource>(url),
      { initialProps: { url: "/resource/1" } },
    );
    await act(async () => {});
    if (status === 200) {
      expect(result.current.data).toEqual({ name: "Loaded resource" });
    } else {
      expect(result.current.error).toEqual({ status });
    }

    rerender({ url: null });

    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
    expect(fetch).toHaveBeenCalledTimes(1);
  },
);

it("preserves local updates and a stable setter without refetching on rerenders", async () => {
  const { result, rerender } = renderHook(() =>
    useResource<Resource>("/resource/1"),
  );
  await act(async () => {});
  expect(result.current.data).toEqual({ name: "Loaded resource" });
  const setData = result.current.setData;

  act(() => setData({ name: "Saved resource" }));
  act(() => setData((current) => ({ ...current, name: `${current.name}!` })));
  rerender();

  expect(result.current.data).toEqual({ name: "Saved resource!" });
  expect(result.current.setData).toBe(setData);
  expect(result.current.error).toBeNull();
  expect(fetch).toHaveBeenCalledTimes(1);
});

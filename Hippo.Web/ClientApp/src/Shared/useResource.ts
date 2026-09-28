import { useEffect, useState } from "react";
import { authenticatedFetch } from "../util/api";
import type { LoadError } from "./LoadingAndErrors/ResourceLoadError";

// A null URL disables fetching. Callers can use setData for local resources
// and mutation results without triggering another GET.
export const useResource = <T>(url: string | null) => {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<LoadError | null>(null);

  useEffect(() => {
    let ignore = false;
    setData(null);
    setError(null);
    if (url === null) return;

    const load = async () => {
      try {
        const response = await authenticatedFetch(url);
        if (ignore) return;
        if (!response.ok) {
          setError({ status: response.status });
          return;
        }
        const result = (await response.json()) as T;
        if (!ignore) setData(result);
      } catch {
        if (!ignore) setError({});
      }
    };

    load();
    return () => {
      ignore = true;
    };
  }, [url]);

  return { data, error, setData };
};

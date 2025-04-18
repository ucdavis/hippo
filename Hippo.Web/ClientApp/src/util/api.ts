import { AppContextShape, ModelState } from "../types";

declare let Hippo: AppContextShape;

export const authenticatedFetch = async (
  url: string,
  init?: RequestInit,
  additionalHeaders?: HeadersInit,
): Promise<Response> =>
  fetch(url, {
    ...init,
    credentials: "include",
    headers: {
      Accept: "application/json",
      "Content-Type": "application/json",
      RequestVerificationToken: Hippo.antiForgeryToken,
      ...additionalHeaders,
    },
  });

export const parseBadRequest = async (
  response: Response,
): Promise<string[]> => {
  const responseText = await response.text();

  if (!responseText) return [];

  const o = tryParseJSONObject(responseText) as ModelState;

  if (!o) return [responseText];

  return (Object.keys(o) as Array<string>).map((k) => `${k}: ${o[k]}`);
};

export const tryParseJSONObject = (val: string) => {
  try {
    const o = JSON.parse(val);
    if (o && typeof o === "object") return o;
  } catch (e) {
    /* Yes, swallow the exception */
  }

  return undefined;
};

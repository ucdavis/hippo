import { AppContextShape } from "../types";

declare var Hippo: AppContextShape;

export const authenticatedFetch = async (
  url: string,
  init?: RequestInit,
  additionalHeaders?: HeadersInit
): Promise<any> =>
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

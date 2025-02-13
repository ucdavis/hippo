import { AccountRequestModel, GroupRequestModel, RequestModel } from "../types";

export const isString = (value: any): value is string =>
  typeof value === "string";
export const isBoolean = (value: any): value is boolean =>
  typeof value === "boolean";
export const isStringArray = (value: any): value is string[] =>
  Array.isArray(value) && (value.length === 0 || typeof value[0] === "string");
export const isFunction = (value: unknown): value is Function =>
  typeof value === "function";
export const isPromise = (value: any): value is Promise<any> =>
  value instanceof Promise;
export const isAccountRequest = (
  value: RequestModel,
): value is AccountRequestModel =>
  value &&
  (value.action === "CreateAccount" || value.action === "AddAccountToGroup");
export const isGroupRequest = (
  value: RequestModel,
): value is GroupRequestModel => value && value.action === "CreateGroup";
export const isNumber = (value: any): value is number =>
  typeof value === "number";
export const isObject = (value: any): value is object =>
  typeof value === "object" && Boolean(value);

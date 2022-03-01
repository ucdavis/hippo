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

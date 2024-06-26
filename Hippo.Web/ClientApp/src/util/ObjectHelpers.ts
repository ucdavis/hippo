export const filterCommonProperties = <T extends object>(obj: T) => {
  return filterObjectTree(obj, (key, value) => {
    if (key === "id" || key === "uid" || key === "gid") return false;
    if ((Array.isArray(value) || typeof value === "object") && !value)
      return false; // don't display property for empty/null object
    return true;
  });
};

export const filterObjectTree = <T, K extends keyof T>(
  obj: T,
  filter: (key: K, value: any) => boolean,
) => {
  const newObj = (Array.isArray(obj) ? [] : {}) as T;
  const filtered = Object.keys(obj)
    .filter((key) => filter(key as K, obj[key]))
    .reduce((o, k) => {
      const rawValue = obj[k];
      const value =
        (Array.isArray(rawValue) || typeof rawValue === "object") && !!rawValue
          ? filterObjectTree(rawValue, filter)
          : rawValue;
      o[k] = value;
      return o;
    }, newObj);

  return filtered;
};

export function responseMap(
  url: string,
  dict: { [key: string]: Promise<any> }
): Promise<any> {
  let key = Object.keys(dict).find((key: string) => url.includes(key));
  return key ? dict[key] : Promise.resolve(undefined);
}

export const SplitCamelCase = (str: string) => {
  return (
    str
      // insert a space between lower & upper
      .replace(/([a-z])([A-Z])/g, "$1 $2")
      // uppercase the first character
      .replace(/^./, (str) => str.toUpperCase())
  );
};

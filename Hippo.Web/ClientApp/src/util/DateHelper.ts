export const convertToPacificTime = (str: string) => {
  if (!str) {
    return "";
  }
  if (!str.endsWith("Z")) {
    str += "Z";
  }
  return new Date(str).toLocaleString("en-US", {
    timeZone: "America/Los_Angeles",
  });
};

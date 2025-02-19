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

export const convertToPacificDate = (str: string) => {
  if (!str) {
    return "";
  }
  if (!str.endsWith("Z")) {
    str += "Z";
  }
  return new Date(str).toLocaleDateString("en-US", {
    timeZone: "America/Los_Angeles",
  });
};

export const convertDateTimeToDate = (str: string) => {
  if (!str) {
    return "";
  }
  return new Date(str).toDateString();
};

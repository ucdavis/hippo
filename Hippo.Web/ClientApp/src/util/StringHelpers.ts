import { GroupModel } from "../types";

export const SplitCamelCase = (str: string) => {
  return (
    str
      // insert a space between lower & upper
      .replace(/([a-z])([A-Z])/g, "$1 $2")
      // uppercase the first character
      .replace(/^./, (str) => str.toUpperCase())
  );
};

export const getGroupModelString = (arg: GroupModel | GroupModel[]) => {
  const groups = Array.isArray(arg) ? arg : [arg];
  var values = [
    // start with names of groups for sortability
    ...groups.map((g) => g.name),
    // then include remaining properties for filtering
    ...groups.map((g) => [
      g.displayName,
      g.admins.map((a) => [a.name, a.email, a.kerberos]),
    ]),
  ];
  return values.flat().join(" ");
};

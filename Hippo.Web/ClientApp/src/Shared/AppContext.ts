import React from "react";
import {
  AccountModel,
  AppContextShape,
  Cluster,
  User,
  Permission,
} from "../types";

const AppContext = React.createContext<
  [AppContextShape, React.Dispatch<React.SetStateAction<AppContextShape>>]
>([
  {
    antiForgeryToken: "",
    user: {
      detail: {} as User,
      permissions: [] as Permission[],
    },
    accounts: [] as AccountModel[],
    clusters: [] as Cluster[],
  },
  () => {},
]);

export default AppContext;

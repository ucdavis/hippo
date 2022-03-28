import React from "react";
import { Account, AppContextShape, Cluster, User } from "../types";

const AppContext = React.createContext<
  [AppContextShape, React.Dispatch<React.SetStateAction<AppContextShape>>]
>([
  {
    antiForgeryToken: "",
    user: { detail: {} as User },
    accounts: [] as Account[],
    clusters: [] as Cluster[],
  },
  () => {},
]);

export default AppContext;

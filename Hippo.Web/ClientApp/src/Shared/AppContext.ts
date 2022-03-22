import React from "react";
import { Account, AppContextShape, User } from "../types";

const AppContext = React.createContext<
  [AppContextShape, React.Dispatch<React.SetStateAction<AppContextShape>>]
>([
  {
    antiForgeryToken: "",
    user: { detail: {} as User },
    accounts: [] as Account[]
  },
  () => {},
]);

export default AppContext;
